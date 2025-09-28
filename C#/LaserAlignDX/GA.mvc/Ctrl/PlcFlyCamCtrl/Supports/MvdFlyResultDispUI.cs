#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-23 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AUVision;
using JetEazy.QvMath;
using LaserAlignDX.AoiModel;
using LaserAlignDX.UISpace.UIMVC;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VisionDesigner;
using VisionDesigner.PositionFix;


namespace LaserAlignDX.Mvc.Gui
{
    internal class MvdFlyResultDispUI
    {
        #region PRIVATE_DATA
        MVSUI _dispUI;
        #endregion

        public MvdFlyResultDispUI(MVSUI host)
        {
            _dispUI = host;
            _dispUI.HandleDestroyed += (s, e) => Dispose();     // 自我清除
        }

        /// <summary>
        /// 自動釋放
        /// </summary>
        void Dispose()
        {
            disposeMvdTools();
        }

        //Control IvFlyCamViewUI.Window => _dispUI;

        public void Update(FlyMetaData flyMetaData)
        {
            if (_dispUI.InvokeRequired)
            {
                // 處理 多線程 的問題
                _dispUI.Invoke((Action<FlyMetaData>)Update);
            }
            else
            {
                try
                {
                    var flyID = flyMetaData.flyID;
                    int flyIndex = flyID.flyIndex;

                    var mvdShapes = (flyMetaData.xBlobs != null)?
                        createMvdDrawItemsWithBlobs(flyMetaData):
                        createMvdDrawItemsWithCrossLines(flyMetaData);

                    using (var mvdImage = GaMvdConvertor.BitmapToCMvdImage(flyMetaData.bmpFly))
                    {
                        updateDisplayUI(_dispUI, mvdImage, mvdShapes);
                    }
                }
                catch (Exception ex)
                {
                    _ERROR(ex, "MvdFlyResultDispUI.Update");
                }
            }
        }

        #region MVD_DISPLAY_FUNCTIONS
        CMvdShape[] createMvdDrawItemsWithCrossLines(FlyMetaData flyMetaData)
        {
            var mvdShapes = new List<CMvdShape>();

            try
            {
                var xResultBox2D = flyMetaData?.xResultBox2D;
                if (xResultBox2D != null)
                {
                    MVD_COLOR color;

                    var flyID = flyMetaData.flyID;
                    var aoiResult = flyMetaData.flyAoiResult;
                    var resultCode = aoiResult.Code;

                    var roiRect = flyMetaData.roiRect;
                    var centerRun = flyMetaData.xCentroid;
                    var templateRect = flyMetaData.xTemplateRect;
                    var imgSize = flyMetaData.bmpFly.Size;

                    string text = formatText(flyID.ShowID, aoiResult.OffsetX, aoiResult.OffsetY, aoiResult.OffsetAngle);

                    CMvdRectangleF mvdRect;

                    if (resultCode == PlcFlyResultCode.OK)
                    {
                        // OK (Green)
                        color = new MVD_COLOR(0, 255, 0);

                        //-----------------------------------------------------------------------
                        // 使用笨笨的 MVD 找出 Rotated Rectangle (Box2D)
                        // 這個很容易用 OpenCvSharp 達成
                        //-----------------------------------------------------------------------
                        //var mvdRectBase = GaMvdConvertor.ToCMvdRectangleF(ref templateRect);
                        //mvdRect = PositionFixRun(mvdRectBase,
                        //                         templateRect,
                        //                         new Rectangle(Point.Empty, imgSize),
                        //                         flyMetaData.xResult.Value) as CMvdRectangleF;
                        //mvdRect.CenterX += roiRect.X;
                        //mvdRect.CenterY += roiRect.Y;
                        //mvdRect.BorderColor = color;

                        var angle = (float)(xResultBox2D.Theta * 180 / Math.PI);
                        var cx = xResultBox2D.Center.X;
                        var cy = xResultBox2D.Center.Y;
                        var cw = xResultBox2D.MinAreaRectSize.Width;
                        var ch = xResultBox2D.MinAreaRectSize.Height;
                        cx += roiRect.X;
                        cy += roiRect.Y;

                        mvdRect = new CMvdRectangleF(cx, cy, cw, ch)
                        {
                            Angle = angle,
                            BorderColor = color,
                        };
                        mvdShapes.Add(mvdRect);
                    }
                    else
                    {
                        // NG (RED)
                        color = new MVD_COLOR(255, 0, 0);
                        mvdRect = new CMvdRectangleF(centerRun.X, centerRun.Y, templateRect.Width, roiRect.Height)
                        {
                            BorderColor = color
                        };
                        mvdShapes.Add(mvdRect);
                    }

                    ////var RectangleShape
                    ////    = new CMvdRectangleF(centerRun.X, centerRun.Y, _rectF.Width, _rectF.Height);
                    //if (iFlyResult[flyIndex] == 1)
                    //    mvdRect.BorderColor = new MVD_COLOR(0, 255, 0);
                    //else
                    //    mvdRect.BorderColor = new MVD_COLOR(255, 0, 0);
                    //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
                    //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                    //cMvdTextF.FontWidth = 20;

                    //添加十字线
                    if (true)
                    {
                        var W = imgSize.Width;
                        var H = imgSize.Height;
                        CMvdLineSegmentF v1 = new CMvdLineSegmentF(
                                                    new MVD_POINT_F(0, H / 2f),
                                                    new MVD_POINT_F(W, H / 2f));
                        v1.BorderColor = new MVD_COLOR(255, 215, 0);
                        v1.BorderWidth = 1;
                        mvdShapes.Add(v1);

                        CMvdLineSegmentF h1 = new CMvdLineSegmentF(
                                                    new MVD_POINT_F(W / 2f, 0),
                                                    new MVD_POINT_F(W / 2f, H));
                        h1.BorderColor = new MVD_COLOR(255, 215, 0);
                        h1.BorderWidth = 1;
                        mvdShapes.Add(h1);
                    }

                    CMvdTextF mvdText = new CMvdTextF(mvdRect.CenterX, mvdRect.CenterY, text);
                    mvdText.BorderColor = color; // new MVD_COLOR(0, 255, 0);
                    mvdText.FontWidth = 16;
                    mvdShapes.Add(mvdText);
                }
                return mvdShapes.ToArray();
            }
            catch(Exception ex)
            {
                _ERROR(ex, "createMvdDrawItemsWithCrossLines");
                return mvdShapes.ToArray();
            }
        }
        CMvdShape[] createMvdDrawItemsWithBlobs(FlyMetaData flyMetaData)
        {
            var mvdShapes = new List<CMvdShape>();
            try
            {
                MVD_COLOR color;

                var flyID = flyMetaData.flyID;
                var aoiResult = flyMetaData.flyAoiResult;
                var resultCode = aoiResult.Code;
                var roiRect = flyMetaData.roiRect;
                var blobs = flyMetaData.xBlobs;
                if (blobs != null && blobs.Length >= 2)
                {
                    string text = formatText(flyID.ShowID, aoiResult.OffsetX, aoiResult.OffsetY, aoiResult.OffsetAngle);

                    #region MVD_RECTANGLES
                    if (resultCode == PlcFlyResultCode.OK)
                    {
                        color = new MVD_COLOR(0, 255, 0);
                        foreach (QvBox2D blob in blobs)
                        {
                            //var mvdRect = new CMvdRectangleF(
                            //        blob.RectInfo.CenterX + roiRect.X,
                            //        blob.RectInfo.CenterY + roiRect.Y,
                            //        blob.RectInfo.Width,
                            //        blob.RectInfo.Height)
                            //{
                            //    BorderColor = color
                            //};
                            var cx = (float)blob.Center.X + roiRect.X;
                            var cy = (float)blob.Center.Y + roiRect.Y;
                            var size = blob.MinAreaRectSize;
                            var angle = (float)(blob.Theta * 180.0 / Math.PI);
                            var mvdRect = new CMvdRectangleF(cx, cy, size.Width, size.Height)
                            {
                                Angle = angle,
                                BorderColor = color
                            };
                            mvdShapes.Add(mvdRect);
                        }
                    }
                    else
                    {
                        color = new MVD_COLOR(255, 0, 0);
                        //RectangleShape1 = new CMvdRectangleF(roiRect.X, roiRect.Y, roiRect.Width, roiRect.Height);
                        //RectangleShape2 = new CMvdRectangleF(roiRect.X, roiRect.Y, roiRect.Width, roiRect.Height);
                        //RectangleShape1.BorderColor = new MVD_COLOR(255, 0, 0);
                        //RectangleShape2.BorderColor = new MVD_COLOR(255, 0, 0);
                        var mvdRect = GaMvdConvertor.ToCMvdRectangleF(ref roiRect);
                        mvdRect.BorderColor = color;
                        mvdShapes.Add(mvdRect);
                    }
                    #endregion

                    #region MVD_TEXT
                    //CMvdTextF cMvdTextF = new CMvdTextF(100, 100, $"耗时:{ms.ToString("0.00")} ms");
                    //cMvdTextF.BorderColor = new MVD_COLOR(0, 255, 0);
                    //cMvdTextF.FontWidth = 20;
                    var centerPt = JetEazy.Qcvt.Center(ref roiRect);
                    var mvdText = new CMvdTextF(centerPt.X, centerPt.Y, text);
                    //mvdText.BorderColor = new MVD_COLOR(0, 255, 0);
                    mvdText.BorderColor = color;
                    mvdText.FontWidth = 15;
                    mvdShapes.Add(mvdText);
                    #endregion
                }
                return mvdShapes.ToArray();
            }
            catch (Exception ex)
            {
                _ERROR(ex, "createMvdDrawItemsWithBlobs");
                return mvdShapes.ToArray();
            }
        }
        void updateDisplayUI(MVSUI dispUI, CMvdImage mvdImage, params CMvdShape[] shapes)
        {
            var render = dispUI.mvdRenderActivex1;
            render.LoadImageFromObject(mvdImage);

            foreach (var shape in shapes)
                if (shape != null)
                    render.AddShape(shape);

            dispUI.AddCross();
            render.Display();
        }
        string formatText(int flyShowIndex, float offsetX, float offsetY, float offsetAngle)
        {
            string text = $"[{flyShowIndex}]" +
                        $" x:{offsetX:0.000}," +
                        $" y:{offsetY:0.000}," +
                        $" a:{offsetAngle:0.000}";
            return text;
        }
        #endregion

        #region MVD_TOOL
        CPositionFixTool cPositionFixToolObj = null;
        /// <summary>
        /// 计算修正后的位置框
        /// </summary>
        /// <param name="eMVDInput">输入转换的形状</param>
        /// <param name="templateRectF">模板尺寸</param>
        /// <param name="runRect">输入图片尺寸</param>
        /// <param name="templateRunResult">定位的结果</param>
        /// <returns>返回位置的形状</returns>
        CMvdShape PositionFixRun(CMvdShape eMVDInput, RectangleF templateRectF, Rectangle runRect, xFindResult templateRunResult)
        {
            // CreateInstance
            if (cPositionFixToolObj == null)
                cPositionFixToolObj = new CPositionFixTool();

            // Set basic parameter

            cPositionFixToolObj.BasicParam.BasePoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRectF.X + templateRectF.Width / 2, templateRectF.Y + templateRectF.Height / 2), 0);

            cPositionFixToolObj.BasicParam.RunningPoint
                = new VisionDesigner.PositionFix.MVD_FIDUCIAL_POINT_F(
                    new MVD_POINT_F(templateRunResult.fCenterX, templateRunResult.fCenterY), templateRunResult.fAngle);

            cPositionFixToolObj.BasicParam.RunImageSize = new MVD_SIZE_I(runRect.Width, runRect.Height);

            cPositionFixToolObj.BasicParam.FixMode = MVD_POSFIX_MODE.MVD_POSFIX_MODE_HVA;

            cPositionFixToolObj.BasicParam.InitialShape = eMVDInput;

            // Running

            cPositionFixToolObj.Run();

            // Get the result
            return cPositionFixToolObj.Result.CorrectedShape;
        }
        void disposeMvdTools()
        {
            cPositionFixToolObj?.Dispose();
            cPositionFixToolObj = null;
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
