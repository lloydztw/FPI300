using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;
using VisionDesigner.PairLineFind;

namespace LaserAlignDX.GA.BasicSpace
{
    internal class MvdPairLineClass : IDisposable
    {
        VisionDesigner.PairLineFind.CPairLineFindTool cPairLineFindTool = null;

        public MvdPairLineClass() { }
        ~MvdPairLineClass()
        {
            Dispose();
        }
        /// <summary>
        /// 寻找方向 左右型 true从左到右 上下型 true从上到下
        /// </summary>
        public bool bPositive { get; set; } = true;
        /// <summary>
        /// 搜寻方向 true左右型 false上下型
        /// </summary>
        public bool bFindOrient { get; set; } = true;
        ///// <summary>
        ///// 边缘极性 true白到黑 false黑到白
        ///// </summary>
        //public bool bEdgePolarity { get; set; } = true;
        /// <summary>
        /// 卡尺数量
        /// </summary>
        public int CaliperNum { get; set; } = 100;
        /// <summary>
        /// 边缘强度
        /// </summary>
        public int iEdgeStrength { get; set; } = 5;

        public CPairLineFindResult Run(Bitmap bmpInput, RectangleF eRecf)
        {
            VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(
                    eRecf.X + eRecf.Width / 2,
                    eRecf.Y + eRecf.Height / 2,
                    eRecf.Width,
                    eRecf.Height);
            return Run(bmpInput, cMvd);
        }
        public CPairLineFindResult Run(Bitmap bmpInput, CMvdRectangleF eRecf)
        {
            //PointF p1 = new PointF(0, 0);
            //PointF p2 = new PointF(bmpInput.Width, 0);

            VisionDesigner.PairLineFind.CPairLineFindResult cPairLineFindRes = null;

            try

            {

                // CreateInstance
                cPairLineFindTool = new VisionDesigner.PairLineFind.CPairLineFindTool();
                // Set input image
                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                cPairLineFindTool.InputImage = cInputImg;
                // Set ROI region (optional)
                VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(
                    eRecf.CenterX,
                    eRecf.CenterY,
                    eRecf.Width,
                    eRecf.Height);
                if (eRecf.Angle > 0)
                    cMvd.Angle = (bPositive ? eRecf.Angle : eRecf.Angle - 180);
                else
                    cMvd.Angle = (bPositive ? eRecf.Angle + 180 : eRecf.Angle);
                cPairLineFindTool.ROI = cMvd;

                cPairLineFindTool.SetRunParam("CaliperNum", CaliperNum.ToString());//卡尺数量
                cPairLineFindTool.SetRunParam("FindOrient", (bFindOrient ? "LeftToRight" : "UpToDown"));//搜索方向
                cPairLineFindTool.SetRunParam("Edge0Polarity", "Both");//边缘极性
                cPairLineFindTool.SetRunParam("Edge1Polarity", "Both");//边缘极性
                cPairLineFindTool.SetRunParam("EdgeStrength", iEdgeStrength.ToString());//边缘强度

                cPairLineFindTool.SetRunParam("FitInitType", "LLS");//拟合初始化类型
                cPairLineFindTool.SetRunParam("ProjectLen", "5");//卡尺宽度
                cPairLineFindTool.SetRunParam("KernelSize", "1");//滤波核半宽

                // Running

                cPairLineFindTool.Run();

                //  Get the result

                cPairLineFindRes = cPairLineFindTool.Result;
                Console.WriteLine("Recognition status: {0}", cPairLineFindRes.Status);
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            return cPairLineFindRes;
        }
        public void Dispose()
        {
            if (cPairLineFindTool != null)
            {
                cPairLineFindTool.Dispose();
                cPairLineFindTool = null;
            }
        }

        /// <summary>
        /// Bitmap 转成 CMvdImage
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
        private CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        {
            //AForge.Imaging.Filters.Grayscale grayscale = new AForge.Imaging.Filters.Grayscale(0.299, 0.587, 0.114);
            //bmpInputImg = grayscale.Apply(bmpInputImg);

            CMvdImage cMvdImage = new CMvdImage();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0;
                int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
                        bitmapIndex += 3;
                    }
                    bitmapIndex += offset;
                }
                MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
                stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
                stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
                stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
                cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return cMvdImage;
        }
    }
}
