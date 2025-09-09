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

using EzAoiEmptyTrayInspector.Model;
using JetEazy.BasicSpace;
using JetEazy.EzImage;
using JetEazy.Interface;
using LaserAlignDX.AoiModel;
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
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        GaBigImageHolder _lineScanImageHolder => TravellerBigImagesHolder.Instance.LineScanImageHolder;
        #endregion

        #region RECIPE
        RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        JxAoiRecipe _jxEmptyTrayRecipe;
        string getEmptyTrayRecipeFileName()
        {
            return LtAoiFactory.RcpGetRecipeFileName(null);
        }
        #endregion

        #region KERNEL_DATA
        GaCalibCtrl _calibCtrl;
        #endregion

        #region INTERACTOR
        CviGoldenPickingBox _cviGoldenChipBox = new CviGoldenPickingBox(Brushes.Orange) { Visible = false };
        #endregion

        #region GUI_MEMBERS
        #endregion

        #region GUI_LINKS
        internal IvRecipeEditorUI _rcpEditUI;
        Form _frmOwner;

        Button btnRunEmptyTrayInspect => _rcpEditUI.btnRunEmptyTrayInspect;
        Button btnPickGoldenChipRegion => _rcpEditUI.btnPickGoldenChipRegion;
        Button btnCreateRegions => _rcpEditUI.btnCreateCellRegions;

        Button btnOpenTemplateMatchWindow => _rcpEditUI.btnOpenTemplateMatchWindow;
        Button btnOpenFlyCameraRecipeEditor => _rcpEditUI.btnOpenFlyCamRcpWindow;
        Button btnOpenLightCtrlWindow => _rcpEditUI.btnOpenLightCtrlWindow;

        Button btnSaveImage => _rcpEditUI.btnSaveImage;
        Button btnCancel => _rcpEditUI.btnCancel;
        Button btnOK => _rcpEditUI.btnOK;
        #endregion

        #region RUNTIME_DATA
        bool _isGoldenChipPicking => _cviGoldenChipBox.Visible;
        #endregion

        public void Attach(IvRecipeEditorUI editorView)
        {
            _rcpEditUI = editorView;
            _frmOwner = _rcpEditUI.Window.FindForm();
            btnPickGoldenChipRegion.Tag = btnPickGoldenChipRegion.BackColor;

            initImgViewer();
            connectEventHandlers();
        }
        
        void Dispose()
        {
            // 卸載 EventHandler
            _lineScanImageHolder.OnImageChanged -= _lineScanImageHolder_OnImageChanged;
            LtAoiFactory.OnLineScanRequested -= LtAoi_OnLineScanRequested;
            // Recipe
            _jxEmptyTrayRecipe?.Dispose();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initImgViewer()
        {
            var matViewer = _rcpEditUI.ImgViewer.MatViewer;
            matViewer.AddInteractor(_cviGoldenChipBox);
        }
        void reuseCalibCtrl()
        {
            // 載入 空盤檢測 參數
            LoadSettings();

            // 重複利用 GaCalibCtrl
            _calibCtrl = new GaCalibCtrl();
            _calibCtrl.Attach(new PseudoCalibUI(this), _jxEmptyTrayRecipe, getEmptyTrayRecipeFileName());

            // 因為 GaCalibCtrl 會調用 _jxEmptyTrayRecipe.Dispose(), 所以在此處要多 AddRef() 一次.
            _jxEmptyTrayRecipe.AddRef();
        }
        void connectEventHandlers()
        {
            LtAoiFactory.OnLineScanRequested += LtAoi_OnLineScanRequested;
            _lineScanImageHolder.OnImageChanged += _lineScanImageHolder_OnImageChanged;

            btnOK.Click += (s, e) => CloseWindow(confirm: true);
            btnCancel.Click += (s, e) => CloseWindow(confirm: false);
            btnSaveImage.Click += (s, e) => SaveImageOrg();
            //btnLoadImage.Click += (s, e) => LoadImage();
            //btnGrabImage.Click += (s, e) => GrabImage();

            btnPickGoldenChipRegion.Click += (s, e) => toggleGoldenChipPicking();
            btnCreateRegions.Click += (s, e) => AutoCreateRegions();
            _cviGoldenChipBox.OnBoxSelected += (s, e) => BuildGoldenChipRegion();

            btnRunEmptyTrayInspect.Click += (s, e) => RunEmptyTrayInspect();
            btnOpenTemplateMatchWindow.Click += (s, e) => OpenTemplateMatchWindow();
            btnOpenFlyCameraRecipeEditor.Click += (s, e) => OpenFlyCameraRecipeEditor();
            btnOpenLightCtrlWindow.Click += (s, e) => OpenLightCtrlWindow();

            // 自動釋放所有資源
            _rcpEditUI.Window.HandleDestroyed += (s, e) => Dispose();

            // 延遲更新參數
            _rcpEditUI.Window.HandleCreated += (s, e) =>
            {
                reuseCalibCtrl();
                updateRecipeParams(false);

                // 顯示默認的圖
                _lineScanImageHolder.TakeOver((Bitmap)xRecipe.bmpOrg.Clone(), "[參數] bmpOrg");
            };
        }
        #endregion

        #region EVENT_HANDLERS
        private void _lineScanImageHolder_OnImageChanged(object sender, EventArgs e)
        {
            //--------------------------------------------------
            // 顯示畫面 已經 重複利用 GaCalibCtrl 來實現
            //--------------------------------------------------
            if (_frmOwner == null || !_frmOwner.IsHandleCreated)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((EventHandler)_lineScanImageHolder_OnImageChanged);
            }
            else
            {
                //--------------------------------------------------
                // 更新到 xRecipe 的 bmpOrg 或 bmpNoTray
                // xRecipe 會接手 newBmp
                //--------------------------------------------------
                var newBmp = (Bitmap)_lineScanImageHolder.PeekBitmap()?.Clone();
                if (newBmp != null)
                    updateFullfovBmpToRecipe(newBmp, 0);
            }
        }
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
                var properyGrid = _rcpEditUI.wndVisionSettingsPanel as PropertyGrid;
                if (properyGrid != null)
                    properyGrid.SelectedObject = rcpParams;
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
            btnPickGoldenChipRegion.BackColor = _isGoldenChipPicking ? Color.HotPink : (Color)btnPickGoldenChipRegion.Tag;
        }
        void toggleGoldenChipPicking()
        {
            enableGoldenChipPicking(!_isGoldenChipPicking);
        }
        void enableGoldenChipPicking(bool enabled)
        {
            if (_isGoldenChipPicking != enabled)
            {
                _cviGoldenChipBox.Visible = enabled;
                updateGuiStatus();
                var matViewer = _rcpEditUI.ImgViewer.MatViewer;
                matViewer.Invalidate();
            }
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

        void LoadImage(int target = 0)
        {
            //enableGoldenPicking(false);

            ////string fileName = JetEazy.BasicSpace.JzToolsClass.OpenFilePicker("BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            //string fileName = GaUtil.BrowseImageFile();
            //if (string.IsNullOrEmpty(fileName))
            //    return;

            //var oldCursor = GaUtil.SetCursor(_frmOwner, Cursors.WaitCursor);

            //try
            //{
            //    var newBmp = GaImageUtil.LoadBigImage(fileName);
            //    if (newBmp == null)
            //        return;

            //    //--------------------------------------------------
            //    // 更新到 xRecipe 的 bmpOrg 或 bmpNoTray
            //    // xRecipe 會接手 newBmp
            //    //--------------------------------------------------
            //    updateFullfovBmpToRecipe(newBmp, target);
            //}
            //catch (Exception ex)
            //{
            //    GaUtil.SetCursor(_frmOwner, oldCursor);
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning(ex.Message);
            //}
            //finally
            //{
            //    GaUtil.SetCursor(_frmOwner, oldCursor);
            //}
        }
        void GrabImage(int target = 0)
        {
            //enableGoldenPicking(false);

            //var oldCursor = GaUtil.SetCursor(_frmOwner, Cursors.WaitCursor);
            //var freeBmp = IScanCam.GetFreeImageBitmap();

            //if (freeBmp != null)
            //{
            //    //--------------------------------------------------
            //    // 更新到 xRecipe 的 bmpOrg 或 bmpNoTray
            //    // xRecipe 會接手 newBmp
            //    //--------------------------------------------------
            //    updateFullfovBmpToRecipe(freeBmp.ToBitmap(), target);
            //}

            //GaUtil.SetCursor(_frmOwner, oldCursor);
        }
        void SaveImageOrg()
        {
            enableGoldenChipPicking(false);

            //為何不用最近剛抓的圖檔呢?
            //var srcBmp = _lineScanImageHolder.PeekBitmap();
            var srcBmp = xRecipe.bmpOrg;

            string dstFileName = JetEazy.BasicSpace.JzToolsClass.SaveFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (!string.IsNullOrEmpty(dstFileName))
            {
                // 存完圖, 馬上就 Dispose, 所以不需要 DeepCopy
                using (IEzImage ezImage = new EzFreeBitmap(srcBmp, false))
                {
                    ezImage.Save(dstFileName);
                }
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"{ToChangeLanguage("图片保存完成.路径:")}{Environment.NewLine + dstFileName}", false);
            }
        }

        void OpenTemplateMatchWindow()
        {
            enableGoldenChipPicking(false);

            using (var frm = new frmTemplateX3())
            {
                frm.ShowDialog();
            }

            updateRecipeParams(false);
        }
        void OpenFlyCameraRecipeEditor()
        {
            enableGoldenChipPicking(false);

            using (var dlg = new frmFlySetup())
            {
                dlg.ShowDialog();
            }
        }
        void OpenLightCtrlWindow()
        {
            enableGoldenChipPicking(false);

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
                SaveSettings();


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

        void RunEmptyTrayInspect()
        {
            // 更新 Plc Grid
            var traySettings = _jxEmptyTrayRecipe.TrayMiscSettings;
            var rows = (int)traySettings.FullRows.Value;
            var cols = (int)traySettings.FullCols.Value;
            var pitchX = (double)traySettings.PitchX.Value;
            var pitchY = (double)traySettings.PitchY.Value;
            GaMvcConfig.TransformsModel.ConfigPlcGrid(rows, cols, pitchX, pitchY);

            // 執行自動抓取格點
            _calibCtrl.RunAutoFetch(true);
        }
        void BuildGoldenChipRegion(int target = 0)
        {
            var bmpSrc = xRecipe.bmpOrg;
            var bound = new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height);
            var goldenRect = _cviGoldenChipBox.Box;
            JetEazy.QUtilities.QUtility.ClipBoundary(ref goldenRect, ref bound);

            //--------------------------------------------------------------
            // 更新到 xRecipe 的 bmpprinttemplate 或 bmpprintNoTraytemplate
            // xRecipe 會接手 goldenBmp
            //--------------------------------------------------------------
            Bitmap goldenBmp = bmpSrc.Clone(goldenRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            updateGoldenBmpToRecipe(goldenBmp, goldenRect, target);
        }
        void AutoCreateRegions()
        {
            enableGoldenChipPicking(false);

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

        void LoadSettings()
        {
            var recipeFileName = getEmptyTrayRecipeFileName();
            _jxEmptyTrayRecipe?.Dispose();
            _jxEmptyTrayRecipe = new JxAoiRecipe();
            _jxEmptyTrayRecipe.Load(recipeFileName);
        }
        void SaveSettings(bool force = false)
        {
            if (force || _jxEmptyTrayRecipe.Modified)
            {
                var recipeFileName = LtAoiFactory.RcpGetRecipeFileName(null);
                _jxEmptyTrayRecipe.Save(recipeFileName);
            }
        }
    }

    class PseudoCalibUI : IvCalibToolUI
    {
        #region PRIVATE_DATA
        IvRecipeEditorUI _imp;
        #endregion

        public PseudoCalibUI(GaRecipeEditCtrl rcpCtrl)
        {
            _imp = rcpCtrl._rcpEditUI;
        }

        #region WRAPPERS
        Control IvCalibToolUI.Window => _imp.Window;

        RadioButton[] IvCalibToolUI.rdoCarriers => null;
        RadioButton[] IvCalibToolUI.rdoSuckerRows => null;
        GvCalibPointsDataGridView IvCalibToolUI.dgvCalibPointsListView => null;

        JezTransImageViewPanel IvCalibToolUI.ImgViewer => _imp.ImgViewer;
        Control IvCalibToolUI.wndVisionSettingsPanel => _imp.wndVisionSettingsPanel;

        Button IvCalibToolUI.btnLoadImage => _imp.btnLoadImage;
        Button IvCalibToolUI.btnGrabImage => _imp.btnGrabImage;

        Button IvCalibToolUI.btnPickupGolden => _imp.btnPickGoldenEmptyRegion;

        Button IvCalibToolUI.btnRunAutoFetch => null;
        Button IvCalibToolUI.btnBuildCalib => null;
        Button IvCalibToolUI.btnCancel => null;
        Button IvCalibToolUI.btnOK => null;
        #endregion
    }
}
