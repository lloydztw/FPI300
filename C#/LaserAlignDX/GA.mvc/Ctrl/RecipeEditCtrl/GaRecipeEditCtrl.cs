#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.BasicSpace;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using JetEazy.Interface;
using JetEazy.OpenCV;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.FormSpace;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaRecipeEditCtrl
    {
        #region GLOBAL_MESS
        protected IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        protected RecipeFPIX3Class xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        #endregion

        #region INTERACTOR
        CviGoldenPickingBox _cviGoldenBox = new CviGoldenPickingBox(Brushes.Orange) { Visible = false };
        #endregion

        #region GUI_MEMBERS
        //Timer xTimer = null;
        #endregion

        #region GUI_LINKS
        IvRecipeEditorUI _rcpEditUI;
        Form _frmOwner;

        Button btnLoadImage => _rcpEditUI.btnLoadImage;
        Button btnGrabImage => _rcpEditUI.btnGrabImage;
        Button btnSaveImage => _rcpEditUI.btnSaveImage;

        Button btnPickGoldenCell => _rcpEditUI.btnPickGoldenRegion;
        Button btnCreateRegions => _rcpEditUI.btnCreateCellRegions;

        Button btnOpenTemplateMatchWindow => _rcpEditUI.btnOpenTemplateMatchWindow;
        Button btnOpenEmptyTrayInspector => _rcpEditUI.btnOpenEmptyTrayWindow;
        Button btnOpenFlyCameraRecipeEditor => _rcpEditUI.btnOpenFlyCamRcpWindow;
        Button btnOpenLightCtrlWindow => _rcpEditUI.btnOpenLightCtrlWindow;

        Button btnCancel => _rcpEditUI.btnCancel;
        Button btnOK => _rcpEditUI.btnOK;
        #endregion

        #region RUNTIME_DATA
        bool _isGoldenPicking => _cviGoldenBox.Visible;
        #endregion

        public void Attach(IvRecipeEditorUI view)
        {
            _rcpEditUI = view;
            _frmOwner = _rcpEditUI.Window.FindForm();

            initDisplay();
            connectEventHandlers();

            // 自動釋放所有資源
            _rcpEditUI.Window.HandleDestroyed += (s, e) => cleanUp();

            // 延遲更新參數
            _rcpEditUI.Window.HandleCreated += (s, e) =>
            {
                updateRecipeParams(false);
                updateFullfovBmpToImageViewer(xRecipe.bmpOrg);
            };

            // 設定 Form 屬性
            _frmOwner.Text = "参数设定窗口";
            _frmOwner.WindowState = FormWindowState.Maximized;
            LanguageExClass.Instance.EnumControls(_frmOwner);
        }

        void initDisplay()
        {
            var matViewer = _rcpEditUI.ImgViewer.MatViewer;
            matViewer.AddInteractor(_cviGoldenBox);
        }
        void cleanUp()
        {
            //// 注意: xTimer 不用之後, 必須調用 Dispose() !!!
            //xTimer?.Dispose();
            //xTimer = null;

            // 卸載 EventHandler
            LtAoiFactory.OnLineScanRequested -= LtAoi_OnLineScanRequested;
        }
        void connectEventHandlers()
        {
            LtAoiFactory.OnLineScanRequested += LtAoi_OnLineScanRequested;

            btnOK.Click += (s, e) => CloseWindow(confirm: true);
            btnCancel.Click += (s, e) => CloseWindow(confirm: false);

            btnLoadImage.Click += (s, e) => LoadImage();
            btnGrabImage.Click += (s, e) => GrabLineScanCameraImage();
            btnSaveImage.Click += (s, e) => SaveImageOrg();

            btnPickGoldenCell.Click += (s, e) => toggleGoldenPicking();
            btnCreateRegions.Click += (s, e) => AutoCreateRegions();

            btnOpenEmptyTrayInspector.Click += (s, e) => OpenEmptyTrayInspectorTool();
            btnOpenTemplateMatchWindow.Click += (s, e) => OpenTemplateMatchWindow();
            btnOpenFlyCameraRecipeEditor.Click += (s, e) => OpenFlyCameraRecipeEditor();
            btnOpenLightCtrlWindow.Click += (s, e) => OpenLightCtrlWindow();

            _cviGoldenBox.OnBoxSelected += (s, e) => BuildGoldenCell();
        }

        #region EVENT_HANDLERS
        private void LtAoi_OnLineScanRequested(object sender, EventArgs e)
        {
            new Action(() =>
            {
                //System.Threading.Thread.Sleep(2000);
                LtAoiFactory.PushBitmap(xRecipe.bmpOrg, "RecipeOrg");
            }).BeginInvoke(null, null);
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateRecipeParams(bool toModel)
        {
            //(1) 顯示 xRecipe.bmpOrg 圖片
            //DS1.ReplaceDisplayImage(xRecipe.bmpOrg);
            //DS2.ReplaceDisplayImage(xRecipe.bmpOrgNoTray);
            //(2) 顯示 RecipeParaGridClass 參數
            //propertyGrid1.SelectedObject = RecipeParaGridClass.Instance;
            if (toModel)
            {
            }
            else
            {
                var rcpParams = RecipeParaGridClass.Instance;
                _rcpEditUI.pgParamsView.SelectedObject = rcpParams;
            }
        }
        void updateFullfovBmpToImageViewer(Bitmap bmp)
        {
            var matViewer = _rcpEditUI.ImgViewer.MatViewer;
            using (var bridge = new QxImageBridge(bmp))
            {
                var old = matViewer.Image;
                matViewer.Image = bridge.Image.Clone();
            }
        }
        void updateFullfovBmpToRecipe(Bitmap bmp, int target)
        {
            if (target == 0)
            {
                xRecipe.bmpOrg?.Dispose();
                xRecipe.bmpOrg = bmp;
            }
            else
            {
                xRecipe.bmpOrgNoTray?.Dispose();
                xRecipe.bmpOrgNoTray = bmp;
            }
            updateFullfovBmpToImageViewer(bmp);
        }
        void updateGoldenBmpToRecipe(Bitmap goldenBmp, Rectangle goldenRect, int target)
        {
            //    switch (xRegionNameCurrent)
            //    {
            //        case MeasureType.MeasureAOI:
            //            xRecipe.xRectRegionPrint = rectf;
            //            xRecipe.bmpprinttemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //            xRecipe.SavePrintTemplate();
            //            break;
            //        case MeasureType.MeasureNoTray:
            //            xRecipe.xRectRegionPrintNoTray = rectf;
            //            xRecipe.bmpprintNoTraytemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //            xRecipe.SavePrintNoTrayTemplate();
            //            break;
            //    }

            if (target == 0)
            {
                xRecipe.bmpprinttemplate?.Dispose();
                xRecipe.bmpprinttemplate = goldenBmp;
                xRecipe.xRectRegionPrint = goldenRect;
            }
            else
            {
                xRecipe.bmpprintNoTraytemplate?.Dispose();
                xRecipe.bmpprintNoTraytemplate = goldenBmp;
                xRecipe.xRectRegionPrintNoTray = goldenRect;
            }
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void updateGuiStatus()
        {
            btnPickGoldenCell.BackColor = _isGoldenPicking ? Color.HotPink : btnLoadImage.BackColor;
        }
        void toggleGoldenPicking()
        {
            enableGoldenPicking(!_isGoldenPicking);
        }
        void enableGoldenPicking(bool enabled)
        {
            if (_isGoldenPicking != enabled)
            {
                _cviGoldenBox.Visible = enabled;
                updateGuiStatus();
                var matViewer = _rcpEditUI.ImgViewer.MatViewer;
                matViewer.Invalidate();
            }
        }
        #endregion

        void LoadImage(int target = 0)
        {
            enableGoldenPicking(false);

            //string fileName = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            string fileName = GaUtil.BrowseImageFile();
            if (string.IsNullOrEmpty(fileName))
                return;

            var oldCursor = GaUtil.SetCursor(_frmOwner, Cursors.WaitCursor);

            try
            {
                var newBmp = GaImageUtil.LoadBigImage(fileName);
                if (newBmp == null)
                    return;

                //--------------------------------------------------
                // 更新到 xRecipe 的 bmpOrg 或 bmpNoTray
                // xRecipe 會接手 newBmp
                //--------------------------------------------------
                updateFullfovBmpToRecipe(newBmp, target);
            }
            catch (Exception ex)
            {
                GaUtil.SetCursor(_frmOwner, oldCursor);
                JetEazy.BasicSpace.VsMSG.Instance.Warning(ex.Message);
            }
            finally
            {
                GaUtil.SetCursor(_frmOwner, oldCursor);
            }
        }
        void GrabLineScanCameraImage(int target = 0)
        {
            enableGoldenPicking(false);

            var oldCursor = GaUtil.SetCursor(_frmOwner, Cursors.WaitCursor);
            var freeBmp = IScanCam.GetFreeImageBitmap();

            if (freeBmp != null)
            {
                //--------------------------------------------------
                // 更新到 xRecipe 的 bmpOrg 或 bmpNoTray
                // xRecipe 會接手 newBmp
                //--------------------------------------------------
                updateFullfovBmpToRecipe(freeBmp.ToBitmap(), target);
            }

            GaUtil.SetCursor(_frmOwner, oldCursor);
        }
        void SaveImageOrg()
        {
            enableGoldenPicking(false);

            string dstFileName = JetEazy.BasicSpace.JzToolsClass.SaveFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(dstFileName))
            {
                // 存完圖, 馬上就 Dispose, 所以不需要 DeepCopy
                using (IEzImage ezImage = new EzFreeBitmap(xRecipe.bmpOrg, false))
                {
                    ezImage.Save(dstFileName);
                }
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("图片保存完成.路径:")}{Environment.NewLine + dstFileName}", false);
            }
        }

        void OpenEmptyTrayInspectorTool()
        {
            enableGoldenPicking(false);
            LtAoiFactory.OpenEmptyTrayInspectorTool(_frmOwner, bmpToShow: xRecipe.bmpOrg);
        }
        void OpenTemplateMatchWindow()
        {
            enableGoldenPicking(false);

            using (var frm = new frmTemplateX3())
            {
                frm.ShowDialog();
            }

            updateRecipeParams(false);
        }
        void OpenFlyCameraRecipeEditor()
        {
            enableGoldenPicking(false);

            using (var dlg = new frmFlySetup())
            {
                dlg.ShowDialog();
            }
        }
        void OpenLightCtrlWindow()
        {
            enableGoldenPicking(false);

            using (var dlg = new frmLightControl())
            {
                dlg.ShowDialog();
            }
        }
        void CloseWindow(bool confirm)
        {
            if (confirm)
            {
                //updateAllData(true);
                //SaveSettings();
                _frmOwner.DialogResult = DialogResult.OK;
            }
            else
            {
                // 還原舊值
                xRecipe.Load();
                _frmOwner.DialogResult = DialogResult.Cancel;
            }

            _frmOwner?.Close();

            //由上層調用 Dispose
            //_frmOwner?.Dispose();
        }

        void BuildGoldenCell(int target = 0)
        {
            var bmpSrc = xRecipe.bmpOrg;
            var bound = new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height);
            var goldenRect = _cviGoldenBox.Box;
            JetEazy.QUtilities.QUtility.ClipBoundary(ref goldenRect, ref bound);

            Bitmap goldenBmp = bmpSrc.Clone(goldenRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            //--------------------------------------------------------------
            // 更新到 xRecipe 的 bmpprinttemplate 或 bmpprintNoTraytemplate
            // xRecipe 會接手 goldenBmp
            //--------------------------------------------------------------
            updateGoldenBmpToRecipe(goldenBmp, goldenRect, target);
        }
        void AutoCreateRegions()
        {
            enableGoldenPicking(false);

            ////var fullfovBmp = DS2.GetOrgBMP();
            ////var fullfovBmp = xRecipe.bmpOrg;
            ////var grid = LtAoiFactory.DetectGrid(fullfovBmp);
            //xRecipe.CreateViews();

            //DS1.ClearStaticMover();
            //DS2.ClearStaticMover();
            //xMovers.Clear();

            //int i = 0;
            //while (i < xRecipe.xRegionCells.Count)
            //{
            //    var cell = xRecipe.xRegionCells[i];
            //    //EzBloc bloc = grid.Get(cell.CellRow, cell.CellCol);
            //    //if (bloc != null)
            //    //{
            //    //    JetEazy.Qcvt.SetCenter(ref cell.viewRectF, bloc.CenterX, bloc.CenterY);
            //    //}
            //    JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), cell.viewRectF);
            //    _rect.RelateLevel = 2;
            //    _rect.RelateNo = i;
            //    _rect.RelatePosition = 0;
            //    xMovers.Add(_rect);

            //    i++;
            //}

            ////switch (xTabIndex)
            ////{
            ////    case 0:
            ////        DS1.SetStaticMover(xMovers);
            ////        DS1.RefreshDisplayShape();
            ////        DS1.MappingSelect();
            ////        break;
            ////    case 1:
            ////        DS2.SetStaticMover(xMovers);
            ////        DS2.RefreshDisplayShape();
            ////        DS2.MappingSelect();
            ////        break;
            ////}

            ////DS1.SetStaticMover(xMovers);
            ////DS1.RefreshDisplayShape();
            ////DS1.MappingSelect();

            //DS2.SetStaticMover(xMovers);
            //DS2.RefreshDisplayShape();
            //DS2.MappingSelect();

            //DS1.SetStaticMover(xMovers);
            //DS1.RefreshDisplayShape();
            //DS1.MappingSelect();

            //update_Display(false);
        }

        #region LEGACY
        void update_Display(bool eChangeToDefault = true)
        {
            //DS1.Refresh();
            //if (eChangeToDefault)
            //    DS1.DefaultView();

            //DS2.Refresh();
            //if (eChangeToDefault)
            //    DS2.DefaultView();
        }
        void DS_CaptureAction(RectangleF rectf)
        {
            //if (!bSelectRegion)
            //    return;
            //GaUtil.BoundRect(ref rectf, xRecipe.bmpOrg.Size);
            //if (rectf.Width > 1 && rectf.Height > 1)
            //{
            //    //防止大图的问题 不能new图  通过控件的框显示
            //    //Bitmap bmpx = new Bitmap(xRecipe.bmpOrg);
            //    //Graphics g = Graphics.FromImage(bmpx);

            //    DS1.ClearStaticMover();
            //    //DS2.ClearStaticMover();
            //    xMovers.Clear();
            //    JzRectEAG _rect = new JzRectEAG(Color.FromArgb(0, Color.Blue), rectf);
            //    _rect.RelateLevel = 2;
            //    //_rect.RelateNo = i;
            //    _rect.RelatePosition = 0;
            //    xMovers.Add(_rect);

            //    switch (xRegionNameCurrent)
            //    {
            //        case MeasureType.MeasureAOI:
            //            xRecipe.xRectRegionPrint = rectf;
            //            xRecipe.bmpprinttemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //            xRecipe.SavePrintTemplate();
            //            break;
            //        case MeasureType.MeasureNoTray:
            //            xRecipe.xRectRegionPrintNoTray = rectf;
            //            xRecipe.bmpprintNoTraytemplate = xRecipe.bmpOrg.Clone(rectf, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            //            xRecipe.SavePrintNoTrayTemplate();
            //            break;
            //    }

            //    DS1.SetStaticMover(xMovers);
            //    DS1.RefreshDisplayShape();
            //    DS1.MappingSelect();

            //    update_Display(false);
            //}
            //bSelectRegion = false;
        }
        #endregion

        #region LANGUAGE_TOOLS
        private string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
        #endregion
    }
}
