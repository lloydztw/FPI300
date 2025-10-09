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
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LeTian.JxProps.Gui;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;
using JxCalibAoiRecipe = EzAoiEmptyTrayInspector.Model.JxAoiRecipe;
using QCoord = JetEazy.QMath.QVector;


namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaCalibCtrl
    {
        static int N_CALIB_POINTS => TravellerTransforms.N_CALIB_POINTS;

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

        #region GLOBAL_PATH
        static string CALIB_TRANSFORMS_FILE => GaMvcPaths.CALIB_TRANSFORMS_FILE;
        static string CALIB_VISION_FILE(CarrierEnum C)
        {
            return GaMvcPaths.CALIB_VISION_FILE(C);
        }
        static string CALIB_LAST_IMAGE_FILE(CarrierEnum C)
        {
            string file = System.IO.Path.ChangeExtension(CALIB_VISION_FILE(C), ".jpg");
            return file;
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
        ICalibAoiModel _aoiModel => GaMvcConfig.SysModel.CalibAoiModel;
        JxCalibAoiRecipe[] _jxCalibVisionRecipes = new JxCalibAoiRecipe[Enum.GetValues(typeof(CarrierEnum)).Length];
        #endregion

        #region GUI_LINKS
        IvCalibToolUI _calibToolUI;
        Control _wndOwner => _calibToolUI.Window;
        JezTransImageViewPanel _imgViewer => _calibToolUI.ImgViewer;
        GvCalibPointsDataGridView _dgvCalibPointsListView => _calibToolUI.dgvCalibPointsListView;
        RadioButton[] _rdoCarriers => _calibToolUI.rdoCarriers;
        RadioButton[] _rdoSuckerRows => _calibToolUI.rdoSuckerRows;
        Button _btnGrabImage => _calibToolUI.btnGrabImage;
        Button _btnLoadImage => _calibToolUI.btnLoadImage;
        Button _btnPickupGolden => _calibToolUI.btnPickupGolden;
        Button _btnRunAutoFetch => _calibToolUI.btnRunAutoFetch;
        Button _btnBuildCalib => _calibToolUI.btnBuildCalib;
        Button _btnCancel => _calibToolUI.btnCancel;
        Button _btnOK => _calibToolUI.btnOK;
        #endregion

        #region INTERACTORS
        CviGoldenPickingBox _cviGoldenBox = new CviGoldenPickingBox(Brushes.Orange) { Visible = false };
        CviCalibResultBox _cviResultBox = new CviCalibResultBox() { Visible = false };
        CviCalibPointBox[] _cviCalibPointBoxes = new CviCalibPointBox[N_CALIB_POINTS];
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        MatchResult _lastResult;
        bool _isCoordModified = false;
        bool _isGoldenPicking = false;
        bool _isRunning = false;        // 因為目前是單執行緒, 所以此變量用處不大.
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            _btnPickupGolden.Tag = _btnPickupGolden.BackColor;

            InitRecipeBodies();
            initImgViewer();
            LoadSettings();

            //updateAllData(false);
            connectEventHandlers();
        }
        void InitRecipeBodies()
        {
            for (int i = 0, N = _jxCalibVisionRecipes.Length; i < N; i++)
            {
                var old = _jxCalibVisionRecipes[i];
                _jxCalibVisionRecipes[i] = new JxCalibAoiRecipe();
                old?.Dispose();
            }
        }
        void CleanUp()
        {
            CalibAoiModel.OPT_DUMP = false;

            for (int i = 0, N = _jxCalibVisionRecipes.Length; i < N; i++)
            {
                var old = _jxCalibVisionRecipes[i];
                disconnectPropEventHandlers(old);
                _jxCalibVisionRecipes[i] = null;
                old?.Dispose();
            }
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initImgViewer()
        {
            var matViewer = _imgViewer.MatViewer;
            for (int i = 0; i < N_CALIB_POINTS; i++)
            {
                _cviCalibPointBoxes[i] = new CviCalibPointBox(new Rectangle(0, 0, 500, 500), Color.Lime, 0.25f)
                {
                    Visible = false,
                    CrossLength = 100,
                };
                matViewer.AddInteractor(_cviCalibPointBoxes[i]);
            }
            matViewer.AddInteractor(_cviGoldenBox);
            matViewer.AddInteractor(_cviResultBox);
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

            _btnGrabImage.Click += (s, e) => GrabImage();
            _btnLoadImage.Click += (s, e) => LoadImage();

            if (_btnPickupGolden != null)
                _btnPickupGolden.Click += (s, e) => toggleGoldenPicking();

            if (_btnRunAutoFetch != null)
                _btnRunAutoFetch.Click += (s, e) => RunAutoFetch();

            if (_btnBuildCalib != null)
                _btnBuildCalib.Click += (s, e) => BuildAllTransforms();

            _cviGoldenBox.OnBoxSelected += (s, e) => BuildGolden();
            _cviResultBox.OnRequestDumpBindaryImage += (s, e) => RunAutoFetch(dump: true);
            
            _imgViewer.MatViewer.MouseMove += MatViewer_MouseMove;

            // 自動釋放資源
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();

            // 延遲更新
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    //LoadImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID));
                    updateAllData(false);
                    updateGuiStatus();
                }));
            };
        }
        void connectPropEventHandlers(JxCalibAoiRecipe rcp)
        {

        }
        void disconnectPropEventHandlers(JxCalibAoiRecipe rcp)
        {

        }
        #endregion

        #region EVENT_HANDLERS
        private void _rdoSelect_CheckedChanged(object sender, System.EventArgs e)
        {
            _activeCarrierID = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            _activeSuckerRowID = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;

            if (Array.IndexOf(_rdoCarriers, sender) >= 0)
            {
                updateVisionParams(_activeCarrierID, false);
            }

            updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);            
            clearCviResults();

            _cviResultBox.ActiveCarrierID = _activeCarrierID;
            _cviResultBox.ActiveSuckerRowID = _activeSuckerRowID;
        }
        private void _lineScanImageHolder_OnImageChanged(object sender, EventArgs e)
        {
            //if (_wndOwner == null || !_wndOwner.IsHandleCreated)
            //    return;

            //if (_wndOwner.InvokeRequired)
            //{
            //    _wndOwner.Invoke((EventHandler)_lineScanImageHolder_OnImageChanged);
            //}
            //else
            //{
            //    var bmp = _lineScanImageHolder.PeekBitmap();
            //    using (var bridge = new QxImageBridge(bmp))
            //    {
            //        // 暫時使用 Clone 浪費了內存, 但是比較安全.
            //        var old = _imgViewer.MatViewer.Image;
            //        _imgViewer.MatViewer.Image = bridge.Image.Clone();
            //        old?.Dispose();
            //        _imgViewer.lblTitle.Text = _lineScanImageHolder.SrcName;
            //    }
            //}
        }
        private void MatViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dgvCalibPointsListView == null)
                return;

            int x = e.X;
            int y = e.Y;

            _imgViewer.MatViewer.TransCoordToWorld(ref x, ref y);

            for (int i = 0; i < N_CALIB_POINTS; i++)
            {
                var rect = _cviCalibPointBoxes[i].Box2D.BoundaryRect;
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
            updateVisionParams(_activeCarrierID, toModel);
        }
        void updateVisionParams(CarrierEnum carrierID, bool toModel, bool toReloadRecipe = true)
        {
            GwPanePropsViewer panel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (panel == null)
                return;

            if (toModel)
            {
            }
            else
            {
                var jxRecipe = _jxCalibVisionRecipes[(int)_activeCarrierID];

                if (toReloadRecipe)
                {
                    // 設定參數
                    _aoiModel.SetRecipe(jxRecipe);
                    // 載入圖片
                    LoadImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID));
                }

                panel.BuildGuiCtrls(jxRecipe);
                panel.ExpandAll();
            }
        }
        void updateCalibKeyPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID, bool toModel)
        {
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
            var box = _cviCalibPointBoxes[(int)corner];
            if (camCoord == null)
            {
                box.Visible = false;
                return;
            }

            var loc = box.Box2D;
            loc.SetCenter((float)camCoord.X, (float)camCoord.Y);
            box.SetBox(loc);
            box.Visible = show;
        }
        void updateCalibKeyPoints(EzBlocsGrid camGrid)
        {
            if (camGrid == null)
                return;

            //(1) 取出參數設定的 pitch, rows, cols
            var jxRecipe = _jxCalibVisionRecipes[(int)_activeCarrierID];
            var traySettings = jxRecipe.TrayMiscSettings;
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
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void updateGuiStatus()
        {
            bool isEmpty = _imgViewer.MatViewer.Image == null;
            _btnGrabImage.Enabled = !_isRunning;
            _btnLoadImage.Enabled = !_isRunning;

            _btnPickupGolden.BackColor = _isGoldenPicking ? Color.HotPink : (Color)_btnPickupGolden.Tag;
            _btnPickupGolden.Enabled = !_isRunning && !isEmpty;

            if (_btnRunAutoFetch != null)
                _btnRunAutoFetch.Enabled = !_isRunning && !isEmpty;

            if (_btnBuildCalib != null)
                _btnBuildCalib.Enabled = !_isRunning && !isEmpty;
        }
        void toggleGoldenPicking()
        {
            enableGoldenPicking(!_isGoldenPicking);
        }
        void enableGoldenPicking(bool enabled)
        {
            _isGoldenPicking = enabled;
            _cviGoldenBox.Visible = enabled;
            _cviGoldenBox.Enabled = enabled;
            updateGuiStatus();

            if (enabled)
                ShowCviResult(false);
        }
        void clearCviResults()
        {
            _cviResultBox.Reset();
            _imgViewer.MatViewer.Invalidate();
        }
        #endregion

        void ShowCviResult(bool show, bool clear = false)
        {
            if (clear)
            {
                clearCviResults();
                return;
            }

            if (_cviResultBox.Visible != show)
            {
                _cviResultBox.Visible = show;
                _imgViewer.MatViewer.Refresh();
            }
        }
        void EnableGoldenPicking(bool enabled)
        {
            enableGoldenPicking(enabled);
        }

        void BuildGolden()
        {
            clearCviResults();

            var fullfovImg = _imgViewer.MatViewer.Image;
            if (fullfovImg == null)
                return;

            //(1) Peek the ezImage
            var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

            //(2) Cropping the golden Bitmap
            var boundRect = new Rectangle(0, 0, ezImage.Width, ezImage.Height);
            var goldenRect = _cviGoldenBox.Box;
            JetEazy.QUtilities.QUtility.ClipBoundary(ref goldenRect, ref boundRect);
            var goldenBmp = ImageUtil.CropBmp(ezImage, goldenRect);
            //_aoiModel.CropGoldenTemplate(SideID.A, ezImage, goldenRect);

            //(3) Update to Recipe
            var jxRecipe = _jxCalibVisionRecipes[(int)_activeCarrierID];
            var matchSetting = jxRecipe?.VisionSettings?.Match;
            if (matchSetting != null)
            {
                matchSetting.GoldenBmp.Value = goldenBmp;
                matchSetting.GoldenBox.Value = goldenRect;
                updateVisionParams(_activeCarrierID, false, toReloadRecipe: false);
            }

            //(4) CleanUp
            ezImage?.Dispose();
        }
        bool BuildGoldenGrid()
        {
            var fullfovImg = _imgViewer.MatViewer.Image;
            if (fullfovImg == null)
                return false;

            ShowCviResult(false);

            //(1) Peek the ezImage
            var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            //(2) Reset
            _aoiModel.ResetAndClear();

            //(3) Build
            var err = _aoiModel.BuildGoldenGridTemplate(SideID.A, ezImage);
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

        void RunAutoFetch(bool dump = false)
        {
            try
            {
                CalibAoiModel.OPT_DUMP = dump;

                enableGoldenPicking(false);
                ShowCviResult(false, clear: true);

                bool ok = BuildGoldenGrid();
                if (!ok)
                    return;

                var fullfovImg = _imgViewer.MatViewer.Image;
                if (fullfovImg == null)
                    return;

                //(0) Cursor
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                //(0.1) clear 4 corners
                foreach (var cviCornerBox in _cviCalibPointBoxes)
                    cviCornerBox.Box2D.SetCenter(PointF.Empty);

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
                var matchResult = _aoiModel.FetchGridNodes(_activeCarrierID, fullfovImg, refine: true);
                var camGrid = matchResult?.Grid;
                _lastResult = matchResult;

                //(2) Cvi 4 Corners Boxes
                if (camGrid != null)
                    updateCalibKeyPoints(camGrid);


                //(3) Update Grid Result
                _cviResultBox.IsEmptyTrayMode = false;
                _cviResultBox.TransCameraToMotor = null;
                _cviResultBox.TransCameraToWorld = null;
                _cviResultBox.UpdateResult(matchResult);
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
            }
            catch (Exception ex)
            {
            }
            finally
            {
                CalibAoiModel.OPT_DUMP = false;
            }
        }
        void BuildAllTransforms()
        {
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            enableGoldenPicking(false);
            updateAllData(true);

            _transforms.BuildAll();

            // 去除異常 格位 再重建一次
            var lastCamGrid = _lastResult?.Grid;
            if (lastCamGrid != null)
            {
                //if (_aoiModel.RemoveBadNodes(_activeCarrierID, lastCamGrid))
                //{
                //    updateCalibKeyPoints(lastCamGrid);
                //    _cviResultBox.UpdateResult(_lastResult);
                //    _transforms.BuildAll();
                //}
            }

            _cviResultBox.TransCameraToMotor = _transforms.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
            _cviResultBox.TransCameraToWorld = _transforms.GetCameraPhysicTransform(_activeCarrierID);

            GaUtil.SetCursor(_wndOwner, oldCursor);

            string name = GaUtil.GetEnumDescription(_activeCarrierID) + " && " + GaUtil.GetEnumDescription(_activeSuckerRowID);
            VsMessageBox.Info($"{name}\n\r\n\r座標系統建置完成!");
        }

        void LoadImage(string fileName = null)
        {
            enableGoldenPicking(false);

            if (string.IsNullOrEmpty(fileName))
                fileName = GaUtil.BrowseImageFile();

            if (!string.IsNullOrEmpty(fileName) && System.IO.File.Exists(fileName))
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
                _wndOwner?.Refresh();

                // 2025-09-14 _imgViewer 會複製 bigBmp
                var bigBmp = GaImageUtil.LoadBigImage(fileName);
                var srcName = "[校正] " + System.IO.Path.GetFileName(fileName);
                _imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                var dstFileName = CALIB_LAST_IMAGE_FILE(_activeCarrierID);
                if (dstFileName != fileName)
                    SaveImage(dstFileName);

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
            
            updateAllCalibKeyPointBoxes();
            updateGuiStatus();
        }
        void SaveImage(string fileName)
        {
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            //_wndOwner?.Refresh();
            var img = _imgViewer?.Image;
            img?.SaveImage(fileName);
            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void GrabImage()
        {
            enableGoldenPicking(false);
            updateGuiStatus();

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);
            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                // 2025-09-14 _imgViewer 會複製 bigBmp
                var bigBmp = freeBmp.ToBitmap();
                var srcName = "[校正] 線掃相機 擷圖";
                _imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);

                // 2025-09-17 (自動存檔)
                SaveImage(CALIB_LAST_IMAGE_FILE(_activeCarrierID));
            }
            GaUtil.SetCursor(_wndOwner, oldCursor);
        }

        void LoadSettings()
        {
            // (1) Load VISION recipe Files
            int i = 0;
            foreach (var jx in _jxCalibVisionRecipes)
            {
                jx.Load(CALIB_VISION_FILE((CarrierEnum)i));
                jx.VisionSettings.FindAllFailBlocs.Value = false;           // 強制停用 "尋找所有格外區塊"
                jx.VisionSettings.Match.BoundBox.Value = Rectangle.Empty;   // 強制停用 "邊界框"
                connectPropEventHandlers(jx);
                i++;
            }

            // (2) Load TRANSFORMS (永遠載入全域設定)
            _transforms.Load(CALIB_TRANSFORMS_FILE);

            // (3) 第一次建置座標轉換
            _transforms.BuildAll();
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
            foreach (var jx in _jxCalibVisionRecipes)
            {
                if (force || jx.Modified)
                    jx?.Save(CALIB_VISION_FILE((CarrierEnum)i));
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
            }
            else
            {
                // 還原舊值
                bool isModified = _isCoordModified;
                foreach (var jx in _jxCalibVisionRecipes)
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
}
