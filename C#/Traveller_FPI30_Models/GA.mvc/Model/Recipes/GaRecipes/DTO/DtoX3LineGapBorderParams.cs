#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-20 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.DTO;
using JetEazy.QvMath;
using LaserAlignDX.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// 邊隙 手拉框 參數
    /// </summary>
    /// <remarks>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </remarks>
    public class DtoX3LineGapBorderParams : DtoBase
    {
        #region DATA
        /// <summary>
        /// 量測框資料
        /// </summary>
        public readonly LineBorderPairsCollection GapBorderPairs = new LineBorderPairsCollection(true);
        #endregion

        #region PUBLIC_OPERATORS
        public static implicit operator LineBorderPairsCollection(DtoX3LineGapBorderParams dto)
        {
            return dto?.GapBorderPairs;
        }
        public static implicit operator Dictionary<string,LineBorderPair>(DtoX3LineGapBorderParams dto)
        {
            return dto?.GapBorderPairs;
        }
        public LineBorderPair this[GapEnum gap]
        {
            get => GapBorderPairs.TryGetValue(gap.ToString(), out var pair) ? pair : null;
            set => GapBorderPairs[gap.ToString()] = value;
        }
        #endregion

        /// <summary>
        /// 載台 Runtime Tag
        /// </summary>
        /// <remarks>
        /// 目前只有兩種值: "" 或 "@C2"
        /// </remarks>
        public string CarrierTag
        {
            get;
            set;
        }

        /// <summary>
        /// 是否使用平均邊隙
        /// </summary>
        public bool UseAveGaps4
        {
            get; 
            set;
        }

        /// <summary>
        /// 是否 為默認 LD, LU, RU, RD 四邊隙量測位置
        /// </summary>
        public bool UseDefaultBorders
        {
            get;
            set;
        }

        public override void Load(string iniFile)
        {
            Read(iniFile, _SectName0, "UseAveGaps4", true, out bool useAveGaps4);
            UseAveGaps4 = useAveGaps4;

            Read(iniFile, _SectNameC, "UseDefaultBorders", false, out bool useDefaults);
            UseDefaultBorders = useDefaults;

            GapBorderPairs.Clear();

            foreach (GapEnum gap in Enum.GetValues(typeof(GapEnum)))
            {
                loadBorderPair(iniFile, _SectNameC, gap, out var pair);
                if (pair != null)
                    GapBorderPairs[gap.ToString()] = pair;
            }

            int count = GapBorderPairs.GetPairsNumbers(out int numX, out int numY);
            if (count < 8)
                UseDefaultBorders = true;

            //>>> GapBorderPairs.IsLocal = true;
            System.Diagnostics.Debug.Assert(GapBorderPairs.IsLocal == true, "參數設定的 GapBorderPairs 必須是 IsLocal == true!");
        }

        public override void Save(string iniFile)
        {
            Write(iniFile, _SectName0, "UseAveGaps4", UseAveGaps4);
            Write(iniFile, _SectNameC, "UseDefaultBorders", UseDefaultBorders);

            foreach (GapEnum gap in Enum.GetValues(typeof(GapEnum)))
            {
                if (GapBorderPairs.TryGetValue(gap.ToString(), out var pair) && pair != null)
                    saveBorderPair(iniFile, _SectNameC, gap, pair);
            }
        }

        #region PRIVATE_SERIALIZATION_FUNCTIONS
        private string _SectName0
        {
            get => "GapBorders";
        }
        private string _SectNameC
        {
            //注意: CarrierTag 目前只有兩種值: "" 或 "@C2"
            get => $"{_SectName0}{CarrierTag}";
        }
        private bool loadBorderPair(string iniFile, string sectName, GapEnum gap, out LineBorderPair pair)
        {
            string key = $"BorderPair_{gap}";
            Read(iniFile, sectName, key, null, out PointF[] corners);

            if (corners == null || corners.Length < 8)
            {
                pair = null;
                return false;
            }
            else
            {
                pair = new LineBorderPair(GapBorderPairs.IsLocal);
                var corners0 = new PointF[4];
                var corners1 = new PointF[4];
                Array.Copy(corners, 0, corners0, 0, 4);
                Array.Copy(corners, 4, corners1, 0, 4);
                pair.Borders[0] = new QvBox2D() { Corners = corners0 };
                pair.Borders[1] = new QvBox2D() { Corners = corners1 };
                return true;
            }
        }
        private void saveBorderPair(string iniFile, string sectName, GapEnum gap, LineBorderPair pair)
        {
            string key = $"BorderPair_{gap}";
            if (pair == null)
            {
                //writeStr(iniFile, sectName, key, "");
                Write(iniFile, sectName, key, "");
            }
            else
            {
                var corners = new List<PointF>(pair.Borders[0].Corners);
                corners.AddRange(pair.Borders[1].Corners);
                Write(iniFile, sectName, key, corners.ToArray());
            }
        }
        #endregion
    }
}
