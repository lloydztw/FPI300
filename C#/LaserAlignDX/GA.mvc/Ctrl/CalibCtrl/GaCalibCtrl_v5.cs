#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-20 配合 新校正板 (陣列圓點 + INK區塊 二合一) (by LeTian Chang)
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
using JetEazy.Lang;
using JetEazy.Match;
using JetEazy.OpenCV.Viewer;
using JetEazy.QMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Model;
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
using CalibAoiModel = LaserAlignDX.AoiModel.Calib.V5.CalibAoiModel;
using CviBoundBox = EzAoiEmptyTrayInspector.Ctrl.CviRcpBox;
using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;
using QCoord = JetEazy.QMath.QVector;

namespace LaserAlignDX.Mvc.Ctrl.Galib.V5
{
    public partial class GaCalibCtrl
    {
        static bool OPT_SUPPORT_MOTOR_JOG = false;

        #region CONSTANTS
        static int N_CALIB_MOTOR_POINTS => TravellerTransformFactory.N_CALIB_MOTOR_POINTS;
        static int N_CARRIERS_NUMBER => Enum.GetValues(typeof(CarrierEnum)).Length;
        static double TRANS_TOLERANCE => 0.003;
        static double PITCH_TOLERANCE => 0.002;
        static int PIXEL_TOLERANCE => 1;
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
        #endregion

        #region ACTIVE_MODES
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        #endregion

        #region GLOBAL_PATH
        internal static string CALIB_TRANSFORMS_FILE => GaMvcPaths.COMMON_BASE_TRANSFORMS_INI_FILE;
        internal static string CALIB_RECIPE_FILE(CarrierEnum C)
        {
            return GaMvcPaths.CALIB_RECIPE_FILE(C);
        }
        internal static string CALIB_LAST_IMAGE_FILE(CarrierEnum C, SuckerRowEnum S)
        {
            var path = System.IO.Path.GetDirectoryName(CALIB_RECIPE_FILE(C));
            string imgFile = System.IO.Path.Combine(path, $"jx_calib_InkMarks_image_@{C}#{S}.jpg");
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
        JezTransImageViewPanel _getImgViewPanel(object vid = null)
        {
            return _calibToolUI.ImgViewers[0];
        }
        CvMatViewer _getMatViewer(object vid = null)
        {
            return _getImgViewPanel(vid)?.MatViewer;
        }
        bool _isEmptyImage(object vid = null)
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
        Button _btnRunAutoFetchAll => _calibToolUI.btnAutoFetchGrid;
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
            var matViewer = _getMatViewer();
            matViewer.AddInteractor(_cviBigBoundBox);
            matViewer.AddInteractor(_cviGridResultBox);
            for (int i = 0, NP = _cviInkMarkBoxes.Length; i < NP; i++)
            {
                var box = _cviInkMarkBoxes[i] = new CviCalibPointBox(RectangleF.Empty, Color.Orange) { Visible = false };
                box.CrossColor = Color.Orange;
                box.CrossLength = 500;
                matViewer.AddInteractor(box);
            }
        }
        void connectEventHandlers()
        {
            if (_btnOK != null)
                _btnOK.Click += (s, e) => CloseWindow(true);

            if (_btnCancel != null)
                _btnCancel.Click += (s, e) => CloseWindow(false);

            if (_rdoCarriers != null)
                _rdoCarriers[0].CheckedChanged += _rdoSelect_CheckedChanged;

            if (_rdoSuckerRows != null)
                _rdoSuckerRows[0].CheckedChanged += _rdoSelect_CheckedChanged;

            _btnGrabImage.Click += (s, e) => GrabLineScanCameraImage();
            _btnLoadImage.Click += (s, e) => LoadImage();

            if (_btnRunAutoFetchAll != null)
                _btnRunAutoFetchAll.Click += (s, e) => RunAutoFetchAll();

            if (_btnBuildCalib != null)
                _btnBuildCalib.Click += (s, e) => BuildCommonBaseTransforms();

            // Motors
            if (_btnOpenMotorXY != null)
                _btnOpenMotorXY.Click += (s, e) => OpenMotorWindowXY();

            // Interactors
            _cviGridResultBox.OnRequestDumpBindaryImage += (s, e) => RunAutoFetchBoardGrid(dump: true);
            _cviBigBoundBox.OnChanged += _cviBigBoundBox_OnChanged;

            // ImgViewrs' EventHanlders
            _getMatViewer().MouseMove += MatViewer1_MouseMove;
            _getMatViewer().MouseMove += MatViewer2_MouseMove;

            // 自動釋放資源
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();

            // 延遲更新
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    //>>> updateAllData(false);
                    SetActiveView(_activeCarrierID, _activeSuckerRowID, force: true);
                    syncFocusMotorCtrl();
                }));
            };
        }
        #endregion

        #region EVENT_HANDLERS
        //private void _calibToolUI_OnActiveViewChanged(object sender, EventArgs e)
        //{
        //    var vid = (CalibViewEnum)_calibToolUI.ActiveViewID;
        //    if (vid != _activeViewID)
        //    {
        //        SetActiveView(_activeCarrierID, _activeSuckerRowID);
        //    }
        //}
        private void _rdoSelect_CheckedChanged(object sender, System.EventArgs e)
        {
            var C = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            var S = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;
            if (C == _activeCarrierID && S == _activeSuckerRowID)
                return;
            SetActiveView(C, S);
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

            var matViewer = _getMatViewer();
            matViewer.TransCoordToWorld(ref x, ref y);

            for (int i = 0; i < N_CALIB_MOTOR_POINTS; i++)
            {
                var quad = _cviInkMarkBoxes[i]?.Quad2D;
                if (quad == null) continue;

                var rect = quad.BoundaryRect;
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
#if (OPT_REPLACED_BY_MOTOR_JOG_TOOL || true)
            if (e.ColumnIndex >= 3)
            {
                var cornerID = e.RowIndex;
                if (cornerID >= 0 && cornerID < 4)
                {
                    dgvSyncCurrentMotorCoordsToUserInput(cornerID);
                }
            }
#endif
        }
        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                var cornerID = e.RowIndex;
                if (cornerID >= 0 && cornerID < 4)
                {
                    dgvGetUserInputMotorCoords(out var motorCoords);
                    var targetCoord = motorCoords[cornerID];
                    MoveMotorXY(targetCoord);
                }
            }
        }
        private void Dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // 排除標題列
            if (e.RowIndex >= 0)
            {
                _isCoordModified = true;
                //保留: UpdateUserInputMotorCoordsToTrf(false);
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
            var pgvPanel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (pgvPanel == null)
                return;

            var jxRecipe = getActiveCalibRecipe();
            if (jxRecipe == null)
                return;

            if (toModel)
            {
                // RESERVED
            }
            else
            {
                var suckerID = _activeSuckerRowID;
                setJxVisible(jxRecipe.InkMarkSettings1, suckerID == SuckerRowEnum.S1);
                setJxVisible(jxRecipe.InkMarkSettings2, suckerID == SuckerRowEnum.S2);
                pgvPanel.BuildGuiCtrls(jxRecipe);
                pgvPanel.ExpandAll();
            }
        }
        void setJxVisible(JxContainer jx, bool visible)
        {
            if (jx == null) return;

            if (jx.IsHidden() == !visible)
                return;

            if (visible)
            {
                var desc = jx.Description?.Replace("隱藏", "")?.Replace("Hidden", "")?.Replace("()", "");
                jx.Description = desc;
            }
            else
            {
                var desc = jx.Description;
                if (desc == null) desc = "";
                jx.Description = desc + "(隱藏)";
            }
        }
        void updateGuiStatus()
        {
            //var imgPanel = _getImgViewPanel(); 
            //bool isEmpty = _isEmptyImage();

            _btnGrabImage.Enabled = !_isRunning;
            _btnLoadImage.Enabled = !_isRunning;

            _btnRunAutoFetchAll.Visible = !_isRunning;
            _btnBuildCalib.Visible = !_isRunning;

            //var bkColor = isInkMarkMode ? Color.Black : Color.DimGray;
            //foreach (int col in new[] { 3, 4 })
            //{
            //    _dgvCalibPointsListView.SetReadOnly(col, !isInkMarkMode, bkColor);
            //}
            //_dgvCalibPointsListView.Enabled = isInkMarkMode;
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
                var matViewer = _getMatViewer();

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
            _commonBaseTrf.ConfigWorldGridPoints(_activeCarrierID, rows, cols, pitchX, pitchY);
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
                _getMatViewer()?.Refresh();
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
        void updateInkMarkBoxes(EzBloc[] inkMarks, EzBloc[] padBlocs, bool refresh = false)
        {
            if (_cviInkMarkBoxes == null)
                return;

            int N = _cviInkMarkBoxes.Length;
            int NInks = inkMarks != null ? inkMarks.Length : 0;
            int NPads = padBlocs != null ? padBlocs.Length : 0;

            for (int i = 0; i < N; i++)
            {
                var cviBox = _cviInkMarkBoxes[i];
                if (cviBox == null) continue;

                var mark = i < NInks ? inkMarks[i] : null;
                if (mark != null)
                {
                    cviBox.SetBox(mark);
                    cviBox.Color = Color.Orange;
                    cviBox.Visible = true;
                    continue;
                }

                var pad = i < NPads ? padBlocs[i] : null;
                if(pad !=null)
                {
                    cviBox.SetBox(pad);
                    cviBox.Color = Color.Red;
                    cviBox.Visible = true;
                    continue;
                }

                cviBox.Visible = false;
            }

            if (refresh)
                _getMatViewer()?.Refresh();
        }
        /// <summary>
        /// 如果找不到 inkMark, 就用 padBloc 暫代.
        /// </summary>
        EzBloc[] makePseudoInkMarks(EzBloc[] inkMarks, EzBloc[] padBlocs)
        {
            // 如果找不到 inkMark, 就用 padBloc 暫代.
            var NI = inkMarks != null ? inkMarks.Length : 0;
            var NP = padBlocs != null ? padBlocs.Length : 0;
            var N = Math.Max(NI, NP);

            var pseudoMarks = new EzBloc[N];
            for (int i = 0; i < N; i++)
            {
                var mark = i < NI ? inkMarks[i] : null;
                if (mark == null && i < NP)
                    mark = padBlocs[i];         // 如果找不到 inkMark, 就用 padBloc 暫代.
                pseudoMarks[i] = mark;
            }

            return pseudoMarks;
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
                name = QMSG.Text(name);
                dgv.Rows.Add(name, "", "", "", "");
            }

            dgv.CellContentDoubleClick += Dgv_CellContentDoubleClick;
            dgv.CellContentClick += Dgv_CellContentClick;
            dgv.CellValueChanged += Dgv_CellValueChanged;

            int COLS = dgv.Columns.Count;
            for (int c = 0; c < COLS; c++)
                dgv.Columns[c].HeaderText = QMSG.Text(dgv.Columns[c].HeaderText, "gui");
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
            msg += $"\n\r\n\r(X= {currentMotorPos.X:0.000}, Y= {currentMotorPos.Y:0.000})";
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

        bool SetActiveView(CarrierEnum C, SuckerRowEnum S, object dummy = null, bool force = false)
        {
            bool isAnyChanged = _activeCarrierID != C || _activeSuckerRowID != S;
            var oldImgFile = CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID);
            var newImgFile = CALIB_LAST_IMAGE_FILE(C, S);
            bool needsToLoadNewImage = _isEmptyImage() || oldImgFile != newImgFile;

            _activeCarrierID = C;
            _activeSuckerRowID = S;

            // Loading Image
            if (needsToLoadNewImage || force)
            {
                LoadImage(newImgFile);
            }

            if (isAnyChanged || force)
            {
                updateBoundRectBox(false);

                var inkMarks = getActiveInkMarksInRecipe();
                var motorCoords = getActiveMotorCoordsInTrf();
                
                dgvUpdateInkMarks(inkMarks);
                dgvUpdateMotorCoords(motorCoords);

                //bool refresh1 = _activeViewID == CalibViewEnum.BigGridBoardView;
                updateBoardGridBox(getActiveBoardGridInTrf(), true);

                //bool refresh2 = _activeViewID == CalibViewEnum.InkMarksView;
                updateInkMarkBoxes(inkMarks, null, true);

                updatePropertyPanel(false);
                updateGuiStatus();
            }

            return needsToLoadNewImage;
        }
        
        void TakeOverImage(Bitmap bigBmp, string srcName, object vid = null, bool disposeSrc = true)
        {
            var imgPanel = _getImgViewPanel();
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
        /// 自動抓取所有校正點
        /// </summary>
        bool RunAutoFetchAll(bool dump = false)
        {
            bool ok = RunAutoFetchBoardGrid(dump);
            ok = ok && RunAutoFetchInkMarks(dump);
            return ok;
        }

        /// <summary>
        /// 自動抓取 校正板 的 陣列圓點
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
                var matViewer = _getMatViewer();
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
                updateInkMarkBoxes(null, null, refresh: true);

                //(2) image
                var matViewer = _getMatViewer();
                var fullfovImg = matViewer.Image;
                if (fullfovImg == null)
                    return false;

                //(1) Run AOI
                (var inkMarks, var padBlocs) = _calibModel.FetchInkMarks(_activeCarrierID, _activeSuckerRowID, fullfovImg, getActiveCalibRecipe());
                var pseudoInkMarks = makePseudoInkMarks(inkMarks, padBlocs);

                //(2) Update InkMarks to GUI
                setActiveInkMarksToRecipe(pseudoInkMarks);
                updateInkMarkBoxes(inkMarks, padBlocs, refresh: true);
                dgvUpdateInkMarks(pseudoInkMarks);

                //(3) MessageBoxe
                #region MESSAGE_BOX
                GaUtil.SetCursor(_wndOwner, oldCursor);
                if (inkMarks == null || inkMarks.Length < 4)
                {
                    string msg = GaUtil.GetEnumDescription(ErrorCodes.WARN_CAN_NOT_FETCH_INKS);
                    //MessageBox.Show(msg, "Calib", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    VsMessageBox.Warning(msg);
                }
                else if (dump)
                {
                    //string msg = "已成功保存二值化圖檔\n\r於 d:\\paso.log\\Calib";
                    string msg = GaUtil.GetEnumDescription(Prompts.Info_Save_Binary_Image_OK) + "\n\r@ d:\\paso.log\\Calib";
                    VsMessageBox.Info(msg);
                }
                #endregion

                return (inkMarks != null && inkMarks.Length >= 4);
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

            dgvGetUserInputMotorCoords(out var userInputMotorCoords);

            var inkMarks = getActiveInkMarksInRecipe();
            var inkMarkPts = Array.ConvertAll(inkMarks, im => im.Center);

            if (verify)
            {
                #region 驗證參數數據
                bool ok = VerifyGuiInkPoints(inkMarkPts, out string errMsg);
                if (!ok)
                {
                    if (!WarningToContinue(errMsg))
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
                    var errMsg = GaUtil.GetEnumDescription(err) + "\n\r\n\r" + errDetails;
                    if (!WarningToContinue(errMsg))
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

        bool VerifyGuiInkPoints(QVector[] inkMarkPts, out string errMsg)
        {
            var errs = new List<string>();
            for (int i = 0, NP = inkMarkPts.Length; i < NP; i++)
            {
                var diff = inkMarkPts[i] - _cviInkMarkBoxes[i].Quad2D.Center;
                if (diff.NormLength > PIXEL_TOLERANCE)
                {
                    errs.Add($"  墨點 [{i}]: 參數({inkMarkPts[i].X:F1}, {inkMarkPts[i].Y:F1}) vs GUI({_cviInkMarkBoxes[i].Quad2D.Center.X:F1}, {_cviInkMarkBoxes[i].Quad2D.Center.Y:F1})");
                }
            }

            if (errs.Count > 0)
            {
                errMsg = $"以下 墨點, 參數 與 GUI, 兩者誤差超過 {PIXEL_TOLERANCE} Pixels:\n\r\n\r";
                errMsg += string.Join("\n\r", errs);
                return false;
            }

            errMsg = null;
            return true;
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

            //(1) 檢查 WorldGrid
            var worldGrid = _commonBaseTrf?.GetWorldGridPoints(_activeCarrierID);
            if (worldGrid == null)
                return ErrorCodes.NO_RUNTIME_PLC_GRID;

            //(2) 檢查 PitchX, PitchY 的一致性
            var traySettings = getActiveCalibRecipe()?.GridSettings?.EmptyTraySettings;
            if (traySettings != null && worldGrid != null)
            {
                string err = "";
                double rcpPitchX = Math.Round((double)traySettings.PitchX.Value, 3);
                double rcpPitchY = Math.Round((double)traySettings.PitchY.Value, 3);
                double plcPitchX = Math.Round(worldGrid.PitchX, 3);
                double plcPitchY = Math.Round(worldGrid.PitchY, 3);

                if (Math.Abs(rcpPitchX - plcPitchX) > PITCH_TOLERANCE)
                    err += $"\n\r參數 PitchX={rcpPitchX:F3}  vs  共用座標系統 PitchX={plcPitchX:F3}";

                if (Math.Abs(rcpPitchY - plcPitchY) > PITCH_TOLERANCE)
                    err += $"\n\r參數 PitchY={rcpPitchY:F3}  vs  共用座標系統 PitchY={plcPitchY:F3}";

                if (!string.IsNullOrEmpty(err))
                {
                    errDetails = err + $"\n\r差異超過 {PITCH_TOLERANCE:F3} mm";
                    return ErrorCodes.CalibErr_pitch_not_consistent;
                }
            }

            //(3) 驗證 馬達座標轉換 結果
            #region 驗證座標轉換結果
            var inkMarks = getActiveInkMarksInRecipe();
            var inkMarkPts = Array.ConvertAll(inkMarks, m => m.Center);
            dgvGetUserInputMotorCoords(out var userInputMotorCoords);
            var trfCamToMotor = _commonBaseTrf.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
            var motorPts = Array.ConvertAll(inkMarkPts, camPt => trfCamToMotor.Trans(camPt));
            var errs = new List<string>();
            for (int i = 0, NP = motorPts.Length; i < NP; i++)
            {
                var diff = motorPts[i] - userInputMotorCoords[i];
                if (diff.NormLength > TRANS_TOLERANCE)
                {
                    errs.Add($" 馬達點位[{i}]: 轉換後({motorPts[i].X:F3}, {motorPts[i].Y:F3}) vs 輸入({userInputMotorCoords[i].X:F3}, {userInputMotorCoords[i].Y:F3})");
                }
            }
            if (errs.Count > 0)
            {
                var errMsg = $"以下馬達點位, 座標轉換與User輸入,誤差超過 {TRANS_TOLERANCE:F3} mm:\n\r";
                errMsg += string.Join("\n\r", errs);
                return ErrorCodes.CalibErr_Motor_Coords_Transform_Build_NG;
            }
            #endregion

            return ErrorCodes.OK;
        }
        bool WarningToContinue(string errMsg)
        {
            if (string.IsNullOrEmpty(errMsg))
                return true;

            //var ret = VsMessageBox.Question(errMsg + "\n\r\n\r是否繼續?", Color.HotPink);
            //return ret == DialogResult.OK;

            var ret = MessageBox.Show(errMsg + "\n\r\n\r是否繼續?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            return ret == DialogResult.Yes;
        }

        void OpenMotorWindowXY(QVector directTargetPos = null)
        {
            if(!OPT_SUPPORT_MOTOR_JOG)
            {
                VsMessageBox.Info("Funtion Reserved!");
                return;
            }

            using (var dlg = new FormMotors_CarierSuckerXY())
            {
                dlg.OnInkerCoordsUpdated += Dlg_OnInkerCoordsUpdated;
                dlg.OnQueryInkerCoords += Dlg_OnQueryInkerCoords;
                dlg.SetJogTargets(_activeCarrierID, _activeSuckerRowID, directTargetPos);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(_wndOwner);
            }
        }

        void LoadImage(string fileName = null)
        {
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
                TakeOverImage(bigBmp, srcName, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                var dstFileName = CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID);
                if (dstFileName != fileName)
                    SaveImage(dstFileName);

                GaUtil.SetCursor(_wndOwner, oldCursor);

                updateBoardGridBox(null, false);
                updateInkMarkBoxes(null, null, true);
            }
            else
            {
                if (!isBrowsing)
                {
                    // 2025-11-20 (搭配點墨)
                    TakeOverImage(null, "", disposeSrc: false);
                    updateBoardGridBox(null, false);
                    updateInkMarkBoxes(null, null, true);
                }
            }
        }
        void SaveImage(string fileName)
        {
            var imgPanel = _getImgViewPanel();
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var img = imgPanel?.Image;
            img?.SaveImage(fileName);
            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        
        void GrabLineScanCameraImage()
        {
            updateGuiStatus();
            var msg = GaUtil.GetEnumDescription(Prompts.Question_ReTrigger_Carrier_LineScan_Camera);
            bool rescan = VsMessageBox.Question(msg) == DialogResult.OK;

            if (rescan)
            {
                //var ps = LineScanProcess.Instance;
                //ps.Start("USER_TRIGGER");
                MessageBox.Show("驅動 PLC 重新線掃 : 尚未完成 !");
                UpdateLineScanCameraImage();
            }
            else
            {
                UpdateLineScanCameraImage();
            }
        }
        void UpdateLineScanCameraImage()
        {
            updateGuiStatus();
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                // 2025-09-14 _imgViewer 會複製 bigBmp
                var bigBmp = freeBmp.ToBitmap();
                var srcName = "[校正] 線掃相機 擷圖";

                // 2025-11-20 (搭配點墨)
                TakeOverImage(bigBmp, srcName, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                SaveImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID));

                updateBoardGridBox(null, false);
                updateInkMarkBoxes(null, null, true);
            }
            GaUtil.SetCursor(_wndOwner, oldCursor);
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
            var view = _calibToolUI.wndFocusMotorGoPanel;

            if(!OPT_SUPPORT_MOTOR_JOG)
            {
                view.Enabled = false;
                //int x = view.Left;
                //int x2 = _btnBuildCalib.Right;
                //_btnBuildCalib.Left = x;
                //_btnBuildCalib.Width = x2 - x;
                //_btnRunAutoFetchAll.Left = x;
                //_btnRunAutoFetchAll.Width = x2 - x;
                return;
            }

            // 只允許 attach 一次
            if (_focusMotorCtrl != null)
                return;

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
