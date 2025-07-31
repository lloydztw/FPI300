using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThridLibray;

namespace JetEazy.CCDSpace
{
    public class CAM_DAHUAVISON
    {
        List<IGrabbedRawData> m_frameList = new List<IGrabbedRawData>();        /* 图像缓存列表 */
        private string _DhDeviceIp = "127.0.0.1";
        PictureBox pbImage;

        Bitmap bmpTmpCapture = new Bitmap(1, 1);
        Bitmap[] m_Muilt_bmp = new Bitmap[6];

        Thread renderThread = null;         /* 显示线程  */
        bool m_bShowLoop = false;            /* 线程控制变量 */
        Mutex m_mutex = new Mutex();        /* 锁，保证多线程安全 */
        private Graphics _g = null;
        bool m_bShowByGDI;                  /* 是否使用GDI绘图 */
        bool m_sizechange = false;
        public bool Live = true;
        int m_ReTrigger = 0;

        int m_width = 3840;
        int m_height = 2764;
        int iCountLost = 0;//记录相机1s内掉帧的次数

        //多线程
        bool m_thread_muilt = true;
        bool m_thread_isrunning = false;

        int m_Muilt_Index = 0;

        private Graphics _gError = null;
        SolidBrush B = new SolidBrush(Color.Lime);
        Font MyFont = new Font("Arial", 8);
        //int iFPS = 0;
        //Stopwatch m_stopWatch_FPS = new Stopwatch();    /* FPS统计 */

        //Graphics g = Graphics.FromImage(BMP);
        //g.DrawString(Text, MyFont, B, new PointF(5, 5));
        //    g.Dispose();
        private CameraPara m_CameraPara = null;
        public int MuiltIndex
        {
            get { return m_Muilt_Index; }
            set { m_Muilt_Index = value; }
        }

        public bool TriggerLoop
        {
            get { return m_bShowLoop; }
        }

        public string DeviceIP
        {
            get { return _DhDeviceIp; }
            set { _DhDeviceIp = value; }
        }
        public bool pbxChange
        {
            set { m_sizechange = value; }
        }

        public CAM_DAHUAVISON(PictureBox pbx, int icam_no)
        {
            //picForMyCameraHandle = pbx;
            //m_number_no = icam_no;
            pbImage = pbx;
        }
        public int Init(CameraPara eCameraPara)
        {
            m_CameraPara = eCameraPara;
            //_DhDeviceIp = ip;
            int iret = OpenDeviceByKey(m_CameraPara.SerialNumber);
            if (iret == 0 && m_thread_muilt)
            {
                if (null == renderThread)
                {
                    m_thread_isrunning = true;
                    renderThread = new Thread(new ThreadStart(ShowThread));
                    renderThread.Name = "thread" + pbImage.Name;
                    renderThread.Start();

                    //m_stopWatch_FPS.Start();
                }
            }

            //ShowThread();
            //m_stopWatch.Start();


            return iret;
        }
        public void Dispose()
        {
            CloseDevice();

            if (m_dev != null)
            {
                m_dev.Dispose();
                m_dev = null;
            }

            m_thread_isrunning = false;
            m_bShowLoop = false;
            try
            {
                if (m_thread_muilt)
                {
                    if (renderThread != null)
                    {
                        renderThread.Join();

                        renderThread = null;
                    }
                }
            }
            catch
            {
                if (m_thread_muilt)
                {
                    if (renderThread != null)
                    {
                        renderThread.Abort();
                        renderThread = null;
                    }
                }
            }

            if (_g != null)
            {
                _g.Dispose();
                _g = null;
            }
        }
        private int OpenDevice(string m_ip)
        {
            try
            {
                /* 设备搜索 */
                List<IDeviceInfo> li = Enumerator.EnumerateDevices();
                if (li.Count > 0)
                {
                    /* 获取搜索到的第一个设备 */
                    //m_dev = Enumerator.GetDeviceByIndex(0);
                    m_dev = Enumerator.GetDeviceByGigeIP(m_ip);

                    /* 注册链接事件 */
                    //m_dev.CameraOpened += OnCameraOpen;
                    m_dev.ConnectionLost += OnConnectLoss;
                    //m_dev.CameraClosed += OnCameraClose;

                    /* 打开设备 */
                    if (!m_dev.Open())
                    {
                        MessageBox.Show(@"连接相机失败");
                        return -3;
                    }

                    /* 打开Software Trigger */
                    m_dev.TriggerSet.Open(TriggerSourceEnum.Software);
                    //m_dev.TriggerSet.Open(AcquisitionModeEnum.Continuous);//由软体触发改为连续触发


                    ////连续触发
                    //using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
                    //{
                    //    p.SetValue("Off");
                    //}

                    //连续采集
                    using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.AcquisitionMode])
                    {
                        p.SetValue("Continuous");
                    }

                    //using (IIntegraParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImageWidth])
                    //{
                    //    m_width = (int)p.GetValue();
                    //}

                    //using (IIntegraParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImageHeight])
                    //{
                    //    m_height = (int)p.GetValue();
                    //}

                    /* 设置图像格式 */
                    using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImagePixelFormat])
                    {
                        p.SetValue("Mono8");
                    }

                    /* 设置曝光 */
                    using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ExposureTime])
                    {
                        p.SetValue(5000);
                    }

                    /* 设置增益 */
                    using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.GainRaw])
                    {
                        p.SetValue(1.0);
                    }

                    /* 设置缓存个数为8（默认值为16） */
                    m_dev.StreamGrabber.SetBufferCount(8);

                    /* 注册码流回调事件 */
                    m_dev.StreamGrabber.ImageGrabbed += OnImageGrabbed;

                    /* 开启码流 */
                    if (!m_dev.GrabUsingGrabLoopThread())
                    {
                        MessageBox.Show(@"开启码流失败");
                        return -4;
                    }

                    return 0;
                }
                return -1;
            }
            catch (Exception exception)
            {
                return -2;
                //Catcher.Show(exception);
            }
        }
        private int OpenDeviceByKey(string m_key)
        {
            try
            {
                /* 设备搜索 */
                List<IDeviceInfo> li = Enumerator.EnumerateDevices();
                if (li.Count > 0)
                {
                    int index = -1;
                    foreach(IDeviceInfo info in li)
                    {
                        if (info.SerialNumber == m_key)
                        {
                            index = info.Index;
                            break;
                        }
                    }

                    if (index < 0)
                        return -5;

                    /* 获取搜索到的第一个设备 */
                    m_dev = Enumerator.GetDeviceByIndex(index);
                    //m_dev = Enumerator.GetDeviceByKey("Machine Vision:" + m_key);

                    /* 注册链接事件 */
                    //m_dev.CameraOpened += OnCameraOpen;
                    m_dev.ConnectionLost += OnConnectLoss;
                    //m_dev.CameraClosed += OnCameraClose;

                    /* 打开设备 */
                    if (!m_dev.Open())
                    {
                        MessageBox.Show(@"连接相机失败");
                        return -3;
                    }

                    /* 打开Software Trigger */
                    m_dev.TriggerSet.Open(TriggerSourceEnum.Software);
                    //m_dev.TriggerSet.Open(AcquisitionModeEnum.Continuous);//由软体触发改为连续触发


                    //连续触发
                    using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
                    {
                        p.SetValue("On");
                    }

                    //连续采集
                    using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.AcquisitionMode])
                    {
                        p.SetValue("Continuous");
                    }

                    using (IIntegraParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImageWidth])
                    {
                        m_width = (int)p.GetValue();
                    }

                    using (IIntegraParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImageHeight])
                    {
                        m_height = (int)p.GetValue();
                    }

                    /* 设置图像格式 */
                    using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ImagePixelFormat])
                    {
                        p.SetValue("Mono8");
                    }

                    /* 设置曝光 */
                    using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ExposureTime])
                    {
                        p.SetValue(50000);
                    }

                    ///* 设置增益 */
                    //using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.GainRaw])
                    //{
                    //    p.SetValue(1.0);
                    //}

                    /* 设置缓存个数为8（默认值为16） */
                    m_dev.StreamGrabber.SetBufferCount(8);

                    /* 注册码流回调事件 */
                    m_dev.StreamGrabber.ImageGrabbed += OnImageGrabbed;

                    /* 开启码流 */
                    if (!m_dev.GrabUsingGrabLoopThread())
                    {
                        MessageBox.Show(@"开启码流失败");
                        return -4;
                    }

                    return 0;
                }
                return -1;
            }
            catch (Exception exception)
            {
                return -2;
                //Catcher.Show(exception);
            }
        }
        private void CloseDevice()
        {
            try
            {
                if (m_dev == null)
                {
                    throw new InvalidOperationException("Device is invalid");
                }

                //m_dev.TriggerSet.Open(TriggerSourceEnum.Line1);
                using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
                {
                    p.SetValue("Off");
                }

                m_dev.StreamGrabber.ImageGrabbed -= OnImageGrabbed;         /* 反注册回调 */
                m_dev.ShutdownGrab();                                       /* 停止码流 */
                m_dev.Close();                                              /* 关闭相机 */
            }
            catch (Exception exception)
            {
                //Catcher.Show(exception);
            }
        }

        const int DEFAULT_INTERVAL = 40;
        Stopwatch m_stopWatch = new Stopwatch();    /* 时间统计器 */

        /* 判断是否应该做显示操作 */
        private bool isTimeToDisplay()
        {
            m_stopWatch.Stop();
            long m_lDisplayInterval = m_stopWatch.ElapsedMilliseconds;
            if (m_lDisplayInterval <= DEFAULT_INTERVAL)
            {
                m_stopWatch.Start();
                return false;
            }
            else
            {
                m_stopWatch.Reset();
                m_stopWatch.Start();
                return true;
            }
        }

        Stopwatch m_stopWatch_fps = new Stopwatch();    /* 时间统计器 */
        public int iFpsCount = 0;
        int IfpsTempCount = 0;

        public int Trigger()
        {

            //return;
            if (m_dev == null)
            {
                //throw new InvalidOperationException("Device is invalid");
                return -1;
            }
            m_bShowLoop = true;
            try
            {
                m_dev.ExecuteSoftwareTrigger();
                //Thread.Sleep(10);
                //m_dev.ExecuteSoftwareTrigger();
            }
            catch (Exception exception)
            {
                //Catcher.Show(exception);
                throw exception;
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 设置曝光 实际时0-100 0000 软体只设置60 0000
        /// </summary>
        /// <param name="ivalue">soft setup 0-60</param>
        public int SetExposure(double ivalue)
        {
            int ret = -1;
            try
            {
                /* 设置曝光 */
                using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ExposureTime])
                {
                    double max = p.GetMaximum();
                    double min = p.GetMinimum();

                    if (ivalue < min || ivalue > max)
                        ret = -1;
                    else
                    {
                        p.SetValue(ivalue);
                        ret = 0;
                    }
                    //double mValue = ivalue;// * 10000;
                    //p.SetValue(mValue);
                }
            }
            catch
            {
                ret = -2;
            }

            return ret;
        }
        public int SetGain(double ivalue)
        {
            int ret = -1;
            try
            {
                /* 设置曝光 */
                using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.GainRaw])
                {
                    double max = p.GetMaximum();
                    double min = p.GetMinimum();

                    if (ivalue < min || ivalue > max)
                        ret = -1;
                    else
                    {
                        p.SetValue(ivalue);
                        ret = 0;
                    }
                    //double mValue = ivalue;// * 10000;
                    //p.SetValue(mValue);
                }
            }
            catch
            {
                ret = -2;
            }

            return ret;
        }
        public void TriggerMode(int iMode)
        {
            //连续触发
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
            {
                p.SetValue((iMode == 0 ? "On" : "Off"));
            }
        }

        Bitmap bmpOutput = new Bitmap(1, 1);

        public DateTime dtNowTime
        {
            get { return dtPicture; }
        }
        DateTime dtPicture = DateTime.Now;
        public Bitmap CaptureBmp()
        {
            //bmpOutput.Dispose();
            //bmpOutput = (Bitmap)_GetImage(1000).Clone();

            _GetImage(1000);

            //bmpOutput.Dispose();
            //if (bmpTmpCapture == null)
            //{
            //    bmpOutput = new Bitmap(Starconfig.Instance.CxROIWIDTH, Starconfig.Instance.CxROIHEIGHT);
            //}
            //else
            //{
            //    if (bmpTmpCapture.Width < Starconfig.Instance.CxROIWIDTH || bmpTmpCapture.Height < Starconfig.Instance.CxROIHEIGHT)
            //        bmpOutput = new Bitmap(Starconfig.Instance.CxROIWIDTH, Starconfig.Instance.CxROIHEIGHT);
            //    else
            //        bmpOutput = (Bitmap)bmpTmpCapture.Clone(new Rectangle(0, 0, Starconfig.Instance.CxROIWIDTH, Starconfig.Instance.CxROIHEIGHT),
            //                                                                                                 System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //}

            //pbImage.Image = bmpOutput;
            //return bmpOutput;

            bmpOutput.Dispose();
            if (bmpTmpCapture == null)
                bmpOutput = new Bitmap(1, 1);
            else
                bmpOutput = (Bitmap)bmpTmpCapture.Clone();

            //_gError = Graphics.FromImage(bmpOutput);
            //_gError.DrawString(dtPicture.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 18), new SolidBrush(Color.Lime), new PointF(5, 5));
            //_gError.Dispose();

            //旋转图片操作
            //bmpOutput.RotateFlip(RotateFlipType.Rotate180FlipNone);

            return bmpOutput;
        }
        /// <summary>
        /// 取像
        /// </summary>
        /// <param name="timeout">延迟时间</param>
        /// <returns></returns>
        Bitmap _GetImage(int timeout)
        {
            bmpTmpCapture = null;
            Trigger();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                if (bmpTmpCapture != null)
                    return bmpTmpCapture;
                else if (watch.ElapsedMilliseconds > timeout)
                    return null;

            }

            //m_Muilt_bmp[m_Muilt_Index] = null;
            //Trigger();
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //while (true)
            //{
            //    if (m_Muilt_bmp[m_Muilt_Index] != null)
            //        return m_Muilt_bmp[m_Muilt_Index];
            //    else if (watch.ElapsedMilliseconds > timeout)
            //        return null;

            //}
        }
        /* 转码显示线程 */
        private void ShowThread()
        {
            return;
            while (m_thread_isrunning)
            {
                if (m_frameList.Count == 0)
                {
                    Thread.Sleep(10);
                    iCountLost++;

                    if (iCountLost <= 100)
                        continue;

                    bmpTmpCapture = new Bitmap(m_width, m_height);

                    _gError = Graphics.FromImage(bmpTmpCapture);
                    _gError.Clear(Color.Black);
                    _gError.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 100), new SolidBrush(Color.Red), new PointF(5, 5));
                    _gError.DrawString("ccd losting...", new Font("Arial", 180), new SolidBrush(Color.Red), new PointF(5, bmpTmpCapture.Height / 4));

                    _gError.Dispose();


                    continue;
                }

                /* 图像队列取最新帧 */
                m_mutex.WaitOne();
                IGrabbedRawData frame = m_frameList.ElementAt(m_frameList.Count - 1);
                m_frameList.Clear();
                m_mutex.ReleaseMutex();

                Thread.Sleep(1);

                /* 主动调用回收垃圾 */
                //GC.Collect();

                /* 控制显示最高帧率为25FPS */
                //if (false == isTimeToDisplay())
                //{
                //    continue;
                //}

                try
                {
                    if (!m_stopWatch_fps.IsRunning)
                        m_stopWatch_fps.Start();

                    if (m_stopWatch_fps.ElapsedMilliseconds > 1000)
                    {
                        m_stopWatch_fps.Reset();
                        iFpsCount = IfpsTempCount;
                        IfpsTempCount = 0;
                    }
                    else
                        IfpsTempCount++;



                    /* 图像转码成bitmap图像 */
                    var bitmap = frame.ToBitmap(false);
                    //bmpTmpCapture.Dispose();
                    bmpTmpCapture = (Bitmap)bitmap.Clone();

                    //Bitmap bmpERROR = (Bitmap)bitmap.Clone();
                    //_gError = Graphics.FromImage(bmpERROR);
                    //_gError.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 100), new SolidBrush(Color.Lime), new PointF(5, 5));
                    //_gError.Dispose();

                    //bmpTmpCapture = new Bitmap(bitmap);
                    //bmpTmpCapture = (Bitmap)bitmap.Clone();

                    m_bShowByGDI = false;
                    if (m_bShowByGDI)
                    {
                        if (m_sizechange)
                        {
                            m_sizechange = false;
                            _g = null;
                        }

                        /* 使用GDI绘图 */
                        if (_g == null)
                        {
                            pbImage.Invoke(new Action(() =>
                            {
                                _g = pbImage.CreateGraphics();
                            }));

                            //_g = pbImage.CreateGraphics();
                        }
                        //_g.DrawString((frame.ImageSize/ frame.TimeStamp).ToString(), MyFont, B, new PointF(5, 5));
                        //_g.DrawString(frame.TimeStamp.ToString(),new Font(),)
                        if (true)
                        {
                            _g.DrawImage(bitmap, new Rectangle(0, 0, pbImage.Width, pbImage.Height),
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                            _g.DrawString(iFpsCount.ToString() + " fps", new Font("Arial", 18), new SolidBrush(Color.Lime), new PointF(5, 5));
                            dtPicture = DateTime.Now;
                            //_g.DrawString(dtPicture.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 18), new SolidBrush(Color.Lime), new PointF(5, 5));
                        }

                        bitmap.Dispose();
                    }
                    else
                    {
                        pbImage.Invoke(new Action(() =>
                        {
                            pbImage.Image = bitmap;
                        }));

                        /* 使用控件绘图,会造成主界面卡顿 */
                        //if (InvokeRequired)
                        //{
                        //    BeginInvoke(new MethodInvoker(() =>
                        //    {
                        //        try
                        //        {
                        //            pbImage.Image = bitmap;
                        //        }
                        //        catch (Exception exception)
                        //        {
                        //            Catcher.Show(exception);
                        //        }
                        //    }));
                        //}
                    }

                    iCountLost = 0;
                    m_bShowLoop = false;
                    m_ReTrigger = 0;
                }
                catch (Exception exception)
                {
                    //bmpTmpCapture = new Bitmap(m_width, m_height);

                    //_gError = Graphics.FromImage(bmpTmpCapture);
                    //_gError.Clear(Color.Black);
                    //_gError.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 100), new SolidBrush(Color.Red), new PointF(5, 5));
                    //_gError.DrawString("异常:ccd losting...", new Font("Arial", 180), new SolidBrush(Color.Red), new PointF(5, bmpTmpCapture.Height / 4));


                    //_gError.Dispose();

                    iCountLost = 0;
                    m_bShowLoop = false;
                    m_ReTrigger = 0;

                    //Catcher.Show(exception);
                }
            }
        }

        public void CAMTick()
        {
            while (m_bShowLoop && !m_thread_muilt)
            {
                if (m_frameList.Count == 0)
                {
                    Thread.Sleep(10);
                    iCountLost++;

                    if (iCountLost <= 100)
                        continue;

                    bmpTmpCapture = new Bitmap(m_width, m_height);

                    _gError = Graphics.FromImage(bmpTmpCapture);
                    _gError.Clear(Color.Black);
                    _gError.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 100), new SolidBrush(Color.Red), new PointF(5, 5));
                    _gError.DrawString("ccd losting...", new Font("Arial", 180), new SolidBrush(Color.Red), new PointF(5, bmpTmpCapture.Height / 4));

                    _gError.Dispose();

                    return;
                }

                /* 图像队列取最新帧 */
                m_mutex.WaitOne();//CUT
                IGrabbedRawData frame = m_frameList.ElementAt(m_frameList.Count - 1);
                m_frameList.Clear();
                m_mutex.ReleaseMutex();//CUT

                /* 主动调用回收垃圾 */
                GC.Collect();

                /* 控制显示最高帧率为25FPS *///CUT
                                    //if (false == isTimeToDisplay())
                                    //{
                                    //    continue;
                                    //}

                try
                {
                    /* 图像转码成bitmap图像 */
                    var bitmap = frame.ToBitmap(false);
                    //bmpTmpCapture.Dispose();
                    //bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    bmpTmpCapture = (Bitmap)bitmap.Clone();
                    m_bShowByGDI = true;
                    if (m_bShowByGDI)
                    {
                        if (m_sizechange)
                        {
                            m_sizechange = false;
                            _g = null;
                        }

                        /* 使用GDI绘图 */
                        if (_g == null)
                        {
                            pbImage.Invoke(new Action(() =>
                            {
                                _g = pbImage.CreateGraphics();
                            }));
                        }
                        //if (Live)
                        {
                            _g.DrawImage(bitmap, new Rectangle(0, 0, pbImage.Width, pbImage.Height),
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                        }

                        bitmap.Dispose();

                    }
                    else
                    {
                        pbImage.Invoke(new Action(() =>
                        {
                            pbImage.Image = bitmap;
                        }));
                    }

                    iCountLost = 0;
                    m_bShowLoop = false;
                    m_ReTrigger = 0;
                }
                catch (Exception exception)
                {

                    bmpTmpCapture = new Bitmap(m_width, m_height);

                    _gError = Graphics.FromImage(bmpTmpCapture);
                    _gError.Clear(Color.Black);
                    _gError.DrawString(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), new Font("Arial", 100), new SolidBrush(Color.Red), new PointF(5, 5));
                    _gError.DrawString("异常:ccd losting...", new Font("Arial", 180), new SolidBrush(Color.Red), new PointF(5, bmpTmpCapture.Height / 4));


                    _gError.Dispose();

                    iCountLost = 0;
                    m_bShowLoop = false;
                    m_ReTrigger = 0;

                    throw exception;
                    //Catcher.Show(exception);
                }


            }
        }

        /* 码流数据回调 */
        private void OnImageGrabbed(Object sender, GrabbedEventArgs e)
        {
            //bmpTmpCapture = e.GrabResult.ToBitmap(false);

            m_mutex.WaitOne();
            bmpTmpCapture = e.GrabResult.ToBitmap(false);

            //m_frameList.Add(e.GrabResult.Clone());
            m_mutex.ReleaseMutex();

            //m_Muilt_bmp[m_Muilt_Index] = e.GrabResult.ToBitmap(false);

            //m_mutex.WaitOne();
            //m_frameList.Add(e.GrabResult.Clone());
            //m_mutex.ReleaseMutex();
        }

        /* 设备对象 */
        private IDevice m_dev;

        /* 相机打开回调 */
        private void OnCameraOpen(object sender, EventArgs e)
        {
            //this.Invoke(new Action(() =>
            //{
            //    btnOpen.Enabled = false;
            //    btnClose.Enabled = true;
            //    btnSoftwareTrigger.Enabled = true;
            //}));
        }

        /* 相机关闭回调 */
        private void OnCameraClose(object sender, EventArgs e)
        {
            //this.Invoke(new Action(() =>
            //{
            //    btnOpen.Enabled = true;
            //    btnClose.Enabled = false;
            //    btnSoftwareTrigger.Enabled = false;
            //}));
        }

        /* 相机丢失回调 */
        private void OnConnectLoss(object sender, EventArgs e)
        {
            OnLost(true);

            m_dev.ShutdownGrab();
            m_dev.Dispose();
            m_dev = null;

            //this.Invoke(new Action(() =>
            //{
            //    btnOpen.Enabled = true;
            //    btnClose.Enabled = false;
            //    btnSoftwareTrigger.Enabled = false;
            //}));
        }

        public delegate void CamDisConnectHandler(bool islost);
        public event CamDisConnectHandler LostAction;
        public void OnLost(bool islost)
        {
            if (LostAction != null)
            {
                LostAction(islost);
            }
        }
    }
}
