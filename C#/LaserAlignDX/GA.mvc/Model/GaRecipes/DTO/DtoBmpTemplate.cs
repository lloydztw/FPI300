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

using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.Drawing;


namespace LaserAlignDX.Mvc.Model.Recipe
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    public class DtoBmpTemplate : DtoBase, IDisposable
    {
        #region PRIVATE_DATA
        Bitmap _templateBmp;
        string _tpName;
        string _sectName;
        string _keyName;
        string _postTag = "";
        #endregion

        public DtoBmpTemplate(string templateName, string sectName = null, string keyName = null, string ext = ".bmp")
        {
            _tpName = templateName;
            _sectName = sectName;
            _keyName = keyName;
        }
        public void Dispose()
        {
            _templateBmp?.Dispose();
            _templateBmp = null;
        }



        /// <summary>
        /// 目前須由 Caller 管理 Bmp 生命週期
        /// </summary>
        public Bitmap Bmp
        {
            get
            {
                if (_templateBmp == null)
                    LoadBmp(true);
                return _templateBmp;
            }
            set
            {
                _templateBmp = value;
            }
        }
        public RectangleF RectF
        {
            get; set;
        }

        /// <summary>
        /// 後綴名, 會決定 Bmp 存取檔的 後綴名稱.
        /// </summary>
        public DtoBmpTemplate SetTag(string tag)
        {
            _postTag = tag;
            return this;
        }
        public override void Load(string iniFileName)
        {
            LoadRectF(iniFileName, _sectName, _keyName);
            LoadBmp(true);
        }
        public override void Save(string iniFileName)
        {
            SaveRectF(iniFileName, _sectName, _keyName);
            SaveBmp();
        }

        void LoadRectF(string iniFileName, string sectName, string keyName)
        {
            normalize(ref iniFileName, ref sectName, ref keyName);
            RectF = StringtoRectF(ReadINIValue(sectName, keyName, RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFileName));
        }
        void SaveRectF(string iniFileName, string sectName, string keyName)
        {
            normalize(ref iniFileName, ref sectName, ref keyName);
            WriteINIValue(sectName, keyName, RectFtoStringSimple(RectF), iniFileName);
        }

        void LoadBmp(bool force = false)
        {
            if (_templateBmp == null || force)
            {
                // 利用 RcpBmpHolder 來加載圖檔
                var holderName = _tpName + _postTag;
                using (var holder = new RcpBmpHolder(holderName, ".bmp"))
                {
                    var newBmp = (Bitmap)holder.Peek()?.Clone();
                    if (newBmp == null)
                        newBmp = new Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    var old = _templateBmp;
                    _templateBmp = newBmp;
                    old?.Dispose();
                }
            }
        }
        void SaveBmp()
        {
            if (_templateBmp != null)
            {
                // 利用 RcpBmpHolder 來保存圖檔
                var holderName = _tpName + _postTag;
                using (var holder = new RcpBmpHolder(holderName, ".bmp"))
                {
                    holder.TakeOver((Bitmap)_templateBmp.Clone());
                    holder.Save(true);
                }
            }
        }

        #region PRIVATE_FUNCTIONS
        private void normalize(ref string iniFileName, ref string sectName, ref string keyName)
        {
            //if (!string.IsNullOrEmpty(_postTag))
            //{
            //    string path = System.IO.Path.GetDirectoryName(iniFileName);
            //    string stem = System.IO.Path.GetFileNameWithoutExtension(iniFileName);
            //    string ext = System.IO.Path.GetExtension(iniFileName);
            //    string[] strs = stem.Split('@');
            //    stem = strs[0] + _postTag + ext;
            //    iniFileName = System.IO.Path.Combine(path, stem);
            //}

            if (sectName == null)
                sectName = _sectName;
            if (keyName == null)
                keyName = _keyName;

            if (sectName == null)
                sectName = _tpName + _postTag + "_SECT";
            if (keyName == null)
                keyName = "RectF";

            // 根據 _postTag 自動變化 sectName
            if (!string.IsNullOrEmpty(_postTag))
            {
                string[] strs = sectName.Split('@');
                sectName = strs[0] + _postTag;
            }
        }
        #endregion
    }
}
