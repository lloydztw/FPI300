#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-08-30 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QvMath;
using LaserAlignDX.BasicSpace;
using LeTian.AoiLib;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;

namespace LaserAlignDX.Model
{
    public class LineBorderPair
    {
        public const int MAX_PAIRS = 2;

        /// <summary>
        /// 邊線拉框 (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public QvBox2D[] Borders = new QvBox2D[2];

        /// <summary>
        /// 抓到的邊線 (單位 pixels) (FullFov Cammera Coordinates)
        /// (圖示用)
        /// </summary>
        public EzLSD.LineSegment[] LineSegments = new EzLSD.LineSegment[2];

        /// <summary>
        /// 目標值 (Runtime Data)
        /// </summary>
        public float TargetDist = 0f;

        /// <summary>
        /// Deep Clone
        /// </summary>
        public LineBorderPair Clone()
        {
            var clone = new LineBorderPair();
            for (int i = 0; i < 2; i++)
            {
                clone.Borders[i] = this.Borders[i]?.Clone();
                clone.LineSegments[i] = this.LineSegments[i]?.Clone();
            }
            return clone;
        }
    }


    public static class LineEdgeDictExtension
    {
        public static (string, int) GetKeyOffset(this EdgeBorder eb)
        {
            switch (eb)
            {
                case EdgeBorder.Left: return ("X", 0);
                case EdgeBorder.Right: return ("X", 1);
                case EdgeBorder.Top: return ("Y", 0);
                case EdgeBorder.Bottom: return ("Y", 1);
            }
            return (null, -1);
        }

        /// <summary>
        /// 設定目標值
        /// </summary>
        static void SetTargetDists_000(this Dictionary<string, LineBorderPair> lineBorderPairs, SizeF targetSize)
        {
            if (lineBorderPairs == null)
                return;

            for (int i = 0; i < LineBorderPair.MAX_PAIRS; i++)
            {
                if (lineBorderPairs.TryGetValue(i == 0 ? "X" : $"X{i}", out var pairXi))
                    pairXi.TargetDist = targetSize.Width;
                if (lineBorderPairs.TryGetValue(i == 0 ? "Y" : $"Y{i}", out var pairYi))
                    pairYi.TargetDist = targetSize.Height;
            }
        }

        /// <summary>
        /// 設定目標值（支援任意組數）
        /// </summary>
        public static void SetTargetDists(this Dictionary<string, LineBorderPair> lineBorderPairs, SizeF targetSize)
        {
            if (lineBorderPairs == null) return;

            foreach (var kvp in lineBorderPairs)
            {
                if (kvp.Value == null) continue;

                if (kvp.Key.StartsWith("X"))
                    kvp.Value.TargetDist = targetSize.Width;

                else if (kvp.Key.StartsWith("Y"))
                    kvp.Value.TargetDist = targetSize.Height;
            }
        }

        /// <summary>
        /// 枚舉 所有 邊線拉框 (不限於 左上右下 四個)
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public static IEnumerable<QvBox2D> IterLineBorderBoxes(this Dictionary<string, LineBorderPair> LineBorderPairs)
        {
            foreach (var pair in LineBorderPairs.Values)
            {
                if (pair != null)
                {
                    for (int i = 0; i < pair.Borders.Length; i++)
                    {
                        var b = pair.Borders[i];
                        if (b != null)
                            yield return b;
                    }
                }
            }
        }

        /// <summary>
        /// 枚舉 抓到的 所有邊線 (不限於 左上右下 四個) 
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public static IEnumerable<EzLSD.LineSegment> IterLineSegments(this Dictionary<string, LineBorderPair> LineBorderPairs)
        {
            foreach (var pair in LineBorderPairs.Values)
            {
                if (pair != null)
                {
                    for (int i = 0; i < pair.LineSegments.Length; i++)
                    {
                        var ls = pair.LineSegments[i];
                        if (ls != null)
                            yield return ls;
                    }
                }
            }
        }

        /// <summary>
        /// 調整數據數量（使用 HashSet 優化記憶體與搜尋效能）
        /// </summary>
        public static bool AdjustPairsNumber(this Dictionary<string, LineBorderPair> lineBorderPairs, int numX, int numY)
        {
            if (lineBorderPairs == null) return false;

            var targetKeyNames = new HashSet<string>();
            for (int i = 0; i < numX; i++) targetKeyNames.Add(i == 0 ? "X" : $"X{i}");
            for (int i = 0; i < numY; i++) targetKeyNames.Add(i == 0 ? "Y" : $"Y{i}");

            bool isChanged = false;

            // 移除多餘的 Key
            var existingKeys = lineBorderPairs.Keys.ToList();
            foreach (var key in existingKeys)
            {
                if (!targetKeyNames.Contains(key))
                {
                    lineBorderPairs.Remove(key);
                    isChanged = true;
                }
            }

            // 補足缺少的 Key
            foreach (var key in targetKeyNames)
            {
                if (!lineBorderPairs.ContainsKey(key))
                {
                    var pair = new LineBorderPair();
                    pair.Borders[0] = new QvBox2D();
                    pair.Borders[1] = new QvBox2D();
                    lineBorderPairs.Add(key, new LineBorderPair());
                    isChanged = true;
                }
            }

            return isChanged;
        }

        /// <summary>
        /// 取得數據數量
        /// </summary>
        public static int GetPairsNumbers(this Dictionary<string, LineBorderPair> LineBorderPairs, out int numX, out int numY)
        {
            numX = 0;
            numY = 0;
            
            if (LineBorderPairs == null || LineBorderPairs.Count == 0)
                return 0;

            foreach (var kv in LineBorderPairs)
            {
                var key = kv.Key;
                var pair = kv.Value;
                if (pair == null) continue;
                if (key.StartsWith("X"))
                    numX++;
                else
                    numY++;
            }

            return numX + numY;
        }

        /// <summary>
        /// 取得 左, 上, 右, 下, 四邊線 
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        static EzLSD.LineSegment[] GetQuadLineSegments_000(this Dictionary<string, LineBorderPair> LineBorderPairs)
        {
            if (LineBorderPairs.TryGetValue("X", out var pairX) &&
                LineBorderPairs.TryGetValue("Y", out var pairY))
            {
                var lines = new List<EzLSD.LineSegment> {
                    pairX.LineSegments[0],  // Left
                    pairY.LineSegments[0],  // Top
                    pairX.LineSegments[1],  // Right
                    pairY.LineSegments[1],  // Bottom
                };
                lines.RemoveAll(line => line == null);
                return lines.ToArray();
            }
            return null;
        }

        /// <summary>
        /// 取得 左, 上, 右, 下, 四邊線（增加防禦性檢查）
        /// </summary>
        public static EzLSD.LineSegment[] GetQuadLineSegments(this Dictionary<string, LineBorderPair> lineBorderPairs)
        {
            if (lineBorderPairs == null) 
                return null;

            var lines = new List<EzLSD.LineSegment>();

            if (lineBorderPairs.TryGetValue("X", out var pairX) && pairX?.LineSegments != null)
            {
                if (pairX.LineSegments.Length > 0 && pairX.LineSegments[0] != null) 
                    lines.Add(pairX.LineSegments[0]); // Left

                if (pairX.LineSegments.Length > 1 && pairX.LineSegments[1] != null) 
                    lines.Add(pairX.LineSegments[1]); // Right
            }

            if (lineBorderPairs.TryGetValue("Y", out var pairY) && pairY?.LineSegments != null)
            {
                if (pairY.LineSegments.Length > 0 && pairY.LineSegments[0] != null) 
                    lines.Add(pairY.LineSegments[0]); // Top

                if (pairY.LineSegments.Length > 1 && pairY.LineSegments[1] != null) 
                    lines.Add(pairY.LineSegments[1]); // Bottom
            }

            return lines.Count > 0 ? lines.ToArray() : null;
        }
    }
}
