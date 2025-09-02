using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AHBlobPro
{
    class JzAHBlob
    {
    }

    public class JetImgproc
    {
        public unsafe static int AddImg(JetGrayImg xSrcImg1, JetGrayImg xSrcImg2, JetGrayImg xDstImg)
        {
            if (xSrcImg1.Width != xSrcImg2.Width || xSrcImg1.Height != xSrcImg2.Height || xSrcImg1.Pitch != xSrcImg2.Pitch || xSrcImg1.Width != xDstImg.Width || xSrcImg1.Height != xDstImg.Height || xSrcImg1.Pitch != xDstImg.Pitch)
            {
                return -1;
            }
            int width = xSrcImg1.Width;
            int height = xSrcImg1.Height;
            int pitch = xSrcImg1.Pitch;
            fixed (byte* data = xSrcImg1.Data, data2 = xSrcImg2.Data, data3 = xDstImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr = data + i * pitch / 1;
                    byte* ptr2 = data2 + i * pitch / 1;
                    byte* ptr3 = data3 + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        int num = (int)(ptr[j / 1] + *(ptr2 + j / 1));
                        ptr3[j / 1] = (num > 255) ? (byte)255 : ((byte)num);
                    }
                }
            }
            return 0;
        }
        public unsafe static int Inverse(JetGrayImg xSrcImg, JetGrayImg xDstImg)
        {
            if (xSrcImg.Width != xDstImg.Width || xSrcImg.Height != xDstImg.Height || xSrcImg.Pitch != xDstImg.Pitch)
            {
                return -1;
            }
            int width = xSrcImg.Width;
            int height = xSrcImg.Height;
            int pitch = xSrcImg.Pitch;
            fixed (byte* data = xSrcImg.Data, data2 = xDstImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr = data + i * pitch / 1;
                    byte* ptr2 = data2 + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        ptr2[j / 1] = (byte)((byte)255 - *(ptr + j / 1));
                    }
                }
            }
            return 0;
        }
        public unsafe static int Threshold(JetGrayImg xSrcImg, int nThreshold, JetGrayImg xDstImg)
        {
            if (xSrcImg.Width != xDstImg.Width || xSrcImg.Height != xDstImg.Height || xSrcImg.Pitch != xDstImg.Pitch)
            {
                return -1;
            }
            int width = xSrcImg.Width;
            int height = xSrcImg.Height;
            int pitch = xSrcImg.Pitch;
            fixed (byte* data = xSrcImg.Data, data2 = xDstImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr = data + i * pitch / 1;
                    byte* ptr2 = data2 + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        ptr2[j / 1] = (((int)(*(ptr + j / 1)) >= nThreshold) ? (byte)255 : (byte)0);
                    }
                }
            }
            return 0;
        }
        public unsafe static int Threshold(JetGrayImg xSrcImg, int nHighThreshold, int nLowThreshold, JetGrayImg xDstImg)
        {
            if (xSrcImg.Width != xDstImg.Width || xSrcImg.Height != xDstImg.Height || xSrcImg.Pitch != xDstImg.Pitch)
            {
                return -1;
            }
            int width = xSrcImg.Width;
            int height = xSrcImg.Height;
            int pitch = xSrcImg.Pitch;
            fixed (byte* data = xSrcImg.Data, data2 = xDstImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr = data + i * pitch / 1;
                    byte* ptr2 = data2 + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        ptr2[j / 1] = (((int)(*(ptr + j / 1)) >= nLowThreshold && (int)(*(ptr + j / 1)) < nHighThreshold) ? (byte)255 : (byte)0);
                    }
                }
            }
            return 0;
        }
        public unsafe static int ProjectOnRow(JetGrayImg xSrcImg, double[] ProjectedData)
        {
            if (ProjectedData == null)
            {
                return -1;
            }
            if (xSrcImg.Width != ProjectedData.Length)
            {
                return -2;
            }
            Array.Clear(ProjectedData, 0, ProjectedData.Length);
            int width = xSrcImg.Width;
            int height = xSrcImg.Height;
            int pitch = xSrcImg.Pitch;
            fixed (byte* data = xSrcImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    byte* ptr = data + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        ProjectedData[j] += (double)ptr[j / 1];
                    }
                }
                for (int k = 0; k < width; k++)
                {
                    ProjectedData[k] /= (double)height;
                }
            }
            return 0;
        }
        public unsafe static int ProjectOnColumn(JetGrayImg xSrcImg, double[] ProjectedData)
        {
            if (ProjectedData == null)
            {
                return -1;
            }
            if (xSrcImg.Height != ProjectedData.Length)
            {
                return -2;
            }
            Array.Clear(ProjectedData, 0, ProjectedData.Length);
            int width = xSrcImg.Width;
            int height = xSrcImg.Height;
            int pitch = xSrcImg.Pitch;
            fixed (byte* data = xSrcImg.Data)
            {
                for (int i = 0; i < height; i++)
                {
                    int num = 0;
                    byte* ptr = data + i * pitch / 1;
                    for (int j = 0; j < width; j++)
                    {
                        num += (int)ptr[j / 1];
                    }
                    ProjectedData[i] = (double)num / (double)width;
                }
            }
            return 0;
        }
        public static int MinResidue(JetGrayImg xSrcImg, ref int nThreshold)
        {
            return 0;
        }
    }

    //internal class Label
    //{
    //    #region Public Properties

    //    public int Name { get; set; }

    //    public Label Root { get; set; }

    //    public int Rank { get; set; }

    //    #endregion

    //    #region Constructor

    //    public Label(int Name)
    //    {
    //        this.Name = Name;
    //        this.Root = this;
    //        this.Rank = 0;
    //    }

    //    #endregion

    //    #region Public Methods

    //    internal Label GetRoot()
    //    {
    //        if (this.Root != this)
    //        {
    //            this.Root = this.Root.GetRoot();//Compact tree
    //        }

    //        return this.Root;
    //    }

    //    internal void Join(Label root2)
    //    {
    //        if (root2.Rank < this.Rank)//is the rank of Root2 less than that of Root1 ?
    //        {
    //            root2.Root = this;//yes! then Root1 is the parent of Root2 (since it has the higher rank)
    //        }
    //        else //rank of Root2 is greater than or equal to that of Root1
    //        {
    //            this.Root = root2;//make Root2 the parent

    //            if (this.Rank == root2.Rank)//both ranks are equal ?
    //            {
    //                root2.Rank++;//increment Root2, we need to reach a single root for the whole tree
    //            }
    //        }
    //    }

    //    #endregion
    //}

    public enum JColorChannel
    {
        R = 0,
        G,
        B,
    }

    public class JetGrayImg
    {
        private int m_nWidth = 0;
        private int m_nHeight = 0;
        private int m_nPitch = 0;
        private byte[] m_pxData = null;

        public JetGrayImg()
        {
        }

        public JetGrayImg(int nWidth, int nHeight)
        {
            if (nWidth <= 0 && nHeight <= 0)
            {
                return;
            }

            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_nPitch = (nWidth % 4) == 0 ? nWidth : 4 * (1 + (int)(nWidth / 4));
            m_pxData = new byte[m_nPitch * m_nHeight];
        }

        /// <summary>
        /// For Format8bppIndexed, Format24bppRgb and Format32bppArgb
        /// </summary>
        public JetGrayImg(Bitmap xSrcImg)
        {
            switch (xSrcImg.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    {
                        BitmapData bitmapData = xSrcImg.LockBits(
                    new Rectangle(0, 0, xSrcImg.Width, xSrcImg.Height),
                    ImageLockMode.ReadOnly,
                    xSrcImg.PixelFormat);

                        m_pxData = new byte[bitmapData.Stride * bitmapData.Height];
                        IntPtr address = bitmapData.Scan0;
                        Marshal.Copy(address, m_pxData, 0, bitmapData.Stride * bitmapData.Height);

                        m_nWidth = bitmapData.Width;
                        m_nHeight = bitmapData.Height;
                        m_nPitch = bitmapData.Stride;

                        xSrcImg.UnlockBits(bitmapData);
                    }
                    break;

                case PixelFormat.Format24bppRgb:
                    {
                        int nStep = 3;

                        BitmapData bitmapData = xSrcImg.LockBits(
                                            new Rectangle(0, 0, xSrcImg.Width, xSrcImg.Height),
                                            ImageLockMode.ReadOnly,
                                            xSrcImg.PixelFormat);


                        int nNewStride = (4 * (int)(bitmapData.Width / 4)) + ((bitmapData.Width % 4 == 0) ? 0 : 4);

                        m_pxData = new byte[nNewStride * bitmapData.Height];

                        unsafe
                        {
                            fixed (byte* pxDstPtr = m_pxData)
                            {
                                byte* pxSwapSrcPtr = (byte*)bitmapData.Scan0;
                                byte* pxSwapDstPtr = pxDstPtr;
                                for (int j = 0; j < bitmapData.Height; j++)
                                {
                                    for (int i = 0; i < bitmapData.Width; i++)
                                    {
                                        pxSwapDstPtr[i] = (byte)(
                                            pxSwapSrcPtr[(i * nStep)] * 0.3 +
                                            pxSwapSrcPtr[(i * nStep)] * 0.59 +
                                            pxSwapSrcPtr[(i * nStep)] * 0.11);
                                    }
                                    pxSwapSrcPtr += bitmapData.Stride;
                                    pxSwapDstPtr += nNewStride;
                                }
                            }
                        }



                        m_nWidth = bitmapData.Width;
                        m_nHeight = bitmapData.Height;
                        m_nPitch = nNewStride;

                        xSrcImg.UnlockBits(bitmapData);
                    }
                    break;

                case PixelFormat.Format32bppArgb:
                    {
                        int nStep = 4;

                        BitmapData bitmapData = xSrcImg.LockBits(
                                            new Rectangle(0, 0, xSrcImg.Width, xSrcImg.Height),
                                            ImageLockMode.ReadOnly,
                                            xSrcImg.PixelFormat);


                        int nNewStride = (4 * (int)(bitmapData.Width / 4)) + ((bitmapData.Width % 4 == 0) ? 0 : 4);

                        m_pxData = new byte[nNewStride * bitmapData.Height];

                        unsafe
                        {
                            fixed (byte* pxDstPtr = m_pxData)
                            {
                                byte* pxSwapSrcPtr = (byte*)bitmapData.Scan0;
                                byte* pxSwapDstPtr = pxDstPtr;
                                for (int j = 0; j < bitmapData.Height; j++)
                                {
                                    for (int i = 0; i < bitmapData.Width; i++)
                                    {
                                        pxSwapDstPtr[i] = (byte)(
                                            pxSwapSrcPtr[(i * nStep)] * 0.3 +
                                            pxSwapSrcPtr[(i * nStep)] * 0.59 +
                                            pxSwapSrcPtr[(i * nStep)] * 0.11);
                                    }
                                    pxSwapSrcPtr += bitmapData.Stride;
                                    pxSwapDstPtr += nNewStride;
                                }
                            }
                        }



                        m_nWidth = bitmapData.Width;
                        m_nHeight = bitmapData.Height;
                        m_nPitch = nNewStride;

                        xSrcImg.UnlockBits(bitmapData);
                    }
                    break;
            }


        }

        /// <summary>
        /// Only for Format24bppRgb and Format32bppArgb
        /// </summary>
        public JetGrayImg(Bitmap xSrcImg, JColorChannel eChannel)
        {
            int nStep = 0;
            if (xSrcImg.PixelFormat == PixelFormat.Format24bppRgb)
            {
                nStep = 3;
            }

            if (xSrcImg.PixelFormat == PixelFormat.Format32bppArgb)
            {
                nStep = 4;
            }

            if (nStep == 0)
            {
                return;
            }

            int nChannelIndex = 0;
            switch (eChannel)
            {
                case JColorChannel.R:
                    nChannelIndex = 0;
                    break;

                case JColorChannel.G:
                    nChannelIndex = 1;
                    break;

                case JColorChannel.B:
                    nChannelIndex = 2;
                    break;
            }

            BitmapData bitmapData = xSrcImg.LockBits(
                                new Rectangle(0, 0, xSrcImg.Width, xSrcImg.Height),
                                ImageLockMode.ReadOnly,
                                xSrcImg.PixelFormat);


            int nNewStride = (4 * (int)(bitmapData.Width / 4)) + ((bitmapData.Width % 4 == 0) ? 0 : 4);

            m_pxData = new byte[nNewStride * bitmapData.Height];

            unsafe
            {
                fixed (byte* pxDstPtr = m_pxData)
                {
                    byte* pxSwapSrcPtr = (byte*)bitmapData.Scan0;
                    byte* pxSwapDstPtr = pxDstPtr;
                    for (int j = 0; j < bitmapData.Height; j++)
                    {
                        for (int i = 0; i < bitmapData.Width; i++)
                        {
                            pxSwapDstPtr[i] = pxSwapSrcPtr[(i * nStep) + nChannelIndex];
                        }
                        pxSwapSrcPtr += bitmapData.Stride;
                        pxSwapDstPtr += nNewStride;
                    }
                }
            }



            m_nWidth = bitmapData.Width;
            m_nHeight = bitmapData.Height;
            m_nPitch = nNewStride;

            xSrcImg.UnlockBits(bitmapData);
        }

        public void SetResolution(int nWidth, int nHeight)
        {
            if (nWidth <= 0 && nHeight <= 0)
            {
                return;
            }

            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_nPitch = (nWidth % 4) == 0 ? nWidth : 4 * (1 + (int)(nWidth / 4));
            m_pxData = new byte[m_nPitch * m_nHeight];
        }

        public byte[] Data
        {
            get
            {
                return m_pxData;
            }
        }

        public int Width
        {
            get
            {
                return m_nWidth;
            }
        }

        public int Height
        {
            get
            {
                return m_nHeight;
            }
        }

        public int Pitch
        {
            get
            {
                return m_nPitch;
            }
        }

        public JetGrayImg Clone()
        {
            JetGrayImg xRetItem = new JetGrayImg();
            xRetItem.SetResolution(this.Width, this.Height);
            this.m_pxData.CopyTo(xRetItem.Data, 0);
            return xRetItem;
        }

        public Bitmap ToBitmap()
        {
            Bitmap xRetObj = new Bitmap(m_nWidth, m_nHeight, m_nPitch, PixelFormat.Format8bppIndexed, Marshal.UnsafeAddrOfPinnedArrayElement(m_pxData, 0));
            xRetObj.Palette = GetGrayScalePalette();


            return xRetObj;
        }

        private ColorPalette GetGrayScalePalette()
        {
            Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

            ColorPalette monoPalette = bmp.Palette;

            Color[] entries = monoPalette.Entries;

            for (int i = 0; i < 256; i++)
            {
                entries[i] = Color.FromArgb(i, i, i);
            }

            return monoPalette;
        }

    }
    public enum JConnexity
    {
        Connexity4 = 0,
        Connexity8,
    }

    public enum JBlobLayer
    {
        WhiteLayer = 255,
        BlackLayer = 0,
    }

    public enum JBlobIntFeature
    {
        Area,
        BoundingBoxWidth,
        BoundingBoxHeight,
        LeftMost,
        RightMost,
        TopMost,
        BottomMost,
    }

    public enum JBlobFloatFeature
    {
        BoundingBoxCenterX,
        BoundingBoxCenterY,
        MinFeret,
    }

    public class JRotatedRectangleF
    {
        public double fCX = -1;
        public double fCY = -1;
        public double fAngle = -1;
        public double fWidth = -1;
        public double fHeight = -1;
    }

    public class xRunItem //根據 Run-Length coding 所建構的類別
    {
        public int nRow, nStartCol, nEndCol, nLabel, nNext;
        public xRunItem(int r, int s, int e, int l)
        {
            nRow = r;
            nStartCol = s;
            nEndCol = e;
            nLabel = l;
            nNext = -1;
        }
    }

    public class JetBlob
    {
        private const int nLengthLimitor = 4;
        private const int TableCapacityTimes = 5;
        private const int LinkItemTimes = 8000;

        public xRunItem[] m_pxRunItems = null;
        public int[] m_pnBlobIndex = null;

        private class xQueueItem
        {
            public int nSQueue, nEQueue;
            public xQueueItem(int s, int e)
            {
                nSQueue = s;
                nEQueue = e;
            }
        }

        private class xLinkItem
        {
            public int nNext, nTail;
            public xLinkItem()
            {
                nNext = -1;
                nTail = -1;
            }
        }


        private unsafe int[] Labeling(JetGrayImg xSrcImg, List<xRunItem> pxRuns,
            ref int nLabelCount, JConnexity eConnexity, JBlobLayer eLayer)
        {
            /*
            20181018 note by Allen
            pxList 與 pnTable 設計的上限在於 能夠容納多少 Label 存在，要是一張影像裡面有太多的Run, 例如30W畫素的影像包含15W個Run，那會發生存取錯誤的問題
            直接把 pxList 以及 pnTable 的宣告上限改為 Width * Height 即可，目前的設計是固定在 5 倍，即是能夠承受 5 * nWidth 個 Label。
            */
            byte[] ucSrcImg = xSrcImg.Data;
            int nWidth = xSrcImg.Width;
            int nPitch = xSrcImg.Pitch;
            int nHeight = xSrcImg.Height;

            int nConnexity = eConnexity == JConnexity.Connexity8 ? 1 : 0;
            int nThreshold = eLayer == JBlobLayer.WhiteLayer ? 255 : 0;

            int nLength = nWidth * nHeight;
            int nQLength = (int)(nWidth / 2) + 1;
            int nSLabel = 0;
            int nELabel = 0;
            int nQIndexScanFrom, nQIndexScanEnd, nQIndexNextFrom;
            UInt32 nQIndexScan = UInt32.MaxValue;
            int nListCapacity = nWidth * LinkItemTimes;
            //nListCapacity = nWidth * nHeight;
            xLinkItem[] pxList = new xLinkItem[nListCapacity];
            for (int x = 0; x < nListCapacity; x++)
            {
                pxList[x] = new xLinkItem(); ;
            }

            List<xQueueItem> Queue = new List<xQueueItem>();

            int nTableCapacity = nWidth * TableCapacityTimes;
            nTableCapacity = nWidth * nHeight;
            int[] pnTable = new int[nTableCapacity];
            for (int i = 0; i < nTableCapacity; i++)
            {
                pnTable[i] = 0;
            }


            //開始 Labeling
            fixed (byte* pusSrcImg = ucSrcImg)
            {
                int[] pnDstNow = new int[nWidth];
                int[] pnDstPre = new int[nWidth];
                int[] pnSwapBuf = new int[nWidth];
                for (int x = 0; x < nWidth; x++)
                {
                    pnDstNow[x] = 0;
                    pnDstPre[x] = 0;
                    pnSwapBuf[x] = 0;
                }






                nLabelCount = 1;


                #region Process the first row

                bool bStart = false;
                byte* pusSrcPtr = pusSrcImg;
                for (int i = 0; i < nWidth; i++)
                {
                    if (pusSrcPtr[i] == nThreshold)
                    {
                        bStart = true;
                        nSLabel = i;
                        i++;

                        for (; i < nWidth; i++)
                        {
                            if (pusSrcPtr[i] != nThreshold)
                            {
                                nELabel = i - 1;

                                Queue.Add(new xQueueItem(nSLabel, nELabel));

                                //Add Run Element
                                pnDstNow[nSLabel] = nLabelCount;
                                pnDstNow[nELabel] = nLabelCount;

                                pxRuns.Add(new xRunItem(0, nSLabel, nELabel, nLabelCount));

                                pnTable[nLabelCount] = nLabelCount;
                                pxList[nLabelCount].nNext = -1;
                                pxList[nLabelCount].nTail = nLabelCount;
                                nLabelCount++;

                                bStart = false;
                                break;
                            }
                        }
                    }

                }


                if (bStart) //check the last element(pixel) of the first Row
                {//若為真，表前面有一個Run還沒找到Column End Pixel
                    bStart = false;
                    nELabel = nWidth - 1;


                    Queue.Add(new xQueueItem(nSLabel, nELabel));

                    //Add Run Element
                    pnDstNow[nSLabel] = nLabelCount;
                    pnDstNow[nELabel] = nLabelCount;


                    pxRuns.Add(new xRunItem(0, nSLabel, nELabel, nLabelCount));

                    pnTable[nLabelCount] = nLabelCount;
                    pxList[nLabelCount].nNext = -1;
                    pxList[nLabelCount].nTail = nLabelCount;
                    nLabelCount++;
                }


                //Array.Copy(pnDst, 0, pnDst, nROIWidth*2, nROIWidth);
                //memcpy(pnDst + nROIWidth*2, pnDst, nROIWidth*sizeof(int));

                #endregion

                #region Process the other rows


                nQIndexScanEnd = Queue.Count;
                nQIndexScanFrom = 0;


                for (int j = 1; j < nHeight; j++)
                {
                    Array.Copy(pnDstNow, pnDstPre, nWidth);

                    if (j == nHeight - 1)///???? for what
                    {
                        for (int x = 0; x < nWidth; x++)
                        {
                            pnDstNow[x] = 0;
                        }
                    }


                    nQIndexNextFrom = Queue.Count;


                    pusSrcPtr = pusSrcImg + j * nPitch;
                    int i = 0; //Index of ucSrc
                    #region Element 0
                    if (pusSrcPtr[0] == nThreshold)
                    {
                        bStart = true;
                        nSLabel = j * nWidth; //標示出該Run的起始位置(2維的加總起始)	


                        for (i = 1; i < nWidth - 1; i++)
                        {
                            if (pusSrcPtr[i] != nThreshold)
                            {

                                nELabel = j * nWidth + i - 1;

                                Queue.Add(new xQueueItem(nSLabel, nELabel));

                                int nConnectedEnd = nELabel - nWidth + nConnexity;
                                int nConnectedStart = (j - 1) * nWidth;
                                int nLabelNow = 0;
                                int s = -1;
                                int e = -1;


                                int y = nQIndexScanFrom;
                                for (; y < nQIndexScanEnd; y++)
                                {
                                    s = Queue[y].nSQueue;
                                    e = Queue[y].nEQueue;

                                    if (e >= nConnectedStart)
                                    {
                                        if (s <= nConnectedEnd)
                                        {
                                            nLabelNow = pnDstPre[s % nWidth];

                                            //Add Run Element
                                            pnDstNow[nSLabel % nWidth] = nLabelNow;
                                            pnDstNow[nELabel % nWidth] = nLabelNow;
                                            pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                                            y++;
                                            break;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (nLabelNow == 0)
                                {//Current run doesnt connect to any run in previous row.
                                    nLabelNow = nLabelCount;

                                    //Add Run Element
                                    pnDstNow[nSLabel % nWidth] = nLabelNow;
                                    pnDstNow[nELabel % nWidth] = nLabelNow;
                                    pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                                    pnTable[nLabelCount] = nLabelCount;
                                    pxList[nLabelCount].nNext = -1;
                                    pxList[nLabelCount].nTail = nLabelCount;
                                    nLabelCount++;

                                    if (e < 0)  //delete all the previous runs without connection, because they wont be connected in the future
                                        nQIndexScanFrom = nQIndexScanEnd;
                                }
                                else
                                {//Keep Searching the connected object
                                    for (; y < nQIndexScanEnd; y++)
                                    {
                                        s = Queue[y].nSQueue;
                                        e = Queue[y].nEQueue;

                                        if (s <= nConnectedEnd)
                                        {//Resolve

                                            int u, v;

                                            u = pnTable[pnDstPre[s % nWidth]];  //目前搜尋到的Run, 其真正的Label Number (存於Table中的)
                                            v = pnTable[nLabelNow]; //目前真正的Label Number(存於Table中的)

                                            if (u < v)
                                            {//merge operation (u,v), 表示

                                                int z = v;

                                                while (z != -1)
                                                {
                                                    pnTable[z] = u;
                                                    z = pxList[z].nNext;
                                                }

                                                pxList[pxList[u].nTail].nNext = v;
                                                pxList[u].nTail = pxList[v].nTail;

                                            }
                                            else if (u > v)
                                            {//merge operation (v,u)
                                                int z = u;

                                                while (z != -1)
                                                {
                                                    pnTable[z] = v;
                                                    z = pxList[z].nNext;
                                                }

                                                pxList[pxList[v].nTail].nNext = u;
                                                pxList[v].nTail = pxList[u].nTail;
                                            }
                                        }
                                        else
                                        {
                                            nQIndexScanFrom = y - 1;
                                            break;
                                        }

                                    }

                                }

                                bStart = false;
                                break;
                            }

                        }


                    }
                    #endregion


                    #region Element 1 th to (nROIWidth - 1)th
                    for (; i < (nWidth - 1); i++)
                    {

                        if (pusSrcPtr[i] == nThreshold)
                        {
                            bStart = true;
                            nSLabel = j * nWidth + i;
                            i++;

                            for (; i < (nWidth - 1); i++)
                            {
                                if (pusSrcPtr[i] != nThreshold)
                                {

                                    nELabel = (j * nWidth) + i - 1;


                                    Queue.Add(new xQueueItem(nSLabel, nELabel));


                                    int nConnectedEnd = nELabel - nWidth + nConnexity;
                                    int nConnectedStart = nSLabel - nWidth - nConnexity;
                                    int nLabelNow = 0;
                                    int s = -1;
                                    int e = -1;


                                    int y = nQIndexScanFrom;
                                    for (; y < nQIndexScanEnd; y++)
                                    {
                                        s = Queue[y].nSQueue;
                                        e = Queue[y].nEQueue;

                                        if (e >= nConnectedStart)
                                        {
                                            if (s <= nConnectedEnd)
                                            {
                                                nLabelNow = pnDstPre[s % nWidth];

                                                //Add Run Element
                                                pnDstNow[nSLabel % nWidth] = nLabelNow;
                                                pnDstNow[nELabel % nWidth] = nLabelNow;
                                                pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                                                y++;
                                                break;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    if (nLabelNow == 0)
                                    {//Current Row doesnt connect to any run in previous row.
                                        nLabelNow = nLabelCount;

                                        //Add Run Element
                                        pnDstNow[nSLabel % nWidth] = nLabelNow;
                                        pnDstNow[nELabel % nWidth] = nLabelNow;
                                        pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                                        pnTable[nLabelCount] = nLabelCount;
                                        pxList[nLabelCount].nNext = -1;
                                        pxList[nLabelCount].nTail = nLabelCount;
                                        nLabelCount++;

                                        if (e < 0)  //delete all the previous runs without connection, because they wont be connected in the future
                                            nQIndexScanFrom = nQIndexScanEnd;
                                    }
                                    else
                                    {//Keep Searching the connected object
                                        for (; y < nQIndexScanEnd; y++)
                                        {
                                            s = Queue[y].nSQueue;
                                            e = Queue[y].nEQueue;

                                            if (s <= nConnectedEnd)
                                            {//Resolve

                                                int u, v;

                                                u = pnTable[pnDstPre[s % nWidth]];  //目前搜尋到的Run, 其真正的Label Number (存於Table中的)
                                                v = pnTable[nLabelNow]; //目前真正的Label Number(存於Table中的)

                                                if (u < v)
                                                {//merge operation (u,v), 表示

                                                    int z = v;

                                                    while (z != -1)
                                                    {
                                                        pnTable[z] = u;
                                                        z = pxList[z].nNext;
                                                    }

                                                    pxList[pxList[u].nTail].nNext = v;
                                                    pxList[u].nTail = pxList[v].nTail;

                                                }
                                                else if (u > v)
                                                {//merge operation (v,u)
                                                    int z = u;

                                                    while (z != -1)
                                                    {
                                                        pnTable[z] = v;
                                                        z = pxList[z].nNext;
                                                    }

                                                    pxList[pxList[v].nTail].nNext = u;
                                                    pxList[v].nTail = pxList[u].nTail;
                                                }
                                            }
                                            else
                                            {
                                                nQIndexScanFrom = y - 1;
                                                break;
                                            }

                                        }

                                    }

                                    bStart = false;
                                    break;
                                }

                            }


                        }

                    }

                    #endregion

                    //process the last element in the current row
                    #region Element (nROIWidth - 1) th
                    int nSrcIndex = j * nPitch + nWidth - 1;
                    int nROIIndex = j * nWidth + nWidth - 1;
                    if (pusSrcImg[nSrcIndex] == nThreshold)
                    {

                        if (!bStart)
                        {//this run only include one pixel
                            nSLabel = nROIIndex;
                            nELabel = nROIIndex;
                        }
                        else
                        {//this run ends in this pixel
                            bStart = false;
                            nELabel = nROIIndex;
                        }


                        Queue.Add(new xQueueItem(nSLabel, nELabel));


                        int nConnectedEnd = nELabel - nWidth;
                        int nConnectedStart = nSLabel - nWidth - nConnexity;
                        int nLabelNow = 0;
                        int s = -1;
                        int e = -1;


                        int y = nQIndexScanFrom;
                        for (; y < nQIndexScanEnd; y++)
                        {
                            s = Queue[y].nSQueue;
                            e = Queue[y].nEQueue;

                            if (e >= nConnectedStart)
                            {
                                if (s <= nConnectedEnd)
                                {
                                    nLabelNow = pnDstPre[s % nWidth];

                                    //Add Run Element
                                    pnDstNow[nSLabel % nWidth] = nLabelNow;
                                    pnDstNow[nELabel % nWidth] = nLabelNow;
                                    pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));


                                    y++;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (nLabelNow == 0) //Current Row doesnt connect to any run in previous row.
                        {
                            nLabelNow = nLabelCount;

                            //Add Run Element
                            pnDstNow[nSLabel % nWidth] = nLabelNow;
                            pnDstNow[nELabel % nWidth] = nLabelNow;
                            pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                            pnTable[nLabelCount] = nLabelCount;
                            pxList[nLabelCount].nNext = -1;
                            pxList[nLabelCount].nTail = nLabelCount;
                            nLabelCount++;

                            //delete all the previous runs without connection, because they wont be connected in the future
                            if (e < 0)
                            {
                                nQIndexScanFrom = nQIndexScanEnd;
                            }
                        }
                        else
                        {//Keep Searching the connected object
                            for (; y < nQIndexScanEnd; y++)
                            {
                                s = Queue[y].nSQueue;
                                e = Queue[y].nEQueue;

                                if (s <= nConnectedEnd)
                                {//Resolve

                                    int u, v;

                                    u = pnTable[pnDstPre[s % nWidth]];  //目前搜尋到的Run, 其真正的Label Number (存於Table中的)
                                    v = pnTable[nLabelNow]; //目前真正的Label Number(存於Table中的)

                                    if (u < v)
                                    {//merge operation (u,v), 表示
                                        int z = v;
                                        while (z != -1)
                                        {
                                            pnTable[z] = u;
                                            z = pxList[z].nNext;
                                        }

                                        pxList[pxList[u].nTail].nNext = v;
                                        pxList[u].nTail = pxList[v].nTail;
                                    }
                                    else if (u > v)
                                    {//merge operation (v,u)
                                        int z = u;
                                        while (z != -1)
                                        {
                                            pnTable[z] = v;
                                            z = pxList[z].nNext;
                                        }

                                        pxList[pxList[v].nTail].nNext = u;
                                        pxList[v].nTail = pxList[u].nTail;
                                    }
                                }
                                else
                                {
                                    nQIndexScanFrom = y - 1;
                                    break;
                                }

                            }

                        }

                    }
                    else
                    {
                        if (bStart)
                        {
                            bStart = false;
                            nELabel = nROIIndex - 1;

                            Queue.Add(new xQueueItem(nSLabel, nELabel));


                            int nConnectedEnd = nELabel - nWidth + nConnexity;
                            int nConnectedStart = nSLabel - nWidth - nConnexity;
                            int nLabelNow = 0;
                            int s = -1;
                            int e = -1;

                            int y = nQIndexScanFrom;
                            for (; y < nQIndexScanEnd; y++)
                            {
                                s = Queue[y].nSQueue;
                                e = Queue[y].nEQueue;

                                if (e >= nConnectedStart)
                                {
                                    if (s <= nConnectedEnd)
                                    {
                                        nLabelNow = pnDstPre[s % nWidth];

                                        //Add Run Element
                                        pnDstNow[nSLabel % nWidth] = nLabelNow;
                                        pnDstNow[nELabel % nWidth] = nLabelNow;
                                        pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));


                                        y++;
                                        break;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }


                            }

                            if (nLabelNow == 0)
                            {//Current Row doesnt connect to any run in previous row.
                                nLabelNow = nLabelCount;

                                //Add Run Element
                                pnDstNow[nSLabel % nWidth] = nLabelNow;
                                pnDstNow[nELabel % nWidth] = nLabelNow;
                                pxRuns.Add(new xRunItem(j, nSLabel % nWidth, nELabel % nWidth, nLabelNow));

                                pnTable[nLabelCount] = nLabelCount;
                                pxList[nLabelCount].nNext = -1;
                                pxList[nLabelCount].nTail = nLabelCount;
                                nLabelCount++;

                                if (e < 0)  //delete all the previous runs without connection, because they wont be connected in the future
                                    nQIndexScanFrom = y;
                            }
                            else
                            {//Keep Searching the connected object
                                for (; y < nQIndexScanEnd; y++)
                                {
                                    s = Queue[y].nSQueue;
                                    e = Queue[y].nEQueue;

                                    if (s <= nConnectedEnd)
                                    {//Resolve

                                        int u, v;

                                        u = pnTable[pnDstPre[s % nWidth]];  //目前搜尋到的Run, 其真正的Label Number (存於Table中的)
                                        v = pnTable[nLabelNow]; //目前真正的Label Number(存於Table中的)

                                        if (u < v)
                                        {//merge operation (u,v), 表示

                                            int z = v;

                                            while (z != -1)
                                            {
                                                pnTable[z] = u;
                                                z = pxList[z].nNext;
                                            }

                                            pxList[pxList[u].nTail].nNext = v;
                                            pxList[u].nTail = pxList[v].nTail;

                                        }
                                        else if (u > v)
                                        {//merge operation (v,u)
                                            int z = u;

                                            while (z != -1)
                                            {
                                                pnTable[z] = v;
                                                z = pxList[z].nNext;
                                            }

                                            pxList[pxList[v].nTail].nNext = u;
                                            pxList[v].nTail = pxList[u].nTail;
                                        }
                                    }
                                    else
                                    {
                                        nQIndexScanFrom = y - 1;
                                        break;
                                    }

                                }

                            }
                        }
                    }

                    #endregion

                    nQIndexScanEnd = Queue.Count;
                    nQIndexScanFrom = nQIndexNextFrom;
                }

                #endregion

                nLabelCount = nLabelCount - 1;
                return pnTable;
            }


        }

        public JetBlob()
        {
        }

        public int Labeling(JetGrayImg xSrcImg, JConnexity eConnexity, JBlobLayer eLayer)
        {
            int nLabelCount = 0;
            int[] pnSwapTable = null;
            List<xRunItem> xRunsBuf = new List<xRunItem>();


            /*
             執行完 Labeling 後
             pnSwapTable : 放置各初始 Label 對應的最終 Label(尚未排序)。剛結束 Labeling 時會有些零碎的 Label 尚未整合在一起。
             xRunsBuf : 放置所有 run 資訊/類別
             nLabelCount : 初始 Label 所使用到的編號數，因為尚未排序，所以不等於最後的編號數。
             */
            pnSwapTable = Labeling(xSrcImg, xRunsBuf, ref nLabelCount, eConnexity, eLayer);


            //Empty Blob 例外處理
            if (xRunsBuf.Count == 0 || nLabelCount == 0)
            {
                m_pnBlobIndex = null;
                m_pxRunItems = null;
            }

            //目前 nLabelCount 表 Label 編號在 Labeling 過程使用到 1~nLabelCount 號，紀錄於 pnSwapTable。這 Label 數量未經過 Resort，處理過後會更少
            int nReSortLabelCount = 0;
            int[] pnReSortTable = new int[nLabelCount];
            for (int i = 0; i < nLabelCount; i++)
            {
                pnReSortTable[i] = 0;
            }

            for (int x = 1; x <= nLabelCount; x++)
            {
                int nLabel = pnSwapTable[x] - 1;
                if (pnReSortTable[nLabel] == 0)
                {
                    nReSortLabelCount++;
                    pnReSortTable[nLabel] = nReSortLabelCount;
                }
            }




            //Linking and Re-Labeling the data of image1
            int[] pnPreviousIndex = new int[nReSortLabelCount];
            m_pnBlobIndex = new int[nReSortLabelCount];
            for (int i = 0; i < nReSortLabelCount; i++)
            {
                pnPreviousIndex[i] = -1;
                m_pnBlobIndex[i] = -1;
            }



            //Re-Link，將未連結的 Label (未排序的 Label 儲存於 pnSwapTable 中)鏈結起來，處理完後一個 Blob 對應一個 Label 數字
            m_pxRunItems = xRunsBuf.ToArray(); //儲存 Blobs 資訊於 Array
            for (int i = 0; i < m_pxRunItems.Length; i++)
            {
                int nLabel = pnReSortTable[pnSwapTable[m_pxRunItems[i].nLabel] - 1] - 1;

                m_pxRunItems[i].nLabel = nLabel;
                m_pnBlobIndex[nLabel] = (m_pnBlobIndex[nLabel] < 0) ? i : m_pnBlobIndex[nLabel];

                int nPreviousIndex = pnPreviousIndex[nLabel];

                if (nPreviousIndex >= 0)
                {
                    m_pxRunItems[nPreviousIndex].nNext = i;
                }

                pnPreviousIndex[nLabel] = i;
            }





            return 0;
        }

        public int BlobCount
        {
            get
            {
                return (m_pnBlobIndex != null) ? m_pnBlobIndex.Length : 0;
            }
            set
            {
                //do nothing
            }
        }
    }

    public class JetBlobFeature
    {
        public static int[] ComputeIntegerFeature(JetBlob xBlobs, JBlobIntFeature eFeature)
        {
            int nBlobCount = xBlobs.BlobCount;

            if (nBlobCount == 0)
            {
                return null;
            }

            int[] pnFeatures = new int[nBlobCount];

            switch (eFeature)
            {
                case JBlobIntFeature.Area:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeArea(xBlobs, x);
                    }
                    break;

                case JBlobIntFeature.LeftMost:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeLeftMost(xBlobs, x);
                    }
                    break;

                case JBlobIntFeature.RightMost:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeRightMost(xBlobs, x);
                    }
                    break;


                case JBlobIntFeature.TopMost:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeTopMost(xBlobs, x);
                    }
                    break;

                case JBlobIntFeature.BottomMost:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeBottomMost(xBlobs, x);
                    }
                    break;

                case JBlobIntFeature.BoundingBoxWidth:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeBoundingBoxWidth(xBlobs, x);
                    }
                    break;


                case JBlobIntFeature.BoundingBoxHeight:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pnFeatures[x] = ComputeBoundingBoxHeight(xBlobs, x);
                    }
                    break;

            }

            return pnFeatures;
        }

        public static int ComputeIntegerFeature(JetBlob xBlobs, int nIndex, JBlobIntFeature eFeature)
        {
            if (nIndex < 0 || nIndex >= xBlobs.BlobCount)
            {
                return -1;
            }


            switch (eFeature)
            {
                case JBlobIntFeature.TopMost:
                    return ComputeTopMost(xBlobs, nIndex);
                case JBlobIntFeature.BottomMost:
                    return ComputeBottomMost(xBlobs, nIndex);
                case JBlobIntFeature.LeftMost:
                    return ComputeLeftMost(xBlobs, nIndex);
                case JBlobIntFeature.RightMost:
                    return ComputeRightMost(xBlobs, nIndex);
                case JBlobIntFeature.Area:
                    return ComputeArea(xBlobs, nIndex);

                case JBlobIntFeature.BoundingBoxWidth:
                    return ComputeBoundingBoxWidth(xBlobs, nIndex);


                case JBlobIntFeature.BoundingBoxHeight:
                    return ComputeBoundingBoxHeight(xBlobs, nIndex);
            }

            return -1;
        }

        public static double[] ComputeFloatFeature(JetBlob xBlobs, JBlobFloatFeature eFeature)
        {
            int nBlobCount = xBlobs.BlobCount;

            if (nBlobCount == 0)
            {
                return null;
            }

            double[] pfFeatures = new double[nBlobCount];

            switch (eFeature)
            {
                case JBlobFloatFeature.BoundingBoxCenterX:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pfFeatures[x] = ComputeBoundingBoxCenterX(xBlobs, x);
                    }
                    break;

                case JBlobFloatFeature.BoundingBoxCenterY:
                    for (int x = 0; x < nBlobCount; x++)
                    {
                        pfFeatures[x] = ComputeBoundingBoxCenterY(xBlobs, x);
                    }
                    break;

            }
            return pfFeatures;
        }

        public static double ComputeFloatFeature(JetBlob xBlobs, int nIndex, JBlobFloatFeature eFeature)
        {
            if (nIndex < 0 || nIndex >= xBlobs.BlobCount)
            {
                return -1;
            }


            switch (eFeature)
            {
                case JBlobFloatFeature.BoundingBoxCenterX:
                    return ComputeBoundingBoxCenterX(xBlobs, nIndex);


                case JBlobFloatFeature.BoundingBoxCenterY:
                    return ComputeBoundingBoxCenterY(xBlobs, nIndex);

            }
            return -1;
        }

        public static List<Point> ComputeConvexHull(JetBlob xBlobs, int nIndex)
        {

            if (nIndex >= xBlobs.BlobCount || nIndex < 0)
            {
                return null;
            }



            //計算 Row count
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nPreRow = -1;
            int nRowsCount = 0;	//init row count
            int nSwapIndex = xBlobs.m_pnBlobIndex[nIndex];
            int nContourY = pxRunItems[nSwapIndex].nRow;
            while (nSwapIndex != -1)
            {
                xRunItem xItems = pxRunItems[nSwapIndex];
                nRowsCount = (xItems.nRow != nPreRow) ? nRowsCount + 1 : nRowsCount;
                nPreRow = xItems.nRow;
                nSwapIndex = xItems.nNext;
            }



            //獲得每 row 最右邊以及最左邊的pixel位置
            int[] pnLeftSidePt = new int[nRowsCount];  //Temporary
            int[] pnRightSidePt = new int[nRowsCount]; //Temporary
            nSwapIndex = xBlobs.m_pnBlobIndex[nIndex];
            nPreRow = -1;
            int nStoredPtCount = -1;
            while (nSwapIndex != -1)
            {
                xRunItem xItems = pxRunItems[nSwapIndex];
                int nCurRow = xItems.nRow;

                if (nCurRow != nPreRow)
                {
                    nStoredPtCount++;
                    pnLeftSidePt[nStoredPtCount] = xItems.nStartCol;
                }

                pnRightSidePt[nStoredPtCount] = xItems.nEndCol;
                nPreRow = nCurRow;
                nSwapIndex = xItems.nNext;
            }





            //RLC只有一筆資料，兩種特殊情況處理 : 形成 點/線 的這兩種形狀的 Blob
            if (nRowsCount == 1)
            {
                List<Point> xSpcCase = new List<Point>();
                if (pnRightSidePt[0] == pnLeftSidePt[0]) //single point
                {
                    xSpcCase.Add(new Point(pnLeftSidePt[0], nContourY));
                }
                else //single line
                {
                    xSpcCase.Add(new Point(pnLeftSidePt[0], nContourY));
                    xSpcCase.Add(new Point(pnRightSidePt[0], nContourY));
                }
                return xSpcCase;
            }


            //準備儲存 ConvexHull 點的空間, 分為左右(Right/Left)兩邊
            int nLeftAddedCount = 0;
            int[] pnLeftAddedX = new int[nRowsCount];
            int[] pnLeftAddedY = new int[nRowsCount];
            for (int i = 0; i < nRowsCount; i++)
            {
                pnLeftAddedX[i] = -1;
                pnLeftAddedY[i] = -1;
            }


            int nRightAddedCount = 0;
            int[] pnRightAddedX = new int[nRowsCount];
            int[] pnRightAddedY = new int[nRowsCount];
            for (int i = 0; i < nRowsCount; i++)
            {
                pnRightAddedX[i] = -1;
                pnRightAddedY[i] = -1;
            }



            int[] pnInitList = new int[nRowsCount]; //ConvexHull邊緣線的初始集合
            int[] pnFinalList = new int[nRowsCount]; //ConvexHull邊緣線的最終集合

            #region Process Left-Side Point

            //初始化LinkedList
            for (int i = 0; i < nRowsCount - 1; i++)
            {
                pnInitList[i] = i + 1;
            }
            pnInitList[nRowsCount - 1] = -1;

            for (int i = 0; i < nRowsCount; i++)
            {
                pnFinalList[i] = -1;
            }
            pnFinalList[0] = nRowsCount - 1;
            pnFinalList[nRowsCount - 1] = -1;



            int x = 0;
            pnLeftAddedX[nLeftAddedCount] = pnLeftSidePt[x];
            pnLeftAddedY[nLeftAddedCount] = x;
            nLeftAddedCount++;

            while (x < (nRowsCount - 1))
            {
                int nY0 = x;
                int nY1 = pnFinalList[x];
                int nX0 = pnLeftSidePt[nY0];
                int nX1 = pnLeftSidePt[nY1];

                float fSlope = (nX0 == nX1) ? Int32.MaxValue : (float)(nY0 - nY1) / (nX0 - nX1);
                float fConst = (fSlope == Int32.MaxValue) ? nX0 : (float)(nY0 - fSlope * nX0);
                float fDffMax = 0;
                int nMaxIndex = -1;
                if (fSlope != Int32.MaxValue)  //斜率此時不可能為0，因為之前已經排除此Blob為單一Run的情況
                {
                    //此迴圈找出新的三角形(表示有找到ConvexHull新點)
                    int w0 = x;
                    int w1 = pnInitList[x];
                    while (w1 < nY1)
                    {
                        float fXX = (w1 - fConst) / fSlope - pnLeftSidePt[w1];

                        if (fXX > fDffMax)
                        {
                            fDffMax = fXX;
                            nMaxIndex = w1;
                        }

                        if (fXX <= 0)
                        {
                            pnInitList[w0] = pnInitList[w1];

                        }
                        else
                        {
                            w0 = w1;
                        }

                        w1 = pnInitList[w1];

                    }


                }
                else
                {
                    //fSlope不存在，該連線為垂直
                    //此迴圈找出新的三角形(表示有找到ConvexHull新點)
                    int w0 = x;
                    int w1 = pnInitList[x];
                    while (w1 < nY1) //for(int i=1; i<nY1; i++)		
                    {
                        float fXX = fConst - pnLeftSidePt[w1];

                        if (fXX > fDffMax)
                        {
                            fDffMax = fXX;
                            nMaxIndex = w1;
                        }

                        if (fXX <= 0)
                        {
                            pnInitList[w0] = pnInitList[w1];
                        }
                        else
                        {
                            w0 = w1;
                        }


                        w1 = pnInitList[w1];
                    }

                }

                if (nMaxIndex > 0)//表示有找到ConvexHull新點
                {
                    pnFinalList[nMaxIndex] = pnFinalList[x];
                    pnFinalList[x] = nMaxIndex;
                }
                else //表x~pnIndexBuf[x]之間沒有新的ConvexHull點
                {
                    x = pnFinalList[x];

                    pnLeftAddedX[nLeftAddedCount] = nX1;
                    pnLeftAddedY[nLeftAddedCount] = nY1;
                    nLeftAddedCount++;
                }

            }

            #endregion


            #region Process Right-Side Point
            //開始處理Right Side的點
            //初始化LinkedList，記憶體空間重複利用	
            for (int i = 0; i < nRowsCount - 1; i++)
            {
                pnInitList[i] = i + 1;
            }
            pnInitList[nRowsCount - 1] = -1;

            for (int i = 0; i < nRowsCount; i++)
            {
                pnFinalList[i] = -1;
            }
            pnFinalList[0] = nRowsCount - 1;
            pnFinalList[nRowsCount - 1] = -1;

            //將起始點加入陣列，因為接下來的While迴圈都只會新增ConvexHull線段的尾端點
            x = 0;
            pnRightAddedX[nRightAddedCount] = pnRightSidePt[x];
            pnRightAddedY[nRightAddedCount] = x;
            nRightAddedCount++;

            while (x < (nRowsCount - 1))
            {
                int nY0 = x;
                int nY1 = pnFinalList[x];

                int nX0 = pnRightSidePt[nY0];
                int nX1 = pnRightSidePt[nY1];

                float fSlope = (nX0 == nX1) ? Int32.MaxValue : (float)(nY0 - nY1) / (nX0 - nX1);
                float fConst = (fSlope == Int32.MaxValue) ? nX0 : (float)(nY0 - fSlope * nX0);
                float fDffMax = 0;
                int nMaxIndex = -1;
                if (fSlope != Int32.MaxValue)  //斜率此時不可能為0，因為之前已經排除此Blob為單一Run的情況
                {

                    //此迴圈找出新的三角形(表示有找到ConvexHull新點)
                    int w0 = x;
                    int w1 = pnInitList[x];
                    while (w1 < nY1)
                    {
                        float fXX = pnRightSidePt[w1] - (w1 - fConst) / fSlope;

                        if (fXX > fDffMax)
                        {
                            fDffMax = fXX;
                            nMaxIndex = w1;
                        }

                        if (fXX <= 0)
                        {
                            pnInitList[w0] = pnInitList[w1];

                        }
                        else
                        {
                            w0 = w1;
                        }

                        w1 = pnInitList[w1];

                    }


                }
                else
                {
                    //fSlope不存在，該連線為垂直
                    //此迴圈找出新的三角形(表示有找到ConvexHull新點)
                    int w0 = x;
                    int w1 = pnInitList[x];
                    while (w1 < nY1) //for(int i=1; i<nY1; i++)		
                    {
                        float fXX = pnRightSidePt[w1] - fConst;

                        if (fXX > fDffMax)
                        {
                            fDffMax = fXX;
                            nMaxIndex = w1;
                        }

                        if (fXX <= 0)
                        {
                            pnInitList[w0] = pnInitList[w1];
                        }
                        else
                        {
                            w0 = w1;
                        }


                        w1 = pnInitList[w1];
                    }

                }

                if (nMaxIndex > 0)//表示有找到ConvexHull新點
                {
                    pnFinalList[nMaxIndex] = pnFinalList[x];
                    pnFinalList[x] = nMaxIndex;
                }
                else //表x~pnIndexBuf[x]之間沒有新的ConvexHull點
                {
                    x = pnFinalList[x];

                    pnRightAddedX[nRightAddedCount] = nX1;
                    pnRightAddedY[nRightAddedCount] = nY1;
                    nRightAddedCount++;
                }
            }
            #endregion


            List<Point> xConvexHull = new List<Point>();
            #region Store Result
            {

                for (int i = 0; i < nLeftAddedCount; i++)
                {
                    xConvexHull.Add(new Point(pnLeftAddedX[i], pnLeftAddedY[i] + nContourY));
                }

                int nSRIndex, nERIndex;
                if (pnLeftAddedX[nLeftAddedCount - 1] == pnRightAddedX[nRightAddedCount - 1]) //表最後一個Run為單一像素點
                {
                    nSRIndex = nRightAddedCount - 2;
                }
                else
                {
                    nSRIndex = nRightAddedCount - 1;
                }

                if (pnLeftAddedX[0] == pnRightAddedX[0]) //表最後一個Run為單一像素點
                {
                    nERIndex = 1;
                }
                else
                {
                    nERIndex = 0;
                }

                for (int i = nSRIndex; i >= nERIndex; i--)
                {
                    xConvexHull.Add(new Point(pnRightAddedX[i], pnRightAddedY[i] + nContourY));
                }
            }
            #endregion



            return xConvexHull;
        }

        public static double[] ComputeFeretDiameter(JetBlob xBlobs, double fFeretAngle)
        {
            int nBlobCount = xBlobs.BlobCount;

            if (nBlobCount == 0)
            {
                return null;
            }

            double[] pfFeatures = new double[nBlobCount];

            for (int x = 0; x < nBlobCount; x++)
            {
                pfFeatures[x] = ComputeFeretDiameter(xBlobs, x, fFeretAngle);
            }

            return pfFeatures;
        }

        public static double ComputeFeretDiameter(JetBlob xBlobs, int nIndex, double fFeretAngle)
        {
            int nBlobCount = xBlobs.BlobCount;
            if (nIndex >= nBlobCount || nIndex < 0)
            {
                return -1.0;
            }



            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;
            int nSwapIndex = xBlobs.m_pnBlobIndex[nIndex];
            double fMaxY = double.MinValue;
            double fMinY = double.MaxValue;
            double fMaxX = double.MinValue;
            double fMinX = double.MaxValue;
            double fCosSida = Math.Cos(fFeretAngle / 180.0 * Math.PI);
            double fSinSida = Math.Sin(fFeretAngle / 180.0 * Math.PI);
            while (nSwapIndex != -1)
            {
                xRunItem xItems = pxRunItems[nSwapIndex];


                //Compute the height
                /*
                double fX1 = xItems.nRow * fCosSida - xItems.nStartCol * fSinSida;
                double fX2 = xItems.nRow * fCosSida - xItems.nEndCol * fSinSida;

                if (fX1 > fX2)
                {
                    fMaxX = (fX1 > fMaxX) ? fX1 : fMaxX;
                    fMinX = (fX2 < fMinX) ? fX2 : fMinX;
                }
                else
                {
                    fMaxX = (fX2 > fMaxX) ? fX2 : fMaxX;
                    fMinX = (fX1 < fMinX) ? fX1 : fMinX;
                }
                */

                //Compute the width
                double fY1, fY2;
                fY1 = xItems.nRow * fSinSida + xItems.nStartCol * fCosSida;
                fY2 = xItems.nRow * fSinSida + xItems.nEndCol * fCosSida;

                if (fY1 > fY2)
                {
                    fMaxY = (fY1 > fMaxY) ? fY1 : fMaxY;
                    fMinY = (fY2 < fMinY) ? fY2 : fMinY;
                }
                else
                {
                    fMaxY = (fY2 > fMaxY) ? fY2 : fMaxY;
                    fMinY = (fY1 < fMinY) ? fY1 : fMinY;
                }



                nSwapIndex = xItems.nNext;
            }

            double fFeretWidth = fMaxY - fMinY;

            /*
            double fFeretHeight = fMaxX - fMinX;

            fCosSida = Math.Cos(-1.0f * fFeretAngle / 180.0 * Math.PI);
            fSinSida = Math.Sin(-1.0f * fFeretAngle / 180.0 * Math.PI);
            double fFeretCenterX1 = fMaxX * fCosSida - fMaxY * fSinSida;
            double fFeretCenterY1 = fMaxX * fSinSida + fMaxY * fCosSida;
            double fFeretCenterX2 = fMinX * fCosSida - fMinY * fSinSida;
            double fFeretCenterY2 = fMinX * fSinSida + fMinY * fCosSida;
            double fFeretCenterY = (fFeretCenterX1 + fFeretCenterX2) / 2.0 + 0.5;
            double fFeretCenterX = (fFeretCenterY1 + fFeretCenterY2) / 2.0 + 0.5;
            */

            return fFeretWidth;
        }

        public static JRotatedRectangleF ComputeMinRectangle(JetBlob xBlobs, int nIndex)
        {
            int nBlobCount = xBlobs.BlobCount;
            if (nIndex >= nBlobCount || nIndex < 0)
            {
                return null;
            }


            List<Point> xConvexHull = ComputeConvexHull(xBlobs, nIndex);
            if (xConvexHull == null)
            {
                return null;
            }




            int nDataLength = xConvexHull.Count;


            //Blob is a single point, return result directly.
            if (nDataLength == 1)
            {
                JRotatedRectangleF xRetObj = new JRotatedRectangleF();
                xRetObj.fAngle = 0.0;
                xRetObj.fCX = xConvexHull[0].X + 0.5;
                xRetObj.fCY = xConvexHull[0].Y + 0.5;
                xRetObj.fWidth = 1.0;
                xRetObj.fHeight = 1.0;

                return xRetObj;
            }

            //Blob is a vertical/horizontal line, return result directly.
            if (nDataLength == 2)
            {
                int nX1 = xConvexHull[0].X;
                int nY1 = xConvexHull[0].Y;
                int nX2 = xConvexHull[1].X;
                int nY2 = xConvexHull[1].Y;
                double fSlope = (nX1 == nX2) ? double.MaxValue : (float)(nY1 - nY2) / (nX1 - nX2);
                double fTempX = -1.0;
                double fTempY = -1.0;
                double fWidth = -1.0;
                double fHeight = -1.0;
                double fAngle = -1.0;

                if (fSlope == double.MaxValue) //blob 形狀為垂直直線
                {
                    fTempX = nX1;
                    fTempY = (nY1 + nY2) / 2.0;
                    fWidth = Math.Abs(nX2 - nX1) + 1.0;
                    fHeight = 1.0;
                    fAngle = 90.0;
                }
                else
                {
                    if (fSlope == 0) //blob 形狀為水平直線
                    {
                        fTempX = (nX1 + nX2) / 2.0f;
                        fTempY = nY1;
                        fWidth = Math.Abs(nX2 - nX1) + 1.0f;
                        fHeight = 1.0f;
                        fAngle = 0.0f;
                    }
                    else //blob 形狀為斜 45 度
                    {
                        fTempX = (nX1 + nX2) / 2.0f;
                        fTempY = (nY1 + nY2) / 2.0f;
                        fWidth = 1.0;
                        fHeight = Math.Abs(nY2 - nY1) + 1.0;
                        fAngle = (Math.Atan(fSlope) * 180) / Math.PI;
                        fAngle = (fAngle < 0.0) ? fAngle + 360.0 : fAngle;
                    }
                }

                JRotatedRectangleF xRetObj = new JRotatedRectangleF();
                xRetObj.fAngle = fAngle;
                xRetObj.fCX = fTempX;
                xRetObj.fCY = fTempY;
                xRetObj.fWidth = fWidth;
                xRetObj.fHeight = fHeight;

                return xRetObj;
            }


            int[] pnTempBufferX = new int[nDataLength];
            int[] pnTempBufferY = new int[nDataLength];
            for (int i = 0; i < nDataLength; i++)
            {
                pnTempBufferX[i] = xConvexHull[i].X;
                pnTempBufferY[i] = xConvexHull[i].Y;
            }



            double fMinArea = double.MaxValue; //Area of minimum rectangle
            double fRectangleSlope1 = double.MaxValue; //1st Slope of minimum rectangle's edge (m of y = mx + c)
            double fRectangleSlope2 = double.MaxValue; //2nd Slope of minimum rectangle's edge (m of y = mx + c)
            double fRectangleSlope3 = double.MaxValue; //3rd Slope of minimum rectangle's edge (m of y = mx + c)
            double fRectangleSlope4 = double.MaxValue; //4th Slope of minimum rectangle's edge (m of y = mx + c)
            double fRectangleC1 = 0; //1st Const of minimum rectangle's edge (c of y = mx + c)
            double fRectangleC2 = 0; //2nd Const of minimum rectangle's edge (c of y = mx + c)
            double fRectangleC3 = 0; //3rd Const of minimum rectangle's edge (c of y = mx + c)
            double fRectangleC4 = 0; //4th Const of minimum rectangle's edge (c of y = mx + c)
            double fRectangleWidth = 0; //Width of minimum Rectangle
            double fRectangleHeight = 0; //Height of minimum Rectangle
            int nTopLimitIndex = 0;
            int nBottomLimitIndex = 0;
            int nRightLimitIndex = 0;
            int nLeftLimitIndex = 0;
            int nSmallestX = pnTempBufferX[0];
            int nSmallestY = pnTempBufferY[0];
            int nLargestX = pnTempBufferX[0];
            int nLargestY = pnTempBufferY[0];

            //Compute 4 points : Top-Most, Left-Most, Bottom-Most, Right-Most 
            for (int i = 1; i < nDataLength; i++)
            {
                int nTempX = pnTempBufferX[i];
                int nTempY = pnTempBufferY[i];

                if (nLargestX < nTempX)
                {
                    nLargestX = nTempX;
                    nRightLimitIndex = i;
                }

                if (nSmallestX > nTempX)
                {
                    nSmallestX = nTempX;
                    nLeftLimitIndex = i;
                }

                if (nLargestY < nTempY)
                {
                    nLargestY = nTempY;
                    nBottomLimitIndex = i;
                }
            }




            int nIndex1 = nTopLimitIndex;
            int nIndex2 = nLeftLimitIndex;
            int nIndex3 = nBottomLimitIndex;
            int nIndex4 = nRightLimitIndex;
            while (nIndex1 < nBottomLimitIndex)
            {

                int nX1 = pnTempBufferX[nIndex1];
                int nY1 = pnTempBufferY[nIndex1];
                int nX12 = pnTempBufferX[nIndex1 + 1];
                int nY12 = pnTempBufferY[nIndex1 + 1];
                int nX3 = pnTempBufferX[nIndex3 % nDataLength];
                int nY3 = pnTempBufferY[nIndex3 % nDataLength];


                double fSlope1 = (nX1 == nX12) ? double.MaxValue : (double)(nY1 - nY12) / (nX1 - nX12);
                double fC1 = (fSlope1 == double.MaxValue) ? nX1 : (double)(nY1 - fSlope1 * nX1);
                double fCurrentDistance = (fSlope1 == double.MaxValue) ? Math.Abs(nX3 - nX1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);



                //Search the bottom point 
                while (nIndex3 < nDataLength)
                {
                    nIndex3++;

                    nX3 = pnTempBufferX[nIndex3 % nDataLength];
                    nY3 = pnTempBufferY[nIndex3 % nDataLength];

                    double fTempDistance = (fSlope1 == double.MaxValue) ? Math.Abs(nX3 - nX1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);

                    if (fTempDistance > fCurrentDistance)
                    {
                        fCurrentDistance = fTempDistance;
                    }
                    else
                    {
                        nIndex3--;
                        break;
                    }
                }







                //Search the Left point 
                int nX2 = pnTempBufferX[nIndex2];
                int nY2 = pnTempBufferY[nIndex2];

                double fSlope2;
                double fC2;
                double fCurrentDistance2;

                if (fSlope1 == double.MaxValue)
                    fSlope2 = 0;
                else if (fSlope1 == 0)
                    fSlope2 = (float)double.MaxValue;
                else
                    fSlope2 = -1.0 / fSlope1;

                fC2 = (fSlope2 == double.MaxValue) ? nX2 : (double)(nY2 - fSlope2 * nX2);
                fCurrentDistance2 = 0;


                int nSwapIndex2 = nIndex2;
                while (nSwapIndex2 < nDataLength * 2)
                {
                    nSwapIndex2++;

                    nX2 = pnTempBufferX[nSwapIndex2 % nDataLength];
                    nY2 = pnTempBufferY[nSwapIndex2 % nDataLength];

                    if (fSlope2 < double.MaxValue)
                    {
                        if ((int)((nX2 * fSlope2 + fC2 - nY2) * 100) / 100.0 < 0)
                        {
                            double fTempDistance = DistanceFromPointToLine(fSlope2, fC2, (double)nX2, (double)nY2);
                            if (fTempDistance > fCurrentDistance2)
                            {
                                fCurrentDistance2 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex2--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex2--;
                            break;
                        }
                    }
                    else
                    {

                        if (nX2 < fC2)
                        {
                            double fTempDistance = Math.Abs((double)nX2 - fC2);

                            if (fTempDistance > fCurrentDistance2)
                            {
                                fCurrentDistance2 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex2--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex2--;
                            break;
                        }
                    }

                }

                nIndex2 = nSwapIndex2 % nDataLength;


                //Search the Right point 
                int nX4 = pnTempBufferX[nIndex4];
                int nY4 = pnTempBufferY[nIndex4];

                double fSlope4 = fSlope2;
                double fC4 = (fSlope4 == double.MaxValue) ? nX4 : (nY4 - fSlope4 * nX4);
                double fCurrentDistance4 = 0;



                int nSwapIndex4 = nIndex4;
                while (nSwapIndex4 < nDataLength * 2)
                {
                    nSwapIndex4++;

                    nX4 = pnTempBufferX[nSwapIndex4 % nDataLength];
                    nY4 = pnTempBufferY[nSwapIndex4 % nDataLength];

                    if (fSlope4 < double.MaxValue)
                    {
                        if ((int)((nX4 * fSlope4 + fC4 - nY4) * 100) / 100.0f > 0)
                        {
                            double fTempDistance = DistanceFromPointToLine(fSlope4, fC4, (double)nX4, (double)nY4);
                            if (fTempDistance > fCurrentDistance4)
                            {
                                fCurrentDistance4 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex4--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex4--;
                            break;
                        }
                    }
                    else
                    {
                        if (nX4 > fC4)
                        {
                            double fTempDistance = Math.Abs(nX4 - fC4);

                            if (fTempDistance > fCurrentDistance4)
                            {
                                fCurrentDistance4 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex4--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex4--;
                            break;
                        }
                    }

                }

                nIndex4 = nSwapIndex4 % nDataLength;

                //Temp Result
                nX2 = pnTempBufferX[nIndex2 % nDataLength];
                nY2 = pnTempBufferY[nIndex2 % nDataLength];
                fC2 = (fSlope2 == double.MaxValue) ? nX2 : (double)(nY2 - fSlope2 * nX2);

                nX4 = pnTempBufferX[nIndex4 % nDataLength];
                nY4 = pnTempBufferY[nIndex4 % nDataLength];
                fC4 = (fSlope4 == double.MaxValue) ? nX4 : (double)(nY4 - fSlope4 * nX4);

                nX3 = pnTempBufferX[nIndex3 % nDataLength];
                nY3 = pnTempBufferY[nIndex3 % nDataLength];

                double nDistance1 = (fSlope2 == double.MaxValue) ? Math.Abs((float)nX4 - fC2) : DistanceFromPointToLine(fSlope2, fC2, (double)nX4, (double)nY4);
                double nDistance2 = (fSlope1 == double.MaxValue) ? Math.Abs((float)nX3 - fC1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);
                double fArea = nDistance1 * nDistance2;

                if (fArea < fMinArea)
                {
                    fMinArea = fArea;
                    fRectangleWidth = nDistance1;
                    fRectangleHeight = nDistance2;
                    fRectangleSlope1 = fSlope1;
                    fRectangleSlope2 = fSlope2;
                    fRectangleSlope3 = fSlope1;
                    fRectangleSlope4 = fSlope4;
                    fRectangleC1 = fC1;
                    fRectangleC2 = fC2;
                    fRectangleC3 = (fSlope1 == double.MaxValue) ? nX3 : (double)(nY3 - fSlope1 * nX3); ;
                    fRectangleC4 = fC4;
                }




                nIndex1++;

            }



            while (nIndex1 < nDataLength - 1)
            {

                int nX1 = pnTempBufferX[nIndex1];
                int nY1 = pnTempBufferY[nIndex1];
                int nX12 = pnTempBufferX[nIndex1 + 1];
                int nY12 = pnTempBufferY[nIndex1 + 1];
                int nX3 = pnTempBufferX[nIndex3 % nDataLength];
                int nY3 = pnTempBufferY[nIndex3 % nDataLength];


                double fSlope1 = (nX1 == nX12) ? double.MaxValue : (double)(nY1 - nY12) / (nX1 - nX12);
                double fC1 = (fSlope1 == double.MaxValue) ? nX1 : (double)(nY1 - fSlope1 * nX1);
                double fCurrentDistance = (fSlope1 == double.MaxValue) ? Math.Abs(nX3 - nX1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);



                //Search the bottom point 
                while (nIndex3 > 0)
                {
                    nIndex3++;

                    nX3 = pnTempBufferX[nIndex3 % nDataLength];
                    nY3 = pnTempBufferY[nIndex3 % nDataLength];

                    double fTempDistance = (fSlope1 == double.MaxValue) ? Math.Abs(nX3 - nX1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);

                    if (fTempDistance > fCurrentDistance)
                    {
                        fCurrentDistance = fTempDistance;
                    }
                    else
                    {
                        nIndex3--;
                        break;
                    }
                }





                //Search the Left point 
                int nX2 = pnTempBufferX[nIndex2];
                int nY2 = pnTempBufferY[nIndex2];

                double fSlope2;
                double fC2;
                double fCurrentDistance2;

                if (fSlope1 == double.MaxValue)
                    fSlope2 = 0;
                else if (fSlope1 == 0)
                    fSlope2 = double.MaxValue;
                else
                    fSlope2 = -1.0f / fSlope1;

                fC2 = (fSlope2 == double.MaxValue) ? nX2 : (nY2 - fSlope2 * nX2);
                fCurrentDistance2 = 0;


                int nSwapIndex2 = nIndex2;
                while (nSwapIndex2 < nDataLength * 2)
                {
                    nSwapIndex2++;

                    nX2 = pnTempBufferX[nSwapIndex2 % nDataLength];
                    nY2 = pnTempBufferY[nSwapIndex2 % nDataLength];

                    if (fSlope2 < double.MaxValue)
                    {
                        if ((int)((nX2 * fSlope2 + fC2 - nY2) * 100) / 100.0f > 0)
                        {
                            double fTempDistance = DistanceFromPointToLine(fSlope2, fC2, (double)nX2, (double)nY2);
                            if (fTempDistance > fCurrentDistance2)
                            {
                                fCurrentDistance2 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex2--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex2--;
                            break;
                        }
                    }
                    else
                    {
                        if (nX2 > fC2)
                        {
                            double fTempDistance = Math.Abs((float)nX2 - fC2);

                            if (fTempDistance > fCurrentDistance2)
                            {
                                fCurrentDistance2 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex2--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex2--;
                            break;
                        }
                    }

                }

                nIndex2 = nSwapIndex2 % nDataLength;


                //Search the Right point 
                int nX4 = pnTempBufferX[nIndex4];
                int nY4 = pnTempBufferY[nIndex4];

                double fSlope4 = fSlope2;
                double fC4 = (fSlope4 == double.MaxValue) ? nX4 : (nY4 - fSlope4 * nX4);
                double fCurrentDistance4 = 0;

                int nSwapIndex4 = nIndex4;
                while (nSwapIndex4 < nDataLength * 2)
                {
                    nSwapIndex4++;

                    nX4 = pnTempBufferX[nSwapIndex4 % nDataLength];
                    nY4 = pnTempBufferY[nSwapIndex4 % nDataLength];

                    if (fSlope4 < double.MaxValue)
                    {
                        if ((int)((nX4 * fSlope4 + fC4 - nY4) * 100) / 100.0 < 0)
                        {
                            double fTempDistance = DistanceFromPointToLine(fSlope4, fC4, (double)nX4, (double)nY4);
                            if (fTempDistance > fCurrentDistance4)
                            {
                                fCurrentDistance4 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex4--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex4--;
                            break;
                        }
                    }
                    else
                    {
                        if (nX4 < fC4)
                        {
                            double fTempDistance = Math.Abs((double)nX4 - fC4);

                            if (fTempDistance > fCurrentDistance4)
                            {
                                fCurrentDistance4 = fTempDistance;
                            }
                            else
                            {
                                nSwapIndex4--;
                                break;
                            }
                        }
                        else
                        {
                            nSwapIndex4--;
                            break;
                        }
                    }

                }

                nIndex4 = nSwapIndex4 % nDataLength;

                //Calculate the width and height
                nX2 = pnTempBufferX[nIndex2 % nDataLength];
                nY2 = pnTempBufferY[nIndex2 % nDataLength];
                fC2 = (fSlope2 == double.MaxValue) ? nX2 : (double)(nY2 - fSlope2 * nX2);

                nX4 = pnTempBufferX[nIndex4 % nDataLength];
                nY4 = pnTempBufferY[nIndex4 % nDataLength];
                fC4 = (fSlope4 == double.MaxValue) ? nX4 : (double)(nY4 - fSlope4 * nX4);

                nX3 = pnTempBufferX[nIndex3 % nDataLength];
                nY3 = pnTempBufferY[nIndex3 % nDataLength];

                double nDistance1 = (fSlope2 == double.MaxValue) ? Math.Abs((float)nX4 - fC2) : DistanceFromPointToLine(fSlope2, fC2, (double)nX4, (double)nY4);
                double nDistance2 = (fSlope1 == double.MaxValue) ? Math.Abs((float)nX3 - fC1) : DistanceFromPointToLine(fSlope1, fC1, (double)nX3, (double)nY3);
                double fArea = nDistance1 * nDistance2;

                if (fArea < fMinArea)
                {
                    fMinArea = fArea;
                    fRectangleWidth = nDistance1;
                    fRectangleHeight = nDistance2;
                    fRectangleSlope1 = fSlope1;
                    fRectangleSlope2 = fSlope2;
                    fRectangleSlope3 = fSlope1;
                    fRectangleSlope4 = fSlope4;
                    fRectangleC1 = fC1;
                    fRectangleC2 = fC2;
                    fRectangleC3 = (fSlope1 == double.MaxValue) ? nX3 : (double)(nY3 - fSlope1 * nX3); ;
                    fRectangleC4 = fC4;
                }

                nIndex1++;

            }


            //Compute the angle and centerX,Y
            double fRectangleCX; //Center X of minimum rectangle
            double fRectangleCY; //Center Y of minimum rectangle
            double fRectangleAngle; //Angle of minimum rectangle
            if (fRectangleSlope1 == 0)
            {
                double nCrossX1 = fRectangleC2;
                double nCrossY1 = fRectangleC1;
                double nCrossX2 = fRectangleC4;
                double nCrossY2 = fRectangleC3;

                fRectangleCX = (nCrossX1 + nCrossX2) / 2.0;
                fRectangleCY = (nCrossY1 + nCrossY2) / 2.0;
                fRectangleAngle = 0.0;

                JRotatedRectangleF xRetObj = new JRotatedRectangleF();
                xRetObj.fAngle = fRectangleAngle;
                xRetObj.fCX = fRectangleCX + 0.5;
                xRetObj.fCY = fRectangleCY + 0.5;
                xRetObj.fWidth = fRectangleWidth;
                xRetObj.fHeight = fRectangleHeight;

                return xRetObj;

            }
            else if (fRectangleSlope2 == 0)
            {
                double nCrossX1 = fRectangleC1;
                double nCrossY1 = fRectangleC2;
                double nCrossX2 = fRectangleC3;
                double nCrossY2 = fRectangleC4;

                double fSwap = fRectangleWidth;
                fRectangleWidth = fRectangleHeight;
                fRectangleHeight = fSwap;

                fRectangleCX = (nCrossX1 + nCrossX2) / 2.0;
                fRectangleCY = (nCrossY1 + nCrossY2) / 2.0;
                fRectangleAngle = 0.0;

                JRotatedRectangleF xRetObj = new JRotatedRectangleF();
                xRetObj.fAngle = fRectangleAngle;
                xRetObj.fCX = fRectangleCX + 0.5;
                xRetObj.fCY = fRectangleCY + 0.5;
                xRetObj.fWidth = fRectangleWidth;
                xRetObj.fHeight = fRectangleHeight;
                return xRetObj;
            }
            else
            {
                double nCrossX1 = (fRectangleC2 - fRectangleC1) / (fRectangleSlope1 - fRectangleSlope2);
                double nCrossY1 = fRectangleSlope1 * (fRectangleC2 - fRectangleC1) / (fRectangleSlope1 - fRectangleSlope2) + fRectangleC1;
                double nCrossX2 = (fRectangleC4 - fRectangleC3) / (fRectangleSlope3 - fRectangleSlope4);
                double nCrossY2 = fRectangleSlope3 * (fRectangleC4 - fRectangleC3) / (fRectangleSlope3 - fRectangleSlope4) + fRectangleC3;

                fRectangleCX = (nCrossX1 + nCrossX2) / 2.0;
                fRectangleCY = (nCrossY1 + nCrossY2) / 2.0;
                fRectangleAngle = (Math.Atan(fRectangleSlope1) * 180.0) / Math.PI;

                if (fRectangleAngle < 0.0)
                {
                    fRectangleAngle += 90.0;
                    double fSwap = fRectangleWidth;
                    fRectangleWidth = fRectangleHeight;
                    fRectangleHeight = fSwap;
                }

                JRotatedRectangleF xRetObj = new JRotatedRectangleF();
                xRetObj.fAngle = fRectangleAngle;
                xRetObj.fCX = fRectangleCX + 0.5;
                xRetObj.fCY = fRectangleCY + 0.5;
                xRetObj.fWidth = fRectangleWidth;
                xRetObj.fHeight = fRectangleHeight;

                return xRetObj;
            }


        }

        /*
        Private function
         */
        private static int ComputeArea(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;
            int nArea = 0;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];
            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nArea += xItems.nEndCol - xItems.nStartCol + 1;
                nCurIndex = xItems.nNext;
            }


            return nArea;
        }

        private static int ComputeLeftMost(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nLeftMost = Int32.MaxValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];
            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nLeftMost = xItems.nStartCol < nLeftMost ? xItems.nStartCol : nLeftMost;
                nCurIndex = xItems.nNext;
            }

            return nLeftMost;
        }

        private static int ComputeRightMost(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nRightMost = Int32.MinValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];
            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nRightMost = xItems.nEndCol > nRightMost ? xItems.nEndCol : nRightMost;
                nCurIndex = xItems.nNext;
            }


            return nRightMost;
        }

        private static int ComputeTopMost(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nTopMost = Int32.MaxValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nTopMost = xItems.nRow < nTopMost ? xItems.nRow : nTopMost;
                nCurIndex = xItems.nNext;
            }

            return nTopMost;
        }

        private static int ComputeBottomMost(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nBottomMost = Int32.MinValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nBottomMost = xItems.nRow > nBottomMost ? xItems.nRow : nBottomMost;
                nCurIndex = xItems.nNext;
            }

            return nBottomMost;
        }

        private static int ComputeBoundingBoxWidth(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nBBoxW = 0;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                int nTmp = xItems.nEndCol - xItems.nStartCol + 1;
                nBBoxW = nTmp > nBBoxW ? nTmp : nBBoxW;
                nCurIndex = xItems.nNext;
            }

            return nBBoxW;
        }

        private static int ComputeBoundingBoxHeight(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nMinRow = Int32.MaxValue;
            int nMaxRow = Int32.MinValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                int nTmpRow = xItems.nRow;
                nMinRow = nTmpRow < nMinRow ? nTmpRow : nMinRow;
                nMaxRow = nTmpRow > nMaxRow ? nTmpRow : nMaxRow;
                nCurIndex = xItems.nNext;
            }

            return (nMaxRow - nMinRow + 1);
        }

        private static double ComputeBoundingBoxCenterX(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nLeftMost = Int32.MaxValue;
            int nRightMost = Int32.MinValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nLeftMost = nLeftMost > xItems.nStartCol ? xItems.nStartCol : nLeftMost;
                nRightMost = nRightMost < xItems.nEndCol ? xItems.nEndCol : nRightMost;
                nCurIndex = xItems.nNext;
            }

            return (nRightMost + nLeftMost) / 2.0;
        }

        private static double ComputeBoundingBoxCenterY(JetBlob xBlobs, int nIndex)
        {
            xRunItem[] pxRunItems = xBlobs.m_pxRunItems;

            int nTopMost = Int32.MaxValue;
            int nBottomMost = Int32.MinValue;
            int nCurIndex = xBlobs.m_pnBlobIndex[nIndex];
            int nRow = -1;

            while (nCurIndex != -1)
            {
                xRunItem xItems = pxRunItems[nCurIndex];
                nRow = xItems.nRow;
                nTopMost = nTopMost > nRow ? nRow : nTopMost;
                nBottomMost = nBottomMost < nRow ? nRow : nBottomMost;
                nCurIndex = xItems.nNext;
            }

            return (nTopMost + nBottomMost) / 2.0;
        }

        private static double DistanceFromPointToLine(double fSlope, double fC, double fX, double fY)
        {
            //return abs(fSlope * fX - fY + fC) / sqrt(pow(fSlope, 2.0f) + 1);
            return Math.Abs(fSlope * fX - fY + fC) / Math.Sqrt(fSlope * fSlope + 1);
        }


    }
}
