#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-05-05 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using MvCamCtrl.NET;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace EzCamera.Driver.Hikvision
{
    /// <summary>
    /// 以 Huang版 為基礎, 改成單相機版本.
    /// 是對原生 MvCamera 的簡易包裝, 
    /// 來提供: 
    /// <br/> 
    /// <br/> (1) Callbacks, FrameBuffer 管理
    /// <br/> (2) Construct, Open, Close 流程控制
    /// <br/> (3) TriggerSource, TriggerMode, AcquisitionMode 設定
    /// <br/> (4) Camera Properties 讀(get)寫(set) 與 cache.
    /// <br/> 
    /// <br/>
    /// 其中 (4)(3) 的部分比較明確, 所以放到 HikvCamBase
    /// 而 (1)(2) 所參考 Huang 與 Gaara 不太相同的代碼, 
    /// 尚有些部分可能需要增減, 所以拉到 HikvCameraH,
    /// 未來穩定後, HikvCamBase 與 HikvCameraH 可以合為單一 Class.
    /// </summary>
    public partial class HikvCameraH : HikvCamBase
    {
        #region CALLBACK_MEMBERS
        /// <summary>
        /// 流里面图像
        /// </summary>
        private MyCamera.cbOutputExdelegate m_callbackImage;
        /// <summary>
        /// 相机异常回调函数
        /// </summary>
        private MyCamera.cbExceptiondelegate m_callbackException;
        #endregion

        #region PRIVATE_STATUS_DATA
        private bool m_bCreatedOpen = false;
        private bool m_bGrabbing = false;
        #endregion

        #region ERROR_MESSAGING
        /// <summary>
        /// 错误信息處理 (舊接口)
        /// </summary>
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            //統一調用至 _ERR
            _ERR(nErrorNum, csMessage);
        }
        /// <summary>
        /// 返回第一個 err
        /// </summary>
        private int _FIRST_ERROR(params int[] rets)
        {
            foreach (int ret in rets)
            {
                if (ret != MyCamera.MV_OK)
                    return ret;
            }
            return MyCamera.MV_OK;
        }
        #endregion


#if (false && 此處已交給_Factory_處理)
        /// <summary>
        /// 遍历相机
        /// </summary>
        /// <returns>若未获取到相机返回null</returns>
        public string[] ErgodicCamera()
        {
           GC.Collect();
            //创建设备列表
            List<string> cameraList = new List<string>();

            m_pDeviceList.nDeviceNum = 0;

            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE |
                MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
           
            if (0 != nRet)
            {
                ShowErrorMsg("遍历相机失败!", 0);
                return null;
            }
            // 在窗体列表中显示设备名
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
               
                
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cameraList.Add("GEV_" + gigeInfo.chUserDefinedName + "_" + gigeInfo.chSerialNumber);
                    }
                    else
                    {
                        cameraList.Add("GEV_" + gigeInfo.chManufacturerName + "_" + gigeInfo.chModelName + "_" + gigeInfo.chSerialNumber);
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    //if (usbInfo.chUserDefinedName != "")
                    //{
                    //    cameraList.Add("U3V_" + usbInfo.chUserDefinedName + "_" + usbInfo.chSerialNumber );
                    //}
                    //else
                    //{
                    //    cameraList.Add("U3V_" + usbInfo.chManufacturerName + "_" + usbInfo.chModelName + "_" + usbInfo.chSerialNumber );
                    //}

                    if(usbInfo.chSerialNumber == "00DA1455246")
                    {
                         cameraList.Add("U3V_" + usbInfo.chManufacturerName + "_" + usbInfo.chModelName + "_" + usbInfo.chSerialNumber );
                        
                    }
                }
            }

            if (cameraList.Count > 0)
            {
                return cameraList.ToArray();
            }
            else
            {
                return null;
            }

        }
#endif

        /// <summary>
        /// 建構 Hikvision 相機設備
        /// </summary>
        /// <returns>海康錯誤碼</returns>
        public int ConstructDevice(ref MyCamera.MV_CC_DEVICE_INFO deviceInfo)
        {
            _mvDeviceInfo = deviceInfo;

            //绑定回调函数
            m_callbackImage = new MyCamera.cbOutputExdelegate(callback_Image);
            //相机异常函数回调
            m_callbackException = new MyCamera.cbExceptiondelegate(callback_Exception);

            int ret = -1;

            //(0) 建構 MyCamera 
            if (null == _myCamera)
            {
                _myCamera = new MyCamera();
                if (null == _myCamera)
                {
                    //ShowErrorMsg("相机 " + i.ToString() + "初始化错误", 0);
                    _ERR(ret = -1000, "無法建構 MyCamera()");
                    return ret;
                }
            }

            //(1) Create/Open Device
            ret = OpenDevice();
            if (MyCamera.MV_OK != ret)
                return ret;

            //(2) 探测网络最佳包大小(只对GigE相机有效).
            //    (有 err 繼續往下執行?)
            int ret2 = detectOptimalPackageSize();

            //(3) 設定默認的 采集模式, 觸發源, 與觸發模式
            //      AquisitionMode: Continuous (連續)
            //      TriggerSource:  Software
            //      TriggerMode:    ON
            SetDefaultAcqAndTriggerModes();

            //(4) 註冊回調函式
            int ret4 = registerCallbacks();

            //(5) StartGrabbing
            int ret5 = StartGrabbing();

            //返回 ret, ret2, ret4 或 ret5
            ret = _FIRST_ERROR(ret, ret2, ret4, ret5);
            return ret;
        }

        public bool IsOpen()
        {
            return m_bCreatedOpen;
        }

        /// <summary>
        /// 關閉相機設備
        /// </summary>
        public int CloseDevice(bool releaseAll = true)
        {
            int ret = MyCamera.MV_OK;

            if (releaseAll)
            {
                SetDefaultAcqAndTriggerModes();
            }

            if (IsGrabbing())
                StopGrabbing();

            if (releaseAll)
            {
#if (OPT_RESERVED_釋放其他資源)
                if (m_BufForDriver != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForDriver);
                }

                if (m_BufForSaveImage != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForSaveImage);
                }
#endif
                m_currentBmpReady.Set();
            }

            if (m_bCreatedOpen)
            {
                int ret1 = _myCamera.MV_CC_CloseDevice_NET();
                if (ret1 != MyCamera.MV_OK)
                    _ERR(ret1, "錯誤於 MV_CC_CloseDevice_NET");

                int ret2 = _myCamera.MV_CC_DestroyDevice_NET();
                if (ret2 != MyCamera.MV_OK)
                    _ERR(ret2, "錯誤於 MV_CC_DestroyDevice_NET");

                m_bCreatedOpen = false;

                ret = _FIRST_ERROR(ret1, ret2);
            }

            return ret;
        }

        /// <summary>
        /// 開啟相機設備
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public int OpenDevice(int delay = 0)
        {
            int ret = MyCamera.MV_OK;

            if (!m_bCreatedOpen)
            {
                //(1) Create Device
                ret = _myCamera.MV_CC_CreateDevice_NET(ref _mvDeviceInfo);
                if (MyCamera.MV_OK != ret)
                {
                    _ERR(ret, "錯誤於 MV_CC_CreateDevice_NET");
                    return ret;
                }

                // DELAY
                if (delay > 0) Thread.Sleep(delay);

                //(2) Open Device
                ret = _myCamera.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != ret)
                {
                    _ERR(ret, "錯誤於 MV_CC_OpenDevice_NET");

                    // DELAY
                    if (delay > 0) Thread.Sleep(delay);
                    // 強制 Destroy Device
                    _myCamera.MV_CC_DestroyDevice_NET();
                    return ret;
                }

                m_bCreatedOpen = true;
            }

            return ret;
        }

        #region PRIVATE_開啟相機輔助函式
        private int detectOptimalPackageSize()
        {
            // 探测网络最佳包大小 (只对GigE相机有效)

            int ret = MyCamera.MV_OK;
            if (_mvDeviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = _myCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    ret = _myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (ret != MyCamera.MV_OK)
                    {
                        //ShowErrorMsg("Set Packet Size failed!", ret);
                        _ERR(ret, $"無法設定數據包大小 {nPacketSize}");
                    }
                }
                else
                {
                    //ShowErrorMsg("Get Packet Size failed!", nPacketSize);
                    _ERR(ret = -1001, $"最佳數據包大小 {nPacketSize} 必須 > 0!");
                }
            }
            return ret;
        }
        private int registerCallbacks()
        {
            //暫時不需要 pUser 
            IntPtr pUser = IntPtr.Zero;

            //註冊 影像回調函式
            int ret1 = _myCamera.MV_CC_RegisterImageCallBackEx_NET(m_callbackImage, pUser);
            GC.KeepAlive(m_callbackImage);
            if (ret1 != 0)
                _ERR(ret1, "無法註冊 影像回調函式!");

            //註冊 異常回調函式
            int ret2 = _myCamera.MV_CC_RegisterExceptionCallBack_NET(m_callbackException, pUser);
            GC.KeepAlive(m_callbackException);
            if (ret2 != 0)
                _ERR(ret2, "無法註冊 異常回調函式!");


            return ret1 != MyCamera.MV_OK ? ret1 : ret2;
        }
        /// <summary>
        /// 重新初始化掉线相机
        /// </summary>
        private int reInitDevice()
        {
            ////IntPtr pUser = IntPtr.Zero;
            //////重新为相机注册异常回调函数
            ////int nRet = _myCamera.MV_CC_RegisterExceptionCallBack_NET(m_callbackException, (IntPtr)pUser);
            ////GC.KeepAlive(m_callbackException);
            ////if (MyCamera.MV_OK != nRet)
            ////{
            ////    return nRet;
            ////}
            ////////重新为相机注册回调函数
            ////nRet = _myCamera.MV_CC_RegisterImageCallBackEx_NET(m_callbackImage, (IntPtr)pUser);
            ////GC.KeepAlive(m_callbackImage);
            ////if (MyCamera.MV_OK != nRet)
            ////{
            ////    return nRet;
            ////}

            int ret1 = registerCallbacks();

            // 重新回復原有 Properties (從 cache 取得的數據)
            disableBalanceWhiteAuto();
            disableGainAuto();
            setExposureTime(ExposureTime);
            setGain(Gain);

            // Trigger Source
            setTriggerSource(TriggerSource);

            int ret2 = StartGrabbing();
            return ret1 != MyCamera.MV_OK ? ret1 : ret2;
        }
        #endregion

        /// <summary>
        /// 設定默認的 采集模式, 觸發源, 與觸發模式
        /// <br/>   AquisitionMode: Continuous (連續)
        /// <br/>   TriggerSource:  Software
        /// <br/>   TriggerMode:    ON
        /// </summary>
        public int SetDefaultAcqAndTriggerModes()
        {
            int ret = setAcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            int ret2 = setTriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            int ret3 = setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            ret = _FIRST_ERROR(ret, ret2, ret3);
            return ret;
        }

        public bool IsGrabbing()
        {
            return m_bGrabbing;
        }

        /// <summary>
        /// 开始采集图像
        /// </summary>
        public int StartGrabbing()
        {
            //设置SDK内部图像缓存节点个数
            //int nRet = m_myCamera.MV_CC_SetImageNodeNum_NET(0);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("相机" + cameraIndex.ToString() + "设置SDK内部图像缓存节点失败!", nRet);
            //    return;
            //}

            m_bGrabbing = true;
            int ret = _myCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != ret)
            {
                //ShowErrorMsg("取图失败!", nRet);
                _ERR(ret, "錯誤於 MV_CC_StartGrabbing_NET");
            }
            return ret;
        }

        /// <summary>
        /// 相继停止采集图像
        /// </summary>
        public int StopGrabbing()
        {
            m_bGrabbing = false;
            int ret = _myCamera.MV_CC_StopGrabbing_NET();
            if (MyCamera.MV_OK != ret)
            {
                //ShowErrorMsg("相机" + cameraIndex.ToString() + "取图失败!", nRet);
                _ERR(ret, "錯誤於 MV_CC_StopGrabbing_NET");
            }
            return ret;
        }

        /// <summary>
        /// 下達軟體觸發指令
        /// </summary>
        public int TriggerSoftware()
        {
            return setCommand("TriggerSoftware");
        }

        /// <summary>
        /// 下達軟體觸發指令, 並等待影像擷取完成, 傳回該張影像
        /// <br/> (1) quickAccess= true 時, 直接存取 FrameBuf, 調用者不需要 Dispose(), 但是要自行負責多線程衝突.
        /// <br/> (2) quickAccess= false 時, 安全 clone FrameBuf, 調用者負責 Dispose().
        /// <br/> (3) 目前搭配: TriggerSource = Software , TriggerMode = ON
        /// </summary>
        public Bitmap TriggerSoftwareCapture(bool quickAccess = false)
        {
            m_currentBmpReady.Reset();
            TriggerSoftware();

            m_currentBmpReady.WaitOne();

            if (quickAccess)
            {
                // 直接快速傳回 FrameBuffer 的影像,
                // 調用者必須自己處理多線程共享問題.
                return m_currentBmp;
            }
            else
            {
                // 保護性 lock
                lock (m_currentBmpLocker)
                {
                    return (Bitmap)m_currentBmp.Clone();
                }
            }
        }
    }


    //--- CALLBACKS_實作 --------------------------------------------------------------------------------------
    #region CALLBACKS_實作
    partial class HikvCameraH
    {
        #region PRIVATE_IMAGE_BUF
        private AutoResetEvent m_currentBmpReady = new AutoResetEvent(false);
        protected object m_currentBmpLocker = new object();
        protected Bitmap m_currentBmp;
        #endregion

        /// <summary>
        /// 相机异常回调函数
        /// </summary>
        private void callback_Exception(uint nMsgType, IntPtr pUser)
        {
            if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
            {
                #region OLD_CODE
                // 停止采集
                //int cameraIndex = (int)pUser;
                //m_bGrabbing = false;
                //_myCamera.MV_CC_StopGrabbing_NET();

                // 关闭设备
                //_myCamera.MV_CC_CloseDevice_NET();
                //_myCamera.MV_CC_DestroyDevice_NET();

                //// 获取选择的设备信息 
                // MyCamera.MV_CC_DEVICE_INFO device =
                //    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[cameraIndex],
                //                                                  typeof(MyCamera.MV_CC_DEVICE_INFO));
                #endregion

                // 關閉設備 
                // (1) 但是不釋放 buf, cache 等資源
                // (2) 內部會自動調用 StopGrabbing()
                CloseDevice(releaseAll: false);


                // 重新打開設備
                // (ToDO) 後續必須加入 Timeout 檢查!
                while (true)
                {
                    #region OLD_CODE
                    //int nRet = _myCamera.MV_CC_CreateDevice_NET(ref _deviceInfo);
                    //if (MyCamera.MV_OK != nRet)
                    //{
                    //    Thread.Sleep(5);
                    //    continue;
                    //}
                    //nRet = _myCamera.MV_CC_OpenDevice_NET();
                    //if (MyCamera.MV_OK != nRet)
                    //{
                    //    Thread.Sleep(5);
                    //    _myCamera.MV_CC_DestroyDevice_NET();
                    //    continue;
                    //}
                    //else
                    //{
                    //    nRet = InitCamera(cameraIndex);
                    //    if (nRet != MyCamera.MV_OK)
                    //    {
                    //        Thread.Sleep(5);
                    //        _myCamera.MV_CC_DestroyDevice_NET();
                    //        continue;
                    //    }
                    //    break;
                    //}
                    #endregion

                    int ret = OpenDevice(5);
                    if (ret != MyCamera.MV_OK)
                    {
                        Thread.Sleep(5);
                        CloseDevice(releaseAll: false);
                        continue;
                    }

                    ret = reInitDevice();
                    if (ret != MyCamera.MV_OK)
                    {
                        Thread.Sleep(5);
                        CloseDevice(releaseAll: false);
                        continue;
                    }
                    else
                    {
                        // 成功
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// 取流回调函数 
        /// </summary>
        private void callback_Image(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            #region 直接調用海康_將影像_Render_到_hwndDirectDisplay_所指定的視窗
            if (false && m_hwndDirectDisplay != IntPtr.Zero)
            {
                var stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();
                stDisplayInfo.hWnd = m_hwndDirectDisplay;
                stDisplayInfo.pData = pData;
                stDisplayInfo.nDataLen = pFrameInfo.nFrameLen;
                stDisplayInfo.nWidth = pFrameInfo.nWidth;
                stDisplayInfo.nHeight = pFrameInfo.nHeight;
                stDisplayInfo.enPixelType = pFrameInfo.enPixelType;
                _myCamera.MV_CC_DisplayOneFrame_NET(ref stDisplayInfo);
            }
            #endregion

            lock (m_currentBmpLocker)
            {
                m_currentBmp?.Dispose();
                m_currentBmp = get_frame_bmp(pData, pFrameInfo);
            }
            m_currentBmpReady.Set();

            fireFrameCapturedEvent(m_currentBmp);
        }

        /// <summary>
        /// 單純從 stFrameInfo 取出 C# Bitmap
        /// <br/> (1) 調用者必須負責 Dispose()
        /// <br/> (2) 所有影像顏色轉換與旋轉交於後續處理
        /// <br/> (3) 此處不進行 lock()
        /// </summary>
        private Bitmap get_frame_bmp(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo)
        {
            Bitmap bmpFrame = null;
            if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
            {
                bmpFrame = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth, PixelFormat.Format8bppIndexed, pData);
                ColorPalette cp = bmpFrame.Palette;
                // init palette
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                // set palette back
                bmpFrame.Palette = cp;

                //旋转90度配合 (交給後續處理)
                //bmpFrame.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_BGR8_Packed ||
                     stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
            {
                //RGB8 (RGB 24 bit)
                bmpFrame = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pData);

                // BGR -> RGB 交給後續使用 OpenCV 處理 !!!
                if (false)
                {
                    //BGR2RGB 很可能會傳回新 new 的 Bitmap
                    //檢查是否應該 Dispose (Huang's bug)
                    var bmp = BGR2RGB(bmpFrame);
                    if (bmp != bmpFrame)
                    {
                        bmpFrame.Dispose();
                        bmpFrame = bmp;
                    }
                }
            }
            else if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_RGBA8_Packed)
            {
                //RGBA8 (ARGB 32bit)
                bmpFrame = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 4, PixelFormat.Format32bppArgb, pData);
            }
            return bmpFrame;
        }
    }
    #endregion


    //--- 圖像函式 --------------------------------------------------------------------------------------------
    #region BITMAP_FUNCTIONS_圖像函式
    partial class HikvCameraH
    {
        #region PRIVATE_DATA
        /// <summary>
        /// 监控每个相机是否要保存图像
        /// </summary>
        private bool m_bSaveImg = false;
        /// <summary>
        /// 图像保存路径
        /// </summary>
        private string m_saveBmpFilePath = "C:\\";
        /// <summary>
        /// 图像保存名称
        /// </summary>
        private string m_saveBmpName = "temp.bmp";
        /// <summary>
        /// 保存图像的缓存大小
        /// </summary>
        private UInt32 m_nBufSizeForSaveImage = 0;
        /// <summary>
        /// 用于保存图像的缓存
        /// </summary>
        private IntPtr m_pBufForSaveImage = IntPtr.Zero;
        #endregion

        /// <summary>
        /// BGR2RGB
        /// 【注意】: Huang 的寫法, 很可能會生成新 new 的 Bitmap, 調用者要檢查, 並負責 Dispose.
        /// </summary>
        private Bitmap BGR2RGB(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb) return bmp;

            int h = bmp.Height;
            int w = bmp.Width;

            Bitmap bmpOut = new Bitmap(w, h, PixelFormat.Format24bppRgb);    //每像素3字节
            BitmapData dataIn = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataOut = bmpOut.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* pIn = (byte*)(dataIn.Scan0.ToPointer());      //指向源文件首地址
                byte* pOut = (byte*)(dataOut.Scan0.ToPointer());  //指向目标文件首地址
                for (int y = 0; y < dataIn.Height; y++)  //列扫描
                {
                    for (int x = 0; x < dataIn.Width; x++)   //行扫描
                    {
                        pOut[0] = pIn[2];     //R分量
                        pOut[1] = pIn[1];     //G分量
                        pOut[2] = pIn[0];     //B分量
                        pIn += 3;
                        pOut += 3;      //指针后移3个分量位置
                    }
                    pIn += dataIn.Stride - dataIn.Width * 3;
                    pOut += dataOut.Stride - dataOut.Width * 3;
                }
            }
            bmpOut.UnlockBits(dataOut);
            bmp.UnlockBits(dataIn);
            return bmpOut;
        }

        /// <summary>
        /// 旋转图像
        /// </summary>
        /// <param name="cameraIndex">相机索引</param>
        /// <param name="angle">旋转角度(只支持90,180,270)</param>
        private void RotationBmp(int angle, IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo)
        {
            //public MvGvspPixelType enPixelType;
            //public uint nWidth;
            //public uint nHeight;
            //public IntPtr pSrcData;
            //public uint nSrcDataLen;
            //public IntPtr pDstBuf;
            //public uint nDstBufLen;
            //public uint nDstBufSize;

            var mvccRotationParam = new MyCamera.MV_CC_ROTATE_IMAGE_PARAM
            {
                enPixelType = stFrameInfo.enPixelType,
                nWidth = stFrameInfo.nWidth,
                nHeight = stFrameInfo.nHeight,
                pSrcData = pData
            };

            if (angle == 90)
            {
                mvccRotationParam.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_90;
            }
            else if (angle == 180)
            {
                mvccRotationParam.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_180;
            }
            else if (angle == 270)
            {
                mvccRotationParam.enRotationAngle = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_270;
            }
            else
            {
                return;
            }

            int nRet = _myCamera.MV_CC_RotateImage_NET(ref mvccRotationParam);
            if (MyCamera.MV_OK != nRet)
            {
                // ShowErrorMsg("相机" + cameraIndex.ToString() + "旋转失败!", nRet);
                _ERR(nRet, "錯誤於 MV_CC_RotateImage_NET (旋转失败)");
                return;
            }
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        private void SaveImage(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo, int dummy = 0)
        {
            //uint m_nBufSizeForSaveImage = 0;
            //IntPtr m_pBufForSaveImage = IntPtr.Zero;

            if ((3 * stFrameInfo.nFrameLen + 2048) > m_nBufSizeForSaveImage ||
                m_pBufForSaveImage == IntPtr.Zero)
            {
                m_nBufSizeForSaveImage = 3 * stFrameInfo.nFrameLen + 2048;
                m_pBufForSaveImage = Marshal.AllocHGlobal((Int32)m_nBufSizeForSaveImage);
            }

            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX
            {
                //选择保存图像格式
                enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp,
                enPixelType = stFrameInfo.enPixelType,
                pData = pData,
                nDataLen = stFrameInfo.nFrameLen,
                nHeight = stFrameInfo.nHeight,
                nWidth = stFrameInfo.nWidth,
                pImageBuffer = m_pBufForSaveImage,
                nBufferSize = m_nBufSizeForSaveImage
            };

            //stSaveParam.nJpgQuality = 80;//存Jpeg时有效
            int nRet = _myCamera.MV_CC_SaveImageEx_NET(ref stSaveParam);
            if (MyCamera.MV_OK != nRet)
            {
                //ShowErrorMsg("保存图像失败!", 0);
                _ERR(nRet, "保存图像失败!");
            }
            else
            {
                Byte[] bArrBufForSaveImage = new Byte[stSaveParam.nImageLen];
                Marshal.Copy(m_pBufForSaveImage, bArrBufForSaveImage, 0, (Int32)stSaveParam.nImageLen);
                Marshal.Release(m_pBufForSaveImage);
                FileStream file = new FileStream(m_saveBmpFilePath + m_saveBmpName, FileMode.Create, FileAccess.Write);
                file.Write(bArrBufForSaveImage, 0, (int)stSaveParam.nImageLen);
                file.Close();
                //string temp = "No." + (nIndex + 1).ToString() + "Device Save Succeed!";
                //ShowErrorMsg("保存图像成功!路径 " + saveBmpFilePath + saveBmpName, 0);
                _ERR(0, "保存图像成功!路径 " + m_saveBmpFilePath + m_saveBmpName);
            }
        }
    }
    #endregion


    //--- 其他不常用函式 --------------------------------------------------------------------------------------
    #region MISC_FUNCTIONS_其他不常用函式
    partial class HikvCameraH
    {
        #region MISC_DATA
        /// <summary>
        /// 显示句柄 (Window Handle)
        /// </summary>
        private IntPtr m_hwndDirectDisplay = IntPtr.Zero;
        /// <summary>
        /// 用于从驱动获取图像的缓存大小
        /// </summary>
        private uint m_nBufSizeForDriver = 0;
        /// <summary>
        /// 用于从驱动获取图像的缓存
        /// </summary>
        private IntPtr m_BufForDriver = IntPtr.Zero;
        #endregion

        /// <summary>
        /// 获取相机丢失帧数
        /// </summary>
        public int GetLostFrame()
        {
            if (m_bGrabbing)
            {
                MyCamera.MV_ALL_MATCH_INFO pstInfo = new MyCamera.MV_ALL_MATCH_INFO();
                if (_mvDeviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_NET_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_NET_DETECT();
                    pstInfo.nInfoSize = (uint)Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_NET_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    _myCamera.MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_NET_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_NET_DETECT));
                    int count = (int)MV_NetInfo.nLostFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else if (_mvDeviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_MATCH_INFO_USB_DETECT MV_NetInfo = new MyCamera.MV_MATCH_INFO_USB_DETECT();
                    pstInfo.nInfoSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    pstInfo.nType = MyCamera.MV_MATCH_TYPE_USB_DETECT;
                    int size = Marshal.SizeOf(MV_NetInfo);
                    pstInfo.pInfo = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(MV_NetInfo, pstInfo.pInfo, false);
                    _myCamera.MV_CC_GetAllMatchInfo_NET(ref pstInfo);
                    MV_NetInfo = (MyCamera.MV_MATCH_INFO_USB_DETECT)Marshal.PtrToStructure(pstInfo.pInfo, typeof(MyCamera.MV_MATCH_INFO_USB_DETECT));
                    int count = (int)MV_NetInfo.nErrorFrameCount;
                    Marshal.FreeHGlobal(pstInfo.pInfo);
                    return count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 设定设备中图像有效负载大小
        /// </summary>
        public void SetPayloadSize()
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = _myCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                //ShowErrorMsg("获取有效负载大小失败", nRet);
                _ERR(nRet, "获取有效负载大小失败");
                return;
            }
            UInt32 nPayloadSize = stParam.nCurValue;
            if (nPayloadSize > m_nBufSizeForDriver)
            {
                if (m_BufForDriver != IntPtr.Zero)
                {
                    Marshal.Release(m_BufForDriver);
                }
                m_nBufSizeForDriver = nPayloadSize;
                m_BufForDriver = Marshal.AllocHGlobal((Int32)m_nBufSizeForDriver);
            }
            if (m_BufForDriver == IntPtr.Zero)
            {
                return;
            }
        }

        /// <summary>
        /// 外部控件数组绑定相机图像句柄数组
        /// </summary>
        /// <param name="window">外部控件数组引用</param>
        public void SetDirectDisplayWindowHandle(IntPtr hwnd)
        {
            m_hwndDirectDisplay = hwnd;
        }
    }
    #endregion


    //--- HUANG_黃接口 --------------------------------------------------------------------------------------
    #region HUANG_FUNCTIONS
    public class HikvCameraH2 : HikvCameraH
    {
        public HikvCameraH2()
        {
        }

        public bool InitialOK
        {
            get => IsOpen();
        }
        public object CameraLock
        {
            get => m_currentBmpLocker;
        }
        public Bitmap bmpNow
        {
            get => m_currentBmp;
        }

        /// <summary>
        /// 相机软触发(单张取像)
        /// </summary>
        public void TriggerSoftware(int dummy = 0)
        {
            if (IsGrabbing())
            {
                int ret = _myCamera.MV_CC_ClearImageBuffer_NET();
                if (MyCamera.MV_OK != ret)
                {
                    //>>> ShowErrorMsg("清理缓冲帧失败!", ret);
                    _ERR(ret, "錯誤於 MV_CC_ClearImageBuffer_NET", "清理缓冲帧失败!");
                    return;
                }

                //////设置相机采集模式为触发模式(采集单张)
                ////ret = _myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                ////if (MyCamera.MV_OK != ret)
                ////{
                ////    ShowErrorMsg("设定软触发模式失败!", ret);
                ////}

                //下達軟體觸發命令
                //[NOTE] HikvCamBase 內部已經自帶 Error Message 處理
                ret = setCommand("TriggerSoftware");
            }
            else
            {
                //ShowErrorMsg("采集模式为打开!", 0);
                _ERR(-1002, "必須先 StopGrabbing");
            }
        }
        /// <summary>
        /// 设定相机连续采集模式,
        /// 【注意】
        ///     此處實際上 huang 是把 TriggerMode 設定為 OFF,
        ///     函式命名語意 會跟 AcquisitionMode = Continous 混淆 !!!
        /// </summary>
        public void SetContinuesMode(int dummy = 0)
        {
            ////int nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
            ////if (MyCamera.MV_OK != nRet)
            ////{
            ////    ShowErrorMsg("设定连续采集模式失败!", nRet);
            ////}

            //[NOTE] HikvCamBase 內部已經自帶 Error Message 處理
            base.setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
        }
        /// <summary>
        /// 设定相机单张采集模式
        /// (把 TriggerMode 設定為 ON)
        /// </summary>
        public void SetTriggerMode(int dummy = 0)
        {
            //int nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("设定单张采集模式失败!", nRet);
            //}

            //[NOTE] HikvCamBase 內部已經自帶 Error Message 處理
            base.setTriggerMode(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
        }
        /// <summary>
        /// 触发单张
        /// </summary>
        public void TriggerSingle(int dummy = 0)
        {
            #region OLD_CODE
            //////先清空图像
            ////Universal.operMVCameraClass.bmpNows[cameraIndex] = null;

            //////再发触发命令
            ////int nRet = m_pMyCamera[cameraIndex].MV_CC_SetCommandValue_NET("TriggerSoftware");
            ////if (MyCamera.MV_OK != nRet)
            ////{
            ////    ShowErrorMsg("Trigger Software Fail!", nRet);
            ////}
            #endregion

            // 先清空图像
            lock (m_currentBmpLocker)
            {
                m_currentBmp?.Dispose();
                m_currentBmp = null;
            }

            // 下達軟體觸發命令
            //[NOTE] HikvCamBase 內部已經自帶 Error Message 處理
            int ret = setCommand("TriggerSoftware");
        }
        /// <summary>
        /// 設定觸發源為 SOURCE_SOFTWARE
        /// </summary>
        public void SetSoftware(int dummy = 0)
        {
            #region OLD_CODE
            //int nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerSource", 
            //    (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            //if (MyCamera.MV_OK != nRet)
            //{
            //    ShowErrorMsg("SetSoftware Fail!", nRet);
            //}
            #endregion

            //[NOTE] HikvCamBase 內部已經自帶 Error Message 處理
            int ret = base.setTriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
        }
    }
    #endregion
}
