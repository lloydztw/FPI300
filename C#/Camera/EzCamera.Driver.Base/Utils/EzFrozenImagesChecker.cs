/****************************************************************************
 *                                                                          
 * Copyright (c) 2009 Jet Eazy Corp. All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	    2008/12/01 The class is created by LeTian Chang
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace JetEazy.Drivers.Camera
{
    public class EzFrozenImagesChecker : IDisposable
    {
        #region PRIVATE_DATA
        private const byte C_DARK = 0; //0x08;
        private const byte C_WHITE = 0xFF;
        private object m_sync = new object();
        private Rectangle m_rectCamRes;
        private List<Rectangle> m_monitorRects = new List<Rectangle>();
        private List<Bitmap> m_monitorBmps = new List<Bitmap>();
        private List<BitmapData> m_monitorBmpds = new List<BitmapData>();
        JetEazy.QxImageScan.QxBmpCrossScan.ScanAction<int> m_compareAction;
        #endregion

        public static bool SIM_FROZEN = false;

        public EzFrozenImagesChecker(Rectangle rectCamRes)
        {
            m_rectCamRes = rectCamRes;
            _initFrozenMonitor(rectCamRes, 2, 2, new Size(50, 50));
        }
        public void Dispose()
        {
            lock (m_sync)
            {
                try
                {
                    _disposeFrozenMonitor();
                }
                catch
                {
                }
            }
        }

        public int CheckFrozen(Bitmap bmpNewComing)
        {
            lock (m_sync)
            {
                if (bmpNewComing != null)
                {
                    if (bmpNewComing.Size != m_rectCamRes.Size)
                    {
                        return _checkFrozen(null);
                    }

                    BitmapData bmpd = bmpNewComing.LockBits(m_rectCamRes, ImageLockMode.ReadWrite, bmpNewComing.PixelFormat);
                    int diff = _checkFrozen(bmpd);
                    bmpNewComing.UnlockBits(bmpd);
                    return diff;
                }
                else
                {
                    return _checkFrozen(null);
                }
            }
        }
        public int CheckFrozen(BitmapData bmpdNewComing)
        {
            lock (m_sync)
            {
                return _checkFrozen(bmpdNewComing);
            }
        }

        #region PRIVATE_FUNCTIONS
        private void _initFrozenMonitor(Rectangle rectCamRes, int rows, int cols, Size monitorSize)
        {
            Rectangle rc;
            int w = rectCamRes.Width / (cols + 1);
            int h = rectCamRes.Height / (rows + 1);

            for (int row = 0; row < rows; row++)
            {
                int y = rectCamRes.Y + (row + 1) * h;
                for (int col = 0; col < cols; col++)
                {
                    int x = rectCamRes.X + (col + 1) * w;
                    rc = new Rectangle()
                    {
                        X = x - monitorSize.Width / 2,
                        Y = y - monitorSize.Height / 2,
                        Size = monitorSize
                    };

                    JetEazy.QUtilities.QUtility.ClipBoundary(ref rc, ref rectCamRes);
                    if (rc.Width > 0 && rc.Height > 0)
                    {
                        m_monitorRects.Add(rc);
                        m_monitorBmps.Add(null);
                        m_monitorBmpds.Add(null);
                    }
                }
            }
        }
        private void _prepareImgBuf(int id, BitmapData bmpdSrc)
        {
            if (id >= m_monitorBmps.Count)
                return;

            if (m_monitorBmps[id] == null) // || m_monitorBmps[id].PixelFormat != bmpdSrc.PixelFormat)
            {
                Rectangle rc = m_monitorRects[id];
                rc.Location = Point.Empty;
                m_monitorBmps[id] = new Bitmap(rc.Width, rc.Height, bmpdSrc.PixelFormat);
                m_monitorBmpds[id] = m_monitorBmps[id].LockBits(rc, ImageLockMode.ReadWrite, bmpdSrc.PixelFormat);
            }

            if (m_compareAction == null)
            {
                unsafe
                {
                    switch (bmpdSrc.PixelFormat)
                    {
                        case PixelFormat.Format32bppPArgb:
                        case PixelFormat.Format32bppArgb:
                        case PixelFormat.Format32bppRgb:
                        case PixelFormat.Format24bppRgb:
                            m_compareAction = _actionCompareFrozen24;
                            break;
                        case PixelFormat.Format8bppIndexed:
                        default:
                            m_compareAction = _actionCompareFrozen8;
                            break;
                    }
                }
            }
        }
        private void _disposeFrozenMonitor()
        {
            if (m_monitorBmps != null)
            {
                for (int i = 0, iCount = m_monitorBmps.Count; i < iCount; i++)
                {
                    Bitmap bmp = m_monitorBmps[i];
                    BitmapData bmpd = m_monitorBmpds[i];
                    if (bmp != null && bmpd != null)
                    {
                        bmp.UnlockBits(bmpd);
                    }
                    m_monitorBmps[i] = null;
                    m_monitorBmpds[i] = null;
                }
                m_monitorBmps.Clear();
                m_monitorBmpds.Clear();
                m_monitorBmps = null;
                m_monitorBmpds = null;
            }
        }
        private int _checkFrozen(BitmapData bmpdNewComing)
        {
            if (bmpdNewComing == null)
                return int.MaxValue;

            if (bmpdNewComing.Width != m_rectCamRes.Width ||
                bmpdNewComing.Height != m_rectCamRes.Height)
                return int.MaxValue;

            int iDiffCount = 0;

            unsafe
            {
                for (int id = 0, count = m_monitorRects.Count; id < count; id++)
                {
                    _prepareImgBuf(id, bmpdNewComing);
                    BitmapData bmpdDst = m_monitorBmpds[id];
                    Rectangle rc1 = m_monitorRects[id];
                    Point ptDst = Point.Empty;
                    JetEazy.QxImageScan.QxBmpCrossScan.Scan(bmpdDst, bmpdNewComing, ptDst, ref rc1, m_compareAction, ref iDiffCount);
                }
            }

            if (SIM_FROZEN)
                iDiffCount = 0;

            return iDiffCount;
        }
        private unsafe void _actionCompareFrozen24(byte* pucPtr0, byte* pucPtr1, ref int arg)
        {
            if (pucPtr0[0] != pucPtr1[0] ||
                pucPtr0[1] != pucPtr1[1] ||
                pucPtr0[2] != pucPtr1[2])
            {
                pucPtr0[0] = pucPtr1[0];
                pucPtr0[1] = pucPtr1[1];
                pucPtr0[2] = pucPtr1[2];
                arg++;
                return;
            }

            //==========================================================
            // After this remark line,
            //  pucPtr0 and pucPtr1 should be the same color.
            //==========================================================
            // 亮度: 低於 C_DARK 或 大於等於 C_WHITE, 就不比對, 視為Diff.
            //==========================================================

            if (pucPtr0[0] < C_DARK /*&&  pucPtr1[0] < C_DARK */ &&
                pucPtr0[1] < C_DARK /*&&  pucPtr1[1] < C_DARK */ &&
                pucPtr0[2] < C_DARK /*&&  pucPtr1[2] < C_DARK */ )
            {
                arg++;
                return;
            }

            if (pucPtr0[0] >= C_WHITE &&
                pucPtr0[1] >= C_WHITE &&
                pucPtr0[2] >= C_WHITE)
            {
                arg++;
                return;
            }
        }
        private unsafe void _actionCompareFrozen8(byte* pucPtr0, byte* pucPtr1, ref int arg)
        {
            if (pucPtr0[0] != pucPtr1[0])
            {
                pucPtr0[0] = pucPtr1[0];
                arg++;
                return;
            }

            //=========================================================
            // After this remark line,
            //  pucPtr0 and pucPtr1 should be the same color.
            //=========================================================
            // 亮度: 低於 C_DARK 或 大於等於 C_WHITE, 就不比對, 視為Diff.
            //==========================================================

            if (pucPtr0[0] < C_DARK /*&& pucPtr1[0] < C_DARK*/)
            {
                arg++;
                return;
            }

            if (pucPtr0[0] >= C_WHITE)
            {
                arg++;
                return;
            }
        }
        private void _dump(string msg)
        {
        }
        #endregion
    }
}
