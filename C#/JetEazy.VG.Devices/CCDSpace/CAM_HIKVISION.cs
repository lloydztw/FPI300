
#define HIK

#if HIK
using JetEazy.CCDSpace;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JetEazy.ControlSpace
{
    public class CAM_HIKVISION
    {
        #region DEFINE MEMBERS
        MyCamera.MV_CC_DEVICE_INFO_LIST m_stDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        private MyCamera m_MyCamera = new MyCamera();
        bool m_bGrabbing = false;
        Thread m_hReceiveThread = null;
        MyCamera.MV_FRAME_OUT_INFO_EX m_stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();

        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        UInt32 m_nBufSizeForDriver = 0;
        IntPtr m_BufForDriver;
        private static Object BufForDriverLock = new Object();

        // ch:用于保存图像的缓存 | en:Buffer for saving image
        UInt32 m_nBufSizeForSaveImage = 0;
        IntPtr m_BufForSaveImage;

        private string m_cam_serialnumber = "";
        private bool m_triggerModeOn = false;
        private PictureBox picForMyCameraHandle;// = new PictureBox();
        private int m_number_no = 0;
        //private bool m_GetImageOK = false;


        //public uint m_nBufSizeForSaveImage;
        private byte[] m_pBufForSaveImage;         // 用于保存图像的缓存
        private bool m_GetImageOK;
        private Bitmap m_bmpCurrent;

        MyCamera.cbOutputdelegate cbImage;
        /// <summary>
        /// 相机帧数
        /// </summary>
        int m_nFrames = 0;

        private CameraPara m_CameraPara = null;
        #endregion

        //public event EventHandler<string> OnError;

        public CAM_HIKVISION(PictureBox pbx, int icam_no)
        {
            picForMyCameraHandle = pbx;
            m_number_no = icam_no;
        }
        public void Init(CameraPara eCameraPara)
        {
            m_CameraPara = eCameraPara;
            m_cam_serialnumber = m_CameraPara.SerialNumber;
            m_bGrabbing = false;
            cbImage = new MyCamera.cbOutputdelegate(_mvsCallback);
            m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];

            DeviceListAcq();
            OpenDevice();
            StartGrab();
        }
        //public void Init(string eCameraSerialNumber)
        //{
        //    m_cam_serialnumber = eCameraSerialNumber;
        //    m_bGrabbing = false;
        //    cbImage = new MyCamera.cbOutputdelegate(_mvsCallback);
        //    m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];

        //    DeviceListAcq();
        //    OpenDevice();
        //    StartGrab();
        //}
        public void Dispose()
        {
            StopGrab();
            CloseDevice();
        }
        public int GetFloatValue_NET(ref MyCamera.MVCC_FLOATVALUE stParam, string strKey = "ExposureTime")
        {
            //MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = m_MyCamera.MV_CC_GetFloatValue_NET(strKey, ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Get " + strKey + " Fail!", nRet);
            }
            return nRet;
        }
        public void SetExposure(float fexposure)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("ExposureTime", fexposure);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Exposure Time Fail!", nRet);
            }
        }
        public void SetGain(float fgain)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
            int nRet = m_MyCamera.MV_CC_SetFloatValue_NET("Gain", fgain);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Gain Fail!", nRet);
            }
        }
        /// <summary>
        /// 触发模式
        /// </summary>
        /// <param name="iMode">0 OFF 1 ON </param>
        public void TriggerMode(uint iMode)
        {
            TriggerMode2((MyCamera.MV_CAM_TRIGGER_MODE)iMode);
        }
        /// <summary>
        /// 触发模式
        /// </summary>
        /// <param name="iMode">0 OFF 1 ON </param>
        public void TriggerMode2(MyCamera.MV_CAM_TRIGGER_MODE iMode)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)iMode);
            m_triggerModeOn = iMode == MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON;
        }
        /// <summary>
        /// 触发模式 ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0; 1 - Line1; 2 - Line2; 3 - Line3;4 - Counter; 7 - Software
        /// </summary>
        /// <param name="iMode"> ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0; 1 - Line1; 2 - Line2; 3 - Line3;4 - Counter; 7 - Software</param>
        public void TriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE iSource)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)iSource);
        }
        public void TriggerSoftwareX()
        {
            m_GetImageOK = false;
            //m_GetImageOK = false;
            // ch:触发命令 | en:Trigger command
            int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Trigger Software Fail!", nRet);
            }
        }
        public Bitmap CaptureBmp(int roattion)
        {
            //if (false == m_GetImageOK)
            //{
            //    ShowErrorMsg("GetImage Error", 0);
            //    return null;
            //}
            if (false == m_bGrabbing)
            {
                ShowErrorMsg("Not Start Grabbing", 0);
                return null;
            }

            if (RemoveCustomPixelFormats(m_stFrameInfo.enPixelType))
            {
                ShowErrorMsg("Not Support!", 0);
                return null;
            }

            IntPtr pTemp = IntPtr.Zero;
            MyCamera.MvGvspPixelType enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
            if (m_stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 || m_stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
            {
                pTemp = m_BufForDriver;
                enDstPixelType = m_stFrameInfo.enPixelType;
            }
            else
            {
                UInt32 nSaveImageNeedSize = 0;
                MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

                lock (BufForDriverLock)
                {
                    if (m_stFrameInfo.nFrameLen == 0)
                    {
                        ShowErrorMsg("Save Bmp Fail!", 0);
                        return null;
                    }

                    if (IsMonoData(m_stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        nSaveImageNeedSize = (uint)m_stFrameInfo.nWidth * m_stFrameInfo.nHeight;
                    }
                    else if (IsColorData(m_stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                        nSaveImageNeedSize = (uint)m_stFrameInfo.nWidth * m_stFrameInfo.nHeight * 3;
                    }
                    else
                    {
                        ShowErrorMsg("No such pixel type!", 0);
                        return null;
                    }

                    if (m_nBufSizeForSaveImage < nSaveImageNeedSize)
                    {
                        if (m_BufForSaveImage != IntPtr.Zero)
                        {
                            Marshal.Release(m_BufForSaveImage);
                        }
                        m_nBufSizeForSaveImage = nSaveImageNeedSize;
                        m_BufForSaveImage = Marshal.AllocHGlobal((Int32)m_nBufSizeForSaveImage);
                    }

                    stConverPixelParam.nWidth = m_stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = m_stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = m_BufForDriver;
                    stConverPixelParam.nSrcDataLen = m_stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = m_stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = enDstPixelType;
                    stConverPixelParam.pDstBuffer = m_BufForSaveImage;
                    stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;
                    int nRet = m_MyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        ShowErrorMsg("Convert Pixel Type Fail!", nRet);
                        return null;
                    }
                    pTemp = m_BufForSaveImage;
                }
            }

            lock (BufForDriverLock)
            {
                if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    //************************Mono8 转 Bitmap*******************************
                    Bitmap bmp = new Bitmap(m_stFrameInfo.nWidth, m_stFrameInfo.nHeight, m_stFrameInfo.nWidth * 1, PixelFormat.Format8bppIndexed, pTemp);

                    System.Drawing.Imaging.ColorPalette cp = bmp.Palette;
                    // init palette
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    // set palette back
                    bmp.Palette = cp;
                    //bmp.Save("image.bmp", ImageFormat.Bmp);

                    switch (roattion)
                    {
                        case 90:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 270:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 180:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }

                    //bmp.Save("image.bmp", ImageFormat.Bmp);
                    return bmp;
                }
                else
                {
                    //*********************BGR8 转 Bitmap**************************
                    try
                    {
                        Bitmap bmp = new Bitmap(m_stFrameInfo.nWidth, m_stFrameInfo.nHeight, m_stFrameInfo.nWidth * 3, PixelFormat.Format24bppRgb, pTemp);
                        //bmp.Save("image.bmp", ImageFormat.Bmp);

                        switch (roattion)
                        {
                            case 90:
                                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                break;
                            case 270:
                                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                break;
                            case 180:
                                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                break;
                        }

                        return bmp;
                    }
                    catch
                    {
                        ShowErrorMsg("Write File Fail!", 0);
                    }
                }
            }

            //ShowErrorMsg("Save Succeed!", 0);
            return null;
        }
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
                    return m_bmpCurrent.Clone() as Bitmap;
                }
                if (watch.ElapsedMilliseconds > 1000)
                    break;
            }
            return null;
        }

        #region PRIVATE FUNTION

        /// <summary>
        /// 连续采集模式
        /// </summary>
        /// <param name="iMode">0 single frame 1 muti 2 continue</param>
        private void AcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE iMode)
        {
            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)iMode);
        }
        private void AcquisitionStart()
        {
            // ch:触发命令 | en:Trigger command
            int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("AcquisitionStart");
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Acquisition Start Fail!", nRet);
            }
        }
        private void AcquisitionStop()
        {
            // ch:触发命令 | en:Trigger command
            int nRet = m_MyCamera.MV_CC_SetCommandValue_NET("AcquisitionStop");
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Acquisition Stop Fail!", nRet);
            }
        }

        private void OpenDevice()
        {
            if (m_stDeviceList.nDeviceNum == 0)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }

            int device_index = GetDeviceNumber(m_cam_serialnumber);
            if (device_index >= m_stDeviceList.nDeviceNum)
            {
                ShowErrorMsg("over device count", 0);
                return;
            }
            m_number_no = device_index;
            // ch:获取选择的设备信息 | en:Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device =
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[device_index],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));

            // ch:打开设备 | en:Open device
            if (null == m_MyCamera)
            {
                m_MyCamera = new MyCamera();
                if (null == m_MyCamera)
                {
                    return;
                }
            }

            int nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return;
            }

            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_MyCamera.MV_CC_DestroyDevice_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return;
            }

            string _featureFilenamePath = m_CameraPara.CfgPath + "\\" + m_CameraPara.SerialNumber + ".mfs";
            nRet = m_MyCamera.MV_CC_FeatureLoad_NET(_featureFilenamePath);

            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("FeatureLoad fail!", nRet);
                //return;
            }

            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                int nPacketSize = m_MyCamera.MV_CC_GetOptimalPacketSize_NET();
                if (nPacketSize > 0)
                {
                    nRet = m_MyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                    if (nRet != MyCamera.MV_OK)
                    {
                        ShowErrorMsg("Set Packet Size failed!", nRet);
                    }
                }
                else
                {
                    ShowErrorMsg("Get Packet Size failed!", nPacketSize);
                }
            }

            m_MyCamera.MV_CC_RegisterImageCallBack_NET(cbImage, (IntPtr)device_index);

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            //m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            //m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
            //set triggerSource = software
            TriggerSource(MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            AcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            TriggerMode2(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            //close Balance White Auto
            nRet = m_MyCamera.MV_CC_SetEnumValue_NET("BalanceWhiteAuto", 0);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set BalanceWhiteAuto Fail!", nRet);
            }
            //From MVS Setting
            //MyCamera.MVCC_ENUMVALUE mVCC_ENUMVALUE = new MyCamera.MVCC_ENUMVALUE();
            //m_MyCamera.MV_CC_GetEnumValue_NET("PixelFormat", ref mVCC_ENUMVALUE);

            ////设置Enum型参数-相机图像格式
            ////注意点1：相机图像格式设置时，只有在MV_CC_Startgrab接口调用前才能设置,取流过程中，不能修改图像格式
            //nRet = m_MyCamera.MV_CC_SetEnumValue_NET("PixelFormat", (uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8);
            //if (nRet != MyCamera.MV_OK)
            //{
            //    ShowErrorMsg("Set PixelFormat fail!", nRet);
            //}
            //bnGetParam_Click(null, null);// ch:获取参数 | en:Get parameters

            //// ch:控件操作 | en:Control operation
            //SetCtrlWhenOpen();
        }
        private void CloseDevice()
        {

            AcquisitionMode(MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
            TriggerMode2(MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

            // ch:取流标志位清零 | en:Reset flow flag bit
            if (m_bGrabbing == true)
            {
                m_bGrabbing = false;
                //m_hReceiveThread.Join();
            }

            if (m_BufForDriver != IntPtr.Zero)
            {
                Marshal.Release(m_BufForDriver);
            }
            if (m_BufForSaveImage != IntPtr.Zero)
            {
                Marshal.Release(m_BufForSaveImage);
            }

            // ch:关闭设备 | en:Close Device
            m_MyCamera.MV_CC_CloseDevice_NET();
            m_MyCamera.MV_CC_DestroyDevice_NET();

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenClose();
        }
        private void StartGrab()
        {
            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;

            //m_hReceiveThread = new Thread(ReceiveThreadProcess);
            //m_hReceiveThread.Start();

            m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stFrameInfo.enPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Undefined;
            // ch:开始采集 | en:Start Grabbing
            int nRet = m_MyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_bGrabbing = false;
                //m_hReceiveThread.Join();
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return;
            }

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenStartGrab();
        }
        private void StopGrab()
        {
            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;
            //m_hReceiveThread.Join();

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_MyCamera.MV_CC_StopGrabbing_NET();
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!", nRet);
            }

            // ch:控件操作 | en:Control Operation
            //SetCtrlWhenStopGrab();
        }

        public int iCount = 0;
        int iCountTemp = 0;
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        private void ReceiveThreadProcess()
        {
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = m_MyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                ShowErrorMsg("Get PayloadSize failed", nRet);
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

            MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
            MyCamera.MV_DISPLAY_FRAME_INFO stDisplayInfo = new MyCamera.MV_DISPLAY_FRAME_INFO();

            while (m_bGrabbing)
            {
                lock (BufForDriverLock)
                {
                    nRet = m_MyCamera.MV_CC_GetOneFrameTimeout_NET(m_BufForDriver, nPayloadSize, ref stFrameInfo, 1000);
                    if (nRet == MyCamera.MV_OK)
                    {
                        m_stFrameInfo = stFrameInfo;
                        //m_GetImageOK = true;

                        if (!watch.IsRunning)
                            watch.Start();
                        if (watch.ElapsedMilliseconds > 1000)
                        {
                            watch.Reset();
                            iCount = iCountTemp;
                            iCountTemp = 0;
                        }
                        else
                            iCountTemp++;
                    }
                }

                if (nRet == MyCamera.MV_OK)
                {
                    if (RemoveCustomPixelFormats(stFrameInfo.enPixelType))
                    {
                        continue;
                    }
                    //stDisplayInfo.hWnd = pictureBox1.Handle;
                    //stDisplayInfo.pData = m_BufForDriver;
                    //stDisplayInfo.nDataLen = stFrameInfo.nFrameLen;
                    //stDisplayInfo.nWidth = stFrameInfo.nWidth;
                    //stDisplayInfo.nHeight = stFrameInfo.nHeight;
                    //stDisplayInfo.enPixelType = stFrameInfo.enPixelType;
                    //m_MyCamera.MV_CC_DisplayOneFrame_NET(ref stDisplayInfo);
                }
                else
                {
                    //if (m_triggerModeOn)
                    //{
                    //    Thread.Sleep(5);
                    //}
                }
            }
        }
        /// <summary>
        /// 通过相机序列号 对应相机的编号
        /// </summary>
        /// <param name="strSerialNumber">相机序列号</param>
        /// <returns></returns>
        private int GetDeviceNumber(string strSerialNumber)
        {
            int iNumber = 0;

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            //cbDeviceList.Items.Clear();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return iNumber;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    if (gigeInfo.chSerialNumber == strSerialNumber)
                    {
                        iNumber = i;
                        break;
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));

                    if (usbInfo.chSerialNumber == strSerialNumber)
                    {
                        iNumber = i;
                        break;
                    }

                }
            }

            return iNumber;
        }
        private void DeviceListAcq()
        {
            // ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            //cbDeviceList.Items.Clear();
            m_stDeviceList.nDeviceNum = 0;
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_stDeviceList);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!", 0);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    //if (gigeInfo.chUserDefinedName != "")
                    //{
                    //    cbDeviceList.Items.Add("GEV: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    //}
                    //else
                    //{
                    //    cbDeviceList.Items.Add("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    //}
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    //if (usbInfo.chUserDefinedName != "")
                    //{
                    //    cbDeviceList.Items.Add("U3V: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                    //}
                    //else
                    //{
                    //    cbDeviceList.Items.Add("U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    //}
                }
            }

            // ch:选择第一项 | en:Select the first item
            //if (m_stDeviceList.nDeviceNum != 0)
            //{
            //    cbDeviceList.SelectedIndex = 0;
            //}
        }
        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            //MessageBox.Show(errorMsg, "PROMPT");
            //if (OnError != null)
            //    OnError(this, errorMsg);
        }

        private Boolean IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;

                default:
                    return false;
            }
        }
        /************************************************************************
         *  @fn     IsColorData()
         *  @brief  判断是否是彩色数据
         *  @param  enGvspPixelType         [IN]           像素格式
         *  @return 成功，返回0；错误，返回-1 
         ************************************************************************/
        private Boolean IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY:
                    return true;

                default:
                    return false;
            }
        }
        // ch:去除自定义的像素格式 | en:Remove custom pixel formats
        private bool RemoveCustomPixelFormats(MyCamera.MvGvspPixelType enPixelFormat)
        {
            Int32 nResult = ((int)enPixelFormat) & (unchecked((Int32)0x80000000));
            if (0x80000000 == nResult)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void _mvsCallback(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO pFrameInfo, IntPtr pUser)
        {
            int nIndex = (int)pUser;

            // ch:抓取的帧数 | en:Aquired Frame Number
            ++m_nFrames;

            _mvsGetImageNow(pData, pFrameInfo, nIndex);

            //_mvsCallbackTest(pData, pFrameInfo, IntPtr.Zero);
        }
        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="stFrameInfo"></param>
        /// <param name="nIndex"></param>
        private void _mvsGetImageNow(IntPtr pData, MyCamera.MV_FRAME_OUT_INFO stFrameInfo, int nIndex)
        {
            string[] path = { "image1.bmp", "image2.bmp", "image3.bmp", "image4.bmp" };
            int nRet;

            if ((3 * stFrameInfo.nFrameLen + 2048) > m_nBufSizeForSaveImage)
            {
                m_nBufSizeForSaveImage = 3 * stFrameInfo.nFrameLen + 2048;
                m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
            }

            #region MV Rotate
            //IntPtr pImage0 = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
            //MyCamera.MV_CC_ROTATE_IMAGE_PARAM mV_CC_ROTATE_IMAGE_PARAM
            //        = new MyCamera.MV_CC_ROTATE_IMAGE_PARAM();
            //MyCamera.MV_IMG_ROTATION_ANGLE mV_IMG_ROTATION_ANGLE
            //    = new MyCamera.MV_IMG_ROTATION_ANGLE();
            //switch (m_CameraPara.Rotate)
            //{
            //    case 0:
            //        break;
            //    case 90:
            //        mV_IMG_ROTATION_ANGLE = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_90;
            //        break;
            //    case 180:
            //        mV_IMG_ROTATION_ANGLE = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_180;
            //        break;
            //    case 270:
            //        mV_IMG_ROTATION_ANGLE = MyCamera.MV_IMG_ROTATION_ANGLE.MV_IMAGE_ROTATE_270;
            //        break;
            //}
            //mV_CC_ROTATE_IMAGE_PARAM.enPixelType = stFrameInfo.enPixelType;
            //mV_CC_ROTATE_IMAGE_PARAM.nWidth = stFrameInfo.nWidth;
            //mV_CC_ROTATE_IMAGE_PARAM.nHeight = stFrameInfo.nHeight;
            //mV_CC_ROTATE_IMAGE_PARAM.pSrcData = pData;
            //mV_CC_ROTATE_IMAGE_PARAM.nSrcDataLen = stFrameInfo.nFrameLen;
            //mV_CC_ROTATE_IMAGE_PARAM.pDstBuf = pImage0;
            //mV_CC_ROTATE_IMAGE_PARAM.nDstBufLen = stFrameInfo.nFrameLen;
            //mV_CC_ROTATE_IMAGE_PARAM.nDstBufSize = m_nBufSizeForSaveImage;
            //mV_CC_ROTATE_IMAGE_PARAM.enRotationAngle = mV_IMG_ROTATION_ANGLE;
            //nRet = m_MyCamera.MV_CC_RotateImage_NET(ref mV_CC_ROTATE_IMAGE_PARAM);
            //if (MyCamera.MV_OK != nRet)
            //{

            //}

            #endregion

            IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
            MyCamera.MV_SAVE_IMAGE_PARAM_EX stSaveParam = new MyCamera.MV_SAVE_IMAGE_PARAM_EX();
            stSaveParam.enImageType = MyCamera.MV_SAVE_IAMGE_TYPE.MV_Image_Bmp;
            stSaveParam.enPixelType = stFrameInfo.enPixelType;
            stSaveParam.pData = pData;
            stSaveParam.nDataLen = stFrameInfo.nFrameLen;
            stSaveParam.nHeight = stFrameInfo.nHeight;
            stSaveParam.nWidth = stFrameInfo.nWidth;
            stSaveParam.pImageBuffer = pImage;
            stSaveParam.nBufferSize = m_nBufSizeForSaveImage;
            stSaveParam.nJpgQuality = 80;
            nRet = m_MyCamera.MV_CC_SaveImageEx_NET(ref stSaveParam);
            if (MyCamera.MV_OK != nRet)
            {
            }
            else
            {
                MemoryStream memory = new MemoryStream(m_pBufForSaveImage);
                Bitmap bmp = (Bitmap)Image.FromStream(memory, true, true);
                memory.Close();

                //switch (90)
                //{
                //    case 90:
                //        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                //        break;
                //    case 270:
                //        bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                //        break;
                //    case 180:
                //        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                //        break;
                //}


                m_bmpCurrent = bmp;

                //m_bmpCurrent = rotate(bmp, 90);
                //bmp.Dispose();

                //if (INIClass.BitRotateFlipType != RotateFlipType.RotateNoneFlipNone)
                //    m_pMyCamera[nIndex].Bmp.RotateFlip(INIClass.BitRotateFlipType);

                m_GetImageOK = true;

                //FileStream file = new FileStream(path[nIndex], FileMode.Create, FileAccess.Write);
                //file.Write(m_pMyCamera[nIndex].m_pBufForSaveImage, 0, (int)stSaveParam.nImageLen);
                //file.Close();
                //string temp = "No." + (nIndex + 1).ToString() + "Device Save Succeed!";
                //ShowErrorMsg(temp,0);
            }
        }

        #endregion

        #region TEST_ROTATION
#if (true)
        Bitmap m_bmpTestBuf = null;
        private void _mvsCallbackTest(IntPtr pDataCB, MyCamera.MV_FRAME_OUT_INFO stFrameInfo, IntPtr pUser)
        {
            //var pData = _get_buffer_ptr(pDataCB, ref stFrameInfo);
            IntPtr pData = pDataCB;

            int width = stFrameInfo.nWidth;
            int height = stFrameInfo.nHeight;
            int bytesPerPixel;

            if (IsMonoData(stFrameInfo.enPixelType))
                bytesPerPixel = 1;
            else if (IsColorData(stFrameInfo.enPixelType))
                bytesPerPixel = 3;
            else
                bytesPerPixel = 0;

            if (bytesPerPixel > 0)
            {
                int dstW = (RotateAngle == 90 || RotateAngle == 270) ? height : width;
                int dstH = (RotateAngle == 90 || RotateAngle == 270) ? width : height;
                if (m_bmpTestBuf == null)
                    m_bmpTestBuf = new Bitmap(dstW, dstH, PixelFormat.Format24bppRgb);

                var bmp = m_bmpTestBuf;
                BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, dstW, dstH), ImageLockMode.ReadWrite, bmp.PixelFormat);
                switch (RotateAngle)
                {
                    case 90:
                        copy_bits_090(pData, bmpd.Scan0, width, height, width * 1, 1, bmpd.Stride, 3);
                        break;
                    case 180:
                        copy_bits_180(pData, bmpd.Scan0, width, height, width * 1, 1, bmpd.Stride, 3);
                        break;
                    case 270:
                        copy_bits_270(pData, bmpd.Scan0, width, height, width * 1, 1, bmpd.Stride, 3); break;
                    //case 0:
                    default:
                        copy_bits_000(pData, bmpd.Scan0, width, height, width * 1, 1, bmpd.Stride, 3);
                        break;
                }
                bmp.UnlockBits(bmpd);
                m_bmpCurrent = bmp;
                m_GetImageOK = true;
            }
            else
            {
                // the format is not supported yet !!!
                //m_bmpCurrent = bmp;
                //m_GetImageOK = true;
            }
        }
        private IntPtr _get_buffer_ptr(IntPtr pSrcData, ref MyCamera.MV_FRAME_OUT_INFO stFrameInfo)
        {
            

            // Input:
            //  (1) m_stFrameInfo
            //  (2) m_BufForDriver
            //  (3) m_BufForSaveImage
            //  (4) m_nBufSizeForSaveImage

            IntPtr pResult;

            if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 ||
                stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed ||
                stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
            {
                pResult = pSrcData;
            }
            else
            {
                MyCamera.MvGvspPixelType enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                UInt32 nSaveImageNeedSize = 0;
                MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                //lock (BufForDriverLock)
                {
                    if (stFrameInfo.nFrameLen == 0)
                    {
                        ShowErrorMsg("Save Bmp Fail!", 0);
                        return IntPtr.Zero;
                    }

                    if (IsMonoData(stFrameInfo.enPixelType))
                    {
                        //enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        nSaveImageNeedSize = (uint)stFrameInfo.nWidth * stFrameInfo.nHeight;
                    }
                    else if (IsColorData(stFrameInfo.enPixelType))
                    {
                        //enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;
                        nSaveImageNeedSize = (uint)stFrameInfo.nWidth * stFrameInfo.nHeight * 3;
                    }
                    else
                    {
                        ShowErrorMsg("No such pixel type!", 0);
                        return IntPtr.Zero;
                    }

                    if (m_nBufSizeForSaveImage < nSaveImageNeedSize)
                    {
                        if (m_BufForSaveImage != IntPtr.Zero)
                        {
                            Marshal.Release(m_BufForSaveImage);
                        }
                        m_nBufSizeForSaveImage = nSaveImageNeedSize;
                        m_BufForSaveImage = Marshal.AllocHGlobal((Int32)m_nBufSizeForSaveImage);
                    }

                    stConverPixelParam.nWidth = stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = pSrcData;
                    stConverPixelParam.nSrcDataLen = stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = enDstPixelType;

                    // using member data  m_BufForSaveImage
                    stConverPixelParam.pDstBuffer = m_BufForSaveImage;
                    stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;

                    int nRet = m_MyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        ShowErrorMsg("Convert Pixel Type Fail!", nRet);
                        return IntPtr.Zero;
                    }

                    pResult = m_BufForSaveImage;
                }
            }
            return pResult;
        }

        private Bitmap rotate(Bitmap srcBmp, int angle)
        {
            int bytesPerPixel = 0;

            switch(srcBmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    bytesPerPixel = 3;
                    break;
                case PixelFormat.Format8bppIndexed:
                    bytesPerPixel = 1;
                    break;
            }

            //if (bmpSrc.PixelFormat== PixelFormat.Format24bppRgb)
            //    bytesPerPixel = 3;
            //else if (IsColorData(stFrameInfo.enPixelType))
            //    bytesPerPixel = 3;
            //else
            //    bytesPerPixel = 0;

            if (bytesPerPixel > 0)
            {
                int width = srcBmp.Width;
                int height = srcBmp.Height;
                int dstW = width;
                int dstH = height;
                if (angle == 90 || angle == 270)
                {
                    dstW = height;
                    dstH = width;
                }

                if (m_bmpTestBuf == null)
                {
                    m_bmpTestBuf = new Bitmap(dstW, dstH, PixelFormat.Format24bppRgb);
                }
                var dstBmp = m_bmpTestBuf;

                BitmapData srcBmpd = srcBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, srcBmp.PixelFormat);
                BitmapData dstBmpd = dstBmp.LockBits(new Rectangle(0, 0, dstW, dstH), ImageLockMode.ReadWrite, dstBmp.PixelFormat);

                switch (angle)
                {
                    case 90:
                        copy_bits_090(srcBmpd.Scan0, dstBmpd.Scan0, width, height, width * 1, 1, dstBmpd.Stride, 3);
                        break;
                    case 180:
                        copy_bits_180(srcBmpd.Scan0, dstBmpd.Scan0, width, height, width * 1, 1, dstBmpd.Stride, 3);
                        break;
                    case 270:
                        copy_bits_270(srcBmpd.Scan0, dstBmpd.Scan0, width, height, width * 1, 1, dstBmpd.Stride, 3); break;
                    //case 0:
                    default:
                        copy_bits_000(srcBmpd.Scan0, dstBmpd.Scan0, width, height, width * 1, 1, dstBmpd.Stride, 3);
                        break;
                }
                dstBmp.UnlockBits(dstBmpd);
                srcBmp.UnlockBits(srcBmpd);
                return dstBmp;
            }
            else
            {
                // the format is not supported yet !!!
                //m_bmpCurrent = bmp;
                //m_GetImageOK = true;
                return null;
            }
        }

        private void copy_bits_000(IntPtr srcPtr, IntPtr dstPtr, int width, int height,
                                        int srcStride, int srcBytesPerPixel,
                                        int dstStride, int dstBytesPerPixel
                                    )
        {
            int N = width * height;
            unsafe
            {
                Parallel.For(0, N, (n) =>
                {
                    int y = n / width;
                    int x = n % width;

                    byte* pSrc = (byte*)srcPtr;
                    byte* pDst = (byte*)dstPtr;
                    int j = (x * srcBytesPerPixel) + y * srcStride;
                    int k = (x * dstBytesPerPixel) + y * dstStride;

                    convert_pixel_color(pSrc + j, pDst + k, srcBytesPerPixel);
                });
            }
        }

        private void copy_bits_090(IntPtr srcPtr, IntPtr dstPtr, int width, int height,
                                        int srcStride, int srcBytesPerPixel,
                                        int dstStride, int dstBytesPerPixel
                                    )
        {
            //int N = width * height;
            unsafe
            {
                Parallel.For(0, height, (y) =>
                {
                    int x2 = y;
                    byte* pSrc = (byte*)srcPtr + y * srcStride;
                    byte* pDst = (byte*)dstPtr + x2 * dstBytesPerPixel;
                    for (int x = 0; x < width; x++)
                    {
                        //int y2 = x
                        //int j = (x * srcBytesPerPixel) + y * srcStride;
                        //int k = (x2 * dstBytesPerPixel) + y2 * dstStride;

                        convert_pixel_color(pSrc, pDst, srcBytesPerPixel);

                        pSrc = pSrc + srcBytesPerPixel;
                        pDst = pDst + dstStride;
                    }
                });
            }
        }
        private void copy_bits_180(IntPtr srcPtr, IntPtr dstPtr, int width, int height,
                                        int srcStride, int srcBytesPerPixel,
                                        int dstStride, int dstBytesPerPixel
                                    )
        {
            int N = width * height;
            unsafe
            {
                Parallel.For(0, N, (n) =>
                {
                    int y = n / width;
                    int x = n % width;
                    int x2 = x;
                    int y2 = width - 1 - y;

                    byte* pSrc = (byte*)srcPtr;
                    byte* pDst = (byte*)dstPtr;
                    int j = (x * srcBytesPerPixel) + y * srcStride;
                    int k = (x2 * dstBytesPerPixel) + y2 * dstStride;

                    convert_pixel_color(pSrc + j, pDst + k, srcBytesPerPixel);
                });
            }
        }
        private void copy_bits_270(IntPtr srcPtr, IntPtr dstPtr, int width, int height,
                                        int srcStride, int srcBytesPerPixel,
                                        int dstStride, int dstBytesPerPixel
                                    )
        {
            int N = width * height;
            unsafe
            {
                Parallel.For(0, N, (n) =>
                {
                    int y = n / width;
                    int x = n % width;
                    int x2 = height - 1 - x;
                    int y2 = width - 1 - y;

                    byte* pSrc = (byte*)srcPtr;
                    byte* pDst = (byte*)dstPtr;
                    int j = (x * srcBytesPerPixel) + y * srcStride;
                    int k = (x2 * dstBytesPerPixel) + y2 * dstStride;

                    convert_pixel_color(pSrc + j, pDst + k, srcBytesPerPixel);
                });
            }
        }
        private unsafe void convert_pixel_color(byte* src, byte* dst, int srcBytesPerPixel)
        {
            if (srcBytesPerPixel == 1)
            {
                dst[0] = src[0];
                dst[1] = src[0];
                dst[2] = src[0];
            }
            else if(srcBytesPerPixel == 3)
            {
                dst[0] = src[0];
                dst[1] = src[1];
                dst[2] = src[2];
            }
        }
#endif
        #endregion


        public int RotateAngle
        {
            get;
            set;
        } = 0;
    }
}
#endif
