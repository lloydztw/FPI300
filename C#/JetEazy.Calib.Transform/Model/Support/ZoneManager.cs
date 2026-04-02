#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-02 優化 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using System;

namespace JetEazy.Transform.Support
{
    public class ZoneManager
    {
        #region PRIVATE_DATA
        private readonly int _rows;
        private readonly int _cols;
        private readonly QVector[,] _points;
        private readonly ZoneBounds[,] _bounds;
        #endregion

        #region PRIVATE_CACHE_DATA
        // 最近鄰快取 (Temporal Locality Cache)
        private int _lastRow = 0;
        private int _lastCol = 0;
        #endregion

        #region INTERNAL_STRUCTURES 
        private struct ZoneBounds
        {
            public double MinX, MaxX, MinY, MaxY;
        }
        #endregion

        public ZoneManager(int rows, int cols, QVector[,] points)
        {
            _rows = rows;
            _cols = cols;
            _points = points;
            _bounds = new ZoneBounds[rows - 1, cols - 1];
            UpdateBounds();
        }

        /// <summary>
        /// 預先計算每個 Zone 的 AABB 邊界框
        /// </summary>
        public void UpdateBounds()
        {
            for (int i = 0; i < _rows - 1; i++)
            {
                for (int j = 0; j < _cols - 1; j++)
                {
                    var p0 = _points[i, j];
                    var p1 = _points[i, j + 1];
                    var p2 = _points[i + 1, j + 1];
                    var p3 = _points[i + 1, j];

                    _bounds[i, j] = new ZoneBounds
                    {
                        MinX = Math.Min(Math.Min(p0.x, p1.x), Math.Min(p2.x, p3.x)),
                        MaxX = Math.Max(Math.Max(p0.x, p1.x), Math.Max(p2.x, p3.x)),
                        MinY = Math.Min(Math.Min(p0.y, p1.y), Math.Min(p2.y, p3.y)),
                        MaxY = Math.Max(Math.Max(p0.y, p1.y), Math.Max(p2.y, p3.y))
                    };
                }
            }
        }

        /// <summary>
        /// 尋找點位所在的區域索引，整合快取與 AABB 過濾
        /// </summary>
        public bool FindZone(QVector pt, out int r, out int c, Func<QVector, int, int, bool> inZoneFunc)
        {
            r = c = -1;
            if (pt == null) return false;

            // 1. 檢查快取 (Cache Hit)
            if (CheckAndSet(pt, _lastRow, _lastCol, out r, out c, inZoneFunc)) return true;

            // 2. 檢查相鄰區域 (Spatial Locality)
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    if (CheckAndSet(pt, _lastRow + dr, _lastCol + dc, out r, out c, inZoneFunc)) return true;
                }
            }

            // 3. 全域搜尋搭配 AABB
            for (int i = 0; i < _rows - 1; i++)
            {
                for (int j = 0; j < _cols - 1; j++)
                {
                    if (CheckAndSet(pt, i, j, out r, out c, inZoneFunc)) return true;
                }
            }

            // 4. 若以上皆失敗：尋找最近的 4 個點作為 Zone (Extrapolation)
            return FindNearestZone(pt, out r, out c);
        }

        /// <summary>
        /// 當點在所有網格之外時，尋找距離最近的 4 個 Node 組成的 Zone 索引
        /// </summary>
        private bool FindNearestZone(QVector pt, out int r, out int c)
        {
            r = c = -1;
            double minDistance = double.MaxValue;

            // 尋找距離 pt 最近的一個 Node (中心點基準)
            for (int i = 0; i < _rows - 1; i++)
            {
                for (int j = 0; j < _cols - 1; j++)
                {
                    // 計算 Zone 中心點到 pt 的距離 (簡易距離判定)
                    var b = _bounds[i, j];
                    double centerX = (b.MinX + b.MaxX) / 2.0;
                    double centerY = (b.MinY + b.MaxY) / 2.0;

                    double dist = Math.Pow(pt.x - centerX, 2) + Math.Pow(pt.y - centerY, 2);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        r = i;
                        c = j;
                    }
                }
            }

            if (r != -1)
            {
                _lastRow = r;
                _lastCol = c;
                return true;
            }
            return false;
        }

        #region PRIVATE_FUNCTIONS
        private bool CheckAndSet(QVector pt, int row, int col, out int r, out int c, Func<QVector, int, int, bool> inZoneFunc)
        {
            r = c = -1;
            if (row < 0 || row >= _rows - 1 || col < 0 || col >= _cols - 1) return false;

            // AABB 快速過濾
            var b = _bounds[row, col];
            if (pt.x < b.MinX || pt.x > b.MaxX || pt.y < b.MinY || pt.y > b.MaxY) return false;

            // 精確射線法判定 (透過 Delegate 呼叫外部邏輯)
            if (inZoneFunc(pt, row, col))
            {
                r = _lastRow = row;
                c = _lastCol = col;
                return true;
            }
            return false;
        }
        #endregion
    }
}