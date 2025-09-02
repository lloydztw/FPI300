using DVPCameraType;
using FreeImageAPI;
using JetEazy.ControlSpace;
using JetEazy.Interface;
using OpenCvSharp.Flann;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThridLibray;
using static JetEazy.CCDSpace.CameraPara;

namespace JetEazy.CCDSpace.CamLinkDriver
{
    public class Linescan_Dvp2 : IxLineScanCam
    {

        #region PRIVATE VAR

        private string _configFilename = "dvp2Config.ini";

        private uint m_handle = 0;
        private IntPtr m_ptr_wnd = new IntPtr();
        private IntPtr m_ptr = new IntPtr();

        // 显示参数
        private Stopwatch m_Stopwatch = new Stopwatch();
        private Double m_dfDisplayCount = 0;

        private string m_SerialNumber = string.Empty;
        //private bool m_GetImageOK;
        private Bitmap m_bmpCurrent;
        //private int m_Rotate = 0;
        private string m_Dvp2ConfigPath = string.Empty;
        bool m_TriggerOK = false;
        bool m_IsDebug = false;
        bool m_TriggerComplete = false;
        //string m_TrigCtl = "ISO";
        string m_OperateMessage = string.Empty;
        CameraPara _camCfg = new CameraPara();

        #endregion

        public void Init(bool debug, string inipara)
        {
            m_IsDebug = debug;
            _camCfg.FromCameraString(inipara);
        }
        public bool IsSim()
        {
            return m_IsDebug;
        }
        public bool Open()
        {
            return Open("BASE");
        }
        public bool Open(string configFile)
        {
            bool bOK = false;

            if (m_IsDebug)
                return true;

            try
            {
                m_TriggerOK = false;

                bOK = _Init(_camCfg.SerialNumber, _camCfg.CfgPath) == 0;
            }
            catch (Exception ex)
            {
                bOK = false;
            }

            return bOK;
        }
        public bool Close()
        {
            bool bOK = false;
            if (m_IsDebug)
                return true;
            try
            {
                m_TriggerOK = false;

                StopGrab();
                closeDevice();

                bOK = true;
            }
            catch (Exception ex)
            {
                bOK = false;
            }
            return bOK;
        }
        public void SoftTrigger()
        {
            if (m_IsDebug)
                return;
            m_TriggerOK = false;
        }
        public bool IsGrapImageOK
        {
            get { return m_TriggerOK; }
            set { m_TriggerOK = value; }
        }
        public bool IsGrapImageComplete
        {
            get { return m_TriggerComplete; }
            set { 
                m_TriggerComplete = value;
                //if (!value)
                //    EncoderReset();
            }
        }
        public string OperateShowMessage
        {
            get { return m_OperateMessage; }
            set { m_OperateMessage = value; }
        }
        public void EncoderReset()
        {
            //if (m_IsDebug)
            //    return;
            //dvpStatus status = dvpStatus.DVP_STATUS_UNKNOW;
            //if (IsValidHandle(m_handle))
            //{
            //    status = DVPCamera.dvpSetCommandValue(m_handle, "EncoderReset");
            //    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            //}
        }
        public void ShowSetup()
        {
            if (m_IsDebug)
                return;
            if (IsValidHandle(m_handle))
            {
                dvpStatus status = DVPCamera.dvpShowPropertyModalDialog(m_handle, new IntPtr());
            }
        }
        /// <summary>
        /// 获取图像
        /// </summary>
        /// <param name="size">大于0 放大倍数 等于0 不变尺寸 小于0缩小倍数</param>
        /// <returns>返回图像</returns>
        public Bitmap GetPageBitmap(int size)
        {
            if (m_IsDebug)
                return new Bitmap(1, 1);

            //dvpStatus status = dvpStatus.DVP_STATUS_UNKNOW;
            //if (IsValidHandle(m_handle))
            //{
            //    status = DVPCamera.dvpSetCommandValue(m_handle, "EncoderReset");
            //    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            //}

            if (m_bmpCurrent == null)
            {
                m_TriggerOK = false;
                return null;
            }

            //缩小一倍

            if (m_TriggerOK)
            {
                Bitmap bmpLine = new Bitmap(1, 1);
                bmpLine.Dispose();
                //bmpLine = ConvertToImage(bmpDatas);
                bmpLine = (Bitmap)m_bmpCurrent.Clone();
                //if (size == 0)
                //{
                //    bmpLine = (Bitmap)m_bmpCurrent.Clone();
                //    return bmpLine;
                //}
                //if (size > 0)
                //    bmpLine = new Bitmap(m_bmpCurrent, m_bmpCurrent.Width << size, m_bmpCurrent.Height << size);
                //else
                //    bmpLine = new Bitmap(m_bmpCurrent, m_bmpCurrent.Width >> size, m_bmpCurrent.Height >> size);

                return bmpLine;
            }
            m_TriggerOK = false;
            return null;
        }
        public void Dispose()
        {
            Close();
        }

        private int _Init(string eSerialNumberStr, string eDvp2ConfigPath = "")
        {
            m_SerialNumber = eSerialNumberStr;
            m_Dvp2ConfigPath = eDvp2ConfigPath;
            int nRet = -1;
            //DeviceListAcq();
            nRet = openDevice();
            //if (nRet == 0)
            //    StartGrab();

            return nRet;
        }

        #region PRIVATE FUNTION

        private int openDevice()
        {
            int nRet = -1;
            dvpStatus status = dvpStatus.DVP_STATUS_OK;


            if (!IsValidHandle(m_handle))
            {
                if (!string.IsNullOrEmpty(m_SerialNumber))
                {
                    int _index = GetDeviceNumber(m_SerialNumber);
                    status = DVPCamera.dvpOpen((uint)_index, dvpOpenMode.OPEN_NORMAL, ref m_handle);
                    
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        if (System.IO.Directory.Exists(m_Dvp2ConfigPath))
                        {
                            dvpCameraInfo dev_info = new dvpCameraInfo();
                            status = DVPCamera.dvpEnum((uint)_index, ref dev_info);

                            //status = DVPCamera.dvpLoadConfig(m_handle, m_Dvp2ConfigPath + "\\DS" + m_SerialNumber + ".ini");
                            status = DVPCamera.dvpLoadConfig(m_handle, m_Dvp2ConfigPath + "\\" + dev_info.FriendlyName + ".ini");
                            if (status != dvpStatus.DVP_STATUS_OK)
                                DVPCamera.dvpLoadDefault(m_handle);

                        }
                        else
                        {
                            dvpBufferConfig dvpBuffer = new dvpBufferConfig();
                            dvpBuffer.bDropNew = true;
                            dvpBuffer.uQueueSize = 33;
                            dvpBuffer.mode = dvpBufferMode.BUFFER_MODE_NEWEST;
                            DVPCamera.dvpSetBufferConfig(m_handle, dvpBuffer);
                            DVPCamera.dvpSetBufferQueueSize(m_handle, 33);
                        }
                    }

                    //if (UserDefinedName.Checked)
                    //{
                    //    //按照选定的用户定义名称打开指定的相机
                    //    status = DVPCamera.dvpOpenByUserId(DevNameCombo.Text, dvpOpenMode.OPEN_NORMAL, ref m_handle);
                    //}
                    //else
                    //{
                    //    //按照选定的友好名称打开指定的相机
                    //    status = DVPCamera.dvpOpenByName(DevNameCombo.Text, dvpOpenMode.OPEN_NORMAL, ref m_handle);
                    //}

                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
                        //MessageBox.Show("Open the device failed!");
                        nRet = -1;
                    }
                    else
                    {
                        //m_strFriendlyName = DevNameCombo.Text;

                        ////如果需要显示图像，用户需要注册一个回调函数，并在注册的回调函数中完成绘图操作
                        ////注意:在回调函数中绘图可能会对使用“dvpGetFrame”获取图像数据产生一些延迟
                        _proc = _dvpStreamCallback;
                        using (Process curProcess = Process.GetCurrentProcess())
                        using (ProcessModule curModule = curProcess.MainModule)
                        {
                            status = DVPCamera.dvpRegisterStreamCallback(m_handle, _proc, dvpStreamEvent.STREAM_EVENT_FRAME_THREAD, m_ptr);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }

                        //dvpTriggerSource triSource = (dvpTriggerSource)InputIOCombo.SelectedIndex;
                        //dvpStatus status = dvpStatus.DVP_STATUS_DESCR_FAULT;
                        //status = DVPCamera.dvpSetTriggerSource(m_handle, 
                        //    dvpTriggerSource.TRIGGER_SOURCE_LINE1);
                        //TriggerMode(false);
                        //status = DVPCamera.dvpSetTriggerSource(m_handle,
                        //    dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                        //TriggerMode(true);
                        nRet = 0;
                    }
                }
            }
            return nRet;
        }
        private void closeDevice()
        {

            dvpStatus status = dvpStatus.DVP_STATUS_OK;
            //status = DVPCamera.dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE1);
            //TriggerMode(false);
            //关闭相机
            status = DVPCamera.dvpClose(m_handle);
            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            m_handle = 0;
            //pictureBox.Refresh();
        }

        public void StartGrab()
        {
            if (m_IsDebug)
                return;
            //初始化显示计数为0
            m_dfDisplayCount = 0;

            if (IsValidHandle(m_handle))
            {
                dvpStreamState state = new dvpStreamState();
                dvpStatus status;

                //根据当前视频状态用一个按钮实现启动和停止视频流
                status = DVPCamera.dvpGetStreamState(m_handle, ref state);

                if (state == dvpStreamState.STATE_STOPED)
                    status = DVPCamera.dvpStart(m_handle);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            }
        }
        public void StopGrab()
        {
            if (m_IsDebug)
                return;
            dvpStatus status = dvpStatus.DVP_STATUS_OK;
            //检查相机视频流的状态
            dvpStreamState StreamState = new dvpStreamState();
            status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            if (StreamState == dvpStreamState.STATE_STARTED)
            {
                ////初始化显示数量为0
                m_dfDisplayCount = 0;

                //停止视频流
                status = DVPCamera.dvpStop(m_handle);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

            }
        }



        private static Mutex imageMutex = new Mutex();
        private DVPCamera.dvpStreamCallback _proc;
        private ColorPalette tempPalette;
        //Bitmap bmp = new Bitmap(1, 1);
        //byte[] bmpDatas = null;
        //回调函数接收相机图像数据
        FreeImageAPI.FreeImageBitmap bmp = null;// new FreeImageBitmap(1, 1);
        private int _dvpStreamCallback(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {

            m_TriggerComplete = false;
            if (_camCfg.IsFly)
            {
                CameraFrame camera = new CameraFrame();
                if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                    camera.Format = PixelFormat.Format8bppIndexed;
                camera.uBytes = refFrame.uBytes;
                camera.iWidth = refFrame.iWidth;
                camera.iHeight = refFrame.iHeight;
                FireTrigger(camera, pBuffer);

                m_TriggerOK = true;
                m_TriggerComplete = true;
                return 0;
            }


            m_dfDisplayCount++;

            try
            {
                if (bmp != null)
                    bmp.Dispose();

                imageMutex.WaitOne();
                //FreeImageAPI.FreeImageBitmap bmp = null;

                if (refFrame.format == dvpImageFormat.FORMAT_BGR24 || refFrame.format == dvpImageFormat.FORMAT_RGB24)
                {
                    bmp = new FreeImageAPI.FreeImageBitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);

                }
                else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                {
                    bmp = new FreeImageAPI.FreeImageBitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);
                    //m_Buffer = pBuffer;
                    //m_Width = refFrame.iWidth;
                    //m_Height = refFrame.iHeight;
                }
                bmp.Rotate(_camCfg.Rotate);
                //m_bmpCurrent = bmp.ToBitmap();
                m_TriggerOK = true;
            }
            finally
            {
                imageMutex.ReleaseMutex();
            }

            m_TriggerComplete = true;
            return 0;
        }

        public event LineTriggerHandler LineTriggerAction;
        public void FireTrigger(CameraFrame cameraFrame, IntPtr pBuffer)
        {
            if (LineTriggerAction != null)
            {
                LineTriggerAction(cameraFrame, pBuffer);
            }
        }


        private int _dvpStreamCallback_LineTrigger(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {
            int index = (int)pContext;

            ////if (m_ptr_wnd != IntPtr.Zero)
            ////    DVPCamera.dvpDrawPicture(ref refFrame, pBuffer, m_ptr_wnd, (IntPtr)0, (IntPtr)0);
            //_event.
            //CollectList.Add(stopwatch.ElapsedMilliseconds);
            //stopwatch.Restart();

            //if (myIndex >= myListTemp.Count)
            //    myIndex = 0;

            //myListTemp[myIndex] = new byte[refFrame.uBytes];
            //// 将指针 ptr 指向的数据复制到 copiedArray
            //Marshal.Copy(pBuffer, myListTemp[myIndex], 0, myListTemp[myIndex].Length);
            //myIndex++;
            #region MASK_20240407_NO_USE
#if dvp_no_use
            if (myIndex > 490)
                myIndex = 0;
            //lock (obj)
            {
                if (refFrame.format == dvpImageFormat.FORMAT_BGR24 || refFrame.format == dvpImageFormat.FORMAT_RGB24)
                {
                    //myCollectList[myIndex].Dispose();
                    myCollectList[myIndex] = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);
                    //listImage.Add(new Bitmap(bmp));//22ms
                    listImage.Add(myCollectList[myIndex]);
                    //listImage.Add((Bitmap)bmp.Clone());
                    //bmp.Dispose();
                    myIndex++;

                    //myTimetamp = myStopwatch.ElapsedMilliseconds / 1000.0;
                }
                else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                {
                    //myCollectList[myIndex].Dispose();
                    myCollectList[myIndex] = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);

                    ColorPalette tempPalette = myCollectList[myIndex].Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        tempPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                    }
                    myCollectList[myIndex].Palette = tempPalette;

                    //listImage.Add(new Bitmap(bmp));
                    listImage.Add(myCollectList[myIndex]);
                    //listImage.Add((Bitmap)bmp.Clone());
                    //bmp.Dispose();
                    myIndex++;

                    //myTimetamp = myStopwatch.ElapsedMilliseconds / 1000.0;
                }
            }
#endif
            #endregion

            return 0;
        }
        private int _dvpStreamCallback1(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {
            bool bDisplay = true;
            m_TriggerComplete = false;
            //if (m_dfDisplayCount == 0)
            //{
            //    m_Stopwatch.Restart();
            //    bDisplay = true;
            //}
            //else
            //{
            //    if (m_Stopwatch.ElapsedMilliseconds - (long)(m_dfDisplayCount * 33.3f) >= 33)
            //    {
            //        bDisplay = true;
            //    }
            //}

            //if (bDisplay)
            {
                m_dfDisplayCount++;

                //它演示了通常的视频绘制，不建议在回调函数中花费更长的时间操作
                //为了避免影响帧率和图像采集的实时性
                //所获得的图像数据只有在函数返回之前有效，所以缓冲区指针不应该被传递出去
                //但是用户可以malloc内存和复制图像数据
                //dvpStatus status = DVPCamera.dvpDrawPicture(ref refFrame, pBuffer,
                //    m_ptr_wnd, (IntPtr)0, (IntPtr)0);
                //Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                //status = DVPCamera.dvpGetFrame(ref refFrame, pBuffer,
                //    m_ptr_wnd, (IntPtr)0, (IntPtr)0);
                try
                {
                    imageMutex.WaitOne();
                    //转换BMP位图
                    //Bitmap bmp = null;
                    FreeImageAPI.FreeImageBitmap bmp = null;
                    //Bitmap bmp1 = null;
                    //m_bmpCurrent = null;
                    //bmp.Dispose();
                    //bmpDatas = null;
                    //if (refFrame.format == dvpImageFormat.FORMAT_BGR24 || refFrame.format == dvpImageFormat.FORMAT_RGB24)
                    //{
                    //    bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);

                    //}
                    //else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                    //{
                    //    //ColorPalette tempPalette;
                    //    bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);

                    //    tempPalette = bmp.Palette;
                    //    for (int i = 0; i < 256; i++)
                    //    {
                    //        //tempPalette.Entries[i] = System.Drawing.Color.FromArgb(0, i, i, i);
                    //        ////这里的A值 填写255  很奇怪 其它相机都是0 
                    //        tempPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                    //    }
                    //    bmp.Palette = tempPalette;
                    //}
                    bmp = new FreeImageAPI.FreeImageBitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);
                    if (_camCfg.Rotate != 0)
                        bmp.Rotate(_camCfg.Rotate);
                    m_bmpCurrent = bmp.ToBitmap();
                    //m_bmpCurrent = (Bitmap)bmp.Clone();

                    //m_bmpCurrent = (Bitmap)bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format8bppIndexed);
                    //bmpDatas = ConvertToMemory(m_bmpCurrent);

                    //switch (m_Rotate)
                    //{
                    //    case 90:
                    //        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    //        break;
                    //    case 180:
                    //        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    //        break;
                    //    case 270:
                    //        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    //        break;
                    //}

                    //m_bmpCurrent = (Bitmap)bmp.Clone();
                    m_TriggerOK = true;
                }
                finally
                {
                    imageMutex.ReleaseMutex();
                }
                
                //保存图像

                //bool triggerstatus = false;
                //status = DVPCamera.dvpGetTriggerState(handle, ref triggerstatus);
                //if (triggerstatus == true)
                //{
                //    string path = SavePath.Text + "\\" + FileName.Text + "_" + count++.ToString() + "." + PictureSytle.Text;
                //    status = DVPCamera.dvpSavePicture(ref refFrame, pBuffer, path, 100);
                //}
            }
            //else
            //{
            //    m_TriggerOK = false;
            //}
            m_TriggerComplete = true;
            return 0;
        }

        /// <summary>
        /// 判读句柄是否有效
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool IsValidHandle(uint handle)
        {
            bool bValidHandle = false;
            dvpStatus status = DVPCamera.dvpIsValid(handle, ref bValidHandle);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                return bValidHandle;
            }

            return false;
        }
        /// <summary>
        /// 通过相机序列号 对应相机的编号
        /// </summary>
        /// <param name="strSerialNumber">相机序列号</param>
        /// <returns></returns>
        private int GetDeviceNumber(string strSerialNumber)
        {
            int iNumber = -1;

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            dvpStatus status;
            uint i, n = 0;
            dvpCameraInfo dev_info = new dvpCameraInfo();

            //获取连接到计算机上的相机数量
            status = DVPCamera.dvpRefresh(ref n);
            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            //m_n_dev_count = (int)n;
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                //m_CamCount = 0;

                for (i = 0; i < n; i++)
                {
                    //逐个获取每个相机的信息
                    status = DVPCamera.dvpEnum(i, ref dev_info);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        //m_info[m_CamCount] = dev_info;
                        //m_CamCount++;
                        if (dev_info.SerialNumber == strSerialNumber)
                        {
                            iNumber = (int)i;
                            break;
                        }
                    }
                }
            }


            return iNumber;
        }
        public static void DeviceListAcq(string eFilename = "dvpConfig.ini")
        {

            string CCDSEQStr = eFilename;
            string Str = "";

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();

            dvpStatus status;
            uint i, n = 0;
            dvpCameraInfo dev_info = new dvpCameraInfo();

            //获取连接到计算机上的相机数量
            status = DVPCamera.dvpRefresh(ref n);
            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            //m_n_dev_count = (int)n;
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                //m_CamCount = 0;

                for (i = 0; i < n; i++)
                {
                    //逐个获取每个相机的信息
                    status = DVPCamera.dvpEnum(i, ref dev_info);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        //m_info[m_CamCount] = dev_info;
                        Str += dev_info.SerialNumber + ",0" + Environment.NewLine;
                        //m_CamCount++;
                    }
                }
            }

            if (!System.IO.File.Exists(CCDSEQStr))
            {
                Str = RemoveLastChar(Str, 2);
                SaveData(Str, CCDSEQStr);
            }
        }
        static string RemoveLastChar(string Str, int Count)
        {
            if (Str.Length < Count)
                return "";

            return Str.Remove(Str.Length - Count, Count);
        }
        static void SaveData(string DataStr, string FileName)
        {
            StreamWriter Swr = new StreamWriter(FileName, false, Encoding.Default);

            Swr.Write(DataStr);

            Swr.Flush();
            Swr.Close();
            Swr.Dispose();
        }

        // 图片转换为内存缓存
        public static byte[] ConvertToMemory(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, bitmap);
            byte[] buffer = stream.ToArray();
            return buffer;
        }
        // 共享内存缓存转换为图片
        public static Bitmap ConvertToImage(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            Bitmap bitmap = (Bitmap)formatter.Deserialize(stream);
            return bitmap;
        }

        public FreeImageBitmap GetFreeImageBitmap(int size = 0)
        {
            if (m_IsDebug)
                return new FreeImageBitmap(1, 1);
            if (bmp == null)
            {
                m_TriggerOK = false;
                return null;
            }
            if (m_TriggerOK)
            {
                return bmp;
            }
            m_TriggerOK = false;
            return null;
        }

        #endregion

    }
}
