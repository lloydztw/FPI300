#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * Description: 格點 內插 與 外插 
 * 
 * REVISION:
 *      2024-10-02 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QxCollections;
using JetEazy.QxCollections2;
using JetEazy.QxCollections2.ChipMap;
using JetEazy.QxCollections2.ChipMap.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;


namespace JetEazy.Match
{
    using ColorConversionCodes = OpenCvSharp.ColorConversionCodes;
    using Cv2 = OpenCvSharp.Cv2;
    using CvPoint = OpenCvSharp.Point;
    using CvRect = OpenCvSharp.Rect;
    using CvSize = OpenCvSharp.Size;
    using IplImage = OpenCvSharp.Mat;
    using IxGridMap = EzBlocsGrid;
    using MatType = OpenCvSharp.MatType;
    using TemplateMatchModes = OpenCvSharp.TemplateMatchModes;

    /// <summary>
    /// 格點內插與外插 (Interpolation and Expolation)
    /// </summary>
    internal class EzBlocsGridInterpo
    {
        public delegate float FUNC_IMG_MATCH_DIFF(IxGridMap map, QxRowColArg cur, QxRowColArg adj, IplImage imgOrg);
        public event DoWorkEventHandler OnInterpolating;

        #region PRIVATE_DATA
        private FUNC_IMG_MATCH_DIFF m_extMatchFunc = null;
        System.Drawing.Size m_sizeGoldenPitch;
        System.Drawing.Size m_sizeChipRef;
        private GridVector m_gridVector;
        #endregion
        
        public EzBlocsGridInterpo(QVector pitch, SizeF goldenSize)
        {
            m_sizeGoldenPitch = new Size((int)pitch.X, (int)pitch.Y);
            m_sizeChipRef = Size.Round(goldenSize);
        }
        public void RunInterpolation(IxGridMap dstGridMap, IplImage imgOrg, FUNC_IMG_MATCH_DIFF extMatchFunc, bool parallel = false)
        {
            var imgBackup = m_imgTrace;
            m_imgTrace = imgOrg;

            _clearNullChips(dstGridMap);

            // Note: "parallel" 會有小誤差
            if (parallel)
                interpolate_parallel(dstGridMap, 0, extMatchFunc, true);
            else
                interpolate(dstGridMap, 0, extMatchFunc, true);

            m_imgTrace = imgBackup;
        }
        public void RunExpolation(IxGridMap dstGridMap, IplImage imgOrg, FUNC_IMG_MATCH_DIFF extMatchFunc, int openEndSteps, bool resetNullChips = false)
        {
            var imgBackup = m_imgTrace;
            m_imgTrace = imgOrg;

            if (resetNullChips)
                _clearNullChips(dstGridMap);

            expolate(dstGridMap, -5000, extMatchFunc, true);

            m_imgTrace = imgBackup;
        }

        protected void interpolate(IxGridMap map, int iOpenEndSteps, bool bRescanMap, bool usingDefaultImgMatch = false)
        {
            if (usingDefaultImgMatch)
                interpolate(map, iOpenEndSteps, _predictByImgMatch, bRescanMap);
            else
                interpolate(map, iOpenEndSteps, null, bRescanMap);
        }
        protected void interpolate(IxGridMap map, int iOpenEndSteps, FUNC_IMG_MATCH_DIFF extMatchFunc, bool bRescanMap = true)
        {
            _TRACE_INTERPO_RESET();
            m_extMatchFunc = extMatchFunc;

            int iRow1 = 0;

            #region FIND_MOST_ELEMENTS_ROW
            int max = int.MinValue;
            for (int iRow = map.RowMin; iRow < map.RowMax; iRow++)
            {
                var row = map.GetRow(iRow);
                if (row != null)
                {
                    if (row.ElementsCount > max)
                    {
                        iRow1 = iRow;
                        max = row.ElementsCount;
                    }
                }
            }
            #endregion

            for (int iRow = iRow1; iRow >= map.RowMin; iRow--)
            {
                _interpolateInRow(map, iRow, iOpenEndSteps);
            }

            for (int iRow = iRow1 + 1; iRow < map.RowMax; iRow++)
            {
                _interpolateInRow(map, iRow, iOpenEndSteps);
            }

            //> int iCol1 = (map.ColMin + map.ColMax) / 2;
            int iCol1 = map.ColMin;
            for (int iCol = iCol1; iCol < map.ColMax; iCol++)
            {
                _interpolateInCol(map, iCol, iOpenEndSteps);
            }
            for (int iCol = iCol1 - 1; iCol >= map.ColMin; iCol--)
            {
                _interpolateInCol(map, iCol, iOpenEndSteps);
            }

            if (bRescanMap)
                map.RescanCounts();

            m_extMatchFunc = null;
            _TRACE_INTERPO_REPORT();
        }
        protected void interpolate_parallel(IxGridMap map, int iOpenEndSteps, FUNC_IMG_MATCH_DIFF extMatchFunc, bool bRescanMap = true)
        {
            _TRACE_INTERPO_RESET();
            m_extMatchFunc = extMatchFunc;

            int iRow1 = 0;

            #region FIND_MOST_ELEMENTS_ROW
            int max = int.MinValue;
            for (int iRow = map.RowMin; iRow < map.RowMax; iRow++)
            {
                var row = map.GetRow(iRow);
                if (row != null)
                {
                    if (row.ElementsCount > max)
                    {
                        iRow1 = iRow;
                        max = row.ElementsCount;
                    }
                }
            }
            #endregion

            int rowMin = map.RowMin;
            int rowMax = map.RowMax;
            int rowU = iRow1;
            int rowD = iRow1 + 1;
            int countU = rowMin - rowU + 1;
            int countD = rowMax - rowD;
            int count = Math.Max(countU, countD);

            var opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = 4;

            Parallel.For(0, count, opt, step =>
            {
                // Upwards
                int row = rowU - step;
                if (row >= rowMin)
                    _interpolateInRow(map, row, iOpenEndSteps);

                // Downwards
                row = rowD + step;
                if (row < rowMax)
                    _interpolateInRow(map, row, iOpenEndSteps);
            });

            //> int iCol1 = (map.ColMin + map.ColMax) / 2;
            int iCol1 = map.ColMin;
            int colMin = map.ColMin;
            int colMax = map.ColMax;
            int colL = iCol1;
            int colR = iCol1 + 1;
            int countL = colMin - colL + 1;
            int countR = colMax - colR;
            count = Math.Max(countU, countD);

            Parallel.For(0, count, opt, step =>
            {
                // Leftwards
                int col = colL - step;
                if (col >= colMin)
                    _interpolateInCol(map, col, iOpenEndSteps);

                // Rightwards
                col = colR + step;
                if (col < colMax)
                    _interpolateInCol(map, col, iOpenEndSteps);
            });


            if (bRescanMap)
                map.RescanCounts();

            m_extMatchFunc = null;
            _TRACE_INTERPO_REPORT();
        }

        #region INTERPOLATION_HELPER_FUNCTIONS
        void _interpolateInRow(IxGridMap map, int curRowID, int iOpenEndSteps)
        {
            var row = map.GetRow(curRowID);
            if (row == null || row.ElementsCount == 0)
                return;

            int iCol1 = map.ColMin;
            int iCol2 = map.ColMax;
            row.SeekToAvailableElement(ref iCol1);
            row.ReverseSeekToAvailableElement(ref iCol2);

            if (iOpenEndSteps <= 0)
            {
                for (int iCol = iCol1 + 1; iCol < iCol2; iCol++)
                {
                    var chip = map.Get(curRowID, iCol);
                    if (chip == null)
                    {
                        _interpolateRC(map, curRowID, iCol);
                    }
                }
            }
            else
            {
                for (int k = 0, iCol = iCol1; k < iOpenEndSteps && iCol >= map.ColMin; k++, iCol--)
                {
                    var chip = map.Get(curRowID, iCol);
                    if (chip == null)
                    {
                        _interpolateRC(map, curRowID, iCol);
                    }
                }

                for (int k = 0, iCol = iCol2; k < iOpenEndSteps && iCol < map.ColMax; k++, iCol++)
                {
                    var chip = map.Get(curRowID, iCol);
                    if (chip == null)
                    {
                        _interpolateRC(map, curRowID, iCol);
                    }
                }
            }
        }
        void _interpolateInCol(IxGridMap map, int curColID, int iOpenEndSteps)
        {
            int iRow1 = map.RowMin;
            int iRow2 = map.RowMax;

            for (; iRow1 < map.RowMax; iRow1++)
            {
                if (map.Get(iRow1, curColID) != null)
                    break;
            }

            for (; iRow2 >= map.RowMin; iRow2--)
            {
                if (map.Get(iRow2, curColID) != null)
                    break;
            }

            if (iOpenEndSteps <= 0)
            {
                for (int iRow = iRow1 + 1; iRow < iRow2; iRow++)
                {
                    var chip = map.Get(iRow, curColID);
                    if (chip == null)
                    {
                        _interpolateRC(map, iRow, curColID);
                    }
                }
            }
            else
            {
                for (int k = 0, iRow = iRow1; k < iOpenEndSteps && iRow >= map.RowMin; k++, iRow--)
                {
                    var chip = map.Get(iRow, curColID);
                    if (chip == null)
                    {
                        _interpolateRC(map, iRow, curColID);
                    }
                }

                for (int k = 0, iRow = iRow2; k < iOpenEndSteps && iRow < map.RowMax; k++, iRow++)
                {
                    var chip = map.Get(iRow, curColID);
                    if (chip == null)
                    {
                        _interpolateRC(map, iRow, curColID);
                    }
                }
            }
        }
        void _interpolateRC(IxGridMap map, int curRowID, int curColID)
        {
            _TRACE_INTERPO_GEO_INCR();

            List<QxRowCol[]> adjRowColPairs = _findAdjecentRowColPairs(map, curRowID, curColID);

            QxRowCol theNearestRowCol = null;
            int minDiff = int.MaxValue;

            IxBlob curChip = null;
            var xyNodes = new List<System.Drawing.Point>();
            List<double> weightings = new List<double>();
            double totalWeighting = 0;
            int iCount = 0;

            foreach (QxRowCol[] pair in adjRowColPairs)
            {
                curChip = _calcInterpolatePoint(map, curRowID, curColID, pair[0], pair[1]);

                if (curChip != null)
                {
                    //x += chip.CenterX;
                    //y += chip.CenterY;

                    int d0R = curRowID - pair[0].Row;
                    int d0C = curColID - pair[0].Col;
                    int d1R = curRowID - pair[1].Row;
                    int d1C = curColID - pair[1].Col;
                    int d0SQ = d0R * d0R + d0C * d0C;
                    int d1SQ = d1R * d1R + d1C * d1C;

                    if (minDiff > d0SQ)
                    {
                        minDiff = d0SQ;
                        theNearestRowCol = pair[0];
                    }
                    if (minDiff > d1SQ)
                    {
                        minDiff = d1SQ;
                        theNearestRowCol = pair[1];
                    }

                    double dd = Math.Min(d0SQ, d1SQ);   // d0SQ < d1SQ ? d0SQ : d1SQ;
                    dd = Math.Sqrt(dd);
                    weightings.Add(dd);
                    totalWeighting += dd;
                    xyNodes.Add(new System.Drawing.Point(curChip.CenterX, curChip.CenterY));

                    iCount++;
                }
            }

            if (iCount > 0)
            {
                int aveX, aveY;

                #region GET_THE_WEIGHTING_AVE_XY
                {
                    double x = xyNodes[0].X;
                    double y = xyNodes[0].Y;
                    if (iCount > 1)
                    {
                        x = 0;
                        y = 0;
                        for (int i = 0; i < iCount; i++)
                        {
                            double w = (totalWeighting - weightings[i]) / totalWeighting;
                            if (w < 0) w = 0;
                            if (w > 1) w = 1;

                            x += w * xyNodes[i].X;
                            y += w * xyNodes[i].Y;
                        }
                    }
                    aveX = (int)Math.Round(x);
                    aveY = (int)Math.Round(y);
                }
                #endregion

                //--------------------------------------------------------------------------
                // Set a NULL chip at [curRowID, curColID] with interpolated (aveX, aveY)
                //--------------------------------------------------------------------------
                curChip.SetCenter(aveX, aveY);
                map.Set(curRowID, curColID, (EzBloc)curChip);

                //--------------------------------------------------------------------------
                // EXTRA PREDICTION & MATCH
                //--------------------------------------------------------------------------
                if (m_extMatchFunc != null)
                {
                    var adjChip = map.Get(theNearestRowCol.Row, theNearestRowCol.Col);
                    var curRC = new QxRowColArg(curRowID, curColID, curChip);
                    var adjRC = new QxRowColArg(theNearestRowCol, adjChip);
                    m_extMatchFunc(map, curRC, adjRC, m_imgTrace);
                }
            }
        }
        void _interpolateRC1(IxGridMap map, int curRowID, int curColID)
        {
            _TRACE_INTERPO_GEO_INCR();

            var adjRowColPairs = QxRowColScan.FindAdjecentRowColPairs(
                        map, new QxRowColArg(curRowID, curColID),
                        TAGS.IsValidChip);

            QxRowColArg theNearestRowCol = null;
            int minDiff = int.MaxValue;

            QxRowColArg cur = new QxRowColArg(curRowID, curColID);
            IxBlob curChip = null;
            List<System.Drawing.Point> xyNodes = new List<System.Drawing.Point>();
            List<double> weightings = new List<double>();
            double totalWeighting = 0;
            int iCount = 0;

            foreach (var pair in adjRowColPairs)
            {
                curChip = _calcInterpolatePoint(map, curRowID, curColID, pair[0], pair[1]);

                cur.Item = curChip;

                if (curChip != null)
                {
                    //x += chip.CenterX;
                    //y += chip.CenterY;

                    int d0R = curRowID - pair[0].Row;
                    int d0C = curColID - pair[0].Col;
                    int d1R = curRowID - pair[1].Row;
                    int d1C = curColID - pair[1].Col;
                    int d0SQ = d0R * d0R + d0C * d0C;
                    int d1SQ = d1R * d1R + d1C * d1C;

                    if (minDiff > d0SQ)
                    {
                        minDiff = d0SQ;
                        theNearestRowCol = pair[0];
                    }
                    if (minDiff > d1SQ)
                    {
                        minDiff = d1SQ;
                        theNearestRowCol = pair[1];
                    }

                    double dd = Math.Min(d0SQ, d1SQ);   // d0SQ < d1SQ ? d0SQ : d1SQ;
                    dd = Math.Sqrt(dd);
                    weightings.Add(dd);
                    totalWeighting += dd;
                    xyNodes.Add(new System.Drawing.Point(curChip.CenterX, curChip.CenterY));

                    iCount++;
                }
            }

            if (iCount > 0)
            {
                int aveX, aveY;

                #region GET_THE_WEIGHTING_AVE_XY
                {
                    double x = xyNodes[0].X;
                    double y = xyNodes[0].Y;
                    if (iCount > 1)
                    {
                        x = 0;
                        y = 0;
                        for (int i = 0; i < iCount; i++)
                        {
                            double w = (totalWeighting - weightings[i]) / totalWeighting;
                            if (w < 0) w = 0;
                            if (w > 1) w = 1;

                            x += w * xyNodes[i].X;
                            y += w * xyNodes[i].Y;
                        }
                    }
                    aveX = (int)Math.Round(x);
                    aveY = (int)Math.Round(y);
                }
                #endregion

                //--------------------------------------------------------------------------
                // Set a NULL chip at [curRowID, curColID] with interpolated (aveX, aveY)
                //--------------------------------------------------------------------------
                curChip.SetCenter(aveX, aveY);
                map.Set(curRowID, curColID, (EzBloc)curChip);

                //--------------------------------------------------------------------------
                // EXTRA PREDICTION & MATCH
                //--------------------------------------------------------------------------
                if (m_extMatchFunc != null)
                {
                    //var adjChip = map.Get(theNearestRowCol.Row, theNearestRowCol.Col);
                    //var curRC = new QxRowColArg(curRowID, curColID, curChip);
                    //var adjRC = new QxRowColArg(theNearestRowCol, adjChip);
                    //m_extMatchFunc(map, curRC, adjRC, m_imgTrace);
                    cur.Item = curChip;
                    m_extMatchFunc(map, cur, theNearestRowCol, m_imgTrace);
                }
            }
        }
        IxBlob _calcInterpolatePoint(IxGridMap map, int iRowID, int iColID, QxRowCol rc1, QxRowCol rc2)
        {
            var chip1 = map.Get(rc1.Row, rc1.Col);
            var chip2 = map.Get(rc2.Row, rc2.Col);

            QVector v = new QVector(2);
            v.x = chip2.CenterX - chip1.CenterX;
            v.y = chip2.CenterY - chip1.CenterY;

            if (rc1.Row == rc2.Row && rc2.Col != rc1.Col)
            {
                double factor = ((double)(iColID - rc1.Col)) / (rc2.Col - rc1.Col);
                v *= factor;
            }
            else if (rc1.Col == rc2.Col && rc2.Row != rc1.Row)
            {
                double factor = ((double)(iRowID - rc1.Row)) / (rc2.Row - rc1.Row);
                v *= factor;
            }
            else
            {
                /*
                double dr1 = iRowID - rc1.Row;
                double dc1 = iColID - rc1.Col;
                double dr = rc2.Row - rc1.Row;
                double dc = rc2.Col - rc1.Col;
                double factor = Math.Sqrt((dr1 * dr1 + dc1 * dc1) / (dr * dr + dc * dc));
                v *= factor;
                throw new Exception("Should not be used!");
                */
                return null;
            }

            int x = (int)Math.Round(chip1.CenterX + v.x);
            int y = (int)Math.Round(chip1.CenterY + v.y);

            // 注意: 會連同 pads 也被 clone!
            var chip = (IxBlob)chip1.Clone();
            chip.SetCenter(x, y);

            //> chip.Bin = (int)AoiBinCode.NULL_DIE;
            TAGS.SetMark(chip, TAGS.NULL);

            return chip;
        }
        List<QxRowCol[]> _findAdjecentRowColPairs(IxGridMap map, int iRowID, int iColID)
        {
            List<QxRowCol[]> pairs = new List<QxRowCol[]>();
            List<QxRowCol> cc = _findAdjecentCols(map, iRowID, iColID);
            List<QxRowCol> rr = _findAdjecentRows(map, iRowID, iColID);
            if (cc.Count >= 2) pairs.Add(cc.ToArray());
            if (rr.Count >= 2) pairs.Add(rr.ToArray());
            return pairs;
        }
        List<QxRowCol> _findAdjecentRows(IxGridMap map, int iRowID, int iColID)
        {
#if (false)
                        List<QxRowCol> results = new List<QxRowCol>();
                        int iPendulum = 0;

                        while (results.Count < 2)
                        {
                            int iOffset = (iPendulum / 2) + 1;
                            int iSign = (iPendulum % 2 == 0) ? -1 : 1;
                            int j = iRowID + (iOffset * iSign);

                            if (j >= map.RowMin && j < map.RowMax)
                            {
                                if (map.Get(j, iColID) != null)
                                {
                                    results.Add(new QxRowCol(j, iColID));
                                }
                            }

                            if (iSign < 0)
                            {
                                if (j < map.RowMin - 2)
                                    break;
                            }
                            else
                            {
                                if (j > map.RowMax + 1)
                                    break;
                            }

                            iPendulum++;
                        }

                        return results;
#else

            List<QxRowCol> results = new List<QxRowCol>();
            int j = iRowID - 1;
            int k = iRowID + 1;

            for (int i = 0; i < 2; i++)
            {
                for (; j >= map.RowMin; j--)
                {
                    var item = map.Get(j, iColID);
                    if (TAGS.IsValidChip(item))
                    {
                        results.Add(new QxRowCol(j, iColID));
                        j--;
                        break;
                    }
                }

                for (; k < map.RowMax; k++)
                {
                    var item = map.Get(k, iColID);
                    if (TAGS.IsValidChip(item))
                    {
                        results.Add(new QxRowCol(k, iColID));
                        k++;
                        break;
                    }
                }

                if (results.Count >= 2)
                    break;
            }

            return results;
#endif
        }
        List<QxRowCol> _findAdjecentCols(IxGridMap map, int iRowID, int iColID)
        {
#if (false)
                        List<QxRowCol> results = new List<QxRowCol>();
                        int iPendulum = 0;

                        while (results.Count < 2)
                        {
                            int iOffset = (iPendulum / 2) + 1;
                            int iSign = (iPendulum % 2 == 0) ? -1 : 1;
                            int j = iColID + (iOffset * iSign);

                            if (j >= map.ColMin && j < map.ColMax)
                            {
                                if (map.Get(iRowID, j) != null)
                                {
                                    results.Add(new QxRowCol(iRowID, j));
                                }
                            }

                            if (iSign < 0)
                            {
                                if (j < map.ColMin - 2)
                                    break;
                            }
                            else
                            {
                                if (j > map.ColMax + 1)
                                    break;
                            }

                            iPendulum++;
                        }

                        return results;
#endif

            List<QxRowCol> results = new List<QxRowCol>();
            int j = iColID - 1;
            int k = iColID + 1;

            for (int i = 0; i < 2; i++)
            {
                for (; j >= map.ColMin; j--)
                {
                    var item = map.Get(iRowID, j);
                    if (TAGS.IsValidChip(item))
                    {
                        results.Add(new QxRowCol(iRowID, j));
                        j--;
                        break;
                    }
                }

                for (; k < map.ColMax; k++)
                {
                    //if (map.Get(iRowID, k) != null)
                    var item = map.Get(iRowID, k);
                    if (TAGS.IsValidChip(item))
                    {
                        results.Add(new QxRowCol(iRowID, k));
                        k++;
                        break;
                    }
                }

                if (results.Count >= 2)
                    break;
            }

            return results;
        }
        #endregion

        protected void expolate(IxGridMap map, int openEndSteps, FUNC_IMG_MATCH_DIFF extMatchFunc, bool bRescanMap = true)
        {
            _TRACE_INTERPO_RESET();
            m_extMatchFunc = extMatchFunc;

            int iRow1 = (map.RowMin + map.RowMax) / 2;
            for (int rowID = iRow1; rowID >= map.RowMin; rowID--)
            {
                _expolateInRow(map, rowID, openEndSteps);
            }

            for (int rowID = iRow1 + 1; rowID < map.RowMax; rowID++)
            {
                _expolateInRow(map, rowID, openEndSteps);
            }

            int iCol1 = (map.ColMin + map.ColMax) / 2;
            for (int colID = iCol1; colID < map.ColMax; colID++)
            {
                _expolateInCol(map, colID, openEndSteps);
            }
            for (int colID = iCol1 - 1; colID >= map.ColMin; colID--)
            {
                _expolateInCol(map, colID, openEndSteps);
            }

            if (bRescanMap)
                map.RescanCounts();

            m_extMatchFunc = null;
            _TRACE_INTERPO_REPORT();
        }

        #region EXPOLATION_HELPER_FUNCTIONS
        void _expolateInRow(IxGridMap map, int curRowID, int openEndSteps = 1)
        {
            bool toRcBound = openEndSteps < 0;

            var row = map.GetRow(curRowID);
            if (row == null || row.ElementsCount == 0)
                return;

            int colB = map.ColMin;
            int colE = map.ColMax;
            row.SeekToAvailableElement(ref colB);
            row.ReverseSeekToAvailableElement(ref colE);
            
            int beginID = colB;

            //>>> for (int k = -1; k < openEndSteps; k++, colB--)
            for (int stop = toRcBound ? map.ColMin : colB - openEndSteps; 
                 colB >= stop; 
                 colB--)
            {
                var chip = map.Get(curRowID, colB);

                if (!TAGS.IsValidChip(chip))
                    _expolateRC(map, new QxRowColArg(curRowID, colB, chip));
            }

            //>>> for (int k = -1; k < openEndSteps; k++, colE++)
            for (int stop = toRcBound ? map.ColMax - 1 : colE + openEndSteps; 
                 colE <= stop; 
                 colE++)
            {
                if (colE != beginID)
                {
                    var chip = map.Get(curRowID, colE);

                    if (!TAGS.IsValidChip(chip))
                        _expolateRC(map, new QxRowColArg(curRowID, colE, chip));
                }
            }
        }
        void _expolateInCol(IxGridMap map, int curColID, int openEndSteps = 1)
        {
            bool toRcBound = openEndSteps < 0;

            int rowB = map.RowMin;
            int rowE = map.RowMax;

            for (; rowB < map.RowMax; rowB++)
            {
                if (map.Get(rowB, curColID) != null)
                    break;
            }

            for (; rowE >= map.RowMin; rowE--)
            {
                if (map.Get(rowE, curColID) != null)
                    break;
            }

            int beginID = rowB;

            //>>> for (int k = -1; k < openEndSteps; k++, rowB--)
            for (int stop = toRcBound ? map.RowMin : rowB - openEndSteps;
                 rowB >= stop;
                 rowB--)
            {
                var chip = map.Get(rowB, curColID);

                if (!TAGS.IsValidChip(chip))
                    _expolateRC(map, new QxRowColArg(rowB, curColID, chip));
            }

            //>>> for (int k = -1; k < openEndSteps; k++, rowE++)
            for (int stop = toRcBound ? map.RowMax : rowE + openEndSteps;
                 rowE >= stop;
                 rowE--)
            {
                if (rowE != beginID)
                {
                    var chip = map.Get(rowE, curColID);

                    if (!TAGS.IsValidChip(chip))
                        _expolateRC(map, new QxRowColArg(rowE, curColID, chip));
                }
            }
        }
        void _expolateRC(IxGridMap map, QxRowColArg cur)
        {
            _TRACE_INTERPO_GEO_INCR();

            QxRowColArg theNearestRowCol = null;
            int minDiff = int.MaxValue;

            var adjRowColPairs = QxRowColScan.FindAdjecentRowColPairs(map, cur, TAGS.IsValidChip);

            IxBlob curChip = null;
            List<System.Drawing.Point> xyNodes = new List<System.Drawing.Point>();
            List<double> weightings = new List<double>();
            double totalWeighting = 0;
            int iCount = 0;

            foreach (var pair in adjRowColPairs)
            {
                curChip = _calcExpolatePoint(map, cur, pair[0], pair[1]);

                if (curChip != null)
                {
                    int curRowID = cur.Row;
                    int curColID = cur.Col;

                    int d0R = curRowID - pair[0].Row;
                    int d0C = curColID - pair[0].Col;
                    int d1R = curRowID - pair[1].Row;
                    int d1C = curColID - pair[1].Col;
                    int d0SQ = d0R * d0R + d0C * d0C;
                    int d1SQ = d1R * d1R + d1C * d1C;

                    if (minDiff > d0SQ)
                    {
                        minDiff = d0SQ;
                        theNearestRowCol = pair[0];
                    }
                    if (minDiff > d1SQ)
                    {
                        minDiff = d1SQ;
                        theNearestRowCol = pair[1];
                    }

                    double dd = Math.Min(d0SQ, d1SQ);   // d0SQ < d1SQ ? d0SQ : d1SQ;
                    dd = Math.Sqrt(dd);
                    weightings.Add(dd);
                    totalWeighting += dd;
                    xyNodes.Add(new System.Drawing.Point(curChip.CenterX, curChip.CenterY));

                    iCount++;
                }
            }

            if (iCount > 0)
            {
                int aveX, aveY;

                #region GET_THE_WEIGHTING_AVE_XY
                {
                    double x = xyNodes[0].X;
                    double y = xyNodes[0].Y;
                    if (iCount > 1)
                    {
                        x = 0;
                        y = 0;
                        for (int i = 0; i < iCount; i++)
                        {
                            double w = (totalWeighting - weightings[i]) / totalWeighting;
                            if (w < 0) w = 0;
                            if (w > 1) w = 1;

                            x += w * xyNodes[i].X;
                            y += w * xyNodes[i].Y;
                        }
                    }
                    aveX = (int)Math.Round(x);
                    aveY = (int)Math.Round(y);
                }
                #endregion

                //--------------------------------------------------------------------------
                // Set a NULL chip at [curRowID, curColID] with interpolated (aveX, aveY)
                //--------------------------------------------------------------------------
                curChip.SetCenter(aveX, aveY);
                map.Set(cur.Row, cur.Col, (EzBloc)curChip);

                //--------------------------------------------------------------------------
                // EXTRA PREDICTION & MATCH
                //--------------------------------------------------------------------------
                if (m_extMatchFunc != null)
                {
                    //var adjChip = map.Get(theNearestRowCol.Row, theNearestRowCol.Col);
                    //var curRC = new QxRowColArg(curRowID, curColID, curChip);
                    //var adjRC = new QxRowColArg(theNearestRowCol, adjChip);
                    m_extMatchFunc(map, cur, theNearestRowCol, m_imgTrace);
                }
            }
        }
        IxBlob _calcExpolatePoint(IxGridMap map, QxRowColArg cur, QxRowColArg rc1, QxRowColArg rc2)
        {
            var oldChip = (IxBlob)cur.Item; // map.Get(cur.Row, cur.Col);

            var newChip = _calcInterpolatePoint(map, cur.Row, cur.Col, rc1, rc2);

            if (oldChip != null && newChip != null)
            {
                oldChip.SetCenter(newChip.CenterX, newChip.CenterY);
                TAGS.SetMark(oldChip, TAGS.NULL);
                cur.Item = oldChip;
                return oldChip;
            }

            cur.Item = newChip;
            return newChip;
        }
        #endregion

        #region DEFAULT_TEMPLATE_MATCH_AND_FILTERS
        void _clearNullChips(IxGridMap map)
        {
            for (int iRow = map.RowMin; iRow < map.RowMax; iRow++)
            {
                for (int iCol = map.ColMin; iCol < map.ColMax; iCol++)
                {
                    var chip = map.Get(iRow, iCol);
                    if (TAGS.IsNull(chip) || TAGS.IsHole(chip))
                    {
                        map.Set(iRow, iCol, null);
                    }
                }
            }
        }
        void _notifyInterpolating(IxGridMap map, QxRowColArg cur, QxRowColArg adj)
        {
            if (OnInterpolating != null)
            {
                var args = new QxRowColArg[] { cur, adj };
                var ev = new DoWorkEventArgs(args);
                OnInterpolating(map, ev);
            }
        }
        float _predictByImgMatch(IxGridMap map, QxRowColArg cur, QxRowColArg adj, IplImage imgOrg)
        {
            if (imgOrg == null)
                return float.MaxValue;

            _TRACE_INTERPO_PM_INCR();

            CvPoint bestPt;

            float imgDiff = _calcImgMatchDiff(map, cur, adj, imgOrg, out bestPt);

            if (imgDiff < 0.25f)
            {
                var curChip = (IxBlob)cur.Item;
                float geoDiff = _calcGeoLocDiff(
                        curChip.CenterX, curChip.CenterY,
                        bestPt.X, bestPt.Y,
                        true
                    );

                if (geoDiff < 0.25f)
                {
                    // TRACE
                    //> _TRACE(string.Format("[{0}] (imgDiff, geoDiff) = ({1:0.00}, {2:0.00}) OK", m_interpoTraceCountPM, imgDiff, geoDiff));

                    // Update Chip with TAGS.PRED
                    curChip.SetCenter(bestPt.X, bestPt.Y);
                    TAGS.SetMark(curChip, TAGS.PRED);
                    map.Set(cur.Row, cur.Col, (EzBloc)curChip);
                }
                else
                {
                    // TRACE
                    _TRACE(string.Format("[{0}] (imgDiff, geoDiff) = ({1:0.00}, {2:0.00}) NG", m_interpoTraceCountPM, imgDiff, geoDiff));

                    //DEBUG
                    //TAGS.SetMark(curChip, TAGS.PRED);
                    //map.Set(cur.Row, cur.Col, curChip);
                }
            }

            return imgDiff;
        }
        float _calcImgMatchDiff(IxGridMap map, QxRowColArg cur, QxRowColArg adjNode, IplImage imgOrg, out CvPoint bestLoc)
        {
            //------------------------------------------------------------------
            // adjNode 是節點 (Local Golden Sample)
            //------------------------------------------------------------------
            int goldenWidth = m_sizeGoldenPitch.Width;
            int goldenHeight = m_sizeGoldenPitch.Height;

            //------------------------------------------------------------------
            // cur 是預估點
            //------------------------------------------------------------------
            int curWidth = m_sizeGoldenPitch.Width * 2;
            int curHeight = m_sizeGoldenPitch.Height * 2;

            IxBlob adjChip = (IxBlob)adjNode.Item;
            IxBlob curChip = (IxBlob)cur.Item;

            CvRect boundary = new CvRect(0, 0, imgOrg.Width, imgOrg.Height);
            CvRect goldenRoi = _getRoi(adjChip, goldenWidth, goldenHeight);
            CvRect curRoi = _getRoi(curChip, curWidth, curHeight);

            bestLoc = new CvPoint(curChip.CenterX, curChip.CenterY);

            if (JetEazy.Qcvt.ClipBoundary(ref curRoi, ref boundary) ||
                JetEazy.Qcvt.ClipBoundary(ref goldenRoi, ref boundary))
                return float.MaxValue;

            var resultSize = new CvSize(
                    curRoi.Width - goldenRoi.Width + 1,
                    curRoi.Height - goldenRoi.Height + 1
            );

            using (var imgTemplate = new IplImage(goldenRoi.Size, MatType.CV_8UC1))
            using (var simgTemplate = new IplImage(goldenRoi.Size, MatType.CV_8UC1))
            using (var imgCur = new IplImage(curRoi.Size, MatType.CV_8UC1))
            using (var simgCur = new IplImage(curRoi.Size, MatType.CV_8UC1))
            using (var imgResult = new IplImage(resultSize, MatType.CV_32FC1))
            {
                //imgOrg.SetROI(goldenRoi);
                //imgOrg.CvtColor(imgTemplate, ColorConversion.RgbaToGray);
                //imgOrg.SetROI(curRoi);
                //imgOrg.CvtColor(imgCur, ColorConversion.RgbaToGray);
                //imgOrg.ResetROI();
                Cv2.CvtColor(imgOrg.SubMat(goldenRoi), imgTemplate, ColorConversionCodes.RGBA2GRAY);
                Cv2.CvtColor(imgOrg.SubMat(curRoi), imgCur, ColorConversionCodes.RGBA2GRAY);

                _cxApplySobelFilterXY(imgTemplate, simgTemplate, true);
                _cxApplySobelFilterXY(imgCur, simgCur, true);

                _DUMP(simgTemplate, "D:\\simgTempl.png");
                _DUMP(simgCur, "D:\\simgCur.png");

                Cv2.MatchTemplate(simgCur, simgTemplate, imgResult, TemplateMatchModes.SqDiffNormed);


                double min, max;
                CvPoint minPt, maxPt;
                Cv2.MinMaxLoc(imgResult, out min, out max, out minPt, out maxPt);

                //--------------------------------------------------------------------------
                // Translate Left-Top to the Center in "curRoi" Coordinate.
                //--------------------------------------------------------------------------
                minPt.X += (goldenRoi.Width >> 1);
                minPt.Y += (goldenRoi.Height >> 1);

                //> simgAdj.DrawMarker(minPt.X, minPt.Y, CvColor.White, MarkerStyle.Cross);
                //> _DUMP(simgAdj, "D:\\simgAdj.png");

                // Translate to Global Coordinate
                bestLoc = minPt + curRoi.Location;

                return (float)min;
            }
        }
        float _calcGeoLocDiff(int xNode, int yNode, int x1, int y1, bool normalized = false)
        {
#if(OPT_OLD || false)
            float dx = x1 - xNode;
            float dy = y1 - yNode;
            float diff;

            if (normalized)
            {
                float ddx = (float)dx / m_sizeGoldenPitch.Width;
                float ddy = (float)dy / m_sizeGoldenPitch.Height;
                diff = Math.Max(Math.Abs(ddx), Math.Abs(ddy));
            }
            else
            {
                diff = dx * dx + dy * dy;
            }

            return diff;
#else
            float diff;
            float dx = x1 - xNode;
            float dy = y1 - yNode;
            if (normalized)
            {
                var vector = GridVector.GetVectorUnder180d(dx, dy);
                var du = (vector * m_gridVector.U_Unit) / m_gridVector.U_Length;
                var dv = (vector * m_gridVector.V_Unit) / m_gridVector.V_Length;
                diff = (float)Math.Max(Math.Abs(du), Math.Abs(dv));
            }
            else
            {
                diff = dx * dx + dy * dy;
            }
            return diff;
#endif
        }
        void _cxApplySobelFilterXY(IplImage imgSrcU8, IplImage imgDstU8, bool usingAbs)
        {
            using (var imgSob = new IplImage(imgSrcU8.Size(), MatType.CV_16SC1))
            using (var imgTmp = new IplImage(imgSob.Size(), MatType.CV_8UC1))
            {
                if (usingAbs)
                {
                    // Sob X
                    //imgSrcU8.Sobel(imgSob, 1, 0, ApertureSize.Size3);
                    Cv2.Sobel(imgSrcU8, imgSob, imgSob.Type(), 1, 0, 3);
                    Cv2.ConvertScaleAbs(imgSob, imgTmp, 0.5);
                    // Sob Y
                    //imgSrcU8.Sobel(imgSob, 0, 1, ApertureSize.Size3);
                    Cv2.Sobel(imgSrcU8, imgSob, imgSob.Type(), 0, 1, 3);
                    Cv2.ConvertScaleAbs(imgSob, imgDstU8, 0.5);
                    // Combine (X,Y)
                    Cv2.Add(imgTmp, imgDstU8, imgDstU8);
                }
                else
                {
                    // Sob X
                    //imgSrcU8.Sobel(imgSob, 1, 0, ApertureSize.Size3);
                    Cv2.Sobel(imgSrcU8, imgSob, imgSob.Type(), 1, 0, 3);
                    Cv2.ScaleAdd(imgSob, -0.25, 128, imgTmp);
                    // Sob Y
                    //imgSrcU8.Sobel(imgSob, 0, 1, ApertureSize.Size3);
                    Cv2.Sobel(imgSrcU8, imgSob, imgSob.Type(), 0, 1, 3);
                    Cv2.ScaleAdd(imgSob, -0.25, 128, imgDstU8);
                    // Combine (X,Y)
                    Cv2.Add(imgTmp, imgDstU8, imgDstU8);
                }
            }
        }
        CvRect _getRoi(IxBlob chip, int w, int h)
        {
            return new CvRect(
                    chip.CenterX - (w >> 1),
                    chip.CenterY - (h >> 1),
                    w,
                    h
                );
        }
        #endregion

        #region TRACE_FUNCTIONS
        IplImage m_imgTrace = null;
        int m_interpoTraceCountGeo;
        int m_interpoTraceCountPM;
        void _TRACE_INTERPO_RESET()
        {
            m_interpoTraceCountPM = 0;
            m_interpoTraceCountGeo = 0;
        }
        void _TRACE_INTERPO_GEO_INCR()
        {
            m_interpoTraceCountGeo++;
        }
        void _TRACE_INTERPO_PM_INCR()
        {
            m_interpoTraceCountPM++;
        }
        void _TRACE_INTERPO_REPORT()
        {
            _TRACE(string.Format("Interpo Counts : Geo = {0}, Pm = {1}", m_interpoTraceCountGeo, m_interpoTraceCountPM));
        }
        private void _TRACE(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        private void _DUMP(params object[] args)
        {

        }
        #endregion
    }
}
