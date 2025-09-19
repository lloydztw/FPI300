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

using JetEazy.FormSpace;
using JetEazy.OpenCV;
using JetEazy.Utils;
using JzDisplay;
using JzDisplay.UISpace;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;


namespace LaserAlignDX.Mvc.Ctrl
{
    public partial class GaTemplateEditCtrl
    {
        #region ENUM
        enum OpSelector : int
        {
            None = -1,
            Golden,
            LineBorders,
            QrCode,
            Defects,
        }
        OpSelector _opSelector = OpSelector.None;
        #endregion

        #region GLOBAL_MESS
        CarrierEnum _carrierID;
        #endregion

        #region RECIPE_PARAMS
        RecipeFPIX3Class _xRecipe
        {
            get { return RecipeFPIX3Class.Instance; }
        }
        InspectX3ParaClass _xInspectX3
        {
            get { return InspectX3ParaClass.Instance; }
        }
        #endregion

        #region RECIPE_PARAMS_HELPERs
        /// <summary>
        /// 區域樣本 (Region Cell) 影像. 
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpprinttemplate
        /// </summary>
        Bitmap _xBmpGoldenRegionTemplate
        {
            get => _xRecipe.bmpprinttemplate;
            set
            {
                if (_xRecipe.bmpprinttemplate != value)
                {
                    var old = _xRecipe.bmpprinttemplate;
                    _xRecipe.bmpprinttemplate = value;
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// 晶粒樣本 Bitmap .
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpDefectTemplate
        /// </summary>
        Bitmap _xBmpGoldenChipTemplate
        {
            get => _xRecipe.bmpDefectTemplate;
            set
            {
                if (_xRecipe.bmpDefectTemplate != value)
                {
                    var old = _xRecipe.bmpDefectTemplate;
                    _xRecipe.bmpDefectTemplate = value;
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// QrCode 樣本 Bitmap .
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpcodetemplate
        /// </summary>
        Bitmap _xBmpQrCodeTemplate
        {
            get => _xRecipe.bmpcodetemplate;
            set
            {
                if (_xRecipe.bmpcodetemplate != value)
                {
                    var old = _xRecipe.bmpcodetemplate;
                    _xRecipe.bmpcodetemplate = value;
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// _xBmpMask 圖形.
        /// 使用比較有意義的英文命名, 
        /// 對應到 bmpprintmask
        /// </summary>
        Bitmap _xBmpMask
        {
            get => _xRecipe.bmpprintmask;
            set
            {
                if (_xRecipe.bmpprintmask != value)
                {
                    var old = _xRecipe.bmpprintmask;
                    _xRecipe.bmpprintmask = value;
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// golden chip 樣本在 Region Cell 內的矩形位置.
        /// 使用比較有意義的英文命名, 
        /// 對應到 xRegionTrain       
        /// </summary>
        RectangleF _xGoldenChipRect
        {
            get => _xRecipe.xRegionTrain;
            set => _xRecipe.xRegionTrain = value;
        }
        /// <summary>
        /// QrCode 樣本在 Region Cell 內的矩形位置.
        /// 使用比較有意義的英文命名, 
        /// 對應到 xRectCodeRegion 
        /// </summary>
        RectangleF _xQrCodeRect
        {
            get => _xRecipe.xRectCodeRegion;
            set => _xRecipe.xRectCodeRegion = value;
        }
        /// <summary>
        /// _xMaskRects
        /// 使用比較有意義的英文命名, 
        /// 對應到 _xInspectX3.rectangles 
        /// </summary>
        List<RectangleF> _xMaskRects
        {
            get => _xInspectX3.rectangles;
            set => _xInspectX3.rectangles = value;
        }
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        Button btnPickGolden => _editorUI.btnPickGolden;
        Button btnTryScanQrCode => _editorUI.btnTryScanQrCode;
        Button btnAutoLayoutLineBorders => _editorUI.btnAutoLineBorders;
        Button btnCreateTemplate => _editorUI.btnCreateTemplate;
        Button btnSaveTemplateAndParams => _editorUI.btnSaveTemplateAndParams;
        Button btnDefectRegionAdd => _editorUI.btnDefectRegionAdd;
        Button btnDefectRegionDelete => _editorUI.btnDefectRegionDelete;
        Button btnDefectRegionClearAll => _editorUI.btnDefectRegionClearAll;
        DispUI DS1 => _editorUI.DispViewers[0];
        DispUI DS2 => _editorUI.DispViewers[1];
        DispUI DS3 => _editorUI.DispViewers[2];
        #endregion

        #region INTERACTORS
        CviRcpBox _cviGoldenChipBox;
        CviRcpBox _cviQrCodeBox;
        CviRcpBox[] _cviLineBorderBoxes;
        CviLineSegmentsBox[] _cviLineSegmentBoxes;
        List<CviRcpBox> _cviDefectMaskBoxes;
        CviRcpBox _cviActiveMaskBox;
        #endregion

        #region RUNTIME_DATA
        bool _isGoldenModified = false;
        bool _isQrCodeModified = false;
        bool _isLineBorderModified = false;
        bool _isDefectMaskModified = false;
        #endregion

        public void Attach(IvTemplateEditorUI ui, CarrierEnum C)
        {
            _editorUI = ui;
            _carrierID = C;
            initGui();
            connectEventHandlers();
        }
        void initGui()
        {
            initDispUIs();
            initInteractors();
        }
        void connectEventHandlers()
        {
            _editorUI.Window.HandleCreated += Window_HandleCreated;
            btnPickGolden.Click += (s, e) => BuildGoldenChipTemplate();
            btnTryScanQrCode.Click += (s, e) => UpdateQrCodeAndTryDecode();
            btnAutoLayoutLineBorders.Click += (s, e) => AutoLayoutLineBorders();
            btnCreateTemplate.Click += (s, e) => TrainGoldenChipTemplate();
            btnSaveTemplateAndParams.Click += (s, e) => SaveAllParams(prompt: true);

            btnDefectRegionAdd.Click += (s, e) => DfRegion_Add();
            btnDefectRegionDelete.Click += (s, e) => DfRegion_Delete();
            btnDefectRegionClearAll.Click += (s, e) => DfRegion_ClearAll();

            foreach (var rdoBoxSelector in _editorUI.rdoBoxSelectors)
            {
                rdoBoxSelector.CheckedChanged += RdoBoxSelector_CheckedChanged;
            }
            foreach (var cviBox in _cviLineBorderBoxes)
            {
                cviBox.OnChanged += (s, e) =>
                {
                    updateLineBorderBoxes(true);
                    updateLineSegmentBoxes(true);
                };
            }
            _cviQrCodeBox.OnChanged += (s, e) => updateGoldenBoxes(true, true);
        }

        #region EVENT_HANDLERS
        private void Window_HandleCreated(object sender, EventArgs e)
        {
            updateDispUI(DS1, _xBmpGoldenRegionTemplate);
            updateDispUI(DS2, _xBmpGoldenChipTemplate);

            updateGoldenBoxes(false);
            persistLineBorderIndentExt(false);
            updateLineBorderBoxes(false);
            updateDefectMaskBoxes(false);
            updateMaskTemplate(false);
            updateVisionParams();

            SetSelector(OpSelector.Golden);

            _editorUI.Window.FindForm().FormClosing += GaTemplateEditCtrl_FormClosing;
            _editorUI.lblActiveCarrierID.Text = GaUtil.GetEnumDescription(_carrierID);
        }
        private void GaTemplateEditCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAllParams(prompt: false);
            persistLineBorderIndentExt(true);
        }
        private void RdoBoxSelector_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rdo && rdo.Checked)
            {
                int index = Array.IndexOf(_editorUI.rdoBoxSelectors, rdo);
                SetSelector((OpSelector)index);
            }
        }
        private void CviDefectMaskBox_OnChanged(object sender, EventArgs e)
        {
            _cviActiveMaskBox = (sender as CviRcpBox);
            updateMaskTemplate(true);
        }
        #endregion

        #region DISP_UI_FUNCTIONS
        void initDispUIs()
        {
            DS1.Initial(100, 0.01f);
            DS1.SetDisplayType(DisplayTypeEnum.SHOW);
            DS2.Initial(100, 0.01f);
            DS2.SetDisplayType(DisplayTypeEnum.SHOW);
            DS3.Initial(100, 0.01f);
            DS3.SetDisplayType(DisplayTypeEnum.SHOW);
        }
        void initInteractors()
        {
            _cviGoldenChipBox = new CviRcpBox(Brushes.Orange, 1, 3) { Visible = false };
            _cviQrCodeBox = new CviRcpBox(Brushes.Purple, 1, 3) { Visible = false };
            _cviDefectMaskBoxes = new List<CviRcpBox>();

            _cviLineBorderBoxes = new CviRcpBox[4];
            _cviLineSegmentBoxes = new CviLineSegmentsBox[4];
            for (int i = 0, N = _cviLineBorderBoxes.Length; i < N; i++)
            {
                _cviLineBorderBoxes[i] = new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false };
                _cviLineSegmentBoxes[i] = new CviLineSegmentsBox(Color.Cyan) { Visible = false };
            }

            var viewer = DS1.ImageViewer;
            viewer.AddInteractor(_cviGoldenChipBox);
            viewer.AddInteractor(_cviQrCodeBox);

            foreach (var box in _cviLineBorderBoxes)
                viewer.AddInteractor(box);
            foreach (var box in _cviLineSegmentBoxes)
                viewer.AddInteractor(box);
        }
        void updateDispUI(DispUI dispUI, Bitmap bmp, bool autoZoom = false)
        {
            //-------------------------------------------------------------------------------------------------
            // GAARA 版本的 dispUI
            // ReplaceDisplayImage 內部會調用 bmp.Clone()
            //-------------------------------------------------------------------------------------------------

            if (bmp != null)
            {
                dispUI.ReplaceDisplayImage(bmp);
            }
            else
            {
                using (var dummy = new Bitmap(200, 200, System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
                {
                    dispUI.ReplaceDisplayImage(dummy);
                }
            }

            dispUI.Refresh();

            if (autoZoom)
                dispUI.DefaultView();
        }
        void refreshDispUI(DispUI dispUI)
        {
            (dispUI?.ImageViewer as Control).Invalidate();
        }
        #endregion

        void SetSelector(OpSelector selector)
        {
            if (_opSelector != selector)
            {
                _opSelector = selector;
                updateGuiStatus();
                refreshDispUI(DS1);
                refreshDispUI(DS3);
                updateLineSegmentBoxes(_opSelector == OpSelector.LineBorders);
            }
        }

        void BuildGoldenChipTemplate()
        {
            var roiRect = _cviGoldenChipBox.Box;
            var bmpRegion = _xBmpGoldenRegionTemplate;
            var bound = new Rectangle(0, 0, bmpRegion.Width, bmpRegion.Height);
            JetEazy.QUtilities.QUtility.ClipBoundary(ref roiRect, ref bound);
            if (roiRect.Width < 2 || roiRect.Height < 2)
                return;

            // 更新 參數
            var goldenBmp = _xBmpGoldenRegionTemplate.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            this._xBmpGoldenChipTemplate = goldenBmp;
            this._xGoldenChipRect = roiRect;

            //// 保存 參數 (for Golden Chip template)
            //if (saveToFile)
            //    _xRecipe.SavePrintTemplateRegionTrain();
            _isGoldenModified = true;

            // 更新 GUI
            updateDispUI(DS2, goldenBmp, autoZoom: true);

            // 更新 mask
            updateMaskTemplate(false);
        }
        void UpdateQrCodeAndTryDecode()
        {
            var roiRect = _cviQrCodeBox.Box;
            var bmpRegion = _xBmpGoldenRegionTemplate;
            var bound = new Rectangle(0, 0, bmpRegion.Width, bmpRegion.Height);
            JetEazy.QUtilities.QUtility.ClipBoundary(ref roiRect, ref bound);
            if (roiRect.Width < 2 || roiRect.Height < 2)
                return;

            // 更新 參數
            var bmp = _xBmpGoldenRegionTemplate.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            this._xBmpQrCodeTemplate = bmp;
            this._xQrCodeRect = roiRect;

            //// 保存 參數 (for QrCode template)
            //if (saveToFile)
            //    _xRecipe.SaveCodeTemplate();
            _isQrCodeModified = true;

            // DECODE
            aoiDecodeQrCode(this._xBmpQrCodeTemplate, out string text);

            // 更新 Text
            _editorUI.rtbQrCodeResult.Text = text;
        }
        void TrainGoldenChipTemplate()
        {
            int err = _xRecipe.PrintTempTrain();
            if (err == 0)
                VsMessageBox.Info("創建成功.");
            else
                VsMessageBox.Warning("創建失敗!");
        }
        void AutoLayoutLineBorders()
        {
            if (_xBmpGoldenChipTemplate == null || _xGoldenChipRect == RectangleF.Empty)
            {
                VsMessageBox.Warning("請先設定 晶粒匹配樣本!");
                return;
            }

            var rc = Rectangle.Round(_xGoldenChipRect);
            var ind = (int)_editorUI.numBorderIndent.Value;
            var ext = (int)_editorUI.numBorderSize.Value;
            var W = rc.Width;
            var H = rc.Height;
            var ww = (int)(rc.Width * 0.6);
            var hh = (int)(rc.Height * 0.6);
            var dw = W - ww;
            var dh = H - hh;
            var x = rc.X;
            var y = rc.Y;

            int i = 0;
            _cviLineBorderBoxes[i++].Box = new Rectangle(x - ext, y + dh / 2, ind + ext, hh);
            _cviLineBorderBoxes[i++].Box = new Rectangle(x + dw / 2, y - ext, ww, ind + ext);
            _cviLineBorderBoxes[i++].Box = new Rectangle(rc.Right - ind, y + dh / 2, ind + ext, hh);
            _cviLineBorderBoxes[i++].Box = new Rectangle(x + dw / 2, rc.Bottom - ind, ww, ind + ext);
            refreshDispUI(DS1);

            updateLineBorderBoxes(true);
            updateLineSegmentBoxes(true);
        }
        void SaveAllParams(bool prompt)
        {
            // 根據舊的 Gaara 邏輯
            // Golden, QrCode, 與 LineBorder 需要自動存檔.
            // 只剩下 Mask 不會自動存檔

            if (_isGoldenModified)
            {
                _isGoldenModified = false;
                _xRecipe.SavePrintTemplateRegionTrain();
            }

            if (_isQrCodeModified)
            {
                _isQrCodeModified = false;
                _xRecipe.SaveCodeTemplate();
            }

            if (_isLineBorderModified)
            {
                _isLineBorderModified = false;
                _xRecipe.SaveLinesRegion();
            }

            if (_isDefectMaskModified)
            {
                _isDefectMaskModified = false;
                _xRecipe.SavePrintTemplate();
            }

            if (prompt)
            {
                VsMessageBox.Info("保存成功.");
            }
        }

        void DfRegion_Add()
        {
            // 暫停 dispUI 運作
            var dispUI = DS2;
            var imgViewer = dispUI.ImageViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 新增 cviBox
            var count = _cviDefectMaskBoxes.Count;
            var rect = new Rectangle(50 + count * 5, 50 + count * 5, 100, 100);
            var cviDefectMaskBox = new CviRcpBox(Brushes.Blue, 1, 3);
            cviDefectMaskBox.OnChanged += CviDefectMaskBox_OnChanged;

            _cviDefectMaskBoxes.Add(cviDefectMaskBox);
            imgViewer.AddInteractor(cviDefectMaskBox);

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDefectMaskBoxes(true);

            // Update Mask
            updateMaskTemplate(true);
        }
        void DfRegion_Delete()
        {
            if (_cviActiveMaskBox == null)
                return;

            // 暫停 dispUI 運作
            var dispUI = DS2;
            var imgViewer = dispUI.ImageViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            // 移除 _cviActiveMaskBox
            imgViewer.RemoveInteractor(_cviActiveMaskBox);
            _cviDefectMaskBoxes.Remove(_cviActiveMaskBox);
            _cviActiveMaskBox = null;

            // 恢復 dispUI 運作
            dispUI.Enabled = flag;
            refreshDispUI(dispUI);

            // 更新 參數
            updateDefectMaskBoxes(true);

            // Update Mask
            updateMaskTemplate(true);
        }
        void DfRegion_ClearAll()
        {
            // 暫停 dispUI 運作
            var dispUI = DS2;
            var imgViewer = dispUI.ImageViewer;
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
            updateMaskTemplate(true);
        }

        #region PRIVATE_GUI_FUNCTIONS
        List<RectangleF> getDefectMaskRectsFromInteractors()
        {
            var maskRects = new List<RectangleF>();
            //for (int i = 0, N = _xMovers.Count; i < N; i++)
            //{
            //    GraphicalObject grobj = _xMovers[i].Source;
            //    RectangleF rectF = (grobj as JzRectEAG).GetRectF;
            //    maskRects.Add(rectF);
            //}
            foreach (var cviBox in _cviDefectMaskBoxes)
            {
                maskRects.Add(cviBox.Box);
            }
            return maskRects;
        }
        void updateGoldenBoxes(bool toRecipe, bool saveToFile = false)
        {
            if (toRecipe)
            {
                this._xGoldenChipRect = _cviGoldenChipBox.Box;
                this._xQrCodeRect = _cviQrCodeBox.Box;
                if (saveToFile)
                    _xRecipe.SaveCodeTemplate();
            }
            else
            {
                var rect = Rectangle.Round(this._xGoldenChipRect);
                if (rect == RectangleF.Empty)
                    rect = new Rectangle(50, 50, 100, 100);
                _cviGoldenChipBox.Box = rect;

                rect = Rectangle.Round(this._xQrCodeRect);
                if (rect == RectangleF.Empty)
                    rect = _cviGoldenChipBox.Box;
                _cviQrCodeBox.Box = rect;
            }
        }
        void updateLineBorderBoxes(bool toRecipe, bool saveToFile = false)
        {
            if (toRecipe)
            {
                int i = 0;
                _xRecipe.xLineLeft = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineTop = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineRight = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineBottom = _cviLineBorderBoxes[i++].Box;
                //if (saveToFile)
                //    _xRecipe.SaveLinesRegion();
                _isLineBorderModified = true;
            }
            else
            {
                var lineBorderRects = new[] {
                    _xRecipe.xLineLeft,
                    _xRecipe.xLineTop,
                    _xRecipe.xLineRight,
                    _xRecipe.xLineBottom,
                };

                for (int i = 0, N = lineBorderRects.Length; i < N; i++)
                {
                    var rect = Rectangle.Round(lineBorderRects[i]);
                    if (rect == Rectangle.Empty)
                        rect = new Rectangle(50 * i, 50 * i, 100, 100);
                    _cviLineBorderBoxes[i].Box = rect;
                }
            }
        }
        void updateLineSegmentBoxes(bool show)
        {
            if (!show)
            {
                foreach (var lineSegBox in _cviLineSegmentBoxes)
                    lineSegBox.Visible = false;
            }
            else
            {
                var bmpSrcRegion = _xBmpGoldenRegionTemplate;
                foreach (EdgeBorder eBorder in Enum.GetValues(typeof(EdgeBorder)))
                {
                    int idx = (int)eBorder;
                    var borderRect = _cviLineBorderBoxes[idx].Box;
                    bool ok = aoiTryRunFindLineSegment(eBorder, bmpSrcRegion, borderRect, out var mvdLines);
                    var linesOut = GaMvdExt.ToCSharpLines(mvdLines);
                    _cviLineSegmentBoxes[idx].Attach(linesOut);
                    _cviLineSegmentBoxes[idx].Visible = ok;
                }
            }
        }
        void persistLineBorderIndentExt(bool save)
        {
            try
            {
                if (save)
                {
                    Properties.Settings.Default.lineBorderIndent = (int)_editorUI.numBorderIndent.Value;
                    Properties.Settings.Default.lineBorderExt = (int)_editorUI.numBorderSize.Value;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    var indent = Properties.Settings.Default.lineBorderIndent;
                    var ext = Properties.Settings.Default.lineBorderExt;
                    _editorUI.numBorderIndent.Value = indent;
                    _editorUI.numBorderSize.Value = ext;
                }
            }
            catch
            {
            }
        }
        void updateDefectMaskBoxes(bool toRecipe)
        {
            var dispUI = DS2;
            var imgViewer = dispUI.ImageViewer;
            bool flag = dispUI.Enabled;
            dispUI.Enabled = false;

            if (toRecipe)
            {
                _xInspectX3.rectangles = getDefectMaskRectsFromInteractors();
                _isDefectMaskModified = true;
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
                var maskRects = _xInspectX3.rectangles;
                if (maskRects != null && maskRects.Count > 0)
                {
                    foreach (var rectF in maskRects)
                    {
                        var rect = Rectangle.Round(rectF);
                        if (rect == Rectangle.Empty)
                            continue;

                        var cviBox = new CviRcpBox(Brushes.Blue, 1, 3) { Box = rect };
                        cviBox.OnChanged += CviDefectMaskBox_OnChanged;

                        _cviDefectMaskBoxes.Add(cviBox);
                        imgViewer.AddInteractor(cviBox);
                    }
                }
                #endregion
            }

            dispUI.Enabled = flag;
        }
        void updateMaskTemplate(bool toRecipe)
        {
            // 從 interactors 取得 mask 區塊
            var maskRects = getDefectMaskRectsFromInteractors();

            // 以 GoldenChipTemplate 為基礎, 製作 bmpMask
            aoiCreateTemplateMask(_xBmpGoldenChipTemplate, maskRects, out Bitmap bmpMask, out Bitmap bmpDisp);

            if (toRecipe)
            {
                // 更新 參數 (_xBmpMask 會接手 bmpMask 生命)
                _xBmpMask = bmpMask;
                _xMaskRects = maskRects;
            }
            else
            {
                bmpMask?.Dispose();
            }

            // 更新 bmpMask 到 GUI
            updateDispUI(DS3, maskRects.Count > 0 ? bmpDisp : null);

            // CleanUp
            bmpDisp?.Dispose();
        }
        void updateVisionParams()
        {
            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                pg.SelectedObject = _xInspectX3;
            }
        }
        void updateGuiStatus()
        {
            showInteractors(_opSelector);
            _editorUI.btnPickGolden.Enabled = _opSelector == OpSelector.Golden;
            _editorUI.btnAutoLineBorders.Enabled = _opSelector == OpSelector.LineBorders;
            _editorUI.numBorderIndent.Enabled = _opSelector == OpSelector.LineBorders;
            _editorUI.numBorderSize.Enabled = _opSelector == OpSelector.LineBorders;
            _editorUI.btnTryScanQrCode.Enabled = _opSelector == OpSelector.QrCode;
        }
        void showInteractors(OpSelector selector)
        {
            _cviGoldenChipBox.Visible = selector == OpSelector.Golden;
            _cviQrCodeBox.Visible = selector == OpSelector.QrCode;
            foreach (var cviBox in _cviLineBorderBoxes)
                cviBox.Visible = selector == OpSelector.LineBorders;
        }
        #endregion

        #region AOI_MODEL_FUNCTIONS
        //----------------------------------------------------------------------------
        // 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        //----------------------------------------------------------------------------
        void aoiDecodeQrCode(Bitmap srcBmp, out string text)
        {
            if (srcBmp == null)
            {
                text = "";
                return;
            }

            //>>> xRecipe.mvd2DReader.Run(xRecipe.bmpcodetemplate,
            //>>>       new RectangleF(0, 0, xRecipe.bmpcodetemplate.Width, xRecipe.bmpcodetemplate.Height));

            var aoiTool = _xRecipe.mvd2DReader;
            var roi = new RectangleF(0, 0, srcBmp.Width, srcBmp.Height);

            aoiTool.Run(srcBmp, roi);

            var decodeInfo = aoiTool.DCodeInfo;
            text = decodeInfo != null ? decodeInfo.Content : "";
        }
        void aoiCreateTemplateMask(Bitmap bmpTemplate, IEnumerable<RectangleF> maskRects, out Bitmap bmpMask, out Bitmap bmpDisp)
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
        bool aoiTryRunFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF boxRect, out CMvdLineSegmentF[] resultLines)
        {
            resultLines = null;

#if (OPT_RESERVED)
            bool bPositive, bEdgePolarity, bFindOrient;

            #region 方向與極性
            switch (eBorder)
            {
                case EdgeBorder.Left:
                    bPositive = _xInspectX3.bPositive0;
                    bEdgePolarity = _xInspectX3.bEdgePolarity0;
                    bFindOrient = true;
                    break;
                case EdgeBorder.Right:
                    bPositive = _xInspectX3.bPositive1;
                    bEdgePolarity = _xInspectX3.bEdgePolarity1;
                    bFindOrient = false;
                    break;
                case EdgeBorder.Top:
                    bPositive = _xInspectX3.bPositive2;
                    bEdgePolarity = _xInspectX3.bEdgePolarity2;
                    bFindOrient = true;
                    break;
                case EdgeBorder.Bottom:
                    bPositive = _xInspectX3.bPositive3;
                    bEdgePolarity = _xInspectX3.bEdgePolarity3;
                    bFindOrient = false;
                    break;
                default:
                    return false;
            }
            #endregion

            var mflType = _xInspectX3.MFLType;

            // 目前黑色背景 暫時強制用 FindLineType_v1
            mflType = Eazy_Project_III.MeasureFindLineType.FindLineType_v1;

            switch (mflType)
            {
                case Eazy_Project_III.MeasureFindLineType.FindLineType_v2:
                    #region 找平行线
                    using (MvdPairLineClass pairLine = new MvdPairLineClass())
                    {
                        pairLine.bPositive = bPositive;
                        pairLine.bFindOrient = bFindOrient;
                        CPairLineFindResult pair = pairLine.Run(bmpSrc, boxRect, (int)eBorder);
                        if (pair != null)
                        {
                            //drawMvdLinesToDisp(bmpSrc, pair.Line0, pair.Line1);
                            resultLines = new[]
                            {
                                pair.Line0,
                                pair.Line1,
                            };
                            return true;
                        }
                    }
                    #endregion
                    break;
                default:
                    #region 找直线
                    using (MvdFindLineClass finder = new MvdFindLineClass())
                    {
                        finder.bPositive = bPositive;
                        finder.bFindOrient = bFindOrient;
                        finder.bEdgePolarity = bEdgePolarity;
                        var mvdLine = finder.Run(bmpSrc, boxRect, (int)eBorder);
                        //drawMvdLinesToDisp(bmpSrc, mvdLine);
                        resultLines = new[] { mvdLine };
                        return true;
                    }
                    #endregion
            }
#endif

            // 目前邊線 用於 黑色背景 比較準確
            using (MvdFindLineClass lineSegFinder = new MvdFindLineClass())
            {
                lineSegFinder.Background = _xInspectX3.CarrierBackground;
                var mvdLine = lineSegFinder.Run(bmpSrc, boxRect, (int)eBorder);
                resultLines = new[] { mvdLine };
                return mvdLine != null;
            }
        }
        #endregion
    }
}
