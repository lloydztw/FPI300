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
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public class GaTemplDefectsEditCtrl : GaRcpBaseCtrl
    {
        #region RECIPE_PARAMS
        InspectX3ParaClass _xInspectX3
        {
            get { return InspectX3ParaClass.Instance; }
        }
        #endregion

        #region RECIPE_PARAMS_HELPERs
        /// <summary>
        /// 晶粒樣本 Bitmap .
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpDefectTemplate
        /// </summary>
        Bitmap _xRcpGoldenChipBmp
        {
            get => _xRecipe.GoldenChipBmp;
        }
        /// <summary>
        /// _xRcpDefectsMaskBmp 圖形.
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpprintmask
        /// </summary>
        Bitmap _xRcpDefectsMaskBmp
        {
            get => _xRecipe.DefectsMaskBmp;
            set
            {
                if (_xRecipe.DefectsMaskBmp != value)
                {
                    var old = _xRecipe.DefectsMaskBmp;
                    _xRecipe.DefectsMaskBmp = value;
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// _xRcpDefectsMaskRects
        /// 使用比較有意義的英文命名, 
        /// 對應到 _xInspectX3.rectangles 
        /// </summary>
        List<RectangleF> _xRcpDefectsMaskRects
        {
            get => _xInspectX3.DefectMasksRects;
            set => _xInspectX3.DefectMasksRects = value;
        }
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        JezTransImageViewPanel wndGoldenViewer => _editorUI.ImgViewers[1] as JezTransImageViewPanel;
        JezTransImageViewPanel wndDefectsViewer => _editorUI.ImgViewers[2] as JezTransImageViewPanel;
        Button btnDefectRegionAdd => _editorUI.btnDefectRegionAdd;
        Button btnDefectRegionDelete => _editorUI.btnDefectRegionDelete;
        Button btnDefectRegionClearAll => _editorUI.btnDefectRegionClearAll;
        #endregion

        #region VIEWER_TITLES
        //string _Title1 => QMSG.T("Region View");
        //string _Title2 => QMSG.T("Template View");
        string _Title3 => QMSG.T("Defects View");
        #endregion

        #region INTERACTORS
        List<CviRcpBox> _cviDefectMaskBoxes;
        CviRcpBox _cviActiveMaskBox;
        #endregion

        #region RUNTIME_DATA
        //bool _bypassWindowEvents = false;
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
            btnDefectRegionAdd.Click += (s, e) => AddDetectRegion();
            btnDefectRegionDelete.Click += (s, e) => DeleteDetectRegion();
            btnDefectRegionClearAll.Click += (s, e) => ClearAllDetectRegions();
            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;
        }

        #region EVENT_HANDLERS
        private void CviDefectMaskBox_OnChanged(object sender, EventArgs e)
        {
            _cviActiveMaskBox = (sender as CviRcpBox);
            updateMaskTemplateView(true);
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (_isEditting)
            {
                // RESERVED
                //string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;
                //if (propertyName == "xThresholdValue")
                //{
                //    updateMaskTemplateView(true);
                //}
            }
        }
        #endregion

        #region DISP_UI_FUNCTIONS
        void initInteractors()
        {
            _cviDefectMaskBoxes = new List<CviRcpBox>();
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
            updateDefectMaskBoxes(false);
            updateGuiStatus();
        }
        internal void UpdateDefectMaskBoxes(bool editting)
        {
            _isEditting = editting;
            updateDefectMaskBoxes(false);
            if (_isEditting)
                updateMaskTemplateView(false);
            updateGuiStatus();
        }

        void AddDetectRegion()
        {
            if (wndGoldenViewer == null) return;

            // 暫停 dispUI 運作
            var dispUI = wndGoldenViewer;
            var imgViewer = wndGoldenViewer.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 新增 cviMaskBox
            var count = _cviDefectMaskBoxes.Count;
            var rect = new Rectangle(50 + count * 5, 50 + count * 5, 100, 100);
            var cviMaskBox = new CviRcpBox(Brushes.Purple, 1, 3) { Box = rect };
            cviMaskBox.OnChanged += CviDefectMaskBox_OnChanged;

            _cviDefectMaskBoxes.Add(cviMaskBox);
            imgViewer.AddInteractor(cviMaskBox);

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDefectMaskBoxes(true);

            // Update Mask
            updateMaskTemplateView(true);
        }
        void DeleteDetectRegion()
        {
            if (_cviActiveMaskBox == null)
                return;

            // 暫停 dispUI 運作
            var dispUI = wndGoldenViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 移除 _cviActiveMaskBox
            imgViewer.RemoveInteractor(_cviActiveMaskBox);
            _cviDefectMaskBoxes.Remove(_cviActiveMaskBox);
            _cviActiveMaskBox = _cviDefectMaskBoxes.Count > 0 ? _cviDefectMaskBoxes[_cviDefectMaskBoxes.Count - 1] : null;

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDefectMaskBoxes(true);

            // Update Mask
            updateMaskTemplateView(true);
        }
        void ClearAllDetectRegions()
        {
            if (wndGoldenViewer == null) return;

            // 暫停 dispUI 運作
            var dispUI = wndGoldenViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 移除 所有 _cviDefectMaskBoxes
            foreach (var cviBox in _cviDefectMaskBoxes)
                imgViewer.RemoveInteractor(cviBox);
            _cviDefectMaskBoxes.Clear();
            _cviActiveMaskBox = null;

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDefectMaskBoxes(true);

            // Update Mask
            updateMaskTemplateView(true);
        }

        #region PRIVATE_GUI_FUNCTIONS
        List<RectangleF> getDefectMaskRectsFromInteractors()
        {
            var maskRects = new List<RectangleF>();
            foreach (var cviBox in _cviDefectMaskBoxes)
            {
                maskRects.Add(cviBox.Box);
            }
            return maskRects;
        }
        void updateDefectMaskBoxes(bool toRecipe)
        {
            if (wndGoldenViewer == null) 
                return;

            var dispUI = wndGoldenViewer;
            var imgViewer = dispUI.ImgViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            if (toRecipe)
            {
                _xInspectX3.DefectMasksRects = getDefectMaskRectsFromInteractors();
                _isModified = true;
            }
            else
            {
                // 移除舊的
                #region REMOVE_OLD
                foreach (var oldBox in _cviDefectMaskBoxes)
                {
                    if (oldBox != null)
                    {
                        oldBox.Enabled = false;
                        oldBox.Visible = false;
                        imgViewer.RemoveInteractor(oldBox);
                    }
                }
                _cviDefectMaskBoxes.Clear();
                #endregion

                // 重新添加
                #region CREATE_ALL_NEW
                var maskRects = _xInspectX3.DefectMasksRects;
                if (maskRects != null && maskRects.Count > 0)
                {
                    foreach (var rectF in maskRects)
                    {
                        var rect = Rectangle.Round(rectF);
                        if (rect == Rectangle.Empty)
                            continue;

                        AdjustRectToFitViewer(ref rect, imgViewer);

                        var cviMaskBox = new CviRcpBox(Brushes.Purple, 1, 3) { Box = rect };
                        cviMaskBox.OnChanged += CviDefectMaskBox_OnChanged;

                        _cviDefectMaskBoxes.Add(cviMaskBox);
                        imgViewer.AddInteractor(cviMaskBox);
                    }
                }
                #endregion
            }

            dispUI.Enabled = flag;
        }
        void updateMaskTemplateView(bool toRecipe)
        {
            // 從 interactors 取得 mask 區塊
            var maskRects = getDefectMaskRectsFromInteractors();

            // 以 GoldenChipTemplate 為基礎, 製作 bmpMask
            aoiCreateTemplateMask(_xRcpGoldenChipBmp, maskRects, out Bitmap bmpMask, out Bitmap bmpDisp);

            if (toRecipe)
            {
                // 更新 參數 (_xBmpMask 會接手 bmpMask 生命)
                _xRcpDefectsMaskBmp = bmpMask;
                _xRcpDefectsMaskRects = maskRects;
                _isModified = true;
            }
            else
            {
                // CleanUp
                bmpMask?.Dispose();
            }

            // 更新 bmpMask 到 GUI
            if (_isEditting)
                wndDefectsViewer?.UpdateImage(bmpDisp, _Title3, false);

            // CleanUp
            bmpDisp?.Dispose();
        }
        void updateGuiStatus()
        {
            foreach(var box in _cviDefectMaskBoxes)
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
        void aoiCreateTemplateMask(Bitmap bmpTemplate, IEnumerable<RectangleF> maskRects, out Bitmap bmpMask, out Bitmap bmpDisp)
        {
            try
            {
                // 使用 OpenCvSharp, 製作 mask (8 bpp) 將 maskRects 指定的區域塗成白色
                using (var bridge = new QxImageBridge(bmpTemplate))
                using (Mat gray = new Mat(bridge.Image.Size(), MatType.CV_8UC1))
                using (Mat mask = new Mat(bridge.Image.Size(), MatType.CV_8UC1))
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

                    mask.SetTo(Scalar.Black);
                    var bound = new Rect(0, 0, imgSrc.Width, imgSrc.Height);
                    foreach (var rect in maskRects)
                    {
                        var roi = JetEazy.Qcvt.CV(Rectangle.Round(rect));
                        JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);
                        if (roi.Width > 1 && roi.Height > 1)
                            mask[roi].SetTo(Scalar.White);
                    }

                    Cv2.BitwiseAnd(gray, mask, gray);
                    bmpMask = BitmapConverter.ToBitmap(mask);
                    bmpDisp = BitmapConverter.ToBitmap(gray);
                }
            }
            catch (Exception ex)
            {
                bmpMask = null;
                bmpDisp = null;
                HandleException("aoiCreateTemplateMask", ex);
                throw;
            }
        }
        #endregion
    }
}
