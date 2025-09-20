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
        RcpBmpHolder _bmpHolder;
        string _sectName;
        string _keyName;
        #endregion

        public DtoBmpTemplate(string name, string sectName=null, string keyName = null, string ext = ".bmp")
        {
            _bmpHolder = new RcpBmpHolder(name, ext);
            _sectName = sectName;
            _keyName = keyName;
        }
        public void Dispose()
        {
            _bmpHolder?.Dispose();
        }

        public Bitmap Bmp
        {
            get => _bmpHolder.Peek();
            set => _bmpHolder.TakeOver(value);
        }
        public RectangleF RectF
        {
            get; set;
        }

        public override void Load(string iniFileName, string sectName = null, string keyName = null)
        {
            //xRegionTrain = StringtoRectF(ReadINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            LoadRectF(iniFileName, sectName, keyName);
            LoadBmp(true);
        }
        public override void Save(string iniFileName, string sectName = null, string keyName = null)
        {
            //WriteINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(Roi), iniFileName);
            //saveImage(bmpDefectTemplate, "bmpDefectTemplate.bmp");
            SaveRectF(iniFileName, sectName, keyName);
            SaveBmp();
        }

        public void LoadRectF(string iniFileName, string sectName, string keyName)
        {
            normalize(ref sectName, ref keyName);
            RectF = StringtoRectF(ReadINIValue(sectName, keyName, RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFileName));
        }
        public void SaveRectF(string iniFileName, string sectName, string keyName)
        {
            normalize(ref sectName, ref keyName);
            WriteINIValue(sectName, keyName, RectFtoStringSimple(RectF), iniFileName);
        }

        public void LoadBmp(bool force = false)
        {
            _bmpHolder.Load(force);
        }
        public void SaveBmp(bool force = false)
        {
            _bmpHolder.Save(force);
        }

        #region PRIVATE_FUNCTIONS
        private void normalize(ref string sectName, ref string keyName)
        {
            if (sectName == null)
                sectName = _sectName;
            if (keyName == null)
                keyName = _keyName;
            if (sectName == null)
                sectName = _bmpHolder.Name + "_rect";
            if (keyName == null)
                keyName = "rect";
        }
        #endregion
    }
}
