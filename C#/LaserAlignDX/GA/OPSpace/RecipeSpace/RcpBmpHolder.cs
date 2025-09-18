#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-09 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using System;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class RcpBmpHolder : IDisposable
    {
        #region CONFIG
        static bool OPT_AUTO_CHANGE_BMP_TO_JPG = true;
        #endregion

        internal static Func<string> CommonPathFunc;
        string CommonPath
        {
            get
            {
                return (CommonPathFunc == null) ? "" : CommonPathFunc();
            }
        }

        #region PRIVATE_DATA
        Bitmap _bmp;
        string _name;
        bool _isDirty;
        #endregion

        public RcpBmpHolder(string name)
        {
            _name = name;
        }

        public Bitmap Peek()
        {
            if (!checkExiting(_bmp))
                Load();
            return _bmp;
        }
        public void TakeOver(Bitmap bmp)
        {
            if (_bmp != bmp)
            {
                var old = _bmp;
                _bmp = bmp;
                _isDirty = true;
                old?.Dispose();
            }
        }
        public void Load(bool force = false, string ext = null)
        {
            if (_bmp == null || force)
            {
                var old = _bmp;
                _bmp = loadImage(_name, ext);
                _isDirty = false;
                old?.Dispose();
            }
        }
        public void Save(bool force = false, string ext = ".jpg")
        {
            if (_isDirty || force)
            {
                saveImage(_bmp, _name, ext);
                _isDirty = false;
            }
        }
        public void Dispose()
        {
            _bmp?.Dispose();
            _bmp = null;
            _isDirty = false;
        }

        public static implicit operator Bitmap(RcpBmpHolder holder)
        {
            return holder?.Peek();
        }

        #region PRIVATE_FUNCTIONS
        bool checkExiting(Bitmap bmp)
        {
            if (bmp == null)
                return false;
            try
            {
                return bmp != null && bmp.Size != Size.Empty;
            }
            catch
            {
                // 被外部 無預警 調用 Dispose() 清除 了!
                _bmp = null;
                return false;
            }
        }
        Bitmap loadImage(string name, string assignedExt = null)
        {
            // 如果沒有指定, 優先載入 ".jpg" 其次 ".bmp"
            var exts = (assignedExt == null) ? 
                        new[] { ".jpg", ".bmp" }: 
                        new[] { assignedExt };

            foreach (var ext in exts)
            {
                string fileName = System.IO.Path.Combine(CommonPath, name + ext);
                if (System.IO.File.Exists(fileName))
                {
                    var newBmp = GaImageUtil.LoadBigImage(fileName);
                    if (newBmp != null)
                    {
                        GaUtil.LOG($"RcpBmp [{_name}] 載入 {fileName}");
                        autoChangeToJpg(newBmp, fileName);
                    }
                    return newBmp;
                }
            }
            return null;
        }
        void saveImage(Bitmap bmp, string name, string ext = ".jpg")
        {
            string fileName = System.IO.Path.Combine(CommonPath, name + ext);
            GaImageUtil.SaveBigImage(fileName, bmp);
            GaUtil.LOG($"RcpBmp [{_name}] 寫入 {fileName}");
        }
        void autoChangeToJpg(Bitmap bmp, string srcFileName)
        {
            if (!OPT_AUTO_CHANGE_BMP_TO_JPG || bmp == null)
                return;
            string ext = System.IO.Path.GetExtension(srcFileName);
            if (ext != ".jpg")
            {
                var dstFileName = System.IO.Path.ChangeExtension(srcFileName, ".jpg");
                if (!System.IO.File.Exists(dstFileName))
                    GaImageUtil.SaveBigImage(dstFileName, bmp);
                try
                {
                    System.IO.File.Delete(srcFileName);
                }
                catch
                {

                }
            }
        }
        #endregion
    }
}
