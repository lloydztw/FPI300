
#define USE_CALL_BACK

using FreeImageAPI;
using JetEazy.Interface;
using MVSDK;
using OpenCvSharp.Flann;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static JetEazy.CCDSpace.CameraPara;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;


namespace JetEazy.CCDSpace.CamLinkDriver
{
    public class Linescan_Mind : IxLineScanCam
    {
        #region PRIVATE VAR

        private string _configFilename = "LineCameraConfig.ini";

        //private uint m_handle = 0;
        //private IntPtr m_ptr_wnd = new IntPtr();
        //private IntPtr m_ptr = new IntPtr();

        //// 显示参数
        //private Stopwatch m_Stopwatch = new Stopwatch();
        //private Double m_dfDisplayCount = 0;

        private string m_SerialNumber = string.Empty;
        //private bool m_GetImageOK;
        private Bitmap m_bmpCurrent;
        //private int m_Rotate = 0;
        private string m_LineCameraConfigPath = string.Empty;
        bool m_TriggerOK = false;
        bool m_IsDebug = false;
        bool m_TriggerComplete = false;
        //string m_TrigCtl = "ISO";
        bool m_IsGraping = false;
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
            set
            {
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
            //if (m_IsDebug)
            //    return;
            //if (IsValidHandle(m_handle))
            //{
            //    dvpStatus status = DVPCamera.dvpShowPropertyModalDialog(m_handle, new IntPtr());
            //}
        }
        public Bitmap GetPageBitmap(int size)
        {
            if (m_IsDebug)
                return new Bitmap(1, 1);
            if (m_bmpCurrent == null)
            {
                m_TriggerOK = false;
                return null;
            }
            if (m_TriggerOK)
            {
                Bitmap bmpLine = new Bitmap(1, 1);
                bmpLine.Dispose();
                bmpLine = (Bitmap)m_bmpCurrent.Clone();
                return bmpLine;
            }
            m_TriggerOK = false;
            return null;
        }
        public void Dispose()
        {
            Close();
        }

        public event LineTriggerHandler LineTriggerAction;
        public void FireTrigger(CameraFrame cameraFrame, IntPtr pBuffer)
        {
            if (LineTriggerAction != null)
            {
                LineTriggerAction(cameraFrame, pBuffer);
            }
        }


        #region PRIVATE FUNTION
        private int _Init(string eSerialNumberStr, string eDvp2ConfigPath = "")
        {
            m_SerialNumber = eSerialNumberStr;
            m_LineCameraConfigPath = eDvp2ConfigPath;
            int nRet = -1;
            //DeviceListAcq();
            nRet = openDevice();
            //if (nRet == 0)
            //    StartGrab();

            return nRet;
        }
        private int openDevice()
        {
            int nRet = -1;
            CameraSdkStatus status;
            tSdkCameraDevInfo[] tCameraDevInfoList = null;

#if USE_CALL_BACK
            CAMERA_SNAP_PROC pCaptureCallOld = null;
#endif

            if (!string.IsNullOrEmpty(m_SerialNumber))
            {
                int _index = GetDeviceNumber(m_SerialNumber);
                status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);

                // 打开相机。
                //
                // Open camera.
                status = MvApi.CameraInit(ref tCameraDevInfoList[_index], -1, -1, ref m_hCamera);
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    string configFileName = m_LineCameraConfigPath + "\\" + m_SerialNumber + ".config";
                    // 导入配置文件。
                    //
                    // Load configuration file.
                    if (System.IO.File.Exists(configFileName))
                    {
                        MvApi.CameraReadParameterFromFile(m_hCamera, configFileName);
                    }
                    else
                    {
                        MvApi.CameraLoadParameter(m_hCamera, (int)emSdkParameterTeam.PARAMETER_TEAM_A);
                        MvApi.CameraSaveParameterToFile(m_hCamera, configFileName);
                    }

                    //获得相机特性描述
                    status = MvApi.CameraGetCapability(m_hCamera, out tCameraCapability);
                    m_ImageBuffer = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
                    m_ImageBufferSnapshot = Marshal.AllocHGlobal(tCameraCapability.sResolutionRange.iWidthMax * tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);

                    if (tCameraCapability.sIspCapacity.bMonoSensor != 0)
                    {
                        // 黑白相机输出8位灰度数据
                        MvApi.CameraSetIspOutFormat(m_hCamera, (uint)MVSDK.emImageFormat.CAMERA_MEDIA_TYPE_MONO8);
                    }

                    ////初始化显示模块，使用SDK内部封装好的显示接口
                    //MvApi.CameraDisplayInit(m_hCamera, PreviewBox.Handle);
                    //MvApi.CameraSetDisplaySize(m_hCamera, PreviewBox.Width, PreviewBox.Height);

                    ////设置抓拍通道的分辨率。
                    //tSdkImageResolution tResolution;
                    //tResolution.uSkipMode = 0;
                    //tResolution.uBinAverageMode = 0;
                    //tResolution.uBinSumMode = 0;
                    //tResolution.uResampleMask = 0;
                    //tResolution.iVOffsetFOV = 0;
                    //tResolution.iHOffsetFOV = 0;
                    //tResolution.iWidthFOV = tCameraCapability.sResolutionRange.iWidthMax;
                    //tResolution.iHeightFOV = tCameraCapability.sResolutionRange.iHeightMax;
                    //tResolution.iWidth = tResolution.iWidthFOV;
                    //tResolution.iHeight = tResolution.iHeightFOV;
                    ////tResolution.iIndex = 0xff;表示自定义分辨率,如果tResolution.iWidth和tResolution.iHeight
                    ////定义为0，则表示跟随预览通道的分辨率进行抓拍。抓拍通道的分辨率可以动态更改。
                    ////本例中将抓拍分辨率固定为最大分辨率。
                    //tResolution.iIndex = 0xff;
                    //tResolution.acDescription = new byte[32];//描述信息可以不设置
                    //tResolution.iWidthZoomHd = 0;
                    //tResolution.iHeightZoomHd = 0;
                    //tResolution.iWidthZoomSw = 0;
                    //tResolution.iHeightZoomSw = 0;

                    //MvApi.CameraSetResolutionForSnap(m_hCamera, ref tResolution);

                    ////让SDK来根据相机的型号动态创建该相机的配置窗口。
                    //MvApi.CameraCreateSettingPage(m_hCamera, this.Handle, tCameraDevInfoList[0].acFriendlyName,/*SettingPageMsgCalBack*/null,/*m_iSettingPageMsgCallbackCtx*/(IntPtr)null, 0);

                    //两种方式来获得预览图像，设置回调函数或者使用定时器或者独立线程的方式，
                    //主动调用CameraGetImageBuffer接口来抓图。
                    //本例中仅演示了两种的方式,注意，两种方式也可以同时使用，但是在回调函数中，
                    //不要使用CameraGetImageBuffer，否则会造成死锁现象。
#if USE_CALL_BACK
                    m_CaptureCallback = new CAMERA_SNAP_PROC(ImageCaptureCallback);
                    status = MvApi.CameraSetCallbackFunction(m_hCamera, m_CaptureCallback, m_iCaptureCallbackCtx, ref pCaptureCallOld);
#else //如果需要采用多线程，使用下面的方式
                    m_bExitCaptureThread = false;
                    m_tCaptureThread = new Thread(new ThreadStart(CaptureThreadProc));
                    m_tCaptureThread.Start();

#endif
                    if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                        nRet = 0;

                }
                else
                {

                    m_hCamera = 0;
                    String errstr = string.Format("相机初始化错误，错误码{0},错误原因是", status);
                    String errstring = MvApi.CameraGetErrorString(status);
                    MessageBox.Show(errstr + errstring, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }

            return nRet;
        }
        private void closeDevice()
        {
#if !USE_CALL_BACK //使用回调函数的方式则不需要停止线程
            m_bExitCaptureThread = true;
            while (m_tCaptureThread.IsAlive)
            {
                Thread.Sleep(10);
            }
#endif

            if (m_hCamera == 0 || m_ImageBuffer == IntPtr.Zero) // || m_ImageBufferSnapshot[index] == null)
                return;

            if (m_hCamera != 0)
            {
                MvApi.CameraUnInit(m_hCamera);
                m_hCamera = 0;
            }

            if (m_ImageBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_ImageBuffer);
                m_ImageBuffer = IntPtr.Zero;
            }
        }
        public void StartGrab()
        {
            //if (m_IsGraping)
            //    return;

            CameraSdkStatus status = MvApi.CameraPlay(m_hCamera);
            //m_IsGraping = true;
            m_OperateMessage = status.ToString();
        }
        public void StopGrab()
        {
            //if (!m_IsGraping)
            //    return;

            CameraSdkStatus status = MvApi.CameraStop(m_hCamera);
            //m_IsGraping = false;
            m_OperateMessage = status.ToString();
        }

        private static Mutex imageMutex = new Mutex();
        private ColorPalette tempPalette;

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

            CameraSdkStatus status;
            tSdkCameraDevInfo[] tCameraDevInfoList;

            status = MvApi.CameraSdkInit(1);//1:Chinese 0:English
            if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                MessageBox.Show("WIND SDK 初始化失敗。");
                return iNumber;
            }

            status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);
            if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                MessageBox.Show("没有找到相机，如果已经接上相机，可能是权限不够，请尝试使用管理员权限运行程序。");
                return iNumber;
            }

            int i = 0;
            foreach (tSdkCameraDevInfo ds in tCameraDevInfoList)
            {
                string _windSN = System.Text.Encoding.UTF8.GetString(ds.acSn);

                if (_windSN.Contains(strSerialNumber))
                {
                    iNumber = i;
                    break;
                }
                i++;
            }
            return iNumber;
        }

        #endregion

        #region MIND_API_FUNTION

        #region variable
        protected CameraHandle m_hCamera = 0;             // 句柄
        protected IntPtr m_ImageBuffer;             // 预览通道RGB图像缓存
        protected IntPtr m_ImageBufferSnapshot;     // 抓拍通道RGB图像缓存
        protected tSdkCameraCapbility tCameraCapability;  // 相机特性描述
        protected int m_iDisplayedFrames = 0;    //已经显示的总帧数
        protected CAMERA_SNAP_PROC m_CaptureCallback;
        protected IntPtr m_iCaptureCallbackCtx;     //图像回调函数的上下文参数
        protected Thread m_tCaptureThread;          //图像抓取线程
        protected bool m_bExitCaptureThread = false;//采用线程采集时，让线程退出的标志
        protected IntPtr m_iSettingPageMsgCallbackCtx; //相机配置界面消息回调函数的上下文参数   
        protected tSdkFrameHead m_tFrameHead;
        //protected SnapshotDlg m_DlgSnapshot = new SnapshotDlg();               //显示抓拍图像的窗口
        protected bool m_bEraseBk = false;
        protected bool m_bSaveImage = false;
        #endregion

#if USE_CALL_BACK
        //private static Mutex imageMutex = new Mutex();
        FreeImageAPI.FreeImageBitmap bmp = new FreeImageBitmap(1, 1);
        public void ImageCaptureCallback(CameraHandle hCamera, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr pContext)
        {
            m_TriggerComplete = false;

            //FreeImageAPI.FreeImageBitmap bmp = null;
            //int nFrameWidth = 0;
            //int nFrameHeight = 0;
            try
            {
                if (bmp != null)
                    bmp.Dispose();
                imageMutex.WaitOne();
                //图像处理，将原始输出转换为RGB格式的位图数据，同时叠加白平衡、饱和度、LUT等ISP处理。
                MvApi.CameraImageProcess(hCamera, pFrameBuffer, m_ImageBuffer, ref pFrameHead);
                ////叠加十字线、自动曝光窗口、白平衡窗口信息(仅叠加设置为可见状态的)。   
                //MvApi.CameraImageOverlay(hCamera, m_ImageBuffer, ref pFrameHead);
                ////调用SDK封装好的接口，显示预览图像
                //MvApi.CameraDisplayRGB24(hCamera, m_ImageBuffer, ref pFrameHead);
                m_iDisplayedFrames++;

                if (pFrameHead.iWidth != m_tFrameHead.iWidth || pFrameHead.iHeight != m_tFrameHead.iHeight)
                {
                    m_bEraseBk = true;
                    m_tFrameHead = pFrameHead;
                }

                //Bitmap m_bmp_Mvapi = (Bitmap)MvApi.CSharpImageFromFrame(m_ImageBuffer, ref pFrameHead);
                //bmp=new FreeImageAPI.FreeImageBitmap(m_bmp_Mvapi);
                //m_bmp_Mvapi.Dispose();

                bmp = new FreeImageAPI.FreeImageBitmap(pFrameHead.iWidth, pFrameHead.iHeight, pFrameHead.iWidth, PixelFormat.Format8bppIndexed, m_ImageBuffer);
                if (_camCfg.Rotate != 0)
                    bmp.Rotate(_camCfg.Rotate);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                m_bmpCurrent = bmp.ToBitmap();
                m_TriggerOK = true;
            }
            finally
            {
                imageMutex.ReleaseMutex();
            }

            m_TriggerComplete = true;
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
#else
        public void CaptureThreadProc()
        {
            CameraSdkStatus eStatus;
            tSdkFrameHead FrameHead;
            IntPtr uRawBuffer;//rawbuffer由SDK内部申请。应用层不要调用delete之类的释放函数

            while (m_bExitCaptureThread == false)
            {
                m_TriggerComplete = false;
                FreeImageAPI.FreeImageBitmap bmp = null;
                //500毫秒超时,图像没捕获到前，线程会被挂起,释放CPU，所以该线程中无需调用sleep
                eStatus = MvApi.CameraGetImageBuffer(m_hCamera, out FrameHead, out uRawBuffer, 10000);

                if (eStatus == CameraSdkStatus.CAMERA_STATUS_SUCCESS)//如果是触发模式，则有可能超时
                {
                    //图像处理，将原始输出转换为RGB格式的位图数据，同时叠加白平衡、饱和度、LUT等ISP处理。
                    MvApi.CameraImageProcess(m_hCamera, uRawBuffer, m_ImageBuffer, ref FrameHead);
                    ////叠加十字线、自动曝光窗口、白平衡窗口信息(仅叠加设置为可见状态的)。    
                    //MvApi.CameraImageOverlay(m_hCamera, m_ImageBuffer, ref FrameHead);
                    ////调用SDK封装好的接口，显示预览图像
                    //MvApi.CameraDisplayRGB24(m_hCamera, m_ImageBuffer, ref FrameHead);
                    //成功调用CameraGetImageBuffer后必须释放，下次才能继续调用CameraGetImageBuffer捕获图像。
                    MvApi.CameraReleaseImageBuffer(m_hCamera, uRawBuffer);

                    if (FrameHead.iWidth != m_tFrameHead.iWidth || FrameHead.iHeight != m_tFrameHead.iHeight)
                    {
                        m_bEraseBk = true;
                        m_tFrameHead = FrameHead;
                    }

                    m_iDisplayedFrames++;

                    //Bitmap m_bmp_Mvapi = (Bitmap)MvApi.CSharpImageFromFrame(m_ImageBuffer, ref pFrameHead);
                    //bmp=new FreeImageAPI.FreeImageBitmap(m_bmp_Mvapi);
                    //m_bmp_Mvapi.Dispose();

                    //if (m_bSaveImage)
                    //{
                    //    MvApi.CameraSaveImage(m_hCamera, "c:\\test.bmp", m_ImageBuffer, ref FrameHead, emSdkFileType.FILE_BMP, 100);
                    //    m_bSaveImage = false;
                    //}
                    bmp = new FreeImageAPI.FreeImageBitmap(FrameHead.iWidth, FrameHead.iHeight, FrameHead.iWidth, PixelFormat.Format8bppIndexed, m_ImageBuffer);
                    bmp.Rotate(_camCfg.Rotate);
                    m_bmpCurrent = bmp.ToBitmap();
                    m_TriggerOK = true;

                }

                m_TriggerComplete = true;
            }

        }
#endif

        #endregion
    }
}

