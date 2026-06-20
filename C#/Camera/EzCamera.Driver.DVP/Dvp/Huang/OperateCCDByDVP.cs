using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
//using System.Windows.Forms;
using DVPCameraType;
//using LensInspection.GlobalSpace;
//using LensInspection.UniversalSpace;
//using Sunny.UI;

namespace EzCamera.Driver.Dvp.Huang
{
    /// <summary>
    /// 操作度申相机
    /// </summary>
    public class OperateCCDByDVP
    {
        /// <summary>
        /// 显示异常信息
        /// </summary>
        public event Action<string, MsgType> OnShowMsg=null;

        /// <summary>
        /// 相机获得图像,触发其他动作(标定的时候使用)
        /// </summary>
        public event Action<int>OnGetBmpHandler=null;


        /// <summary>
        /// 相机个数
        /// </summary>
        public uint deviceCount = 0;
        /// <summary>
        /// 相机的句柄
        /// </summary>
        private  uint[]handles=null;

        /// <summary>
        /// 设备信息
        /// </summary>
        private dvpCameraInfo[] DeviceInfo = null;
        /// <summary>
        /// 注册回调函数句柄
        /// </summary>
        private  IntPtr m_ptr = new IntPtr();
        /// <summary>
        /// 相机图像
        /// </summary>
        public  Bitmap[] CCDBmp = null;

        /// <summary>
        /// 显示图像张数
        /// </summary>
        public  int[] m_dfDisplayCount = null;

        private DVPCamera.dvpStreamCallback _proc;
        /// <summary>
        /// 显示图片控件句柄
        /// </summary>
        private IntPtr[] showPicHandle = null;
        /// <summary>
        /// 显示图片句柄
        /// </summary>
        public IntPtr showPic = IntPtr.Zero;

        /// <summary>
        /// 相机实际序列号
        /// </summary>
        public Dictionary<int,string>ccdSerial =new Dictionary<int,string>();
        /// <summary>
        /// 相机在文件中序列号
        /// </summary>
        private string[]ccdFileSerial=null;

#if(false)
        /// <summary>
        /// 收集度申图片
        /// 要记录图片ID
        /// 马达信息等等
        /// </summary>
        public ConcurrentQueue<CheckBmpInfo> listImage =
            new ConcurrentQueue<CheckBmpInfo> ();
#endif

        /// <summary>
        /// 空白图用来测试
        /// </summary>
        private Bitmap blankBmp= null;
        /// <summary>
        /// 添加一个黑色样品
        /// </summary>
        private Bitmap blankBmpAddSample=null;


        /// <summary>
        /// 记录标定编号
        /// </summary>
        public List<int> calibrationIndexList = new List<int>();


        /// <summary>
        ///  操作度申相机
        /// </summary>
        /// <param name="intPtr">外部显示图片控件句柄,
        /// 例如PictureBox.Handle的句柄数组
        /// </param>
        public OperateCCDByDVP(IntPtr [] picHandle)
        {
            showPicHandle = picHandle;
        }
        /// <summary>
        /// 操作度申相机
        /// </summary>
        public OperateCCDByDVP()
        {
            //生成空白图
            blankBmp = new Bitmap( Universal.CheckCCDWidth, Universal.CheckCCDHeight);
            using(Graphics g = Graphics.FromImage(blankBmp))
            {
                g.Clear(Color.White);
            }


            //生成空白图,添加一些矩形,用作检测
            blankBmpAddSample = new Bitmap( Universal.CheckCCDWidth, Universal.CheckCCDHeight);

            List<Rectangle> rectList = new List<Rectangle>();
            int col = 40;
            int row = 40;
            int rectWidth = 10;
           int colWidth= blankBmpAddSample.Width / col;
            int rowHeight = blankBmpAddSample.Height / row;
            for (int i = 1; i < 40; i++)
            {
                for(int j = 1; j < 40; j++)
                {

                    rectList.Add(new Rectangle(i*colWidth,j*rowHeight, rectWidth, rectWidth));
                }
                
            }
            using(Graphics g = Graphics.FromImage(blankBmpAddSample))
            {
                g.Clear(Color.White);
                g.FillRectangles(Brushes.Black, rectList.ToArray());
            }

            //blankBmp.Save("D:\\1.png");
            //blankBmpAddSample.Save("D:\\2.png");
        }
        /// <summary>
        /// 显示错误信息
        /// </summary>
        private void ErrorMsg(string msg, dvpStatus dvpStatus)
        {
            //MessageBoxIcon icon = dvpStatus > dvpStatus.DVP_STATUS_OK ? MessageBoxIcon.Warning : MessageBoxIcon.Error;
            MsgType temp = dvpStatus > dvpStatus.DVP_STATUS_OK ? MsgType.Warning : MsgType.Error;

            OnShowMsg?.Invoke($"{msg}\n错误原因: {GetStatusDesc(dvpStatus)}", temp);
        }


        /// <summary>
        /// 遍历相机(遍历相机个数为0时候算遍历失败)
        /// </summary>
        /// <returns>是否遍历成功</returns>
        private bool ErgodicDevice()
        {
            bool IsErgodicOk = false;
            dvpStatus dvpStatus = DVPCamera.dvpRefresh(ref deviceCount);
            if (dvpStatus == dvpStatus.DVP_STATUS_OK)
            {
                if (deviceCount > 0)
                {
                    //相机编号
                    ccdSerial.Clear();
                    //图像数组
                    CCDBmp =new Bitmap[deviceCount];
                    //设备数组
                    DeviceInfo = new dvpCameraInfo[deviceCount];
                    //相机累计采集张数数组
                    m_dfDisplayCount = new int[deviceCount];
                    for (uint i = 0; i < deviceCount; i++)
                    {
                        dvpCameraInfo cameraInfo = new dvpCameraInfo();
                        //获取设备信息
                        dvpStatus status = DVPCamera.dvpEnum(i, ref cameraInfo);
                        if (status == dvpStatus.DVP_STATUS_OK)
                        {
                            ccdSerial.Add((int)i,$"{cameraInfo.SerialNumber}@{cameraInfo.UserID}");

                            DeviceInfo[i] = cameraInfo;
                            IsErgodicOk = true;
                        }
                        else
                        {
                            IsErgodicOk = false;
                        }
                    }

                }
                else
                {
                    ErrorMsg("枚举设备数目为0", dvpStatus);
                }
            }
            else
            {
                ErrorMsg("枚举设备失败", dvpStatus);
            }
            return IsErgodicOk;
        }

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns></returns>
        public bool InitialDevice()
        {

            dvpStatus status = dvpStatus.DVP_STATUS_OK;
            //遍历相机
            if (!ErgodicDevice()) return false;

            //加载相机编号
            LoadCCDSerial();

            //相机的句柄
            handles = new uint[deviceCount];

            //相机索引
            int _ccdIndex = -1;

            for (int i = 0; i < DeviceInfo.Length; i++)
            {
                //相机索引必须在下列过程中获得值,不然初始化错误
                _ccdIndex = -1;

                //本地相机编号文件存在的时候,按照本地相机编号顺序给相机排序
                if (ccdFileSerial != null)
                {
                    for (int j = 0; j < ccdFileSerial.Length; j++)
                    {

                        if (!string.IsNullOrEmpty(ccdFileSerial[j]) && ccdFileSerial[j].Split('@')[0] == DeviceInfo[i].SerialNumber)
                        {
                            _ccdIndex = j;
                            break;
                        }

                    }
                }
                //本地没有相机编号文件
                else
                {
                    _ccdIndex = i;
                }


                //通过相机句柄开启相机
                // dvpStatus status = DVPCamera.dvpOpen(i, dvpOpenMode.OPEN_NORMAL, ref handles[i]);
                //通过相机默认名称开启相机,例如 M3S1207 - H - O2C@U016000000226
                status = DVPCamera.dvpOpenByName(DeviceInfo[_ccdIndex].FriendlyName, dvpOpenMode.OPEN_NORMAL, ref handles[i]);
                //通过相机用户自定义名开启相机
                //status = DVPCamera.dvpOpenByUserId(DeviceInfo[i].UserID, dvpOpenMode.OPEN_NORMAL, ref handles[i]);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    ErrorMsg($"打开相机_{DeviceInfo[i].FriendlyName} 失败", status);
                    return false;
                }

                //这里可以加载相机配置文档
                //LoadConfig(i,"相机配置文档的路径");

                //确保不被回收
                GCHandle.Alloc(_proc);

                _proc = _dvpStreamCallback;

                ///回调函数要绑定在主线程开启,不然会被垃圾回调函数处理掉
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    //为设备注册回调函数
                    status = DVPCamera.dvpRegisterStreamCallback(handles[i], _proc,
                        dvpStreamEvent.STREAM_EVENT_PROCESSED, m_ptr);
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        ErrorMsg($"为相机_{DeviceInfo[_ccdIndex].FriendlyName} 注册回调函数失败", status);
                        return false;
                    }
                }

            }


            //分别对相机进行设置
            for (int i = 0; i < DeviceInfo.Length; i++)
            {
                //相机缓存队列尺寸
                SetImageBuffer(i);
                //相机图片缓存模式
                SetImageBufferMode(i);
                //设置触发模式(具体是软触发还是线触发)
                SetTriggerMode(i);
                //开启采集
                StartGrab(i);
            }
            return true;

        }

        /// <summary>
        /// 加载相机编号
        /// 如果本地文件存在,其内容中相机个数与编号要与实物对应上,
        /// 否则提示用户修改本地文件
        /// </summary>
        private void LoadCCDSerial()
        {
            string dvpFilePath = $"{Universal.DirRoot}\\DVP.txt";
            if (File.Exists(dvpFilePath))
            {
                using (FileStream fs = new FileStream(dvpFilePath, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(content))
                    {
                        ccdFileSerial = content.Replace(Environment.NewLine, "#").Split('#');
                    }
                }
                //本地相机编号内容中相机个数与编号要与实物对应上
                if(ccdFileSerial.Length>ccdSerial.Count)
                {
                    //MessageBox.Show($"{dvpFilePath}中相机个数比实际相机个数多,请去除多余相机编号,或者删除该文件(由程序自己生成)并重启程序");
                    Environment.Exit(0);
                }
                else if(ccdFileSerial.Length < ccdSerial.Count)
                {
                    //MessageBox.Show($"{dvpFilePath}中相机个数比实际相机个数少,请添加缺失相机编号,或者删除该文件(由程序自己生成)并重启程序");
                    Environment.Exit(0);
                }
                else
                {
                    //本地文件与实际相机个数相等,检查文件中相机编号是否有实际相机对应
                    for (int i = 0; i < ccdSerial.Count; i++)
                    {

                       if(!ccdSerial.Values.Contains(ccdFileSerial[i]))
                        {
                            //MessageBox.Show($"实物不存在编号为:{ccdFileSerial[i].Split('@')[0]}度申相机,请检查文件{dvpFilePath},或者删除该文件(由程序自己生成)并重启程序");
                            Environment.Exit(0);
                            break;
                        }
                    }
                }
            }
            else
            {
                //将当前现有相机的序列号保存在本地
                if (ccdSerial != null)
                {
                    string _ccdSerialFilePath = $"{Universal.DirRoot}\\DVP.txt";

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ccdSerial.Count; i++)
                    {
                        if (i != ccdSerial.Count - 1)
                            sb.Append($"{ccdSerial[i]}{Environment.NewLine}");
                        else
                            sb.Append(ccdSerial[i]);
                    }
                    using (FileStream fs = new FileStream(_ccdSerialFilePath, FileMode.OpenOrCreate))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(sb.ToString());
                    }

                    //warn 这里要提示相机顺序可能要人为排序,如果相机只有一个则忽略
                    if (ccdSerial.Count > 1)
                    {
                        //MessageBox.Show($"{_ccdSerialFilePath}{Environment.NewLine}相机序列号为机器自动生成,请检查相机是否要重新手动排序,{Environment.NewLine}" +
                        //    $"手动排序后并重启程序!");
                    }

                }
            }
        }


        /// <summary>
        /// 相机图片缓存模式
        /// </summary>
        /// <param name="CCDIndex"></param>
        private void SetImageBufferMode(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpBufferConfig dvpBufferConfig = new dvpBufferConfig();
                //最新帧输出，旧帧将被覆盖 
                dvpBufferConfig.mode = dvpBufferMode.BUFFER_MODE_NEWEST;
                dvpStatus status = DVPCamera.dvpSetBufferConfig(handle, dvpBufferConfig);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置相机缓存模式失败", status);
                }
            }
        }

        /// <summary>
        /// 当前取像ID,每次重新测试产品之前要复位
        /// </summary>
        public int currentImageID = 0;
        /// <summary>
        /// 当前Z轴ID,每次重新测试产品之前要复位
        /// </summary>
        public int currentImageZIndex = 0;

        /// <summary>
        /// 当前测试参数使用相机
        /// </summary>
        //>> public EnumDVPIndex enumDVPIndex=EnumDVPIndex.Lens;

        //记录两张图片间隔
        //private Stopwatch Stopwatch = new Stopwatch();
        ////两次取图之间耗时
        //public List<long>costTime= new List<long>();

        /// <summary>
        /// 回调函数接收相机图像数据
        /// </summary>
        /// <param name="handle">相机句柄(度申相机句柄第一个是1不是0)</param>
        /// <param name="_event">事件类型</param>
        /// <param name="pContext">用户指针</param>
        /// <param name="refFrame">帧信息</param>
        /// <param name="pBuffer">图像数据</param>
        /// <returns></returns>
        private int _dvpStreamCallback(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {
#if(false)
            int index = (int)(handle - 1);
            if (index == enumDVPIndex.GetHashCode())
            {
                //采集累计张数
                //m_dfDisplayCount[0]++;

                //if (showPic != IntPtr.Zero)
                //    //直接绑定控件句柄进行刷新
                //    DVPCamera.dvpDrawPicture(ref refFrame, pBuffer, showPic, (IntPtr)0, (IntPtr)0);

                //做标定
                if (Universal.testParamerClass.IsCalibration)
                {
                    int nextIndex = getImageID(currentImageID + 1);
                    if (nextIndex != -1)
                    {
                        //calibrationIndexList.Add(nextIndex);
                        //拿到当前图像索引的下一张有效图像索引
                        OnGetBmpHandler?.Invoke(nextIndex);
                    }
                }

                if (refFrame.format == dvpImageFormat.FORMAT_BGR24 || refFrame.format == dvpImageFormat.FORMAT_RGB24)
                {
                    int imageId = getImageID(currentImageID);
                    if (imageId != -1)
                    {
                        CheckBmpInfo checkBmpInfo = new CheckBmpInfo();
                        checkBmpInfo.ID = imageId;
                        checkBmpInfo.ZIndex = currentImageZIndex;
                        checkBmpInfo.bmpCCD = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);

                        listImage.Enqueue(checkBmpInfo);
                    }
                }
                else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                {
                    #region 用来记录飞拍时候两张图片取像时间间隔
                    //costTime.Add(Stopwatch.ElapsedMilliseconds);
                    //Stopwatch.Restart();
                    #endregion

                    int imageId = getImageID(currentImageID);
                    //第一个成立条件:imageId=-1是要舍弃的图像,
                   
                    if (imageId != -1)
                    { 
                        //第二个成立条件:选择的Z轴深度的图片
                        if (Universal.testParamerClass.IsSaveAllLevelBmp ? true : Universal.testParamerClass.SelectImageIndex[imageId] == currentImageZIndex)
                        {
                            Bitmap bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight,
                                refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);

                            ColorPalette tempPalette = bmp.Palette;
                            for (int i = 0; i < 256; i++)
                            {
                                tempPalette.Entries[i] = Color.FromArgb(255, i, i, i);
                            }
                            bmp.Palette = tempPalette;
                            CheckBmpInfo checkBmpInfo = new CheckBmpInfo();
                            checkBmpInfo.ID = imageId;
                            checkBmpInfo.ZIndex = currentImageZIndex;
                            checkBmpInfo.bmpCCD = new Bitmap(bmp);
                            listImage.Enqueue(checkBmpInfo);
                            bmp.Dispose();


                            #region 模拟
                            //if (imageId == 28 || imageId == 43 || imageId == 39 || imageId == 30 || imageId == 13)
                            //if (imageId == 30)
                            // {

                            //     CheckBmpInfo checkBmpInfo = new CheckBmpInfo();
                            //     checkBmpInfo.ID = imageId;
                            //     checkBmpInfo.ZIndex = currentImageZIndex;
                            //     checkBmpInfo.bmpCCD = new Bitmap(blankBmpAddSample);
                            //     listImage.Enqueue(checkBmpInfo);
                            // }
                            // else
                            // {
                            //     CheckBmpInfo checkBmpInfo = new CheckBmpInfo();
                            //     checkBmpInfo.ID = imageId;
                            //     checkBmpInfo.ZIndex = currentImageZIndex;
                            //     checkBmpInfo.bmpCCD = new Bitmap(blankBmp);
                            //     listImage.Enqueue(checkBmpInfo);

                            // }
                            #endregion
                        }
                    }

                }

                currentImageID++;

            }
#endif
            return 0;
        }

        /// <summary>
        /// 获取图片ID
        /// 马达取像走S型路线,所以取像ID与图片真实ID不一致
        /// 每行结束那张图片要舍弃
        /// </summary>
        /// <param name="currentID"></param>
        /// <param name="XMoveStepCount"></param>
        /// <returns></returns>
        private int getImageID(int index)
        {
#if(false)
            int row = index / Universal.testParamerClass.X_Count;
            int col = index % Universal.testParamerClass.X_Count;

            int imageID ;
            if (row % 2 == 0)
            {
                if (col != Universal.testParamerClass.X_Count - 1)
                {
                    int temp = row * Universal.testParamerClass.X_Count + (Universal.testParamerClass.X_Count - col - 2);

                    imageID = temp;
                }
                else
                {

                    imageID = index;
                }
            }
            else
            {
                imageID = index;

            }

            if (imageID % Universal.testParamerClass.X_Count != (Universal.testParamerClass.X_Count-1))
            {
                int rowIndex = imageID / Universal.testParamerClass.X_Count;
                imageID -= rowIndex;
                return imageID;
            }
            else
            {
                //舍弃图片
                return -1;
            }
#endif
            return 0;
        }

       

        /// <summary>
        /// 相机采集张数
        /// 软触发开启时候会清零
        /// </summary>
        /// <returns></returns>
        public uint FrameCount(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpFrameCount count = new dvpFrameCount();
                dvpStatus status = DVPCamera.dvpGetFrameCount(handle, ref count);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    return count.uFrameCount;
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取相机采集张数失败", status);
                }

            }
            return 0;
        }
        /// <summary>
        /// 显示属性框
        /// </summary>
        /// <param name="handle"></param>
        public void ShowProperty(int CCDIndex,IntPtr mainHandle)
        {
         
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStatus status = DVPCamera.dvpShowPropertyModalDialog(handle, mainHandle);
                if (status != dvpStatus.DVP_STATUS_OK)
                {

                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}打开属性框设定失败", status);
                }
            }
        }


        /// <summary>
        /// 取得枚举描述
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        private string GetStatusDesc(Enum para)
        {
            string strValue = para.ToString();
            FieldInfo fieldInfo = para.GetType().GetField(strValue);
            Object[] objs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0)
            {
                return strValue;
            }
            else
            {
                var da = (DescriptionAttribute)objs[0];
                return da.Description;
            }
        }

        /// <summary>
        /// 加载相机设定
        /// </summary>
        /// <param name="CCDIndex">相机索引</param>
        /// <param name="ccdSetFilePath">相机设定档</param>
        public void LoadConfig(int CCDIndex, string ccdSetFilePath)
        {
         
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStatus status = DVPCamera.dvpLoadConfig(handle, ccdSetFilePath);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                  
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}加载相机设定失败", status);
                }
            }
        }
        /// <summary>
        /// 保存config
        /// </summary>
        /// <param name="CCDIndex">相机索引</param>
        /// <param name="ccdConfigSaveDir">相机当保存目录</param>
        public void SaveConfig(int CCDIndex, string ccdConfigSaveDir)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStatus status = DVPCamera.dvpSaveConfig(handle, $"{ccdConfigSaveDir}\\{DeviceInfo[CCDIndex].FriendlyName}.INI");
                if (status != dvpStatus.DVP_STATUS_OK)
                {
                  
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}保存相机设定失败", status);
                }
            }
        }


        /// <summary>
        /// 设置黑白模式
        /// </summary>
        /// <returns></returns>
        public bool SetMonoFormat(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStreamFormat dvpStreamFormat = new dvpStreamFormat();
                dvpStatus status = DVPCamera.dvpGetTargetFormat(handle, ref dvpStreamFormat);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    if (dvpStreamFormat == dvpStreamFormat.S_MONO8)
                    {
                        //相机原本就是灰度
                        return true;
                    }
                    else
                    {
                        status = DVPCamera.dvpSetTargetFormat(handle, dvpStreamFormat.S_MONO8);
                        if (dvpStatus.DVP_STATUS_OK == status)
                        {
                            return true;
                        }
                        else
                        {
                          
                            ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置相机输出为MONO8失败", status);
                        }
                    }

                }
                else
                {
                   
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取相机输出格式失败", status);
                }

            }
            return false;
        }
        /// <summary>
        /// 设置彩色模式
        /// </summary>
        /// <returns></returns>
        public bool SetRGBFormat(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStreamFormat dvpStreamFormat = new dvpStreamFormat();

                dvpStatus status = DVPCamera.dvpGetTargetFormat(handle, ref dvpStreamFormat);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    if (dvpStreamFormat == dvpStreamFormat.S_BGR24)
                    {
                        //相机原本就是彩色
                        return true;
                    }
                    else
                    {
                        status = DVPCamera.dvpSetTargetFormat(handle, dvpStreamFormat.S_BGR24);
                        if (dvpStatus.DVP_STATUS_OK == status)
                        {
                            return true;
                        }
                        else
                        {
                         
                            ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置相机输出为RGB24失败", status);
                        }
                    }

                }
                else
                {
                   
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取相机输出格式失败", status);
                }
            }
            return false;
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        /// <param name="handle"></param>
        private bool StartGrab(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStreamState StreamState = new dvpStreamState();
                dvpStatus status = DVPCamera.dvpGetStreamState(handle, ref StreamState);
                if (dvpStatus.DVP_STATUS_OK != status)
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取采集流状态失败", status);
                    return false;
                }
                //当前相机流状态停止就开启
                if (StreamState == dvpStreamState.STATE_STOPED)
                {
                    status = DVPCamera.dvpStart(handle);
                   if(dvpStatus.DVP_STATUS_OK==status)
                    {
                        return true;
                    }
                    else
                    {
                        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}开启采集流失败", status);
                    }
                }
                else
                {
                    //相机原本状态为实时采集中
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 停止采集
        /// </summary>
        /// <param name="handle"></param>
        private bool StopGrab(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStreamState StreamState = new dvpStreamState();
                dvpStatus status = DVPCamera.dvpGetStreamState(handle, ref StreamState);
                if (dvpStatus.DVP_STATUS_OK != status)
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取采集流状态失败", status);
                    return false;
                }
                //当前相机流状态开启就停止
                if (StreamState != dvpStreamState.STATE_STOPED)
                {
                    status = DVPCamera.dvpStop(handle);
                  if(dvpStatus.DVP_STATUS_OK==status)
                    {
                        return true;
                    }
                    else
                    {
                        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}停止采集流失败", status);
                    }
                }
                else
                {
                    //相机原本为停止采集状态
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 设置触发模式(触发源为线触发)
        /// </summary>
        public bool SetTriggerMode(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                bool triggerState = false;

                //获取相机触发状态
                dvpStatus status = DVPCamera.dvpGetTriggerState(handle, ref triggerState);

                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    if (!triggerState)
                    {
                        //更改采集模式前要先停止流采集
                        StopGrab(CCDIndex);

                        //将相机设置为触发状态
                        status= DVPCamera.dvpSetTriggerState(handle, true);

                        if (dvpStatus.DVP_STATUS_OK == status)
                        {
                            //设置触发源
                            if (SetLineTrigger(CCDIndex))
                            {
                                //更改采集模式后要开启流采集
                                StartGrab(CCDIndex);
                            }
                            else
                            {
                                ErrorMsg("设置触发源时报错", dvpStatus.DVP_STATUS_UNKNOW);
                            }

                        }
                        else
                        {
                            ErrorMsg("设置触发模式时报错", status);
                        }
                    }
                    else
                    {
                        //当前为触发状态,设置触发源为线触发
                        return SetLineTrigger(CCDIndex);
                    }
                }
                else
                {
                   
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取触发模式失败", status);
                }
            }

            return false;
        }

        /// <summary>
        /// 设置软触发源
        /// </summary>
        private bool SetSoftTrigger(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                //确保当前设备处在触发模式下
                dvpTriggerSource dvpTriggerSource = new dvpTriggerSource();
                //获得当前设备触发源
              dvpStatus status=  DVPCamera.dvpGetTriggerSource(handle, ref dvpTriggerSource);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    //当前设备触发源不为软触发
                    if (dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE != dvpTriggerSource)
                    {
                        if (dvpStatus.DVP_STATUS_OK == DVPCamera.dvpSetTriggerSource(handle, dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE))
                        {
                            return true;
                        }
                        else
                        {
                            ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置软触发源失败", status);
                        }
                    }
                    else
                    {
                        //当前设备触发源原本为软触发
                        return true;
                    }

                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取触发源失败", status);
                }

            }
            return false;
        }
        /// <summary>
        /// 设置线触发源
        /// </summary>
        private bool SetLineTrigger(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                //确保当前设备处在触发模式下
                dvpTriggerSource dvpTriggerSource = new dvpTriggerSource();
                //获得当前设备触发源
                dvpStatus status = DVPCamera.dvpGetTriggerSource(handle, ref dvpTriggerSource);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    //当前设备触发源不为线触发
                    if (dvpTriggerSource.TRIGGER_SOURCE_LINE1 != dvpTriggerSource)
                    {
                        if (dvpStatus.DVP_STATUS_OK == DVPCamera.dvpSetTriggerSource(handle, dvpTriggerSource.TRIGGER_SOURCE_LINE1))
                        {
                            return true;
                        }
                        else
                        {
                            ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置软触发源失败", status);
                        }
                    }
                    else
                    {
                        //当前设备触发源原本为软触发
                        return true;
                    }

                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取触发源失败", status);
                }

            }
            return false;
        }



        /// <summary>
        /// 图像缓存队列尺寸
        /// </summary>
        /// <param name="CCDIndex"></param>
        private void SetImageBuffer(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStatus status = DVPCamera.dvpSetBufferQueueSize(handle, 1);
                if (status != dvpStatus.DVP_STATUS_OK)
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置触发缓存队列失败", status);
            }
        }

        /// <summary>
        /// 设置连续采集模式
        /// </summary>
        public bool SetContinuousMode(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                bool triggerState = false;
                //获取相机触发状态
             dvpStatus status=   DVPCamera.dvpGetTriggerState(handle, ref triggerState);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    //当前设备为触发状态
                    if (triggerState)
                    {
                        //更改采集模式前要先停止流采集
                        StopGrab(CCDIndex);

                        if (dvpStatus.DVP_STATUS_OK == DVPCamera.dvpSetTriggerState(handle, false))
                        {
                            //更改采集模式后要开启流采集
                            StartGrab(CCDIndex);
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取触发状态失败", status);
                }
            }
            return false;
        }

        /// <summary>
        /// 触发一张图片
        /// </summary>
        /// <param name="handle"></param>
        public void Trigger(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                //触发前主动清空图像数组缓存
                ClearImage(CCDIndex);

                uint handle = handles[CCDIndex];
                dvpStatus status = DVPCamera.dvpTriggerFire(handle);
                if (status != dvpStatus.DVP_STATUS_OK)
                {
      
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}触发失败", status);
                }
            }

        }

        /// <summary>
        /// 清空图像缓存
        /// </summary>
        ///为了捕获System.AccessViolationException异常
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        private void ClearImage(int CCDIndex)
        {
            Monitor.Enter(Universal.CameraLock[CCDIndex]);
            try
            {
                CCDBmp[CCDIndex] = null;
            }
            catch (Exception ex)
            {
               
            }
            finally
            {
                Monitor.Exit(Universal.CameraLock[CCDIndex]);
            }
        }

        /// <summary>
        ///  设置曝光时间
        /// </summary>
        /// <param name="handle">相机句柄</param>
        /// <param name="fexposure">曝光值</param>
        /// <returns>是否设定成功</returns>
        public bool SetExposureTime(int CCDIndex,float fexposure)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpDoubleDescr expoDescr = new dvpDoubleDescr();

                dvpStatus status = DVPCamera.dvpGetExposureDescr(handle, ref expoDescr);

                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    //设定值超过上下限
                    if (fexposure >= expoDescr.fMax || fexposure <= expoDescr.fMin)
                        return false;

                    //打开/关闭相机触发模式
                    status = DVPCamera.dvpSetExposure(handle, fexposure);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {

                        return true;
                    }
                    else
                    {
                        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置曝光值失败", status);
                    }
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取曝光值失败", status);
                }

            
            }
            return false;
        }

        /// <summary>
        ///  获取最大曝光时间
        /// </summary>
        /// <param name="handle">相机句柄</param>
        /// <param name="fexposure">曝光值</param>
        /// <returns>获取失败返回-1</returns>
        public double GetMaxExposureTime(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpDoubleDescr expoDescr = new dvpDoubleDescr();

                dvpStatus status = DVPCamera.dvpGetExposureDescr(handle, ref expoDescr);

                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    //设定值超过上下限
                    return expoDescr.fMax - 1;
                     

                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取最大曝光值失败", status);
                }


            }
            return -1;
        }


        /// <summary>
        /// 获取相机曝光时间
        /// </summary>
        /// <param name="handle"></param>
        /// <returns>若返回值为负数,获取失败</returns>
        public double GetExposureTime(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpDoubleDescr expoDescr = new dvpDoubleDescr();
                dvpStatus status = DVPCamera.dvpGetExposureDescr(handle, ref expoDescr);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    return expoDescr.fDefault;
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取曝光值失败", status);
                }
            }
            return -1;
        }

        /// <summary>
        /// 设置增益值
        /// </summary>
        /// <param name="handle">相机句柄</param>
        /// <param name="fgain">增益值</param>
        /// <returns>是否设定成功</returns>
        public bool SetGain(int CCDIndex, float fgain)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStatus status = new dvpStatus();
                dvpFloatDescr expoDescr = new dvpFloatDescr();

                status = DVPCamera.dvpGetAnalogGainDescr(handle, ref expoDescr);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    if (fgain >= expoDescr.fMax || fgain <= expoDescr.fMin)
                        return false;

                    //打开/关闭相机触发模式
                    status = DVPCamera.dvpSetAnalogGain(handle, fgain);
                    if(status == dvpStatus.DVP_STATUS_OK)
                    {
                        return true;
                    }
                    else
                    {

                        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}设置增益值失败", status);
                    }
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取增益值失败", status);
                }
            }

            return false;
        }

      
        /// <summary>
        ///关闭指定设备
        /// </summary>
        private bool CloseDevice(int CCDIndex)
        {
            if (IsValidHandle(CCDIndex))
            {
                uint handle = handles[CCDIndex];
                dvpStreamState StreamState = new dvpStreamState();
                dvpStatus status = DVPCamera.dvpGetStreamState(handle, ref StreamState);
                if (dvpStatus.DVP_STATUS_OK == status)
                {
                    //当前设备在采集中
                    if (StreamState == dvpStreamState.STATE_STARTED)
                    {
                        //关掉采集流
                        if (dvpStatus.DVP_STATUS_OK != DVPCamera.dvpSetStreamState(handle, dvpStreamState.STATE_STOPED))
                        {
                            ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}关闭采集流失败", status);
                            return false;
                        }
                    }

                    //关闭相机句柄
                    if (dvpStatus.DVP_STATUS_OK == DVPCamera.dvpClose(handle))
                    {
                        return true;
                    }
                    else
                    {
                        ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}关闭失败", status);
                    }
                }
                else
                {
                    ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}获取相机采集状态失败", status);
                }
            }

            return false;
        }
        /// <summary>
        /// 关闭所有设备
        /// </summary>
        /// <returns></returns>
        public bool CloseAllDevice()
        {
            bool IsCloseOK = false;
            if (deviceCount > 0)
            {
                for (int i = 0; i < handles.Length; i++)
                {
                    IsCloseOK = CloseDevice(i);
                    if (!IsCloseOK)
                    {
                        break;
                    }
                }
            }

            return IsCloseOK;
        }


      /// <summary>
      /// 判断相机句柄是否合法
      /// </summary>
      /// <param name="CCDIndex">相机索引</param>
      /// <returns></returns>
        private bool IsValidHandle(int CCDIndex)
        {
            //传过来的相机索引不合法
            if (CCDIndex >= deviceCount||CCDIndex<0) return false;

            uint handle = handles[CCDIndex];
            bool bValidHandle = false;
            dvpStatus status = DVPCamera.dvpIsValid(handle, ref bValidHandle);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                return bValidHandle;
            }
            else
            {
                ErrorMsg($"相机{DeviceInfo[CCDIndex].FriendlyName}句柄不合法", status);
            }

            return false;
        }
    }


    /// <summary>
    /// 信息类型
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 警告
        /// </summary>
        Warning = 1,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 2,
    }
}
