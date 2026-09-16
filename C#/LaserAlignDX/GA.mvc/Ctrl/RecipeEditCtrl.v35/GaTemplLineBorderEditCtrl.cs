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
using JetEazy.Lang;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.Mvc.Model.Recipe;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VisionDesigner;
using CviLineBorderBox = LaserAlignDX.Mvc.Gui.CviLineBorderBox;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public class GaTemplLineBorderEditCtrl : GaRcpBaseCtrl
    {
        #region RECIPE_PARAMS
        InspectX3ParaClass _xInspectParams
        {
            get => _xRecipe.InspectParams;
        }
        MatchAlgorithmEnum _xAlgorithm
        {
            get => _xInspectParams.xAlgorithm;
        }
        DtoX3LineBorderParams _xLineBorderParams
        {
            get => _xRecipe.LineBorderParams;
        }
        LineBorderPairsCollection _xLineBorderPairs
        {
            get => _xLineBorderParams.LineBorderPairs;
        }
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        IvTemplLineBordersEditorUI _lineBordersEditorUI => _editorUI;
        JezTransImageViewPanel wndRegionViewer => _editorUI.ImgViewers[0] as JezTransImageViewPanel;
        NumericUpDown numMeasureDistXs => _lineBordersEditorUI.numMeasureDistXs;
        NumericUpDown numMeasureDistYs => _lineBordersEditorUI.numMeasureDistYs;
        NumericUpDown numMeasureMasks => _lineBordersEditorUI.numMeasureMasks;
        NumericUpDown numBorderIndent => _lineBordersEditorUI.numBorderIndent;
        NumericUpDown numBorderOutdent => _lineBordersEditorUI.numBorderExtend;
        NumericUpDown numLineSpanPercentage => _lineBordersEditorUI.numLineSpanPercentage;
        NumericUpDown numGrayLimitHi => _lineBordersEditorUI.numGrayLimitHi;
        NumericUpDown numGrayLimitLo => _lineBordersEditorUI.numGrayLimitLo;
        CheckBox chkAlwaysShowFilterResult => _lineBordersEditorUI.chkShowFilterResult;
        Button btnAutoLayoutLineBorders => _lineBordersEditorUI.btnAutoLineBorders;
        Button btnBuildMictroTransform => _lineBordersEditorUI.btnBuildMircoTransform;
        Timer _restoreRegionViewTimer;
        #endregion

        #region GUI_DRAWING_OBJECTS
        Brush getStockBrush(string keyName)
        {
            (string cate, int idx) = LineBorderPair.ParseKeyName(keyName);
            if (cate == "X")
                return idx % 2 == 0 ? Brushes.Blue : Brushes.MediumBlue;
            else
                return idx % 2 == 0 ? Brushes.Purple : Brushes.MediumPurple;
        }
        #endregion

        #region INTERACTORS
        int MAX_LINE_BORDER_PAIRS => LineBorderPair.MAX_PAIRS;
        CviLineBorderBox[] _cviLineBorderBoxes;
        CviLineSegmentsBox[] _cviLineSegmentBoxes;
        #endregion

        #region CHILD_CTRL
        readonly GaTemplLineGapBorderEditCtrl _gapBordersCtrl = new GaTemplLineGapBorderEditCtrl();
        #endregion

        #region RUNTIME_DATA
        bool _isGapBorderBoxActived = false;
        bool _bypassWindowEvents = false;
        bool _isEditting = false;
        bool _isModified = false;
        int _actualLineBordersCount = 0;
        #endregion

        public EventHandler<DoWorkEventArgs> OnRequestToLocateGoldenQuad;
        public EventHandler OnMicroTransformChanged;

        public void Attach(IvTemplateEditorUI ui)
        {
            _editorUI = ui;
            initGui();
            connectEventHandlers();
            _gapBordersCtrl.Attach(ui);
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initGui()
        {
            numMeasureDistXs.Maximum = MAX_LINE_BORDER_PAIRS;
            numMeasureDistYs.Maximum = MAX_LINE_BORDER_PAIRS;
            numMeasureMasks.Visible = true;
            initInteractors();
            updateFilterSettings(false);
        }
        void initInteractors()
        {
            int N4 = MAX_LINE_BORDER_PAIRS * 4;

            _cviLineBorderBoxes = new CviLineBorderBox[N4];
            _cviLineSegmentBoxes = new CviLineSegmentsBox[N4];

            for (int i = 0; i < N4; i++)
            {
                _cviLineBorderBoxes[i] = new CviLineBorderBox(Brushes.Blue, 1, 3) { Visible = false };
                _cviLineSegmentBoxes[i] = new CviLineSegmentsBox(Color.Cyan) { Visible = false };
            }

            var viewer = wndRegionViewer?.ImgViewer;
            if (viewer != null)
            {
                foreach (var box in _cviLineBorderBoxes)
                    viewer.AddInteractor(box);
                foreach (var box in _cviLineSegmentBoxes)
                    viewer.AddInteractor(box);
            }

            //_cviLineBorderBoxPairsH = new List<CviRcpBox[]>();
            //_cviLineBorderBoxPairsV = new List<CviRcpBox[]>();
            //_cviLineSegmentPairsH = new List<CviLineSegmentsBox[]>();
            //_cviLineSegmentPairsV = new List<CviLineSegmentsBox[]>();
            //for (int i = 0; i < N_PAIRS; i++)
            //{
            //    _cviLineBorderBoxPairsH.Add(new CviRcpBox[]
            //    {
            //        new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false },
            //        new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false },
            //    });
            //    _cviLineBorderBoxPairsV.Add(new CviRcpBox[]
            //    {
            //        new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false },
            //        new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false },
            //    });
            //    _cviLineSegmentPairsH.Add(new CviLineSegmentsBox[]
            //    {
            //        new CviLineSegmentsBox(Color.Cyan) { Visible = false },
            //        new CviLineSegmentsBox(Color.Cyan) { Visible = false },
            //    });
            //    _cviLineSegmentPairsV.Add(new CviLineSegmentsBox[]
            //    {
            //        new CviLineSegmentsBox(Color.Cyan) { Visible = false },
            //        new CviLineSegmentsBox(Color.Cyan) { Visible = false },
            //    });
            //}

            //var viewer = wndRegionViewer?.ImgViewer;
            //if (viewer != null)
            //{
            //    foreach (var pair in _cviLineBorderBoxPairsH)
            //    {
            //        viewer.AddInteractor(pair[0]);
            //        viewer.AddInteractor(pair[1]);
            //    }
            //    foreach (var pair in _cviLineBorderBoxPairsV)
            //    {
            //        viewer.AddInteractor(pair[0]);
            //        viewer.AddInteractor(pair[1]);
            //    }
            //    foreach (var pair in _cviLineSegmentPairsH)
            //    {
            //        viewer.AddInteractor(pair[0]);
            //        viewer.AddInteractor(pair[1]);
            //    }
            //    foreach (var pair in _cviLineSegmentPairsV)
            //    {
            //        viewer.AddInteractor(pair[0]);
            //        viewer.AddInteractor(pair[1]);
            //    }
            //}
        }
        void connectEventHandlers()
        {
            _editorUI.Window.HandleDestroyed += Window_HandleDestroyed;

            btnAutoLayoutLineBorders.Click += (s, e) => AutoLayoutLineBorders();
            btnBuildMictroTransform.Click += (s, e) => BuildMicroTransform();

            numMeasureDistXs.ValueChanged += NumMeasureDistXs_ValueChanged;
            numMeasureDistYs.ValueChanged += NumMeasureDistYs_ValueChanged;
            numMeasureMasks.ValueChanged += NumMeasureMasks_ValueChanged;

            numBorderIndent.ValueChanged += NumBorderOutdent_ValueChanged;
            numBorderOutdent.ValueChanged += NumBorderOutdent_ValueChanged;
            numLineSpanPercentage.ValueChanged += NumBorderOutdent_ValueChanged;

            // FILTERS
            numGrayLimitHi.ValueChanged += NumGrayLimit_ValueChanged;
            numGrayLimitLo.ValueChanged += NumGrayLimit_ValueChanged;
            chkAlwaysShowFilterResult.CheckedChanged += ChkShowFilterResult_CheckedChanged;

            // LINE BORDER BOXES
            foreach (var cviBox in _cviLineBorderBoxes)
            {
                cviBox.OnChanged += (s, e) =>
                {
                    updateLineBorderBoxes(true);
                    updateLineSegmentBoxes(true);
                };
            }

            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;

            // GAP BORDER
            _editorUI.OnGapBorderViewActiveChanged += (s, e) => SwitchToGapBorderOpMode(e);
            _gapBordersCtrl.OnRequestToLocateGoldenQuad += (s, e) =>
            {
                OnRequestToLocateGoldenQuad?.Invoke(s, e);
                fetchLineSegments(false);
            };
        }
        #endregion

        #region EVENT_HANDLERS
        private void Window_HandleDestroyed(object sender, EventArgs e)
        {
            disposeRestoreRegionViewTimer();
            persistLineBorderIndentExt(true);
            VxDebugDrawer.DestroyAllWindows();
        }
        private void NumMeasureDistXs_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents) return;
            updateLineBorderBoxesNumber(true);
        }
        private void NumMeasureDistYs_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents) return;
            updateLineBorderBoxesNumber(true);
        }
        private void NumMeasureMasks_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents) return;
            throw new NotImplementedException();
        }
        private void NumBorderOutdent_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents)
                return;

            _editorUI?.Window?.BeginInvoke((Action)AutoLayoutLineBorders);
        }
        private void NumGrayLimit_ValueChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents) return;

            bool autoRestore = !chkAlwaysShowFilterResult.Checked;
            ApplyLineBorderFilters(autoRestore);
        }
        private void ChkShowFilterResult_CheckedChanged(object sender, EventArgs e)
        {
            if(_bypassWindowEvents) return;

            if (chkAlwaysShowFilterResult.Checked)
                ApplyLineBorderFilters(autoRestore: false);
            else
                startRestoreRegionViewTimer(1);
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;

            if (propertyName == "xAlgorithm")
            {
                this.UpdateAlgorithmStatus();
            }

            if (_isEditting)
            {
                if (propertyName == "xCarrierBackground")
                {
                    //自動刷新 邊線 抓取結果
                    this.UpdateLineSegmentBoxes(true);
                    refreshViewer(wndRegionViewer);
                }
            }
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
                    _gapBordersCtrl.IsEditting = _isEditting && _isGapBorderBoxActived;
                    updateGuiStatus();
                }
            }
        }
        internal bool IsModified
        {
            get
            {
                return _isModified || _gapBordersCtrl.IsModified;
            }
            set
            {
                _isModified = value;
                _gapBordersCtrl.IsModified = value;
            }
        }
        internal void PostInit()
        {
            persistLineBorderIndentExt(false);
            updateLineBorderBoxesNumber(false);
            updateLineBorderBoxes(false);
            updateGuiStatus();
            _gapBordersCtrl.PostInit();
        }
        internal void UpdateLineSegmentBoxes(bool show)
        {
            bool showLB = show && !_isGapBorderBoxActived;
            bool showGap = show && _isGapBorderBoxActived;
            updateLineSegmentBoxes(showLB, false);
            _gapBordersCtrl.UpdateLineSegmentBoxes(showGap);
        }

        void SwitchToGapBorderOpMode(bool toGapBorderOpMode)
        {
            _isGapBorderBoxActived = toGapBorderOpMode;
            _gapBordersCtrl.IsEditting = toGapBorderOpMode && _isEditting;
            _gapBordersCtrl.UpdateLineSegmentBoxes(toGapBorderOpMode && _isEditting);
            updateLineSegmentBoxes(!toGapBorderOpMode && _isEditting);
            updateGuiStatus();
            refreshViewer(wndRegionViewer);
        }

        void AutoLayoutLineBorders()
        {
            if (_xAlgorithm == MatchAlgorithmEnum.GridMatch)
                autoLayoutLineBorders_for_GridPad_Chips();
            else
                autoLayoutLineBorders_for_General_Chips();
        }
        void autoLayoutLineBorders_for_GridPad_Chips()
        {
            if (_xAlgorithm != MatchAlgorithmEnum.GridMatch)
                return;

            //(1) 強制 重新抓取 goldenPadsQuad
            var goldenPadsQuad = requestToLocateGoldenQuad(true);

            int indent = updateNumBorderIndentDynamically();
            int outdent = (int)numBorderOutdent.Value;
            var spanRatio = (double)numLineSpanPercentage.Value * 0.01;

            var baseRect = Rectangle.Round(_xRecipe.GoldenChipRect);
            var W = baseRect.Width;
            var H = baseRect.Height;
            var ww = (int)(baseRect.Width * spanRatio);
            var hh = (int)(baseRect.Height * spanRatio);
            var dw = W - ww;
            var dh = H - hh;

            //(2) 計算 rectL, rectT, rectR, rectB
            Rectangle rectL;
            Rectangle rectT;
            Rectangle rectR;
            Rectangle rectB;

            if (goldenPadsQuad != null)
            {
                // ind 固定在 baseRect 與 quadRect 中間處
                var quadRect = Rectangle.Round(goldenPadsQuad.BoundaryRect);
                var inX = (quadRect.X + baseRect.X) / 2;
                var inY = (quadRect.Y + baseRect.Y) / 2;
                var inX2 = (quadRect.Right + baseRect.Right) / 2;
                var inY2 = (quadRect.Bottom + baseRect.Bottom) / 2;
                var xL = baseRect.X;
                var xR = baseRect.Right;
                var indL = Math.Abs(inX - xL);
                var indR = Math.Abs(inX2 - xR);
                var yT = baseRect.Y;
                var yB = baseRect.Bottom;
                var indT = Math.Abs(inY - yT);
                var indB = Math.Abs(inY2 - yB);

                rectL = new Rectangle(xL - outdent, yT + dh / 2, indL + outdent, hh);   //Left
                rectT = new Rectangle(xL + dw / 2, yT - outdent, ww, indT + outdent);   //Top
                rectR = new Rectangle(xR - indR, yT + dh / 2, indR + outdent, hh);      //Right
                rectB = new Rectangle(xL + dw / 2, yB - indB, ww, indB + outdent);      //Bottom

                updateNumBorderIndentDynamically((indL + indR + indT + indB) / 4);
            }
            else
            {
                var x = baseRect.X;
                var y = baseRect.Y;

                rectL = new Rectangle(x - outdent, y + dh / 2, indent + outdent, hh);               //Left
                rectT = new Rectangle(x + dw / 2, y - outdent, ww, indent + outdent);               //Top
                rectR = new Rectangle(baseRect.Right - indent, y + dh / 2, indent + outdent, hh);   //Right
                rectB = new Rectangle(x + dw / 2, baseRect.Bottom - indent, ww, indent + outdent);  //Bottom

                updateNumBorderIndentDynamically();
            }

            //(3) 更新 Recipe 的 LineBorderParams.LineBorderPairs
            var rcpLineBorderPairs = _xRecipe.LineBorderParams.LineBorderPairs;
            rcpLineBorderPairs.AdjustPairsNumber(1, 1);

            if (!rcpLineBorderPairs.TryGetValue("X", out var pairX)) 
                rcpLineBorderPairs["X"] = pairX = new LineBorderPair(rcpLineBorderPairs.IsLocal);

            if (!rcpLineBorderPairs.TryGetValue("Y", out var pairY))
                rcpLineBorderPairs["Y"] = pairY = new LineBorderPair(rcpLineBorderPairs.IsLocal);

            pairX.Borders[0] = QvQuad2D.From(rectL).ToBox2D();
            pairX.Borders[1] = QvQuad2D.From(rectR).ToBox2D();
            pairY.Borders[0] = QvQuad2D.From(rectT).ToBox2D();
            pairY.Borders[1] = QvQuad2D.From(rectB).ToBox2D();

            updateLineBorderBoxes(false);
            updateLineSegmentBoxes(true);
            refreshViewer(wndRegionViewer);

            _isModified = true;
        }
        void autoLayoutLineBorders_for_General_Chips()
        {
            //(0) indent, outdent, spanRatio
            int indent = updateNumBorderIndentDynamically();
            int outdent = (int)numBorderOutdent.Value;
            double spanRatio = (double)numLineSpanPercentage.Value * 0.01;

            //(1) 強制 重新抓取 goldenQuad
            var goldenQuad = requestToLocateGoldenQuad(true);
            if (goldenQuad == null)
            {
                goldenQuad = QvQuad2D.From(_xRecipe.GoldenChipRect);
                updateNumBorderIndentDynamically();
            }

            //(2) 分類所有 Boxes
            int N;
            var boxesL = new List<CviLineBorderBox>();  // LEFT
            var boxesR = new List<CviLineBorderBox>();  // RIGHT
            var boxesT = new List<CviLineBorderBox>();  // TOP
            var boxesB = new List<CviLineBorderBox>();  // BOTTOM
            foreach (var cviBox in _cviLineBorderBoxes)
            {
                if (cviBox.Visible && cviBox.Tag is Tuple<string, int> tag)
                {
                    string keyName = tag.Item1;
                    int borderIdx = tag.Item2;
                    if(keyName.StartsWith("X"))
                    {
                        if (borderIdx == 0) 
                            boxesL.Add(cviBox);
                        else 
                            boxesR.Add(cviBox);
                    }
                    else
                    {
                        if (borderIdx == 0)
                            boxesT.Add(cviBox);
                        else
                            boxesB.Add(cviBox);
                    }
                }
            }

            //(3.1) 自動布局: 左邊
            var boxes = boxesL;
            if ((N = boxes.Count) > 0)
            {
                boxesL.Sort((b1, b2) => b1.Text.Length - b2.Text.Length);

                var gapRatio = (1.0 - spanRatio) / 2.0;
                var remainRatio = 1.0 - gapRatio * (N + 1);
                var boxRatio = remainRatio / N;

                var pBegin = goldenQuad.Corners[0];
                var pEnd = goldenQuad.Corners[3];
                var vect = pEnd - pBegin;
                var len = vect.NormLength;
                vect /= len;

                var pAnchor = pBegin + vect * (len * gapRatio);
                for (int i = 0; i < N; i++)
                {
                    var x = (int)Math.Round(pAnchor.X - outdent);
                    var y = (int)Math.Round(pAnchor.Y);
                    var w = (int)(outdent - indent);
                    var h = (int)Math.Round(len * boxRatio);
                    boxes[i].Box = new Rectangle(x, y, w, h); 
                    pAnchor += vect * (len * gapRatio + len * boxRatio);
                }
            }

            //(3.2) 自動布局: 右邊
            boxes = boxesR;
            if ((N = boxes.Count) > 0)
            {
                boxesL.Sort((b1, b2) => b1.Text.Length - b2.Text.Length);

                var gapRatio = (1.0 - spanRatio) / 2.0;
                var remainRatio = 1.0 - gapRatio * (N + 1);
                var boxRatio = remainRatio / N;

                var pBegin = goldenQuad.Corners[1];
                var pEnd = goldenQuad.Corners[2];
                var vect = pEnd - pBegin;
                var len = vect.NormLength;
                vect /= len;

                var pAnchor = pBegin + vect * (len * gapRatio);
                for (int i = 0; i < N; i++)
                {
                    var x = (int)Math.Round(pAnchor.X + indent);
                    var y = (int)Math.Round(pAnchor.Y);
                    var w = (int)(outdent - indent);
                    var h = (int)Math.Round(len * boxRatio);
                    boxes[i].Box = new Rectangle(x, y, w, h);
                    pAnchor += vect * (len * gapRatio + len * boxRatio);
                }
            }

            //(3.3) 自動布局: 上邊
            boxes = boxesT;
            if ((N = boxes.Count) > 0)
            {
                boxesL.Sort((b1, b2) => b1.Text.Length - b2.Text.Length);

                var gapRatio = (1.0 - spanRatio) / 2.0;
                var remainRatio = 1.0 - gapRatio * (N + 1);
                var boxRatio = remainRatio / N;

                var pBegin = goldenQuad.Corners[0];
                var pEnd = goldenQuad.Corners[1];
                var vect = pEnd - pBegin;
                var len = vect.NormLength;
                vect /= len;

                var pAnchor = pBegin + vect * (len * gapRatio);
                for (int i = 0; i < N; i++)
                {
                    var x = (int)Math.Round(pAnchor.X);
                    var y = (int)Math.Round(pAnchor.Y - outdent);
                    var h = (int)(outdent - indent);
                    var w = (int)Math.Round(len * boxRatio);
                    boxes[i].Box = new Rectangle(x, y, w, h);
                    pAnchor += vect * (len * gapRatio + len * boxRatio);
                }
            }

            //(3.4) 自動布局: 下邊
            boxes = boxesB;
            if ((N = boxes.Count) > 0)
            {
                boxesL.Sort((b1, b2) => b1.Text.Length - b2.Text.Length);

                var gapRatio = (1.0 - spanRatio) / 2.0;
                var remainRatio = 1.0 - gapRatio * (N + 1);
                var boxRatio = remainRatio / N;

                var pBegin = goldenQuad.Corners[3];
                var pEnd = goldenQuad.Corners[2];
                var vect = pEnd - pBegin;
                var len = vect.NormLength;
                vect /= len;

                var pAnchor = pBegin + vect * (len * gapRatio);
                for (int i = 0; i < N; i++)
                {
                    var x = (int)Math.Round(pAnchor.X);
                    var y = (int)Math.Round(pAnchor.Y + indent);
                    var h = (int)(outdent - indent);
                    var w = (int)Math.Round(len * boxRatio);
                    boxes[i].Box = new Rectangle(x, y, w, h);
                    pAnchor += vect * (len * gapRatio + len * boxRatio);
                }
            }

            refreshViewer(wndRegionViewer);
            updateLineBorderBoxes(true);
            updateLineSegmentBoxes(true);

            _isModified = true;
        }

        void BuildMicroTransform()
        {
            //if (DialogResult.Yes != MessageBox.Show("是否要重新設定 樣本尺寸?", "參數設定", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            //    return;

            if (VsMessageBox.Question(QMSG.Text(Prompts.Question_To_Rebuild_Template_Dimension)) != DialogResult.Yes)
                return;


            //_isPropertyModified = true;

            updateLineSegmentBoxes(true, calcGoldenDim: true);
            
            OnMicroTransformChanged?.Invoke(this, null);
        }
        void UpdateAlgorithmStatus()
        {
            updateNumBorderIndentDynamically();
            updateLineBorderAlgorithmStatus();
        }
        void ApplyLineBorderFilters(bool autoRestore = true)
        {
            var aoi = _sysModel?.AoiModel?.GetChipMeasureAoi();
            if (aoi == null)
                return;

            updateFilterSettings(true);

            if (aoi.TryApplyLineFilters(_xRecipe.GoldenRegionCellBmp, out Bitmap bmpFilter))
            {
                wndRegionViewer.UpdateImage(bmpFilter, "Region View (Filtered)", true);

                if(autoRestore)
                    startRestoreRegionViewTimer(5000);
            }
            else
            {
                if (autoRestore)
                    startRestoreRegionViewTimer(1);
            }
        }

        #region PRIVATE_FUNCTIONS
        QvQuad2D requestToLocateGoldenQuad(bool force)
        {
#if (OPT_MOVED_TO_EXTERNAL)
            //-----------------------------------------------------------------------------------
            // REV_2026 - 03 - 09 整合海康 Template Match
            //-----------------------------------------------------------------------------------
            if (_xBmpGoldenChipTemplate == null || _xGoldenChipRect == RectangleF.Empty)
            {
                //VsMessageBox.Warning("請先設定 晶粒匹配樣本!");
                var errMsg = GaUtil.GetEnumDescription(ErrorCodes.WARN_NO_GOLDN_TEMPLATE_SETUP);
                VsMessageBox.Warning(errMsg);
                return;
            }

            if (_cviGoldenChipBox.Box != Rectangle.Round(_xGoldenChipRect))
            {
                // 重新 擷取 Golden Chip
                BuildGoldenChipTemplate();
            }
#endif

#if (OPT_MOVED_TO_EXTERNAL_II)
            // 強制 重新抓取 goldenQuad2D
            if (true)  // if( _goldenQuad2D == null)
            {
                try
                {
                    //var matcherComposite = _xRecipe.mvdprinttemp_Find;
                    var matcherComposite = _sysModel?.AoiModel?.GetChipLocAoi()?.GetTemplateMatcher();

                    matcherComposite.SetRecipeParams(_xRecipe.InspectParams);   //<<< 使用 matcherComposite.SetRecipeParams 才能反映 _xAlogrithm

                    var matcher = matcherComposite.GetMatcher(0);
                    matcher.Train(_xBmpGoldenChipTemplate);

                    _goldenQuad2D = matcher.GoldenQuad2D?.Clone();
                    _goldenQuad2D?.Offset(_xGoldenChipRect.X, _xGoldenChipRect.Y);
                }
                catch (Exception ex)
                {
                    var errMsg = GaUtil.GetEnumDescription(ErrorCodes.WARN_CAN_NOT_FETCH_QUAD_2D) + "\n\r\n\r" + ex.Message;
                    VsMessageBox.Warning(errMsg);
                    return;
                }
            }
#endif

            QvQuad2D goldenQuad = null;

            if (OnRequestToLocateGoldenQuad != null)
            {
                var e = new DoWorkEventArgs(force);
                OnRequestToLocateGoldenQuad(this, e);
                goldenQuad = (QvQuad2D)e.Result;
            }

            return goldenQuad;
        }
        void startRestoreRegionViewTimer(int delay = 5000)
        {
            if (_restoreRegionViewTimer == null)
            {
                _restoreRegionViewTimer = new Timer();
                _restoreRegionViewTimer.Tick += (s, e) =>
                {
                    _restoreRegionViewTimer.Stop();
                    if(!chkAlwaysShowFilterResult.Checked)
                        wndRegionViewer?.UpdateImage(_xRecipe.GoldenRegionCellBmp, "Region View", false);
                };
            }
            _restoreRegionViewTimer?.Stop();
            _restoreRegionViewTimer.Interval = delay;
            _restoreRegionViewTimer?.Start();
        }
        void disposeRestoreRegionViewTimer()
        {
            _restoreRegionViewTimer?.Stop();
            _restoreRegionViewTimer?.Dispose();
            _restoreRegionViewTimer = null;
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void refreshViewer(Control wnd)
        {
            if (wnd is JezTransImageViewPanel panel)
                panel.MatViewer.Invalidate();
            else
                wnd?.Invalidate();
        }

        int updateNumBorderIndentDynamically(int? indent = null)
        {
            //----------------------------------------------
            // 根據晶粒型態動態配置 內緣 NumericUpDown
            //----------------------------------------------
            if (numBorderIndent == null)
                return 0;

            _bypassWindowEvents = true;

            if (_xAlgorithm == MatchAlgorithmEnum.GridMatch)
            {
                // 格點型 晶粒
                numBorderIndent.Minimum = 1m;
                numBorderIndent.Maximum = 100m;
                GaUtil.SetNum(numBorderIndent, 1);
                numBorderIndent.Enabled = false;
            }
            else
            {
                // 一般型 晶粒 (使用海康 template match)
                decimal value = indent != null ? indent.Value : numBorderIndent.Value;
                numBorderIndent.Minimum = -32m;
                numBorderIndent.Maximum = 100m;
                if (value >= 0)
                    value = numBorderIndent.Minimum / 2m;
                GaUtil.SetNum(numBorderIndent, value);
                numBorderIndent.Enabled = true;
            }

            _bypassWindowEvents = false;

            return (int)numBorderIndent.Value;
        }
        void persistLineBorderIndentExt(bool save)
        {
            try
            {
                var settings = Properties.Settings.Default;
                if (save)
                {
                    //>>> settings.lineBorderIndent = (int)_editorUI.numBorderIndent.Value;
                    settings.lineBorderIndent = updateNumBorderIndentDynamically();
                    settings.lineBorderExt = (int)numBorderOutdent.Value;
                    settings.lineSpanPercentage = (float)numLineSpanPercentage.Value;
                    settings.Save();
                }
                else
                {
                    _bypassWindowEvents = true;
                    //>>> GaUtil.SetNum(_editorUI.numBorderIndent, settings.lineBorderIndent);
                    GaUtil.SetNum(numBorderOutdent, settings.lineBorderExt);
                    GaUtil.SetNum(numLineSpanPercentage, (decimal)settings.lineSpanPercentage);
                    _bypassWindowEvents = false;

                    updateNumBorderIndentDynamically(settings.lineBorderIndent);
                }
            }
            catch
            {
            }
        }

#if (OPT_OLD_LINE_BORDER_FUNCTIONS)
        void _updateLineBorderBoxes_000(bool toRecipe)
        {
            if (toRecipe)
            {
                int i = 0;
                _xRecipe.xLineLeft = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineTop = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineRight = _cviLineBorderBoxes[i++].Box;
                _xRecipe.xLineBottom = _cviLineBorderBoxes[i++].Box;
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
        void _updateLineSegmentBoxes_000(bool show, bool calcGoldenDim = false)
        {
            if (!show)
            {
                foreach (var lineSegBox in _cviLineSegmentBoxes)
                    lineSegBox.Visible = false;
            }
            else
            {
                var bmpSrcRegion = _xRecipe.GoldenRegionCellBmp;
                var edgeLines = new List<EzLSD.LineSegment>();

                foreach (EdgeBorder eBorder in Enum.GetValues(typeof(EdgeBorder)))
                {
                    int idx = (int)eBorder;
                    var borderRect = _cviLineBorderBoxes[idx].Box;

                    bool ok = aoiTryRunFindLineSegment(eBorder, bmpSrcRegion, borderRect, out var mvdLines);
                    var linesOut = GaMvdExt.ToCSharpLines(mvdLines);
                    _cviLineSegmentBoxes[idx].Attach(linesOut);
                    _cviLineSegmentBoxes[idx].Visible = ok;

                    if (mvdLines.Length > 0)
                        edgeLines.Add(mvdLines[0]?.ToLineSegment());
                    else
                        edgeLines.Add(null);
                }

                if (calcGoldenDim)
                {
                    // NOTE: 這裡會影響 PAD型晶粒的 Golden Template "GRID" !!! 
                    aoiCalcGoldenChipDimension_000(edgeLines);
                }
            }
        }
#endif

        void updateLineBorderAlgorithmStatus()
        {
            // 格點型晶粒 : 只能固定使用 numX = 1, numY = 1
            if (_xAlgorithm == MatchAlgorithmEnum.GridMatch)
            {
                if (numMeasureDistXs.Value != 1 || numMeasureDistYs.Value != 1)
                {
                    _editorUI.Window.BeginInvoke(new Action(() =>
                    {
                        if (numMeasureDistXs.Value != 1)
                        {
                            numMeasureDistXs.Enabled = true;
                            numMeasureDistXs.Value = 1;
                        }
                        if (numMeasureDistYs.Value != 1)
                        {
                            numMeasureDistYs.Enabled = true;
                            numMeasureDistYs.Value = 1;
                        }
                    }));
                }
            }
            else
            {
                updateGuiStatus();
            }
        }
        void updateLineBorderBoxesNumber(bool toRecipe)
        {
            if (_xLineBorderPairs == null)
                return;

            if (toRecipe)
            {
                int numX = (int)numMeasureDistXs.Value;
                int numY = (int)numMeasureDistYs.Value;
                if (_xLineBorderPairs.AdjustPairsNumber(numX, numY))
                {
                    _isModified = true;
                    updateLineBorderBoxes(false);
                    updateCviBoxesStatus();
                    refreshViewer(wndRegionViewer);
                }
            }
            else
            {
                _xLineBorderPairs.GetPairsNumbers(out int numX, out int numY);
                _bypassWindowEvents = true;
                GaUtil.SetNum(numMeasureDistXs, numX);
                GaUtil.SetNum(numMeasureDistYs, numY);
                _bypassWindowEvents = false;
            }
        }
        void updateLineBorderBoxes(bool toRecipe)
        {
            if (_xLineBorderPairs == null)
                return;

            if (toRecipe)
            {
                int idx = 0;
                foreach ((var keyName, var pair) in _xLineBorderPairs.IterPairs())
                {
                    if (pair == null)
                        continue;

                    for (int ib = 0; ib < 2; ib++)
                    {
                        if (idx >= _cviLineBorderBoxes.Length)
                            break;

                        var cviBox = _cviLineBorderBoxes[idx];
                        pair.Borders[ib] = QvQuad2D.From(cviBox.Box).ToBox2D();
                        idx++;
                    }

                    _isModified = true;
                }
            }
            else
            {
                int idx = 0;
                foreach ((var keyName, var pair) in _xLineBorderPairs.IterPairs())
                {
                    if (pair == null)
                        continue;

                    for (int ib = 0; ib < 2; ib++)
                    {
                        if (idx >= _cviLineBorderBoxes.Length)
                            break;

                        var border = pair.Borders[ib];
                        if (border == null)
                            border = pair.Borders[ib] = QvQuad2D.From(RectangleF.Empty).ToBox2D();

                        var rect = Rectangle.Round(border.BoundaryRect);
                        if (rect == Rectangle.Empty)
                        {
                            rect = new Rectangle(50 * idx, 50 * idx, 100, 100);
                            pair.Borders[ib] = QvQuad2D.From(rect).ToBox2D();
                        }

                        var cviBox = _cviLineBorderBoxes[idx];
                        cviBox.BoxBrush = getStockBrush(keyName);
                        cviBox.Text = keyName + LineBorderPair.GetPostfix(keyName, ib);
                        cviBox.Box = rect;
                        cviBox.Tag = new Tuple<string, int>(keyName, ib);

                        idx++;
                    }
                }

                _actualLineBordersCount = idx;
            }
        }
        void updateLineSegmentBoxes(bool show, bool calcGoldenDim = false)
        {
            if (_isGapBorderBoxActived)
                show = false;

            if (!show)
            {
                foreach (var lineSegBox in _cviLineSegmentBoxes)
                    lineSegBox.Visible = false;
            }
            else
            {
                if (_xLineBorderPairs != null)
                {
                    // 實時抓取邊線, 並更新到 _xLineBorderPairs 對應欄位
                    fetchLineSegments(show);

                    if (calcGoldenDim)
                    {
                        // 設定 量測尺寸的目標值 並 建立微距轉換系統
                        // 注意: _xLineBorderPairs 在參數檔內為 Local Region Coordinates !
                        // 注意: 這裡會影響 PAD型晶粒的 Golden Template "GRID" !!! 
                        aoiCalcGoldenChipDimension(_xLineBorderPairs);
                    }

                    refreshViewer(wndRegionViewer);
                }
            }
        }
        void fetchLineSegments(bool show)
        {
            // 參數 (邊線手拉框)
            var xLineBorderPairs = _xLineBorderPairs;   
            if (xLineBorderPairs == null)
                return;

            // 重置 即將顯示抓到的線段
            foreach (var lineSegBox in _cviLineSegmentBoxes)
            {
                lineSegBox.Attach(null);
                lineSegBox.Visible = false;
            }

            // 根據每一個 _cviLineBorderBoxes 來抓取對應 邊線
            for (int index = 0, N = _cviLineBorderBoxes.Length; index < N; index++)
            {
                var cviBorderBox = _cviLineBorderBoxes[index];
                if (cviBorderBox.Tag is Tuple<string, int> tag)
                {
                    var keyName = tag.Item1;
                    var ib = tag.Item2;

                    if (!xLineBorderPairs.ContainsKey(keyName))
                        continue;

                    // 範圍框
                    var borderRect = cviBorderBox.Box;

                    // borderID
                    var eBorder = getBorderEnum(keyName, ib);

                    // 調用 aoi (海康) 來抓邊線
                    bool ok = aoiTryRunFindLineSegments(eBorder, _xRecipe.GoldenRegionCellBmp, borderRect, out var linesOut);

                    // 更新到 _cviLineSegmentBoxes
                    _cviLineSegmentBoxes[index].Attach(linesOut);
                    _cviLineSegmentBoxes[index].Visible = ok && show && !_isGapBorderBoxActived;

                    // 將抓到的 邊線 更新至 參數 xlineBorderPairs 的對應欄位
                    if (linesOut != null && linesOut.Length > 0)
                    {
                        if (!xLineBorderPairs.TryGetValue(keyName, out var rcpPair))
                        {
                            rcpPair = new LineBorderPair(xLineBorderPairs.IsLocal);
                            rcpPair.Borders[ib] = QvQuad2D.From(borderRect).ToBox2D();
                            xLineBorderPairs[keyName] = rcpPair;
                        }
                        rcpPair.LineSegments[ib] = new EzLSD.LineSegment(linesOut[0][0], linesOut[0][1]);
                    }
                }
            }
        }

        void updateFilterSettings(bool toRecipe)
        {
            if (toRecipe)
            {
                _xInspectParams.GrayLimitHi = (int)numGrayLimitHi.Value;
                _xInspectParams.GrayLimitLo = (int)numGrayLimitLo.Value;
                _isModified = true;
            }
            else
            {
                _bypassWindowEvents = true;
                GaUtil.SetNum(numGrayLimitHi, _xInspectParams.GrayLimitHi);
                GaUtil.SetNum(numGrayLimitLo, _xInspectParams.GrayLimitLo);
                _bypassWindowEvents = false;
            }
        }
        void updateCviBoxesStatus()
        {
            bool visible = _isEditting && !_isGapBorderBoxActived;

            int idx = 0;
            foreach (var cviBox in _cviLineBorderBoxes)
            {
                cviBox.Visible = visible && (idx < _actualLineBordersCount);
                idx++;
            }

            //bool includeLineSeg = false;
            //if (includeLineSeg)
            //{
            //    idx = 0;
            //    foreach (var cviBox in _cviLineSegmentBoxes)
            //    {
            //        cviBox.Visible = visible && (idx < _actualLineBordersCount);
            //        idx++;
            //    }
            //}
        }
        void updateGuiStatus()
        {
            // Line Border Gui
            updateNumBorderIndentDynamically();

            bool isGridMatch = _xAlgorithm == MatchAlgorithmEnum.GridMatch;

            numMeasureDistXs.Enabled = _isEditting && !isGridMatch;
            numMeasureDistYs.Enabled = _isEditting && !isGridMatch;
            numMeasureMasks.Enabled = _isEditting && !isGridMatch && false;

            numBorderIndent.Enabled = false;
            numBorderOutdent.Enabled = _isEditting;
            numLineSpanPercentage.Enabled = _isEditting;

            btnAutoLayoutLineBorders.Enabled = _isEditting;
            btnBuildMictroTransform.Enabled = _isEditting;

            updateCviBoxesStatus();
        }
        #endregion

        #region AOI_MODEL_FUNCTIONS
        EdgeBorder getBorderEnum(string keyName, int idx)
        {
            if(keyName.StartsWith("X"))
                return idx==0 ? EdgeBorder.Left : EdgeBorder.Right;
            else
                return idx==0 ? EdgeBorder.Top : EdgeBorder.Bottom;
        }

        /// <summary>
        /// 嘗試尋找 LineSegments
        /// (LineSegments 單位 pixels, 必須是 FullFov Camera Coordinates)
        /// </summary>
        /// <remarks>
        /// 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        /// </remarks>
        bool aoiTryRunFindLineSegments(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF boxRect, out PointF[][] resultLines)
        {
#if (OPT_LEGACY_000)
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

#if (OPT_LEGACY_001)
            // 目前邊線 用於 黑色背景 比較準確
            using (MvdFindLineClass lineSegFinder = new MvdFindLineClass())
            {
                lineSegFinder.Background = _xInspectX3.xCarrierBackground;
                var mvdLine = lineSegFinder.Run(bmpSrc, boxRect, (int)eBorder);
                resultLines = new[] { mvdLine };
                return mvdLine != null;
            }
#endif
            resultLines = null;

            var aoi = _sysModel?.AoiModel?.GetChipMeasureAoi();
            if (aoi == null)
                return false;

            aoi.TryFindLineSegment(eBorder, bmpSrc, boxRect, out var mvdLine);

            if (mvdLine != null)
            {
                // 轉換 mveLines to CSharp Lines
                resultLines = GaMvdExt.ToCSharpLines(new[] { mvdLine });
            }

            return resultLines != null;
        }

        /// <summary>
        /// 設定 量測尺寸的目標值 並 建立微距轉換系統
        /// </summary>
        /// <remarks>
        /// 注意: _xLineBorderPairs 在參數檔內 可能為 Local Region Coordinates ! <br/>
        /// 注意: 這裡會影響 PAD型晶粒的 Golden Template "GRID" !!!  
        /// </remarks>
        void aoiCalcGoldenChipDimension(LineBorderPairsCollection lineBorderPairs)
        {
            //(0) AoiModel
            var aoiModel = _sysModel?.AoiModel;

            if (aoiModel != null && lineBorderPairs != null)
            {
                //(1) Region Roi 與 Region Bmp
                var regionRoi = _xRecipe.GoldenRegionCellRect;
                var regionBmp = _xRecipe.GoldenRegionCellBmp;
                if (regionBmp == null)
                    return;

                //(2) 設定 目標尺寸
                var targetW = _xInspectParams.xTemplateChipWidth;
                var targetH = _xInspectParams.xTemplateChipHeight;
                var targetDim = new SizeF(targetW, targetH);
                lineBorderPairs.SetTargetDists(targetDim);
  
                ErrorCodes err;

                //(3) 是否 使用 簡單四邊線
                if (_xRecipe.LineBorderParams.IsSimpleQuad)
                {
                    //(3.1) 取得 4邊線
                    var edgeLines4 = lineBorderPairs.GetQuadLineSegments();
                    
                    //(3.2) 使用 原有 微距轉換系統 的建構方式
                    err = aoiModel.BuildMicroChipTransform(targetDim, edgeLines4, regionBmp, regionRoi, lineBorderPairs.IsLocal);
                }
                else
                {
                    //(3.3) 使用 新的 微距轉換系統 的建構方式
                    err = aoiModel.BuildMicroChipTransform(lineBorderPairs, regionBmp, regionRoi);
                }

                //(4) 顯示訊息
                if (err != Model.ErrorCodes.OK)
                {
                    string errMsg = "無法建立 Micro Transform:\n\r\n\r" + GaUtil.GetEnumDescription(err);
                    VsMessageBox.Warning(errMsg);
                }
                else
                {
                    string msg = "成功 設定樣本尺寸 並 建立 Micro Transform!";
                    VsMessageBox.Info(msg);
                }
            }
        }

        /// <summary>
        /// 計算所有 量測尺寸 的 理想預期值
        /// </summary>
        /// <remarks>
        /// 注意: 這裡會影響 PAD型晶粒的 Golden Template "GRID" !!! 
        /// </remarks>
        void aoiCalcGoldenChipDimension_000(List<EzLSD.LineSegment> lines)
        {
#if (OPT_OLD)
            var aoiModel = _sysModel?.AoiModel;
            if (aoiModel != null && lines != null)
            {
                var regionBmp = _xRecipe.GoldenRegionCellBmp;
                if (regionBmp == null)
                    return;

                var regionRoi = _xRecipe.xRectRegionPrint;
                foreach (var line in lines)
                    line?.Offset(regionRoi.X, regionRoi.Y);

                var targetSize = new SizeF(_xInspectX3.xTemplateChipWidth, _xInspectX3.xTemplateChipHeight);
                var err = aoiModel.BuildMicroChipTransform(targetSize, lines.ToArray(), regionBmp, regionRoi);

                if (err != Model.ErrorCodes.OK)
                {
                    string errMsg = "無法建立 Micro Transform:\n\r\n\r" + GaUtil.GetEnumDescription(err);
                    VsMessageBox.Warning(errMsg);
                }
                else
                {
                    string msg = "成功 設定樣本尺寸 並 建立 Micro Transform!";
                    VsMessageBox.Info(msg);
                }
            }
#endif
        }
        #endregion
    }
}
