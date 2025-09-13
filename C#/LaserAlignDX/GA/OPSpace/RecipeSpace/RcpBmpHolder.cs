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

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class RcpBmpHolder : IDisposable
    {
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
            if (_bmp == null)
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
        public void Load(bool force = false)
        {
            if (_bmp == null || force)
            {
                var old = _bmp;
                _bmp = loadImage(_name);
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
        Bitmap loadImage(string name)
        {
            foreach (var ext in new[] { ".jpg", ".bmp" })
            {
                string fileName = System.IO.Path.Combine(CommonPath, name + ext);
                if (System.IO.File.Exists(fileName))
                {
                    return GaImageUtil.LoadBigImage(fileName);
                }
            }
            return null;
        }
        void saveImage(Bitmap bmp, string name, string ext = ".jpg")
        {
            string fileName = System.IO.Path.Combine(CommonPath, name + ext);
            GaImageUtil.SaveBigImage(fileName, bmp);
        }
        #endregion
    }
}
