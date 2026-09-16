
#define DVP2

#if DVP2
using DVPCameraType;
using JetEazy.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JetEazy.ControlSpace
{
    public class Dvp2Class
    {
        //private string _configFilename = "dvp2Config.ini";

        private uint m_handle = 0;
        private IntPtr m_ptr_wnd = new IntPtr();
        private IntPtr m_ptr = new IntPtr();

        // 显示参数
        private Stopwatch m_Stopwatch = new Stopwatch();
        private Double m_dfDisplayCount = 0;

        private string m_SerialNumber = string.Empty;
        private bool m_GetImageOK;
        private Bitmap m_bmpCurrent;
        private int m_Rotate = 0;
        private string m_Dvp2ConfigPath = string.Empty;

        public Dvp2Class(System.Windows.Forms.PictureBox pbx, int icam_no)
        {
            m_ptr_wnd = pbx.Handle;
        }
        public Dvp2Class()
        {
            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox();
            m_ptr_wnd = pictureBox.Handle;
        }
        public int Init(string eSerialNumberStr, string eDvp2ConfigPath = "")
        {
            m_SerialNumber = eSerialNumberStr;
            m_Dvp2ConfigPath = eDvp2ConfigPath;
            int nRet = -1;
            //DeviceListAcq();
            nRet = openDevice();
            if (nRet == 0)
                StartGrab();

            return nRet;
        }
        public void Dispose()
        {
            StopGrab();
            closeDevice();
        }

        public void TriggerMode(bool etrigger)
        {
            if (IsValidHandle(m_handle))
            {
                dvpStatus status = new dvpStatus();

                //打开/关闭相机触发模式
                status = DVPCamera.dvpSetTriggerState(m_handle, etrigger);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            }
        }
        public void SetExposure(float fexposure)
        {
            if (IsValidHandle(m_handle))
            {
                dvpStatus status = new dvpStatus();
                dvpDoubleDescr expoDescr = new dvpDoubleDescr();
                status = DVPCamera.dvpGetExposureDescr(m_handle, ref expoDescr);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    if (fexposure >= expoDescr.fMax || fexposure <= expoDescr.fMin)
                        return;

                    //打开/关闭相机触发模式
                    status = DVPCamera.dvpSetExposure(m_handle, fexposure);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                }
            }
        }
        public void SetGain(float fgain)
        {
            if (IsValidHandle(m_handle))
            {
                dvpStatus status = new dvpStatus();
                dvpFloatDescr expoDescr = new dvpFloatDescr();
                status = DVPCamera.dvpGetAnalogGainDescr(m_handle, ref expoDescr);
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    if (fgain >= expoDescr.fMax || fgain <= expoDescr.fMin)
                        return;

                    //打开/关闭相机触发模式
                    status = DVPCamera.dvpSetAnalogGain(m_handle, fgain);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                }
            }
        }
        public void TriggerSoftwareX()
        {
            m_GetImageOK = false;
            if (IsValidHandle(m_handle))
            {
                //一旦执行这个函数就相当于生成一个外部触发器
                //注意:如果曝光时间过长，点击“发送软触发信号”太快可能会导致触发失败
                //因为前一帧可能处于连续曝光或输出不完全的状态
                dvpStatus status = DVPCamera.dvpTriggerFire(m_handle);
                
            }
        }

        #region
        /* MASK 20231108 GetFrame
         * 
        //public Bitmap GetImageNow()
        //{
        //    TriggerSoftwareX();
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //    dvpStatus status;
        //    for (; ; )
        //    {
        //        IntPtr pBuffer = new IntPtr();
        //        dvpFrame refFrame = new dvpFrame();

        //        if (true)
        //        {
        //            bool bSoftTrigger = false;

        //            status = DVPCamera.dvpGetTriggerState(m_handle, ref bSoftTrigger);
        //            // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
        //            if (bSoftTrigger)
        //            {
        //                bool bSoftTriggerLoop = false;
        //                status = DVPCamera.dvpGetSoftTriggerLoopState(m_handle, ref bSoftTriggerLoop);
        //                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
        //                if (!bSoftTriggerLoop)
        //                {
        //                    status = DVPCamera.dvpTriggerFire(m_handle);
        //                    if (status != dvpStatus.DVP_STATUS_OK)
        //                    {
        //                        // Trigger failure maybe result from that the trigger signal interval is too dense.
        //                        // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
        //                        continue;
        //                    }
        //                }
        //            }
        //        }

        //        // Grab a frame image from the video stream and timeout should not less than the current exposure time.
        //        status = DVPCamera.dvpGetFrame(m_handle, ref refFrame, ref pBuffer, ((uint)(1000)));
        //        if (status != dvpStatus.DVP_STATUS_OK)
        //        {
        //            // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
        //            continue;
        //        }

        //        //转换BMP位图
        //        Bitmap bmp = null;
        //        //Bitmap bmp1 = null;
        //        if (refFrame.format == dvpImageFormat.FORMAT_BGR24)
        //        {
        //            bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);

        //        }
        //        else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
        //        {
        //            //ColorPalette tempPalette;
        //            bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);

        //            tempPalette = bmp.Palette;
        //            for (int i = 0; i < 256; i++)
        //            {
        //                tempPalette.Entries[i] = System.Drawing.Color.FromArgb(0, i, i, i);
        //            }
        //            bmp.Palette = tempPalette;
        //        }

        //        switch (m_Rotate)
        //        {
        //            case 90:
        //                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
        //                break;
        //            case 180:
        //                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
        //                break;
        //            case 270:
        //                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
        //                break;
        //        }

        //        m_bmpCurrent = bmp;
        //        m_GetImageOK = true;
        //        bmp.Save("D:\\TEST.BMP");
        //        //string _pathfilename = "D:\\TEST.BMP";

        //        //status = DVPCamera.dvpSavePicture(ref refFrame, pBuffer, _pathfilename, 100);
        //        if (m_GetImageOK)
        //        {
        //            if (m_bmpCurrent == null)
        //                return null;
        //            return m_bmpCurrent.Clone() as Bitmap;
        //        }
        //        if (watch.ElapsedMilliseconds > 1000)
        //            break;
        //    }
        //    return null;
        //}
        */
        #endregion

        public Bitmap GetImageNow()
        {
            TriggerSoftwareX();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (; ; )
            {
                if (m_GetImageOK)
                {
                    if (m_bmpCurrent == null)
                        return null;

                    //return new Bitmap(m_bmpCurrent);

                    return m_bmpCurrent.Clone() as Bitmap;
                }
                if (watch.ElapsedMilliseconds > 1000)
                    break;
            }
            return null;
        }
        public int Rotate
        {
            get { return m_Rotate; }
            set { m_Rotate = value; }
        }

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
                            status = DVPCamera.dvpRegisterStreamCallback(m_handle, _proc, dvpStreamEvent.STREAM_EVENT_PROCESSED, m_ptr);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }

                        //dvpTriggerSource triSource = (dvpTriggerSource)InputIOCombo.SelectedIndex;
                        //dvpStatus status = dvpStatus.DVP_STATUS_DESCR_FAULT;
                        //status = DVPCamera.dvpSetTriggerSource(m_handle, 
                        //    dvpTriggerSource.TRIGGER_SOURCE_LINE1);
                        //TriggerMode(false);
                        status = DVPCamera.dvpSetTriggerSource(m_handle,
                            dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                        TriggerMode(true);
                        nRet = 0;
                    }
                }
            }
            return nRet;
        }
        private void closeDevice()
        {

            dvpStatus status = dvpStatus.DVP_STATUS_OK;
            status = DVPCamera.dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE1);
            TriggerMode(false);
            //关闭相机
            status = DVPCamera.dvpClose(m_handle);
            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            m_handle = 0;
            //pictureBox.Refresh();
        }

        private void StartGrab()
        {
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
        private void StopGrab()
        {
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

        private DVPCamera.dvpStreamCallback _proc;
        private ColorPalette tempPalette;
        //回调函数接收相机图像数据
        private int _dvpStreamCallback(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {
            bool bDisplay = false;

            if (m_dfDisplayCount == 0)
            {
                m_Stopwatch.Restart();
                bDisplay = true;
            }
            else
            {
                if (m_Stopwatch.ElapsedMilliseconds - (long)(m_dfDisplayCount * 33.3f) >= 33)
                {
                    bDisplay = true;
                }
            }

            if (bDisplay)
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

                //转换BMP位图
                Bitmap bmp = null;
                //Bitmap bmp1 = null;
                if (refFrame.format == dvpImageFormat.FORMAT_BGR24 || refFrame.format == dvpImageFormat.FORMAT_RGB24)
                {
                    bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth * 3, PixelFormat.Format24bppRgb, pBuffer);

                }
                else if (refFrame.format == dvpImageFormat.FORMAT_MONO)
                {
                    //ColorPalette tempPalette;
                    bmp = new Bitmap(refFrame.iWidth, refFrame.iHeight, refFrame.iWidth, PixelFormat.Format8bppIndexed, pBuffer);

                    tempPalette = bmp.Palette;
                    for (int i = 0; i < 256; i++)
                    {

                        //这里的A值 填写255  很奇怪 其它相机都是0 
                        tempPalette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                    }
                    bmp.Palette = tempPalette;
                }

                switch (m_Rotate)
                {
                    case 90:
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 180:
                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 270:
                        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }

                m_bmpCurrent = bmp;
                m_GetImageOK = true;

                //保存图像

                //bool triggerstatus = false;
                //status = DVPCamera.dvpGetTriggerState(handle, ref triggerstatus);
                //if (triggerstatus == true)
                //{
                //    string path = SavePath.Text + "\\" + FileName.Text + "_" + count++.ToString() + "." + PictureSytle.Text;
                //    status = DVPCamera.dvpSavePicture(ref refFrame, pBuffer, path, 100);
                //}
            }
            return 1;
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
        public static void DeviceListAcq(string eFilename="dvpConfig.ini")
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







    }
}

#endif