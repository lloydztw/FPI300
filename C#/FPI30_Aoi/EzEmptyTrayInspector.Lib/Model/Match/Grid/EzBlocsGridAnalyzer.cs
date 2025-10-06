#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-09 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QvMath;
using JetEazy.QxCollections;
using JetEazy.QxCollections2;
using OpenCvSharp;
using System;
using System.Collections.Generic;


namespace JetEazy.Match
{
    public class EzBlocsGridAnalyzer
    {
        #region PRIVATE_RUNTIME_DATA
        IxGridMap<EzBloc> _grid;
        int _rowMin => _grid.RowMin;
        int _rowMax => _grid.RowMax;
        int _colMin => _grid.ColMin;
        int _colMax => _grid.ColMax;
        #endregion

        /// <summary>
        /// 使用 QvBox2D 可以免除 OpenCV RotatedRect 長短邊造成角度定義不同的效應
        /// </summary>
        public static void CalcRotatedBox2D(IxGridMap<EzBloc> grid, out QvBox2D box2d, bool useBoundaryPoints = false)
        {
            // 注意: Cv2.MinAreaRect 無法反映 透視投影 效果 !!!
            var rotRect = useBoundaryPoints ? 
                Cv2.MinAreaRect(IterBoundaryPoints(grid)):
                Cv2.MinAreaRect(IterCentroids(grid));
            box2d = new QvBox2D();
            box2d.SetBox(rotRect);
        }

        /// <summary>
        /// 格位內容物不同之計數
        /// </summary>
        public int CalcLogicDiff(IxGridMap<EzBloc> grid, IxGridMap<EzBloc> goldenGrid)
        {
            int count = 0;
            for (int r = goldenGrid.RowMin; r < goldenGrid.RowMax; r++)
            {
                for (int c = goldenGrid.ColMin; c < goldenGrid.ColMax; c++)
                {
                    var goldenBloc = goldenGrid.Get(r, c);
                    var sceneBloc = grid.Get(r, c);
                    bool isSolidGolden = goldenBloc != null && goldenBloc.IsMajorNode();
                    bool isSolidScene = sceneBloc != null && sceneBloc.IsMajorNode();
                    if (isSolidGolden != isSolidScene)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 剛體座標差異 (保留)
        /// </summary>
        public void CalcRigidBodyDiff(IxGridMap<EzBloc> grid, IxGridMap<EzBloc> goldenGrid)
        {
            throw new NotImplementedException();

            CalcRotatedBox2D(grid, out QvBox2D boxTo);
            CalcRotatedBox2D(goldenGrid, out QvBox2D boxFrom);

            //// 使用前三個頂點
            //var toPoints = new Point2f[3];
            //var fromPoints = new Point2f[3];
            //for (int i = 0; i < 3; i++)
            //{
            //    toPoints[i] = new Point2f(boxTo.Corners[i].X, boxTo.Corners[i].Y);
            //    fromPoints[i] = new Point2f(boxFrom.Corners[i].X, boxFrom.Corners[i].Y);
            //}
            //// 計算轉換矩陣
            //Mat affineMatrix = Cv2.GetAffineTransform(fromPoints, toPoints);

            var offset_x = boxTo.Center.X - boxFrom.Center.X;
            var offset_y = boxTo.Center.Y - boxFrom.Center.Y;

            //double baseLen = Math.Max(1, boxFrom.MinAreaRectSize.Width);

            //double totalDist = 0;

            //for (int r=0; r<goldenGrid.Rows; r++)
            //{
            //    for (int c = 0; c < goldenGrid.Cols; c++)
            //    {
            //        var sceneBloc = grid.Get(r, c);
            //        var goldenBloc = goldenGrid.Get(r, c);

            //        if (sceneBloc == null)
            //            continue;

            //        if (goldenBloc == null)
            //            continue;

            //        //Mat projected = new Mat();
            //        //Cv2.Transform(modelPoints, projected, affineMatrix);
            //        //Mat v = affineMatrix.Mul();
            //    }
            //}

            //affineMatrix?.Dispose();
        }

        /// <summary>
        /// 自我檢查 出格 狀況 
        /// </summary>
        public void ScanDefects(IxGridMap<EzBloc> grid, out Mat defectsMat, float emptyPenalty = 0f)
        {
            _grid = grid;

            defectsMat = new Mat(_grid.Rows, _grid.Cols, MatType.CV_32FC1);
            defectsMat.SetTo(new Scalar(0));

            for (int row = grid.RowMin; row < grid.RowMax; row++)
            {
                scanDefectsInRowLinear(row, defectsMat, emptyPenalty);
            }
            for (int col = grid.ColMin; col < grid.ColMax; col++)
            {
                scanDefectsInCol(col, defectsMat, emptyPenalty);
            }
        }

        /// <summary>
        /// 枚舉中心點
        /// </summary>
        public static IEnumerable<Point2f> IterCentroids(IxGridMap<EzBloc> grid)
        {
            if (grid == null)
                yield break;

            var r = grid.Rows - 1;
            var c = grid.Cols - 1;
            var corners = new[]
            {
                grid.Get(0,0),
                grid.Get(0,c),
                grid.Get(r,c),
                grid.Get(r,0),
            };
            
            int count = 0;
            foreach (var bloc in corners)
            {
                if (bloc == null) continue;
                yield return new Point2f((float)bloc.Center.X, (float)bloc.Center.Y);
                count++;
            }
            if (count >= 4)
                yield break;

            for (int row = grid.RowMin; row < grid.RowMax; row++)
            {
                for (int col = grid.ColMin; col < grid.ColMax; col++)
                {
                    var bloc = grid.Get(row, col);
                    if (bloc != null)
                        yield return new Point2f((float)bloc.Center.X, (float)bloc.Center.Y);
                }
            }
        }

        /// <summary>
        /// 枚舉邊界點
        /// </summary>
        public static IEnumerable<Point2f> IterBoundaryPoints(IxGridMap<EzBloc> grid)
        {
            if (grid == null)
                yield break;

            for (int row = grid.RowMin; row < grid.RowMax; row++)
            {
                for (int col = grid.ColMin; col < grid.ColMax; col++)
                {
                    var bloc = grid.Get(row, col);
                    if (bloc == null) continue;
                    if (!bloc.IsMajorNode())
                        yield return new Point2f((float)bloc.Center.X, (float)bloc.Center.Y);
                    yield return new Point2f(bloc.Rect.Left, bloc.Rect.Top);
                    yield return new Point2f(bloc.Rect.Right, bloc.Rect.Top);
                    yield return new Point2f(bloc.Rect.Right, bloc.Rect.Bottom);
                    yield return new Point2f(bloc.Rect.Left, bloc.Rect.Bottom);
                }
            }

            //foreach (var bloc in grid)
            //{
            //    if (bloc != null)
            //        yield return new Point(bloc.CenterX, bloc.CenterY);
            //}
        }

        #region PRIVATE_FUNCTIONS
        void scanDefectsInRow(int row, Mat defectsMat = null, float emptyPenality = 0f)
        {
            QxRowCol<EzBloc> begin = null;
            QxRowCol<EzBloc> last = null;
            float penalty, old;

            #region 掃描端點
            // 找到第一個有內容物的 col++
            for (int col = _colMin; col < _colMax; col++)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    begin = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (begin == null)
                return;

            // 找到最後一個有內容物的 (row, col) with col--
            for (int col = _colMax - 1; col >= _colMin; col--)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    last = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (last == null)
                return;

            int count = last.Col - begin.Col + 1;
            if (count < 5)
            {
                // 太少樣本不予計算
                return;
            }
            #endregion

            #region CALC_DEFECT_PENALITY

            // 端點
            EzBloc beginBloc = begin.TObj;
            EzBloc lastBloc = last.TObj;

            // 理想跨距向量
            QVector pitchVector = (lastBloc.Center - beginBloc.Center) / (count + 1);

            // 理想節點
            QVector gridNodePt = beginBloc.Center;

            for (int col = begin.Col; col < last.Col; col++, gridNodePt += pitchVector)
            {
                var curBloc = _grid.Get(row, col);
                var curPoint = curBloc?.Center;

                // 目前是空洞
                if (curPoint == null)
                {
                    if (emptyPenality > 0f)
                    {
                        // 更新 penality
                        old = defectsMat.At<float>(row, col);
                        defectsMat.Set(row, col, Math.Max(old, emptyPenality));
                    }
                    continue;
                }

                // 計算 penality
                var delta = curPoint - gridNodePt;
                penalty = (float)(delta.NormLength / pitchVector.NormLength);

                // 更新 penality
                old = defectsMat.At<float>(row, col);
                defectsMat.Set(row, col, Math.Max(old, penalty));
            }
            #endregion
        }
        void scanDefectsInCol(int col, Mat defectsMat = null, float emptyPenality = 0f)
        {
            QxRowCol<EzBloc> begin = null;
            QxRowCol<EzBloc> last = null;
            float penalty, old;

            #region 掃描端點
            // 找到第一個有內容物的 row++
            for (int row = _rowMin; row < _rowMax; row++)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    begin = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (begin == null)
                return;

            // 找到最後一個有內容物的 row--
            for (int row = _rowMax - 1; row >= _rowMin; row--)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    last = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (last == null)
                return;

            int count = last.Row - begin.Row + 1;
            if (count < 5)
            {
                // 太少樣本不予計算
                return;
            }
            #endregion

            #region CALC_DEFECT_PENALTY
            // 端點
            EzBloc beginBloc = begin.TObj;
            EzBloc lastBloc = last.TObj;

            // 理想跨距向量
            QVector pitchVector = (lastBloc.Center - beginBloc.Center) / (count - 1);

            // 理想節點
            QVector gridNodePt = beginBloc.Center;

            for (int row = begin.Row; row < last.Row; row++, gridNodePt += pitchVector)
            {
                var curBloc = _grid.Get(row, col);
                var curPoint = curBloc?.Center;

                // 目前是空洞
                if (curPoint == null)
                {
                    if (emptyPenality > 0f)
                    {
                        // 更新 penalty
                        old = defectsMat.At<float>(row, col);
                        defectsMat.Set(row, col, Math.Max(old, emptyPenality));
                    }
                    continue;
                }

                // 計算 penalty
                var delta = curPoint - gridNodePt;
                penalty = (float)(delta.NormLength / pitchVector.NormLength);

                // 更新 penality
                old = defectsMat.At<float>(row, col);
                defectsMat.Set(row, col, Math.Max(old, penalty));
            }
            #endregion
        }
        void scanDefectsInRowLinear(int row, Mat defectsMat = null, float emptyPenality = 0f)
        {
            QxRowCol<EzBloc> begin = null;
            QxRowCol<EzBloc> last = null;
            float old, penalty;

            #region 掃描端點
            // 找到第一個有內容物的 col++
            for (int col = _colMin; col < _colMax; col++)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    begin = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (begin == null)
                return;

            // 找到最後一個有內容物的 (row, col) with col--
            for (int col = _colMax - 1; col >= _colMin; col--)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    last = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (last == null)
                return;

            int count = last.Col - begin.Col + 1;
            if (count < 5)
            {
                // 太少樣本不予計算
                return;
            }
            #endregion

            #region CALC_DEFECT_PENALTY
            // 端點
            EzBloc beginBloc = begin.TObj;
            EzBloc lastBloc = last.TObj;

            // 直線公式
            var line = new QxLineFormula();
            var p1 = new System.Drawing.PointF(beginBloc.CenterX, beginBloc.CenterY);
            var p2 = new System.Drawing.PointF(lastBloc.CenterX, lastBloc.CenterY);
            line.Build(p1, p2);

            // 計算 dist error
            for (int col = begin.Col; col < last.Col; col++)
            {
                var curBloc = _grid.Get(row, col);

                // 目前是空洞
                if (curBloc == null)
                {
                    if (emptyPenality > 0f)
                    {
                        // 更新 penalty
                        old = defectsMat.At<float>(row, col);
                        defectsMat.Set(row, col, Math.Max(old, emptyPenality));
                    }
                    continue;
                }

                //debugText += $"{curBloc.CenterX},{curBloc.CenterY}\n";

                // 計算 dist
                var dist = line.Distance(curBloc.CenterX, curBloc.CenterY);
                penalty = (float)dist / Math.Max(curBloc.Rect.Width, curBloc.Rect.Height);

                old = defectsMat.At<float>(row, col);
                defectsMat.Set(row, col, Math.Max(old, penalty));
            }
            #endregion
        }
        void scanDefectsInColLinear(int col, Mat defectsMat = null, float emptyPenality = 0f)
        {
            QxRowCol<EzBloc> begin = null;
            QxRowCol<EzBloc> last = null;
            float old, penalty;

            #region 掃描端點
            // 找到第一個有內容物的 row++
            for (int row = _rowMin; row < _rowMax; row++)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    begin = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (begin == null)
                return;

            // 找到最後一個有內容物的 row--
            for (int row = _rowMax - 1; row >= _rowMin; row--)
            {
                var blob = _grid.Get(row, col);
                if (blob != null)
                {
                    last = new QxRowCol<EzBloc>(row, col, blob);
                    break;
                }
            }
            if (last == null)
                return;

            int count = last.Row - begin.Row + 1;
            if (count < 5)
            {
                // 太少樣本不予計算
                return;
            }
            #endregion

            #region CALC_DEFECT_PENALTY
            // 端點
            EzBloc beginBloc = begin.TObj;
            EzBloc lastBloc = last.TObj;

            // 直線公式
            var line = new QxLineFormula();
            var p1 = new System.Drawing.PointF(beginBloc.CenterX, beginBloc.CenterY);
            var p2 = new System.Drawing.PointF(lastBloc.CenterX, lastBloc.CenterY);
            line.Build(p1, p2);

            // 計算 dist error
            //string debugText = "";

            for (int row = begin.Row; row < last.Row; row++)
            {
                var curBloc = _grid.Get(row, col);

                // 目前是空洞
                if (curBloc == null)
                {
                    if (emptyPenality > 0f)
                    {
                        // 更新 penality
                        old = defectsMat.At<float>(row, col);
                        defectsMat.Set(row, col, Math.Max(old, emptyPenality));
                    }
                    continue;
                }

                //debugText += $"{curBloc.CenterX},{curBloc.CenterY}\n";

                // 計算 dist
                var dist = line.Distance(curBloc.CenterX, curBloc.CenterY);
                penalty = (float)dist / Math.Max(curBloc.Rect.Width, curBloc.Rect.Height);

                old = defectsMat.At<float>(row, col);
                defectsMat.Set(row, col, Math.Max(old, penalty));
            }

            //GaUtil.SaveData(debugText, $"d:\\paso.log\\line_points_{_debugCount++}.csv");
            #endregion
        }
        int _debugCount = 0;
        #endregion
    }
}
