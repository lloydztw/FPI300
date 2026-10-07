#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-09 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using JetEazy.ImageViewerEx.Interactors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviRcpBox : CviRect
    {
        public event EventHandler OnChanged;

        #region PRIVATE_DATA
        bool _isHit;
        #endregion

        public CviRcpBox(Brush br, int lineWidth, int cornerSize) : base(br, lineWidth, cornerSize)
        {
        }

        #region OVERRIDES
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            bool ret = base.OnMouseDown(viewer, e);
            if (ret)
                _isHit = true;
            return ret;
        }
        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            if (_isHit)
            {
                _isHit = false;
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
            return base.OnMouseUp(viewer, e);
        }
        #endregion
    }
}
