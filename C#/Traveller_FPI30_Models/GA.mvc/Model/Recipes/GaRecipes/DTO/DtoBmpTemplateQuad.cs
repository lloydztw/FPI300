#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-08-25 新增 Corners 欄位
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Drawing;
using System.Linq;

namespace LaserAlignDX.Mvc.Model.Recipe
{
    public class DtoBmpTemplateQuad : DtoBmpTemplate
    {
        #region PRIVATE_DATA
        private PointF[] _corners;
        #endregion

        public DtoBmpTemplateQuad(string templateName, string sectName = null, string keyName = null, string ext = ".bmp")
            : base(templateName, sectName, keyName, ext)
        {
        }

        /// <summary>
        /// 頂點座標陣列 (例如 4 個角點)。
        /// 若為 null 或長度小於 4，則預設回傳由 RectF 組成的 4 個角點 (左上、右上、右下、左下)。
        /// 設定時若含有 >= 4 個點，會自動計算能包覆所有 Corners 的最小 RectF。
        /// </summary>
        public PointF[] Corners
        {
            get
            {
                // 如果 _corners 為 null 或角點數量小於 4，使用 RectF 產生四個角點 (順時針: 左上, 右上, 右下, 左下)
                if (_corners == null || _corners.Length < 4)
                {
                    return new PointF[]
                    {
                        new PointF(RectF.Left, RectF.Top),      // 左上 (Top-Left)
                        new PointF(RectF.Right, RectF.Top),     // 右上 (Top-Right)
                        new PointF(RectF.Right, RectF.Bottom),  // 右下 (Bottom-Right)
                        new PointF(RectF.Left, RectF.Bottom)    // 左下 (Bottom-Left)
                    };
                }

                return _corners;
            }
            set
            {
                _corners = value;

                // 若傳入有效的角點陣列 (>= 4 個點)，自動更新包覆所有 Corners 的最小 RectF
                if (_corners != null && _corners.Length >= 4)
                {
                    float minX = _corners.Min(p => p.X);
                    float maxX = _corners.Max(p => p.X);
                    float minY = _corners.Min(p => p.Y);
                    float maxY = _corners.Max(p => p.Y);

                    RectF = new RectangleF(minX, minY, maxX - minX, maxY - minY);
                }
            }
        }

        /// <summary>
        /// 覆寫 SetTag 讓鏈式呼叫回傳自身類別
        /// </summary>
        public new DtoBmpTemplateQuad SetTag(string tag)
        {
            base.SetTag(tag);
            return this;
        }

        public override void Load(string iniFileName)
        {
            base.Load(iniFileName);
            LoadCorners(iniFileName, _sectName, _keyName != null ? _keyName + "_Corners" : "Corners");
        }

        public override void Save(string iniFileName)
        {
            base.Save(iniFileName);
            SaveCorners(iniFileName, _sectName, _keyName != null ? _keyName + "_Corners" : "Corners");
        }

        #region PRIVATE_LOAD_SAVE_FUNCTIONS
        void LoadCorners(string iniFileName, string sectName, string keyName)
        {
            normalize(ref iniFileName, ref sectName, ref keyName);

            // 讀取 INI 檔，若讀出的資料小於 4 個點，Read 會賦予 Corners null/空陣列，進而自動觸發 get 回傳 RectF 預設角點
            Read(iniFileName, sectName, keyName, new PointF[0], out PointF[] corners);
            Corners = corners;
        }

        void SaveCorners(string iniFileName, string sectName, string keyName)
        {
            normalize(ref iniFileName, ref sectName, ref keyName);

            // 寫入 INI 時直接取用 Corners (若未手動指定過，會自動取 RectF 拆解出來的 4 個角點儲存)
            Write(iniFileName, sectName, keyName, Corners);
        }
        #endregion
    }
}