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
using JetEazy.Interface;
using JetEazy.Match;
using JetEazy.OpenCV;
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
        static string GA_WORK_PATH => Traveller106.Universal.WORKPATH;
        static string CALIB_VISION_JSON_FILE => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Vision_Settings.json");
        static string CALIB_TRANSFORMS_INI_FILE => System.IO.Path.Combine(GA_WORK_PATH, "Calibration", "Jx_Calib_Transforms.ini");
        #endregion

        #region GLOBAL_MESS
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        #endregion

        #region KERNEL_MEMBERS
        TravellerTransforms _transforms => GaMvcConfig.TransformsModel;
        JxAoiRecipe _jxCalibAoiSettings = new JxAoiRecipe();
        CalibAoiModel _aoiModel = new CalibAoiModel();
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
        Button _btnAutoFindCalibPoints => _calibToolUI.btnAutoFindCalibPoints;
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
        bool _isCoordModified = false;
        bool _isGoldenPicking = false;
        bool _isRunning = false;
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            initImgViewer();
            LoadSettings();
            updateAllData(false);
            connectEventHandlers();
        }
        public void CleanUp()
        {
            _aoiModel?.Dispose();
            _aoiModel = null;
            _jxCalibAoiSettings.Dispose();
            _jxCalibAoiSettings = null;
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
            _btnOK.Click += (s, e) => CloseWindow(true);
            _btnCancel.Click += (s, e) => CloseWindow(false);

            _rdoCarriers[0].CheckedChanged += _rdoSelect_CheckedChanged;
            _rdoSuckerRows[0].CheckedChanged += _rdoSelect_CheckedChanged;

            _btnGrabImage.Click += (s, e) => GrabImage();
            _btnLoadImage.Click += (s, e) => LoadImage();

            _btnPickupGolden.Click += (s, e) => toggleGoldenPicking();
            _btnAutoFindCalibPoints.Click += (s, e) => RunAutoFetchCalibPoints();
            _btnBuildCalib.Click += (s, e) => BuildAllTransforms();
            _cviGoldenBox.OnBoxSelected += (s, e) => BuildGolden();

            _imgViewer.MatViewer.MouseMove += MatViewer_MouseMove;
            _wndOwner.HandleCreated += (s, e) => updateGuiStatus();
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();           // 自動釋放資源
        }
        #endregion

        #region EVENT_HANDLERS
        private void _rdoSelect_CheckedChanged(object sender, System.EventArgs e)
        {
            _activeCarrierID = _rdoCarriers[0].Checked ? CarrierEnum.C1 : CarrierEnum.C2;
            _activeSuckerRowID = _rdoSuckerRows[0].Checked ? SuckerRowEnum.S1 : SuckerRowEnum.S2;
            
            if (Array.IndexOf(_rdoCarriers, sender) >= 0)
                updateVisionParams(_activeCarrierID, false);

            updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);            
            clearCviResults();

            _cviResultBox.ActiveCarrierID = _activeCarrierID;
            _cviResultBox.ActiveSuckerRowID = _activeSuckerRowID;
        }
        private void MatViewer_MouseMove(object sender, MouseEventArgs e)
        {
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
        void updateVisionParams(CarrierEnum carrierID, bool toModel)
        {
            GwPanePropsViewer panel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (panel == null)
                return;

            if (toModel)
            {
            }
            else
            {
                panel.BuildGuiCtrls(_jxCalibAoiSettings);
                panel.ExpandAll();
            }
        }
        void updateCalibKeyPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID, bool toModel)
        {
            var corners = Enum.GetValues(typeof(CalibCornersEnum));
            var dgv = _dgvCalibPointsListView.DataGridView;

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
            var traySettings = _jxCalibAoiSettings.TrayMiscSettings;
            var pitchX = (double)traySettings.PitchX.Value;
            var pitchY = (double)traySettings.PitchY.Value;
            var rows = (int)traySettings.FullRows.Value;
            var cols = (int)traySettings.FullCols.Value;
            bool areRowsColsMatched = (rows == camGrid.Rows && cols == camGrid.Cols);

            if (!areRowsColsMatched)
            {
                string msg = $"像測的 Rows={camGrid.Rows} Cols={camGrid.Cols} 與\n\r"
                           + $"參數的 Rows={rows} Cols={cols} 不一致 !";
                MessageBox.Show(msg, _wndOwner.FindForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //(2) 將 校正點位群 更新到 座標轉換 系統 (Model)
            if (areRowsColsMatched)
            {
                _transforms.ConfigPlcGrid(rows, cols, pitchX, pitchY);
                _transforms.UpdateCalibPoints(_activeCarrierID, _activeSuckerRowID, camGrid);
            }

            //(3) 更新 GUI
            updateCalibKeyPoints(_activeCarrierID, _activeSuckerRowID, false);
            //updateAllCalibKeyPointBoxes();

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
        void updateGuiStatus()
        {
            bool isEmpty = _imgViewer.MatViewer.Image == null;
            _btnGrabImage.Enabled = !_isRunning;
            _btnLoadImage.Enabled = !_isRunning;

            _btnPickupGolden.BackColor = _isGoldenPicking ? Color.HotPink : _btnAutoFindCalibPoints.BackColor;
            _btnPickupGolden.Enabled = !_isRunning && !isEmpty;
            _btnAutoFindCalibPoints.Enabled = !_isRunning && !isEmpty;
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
        }
        void clearCviResults()
        {
            _cviResultBox.Reset();
            _imgViewer.MatViewer.Invalidate();
        }
        #endregion

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

            //(3) Update to Recipe
            var matchSetting = _jxCalibAoiSettings.VisionSettings.Match;
            matchSetting.GoldenBmp.Value = goldenBmp;
            matchSetting.GoldenBox.Value = goldenRect;
            updateVisionParams(_activeCarrierID, false);

            //(4) CleanUp
            ezImage?.Dispose();
        }
        bool BuildGoldenGrid()
        {
            var fullfovImg = _imgViewer.MatViewer.Image;
            if (fullfovImg == null)
                return false;

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
        void RunAutoFetchCalibPoints()
        {
            enableGoldenPicking(false);
            
            bool ok = BuildGoldenGrid();
            if (!ok)
                return;

            var fullfovImg = _imgViewer.MatViewer.Image;
            if (fullfovImg == null)
                return;

            //(1) Peek the ezImage
            var ezImage = new EzQuickImage(fullfovImg, deepCopy: false);

            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            //(2) Run Aoi
            _aoiModel.RunMatch(SideID.A, ezImage);

            //(3) Update Result
            var matchResult = _aoiModel.GetMatchResult(SideID.A);

            //(4) Refine each detail locations
            _aoiModel.RefineCentroidLocations(matchResult, ezImage);

            //(5) Update Grid 4 Corners To Model
            updateCalibKeyPoints(matchResult?.Grid);

            //(6) Update Calib Grid Result
            _cviResultBox.TransCameraToMotor = null;
            _cviResultBox.TransCameraToWorld = null;
            _cviResultBox.UpdateResult(matchResult);
            _cviResultBox.Visible = true;
            _imgViewer.MatViewer.Invalidate();

            //(6) CleanUp
            ezImage?.Dispose();

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }
        void BuildAllTransforms()
        {
            var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

            enableGoldenPicking(false);
            updateAllData(true);

            _transforms.BuildAll();

            _cviResultBox.TransCameraToMotor = _transforms.GetCameraMotorTransform(_activeCarrierID, _activeSuckerRowID);
            _cviResultBox.TransCameraToWorld = _transforms.GetCameraPhysicTransform(_activeCarrierID);

            GaUtil.SetCursor(_wndOwner, oldCursor);
        }

        void LoadImage()
        {
            enableGoldenPicking(false);

            string fileName = GaUtil.BrowseImageFile();
            if (fileName != null)
            {
                var oldCursor = GaUtil.SetCursor(_wndOwner, Cursors.WaitCursor);

                // MatViewer 會接手管理 Image 生命
                _imgViewer.MatViewer.LoadImage(fileName);
                _imgViewer.lblTitle.Text = "[校正] " + System.IO.Path.GetFileName(fileName);

                GaUtil.SetCursor(_wndOwner, oldCursor);
            }
            
            updateAllCalibKeyPointBoxes();
            updateGuiStatus();
        }
        void GrabImage()
        {
            enableGoldenPicking(false);
            updateGuiStatus();
            var freeBmp = IScanCam.GetFreeImageBitmap();
            if (freeBmp != null)
            {
                using (var bmp = freeBmp.ToBitmap())
                using (var bridge = new QxImageBridge(bmp))
                {
                    var old = _imgViewer.MatViewer.Image;
                    _imgViewer.MatViewer.Image = bridge.Image.Clone();
                }
            }
        }

        void LoadSettings()
        {
            // VISIONS
            _jxCalibAoiSettings.Load(CALIB_VISION_JSON_FILE);
            _aoiModel.SetRecipe(_jxCalibAoiSettings);

            // TRANSFORMS
            var newFile = CALIB_TRANSFORMS_INI_FILE;
            if (!System.IO.File.Exists(newFile))
                _transforms.LoadGaaraIniFile();
            else
                _transforms.Load(newFile);

            // 第一次建置座標轉換
            _transforms.BuildAll();
        }
        void SaveSettings(bool force = false)
        {
            // TRANSFORMS
            if (force || _isCoordModified)
            {
                _transforms.Save(CALIB_TRANSFORMS_INI_FILE);
                _transforms.SaveGaaraIniFile();
            }

            // VISIONS
            if (force || _jxCalibAoiSettings.Modified)
            {
                _jxCalibAoiSettings.Save(CALIB_VISION_JSON_FILE);
            }
        }
        void CloseWindow(bool confirm)
        {
            if (confirm)
            {
                updateAllData(true);
                SaveSettings();
            }
            else
            {
                // 還原舊值
                if (_isCoordModified || _jxCalibAoiSettings.Modified)
                    LoadSettings();
            }

            var frm = _wndOwner.FindForm();
            frm?.Close();
            //frm?.Dispose();
        }
    }
}
