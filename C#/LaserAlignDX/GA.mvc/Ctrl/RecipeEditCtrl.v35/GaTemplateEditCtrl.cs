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
using JetEazy.OpenCV;
using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using CviGoldenBox = JetEazy.ImageViewerEx.Interactors.CviQuad;

namespace LaserAlignDX.Mvc.Ctrl.V35
{
    public class GaTemplateEditCtrl
    {
        #region CONSTS
        /// <summary>
        /// 海康 邊線自動框 最小內縮 
        /// </summary>
        const int MIN_INDENT_FOR_MVD = -32;
        #endregion

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

        #region CHILD_CTRL
        readonly GaTemplateLineBorderEditCtrl _lineBordersCtrl = new GaTemplateLineBorderEditCtrl();
        #endregion

        #region GLOBAL_MESS
        ITravelerModel _sysModel => GaMvcConfig.SysModel;
        CarrierEnum _carrierID => _sysModel.ActiveCarrierID;
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
        MatchAlgorithmEnum _xAlgorithm
        {
            get => _xInspectX3.xAlgorithm;
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
            get => _xRecipe.GoldenRegionCellBmp;
            set
            {
                if (_xRecipe.GoldenRegionCellBmp != value)
                {
                    var old = _xRecipe.GoldenRegionCellBmp;
                    _xRecipe.GoldenRegionCellBmp = value;
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
            get => _xRecipe.GoldenChipBmp;
            set
            {
                if (_xRecipe.GoldenChipBmp != value)
                {
                    var old = _xRecipe.GoldenChipBmp;
                    _xRecipe.GoldenChipBmp = value;
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
            get => _xRecipe.QrCodeBmp;
            set
            {
                if (_xRecipe.QrCodeBmp != value)
                {
                    var old = _xRecipe.QrCodeBmp;
                    _xRecipe.QrCodeBmp = value;
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
        /// golden chip 樣本在 Region Cell 內的矩形位置.
        /// 使用比較有意義的英文命名, 
        /// 對應到 xRegionTrain       
        /// </summary>
        RectangleF _xGoldenChipRect
        {
            //get => _xRecipe.xRegionTrain;
            //set => _xRecipe.xRegionTrain = value;
            get => _xRecipe.GoldenChipRect;
            set => _xRecipe.GoldenChipRect = value;
        }
        /// <summary>
        /// QrCode 樣本在 Region Cell 內的矩形位置.
        /// 使用比較有意義的英文命名, 
        /// 對應到 xRectCodeRegion 
        /// </summary>
        RectangleF _xQrCodeRect
        {
            //get => _xRecipe.xRectCodeRegion;
            //set => _xRecipe.xRectCodeRegion = value;
            get => _xRecipe.QrCodeRect;
            set => _xRecipe.QrCodeRect = value;
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
        JezTransImageViewPanel wndRegionViewer => _editorUI.ImgViewers[0] as JezTransImageViewPanel;
        JezTransImageViewPanel wndGoldenViewer => _editorUI.ImgViewers[1] as JezTransImageViewPanel;
        JezTransImageViewPanel wndDefectsViewer => _editorUI.ImgViewers[2] as JezTransImageViewPanel;
        Button btnPickGolden => _editorUI.btnPickGolden;
        Button btnRotateGolden => _editorUI.btnRotateGolden;
        Button btnTryScanQrCode => _editorUI.btnTryScanQrCode;
        Button btnDefectRegionAdd => _editorUI.btnDefectRegionAdd;
        Button btnDefectRegionDelete => _editorUI.btnDefectRegionDelete;
        Button btnDefectRegionClearAll => _editorUI.btnDefectRegionClearAll;
        Button btnTrainTemplate => _editorUI.btnTrainTemplate;
        Button btnSaveAllParams => _editorUI.btnSaveAllParams;
        Button btnCancel => _editorUI.btnCancel;
        #endregion

        #region VIEWER_TITLES
        string _Title1 => QMSG.T("Region View");
        string _Title2 => QMSG.T("Template View");
        string _Title3 => QMSG.T("Defects View");
        #endregion

        #region INTERACTORS
        CviGoldenBox _cviGoldenChipBox;
        CviRcpBox _cviQrCodeBox;
        List<CviRcpBox> _cviDefectMaskBoxes;
        CviRcpBox _cviActiveMaskBox;
        #endregion

        #region RUNTIME_DATA
        bool _bypassWindowEvents = false;
        bool _isGoldenModified = false;
        bool _isQrCodeModified = false;
        bool _isDefectMaskModified = false;
        bool _isPropertyModified = false;
        bool _isLineBorderModified
        {
            get => _lineBordersCtrl.IsModified;
            set => _lineBordersCtrl.IsModified = value;
        }
        QvQuad2D _goldenQuad2D = null;
        #endregion

        public void Attach(IvTemplateEditorUI ui)
        {
            _editorUI = ui;

            initGui();
            connectEventHandlers();
            
            _lineBordersCtrl.Attach(ui);
            _lineBordersCtrl.OnRequestToLocateGoldenQuad += OnRequestToLocateGoldenQuad;
            _lineBordersCtrl.OnMicroTransformChanged += OnMicroTransformChanged;
        }
        void initGui()
        {
            initInteractors();
            btnTrainTemplate.Visible = true;
        }
        void connectEventHandlers()
        {
            _editorUI.Window.HandleCreated += Window_HandleCreated;

            btnRotateGolden.Click += (s, e) => RotateGoldenRegionBmp();
            btnPickGolden.Click += (s, e) => BuildGoldenChipTemplate();
            btnTryScanQrCode.Click += (s, e) => BuildQRCodeTemplate();

            //btnAutoLayoutLineBorders.Click += (s, e) => AutoLayoutLineBorders();
            //btnBuildMictroTransform.Click += (s, e) => BuildMicroTransform();
            //_editorUI.numBorderIndent.ValueChanged += NumBorderExtend_ValueChanged;
            //_editorUI.numBorderExtend.ValueChanged += NumBorderExtend_ValueChanged;
            //_editorUI.numLineSpanPercentage.ValueChanged += NumBorderExtend_ValueChanged;

            btnDefectRegionAdd.Click += (s, e) => DfRegion_Add();
            btnDefectRegionDelete.Click += (s, e) => DfRegion_Delete();
            btnDefectRegionClearAll.Click += (s, e) => DfRegion_ClearAll();

            btnTrainTemplate.Click += (s, e) => TrainGoldenChipTemplate();
            btnSaveAllParams.Click += (s, e) => SaveAllParams(force: true);
            btnCancel.Click += (s, e) => CancelAndExit();

            foreach (var rdoBoxSelector in _editorUI.rdoBoxSelectors)
            {
                rdoBoxSelector.CheckedChanged += RdoBoxSelector_CheckedChanged;
            }

            //foreach (var cviBox in _cviLineBorderBoxes)
            //{
            //    cviBox.OnChanged += (s, e) =>
            //    {
            //        updateLineBorderBoxes(true);
            //        updateLineSegmentBoxes(true);
            //    };
            //}

            _cviQrCodeBox.OnChanged += (s, e) => BuildQRCodeTemplate(decode: false);

            if(_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
                pg.PropertyValueChanged += Pg_PropertyValueChanged;
        }

        #region EVENT_HANDLERS
        private void Window_HandleCreated(object sender, EventArgs e)
        {
            //_editorUI.lblActiveCarrierID.Text = GaUtil.GetEnumDescription(_carrierID) + " 晶粒模板設定";
            _editorUI.Window.FindForm().FormClosing += GaTemplateEditCtrl_FormClosing;

            _editorUI.Window.BeginInvoke((Action)updateSubTitle);

            //updateDispUI(DS1, _xBmpGoldenRegionTemplate);
            //updateDispUI(DS2, _xBmpGoldenChipTemplate);

            wndRegionViewer?.UpdateImage(_xBmpGoldenRegionTemplate, _Title1, false);
            wndGoldenViewer?.UpdateImage(_xBmpGoldenChipTemplate, _Title2, false);

            updateGoldenBoxes(false);

            //persistLineBorderIndentExt(false);
            //updateLineBorderBoxes(false);
            _lineBordersCtrl.PostInit();

            updateDefectMaskBoxes(false);
            updateMaskTemplate(false);
            updateVisionParams();

            _editorUI.Window.BeginInvoke(new Action(() =>
            {
                SetSelector(OpSelector.Golden);
            }));
        }
        private void GaTemplateEditCtrl_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAllParams(force: false);
            //persistLineBorderIndentExt(true);
            VxDebugDrawer.DestroyAllWindows();
        }
        private void RdoBoxSelector_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rdo && rdo.Checked)
            {
                int index = Array.IndexOf(_editorUI.rdoBoxSelectors, rdo);
                SetSelector((OpSelector)index);
            }
        }
        private void Pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _isPropertyModified = true;
            string propertyName = e.ChangedItem?.PropertyDescriptor?.Name;

            if (propertyName == "xAlgorithm")
            {
                _lineBordersCtrl.UpdateAlgorithmStatus();
            }

            if (_opSelector == OpSelector.LineBorders)
            {
                if (propertyName == "xCarrierBackground")
                {
                    //自動刷新 邊線 抓取結果
                    _lineBordersCtrl.UpdateLineSegmentBoxes(true);
                    refreshDispUI(wndRegionViewer);
                }
            }
        }
        private void CviDefectMaskBox_OnChanged(object sender, EventArgs e)
        {
            _cviActiveMaskBox = (sender as CviRcpBox);
            updateMaskTemplate(true);
        }
        private void OnMicroTransformChanged(object sender, EventArgs e)
        {
            _isPropertyModified = true;
        }
        private void OnRequestToLocateGoldenQuad(object sender, DoWorkEventArgs e)
        {
            if (_xAlgorithm == MatchAlgorithmEnum.GridMatch)
            {
                e.Result = null;

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

                // 強制 重新抓取 goldenQuad2D
                if ((e.Argument is bool force) && force || _goldenQuad2D == null)
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

                        // 回傳到 event args
                        e.Result = _goldenQuad2D;
                    }
                    catch (Exception ex)
                    {
                        var errMsg = GaUtil.GetEnumDescription(ErrorCodes.WARN_CAN_NOT_FETCH_QUAD_2D) + "\n\r\n\r" + ex.Message;
                        VsMessageBox.Warning(errMsg);
                        return;
                    }
                }
            }
            else
            {
                e.Result = new QvQuad2D()
                {
                    Corners = Array.ConvertAll(_cviGoldenChipBox.Corners, c => new QVector2(c.X, c.Y))
                };
            }
        }
        #endregion

        #region DISP_UI_FUNCTIONS
        void initInteractors()
        {
            _cviGoldenChipBox = new CviGoldenBox(Brushes.Orange, 1, 3) { Visible = false };
            _cviQrCodeBox = new CviRcpBox(Brushes.DeepPink, 1, 3) { Visible = false };
            _cviDefectMaskBoxes = new List<CviRcpBox>();

            //_cviLineBorderBoxes = new CviRcpBox[4];
            //_cviLineSegmentBoxes = new CviLineSegmentsBox[4];
            //for (int i = 0, N = _cviLineBorderBoxes.Length; i < N; i++)
            //{
            //    _cviLineBorderBoxes[i] = new CviRcpBox(Brushes.Blue, 1, 3) { Visible = false };
            //    _cviLineSegmentBoxes[i] = new CviLineSegmentsBox(Color.Cyan) { Visible = false };
            //}

            var viewer = wndRegionViewer?.ImgViewer;
            if (viewer != null)
            {
                viewer.AddInteractor(_cviGoldenChipBox);
                viewer.AddInteractor(_cviQrCodeBox);

                //foreach (var box in _cviLineBorderBoxes)
                //    viewer.AddInteractor(box);
                //foreach (var box in _cviLineSegmentBoxes)
                //    viewer.AddInteractor(box);
            }
        }
        void refreshDispUI(Control dispUI)
        {
            if (dispUI is JezTransImageViewPanel panel)
                panel.MatViewer.Invalidate();
            else
                dispUI?.Invalidate();
        }
        #endregion

        void SetSelector(OpSelector selector)
        {
            if (_opSelector != selector)
            {
                _opSelector = selector;
                updateSubTitle();
                updateGuiStatus();
                
                _lineBordersCtrl.UpdateLineSegmentBoxes(_opSelector == OpSelector.LineBorders);

                refreshDispUI(wndRegionViewer);
                refreshDispUI(wndGoldenViewer);
                refreshDispUI(wndDefectsViewer);
            }
        }

        void RotateGoldenRegionBmp()
        {
            Bitmap bmpRot = null;

            try
            {
                var srcBmp = _xBmpGoldenRegionTemplate;
                if (srcBmp == null || srcBmp.Width <= 0 || srcBmp.Height <= 0)
                    return;

                // 1. 取得來源 quad
                var quadSrc = new QvQuad2D
                {
                    Corners = Array.ConvertAll(_cviGoldenChipBox.Corners, c => new QVector2(c.X, c.Y))
                };

                // 2. 計算目標角度與旋轉差值
                double currentAngle = quadSrc.Angle;
                double targetAngle = Math.Abs(currentAngle) < Math.Abs(Math.Abs(currentAngle) - 90) ? 0 : 90;

                // 目標角度 - 當前角度 (若 QvQuad2D 為順時針，OpenCV GetRotationMatrix2D 需留意逆時針正負號)
                double deltaAngle = -(targetAngle - currentAngle);

                // 2.1 角度差極小，無需旋轉
                System.Diagnostics.Debug.WriteLine("轉正角度 = {0:0.00}°", deltaAngle);
                if (Math.Abs(deltaAngle) < 0.01)
                    return;

                // 3. 以 quadSrc 中心點進行 OpenCV 旋轉
                var center = new Point2f((float)quadSrc.Center.X, (float)quadSrc.Center.Y);
                var dSize = new OpenCvSharp.Size(srcBmp.Width, srcBmp.Height);

                using (var matRot = Cv2.GetRotationMatrix2D(center, deltaAngle, 1.0))
                using (var srcBridge = new QxImageBridge(srcBmp))
                using (var dstImg = new Mat(dSize, srcBridge.Image.Type()))
                {
                    Cv2.WarpAffine(
                        srcBridge.Image,
                        dstImg,
                        matRot,
                        dSize,
                        InterpolationFlags.Cubic,
                        BorderTypes.Replicate
                    );

                    bmpRot = dstImg.ToBitmap();
                }

                // 4. 同步更新 Target Quad 與內部 ROI 幾何資訊
                var quadDst = quadSrc.Clone();
                quadDst.Angle = targetAngle;
                quadDst.GetMidSize(out var midSize);
                var rectDst = JetEazy.Qcvt.CreateCenterRect((float)quadDst.Center.X, (float)quadDst.Center.Y, midSize.Width, midSize.Height);
                quadDst = QvQuad2D.From(rectDst);

                _cviGoldenChipBox.Corners = Array.ConvertAll(quadDst.Corners, c => new PointF((float)c.X, (float)c.Y));
                _cviGoldenChipBox.Box = Rectangle.Round(rectDst);
                _xGoldenChipRect = rectDst;

                // 5. 直接將新圖交給物件管理 (內部已自動處置舊圖)
                _xBmpGoldenRegionTemplate = bmpRot;
                bmpRot = null; // 轉移成功，防止 finally 處置到新圖

                // 6. 更新 UI 與狀態
                _isGoldenModified = true;
                wndRegionViewer?.UpdateImage(_xBmpGoldenRegionTemplate, _Title1, false);
            }
            catch (Exception ex)
            {
                HandleException("RotateGoldenRegionBmp", ex);
            }
            finally
            {
                // 僅在轉移失敗 (發生 Exception) 時，才需要處置未付諸使用的 bmpRot
                bmpRot?.Dispose();
                bmpRot = null;
            }
        }
        void BuildGoldenChipTemplate()
        {
            try
            {
                // 從 _xBmpGoldenRegionTemplate 切出 bmpTemplate
                var roiRect = _cviGoldenChipBox.Box;
                var bmpGoldenChip = cropBitmap(_xBmpGoldenRegionTemplate, ref roiRect);
                if (bmpGoldenChip == null)
                    return;

                // 更新 參數
                this._xBmpGoldenChipTemplate = bmpGoldenChip;
                this._xGoldenChipRect = roiRect;

                // 標記 已改變
                _isGoldenModified = true;
                _goldenQuad2D = null;

                // 更新 GUI
                wndGoldenViewer?.UpdateImage(_xBmpGoldenChipTemplate, _Title2, false);

                // 更新 mask
                updateMaskTemplate(false);
            }
            catch (Exception ex)
            {
                HandleException("BuildGoldenChipTemplate", ex);
            }
        }
        void BuildQRCodeTemplate(bool decode = true)
        {
            try
            {
                // 從 _xBmpGoldenRegionTemplate 切出 bmp
                var roiRect = _cviQrCodeBox.Box;
                var bmp = cropBitmap(_xBmpGoldenRegionTemplate, ref roiRect);
                if (bmp == null)
                    return;

                // 更新 參數
                this._xBmpQrCodeTemplate = bmp;
                this._xQrCodeRect = roiRect;

                // 標記 已改變
                _isQrCodeModified = true;

                if (decode)
                {
                    // DECODE
                    aoiDecodeQrCode(this._xBmpQrCodeTemplate, out string text);

                    // 更新 Text
                    _editorUI.wndQrCodeResult.Text = text;
                }
            }
            catch(Exception ex)
            {
                HandleException("BuildQRCodeTemplate", ex);
            }
        }

#if (OPT_MOVED_TO_CHILD_CTRL)
        void AutoLayoutLineBorders()
        {
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

            //>>> var ind = (int)_editorUI.numBorderIndent.Value;
            var ind = updateNumBorderIndentDynamically();
            var ext = (int)_editorUI.numBorderExtend.Value;
            var ratio = (double)_editorUI.numLineSpanPercentage.Value * 0.01;

            var baseRect = Rectangle.Round(_xGoldenChipRect);
            var W = baseRect.Width;
            var H = baseRect.Height;
            var ww = (int)(baseRect.Width * ratio);
            var hh = (int)(baseRect.Height * ratio);
            var dw = W - ww;
            var dh = H - hh;

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

            if (_goldenQuad2D != null)
            {
                if (_xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    // ind 固定在 baseRect 與 quadRect 中間處
                    var quadRect = Rectangle.Round(_goldenQuad2D.BoundaryRect);
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
                    
                    int i = 0;
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL - ext, yT + dh / 2, indL + ext, hh);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL + dw / 2, yT - ext, ww, indT + ext);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xR - indR, yT + dh / 2, indR + ext, hh);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL + dw / 2, yB - indB, ww, indB + ext);

                    //_bypassWindowEvents = true;
                    //_editorUI.numBorderIndent.Enabled = false;
                    //GaUtil.SetNum(_editorUI.numBorderIndent, (indL + indR + indT + indB) / 4m);
                    //_bypassWindowEvents = false;
                    updateNumBorderIndentDynamically((indL + indR + indT + indB) / 4);
                }
                else
                {
                    // ind 固定在 baseRect 與 quadRect 中間處
                    var quadRect = Rectangle.Round(_goldenQuad2D.BoundaryRect);
                    var extraIndent = (int)Math.Abs(_editorUI.numBorderIndent.Minimum) / 2;

                    var inX = (quadRect.X + baseRect.X) / 2 + extraIndent;
                    var inY = (quadRect.Y + baseRect.Y) / 2 + extraIndent;
                    var inX2 = (quadRect.Right + baseRect.Right) / 2 - extraIndent;
                    var inY2 = (quadRect.Bottom + baseRect.Bottom) / 2 - extraIndent;
                    
                    var xL = baseRect.X;
                    var xR = baseRect.Right;
                    var yT = baseRect.Y;
                    var yB = baseRect.Bottom;
                    var indL = Math.Abs(inX - xL);
                    var indR = Math.Abs(inX2 - xR);
                    var indT = Math.Abs(inY - yT);
                    var indB = Math.Abs(inY2 - yB);

                    int i = 0;
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL - ext, yT + dh / 2, indL + ext, hh);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL + dw / 2, yT - ext, ww, indT + ext);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xR - indR, yT + dh / 2, indR + ext, hh);
                    _cviLineBorderBoxes[i++].Box = new Rectangle(xL + dw / 2, yB - indB, ww, indB + ext);
                }
            }
            else
            {
                var x = baseRect.X;
                var y = baseRect.Y;
                int i = 0;
                _cviLineBorderBoxes[i++].Box = new Rectangle(x - ext, y + dh / 2, ind + ext, hh);
                _cviLineBorderBoxes[i++].Box = new Rectangle(x + dw / 2, y - ext, ww, ind + ext);
                _cviLineBorderBoxes[i++].Box = new Rectangle(baseRect.Right - ind, y + dh / 2, ind + ext, hh);
                _cviLineBorderBoxes[i++].Box = new Rectangle(x + dw / 2, baseRect.Bottom - ind, ww, ind + ext);

                //>>> _editorUI.numBorderIndent.Enabled = true;
                updateNumBorderIndentDynamically();
            }

            refreshDispUI(wndRegionViewer);
            updateLineBorderBoxes(true);
            updateLineSegmentBoxes(true);
            _isLineBorderModified = true;
        }
        void BuildMicroTransform()
        {
            //if (DialogResult.Yes != MessageBox.Show("是否要重新設定 樣本尺寸?", "參數設定", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            //    return;

            if (VsMessageBox.Question(QMSG.Text(Prompts.Question_To_Rebuild_Template_Dimension)) != DialogResult.Yes)
                return;

            _isPropertyModified = true;
            updateLineSegmentBoxes(true, calcGoldenDim: true);
        }
#endif

        void TrainGoldenChipTemplate(bool silentSuccess = false)
        {
            try
            {
                VxDebugDrawer.DestroyAllWindows();

#if (OPT_OLD_CODE)
            int err = _xRecipe.PrintTempTrain(!silentSuccess);

            if (err != 0)
            {
                //VsMessageBox.Warning("匹配模板 創建失敗!");
                VsMessageBox.Warning(QMSG.Text(ErrorCodes.AoiErr_Template_Creation_Failed));
            }
            else if (!silentSuccess)
            {
                //VsMessageBox.Info("匹配模板 創建成功!");
                VsMessageBox.Info(QMSG.Text(ErrorCodes.AoiErr_Template_Creation_OK));
            }
#endif

                bool ok = false;
                var aoi = _sysModel.AoiModel.GetChipLocAoi();
                if (aoi != null)
                {
                    var goldenBmp = _xRecipe.GoldenChipBmp;
                    ok = aoi.Train(goldenBmp, !silentSuccess);
                }

                if (!ok)
                {
                    //VsMessageBox.Warning("匹配模板 創建失敗!");
                    VsMessageBox.Warning(QMSG.Text(ErrorCodes.AoiErr_Template_Creation_Failed));
                }
                else if (!silentSuccess)
                {
                    //VsMessageBox.Info("匹配模板 創建成功!");
                    VsMessageBox.Info(QMSG.Text(ErrorCodes.AoiErr_Template_Creation_OK));
                }
            }
            catch(Exception ex)
            {
                HandleException("TrainGoldenTemplate", ex);
            }
        }
        void SaveAllParams(bool force)
        {
            bool isAnySaved = false;
            string target = "";

            TrainGoldenChipTemplate(silentSuccess: true);

            if (_isGoldenModified || force)
            {
                _isGoldenModified = false;
                target += "_REGION_CHIP";
            }

            if (_isQrCodeModified || force)
            {
                _isQrCodeModified = false;
                target += "_QRCODE";
            }

            if (_isLineBorderModified || force)
            {
                _isLineBorderModified = false;
                target += "_LINEBORDER";
            }

            if (_isDefectMaskModified || force)
            {
                _isDefectMaskModified = false;
                target += "_MASK";
            }

            if (_isPropertyModified || force)
            {
                _isPropertyModified = false;
                target += "_InspectX3";
                _xInspectX3.Save();
            }

            if (!string.IsNullOrEmpty(target))
            {
                _xRecipe.SaveTemplate(target);
                isAnySaved = true;
            }

            if (force)
                //VsMessageBox.Info("參數 保存成功.");
                VsMessageBox.Info(QMSG.Text(Prompts.Info_Save_Recipe_OK));
            else if (isAnySaved)
                //VsMessageBox.Info("參數 已經自動保存.");
                VsMessageBox.Info(QMSG.Text(Prompts.Info_Auto_Save_Rcipe_OK));
        }
        void CancelAndExit()
        {
            // 此功能保留
            // 目前架構 尚無法完美取消
            return;

            _isGoldenModified = false;
            _isQrCodeModified = false;
            _isLineBorderModified = false;
            _isDefectMaskModified = false;
            _isPropertyModified = false;
            _editorUI.Window?.FindForm()?.Close();
        }

        void DfRegion_Add()
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
            updateMaskTemplate(true);
        }
        void DfRegion_Delete()
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
            updateMaskTemplate(true);
        }
        void DfRegion_ClearAll()
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
        void updateCviBoxesStatus(OpSelector selector)
        {
            // Region View Gui
            _cviGoldenChipBox.Visible = selector == OpSelector.Golden;

            // QR Code Gui
            _cviQrCodeBox.Visible = selector == OpSelector.QrCode;
            
            //// Line Border Gui
            //foreach (var cviBox in _cviLineBorderBoxes)
            //    cviBox.Visible = selector == OpSelector.LineBorders;

            // Defects Gui
            foreach (var cviBox in _cviDefectMaskBoxes)
                cviBox.Visible = selector == OpSelector.Defects;
        }
        void updateSubTitle()
        {
            string subTitle = GaUtil.GetEnumDescription(_carrierID);
            subTitle = QMSG.Text(subTitle, "gui");

            int idx = (int)_opSelector;
            
            if (0 <= idx && idx < _editorUI.rdoBoxSelectors.Length)
                subTitle += " : " + _editorUI.rdoBoxSelectors[idx].Text;

            _editorUI.lblActiveCarrierID.Text = subTitle;
        }
        void updateGoldenBoxes(bool toRecipe)
        {
            if (toRecipe)
            {
                this._xGoldenChipRect = _cviGoldenChipBox.Box;
                this._xQrCodeRect = _cviQrCodeBox.Box;
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

#if (OPT_MOVED_TO_CHILD_CTRL)
        void updateLineBorderBoxes(bool toRecipe)
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
        void updateLineSegmentBoxes(bool show, bool calcGoldenDim = false)
        {
            if (!show)
            {
                foreach (var lineSegBox in _cviLineSegmentBoxes)
                    lineSegBox.Visible = false;
            }
            else
            {
                var bmpSrcRegion = _xBmpGoldenRegionTemplate;
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
                    aoiCalcGoldenChipDimension(edgeLines);
                }
            }
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
                    settings.lineBorderExt = (int)_editorUI.numBorderExtend.Value;
                    settings.lineSpanPercentage = (float)_editorUI.numLineSpanPercentage.Value;
                    settings.Save();
                }
                else
                {
                    _bypassWindowEvents = true;
                    //>>> GaUtil.SetNum(_editorUI.numBorderIndent, settings.lineBorderIndent);
                    GaUtil.SetNum(_editorUI.numBorderExtend, settings.lineBorderExt);
                    GaUtil.SetNum(_editorUI.numLineSpanPercentage, (decimal)settings.lineSpanPercentage);
                    _bypassWindowEvents = false;

                    updateNumBorderIndentDynamically(settings.lineBorderIndent);
                }
            }
            catch
            {
            }
        }
#endif

        void updateDefectMaskBoxes(bool toRecipe)
        {
            if (wndGoldenViewer == null) return;

            var dispUI = wndGoldenViewer;
            var imgViewer = dispUI.ImgViewer;
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
                _isDefectMaskModified = true;
            }
            else
            {
                // CleanUp
                bmpMask?.Dispose();
            }

            // 更新 bmpMask 到 GUI
            // updateDispUI(wndDefectsViewer, maskRects.Count > 0 ? bmpDisp : null);
            wndDefectsViewer?.UpdateImage(bmpDisp, _Title3, false);

            // CleanUp
            bmpDisp?.Dispose();
        }
        void updateVisionParams()
        {
            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
            {
                try
                {
                    EzPropertyGridTranslator.Register(_xInspectX3);
                    pg.SelectedObject = _xInspectX3;
                }
                catch (Exception ex)
                {
                    // 應該不會跑到此處, 有問題需要改正
                    // EzPropertyGridTranslator.Register
                    string errMsg = "Translate(_xInspectX3) Error";
                    errMsg += "\n\r\n\r" + ex.Message;
                    errMsg += "\n\n" + ex.StackTrace;
                    QMessageBox.Show(errMsg, GlobalConfig.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        void updateGuiStatus()
        {
            updateCviBoxesStatus(_opSelector);

            // Template Gui
            _editorUI.btnPickGolden.Enabled = _opSelector == OpSelector.Golden;

            //// Line Border Gui
            //updateNumBorderIndentDynamically();
            //_editorUI.btnAutoLineBorders.Enabled = _opSelector == OpSelector.LineBorders;
            //_editorUI.btnBuildMircoTransform.Enabled = _opSelector == OpSelector.LineBorders;
            //_editorUI.numBorderExtend.Enabled = _opSelector == OpSelector.LineBorders;
            //_editorUI.numLineSpanPercentage.Enabled = _opSelector == OpSelector.LineBorders;
            _lineBordersCtrl.IsEditting = _opSelector == OpSelector.LineBorders;

            // QR Code Gui
            _editorUI.btnTryScanQrCode.Enabled = _opSelector == OpSelector.QrCode;
        }

#if (OPT_MOVED_TO_CHILD_CTRL)
        int updateNumBorderIndentDynamically(int? indent = null)
        {
            //----------------------------------------------
            // 根據晶粒型態動態配置 內緣 NumericUpDown
            //----------------------------------------------
            var num = _editorUI.numBorderIndent;
            if (num == null)
                return 0;

            decimal value = indent != null ? indent.Value : num.Value;

            _bypassWindowEvents = true;

            if(_xAlgorithm == MatchAlgorithmEnum.GridMatch)
            {
                // 格點型 晶粒
                num.Minimum = 1;
                GaUtil.SetNum(num, 1);
                num.Enabled = false;
            }
            else
            {
                // 一般型 晶粒 (使用海康 template match)
                num.Enabled = true;
                num.Minimum = MIN_INDENT_FOR_MVD;
                if (value >= 0)
                    value = num.Minimum / 2m;
                GaUtil.SetNum(num, value);
            }

            _bypassWindowEvents = false;

            return (int)num.Value;
        }
#endif
        #endregion

        #region AOI_MODEL_FUNCTIONS
        //----------------------------------------------------------------------------
        // 此處函式不牽扯到 GUI, 將來要納入 AOI MODEL
        //----------------------------------------------------------------------------
        void aoiDecodeQrCode(Bitmap srcBmp, out string text)
        {
#if (OPT_CODE_CODE)
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
#endif
            text = _sysModel?.AoiModel?.DecodeQrCode(srcBmp);
            if (text == null)
                text = "";
        }
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
#if (OPT_MOVED_TO_CHILD_CTRL)
        bool aoiTryRunFindLineSegment(EdgeBorder eBorder, Bitmap bmpSrc, RectangleF boxRect, out CMvdLineSegmentF[] resultLines)
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
            var aoiModel = _sysModel.AoiModel;
            aoiModel.TryFindLineSegment(eBorder, bmpSrc, boxRect, out var line);
            resultLines = new CMvdLineSegmentF[] { line };
            return line != null;
        }
        /// <summary>
        /// 注意: 這裡會影響 PAD型晶粒的 Golden Template "GRID" !!! 
        /// </summary>
        void aoiCalcGoldenChipDimension(List<EzLSD.LineSegment> lines)
        {
            var aoiModel = _sysModel?.AoiModel;
            if (aoiModel != null && lines != null)
            {
                var regionBmp = _xBmpGoldenRegionTemplate;
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
        }
#endif
        Bitmap cropBitmap(Bitmap bmpSrc, ref Rectangle roiRect)
        {
            GaUtil.Clip(ref roiRect, bmpSrc.Size);
            if (roiRect.Width < 2 || roiRect.Height < 2)
                return null;
            var bmp = bmpSrc.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            return bmp;
        }
        #endregion

        void HandleException(string funcName, Exception ex)
        {
            var errMsg = $"Error : {GetType().Name}.{funcName}";
            errMsg += "\n\r" + ex.Message;
            errMsg += "\n\r" + ex.StackTrace;
            QMessageBox.Warning(errMsg, translate: false);
        }
    }
}
