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
    public class GaTemplateEditCtrl : GaRcpBaseCtrl
    {
        #region ENUM
        enum OpSelector : int
        {
            None = -1,
            Golden,
            LineBorders,
            QrCode,
            Defects,
            BadConns,
        }
        OpSelector _opSelector = OpSelector.None;
        #endregion

        #region GLOBAL_MESS
        CarrierEnum _carrierID => _sysModel.ActiveCarrierID;
        #endregion

        #region RECIPE_PARAMS
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
        #endregion

        #region GUI_LINKS
        IvTemplateEditorUI _editorUI;
        JezTransImageViewPanel wndRegionViewer => _editorUI.ImgViewers[0] as JezTransImageViewPanel;
        JezTransImageViewPanel wndGoldenViewer => _editorUI.ImgViewers[1] as JezTransImageViewPanel;
        JezTransImageViewPanel wndDefectsViewer => _editorUI.ImgViewers[2] as JezTransImageViewPanel;
        Button btnPickGolden => _editorUI.btnPickGolden;
        Button btnRotateGolden => _editorUI.btnRotateGolden;
        Button btnTryScanQrCode => _editorUI.btnTryScanQrCode;
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
        //List<CviRcpBox> _cviDefectMaskBoxes;
        //CviRcpBox _cviActiveMaskBox;
        #endregion

        #region RUNTIME_DATA
        bool _bypassWindowEvents = false;
        bool _isGoldenModified = false;
        bool _isQrCodeModified = false;
        bool _isPropertyModified = false;
        bool _isLineBorderModified
        {
            get => _lineBordersCtrl.IsModified;
            set => _lineBordersCtrl.IsModified = value;
        }
        bool _isDefectMaskModified
        {
            get => _defectsCtrl.IsModified;
            set => _defectsCtrl.IsModified = value;
        }
        bool _isBadConnsModified
        {
            get => _badConnsCtrl.IsModified;
            set => _badConnsCtrl.IsModified = value;
        }
        QvQuad2D _goldenQuad2D = null;
        #endregion

        #region CHILD_CTRLS
        readonly GaTemplLineBorderEditCtrl _lineBordersCtrl = new GaTemplLineBorderEditCtrl();
        readonly GaTemplDefectsEditCtrl _defectsCtrl = new GaTemplDefectsEditCtrl();
        readonly GaTemplBadConnsEditCtrl _badConnsCtrl = new GaTemplBadConnsEditCtrl();
        #endregion

        public void Attach(IvTemplateEditorUI ui)
        {
            _editorUI = ui;

            initGui();
            connectEventHandlers();

            _lineBordersCtrl.Attach(ui);
            _lineBordersCtrl.OnRequestToLocateGoldenQuad += OnRequestToLocateGoldenQuad;
            _lineBordersCtrl.OnMicroTransformChanged += OnMicroTransformChanged;

            _defectsCtrl.Attach(ui);
            _badConnsCtrl.Attach(ui);
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
            //btnDefectRegionAdd.Click += (s, e) => DfRegion_Add();
            //btnDefectRegionDelete.Click += (s, e) => DfRegion_Delete();
            //btnDefectRegionClearAll.Click += (s, e) => DfRegion_ClearAll();

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

            if (_editorUI.wndVisionSettingsPanel is PropertyGrid pg)
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

            _lineBordersCtrl.PostInit();
            _defectsCtrl.PostInit();
            _badConnsCtrl.PostInit();

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

#if (OPT_MOVED_TO_CHILD_CTRLS)
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
#endif
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

            //_cviDefectMaskBoxes = new List<CviRcpBox>();
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
                _defectsCtrl.UpdateDefectMaskBoxes(_opSelector == OpSelector.Defects);
                _badConnsCtrl.UpdateDetectionBoxes(_opSelector == OpSelector.BadConns);

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

                // 更新 Defects GUI
                _defectsCtrl.UpdateDefectMaskBoxes(false);

                // 更新 BadConns GUI
                _badConnsCtrl.UpdateDetectionBoxes(false);
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
            catch (Exception ex)
            {
                HandleException("BuildQRCodeTemplate", ex);
            }
        }

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
            catch (Exception ex)
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

            if (_isBadConnsModified || force)
            {
                _isBadConnsModified = false;
                target += "_BADCONNS";
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

        #region PRIVATE_GUI_FUNCTIONS
        void updateCviBoxesStatus(OpSelector selector)
        {
            // Region View Gui
            _cviGoldenChipBox.Visible = selector == OpSelector.Golden;

            // QR Code Gui
            _cviQrCodeBox.Visible = selector == OpSelector.QrCode;

            //// Line Border Gui
            //foreach (var cviBox in _cviLineBorderBoxes)
            //    cviBox.Visible = selector == OpSelector.LineBorders;

            //// Defects Gui
            //foreach (var cviBox in _cviDefectMaskBoxes)
            //    cviBox.Visible = selector == OpSelector.Defects;
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
                AdjustRectToFitViewer(ref rect, wndRegionViewer.ImgViewer);
                _cviGoldenChipBox.Box = rect;

                rect = Rectangle.Round(this._xQrCodeRect);
                if (rect == RectangleF.Empty)
                    rect = _cviGoldenChipBox.Box;
                AdjustRectToFitViewer(ref rect, wndRegionViewer.ImgViewer);
                _cviQrCodeBox.Box = rect;
            }
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

            // QR Code Gui
            _editorUI.btnTryScanQrCode.Enabled = _opSelector == OpSelector.QrCode;

            // Line Border Gui
            _lineBordersCtrl.IsEditting = _opSelector == OpSelector.LineBorders;

            // Detects Gui
            _defectsCtrl.IsEditting = _opSelector == OpSelector.Defects;

            // BadConns Gui (連筋檢測)
            _badConnsCtrl.IsEditting = _opSelector == OpSelector.BadConns;
        }
        //void updateViewersVisible()
        //{
        //    switch(_opSelector)
        //    {
        //        case OpSelector.Defects:
        //            wndRegionViewer.Visible = false;
        //            wndGoldenViewer.Visible = true;
        //            wndDefectsViewer.Visible = true;
        //            break;
        //        case OpSelector.BadConn:
        //            wndGoldenViewer.Visible = false;
        //            wndRegionViewer.Visible = true;
        //            wndDefectsViewer.Visible = true;
        //            break;
        //        default:
        //            wndRegionViewer.Visible = true;
        //            wndGoldenViewer.Visible = true;
        //            wndDefectsViewer.Visible = false;
        //            break;
        //    }
        //}
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
            text = _sysModel?.AoiModel?.GetAoiQrDecoder()?.TryDecode(srcBmp);
            if (text == null)
                text = "";
        }
        Bitmap cropBitmap(Bitmap bmpSrc, ref Rectangle roiRect)
        {
            GaUtil.Clip(ref roiRect, bmpSrc.Size);
            if (roiRect.Width < 2 || roiRect.Height < 2)
                return null;
            var bmp = bmpSrc.Clone(roiRect, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            return bmp;
        }
        #endregion
    }
}
