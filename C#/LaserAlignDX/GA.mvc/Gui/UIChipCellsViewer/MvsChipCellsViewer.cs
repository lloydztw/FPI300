#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-25 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace;
using LaserAlignDX.UISpace.UIMVC;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Traveller106;
using VisionDesigner;

using CELL = LaserAlignDX.OPSpace.RegionCellX3Class;
using InspectParams = LaserAlignDX.OPSpace.RecipeSpace.InspectX3ParaClass;
using RECIPE = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;


namespace LaserAlignDX.UISpace.ChipCellsViewer
{
    /// <summary>
    /// 對 MVSUI 進行包裝, 
    /// 把原來 gaara 利用 MVS 套件顯示 圖像控件,
    /// 的一大堆冗長的代碼, 重新整理至此.
    /// </summary>
    public partial class MvsChipCellsViewer : IvChipCellsViewer
    {
        #region GLOBAL_MESS
        RECIPE _xRecipe
        {
            get => RECIPE.Instance;
        }
        InspectParams _inspectParams
        {
            get => InspectParams.Instance;
        }
        #endregion

        #region PRIVATE_DATA
        ScanInspectMode _mode;
        #endregion

        #region PRIVATE_DATA_LINKS
        MVSUI _mvsUI;
        #endregion

        public MvsChipCellsViewer(MVSUI host)
        {
            _mvsUI = host;
        }

        Control IvChipCellsViewer.Window => _mvsUI;
        void IvChipCellsViewer.Reset()
        {

        }
        void IvChipCellsViewer.UpdateImageSrc(object fullfovImage, string srcName = null)
        {
            if (!IsHandleCreated)
                return;

            if (fullfovImage is GaBigImageHolder imgHolder)
            {
                CMvdImage img = imgHolder.PeekMvdImage();
                updateMvd_LineScanImage(img);
            }
            else if (fullfovImage is CMvdImage mvdImage)
            {
                updateMvd_LineScanImage(mvdImage);
            }
            else
            {
            }
        }
        void IvChipCellsViewer.UpdateCells(IEnumerable<CELL> cells, int mode)
        {
            if (!IsHandleCreated)
                return;

            _mode = (ScanInspectMode)mode;
            updateMvd_AoiResultData(cells);
        }
        bool IsHandleCreated
        {
            get
            {
                return _mvsUI != null && _mvsUI.IsHandleCreated;
            }
        }

        #region UPDATE_MVD_FUNCTIONS
        void initMvd_Render()
        {
            ////删除矩形菜单项，右键菜单中对应项会被删除
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdAddShape),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdFile),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdZoom),
            //  System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdRotate),
            //   System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdEraser),
            //   System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            //mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdShapeMenuPaste),
            //    System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);

            ////CMvdImage cMvdImage = new CMvdImage();
            ////cMvdImage.InitImage(1000, 1000, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            ////mvdRenderActivex1.LoadImageFromObject(cMvdImage);
            ////SizeChanged += MVSUI_SizeChanged;
        }
        /// <summary>
        /// Caller 必須負責 mvdImage 生命週期
        /// </summary>
        void updateMvd_LineScanImage(CMvdImage mvdImage)
        {
            _mvsUI.mvdRenderActivex1.LoadImageFromObject(mvdImage);
            _mvsUI.mvdRenderActivex1.ClearShapes();
            _mvsUI.mvdRenderActivex1.Display();
        }
        void updateMvd_AoiResultData(IEnumerable<CELL> xRegionCells)
        {
            // 清除 MVD canvas
            _mvsUI.mvdRenderActivex1.ClearShapes();

            // 報表 & LOG
            //generate_report_and_log();

            // 為每一個 Cell 更新 MVD 顯示元件
            foreach (CELL cell in xRegionCells)
            {
                updateMvd_OneCellData(cell);
            }

            // 显示格点之外的料件
            updateMvd_OutGrid_Blocs();

            // MVD render
            _mvsUI.mvdRenderActivex1.Display();
        }
        void updateMvd_OneCellData(CELL cell)
        {
            switch (_mode)
            {
                case ScanInspectMode.NOTRAY:
                    updateMvd_OneCellData_NoTray(cell);
                    break;
                case ScanInspectMode.MEASUREAOI:
                case ScanInspectMode.QRCODE:
                default:
                    updateMvd_OneCellData_Location(cell);
                    break;
            }
        }
        void updateMvd_OneCellData_NoTray(CELL cell)
        {
            //填写数据 疑似有料
            //try
            {
                string strNoTray = cell.GetNoTrayDesc();
                if (!string.IsNullOrEmpty(strNoTray))
                {
                    CMvdRectangleF mvdDrawResultRectF = cell.DrawResultRectF();
                    CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(mvdDrawResultRectF.CenterX,
                                                                  mvdDrawResultRectF.CenterY,
                                                                  $"{strNoTray}");

                    cMvdTextFShowNoTray.BorderColor = new MVD_COLOR(255, 0, 0);
                    cMvdTextFShowNoTray.FontWidth = 11;
                    //if (INI.Instance.IsResultShowChar)
                    _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowNoTray);
                    _mvsUI.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));
                }
                else
                {
                    var mvdRect = cell.DrawBaseRectFFixSize();
                    _mvsUI.mvdRenderActivex1.AddShape(mvdRect);
                }
            }
            //catch (Exception ex)
            //{
            //    string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
            //    MessageBox.Show(errMsg);
            //    return;
            //}
        }
        void updateMvd_OneCellData_Location(CELL cell)
        {
            RectangleF cellRect = cell.viewRectF;

            cellRect.Inflate(_xRecipe.xExtendx, _xRecipe.xExtendy);

            //BoundRect(ref _rectF, new Size((int)ProcessRunClass.Instance.cMvdInput.Width,
            //                               (int)ProcessRunClass.Instance.cMvdInput.Height));

            //try
            {
                if (_inspectParams.bOpenLineMeasure)
                {
                    // 繪製 找到的邊線
                    updateMvd_OneCellData_LinesOutSide(cell, cellRect.Location);

                    // 繪製 邊線手拉框
                    updateMvd_OneCellData_LinesOutsideBoxes(cell);

                    if (_inspectParams.bCheckMeasureOffset)
                    {
                        // 繪製 cMvdLineSegmentFsInSide
                        updateMvd_OneCellData_LinesInSide(cell, cellRect.Location);
                    }
                }

                // 文字
                updateMvd_OneCellData_Text(cell);

                //二维码
                updateMvd_OneCellData_Barcode(cell);
            }
            //catch (Exception ex)
            //{
            //    string errMsg = $"顯示結果異常: {ex.Message}\n\r\n\r@{ex.StackTrace}";
            //    MessageBox.Show(errMsg);
            //    return;
            //}
        }
        void updateMvd_OneCellData_LinesOutsideBoxes(CELL cell)
        {
            if (_inspectParams.bOpenLineMeasure)
            {
                // 繪製 邊線手拉框
                foreach (CMvdShape mvdShape in cell.cMvdShapesForFindLineRegion)
                {
                    if (mvdShape != null)
                    {
                        mvdShape.BorderColor = new MVD_COLOR(38, 127, 0);
                        _mvsUI.mvdRenderActivex1.AddShape(mvdShape);
                    }
                }
            }
        }
        void updateMvd_OneCellData_LinesOutSide(CELL cell, PointF offset)
        {
            if (_inspectParams.bOpenLineMeasure)
            {
                // 繪製 找到的邊線
                foreach (CMvdLineSegmentF mLine in cell.cMvdLineSegmentFsOut)
                {
                    if (mLine != null)
                    {
                        var p1 = mLine.StartPoint;
                        var p2 = mLine.EndPoint;
                        p1.fX += offset.X;
                        p1.fY += offset.Y;
                        p2.fX += offset.X;
                        p2.fY += offset.Y;
                        //MVD_POINT_F s0 = new MVD_POINT_F(mLine.StartPoint.fX + offset.X,
                        //    mLine.StartPoint.fY + offset.Y);
                        //MVD_POINT_F s1 = new MVD_POINT_F(mLine.EndPoint.fX + offset.X,
                        //    mLine.EndPoint.fY + offset.Y);
                        //CMvdLineSegmentF newLine = new CMvdLineSegmentF(s0, s1);
                        CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                        newLine.BorderColor = new MVD_COLOR(255, 0, 255);
                        _mvsUI.mvdRenderActivex1.AddShape(newLine);
                    }
                }
            }
        }
        void updateMvd_OneCellData_LinesInSide(CELL cell, PointF offset)
        {
            if (_inspectParams.bOpenLineMeasure && _inspectParams.bCheckMeasureOffset)
            {
                foreach (CMvdLineSegmentF mLine in cell.cMvdLineSegmentFsInSide)
                {
                    if (mLine != null)
                    {
                        var p1 = mLine.StartPoint;
                        var p2 = mLine.EndPoint;
                        p1.fX += offset.X;
                        p1.fY += offset.Y;
                        p2.fX += offset.X;
                        p2.fY += offset.Y;
                        //MVD_POINT_F s0 = new MVD_POINT_F(
                        //      mLine.StartPoint.fX + offset.X,
                        //      mLine.StartPoint.fY + offset.Y);
                        //MVD_POINT_F s1 = new MVD_POINT_F(
                        //      mLine.EndPoint.fX + offset.X,
                        //      mLine.EndPoint.fY + offset.Y);
                        CMvdLineSegmentF newLine = new CMvdLineSegmentF(p1, p2);
                        newLine.BorderColor = new MVD_COLOR(112, 48, 160);
                        _mvsUI.mvdRenderActivex1.AddShape(newLine);
                    }
                }
            }
        }
        void updateMvd_OneCellData_Text(CELL cell)
        {
            try
            {
                // 注意: MVD 顯示中文, 在 繁簡不同windows下, 會出現亂碼 !!!
                var formatter = new MainDispTextFormatter();
                string cellInfoStr = formatter.Format(cell);

                var mvdDrawResultRectF = cell.DrawResultRectF();

                //显示结果的xy angle
                CMvdTextF cMvdTextFShowMain = new CMvdTextF(
                                                mvdDrawResultRectF.CenterX,
                                                mvdDrawResultRectF.CenterY,
                                                cellInfoStr);

                cMvdTextFShowMain.BorderColor = new MVD_COLOR(0, 255, 0);// cell.DrawResultRectF().BorderColor;// new MVD_COLOR(0, 255, 0);
                cMvdTextFShowMain.FontWidth = 11;
                cMvdTextFShowMain.FillColor = new MVD_COLOR(0, 0, 0, 50);

                if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    //引导数据
                    if (INI.Instance.IsResultShowChar)
                        _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowMain);
                    //定位框
                    _mvsUI.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                    //DSMain.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(true));
                }
                else
                {
                    //引导数据
                    if (cell.inspectReason != InspectReason.INS_ALIGNERR)
                    {
                        if (INI.Instance.IsResultShowChar)
                        {
                            cMvdTextFShowMain = new CMvdTextF(
                                mvdDrawResultRectF.CenterX,
                                mvdDrawResultRectF.CenterY,
                                $"{cellInfoStr}{Environment.NewLine}{GaUtil.GetEnumDescription(cell.inspectReason)}");

                            cMvdTextFShowMain.BorderColor = new MVD_COLOR(255, 0, 0);
                            _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowMain);

                            //定位框
                            _mvsUI.mvdRenderActivex1.AddShape(mvdDrawResultRectF);
                        }
                    }
                    else
                    {
                        _mvsUI.mvdRenderActivex1.AddShape(cell.DrawBaseRectFFixSize(false));
                    }
                }
            }
            catch (Exception ex)
            {
                LtDebug.LOG.Error(ex, "updateMvd_OneCellData_Text 異常!");
            }
        }
        void updateMvd_OneCellData_Barcode(CELL cell)
        {
            //二维码
            if (cell.DrawBarcodePosition != null)
            {
                _mvsUI.mvdRenderActivex1.AddShape(cell.DrawBarcodePosition);
                CMvdTextF _CodeText
                    = new CMvdTextF(cell.DrawBarcodePosition.GetVertex(2).fX,
                                                  cell.DrawBarcodePosition.GetVertex(2).fY + 120,
                                                  cell.RunCodeInfo.Content);
                _CodeText.BorderColor = new MVD_COLOR(0, 255, 0);
                //_CodeText.FontWidth = 11;
                _CodeText.FillColor = new MVD_COLOR(0, 0, 0);
                _mvsUI.mvdRenderActivex1.AddShape(_CodeText);

            }
        }
        void updateMvd_OutGrid_Blocs()
        {
            // 显示格点之外的料件
            switch (_mode)
            {
                case ScanInspectMode.NOTRAY:

                    foreach (var rect in _xRecipe.xOutBlocs)
                    {
                        PointF ptCenter = new PointF(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                        CMvdTextF cMvdTextFShowNoTray = new CMvdTextF(ptCenter.X,
                                                                       ptCenter.Y,
                                                                       $"疑似有料");

                        cMvdTextFShowNoTray.BorderColor = new MVD_COLOR(255, 0, 0);
                        cMvdTextFShowNoTray.FontWidth = 11;

                        _mvsUI.mvdRenderActivex1.AddShape(cMvdTextFShowNoTray);
                        CMvdRectangleF rectRect = new CMvdRectangleF(ptCenter.X, ptCenter.Y, 200, 200);
                        rectRect.BorderColor = new MVD_COLOR(255, 0, 0);
                        _mvsUI.mvdRenderActivex1.AddShape(rectRect);
                    }

                    break;
            }
        }
        #endregion
    }
}
