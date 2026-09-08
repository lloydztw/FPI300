#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-19 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Lang;
using JetEazy.OpenCV;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public class GaTemplBadConnsEditCtrl : GaRcpBaseCtrl
    {
        #region RECIPE_PARAMS
        InspectX3ParaClass _xInspectParams
        {
            get { return InspectX3ParaClass.Instance; }
        }
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        IvTemplBadConnsEditorUI _badConnsEditorUI => _editorUI;
        JezTransImageViewPanel wndRegionViewer => _editorUI.ImgViewers[0] as JezTransImageViewPanel;
        JezTransImageViewPanel wndFeatureViewer => _editorUI.ImgViewers[2] as JezTransImageViewPanel;
        Button btnAddRegion => _badConnsEditorUI.btnAddRegion;
        Button btnDeleteRegion => _badConnsEditorUI.btnDeleteRegion;
        Button btnClearAllRegions => _badConnsEditorUI.btnClearAllRegions;
        #endregion

        #region VIEWER_TITLES
        //string _Title1 => QMSG.T("Region View");
        //string _Title2 => QMSG.T("Template View");
        string _Title3 => QMSG.T("Feature View");
        #endregion

        #region INTERACTORS
        Brush _boxBrush => Brushes.OrangeRed;
        List<CviRcpBox> _cviDetectBoxes;
        CviRcpBox _cviActiveBox;
        #endregion

        #region RUNTIME_DATA
        bool _bypassWindowEvents = false;
        bool _isEditting = false;
        bool _isModified = false;
        #endregion

        public void Attach(IvTemplateEditorUI ui)
        {
            _editorUI = ui;
            initGui();
            connectEventHandlers();
        }
        void initGui()
        {
            initInteractors();
        }
        void connectEventHandlers()
        {
            btnAddRegion.Click += (s, e) => AddDetectRegion();
            btnDeleteRegion.Click += (s, e) => DeleteDetectRegion();
            btnClearAllRegions.Click += (s, e) => ClearAllDetectRegions();
            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;
        }

        #region EVENT_HANDLERS
        private void CviDefectionBox_OnChanged(object sender, EventArgs e)
        {
            _cviActiveBox = (sender as CviRcpBox);
            updateDetectionBoxes(true);
            updateFeatureView(true);
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (_bypassWindowEvents) 
                return;

            if (_isEditting)
            {
                string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;
                if (propertyName != null && propertyName.StartsWith("BadConn"))
                {
                    updateFeatureView(true);
                }
            }
        }
        #endregion

        #region DISP_UI_FUNCTIONS
        void initInteractors()
        {
            _cviDetectBoxes = new List<CviRcpBox>();
        }
        void refreshDispUI(Control dispUI)
        {
            if (dispUI is JezTransImageViewPanel panel)
                panel.MatViewer.Invalidate();
            else
                dispUI?.Invalidate();
        }
        #endregion

        internal bool IsEditting
        {
            get => _isEditting;
            set
            {
                if (_isEditting != value)
                {
                    _isEditting = value;
                    updateGuiStatus();
                    //updateDetectionBoxes(value);
                }
            }
        }
        internal bool IsModified
        {
            get => _isModified;
            set => _isModified = value;
        }
        internal void PostInit()
        {
            updateDetectionBoxes(false);
            updateGuiStatus();
        }
        internal void UpdateDetectionBoxes(bool editting)
        {
            _isEditting = editting;
            
            updateDetectionBoxes(false);

            if (_isEditting)
                updateFeatureView(false);
            
            updateGuiStatus();
        }

        void AddDetectRegion()
        {
            if (wndRegionViewer == null) return;

            // 暫停 dispUI 運作
            var dispUI = wndRegionViewer;
            var imgViewer = wndRegionViewer.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 新增 cviMaskBox
            var count = _cviDetectBoxes.Count;
            var rect = new Rectangle(50 + count * 5, 50 + count * 5, 100, 100);

            var cviBox = new CviRcpBox(_boxBrush, 1, 3) { Box = rect };
            cviBox.OnChanged += CviDefectionBox_OnChanged;

            _cviDetectBoxes.Add(cviBox);
            imgViewer.AddInteractor(cviBox);

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDetectionBoxes(true);

            // Update Feature Image
            updateFeatureView(true);
        }
        void DeleteDetectRegion()
        {
            if (_cviActiveBox == null)
                return;

            // 暫停 dispUI 運作
            var dispUI = wndRegionViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 移除 _cviActiveMaskBox
            imgViewer.RemoveInteractor(_cviActiveBox);
            _cviDetectBoxes.Remove(_cviActiveBox);
            _cviActiveBox = _cviDetectBoxes.Count > 0 ? _cviDetectBoxes[_cviDetectBoxes.Count - 1] : null;

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDetectionBoxes(true);

            // Update Feature Image
            updateFeatureView(true);
        }
        void ClearAllDetectRegions()
        {
            if (wndRegionViewer == null) return;

            // 暫停 dispUI 運作
            var dispUI = wndRegionViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 移除 所有 _cviDefectMaskBoxes
            foreach (var cviBox in _cviDetectBoxes)
                imgViewer.RemoveInteractor(cviBox);
            _cviDetectBoxes.Clear();
            _cviActiveBox = null;

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDetectionBoxes(true);

            // Update Feature Image
            updateFeatureView(true);
        }

        #region PRIVATE_GUI_FUNCTIONS
        IEnumerable<RectangleF> iterCviDetectionBoxes()
        {
            foreach (var cviBox in _cviDetectBoxes)
            {
                yield return cviBox.Box;
            }
        }
        void updateDetectionBoxes(bool toRecipe)
        {
            if (wndRegionViewer == null) 
                return;

            var dispUI = wndRegionViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            if (toRecipe)
            {
                _xInspectParams.BadConnsRects = new List<RectangleF>(iterCviDetectionBoxes());
                _isModified = true;
            }
            else
            {
                // 移除舊的
                #region REMOVE_OLD
                foreach (var oldBox in _cviDetectBoxes)
                {
                    if (oldBox != null)
                    {
                        oldBox.Enabled = false;
                        oldBox.Visible = false;
                        imgViewer.RemoveInteractor(oldBox);
                    }
                }
                _cviDetectBoxes.Clear();
                #endregion

                // 重新添加
                #region CREATE_ALL_NEW
                var rects = _xInspectParams.BadConnsRects;
                if (rects != null && rects.Count > 0)
                {
                    foreach (var rectF in rects)
                    {
                        var rect = Rectangle.Round(rectF);
                        if (rect == Rectangle.Empty)
                            continue;

                        var cviBox = new CviRcpBox(_boxBrush, 1, 3) { Box = rect };
                        cviBox.OnChanged += CviDefectionBox_OnChanged;

                        _cviDetectBoxes.Add(cviBox);
                        imgViewer.AddInteractor(cviBox);
                    }
                }
                #endregion
            }

            dispUI.Enabled = flag;
        }
        void updateFeatureView(bool toRecipe)
        {
            // 更新 Threshold
            updateThreshold(toRecipe);

            // 以 GoldenRegionCellBmp 為基礎, 生成 bmpFeature
            aoiCreateFeatureBmp(_xRecipe.GoldenRegionCellBmp, iterCviDetectionBoxes(), out Bitmap bmpDisp);

            // 更新 featureBmp 到 GUI
            if (_isEditting && bmpDisp != null)
                wndFeatureViewer?.UpdateImage(bmpDisp, _Title3, false);

            // CleanUp
            bmpDisp?.Dispose();
        }
        void updateThreshold(bool toRecipe)
        {
            //--------------------------
            // 已經由 PropertyGrid 處理
            //--------------------------

            //if (toRecipe)
            //{
            //    _xInspectParams.BadConnsThreshold = (int)numBadConnThreshold.Value;
            //    _isModified = true;
            //}
            //else
            //{
            //    _bypassWindowEvents = true;
            //    numBadConnThreshold.Value = _xInspectParams.BadConnsThreshold;
            //    _bypassWindowEvents = false;
            //}
        }
        void updateGuiStatus()
        {
            foreach(var box in _cviDetectBoxes)
            {
                box.Visible = _isEditting;
                box.Enabled = _isEditting;
            }
        }
        #endregion

        #region AOI_MODEL_FUNCTIONS
        //----------------------------------------------------------------------------
        // 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        //----------------------------------------------------------------------------
        void aoiCreateFeatureBmp(Bitmap bmpTemplate, IEnumerable<RectangleF> rects, out Bitmap bmpDisp)
        {
            try
            {
#if (false)
                var threshold = _xInspectParams.BadConnsThreshold;

                // 使用 OpenCvSharp, 製作 mask (8 bpp) 將 maskRects 指定的區域塗成白色
                using (var bridge = new QxImageBridge(bmpTemplate))
                using (Mat gray = new Mat(bridge.Image.Size(), MatType.CV_8UC1))
                using (Mat feature = new Mat(bridge.Image.Size(), MatType.CV_8UC1))
                using (Mat disp = new Mat(bridge.Image.Size(), MatType.CV_8UC3))
                {
                    var imgSrc = bridge.Image;
                    switch (imgSrc.Channels())
                    {
                        case 4: Cv2.CvtColor(imgSrc, gray, ColorConversionCodes.RGBA2GRAY); break;
                        case 3: Cv2.CvtColor(imgSrc, gray, ColorConversionCodes.RGB2GRAY); break;
                        case 2: Cv2.CvtColor(imgSrc, gray, ColorConversionCodes.BGR5652GRAY); break;
                        case 1: imgSrc.CopyTo(gray); break;
                        default:
                            throw new Exception("bmpTemplate 格式異常!");
                    }

                    //Cv2.CvtColor(gray, disp, ColorConversionCodes.GRAY2BGR);
                    disp.SetTo(Scalar.Black);

                    var bound = new Rect(0, 0, imgSrc.Width, imgSrc.Height);
                    foreach (var rect in rects)
                    {
                        var roi = JetEazy.Qcvt.CV(Rectangle.Round(rect));
                        JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                        if (roi.Width > 1 && roi.Height > 1)
                        {
                            Cv2.CvtColor(gray[roi], disp[roi], ColorConversionCodes.GRAY2BGR);
                            Cv2.Threshold(gray[roi], feature[roi], threshold, 255, ThresholdTypes.Binary);
                            disp[roi].SetTo(Scalar.Red, feature[roi]);
                        }
                    }

                    bmpDisp = BitmapConverter.ToBitmap(disp);
                }
#endif
                bmpDisp = null;
                var aoi = _sysModel?.AoiModel?.GetBadConnInspector();
                if (aoi == null)
                    return;
                aoi.TryApplyBadConnFilters(bmpTemplate, out bmpDisp, rects);
            }
            catch (Exception ex)
            {
                bmpDisp = null;
                HandleException("aoiCreateFeatureMap", ex);
                throw;
            }
        }
        #endregion
    }
}
