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
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.JxProps.Gui;
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
        //GaBigImageHolder _lineScanImageHolder => _sysModel.LineScanImageHolder;
        //TravellerTransforms _transforms => _sysModel.TransformsModel;
        #endregion

        #region RECIPES
        RecipeFPIX3Class xRecipe => RecipeFPIX3Class.Instance;
        RecipeParaGridClass xParamGrid => RecipeParaGridClass.Instance;
        JxRecipeCombo _jxRecipeCombo => _sysModel?.GetCurrentRecipe();
        #endregion

        #region INTERACTOR
        CviGoldenPickingBox _cviGoldenRegionBox = new CviGoldenPickingBox(Brushes.Orange) { Visible = false };
        CviCalibResultBox _cviRegionsBox = new CviCalibResultBox();
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

        Button btnGrabImage => _rcpEditUI.btnGrabImage;
        Button btnLoadImage => _rcpEditUI.btnLoadImage;
        Button btnSaveImage => _rcpEditUI.btnSaveImage;
        Button btnCancel => _rcpEditUI.btnCancel;
        Button btnOK => _rcpEditUI.btnOK;
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _currentCarrierID = CarrierEnum.C1;
        bool _isGoldenRegionPicking => _cviGoldenRegionBox.Visible;
        bool _isOrgBmpChanged = false;
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

            // 自動釋放所有資源
            _rcpEditUI.Window.HandleDestroyed += (s, e) => Dispose();

            // 延遲更新參數
            _rcpEditUI.Window.HandleCreated += (s, e) =>
            {
                var delayAction = new Action(() =>
                {
                    updateAllRecipeData(false, _currentCarrierID);
                    updateRecipeOrgBmpToViewer(_currentCarrierID);
                });

                _wndOwner.BeginInvoke(delayAction);
            };
        }
        #endregion

        #region EVENT_HANDLERS
        private void rdoCarrier_CheckedChanged(object sender, EventArgs e)
        {
            var carrierID = rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            updateAllRecipeData(false, carrierID);
        }
        private void LtAoi_OnLineScanRequested(object sender, EventArgs e)
        {
            //new Action(() =>
            //{
            //    //System.Threading.Thread.Sleep(2000);
            //    LtAoiFactory.PushBitmap(xRecipe.bmpOrg, "RecipeOrg");
            //}).BeginInvoke(null, null);
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateAllRecipeData(bool toModel, CarrierEnum carrierID = CarrierEnum.C1)
        {
            xParamGrid.xStageNumber = (StageNumber)carrierID;

            if (toModel)
            {
            }
            else
            {
                _currentCarrierID = carrierID;

                updateRecipePropertyView();

                updatePlcCoordsRef(false);
            }
        }
        void updateRecipePropertyView()
        {
            //(1) 顯示 jxRecipeComboe
            if (_rcpEditUI.wndVisionSettingsPanel is GwPanePropsViewer propsViewer)
            {
                propsViewer.BuildGuiCtrls(_jxRecipeCombo);
                propsViewer.ExpandAll();
            }

            //(2) 或是顯示 RecipeParaGridClass 參數
            if (_rcpEditUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                pg.SelectedObject = xParamGrid;
            }
        }
        void updateRecipeOrgBmpToViewer(CarrierEnum carrierID)
        {
            string srcName = "[參數] bmpOrg";
            Bitmap bmpOrg = xRecipe.bmpOrg;
            _imgViewer.UpdateImage(bmpOrg, srcName, false);
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
            var old = xRecipe.bmpprintFlytemplate;
            xRecipe.bmpprintFlytemplate = goldenRegionBmp;
            old?.Dispose();

            //// 更新至 jxRecipeCombo
            //_jxRecipeCombo.GaGridParams.ChipGoldenRegionBmp.Value = (Bitmap)goldenRegionBmp?.Clone();
        }
        void updatePlcCoordsRef(bool toModel)
        {
            //var traySettings = _jxRecipeCombo.EmptyTrayParams.TrayMiscSettings;
            //var rows = (int)traySettings.FullRows.Value;
            //var cols = (int)traySettings.FullCols.Value;
            //var pitchX = (double)traySettings.PitchX.Value;
            //var pitchY = (double)traySettings.PitchY.Value;

            //var transformsModel = GaMvcConfig.SysModel.TransformsModel;
            //var plcGrid = transformsModel.ConfigPlcGrid(rows, cols, pitchX, pitchY);

            //if (toModel)
            //{

            //}
            //else
            //{
            //    var trfCP = transformsModel.GetCameraPhysicTransform(_activeCarrierID);
            //    var trfCM = transformsModel.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);

            //    var plcPt = plcGrid[0, 0];
            //    var camPt = trfCP.InvTrans(plcPt);
            //    var motorPt = trfCM.Trans(camPt);

            //    _rcpEditUI.UpdateCoordsRef(camPt, motorPt);
            //}
        }
        void showCviResult(bool show)
        {
            //_calibCtrl.ShowCviResult(show);
            //_cviGoldenRegionBox.Visible = false;
            _cviRegionsBox.Visible = show;
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

        void LoadImage(string fileName = null)
        {
            enableGoldenRegionPicking(false);

            if (fileName == null)
                fileName = GaUtil.BrowseImageFile();

            if (fileName == null)
                return;

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var srcName = "[參數] " + System.IO.Path.GetFileName(fileName);
            var bigBmp = GaImageUtil.LoadBigImage(fileName);
            _imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);

            GaUtil.SetCursor(_wndOwner, oldCursor);
            updateGuiStatus();
        }
        void GrabImage()
        {
            enableGoldenRegionPicking(false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                var srcName = "[參數] 線掃相機擷圖";
                var bigBmp = freeBmp.ToBitmap();
                _imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void SaveImageOrg()
        {
            enableGoldenRegionPicking(false);

            // 直接使用 viewer 的影像 (OpenCvSharp 的 Mat) 存檔
            Mat img = _imgViewer.MatViewer.Image;
            if (img == null)
            {
                VsMSG.Instance.Warning("沒有影像", false);
                return;
            }

            // 選擇檔名
            string dstFileName = JzToolsClass.SaveFilePicker("JPG Files (*.jpg)|*.JPG|" + "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*", "");
            if (string.IsNullOrEmpty(dstFileName))
                return;

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            bool ok = img.SaveImage(dstFileName);
            GaUtil.SetCursor(_wndOwner, oldCursor);

            if (ok)
                VsMSG.Instance.Warning($"完成保存圖片.\n\r檔名: {dstFileName}", false);
            else
                VsMSG.Instance.Warning($"無法保存圖片!\n\r檔名: {dstFileName}", true);
        }

        void OpenEmptyTrayInspectWindow()
        {
            // 關掉 Golden Picking 
            enableGoldenRegionPicking(false);

            GaMvcConfig.OpenEmptyTrayInspectTool(_wndOwner.FindForm());

            MessageBox.Show("SysModel 需要進一步處理 空盤檢測的 結果!");

            //// 更新 Plc Grid
            //updatePlcGridConfig(true);

            //// 利用 GaCalibCtrl 執行空盤檢測 並且 自動抓取格點
            ////_calibCtrl.RunAutoFetch(true);

            //// 將 格點 回存 Recipe
            //var aoiModel = _calibCtrl.GetAoiModel();
            //var result = aoiModel.GetResult();
            //var grid = result?.Grid;
            //if (grid != null)
            //    _jxRecipeCombo.EmptyTrayParams.TrayMiscSettings.SetGoldenGrid(grid);
        }
        void OpenTemplateMatchWindow()
        {
            // 關掉 Golden Picking 
            enableGoldenRegionPicking(false);

            using (var frm = new frmTemplateX3())
            {
                frm.ShowDialog();
            }

            updateAllRecipeData(false);
        }
        void OpenFlyCameraRecipeEditor()
        {
            // 關掉 Golden Picking 
            enableGoldenRegionPicking(false);

            using (var dlg = new frmFlySetup())
            {
                dlg.ShowDialog();
            }
        }
        void OpenLightCtrlWindow()
        {
            // 關掉 Golden Picking 
            enableGoldenRegionPicking(false);

            using (var dlg = new FormLightControl())
            {
                dlg.LightChannel = xParamGrid.xChNum;
                dlg.LightValue = xParamGrid.xChValue;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    xParamGrid.xChNum = dlg.LightChannel;
                    xParamGrid.xChValue = (int)dlg.LightValue;
                    updateRecipePropertyView();
                }
            }
        }

        void BuildGoldenRegion()
        {
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
            enableGoldenRegionPicking(false);
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            _sysModel?.ApplyRecipe(null, true);
            updateAllRecipeData(false);

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }

        void LoadSettings(bool reloadGaara = false)
        {
            if (_jxRecipeCombo.Modified || reloadGaara)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                if (reloadGaara)
                    xRecipe.Load();

                _jxRecipeCombo.Load(null);
                //_sysModel.ApplyRecipe();

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void SaveSettings(bool force = false)
        {
            if (force || _jxRecipeCombo.Modified)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                xRecipe.Save();
                _jxRecipeCombo.Save(null);
                //_sysModel.ApplyRecipe();

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void CloseWindow(bool confirm)
        {
            if (confirm)
            {
                // 保存更新的參數
                updateAllRecipeData(true);
                SaveSettings();
                _wndOwner.DialogResult = DialogResult.OK;
            }
            else
            {
                // 還原舊值
                LoadSettings(true);
                _wndOwner.DialogResult = DialogResult.Cancel;
            }

            _wndOwner.Close();

            //由上層調用 _frmOwner.Dispose();
        }
    }
}
