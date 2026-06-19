#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-20 優化 (by LeTian Chang) 
 *      2024-10-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QxCollections;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace JetEazy.Match
{
    public class EzBlocsGridBuilder
    {
        public EzBlocsGrid Build(IList<EzBloc> blocs, Comparison<EzBlocsGrid> comparer = null, int targetRows = 0, int targetCols = 0, bool resetOwner = false, SizeF? targetPitch = null)
        {
            if (blocs == null || blocs.Count == 0)
                return null;

            var bound = get_boundary(blocs);
            var aveSize = get_ave_size(blocs);
            var defaultPitch = aveSize;

            #region 取得合適的_defaultPitch
            if (targetRows > 0 && targetCols > 0)
            {
                defaultPitch.Width = (float)bound.Width / targetCols;
                defaultPitch.Height = (float)bound.Height / targetRows;
            }
            if (targetPitch != null && targetPitch.HasValue)
            {
                defaultPitch = targetPitch.Value;
            }
            #endregion

            var pitch = search_best_pitches(blocs, defaultPitch);

            #region 如果沒找到_PITCH，就放大搜尋範圍再試一次 
            for (int trial = 0; trial < 2; trial++)
            {
                if (pitch.X > 0 && pitch.Y > 0)
                    break;
                if (pitch.X <= 0)
                    defaultPitch.Width = (float)(defaultPitch.Width * 1.25);
                if (pitch.Y <= 0)
                    defaultPitch.Height = (float)(defaultPitch.Height * 1.25);
                pitch = search_best_pitches(blocs, defaultPitch);
            }
            #endregion

            #region 如果還是沒找到_PITCH，就直接用邊界寬高當作_PITCH
            if (pitch.X <= 0) pitch.X = bound.Width;
            if (pitch.Y <= 0) pitch.Y = bound.Height;
            #endregion

            var locaBuilder = new LocalGridBuilder();
            var localMaps = new List<EzBlocsGrid>();

            if (resetOwner)
            {
                int count = blocs.Count;
                for (int i = 0; i < count; i++)
                {
                    if (blocs[i] != null)
                        blocs[i].Owner = null;
                }
            }

            foreach (var bloc in blocs)
            {
                if (bloc == null || bloc.Owner != null)
                    continue;

                var map = locaBuilder.Build(bloc, pitch);
                if (map != null && map.ActualCount > 0)
                    localMaps.Add(map);
            }

            if (localMaps.Count == 0)
                return null;

            // 可能失效了
            if (localMaps.Count > 1)
            {                
                // SORT
                if (comparer != null)
                    localMaps.Sort(comparer);
                else
                    localMaps.Sort((a, b) => { return b.ActualCount - a.ActualCount; });

                // DUMP
                EzBlocsStorage.SaveToFile($"d:\\paso.log\\GridBuilder\\blocs_NG_{localMaps[0]}.json", blocs);
            }

            var globalGrid = new EzBlocsGrid(bound, pitch, localMaps[0]);
            var interpo = new EzBlocsGridInterpo(pitch, aveSize);
            interpo.RunInterpolation(globalGrid, null, null);
            interpo.RunExpolation(globalGrid, null, null, 2);
            globalGrid.RebuildRowColTags();
            return globalGrid;
        }

        public EzBlocsGrid BuildEmptyGrid(IList<EzBloc> blocs, int rows, int cols, QVector assignedPitch = null)
        {
            if (blocs == null || blocs.Count == 0)
                return null;

            var bound = get_boundary(blocs);
            var aveSize = get_ave_size(blocs);
            var pitch = assignedPitch == null ? search_best_pitches(blocs, aveSize) : assignedPitch;
            if (pitch.X <= 0) pitch.X = bound.Width;
            if (pitch.Y <= 0) pitch.Y = bound.Height;

            return new EzBlocsGrid(bound, pitch, rows, cols);
        }

        #region PRIVATE_BUILD_FUNCTIONS
        internal static Rectangle get_boundary(IEnumerable<EzBloc> blocs)
        {
            int xmin = int.MaxValue;
            int ymin = int.MaxValue;
            int xmax = int.MinValue;
            int ymax = int.MinValue;
            bool hasValid = false;

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;
                hasValid = true;
                if (bloc.Rect.X < xmin) xmin = bloc.Rect.X;
                if (bloc.Rect.Y < ymin) ymin = bloc.Rect.Y;
                if (bloc.Rect.Right > xmax) xmax = bloc.Rect.Right;
                if (bloc.Rect.Bottom > ymax) ymax = bloc.Rect.Bottom;
            }
            return hasValid ? new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin) : Rectangle.Empty;
        }

        internal static SizeF get_ave_size(IEnumerable<EzBloc> blocs)
        {
            if (blocs == null)
                return SizeF.Empty;

            int count = 0;
            double sumW = 0;
            double sumH = 0;
            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;
                sumW += bloc.Rect.Width;
                sumH += bloc.Rect.Height;
                count++;
            }
            if (count == 0) return SizeF.Empty;
            return new SizeF((float)(sumW / count), (float)(sumH / count));
        }

        /// <summary>
        /// 優化後的拓撲連結建立：引入空間網格索引 (Spatial Grid Indexing)，效能從 O(N^2) 降至逼近 O(N)
        /// </summary>
        static void build_inter_quad_links(IEnumerable<EzBloc> blocs, SizeF? goldenSize = null)
        {
            if (blocs == null)
                return;
            
            // 上下限條件
            var baseSize = goldenSize ?? get_ave_size(blocs);
            var minPitchX = baseSize.Width * 0.8;
            var minPitchY = baseSize.Height * 0.8;
            var maxPitchX = baseSize.Width * 2.25;
            var maxPitchY = baseSize.Height * 2.25;

            // 極值紀錄
            var runtime_max_pitch = new QVector2(0, 0);
            var runtime_min_pitch = new QVector2(double.MaxValue, double.MaxValue);

            // 1. 建立空間網格索引，Grid 大小設為 maxPitch 的最大值
            double cellSize = Math.Max(maxPitchX, maxPitchY);
            var spatialIndex = new Dictionary<ulong, List<EzBloc>>();

            foreach (var b in blocs)
            {
                if (b == null) continue;
                b.Tag = new QuadLinkNode(); // 預先初始化，避免後面重複判斷

                int gridX = (int)Math.Floor(b.Center.X / cellSize);
                int gridY = (int)Math.Floor(b.Center.Y / cellSize);
                ulong key = ((ulong)(uint)gridX << 32) | (uint)gridY;

                if (!spatialIndex.TryGetValue(key, out var list))
                {
                    list = new List<EzBloc>();
                    spatialIndex[key] = list;
                }
                list.Add(b);
            }

            // 2. 僅與相鄰的 9 個網格格點進行比對
            foreach (var cur in blocs)
            {
                if (cur == null) continue;
                var links = (QuadLinkNode)cur.Tag;

                int curGridX = (int)Math.Floor(cur.Center.X / cellSize);
                int curGridY = (int)Math.Floor(cur.Center.Y / cellSize);

                for (int dxIndex = -1; dxIndex <= 1; dxIndex++)
                {
                    for (int dyIndex = -1; dyIndex <= 1; dyIndex++)
                    {
                        int targetGridX = curGridX + dxIndex;
                        int targetGridY = curGridY + dyIndex;
                        ulong key = ((ulong)(uint)targetGridX << 32) | (uint)targetGridY;

                        if (spatialIndex.TryGetValue(key, out var targetList))
                        {
                            int targetCount = targetList.Count;
                            for (int i = 0; i < targetCount; i++)
                            {
                                var next = targetList[i];
                                if (next == cur) continue;

                                var vect = next.Center - cur.Center;
                                double dx = Math.Abs(vect.X);
                                double dy = Math.Abs(vect.Y);

                                if (dx > maxPitchX || dy > maxPitchY)
                                    continue;

                                #region 紀錄極值
                                if (dx > minPitchX * 0.5 && dy > minPitchY * 0.5)
                                {
                                    runtime_max_pitch.X = Math.Max(runtime_max_pitch.X, dx);
                                    runtime_max_pitch.Y = Math.Max(runtime_max_pitch.Y, dy);
                                    runtime_min_pitch.X = Math.Min(runtime_min_pitch.X, dx);
                                    runtime_min_pitch.Y = Math.Min(runtime_min_pitch.Y, dy);
                                }
                                #endregion

                                // (1) RIGHT
                                if (vect.X > minPitchX && dy < minPitchY * 0.5)
                                    check_in_best_link(links, vect, next, QuadLinkNode.Dir.Right);

                                // (2) LEFT
                                else if (vect.X < -minPitchX && dy < minPitchY * 0.5)
                                    check_in_best_link(links, vect, next, QuadLinkNode.Dir.Left);

                                // (3) DOWN
                                else if (vect.Y > minPitchY && dx < minPitchX * 0.5)
                                    check_in_best_link(links, vect, next, QuadLinkNode.Dir.Down);

                                // (4) UP
                                else if (vect.Y < -minPitchY && dx < minPitchX * 0.5)
                                    check_in_best_link(links, vect, next, QuadLinkNode.Dir.Up);
                            }
                        }
                    }
                }
            }

            // 3. DUMP
            if (true)
            {
                runtime_min_pitch.X /= baseSize.Width;
                runtime_max_pitch.X /= baseSize.Width;
                runtime_min_pitch.Y /= baseSize.Height;
                runtime_max_pitch.Y /= baseSize.Height;
                System.Diagnostics.Debug.WriteLine($"[GridBuilder.Pitch] Ratio X = {runtime_min_pitch.X:0.00} ~ {runtime_max_pitch.X:0.00}");
                System.Diagnostics.Debug.WriteLine($"[GridBuilder.Pitch] Ratio Y = {runtime_min_pitch.Y:0.00} ~ {runtime_max_pitch.Y:0.00}");
            }
        }

        static void check_in_best_link(QuadLinkNode links, QVector vect, EzBloc bloc, QuadLinkNode.Dir dir)
        {
            QVector existV = links[dir];
            if (existV == null || existV.NormLengthSQ > vect.NormLengthSQ)
                links[dir] = new QuadLinkNode.Link(bloc, vect);
        }

        static QVector search_best_pitches(IList<EzBloc> blocs, SizeF? goldenSize = null)
        {
            if (blocs == null || blocs.Count == 0)
                return new QVector(0.0, 0.0);

            build_inter_quad_links(blocs, goldenSize);

            // 預配容量以減少 GC 負擔
            var listPitchU = new List<int>(blocs.Count);
            var listPitchV = new List<int>(blocs.Count);

            int blocCount = blocs.Count;
            for (int i = 0; i < blocCount; i++)
            {
                var bloc = blocs[i];
                if (bloc?.Tag is QuadLinkNode link)
                {
                    EzBloc blocRight = link.Right;
                    if (blocRight != null)
                    {
                        listPitchU.Add((int)(blocRight.Center - bloc.Center).NormLength);
                    }
                    EzBloc blocDown = link.Down;
                    if (blocDown != null)
                    {
                        listPitchV.Add((int)(blocDown.Center - bloc.Center).NormLength);
                    }
                }
            }

            listPitchU.Sort();
            listPitchV.Sort();

            double bestPitchU = listPitchU.Count <= 2 ? 0 : listPitchU[(listPitchU.Count - 1) / 2];
            double bestPitchV = listPitchV.Count <= 2 ? 0 : listPitchV[(listPitchV.Count - 1) / 2];
            return new QVector(bestPitchU, bestPitchV);
        }
        #endregion
    }


    class LocalGridBuilder
    {
        private static readonly Dictionary<QuadLinkNode.Dir, QVector> _linksTable = new Dictionary<QuadLinkNode.Dir, QVector>()
        {
            { QuadLinkNode.Dir.Right, new QVector(1.0, 0.0) },
            { QuadLinkNode.Dir.Down, new QVector(0.0, 1.0) },
            { QuadLinkNode.Dir.Left, new QVector(-1.0, 0.0) },
            { QuadLinkNode.Dir.Up, new QVector(0.0, -1.0) },
        };

        private EzBlocsGrid _grid;
        private HashSet<EzBloc> _visited; // 新增：明確的 Hash 集合避免重複遞迴

        public EzBlocsGrid Build(EzBloc bloc, QVector avePitch)
        {
            _grid = new EzBlocsGrid();
            _visited = new HashSet<EzBloc>();

            visit_inter_links(bloc, new QxRowCol(0, 0));
            _grid.Rescan(null);
            _grid.RowMin = 0;
            _grid.ColMin = 0;

            return _grid;
        }

        private void visit_inter_links(EzBloc bloc, QxRowCol cursor)
        {
            if (bloc == null || !_visited.Add(bloc))
                return;

            if (!(bloc.Tag is QuadLinkNode links))
                return;

            if (links.rowCol != null)
                return;

            links.rowCol = new QxRowCol(cursor);

            var old = _grid.Get(cursor.Row, cursor.Col);
            if (old == null || old.Score < bloc.Score)
            {
                _grid.Set(cursor.Row, cursor.Col, bloc);
                bloc.Owner = _grid;
                if (old != null)
                    old.Owner = null;
            }

            foreach (var kp in _linksTable)
            {
                var dir = kp.Key;
                var link = links[dir];
                if (link?.Next == null)
                    continue;

                QVector incr = kp.Value;
                int c = (int)Math.Round(incr.X);
                int r = (int)Math.Round(incr.Y);
                visit_inter_links(link.Next, new QxRowCol(cursor.Row + r, cursor.Col + c));
            }
        }
    }
}
