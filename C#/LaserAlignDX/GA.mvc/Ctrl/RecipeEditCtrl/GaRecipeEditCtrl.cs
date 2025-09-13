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

using Eazy_Project_III;
using JetEazy.BasicSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.FormSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
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
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        IProcessRunFPI _aoiModel => _sysModel.AoiModel;
        #endregion

        #region RECIPES
        RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        RecipeParaGridClass xParamGrid => RecipeParaGridClass.Instance;
        //JxRecipeCombo _jxRecipeCombo => _sysModel?.GetCurrentRecipe();
        #endregion

        #region INTERACTOR
        CviGoldenPickingBox _cviGoldenRegionBox = new CviGoldenPickingBox(Brushes.Orange) { Visible = false };
        CviCalibResultBox _cviCamGridBox = new CviCalibResultBox();
        #endregion

        #region GUI_LINKS
        internal IvRecipeEditorUI _rcpEditUI;
        Form _wndOwner;
        JezTransImageViewPanel _imgViewer => _rcpEditUI.ImgViewer;
        RadioButton[] rdoCarriers => _rcpEditUI.rdoCarriers;
        Button btnOpenEmptyTrayWindow => _rcpEditUI.btnOpenEmptyTrayWindow;
        Button btnPickGoldenRegion => _rcpEditUI.btnPickGoldenChipRegion;
        Button btnAutoCreateRegions => _rcpEditUI.btnAutoCreateCellRegions;
        Button btnOpenTemplateMatchWindow => _rcpEditUI.btnOpenTemplateMatchWindow;
        Button btnOpenFlyCameraRecipeEditor => _rcpEditUI.btnOpenFlyCamRcpWindow;
        Button btnOpenLightCtrlWindow => _rcpEditUI.btnOpenLightCtrlWindow;
        Button btnWriteCoordsRefToPlc => _rcpEditUI.btnWriteCoordsToPlc;
        Button btnGrabImage => _rcpEditUI.btnGrabImage;
        Button btnLoadImage => _rcpEditUI.btnLoadImage;
        Button btnSaveImage => _rcpEditUI.btnSaveImage;
        Button btnCancel => _rcpEditUI.btnCancel;
        Button btnOK => _rcpEditUI.btnOK;
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _currentCarrierID = CarrierEnum.C1;
        bool _isGoldenRegionPicking => _cviGoldenRegionBox.Visible;
        bool _isModified = false;
        #endregion

        public void Attach(IvRecipeEditorUI editorView)
        {
            _rcpEditUI = editorView;
            _wndOwner = _rcpEditUI.Window.FindForm();
            btnPickGoldenRegion.Tag = btnPickGoldenRegion.BackColor;

            initImgViewer();
            connectEventHandlers();
        }
        void Dispose()
        {
            // 卸載 EventHandler
            LtAoiFactory.OnLineScanRequested -= LtAoi_OnLineScanRequested;
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initImgViewer()
        {
            var matViewer = _rcpEditUI.ImgViewer.MatViewer;
            matViewer.AddInteractor(_cviGoldenRegionBox);
            matViewer.AddInteractor(_cviCamGridBox);
        }
        void connectEventHandlers()
        {
            LtAoiFactory.OnLineScanRequested += LtAoi_OnLineScanRequested;

            btnOK.Click += (s, e) => CloseWindow(confirm: true);
            btnCancel.Click += (s, e) => CloseWindow(confirm: false);

            btnSaveImage.Click += (s, e) => SaveImageOrg();
            btnLoadImage.Click += (s, e) => LoadImage();
            btnGrabImage.Click += (s, e) => GrabImage();

            rdoCarriers[0].CheckedChanged += rdoCarrier_CheckedChanged;
            btnOpenEmptyTrayWindow.Click += (s, e) => OpenEmptyTrayInspectWindow();
            btnPickGoldenRegion.Click += (s, e) => toggleGoldenRegionPicking();
            btnAutoCreateRegions.Click += (s, e) => AutoCreateRegions();
            _cviGoldenRegionBox.OnBoxSelected += (s, e) => BuildGoldenRegion();

            btnOpenTemplateMatchWindow.Click += (s, e) => OpenTemplateMatchWindow();
            btnOpenFlyCameraRecipeEditor.Click += (s, e) => OpenFlyCameraRecipeEditor();
            btnOpenLightCtrlWindow.Click += (s, e) => OpenLightCtrlWindow();
            btnWriteCoordsRefToPlc.Click += (s, e) => WriteCoordsRefToPlc();

            // ProperyGrid
            if ( _rcpEditUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                pg.PropertyValueChanged += Pg_PropertyValueChanged;
            }

            // 自動釋放所有資源
            _rcpEditUI.Window.HandleDestroyed += (s, e) => Dispose();

            // 延遲更新參數
            _rcpEditUI.Window.HandleCreated += (s, e) =>
            {
                var delayAction = new Action(() =>
                {
                    updateAllRecipeData(false, _currentCarrierID);
                });

                _wndOwner.BeginInvoke(delayAction);
            };
        }
        #endregion

        #region EVENT_HANDLERS
        private void rdoCarrier_CheckedChanged(object sender, EventArgs e)
        {
            var carrierID = rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            if (carrierID != _currentCarrierID)
            {
                showCviResult(false);
                enableGoldenRegionPicking(false);
                _sysModel.ActiveCarrierID = carrierID;
                _sysModel.ApplyRecipe();
                updateAllRecipeData(false, carrierID);
            }
        }
        private void LtAoi_OnLineScanRequested(object sender, EventArgs e)
        {
            //new Action(() =>
            //{
            //    //System.Threading.Thread.Sleep(2000);
            //    LtAoiFactory.PushBitmap(xRecipe.bmpOrg, "RecipeOrg");
            //}).BeginInvoke(null, null);
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // 取得被改變的屬性名稱
            string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;
            _isModified = true;
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void updateGuiStatus()
        {
            btnPickGoldenRegion.BackColor = _isGoldenRegionPicking ? Color.HotPink : (Color)btnPickGoldenRegion.Tag;
            bool hasImage = _imgViewer.Image != null;
            btnSaveImage.Enabled = hasImage;
            btnPickGoldenRegion.Enabled = hasImage;
            btnAutoCreateRegions.Enabled = hasImage;
            btnOpenTemplateMatchWindow.Enabled = hasImage;
        }
        void toggleGoldenRegionPicking()
        {
            enableGoldenRegionPicking(!_isGoldenRegionPicking);
            //_calibCtrl.EnableGoldenPicking(false);
        }
        void enableGoldenRegionPicking(bool enabled)
        {
            if (_isGoldenRegionPicking != enabled)
            {
                _cviGoldenRegionBox.Visible = enabled;

                updateGuiStatus();

                if (enabled)
                    showCviResult(false);

                var matViewer = _rcpEditUI.ImgViewer.MatViewer;
                matViewer.Invalidate();
            }
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateAllRecipeData(bool toModel, CarrierEnum carrierID)
        {
            if (toModel)
            {
                xRecipe.ReleaseOrgBmps(save: true);
            }
            else
            {
                _currentCarrierID = carrierID;
                updateRecipePropertyView(carrierID);
                updatePlcCoordsRef(carrierID);
                updateRecipeOrgBmpToViewer(carrierID);
            }
        }
        void updateRecipeOrgBmpToViewer(CarrierEnum carrierID)
        {
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            string recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();
            string srcName = $"[參數] {recipeName} (bmpOrg @ {carrierID})";

            Bitmap bmpOrg = xRecipe.PeekOrgBmp(carrierID);
            
            if (bmpOrg != null)
            {
                _imgViewer.UpdateImage(bmpOrg, srcName, disposeSrc: false);
            }
            else
            {
                _imgViewer.UpdateImage(new Bitmap(800, 800, System.Drawing.Imaging.PixelFormat.Format8bppIndexed), srcName, disposeSrc: true);
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);
            updateGuiStatus();
        }
        void updateGoldenRegionToRecipe(Mat goldenRegionImg, Rect goldenRegionRect)
        {
            if (goldenRegionImg == null)
                return;

            //--------------------------------------------------------------------------------
            // 更新到 xRecipe 的 bmpprinttemplate
            // xRecipe 負責接手管理 goldenRegionBmp
            //--------------------------------------------------------------------------------
            Bitmap goldenRegionBmp = BitmapConverter.ToBitmap(goldenRegionImg);
            xRecipe.xRectRegionPrint = JetEazy.Qcvt.CC(goldenRegionRect);
            var old = xRecipe.bmpprinttemplate;
            xRecipe.bmpprinttemplate = goldenRegionBmp;
            old?.Dispose();

            //// 更新至 jxRecipeCombo
            //_jxRecipeCombo.GaGridParams.ChipGoldenRegionBmp.Value = (Bitmap)goldenRegionBmp?.Clone();
        }
        void updateRecipePropertyView(CarrierEnum carrierID)
        {
            //(1) 顯示 RecipeParaGridClass 參數
            if (_rcpEditUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                pg.SelectedObject = xParamGrid;
            }

            ////(2) 顯示 jxRecipeComboe
            //if (_rcpEditUI.wndVisionSettingsPanel is GwPanePropsViewer propsViewer)
            //{
            //    propsViewer.BuildGuiCtrls(_jxRecipeCombo);
            //    propsViewer.ExpandAll();
            //}

            xParamGrid.xStageNumber = (StageNumber)carrierID;
        }
        void updatePlcCoordsRef(CarrierEnum carrierID)
        {
            bool ok1 = _sysModel.GetCoordsRef(carrierID, SuckerRowEnum.S1, out var camCoord1, out var worldSucker1, out string msg1);
            bool ok2 = _sysModel.GetCoordsRef(carrierID, SuckerRowEnum.S2, out var camCoord2, out var worldSucker2, out string msg2);

            _rcpEditUI.UpdateCoordsRef(camCoord1, worldSucker1, worldSucker2);

            if (!ok1 || !ok2)
            {
                VsMSG.Instance.Warning(msg1 + "\n\r" + msg2, false);
            }
        }
        void showCviResult(bool show)
        {
            //_calibCtrl.ShowCviResult(show);
            //_cviGoldenRegionBox.Visible = false;
            _cviCamGridBox.Visible = show;
        }
        #endregion

        void LoadImage(string fileName = null)
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            if (fileName == null)
                fileName = GaUtil.BrowseImageFile();

            if (fileName == null)
                return;

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var bigBmp = GaImageUtil.LoadBigImage(fileName);

            if (bigBmp != null)
            {
                _isModified = true;
                xRecipe.TakeInOrgBmp(_currentCarrierID, bigBmp);
                _wndOwner.BeginInvoke(new Action(() => updateRecipeOrgBmpToViewer(_currentCarrierID)));
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void GrabImage()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                var bigBmp = freeBmp.ToBitmap();

                //var srcName = "[線掃相機] 擷圖";
                //_imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);

                if (bigBmp != null)
                {
                    _isModified = true;
                    xRecipe.TakeInOrgBmp(_currentCarrierID, bigBmp);
                    _wndOwner.BeginInvoke(new Action(() => updateRecipeOrgBmpToViewer(_currentCarrierID)));
                }
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void SaveImageOrg()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            var carrierID = this._currentCarrierID;

            // 從參數抓取 bmpOrg
            Bitmap srcBmp = xRecipe.PeekOrgBmp(carrierID);
            if(srcBmp == null)
            {
                VsMSG.Instance.Warning($"參數 @ {carrierID} 沒有影像", true);
                return;
            }

            // 選擇檔名
            string dstFileName = JzToolsClass.SaveFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(dstFileName))
                return;

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            bool ok = GaImageUtil.SaveBigImage(dstFileName, srcBmp);

            GaUtil.SetCursor(_wndOwner, oldCursor);

            if (ok)
                VsMSG.Instance.Warning($"完成保存圖片.\n\r檔名: {dstFileName}", false);
            else
                VsMSG.Instance.Warning($"無法保存圖片!\n\r檔名: {dstFileName}", true);
        }

        void OpenEmptyTrayInspectWindow()
        {
            // 關掉 Golden Picking 
            showCviResult(false);
            enableGoldenRegionPicking(false);

            CarrierEnum carrierID = _currentCarrierID;
            var bmpOrg = xRecipe.PeekOrgBmp(carrierID);
            GaMvcConfig.OpenEmptyTrayInspectTool(_wndOwner.FindForm(), bmpToShow: bmpOrg);
        }
        void OpenTemplateMatchWindow()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            using (var frm = new frmTemplateX3())
            {
                frm.ShowDialog();
            }

            updateAllRecipeData(false, _currentCarrierID);
        }
        void OpenFlyCameraRecipeEditor()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            using (var dlg = new frmFlySetup())
            {
                dlg.ShowDialog();
            }

            updateAllRecipeData(false, _currentCarrierID);
        }
        void OpenLightCtrlWindow()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            using (var dlg = new FormLightControl())
            {
                dlg.LightChannel = xParamGrid.xChNum;
                dlg.LightValue = xParamGrid.xChValue;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    xParamGrid.xChNum = dlg.LightChannel;
                    xParamGrid.xChValue = (int)dlg.LightValue;

                    updateRecipePropertyView(_currentCarrierID);
                }
            }
        }
        void WriteCoordsRefToPlc()
        {
            string msg;
            bool ok = _sysModel.WriteCoordsRefToPlc(out msg);
            if (ok)
                VsMSG.Instance.Tishi("座標成功寫入至 PLC.");
            else
                VsMSG.Instance.Warning(msg, true);
        }

        void BuildGoldenRegion()
        {
            showCviResult(false);
            // 直接使用 viewer 的影像 (OpenCvSharp 的 Mat)
            var imgSrc = _imgViewer.MatViewer.Image;            
            var goldenRegionRect = _cviGoldenRegionBox.Box;

            // ROI            
            GaUtil.Clip(ref goldenRegionRect, imgSrc.Width, imgSrc.Height);
            var goldenRoi = JetEazy.Qcvt.CV(goldenRegionRect);

            // 更新至 recipe
            updateGoldenRegionToRecipe(imgSrc[goldenRoi], goldenRoi);
        }
        void AutoCreateRegions()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);
            _imgViewer.MatViewer.Refresh();

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var carrierID = _currentCarrierID;

            // 從參數抓取 bmpOrg
            Bitmap srcBmp = xRecipe.PeekOrgBmp(carrierID);
            if (srcBmp == null)
            {
                VsMSG.Instance.Warning($"參數 @ {carrierID} 沒有影像", true);
                return;
            }

            // 偵測格點
            var result = _sysModel.DetectCameraGrid(srcBmp);
            var camGrid = result?.Grid;
            if (camGrid == null)
                return;

            // 建構 Region Cells
            _sysModel.BuildCellRegions(carrierID, camGrid, true);
            _isModified = true;
            
            // 更新 GUI
            _cviCamGridBox.IsEmptyTrayMode = true;
            _cviCamGridBox.UpdateResult(result);
            _cviCamGridBox.Visible = true;
            _imgViewer.MatViewer.Invalidate();

            // 更新 參數畫面
            updateRecipePropertyView(carrierID);

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }

        void LoadSettings(bool reloadGaara = false)
        {
            if (reloadGaara)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                if (reloadGaara)
                    xRecipe.Load();

                //_jxRecipeCombo.Load(null);
                //_sysModel.ApplyRecipe();

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void SaveSettings(bool force = false)
        {
            if (force || _isModified)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                xRecipe.Save();
                //_jxRecipeCombo.Save(null);
                //_sysModel.ApplyRecipe();

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void CloseWindow(bool confirm)
        {
            if (confirm)
            {
                // 保存更新的參數
                updateAllRecipeData(true, _currentCarrierID);
                SaveSettings();
                _wndOwner.DialogResult = DialogResult.OK;
            }
            else
            {
                // 還原舊值
                LoadSettings(true);
                _wndOwner.DialogResult = DialogResult.Cancel;
            }

            xRecipe.ReleaseOrgBmps(false);

            _wndOwner.Close();
        }
    }
}
