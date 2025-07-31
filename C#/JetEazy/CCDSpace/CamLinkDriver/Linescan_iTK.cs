
using DVPCameraType;
using FreeImageAPI;
using FreeImageAPI.IO;
using IKapBoardClassLibrary;
using IKapC.NET;

using JetEazy.Interface;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static JetEazy.CCDSpace.CameraPara;


namespace JetEazy.CCDSpace.CamLinkDriver
{
    public class Linescan_iTK : IxLineScanCam
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
        bool m_BufferFull = false;
        CameraPara _camCfg = new CameraPara();
        string m_OperateMessage = string.Empty;
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

                ////停止图像采集。

                //// Stop grabbing images.
                //int ret = 0;
                //ret = IKapBoard.IKapStopGrab(m_hBoard);
                //CheckIKapBoard(ret);

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

            //if (nRet == 0)
            //{
            //    int ret = 0;
            //    // 开始图像采集。
            //    //
            //    // Start grabbing images.
            //    ret = IKapBoard.IKapStartGrab(m_hBoard, 0);
            //    CheckIKapBoard(ret);
            //}

            return nRet;
        }
        private int openDevice()
        {
            InitEnvironment();
            int nRet = -1;
            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;

            if (!string.IsNullOrEmpty(m_SerialNumber))
            {
                int _index = GetDeviceNumber(m_SerialNumber);
                IKapCLib.ITK_CL_DEV_INFO cl_board_info = new IKapCLib.ITK_CL_DEV_INFO();

                // 打开相机。
                //
                // Open camera.
                res = IKapCLib.ItkDevOpen((uint)_index, (int)ItkDeviceAccessMode.ITKDEV_VAL_ACCESS_MODE_EXCLUSIVE, ref m_hCamera);
                //CheckIKapC(res);

                // 获取 CameraLink 相机设备信息。
                //
                // Get CameraLink camera device information.
                res = IKapCLib.ItkManGetCLDeviceInfo((uint)_index, ref cl_board_info);
                //CheckIKapC(res);

                // 打开采集卡。
                //
                // Open frame grabber.
                m_hBoard = IKapBoard.IKapOpen(cl_board_info.HostInterface, cl_board_info.BoardIndex);
                if (m_hBoard.Equals(-1))
                    CheckIKapBoard((int)ErrorCode.IKStatus_OpenBoardFail);

                int ret = (int)ErrorCode.IK_RTN_OK;
                string configFileName = null;

                // 导入配置文件。
                //
                // Load configuration file.
                //configFileName = GetOption();
                if (System.IO.Directory.Exists(m_LineCameraConfigPath))
                    configFileName = m_LineCameraConfigPath + "\\" + m_SerialNumber + ".vlcf";
                if (configFileName == null)
                {
                    Console.WriteLine("Fail to get configuration, using default setting!\n");
                }
                else
                {
                    ret = IKapBoard.IKapLoadConfigurationFromFile(m_hBoard, configFileName);
                    CheckIKapBoard(ret);
                }

                // 设置图像缓冲区帧数。
                //
                // Set frame count of buffer.
                ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_COUNT, m_nTotalFrameCount);
                CheckIKapBoard(ret);

                // 设置超时时间。
                //
                // Set time out time.
                int timeout = -1;
                ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_TIME_OUT, timeout);
                CheckIKapBoard(ret);

                // 设置采集模式。
                //
                // Set grab mode.
                int grab_mode = (int)GrabMode.IKP_GRAB_NON_BLOCK;
                ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_GRAB_MODE, grab_mode);
                CheckIKapBoard(ret);

                // 设置传输模式。
                //
                // Set transfer mode.
                int transfer_mode = (int)FrameTransferMode.IKP_FRAME_TRANSFER_SYNCHRONOUS_NEXT_EMPTY_WITH_PROTECT;
                ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_TRANSFER_MODE, transfer_mode);
                CheckIKapBoard(ret);

                // 注册回调函数
                //
                // Register callback functions.
                OnGrabStartProc = new IKapCallBackProc(OnGrabStartFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStart, Marshal.GetFunctionPointerForDelegate(OnGrabStartProc), m_hBoard);
                CheckIKapBoard(ret);
                OnFrameReadyProc = new IKapCallBackProc(OnFrameReadyFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameReady, Marshal.GetFunctionPointerForDelegate(OnFrameReadyProc), m_hBoard);
                CheckIKapBoard(ret);
                OnFrameLostProc = new IKapCallBackProc(OnFrameLostFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameLost, Marshal.GetFunctionPointerForDelegate(OnFrameLostProc), m_hBoard);
                CheckIKapBoard(ret);
                OnTimeoutProc = new IKapCallBackProc(OnTimeoutFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_TimeOut, Marshal.GetFunctionPointerForDelegate(OnTimeoutProc), m_hBoard);
                CheckIKapBoard(ret);
                OnGrabStopProc = new IKapCallBackProc(OnGrabStopFunc);
                ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStop, Marshal.GetFunctionPointerForDelegate(OnGrabStopProc), m_hBoard);
                CheckIKapBoard(ret);


                //SetLineTrigger();

                if (ret == 1)
                    nRet = 0;
            }

            return nRet;
        }
        private void closeDevice()
        {
            UnRegisterCallback();
            CloseDevice();
            ReleaseEnvironment();
        }
        public void StartGrab()
        {
            //m_BufferFull = false;
            int ret = 0;
            // 开始图像采集。
            //
            // Start grabbing images.
            ret = IKapBoard.IKapStartGrab(m_hBoard, 0);
            CheckIKapBoard(ret);

            //// wait
            //ret = IKapBoard.IKapWaitGrab(m_hBoard);
            //CheckIKapBoard(ret);

            //ret = IKapBoard.IKapClearGrab(m_hBoard);
            //CheckIKapBoard(ret);
        }
        public void StopGrab()
        {
            int ret = 0;

            //// wait
            //ret = IKapBoard.IKapWaitGrab(m_hBoard);
            //CheckIKapBoard(ret);

            //停止图像采集。
            
            // Stop grabbing images.
            ret = IKapBoard.IKapStopGrab(m_hBoard);
            CheckIKapBoard(ret);
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

            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
            uint numCameras = 0;

            // 枚举可用相机的数量。在打开相机前，必须调用 ItkManGetDeviceCount() 函数。
            //
            // Enumerate the number of available cameras. Before opening the camera, ItkManGetDeviceCount() function must be called.
            res = IKapCLib.ItkManGetDeviceCount(ref numCameras);
            CheckIKapC(res);//need

            //// 当没有连接的相机时。
            ////
            //// When there is no connected cameras.
            //if (numCameras == 0)
            //{
            //    Console.Write("No camera.\n");
            //    IKapCLib.ItkManTerminate();
            //    Console.ReadLine();
            //    Environment.Exit(1);
            //}

            // 打开CameraLink相机。
            //
            // Open CameraLink camera.
            for (uint i = 0; i < numCameras; i++)
            {
                IKapCLib.ITKDEV_INFO di = new IKapCLib.ITKDEV_INFO();

                // 获取相机设备信息。
                //
                // Get camera device information.
                res = IKapCLib.ItkManGetDeviceInfo(i, ref di);
                Console.Write("Using camera: serial: {0}, name: {1}, interface: {2}.\n", di.SerialNumber, di.FullName, di.DeviceClass);

                // 当设备为 CameraLink 相机且序列号正确时。
                //
                // When the device is CameraLink camera and the serial number is proper.
                if (di.DeviceClass == "CameraLink" && (di.SerialNumber != "" || di.FullName == strSerialNumber))
                {
                    if (di.SerialNumber == strSerialNumber || di.FullName == strSerialNumber)
                    {
                        iNumber = (int)i;
                        break;
                    }

                    //IKapCLib.ITK_CL_DEV_INFO cl_board_info = new IKapCLib.ITK_CL_DEV_INFO();

                    //// 打开相机。
                    ////
                    //// Open camera.
                    //res = IKapCLib.ItkDevOpen(i, (int)ItkDeviceAccessMode.ITKDEV_VAL_ACCESS_MODE_EXCLUSIVE, ref m_hCamera);
                    //CheckIKapC(res);

                    //// 获取 CameraLink 相机设备信息。
                    ////
                    //// Get CameraLink camera device information.
                    //res = IKapCLib.ItkManGetCLDeviceInfo(i, ref cl_board_info);
                    //CheckIKapC(res);

                    //// 打开采集卡。
                    ////
                    //// Open frame grabber.
                    //m_hBoard = IKapBoard.IKapOpen(cl_board_info.HostInterface, cl_board_info.BoardIndex);
                    //if (m_hBoard.Equals(-1))
                    //    CheckIKapBoard((int)ErrorCode.IKStatus_OpenBoardFail);

                    break;
                }
            }

            return iNumber;
        }

        #endregion

        #region ITK_API_FUNTION

        // 相机设备句柄。
        //
        // Camera device handle.
        public IntPtr m_hCamera = new IntPtr(-1);

        // 采集卡设备句柄。
        //
        // Frame grabber device handle.
        public IntPtr m_hBoard = new IntPtr(-1);

        // 保存图像的文件名。
        //
        // File name of image.
        public string m_strFileName = "C:\\CSharpImage.tif";

        // 当前帧索引。
        //
        // Current frame index.
        public int m_nCurFrameIndex = 0;

        // 图像缓冲区申请的帧数。
        //
        // The number of frames requested by buffer.
        public int m_nTotalFrameCount = 1;

        /* @brief：判断 IKapC 函数是否成功调用。
         * @param[in] res：函数返回值。
         *
         * @brief：Determine whether the IKapC function is called successfully.
         * @param[in] res：Function return value. */
        static void CheckIKapC(uint res)
        {
            if (res != (uint)ItkStatusErrorId.ITKSTATUS_OK)
            {
                Console.Error.WriteLine("Error Code: {0}.\n", res.ToString("x8"));
                IKapCLib.ItkManTerminate();
                Console.ReadLine();
                MessageBox.Show($"Error Code: {res.ToString("x8")}.\n", "IKapC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        /* @brief：判断 IKapBoard 函数是否成功调用。
         * @param[in] ret：函数返回值。
         *
         * @brief：Determine whether the IKapBoard function is called successfully.
         * @param[in] ret：Function return value. */
        void CheckIKapBoard(int ret)
        {
            m_OperateMessage = string.Empty;
            if (ret != (int)ErrorCode.IK_RTN_OK)
            {
                string sErrMsg = "";
                IKapBoard.IKAPERRORINFO tIKei = new IKapBoardClassLibrary.IKapBoard.IKAPERRORINFO();

                // 获取错误码信息。
                //
                // Get error code message.
                IKapBoard.IKapGetLastError(ref tIKei, true);

                // 打印错误信息。
                //
                // Print error message.
                sErrMsg = string.Concat("Error",
                                        sErrMsg,
                                        "Board Type\t = 0x", tIKei.uBoardType.ToString("X4"), "\n",
                                        "Board Index\t = 0x", tIKei.uBoardIndex.ToString("X4"), "\n",
                                        "Error Code\t = 0x", tIKei.uErrorCode.ToString("X4"), "\n"
                                        );
                Console.Write(sErrMsg);
                Console.ReadLine();

                m_OperateMessage = sErrMsg;
                //MessageBox.Show(sErrMsg, "IKapBoard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //Environment.Exit(1);
            }
        }

        #region Callback
        delegate void IKapCallBackProc(IntPtr pParam);

        /* @brief：本函数被注册为一个回调函数。当图像采集开始时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When starting grabbing images, the function will be called. */
        private IKapCallBackProc OnGrabStartProc;

        /* @brief：本函数被注册为一个回调函数。当采集丢帧时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When grabbing frame lost, the function will be called. */
        private IKapCallBackProc OnFrameLostProc;

        /* @brief：本函数被注册为一个回调函数。当图像采集超时时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When grabbing images time out, the function will be called. */
        private IKapCallBackProc OnTimeoutProc;

        /* @brief：本函数被注册为一个回调函数。当一帧图像采集完成时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When a frame of image grabbing ready, the function will be called. */
        private IKapCallBackProc OnFrameReadyProc;

        /* @brief：本函数被注册为一个回调函数。当图像采集停止时，函数被调用。
         *
         * @brief：This function is registered as a callback function. When stopping grabbing images, the function will be called. */
        private IKapCallBackProc OnGrabStopProc;
        #endregion

        #region Callback
        /* @brief：本函数被注册为一个回调函数。当图像采集开始时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When starting grabbing images, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnGrabStartFunc(IntPtr pParam)
        {
            Console.WriteLine("Start grabbing image");
        }

        /* @brief：本函数被注册为一个回调函数。当采集丢帧时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When grabbing frame lost, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnFrameLostFunc(IntPtr pParam)
        {
            Console.WriteLine("Image frame lost");
        }

        /* @brief：本函数被注册为一个回调函数。当图像采集超时时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When grabbing images time out, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnTimeoutFunc(IntPtr pParam)
        {
            Console.WriteLine("Grab image timeout");
        }

        FreeImageAPI.FreeImageBitmap bmp = new FreeImageBitmap(1, 1);
        /* @brief：本函数被注册为一个回调函数。当一帧图像采集完成时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When a frame of image grabbing ready, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnFrameReadyFunc(IntPtr pParam)
        {
            //if (m_BufferFull)
            //    return;

            Console.WriteLine("Grab frame ready");
            m_TriggerComplete = false;

            IntPtr hDev = (IntPtr)pParam;
            IntPtr pUserBuffer = IntPtr.Zero;
            int nFrameSize = 0;
            int nFrameCount = 0;

         
            int nFrameWidth = 0;
            int nFrameHeight = 0;

            IKapBoard.IKAPBUFFERSTATUS status = new IKapBoard.IKAPBUFFERSTATUS();

            IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_COUNT, ref nFrameCount);
            IKapBoard.IKapGetBufferStatus(hDev, m_nCurFrameIndex, ref status);

            if (bmp != null)
                bmp.Dispose();

            // 当图像缓冲区满时。
            //
            // When the buffer is full.
            if (status.uFull == 1)
            {
                //m_BufferFull = true;
                // 获取一帧图像的大小。
                //
                // Get the size of a frame of image.
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_FRAME_SIZE, ref nFrameSize);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_WIDTH, ref nFrameWidth);
                IKapBoard.IKapGetInfo(hDev, (uint)INFO_ID.IKP_IMAGE_HEIGHT, ref nFrameHeight);
                // 获取缓冲区地址。
                //
                // Get the buffer address.
                IKapBoard.IKapGetBufferAddress(hDev, m_nCurFrameIndex, ref pUserBuffer);
                //bmp =
                //    new System.Drawing.Bitmap(nFrameWidth, nFrameHeight, nFrameWidth, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, pUserBuffer);
                bmp = new FreeImageAPI.FreeImageBitmap(nFrameWidth, nFrameHeight, nFrameWidth, PixelFormat.Format8bppIndexed, pUserBuffer);
                bmp.Rotate(_camCfg.Rotate);
                //m_bmpCurrent = bmp.ToBitmap();
                m_TriggerOK = true;

                // 保存图像。
                //
                // Save image.
                /*
                IKapBoard.IKapSaveBuffer(hDev,m_nCurFrameIndex,m_strFileName,(int)ImageCompressionFalg.IKP_DEFAULT_COMPRESSION);
                 */
            }
            m_nCurFrameIndex++;
            m_nCurFrameIndex = m_nCurFrameIndex % m_nTotalFrameCount;
            m_TriggerComplete = true;
        }

        /* @brief：本函数被注册为一个回调函数。当图像采集停止时，函数被调用。
         * @param[in] pParam：输入参数。
         *
         * @brief：This function is registered as a callback function. When stopping grabbing images, the function will be called.
         * @param[in] pParam：Input parameter. */
        public void OnGrabStopFunc(IntPtr pParam)
        {
            Console.WriteLine("Stop grabbing image");
        }
        #endregion

        #region member function

        /* @brief：初始化IKapC 运行环境。
         *
         * @brief：Initialize IKapC runtime environment. */
        private void InitEnvironment()
        {
            // IKapC 函数返回值。
            //
            // Return value of IKapC functions.
            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;

            res = IKapCLib.ItkManInitialize();
            CheckIKapC(res);//need
        }

        /* @brief：释放 IKapC 运行环境。
         *
         * @brief：Release IKapC runtime environment. */
        private void ReleaseEnvironment()
        {
            IKapCLib.ItkManTerminate();
        }

        /* @brief：配置相机设备。
         *
         * @brief：Configure camera device. */
        private void ConfigureCamera()
        {
            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
            uint numCameras = 0;

            // 枚举可用相机的数量。在打开相机前，必须调用 ItkManGetDeviceCount() 函数。
            //
            // Enumerate the number of available cameras. Before opening the camera, ItkManGetDeviceCount() function must be called.
            res = IKapCLib.ItkManGetDeviceCount(ref numCameras);
            //CheckIKapC(res);

            // 当没有连接的相机时。
            //
            // When there is no connected cameras.
            if (numCameras == 0)
            {
                Console.Write("No camera.\n");
                IKapCLib.ItkManTerminate();
                Console.ReadLine();
                Environment.Exit(1);
            }

            // 打开CameraLink相机。
            //
            // Open CameraLink camera.
            for (uint i = 0; i < numCameras; i++)
            {
                IKapCLib.ITKDEV_INFO di = new IKapCLib.ITKDEV_INFO();

                // 获取相机设备信息。
                //
                // Get camera device information.
                res = IKapCLib.ItkManGetDeviceInfo(i, ref di);
                Console.Write("Using camera: serial: {0}, name: {1}, interface: {2}.\n", di.SerialNumber, di.FullName, di.DeviceClass);

                // 当设备为 CameraLink 相机且序列号正确时。
                //
                // When the device is CameraLink camera and the serial number is proper.
                if (di.DeviceClass == "CameraLink" && di.SerialNumber != "")
                {
                    IKapCLib.ITK_CL_DEV_INFO cl_board_info = new IKapCLib.ITK_CL_DEV_INFO();

                    // 打开相机。
                    //
                    // Open camera.
                    res = IKapCLib.ItkDevOpen(i, (int)ItkDeviceAccessMode.ITKDEV_VAL_ACCESS_MODE_EXCLUSIVE, ref m_hCamera);
                    //CheckIKapC(res);

                    // 获取 CameraLink 相机设备信息。
                    //
                    // Get CameraLink camera device information.
                    res = IKapCLib.ItkManGetCLDeviceInfo(i, ref cl_board_info);
                    //CheckIKapC(res);

                    // 打开采集卡。
                    //
                    // Open frame grabber.
                    m_hBoard = IKapBoard.IKapOpen(cl_board_info.HostInterface, cl_board_info.BoardIndex);
                    if (m_hBoard.Equals(-1))
                        CheckIKapBoard((int)ErrorCode.IKStatus_OpenBoardFail);

                    break;
                }
            }
        }

        /* @brief：配置采集卡设备。
         *
         * @brief：Configure frame grabber device. */
        private void ConfigureFrameGrabber()
        {
            int ret = (int)ErrorCode.IK_RTN_OK;
            string configFileName;

            // 导入配置文件。
            //
            // Load configuration file.
            configFileName = GetOption();
            if (configFileName == null)
            {
                Console.WriteLine("Fail to get configuration, using default setting!\n");
            }
            else
            {
                ret = IKapBoard.IKapLoadConfigurationFromFile(m_hBoard, configFileName);
                CheckIKapBoard(ret);
            }

            // 设置图像缓冲区帧数。
            //
            // Set frame count of buffer.
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_COUNT, m_nTotalFrameCount);
            CheckIKapBoard(ret);

            // 设置超时时间。
            //
            // Set time out time.
            int timeout = -1;
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_TIME_OUT, timeout);
            CheckIKapBoard(ret);

            // 设置采集模式。
            //
            // Set grab mode.
            int grab_mode = (int)GrabMode.IKP_GRAB_NON_BLOCK;
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_GRAB_MODE, grab_mode);
            CheckIKapBoard(ret);

            // 设置传输模式。
            //
            // Set transfer mode.
            int transfer_mode = (int)FrameTransferMode.IKP_FRAME_TRANSFER_SYNCHRONOUS_NEXT_EMPTY_WITH_PROTECT;
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_FRAME_TRANSFER_MODE, transfer_mode);
            CheckIKapBoard(ret);

            // 注册回调函数
            //
            // Register callback functions.
            OnGrabStartProc = new IKapCallBackProc(OnGrabStartFunc);
            ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStart, Marshal.GetFunctionPointerForDelegate(OnGrabStartProc), m_hBoard);
            CheckIKapBoard(ret);
            OnFrameReadyProc = new IKapCallBackProc(OnFrameReadyFunc);
            ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameReady, Marshal.GetFunctionPointerForDelegate(OnFrameReadyProc), m_hBoard);
            CheckIKapBoard(ret);
            OnFrameLostProc = new IKapCallBackProc(OnFrameLostFunc);
            ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameLost, Marshal.GetFunctionPointerForDelegate(OnFrameLostProc), m_hBoard);
            CheckIKapBoard(ret);
            OnTimeoutProc = new IKapCallBackProc(OnTimeoutFunc);
            ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_TimeOut, Marshal.GetFunctionPointerForDelegate(OnTimeoutProc), m_hBoard);
            CheckIKapBoard(ret);
            OnGrabStopProc = new IKapCallBackProc(OnGrabStopFunc);
            ret = IKapBoard.IKapRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStop, Marshal.GetFunctionPointerForDelegate(OnGrabStopProc), m_hBoard);
            CheckIKapBoard(ret);
        }

        /* @brief：设置行触发参数。
         *
         * @brief：Set line trigger parameters. */
        void SetLineTrigger()
        {
            uint res = (uint)ItkStatusErrorId.ITKSTATUS_OK;
            int ret = (int)ErrorCode.IK_RTN_OK;

            // 设置CC1信号源。
            //
            // Set CC1 signal source.
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_CC1_SOURCE, (int)CCSource.IKP_CC_SOURCE_VAL_INTEGRATION_SIGNAL1);
            CheckIKapBoard(ret);

            // 设置积分控制方法触发信号源。
            //
            // Set integration control method trigger source.
            ret = IKapBoard.IKapSetInfo(m_hBoard, (uint)INFO_ID.IKP_INTEGRATION_TRIGGER_SOURCE, (int)IntegrationTriggerSource.IKP_INTEGRATION_TRIGGER_SOURCE_VAL_SHAFT_ENCODER1);
            CheckIKapBoard(ret);

            //// 设置同步模式。
            ////
            //// Set synchronization mode.
            //string syncName = "SynchronizationMode";
            //string syncParameter = "ExternalPulse";
            //res = IKapCLib.ItkDevFromString(m_hCamera, syncName, syncParameter);
            //CheckIKapC(res);
        }

        /* @brief：清除回调函数。
         *
         * @brief：Unregister callback functions. */
        private void UnRegisterCallback()
        {
            int ret = (int)ErrorCode.IK_RTN_OK;

            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStart);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameReady);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_FrameLost);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_TimeOut);
            ret = IKapBoard.IKapUnRegisterCallback(m_hBoard, (uint)CallBackEvents.IKEvent_GrabStop);
        }

        /* @brief：关闭设备。
         *
         * @brief：Close device. */
        private void CloseDevice()
        {
            // 关闭采集卡设备。
            //
            // Close frame grabber device.
            if (!m_hBoard.Equals(-1))
            {
                IKapBoard.IKapClose(m_hBoard);
                m_hBoard = (IntPtr)(-1);
            }

            // 关闭相机设备。
            //
            // Close camera device.
            if (!m_hCamera.Equals(-1))
            {
                IKapCLib.ItkDevClose(m_hCamera);
                m_hCamera = (IntPtr)(-1);
            }
        }

        /* @brief：选择用户配置文件。
         *
         * @brief：Select configuration file. */
        private string GetOption()
        {
            string vlcfFileName = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "vlcf文件(*.vlcf)|*.vlcf|所有文件(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "选择打开文件";
            if (ofd.ShowDialog() == DialogResult.OK)
                vlcfFileName = ofd.FileName;
            return vlcfFileName;
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

        #endregion
    }
}
