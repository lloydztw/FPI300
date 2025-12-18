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

using EzAoiEmptyTrayInspector;
using EzAoiEmptyTrayInspector.Model;
using JetEazy;
using JetEazy.EzImage;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Match;
using JetEazy.OpenCV.Viewer;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.Calib;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LeTian.JxProps.Gui;
using NLog;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

using CviBoundBox = EzAoiEmptyTrayInspector.Ctrl.CviRcpBox;
using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;
using CviGridDotBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;
using QCoord = JetEazy.QMath.QVector;


namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaCalibCtrl
    {
        #region CONSTANTS
        static int N_CALIB_POINTS => TravellerTransforms.N_CALIB_POINTS;
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
            [Description("大校正板")]
            BigGridBoardView,
            [Description("點墨小校正塊")]
            SmallDotBlocsView,
        };
        #endregion

        #region ACTIVE_MODES
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        CalibViewEnum _activeViewID = CalibViewEnum.BigGridBoardView;
        #endregion

        #region GLOBAL_PATH
        internal static string CALIB_TRANSFORMS_FILE => GaMvcPaths.CALIB_TRANSFORMS_FILE;
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

        #region GLOBAL_MESS
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        TravellerTransforms _transforms => GaMvcConfig.SysModel.TransformsModel;
        #endregion

        #region CALIB_AOI_MODEL
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
        Button _btnPickupGolden => _calibToolUI.btnPickupGolden;
        Button _btnRunAutoFetchGrid => _calibToolUI.btnRunAutoFetch;
        Button _btnBuildCalib => _calibToolUI.btnBuildCalib;
        Button _btnAutoFetchInkMarks => _calibToolUI.btnAutoFetchInkPts;
        Button _btnBuildCalibInkAdj => _calibToolUI.btnBuildCalibInkAdj;
        Button _btnCancel => _calibToolUI.btnCancel;
        Button _btnOK => _calibToolUI.btnOK;
        #endregion

        #region INTERACTORS
        CviBoundBox _cviBigBoundBox = new CviBoundBox(Brushes.Blue, 1, 3) { Visible = false };
        CviCalibResultBox _cviGridResultBox = new CviCalibResultBox() { Visible = false };
        CviCalibPointBox[] _cviCalibPointBoxes = new CviCalibPointBox[N_CALIB_POINTS];
        CviCalibPointBox[] _cviInkPointBoxes = new CviCalibPointBox[N_CALIB_POINTS];
        #endregion

        #region RUNTIME_DATA
        MatchResult _lastMatchResult;
        bool _isCoordModified = false;
        bool _isGoldenPicking = false;
        bool _isRunning = false;        // 因為目前是單執行緒, 所以此變量用處不大.
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            //_btnPickupGolden.Tag = _btnPickupGolden.BackColor;

            InitRecipeBody();
            initImgViewers();
            LoadSettings();

            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void InitRecipeBody()
        {
            for (int i = 0, N = _jxCalibRecipes.Length; i < N; i++)
            {
                var old = _jxCalibRecipes[i];
                _jxCalibRecipes[i] = new JxCalibRecipe();
                old?.Dispose();
            }
        }
        void CleanUp()
        {
            CalibAoiModel.OPT_DUMP = false;

            for (int i = 0, N = _jxCalibRecipes.Length; i < N; i++)
            {
                var old = _jxCalibRecipes[i];
                disconnectPropEventHandlers(old);
                _jxCalibRecipes[i] = null;
                old?.Dispose();
            }
        }
        void initImgViewers()
        {
            //int viewID = 0;
            //var imgViewer = _calibToolUI.ImgViewers[viewID];
            //var matViewer = imgViewer.MatViewer;
            //for (int i = 0; i < N_CALIB_POINTS; i++)
            //{
            //    _cviCalibPointBoxes[i] = new CviCalibPointBox(new Rectangle(0, 0, 500, 500), Color.Lime, 0.25f)
            //    {
            //        Visible = false,
            //        CrossLength = 100,
            //    };
            //    matViewer.AddInteractor(_cviCalibPointBoxes[i]);
            //}
            //matViewer.AddInteractor(_cviGoldenBox);
            //matViewer.AddInteractor(_cviResultBox);

            foreach(CalibViewEnum vid in Enum.GetValues(typeof(CalibViewEnum)))
            {
                var matViewer = _getMatViewer(vid);
                matViewer.AddInteractor(_cviBigBoundBox);
                matViewer.AddInteractor(_cviGridResultBox);
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

            //if (_btnPickupGolden != null)
            //    _btnPickupGolden.Click += (s, e) => toggleGoldenPicking();
            //_cviGoldenBox.OnBoxSelected += (s, e) => BuildGolden();

            if (_btnRunAutoFetchGrid != null)
                _btnRunAutoFetchGrid.Click += (s, e) => RunAutoFetchGrid();

            if (_btnBuildCalib != null)
                _btnBuildCalib.Click += (s, e) => BuildAllTransforms();

            if (_btnAutoFetchInkMarks != null)
                _btnAutoFetchInkMarks.Click += (s, e) => RunAutoFetchInkMarks();

            if (_btnBuildCalibInkAdj != null)
                _btnBuildCalibInkAdj.Click += (s, e) => BuildAllTransforms_for_Ink_Adjustment();

            _cviGridResultBox.OnRequestDumpBindaryImage += (s, e) => RunAutoFetchGrid(dump: true);

            // ImgViewrs' EventHanlders
            var imgViewer0 = _calibToolUI.ImgViewers[0];
            imgViewer0.MatViewer.MouseMove += MatViewer_MouseMove;

            // 自動釋放資源
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();

            // 延遲更新
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    //>>> LoadImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID));
                    updateAllData(false);
                    SetActiveView(_activeCarrierID, _activeSuckerRowID, _activeViewID);
                    updateGuiStatus();
                }));
            };
        }
        void connectPropEventHandlers(JxCalibRecipe rcp)
        {
            // RESERVED
        }
        void disconnectPropEventHandlers(JxCalibRecipe rcp)
        {
            // RESERVED
        }
        #endregion

        #region EVENT_HANDLERS
        private void _calibToolUI_OnActiveViewChanged(object sender, EventArgs e)
        {
            var vid = (CalibViewEnum)_calibToolUI.ActiveViewID;
            if (vid != _activeViewID)
            {
                SetActiveView(_activeCarrierID, _activeSuckerRowID, vid);
                //updateVisionGridParams(_activeCarrierID, false, false);
                //updateGuiStatus();
            }
        }
        private void _rdoSelect_CheckedChanged(object sender, System.EventArgs e)
        {
            //_activeCarrierID = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            //_activeSuckerRowID = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;

            //if (Array.IndexOf(_rdoCarriers, sender) >= 0)
            //{
            //    updateVisionGridParams(_activeCarrierID, false, toReloadRecipe: true);
            //}

            //updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);

            //if (_activeViewID == 0)
            //{
            //    clearCviGridResults();
            //}

            //_cviGridResultBox.ActiveCarrierID = _activeCarrierID;
            //_cviGridResultBox.ActiveSuckerRowID = _activeSuckerRowID;

            var C = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            var S = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;
            if (C == _activeCarrierID && S == _activeSuckerRowID)
                return;

            var vid = _activeViewID;
            SetActiveView(C, S, vid);

            if (Array.IndexOf(_rdoCarriers, sender) >= 0)
            {
                updateVisionGridParams(_activeCarrierID, false, toReloadRecipe: true);
            }

            if (vid == CalibViewEnum.BigGridBoardView)
            {
                updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);
                clearCviGridResults();
                _cviGridResultBox.ActiveCarrierID = _activeCarrierID;
                _cviGridResultBox.ActiveSuckerRowID = _activeSuckerRowID;
            }
        }
        private void MatViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dgvCalibPointsListView == null)
                return;

            if (_cviCalibPointBoxes == null)
                return;

            int x = e.X;
            int y = e.Y;

            var imgPanel = _getImgViewPanel(CalibViewEnum.BigGridBoardView);
            imgPanel.MatViewer.TransCoordToWorld(ref x, ref y);

            for (int i = 0; i < N_CALIB_POINTS; i++)
            {
                var box = _cviCalibPointBoxes[i];
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
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        void updateAllData(bool toModel)
        {
            updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, toModel);
            updateVisionGridParams(_activeCarrierID, toModel, toReloadRecipe: true);
            updateVisionInkMarkParams(toModel);
        }
        void updateVisionGridParams(CarrierEnum carrierID, bool toModel, bool toReloadRecipe = true)
        {
            GwPanePropsViewer pgvPanel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (pgvPanel == null)
                return;

            var jxRecipe = _jxCalibRecipes[(int)carrierID];
            if (jxRecipe == null)
                return;

            //var vid = CalibViewEnum.BigGridBoardView;
            var suckerID = _activeSuckerRowID;

            if (toModel)
            {
            }
            else
            {
                if (toReloadRecipe)
                {
                    // 設定參數
                    _calibModel.SetRecipe(jxRecipe);

                    //// 載入圖片
                    //var imgPanel = _getImgViewPanel(vid);
                    //var fileName = CALIB_LAST_IMAGE_FILE(carrierID, suckerID, (int)vid);
                    //LoadImage(fileName, (int)vid);
                }

                var jx = suckerID == SuckerRowEnum.S1 ? jxRecipe.InkMarkSettings : jxRecipe.InkMarkSettings2;
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
            //var vid = CalibViewEnum.SmallDotBlocsView;

            var jxRecipe = _jxCalibRecipes[(int)carrierID];
            if (jxRecipe == null)
                return;

            if (toModel)
            {
            }
            else
            {
                if (_activeViewID == CalibViewEnum.SmallDotBlocsView)
                {
                    var jx = suckerID == SuckerRowEnum.S1 ? jxRecipe.InkMarkSettings : jxRecipe.InkMarkSettings2;
                    pgvPanel.BuildGuiCtrls(jx);
                    pgvPanel.ExpandAll();
                    updateBoundBox(false);
                }
            }
        }
        void updateCalibKeyPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID, bool toModel)
        {
            if (_activeViewID != 0 && toModel)
                return;

            var dgv = _dgvCalibPointsListView?.DataGridView;
            if (dgv == null) return;

            var corners = Enum.GetValues(typeof(CalibCornersEnum));
            var transform = _transforms.GetCameraMotorTransform(carrierID, suckerRowID);
            var trfCalibCorners = transform.GetCalibCornerPoints();
            var camPts = trfCalibCorners?.GetAll(isSrc: true);
            var motorPts = trfCalibCorners?.GetAll(isSrc: false);

            if (toModel)
            {
                if (transform == null)
                    return;

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
                foreach (CalibCornersEnum corner in corners)
                {
                    int rowId = (int)corner;
                    var camPt = camPts != null ? camPts[rowId] : null;
                    var motorPt = motorPts != null ? motorPts[rowId] : null;

                    var name = GaUtil.GetEnumDescription(corner);
                    dgv.Rows.Add(name, 0.0, 0.0, 0.0, 0.0);
                    dgv.Rows[rowId].Cells[0].Value = GaUtil.GetEnumDescription(corner);
                    updateCalibKeyPoints(dgv, rowId, camPt, motorPt, toModel);
                    updateCalibKeyPointBox(corner, camPt);
                }
            }
        }
        bool updateCalibKeyPoints(DataGridView dgv, int rowId, QCoord camCoord, QCoord motorCoord, bool toModel)
        {
            if (_activeViewID != 0 && toModel)
                return false;

            int col = 1;
            var dgvRow = dgv.Rows[rowId];

            if (toModel)
            {
                bool isChanged = false;

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
                dgvRow.Cells[col++].Value = camCoord != null ? camCoord.X : 0.0;
                dgvRow.Cells[col++].Value = camCoord != null ? camCoord.Y : 0.0;
                dgvRow.Cells[col++].Value = motorCoord != null ? motorCoord.X : 0.0;
                dgvRow.Cells[col++].Value = motorCoord != null ? motorCoord.Y : 0.0;
                return true;
            }
        }
        void updateCalibKeyPointBox(CalibCornersEnum corner, QCoord camCoord, bool show = true)
        {
            //if (_activeViewID != 0)
            //    return;

            if (_cviCalibPointBoxes == null)
                return;

            var box = _cviCalibPointBoxes[(int)corner];
            if (box == null)
                return;

            if (camCoord == null)
            {
                box.Visible = false;
                return;
            }

            //var loc = box.Box2D;
            //loc.SetCenter((float)camCoord.X, (float)camCoord.Y);
            //box.SetBox(loc);

            box.Quad2D?.SetCenter(camCoord.X, camCoord.Y);
            box.Visible = show;
        }
        void updateCalibKeyPoints(EzBlocsGrid camGrid)
        {
            if (_activeViewID != 0)
                return;

            if (camGrid == null)
                return;

            //(1) 取出參數設定的 pitch, rows, cols
            var jxRecipe = _jxCalibRecipes[(int)_activeCarrierID];
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
        void updateAllCalibKeyPointBoxes()
        {
            if (_activeViewID != 0)
                return;

            var carrierID = _activeCarrierID;
            var suckerRowID = _activeSuckerRowID;

            var transform = _transforms.GetCameraMotorTransform(carrierID, suckerRowID);
            var trfCorners = transform.GetCalibCornerPoints();
            var camPts = trfCorners?.GetAll(isSrc: true);

            var corners = Enum.GetValues(typeof(CalibCornersEnum));
            foreach (CalibCornersEnum corner in corners)
            {
                int rowId = (int)corner;
                var camPt = camPts != null ? camPts[rowId] : null;
                updateCalibKeyPointBox(corner, camPt);
            }
        }
        void updateGuiStatus()
        {
            var vid = _activeViewID;
            var imgPanel = _getImgViewPanel(vid);    // _calibToolUI.ImgViewers[viewID];
            bool isEmpty = _isEmptyImage(vid);
            bool isInkMarkMode = vid == CalibViewEnum.SmallDotBlocsView;

            _btnGrabImage.Enabled = !_isRunning;
            _btnLoadImage.Enabled = !_isRunning;

            _btnPickupGolden.Visible = false;   // !isInkViewingMode;
            _btnRunAutoFetchGrid.Visible = !_isRunning && !isInkMarkMode;
            _btnAutoFetchInkMarks.Visible = !_isRunning && !isInkMarkMode;
            _btnBuildCalibInkAdj.Visible = !_isRunning && !isInkMarkMode;
            _btnBuildCalib.Visible = false;     // !_isRunning && isInkMarkMode;

            var bkColor = isInkMarkMode ? Color.Gray : Color.Black;
            foreach (int col in new[] { 3, 4 })
            {
                _dgvCalibPointsListView.SetReadOnly(col, isInkMarkMode, bkColor);
            }
        }
        void clearCviGridResults()
        {
            var vid = _activeViewID;
            if (vid != CalibViewEnum.BigGridBoardView)
                return;

            var matViewer = _getMatViewer(vid);
            _cviGridResultBox.Reset();
            matViewer.Invalidate();
        }
        void updateBoundBox(bool toModel)
        {
            if (_activeViewID != CalibViewEnum.SmallDotBlocsView)
                return;

            var jxRecipe = _jxCalibRecipes[(int)_activeCarrierID];
            var jxSettings = _activeSuckerRowID == SuckerRowEnum.S1 ? jxRecipe?.InkMarkSettings : jxRecipe?.InkMarkSettings2;
            if (jxSettings == null)
                return;

            var rcpPanel = _calibToolUI.wndVisionSettingsPanel;
            var jxRect = jxSettings.Marks.BoundRect;

            if (toModel)
            {
                jxRect.Value = _cviBigBoundBox.Box;
                rcpPanel?.Refresh();
            }
            else
            {
                var rect = jxRect.Value;

                if (rect == Rectangle.Empty)
                {
                    var image = _calibToolUI.ImgViewers[1].Image;
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
                    _calibToolUI.ImgViewers[1]?.MatViewer?.Invalidate();
                }
            }
        }
        #endregion

        void SetActiveView(CarrierEnum C, SuckerRowEnum S, CalibViewEnum vid)
        {
            var oldImgFile = CALIB_LAST_IMAGE_FILE(_activeCarrierID, _activeSuckerRowID, (int)_activeViewID);
            var newImgFile = CALIB_LAST_IMAGE_FILE(C, S, (int)vid);
            bool needsToLoadImage = _isEmptyImage(vid) || oldImgFile != newImgFile;
            bool isAnyChanged = _activeCarrierID != C || _activeSuckerRowID != S || _activeViewID != vid;

            _activeCarrierID = C;
            _activeSuckerRowID = S;
            _activeViewID = vid;

            // Loading Image
            if (needsToLoadImage)
            {
                LoadImage(newImgFile, (int)vid);
                _getMatViewer(vid)?.Invalidate();
            }

            if (isAnyChanged)
            {
                if (_cviInkPointBoxes != null)
                {
                    foreach (var box in _cviInkPointBoxes)
                        if (box != null)
                            box.Visible = false;
                }

                if (vid == CalibViewEnum.BigGridBoardView)
                {
                    updateVisionGridParams(_activeCarrierID, false, false);
                }
                else
                {
                    updateVisionInkMarkParams(false);
                }

                updateGuiStatus();
            }
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

        #region PRIVATE_ACTION_FUNCTIONS
        void ShowCviResult(bool show, bool clear = false)
        {
            var vid = _activeViewID;
            if (vid != CalibViewEnum.BigGridBoardView)
                return;

            var matViewer = _getMatViewer(vid);

            if (clear)
            {
                clearCviGridResults();
                return;
            }

            if (_cviGridResultBox.Visible != show)
            {
                _cviGridResultBox.Visible = show;
                matViewer.Refresh();
            }
        }

        void BuildGolden()
        {
            //var viewID = _activeViewID;
            //if (viewID != 0)
            //    return;

            //var imgViewer = _calibToolUI.ImgViewers[viewID];

            //clearCviGridResults();

            //var fullfovImg = imgViewer.MatViewer.Image;
            //if (fullfovImg == null)
            //    return;

            ////(1) Peek the ezImage
            //var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

            ////(2) Cropping the golden Bitmap
            //var goldenRect = _cviGoldenBox.Box;
            //GaUtil.Clip(ref goldenRect, ezImage.Width, ezImage.Height);
            //var goldenBmp = ImageUtil.CropBmp(ezImage, goldenRect);
            ////_aoiModel.CropGoldenTemplate(SideID.A, ezImage, goldenRect);

            ////(3) Update to Recipe
            //var jxRecipe = _jxCalibRecipes[(int)_activeCarrierID];
            //var matchSetting = jxRecipe?.VisionSettings?.Match;
            //if (matchSetting != null)
            //{
            //    matchSetting.GoldenBmp.Value = goldenBmp;
            //    matchSetting.GoldenBox.Value = goldenRect;
            //    updateVisionGridParams(_activeCarrierID, false, toReloadRecipe: false);
            //}

            ////(4) CleanUp
            //ezImage?.Dispose();
        }
        bool BuildGoldenGrid()
        {
            var vid = _activeViewID;
            if (vid != CalibViewEnum.BigGridBoardView)
                return false;

            if (_isEmptyImage(vid))
                return false;

            var imgPanel = _getImgViewPanel(vid);  // _calibToolUI.ImgViewers[vid];

            var fullfovImg = imgPanel.MatViewer.Image;
            if (fullfovImg == null)
                return false;

            ShowCviResult(false);

            //(1) Peek the ezImage
            var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            //(2) Reset
            _calibModel.ResetAndClear();

            //(3) Build
            var err = _calibModel.BuildGoldenGridTemplate(SideID.A, ezImage);
            if (err != ErrCodes.OK)
            {
                var msg = QxNums.GetEnumDescription(err);
                MessageBox.Show(msg, _wndOwner.FindForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //(4) CleanUp
            ezImage?.Dispose();

            GaUtil.SetCursor(_wndOwner, oldCursor);

            return err == ErrCodes.OK;
        }

        bool RunAutoFetchGrid(bool dump = false, bool force = false)
        {
            var vid = _activeViewID;
            if (vid != CalibViewEnum.BigGridBoardView && !force)
                return false;

            var imgPanel = _getImgViewPanel(vid);    // _calibToolUI.ImgViewers[0];

            try
            {
                CalibAoiModel.OPT_DUMP = dump;

                //enableGoldenPicking(false);
                ShowCviResult(false, clear: true);

                bool ok = BuildGoldenGrid();
                if (!ok)
                    return false;

                var fullfovImg = imgPanel.MatViewer.Image;
                if (fullfovImg == null)
                    return false;

                //(0) Cursor
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                //(0.1) clear 4 corners
                foreach (var cviCornerBox in _cviCalibPointBoxes)
                    cviCornerBox.Quad2D.SetCenter(0, 0);

                #region OLD_CODE
                ////(A1) Peek the ezImage
                //var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

                ////(A2) Run Aoi
                //_aoiModel.RunMatch(SideID.A, ezImage);

                ////(A3) Update Result
                //var matchResult = _aoiModel.GetMatchResult(SideID.A);
                //var camGrid = matchResult?.Grid;

                //if (camGrid != null)
                //{
                //    //(A4) Refine each detail locations
                //    _aoiModel.RefineCentroidLocations(matchResult, ezImage);

                //    //(A5) Update Grid 4 Corners To Model
                //    updateCalibKeyPoints(camGrid);
                //}
                #endregion

                //(1) Run AOI
                var matchResult = _calibModel.FetchGridNodes(_activeCarrierID, fullfovImg, refine: true);
                var camGrid = matchResult?.Grid;
                _lastMatchResult = matchResult;

                //(2) Cvi 4 Corners Boxes
                if (camGrid != null)
                    updateCalibKeyPoints(camGrid);


                //(3) Update Grid Result
                _cviGridResultBox.IsEmptyTrayMode = false;
                _cviGridResultBox.TransCameraToMotor = null;
                _cviGridResultBox.TransCameraToWorld = null;
                _cviGridResultBox.UpdateResult(matchResult);
                ShowCviResult(true);

                //(4) Cursor
                GaUtil.SetCursor(_wndOwner, oldCursor);

                //(5) MessageBoxe
                #region MESSAGE_BOX
                if (camGrid == null)
                {
                    string msg = "無法自動抓到 四角定位點!\n\r請確認以下 參數 是否設定為 true?\n\r\n\r 空盤像測參數 \\ 吸嘴比對設定 \\ 建立網格";
                    MessageBox.Show(msg, "Calib", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (dump)
                {
                    VsMessageBox.Info("已成功保存二值化圖檔\n\r於 d:\\paso.log\\Calib");
                }
                #endregion

                return camGrid != null;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                CalibAoiModel.OPT_DUMP = false;
            }
        }
        void BuildAllTransforms(bool force = false)
        {
            if (_activeViewID != 0 && !force)
                return;

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            //enableGoldenPicking(false);
            updateAllData(true);

#if (OPT_OLD_CODE)
            _transforms.BuildAll();

            // 去除異常 格位 再重建一次
            var lastCamGrid = _lastMatchResult?.Grid;
            if (lastCamGrid != null)
            {
                if (_calibModel.AdjustBadNodes(_activeCarrierID, lastCamGrid))
                {
                    updateCalibKeyPoints(lastCamGrid);
                    _cviResultBox.UpdateResult(_lastMatchResult);
                    _transforms.BuildAll();
                }
            }
#endif

            var sysModel = GaMvcConfig.SysModel;
            var lastCamGrid = _lastMatchResult?.Grid;
            bool ok = false;

            if (sysModel != null)
            {
                ok = sysModel.BuildTransformAndRegionCells(_activeCarrierID, lastCamGrid);
                if (ok)
                {
                    updateCalibKeyPoints(lastCamGrid);
                    _cviGridResultBox.UpdateResult(_lastMatchResult);
                }
            }

            _cviGridResultBox.TransCameraToMotor = _transforms.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
            _cviGridResultBox.TransCameraToWorld = _transforms.GetCameraPhysicTransform(_activeCarrierID);

            GaUtil.SetCursor(_wndOwner, oldCursor);

            if (ok)
            {
                string name = GaUtil.GetEnumDescription(_activeCarrierID) + " && " + GaUtil.GetEnumDescription(_activeSuckerRowID);
                VsMessageBox.Info($"{name}\n\r\n\r座標系統建置完成!");
            }
        }

        void RunAutoFetchInkMarks()
        {
            if (_activeViewID == CalibViewEnum.SmallDotBlocsView)
            {
                //_calibInkMarkCtrl.AutoFetchInkPoints();
            }
        }
        void BuildAllTransforms_for_Ink_Adjustment()
        {
            //if (_activeViewID == 1)
            //{
            //    bool ok = _calibInkMarkCtrl.BuildCalibInkAdj();

            //    if (ok)
            //    {
            //        // 暫時將 activeViewID 設定為 0 (主頁)
            //        _activeViewID = 0;
            //        updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);

            //        if (_wndOwner != null)
            //        {
            //            _wndOwner.BeginInvoke(new Action(() =>
            //            {
            //                // 暫時將 activeViewID 設定為 0 (主頁)
            //                _activeViewID = 0;

            //                ok = RunAutoFetch();
            //                if (ok) BuildAllTransforms();

            //                // 恢復 activeViewID 設定為 1 (點墨頁)
            //                _activeViewID = 1;
            //                updateGuiStatus();
            //            }));
            //        }
            //        else
            //        {
            //            // 恢復 activeViewID 設定為 1 (點墨頁)
            //            _activeViewID = 1;
            //            updateGuiStatus();
            //        }
            //    }
            //}
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

            if (vid == CalibViewEnum.BigGridBoardView)
                updateAllCalibKeyPointBoxes();

            updateGuiStatus();
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

        void LoadSettings()
        {
            // (1) Load VISION recipe Files
            int i = 0;
            foreach (var jx in _jxCalibRecipes)
            {
                jx.Load(CALIB_RECIPE_FILE((CarrierEnum)i));
                //var gridVisionSettings = jx.GridSettings.GridVisionSettings;
                //jx.VisionSettings.FindAllFailBlocs.Value = false;           // 強制停用 "尋找所有格外區塊"
                //jx.VisionSettings.Match.BoundBox.Value = Rectangle.Empty;   // 強制停用 "邊界框"
                connectPropEventHandlers(jx);
                i++;
            }

            // (2) Load TRANSFORMS (永遠載入全域設定)
            _transforms.Load(CALIB_TRANSFORMS_FILE);

            // (3) 第一次建置座標轉換
            _transforms.BuildAll();

            // (4) Load Ink Mark Settings
            //_calibInkMarkCtrl?.LoadSettings();
        }
        void SaveSettings(bool force = false)
        {
            // 保存全域設定
            // (1) TRANSFORMS
            if (force || _isCoordModified)
            {
                _transforms.Save(CALIB_TRANSFORMS_FILE);
                _transforms.SaveGaaraIniFile();
            }

            // (2) Save VISION recipe files
            int i = 0;
            foreach (var jx in _jxCalibRecipes)
            {
                if (force || jx.Modified)
                    jx?.Save(CALIB_RECIPE_FILE((CarrierEnum)i));
                i++;
            }

            // (3) Save Ink Mark Settings
            //_calibInkMarkCtrl?.SaveSettings(force);
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
        #endregion
    }
}
