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

using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Model.Transforms;
using LaserAlignDX.Mvc.Gui;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using CalibVisionParams = LaserAlignDX.BasicSpace.ParaSpace.MvdFindCircleClass;
using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;


namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaCalibCtrl
    {
        static int N_CALIB_POINTS => QTransform.N_POINTS;
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
        static string GA_CALIB_VISION_INI_FILE => "CalibrateForm_default_info.ini";
        #endregion

        #region GLOBAL_MESS
        IxLineScanCam IScanCam
        {
            get { return Traveller106.Universal.IxLineScan; }
        }
        CalibVisionParams _calibVisionParams => CalibVisionParams.Instance;
        #endregion

        #region KERNEL_MEMBERS
        TravellerTransforms _transforms => GaMvcConfig.Transforms;
        #endregion

        #region GUI_MEMBERS
        IvCalibToolUI _calibToolUI;
        Control _wndOwner => _calibToolUI.Window;
        JezTransImageViewPanel _imgViewer => _calibToolUI.ImgViewer;
        GvCalibPointsDataGridView _dgvCalibPointsListView => _calibToolUI.dgvCalibPointsListView;
        RadioButton[] _rdoCarriers => _calibToolUI.rdoCarriers;
        RadioButton[] _rdoSuckerRows => _calibToolUI.rdoSuckerRows;
        Button _btnGrabImage => _calibToolUI.btnGrabImage;
        Button _btnLoadImage => _calibToolUI.btnLoadImage;
        Button _btnOK => _calibToolUI.btnOK;
        Button _btnCancel => _calibToolUI.btnCancel;
        #endregion

        #region INTERACTORS
        CviCalibResultBox _cviResultBox = new CviCalibResultBox() { Visible = false };
        CviCalibPointBox[] _cviCalibPointBoxes = new CviCalibPointBox[N_CALIB_POINTS];
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        bool _modified = false;
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            initImgViewer();
            loadSettings();
            updateAllData(false);
            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initImgViewer()
        {
            var matViewer = _imgViewer.MatViewer;

            matViewer.AddInteractor(_cviResultBox);

            for (int i = 0; i < N_CALIB_POINTS; i++)
            {
                _cviCalibPointBoxes[i] = new CviCalibPointBox(new Rectangle(0, 0, 500, 500), Color.Lime, 0.25f)
                {
                    Visible = false,
                    CrossLength = 100,
                };
                matViewer.AddInteractor(_cviCalibPointBoxes[i]);
            }
        }
        void connectEventHandlers()
        {
            _btnOK.Click += (s, e) => closeTool(true);
            _btnCancel.Click += (s, e) => closeTool(false);
            _rdoCarriers[0].CheckedChanged += _rdoSelect_CheckedChanged;
            _rdoSuckerRows[0].CheckedChanged += _rdoSelect_CheckedChanged;
            _btnGrabImage.Click += (s, e) => grabImage();
            _btnLoadImage.Click += (s, e) => loadImage();
            _imgViewer.MatViewer.MouseMove += MatViewer_MouseMove;
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
            if (toModel)
            {
            }
            else
            {
                _calibToolUI.pgridVisionParams.SelectedObject = _calibVisionParams;
            }
        }
        void updateCalibKeyPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID, bool toModel)
        {
            var transform = _transforms.GetCameraMotorTransform(carrierID, suckerRowID);
            var corners = Enum.GetValues(typeof(CalibCornersEnum));
            var dgv = _dgvCalibPointsListView.DataGridView;

            if (toModel)
            {
                if (transform == null)
                    return;

                foreach (CalibCornersEnum corner in corners)
                {
                    int rowId = (int)corner;
                    var camPt = transform.GetSrcRef(rowId);
                    var motorPt = transform.GetDstRef(rowId);
                    bool isChanged = updateCalibKeyPoints(dgv, rowId, camPt, motorPt, toModel);
                    if (isChanged)
                    {
                        transform.SetSrcRef(rowId, camPt);
                        transform.SetDstRef(rowId, motorPt);
                        _modified = true;
                    }
                }
            }
            else
            {
                dgv.Rows.Clear();
                foreach (CalibCornersEnum corner in corners)
                {
                    int rowId = (int)corner;
                    var camPt = transform?.GetSrcRef(rowId);
                    var motorPt = transform?.GetDstRef(rowId);

                    var name = GaUtil.GetEnumDescription(corner);
                    dgv.Rows.Add(name, 0, 0, 0.0, 0.0);
                    dgv.Rows[rowId].Cells[0].Value = GaUtil.GetEnumDescription(corner);
                    updateCalibKeyPoints(dgv, rowId, camPt, motorPt, toModel);
                    updateCalibKeyPointBox(corner, camPt);
                }
            }
        }
        bool updateCalibKeyPoints(DataGridView dgv, int rowId, QCoord camPt, QCoord motorPt, bool toModel)
        {
            int col = 1;
            var dgvRow = dgv.Rows[rowId];

            if (toModel)
            {
                bool isChanged = false;

                var cx = (int)dgvRow.Cells[col++].Value;
                var cy = (int)dgvRow.Cells[col++].Value;
                var mx = (double)dgvRow.Cells[col++].Value;
                var my = (double)dgvRow.Cells[col++].Value;

                if (camPt.X != cx || camPt.Y != cy)
                {
                    camPt.X = cx;
                    camPt.Y = cy;
                    isChanged = true;
                }

                if (motorPt.X != mx || motorPt.Y != my)
                {
                    motorPt.X = mx;
                    motorPt.Y = my;
                    isChanged = true;
                }

                return isChanged;
            }
            else
            {
                dgvRow.Cells[col++].Value = camPt != null ? (int)camPt.X : 0;
                dgvRow.Cells[col++].Value = camPt != null ? (int)camPt.Y : 0;
                dgvRow.Cells[col++].Value = motorPt != null ? motorPt.X : 0.0;
                dgvRow.Cells[col++].Value = motorPt != null ? motorPt.Y : 0.0;
                return true;
            }
        }
        void updateCalibKeyPointBox(CalibCornersEnum corner, QCoord camPt, bool show = true)
        {
            var box = _cviCalibPointBoxes[(int)corner];
            if (camPt == null)
            {
                box.Visible = false;
                return;
            }

            var loc = box.Box2D;
            loc.SetCenter((float)camPt.X, (float)camPt.Y);
            box.SetBox(loc);
            box.Visible = show;
        }
        void updateAllCalibKeyPointBoxes()
        {
            var carrierID = _activeCarrierID;
            var suckerRowID = _activeSuckerRowID;

            var transform = _transforms.GetCameraMotorTransform(carrierID, suckerRowID);
            var corners = Enum.GetValues(typeof(CalibCornersEnum));

            foreach (CalibCornersEnum corner in corners)
            {
                int rowId = (int)corner;
                var camPt = transform?.GetSrcRef(rowId);
                updateCalibKeyPointBox(corner, camPt);
            }
        }
        #endregion

        void loadImage()
        {
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
        }
        void grabImage()
        {
            //throw new NotImplementedException();
        }
        void closeTool(bool save)
        {
            if (save && _modified)
            {
                updateAllData(true);
                saveSettings();
            }

            var frm = _wndOwner.FindForm();
            frm?.Close();
            frm?.Dispose();
        }
        void loadSettings()
        {
            _transforms.LoadGaaraIniFile();
            _calibVisionParams.Initial(GA_WORK_PATH, 0, GA_CALIB_VISION_INI_FILE);
        }
        void saveSettings()
        {

        }
    }
}
