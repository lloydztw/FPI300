using System;
using System.Drawing;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    /// <summary>
    /// DTO (Data Transfer Object) 類別
    /// DTO 是標準用詞 請自行查 ChatGPT or DeepSeek
    /// </summary>
    internal class DtoBmpTemplate : DtoBase, IDisposable
    {
        #region PRIVATE_DATA
        RcpBmpHolder _bmpHolder;
        #endregion

        public DtoBmpTemplate(string name, string ext = ".bmp")
        {
            _bmpHolder = new RcpBmpHolder(name, ext);
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
            RectF = StringtoRectF(ReadINIValue(sectName, keyName, RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), iniFileName));
        }
        public void SaveRectF(string iniFileName, string sectName, string keyName)
        {
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
    }
}
