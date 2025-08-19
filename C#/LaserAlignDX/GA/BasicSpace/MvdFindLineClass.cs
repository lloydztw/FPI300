using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VisionDesigner;

namespace LaserAlignDX.GA.BasicSpace
{
    public class MvdFindLineClass : IDisposable
    {
        VisionDesigner.LineFind.CLineFindTool cLineFindToolObj = null;

        public MvdFindLineClass() { }
        ~MvdFindLineClass()
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
        /// <summary>
        /// 边缘极性 true白到黑 false黑到白
        /// </summary>
        public bool bEdgePolarity {  get; set; } = true;
        /// <summary>
        /// 卡尺数量
        /// </summary>
        public int iRayNum { get; set; } = 100;
        /// <summary>
        /// 边缘强度
        /// </summary>
        public int iEdgeStrength { get; set; } = 5;

        public CMvdLineSegmentF Run(Bitmap bmpInput, RectangleF eRecf)
        {
            VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(
                    eRecf.X + eRecf.Width / 2,
                    eRecf.Y + eRecf.Height / 2,
                    eRecf.Width,
                    eRecf.Height);
            return Run(bmpInput, cMvd);
        }
        public CMvdLineSegmentF Run(Bitmap bmpInput, CMvdRectangleF eRecf)
        {
            //PointF p1 = new PointF(0, 0);
            //PointF p2 = new PointF(bmpInput.Width, 0);

            CMvdLineSegmentF retLineSegment = null;

            try

            {

                // CreateInstance
                cLineFindToolObj = new VisionDesigner.LineFind.CLineFindTool();
                // Set input image
                VisionDesigner.CMvdImage cInputImg = BitmapToCMvdImage(bmpInput);
                cLineFindToolObj.InputImage = cInputImg;
                // Set ROI region (optional)
                VisionDesigner.CMvdRectangleF cMvd = new VisionDesigner.CMvdRectangleF(
                    eRecf.CenterX,
                    eRecf.CenterY,
                    eRecf.Width,
                    eRecf.Height);
                cMvd.Angle = (bPositive ? 0 : 180);
                cLineFindToolObj.ROI = cMvd;

                cLineFindToolObj.SetRunParam("RayNum", iRayNum.ToString());//卡尺数量
                //cLineFindToolObj.SetRunParam("RejectNum", "30");//剔除点数
                //cLineFindToolObj.SetRunParam("RejectDist", "10");//剔除距离
                cLineFindToolObj.SetRunParam("FindOrient", (bFindOrient ? "LeftToRight" : "UpToDown"));//搜索方向
                cLineFindToolObj.SetRunParam("EdgePolarity", "Both");//边缘极性
                //cLineFindToolObj.SetRunParam("EdgePolarity", (bEdgePolarity ? "WhiteToBlack" : "BlackToWhite"));//边缘极性
                cLineFindToolObj.SetRunParam("EdgeStrength", iEdgeStrength.ToString());//边缘强度
                cLineFindToolObj.SetRunParam("LineFindMode", "Best");//查找模式
                //cLineFindToolObj.SetRunParam("KernelSize", "5");

                // Running

                cLineFindToolObj.Run();

                //  Get the result

                VisionDesigner.LineFind.CLineFindResult cLineFindRes = cLineFindToolObj.Result;

                Console.WriteLine("Recognition status: {0}", cLineFindRes.Status);

                Console.WriteLine("Start point of line: ({0},{1}）", cLineFindRes.LineStartPoint.fX, cLineFindRes.LineStartPoint.fY);

                if (cLineFindRes.Status == 1)
                {
                    retLineSegment = new CMvdLineSegmentF(cLineFindRes.LineStartPoint, cLineFindRes.LineEndPoint);
                }
            }

            catch (MvdException ex)

            {

                Console.WriteLine("Fail with ErrorCode: 0x" + ex.ErrorCode.ToString("X"));

            }

            catch (System.Exception ex)

            {

                Console.WriteLine("Fail with error " + ex.Message);

            }
            return retLineSegment;
        }
        public void Dispose()
        {
            if (cLineFindToolObj != null)
            {
                cLineFindToolObj.Dispose();
                cLineFindToolObj = null;
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
