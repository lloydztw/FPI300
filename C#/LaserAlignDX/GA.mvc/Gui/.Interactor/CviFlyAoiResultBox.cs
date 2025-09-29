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
using LeTian.AoiLib;
using System;
using System.Drawing;

namespace LaserAlignDX.Mvc.Gui
{
    public class CviFlyAoiResultBox : CvImageViewerInteractor
    {
        #region GUI_MEMBERS
        FlyAoiResult _aoiResult;
        CviRotRectBox[] _drawItems;
        #endregion

        public void Update(FlyAoiResult result)
        {
            _aoiResult = result;
            var meta = result?.MetaData;
            if (meta != null)
            {
                if (meta.xBlobs == null)
                    _drawItems = createSingleDrawItem(result);
                else
                    _drawItems = createBlobsDrawItems(result);
            }
            else
            {
                _drawItems = null;
            }
        }

        #region OVERRIDES
        public override void OnDraw(CvImageViewer viewer, Graphics gxView)
        {
            bool isWorld = viewer.IsInWorldCoordinate();
            if (!isWorld)
                viewer.SwitchToWorldCoordinate(gxView);

            draw_cross_lines(viewer, gxView);
            draw_contents(viewer, gxView);

            if (!isWorld)
                viewer.SwitchToViewportCoordinate(gxView);

            draw_text(viewer, gxView);
        }
        #endregion

        #region DRAW_FUNCTIONS
        void draw_cross_lines(CvImageViewer viewer, Graphics gxView)
        {
            var pen = viewer.GetOnePixelPen(Color.Gold);
            var rect = viewer.GetWorldRect();
            var w = rect.Width;
            var h = rect.Height;
            gxView.DrawLine(pen, 0f, h / 2f, w, h / 2f);
            gxView.DrawLine(pen, w / 2f, 0f, w / 2f, h);
        }
        void draw_contents(CvImageViewer viewer, Graphics gxView)
        {
            if (_drawItems != null)
            {
                foreach (var drawItem in _drawItems)
                    drawItem?.OnDraw(viewer, gxView);
            }
        }
        void draw_text(CvImageViewer viewer, Graphics gxView)
        {
            if (_aoiResult != null && _aoiResult.Code == PlcFlyResultCode.OK)
            {
                bool isWorld = viewer.IsInWorldCoordinate();
                if (isWorld)
                    viewer.SwitchToViewportCoordinate(gxView);

                //if (_aoiResult != null && _aoiResult.Code == PlcFlyResultCode.OK)
                {
                    var rect = viewer.GetViewportRect();
                    rect.Height = 26;
                    gxView.FillRectangle(Brushes.DimGray, rect);

                    var text = formatText(_aoiResult);
                    var font = viewer.Font;
                    var br = _aoiResult.Code == PlcFlyResultCode.OK ? Brushes.Lime : Brushes.Red;
                    gxView.DrawString(text, font, br, 8f, 2f);
                }

                if (isWorld)
                    viewer.SwitchToWorldCoordinate(gxView);
            }
        }
        #endregion

        #region MVD_DISPLAY_FUNCTIONS
        CviRotRectBox[] createSingleDrawItem(FlyAoiResult aoiResult)
        {
            try
            {
                var flyMetaData = aoiResult?.MetaData;
                var xResultBox2D = flyMetaData?.xResultBox2D;

                if (xResultBox2D != null)
                {
                    var flyID = flyMetaData.flyID;
                    var resultCode = aoiResult.Code;

                    //string text = formatText(flyID.ShowID, aoiResult.OffsetX, aoiResult.OffsetY, aoiResult.OffsetAngle);
                    Color color = resultCode == PlcFlyResultCode.OK ? Color.Lime : Color.Red;
                    var drawItem = new CviRotRectBox(xResultBox2D, color);
                    return new CviRotRectBox[] { drawItem };
                }
            }
            catch (Exception ex)
            {
                _ERROR(ex, "createSingleDrawItem");
            }
            return new CviRotRectBox[0];
        }
        CviRotRectBox[] createBlobsDrawItems(FlyAoiResult aoiResult)
        {
            try
            {
                var flyMetaData = aoiResult?.MetaData;
                var xBlobs = flyMetaData?.xBlobs;

                if (xBlobs != null && xBlobs.Length >= 2)
                {
                    var flyID = flyMetaData.flyID;
                    var resultCode = aoiResult.Code;

                    //string text = formatText(flyID.ShowID, aoiResult.OffsetX, aoiResult.OffsetY, aoiResult.OffsetAngle);
                    Color color = resultCode == PlcFlyResultCode.OK ? Color.Lime : Color.Red;

                    if (resultCode == PlcFlyResultCode.OK)
                    {
                        var drawItems = Array.ConvertAll(xBlobs, b => new CviRotRectBox(b, color));
                        return drawItems;
                    }
                    else
                    {
                        var rect = flyMetaData.roiRect;
                        var drawItem = new CviRotRectBox(rect, color);
                    }
                }
            }
            catch (Exception ex)
            {
                _ERROR(ex, "createBlobsDrawItems");
            }
            return new CviRotRectBox[0];
        }
        string formatText(int flyShowIndex, float offsetX, float offsetY, float offsetAngle)
        {
            string text = $"[{flyShowIndex}]" +
                        $" x:{offsetX:0.000}," +
                        $" y:{offsetY:0.000}," +
                        $" a:{offsetAngle:0.000}";
            return text;
        }
        string formatText(FlyAoiResult flyAoiResult)
        {
            var meta = flyAoiResult?.MetaData;
            if (meta != null)
                return formatText(meta.flyID.ShowID, flyAoiResult.OffsetX, flyAoiResult.OffsetY, flyAoiResult.OffsetAngle);
            return "";
        }
        #endregion

        #region LOG
        void _ERROR(Exception ex, string message)
        {
            LtDebug.LOG.Error(ex, message);
        }
        #endregion
    }
}
