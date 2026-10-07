using JetEazy.QvMath;
using LaserAlignDX.Model;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using Traveller106;

namespace LaserAlignDX.OPSpace
{
    public class RegionCellX3Class : IDisposable
    {
        #region GLOBAL_MESS
        public InspectX3ParaClass xInspect
        {
            get => RecipeFPIX3Class.Instance.InspectParams;
        }
        static INI _INI => INI.Instance;
        #endregion

        public RegionCellX3Class()
        {
        }

        public void Dispose()
        {
            //cPositionFixToolObj?.Dispose();
            //cPositionFixToolObj = null;
            //cImageArithmeticToolObj?.Dispose();
            //cImageArithmeticToolObj = null;
            //cImageBinaryToolObj?.Dispose();
            //cImageBinaryToolObj = null;
            //cImageMorphToolObj?.Dispose();
            //cImageMorphToolObj = null;
            //cBlobFindToolObj?.Dispose();
            //cBlobFindToolObj = null;
            //mvd2DReader?.Dispose();
            //mvd2DReader = null;
            //mvdFindLineClass?.Dispose();
            //mvdFindLineClass = null;
            //mvdPairLineClass?.Dispose();
            //mvdPairLineClass = null;
            //cImageAffineTransformToolObj?.Dispose();
            //cImageAffineTransformToolObj = null;

            try { OutGridLink?.Dispose(); } catch { }
            OutGridLink = null;
        }

        // INDEX
        #region INDEX_LABEL_AND_ROW_COLS
        public int Index = 0;
        public string lblName = "";
        public int CellRow = 0;
        public int CellCol = 0;
        public bool ByPass { get; set; } = false;   // 沒用到
        #endregion

        /// <summary>
        /// runtime chip ROI (FullFov Camera Coordinates) (單位 pixel)
        /// 更精確(內縮)的晶粒矩形區域 (對應 xRecipe.xRegionTrain) 
        /// </summary>
        public RectangleF viewRectF = new RectangleF();
        /// <summary>
        /// 優化後, 晶粒 像測的詳細數據 皆放置於此 
        /// </summary>
        public GaChipData ChipData = new GaChipData();
        /// <summary>
        /// 用來連結 最接近此格位 的 散落在外圍 晶粒
        /// (2025-10-22 新增)
        /// </summary>
        public RegionCellX3Class OutGridLink { get; set; } = null;

        /// <summary>
        /// 是否已經定位成功
        /// </summary>
        public bool IsLocated()
        {
            return ChipData != null && ChipData.ChipQuad2D != null;
        }

        #region PUBLIC_CACHE_PROPERTIES
        /// <summary>
        /// 理想 格位中心座標 X (單位 mm)
        /// </summary>
        public float OrgX { get; set; } = 0;
        /// <summary>
        /// 理想 格位中心座標 Y (單位 mm)
        /// </summary>
        public float OrgY { get; set; } = 0;
        /// <summary>
        /// PLC 定位補償 X (單位 mm)
        /// </summary>
        public float RunX = 0;
        /// <summary>
        /// PLC 定位補償 Y (單位 mm)
        /// </summary>
        public float RunY = 0;
        /// <summary>
        /// PLC 定位角度 A (單位 degree)
        /// </summary>
        public float RunAngle = 0;
        /// <summary>
        /// 推算馬達座標: 吸嘴排1 (單位 mm)
        /// </summary>
        public PointF Sur1 = new PointF();
        /// <summary>
        /// 推算馬達座標: 吸嘴排2 (單位 mm)
        /// </summary>
        public PointF Sur2 = new PointF();
        /// <summary>
        /// 测量结果: 晶粒尺寸X (單位 mm)
        /// </summary>
        public float RunWidth
        {
            get => ChipData.ChipDimension.ChipWidth;
            set => ChipData.ChipDimension.ChipWidth = value;
        }
        /// <summary>
        /// 测量结果: 晶粒尺寸Y (單位 mm)
        /// </summary>
        public float RunHeight
        {
            get => ChipData.ChipDimension.ChipHeight;
            set => ChipData.ChipDimension.ChipHeight = value;
        }
        #endregion

        #region PRIVATE_INSPECTION_RESULTS_DATA
        List<InspectReason> _inspectNgList = new List<InspectReason>();
        InspectReason _inspectResult = InspectReason.PASS;
        #endregion

        /// <summary>
        /// 檢測總合結果
        /// </summary>
        public InspectReason FinalInspectResult
        {
            get => _inspectResult;
        }
        /// <summary>
        /// 檢測總合結果 為 PASS
        /// </summary>
        public bool IsResultPass()
        {
            return _inspectResult == InspectReason.PASS && _inspectNgList.Count == 0;
        }
        /// <summary>
        /// 是否為 吸嘴空格
        /// </summary>
        public bool IsEmptyPlaceHold()
        {
            // 是否為 吸嘴空格
            return _inspectResult == InspectReason.NG_EMPTY;
        }
        /// <summary>
        /// 是否為 疑似有料 之 不明區塊 
        /// (2025-11-17 新增, 用來標記 踩腳)
        /// </summary>
        public bool IsAmbiguousBloc()
        {
            return _inspectResult == InspectReason.NG_AMBIGUOUS_BLOC;
        }
        /// <summary>
        /// 標記 檢測結果
        /// </summary>
        public void MarkResult(InspectReason result, bool reset = false)
        {
            if (reset)
            {
                _inspectResult = result;
                _inspectNgList.Clear();
            }
            else
            {
                // 已經是空格: 不能被執行檢測 (再被指定其他結果碼)
                if (IsEmptyPlaceHold())
                    return;

                _inspectResult = result;

                if (result != InspectReason.PASS)
                    _inspectNgList.Add(result);
            }
        }
        /// <summary>
        /// 枚舉所有 NG
        /// </summary>
        public IEnumerable<InspectReason> IterNgResults(bool reverse = false)
        {
            if (_inspectNgList != null)
            {
                if (reverse)
                {
                    for (int i = _inspectNgList.Count - 1; i >= 0; i--)
                        yield return _inspectNgList[i];
                }
                else
                {
                    foreach (var ng in _inspectNgList)
                        yield return ng;
                }
            }
        }
        /// <summary>
        /// 根據 inspectResult 傳回是否為 "疑似有料"
        /// </summary>
        public string GetNoTrayDesc()
        {
            string str = string.Empty;
            if (_inspectResult == InspectReason.NG_APPEARANCE)
            {
                str = "疑似有料";
                return str;
            }
            foreach (var reason in _inspectNgList)
            {
                if (reason == InspectReason.NG_APPEARANCE)
                {
                    str = "疑似有料";
                    //str = "Maybe.P";
                    break;
                }
            }
            return str;
        }

        #region RUNTIME_QRCODE_RESULT_DATA
        public string BarcodeResultText { get; set; } = "";
        public QvQuad2D BarcodeResultQuad { get; set; } = null;
        #endregion

        /// <summary>
        /// 清除上一次的檢測結果
        /// </summary>
        public void Reset()
        {
            ChipData = new GaChipData();

            try { OutGridLink?.Dispose(); } catch { }
            OutGridLink = null;

            ////<<< 廢除 >>> xFindResult = new AUVision.xFindResult();

            //inspectReason = InspectReason.PASS;
            //inspectReasons.Clear();
            MarkResult(InspectReason.PASS, reset: true);

            //RunCodeInfo = null;
            //if (mvd2DReader != null)
            //    mvd2DReader.DCodeInfo = null;
            //DrawBarcodePosition = null;
            BarcodeResultText = "";
            BarcodeResultQuad = null;

            RunX = 0;
            RunY = 0;
            RunAngle = 0;

            //IsSaveDebugPicture = false;

            //int i = 0;
            //while (i < 4)
            //{
            //    //CMvdLineSegmentF mLine = cMvdLineSegmentFsOut[i];
            //    if (cMvdLineSegmentFsOut[i] != null)
            //        cMvdLineSegmentFsOut[i] = null;
            //    if (cMvdLineSegmentFsInSide[i] != null)
            //        cMvdLineSegmentFsInSide[i] = null;
            //    CMvdShape mvdShape = cMvdShapesForFindLineRegion[i];
            //    if (cMvdShapesForFindLineRegion[i] != null)
            //        cMvdShapesForFindLineRegion[i] = null;
            //    i++;
            //}

            RunWidth = 0;
            RunHeight = 0;

            //DisLeft = 0;
            //DisTop = 0;
            //DisRight = 0;
            //DisBottom = 0;
        }

        public override string ToString()
        {
            return $"Cell[{CellRow},{CellCol}] {_inspectResult}";
        }
    }
}
