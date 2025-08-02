using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using IMVSMarkFindModuCs;

using VM.Core;
using VM.PlatformSDKCS;
using ImageSourceModuleCs;
using IMVSQuadrangleFindModuCs;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PointF = System.Drawing.PointF;
using static VM.PlatformSDKCS.ImvsSdkDefine;
using System.Windows.Forms;

namespace WindowsFormsApp11
{
    public enum SolMode
    {
        Sol_FindEdge = 0,
        Sol_FindMark = 1,
    }
    public class FindRectByVM
    {

        /// <summary>
        /// 流程
        /// </summary>
        public VM.Core.VmProcedure mProcedure = null;

        /// <summary>
        /// 图像源
        /// </summary>
        private ImageSourceModuleTool mImage = null;
        /// <summary>
        /// 找比工具
        /// </summary>
        private IMVSQuadrangleFindModuTool FindEdgeTool = null;

        /// <summary>
        /// 找图形工具
        /// </summary>
        private IMVSMarkFindModuTool FindMarkTool1 = null;

        /// <summary>
        /// 找图形工具
        /// </summary>
        private IMVSMarkFindModuTool FindMarkTool2 = null;

        /// <summary>
        /// 找边FindTheEdge 找mark FindMark
        /// </summary>
        /// <param name="solname"></param>
        public FindRectByVM(SolMode sol = SolMode.Sol_FindEdge)
        {
            switch(sol)
            {
                case SolMode.Sol_FindMark:

                    #region FIND_MARK

                    try
                    {
                        string path = System.Environment.CurrentDirectory;
                        VM.Core.VmSolution.Load($"{path}\\FindMark.sol");
                        //加载方案
                        mProcedure = (VM.Core.VmProcedure)VM.Core.VmSolution.Instance["LC"];
                        //加载图像工具
                        mImage = (ImageSourceModuleTool)VM.Core.VmSolution.Instance["LC.LoadImg"];

                        //找图形工具
                        FindMarkTool1 = (IMVSMarkFindModuTool)VM.Core.VmSolution.Instance["LC.FindMark1"];
                        //找图形工具
                        FindMarkTool2 = (IMVSMarkFindModuTool)VM.Core.VmSolution.Instance["LC.FindMark2"];


                    }
                    catch (Exception ex)
                    {
                        //初始流程异常
                        MessageBox.Show($"{ex.Message}{Environment.NewLine}造成这种情况原因:{Environment.NewLine}" +
                            $"1:没有插加密狗;{Environment.NewLine}2:VisionMaster软件在运行;");
                        Environment.Exit(0);
                    }

                    #endregion

                    break;
                default:

                    #region FIND_EDGE

                    try
                    {
                        string path = System.Environment.CurrentDirectory;
                        VM.Core.VmSolution.Load($"{path}\\FindTheEdge.sol");
                        //加载方案
                        mProcedure = (VM.Core.VmProcedure)VM.Core.VmSolution.Instance["LC"];
                        //加载图像工具
                        mImage = (ImageSourceModuleTool)VM.Core.VmSolution.Instance["LC.LoadImg"];

                        //找边工具
                        FindEdgeTool = (IMVSQuadrangleFindModuTool)VM.Core.VmSolution.Instance["LC.FindTheEdge"];
                    }
                    catch (Exception ex)
                    {
                        //初始流程异常
                        MessageBox.Show($"{ex.Message}{Environment.NewLine}造成这种情况原因:{Environment.NewLine}" +
                            $"1:没有插加密狗;{Environment.NewLine}2:VisionMaster软件在运行;");
                        Environment.Exit(0);
                    }


                    #endregion

                    break;
            }

           
           
        }

        /// <summary>
        /// 运行找边工具
        /// </summary>
        /// <param name="bmpsrc">找边图像(不需要预处理)</param>
        /// <param name="linsInfo">四条边信息</param>
        /// <param name="lineAngle">四条边角度</param>
        /// <param name="center">中心</param>
        /// <returns>矩形四个顶点</returns>
        public PointF[] RunFindEdgeTool(Bitmap bmpsrc, out float[] lineAngle, out PointF center)
        {
            if (mImage == null || FindEdgeTool == null)
            {
                lineAngle = null;
                center = new PointF(0, 0);
                return null;
            }

            Bitmap bmpGray = null;
            if (bmpsrc.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                bmpGray = RgbToR8Ex(bmpsrc);
            }
            else
            {
                bmpGray = bmpsrc;
            }

            VM.PlatformSDKCS.ImageBaseData baseData = BitmapToImageBaseData(bmpGray);

            //加载图像
            //mImage.ClearAllInputImage();
            mImage.SetImageData(baseData);

            VmSolution.Instance.SyncRun();
            
            //mImage.Run();
            ////运行找边
            //FindEdgeTool.Run();

            //找边结果
            QuadrangleFindResult findEdgeResult = (QuadrangleFindResult)FindEdgeTool.ModuResult;


            PointF[] linsInfo = new PointF[8];
            //四条边角度
            lineAngle = new float[4];
            //中心
            center = new PointF(0, 0);
            if (findEdgeResult != null)
            {

                #region 四条直线起点终点
                linsInfo[0] = new PointF(findEdgeResult.EdgeLine1.StartPoint.X, findEdgeResult.EdgeLine1.StartPoint.Y);
                linsInfo[1] = new PointF(findEdgeResult.EdgeLine1.EndPoint.X, findEdgeResult.EdgeLine1.EndPoint.Y);

                linsInfo[2] = new PointF(findEdgeResult.EdgeLine2.StartPoint.X, findEdgeResult.EdgeLine2.StartPoint.Y);
                linsInfo[3] = new PointF(findEdgeResult.EdgeLine2.EndPoint.X, findEdgeResult.EdgeLine2.EndPoint.Y);

                linsInfo[4] = new PointF(findEdgeResult.EdgeLine3.StartPoint.X, findEdgeResult.EdgeLine3.StartPoint.Y);
                linsInfo[5] = new PointF(findEdgeResult.EdgeLine3.EndPoint.X, findEdgeResult.EdgeLine3.EndPoint.Y);

                linsInfo[6] = new PointF(findEdgeResult.EdgeLine4.StartPoint.X, findEdgeResult.EdgeLine4.StartPoint.Y);
                linsInfo[7] = new PointF(findEdgeResult.EdgeLine4.EndPoint.X, findEdgeResult.EdgeLine4.EndPoint.Y);

                #endregion

                #region 四条变角度
                lineAngle[0] = findEdgeResult.Line1Angle;
                lineAngle[1] = findEdgeResult.Line2Angle;
                lineAngle[2] = findEdgeResult.Line3Angle;
                lineAngle[3] = findEdgeResult.Line4Angle;
                #endregion

                center = new PointF(findEdgeResult.CentralPoint.X, findEdgeResult.CentralPoint.Y);
            }


            findEdgeResult = null;
            return linsInfo;
        }
        /// <summary>
        /// 找Mark点
        /// </summary>
        /// <param name="bmpsrc">源图</param>
        /// <param name="findScore">匹配分数</param>
        /// <returns>返回结果0,0就是没有找到</returns>
        public PointF RunFindMark(Bitmap bmpsrc, out float score, float findScore = 0.4f)
        {
            if (mImage == null || FindMarkTool1 == null || FindMarkTool2 == null)
            {
                score = 0;
                return new PointF(0, 0);
            }

            Bitmap bmpGray = null;
            if (bmpsrc.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                bmpGray = RgbToR8Ex(bmpsrc);
            }
            else
            {
                bmpGray = bmpsrc;
            }

            VM.PlatformSDKCS.ImageBaseData baseData = BitmapToImageBaseData(bmpGray);

            //加载图像
            //mImage.ClearAllInputImage();
            mImage.SetImageData(baseData);

            VmSolution.Instance.SyncRun();

            //找mark结果
            MarkFindResult findResult1 = FindMarkTool1.ModuResult;
            MarkFindResult findResult2 = FindMarkTool2.ModuResult;

            //都没有匹配结果
            //if(findResult1.MatchScore.Count==0&& findResult2.MatchScore.Count == 0)return new PointF(0, 0);


            if (findResult1.MatchScore.Count > 0 && findResult1.MatchScore[0] >= findScore)
            {
                VM.PlatformSDKCS.PointF center = findResult1.MatchRect[0].CenterPoint;
                score = findResult1.MatchScore[0];
                return new PointF(center.X, center.Y);
            }
            else if (findResult2.MatchScore.Count > 0 && findResult2.MatchScore[0] >= findScore)
            {
                VM.PlatformSDKCS.PointF center = findResult2.MatchRect[0].CenterPoint;
                score = findResult2.MatchScore[0];
                return new PointF(center.X, center.Y);
            }
            else
            {
                score = 0;
                return new PointF(0, 0);
            }
        }

        public void RunLcheng()
        {
            //通过流程获取结果
            VmProcedure vmProcess1 = (VmProcedure)VmSolution.Instance["流程1"];
            string ocrResult = vmProcess1.ModuResult.GetOutputString("out").astStringVal[0].strValue;
            string ocrNum = vmProcess1.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
        }





        #region  图像转VM图像
        /// <summary>
        /// 取源图像R通道数据，并转化为8位灰度图像。
        /// </summary>
        /// <param name="original"> 源图像。 </param>
        /// <returns> 8位灰度图像。 </returns>
        private Bitmap RgbToR8(Bitmap original)
        {
            if (original != null)
            {
                // 将源图像内存区域锁定
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);

                // 获取图像参数
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度,比实际图片要大
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置的指针                
                int scanBytesLength = stride * height;  // 用stride宽度，表示这是内存区域的大小

                // 分别设置两个位置指针，指向源数组和目标数组
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytesLength];  // 为目标数组分配内存
                Marshal.Copy(ptr, rgbValues, 0, scanBytesLength);  // 将图像数据拷贝到rgbValues中
                // 分配灰度数组
                byte[] grayValues = new byte[width * height]; // 不含未用空间。
                                                              // 计算灰度数组

                byte blue, green, red;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        blue = rgbValues[posScan];
                        green = rgbValues[posScan + 1];
                        red = rgbValues[posScan + 2];
                        grayValues[posDst] = red;
                        posScan += 3;
                        posDst++;

                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                    posScan += offset;
                }

                // 内存解锁
                Marshal.Copy(rgbValues, 0, ptr, scanBytesLength);
                original.UnlockBits(bmpData);  // 解锁内存区域

                // 构建8位灰度位图
                Bitmap retBitmap = BuiltGrayBitmap(grayValues, width, height);
                return retBitmap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 彩色图转灰度图(只支持32位和24位)
        /// </summary>
        /// <param name="bmprgb"></param>
        /// <returns></returns>
        private Bitmap RgbToR8Ex(Bitmap bmprgb)
        {
            if (bmprgb == null) return null;

            #region 只处理32位或者24位转灰度图
            if (bmprgb.PixelFormat != PixelFormat.Format32bppArgb && bmprgb.PixelFormat != PixelFormat.Format24bppRgb)
            {
                return null;
            }
            #endregion

            //图像深度
            int deep = bmprgb.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            #region 转灰度图
            // 将源图像内存区域锁定
            BitmapData bmpData = bmprgb.LockBits(new Rectangle(0, 0, bmprgb.Width, bmprgb.Height), ImageLockMode.ReadOnly,
                    bmprgb.PixelFormat);

            // 获取图像参数
            int width = bmpData.Width;
            int height = bmpData.Height;
            int stride = bmpData.Stride;  // 扫描线的宽度,比实际图片要大
            int offset = stride - width * deep;  // 显示宽度与扫描线宽度的间隙

            //灰度图
            Bitmap bmpGray = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bitmapDataDst = bmpGray.LockBits(new Rectangle(0, 0, bmpGray.Width, bmpGray.Height), ImageLockMode.WriteOnly, bmpGray.PixelFormat);

            int offsetDst = bitmapDataDst.Stride - bmpGray.Width;
            unsafe
            {
                byte* ptrSrc = (byte*)bmpData.Scan0;   // 获取bmpData的内存起始位置的指针   
                byte* ptrDst = (byte*)bitmapDataDst.Scan0;
                byte blue, green, red;

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        blue = ptrSrc[0];
                        green = ptrSrc[1];
                        red = ptrSrc[2];


                        // 计算灰度值（使用常见的加权平均值方法）  
                        byte gray = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));
                        *ptrDst = gray;

                        ptrSrc += deep;
                        ptrDst++;
                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                    ptrSrc += offset;
                    ptrDst += offsetDst;
                }
            }

            bmprgb.UnlockBits(bmpData);  // 解锁内存区域
            bmpGray.UnlockBits(bitmapDataDst);// 解锁内存区域
            #endregion

            #region 灰度图调色
            // 修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette palette;
            // 获取一个Format8bppIndexed格式图像的Palette对象
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 修改生成位图的索引表
            bmpGray.Palette = palette;
            #endregion

            return bmpGray;

        }


        /// <summary>
        /// 用灰度数组新建一个8位灰度图像。
        /// </summary>
        /// <param name="rawValues"> 灰度数组(length = width * height)。 </param>
        /// <param name="width"> 图像宽度。 </param>
        /// <param name="height"> 图像高度。 </param>
        /// <returns> 新建的8位灰度位图。 </returns>
        private Bitmap BuiltGrayBitmap(byte[] rawValues, int width, int height)
        {
            // 新建一个8位灰度位图，并锁定内存区域操作
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                 ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // 计算图像参数
            int offset = bmpData.Stride - bmpData.Width;        // 计算每行未用空间字节数
            IntPtr ptr = bmpData.Scan0;                         // 获取首地址
            int scanBytes = bmpData.Stride * bmpData.Height;    // 图像字节数 = 扫描字节数 * 高度
            byte[] grayValues = new byte[scanBytes];            // 为图像数据分配内存

            // 为图像数据赋值
            int posSrc = 0, posScan = 0;                        // rawValues和grayValues的索引
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    grayValues[posScan++] = rawValues[posSrc++];
                }
                // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel
                posScan += offset;
            }

            // 内存解锁
            Marshal.Copy(grayValues, 0, ptr, scanBytes);
            bitmap.UnlockBits(bmpData);  // 解锁内存区域

            // 修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette palette;
            // 获取一个Format8bppIndexed格式图像的Palette对象
            using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                palette = bmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            // 修改生成位图的索引表
            bitmap.Palette = palette;

            return bitmap;
        }

        /// <summary>
        /// Bitmap转换成 MVS 图像
        /// </summary>
        /// <param name="bmp">Bitmap 图像</param>
        /// <returns></returns>
        private ImageBaseData BitmapToMvsImage(Bitmap bmp)
        {
            VM.PlatformSDKCS.ImageBaseData imageInout = new VM.PlatformSDKCS.ImageBaseData();
            byte[] bits = GetBitmapBytes(bmp);
            imageInout.DataLen = (uint)bits.Length;// (uint)(bmp.Height * bmp.Width * (uint)3);
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                imageInout.Pixelformat = 17301505;
            else if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                imageInout.Pixelformat = 35127316;
            else
                imageInout.Pixelformat = (int)bmp.PixelFormat;
            imageInout.ImageData = bits;
            imageInout.Width = bmp.Width;
            imageInout.Height = bmp.Height;

            return imageInout;
        }

        /// <summary>
        /// 图像转换成Byte[]
        /// </summary>
        /// <param name="bitmap">图像</param>
        /// <returns></returns>
        private byte[] GetBitmapBytes(Bitmap bitmap)
        {
            // 锁定位图的像素区域
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                    ImageLockMode.ReadOnly,
                                                    bitmap.PixelFormat);
            try
            {
                // 计算数组的长度
                int bytesLength = bitmapData.Stride * bitmap.Height;
                byte[] bytes = new byte[bytesLength];

                // 拷贝像素数据到字节数组
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bytes, 0, bytesLength);

                return bytes;
            }
            finally
            {
                // 解锁位图像素区域
                bitmap.UnlockBits(bitmapData);
            }
        }



        /// <summary>
        /// Bitmap转图像源SDK输入（ImageBaseData）
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
        private ImageBaseData BitmapToImageBaseData(Bitmap bmpInputImg)
        {
            ImageBaseData imageBaseData = new ImageBaseData();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度

                int offset = bmData.Stride - bmData.Width;

                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;
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
                imageBaseData = new ImageBaseData(_ImageBaseDataBufferBytes, (uint)ImageBaseDataSize, bmData.Width, bmData.Height, (int)VMPixelFormat.VM_PIXEL_MONO_08);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;
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
                imageBaseData = new ImageBaseData(_ImageBaseDataBufferBytes, (uint)ImageBaseDataSize, bmData.Width, bmData.Height, (int)VMPixelFormat.VM_PIXEL_RGB24_C3);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定
            return imageBaseData;
        }


        /// <summary>
        /// Bitmap转模块输入（InputImageData）
        /// </summary>
        /// <param name="bmpInputImg"></param>
        /// <returns></returns>
        private InputImageData BitmapToInputImageData(Bitmap bmpInputImg)
        {
            InputImageData inputImageData = new InputImageData();
            System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
            BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定
            inputImageData.Names.DataName = "InImage";//只能使用默认名称InImage
            inputImageData.Names.HeightName = "InImageHeight";//默认InImageHeight
            inputImageData.Names.WidthName = "InImageWidth";//默认InImageWidth
            inputImageData.Names.PixelFormatName = "InImagePixelFormat";//默认InImagePixelFormat
            inputImageData.Width = bmData.Width;
            inputImageData.Height = bmData.Height;
            if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0; int ImageBaseDataIndex = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width; j++)
                    {
                        _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
                    }
                    bitmapIndex += offset;
                }
                inputImageData.Pixelformat = ImagePixelFormat.IMAGE_PIXEL_FORMAT_MONO8;
                inputImageData.DataLen = (uint)ImageBaseDataSize;
                inputImageData.Data = Marshal.AllocHGlobal(ImageBaseDataSize);//inputImageData.Data需要申请内存;
                Marshal.Copy(_ImageBaseDataBufferBytes, 0, inputImageData.Data, _ImageBaseDataBufferBytes.Length);
            }
            else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
                int offset = bmData.Stride - bmData.Width * 3;
                Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;//imageBaseData_V2图像真正的缓存长度
                byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
                byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
                Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
                int bitmapIndex = 0; int ImageBaseDataIndex = 0;
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
                inputImageData.Pixelformat = ImagePixelFormat.IMAGE_PIXEL_FORMAT_RGB24;
                inputImageData.DataLen = (uint)ImageBaseDataSize;
                inputImageData.Data = Marshal.AllocHGlobal(ImageBaseDataSize);//inputImageData.Data需要申请内存;
                Marshal.Copy(_ImageBaseDataBufferBytes, 0, inputImageData.Data, _ImageBaseDataBufferBytes.Length);
            }
            bmpInputImg.UnlockBits(bmData);  // 解除锁定   
            return inputImageData;
        }

        #endregion


        #region Bitmap与算子（CmvdImage） 互转
        ///// <summary>
        //       /// Bitmap与算子（CmvdImage） 互转
        //       /// </summary>
        //       /// <param name="bmpInputImg"></param>
        //       /// <returns></returns>
        //       public CMvdImage BitmapToCMvdImage(Bitmap bmpInputImg)
        //       {
        //           CMvdImage cMvdImage = new CMvdImage();
        //           System.Drawing.Imaging.PixelFormat bitPixelFormat = bmpInputImg.PixelFormat;
        //           BitmapData bmData = bmpInputImg.LockBits(new Rectangle(0, 0, bmpInputImg.Width, bmpInputImg.Height), ImageLockMode.ReadOnly, bitPixelFormat);//锁定

        //           if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
        //           {
        //               Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
        //               int offset = bmData.Stride - bmData.Width;
        //               Int32 ImageBaseDataSize = bmData.Width * bmData.Height;
        //               byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
        //               byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
        //               Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
        //               int bitmapIndex = 0;
        //               int ImageBaseDataIndex = 0;
        //               for (int i = 0; i < bmData.Height; i++)
        //               {
        //                   for (int j = 0; j < bmData.Width; j++)
        //                   {
        //                       _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex++];
        //                   }
        //                   bitmapIndex += offset;
        //               }
        //               MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
        //               stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width;
        //               stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
        //               stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
        //               stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
        //               cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08, stImageData);
        //           }
        //           else if (bitPixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
        //           {
        //               Int32 bitmapDataSize = bmData.Stride * bmData.Height;//bitmap图像缓存长度
        //               int offset = bmData.Stride - bmData.Width * 3;
        //               Int32 ImageBaseDataSize = bmData.Width * bmData.Height * 3;
        //               byte[] _BitImageBufferBytes = new byte[bitmapDataSize];
        //               byte[] _ImageBaseDataBufferBytes = new byte[ImageBaseDataSize];
        //               Marshal.Copy(bmData.Scan0, _BitImageBufferBytes, 0, bitmapDataSize);
        //               int bitmapIndex = 0;
        //               int ImageBaseDataIndex = 0;
        //               for (int i = 0; i < bmData.Height; i++)
        //               {
        //                   for (int j = 0; j < bmData.Width; j++)
        //                   {
        //                       _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 2];
        //                       _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex + 1];
        //                       _ImageBaseDataBufferBytes[ImageBaseDataIndex++] = _BitImageBufferBytes[bitmapIndex];
        //                       bitmapIndex += 3;
        //                   }
        //                   bitmapIndex += offset;
        //               }
        //               MVD_IMAGE_DATA_INFO stImageData = new MVD_IMAGE_DATA_INFO();
        //               stImageData.stDataChannel[0].nRowStep = (uint)bmData.Width * 3;
        //               stImageData.stDataChannel[0].nLen = (uint)ImageBaseDataSize;
        //               stImageData.stDataChannel[0].nSize = (uint)ImageBaseDataSize;
        //               stImageData.stDataChannel[0].arrDataBytes = _ImageBaseDataBufferBytes;
        //               cMvdImage.InitImage((uint)bmData.Width, (uint)bmData.Height, MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3, stImageData);
        //           }
        //           bmpInputImg.UnlockBits(bmData);  // 解除锁定
        //           return cMvdImage;
        //       }

        //       public Bitmap CMvdImageToBitmap(CMvdImage cMvdImage)
        //       {
        //           Bitmap bmpInputImg = null;
        //           byte[] buffer = new byte[cMvdImage.GetImageData(0).arrDataBytes.Length];
        //           buffer = cMvdImage.GetImageData(0).arrDataBytes;

        //           if (MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08 == cMvdImage.PixelFormat)
        //           {
        //               Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
        //               Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
        //               System.Drawing.Imaging.PixelFormat bitMaPixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        //               bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
        //               int offset = imageWidth % 4 != 0 ? (4 - imageWidth % 4) : 0;//添加冗余位，变成4的倍数
        //               int strid = imageWidth + offset;
        //               int bitmapBytesLenth = strid * imageHeight;
        //               byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
        //               for (int i = 0; i < imageHeight; i++)
        //               {
        //                   for (int j = 0; j < strid; j++)
        //                   {
        //                       int bitIndex = i * strid + j;
        //                       int mvdIndex = i * imageWidth + j;
        //                       if (j >= imageWidth)
        //                       {
        //                           bitmapDataBytes[bitIndex] = 0;//冗余位填充0
        //                       }
        //                       else
        //                       {
        //                           bitmapDataBytes[bitIndex] = buffer[mvdIndex];
        //                       }
        //                   }
        //               }
        //               BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
        //               IntPtr imageBufferPtr = bitmapData.Scan0;
        //               Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
        //               bmpInputImg.UnlockBits(bitmapData);

        //               var colorPalettes = bmpInputImg.Palette;
        //               for (int j = 0; j < 256; j++)
        //               {
        //                   colorPalettes.Entries[j] = Color.FromArgb(j, j, j);
        //               }
        //               bmpInputImg.Palette = colorPalettes;
        //           }
        //           else if (MVD_PIXEL_FORMAT.MVD_PIXEL_RGB_RGB24_C3 == cMvdImage.PixelFormat)
        //           {
        //               Int32 imageWidth = Convert.ToInt32(cMvdImage.Width);
        //               Int32 imageHeight = Convert.ToInt32(cMvdImage.Height);
        //               System.Drawing.Imaging.PixelFormat bitMaPixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //               bmpInputImg = new Bitmap(imageWidth, imageHeight, bitMaPixelFormat);
        //               int offset = imageWidth % 4 != 0 ? (4 - (imageWidth * 3) % 4) : 0;//添加冗余位，变成4的倍数
        //               int strid = imageWidth * 3 + offset;
        //               int bitmapBytesLenth = strid * imageHeight;
        //               byte[] bitmapDataBytes = new byte[bitmapBytesLenth];
        //               for (int i = 0; i < imageHeight; i++)
        //               {
        //                   for (int j = 0; j < imageWidth; j++)
        //                   {
        //                       int mvdIndex = i * imageWidth * 3 + j * 3;
        //                       int bitIndex = i * strid + j * 3;
        //                       bitmapDataBytes[bitIndex] = buffer[mvdIndex + 2];
        //                       bitmapDataBytes[bitIndex + 1] = buffer[mvdIndex + 1];
        //                       bitmapDataBytes[bitIndex + 2] = buffer[mvdIndex];
        //                   }
        //                   for (int k = 0; k < offset; k++)
        //                   {
        //                       bitmapDataBytes[i * strid + imageWidth * 3 + k] = 0;
        //                   }
        //               }
        //               BitmapData bitmapData = bmpInputImg.LockBits(new Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.WriteOnly, bitMaPixelFormat);
        //               IntPtr imageBufferPtr = bitmapData.Scan0;
        //               Marshal.Copy(bitmapDataBytes, 0, imageBufferPtr, bitmapBytesLenth);
        //               bmpInputImg.UnlockBits(bitmapData);
        //           }
        //           return bmpInputImg;
        //       }

        #endregion

    }
}
