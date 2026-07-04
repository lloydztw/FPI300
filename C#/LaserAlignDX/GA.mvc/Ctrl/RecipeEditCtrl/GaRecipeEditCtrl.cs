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
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Lang;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.FormSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.JxProps.PropertyMeta;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;


namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaRecipeEditCtrl
    {
        #region ENUMS
        enum FocusMode : int
        {
            [Description("對焦在晶粒表面")]
            FocusOnChip = ZPosDataSrc.FocusOnChip,
            [Description("對焦在空載台")]
            FocusOnCarrier = ZPosDataSrc.FocusOnCarrier,
        };
        #endregion

        #region GLOBAL_MESS
        IxLineScanCam _bigScanCamera
        {
            get => Traveller106.Universal.IxLineScan;
        }
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        #endregion

        #region RECIPES
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        RecipeParaGridClass _xParamGrid => RecipeParaGridClass.Instance;
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
        Button btnAutoCreateRegionsArray => _rcpEditUI.btnAutoCreateCellRegions;
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
        FocusMode _focusMode = FocusMode.FocusOnChip;
        CarrierEnum _currentCarrierID = CarrierEnum.C1;
        ITravellerTransforms _trfModel => GaMvcConfig.SysModel.TransformsModel;
        bool _isGoldenRegionPicking => _cviGoldenRegionBox.Visible;
        bool _isModified = false;
        #endregion

        public void Attach(IvRecipeEditorUI editorView)
        {
            _rcpEditUI = editorView;
            _wndOwner = _rcpEditUI.Window.FindForm();
            _cviCamGridBox.Attach(_trfModel);

            // 保存原有的 BackColor
            btnPickGoldenRegion.Tag = btnPickGoldenRegion.BackColor;

            attachFocusMotorCtrl();
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
            _rcpEditUI.OnSelectedChanged += rcpEditUI_OnSelectedIndexChanged;

            btnOK.Click += (s, e) => CloseWindow(confirm: true);
            btnCancel.Click += (s, e) => CloseWindow(confirm: false);

            btnSaveImage.Click += (s, e) => SaveImageOrg();
            btnLoadImage.Click += (s, e) => LoadImage();
            btnGrabImage.Click += (s, e) => GrabImage();

            rdoCarriers[0].CheckedChanged += rdoCarrier_CheckedChanged;
            btnOpenEmptyTrayWindow.Click += (s, e) => OpenEmptyTrayInspectWindow();
            btnPickGoldenRegion.Click += (s, e) => toggleGoldenRegionPicking();

            btnAutoCreateRegionsArray.Click += (s, e) => AutoCreateRegionsArray();
            _cviGoldenRegionBox.OnBoxSelected += (s, e) => BuildGoldenRegion();

            btnOpenTemplateMatchWindow.Click += (s, e) => OpenTemplateMatchWindow();
            btnOpenFlyCameraRecipeEditor.Click += (s, e) => OpenFlyCameraRecipeEditor();
            btnOpenLightCtrlWindow.Click += (s, e) => OpenLightCtrlWindow();
            btnWriteCoordsRefToPlc.Click += (s, e) => WriteCoordsRefToPlc();

            //// Camera Focus Motor
            //if (btnFocusMotorSettings != null)
            //    btnFocusMotorSettings.Click += (s, e) => OpenFocusMotorWindow();
            //if (btnFocusMotorGo != null)
            //    btnFocusMotorGo.Click += (s, e) => MoveFocusMotorToRecipePos();

            // ProperyGrid
            if ( _rcpEditUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;

            // 自動釋放所有資源
            _rcpEditUI.Window.HandleDestroyed += (s, e) => Dispose();

            // 延遲更新參數
            _rcpEditUI.Window.HandleCreated += (s, e) =>
            {
                new Action(() =>
                {
                    System.Threading.Thread.Sleep(100);
                    postInit();
                }).BeginInvoke(null, null);
            };
        }
        void postInit()
        {
            if (_wndOwner.InvokeRequired)
            {
                _wndOwner.BeginInvoke((Action)postInit);
            }
            else
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                _wndOwner.Refresh();

                makeSomeReadonlyInGaaraRecipe();
                updateAllRecipeData(false, _currentCarrierID = _sysModel.ActiveCarrierID);

                _rcpEditUI.rdoCarriers[(int)_currentCarrierID].Checked = true;

                // 初始態: FocusOnChip (對焦在晶粒表面)
                _rcpEditUI.SelectedIndex = 1;

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        #endregion

        #region EVENT_HANDLERS
        private void rcpEditUI_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            int index = _rcpEditUI.SelectedIndex;
            if (index < 0)
                return;

            var focusMode = index == 0 ? FocusMode.FocusOnCarrier : FocusMode.FocusOnChip;
            ChangeFocusMode(focusMode);
        }
        private void rdoCarrier_CheckedChanged(object sender, EventArgs e)
        {
            var carrierID = rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            ChangeActiveCarrier(carrierID);
        }
        private void LtAoi_OnLineScanRequested(object sender, EventArgs e)
        {
            new Action(() =>
            {
                System.Threading.Thread.Sleep(500);
                var focusOnEmptyCarrier = _focusMode == FocusMode.FocusOnCarrier;
                var bmpOrg = _xRecipe.PeekBmpOrg(_currentCarrierID, focusOnEmptyCarrier);
                var srcName = $"bmpOrg @ {_currentCarrierID}";
                GaMvcConfig.PushBitmapToEmptyTrayTool(bmpOrg, srcName);
            }).BeginInvoke(null, null);
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // 取得被改變的屬性名稱
            //string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;
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
            btnAutoCreateRegionsArray.Enabled = hasImage;
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
        void makeSomeReadonlyInGaaraRecipe()
        {
            // 在設定 SelectedObject 之前，將 一些 屬性設為唯讀
            var readonlyPropNames = new List<string>()
            {
                "xAngle",
                "xRow",
                "xColumn",
                "xLeftTopX",
                "xLeftTopY",
                "xRowOffset",
                "xColumnOffset",
                "xChipWidth",
                "xChipHeight",
                "xRealLeftX",
                "xRealLeftY",
                "xRealOffsetX",
                "xRealOffsetY",
                "xStageNumber",
                "zFocusOnCarrier",
                "zFocusOnChip",
            };

            TypeDescriptor.AddProvider(
                new CustomReadOnlyTypeDescriptionProvider(_xParamGrid.GetType(), readonlyPropNames),
                _xParamGrid
            );
        }
        void updateAllRecipeData(bool toModel, CarrierEnum carrierID)
        {
            if (toModel)
            {
                //>>> xRecipe.ReleaseBmpsOrg(save: true);
            }
            else
            {
                _currentCarrierID = carrierID;
                updateRecipePropertyView(carrierID);
                updatePlcCoordsRef(_trfModel, carrierID);
                updateRecipeOrgBmpToViewer(carrierID);
                syncFocusMotorCtrl();
            }
        }
        void updateRecipeOrgBmpToViewer(CarrierEnum carrierID)
        {
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            string recipeName = LtAoiFactory.GetActiveRecipeNameAtFPI30();

            //string srcName = $"[參數] {recipeName} (bmpOrg @ {carrierID})";
            //Bitmap bmpOrg = _xRecipe.PeekBmpOrg(carrierID);
            (var srcName, var bmpOrg) = PeekBmp(carrierID, _focusMode);
            srcName = $"[參數] {recipeName} ({srcName})";
            
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
            _xRecipe.xRectRegionPrint = JetEazy.Qcvt.CC(goldenRegionRect);
            var old = _xRecipe.bmpprinttemplate;
            _xRecipe.bmpprinttemplate = goldenRegionBmp;
            old?.Dispose();

            //// 更新至 jxRecipeCombo
            //_jxRecipeCombo.GaGridParams.ChipGoldenRegionBmp.Value = (Bitmap)goldenRegionBmp?.Clone();
        }
        void updateRecipePropertyView(CarrierEnum carrierID)
        {
            //(1) 顯示 RecipeParaGridClass 參數
            if (_rcpEditUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                try
                {
                    PGTranslator.Register(_xParamGrid);
                    pg.SelectedObject = _xParamGrid;
                }
                catch (Exception ex)
                {
                    string errMsg = "Translate(_xParamGrid) Error";
                    errMsg += "\n\r\n\r" + ex.Message;
                    errMsg += "\n\n" + ex.StackTrace;
                    MessageBox.Show(errMsg, GlobalConfig.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            ////(2) 顯示 jxRecipeComboe
            //if (_rcpEditUI.wndVisionSettingsPanel is GwPanePropsViewer propsViewer)
            //{
            //    propsViewer.BuildGuiCtrls(_jxRecipeCombo);
            //    propsViewer.ExpandAll();
            //}

            _xParamGrid.xStageNumber = (StageNumber)carrierID;
        }
        void updatePlcCoordsRef(ITravellerTransforms trfModel, CarrierEnum carrierID)
        {
            (var err, var errMsg) = trfModel.GetCoordsRef(carrierID, out var camCoord, out var s1MotorCoord, out var s2MotorCoord);

            _rcpEditUI.UpdateCoordsRef(camCoord, s1MotorCoord, s2MotorCoord);

            if (err != Model.ErrorCodes.OK)
            {
                //VsMSG.Instance.Warning(errMsg, true);
                VsMessageBox.Warning(errMsg);
            }
        }
        void showCviResult(bool show)
        {
            //_calibCtrl.ShowCviResult(show);
            //_cviGoldenRegionBox.Visible = false;
            _cviCamGridBox.Visible = show;
        }
        #endregion

        (string, Bitmap) PeekBmp(CarrierEnum C, FocusMode focus)
        {
            var empty = focus == FocusMode.FocusOnCarrier;
            var bmp = _xRecipe.PeekBmpOrg(C, empty);

            var srcName = empty ?
                $"bmpOrgE @ {_currentCarrierID}":
                $"bmpOrg @ {_currentCarrierID}";

            return (srcName, bmp);
        }

        void ChangeFocusMode(FocusMode newFocusMode)
        {
            if (_focusMode != newFocusMode)
            {
                _focusMode = newFocusMode;

                syncFocusMotorCtrl(autoMoveZ: true);

                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                updateRecipeOrgBmpToViewer(_currentCarrierID);

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void ChangeActiveCarrier(CarrierEnum C)
        {
            if (C != _currentCarrierID)
            {
                showCviResult(false);
                enableGoldenRegionPicking(false);

                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                _sysModel.ActiveCarrierID = C;
                _sysModel.ApplyRecipe();
                updateAllRecipeData(false, C);

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void LoadImage(string fileName = null)
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            if (fileName == null)
            {
                fileName = GaUtil.BrowseImageFile();
                if (fileName == null) return;
            }
            else
            {
                if (string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(fileName))
                    return;
            }

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var bigBmp = GaImageUtil.LoadBigImage(fileName, autoSaveJpg: true);

            if (bigBmp != null)
            {
                _isModified = true;
                _xRecipe.TakeInBmpOrg(_currentCarrierID, (_focusMode == FocusMode.FocusOnCarrier), bigBmp);
                _wndOwner.BeginInvoke(new Action(() => updateRecipeOrgBmpToViewer(_currentCarrierID)));
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void GrabImage()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var freeBmp = _bigScanCamera.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                var bigBmp = freeBmp.ToBitmap();

                //var srcName = "[線掃相機] 擷圖";
                //_imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);

                if (bigBmp != null)
                {
                    _isModified = true;
                    _xRecipe.TakeInBmpOrg(_currentCarrierID, (_focusMode == FocusMode.FocusOnCarrier), bigBmp);
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
            //Bitmap srcBmp = _xRecipe.PeekBmpOrg(carrierID);
            (var _, var srcBmp) = PeekBmp(carrierID, _focusMode);

            if (srcBmp == null)
            {
                VsMessageBox.Warning($"參數 @ {carrierID} 沒有影像");
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
                //VsMSG.Instance.Warning($"完成保存圖片.\n\r檔名: {dstFileName}", false);
                VsMessageBox.Info($"完成保存圖片.\n\r檔名: {dstFileName}");
            else
                //VsMSG.Instance.Warning($"無法保存圖片!\n\r檔名: {dstFileName}", true);
                VsMessageBox.Warning($"無法保存圖片!\n\r檔名: {dstFileName}");
        }

        void OpenEmptyTrayInspectWindow()
        {
            // 關掉 Golden Picking 
            showCviResult(false);
            enableGoldenRegionPicking(false);

            //CarrierEnum carrierID = _currentCarrierID;
            //var bmpOrg = _xRecipe.PeekBmpOrg(carrierID);
            (var _, var bmpOrg) = PeekBmp(_currentCarrierID, _focusMode);

            GaMvcConfig.OpenEmptyTrayInspectTool(_wndOwner.FindForm(), bmpToShow: bmpOrg);
        }
        void OpenTemplateMatchWindow()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            GaMvcConfig.OpenTamplateEditor(_currentCarrierID);

            updateAllRecipeData(false, _currentCarrierID);
        }
        void OpenFlyCameraRecipeEditor()
        {
            showCviResult(false);
            enableGoldenRegionPicking(false);

            using (var dlg = new FormFlySetup())
            {
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.Size = _wndOwner.Size;
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
                dlg.LightChannel = _xParamGrid.xChNum;
                dlg.LightValue = _xParamGrid.xChValue;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _xParamGrid.xChNum = dlg.LightChannel;
                    _xParamGrid.xChValue = (int)dlg.LightValue;

                    updateRecipePropertyView(_currentCarrierID);
                }
            }
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
        #region OLD_CODE
        void __AutoUpdateRegions()
        {
#if(OPT_REMARK_2026_0308)
            //-------------------------------------
            // 2025-12-11 針對 校正塊 改版
            //-------------------------------------

            showCviResult(false);
            enableGoldenRegionPicking(false);
            _imgViewer.MatViewer.Refresh();

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var carrierID = _currentCarrierID;

            // 從參數取得 camGrid
            var camGrid = carrierID == CarrierEnum.C1 ? _xRecipe.xCamGrid1 : _xRecipe.xCamGrid2;
            if (camGrid == null)
            {
                VsMessageBox.Warning($"請為 載台{GaUtil.GetEnumDescription(carrierID)} 進行校正!");
                return;
            }

            // 更新 CoordRef
            updatePlcCoordsRef(carrierID);

            #region NOT_USED_CODE
            //// 設定旗標
            //_isModified = true;
            //// 強制保存
            //_xRecipe.SaveCameraGrids();
            #endregion

            // 更新 GUI
            var blocs = new List<EzBloc>();
            blocs.AddRange(camGrid.IterBlocs());
            var matchResult = new EzAoiEmptyTrayInspector.Model.MatchResult((int)carrierID, camGrid, blocs);

            _cviCamGridBox.TransCameraToWorld = _sysModel.TransformsModel.GetCameraPhysicTransform(carrierID);
            _cviCamGridBox.IsEmptyTrayMode = true;
            //_cviCamGridBox.IsEmptyTrayMode = false;
            _cviCamGridBox.UpdateResult(matchResult);
            _cviCamGridBox.Visible = true;
            _imgViewer.MatViewer.Invalidate();

            // 更新 參數畫面
            updateRecipePropertyView(carrierID);

            GaUtil.SetCursor(_wndOwner, oldCursor);
#endif
        }
        #endregion
        void AutoCreateRegionsArray()
        {
            //---------------------------------------------------------------
            // 2026-03-08 針對個別參數 進行 座標轉換系統 線性 遷移
            //---------------------------------------------------------------
            bool migrate = VsMessageBox.Question(GaUtil.GetEnumDescription(Prompts.Question_To_Migrate_Transforms_Models)) == DialogResult.OK;

            // 鼠標 (忙碌)
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            try
            {
                showCviResult(false);
                enableGoldenRegionPicking(false);
                _imgViewer.MatViewer.Refresh();

                // 載台號
                var carrierID = _currentCarrierID;

                // 從參數取得 bmpOrg
                // Bitmap srcBmp = _xRecipe.PeekBmpOrg(carrierID);
                (var srcName, var srcBmp) = PeekBmp(carrierID, _focusMode);

                if (srcBmp == null)
                {
                    //VsMSG.Instance.Warning($"參數 @ {carrierID} 沒有影像", true);
                    VsMessageBox.Warning($"{GaUtil.GetEnumDescription(carrierID)} : 參數沒有 {srcName} 影像!");
                    return;
                }

                // 自動抓取陣列 (並進行 座標轉換系統 線性遷移)
                _sysModel.ActiveCarrierID = carrierID;
                bool ok = _sysModel.AutoBuildRegionCells(carrierID, srcBmp, migrate, out var result);
                if (!ok)
                {
                    // _sysModel 內部會自動發出報警 Event
                    return;
                }

                // 線性遷移後 的 座標轉換系統
                var trfModel = _sysModel.TransformsModel;

                // 更新 PlcCoordRef 到 GUI (個別參數 準被寫給 PLC 的馬達參考座標點)
                updatePlcCoordsRef(trfModel, carrierID);

                // 設定旗標
                _isModified = true;

                // 參數 強制保存
                trfModel?.Save(GaMvcPaths.TRANSFORMS_INI_FILE(_xRecipe.INIFILE));

                // 更新 格位陣列 到 GUI
                _cviCamGridBox.Attach(trfModel);
                _cviCamGridBox.ActiveCarrierID = carrierID;
                _cviCamGridBox.TransCameraToWorld = trfModel.GetCameraPhysicTransform(carrierID);
                _cviCamGridBox.TransCameraToMotor = trfModel.GetCameraMotorTransform(carrierID, SuckerRowEnum.S1);
                _cviCamGridBox.TransCameraToMotor2 = trfModel.GetCameraMotorTransform(carrierID, SuckerRowEnum.S2);
                _cviCamGridBox.IsEmptyTrayMode = true;
                _cviCamGridBox.UpdateResult(result);
                _cviCamGridBox.Visible = true;
                _imgViewer.MatViewer.Invalidate();

                // 更新 參數畫面
                updateRecipePropertyView(carrierID);

                //// 是否直接把 CoordsRef 寫入PLC ?
                //var ret = VsMessageBox.Question(GaUtil.GetEnumDescription(Prompts.Question_Write_CoordRefs_To_PLC));
                //if (ret == DialogResult.Yes)
                //    WriteCoordsRefToPlc();

            }
            catch(Exception ex)
            {
                // Model 內部已經有 NLOG 了
                // LtDebug.LOG.Error(ex, "[異常] 自動生成陣列");
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "自動生成陣列異常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            finally
            {
                // 鼠標 (恢復) 
                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void WriteCoordsRefToPlc()
        {
            bool ok = _sysModel.WriteAllCoordsToPlc(out string errMsg);
            if (ok)
                //VsMSG.Instance.Tishi("座標成功寫入至 PLC.");
                VsMessageBox.Info(GaUtil.GetEnumDescription(Prompts.Info_Write_CoordRefs_To_PLC_OK));
            else
                //VsMSG.Instance.Warning(msg, true);
                VsMessageBox.Warning(errMsg);
        }

        void LoadSettings(bool reloadGaara = false)
        {
            if (reloadGaara)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                _xRecipe.Load();
                //>>> _sysModel.TransformsModel?.Load(GaMvcPaths.TRANSFORMS_INI_FILE(_xRecipe.INIFILE));

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void SaveSettings(bool force = false)
        {
            if (force || _isModified)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                _isModified = false;
                _xRecipe.Save();
                //_sysModel.TransformsModel?.Save(GaMvcPaths.TRANSFORMS_INI_FILE(_xRecipe.INIFILE));

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }
        void CloseWindow(bool confirm)
        {
            if (_wndOwner == null || !_wndOwner.IsHandleCreated)
                return;

            var oldCursor = GaUtil.SetCursor (_wndOwner, Cursors.WaitCursor);

            if (confirm)
            {
                //(1) 保存更新的參數
                updateAllRecipeData(true, _currentCarrierID);
                _xRecipe.ReleaseBmpsOrg(save: true);
                SaveSettings();
                _wndOwner.DialogResult = DialogResult.OK;
            }
            else
            {
                //(2) 還原舊值
                LoadSettings(true);
                _xRecipe.ReleaseBmpsOrg(save: false);
                _wndOwner.DialogResult = DialogResult.Cancel;
            }

            GaUtil.SetCursor(_wndOwner, oldCursor);

            //(3) 最後必須讓 Focus Motor 對焦到晶粒表面 !!!
            _focusMotorCtrl.SetDataSrc((ZPosDataSrc)FocusMode.FocusOnChip);
            _focusMotorCtrl.MoveMotorToPosHolder();
            _focusMotorCtrl = null;

            //(4) 關閉視窗
            _wndOwner.Close();
        }
    }


    partial class GaRecipeEditCtrl
    {
        GaMotorZCtrl _focusMotorCtrl = null;
        void attachFocusMotorCtrl()
        {
            // 只允許 attach 一次
            if (_focusMotorCtrl != null)
                return;

            var view = _rcpEditUI.wndFocusMotorGoPanel;

            _focusMotorCtrl = new GaMotorZCtrl();
            _focusMotorCtrl.Attach(view);
            _focusMotorCtrl.SetDataSrc((ZPosDataSrc)_focusMode);
            _focusMotorCtrl.OnPosDataSrcModified += (s, e) =>
            {
                _isModified = true;
            };
        }
        void syncFocusMotorCtrl(bool autoMoveZ = false)
        {
            if (_focusMotorCtrl == null)
                return;

            _focusMotorCtrl.SetDataSrc((ZPosDataSrc)_focusMode);
            _focusMotorCtrl.UpdatePosHolderToGui();

            if (autoMoveZ)
            {
                _wndOwner.BeginInvoke((Action)_focusMotorCtrl.MoveMotorToPosHolder);
            }
        }
    }
}
