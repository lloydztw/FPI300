#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-10-07 優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using System;
using System.Drawing;
using ErrCodes = LaserAlignDX.Mvc.Model.ErrCodes;


namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 透視投影座標轉換
    /// </summary>
    public partial class QMicroChipTransform : ITransform
    {
        #region PRIVATE_DATA
        QVector _roiCenterPt = new QVector(0, 0);      // camera coordinates (pixels)
        QVector _mmPerPixel = new QVector(1.0, 1.0);
        ITransform _localTrf = null;
        ITransform _padsTrf = null;
        #endregion

        public QMicroChipTransform(string name, string srcUnit, string dstUnit)
        {
            _localTrf = new QTransform(name, srcUnit, dstUnit);
            _padsTrf = new QTransform(name + "_PAD", srcUnit, dstUnit);
        }
        public QMicroChipTransform(string name) : this(name, "pix", "mm")
        {
        }
        public void Dispose()
        {
            _localTrf?.Dispose();
            _localTrf = null;
            _padsTrf?.Dispose();
            _padsTrf = null;
        }

        public string Name
        {
            get => _localTrf.Name;
        }
        public ICalibGridPoints GetCalibGridPoints()
        {
            return _localTrf.GetCalibGridPoints();
        }
        public ICalibCornerPoints GetCalibCornerPoints()
        {
            return _localTrf.GetCalibCornerPoints();
        }
        public bool CheckBuildCondition(out double det, out double det2)
        {
            return _localTrf.CheckBuildCondition(out det, out det2);
        }
        QVector ITransform.Trans(QVector pt)
        {
            var q = _localTrf.Trans(pt);
            return q;
        }
        QVector ITransform.InvTrans(QVector pt)
        {
            var q = _localTrf.InvTrans(pt);
            return q;
        }

        public bool Build()
        {
            var err = buildTrfs();
            bool ok = err == ErrCodes.OK;
            return ok;
        }
        public void Load(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Load(filename);
            _padsTrf?.Load(filename);

            string sectName = Name + "_extra_settings";
            _roiCenterPt?.LoadIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.LoadIni(filename, sectName, "mmPerPixel");
        }
        public void Save(string filename)
        {
            if (filename == null)
                filename = getRecipeIniFileName();

            _localTrf?.Save(filename);
            _padsTrf?.Save(filename);

            string sectName = Name + "_extra_settings";
            _roiCenterPt?.SaveIni(filename, sectName, "roiCenterPt");
            _mmPerPixel?.SaveIni(filename, sectName, "mmPerPixel", fixDigit: false);
        }

        ErrCodes buildTrfs()
        {
            ErrCodes err = ErrCodes.OK;
            var trfs = new[] { _localTrf, _padsTrf };
            var errs = new[] { ErrCodes.ERR_WEAK_LINE_CONDITION, ErrCodes.ERR_WEAK_PADS_CONDITION };
            int idx = 0;
            foreach (var trf in trfs)
            {
                if (trf != null)
                {
                    bool ok = trf.Build();
                    trf.CheckBuildCondition(out var det1, out var det2);
                    GaUtil.LOG($"MATRIX_{trf.Name} det1 = {det1:0.000000}");
                    GaUtil.LOG($"MATRIX_{trf.Name} det2 = {det2:0.000000}");
                    if (!ok && err == ErrCodes.OK)
                        err = errs[idx];
                }
                idx++;
            }
            return err;
        }
        public ErrCodes BuildMicroTransform(SizeF targetDim, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            int NP = 4;

            //(0) Check Lines Condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var cc = chipData.ChipBox2D.Center;
            _roiCenterPt = new QVector(cc.X, cc.Y);

            //(2) MeasurePoints (local scope) : 左0 上1 右2 下3 
            var camMeasurePts = getMidMeasurePoints(lines);
            camMeasurePts = Array.ConvertAll(camMeasurePts, p => p - _roiCenterPt);

            //(3) DST POINTS (local scope)
            double W = targetDim.Width;
            double H = targetDim.Height;
            var dstMidPoints = new QVector[]
            {
                new QVector(0, H/2),            //mid LEFT
                new QVector(W/2, 0),            //mid TOP
                new QVector(W, H/2),            //mid RIGHT
                new QVector(W/2, H),            //mid Bottom
            };

            //(4) localTrf (local scope)
            #region localTrf
            var impCalib = _localTrf.GetCalibCornerPoints();
            for (int i = 0; i < NP; i++)
                impCalib.Set(i, camMeasurePts[i], dstMidPoints[i]);
            _localTrf.Build();
            #endregion

            //(5) padsTrf (local scope)
            #region padsTrf
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var padsBox2D, false);
                var padsCamCorners = Array.ConvertAll(padsBox2D.Corners, c => new QVector(c.X, c.Y) - _roiCenterPt);
                var padsCalib = _padsTrf.GetCalibCornerPoints();
                for (int i = 0; i < NP; ++i)
                {
                    var dst = _localTrf.Trans(padsCamCorners[i]);
                    padsCalib.Set(i, padsCamCorners[i], dst);
                }
            }
            #endregion

            //(6) build
            err = buildTrfs();

            //(7) verify
            #region VERIFY
            if (true)
            {
                var microWorldPts = new QVector[NP];
                for (int i = 0; i < NP; i++)
                {
                    var p = _localTrf.Trans(camMeasurePts[i]);
                    var d = (dstMidPoints[i] - p).NormLength;
                    microWorldPts[i] = p;
                    System.Diagnostics.Debug.WriteLine($"Micro_Err[{i}] = {d:0.000} mm");
                }
                W = (microWorldPts[0] - microWorldPts[2]).NormLength;
                H = (microWorldPts[1] - microWorldPts[3]).NormLength;
                System.Diagnostics.Debug.WriteLine($"Micro_W_Err = {(targetDim.Width - W):0.000} mm");
                System.Diagnostics.Debug.WriteLine($"Micro_H_Err = {(targetDim.Height - H):0.000} mm");
            }
            #endregion

            //(8) 計算 mm per pixel
            #region 計算_mm_per_pixel
            var c0 = camMeasurePts[0];
            var c2 = camMeasurePts[2];  c2.Y = c0.Y;
            var c1 = camMeasurePts[1];
            var c3 = camMeasurePts[3];  c3.X = c1.X;
            var q0 = _localTrf.Trans(c0);
            var q2 = _localTrf.Trans(c2);
            var q1 = _localTrf.Trans(c1);
            var q3 = _localTrf.Trans(c3);
            var w_pix = (c0 - c2).NormLength;
            var h_pix = (c1 - c3).NormLength;
            var w_mm = (q0 - q2).NormLength;
            var h_mm = (q1 - q3).NormLength;
            double w_res = w_mm / (w_pix + 1e-6);
            double h_res = h_mm / (h_pix + 1e-6);
            _mmPerPixel = new QVector(w_res, h_res);
            #endregion

            return err;
        }
        public ErrCodes CalcChipDimension(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            return CalcChipDimension_002(out dimension, lines, chipData);
        }
        ErrCodes CalcChipDimension_000(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            dimension = SizeF.Empty;

            // (0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var cc = chipData.ChipBox2D.Center;
            var runtimeChipCenter = new QVector(cc.X, cc.Y);

            // (1) 取得 measurePts (local scope) 順序: 左0 上1 右2 下3
            var localLines = Array.ConvertAll(lines, line => new EzLSD.LineSegment(line.P1 - runtimeChipCenter, line.P2 - runtimeChipCenter));
            var workLines = transform(localLines, _mmPerPixel.X, _mmPerPixel.Y);
            var dimMeasurePts = getMidMeasurePoints(workLines);

            // (1) PAD 比例修正
            double scaleW = 1.0;
            double scaleH = 1.0;
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                // Golden PadsCorner (local scop)
                var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                if (goldenPadsCorners != null)
                {
                    var goldenMpts = getMidMeasurePoints(goldenPadsCorners);
                    var goldenPadW = (goldenMpts[0] - goldenMpts[2]).NormLength;
                    var goldenPadH = (goldenMpts[1] - goldenMpts[3]).NormLength;

                    // Runtime PadsCorners (local scope)
                    EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                    var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                    var runMpts = getMidMeasurePoints(runPadsCorners);
                    var runPadW = (runMpts[0] - runMpts[2]).NormLength;
                    var runPadH = (runMpts[1] - runMpts[3]).NormLength;

                    scaleW = (goldenPadW / runPadW);
                    scaleH = (goldenPadH / runPadH);
                }
            }

            // (2) 計算距離
            double W = (dimMeasurePts[0] - dimMeasurePts[2]).NormLength;
            double H = (dimMeasurePts[1] - dimMeasurePts[3]).NormLength;
            W *= scaleW;
            H *= scaleH;

            // (3) Results
            dimension.Width = (float)Math.Round(W, 3);
            dimension.Height = (float)Math.Round(H, 3);
            return ErrCodes.OK;
        }
        ErrCodes CalcChipDimension_001(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            dimension = SizeF.Empty;

            //(0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var cc = chipData.ChipBox2D.Center;
            var runtimeChipCenter = new QVector(cc.X, cc.Y);

            //(2) 取得 measurePts (local scope) 順序: 左0 上1 右2 下3
            var localLines = Array.ConvertAll(lines, line => new EzLSD.LineSegment(line.P1 - runtimeChipCenter, line.P2 - runtimeChipCenter));
            var workLines = transform(localLines, _localTrf);
            var dimMeasurePts = getMidMeasurePoints(workLines);

            // (1) PAD 比例修正
            double scaleW = 1.0;
            double scaleH = 1.0;
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                // Golden PadsCorner (local scop)
                var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                if (goldenPadsCorners != null)
                {
                    var goldenMpts = getMidMeasurePoints(goldenPadsCorners);
                    var goldenPadW = (goldenMpts[0] - goldenMpts[2]).NormLength;
                    var goldenPadH = (goldenMpts[1] - goldenMpts[3]).NormLength;

                    // Runtime PadsCorners (local scope)
                    EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                    var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                    var runMpts = getMidMeasurePoints(runPadsCorners);
                    var runPadW = (runMpts[0] - runMpts[2]).NormLength;
                    var runPadH = (runMpts[1] - runMpts[3]).NormLength;

                    scaleW = (goldenPadW / runPadW);
                    scaleH = (goldenPadH / runPadH);
                }
            }

            // (2) 計算距離
            double W = (dimMeasurePts[0] - dimMeasurePts[2]).NormLength;
            double H = (dimMeasurePts[1] - dimMeasurePts[3]).NormLength;
            W *= scaleW;
            H *= scaleH;

            // (3) Results
            dimension.Width = (float)Math.Round(W, 3);
            dimension.Height = (float)Math.Round(H, 3);
            return ErrCodes.OK;
        }
        ErrCodes CalcChipDimension_002(out SizeF dimension, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            dimension = SizeF.Empty;

            //(0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            //(1) ROI offset
            var cc = chipData.ChipBox2D.Center;
            var runtimeChipCenter = new QVector(cc.X, cc.Y);

            //(2) 取得 measurePts (local scope) 順序: 左0 上1 右2 下3
            var localLines = Array.ConvertAll(lines, line => new EzLSD.LineSegment(line.P1 - runtimeChipCenter, line.P2 - runtimeChipCenter));
            var workLines = transform(localLines, _localTrf);
            var dimMeasurePts = getMidMeasurePoints(workLines);

            // (1) PAD Local Transform
            var padsGrid = chipData.PadsGrid;
            if (padsGrid != null)
            {
                // Golden PadsCorner (local scop)
                var goldenPadsCorners = _padsTrf?.GetCalibCornerPoints()?.GetAll(isSrc: true);
                if (goldenPadsCorners != null)
                {
                    int NP = 4;
                    // Runtime PadsCorners (local scope)
                    EzBlocsGridAnalyzer.CalcRotatedBox2D(padsGrid, out var runtimePadsBox2D, false);
                    var runPadsCorners = Array.ConvertAll(runtimePadsBox2D.Corners, c => new QVector(c.X, c.Y) - runtimeChipCenter);
                    var trf = new QTransform("RunTimePadTrf");
                    var trfCalib = trf.GetCalibCornerPoints();
                    for (int i = 0; i < NP; i++)
                        trfCalib.Set(i, runPadsCorners[i], goldenPadsCorners[i]);
                    bool ok = trf.Build();
                    if (!ok)
                    {
                        return CalcChipDimension_001(out dimension, lines, chipData);
                    }

                    for (int i = 0; i < NP; i++)
                        dimMeasurePts[i] = trf.Trans(dimMeasurePts[i]);
                }
            }

            // (2) 計算距離
            double W = (dimMeasurePts[0] - dimMeasurePts[2]).NormLength;
            double H = (dimMeasurePts[1] - dimMeasurePts[3]).NormLength;

            // (3) Results
            dimension.Width = (float)Math.Round(W, 3);
            dimension.Height = (float)Math.Round(H, 3);
            return ErrCodes.OK;
        }

        public ErrCodes CalcPadEdgeSizes(out double[] padEdgeSizes, EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            int NP = 4;
            padEdgeSizes = new double[NP];

            // (0) check lines condition
            var err = check(lines, chipData);
            if (err != ErrCodes.OK)
                return err;

            var chipBox2D = chipData?.ChipBox2D;
            if (chipBox2D == null)
                return ErrCodes.ERR_NO_CHIP_LOCATION;

            //晶格角點: 0左上, 1右上, 2右下, 3左下
            var corners = Array.ConvertAll(chipBox2D.Corners, c => new QVector(c.X, c.Y));
            //晶格左 點平均: (左上 + 左下) / 2
            var left = (corners[0] + corners[3]) / 2;
            //晶格右點 平均: (右上 + 右下) / 2
            var right = (corners[1] + corners[2]) / 2;
            //晶格上點 平均: (左上 + 右上) / 2
            var top = (corners[0] + corners[1]) / 2;
            //晶格下點 平均: (左下 + 右下) / 2
            var bottom = (corners[2] + corners[3]) / 2;

            //轉換到 world
            left = _localTrf.Trans(left);
            top = _localTrf.Trans(top);
            right = _localTrf.Trans(right);
            bottom = _localTrf.Trans(bottom);
            lines = transform(lines, _localTrf);

            var lineL = lines[0];   // 左邊線
            var lineT = lines[1];   // 上邊線
            var lineR = lines[2];   // 右邊線
            var lineB = lines[3];   // 下邊線

            if (lineL != null && lineR != null)
            {
                //晶格 左邊緣 厚度 = 晶格左點 至 左邊線(line0) 距離
                var edge_left = lineL.CalcDistance(left);
                //晶格 右邊緣 厚度 = 晶格右點 至 右邊線(line2) 距離
                var edge_right = lineR.CalcDistance(right);
                //記入 結果
                padEdgeSizes[(int)EdgeBorder.Left] = (float)Math.Round(edge_left, 3);
                padEdgeSizes[(int)EdgeBorder.Right] = (float)Math.Round(edge_left, 3);
            }

            if (lineT != null && lineB != null)
            {
                //晶格 上邊緣 厚度 = 晶格上點 至 上邊線(line1) 距離
                var edge_top = lineT.CalcDistance(top);
                //晶格 下邊緣 厚度 = 晶格下點 至 下邊線(line3) 距離
                var edge_bottom = lineB.CalcDistance(bottom);
                //記入 結果
                padEdgeSizes[(int)EdgeBorder.Top] = (float)Math.Round(edge_top, 3);
                padEdgeSizes[(int)EdgeBorder.Bottom] = (float)Math.Round(edge_bottom, 3);
            }

            return ErrCodes.OK;
        }
        public bool GetDimMeasurePoints(EzLSD.LineSegment[] lines, out QVector[] dimMeasurePts, ITransform trf = null)
        {
            // 順序: 左0 上1 右2 下3
            if (trf != null)
                lines = transform(lines, trf);
            dimMeasurePts = getMidMeasurePoints(lines);
            return checkLines(lines) == ErrCodes.OK;
        }

        #region HELPER_FUNCTIONS
        ErrCodes check(EzLSD.LineSegment[] lines, GaChipData chipData)
        {
            if (chipData == null || chipData.ChipBox2D == null)
                return ErrCodes.ERR_NO_CHIP_LOCATION;
            return checkLines(lines);
        }
        ErrCodes checkLines(EzLSD.LineSegment[] lines)
        {
            int count = 0;
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (line != null)
                        count++;
                }
            }
            return count >= 4 ? ErrCodes.OK : ErrCodes.ERR_WEAK_LINE_CONDITION;
        }
        QVector[] getMidMeasurePoints(QVector[] corners)
        {
            var mpts = new QVector[4];
            mpts[0] = (corners[3] + corners[0]) / 2;    //mid left
            mpts[1] = (corners[0] + corners[1]) / 2;    //mid top
            mpts[2] = (corners[1] + corners[2]) / 2;    //mid right
            mpts[3] = (corners[2] + corners[3]) / 2;    //mid bottom
            return mpts;
        }
        QVector[] getMidMeasurePoints(EzLSD.LineSegment[] lines)
        {
            // 順序: 左上右下
            var measurePts = new QVector[4];

            #region 寬方向
            bool okW = false;
            try
            {
                var line0 = lines[0];     //左邊線
                var line2 = lines[2];     //右邊線
                if (line0 != null && line2 != null)
                {
                    //(1) 取點
                    var P = line0.GetMidPoint();
                    var Q = line2.CalcTheNearestPoint(P);

                    //(2) Result
                    measurePts[0] = P;
                    measurePts[2] = Q;
                    okW = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "GetDimMeasurePoints (W) 異常");
                okW = false;
            }
            #endregion

            #region 高方向
            bool okH = false;
            try
            {
                var line1 = lines[1];     //上邊線
                var line3 = lines[3];     //下邊線
                if (line1 != null && line3 != null)
                {
                    //(1) 取點
                    var P = line1.GetMidPoint();
                    var Q = line3.CalcTheNearestPoint(P);

                    //(2) Result
                    measurePts[1] = P;
                    measurePts[3] = Q;
                    okH = true;
                }
            }
            catch (Exception ex)
            {
                _LOG_ERROR(ex, "GetDimMeasurePoints (H) 異常");
                okH = false;
            }
            #endregion

            return measurePts;
        }
        EzLSD.LineSegment[] transform(EzLSD.LineSegment[] lines, ITransform trf)
        {
            if (lines == null)
                return null;
            int N = lines.Length;
            var results = new EzLSD.LineSegment[N];
            for (int i = 0; i < N; i++)
            {
                var line = lines[i];
                if (line == null) continue;
                var P1 = trf.Trans(line.P1);
                var P2 = trf.Trans(line.P2);
                results[i] = new EzLSD.LineSegment(P1, P2);
            }
            return results;
        }
        EzLSD.LineSegment[] transform(EzLSD.LineSegment[] lines, double scaleX, double scaleY)
        {
            if (lines == null)
                return null;
            int N = lines.Length;
            var results = new EzLSD.LineSegment[N];
            for (int i = 0; i < N; i++)
            {
                var line = lines[i];
                if (line == null) continue;
                double x1 = line.P1.X * scaleX;
                double y1 = line.P1.Y * scaleY;
                double x2 = line.P2.X * scaleX;
                double y2 = line.P2.Y * scaleY;
                var P1 = new QVector(x1, y1);
                var P2 = new QVector(x2, y2);
                results[i] = new EzLSD.LineSegment(P1, P2);
            }
            return results;
        }
        #endregion

        #region LOG_FUNCTIONS
        void _LOG_ERROR(Exception ex, string msg)
        {
            GaUtil.LOG_ERROR(ex, msg);
        }
        #endregion

        private string getRecipeIniFileName()
        {
            var _xRecipe = RecipeFPIX3Class.Instance;
            CarrierEnum C = _xRecipe.ActiveCarrierID;
            string path = System.IO.Path.GetDirectoryName(_xRecipe.INIFILE);
            string iniFileName = System.IO.Path.Combine(path, $"micro_transform@{C}.ini");
            return iniFileName;
        }
    }
}
