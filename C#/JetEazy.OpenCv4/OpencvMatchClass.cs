//using JetEazy.BasicSpace;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace JetEazy.BasicSpace
{
    public class OpencvMatchClass
    {
        public OpencvMatchClass()
        {
        }
        public void FindSimilarEx(Bitmap bmpinput, Bitmap bmppattern, float tolerance, List<DoffsetClass> doffsetlist)
        {
            //// 定义输入图像路径和模板图像路径
            //string srcImgPath = "C:/Users/CGW/Desktop/digits/0.1.jpg";
            //string tempImgPath = "C:/Users/CGW/Desktop/digits/0.4.jpg";

            //// 读取输入图像和模板图像
            //Mat srcImage = Cv2.ImRead(srcImgPath, ImreadModes.AnyColor);
            //Mat tempImage = Cv2.ImRead(tempImgPath, ImreadModes.AnyColor);

            Mat srcImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpinput);
            Mat tempImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmppattern);

            // 创建用于存储模板和匹配结果的图像
            Mat result = new Mat();

            // 使用模板匹配方法进行匹配，这里使用归一化相关系数匹配法
            Cv2.MatchTemplate(srcImage, tempImage, result, TemplateMatchModes.CCoeffNormed);

            //        for y in range(len(result)):
            //for x in range(len(result[y])):
            //    if result[y][x] > 0.952:
            //        cv2.rectangle(img, (x, y), (x + weight, y + height), (0, 0, 255, 2))


            // 获取匹配结果中的最小值、最大值以及对应的位置
            double minVal, maxVal;
            OpenCvSharp.Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);

            // 获取最佳匹配的位置
            OpenCvSharp.Point matchLoc = maxLoc;

            // 在输入图像上用绿色矩形框标记匹配的位置
            Cv2.Rectangle(srcImage, matchLoc, new OpenCvSharp.Point(matchLoc.X + tempImage.Cols, matchLoc.Y + tempImage.Rows), Scalar.Green, 2);

            // 显示模板和匹配结果的图像
            Cv2.ImShow("模板", tempImage);
            Cv2.ImShow("Matched Result", srcImage);

            Bitmap bmpMatched = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(srcImage);
            bmpMatched.Save("D:\\_tmp\\bmpMatched.png", System.Drawing.Imaging.ImageFormat.Png);

            Cv2.WaitKey(0);

            // 释放图像对象，关闭显示窗口
            srcImage.Release();
            tempImage.Release();
            result.Release();
            Cv2.DestroyAllWindows();
            Cv2.DestroyAllWindows();

        }
        public Bitmap Recoganize(Bitmap bmpinput, Bitmap bmppattern, double threshold = 0.5, double compressed = 0.5, string name = "target", int space = 10)
        {
            DateTime beginTime = DateTime.Now;            //获取开始时间  
            // 新建图变量并分配内存
            Mat srcImg = new Mat();
            // 读取要被匹配的图像
            srcImg = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpinput);
            // 更改尺寸
            Cv2.Resize(srcImg, srcImg, new OpenCvSharp.Size((int)srcImg.Cols * compressed, (int)srcImg.Rows * compressed));
            // 初始化保存保存匹配结果的横纵坐标列表
            List<int> Xlist = new List<int> { };
            List<int> Ylist = new List<int> { };

            int order = 0;
            Mat tempImg = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmppattern);
            Cv2.Resize(tempImg, tempImg, new OpenCvSharp.Size((int)tempImg.Cols * compressed, (int)tempImg.Rows * compressed));
            Mat result = srcImg.Clone();

            int dstImg_rows = srcImg.Rows - tempImg.Rows + 1;
            int dstImg_cols = srcImg.Cols - tempImg.Cols + 1;
            Mat dstImg = new Mat(dstImg_rows, dstImg_cols, MatType.CV_32F, 1);
            Cv2.MatchTemplate(srcImg, tempImg, dstImg, TemplateMatchModes.CCoeffNormed);

            int count = 0;
            for (int i = 0; i < dstImg_rows; i++)
            {
                for (int j = 0; j < dstImg_cols; j++)
                {
                    float matchValue = dstImg.At<float>(i, j);
                    if (matchValue >= threshold && Xlist.Count == 0)
                    {
                        count++;
                        Cv2.Rectangle(result, new Rect(j, i, tempImg.Width, tempImg.Height), new Scalar(0, 255, 0), 2);
                        Cv2.PutText(result, name, new OpenCvSharp.Point(j, i - (int)20 * compressed), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 1);
                        Xlist.Add(j);
                        Ylist.Add(i);
                    }

                    if (matchValue >= threshold && Xlist.Count != 0)
                    {
                        for (int q = 0; q < Xlist.Count; q++)
                        {
                            if (Math.Abs(j - Xlist[q]) + Math.Abs(i - Ylist[q]) < space)
                            {
                                order = 1;
                                break;
                            }
                        }
                        if (order != 1)
                        {
                            count++;
                            Cv2.Rectangle(result, new Rect(j, i, tempImg.Width, tempImg.Height), new Scalar(0, 255, 0), 2);
                            Cv2.PutText(result, name, new OpenCvSharp.Point(j, i - (int)20 * compressed), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 1);
                            Xlist.Add(j);
                            Ylist.Add(i);
                        }
                        order = 0;
                    }
                }
            }
            Console.WriteLine("目标数量:{0}", count);
            DateTime endTime = DateTime.Now;              //获取结束时间  
            TimeSpan oTime = endTime.Subtract(beginTime); //求时间差的函数  
            Console.WriteLine("程序的运行时间：{0} 毫秒", oTime.TotalMilliseconds);
            return result.ToBitmap();
        }
        public Bitmap Recoganize(Bitmap bmpinput, Bitmap bmppattern, List<DoffsetClass> doffsetlist, double threshold = 0.7, double compressed = 0.5, string name = "target", int space = 100)
        {
            DateTime beginTime = DateTime.Now;            //获取开始时间  
            // 新建图变量并分配内存
            Mat srcImg = new Mat();
            // 读取要被匹配的图像
            srcImg = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpinput);
            // 更改尺寸
            Cv2.Resize(srcImg, srcImg, new OpenCvSharp.Size((int)srcImg.Cols * compressed, (int)srcImg.Rows * compressed));
            // 初始化保存保存匹配结果的横纵坐标列表
            List<int> Xlist = new List<int> { };
            List<int> Ylist = new List<int> { };

            int order = 0;
            Mat tempImg = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmppattern);
            Cv2.Resize(tempImg, tempImg, new OpenCvSharp.Size((int)tempImg.Cols * compressed, (int)tempImg.Rows * compressed));
            Mat result = srcImg.Clone();

            int dstImg_rows = srcImg.Rows - tempImg.Rows + 1;
            int dstImg_cols = srcImg.Cols - tempImg.Cols + 1;
            Mat dstImg = new Mat(dstImg_rows, dstImg_cols, MatType.CV_32F, 1);
            Cv2.MatchTemplate(srcImg, tempImg, dstImg, TemplateMatchModes.CCoeffNormed);

            int count = 0;
            for (int i = 0; i < dstImg_rows; i++)
            {
                for (int j = 0; j < dstImg_cols; j++)
                {
                    float matchValue = dstImg.At<float>(i, j);
                    if (matchValue >= threshold && Xlist.Count == 0)
                    {
                        count++;
                        Cv2.Rectangle(result, new Rect(j, i, tempImg.Width, tempImg.Height), new Scalar(0, 255, 0), 2);
                        //Cv2.PutText(result, name, new OpenCvSharp.Point(j, i - (int)20 * compressed), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 1);
                        Xlist.Add(j);
                        Ylist.Add(i);

                        DoffsetClass doffset = new DoffsetClass(0, new PointF(j / (float)compressed + tempImg.Width / (float)compressed / 2,
                                                                                                                i / (float)compressed + tempImg.Height / (float)compressed / 2));
                        doffsetlist.Add(doffset);
                    }

                    if (matchValue >= threshold && Xlist.Count != 0)
                    {
                        for (int q = 0; q < Xlist.Count; q++)
                        {
                            if (Math.Abs(j - Xlist[q]) + Math.Abs(i - Ylist[q]) < (space * (float)compressed))
                            {
                                order = 1;
                                break;
                            }
                        }
                        if (order != 1)
                        {
                            count++;
                            Cv2.Rectangle(result, new Rect(j, i, tempImg.Width, tempImg.Height), new Scalar(0, 255, 0), 2);
                            //Cv2.PutText(result, name, new OpenCvSharp.Point(j, i - (int)20 * compressed), HersheyFonts.HersheySimplex, 0.5, new Scalar(0, 0, 0), 1);
                            Xlist.Add(j);
                            Ylist.Add(i);

                            DoffsetClass doffset = new DoffsetClass(0, new PointF(j / (float)compressed + tempImg.Width / (float)compressed / 2,
                                                                                                             i / (float)compressed + tempImg.Height / (float)compressed / 2));
                            doffsetlist.Add(doffset);
                        }
                        order = 0;
                    }
                }
            }
            Console.WriteLine("目标数量:{0}", count);
            DateTime endTime = DateTime.Now;              //获取结束时间  
            TimeSpan oTime = endTime.Subtract(beginTime); //求时间差的函数  
            Console.WriteLine("程序的运行时间：{0} 毫秒", oTime.TotalMilliseconds);
            return result.ToBitmap();
            //return new Bitmap(1, 1);
        }

    }
}
