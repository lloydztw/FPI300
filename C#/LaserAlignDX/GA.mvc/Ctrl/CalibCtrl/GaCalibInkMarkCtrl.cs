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

using EzAoiEmptyTrayInspector.Model.Aoi;
using JetEazy.FormSpace;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Properties;
using LeTian.JxProps.Gui;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Windows.Forms;

using CviBoundBox = EzAoiEmptyTrayInspector.Ctrl.CviRcpBox;
using CviCalibPointBox = LaserAlignDX.Mvc.Gui.CviRotRectBox;


namespace LaserAlignDX.Mvc.Ctrl
{
    public class GaCalibInkMarkCtrl
    {
        static int N_CALIB_POINTS => TravellerTransforms.N_CALIB_POINTS;
        public event EventHandler OnRequestToLoadInkImage;

        #region GLOBAL_PATH
        static string CALIB_INK_MARK_FILE(CarrierEnum C, SuckerRowEnum S = SuckerRowEnum.S1, int viewID = 0)
        {
            string fileName = GaMvcPaths.CALIB_VISION_FILE(C, S, viewID);
            return fileName.Replace("_Vision_", "_InkMark_");
        }
        #endregion

        #region GLOBAL_MESS
        //IxLineScanCam IScanCam
        //{
        //    get { return Traveller106.Universal.IxLineScan; }
        //}
        //TravellerTransforms _transforms => GaMvcConfig.SysModel.TransformsModel;
        #endregion

        #region SETTINGS
        JxCalibInkMarkSettings[] _jxCalibInkMarkRecipes = new JxCalibInkMarkSettings[4];
        #endregion

        #region GUI_LINKS
        IvCalibToolUI _calibToolUI;
        Control _wndOwner => _calibToolUI.Window;
        #endregion

        #region INTERACTORS
        CviBoundBox _cviBigBoundBox = new CviBoundBox(Brushes.Blue, 1, 3);
        CviCalibPointBox[] _cviInkPointBoxes = new CviCalibPointBox[N_CALIB_POINTS];
        #endregion

        #region RUNTIME_DATA
        CarrierEnum _activeCarrierID = CarrierEnum.C1;
        SuckerRowEnum _activeSuckerRowID = SuckerRowEnum.S1;
        int _activeViewID = 0;
        #endregion

        #region RUNTIME_IMAGE
        bool _isInkPtsFetched = false;
        #endregion

        public void Attach(IvCalibToolUI toolView)
        {
            _calibToolUI = toolView;
            initRecipeBody();
            initImgViewer();
            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initRecipeBody()
        {
            for (int i = 0, N = _jxCalibInkMarkRecipes.Length; i < N; i++)
            {
                var old = _jxCalibInkMarkRecipes[i];
                disconnectPropEventHandlers(old);
                _jxCalibInkMarkRecipes[i] = new JxCalibInkMarkSettings();
                connectPropEventHandlers(_jxCalibInkMarkRecipes[i]);
                old?.Dispose();
            }
        }
        void CleanUp()
        {
            for (int i = 0, N = _jxCalibInkMarkRecipes.Length; i < N; i++)
            {
                var old = _jxCalibInkMarkRecipes[i];
                disconnectPropEventHandlers(old);
                _jxCalibInkMarkRecipes[i] = null;
                old?.Dispose();
            }
        }
        void initImgViewer()
        {
            int viewID = 1;
            var imgViewer = _calibToolUI.ImgViewers[viewID];
            var matViewer = imgViewer.MatViewer;

            _cviBigBoundBox.Visible = false;
            matViewer.AddInteractor(_cviBigBoundBox);

            for (int i = 0; i < N_CALIB_POINTS; i++)
            {
                _cviInkPointBoxes[i] = new CviCalibPointBox(new Rectangle(0, 0, 500, 500), Color.Lime, 0.25f)
                {
                    Visible = false,
                    CrossLength = 100,
                };

                matViewer.AddInteractor(_cviInkPointBoxes[i]);
            }
        }
        void connectEventHandlers()
        {
            _cviBigBoundBox.OnChanged += _cviBigBoundBox_OnChanged;

            var dgv = _calibToolUI.dgvCalibPointsListView.DataGridView;
            dgv.CellEndEdit += Dgv_CellEndEdit;

            // 自動釋放資源
            _wndOwner.HandleDestroyed += (s, e) => CleanUp();

            // 延遲更新
            _wndOwner.HandleCreated += (s, e) =>
            {
                _wndOwner.BeginInvoke(new Action(() =>
                {
                    //updateAllData(false);
                    updateGuiStatus();
                }));
            };
        }



        void connectPropEventHandlers(JxCalibInkMarkSettings rcp)
        {
            if (rcp == null)
                return;
            rcp.OnModified += Rcp_OnModified;
        }
        void disconnectPropEventHandlers(JxCalibInkMarkSettings rcp)
        {
            if (rcp == null)
                return;
            rcp.OnModified -= Rcp_OnModified;
        }
        #endregion

        #region EVENT_HANDLERS
        private void Rcp_OnModified(object sender, EventArgs e)
        {
        }
        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 3)
            {
                var dgv = _calibToolUI.dgvCalibPointsListView.DataGridView;

                // 取得被編輯單元格的物件
                DataGridViewCell cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];

                // 取得新的內容 (在 CellEndEdit 觸發時，cell.Value 已經是新值)
                object value = cell.Value;

                // 檢查 value 是否為 null，然後轉換為字串
                string newValue = value != null ? value.ToString() : string.Empty;

                //>>> System.Diagnostics.Debug.WriteLine($"使用者完成了單元格 ({e.RowIndex}, {e.ColumnIndex}) 的編輯，新值是: {newValue}");

                if (!float.TryParse(newValue, out var v))
                    return;

                var jxSettings = _getActiveSettings();
                var jxRawMotorPt = jxSettings?.GetRawMotorPoint(e.RowIndex);
                if (jxRawMotorPt == null)
                    return;

                var pt = jxRawMotorPt.Value;
                if (e.ColumnIndex == 3)
                    pt.X = v;
                else
                    pt.Y = v;

                jxRawMotorPt.Value = pt;

                _calibToolUI?.wndVisionSettingsPanel?.Invalidate();
            }
        }
        private void _cviBigBoundBox_OnChanged(object sender, EventArgs e)
        {
            updateBoundBox(true);
        }
        #endregion

        #region PRIVATE_UPDATE_FUNCTIONS
        JxCalibInkMarkSettings _getActiveSettings()
        {
            int index = ((int)(_activeCarrierID - CarrierEnum.C1)) * 2 + (int)(_activeSuckerRowID - SuckerRowEnum.S1);
            var jx = _jxCalibInkMarkRecipes[index];
            return jx;
        }
        void updateInkMarkerParams(bool toModel)
        {
            GwPanePropsViewer panel = _calibToolUI.wndVisionSettingsPanel as GwPanePropsViewer;
            if (panel == null)
                return;

            if (toModel)
            {
            }
            else
            {
                if (_activeViewID == 1)
                {
                    var jx = _getActiveSettings();
                    panel.BuildGuiCtrls(jx);
                    panel.ExpandAll();
                    updateBoundBox(false);
                }
            }
        }
        void updateBoundBox(bool toModel)
        {
            var jxSettings = _getActiveSettings();
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
        void updateGuiStatus()
        {
            var btnBuildCalibInkAdj = _calibToolUI.btnBuildCalibInkAdj;
            if (btnBuildCalibInkAdj != null)
                btnBuildCalibInkAdj.Enabled = _isInkPtsFetched && _activeViewID == 1;
        }
        #endregion

        bool IsImageEmpty()
        {
            try
            {
                return _calibToolUI.ImgViewers[1].Image == null;
            }
            catch
            {
                return false;
            }
        }
        internal void SetActiveView(CarrierEnum C, SuckerRowEnum S, int viewID)
        {
            bool isCarrierSuckerChanged = _activeCarrierID != C || _activeSuckerRowID != S;
            bool isViewChanged = _activeViewID != viewID;
            bool isAnyChanged = isCarrierSuckerChanged || isViewChanged;
            
            _activeCarrierID = C;
            _activeSuckerRowID = S;
            _activeViewID = viewID;

            if (isAnyChanged)
            {
                foreach (var box in _cviInkPointBoxes)
                    box.Visible = false;

                //if (isCarrierSuckerChanged)
                //{
                //    OnRequestToLoadInkImage?.Invoke(this, null);
                //}
                //else if (_activeViewID == 1 && IsImageEmpty())
                //{
                //    OnRequestToLoadInkImage?.Invoke(this, null);
                //}

                OnRequestToLoadInkImage?.Invoke(this, null);
                updateInkMarkerParams(false);
                updateGuiStatus();

                // refresh imgViewer
                if (_activeViewID == 1)
                    _calibToolUI?.ImgViewers[1].MatViewer?.Invalidate();
            }
        }
        internal void TakeOverImage(Bitmap bigBmp, string srcName, int viewID = -1)
        {
            if (viewID < 0)
                viewID = _activeViewID;

            var imgViewer = _calibToolUI.ImgViewers[viewID];

            if (bigBmp != null)
            {
                imgViewer.UpdateImage(bigBmp, srcName, disposeSrc: true);
            }
            else
            {
                var old = imgViewer.MatViewer.Image;
                imgViewer.MatViewer.Image = null;
                old?.Dispose();
                imgViewer.UpdateImage(null, "", disposeSrc: false);
            }

            updateBoundBox(false);
            updateGuiStatus();
        }
        internal void LoadSettings()
        {
            int i = 0;
            foreach (CarrierEnum C in Enum.GetValues(typeof(CarrierEnum)))
            {
                foreach (SuckerRowEnum S in Enum.GetValues(typeof(SuckerRowEnum)))
                {
                    var file = CALIB_INK_MARK_FILE(C, S, 1);
                    var jx = _jxCalibInkMarkRecipes[i];
                    jx.Load(file);
                    i++;
                }
            }
        }
        internal void SaveSettings(bool force = false)
        {
            int i = 0;
            foreach (CarrierEnum C in Enum.GetValues(typeof(CarrierEnum)))
            {
                foreach (SuckerRowEnum S in Enum.GetValues(typeof(SuckerRowEnum)))
                {
                    var jx = _jxCalibInkMarkRecipes[i];
                    if (force || jx.Modified)
                    {
                        var file = CALIB_INK_MARK_FILE(C, S, 1);
                        jx.Save(file);
                    }
                    i++;
                }
            }
        }
        internal bool AutoFetchInkPoints()
        {
            if (_activeViewID != 1 || IsImageEmpty())
                return false;

            var imgViewer = _calibToolUI.ImgViewers[_activeViewID];
            var imgSrc = imgViewer.Image;
            if (imgSrc == null)
                return false;

            _isInkPtsFetched = false;

            fetchInkPoints(imgSrc, out var blocs);

            if (blocs != null && blocs.Count >= 4)
            {
                updateInkPoints(blocs);
                _isInkPtsFetched = true;
                updateGuiStatus();
            }

            return _isInkPtsFetched;
        }
        internal bool BuildCalibInkAdj()
        {
            if (!_isInkPtsFetched)
                return false;
            bool ok = adjustCalibMotorPoints(_activeCarrierID, _activeSuckerRowID);
            return ok;
        }

        #region PRIVATE_FUNCTIONS
        void fetchInkPoints(Mat imgSrc, out List<EzBloc> inkBlocs)
        {
            inkBlocs = null;

            if (_activeViewID != 1 || imgSrc == null)
                return;

            var jxSettings = _getActiveSettings();
            var blockMinSize = jxSettings.Vision.BlockMinSize.Value;

            int NP = 4;
            var roi = JetEazy.Qcvt.CV(_cviBigBoundBox.Box);
            GaUtil.Clip(ref roi, imgSrc.Width, imgSrc.Height);

            //(1) Find Calib Blocks 
            var calibBlocks = new List<EzBloc>();
            using (var imgCrop = imgSrc[roi].Clone())
            {
                var thres = jxSettings.Vision.BlockThreshold.Value;
                if (thres <= 0)
                {
                    Cv2.Threshold(imgCrop, imgCrop, thres, 255, ThresholdTypes.Otsu);
                }
                else
                {
                    Cv2.Threshold(imgCrop, imgCrop, thres, 255, ThresholdTypes.Binary);
                }

                EzBlobFinder finder = new EzBlobFinder
                {
                    MorphIterations = 0,
                    OptFillBorder = false,
                    MinSize = new OpenCvSharp.Size(blockMinSize, blockMinSize)
                };

                finder.FindWhiteBlobs(imgCrop, out calibBlocks);
                if (calibBlocks == null)
                    return;

                if (calibBlocks.Count > 1)
                {
                    calibBlocks.Sort((a, b) =>
                    {
                        var px1 = a != null ? a.Pixels : 0;
                        var px2 = b != null ? b.Pixels : 0;
                        return px2 - px1;
                    });
                }

                if (calibBlocks.Count > NP)
                    calibBlocks.RemoveRange(NP, calibBlocks.Count - NP);

                foreach (var b in calibBlocks)
                    b?.Offset(roi.X, roi.Y);
            }

            //(2) Find the ink marks
            if (calibBlocks.Count > 0)
            {
                inkBlocs = new List<EzBloc>(calibBlocks);

                int index = 0;
                var inkMinSize = Math.Max(2, blockMinSize / 20);
                foreach (var calibBloc in calibBlocks)
                {
                    roi = JetEazy.Qcvt.CV(calibBloc.Rect);
                    roi.Inflate(-5, -5);
                    GaUtil.Clip(ref roi, imgSrc.Width, imgSrc.Height);

                    using (var imgCrop = imgSrc[roi].Clone())
                    {
                        Cv2.Threshold(imgCrop, imgCrop, 0, 255, ThresholdTypes.Otsu);
                        Cv2.BitwiseNot(imgCrop, imgCrop);

                        EzBlobFinder finder = new EzBlobFinder
                        {
                            MorphIterations = 0,
                            OptFillBorder = false,
                            MinSize = new OpenCvSharp.Size(inkMinSize, inkMinSize)
                        };
                        finder.FindWhiteBlobs(imgCrop, out var whiteBlobs);

                        if (whiteBlobs != null && whiteBlobs.Count > 0)
                        {
                            if (whiteBlobs.Count > 1)
                            {
                                whiteBlobs.Sort((a, b) =>
                                {
                                    var px1 = a != null ? a.Pixels : 0;
                                    var px2 = b != null ? b.Pixels : 0;
                                    return px2 - px1;
                                });
                            }

                            foreach (var b in whiteBlobs)
                                b?.Offset(roi.X, roi.Y);

                            inkBlocs[index] = whiteBlobs[0];
                        }
                    }
                    index++;
                }
            }
        }
        void updateInkPoints(IEnumerable<EzBloc> blocs)
        {
            var dict = new Dictionary<QVector, EzBloc>();
            foreach (var b in blocs)
            {
                if (b != null)
                    dict.Add(b.Center, b);
            }

            QVector[] corners = dict.Keys.ToArray();
            if (true)
            {
                var sorter = new QvQuad2D();
                sorter.Corners = corners;
                sorter.SortByCornerTheta();
                corners = sorter.Corners;
            }

            var jxSettings = _getActiveSettings();

            int i = 0;
            foreach (var inkPoint in corners)
            {
                roundHalfPixel(inkPoint);

                dict.TryGetValue(inkPoint, out var inkBloc);
                if (inkBloc != null)
                {
                    var rect = inkBloc.Rect;
                    _cviInkPointBoxes[i].SetBox(rect);
                }
                _cviInkPointBoxes[i].Quad2D?.SetCenter(inkPoint);
                _cviInkPointBoxes[i].Visible = true;

                jxSettings.GetInkPoint(i).Value = new PointF((float)inkPoint.X, (float)inkPoint.Y);

                i++;
            }

            _calibToolUI.ImgViewers[1]?.MatViewer?.Invalidate();
            _calibToolUI.wndVisionSettingsPanel?.Refresh();
        }
        bool adjustCalibMotorPoints(CarrierEnum carrierID, SuckerRowEnum suckerRowID)
        {
            bool isChanged = false;

            if (_activeViewID != 1)
                return isChanged;

            var jxSettings = _getActiveSettings();
            if (jxSettings == null)
                return isChanged;

            try
            {
                //(1) 取得 馬達轉換 transform (from Camera To Motor)
                var transformModel = GaMvcConfig.SysModel.TransformsModel;
                var transform = transformModel.GetCameraMotorTransform(carrierID, suckerRowID);
                if (transform == null)
                    return isChanged;

                //(2) 目前的 校正點位
                var trfCalibCorners = transform.GetCalibCornerPoints();
                var camPts = trfCalibCorners?.GetAll(isSrc: true);
                var motorPts = trfCalibCorners?.GetAll(isSrc: false);
                var rawMotorPts = getRawMotorPts(motorPts);

                //(3) 墨點
                var inkPoints = new QVector[N_CALIB_POINTS];
                for (int i = 0; i < N_CALIB_POINTS; i++)
                {
                    var pt = jxSettings.GetInkPoint(i).Value;
                    inkPoints[i] = new QVector(pt.X, pt.Y);
                }

                //(4) 建置 inkPoints 到 motorPts 的轉換矩陣
                var srcPts = Array.ConvertAll(inkPoints, p => new Point2f((float)p.X, (float)p.Y));
                var dstPts = Array.ConvertAll(rawMotorPts, p => new Point2f((float)p.X, (float)p.Y));
                using (var matTrf = Cv2.GetPerspectiveTransform(srcPts, dstPts))
                {
                    //(5) 由現有的 camPts 點位, 求出更精確的 motorPts
                    srcPts = Array.ConvertAll(camPts, p => new Point2f((float)p.X, (float)p.Y));
                    dstPts = Cv2.PerspectiveTransform(srcPts, matTrf);
                    var motorAdjPts = Array.ConvertAll(dstPts, p => new QVector(p.X, p.Y));

                    //(6) 用 motorAdjPts 取代 motorPts
                    for (int index = 0, N = motorAdjPts.Length; index < N; index++)
                    {
                        var camPt = camPts[index];
                        var motorPt = motorAdjPts[index];
                        trfCalibCorners.Set(index, camPt, motorPt);
                    }

                    isChanged = true;
                }

                return isChanged;
            }
            catch (Exception ex)
            {
                VsMessageBox.Warning(ex.Message);
                return false;
            }
        }
        QVector[] getRawMotorPts(QVector[] defaultPts)
        {
            if (defaultPts == null)
                return defaultPts;

            var jxSettings = _getActiveSettings();
            if (jxSettings == null)
                return defaultPts;

            bool isJxChanged = false;

            int NP = defaultPts.Length;
            var results = Array.ConvertAll(defaultPts, p => p != null ? new QVector(p) : new QVector(0, 0));

            for (int i = 0; i < NP; i++)
            {
                var jxPoint = jxSettings.GetRawMotorPoint(i);
                if (jxPoint != null)
                {
                    var rawPt = jxPoint.Value;

                    if (!isCloseToEmpty(rawPt))
                    {
                        // 更新 results[i]
                        results[i] = new QVector(rawPt.X, rawPt.Y);
                    }
                    else
                    {
                        // 將 defaultPts[i] 寫入 jxSettings
                        jxPoint.Value = new PointF((float)defaultPts[i].X, (float)defaultPts[i].Y);
                        isJxChanged = true;
                    }
                }
            }

            if (isJxChanged)
            {
                _calibToolUI?.wndVisionSettingsPanel?.Refresh();
            }

            return results;
        }
        void roundHalfPixel(QVector pt)
        {
            if (pt != null)
            {
                pt.X = Math.Round(pt.X * 2.0) / 2.0;
                pt.Y = Math.Round(pt.Y * 2.0) / 2.0;
            }
        }
        bool isCloseToEmpty(PointF pt)
        {
            double tiny = 1e-5;
            if (Math.Abs(pt.X) < tiny && Math.Abs(pt.Y) < tiny)
                return true;
            return false;
        }
        #endregion
    }
}
