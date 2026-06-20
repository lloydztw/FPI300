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

using EzAoiChipLocQC.Model;
using JetEazy.ImageViewerEx;
using System.Collections.Generic;
using System.Drawing;

namespace EzAoiChipLocQC.Gui.QcTrayView
{
    public class CviQcTrayPlaceHoldsBox : CvImageViewerInteractor
    {
        #region CONFIG
        static int MM_PER_PIXEL => QcTrayDrawConfig.MM_PER_PIXEL;
        #endregion

        #region GUI_MEMBERS
        Font _font = null;
        List<CviRotRectBox> _cviPlaceHolds;
        #endregion

        public object Tag
        {
            get;
            set;
        }

        public void UpdatePlaceHolds(JxTrayDimSettings traySettings)
        {
            _cviPlaceHolds = new List<CviRotRectBox>();

            if (traySettings == null)
                return;

            int rows = (int)traySettings.FullRows.Value;
            int cols = (int)traySettings.FullCols.Value;

            double pitchX = (double)traySettings.PitchX.Value;
            double pitchY = (double)traySettings.PitchY.Value;

            double blocWidth = (double)traySettings.PlaceHoldSizeX.Value;
            double blocHeight = (double)traySettings.PlaceHoldSizeY.Value;

            var center0 = traySettings.GetFirstPlaceHoldCoord();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double y = center0.Y + pitchY * r;
                    double x = center0.X + pitchX * c;
                    var cx = (float)(x * MM_PER_PIXEL);
                    var cy = (float)(y * MM_PER_PIXEL);
                    var cw = (float)(blocWidth * MM_PER_PIXEL);
                    var ch = (float)(blocHeight * MM_PER_PIXEL);
                    var rect = JetEazy.Qcvt.CreateCenterRect(cx, cy, cw, ch);
                    var box = new CviRotRectBox(ref rect, QcTrayDrawConfig.PlaceHoldColor, 0.25f);
                    box.Visible = true;
                    _cviPlaceHolds.Add(box);
                }
            }
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            if (_cviPlaceHolds == null || _cviPlaceHolds.Count == 0)
                return;

            if (_font == null)
                _font = viewer.Font;

            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            foreach(var box in _cviPlaceHolds)
            {
                if (box == null) continue;
                box.OnDraw(viewer, gxView);
            }

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);
        }
        #endregion
    }
}
