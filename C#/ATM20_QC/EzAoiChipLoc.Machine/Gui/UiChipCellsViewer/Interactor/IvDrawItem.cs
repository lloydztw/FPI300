#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-31 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using System.Drawing;

namespace LaserAlignDX.Mvc.Gui
{
    public interface IvDrawItem
    {
        object Tag { get; set; }
        void OnDraw(CvImageViewer viewer, Graphics gxView);
    }
}
