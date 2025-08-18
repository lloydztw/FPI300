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
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using CvSize = OpenCvSharp.Size;
using EzAoiBase = EzAoiEmptyTrayInspector.Model.Aoi.EzAoiBase;


namespace LeTian.Match
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
            _goldenRigidBody?.Grid?.Dispose();
            _goldenRigidBody = null;
        }

        public void SetGoldenTemplate(Bitmap bmpGolden)
        {
            using (var bridge = new QxImageBridge(bmpGolden))
            {
                analyzeGoldenTemplate(bridge.Image);
            }
        }
        public void SetGoldenTemplate(Mat imgGolden)
        {
            analyzeGoldenTemplate(imgGolden);
        }

        public RigidBody FindBestMatch(Bitmap bmpScene, string debugDumpFile = null)
        {
            using (var bridge = new QxImageBridge(bmpScene))
            {
                return FindBestMatch(bridge.Image, debugDumpFile);
            }
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
                _LOG.Debug("Golden Pitch = {0}", goldenPitch);
                _LOG.Debug("Scene Pitch = {0}", scenePitch);
                _LOG.Debug("Scene Pitch Ratio = {0:0.000}, {1:0.000}", pitchRatioX, pitchRatioY);
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
                foreach (var angle in new double[] { -45.0, 45.0, - 22.5, 22.5 })
                {
                    grid2?.Dispose();

                    _padsFinder.RebuildPadsGrid(imgScene, angle, out grid2, out kr, out kc, out kSQ);

                    _LOG.Warn("Rebuild @ {0:0.00}°", angle);
                    if (kr == _goldenRigidBody.KeyRow && kc == _goldenRigidBody.KeyCol)
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
                VxDebugDrawer.Draw(imgScene, null, sceneGrid, kr, kc, Scalar.Blue, $"Grid [{sceneGrid.Rows}x{sceneGrid.Cols}] = {sceneGrid.GetMajorCount()} @ {debugTitle}");
            }
            #endregion

            var rigitBody = findBestGridMatch(imgScene, sceneGrid, out var bestGrid, out var box2D, debug: debugDumpFile != null);

            if (rigitBody != null)
            {
                EzBlocsGridAnalyzer.CalcRotatedBox2D(rigitBody.Grid, out box2D, useBoundaryPoints: false);
                rigitBody.Box2D = box2D;
            }

            return rigitBody;
        }
        public RigidBody GetGoldenBody()
        {
            return _goldenRigidBody;
        }

        #region PRIVATE_SEARCH_FUNCTIONS
        void analyzeGoldenTemplate(Mat goldenImg)
        {
            _goldenImg?.Dispose();
            _goldenImg = goldenImg.Clone();
            _goldenRigidBody?.Grid?.Dispose();

            _padsFinder.GoldenPadSize = CvSize.Zero;
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

                    // (3) 檢查 KeyPad 不一致
                    //EzPadsGridFinder.FindSpecialKeyPad(sceneSubGrid, out int keyRow, out int keyCol, out int keyPixels);
                    EzPadsGridFinder.FindSpecialKeyPad(imgScene, sceneSubGrid, out int keyRow, out int keyCol, out double keySQ);
                    if (keyRow != _goldenRigidBody.KeyRow || keyCol != _goldenRigidBody.KeyCol)
                        penalty += 10f;

                    //// (4) 檢查 KeyPixels (因為投影關係, 不是很準!)
                    //double pixDiff = ((double)Math.Abs(keyPixels - goldenPixels)) / goldenPixels;
                    //pixDiff = Math.Round(pixDiff, 3);
                    //penalty += pixDiff;

                    // (5) 檢查 SQ ratio 缺角 (保留)
                    penalty += keySQ;

                    // (6) 自我檢查 出格錯排 (保留)
                    gridChecker.ScanDefects(sceneSubGrid, out Mat defectsMat, 1f);
                    double defects = defectsMat.Sum().Val0;
                    defects = Math.Round(defects, 3);
                    penalty += defects;

                    // (7) 保留最佳值
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
                        VxDebugDrawer.Draw(imgScene, null, sceneSubGrid, keyRow, keyCol, Scalar.Magenta, title);
                    }
                }
            }

            return bestBody;
        }
        #endregion

        #region PRIVATE_HELPER_FUNCTIONS
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
        public class RigidBody
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

            /// <summary>
            /// 如果是 Perspective Transform, 則會不準
            /// </summary>
            public QvBox2D Box2D
            {
                get;
                internal set;
            }

            internal void getMinMaxBlocSize(out CvSize sizeMin, out CvSize sizeMax)
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

            public RigidBody(EzBlocsGrid grid, int keyRow, int keyCol, int keyPixels = 0, double keySQRatio = 1)
            {
                Grid = grid;
                KeyRow = keyRow;
                KeyCol = keyCol;
                KeyPixels = keyPixels;
                KeySQRatio = keySQRatio;
            }
        }
    }
}
