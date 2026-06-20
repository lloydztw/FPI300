#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-20 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using OpenCvSharp;
using System.Drawing;

namespace EzAoiChipLocQC.Gui.QcTrayView
{
    internal static class QcTrayDrawConfig
    {
        public const int MM_PER_PIXEL = 10;
        public static Scalar TrayBackGroundColor => new Scalar(32, 32, 32);
        public static Color PlaceHoldColor => Color.Blue;
    }
}
