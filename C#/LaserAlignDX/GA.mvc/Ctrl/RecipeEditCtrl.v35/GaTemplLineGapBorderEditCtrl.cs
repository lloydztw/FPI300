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
using JetEazy.QvMath;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.Mvc.Model.Recipe;
using LeTian.AoiLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CviGapBorderBox = LaserAlignDX.Mvc.Gui.CviLineBorderBox;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public class GaTemplLineGapBorderEditCtrl : GaRcpBaseCtrl
    {
        #region RECIPE_PARAMS
        MatchAlgorithmEnum _xAlgorithm
        {
            get => _xRecipe.InspectParams.xAlgorithm;
        }
        DtoX3LineGapBorderParams _xGapBorderParams
        {
            get => _xRecipe.GapBorderParams;
        }
        LineBorderPairsCollection _xGapBorderPairs
        {
            get => _xRecipe.GapBorderParams;
        }
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        IvTemplLineGapBordersEditorUI _gapBordersEditorUI => _editorUI;
        JezTransImageViewPanel wndRegionViewer => _editorUI.ImgViewers[0] as JezTransImageViewPanel;
        CheckBox chkUseAveGap4 => _gapBordersEditorUI.chkUseAveGaps4;
        Button btnAutoLayoutGapBorders => _gapBordersEditorUI.btnAutoGapBorders;
        #endregion

        #region GUI_DRAWING_OBJECTS
        bool isX(GapEnum gap)
        {
            switch (gap)
            {
                case GapEnum.LUX:
                case GapEnum.RUX:
                case GapEnum.RDX:
                case GapEnum.LDX:
                    return true;
                default:
                    return false;
            }
        }
        Brush getStockBrush(GapEnum gap)
        {
            return isX(gap) ? Brushes.Magenta : Brushes.Purple;
        }
        EdgeBorder getBorderID(GapEnum gap)
        {
            switch (gap)
            {
                case GapEnum.LUX:
                case GapEnum.LDX:
                    return EdgeBorder.Left;
                case GapEnum.RUX:
                case GapEnum.RDX:
                    return EdgeBorder.Right;
                case GapEnum.LUY:
                case GapEnum.RUY:
                    return EdgeBorder.Top;
                case GapEnum.LDY:
                case GapEnum.RDY:
                default:
                    return EdgeBorder.Bottom;
            }
        }
        string getDisplayText(GapEnum gap)
        {
            return $"{gap}{LineBorderPair.GetPostfix(gap)}";
        }
        #endregion

        #region INTERACTORS
        int MAX_GAP_BORDER_NUMBER => 8;
        CviGapBorderBox[] _cviGapBorderBoxes;
        CviLineSegmentsBox[] _cviGapLineSegmentBoxes;
        #endregion

        #region RUNTIME_DATA
        bool _bypassWindowEvents = false;
        bool _isEditting = false;
        bool _isModified = false;
        #endregion

        public EventHandler<DoWorkEventArgs> OnRequestToLocateGoldenQuad;
        public EventHandler OnMicroTransformChanged;

        public void Attach(IvTemplateEditorUI ui)
        {
            _editorUI = ui;
            initGui();
            connectEventHandlers();
        }

        #region PRIVATE_INIT_FUNCTIONS
        void initGui()
        {
            //numMeasureDistXs.Maximum = MAX_GAP_BORDER_NUMBER;
            //numMeasureDistYs.Maximum = MAX_GAP_BORDER_NUMBER;
            //numMeasureMasks.Visible = true;
            initInteractors();
        }
        void initInteractors()
        {
            int N = Enum.GetValues(typeof(GapEnum)).Length;

            _cviGapBorderBoxes = new CviGapBorderBox[N];
            _cviGapLineSegmentBoxes = new CviLineSegmentsBox[N];

            for (int i = 0; i < N; i++)
            {
                var brush = getStockBrush((GapEnum)i);
                _cviGapBorderBoxes[i] = new CviGapBorderBox(brush, 1, 3) { Visible = false, Text = getDisplayText((GapEnum)i) };
                _cviGapLineSegmentBoxes[i] = new CviLineSegmentsBox(Color.Cyan) { Visible = false };
                _cviGapLineSegmentBoxes[i].EnableMidPoint(Brushes.DarkCyan, 5);
            }

            var viewer = wndRegionViewer?.ImgViewer;
            if (viewer != null)
            {
                foreach (var box in _cviGapBorderBoxes)
                    viewer.AddInteractor(box);
                foreach (var box in _cviGapLineSegmentBoxes)
                    viewer.AddInteractor(box);
            }
        }
        void connectEventHandlers()
        {
            chkUseAveGap4.CheckedChanged += ChkUseAveGap4_CheckedChanged;
            btnAutoLayoutGapBorders.Click += (s, e) => AutoLayoutGapBorders();

            foreach (var cviBox in _cviGapBorderBoxes)
            {
                cviBox.OnChanged += (s, e) =>
                {
                    updateGapBorderBoxes(true);
                    updateLineSegmentBoxes(true);
                    _xGapBorderParams.UseDefaultBorders = false;
                };
            }

            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;
        }
        #endregion

        #region EVENT_HANDLERS
        private void ChkUseAveGap4_CheckedChanged(object sender, EventArgs e)
        {
            if (_bypassWindowEvents) return;
            updateMiscSettings(true);
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
            updateMiscSettings(false);
            updateGapBorderBoxes(false);
            updateGuiStatus();
        }
        internal void UpdateLineSegmentBoxes(bool show)
        {
            updateLineSegmentBoxes(show, false);
        }

        void AutoLayoutGapBorders()
        {
            try
            {
                if (_xAlgorithm != MatchAlgorithmEnum.GridMatch)
                    return;

                //(0) 清除
                _xGapBorderParams.GapBorderPairs.Clear();

                //(1) 強制 重新抓取 goldenPadsQuad (同時會重新抓 邊線)
                requestToLocateGoldenQuad(true);

                using (var cellBmp = (Bitmap)_xRecipe.GoldenRegionCellBmp.Clone())
                {
                    //(2) 晶粒定位 (FullFov Coorindates)
                    var cellRect = _xRecipe.GoldenRegionCellRect;
                    var aoi = GaMvcConfig.SysModel.AoiModel.GetChipLocAoi();
                    bool ok = aoi.LocateOneChip(cellBmp, ref cellRect, out GaChipData goldenChipData);
                    var goldenPadsGrid = goldenChipData?.PadsGrid;
                    if (!ok || goldenPadsGrid == null)
                    {
                        QMessageBox.Warning(ErrorCodes.ERR_NO_CHIP_PADS);
                        return;
                    }

                    //(3) edge Lines (Local Region Coordinates)
                    var edgeLines = _xRecipe.LineBorderParams.LineBorderPairs.GetQuadLineSegments(true);
                    //(3.1) To FullFov Coordinates
                    foreach (var line in edgeLines)
                        line.Offset(cellRect.X, cellRect.Y);

                    //(4) default gap measure points (FullFov Coordinates)
                    var err = goldenChipData.CalcGapMeasurePointPairs(edgeLines, out var gapMeasurePointPairs, out var cornerPadCenters);
                    if (err != ErrorCodes.OK)
                    {
                        QMessageBox.Warning(err);
                        return;
                    }

                    //(5) Reference Box and size
                    var padBoxG0 = QvQuad2D.From(goldenPadsGrid[0, 0].Rect).ToBox2D();
                    var sizeG = padBoxG0.MinAreaRectSize;
                    var sizeX = new SizeF(sizeG.Width / 2f, sizeG.Height);
                    var sizeY = new SizeF(sizeG.Width, sizeG.Height / 2f);

                    //(6) 轉換成 LUX, LUY, ... , LDX, LDY, 等 Borders
                    foreach (GapEnum gap in gapMeasurePointPairs.Keys)
                    {
                        //(6.1) pointPair
                        var pointsPair = gapMeasurePointPairs[gap];
                        var G = pointsPair[0];
                        var P = pointsPair[1];

                        //(6.2) To Local Coordinates
                        G.Offset(-cellRect.X, -cellRect.Y);
                        P.Offset(-cellRect.X, -cellRect.Y);

                        //(6.3) default border
                        var border = padBoxG0.Clone();
                        border.SetBox(PointF.Empty, isX(gap) ? sizeX : sizeY);
                        border.SetCenter((float)P.X, (float)P.Y);

                        //(6.4) 存入 Recipe
                        var xBorderPair = _xGapBorderParams[gap] = new LineBorderPair(true);
                        xBorderPair.Borders[0] = border;
                        xBorderPair.Borders[1] = border;
                    }
                }

                //(6) 標記為使用默認邊隙框
                _xGapBorderParams.UseDefaultBorders = true;

                //(7) 更新 GUI
                updateGapBorderBoxes(false);
                updateLineSegmentBoxes(true);
                refreshViewer(wndRegionViewer);

                //(8) 標記 modified
                _isModified = true;
            }
            catch(Exception ex)
            {
                var errMsg = "Error: " + ex.Message + "\n\r@" + ex.StackTrace;
                QMessageBox.Warning(errMsg);
            }
        }

        void UpdateAlgorithmStatus()
        {
            updateGuiStatus();
        }

        #region PRIVATE_FUNCTIONS
        QvQuad2D requestToLocateGoldenQuad(bool force)
        {
            QvQuad2D goldenQuad = null;

            if (OnRequestToLocateGoldenQuad != null)
            {
                var e = new DoWorkEventArgs(force);
                OnRequestToLocateGoldenQuad(this, e);
                goldenQuad = (QvQuad2D)e.Result;
            }

            return goldenQuad;
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
        void updateMiscSettings(bool toRecipe)
        {
            if (toRecipe)
            {
                _xGapBorderParams.UseAveGaps4 = chkUseAveGap4.Checked;
            }
            else
            {
                _bypassWindowEvents = true;
                chkUseAveGap4.Checked = _xGapBorderParams.UseAveGaps4;
                _bypassWindowEvents = false;
            }
        }
        void updateGapBorderBoxes(bool toRecipe)
        {
            var xGapBorderPairs = _xGapBorderPairs;
            if (xGapBorderPairs == null)
                return;

            if (toRecipe)
            {
                foreach (GapEnum gap in Enum.GetValues(typeof(GapEnum)))
                {
                    if (!xGapBorderPairs.TryGetValue(gap.ToString(), out var pair) || pair == null)
                        xGapBorderPairs[gap.ToString()] = pair = new LineBorderPair(true);

                    var cviBox = _cviGapBorderBoxes[(int)gap];
                    var border = QvQuad2D.From(cviBox.Box).ToBox2D();

                    // 只使用第一個
                    pair.Borders[0] = border;
                    pair.Borders[1] = border;

                    _isModified = true;
                }
            }
            else
            {
                foreach (GapEnum gap in Enum.GetValues(typeof(GapEnum)))
                {
                    int idx = (int)gap;
                    string keyName = gap.ToString();
                    if (!xGapBorderPairs.TryGetValue(keyName, out var pair) || pair == null)
                        continue;

                    // 只使用第一個
                    var border = pair.Borders[0];
                    if (border == null)
                        border = pair.Borders[0] = QvQuad2D.From(RectangleF.Empty).ToBox2D();

                    var rect = Rectangle.Round(border.BoundaryRect);
                    if (rect == Rectangle.Empty)
                    {
                        rect = new Rectangle(50 * idx, 50 * idx, 50, 50);
                        pair.Borders[0] = QvQuad2D.From(rect).ToBox2D();
                    }

                    var cviBox = _cviGapBorderBoxes[(int)gap];
                    cviBox.BoxBrush = getStockBrush(gap);
                    cviBox.Text = getDisplayText(gap);
                    cviBox.Box = rect;
                    //cviBox.Tag = new Tuple<string, int>(keyName, 0);
                }
            }
        }
        void updateLineSegmentBoxes(bool show, bool calcGoldenDim = false)
        {
            if (!show)
            {
                foreach (var lineSegBox in _cviGapLineSegmentBoxes)
                    lineSegBox.Visible = false;
            }
            else
            {
                var gapBorderPairs = _xGapBorderPairs;
                if (gapBorderPairs == null)
                    return;

                foreach(var lineSegBox in _cviGapLineSegmentBoxes)
                {
                    lineSegBox.Attach(null);
                    lineSegBox.Visible = false;
                }

                if (_xGapBorderPairs != null)
                {
                    fetchGapLineSegments(show);
                    refreshViewer(wndRegionViewer);
                }
            }
        }
        void fetchGapLineSegments(bool show)
        {
            // 參數
            var xGapBorderPairs = _xGapBorderPairs;    // _xRecipe?.LineBorderParams.LineBorderPairs;
            if (xGapBorderPairs == null)
                return;

            // 即將顯示抓到的線段
            foreach (var lineSegBox in _cviGapLineSegmentBoxes)
            {
                lineSegBox.Attach(null);
                lineSegBox.Visible = false;
            }

            // 根據每一個 borderBoxes 來抓取對應 Gap 邊線
            for (int idx = 0, N = _cviGapBorderBoxes.Length; idx < N; idx++)
            {
                // 範圍框
                var cviBorderBox = _cviGapBorderBoxes[idx];
                var borderRect = cviBorderBox.Box;

                // 調用 aoi (海康) 來抓邊線
                bool ok = aoiTryRunFindGapLineSegments((GapEnum)idx, _xRecipe.GoldenRegionCellBmp, borderRect, out var linesOut);

                // 更新到 _cviGapLineSegmentBoxes
                _cviGapLineSegmentBoxes[idx].Attach(linesOut);
                _cviGapLineSegmentBoxes[idx].Visible = ok && show;

                // 更新到 參數 xGapBorderPairs
                if (linesOut != null && linesOut.Length > 0)
                {
                    var keyName = ((GapEnum)idx).ToString();
                    if (!xGapBorderPairs.TryGetValue(keyName, out var xPair))
                    {
                        var border = QvQuad2D.From(borderRect).ToBox2D();
                        xGapBorderPairs[keyName] = xPair = new LineBorderPair(true);
                        xPair.Borders[0] = border;
                        xPair.Borders[1] = border;
                    }
                    xPair.LineSegments[0] = new EzLSD.LineSegment(linesOut[0][0], linesOut[0][1]);
                }
            }
        }
        void updateGuiStatus()
        {
            bool isGridMatch = _xAlgorithm == MatchAlgorithmEnum.GridMatch;
            chkUseAveGap4.Enabled = isGridMatch;
            btnAutoLayoutGapBorders.Enabled = isGridMatch;
            foreach (var cviBox in _cviGapBorderBoxes)
            {
                cviBox.Visible = _isEditting && isGridMatch;
            }
        }
        #endregion

        #region AOI_MODEL_FUNCTIONS
        /// <summary>
        /// 嘗試尋找 LineSegments (Fullfov Coordinates)
        /// </summary>
        bool aoiTryRunFindGapLineSegments(GapEnum gap, Bitmap bmpSrc, RectangleF boxRect, out PointF[][] resultLines)
        {
            resultLines = null;

            var aoi = _sysModel?.AoiModel?.GetChipMeasureAoi();
            if (aoi == null)
                return false;

            var backup = _xRecipe.InspectParams.xCarrierBackground;

            try
            {
                // 強制使用 深色背景 找 Pad Gap 邊線
                _xRecipe.InspectParams.xCarrierBackground = EdgeBackGroundType.Dark;

                EdgeBorder eBorder = getBorderID(gap);
                
                aoi.TryFindLineSegment(eBorder, bmpSrc, boxRect, out var mvdLine);

                if (mvdLine != null)
                {
                    // 轉換 mveLines to CSharp Lines
                    resultLines = GaMvdExt.ToCSharpLines(new[] { mvdLine });
                }
            }
            catch
            {
            }
            finally
            {
                // 還原 xCarrierBackground
                _xRecipe.InspectParams.xCarrierBackground = backup;
            }

            return resultLines != null;
        }
        #endregion
    }
}
