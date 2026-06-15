#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
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
using System.Linq;


namespace JetEazy.Match
{
    public class EzBlocsGridBuilder
    {
        public EzBlocsGrid Build(IList<EzBloc> blocs, Comparison<EzBlocsGrid> comparer = null, int targetRows = 0, int targetCols = 0, bool resetOwner = false, SizeF? targetPitch = null)
        {
            if (blocs == null)
                return null;

            var bound = get_boundary(blocs);
            var aveSize = get_ave_size(blocs);
            var defaultPitch = aveSize;

            if (targetRows > 0 && targetCols > 0)
            {
                defaultPitch.Width = bound.Width / targetCols;
                defaultPitch.Height = bound.Height / targetRows;
            }
            if(targetPitch != null && targetPitch.HasValue)
            {
                defaultPitch = targetPitch.Value;
            }

            var pitch = search_best_pitches(blocs, defaultPitch);


            #region 如果沒找到_PITCH，就放大搜尋範圍再試一次 
            for (int trial = 0; trial < 2; trial++)
            {
                if (pitch.X > 0 && pitch.Y > 0)
                    break;
                if (pitch.X <= 0)
                    defaultPitch.Width = (int)(defaultPitch.Width * 1.25);
                if (pitch.Y <= 0)
                    defaultPitch.Height = (int)(defaultPitch.Height * 1.25);
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
                foreach (var bloc in blocs)
                {
                    if (bloc != null)
                        bloc.Owner = null;
                }
            }

            foreach (var bloc in blocs)
            {
                if (bloc == null || bloc.Owner != null)
                    continue;

                //if (bloc.Tag is QuadLinks links)
                //    if (links.rowCol != null)
                //        continue;

                var map = locaBuilder.Build(bloc, pitch);
                if (map != null && map.ActualCount > 0)
                    localMaps.Add(map);
            }

            if (localMaps.Count == 0)
                return null;

            if (localMaps.Count > 1)
            {
                if (comparer != null)
                    localMaps.Sort(comparer);
                else
                    localMaps.Sort((a, b) =>
                    {
                        return b.ActualCount - a.ActualCount;
                    });
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
            if (blocs == null)
                return null;

            var bound = get_boundary(blocs);
            var aveSize = get_ave_size(blocs);
            var pitch = assignedPitch == null ? search_best_pitches(blocs, aveSize) : assignedPitch;
            if (pitch.X <= 0) pitch.X = bound.Width;
            if (pitch.Y <= 0) pitch.Y = bound.Height;

            var grid = new EzBlocsGrid(bound, pitch, rows, cols);
            return grid;
        }

        #region PRIVATE_BUILD_FUNCTIONS
        internal static Rectangle get_boundary(IEnumerable<EzBloc> blocs)
        {
            int xmin = int.MaxValue;
            int ymin = int.MaxValue;
            int xmax = 0;
            int ymax = 0;
            foreach (var bloc in blocs)
            {
                if (bloc == null) 
                    continue;
                xmin = Math.Min(xmin, bloc.Rect.X);
                ymin = Math.Min(ymin, bloc.Rect.Y);
                xmax = Math.Max(xmax, bloc.Rect.Right);
                ymax = Math.Max(ymax, bloc.Rect.Bottom);
            }
            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
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
            var w = (int)Math.Round(sumW / count);
            var h = (int)Math.Round(sumH / count);
            return new SizeF(w, h);
        }
        static void build_inter_quad_links(IEnumerable<EzBloc> blocs, SizeF? goldenSize = null)
        {
            if (blocs == null)
                return;

            var baseSize = goldenSize == null ? get_ave_size(blocs) : goldenSize.Value;
            var minPitchX = baseSize.Width * 0.8;
            var minPitchY = baseSize.Height * 0.8;
            var maxPitchX = baseSize.Width * 2;
            var maxPitchY = baseSize.Height * 2;

            // 暴力搜尋
            foreach (var cur in blocs)
            {
                if (cur == null) 
                    continue;

                var links = new QuadLinkNode();
                cur.Tag = links;

                foreach (var next in blocs)
                {
                    if (next == cur)
                        continue;

                    // Vector
                    var vect = next.Center - cur.Center;
                    var dx = Math.Abs(vect.X);
                    var dy = Math.Abs(vect.Y);

                    if (dx > maxPitchX || dy > maxPitchY)
                        continue;

                    //(1) best link RIGHT
                    if (vect.X > minPitchX && dy < minPitchY / 2)
                    {
                        check_in_best_link(links, vect, next, QuadLinkNode.Dir.Right);
                    }

                    //(2) best link LEFT
                    if (vect.X < -minPitchX && dy < minPitchY / 2)
                    {
                        check_in_best_link(links, vect, next, QuadLinkNode.Dir.Left);
                    }

                    //(3) best link DOWN
                    if (vect.Y > minPitchY && dx < minPitchX / 2)
                    {
                        check_in_best_link(links, vect, next, QuadLinkNode.Dir.Down);
                    }

                    //(4) best link UP
                    if (vect.Y < -minPitchY && dx < minPitchX / 2)
                    {
                        check_in_best_link(links, vect, next, QuadLinkNode.Dir.Up);
                    }
                }
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
            {
                return new QVector(0.0, 0.0);
            }

            build_inter_quad_links(blocs, goldenSize);

            var listPitchU = new List<int>();
            var listPitchV = new List<int>();

            foreach (var bloc in blocs)
            {
                if (bloc == null)
                    continue;

                if (bloc.Tag is QuadLinkNode link)
                {
                    EzBloc blocRight = link.Right;
                    if (blocRight != null)
                    {
                        QVector U = blocRight.Center - bloc.Center;
                        listPitchU.Add((int)U.NormLength);
                    }
                    EzBloc blocDown = link.Down;
                    if (blocDown != null)
                    {
                        QVector V = blocDown.Center - bloc.Center;
                        listPitchV.Add((int)V.NormLength);
                    }
                }
            }

            // 取中位數
            listPitchU.Sort();
            listPitchV.Sort();
            double bestPitchU = listPitchU.Count <= 2 ? 0 : listPitchU[(listPitchU.Count - 1) / 2];
            double bestPitchV = listPitchV.Count <= 2 ? 0 : listPitchV[(listPitchV.Count - 1) / 2];
            return new QVector(bestPitchU, bestPitchV);
        }
        #endregion

        #region RESERVED_PRIVATE_GROUPING_FUNCTIONS
        List<EzBloc> remove_overlaps(List<EzBloc> blocs)
        {
            // 建立一個新的清單來儲存非重疊項目
            var results = new List<EzBloc>();

            foreach (var current in blocs)
            {
                bool hasOverlap = false;

                for (int i = results.Count - 1; i >= 0; i--)
                {
                    var existing = results[i];

                    // 判斷 Rect 是否重疊
                    if (current.Rect.IntersectsWith(existing.Rect))
                    {
                        hasOverlap = true;

                        // 保留 Score 較大者
                        if (current.Score > existing.Score)
                        {
                            results.RemoveAt(i); // 移除較小的
                            results.Add(current); // 加入較大的
                        }

                        break;
                    }
                }

                if (!hasOverlap)
                {
                    results.Add(current);
                }
            }

            return results;
        }
        List<EzBloc> remove_overlaps_001(List<EzBloc> blocs)
        {
            var results = new List<EzBloc>();
            var groups = group_overlaps(blocs);
            int compare_score(EzBloc e1, EzBloc e2)
            {
                if (e2.Score > e1.Score)
                    return -1;
                else if (e2.Score < e1.Score)
                    return 1;
                else
                    return 0;
            }

            foreach (var group in groups)
            {
                if (group.Count > 1)
                    group.Sort(compare_score);
                results.Add(group[0]);
            }
            return results;
        }
        List<List<EzBloc>> group_overlaps(List<EzBloc> blocs)
        {
            List<List<EzBloc>> groups = new List<List<EzBloc>>();
            HashSet<int> visited = new HashSet<int>(); // 用來記錄已處理的元素

            // 定義 DFS 搜尋方法來找出所有互相重疊的矩形
            void Dfs(List<EzBloc> group, int index)
            {
                if (visited.Contains(index))
                    return;

                visited.Add(index);
                group.Add(blocs[index]);

                for (int i = 0; i < blocs.Count; i++)
                {
                    if (i != index && !visited.Contains(i) && blocs[index].Rect.IntersectsWith(blocs[i].Rect))
                    {
                        Dfs(group, i); // 遞迴搜索重疊的矩形
                    }
                }
            }

            // 遍歷列表中的每個 EzLoc 元素，並進行 DFS
            for (int i = 0; i < blocs.Count; i++)
            {
                if (!visited.Contains(i))
                {
                    List<EzBloc> group = new List<EzBloc>();
                    Dfs(group, i); // 開始從第 i 個元素進行深度優先搜尋
                    groups.Add(group); // 將找到的群組加入到結果中
                }
            }

            return groups;
        }
        int find_most_common_diff(List<int> values)
        {
            if (values.Count < 2)
            {
                //throw new ArgumentException("List must contain at least two values.");
                return 0;
            }

            // 先對 values 進行排序
            values.Sort();

            if (values.Count == 2)
            {
                return values[1] - values[0];
            }

            // 創建一個字典來存儲每個差值和它的出現次數
            Dictionary<int, int> differenceCount = new Dictionary<int, int>();

            // 計算相鄰元素之間的差值
            for (int i = 0; i < values.Count - 1; i++)
            {
                var difference = values[i + 1] - values[i];

                if (differenceCount.ContainsKey(difference))
                {
                    differenceCount[difference]++;
                }
                else
                {
                    differenceCount[difference] = 1;
                }
            }

            // 找出出現次數最多的差值
            var mostCommonDifference = differenceCount.OrderByDescending(x => x.Value).First().Key;

            return mostCommonDifference;
        }
        #endregion
    }


    class LocalGridBuilder
    {
        #region CONST_TABLE
        static Dictionary<QuadLinkNode.Dir, QVector> _linksTable = new Dictionary<QuadLinkNode.Dir, QVector>()
        {
            { QuadLinkNode.Dir.Right, new QVector(1.0, 0.0) },
            { QuadLinkNode.Dir.Down, new QVector(0.0, 1.0) },
            { QuadLinkNode.Dir.Left, new QVector(-1.0, 0.0) },
            { QuadLinkNode.Dir.Up, new QVector(0.0, -1.0) },
        };
        #endregion

        #region RUNTIME_DATA
        EzBlocsGrid _grid;
        QVector _avePitch;
        #endregion

        public EzBlocsGrid Build(EzBloc bloc, QVector avePitch)
        {
            _grid = new EzBlocsGrid();
            _avePitch = avePitch;

            visit_inter_links(bloc, new QxRowCol(0, 0));
            _grid.Rescan(null);
            _grid.RowMin = 0;
            _grid.ColMin = 0;

            return _grid;
        }

        #region PRIVATE_FUNCTIONS
        void visit_inter_links(EzBloc bloc, QxRowCol cursor)
        {
            if (bloc == null)
                return;

            var links = get_safe_quad_links(bloc);
            if (links == null)
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
                if (link == null)
                    continue;

                EzBloc nextBloc = link.Next;
                if (nextBloc == null)
                    continue;

                QVector incr = kp.Value;
                //double standSpan = Math.Abs(incr * _avePitch);
                //double actualSpan = incr * link.Vector;
                //double jumpFactor = actualSpan / standSpan;
                //if (jumpFactor < 0.8 || jumpFactor > 1.2)
                //    continue;

                int c = (int)Math.Round(incr.X);
                int r = (int)Math.Round(incr.Y);
                visit_inter_links(nextBloc, new QxRowCol(cursor.Row + r, cursor.Col + c));
            }
        }
        static QuadLinkNode get_safe_quad_links(EzBloc bloc)
        {
            if (bloc == null)
                return null;
            if (bloc.Tag is QuadLinkNode lnk)
                return lnk;
            var link = new QuadLinkNode();
            bloc.Tag = link;
            return link;
        }
        #endregion
    }
}
