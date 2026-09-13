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
using System.Linq;

namespace LaserAlignDX.Model
{
    public class LineBorderPair
    {
        public const int MAX_PAIRS = 2;

        public LineBorderPair(bool isLocal)
        {
            IsLocal = isLocal;
        }

        public bool IsLocal
        {
            get;
            set;
        }

        /// <summary>
        /// 邊線拉框 (單位 pixels) (FullFov Cammera Coordinates)
        /// </summary>
        public readonly QvBox2D[] Borders = new QvBox2D[2];

        /// <summary>
        /// 抓到的邊線 (單位 pixels) (FullFov Cammera Coordinates)
        /// (圖示用)
        /// </summary>
        public readonly EzLSD.LineSegment[] LineSegments = new EzLSD.LineSegment[2];

        /// <summary>
        /// 目標值 (Runtime Data)
        /// </summary>
        public float TargetDist = 0f;

        /// <summary>
        /// Deep Clone
        /// </summary>
        public LineBorderPair Clone()
        {
            var clone = new LineBorderPair(IsLocal);
            for (int i = 0; i < 2; i++)
            {
                clone.Borders[i] = this.Borders[i]?.Clone();
                clone.LineSegments[i] = this.LineSegments[i]?.Clone();
            }
            return clone;
        }

        #region STATIC_UTIL_FUNCTIONS
        public static (string, int) ParseKeyName(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
                return ("", -1);

            int index = 0;
            string category = keyName.Substring(0, 1);
            if (keyName.Length > 1)
                int.TryParse(keyName.Substring(1), out index);

            return (category, index);
        }
        public static string GetPostfix(string keyName, int offset)
        {
            if (keyName == null)
                return null;

            if (keyName.StartsWith("X"))
            {
                //return offset == 0 ? ".L" : ".R";
                return offset == 0 ? " ◀" : " ▶";
            }
            else
            {
                //return offset == 0 ? ".T" : ".B";
                return offset == 0 ? " ▲" : " ▼";
            }
        }
        public static string GetPostfix(GapEnum gap)
        {
            switch(gap)
            {
                case GapEnum.LUX:
                case GapEnum.LDX:
                    return " ◀";
                case GapEnum.RUX:
                case GapEnum.RDX:
                    return " ▶";
                case GapEnum.LUY:
                case GapEnum.RUY:
                    return " ▲";
                case GapEnum.LDY:
                case GapEnum.RDY:
                default:
                    return " ▼";
            }
        }
        #endregion
    }


    public class LineBorderPairsCollection
    {
        #region PRIVATE_DATA
        readonly Dictionary<string, LineBorderPair> _dict = new Dictionary<string, LineBorderPair>();
        bool _isLocal;
        #endregion

        #region PUBLIC_DICT_DATA
        public static implicit operator Dictionary<string, LineBorderPair>(LineBorderPairsCollection collection)
        {
            return collection?._dict;
        }
        #endregion

        #region PUBLIC_DICT_OPERATORS
        public LineBorderPair this[string key]
        {
            get => _dict.TryGetValue(key, out var pair) ? pair : null;
            set
            {
                if (value != null) 
                    _dict[key] = value;
            }
        }
        public IEnumerable<string> Keys => _dict.Keys;
        public bool TryGetValue(string key, out LineBorderPair pair)
        {
            return _dict.TryGetValue(key, out pair);
        }
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
        public void Clear()
        {
            _dict.Clear();
        }
        #endregion

        public LineBorderPairsCollection(bool isLocal)
        {
            _isLocal = isLocal;
        }

        public bool IsLocal
        {
            get => _isLocal;
            set
            {
                _isLocal = value;
                foreach (var pair in _dict.Values)
                {
                    if (pair != null)
                        pair.IsLocal = _isLocal;
                }
            }
        }

        /// <summary>
        /// 設定目標值（支援任意組數）
        /// </summary>
        public void SetTargetDists(SizeF targetSize)
        {
            foreach (var kvp in _dict)
            {
                if (kvp.Value == null) continue;

                if (kvp.Key.StartsWith("X"))
                    kvp.Value.TargetDist = targetSize.Width;

                else if (kvp.Key.StartsWith("Y"))
                    kvp.Value.TargetDist = targetSize.Height;
            }
        }

        /// <summary>
        /// 枚舉所有量測框對
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string key, LineBorderPair pair)> IterPairs()
        {
            foreach (var kvp in _dict)
            {
                if(kvp.Value != null)
                    yield return (kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 枚舉 所有 邊線拉框
        /// (不限於 左上右下 四個)
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public IEnumerable<QvBox2D> IterLineBorderBoxes()
        {
            foreach (var kvp in _dict)
            {
                var pair = kvp.Value;
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
        /// 枚舉 抓到的 所有邊線 
        /// (不限於 左上右下 四個) 
        /// </summary>
        /// <remarks>
        /// 單位 pixels (FullFov Cammera Coordinates)
        /// </remarks>
        public IEnumerable<EzLSD.LineSegment> IterLineSegments()
        {
            foreach (var kvp in _dict)
            {
                var pair = kvp.Value;
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
        public bool AdjustPairsNumber(int numX, int numY)
        {
            var targetKeyNames = new HashSet<string>();
            for (int i = 0; i < numX; i++) targetKeyNames.Add(i == 0 ? "X" : $"X{i}");
            for (int i = 0; i < numY; i++) targetKeyNames.Add(i == 0 ? "Y" : $"Y{i}");

            bool isChanged = false;

            // 移除多餘的 Key
            var existingKeys = _dict.Keys.ToList();
            foreach (var key in existingKeys)
            {
                if (!targetKeyNames.Contains(key))
                {
                    _dict.Remove(key);
                    isChanged = true;
                }
            }

            // 補足缺少的 Key
            foreach (var key in targetKeyNames)
            {
                if (!_dict.ContainsKey(key))
                {
                    //var pair = new LineBorderPair(_isLocal);
                    //pair.Borders[0] = new QvBox2D();
                    //pair.Borders[1] = new QvBox2D();
                    _dict.Add(key, new LineBorderPair(_isLocal));
                    isChanged = true;
                }
            }

            return isChanged;
        }

        /// <summary>
        /// 取得數據數量
        /// </summary>
        public int GetPairsNumbers(out int numX, out int numY)
        {
            numX = 0;
            numY = 0;

            if (_dict.Count == 0)
                return 0;

            foreach (var kv in _dict)
            {
                var key = kv.Key;
                var pair = kv.Value;
                if (pair == null) continue;
                if (key.StartsWith("X") || key.EndsWith("X"))
                    numX++;
                else
                    numY++;
            }

            return numX + numY;
        }

        #region INTERFACE_FOR_SIMPLE_QUAD_為了相容_舊接口
        /// <summary>
        /// 取得 左, 上, 右, 下, 四邊線
        /// </summary>
        public EzLSD.LineSegment[] GetQuadLineSegments(bool allowsNull = false)
        {
            if (!_dict.TryGetValue("X", out var pairX) && allowsNull)
            {
                _dict["X"] = pairX = new LineBorderPair(_isLocal);
            }
            if (!_dict.TryGetValue("Y", out var pairY) && allowsNull)
            {
                _dict["Y"] = pairY = new LineBorderPair(_isLocal);
            }

            if (pairX == null || pairY == null)
                return null;

            var lines = new[]
            {
                pairX?.LineSegments[0],
                pairY?.LineSegments[0],
                pairX?.LineSegments[1],
                pairY?.LineSegments[1],
            };

            if (!allowsNull)
            {
                foreach (var line in lines)
                {
                    if (line == null)
                        return null;
                }
            }

            return lines;
        }
        public bool Get(EdgeBorder eb, out EzLSD.LineSegment borderLine)
        {
            (string key, int i) = eb.GetKeyOffset();
            if (_dict.TryGetValue(key, out var pair))
                borderLine = pair.LineSegments[i];
            else
                borderLine = null;
            return borderLine != null;
        }
        public void Set(EdgeBorder eb, EzLSD.LineSegment borerLine)
        {
            (string key, int i) = eb.GetKeyOffset();
            if (!_dict.TryGetValue(key, out var pair))
                _dict[key] = pair = new LineBorderPair(_isLocal);
            pair.LineSegments[i] = borerLine;
        }
        public bool Get(EdgeBorder eb, out QvBox2D borderBox)
        {
            (string key, int i) = eb.GetKeyOffset();
            if (_dict.TryGetValue(key, out var pair))
                borderBox = pair.Borders[i];
            else
                borderBox = null;
            return borderBox != null;
        }
        public void Set(EdgeBorder eb, QvBox2D borderBox)
        {
            (string key, int i) = eb.GetKeyOffset();
            if (!_dict.TryGetValue(key, out var pair))
                _dict[key] = pair = new LineBorderPair(_isLocal);
            pair.Borders[i] = borderBox;
        }
        #endregion
    }


    public static class LineEdgeBorderExtension
    {
        public static (string, int) GetKeyOffset(this EdgeBorder eb)
        {
            switch (eb)
            {
                case EdgeBorder.Left: return ("X", 0);
                case EdgeBorder.Top: return ("Y", 0);
                case EdgeBorder.Right: return ("X", 1);
                case EdgeBorder.Bottom: return ("Y", 1);
            }
            return (null, -1);
        }
    }
}
