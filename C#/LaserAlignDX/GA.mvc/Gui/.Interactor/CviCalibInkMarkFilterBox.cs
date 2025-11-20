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
using LaserAlignDX.AoiModel;
using System.Drawing;


namespace LaserAlignDX.Mvc.Gui
{
    public class CviCalibInkMarkFilterBox : CvImageViewerInteractor
    {
        #region PRIVATE_DATA
        JxCalibInkMarkSettings _jxRecipe;
        #endregion

        public void Apply(JxCalibInkMarkSettings jxRecipe)
        {
            _jxRecipe = jxRecipe;
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_jxRecipe != null)
            {

            }
        }
        #endregion
    }
}
