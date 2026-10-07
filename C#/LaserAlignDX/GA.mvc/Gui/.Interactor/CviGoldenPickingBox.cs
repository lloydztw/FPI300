#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.ImageViewerEx;
using System;
using System.Drawing;
using System.Windows.Forms;
using CviSelectionBox = JetEazy.ImageViewerEx.Interactors.CviSelectionBox;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviGoldenPickingBox : CviSelectionBox
    {
        public event EventHandler OnBoxSelected;

        #region PRIVATE_DATA
        bool _isPicking = false;
        #endregion

        public CviGoldenPickingBox(Brush brush) : base(brush)
        {
            Visible = false;
        }
        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            _isPicking = Visible;
            return base.OnMouseDown(viewer, e);
        }
        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            if (_isPicking)
            {
                _isPicking = false;
                OnBoxSelected?.Invoke(this, null);
            }
            return base.OnMouseUp(viewer, e);
        }
    }
}
