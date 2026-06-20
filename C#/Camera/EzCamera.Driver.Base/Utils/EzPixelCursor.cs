#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-04-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using System.Drawing;

namespace EzCamera.Driver.Utils
{
    public class EzPixelCursor
    {
        #region PRIVATE_DATA
        bool _isPosChanged = false;
        #endregion

        public Color Color { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public void MoveTo(int x, int y)
        {
            if (X != x || Y != y)
            {
                X = x;
                Y = y;
                _isPosChanged = true;
            }
        }
        public void PickColor(Bitmap bmp, bool force = false)
        {
            if (_isPosChanged || force)
            {
                // 先用 local 變數保存 X, Y, 以防止其他線程造成變動.
                int x = X;  
                int y = Y;
                if (bmp != null && x >= 0 && y >= 0 && x < bmp.Width && y < bmp.Height)
                {
                    Color = bmp.GetPixel(x, y);
                }
                _isPosChanged = false;
            }
        }
    }
}
