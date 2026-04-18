#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-06 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Match;
using JetEazy.OpenCV.Viewer;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LeTian.JxProps;
using LeTian.JxProps.Gui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CviBoundBox = EzAoiEmptyTrayInspector.Ctrl.CviRcpBox;
using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;
using QCoord = JetEazy.QMath.QVector;

namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaCalibCtrl
    {
        #region CONSTANTS
        static int N_CALIB_MOTOR_POINTS => TravellerTransformFactory.N_CALIB_MOTOR_POINTS;
        static int N_CARRIERS_NUMBER => Enum.GetValues(typeof(CarrierEnum)).Length;
        #endregion

        #region ENUM
        enum CalibCornersEnum : int
        {
            [Description("載台左上")]
            LeftTop,
            [Description("載台右上")]
            RightTop,
            [Description("載台右下")]
            RigthBottom,
            [Description("載台左下")]
            LeftBottom
        };
        enum CalibViewEnum : int
        {
            [Description("格點 校正板")]
            BigGridBoardView,
            [Description("點墨 校正塊")]
            InkMarksView,
        };
        #endregion

        #region ACTIVE_MODES
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        CalibViewEnum _activeViewID = CalibViewEnum.BigGridBoardView;
        #endregion

        #region GLOBAL_PATH
        internal static string CALIB_TRANSFORMS_FILE => GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE;
        internal static string CALIB_RECIPE_FILE(CarrierEnum C, params object[] dummyArgs)
        {
            return GaMvcPaths.CALIB_RECIPE_FILE(C);
        }
        internal static string CALIB_LAST_IMAGE_FILE(CarrierEnum C, SuckerRowEnum S, int viewID)
        {
            var path = System.IO.Path.GetDirectoryName(CALIB_RECIPE_FILE(C));
            string imgFile;
            if (viewID == (int)CalibViewEnum.BigGridBoardView)
                imgFile = System.IO.Path.Combine(path, $"jx_calib_BigBoard_image_@{C}.jpg");
            else
                imgFile = System.IO.Path.Combine(path, $"jx_calib_InkMarks_image_@{C}#{S}.jpg");
            return imgFile;
        }
        #endregion

        #region GLOBAL_CAMERA_MESS
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        #endregion

        #region CALIB_MODEL
        ITravellerTransforms _commonBaseTrf => TravellerTransformFactory.CommonBase;
        ICalibAoiModel _calibModel => GaMvcConfig.SysModel.CalibAoiModel;
        JxCalibRecipe[] _jxCalibRecipes = new JxCalibRecipe[N_CARRIERS_NUMBER];
        #endregion

        #region GUI_LINKS
        IvCalibToolUI _calibToolUI;
        Control _wndOwner => _calibToolUI.Window;
        JezTransImageViewPanel _getImgViewPanel(CalibViewEnum vid)
        {
            return _calibToolUI.ImgViewers[(int)vid];
        }
        CvMatViewer _getMatViewer(CalibViewEnum vid)
        {
            return _getImgViewPanel(vid)?.MatViewer;
        }
        bool _isEmptyImage(CalibViewEnum vid)
        {
            var matViewer = _getMatViewer(vid);
            if (matViewer == null || matViewer.Image == null)
                return true;
            return false;
        }

        GvCalibPointsDataGridView _dgvCalibPointsListView => _calibToolUI.dgvCalibPointsListView;
        RadioButton[] _rdoCarriers => _calibToolUI.rdoCarriers;
        RadioButton[] _rdoSuckerRows => _calibToolUI.rdoSuckerRows;
        Button _btnGrabImage => _calibToolUI.btnGrabImage;
        Button _btnLoadImage => _calibToolUI.btnLoadImage;
        Button _btnRunAutoFetchGrid => _calibToolUI.btnAutoFetchGrid;
        Button _btnAutoFetchInkMarks => _calibToolUI.btnAutoFetchInkMarks;
        Button _btnBuildCalib => _calibToolUI.btnBuildCalib;
        Button _btnOpenMotorXY => _calibToolUI.btnOpenMotorXY;

        Button _btnCancel => _calibToolUI.btnCancel;
        Button _btnOK => _calibToolUI.btnOK;
        #endregion

        #region INTERACTORS
        CviBoundBox _cviBigBoundBox = new CviBoundBox(Brushes.Blue, 1, 3) { Visible = true };
        CviCalibResultBox _cviGridResultBox = new CviCalibResultBox() { Visible = false };
        CviCalibPointBox[] _cviInkMarkBoxes = new CviCalibPointBox[N_CALIB_MOTOR_POINTS];
        #endregion

        #region RUNTIME_DATA
        bool _isCoordModified = false;
        bool _isRunning = false;        // 因為目前是單執行緒, 所以此變量用處不大.
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            _cviGridResultBox.Attach(TravellerTransformFactory.CommonBase);

            //_btnPickupGolden.Tag = _btnPickupGolden.BackColor;

            instanceRecipes();
            initImgViewers();
            dgvInit();

            LoadSettings();
            connectEventHandlers();
            
            attachFocusMotorCtrl();

            _isCoordModified = false;
        }

        #region PRIVATE_INIT_FUNCTIONS
        void CleanUp()
        {
            CalibAoiModel.OPT_DUMP = false;
            disposeRecipes();
        }
        void initImgViewers()
        {
            foreach(CalibViewEnum vid in Enum.GetValues(typeof(CalibViewEnum)))
            {
                var matViewer = _getMatViewer(vid);
                if (vid == CalibViewEnum.BigGridBoardView)
                {
                    matViewer.AddInteractor(_cviBigBoundBox);
                    matViewer.AddInteractor(_cviGridResultBox);
                }
                else
                {
                    matViewer.AddInteractor(_cviBigBoundBox);
                    for (int i = 0, NP = _cviInkMarkBoxes.Length; i < NP; i++)
                    {
                        var box = _cviInkMarkBoxes[i] = new CviCalibPointBox(RectangleF.Empty, Color.Lime) { Visible = false };
                        box.CrossColor = Color.Green;
                        box.CrossLength = 500;
                        matViewer.AddInteractor(box);
                    }
                }
            }
        }
        void connectEventHandlers()
        {
            if (_btnOK != null)
                _btnOK.Click += (s, e) => CloseWindow(true);

            if (_btnCancel != null)
                _btnCancel.Click += (s, e) => CloseWindow(false);

            _calibToolUI.OnActiveViewChanged += _calibToolUI_OnActiveViewChanged;

            if (_rdoCarriers != null)
                _rdoCarriers[0].CheckedChanged += _rdoSelect_CheckedChanged;

            if (_rdoSuckerRows != null)
                _rdoSuckerRows[0].CheckedChanged += _rdoSelect_CheckedChanged;

            _btnGrabImage.Click += (s, e) => GrabImage();
            _btnLoadImage.Click += (s, e) => LoadImage();

            if (_btnRunAutoFetchGrid != null)
                _btnRunAutoFetchGrid.Click += (s, e) => RunAutoFetchBoardGrid();

            if (_btnAutoFetchInkMarks != null)
                _btnAutoFetchInkMarks.Click += (s, e) => RunAutoFetchInkMarks();

            if (_btnBuildCalib != null)
                _btnBuildCalib.Click += (s, e) => BuildCommonBaseTransforms();

            // Motors
            if (_btnOpenMotorXY != null)
                _btnOpenMotorXY.Click += (s, e) => OpenMotorWindowXY();

            // Interactors
            _cviGridResultBox.OnRequestDumpBindaryImage += (s, e) => RunAutoFetchBoardGrid(dump: true);
            _cviBigBoundBox.OnChanged += _cviBigBoundBox_OnChanged;

            // ImgViewrs' EventHanlders
            _getMatViewer(CalibViewEnum.BigGridBoardView).MouseMove += MatViewer1_MouseMove;
            _getMatViewer(CalibViewEnum.InkMarksView).MouseMove += MatViewer2_MouseMove;

            // 自動釋放資源
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();

            // 延遲更新
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    //>>> updateAllData(false);
                    SetActiveView(_activeCarrierID, _activeSuckerRowID, _activeViewID, force: true);
                    syncFocusMotorCtrl();
                }));
            };
        }
        #endregion

        #region EVENT_HANDLERS
        private void _calibToolUI_OnActiveViewChanged(object sender, EventArgs e)
        {
            var vid = (CalibViewEnum)_calibToolUI.ActiveViewID;
            if (vid != _activeViewID)
            {
                SetActiveView(_activeCarrierID, _activeSuckerRowID, vid);
            }
        }
        private void _rdoSelect_CheckedChanged(object sender, System.EventArgs e)
        {
            var C = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            var S = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;
            var vid = _activeViewID;
            if (C == _activeCarrierID && S == _activeSuckerRowID)
                return;

            SetActiveView(C, S, vid);

            //if (Array.IndexOf(_rdoCarriers, sender) >= 0)
            //{
            //    updateVisionGridParams(_activeCarrierID, false, toReloadRecipe: true);
            //}
            //if (vid == CalibViewEnum.BigGridBoardView)
            //{
            //    updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);
            //    clearCviGridResults();
            //    _cviGridResultBox.ActiveCarrierID = _activeCarrierID;
            //    _cviGridResultBox.ActiveSuckerRowID = _activeSuckerRowID;
            //}
        }
        private void _cviBigBoundBox_OnChanged(object sender, EventArgs e)
        {
            updateBoundRectBox(true);
        }
        private void MatViewer1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dgvCalibPointsListView == null)
                return;

            _dgvCalibPointsListView.SelectedIndex = -1;
        }
        private void MatViewer2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dgvCalibPointsListView == null)
                return;

            if (_cviInkMarkBoxes == null)
                return;

            int x = e.X;
            int y = e.Y;

            var matViewer = _getMatViewer(CalibViewEnum.InkMarksView);
            matViewer.TransCoordToWorld(ref x, ref y);

            for (int i = 0; i < N_CALIB_MOTOR_POINTS; i++)
            {
                var box = _cviInkMarkBoxes[i];
                if (box == null) continue;

                var rect = box.Quad2D.BoundaryRect;
                if (rect.Contains(x, y))
                {
                    _dgvCalibPointsListView.SelectedIndex = i;
                    return;
                }
            }

            _dgvCalibPointsListView.SelectedIndex = -1;
        }
        private void Dgv_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
#if(OPT_REPLACED_BY_MOTOR_JOG_TOOL)
            if (_activeViewID == CalibViewEnum.InkMarksView)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 0)
                {
                    dgvSyncCurrentMotorCoordsToUserInput(e.RowIndex);
                }
            }
#endif
        }
        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (true) // if(_activeViewID == CalibViewEnum.InkMarksView)
            {
                if (e.ColumnIndex == 0 && e.RowIndex >= 0 && e.RowIndex < 4)
                {
                    dgvGetUserInputMotorCoords(out var motorCoords);
                    var targetCoord = motorCoords[e.RowIndex];
                    MoveMotorXY(targetCoord);
                }
            }
        }
        private void Dlg_OnInkerCoordsUpdated(object sender, InkerCoordsEventArgs e)
        {
            int cornerId = e.CornerID;
            if (cornerId >= 0)
            {
                dgvSyncCurrentMotorCoordsToUserInput(cornerId, e.MotorCoord);
            }
        }
        private void Dlg_OnQueryInkerCoords(object sender, InkerCoordsEventArgs e)
        {
            e.MotorCoord = null;
            int cornerId = e.CornerID;
            if (cornerId >= 0)
            {
                dgvGetUserInputMotorCoords(out var motorCoords);
                if (motorCoords != null && motorCoords.Length > cornerId)
                    e.MotorCoord = motorCoords[cornerId];
            }
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateAllData(bool toModel)
        {
            dgvUpdateAllCalibKeyPoints(toModel);
            updatePropertyPanel(toModel);
            //updateFocusMotorPos(toModel);
        }
        void updatePropertyPanel(bool toModel)
        {
            var vid = _activeViewID;
            if (vid == CalibViewEnum.BigGridBoardView)
            {
                updateVisionGridParams(toModel);
            }
            else
            {
                updateVisionInkMarkParams(toModel);
            }
        }
        void updateVisionGridParams(bool toModel)
        {
            var pgvPanel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (pgvPanel == null)
                return;

            //var carrierID = _activeCarrierID;
            //var suckerID = _activeSuckerRowID;
            //var vid = _activeViewID;

            var jxRecipe = getActiveCalibRecipe();   // _jxCalibRecipes[(int)carrierID];
            if (jxRecipe == null)
                return;

            if (toModel)
            {
            }
            else
            {
                var jx = jxRecipe.GridSettings;
                pgvPanel.BuildGuiCtrls(jx);
                pgvPanel.ExpandAll();
            }
        }
        void updateVisionInkMarkParams(bool toModel)
        {
            var pgvPanel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (pgvPanel == null)
                return;

            var carrierID = _activeCarrierID;
            var suckerID = _activeSuckerRowID;

            var jxRecipe = getActiveCalibRecipe(); // _jxCalibRecipes[(int)carrierID];
            if (jxRecipe == null)
                return;

            if (toModel)
            {
            }
            else
            {
                if (_activeViewID == CalibViewEnum.InkMarksView)
                {
                    var jx = suckerID == SuckerRowEnum.S1 ? jxRecipe.InkMarkSettings1 : jxRecipe.InkMarkSettings2;
                    pgvPanel.BuildGuiCtrls(jx);
                    pgvPanel.ExpandAll();
                }
            }
        }
        void updateGuiStatus()
        {
            var vid = _activeViewID;
            var imgPanel = _getImgViewPanel(vid);    // _calibToolUI.ImgViewers[viewID];
            bool isEmpty = _isEmptyImage(vid);
            bool isInkMarkMode = vid == CalibViewEnum.InkMarksView;

            _btnGrabImage.Enabled = !_isRunning;
            _btnLoadImage.Enabled = !_isRunning;

            _btnRunAutoFetchGrid.Visible = !_isRunning && !isInkMarkMode;
            _btnAutoFetchInkMarks.Visible = !_isRunning && isInkMarkMode;
            _btnBuildCalib.Visible = !_isRunning && isInkMarkMode;

            var bkColor = isInkMarkMode ? Color.Black : Color.DimGray;
            foreach (int col in new[] { 3, 4 })
            {
                _dgvCalibPointsListView.SetReadOnly(col, !isInkMarkMode, bkColor);
            }
            _dgvCalibPointsListView.Enabled = isInkMarkMode;
        }
        #endregion

        #region RECIPES_FUNCTIONS
        JxCalibRecipe getActiveCalibRecipe()
        {
            return _jxCalibRecipes[(int)_activeCarrierID];
        }
        void instanceRecipes()
        {
            for (int i = 0, N = _jxCalibRecipes.Length; i < N; i++)
            {
                var old = _jxCalibRecipes[i];
                _jxCalibRecipes[i] = new JxCalibRecipe();
                old?.Dispose();
            }
        }
        void disposeRecipes()
        {
            for (int i = 0, N = _jxCalibRecipes.Length; i < N; i++)
            {
                var old = _jxCalibRecipes[i];
                disconnectPropEventHandlers(old);
                _jxCalibRecipes[i] = null;
                old?.Dispose();
            }
        }
        void connectPropEventHandlers(JxCalibRecipe rcp)
        {
            rcp.GridSettings.EmptyTraySettings.OnModified += EmptyTraySettings_OnModified;
        }
        void disconnectPropEventHandlers(JxCalibRecipe rcp)
        {
            rcp.GridSettings.EmptyTraySettings.OnModified += EmptyTraySettings_OnModified;
        }
        private void EmptyTraySettings_OnModified(object sender, EventArgs e)
        {
            // 強制同步 FullRows, FullCols, PitchX, PitchY
            foreach(var jxSrc in _jxCalibRecipes)
            {
                if (check_jx_contains(jxSrc.GridSettings.EmptyTraySettings, sender))
                {
                    foreach (var jx in _jxCalibRecipes)
                    {
                        if (jx != jxSrc)
                            jx.SyncTraySettings(jxSrc);
                    }
                    return;
                }
            }
        }
        private bool check_jx_contains(JxContainer container, object item)
        {
            if (container is JxContainer jx)
            {
                foreach (var subItem in jx.LoopItems())
                {
                    if (subItem == item)
                        return true;
                    if (subItem is JxContainer subContainer)
                        if (check_jx_contains(subContainer, item))
                            return true;
                }
            }
            return false;
        }
        #endregion

        #region PRIVATE_BOUND_RECT_FUNCTIONS
        void updateBoundRectBox(bool toModel)
        {
            var carrierID = _activeCarrierID;
            var vid = _activeViewID;

            var jxRecipe = getActiveCalibRecipe();
            if (jxRecipe == null)
                return;

            var rcpPanel = _calibToolUI.wndVisionSettingsPanel;
            var jxRect = jxRecipe.GridSettings.BoundRect;

            if (toModel)
            {
                jxRect.Value = _cviBigBoundBox.Box;
                rcpPanel?.Refresh();
            }
            else
            {
                var rect = jxRect.Value;
                var matViewer = _getMatViewer(vid);

                if (rect == Rectangle.Empty)
                {
                    var image = matViewer.Image;
                    if (image != null)
                    {
                        var w = image.Width;
                        var h = image.Height;
                        rect = new Rectangle(0, 0, w, h);
                        var dw = w / 10 / 2;
                        var dh = h / 10 / 2;
                        rect.Inflate(-dw, -dh);
                    }
                    else
                    {
                        rect = new Rectangle(10, 10, 500, 500);
                    }

                    jxRect.Value = rect;
                    rcpPanel?.Refresh();
                }

                _cviBigBoundBox.Box = rect;

                if (!_cviBigBoundBox.Visible)
                {
                    _cviBigBoundBox.Visible = true;
                    matViewer?.Invalidate();
                }
            }
        }
        #endregion

        #region PRIVATE_GRID_BOARD_FUNCTIONS
        /// <summary>
        /// 從 共用座標轉換系統 取出 相機 格點
        /// </summary>
        EzBlocsGrid getActiveBoardGridInTrf()
        {
            var grid = _commonBaseTrf?.GetCalibCamGrid(_activeCarrierID);
            return grid;
        }
        /// <summary>
        /// 將 大校正板 格點 記入 共用座標轉換系統
        /// </summary>
        bool setActiveBoardGridToTrf(EzBlocsGrid boardGrid)
        {
            //(0) 檢查 boardGrid
            var err = VerifyBoardGrid(boardGrid, out string errDetails);
            if (err != ErrorCodes.OK)
            {
                var msg = GaUtil.GetEnumDescription(err) + "\n\r" + errDetails;
                VsMessageBox.Warning(msg);
                return false;
            }

            //(1) 檢查 共用校正參數
            var jxRecipe = getActiveCalibRecipe();
            var traySettings = jxRecipe?.GridSettings?.EmptyTraySettings;
            if (traySettings == null)
            {
                var msg = GaUtil.GetEnumDescription(ErrorCodes.CalibErr_No_Recipe);
                VsMessageBox.Warning(msg);
                return false;
            }

            //(2) 從 共用校正參數 取出 pitch, rows, cols
            var pitchX = (double)traySettings.PitchX.Value;
            var pitchY = (double)traySettings.PitchY.Value;
            var rows = (int)traySettings.FullRows.Value;
            var cols = (int)traySettings.FullCols.Value;

            //(3) 將 大校正板 格點 記入 座標轉換系統
            _commonBaseTrf.ConfigWorldGridPoints(rows, cols, pitchX, pitchY);
            _commonBaseTrf.SetCalibCamGrid(_activeCarrierID, boardGrid);

            //(4) 設定 旗標
            _isCoordModified = true;
            return true;
        }
        /// <summary>
        /// 更新 相機格點 到 GUI
        /// </summary>
        void updateBoardGridBox(EzBlocsGrid camGrid, bool refresh = false)
        {
            _cviGridResultBox.IsEmptyTrayMode = false;

            _cviGridResultBox.ActiveCarrierID = _activeCarrierID;
            //_cviGridResultBox.ActiveSuckerRowID = _activeSuckerRowID;
            _cviGridResultBox.TransCameraToWorld = _commonBaseTrf?.GetCameraPhysicTransform(_activeCarrierID);
            _cviGridResultBox.TransCameraToMotor = _commonBaseTrf?.GetCameraMotorTransform(_activeCarrierID, SuckerRowEnum.S1);
            _cviGridResultBox.TransCameraToMotor2 = _commonBaseTrf?.GetCameraMotorTransform(_activeCarrierID, SuckerRowEnum.S2);

            _cviGridResultBox.UpdateResult(camGrid);
            _cviGridResultBox.Visible = camGrid != null;

            if (refresh)
                _getMatViewer(CalibViewEnum.BigGridBoardView)?.Refresh();
        }
        #endregion

        #region PRIVATE_INK_MARKS_FUNCTIONS
        /// <summary>
        /// 從 共用校正參數, 取出 inkMarks (順時針四角: 左上, 右上, 右下, 左下) 
        /// </summary>
        EzBloc[] getActiveInkMarksInRecipe()
        {
            var calibRecipe = getActiveCalibRecipe();
            var jxSettings = (_activeSuckerRowID == SuckerRowEnum.S1) ?  
                                calibRecipe?.InkMarkSettings1: 
                                calibRecipe?.InkMarkSettings2;
            if (jxSettings != null)
            {
                jxSettings.GetInkMarks(out var marks);
                return marks;
            }
            return null;
        }
        /// <summary>
        /// 將 inkMarks 記入 共用校正參數
        /// </summary>
        void setActiveInkMarksToRecipe(EzBloc[] inkMarks)
        {
            //將 inkMarks 更新到 共用校正參數
            var calibRecipe = getActiveCalibRecipe();
            var jxSettings = (_activeSuckerRowID == SuckerRowEnum.S1) ? 
                                calibRecipe?.InkMarkSettings1: 
                                calibRecipe?.InkMarkSettings2;
            jxSettings?.SetInkMarks(inkMarks);
        }
        /// <summary>
        /// 將 inkMarks 更新到 GUI
        /// </summary>
        void updateInkMarkBoxes(EzBloc[] inkMarks, bool refresh = false)
        {
            if (_cviInkMarkBoxes == null)
                return;

            if (inkMarks == null)
            {
                foreach (var box in _cviInkMarkBoxes)
                {
                    if (box != null)
                        box.Visible = false;
                }
            }
            else
            {
                int idx = 0;
                var NP = Math.Min(inkMarks.Length, _cviInkMarkBoxes.Length);

                foreach (var cviBox in _cviInkMarkBoxes)
                {
                    var mark = inkMarks[idx++];
                    if (cviBox == null) continue;

                    if (mark != null && idx <= NP)
                    {
                        cviBox.SetBox(mark);
                        cviBox.Visible = true;
                    }
                    else
                    {
                        cviBox.Visible = false;
                    }
                }
            }

            if (refresh)
                _getMatViewer(CalibViewEnum.InkMarksView)?.Refresh();
        }
        #endregion

        #region PRIVATE_MOTOR_COORDS_FUNCTIONS
        /// <summary>
        /// 從 commonBaseTrf 取出 馬達座標
        /// </summary>
        QCoord[] getActiveMotorCoordsInTrf()
        {
            var transform = _commonBaseTrf.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
            var trfCalibCorners = transform.GetCalibCornerPoints();
            var motorPts = trfCalibCorners?.GetAll(isSrc: false);
            //var camPts = trfCalibCorners?.GetAll(isSrc: true);
            return motorPts;
        }
        #endregion

        #region PRIVATE_DGV_FUNCTIONS
        void dgvInit()
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;
            dgv.Rows.Clear();

            var corners = Enum.GetValues(typeof(CalibCornersEnum));
            foreach (CalibCornersEnum corner in corners)
            {
                var name = GaUtil.GetEnumDescription(corner);
                dgv.Rows.Add(name, "", "", "", "");
            }

            dgv.CellValueChanged += (s, e) =>
            {
                // 排除標題列
                if (e.RowIndex >= 0)
                {
                    _isCoordModified = true;
                }
            };

            //dgv.CellContentDoubleClick += Dgv_CellContentDoubleClick;
            dgv.CellContentClick += Dgv_CellContentClick;
        }
        void dgvUpdateInkMarks(EzBloc[] inkMarks)
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;

            int rowsCount = dgv.Rows.Count;
            for (int rowId = 0; rowId < rowsCount; rowId++)
            {
                var camPt = (inkMarks != null && rowId < inkMarks.Length) ? inkMarks[rowId].Center : null;
                var dgvRow = dgv.Rows[rowId];
                dgvRow.Cells[1].Value = camPt != null ? camPt.X : 0.0;
                dgvRow.Cells[2].Value = camPt != null ? camPt.Y : 0.0;
            }
        }
        void dgvUpdateMotorCoords(QCoord[] motorCoords)
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;

            int rowsCount = dgv.Rows.Count;
            for (int rowId = 0; rowId < rowsCount; rowId++)
            {
                var motorPt = (motorCoords != null && rowId < motorCoords.Length) ? motorCoords[rowId] : null;
                var dgvRow = dgv.Rows[rowId];
                dgvRow.Cells[3].Value = motorPt != null ? motorPt.X : 0.0;
                dgvRow.Cells[4].Value = motorPt != null ? motorPt.Y : 0.0;
            }
        }
        void dgvGetUserInputMotorCoords(out QCoord[] motorCoords)
        {
            // 順時針四角: 左上, 右上, 右下, 左下
            motorCoords = null;

            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;

            try
            {
                double mx, my;
                var results = new List<QVector>();

                int rowsCount = Math.Min(dgv.Rows.Count, 4);
                for (int rowId = 0; rowId < rowsCount; rowId++)
                {
                    var dgvRow = dgv.Rows[rowId];
                    if (dgvRow != null && dgvRow.Cells.Count > 4)
                    {
                        try
                        {
                            mx = (double)dgvRow.Cells[3].Value;
                            my = (double)dgvRow.Cells[4].Value;
                        }
                        catch
                        {
                            mx = 0.0;
                            my = 0.0;
                        }
                        results.Add(new QVector(mx, my));
                    }
                }

                if (results.Count == 4)
                {
                    motorCoords = results.ToArray();
                }
                else
                {
                    throw new Exception("Data Grid View 馬達座標格式有誤!");
                }
            }
            catch (Exception ex)
            {
                VsMessageBox.Warning(ex.Message);
                motorCoords = null;
            }
        }
        void dgvSyncCurrentMotorCoordsToUserInput(int rowIndex, QVector motorCoords = null)
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count)
                return;

            var currentMotorPos = motorCoords == null ?
                                    QueryCurrentMotorXY() :
                                    motorCoords;

            var dgvRow = dgv.Rows[rowIndex];
            var targetName = dgvRow.Cells[0].Value;
            var msg = GaUtil.GetEnumDescription(Prompts.Question_Update_Motor_Coord_To_Calib);
            msg += $"?\n\r\n\r(X= {currentMotorPos.X:0.000}, Y= {currentMotorPos.Y:0.000})";
            msg += $"\n\r\n\rTo 【{targetName}】";
            //>> bool ok = MessageBox.Show(msg, "Calibration", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            bool ok = VsMessageBox.Question(msg) == DialogResult.OK;

            if (ok)
            {
                dgvRow.Cells[3].Value = currentMotorPos.X;
                dgvRow.Cells[4].Value = currentMotorPos.Y;
            }
        }
        void dgvUpdateAllCalibKeyPoints(bool toModel)
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;

            if (toModel)
            {
#if (OPT_REV_2026_0308_LEGACY)
                dgvGetMotorCoords(out var motorCoords);
                if (motorCoords == null)
                {
                    //VsMessageBox.Warning("馬達座標 不完整!");
                    VsMessageBox.Warning(GaUtil.GetEnumDescription(Prompts.Warn_Motor_Coords_Not_Completed));
                    return;
                }

                var inkMarks = getActiveInkMarks();
                if (inkMarks == null)
                {
                    //VsMessageBox.Warning("點墨 不完整!");
                    VsMessageBox.Warning(GaUtil.GetEnumDescription(Prompts.Warn_Ink_Marks_Not_Completed));
                    return;
                }

                var transform = _commonBaseTrf.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
                if (transform == null)
                {
                    //VsMessageBox.Warning($"沒有座標轉換 模型 {_activeCarrierID} {_activeSuckerRowID}!");
                    string msg = GaUtil.GetEnumDescription(Prompts.Warn_No_Transform) + $" {_activeCarrierID} {_activeSuckerRowID}";
                    VsMessageBox.Warning(msg);
                    return;
                }

                // 重新設定校正點 (兩盤兩排總共 4x4 = 16 點)
                var trfCalibPoints = transform.GetCalibGridPoints();
                var camPts = new QCoord[2, 2] {
                    { inkMarks[0].Center, inkMarks[1].Center },
                    { inkMarks[3].Center, inkMarks[2].Center },
                };
                var motorPts = new QCoord[2, 2] {
                    { motorCoords[0], motorCoords[1] },
                    { motorCoords[3], motorCoords[2] },
                };
                trfCalibPoints.SetAll(camPts, motorPts);

                _isCoordModified = true;
#endif
            }
            else
            {
                var inkMarks = getActiveInkMarksInRecipe();
                var motorCoords = getActiveMotorCoordsInTrf();
                dgvUpdateInkMarks(inkMarks);
                dgvUpdateMotorCoords(motorCoords);
            }
        }
        #endregion

        #region OLD_CALIB_KEY_POINT_FUNCTIONS
#if (false)
        void updateCalibKeyPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID, bool toModel)
        {
            //if (_activeViewID != 0 && toModel)
            //    return;
            return;

            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;
            
            var transform = _transforms.GetCameraMotorTransform(carrierID, suckerRowID);
            var trfCalibCorners = transform.GetCalibCornerPoints();
            var camPts = trfCalibCorners?.GetAll(isSrc: true);
            var motorPts = trfCalibCorners?.GetAll(isSrc: false);

            if (toModel)
            {
                if (transform == null)
                    return;

                var corners = Enum.GetValues(typeof(CalibCornersEnum));
                foreach (CalibCornersEnum corner in corners)
                {
                    int index = (int)corner;
                    var camPt = camPts[index];
                    var motorPt = motorPts[index];
                    bool isChanged = updateCalibKeyPoints(dgv, index, camPt, motorPt, toModel);
                    if (isChanged)
                    {
                        //transform.setCalibCornerPoints(index, camPt, motorPt);
                        trfCalibCorners.Set(index, camPt, motorPt);
                        _isCoordModified = true;
                    }
                }
            }
            else
            {
                dgv.Rows.Clear();

                var corners = Enum.GetValues(typeof(CalibCornersEnum));
                foreach (CalibCornersEnum corner in corners)
                {
                    int rowId = (int)corner;
                    var camPt = camPts != null ? camPts[rowId] : null;
                    var motorPt = motorPts != null ? motorPts[rowId] : null;

                    var name = GaUtil.GetEnumDescription(corner);
                    dgv.Rows.Add(name, 0.0, 0.0, 0.0, 0.0);
                    dgv.Rows[rowId].Cells[0].Value = GaUtil.GetEnumDescription(corner);
                    updateCalibKeyPoints(dgv, rowId, camPt, motorPt, toModel);
                    //updateCalibKeyPointBox(corner, camPt);
                }
            }
        }
        bool updateCalibKeyPoints(DataGridView dgv, int rowId, QCoord camCoord, QCoord motorCoord, bool toModel)
        {
            return false;

            var dgvRow = dgv.Rows[rowId];

            if (toModel)
            {
                bool isChanged = false;

                int col = 1;
                var cx = (double)dgvRow.Cells[col++].Value;
                var cy = (double)dgvRow.Cells[col++].Value;
                var mx = (double)dgvRow.Cells[col++].Value;
                var my = (double)dgvRow.Cells[col++].Value;

                //===================================================
                // camCoord 由像測自動改變
                // 在此更新會有數值誤差 !!!
                //===================================================
                //if (camCoord.X != cx || camCoord.Y != cy)
                //{
                //    camCoord.X = cx;
                //    camCoord.Y = cy;
                //    isChanged = true;
                //}

                if (motorCoord.X != mx || motorCoord.Y != my)
                {
                    motorCoord.X = mx;
                    motorCoord.Y = my;
                    isChanged = true;
                }

                return isChanged;
            }
            else
            {
                int col = 1;
                dgvRow.Cells[col++].Value = camCoord != null ? camCoord.X : 0.0;
                dgvRow.Cells[col++].Value = camCoord != null ? camCoord.Y : 0.0;
                dgvRow.Cells[col++].Value = motorCoord != null ? motorCoord.X : 0.0;
                dgvRow.Cells[col++].Value = motorCoord != null ? motorCoord.Y : 0.0;
                return true;
            }
        }

        void updateCalibKeyPointBox(CalibCornersEnum corner, QCoord camCoord, bool show = true)
        {
            //if (_cviInkMarkBoxes == null)
            //    return;

            //var box = _cviInkMarkBoxes[(int)corner];
            //if (box == null)
            //    return;

            //if (camCoord == null)
            //{
            //    box.Visible = false;
            //    return;
            //}

            ////var loc = box.Box2D;
            ////loc.SetCenter((float)camCoord.X, (float)camCoord.Y);
            ////box.SetBox(loc);

            //box.Quad2D?.SetCenter(camCoord.X, camCoord.Y);
            //box.Visible = show;
        }
        void updateCalibGridBoardPoints(EzBlocsGrid camGrid)
        {
            throw new NotImplementedException();

            if (_activeViewID != 0)
                return;

            if (camGrid == null)
                return;

            //(1) 取出參數設定的 pitch, rows, cols
            var jxRecipe = getActiveCalibRecipe();  // _jxCalibRecipes[(int)_activeCarrierID];
            var traySettings = jxRecipe.GridSettings.EmptyTraySettings;
            var pitchX = (double)traySettings.PitchX.Value;
            var pitchY = (double)traySettings.PitchY.Value;
            var rows = (int)traySettings.FullRows.Value;
            var cols = (int)traySettings.FullCols.Value;

            bool areAllRowsColsMatched = (rows == camGrid.Rows && cols == camGrid.Cols);
            if (!areAllRowsColsMatched)
            {
                string msg = $"像測的 Rows={camGrid.Rows} Cols={camGrid.Cols} 與\n\r"
                           + $"參數的 Rows={rows} Cols={cols} 不一致 !";
                MessageBox.Show(msg, _wndOwner.FindForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //(2) 將 校正點位群 更新到 座標轉換 系統 (Model)
            if (areAllRowsColsMatched)
            {
                _transforms.ConfigGlobalCalibPlcGrid(rows, cols, pitchX, pitchY);
                _transforms.UpdateCalibPoints(_activeCarrierID, _activeSuckerRowID, camGrid);
            }

            //(3) 更新 GUI
            updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);

            //(4) 設定 旗標
            _isCoordModified = true;
        }
#endif
        #endregion

        bool SetActiveView(CarrierEnum C, SuckerRowEnum S, CalibViewEnum vid, bool force = false)
        {
            bool isAnyChanged = _activeCarrierID != C || _activeSuckerRowID != S || _activeViewID != vid;
            var oldImgFile = CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID, (int)_activeViewID);
            var newImgFile = CALIB_LAST_IMAGE_FILE(C, S, (int)vid);
            bool needsToLoadNewImage = _isEmptyImage(vid) || oldImgFile != newImgFile;

            _activeCarrierID = C;
            _activeSuckerRowID = S;
            _activeViewID = vid;

            // Loading Image
            if (needsToLoadNewImage || force)
            {
                LoadImage(newImgFile, (int)vid);
            }

            if (isAnyChanged || force)
            {
                updateBoundRectBox(false);

                var inkMarks = getActiveInkMarksInRecipe();
                var motorCoords = getActiveMotorCoordsInTrf();
                
                dgvUpdateInkMarks(inkMarks);
                dgvUpdateMotorCoords(motorCoords);

                bool refresh1 = _activeViewID == CalibViewEnum.BigGridBoardView;
                updateBoardGridBox(getActiveBoardGridInTrf(), refresh1);

                bool refresh2 = _activeViewID == CalibViewEnum.InkMarksView;
                updateInkMarkBoxes(inkMarks, refresh2);

                updatePropertyPanel(false);
                updateGuiStatus();
            }

            return needsToLoadNewImage;
        }
        void TakeOverImage(Bitmap bigBmp, string srcName, CalibViewEnum vid, bool disposeSrc = true)
        {
            var imgPanel = _getImgViewPanel(vid);
            if (imgPanel == null)
                return;

            if (bigBmp != null)
            {
                imgPanel.UpdateImage(bigBmp, srcName, disposeSrc);
            }
            else
            {
                var old = imgPanel.MatViewer.Image;
                imgPanel.MatViewer.Image = null;
                old?.Dispose();
                imgPanel.UpdateImage(null, "", false);
            }
        }

        /// <summary>
        /// 自動抓取 大校正版 的 格位點
        /// </summary>
        bool RunAutoFetchBoardGrid(bool dump = false)
        {
            //(0) Cursor
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            try
            {
                CalibAoiModel.OPT_DUMP = dump;

                //(1) clear interactors
                updateBoardGridBox(null, refresh: true);

                //(2) image
                var matViewer = _getMatViewer(CalibViewEnum.BigGridBoardView);
                var fullfovImg = matViewer.Image;
                if (fullfovImg == null)
                    return false;

                //(3) Run AOI
                var boardGrid = _calibModel.FetchBoardGrid(_activeCarrierID, fullfovImg, getActiveCalibRecipe());

                //(4) Update Grid Result
                bool ok = setActiveBoardGridToTrf(boardGrid);
                updateBoardGridBox(boardGrid, refresh: true);
                _isCoordModified = true;

                //(5) MessageBoxe
                #region MESSAGE_BOX
                if (ok && dump)
                {
                    GaUtil.SetCursor(_wndOwner, oldCursor);
                    VsMessageBox.Info("已成功保存二值化圖檔\n\r於 d:\\paso.log\\Calib\\BoardGrid");
                }
                #endregion

                return boardGrid != null;
            }
            catch (Exception ex)
            {
                GaUtil.SetCursor(_wndOwner, oldCursor);
                VsMessageBox.Warning($"異常: {ex.Message}");
                return false;
            }
            finally
            {
                CalibAoiModel.OPT_DUMP = false;
                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }

        /// <summary>
        /// 自動抓取 墨點 (目前只用四角)
        /// </summary>
        bool RunAutoFetchInkMarks(bool dump = false)
        {
            //(0) Cursor
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            try
            {
                CalibAoiModel.OPT_DUMP = dump;

                //(1) clear interactors
                updateInkMarkBoxes(null, refresh: true);

                //(2) image
                var matViewer = _getMatViewer(CalibViewEnum.InkMarksView);
                var fullfovImg = matViewer.Image;
                if (fullfovImg == null)
                    return false;

                //(1) Run AOI
                var inkMarks = _calibModel.FetchInkMarks(_activeCarrierID, _activeSuckerRowID, fullfovImg, getActiveCalibRecipe());

                //(2) Update InkMarks to GUI
                setActiveInkMarksToRecipe(inkMarks);
                updateInkMarkBoxes(inkMarks, refresh: true);
                dgvUpdateInkMarks(inkMarks);

                //(3) MessageBoxe
                #region MESSAGE_BOX
                GaUtil.SetCursor(_wndOwner, oldCursor);
                if (inkMarks == null)
                {
                    string msg = "無法自動抓到 四角定位點!\n\r請確認 參數 是否適配?";
                    MessageBox.Show(msg, "Calib", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (dump)
                {
                    VsMessageBox.Info("已成功保存二值化圖檔\n\r於 d:\\paso.log\\Calib");
                }
                #endregion

                return inkMarks != null;
            }
            catch (Exception ex)
            {
                GaUtil.SetCursor(_wndOwner, oldCursor);
                VsMessageBox.Warning($"異常: {ex.Message}");
                return false;
            }
            finally
            {
                CalibAoiModel.OPT_DUMP = false;
                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }

        /// <summary>
        /// 將 User 輸入的馬達座標值 記入 CommmonBaseTrf
        /// </summary>
        bool UpdateUserInputMotorCoordsToTrf(bool verify = true)
        {
            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null)
            {
                VsMessageBox.Warning("GUI (DataGridView == null) 已經不存在!");
                return false;
            }

            var inkMarks = getActiveInkMarksInRecipe();
            var inkMarkPts = Array.ConvertAll(inkMarks, im => im.Center);
            dgvGetUserInputMotorCoords(out var userInputMotorCoords);

            if (verify)
            {
                #region 驗證參數數據
                var errs = new List<string>();
                for (int i = 0, NP = inkMarkPts.Length; i < NP; i++)
                {
                    var diff = inkMarkPts[i] - _cviInkMarkBoxes[i].Quad2D.Center;
                    if(diff.NormLength > 0.001)
                    {
                        errs.Add($"  墨點 [{i}]: 參數({inkMarkPts[i].X:F3}, {inkMarkPts[i].Y:F3}) vs GUI({_cviInkMarkBoxes[i].Quad2D.Center.X:F3}, {_cviInkMarkBoxes[i].Quad2D.Center.Y:F3})");
                    }
                }
                if(errs.Count > 0)
                {
                    var errMsg = "以下 墨點, 參數 與 GUI, 兩者誤差太大:\n\r";
                    errMsg += string.Join("\n\r", errs);
                    VsMessageBox.Warning(errMsg);
                    return false;
                }
                #endregion
            }

            var err = _commonBaseTrf.SetCalibMotorCoords(_activeCarrierID, _activeSuckerRowID, inkMarkPts, userInputMotorCoords);
            if (err != ErrorCodes.OK)
            {
                VsMessageBox.Warning(GaUtil.GetEnumDescription(err));
                return false;
            }

            if (verify)
            {
                #region 驗證座標轉換結果
                var trfCamToMotor = _commonBaseTrf.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
                var motorPts = Array.ConvertAll(inkMarkPts, camPt => trfCamToMotor.Trans(camPt));
                var errs = new List<string>();
                for (int i = 0, NP = motorPts.Length; i < NP; i++)
                {
                    var diff = motorPts[i] - userInputMotorCoords[i];
                    if (diff.NormLength > 0.001)
                    {
                        errs.Add($"  馬達點位 [{i}]: 轉換後({motorPts[i].X:F3}, {motorPts[i].Y:F3}) vs 輸入({userInputMotorCoords[i].X:F3}, {userInputMotorCoords[i].Y:F3})");
                    }
                }
                if (errs.Count > 0)
                {
                    var errMsg = "以下 馬達點位, 座標轉換 與 User 輸入, 兩者誤差太大:\n\r";
                    errMsg += string.Join("\n\r", errs);
                    VsMessageBox.Warning(errMsg);
                    return false;
                }
                #endregion
            }

            return true;
        }

        /// <summary>
        /// 建立 座標轉換系統
        /// </summary>
        void BuildCommonBaseTransforms(bool force = false)
        {
            //(0) Cursor
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            try
            {
                //(1) 更新 Gui 輸入數據到 Model
                updateAllData(true);

                //(2) 將 User 輸入的馬達座標值 記入 CommmonBaseTrf
                bool ok = UpdateUserInputMotorCoordsToTrf();
                if (!ok)
                    return;

                //(3) 查核 大校正版 格位點 的正確性!
                var boardGrid = getActiveBoardGridInTrf();
                var err = VerifyBoardGrid(boardGrid, out string errDetails);
                if (err != ErrorCodes.OK)
                {
                    var msg = GaUtil.GetEnumDescription(err) + "\n\r" + errDetails;
                    VsMessageBox.Warning(msg);
                    return;
                }

                //(4) 建立 Transform
                _commonBaseTrf.BuildAll();
                _isCoordModified = true;

                //(5) 去除異常 格位 再重建一次 Transform
#if (OPT_RESERVED)
                if (_calibModel.AdjustBadNodes(carrierID, camGrid))
                    transformsModel.BuildAll();
#endif

                //(6) 檢查 CommonBaseTrf 的結果
                err = VerifyCommonBaseTrf(out errDetails);
                if (err != ErrorCodes.OK)
                {
                    var msg = GaUtil.GetEnumDescription(err) + "\n\r" + errDetails;
                    VsMessageBox.Warning(msg);
                    return;
                }

                //(7) 成功訊息
                string info = GaUtil.GetEnumDescription(_activeCarrierID) 
                            + " && " + GaUtil.GetEnumDescription(_activeSuckerRowID)
                            + "\n\r\n\r" + GaUtil.GetEnumDescription(Prompts.Info_CommonBase_Trf_Successed);
                VsMessageBox.Info(info);

                //(8) 更新 Interactor
                _cviGridResultBox.TransCameraToWorld = _commonBaseTrf?.GetCameraPhysicTransform(_activeCarrierID);
                _cviGridResultBox.TransCameraToMotor = _commonBaseTrf?.GetCameraMotorTransform(_activeCarrierID, SuckerRowEnum.S1);
                _cviGridResultBox.TransCameraToMotor2 = _commonBaseTrf?.GetCameraMotorTransform(_activeCarrierID, SuckerRowEnum.S2);
            }
            catch (Exception ex)
            {
                VsMessageBox.Warning($"異常: {ex.Message}");
            }
            finally
            {
                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
        }

        ErrorCodes VerifyBoardGrid(EzBlocsGrid boardGrid, out string errDetails)
        {
            errDetails = "";

            if (boardGrid == null)
            {
                return ErrorCodes.AoiErr_can_not_fetch_camera_grid;
            }

            var traySettings = getActiveCalibRecipe()?.GridSettings?.EmptyTraySettings;
            if (traySettings != null)
            {
                string errStr = "";

                int rcpRows = traySettings.FullRows.Value;
                int rcpCols = traySettings.FullCols.Value;

                if (rcpRows != boardGrid.Rows)
                    errStr += $"\n\r參數 Rows={rcpRows}  vs  像測 Rows={boardGrid.Rows}";
                if (rcpCols != boardGrid.Cols)
                    errStr += $"\n\r參數 Cols={rcpCols}  vs  像測 Cols={boardGrid.Cols}";

                if (!string.IsNullOrEmpty(errStr))
                {
                    errDetails = errStr;
                    return ErrorCodes.AoiErr_camera_grid_not_consistent;
                }
            }

            // 最後檢查
            (var err, var errMsg) = _commonBaseTrf.checkCameraGrid(_activeCarrierID, boardGrid);
            if (err != ErrorCodes.OK)
                errDetails = errMsg;

            return err;
        }
        ErrorCodes VerifyCommonBaseTrf(out string errDetails)
        {
            errDetails = "";

            var worldGrid = _commonBaseTrf?.GetWorldGridPoints();
            if (worldGrid == null)
                return ErrorCodes.NO_RUNTIME_PLC_GRID;

            var traySettings = getActiveCalibRecipe()?.GridSettings?.EmptyTraySettings;
            if (traySettings != null && worldGrid != null)
            {
                string err = "";
                double rcpPitchX = Math.Round((double)traySettings.PitchX.Value, 3);
                double rcpPitchY = Math.Round((double)traySettings.PitchY.Value, 3);
                double plcPitchX = Math.Round(worldGrid.PitchX, 3);
                double plcPitchY = Math.Round(worldGrid.PitchY, 3);

                if (rcpPitchX != plcPitchX)
                    err += $"\n\r參數 PitchX={rcpPitchX}  vs  共用座標系統 PitchX={plcPitchX}";
                if (rcpPitchY != plcPitchY)
                    err += $"\n\r參數 PitchY={rcpPitchY}  vs  共用座標系統 PitchY={plcPitchY}";

                if (!string.IsNullOrEmpty(err))
                {
                    errDetails = err;
                    return ErrorCodes.OK;
                }
            }

            return ErrorCodes.OK;
        }

        void LoadImage(string fileName = null, int viewID = -1)
        {
            var vid = viewID < 0 ? _activeViewID : (CalibViewEnum)viewID;

            //var imgViewer = _getImgViewPanel(vid);
            //if (viewID == 0)
            //    enableGoldenPicking(false);

            bool isBrowsing = string.IsNullOrEmpty(fileName);
            if (isBrowsing)
                fileName = GaUtil.BrowseImageFile();

            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(fileName))
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                _wndOwner?.Refresh();

                // 2025-09-14 _imgViewer 會複製 bigBmp
                var bigBmp = GaImageUtil.LoadBigImage(fileName, autoSaveJpg: true);
                var srcName = "[校正] " + System.IO.Path.GetFileName(fileName);

                // 2025-11-20 (搭配點墨)
                TakeOverImage(bigBmp, srcName, vid, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                var dstFileName = CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID, (int)vid);
                if (dstFileName != fileName)
                    SaveImage(dstFileName);

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
            else
            {
                if (!isBrowsing)
                {
                    // 2025-11-20 (搭配點墨)
                    TakeOverImage(null, "", vid, disposeSrc: false);
                }
            }

            //if (vid == CalibViewEnum.BigGridBoardView)
            //    updateAllCalibKeyPointBoxes();
            //updateGuiStatus();
        }
        void SaveImage(string fileName)
        {
            var vid = _activeViewID;
            var imgPanel = _getImgViewPanel(vid); // _calibToolUI.ImgViewers[viewID];

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            var img = imgPanel?.Image;
            img?.SaveImage(fileName);

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void GrabImage()
        {
            var vid = _activeViewID;

            //var imgPanel = _getImgViewPanel(viewID); //_calibToolUI.ImgViewers[viewID];
            //if (viewID == 0)
            //    enableGoldenPicking(false);

            updateGuiStatus();

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                // 2025-09-14 _imgViewer 會複製 bigBmp
                var bigBmp = freeBmp.ToBitmap();
                var srcName = "[校正] 線掃相機 擷圖";

                // 2025-11-20 (搭配點墨)
                TakeOverImage(bigBmp, srcName, vid, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                SaveImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID, (int)vid));
            }
            GaUtil.SetCursor(_wndOwner, oldCursor);
        }

        void OpenMotorWindowXY(QVector directTargetPos = null)
        {
            using (var dlg = new FormMotors_CarierSuckerXY())
            {
                dlg.OnInkerCoordsUpdated += Dlg_OnInkerCoordsUpdated;
                dlg.OnQueryInkerCoords += Dlg_OnQueryInkerCoords;
                dlg.SetJogTargets(_activeCarrierID, _activeSuckerRowID, directTargetPos);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(_wndOwner);
            }
        }



        void LoadSettings()
        {
            // (1) Load VISION recipe Files
            int i = 0;
            foreach (var jx in _jxCalibRecipes)
                jx.Load(CALIB_RECIPE_FILE((CarrierEnum)i++));
            // (1.1) Sync
            foreach (var jx in _jxCalibRecipes)
                jx.SyncTraySettings(_jxCalibRecipes[0]);
            // (1.2) Handlers
            foreach (var jx in _jxCalibRecipes)
                connectPropEventHandlers(jx);

            // (2) Load TRANSFORMS (永遠載入全域設定)
            _commonBaseTrf.Load(CALIB_TRANSFORMS_FILE);

            // (3) 第一次建置座標轉換
            _commonBaseTrf.BuildAll();
        }
        void SaveSettings(bool force = false)
        {
            // 保存全域設定
            // (1) TRANSFORMS
            if (force || _isCoordModified)
            {
                _commonBaseTrf.Save(CALIB_TRANSFORMS_FILE);
                //>>> _transforms.SaveGaaraIniFile();
                _isCoordModified = false;
            }

            // (2) Save VISION recipe files
            int i = 0;
            foreach (var jx in _jxCalibRecipes)
            {
                if (force || jx.Modified)
                    jx?.Save(CALIB_RECIPE_FILE((CarrierEnum)i));
                i++;
            }
        }
        void CloseWindow(bool confirm)
        {
            if (confirm)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                updateAllData(true);
                SaveSettings();
                GaUtil.SetCursor(_wndOwner, oldCursor);
                return;
            }
            else
            {
                // 還原舊值
                bool isModified = _isCoordModified;
                foreach (var jx in _jxCalibRecipes)
                    isModified |= jx.Modified;
                if (isModified)
                    LoadSettings();
            }

            var frm = _wndOwner.FindForm();
            frm?.Close();

            //由上層負責調用 Dispose()
            //frm?.Dispose();
        }
    }


    partial class GaCalibCtrl
    {
        #region XY_MOTOR_MESS
        IAxis _activeMotorX => Traveller106.Universal.GetMotorX(_activeSuckerRowID);
        IAxis _activeMotorY => Traveller106.Universal.GetMotorY(_activeCarrierID);
        QVector QueryCurrentMotorXY()
        {
            var motorX = _activeMotorX;
            var motorY = _activeMotorY;
            double x = motorX != null ? motorX.GetPos() : 0.0;
            double y = motorY != null ? motorY.GetPos() : 0.0;
            return new QVector2(x, y);
        }
        void MoveMotorXY(QVector targetPos)
        {
            if (targetPos == null) return;

            var motorNames = new[]
            {
                $"X ({GaUtil.GetEnumDescription(_activeSuckerRowID)})",
                $"Y ({GaUtil.GetEnumDescription(_activeCarrierID)})",
            };
            var motors = new[]
            {
                _activeMotorX,
                _activeMotorY,
            };

            int N = Math.Min(motors.Length, motorNames.Length);
            var errs = new List<string>();

            #region CHECK_MOTOR_EMPTY
            for (int i = 0; i < N; i++)
            {
                if (motors[i] == null)
                    errs.Add(motorNames[i] + " is null.");
            }
            if (errs.Count > 0)
            {
                var warnings = GaUtil.GetEnumDescription(Prompts.Warning_No_Motor);
                warnings += "\n\r\n\r" + string.Join("\n\r", errs);
                VsMessageBox.Warning(warnings);
                return;
            }
            #endregion

            #region CHECK_MOTOR_BUSY
            for (int i = 0; i < N; i++)
            {
                if (!motors[i].IsOK)
                    errs.Add(motorNames[i] + (motors[i].IsError ? " Error!" : " Busy."));
            }
            if (errs.Count > 0)
            {
                var warnings = GaUtil.GetEnumDescription(Prompts.Warning_Motor_Busy);
                warnings += "\n\r\n\r" + string.Join("\n\r", errs);
                VsMessageBox.Warning(warnings);
                return;
            }
            #endregion

            var currentPos = QueryCurrentMotorXY();
            var delta = targetPos - currentPos;
            if (GaBasicMotorUtil.IsTinyDelta(delta.X) && GaBasicMotorUtil.IsTinyDelta(delta.Y))
                return;

            #region PROMPTS
            var msg = GaUtil.GetEnumDescription(Prompts.Question_Motor_GoTo_Pos);
            msg += $"\n\r\n\r{motorNames[0]} To {targetPos.X:0.000}";
            msg += $"\n\r\n\r{motorNames[1]} To {targetPos.Y:0.000}";
            if (VsMessageBox.Question(msg) != DialogResult.OK)
                return;
            #endregion

            //// Y 較長先移動
            //// X 次之
            //_activeMotorY?.Go(targetPos.Y, 0);
            //_activeMotorX?.Go(targetPos.X, 0);

            OpenMotorWindowXY(targetPos);
        }
        #endregion

        #region FOCUS_MOTOR_JOG
        GaMotorZCtrl _focusMotorCtrl = null;
        void attachFocusMotorCtrl()
        {
            // 只允許 attach 一次
            if (_focusMotorCtrl != null)
                return;

            var view = _calibToolUI.wndFocusMotorGoPanel;

            _focusMotorCtrl = new GaMotorZCtrl();
            _focusMotorCtrl.Attach(view);
            _focusMotorCtrl.SetDataSrc(ZPosDataSrc.Calib);
            _focusMotorCtrl.OnPosDataSrcModified += (s, e) =>
            {
                _isCoordModified = true;
            };
        }
        void syncFocusMotorCtrl()
        {
            _focusMotorCtrl?.UpdatePosHolderToGui();
        }
        #endregion
    }
}
