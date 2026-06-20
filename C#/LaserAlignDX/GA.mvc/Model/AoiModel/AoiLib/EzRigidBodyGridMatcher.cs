#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-16 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.OpenCV;
using JetEazy.QvMath;
using JetEazy.QxCollections;
using JetEazy.Utils;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using CvSize = OpenCvSharp.Size;
using EzAoiBase = EzAoiEmptyTrayInspector.Model.Aoi.EzAoiBase;

namespace LeTian.AoiLib
{
    public partial class EzRigidBodyGridMatcher : EzAoiBase, IDisposable
    {
        #region PRIVATE_GOLDEN_DATA
        Mat _goldenImg;                     // in non-shrink domain
        RigidBody _goldenRigidBody;
        EzBlocsGrid _goldenGrid => _goldenRigidBody?.Grid;
        #endregion

        #region PRIVATE_MEMBERS
        EzPadsGridFinder _padsFinder;
        #endregion

        public EzRigidBodyGridMatcher(int shrink = 1)
        {
            _shrinkFactor = Math.Max(shrink, 1);
            _padsFinder = new EzPadsGridFinder(_shrinkFactor);
        }
        public void Dispose()
        {
            _goldenImg?.Dispose();
            _goldenImg = null;
            _goldenRigidBody?.Dispose();
            _goldenRigidBody = null;
        }
        
        public RigidBody GetGoldenBody()
        {
            return _goldenRigidBody;
        }
        public RigidBody DetachGoldenBody()
        {
            var body = _goldenRigidBody;
            _goldenRigidBody = null;
            return body;
        }

        public void SetGoldenTemplate(Bitmap bmpGolden)
        {
            Bitmap bmpU8 = GaImageUtil.ToU8(bmpGolden);

            using (var bridge = new QxImageBridge(bmpU8))
            {
                SetGoldenTemplate(bridge.Image);
            }

            if (bmpU8 != bmpGolden)
                bmpU8?.Dispose();
        }
        public void SetGoldenTemplate(Mat imgGolden)
        {
            analyzeGoldenTemplate(imgGolden);
        }

        /// <summary>
        /// 顯示 Golden Template 特徵圖 (調試用)
        /// </summary>
        public void ShowGoldenGridVisualizer(bool show = true)
        {
            if (!show || _goldenImg == null || _goldenRigidBody == null || _goldenGrid == null)
                return;

            string title = $"GOLDEN [{_goldenGrid.Rows},{_goldenGrid.Cols}]";
            VxDebugDrawer.Draw(_goldenImg, (QvBox2D)null, _goldenGrid, _goldenRigidBody.KeyRow, _goldenRigidBody.KeyCol, Scalar.Lime, title);
        }

        public bool LargeAngleEnabled
        {
            get;
            set;
        }
        public int PadThreshold
        {
            get => _padsFinder.PadThreshold;
            set => _padsFinder.PadThreshold = value;
        }
        public int DistTransThreshold
        {
            get => _padsFinder.DistTransThreshold;
            set => _padsFinder.DistTransThreshold = value;
        }

        public RigidBody FindBestMatch(Bitmap bmpScene, string debugDumpFile = null)
        {
            Bitmap bmpU8 = GaImageUtil.ToU8(bmpScene);
            RigidBody result;

            using (var bridge = new QxImageBridge(bmpU8))
            {
                result = FindBestMatch(bridge.Image, debugDumpFile);
            }

            if (bmpU8 != bmpScene)
                bmpU8?.Dispose();
            return result;
        }
        public RigidBody FindBestMatch(Mat imgScene, string debugDumpFile = null)
        {
            if (_goldenGrid == null)
                return null;

            #region DEBUG_TRACE
            string debugTitle = null;
            if (debugDumpFile != null)
            {
                debugTitle = System.IO.Path.GetFileName(debugDumpFile);
                debugDumpFile = System.IO.Path.ChangeExtension(debugDumpFile, ".png");
                _padsFinder.FindBlocs(imgScene, out var blocs, debugDumpFile);
                VxDebugDrawer.Draw(imgScene, blocs, true, Scalar.Cyan, $"Points = {blocs.Count} @ {debugTitle}");
            }
            #endregion

            _padsFinder.FindPadsGrid(imgScene, out var sceneGrid, out int kr, out int kc, out double kSQ);

            bool needsRebuild = false;
            if (sceneGrid != null)
            {
                var goldenPitch = _goldenGrid.GetPitch();
                var scenePitch = sceneGrid.GetPitch();
                var pitchRatioX = Math.Abs(scenePitch.X / goldenPitch.X);
                var pitchRatioY = Math.Abs(scenePitch.Y / goldenPitch.Y);

                if (false)
                {
                    _LOG.Debug("Golden Pitch = {0}", goldenPitch);
                    _LOG.Debug("Scene Pitch = {0}", scenePitch);
                    _LOG.Debug("Scene Pitch Ratio = {0:0.000}, {1:0.000}", pitchRatioX, pitchRatioY);
                }

                if (pitchRatioX >= 1.4 || pitchRatioY >= 1.4)
                {
                    needsRebuild = true;
                }
                else
                {
                    if (sceneGrid.Rows < _goldenGrid.Rows || sceneGrid.Cols < _goldenGrid.Cols)
                    {
                        var boundary = new Rectangle(0, 0, imgScene.Width, imgScene.Height);
                        var rect = sceneGrid.GetBoundary();
                        rect.Inflate(2, 2);
                        bool is_clipped = JetEazy.QUtilities.QUtility.ClipBoundary(ref rect, ref boundary);
                        if (is_clipped)
                            needsRebuild = true;
                    }
                }
            }
            else
            {
                needsRebuild = true;
            }

            if (needsRebuild)
            {
                // 旋轉 -45度, +45度 重新抓
                EzBlocsGrid grid2 = null;

                var angle90s = LargeAngleEnabled ?
                    new double[] { 0, -90, 90 } :
                    new double[] { 0 };

                bool isFound = false;

                foreach (var angle90 in angle90s)
                {
                    foreach (var angleD in new double[] { 0, -45, 45, -22.5, 22.5 })
                    {
                        double angle = angle90 + angleD;
                        if (Math.Abs(angle) < 1)
                            continue;

                        grid2?.Dispose();

                        _padsFinder.RebuildPadsGrid(imgScene, angle, out grid2, out kr, out kc, out kSQ);

                        _LOG.Warn("Rebuild @ {0:0.00}°", angle);
                        if (kr == _goldenRigidBody.KeyRow && kc == _goldenRigidBody.KeyCol)
                        {
                            isFound = true;
                            break;
                        }
                    }

                    if (isFound)
                        break;
                }

                // DEBUG:
                // CvDebugDrawer.Draw(imgScene, null, grid2, kr, kc, Scalar.DarkOliveGreen, $"ROTATE [{grid2.Rows}x{grid2.Cols}] = {grid2.GetMajorCount()} @ {debugTitle}");
                // return;

                sceneGrid?.Dispose();
                sceneGrid = grid2;
            }

            if (sceneGrid == null)
                return null;

            #region DEBUG_TRACE
            if (debugDumpFile != null)
            {
                VxDebugDrawer.Draw(imgScene, (QvBox2D)null, sceneGrid, kr, kc, Scalar.Blue, $"Grid [{sceneGrid.Rows}x{sceneGrid.Cols}] = {sceneGrid.GetMajorCount()} @ {debugTitle}");
            }
            #endregion

            var rigitBody = findBestGridMatch(imgScene, sceneGrid, out var bestGrid, out var box2D, debug: debugDumpFile != null);

            if (rigitBody != null)
            {
                //EzBlocsGridAnalyzer.CalcRotatedBox2D(rigitBody.Grid, out box2D, useBoundaryPoints: false);
                //rigitBody.Box2D = box2D;

                // 缺兩個 pad
                if (rigitBody.Score < -200)
                    rigitBody = null;
            }

            if (rigitBody != null)
            {
                findGridCornersBox2D(imgScene, rigitBody.Grid);
            }

            return rigitBody;
        }

        #region PRIVATE_SEARCH_FUNCTIONS
        void analyzeGoldenTemplate(Mat goldenImg)
        {
            _goldenImg?.Dispose();
            _goldenImg = goldenImg.Clone();
            _goldenRigidBody?.Grid?.Dispose();

            _padsFinder.GoldenPadSize = CvSize.Zero;
            _padsFinder.GoldenGrid = null;
            _padsFinder.FindPadsGrid(
                    _goldenImg, 
                    out var grid, 
                    out int keyRow, 
                    out int keyCol, 
                    out double keySQ);

            if (grid != null)
            {
                grid.RowMin = 0;
                grid.ColMin = 0;

                _goldenRigidBody = new RigidBody(grid, keyRow, keyCol, keySQRatio: keySQ);
                _goldenRigidBody.getMinMaxBlocSize(out var sizeMin, out var sizeMax);
                _padsFinder.GoldenPadSize = sizeMax;
                _padsFinder.GoldenGrid = grid;
            }
        }
        RigidBody findBestGridMatch(Mat imgScene, EzBlocsGrid sceneGrid, out EzBlocsGrid bestGrid, out QvBox2D bestBox2D, bool debug = false)
        {
            RigidBody bestBody = null;
            bestBox2D = null;
            bestGrid = null;

            double bestPenalty = double.MaxValue;
            int bestRowOffset = 0;
            int bestColOffset = 0;

            int goldenPixels = Math.Max(_goldenRigidBody.KeyPixels, 1);
            var goldenGrid = _goldenRigidBody?.Grid;
            EzBlocsGridAnalyzer.CalcRotatedBox2D(goldenGrid, out var goldenBox2D, true);


            Rect sceneBoundary = new Rect(0, 0, imgScene.Width, imgScene.Height);
            sceneGrid.RowMin = 0;
            sceneGrid.ColMin = 0;
            goldenGrid.RowMin = 0;
            goldenGrid.ColMin = 0;

            int deltaRows = sceneGrid.Rows - goldenGrid.Rows;
            int deltaCols = sceneGrid.Cols - goldenGrid.Cols;
            var gridChecker = new EzBlocsGridAnalyzer();

            if (debug)
            {
                string title = $"GOLDEN [{goldenGrid.Rows},{goldenGrid.Cols}] gx={goldenPixels}";
                VxDebugDrawer.Draw(_goldenImg, goldenBox2D, goldenGrid, _goldenRigidBody.KeyRow, _goldenRigidBody.KeyCol, Scalar.Orange, title);
            }

            //Mat binary = new Mat();
            //Cv2.Threshold(imgScene, binary, 0, 255, ThresholdTypes.Otsu);

            for (int rowOffset = 0; rowOffset <= deltaRows; rowOffset++)
            {
                for (int colOffset = 0; colOffset <= deltaCols; colOffset++)
                {
                    int r0 = rowOffset;
                    int c0 = colOffset;
                    int r1 = r0 + goldenGrid.Rows;
                    int c1 = c0 + goldenGrid.Cols;
                    r1 = Math.Min(r1, sceneGrid.Rows);
                    c1 = Math.Min(c1, sceneGrid.Cols);

                    //var sceneSubGrid = new SubGrid(sceneGrid, r0, c0, r1, c1);
                    var sceneSubGrid = sceneGrid.Slice(r0, c0, r1, c1);
                    double penalty = 0;

                    // (0) ROI of sceneSubGrid
                    EzBlocsGridAnalyzer.CalcRotatedBox2D(sceneSubGrid, out var box2d, useBoundaryPoints: true);
                    //var bxSize = goldenBox2D.MinAreaRectSize;
                    //var imSize = _goldenImg.Size();
                    //float inflate_x = imSize.Width - bxSize.Width;
                    //float inflate_y = imSize.Height - bxSize.Height;
                    ////var firstBloc = goldenGrid[0, 0];
                    ////var ext_x = firstBloc.CenterX;
                    ////var ext_y = firstBloc.CenterY;
                    //var size = box2d.MinAreaRectSize;
                    //size.Width += inflate_x;
                    //size.Height += inflate_y;
                    //box2d.MinAreaRectSize = size;
                    Rect subRoi = JetEazy.Qcvt.CV(Rectangle.Round(box2d.BoundaryRect));

                    // (1) 檢查 邊界
                    if (JetEazy.Qcvt.ClipBoundary(ref subRoi, ref sceneBoundary))
                        penalty += 1000f;

                    // (2) 檢查 格點空洞 不一致
                    int logicDiff = gridChecker.CalcLogicDiff(sceneSubGrid, goldenGrid);
                    if (logicDiff > 0)
                        penalty += logicDiff * 100f;

                    // (3) 檢查 KeyPad (KeyRow && KeyCol) 不一致
                    _padsFinder.FindSpecialKeyPad(imgScene, sceneSubGrid, out int keyRow, out int keyCol, out double keySQ);
                    if (keyRow != _goldenRigidBody.KeyRow || keyCol != _goldenRigidBody.KeyCol)
                        penalty += 10f;

                    // (4) 檢查 SQ ratio 缺角
                    penalty += keySQ;

                    // (5) 自我檢查 出格錯排
                    gridChecker.ScanDefects(sceneSubGrid, out Mat defectsMat, 1f);
                    double defects = defectsMat.Sum().Val0;
                    defects = Math.Round(defects, 3);
                    penalty += defects;

                    // (6) 保留最佳值
                    if (bestPenalty > penalty)
                    {
                        bestPenalty = penalty;
                        bestColOffset = colOffset;
                        bestRowOffset = rowOffset;
                        bestBox2D = box2d;
                        bestGrid = sceneSubGrid;
                        bestBody = new RigidBody(bestGrid, keyRow, keyCol, keySQRatio: keySQ);
                        bestBody.Score = 1.0 - bestPenalty;
                    }

                    if (debug)
                    {
                        //string title = $"DEBUG_{rowOffset}_{colOffset} [{sceneSubGrid.Rows},{sceneSubGrid.Cols}] penalty={penalty:0.000}, defects={defects:0.000}, keyPD={pixDiff:0.000}";
                        string title = $"DEBUG_{rowOffset}_{colOffset} [{sceneSubGrid.Rows},{sceneSubGrid.Cols}] penalty={penalty:0.000}, defects={defects:0.000}, kSQ={keySQ:0.000}";
                        VxDebugDrawer.Draw(imgScene, (QvBox2D)null, sceneSubGrid, keyRow, keyCol, Scalar.Magenta, title);
                    }
                }
            }

            return bestBody;
        }

        void findGridCornersBox2D(Mat imgScene, EzBlocsGrid sceneGrid)
        {
            if (sceneGrid == null)
                return;

            var cornerPads = sceneGrid.GetCornerBlocs();
            var bound = new Rect(0, 0, imgScene.Width, imgScene.Height);
            var thres = PadThreshold;

            for (int idx = 0, len = cornerPads.Length; idx < len; idx++)
            {
                var padBloc = cornerPads[idx];
                if (padBloc == null)
                    continue;

                var roi = JetEazy.Qcvt.CV(cornerPads[idx].Rect);
                roi.Inflate(2, 2);
                JetEazy.Qcvt.ClipBoundary(ref roi, ref bound);

                QvBox2D box2D = null;
                using (var binary = new Mat())
                {
                    if (thres > 0)
                        Cv2.Threshold(imgScene[roi], binary, thres, 255, ThresholdTypes.Binary);
                    else
                        Cv2.Threshold(imgScene[roi], binary, 0, 255, ThresholdTypes.Otsu);

                    Cv2.Erode(binary, binary, null, iterations: 1);
                    Cv2.Dilate(binary, binary, null, iterations: 1);

                    // 1. 提取輪廓
                    Cv2.FindContours(binary, out var contours, out var hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    if (contours.Length == 0)
                        continue;

                    // 2. 遍歷每一個輪廓並計算其 MinAreaRect
                    double maxArea = 0;
                    for (int k = 0; k < contours.Length; k++)
                    {
                        // MinAreaRect 要求輪廓的點數至少為 5。
                        // 如果點數太少，MinAreaRect 可能無法準確計算或拋出錯誤。
                        try
                        {
                            var contour = contours[k];
                            if (contour.Length >= 4)
                            {
                                RotatedRect rotatedRect = Cv2.MinAreaRect(contour);
                                var size = rotatedRect.Size;
                                var area = size.Width * size.Height;
                                if (area > maxArea)
                                {
                                    maxArea = area;
                                    box2D = new QvBox2D();
                                    rotatedRect.Center.X += roi.X;
                                    rotatedRect.Center.Y += roi.Y;
                                    box2D.SetBox(rotatedRect);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                }
                
                // 直接存回到 EzBloc.ExtraBox2D
                padBloc.ExtraBox2D = box2D;
            }

        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
#if (OPT_RESERVED)
        void convert(IxGridMap<EzBloc> grid, out EzBlocsGrid grid2)
        {
            if (grid == null)
            {
                grid2 = null;
                return;
            }
            
            var blocs = new List<EzBloc>(iterBlocs(grid));
            foreach (var b in blocs)
                b.Owner = null;

            var builder = new EzBlocsGridBuilder();
            grid2 = builder.Build(blocs);
        }
#endif
        IEnumerable<EzBloc> iterBlocs(IxGridMap<EzBloc> grid)
        {
            if (grid != null)
            {
                //---------------------------------------------------------
                // NOTE:　不要使用 grid 預設的 Enumerator
                //---------------------------------------------------------
                //foreach (var bloc in grid)
                //{
                //    if (bloc != null && bloc.IsMajorNode())
                //        yield return bloc;
                //}
                //yield break;

                for (int row = grid.RowMin; row < grid.RowMax; row++)
                {
                    for (int col = grid.ColMin; col < grid.ColMax; col++)
                    {
                        var bloc = grid.Get(row, col);
                        if (bloc != null && bloc.IsMajorNode())
                            yield return bloc;
                    }
                }
            }
        }
        #endregion
    }


    partial class EzRigidBodyGridMatcher
    {
        public class RigidBody : IDisposable
        {
            public EzBlocsGrid Grid               // in non-shrink domain
            {
                get; private set;
            }
            public EzBloc KeyBloc
            {
                get
                {
                    var bloc = Grid?.Get(KeyRow, KeyCol);
                    if (bloc.IsMajorNode())
                        return bloc;
                    return null;
                }
            }
            public int KeyRow
            {
                get; private set;
            }
            public int KeyCol
            {
                get; private set;
            }
            public int KeyPixels
            {
                get; private set;
            }
            public double KeySQRatio
            {
                get; private set;
            }
            public double Score
            {
                get; internal set;
            }


            public RigidBody(EzBlocsGrid grid, int keyRow, int keyCol, int keyPixels = 0, double keySQRatio = 1)
            {
                Grid = grid;
                KeyRow = keyRow;
                KeyCol = keyCol;
                KeyPixels = keyPixels;
                KeySQRatio = keySQRatio;
            }

            public void Dispose()
            {
                Grid?.Dispose();
                Grid = null;
            }

            ///// <summary>
            ///// 如果 晶粒表面 與 載台 不平行, 則 Box2D 不會完美 切齊 pads 
            ///// </summary>
            ///// <param name="useBoundaryPoints"></param>
            //public QvBox2D CalcBox2D(bool useBoundaryPoints = false)
            //{
            //    EzBlocsGridAnalyzer.CalcRotatedBox2D(this.Grid, out var box2D, useBoundaryPoints);
            //    return box2D;
            //}

            /// <summary>
            /// 取的 最小 pad, 與最大 pad 的 size
            /// </summary>
            /// <param name="sizeMin"></param>
            /// <param name="sizeMax"></param>
            public void getMinMaxBlocSize(out CvSize sizeMin, out CvSize sizeMax)
            {
                sizeMin = new CvSize(int.MaxValue, int.MaxValue);
                sizeMax = new CvSize(0, 0);
                if (Grid == null)
                {
                    sizeMin = new CvSize(0, 0);
                    return;
                }
                foreach (var bloc in Grid.IterMajorBlocs())
                {
                    sizeMin.Width = Math.Min(sizeMin.Width, bloc.Rect.Width);
                    sizeMin.Height = Math.Min(sizeMin.Height, bloc.Rect.Height);
                    sizeMax.Width = Math.Max(sizeMax.Width, bloc.Rect.Width);
                    sizeMax.Height = Math.Max(sizeMax.Height, bloc.Rect.Height);
                }
            }
        }
    }
}
