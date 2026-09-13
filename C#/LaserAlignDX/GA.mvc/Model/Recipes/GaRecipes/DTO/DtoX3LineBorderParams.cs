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
    /// 邊線_手拉框區塊_LINE_BORDER_BOXES
    /// </summary>
    /// <remarks>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </remarks>
    public class DtoX3LineBorderParams : DtoBase
    {
        #region DATA
        /// <summary>
        /// 量測框資料
        /// </summary>
        public readonly LineBorderPairsCollection LineBorderPairs = new LineBorderPairsCollection(true);
        #endregion

        #region PUBLIC_OPERATORS
        public static implicit operator LineBorderPairsCollection(DtoX3LineBorderParams dto)
        {
            return dto?.LineBorderPairs;
        }
        public static implicit operator Dictionary<string,LineBorderPair>(DtoX3LineBorderParams dto)
        {
            return dto?.LineBorderPairs;
        }
        public LineBorderPair this[string measureKeyName]
        {
            //get => LineBorderPairs.TryGetValue(measureKeyName, out var pair) ? pair : null;
            //set { if (value != null) LineBorderPairs[measureKeyName] = value; }
            get => LineBorderPairs[measureKeyName];
            set => LineBorderPairs[measureKeyName] = value;
        }
        #endregion

        #region 相容舊接口
        public RectangleF xLineLeft { get => getOldBorder("X", 0); set => setOldBorder("X", 0, value); }
        public RectangleF xLineRight { get => getOldBorder("X", 1); set => setOldBorder("X", 1, value); }
        public RectangleF xLineTop { get => getOldBorder("Y", 0); set => setOldBorder("Y", 0, value); }
        public RectangleF xLineBottom { get => getOldBorder("Y", 1); set => setOldBorder("Y", 1, value); }

        private RectangleF getOldBorder(string key, int borderIndex)
        {
            if (LineBorderPairs.TryGetValue(key, out var pair) &&
                pair?.Borders != null &&
                pair.Borders.Length > borderIndex &&
                pair.Borders[borderIndex] != null)
            {
                return pair.Borders[borderIndex].BoundaryRect;
            }
            return new RectangleF(0, 0, 100, 100);
        }
        private void setOldBorder(string key, int borderIndex, RectangleF rect)
        {
            if (!LineBorderPairs.TryGetValue(key, out var pair) || pair == null)
            {
                LineBorderPairs[key] = pair = new LineBorderPair(LineBorderPairs.IsLocal);
            }
            pair.Borders[borderIndex] = QvQuad2D.From(rect).ToBox2D();
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
        /// 是否 為 簡單四邊線 (相容原有的計算)
        /// </summary>
        public bool IsSimpleQuad
        {
            get => LineBorderPairs.ContainsKey("X") && LineBorderPairs.ContainsKey("Y");
        }

        public override void Load(string iniFile)
        {
            LineBorderPairs.Clear();

            bool ok = loadMeasureKeyNames(iniFile, _SectName, out var keyNames);

            if (ok)
            {
                // 載入 新版數據
                int N = keyNames.Length;
                for (int i = 0; i < N; i++)
                {
                    loadBorderPair(iniFile, _SectName, i, out var pair);
                    if (pair != null)
                        LineBorderPairs[keyNames[i]] = pair;
                }
            }
            else
            {
                // 載入 舊版數據
                var oldParams = new DtoX3OldLineBorderParams() { CarrierTag = CarrierTag };
                oldParams.Load(iniFile);

                // X: xLineLeft, xLineRight
                var pairX = new LineBorderPair(LineBorderPairs.IsLocal);
                pairX.Borders[0] = QvQuad2D.From(oldParams.xLineLeft).ToBox2D();
                pairX.Borders[1] = QvQuad2D.From(oldParams.xLineRight).ToBox2D();
                LineBorderPairs["X"] = pairX;

                // Y: xLineTop, xLineBottom
                var pairY = new LineBorderPair(LineBorderPairs.IsLocal);
                pairY.Borders[0] = QvQuad2D.From(oldParams.xLineTop).ToBox2D();
                pairY.Borders[1] = QvQuad2D.From(oldParams.xLineBottom).ToBox2D();
                LineBorderPairs["Y"] = pairY;
            }

            LineBorderPairs.IsLocal = true;
        }

        public override void Save(string iniFile)
        {
            var measureKeyNames = LineBorderPairs.Keys.ToArray();
            saveMeasureKeyNames(iniFile, _SectName, measureKeyNames);
            for(int i = 0; i < measureKeyNames.Length; i++)
            {
                var name = measureKeyNames[i];
                saveBorderPair(iniFile, _SectName, i, LineBorderPairs[name]);
            }
        }

        #region PRIVATE_SERIALIZATION_FUNCTIONS
        private string _SectName
        {
            //注意: CarrierTag 目前只有兩種值: "" 或 "@C2"
            //      這裡多了一個 _ , 將錯就錯.
            get => $"LineBorders_{CarrierTag}";
        }
        private bool loadMeasureKeyNames(string iniFile, string sectName, out string[] keyNames)
        {
            keyNames = null;

            Read(iniFile, _SectName, "KeyNames", "", out string str);
            if (string.IsNullOrEmpty(str))
                return false;

            var strs = str.Split(',');
            var lst = Array.ConvertAll(strs, s => s?.Trim()).ToList();
            lst.RemoveAll(s => string.IsNullOrEmpty(s));
            keyNames = lst.ToArray();

            // 允許 keyNames.Length == 0
            return true;
        }
        private void saveMeasureKeyNames(string iniFile, string sectName, string[] keyNames)
        {
            string str;

            if (keyNames == null || keyNames.Length == 0)
                str = "";
            else
                str = string.Join(",", keyNames);

            Write(iniFile, _SectName, "KeyNames", str);
        }
        private bool loadBorderPair(string iniFile, string sectName, int index, out LineBorderPair pair)
        {
            string key = $"BorderPair_{index}";
            Read(iniFile, sectName, key, null, out PointF[] corners);

            if(corners==null || corners.Length < 8)
            {
                pair = null;
                return false;
            }
            else
            {
                pair = new LineBorderPair(LineBorderPairs.IsLocal);
                var corners0 = new PointF[4];
                var corners1 = new PointF[4];
                Array.Copy(corners, 0, corners0, 0, 4);
                Array.Copy(corners, 4, corners1, 0, 4);
                pair.Borders[0] = new QvBox2D() { Corners = corners0 };
                pair.Borders[1] = new QvBox2D() { Corners = corners1 };
                return true;
            }
        }
        private void saveBorderPair(string iniFile, string sectName, int index, LineBorderPair pair)
        {
            string key = $"BorderPair_{index}";
            if (pair == null)
            {
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


    /// <summary>
    /// 舊版 邊線_手拉框區塊_LINE_BORDER_BOXES
    /// </summary>
    public class DtoX3OldLineBorderParams : DtoBase
    {
        #region 邊線_手拉框區塊_LINE_BORDER_BOXES
        /// <summary>
        /// 邊線框(左)
        /// </summary>
        public RectangleF xLineLeft = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(上)
        /// </summary>
        public RectangleF xLineTop = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(右)
        /// </summary>
        public RectangleF xLineRight = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(下)
        /// </summary>
        public RectangleF xLineBottom = new RectangleF(0, 0, 100, 100);
        #endregion

        public string CarrierTag
        {
            get;
            set;
        }
        public override void Load(string iniFile)
        {
            string sectName = "Recipe Basic" + CarrierTag;
            xLineLeft = StringtoRectF(ReadINIValue(sectName, "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineTop = StringtoRectF(ReadINIValue(sectName, "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineRight = StringtoRectF(ReadINIValue(sectName, "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
            xLineBottom = StringtoRectF(ReadINIValue(sectName, "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFile));
        }
        public override void Save(string iniFile)
        {
            string sectName = "Recipe Basic" + CarrierTag;
            WriteINIValue(sectName, "xLineLeft", RectFtoStringSimple(xLineLeft), iniFile);
            WriteINIValue(sectName, "xLineTop", RectFtoStringSimple(xLineTop), iniFile);
            WriteINIValue(sectName, "xLineRight", RectFtoStringSimple(xLineRight), iniFile);
            WriteINIValue(sectName, "xLineBottom", RectFtoStringSimple(xLineBottom), iniFile);
        }

        #region PRIVATE_SERIALIZATION_FUNCTIONS
        private string RectFtoStringSimple(RectangleF RectF)
        {
            string Str = "";

            Str += RectF.X.ToString() + ",";
            Str += RectF.Y.ToString() + ",";
            Str += RectF.Width.ToString() + ",";
            Str += RectF.Height.ToString();

            return Str;
        }
        private RectangleF StringtoRectF(string RectStr)
        {
            if (string.IsNullOrEmpty(RectStr))
                return new RectangleF(0, 0, 100, 100);

            string[] strs = RectStr.Split(',');
            if (strs.Length < 4)
                return new RectangleF(0, 0, 100, 100);

            float.TryParse(strs[0], out float x);
            float.TryParse(strs[1], out float y);
            float.TryParse(strs[2], out float w);
            float.TryParse(strs[3], out float h);

            return new RectangleF(x, y, w, h);
        }
        #endregion
    }
}
