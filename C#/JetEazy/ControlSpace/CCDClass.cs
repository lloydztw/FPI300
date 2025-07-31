//#define IDS
#define EPIX
//#define TIS
//#define TISUSB
//#define PTG
//#define AISYS
//#define USBCAM

//#define HIK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using JetEazy;

#if TIS || TISUSB
using TIS.Imaging;
using TIS.Imaging.VCDHelpers;
#endif

#if PTG
using FlyCapture2Managed;
#endif

#if AISYS
using AxAltairUDrv;
using AxAxAltairUDrv;
#endif

#if USBCAM
using Camera_NET;
#endif


using JetEazy.BasicSpace;

namespace JetEazy.ControlSpace
{
    public class CCDCollectionClass
    {
        int CCDHead = 0;
        int LiveCycle = 0;

        public string TESTFilePath
        {
            set
            {
                foreach (CCDClass ccd in CCDList)
                {
                    ccd.TESTFilePath = value;
                }
            }
        }

        public Bitmap bmpALL = new Bitmap(1, 1);
        
        List<CCDClass> CCDList = new List<CCDClass>();
        List<int> CCDIndexList = new List<int>();

        public CCDCollectionClass(int ccdhead)
        {
            CCDHead = ccdhead;
        }

        public void Add(CCDClass ccd)
        {
            CCDList.Add(ccd);
            CCDIndexList.Add(ccd.CCDBMP.Length);
        }

        public CCDClass GetCCDKind(int index)
        {
            return CCDList[index];
        }

        public Bitmap GetBitmap(int CameraNo)
        {
            //if (CameraNo == 11)
            //    CameraNo = CameraNo;
            
            int CCDInsideIndex = 0;

            int CCDSection = GetCCDSection(CameraNo, ref CCDInsideIndex);

            //if (CCDList[CCDSection].GetCCDType() == CCDTYPEEnum.FILE)
            //    CCDInsideIndex = CameraNo;

            return CCDList[CCDSection].CCDBMP[CCDInsideIndex];
        }
        public Bitmap GetImage(int CameraNo)
        {

            int CCDInsideIndex = 0;
            int CCDSection = GetCCDSection(CameraNo, ref CCDInsideIndex);

            //if (CCDList[CCDSection].GetCCDType() == CCDTYPEEnum.FILE)
            //    CCDInsideIndex = CameraNo;

            if (CCDList[CCDSection].GetCCDType() != CCDTYPEEnum.FILE)
                return CCDList[CCDSection].GetImage(CCDInsideIndex);
            else
                return GetBitmap(CameraNo);
        }
        public Bitmap GetFileImage(int CameraNo)
        {

            int CCDInsideIndex = 0;
            int CCDSection = GetCCDSection(CameraNo, ref CCDInsideIndex);

            return CCDList[CCDSection].GetImage(CCDInsideIndex);
        }

        public void GetImage()
        {
            foreach (CCDClass ccd in CCDList)
            {
                ccd.GetImage();
            }
        }
        public void GetImage(int CameraNo, ref Bitmap FromBMP)
        {
            FromBMP.Dispose();
            FromBMP = new Bitmap(GetImage(CameraNo));
        }
        public void GetImage(int FromCameraNo, int ToCameraNo)
        {
            int i = 0;

            while (i < CCDHead)
            {
                if (i >= FromCameraNo && i <= ToCameraNo)
                {
                    GetImage(i);
                }
                i++;
            }
        }

        #region MultiThrad Region

        public Bitmap[] Mbmp;
        public int MCameraNo;
        Object obj = new object();

        public void GetImageM()
        {
            int CCDInsideIndex = 0;
            int CCDSection = GetCCDSection(MCameraNo, ref CCDInsideIndex);

            lock (obj)
            {
                Mbmp[CCDInsideIndex].Dispose();
                Mbmp[CCDInsideIndex] = CCDList[CCDSection].GetImage(CCDInsideIndex);
            }
        }

        #endregion


        public int GetCCDSection(int Index, ref int insideindex)
        {
            int ccdsetion = 0;
            int Sum = 0;
            int LastSum = 0;

            foreach (int xx in CCDIndexList)
            {
                Sum += xx;

                if (Index >= Sum)
                {
                    ccdsetion++;
                    LastSum += xx;
                }
            }
            if (ccdsetion > 0)
            {
                //insideindex = Index - CCDIndexList[ccdsetion - 1];
                insideindex = Index - LastSum;
            }
            else
                insideindex = Index;

            return ccdsetion;
        }

        public void SetExposure(int[] exposure)
        {
            int i = 0;

            while(i < exposure.Length)
            {
                SetExposure(i, exposure[i]);
                i++;
            }
        }


        public void SetExposure(int CameraNo, int Exposure)
        {
            int CCDInsideIndex = 0;
            int CCDSection = GetCCDSection(CameraNo, ref CCDInsideIndex);

            CCDList[CCDSection].SetExposure(CCDInsideIndex, Exposure);
        }
        public void SetExposure(string Str)
        {
            string[] str = Str.Split(',');

            int i = 0;

            while (i < CCDHead)
            {
                SetExposure(i, int.Parse(str[i]));
                i++;
            }
        }
        public void GenAllBMP(Size AllViewSize)
        {
            GenAllBMP(AllViewSize, 1f);
        }
        public void GenAllBMP(Size AllViewSize,float ratio)
        {
            bmpALL.Dispose();
            bmpALL = new Bitmap((int)((float)AllViewSize.Width * ratio), (int)((float)AllViewSize.Height * ratio));
        }

        public void InitialAISYSInFormLoad()
        {
            foreach (CCDClass ccd in CCDList)
            {
                if (ccd.GetCCDType() == CCDTYPEEnum.AISYS)
                {
                    ccd.InitialAISYSInFormLoad();
                }
            }
        }
        public void InitialAISYSBeforeFormLoad(Control.ControlCollection controls)
        {
            foreach (CCDClass ccd in CCDList)
            {
                if (ccd.GetCCDType() == CCDTYPEEnum.AISYS)
                {
                    ccd.InitialAISYSBeForeFormLoad(controls);
                }
            }
        }

        public Bitmap GetAllBMP(bool IsChangeSize, Size AllViewSize, Point[] CCDLocations, int[] CameraNos,int ccdwidth,int ccdheight,int ccdhead)
        {
            return GetAllBMP(IsChangeSize, AllViewSize, CCDLocations, CameraNos, ccdwidth, ccdheight, 1f, ccdhead);

        }
        public Bitmap GetAllBMP(List<Point> ccdlocationlist,int ccdwidth,int ccdheight)
        {
            Graphics g = Graphics.FromImage(bmpALL);

            int i = 0;
            foreach (Point location in ccdlocationlist)
            {
                Bitmap bmp = GetBitmap(i);
                //bmp.Save( m_BasePath + @"LOA\CHECK" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);
                g.DrawImage(bmp,
                    new Rectangle(new Point(location.X, location.Y), new Size(ccdwidth, ccdheight)),
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    GraphicsUnit.Pixel);

                i++;
            }

            g.Dispose();

            return bmpALL;
        }

        public Bitmap GetAllBMP(bool IsChangeSize, Size AllViewSize, Point[] CCDLocations, int[] CameraNos, int ccdwidth, int ccdheight, float ratio,int ccdhead)
        {
            if (IsChangeSize)
            {
                GenAllBMP(AllViewSize,ratio);
            }

            ////if (!Universal.IsDebug)
            //{
            //    GetImage(LiveCycle);

            //    LiveCycle++;

            //    if (LiveCycle >= CCDHead)
            //        LiveCycle = 0;
            //}

            Graphics g = Graphics.FromImage(bmpALL);

            int i = 0;
            foreach (Point location in CCDLocations)
            {
                Bitmap bmp = GetImage(CameraNos[i % ccdhead]);

                //Bitmap bmp = GetBitmap(CameraNos[i % ccdhead]);

                //bmp.Save( m_BasePath + @"LOA\CHECK" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

                Point NewLocation = new Point(location.X, location.Y);

                NewLocation.X = (int)((float)location.X * ratio);
                NewLocation.Y = (int)((float)location.Y * ratio);

                int CCDW = (int)((float)bmp.Width * ratio);
                int CCDH = (int)((float)bmp.Height * ratio);

                g.DrawImage(bmp,
                    new Rectangle(new Point(NewLocation.X, NewLocation.Y), new Size(CCDW,CCDH)),
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    GraphicsUnit.Pixel);

                i++;
            }

            g.Dispose();

            //bmpALL.Save( m_BasePath + @"LOA\CHECK" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

            return bmpALL;

        }
        public Bitmap GetAllBMP(Point[] CCDLocations, Bitmap[] bmpwork, int ccdwidth, int ccdheight, float ratio)
        {
            Graphics g = Graphics.FromImage(bmpALL);

            int i = 0;
            foreach (Point location in CCDLocations)
            {
                Bitmap bmp = bmpwork[i];

                //bmp.Save( m_BasePath + @"LOA\CHECK" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

                Point NewLocation = new Point(location.X, location.Y);

                NewLocation.X = (int)((float)location.X * ratio);
                NewLocation.Y = (int)((float)location.Y * ratio);

                int CCDW = (int)((float)ccdwidth * ratio);
                int CCDH = (int)((float)ccdheight * ratio);

                g.DrawImage(bmp,
                    new Rectangle(new Point(NewLocation.X, NewLocation.Y), new Size(CCDW, CCDH)),
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    GraphicsUnit.Pixel);

                i++;
            }

            g.Dispose();

            //bmpALL.Save( m_BasePath + @"LOA\CHECK" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

            return bmpALL;

        }

        public void Close()
        {
            foreach (CCDClass ccd in CCDList)
            {
                ccd.Close();
            }
        }
    }


    public class CCDClass
    {

#if EPIX
        #region Library

        const int STRETCH_DELETESCANS = 3;

        //[DllImport("c:\\xclib\\xclibwnt.dll")]
        //private static extern int pxd_PIXCIopen(string c_driverparms, string c_formatname, string c_formatfile);

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_PIXCIopen(string c_driverparms, string c_formatname, string c_formatfile);
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_PIXCIclose();

        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_doSnap(int c_unitmap, int c_buffer, int timeout);
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goSnap(int c_unitmap, int c_buffer);
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goLive(int c_unitmap, int c_buffer);
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_goUnLive(int c_unitmap);
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_renderStretchDIBits(int c_unitmap, int c_buf, int c_ulx, int c_uly, int c_lrx, int c_lry, int c_options, IntPtr c_hDC, int c_nX, int c_nY, int c_nWidth, int c_nHeight, int c_winoptions);
        [DllImport("XCLIBW64.dll")]
        public static extern int pxd_SV9M001_setExposureAndGain(int c_unitmap, int c_rsvd, double c_exposure, double c_redgain, double c_grnrgain, double c_bluegain, double c_grnbgain);
        [DllImport("XCLIBW64.dll")]
        public static extern int pxd_SV9M001_setExposureAndDigitalGain(int c_unitmap, int c_rsvd, double c_exposure, double c_digitalgain, double c_rsvd2, double c_rsvd3, double c_rsvd4);

        //_pxd_mesgFault@4
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_mesgFault(int c_unitmap);
        //_pxd_mesgFaultText@12
        [DllImport("XCLIBW64.dll")]
        private static extern int pxd_mesgFaultText(int c_unitmap, StringBuilder buf, int bufsize);
        //_pxd_loadBmp@36
        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_loadBmp@36")]
        private static extern int EPIXLOADBMP(int c_unitmap, string c_pathname, int c_framebuf, int c_ulx, int c_uly, int c_lrx, int c_lry, int c_loadmode, int c_options);
        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_imageZdim@0")]
        private static extern int pxd_imageZdim();
        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_loadTiff@36")]
        private static extern int pxd_loadTiff(int c_unitmap, StringBuilder c_pathname, int c_buf, int c_ulx, int c_uly, int c_lrx, int c_lry, int c_loadmode, int c_options);
        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_xclibEscape@12")]
        private static extern IntPtr pxd_xclibEscape(int rsvd1, int rsvd2, int rsvd3);
        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_xclibEscaped@12")]
        private static extern int pxd_xclibEscaped(int rsvd1, int rsvd2, int rsvd3);
        [DllImport("XCLIBW64.dll", EntryPoint = "pxd_capturedFieldCount")]
        private static extern int pxd_capturedFieldCount(int c_unitmap);
        [DllImport("exBayerPhase.dll", EntryPoint = "_pxd_setBayerPhaseEx")]
        private static extern int pxd_setBayerPhaseEx(int iCamID, int iPhase, IntPtr pXclibs);

        [DllImport("gdi32.dll")]
        private static extern int SetStretchBltMode(IntPtr hDC, int mode);

        [DllImport("XCLIBW64.dll", EntryPoint = "_pxd_renderDIBCreate@32")]
        public static extern IntPtr pxd_renderDIBCreate(int unitmap, int buf, int ulx, int uly, int lrx, int lry, int mode, int options);

        #endregion
#endif
        public static bool IsConnectionError = false;

        CCDTYPEEnum CCDTYPE = CCDTYPEEnum.FILE;

        int LiveCycle = 0;
        bool IsFirstOver = false;

        int CCDHead = 0;
        int CCDWidth = 0;
        int CCDHeight = 0;
        int OFFSETIndex = 0;
        int CCDRotate = 0;

        static int EPIXCount = 0;

        int EPIXOffset = 0;

        string WorkPath = "";

        public string TESTFilePath = "";
        public Bitmap bmpTmp = new Bitmap(1, 1);
        public Bitmap[] CCDBMP;
        public Bitmap bmpALL = new Bitmap(1, 1);
        public Bitmap[] m_bmpTmp;

        //JzToolsClass myJzTools = new JzToolsClass();

        bool IsNoLive = false;

        JzTimes myTime = new JzTimes();

#if IDS
        uEye.Camera[] iDSCAM;
#endif

#if TIS || TISUSB
        int TISMaxGain = 24;
        int TISMinGain = 4;

        int TISRangeDiff
        {
            get
            {
                return TISMaxGain - TISMinGain;
            }
        }
        
        FrameFilter RotateFlipFilter;
        ICImagingControl[] TISCAM;
        TIS.Imaging.ImageBuffer[] TISImageBuffer;
        VCDSimpleProperty [] TISCAMTUNING;
        VCDSwitchProperty [] TISPolaritySwitch;

        //USE FOR USB TRIGGER
        VCDSwitchProperty [] TISTriggerEnable;

        VCDButtonProperty[] TISSoftTrigger;
#endif

#if PTG

        ManagedImage [] PTGRowImage;
        ManagedImage[] PTGProcessedImage;

        ManagedBusManager PTGBusManage;

        ManagedPGRGuid[] PTGCAMGuid;
        ManagedGigECamera[] PTGCAM;

        CameraProperty [] PTGCAMProperty;
        GigEImageSettings[] PTGImageSetting;
#endif

#if AISYS
        AxAxAltairU[] AISYSCameras;
#endif

#if (USBCAM)
        CameraControl[] m_CameraControl = null;
#endif

#if HIK

        CAM_HIKVISION[] CAM_HIK;

#endif

        public CCDClass()
        {

        }

        public CCDClass(CCDTYPEEnum ccdtype)
        {
            CCDTYPE = ccdtype;
        }

        public bool Initial(string InitialString)
        {
            return Initial(InitialString, CCDTYPE, false);
        }

        public static void InitialEPIXPara()
        {
            EPIXCount = 0;
        }

        public static string EPIXFMTPath = "";

        static Bitmap bmpTMP = new Bitmap(1, 1);
        public static void GetBMP(string BMPFileStr, ref Bitmap BMP)
        {
            bmpTMP.Dispose();
            bmpTMP = new Bitmap(BMPFileStr);

            BMP.Dispose();
            BMP = new Bitmap(bmpTMP);

            bmpTMP.Dispose();
            bmpTMP = new Bitmap(1, 1);
        }

        public bool Initial(string InitialString, CCDTYPEEnum ccdtype,bool isnolive)
        {
            CCDTYPE = ccdtype;

            IsNoLive = isnolive;

            switch (CCDTYPE)
            {
                case CCDTYPEEnum.FILE:
                    #region FILE TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);
                            OFFSETIndex = int.Parse(str[4]);

                            WorkPath = str[3];

                            CCDBMP = new Bitmap[CCDHead];

                            int i = 0;
                            while (i < CCDHead)
                            {
                                CCDBMP[i] = new Bitmap(1, 1);
                                GetBMP(WorkPath + "\\" + (i + OFFSETIndex).ToString("000") + Universal.GlobalImageTypeString, ref CCDBMP[i]);

                                i++;
                            }
                        }
                        return true;
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                #endregion
#if IDS
                case CCDTYPEEnum.IDS:
                #region IDS TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');

                        CCDHead = int.Parse(str[0]);
                        CCDWidth = int.Parse(str[1]);
                        CCDHeight = int.Parse(str[2]);
                        
                        CCDBMP = new Bitmap[CCDHead];

                        iDSCAM = new uEye.Camera[CCDHead];

                        //bmpTmp.Dispose();
                        //bmpTmp = new Bitmap(CCDHeight, CCDWidth);

                        int i = 0;
                        while (i < CCDHead)
                        {
                            CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);

                            iDSCAM[i] = new uEye.Camera();

                            if (!InitialiDS(iDSCAM[i], i))
                                break;
                            i++;
                        }

                        return i == CCDHead;
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                #endregion
#endif
#if HIK
                #region HIKVISION
                case CCDTYPEEnum.HIK:

                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);
                            WorkPath = str[3];
                            OFFSETIndex = int.Parse(str[4]);
                            CCDRotate = int.Parse(str[5]);
                            //if (str.Length > 5)
                            //{
                            //    CCDRotaion = int.Parse(str[5]);
                            //}

                            CAM_HIK = new CAM_HIKVISION[CCDHead];
                            CCDBMP = new Bitmap[CCDHead];
                            LastCount = new int[CCDHead];
                            CountErrorRetry = new int[CCDHead];
                            SideErrorRetry = new int[CCDHead];
                            m_bmpTmp = new Bitmap[CCDHead];

                            //bmpTmp.Dispose();
                            //bmpTmp = new Bitmap(CCDWidth, CCDHeight);
                        }

                        //EPIXOffset = EPIXCount;

                        //if (InitialEPIX())

                        int i = 0;
                        while (i < CCDHead)
                        {
                            if (!IsFirstOver)
                            {
                                CAM_HIK[i] = new CAM_HIKVISION(new PictureBox(), OFFSETIndex + i);
                                //CAM_HIK[i].Init(WorkPath);

                                //CAM_HIK[i].SetGain(INI.CCD_GAIN);
                                //CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);
                                //m_bmpTmp[i] = new Bitmap(CCDWidth, CCDHeight);
                                CAM_HIK[i].TriggerSoftwareX();
                                CCDBMP[i] = new Bitmap(CAM_HIK[i].CaptureBmp(CCDRotate));
                                m_bmpTmp[i] = new Bitmap(CAM_HIK[i].CaptureBmp(CCDRotate));
                            }

                            i++;
                        }
                        return true;
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }

                    break;
                #endregion
#endif
#if EPIX
                case CCDTYPEEnum.EPIX:
                    #region EPIX TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);
                            CCDRotate = int.Parse(str[5]);

                            CCDBMP = new Bitmap[CCDHead];

                            bmpTmp.Dispose();
                            bmpTmp = new Bitmap(CCDWidth, CCDHeight);

                            m_bmpTmp = new Bitmap[CCDHead];
                        }

                        EPIXOffset = EPIXCount;

                        string fmtpathstring = EPIXFMTPath;

                        if (InitialEPIX(fmtpathstring))
                        {

                            int i = 0;
                            while (i < CCDHead)
                            {
                                if(!IsFirstOver)
                                {
                                    CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);
                                    m_bmpTmp[i] = new Bitmap(CCDWidth, CCDHeight);
                                }

                                if(!IsNoLive)
                                    EPIXLive(i);

                                EPIXRender(i, CCDRotate);

                                i++;
                            }

                            EPIXCount += CCDHead;

                            return true;
                        }
                        else
                            return false;
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                    #endregion
#endif
                case CCDTYPEEnum.USBCAM:
#if(USBCAM)
                    #region USBCAM TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);
                            CCDRotate = int.Parse(str[5]);

                            //CCDIP = str[6];
                            //CCDPORT = int.Parse(str[7]);

                            CCDBMP = new Bitmap[CCDHead];

                            bmpTmp.Dispose();
                            bmpTmp = new Bitmap(CCDWidth, CCDHeight);
                        }

                        if (_InitUSBCamera())
                        {
                            int i = 0;
                            while (i < CCDHead)
                            {
                                if (!IsFirstOver)
                                {
                                    CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);

                                    UsbCAMLive(i);
                                }
                                i++;
                            }

                            return true;
                        }
                        else
                            return false;
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                    #endregion
#endif
#if TIS || TISUSB
                case CCDTYPEEnum.TIS:
                case CCDTYPEEnum.TISUSB:
                    #region TIS TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');


                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);

                            CCDBMP = new Bitmap[CCDHead];

                            bmpTmp.Dispose();
                            bmpTmp = new Bitmap(CCDWidth, CCDHeight);
                        }

                        TISCAM = new ICImagingControl[CCDHead];
                        TISCAMTUNING = new VCDSimpleProperty[CCDHead];
                        TISPolaritySwitch = new VCDSwitchProperty[CCDHead];

                        //USE FOR USB ACTION
                        TISTriggerEnable = new VCDSwitchProperty[CCDHead];

                        TISSoftTrigger = new VCDButtonProperty[CCDHead];
                        TISImageBuffer = new TIS.Imaging.ImageBuffer[CCDHead];

                        return InitialTIS();
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                    #endregion
#endif
#if PTG
                case CCDTYPEEnum.PTG:
                #region POINT GREY TYPE
                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);

                            CCDBMP = new Bitmap[CCDHead];

                            bmpTmp.Dispose();
                            bmpTmp = new Bitmap(CCDWidth, CCDHeight);
                        }

                        PTGCAM = new ManagedGigECamera[CCDHead];
                        PTGRowImage = new ManagedImage[CCDHead];
                        PTGProcessedImage = new ManagedImage[CCDHead];
                        PTGCAMProperty = new CameraProperty[CCDHead];
                        PTGImageSetting = new GigEImageSettings[CCDHead];

                        PTGBusManage = new ManagedBusManager();
                        //TISCAMTUNING = new VCDSimpleProperty[CCDHead];
                        //TISPolaritySwitch = new VCDSwitchProperty[CCDHead];

                        //USE FOR USB ACTION
                        //TISTriggerEnable = new VCDSwitchProperty[CCDHead];

                        //TISSoftTrigger = new VCDButtonProperty[CCDHead];
                        //TISImageBuffer = new TIS.Imaging.ImageBuffer[CCDHead];

                        string fmtpathstring = EPIXFMTPath;

                        return InitialPTG(fmtpathstring);
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                #endregion
#endif
#if AISYS
                case CCDTYPEEnum.AISYS:
                #region AISYS Cameras
                    try
                    {
                        string[] str = InitialString.Split('@');

                        if (!IsFirstOver)
                        {
                            CCDHead = int.Parse(str[0]);
                            CCDWidth = int.Parse(str[1]);
                            CCDHeight = int.Parse(str[2]);

                            CCDBMP = new Bitmap[CCDHead];

                            bmpTmp.Dispose();
                            bmpTmp = new Bitmap(CCDWidth, CCDHeight);

                            int i = 0;
                            while (i < CCDHead)
                            {
                                CCDBMP[i] = new Bitmap(bmpTmp);

                                i++;
                            }
                        }

                        return InitialAISYS();
                    }
                    catch (Exception exx)
                    {
                        string str = exx.ToString();

                        return false;
                    }
                #endregion
#endif
                default:
                    return false;
            }

            IsFirstOver = true;
        }
#if IDS
        bool InitialiDS(uEye.Camera CAM, int id)
        {
            uEye.Defines.Status statusRet = uEye.Defines.Status.NO_SUCCESS;

            statusRet = CAM.Init(1 + id);

            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Initializing IDS Cameras failed");
                IsConnectionError = true;

                return false;
            }

            statusRet = CAM.Memory.Allocate();
            if (statusRet != uEye.Defines.Status.SUCCESS)
            {
                MessageBox.Show("Allocating IDS Cameras memory failed");
                IsConnectionError = true;

                return false;
            }

            uEye.Types.ImageInfo imageinfo;

            Int32 s32MemID;
            CAM.Memory.GetActive(out s32MemID);
            CAM.Information.GetImageInfo(s32MemID, out imageinfo);

            //double f64Min = 0;
            //double f64Max = 0;
            //double f64Inc = 0;

            //CAM.Timing.Exposure.GetRange(out f64Min, out f64Max, out f64Inc);

            CAM.Acquisition.Capture();

            CAM.Parameter.Load(INI.WORK_PATH + "\\IDS.INI");

            return true;
        }
#endif
#if EPIX
        bool InitialEPIX(string fmtpath)
        {
            if (IsFirstOver)
                return true;

            if (EPIXCount > 0)
                return true;

            int i = 0;
            int ret = 1;

            while (ret != 0 && i < 5)
            {
                pxd_PIXCIclose();
                pxd_PIXCIclose();
                pxd_PIXCIclose();

                ret = pxd_PIXCIopen("", null, fmtpath + "\\EPIX.FMT");

                i++;
            }
            return ret == 0;

            //InitialEPIX(0);
        }
        bool InitialEPIX(int kind,int fmtpath)
        {
            int i = 0;
            int ret = 1;

            while (ret != 0 && i < 5)
            {
                pxd_PIXCIclose();
                pxd_PIXCIclose();
                pxd_PIXCIclose();

                if(kind > 0)
                    ret = pxd_PIXCIopen("", null, fmtpath + "\\EPIX" + kind.ToString() + ".FMT");
                else
                    ret = pxd_PIXCIopen("", null, fmtpath + "\\EPIX.FMT");

                i++;
            }
            return ret == 0;
        }
        public void EPIXLive(int Index)
        {
            pxd_goLive(1 << (Index + EPIXOffset), 1);

        }
        public void EPIXRender(int Index,int roattion)
        {
            if (roattion == 270)
                m_bmpTmp[Index].RotateFlip(RotateFlipType.Rotate90FlipNone);

            if (IsNoLive)
                pxd_doSnap(1 << (Index + EPIXOffset), 1, 0);

            Graphics g = Graphics.FromImage(m_bmpTmp[Index]);

            IntPtr hDC = g.GetHdc();
            SetStretchBltMode(hDC, STRETCH_DELETESCANS);

            if (roattion == 270)
                pxd_renderStretchDIBits((1 << (Index + EPIXOffset)), 1, 0, 0, -1, -1, 0, hDC, 0, 0, CCDHeight, CCDWidth, 0);
            else
                pxd_renderStretchDIBits((1 << (Index + EPIXOffset)), 1, 0, 0, -1, -1, 0, hDC, 0, 0, CCDWidth, CCDHeight, 0);

            g.ReleaseHdc(hDC);
            g.Dispose();

            if (roattion == 270)
                m_bmpTmp[Index].RotateFlip(RotateFlipType.Rotate270FlipNone);

            CCDBMP[Index].Dispose();
            CCDBMP[Index] = new Bitmap(m_bmpTmp[Index]);

        }

        //public void Close()
        //{
        //    switch (CCDTYPE)
        //    {
        //        case CCDTYPEEnum.EPIX:
        //            pxd_PIXCIclose();
        //            break;
        //        default:
        //            break;
        //    }
        //}
#endif

#if TIS || TISUSB
        bool InitialTIS()
        {
            int i = 0;
            int j = 0;
            string Str = "";
            string CCDSEQStr = "";

            List<string> DeviceList = new List<string>();
            List<string> SerialList = new List<string>();
            List<string> FormatList = new List<string>();

            try
            {

                CCDSEQStr = INI.WORK_PATH + "\\CCDSEQ.INI";

                while (i < CCDHead)
                {
                    if (!IsFirstOver)
                    {
                        CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);
                    }

                    TISCAM[i] = new ICImagingControl();
                    TISCAM[i].ImageRingBufferSize = 20;
                    TISCAM[i].ImageAvailableExecutionMode = EventExecutionMode.MultiThreaded;

                    #region Check CCD Serial
                    if (i == 0)
                    {
                        if (File.Exists(CCDSEQStr))
                        {
                            myJzTools.ReadData(ref Str, CCDSEQStr);

                            string[] strs;

                            Str = Str.Replace(Environment.NewLine, "@");
                            strs = Str.Split('@');

                            foreach (string str in strs)
                            {
                                SerialList.Add(str);
                            }
                        }
                        else
                        {
                            foreach (Device device in TISCAM[i].Devices)
                            {
                                string serial = "";
                                device.GetSerialNumber(out serial);
                                SerialList.Add(serial + ",N");
                            }

                            Str = "";

                            foreach (string str in SerialList)
                            {
                                Str += str + Environment.NewLine;
                            }

                            Str = myJzTools.RemoveLastChar(Str, 2);

                            myJzTools.SaveData(Str, CCDSEQStr);
                        }

                        foreach (string str in SerialList)
                        {
                            foreach (Device device in TISCAM[i].Devices)
                            {
                                string serial = "";

                                device.GetSerialNumber(out serial);

                                if (str.IndexOf(serial) > -1)
                                {
                                    DeviceList.Add(device.Name + "," + str.Split(',')[1]);
                                }
                            }
                        }
                    }

                    if (DeviceList.Count != CCDHead)
                        return false;

                    #endregion

                    TISCAM[i].Device = DeviceList[i].Split(',')[0];

                    j = 0;

                    while (j < TISCAM[i].VideoFormats.Length)
                    {
                        Str = TISCAM[i].VideoFormats[j];

                        if (Str.IndexOf("Y800") > -1 && Str.IndexOf(INI.CCD_MINWIDTH.ToString()) > -1)
                            break;
                        j++;
                    }

                    TISCAM[i].VideoFormat = TISCAM[i].VideoFormats[j].Name;


                    RotateFlipFilter = TISCAM[i].FrameFilterCreate("Rotate Flip", "");

                    if (DeviceList[i].Split(',')[1] == "R")
                    {
                        RotateFlipFilter.SetIntParameter("Rotation Angle", 180);
                        TISCAM[i].DeviceFrameFilters.Add(RotateFlipFilter);
                    }

                    TISCAM[i].Tag = i;

                    TISCAM[i].MemoryCurrentGrabberColorformat = ICImagingControlColorformats.ICY800;
                    TISCAM[i].LiveCaptureContinuous = true;
                    TISCAM[i].LiveDisplay = false;

                    if (TISCAM[i].DeviceValid)
                    {

                        switch (CCDTYPE)
                        {
                            case CCDTYPEEnum.TIS:

                                TISPolaritySwitch[i] = (TIS.Imaging.VCDSwitchProperty)TISCAM[i].VCDPropertyItems.FindInterface(TIS.Imaging.VCDIDs.VCDID_TriggerMode
                                    + ":" + TIS.Imaging.VCDIDs.VCDElement_TriggerPolarity
                                    + ":" + TIS.Imaging.VCDIDs.VCDInterface_Switch);

                                TISPolaritySwitch[i].Switch = true;
                                break;
                            case CCDTYPEEnum.TISUSB:

                                TISTriggerEnable[i] = (TIS.Imaging.VCDSwitchProperty)TISCAM[i].VCDPropertyItems.FindInterface(TIS.Imaging.VCDIDs.VCDID_TriggerMode 
                                    + ":" + TIS.Imaging.VCDIDs.VCDElement_Value 
                                    + ":" + TIS.Imaging.VCDIDs.VCDInterface_Switch);

                                TISTriggerEnable[i].Switch = true; 

                                //TISTrigger[i] = TISCAM[i].VCDPropertyItems.FindItem(TIS.Imaging.VCDIDs.VCDID_TriggerMode);

                                break;
                        }


                        TISSoftTrigger[i] = (TIS.Imaging.VCDButtonProperty)TISCAM[i].VCDPropertyItems.FindInterface(TIS.Imaging.VCDIDs.VCDID_TriggerMode
                            + ":{FDB4003C-552C-4FAA-B87B-42E888D54147}:"
                            + TIS.Imaging.VCDIDs.VCDInterface_Button);

                        TIS_AutoExposure(TISCAM[i], false);
                        TIS_AutoGain(TISCAM[i], false);

                        switch (CCDTYPE)
                        {
                            case CCDTYPEEnum.TIS:
                                TIS_SetGainAbs(TISCAM[i], 0);
                                TISCAM[i].DeviceFrameRate = 15;
                                break;
                            case CCDTYPEEnum.TISUSB:
                                IC_SetGain(TISCAM[i], 4);

                                switch (Universal.VER)
                                {
                                    case "R21":
                                        TISCAM[i].DeviceFrameRate = 7;
                                        break;
                                    default:
                                        TISCAM[i].DeviceFrameRate = 3;
                                        break;
                                }
                                
                                break;
                        }


                        TISCAM[i].ImageAvailable += new EventHandler<ICImagingControl.ImageAvailableEventArgs>(CCDClass_ImageAvailable);
                        TISCAM[i].DeviceLost += new EventHandler<ICImagingControl.DeviceLostEventArgs>(TISCCD_DeviceLost);

                        TISCAM[i].LiveStart();
                    }
                    i++;

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Initializing TIS Cameras Fail " + e.ToString());
                IsConnectionError = true;
            }

            return true;
        }

        void CCDClass_ImageAvailable(object sender, ICImagingControl.ImageAvailableEventArgs e)
        {
            ICImagingControl TISCAM = (ICImagingControl)sender;

            TISImageBuffer[(int)TISCAM.Tag] = TISCAM.ImageBuffers[e.bufferIndex];

            //TISImageBuffer[(int)TISCAM.Tag].Lock();

            //CCDBMP[(int)TISCAM.Tag].Dispose();
            //CCDBMP[(int)TISCAM.Tag] = new Bitmap(TISImageBuffer[(int)TISCAM.Tag].Bitmap);

            //TISImageBuffer[(int)TISCAM.Tag].Unlock();
        }

        void TISCCD_DeviceLost(object sender, ICImagingControl.DeviceLostEventArgs e)
        {
            ICImagingControl TISCAM = (ICImagingControl)sender;

            MessageBox.Show("Initializing TIS Cameras CAM" + ((int)TISCAM.Tag).ToString() + " Fail.");
            IsConnectionError = true;
        }

        private bool TIS_AutoExposure(TIS.Imaging.ICImagingControl ic, bool onoff)
        {
            TIS.Imaging.VCDSwitchProperty ExposureAuto;
            ExposureAuto = (TIS.Imaging.VCDSwitchProperty)ic.VCDPropertyItems.FindInterface(
                                                          TIS.Imaging.VCDIDs.VCDID_Exposure + ":"
                                                        + TIS.Imaging.VCDIDs.VCDElement_Auto + ":"
                                                        + TIS.Imaging.VCDIDs.VCDInterface_Switch);

            if (ExposureAuto != null && ExposureAuto.Available)
            {
                ExposureAuto.Switch = onoff;
                return true;
            }
            return false;
        }
        private bool TIS_SetExposureAbs(TIS.Imaging.ICImagingControl ic, double value)
        {
            TIS.Imaging.VCDAbsoluteValueProperty ExposureAbs;
            ExposureAbs = (TIS.Imaging.VCDAbsoluteValueProperty)ic.VCDPropertyItems.FindInterface(
                                                           TIS.Imaging.VCDIDs.VCDID_Exposure + ":"
                                                         + TIS.Imaging.VCDIDs.VCDElement_Value + ":"
                                                         + TIS.Imaging.VCDIDs.VCDInterface_AbsoluteValue);

            if (ExposureAbs != null)
            {
                ExposureAbs.Value = value;
                return true;
            }
            return false;
        }

        private bool TIS_AutoGain(TIS.Imaging.ICImagingControl ic, bool onoff)
        {
            TIS.Imaging.VCDSwitchProperty GainAuto;
            GainAuto = (TIS.Imaging.VCDSwitchProperty)ic.VCDPropertyItems.FindInterface(
                                                          TIS.Imaging.VCDIDs.VCDID_Gain + ":"
                                                        + TIS.Imaging.VCDIDs.VCDElement_Auto + ":"
                                                        + TIS.Imaging.VCDIDs.VCDInterface_Switch);

            if (GainAuto != null && GainAuto.Available)
            {
                GainAuto.Switch = onoff;
                return true;
            }
            return false;
        }
        private bool TIS_SetGainAbs(TIS.Imaging.ICImagingControl ic, double value)
        {
            TIS.Imaging.VCDAbsoluteValueProperty GainAbs;
            GainAbs = (TIS.Imaging.VCDAbsoluteValueProperty)ic.VCDPropertyItems.FindInterface(
                                                           TIS.Imaging.VCDIDs.VCDID_Gain + ":"
                                                         + TIS.Imaging.VCDIDs.VCDElement_Value + ":"
                                                         + TIS.Imaging.VCDIDs.VCDInterface_AbsoluteValue);

            if (GainAbs != null)
            {
                GainAbs.Value = value;
                return true;
            }
            return false;
        }

        private bool IC_SetGain(TIS.Imaging.ICImagingControl ic, int value)
        {
            TIS.Imaging.VCDRangeProperty GainRange;
            GainRange = (TIS.Imaging.VCDRangeProperty)ic.VCDPropertyItems.FindInterface(
                                                           TIS.Imaging.VCDIDs.VCDID_Gain + ":"
                                                         + TIS.Imaging.VCDIDs.VCDElement_Value + ":"
                                                         + TIS.Imaging.VCDIDs.VCDInterface_Range);

            if (GainRange != null)
            {
                GainRange.Value = value;
                return true;
            }
            return false;
        }
#endif
#if PTG

        bool InitialPTG(string fmtpath)
        {
            int i = 0;
            int j = 0;
            string Str = "";
            string CCDSEQStr = "";

            List<string> DeviceList = new List<string>();
            List<string> SerialList = new List<string>();
            List<string> FormatList = new List<string>();

            try
            {

                CCDSEQStr = fmtpath + "\\PTGCCDSEQ.INI";

                while (i < CCDHead)
                {
                    if (!IsFirstOver)
                    {
                        CCDBMP[i] = new Bitmap(CCDWidth, CCDHeight);
                        //m_bmpTmp[i] = new Bitmap(CCDWidth, CCDHeight);
                    }

                    PTGCAM[i] = new ManagedGigECamera();
                    PTGCAMProperty[i] = new CameraProperty();

                    PTGRowImage[i] = new ManagedImage();
                    PTGProcessedImage[i] = new ManagedImage();

                    #region Check CCD Serial
                    if (i == 0)
                    {
                        if (File.Exists(CCDSEQStr))
                        {
                            JzTools.ReadData(ref Str, CCDSEQStr);

                            string[] strs;

                            Str = Str.Replace(Environment.NewLine, "@");
                            strs = Str.Split('@');

                            foreach (string str in strs)
                            {
                                SerialList.Add(str);
                            }
                        }
                        else
                        {
                            uint camcount = PTGBusManage.GetNumOfCameras();

                            j = 0;

                            while (j < camcount)
                            {
                                SerialList.Add(PTGBusManage.GetCameraSerialNumberFromIndex((uint)j).ToString() + ",N,0,0");
                                j++;
                            }

                            Str = "";

                            foreach (string str in SerialList)
                            {
                                Str += str + Environment.NewLine;
                            }

                            Str = JzTools.RemoveLastChar(Str, 2);

                            JzTools.SaveData(Str, CCDSEQStr);
                        }

                    //    foreach (string str in SerialList)
                    //    {
                    //        foreach (ManagedGigECamera cam in TISCAM[i].Devices)
                    //        {
                    //            string serial = "";

                    //            device.GetSerialNumber(out serial);

                    //            if (str.IndexOf(serial) > -1)
                    //            {
                    //                DeviceList.Add(device.Name + "," + str.Split(',')[1]);
                    //            }
                    //        }
                    //    }
                    }

                    //if (DeviceList.Count != CCDHead)
                    //    return false;

                    #endregion

                    //TISCAM[i].Device = DeviceList[i].Split(',')[0];

                    PTGCAM[i].Connect(PTGBusManage.GetCameraFromSerialNumber(uint.Parse(SerialList[i].Split(',')[0])));

                    j = 0;
                    while (j < (int)PropertyType.Unspecified)
                    {
                        if ((j > 2 && j < 12) || (j > 13))
                        {
                            j++;
                            continue;
                        }


                        PTGCAMProperty[i] = PTGCAM[i].GetProperty((PropertyType)j);

                        PTGCAMProperty[i].autoManualMode = false;

                        #region Set ROI
                        PTGImageSetting[i] = new GigEImageSettings();

                        PTGImageSetting[i].offsetX = uint.Parse(SerialList[i].Split(',')[2]);
                        PTGImageSetting[i].offsetY = uint.Parse(SerialList[i].Split(',')[3]);
                        PTGImageSetting[i].width =uint.Parse(CCDWidth.ToString());
                        PTGImageSetting[i].height = uint.Parse(CCDHeight.ToString());

                        PTGCAM[i].SetGigEImageSettings(PTGImageSetting[i]);
                        #endregion

                        switch ((PropertyType)j)
                        {
                            case PropertyType.Brightness:
                                PTGCAMProperty[i].valueA = 399;
                                break;
                            case PropertyType.AutoExposure:
                                PTGCAMProperty[i].onOff = true;
                                PTGCAMProperty[i].valueA = 292;
                                break;
                            case PropertyType.Sharpness:
                                PTGCAMProperty[i].onOff = true;
                                PTGCAMProperty[i].valueA = 1536;
                                break;
                            case PropertyType.Gamma:
                                PTGCAMProperty[i].onOff = true;
                                PTGCAMProperty[i].valueA = 1024;
                                break;
                            case PropertyType.Shutter:
                                PTGCAMProperty[i].valueA = 3044;
                                break;
                            case PropertyType.Gain:
                                PTGCAMProperty[i].valueA = 48;
                                break;
                            case PropertyType.FrameRate:
                                PTGCAMProperty[i].autoManualMode = true;
                                break;
                        }

                        PTGCAM[i].SetProperty(PTGCAMProperty[i]);

                        j++;
                    }
                    //PTGCAM[i].SetProperty(PTGCAMProperty);

                    PTGCAM[i].StartCapture();

                    i++;

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Initializing PTG Cameras Fail " + e.ToString());
                IsConnectionError = true;
            }

            return true;
        }
#endif
#if AISYS
        bool InitialAISYS()
        {
            int i = 0;

            AISYSCameras = new AxAxAltairU[CCDHead];

            while (i < CCDHead)
            {
                AISYSCameras[i] = new AxAxAltairU();
                //AISYSCameras[i].ElShutterSyncMode = false;

                i++;
            }

            return true;
        }

        public void AISYSRender(int Index)
        {
            //if (INI.CCD_ROTATE == 270)
            //    bmpTmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

            Graphics g = Graphics.FromImage(bmpTmp);

            //AISYSCameras[Index].SnapAndWait();

            //AISYSCameras[Index].Freeze();
            AISYSCameras[Index].SnapAndWait();

            long hSurface = AISYSCameras[Index].ActiveSurfaceHandle;
            IntPtr hDC = g.GetHdc();

            AISYSCameras[Index].DrawSurface(hSurface, (int)hDC, 1.0f, 1.0f, 0, 0);

            g.ReleaseHdc(hDC);
            g.Dispose();

            //if (INI.CCD_ROTATE == 270)
            //    bmpTmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

            CCDBMP[Index].Dispose();
            CCDBMP[Index] = new Bitmap(bmpTmp);

        }

#endif

        public void InitialAISYSBeForeFormLoad(Control.ControlCollection controls)
        {
            bool ret = true;
#if AISYS
            int i = 0;
            

            i = 0;
            while (i < CCDHead)
            {
                controls.Add(AISYSCameras[i]);

                //bool retx = AISYSCameras[i].QuickCreateChannel();

                //if (retx)
                //    AISYSCameras[i].Live();

                //ret &= retx;

                i++;
            }
#endif
            if (!ret)
                MessageBox.Show("太陽牌像機初使化錯誤。");

        }
#if(USBCAM)
        bool _InitUSBCamera()
        {
            if (IsFirstOver)
                return true;

            m_CameraControl = new CameraControl[CCDHead];
            // Camera choice
            CameraChoice _CameraChoice = new CameraChoice();
            // Get List of devices (cameras)
            _CameraChoice.UpdateDeviceList();
            // To get an example of camera and resolution change look at other code samples 
            //if (_CameraChoice.Devices.Count > 0)
            //{
            //    // Device moniker. It's like device id or handle.
            //    // Run first camera if we have one
            //    var camera_moniker = _CameraChoice.Devices[1].Mon;
            //    // Set selected camera to camera control with default resolution
            //    m_CameraControl.SetCamera(camera_moniker, null);
            //    return true;
            //}

            if (_CameraChoice.Devices.Count <= 0)
                return false;
            int i = 0;
            foreach (DirectShowLib.DsDevice dsdevice in _CameraChoice.Devices)
            {
                if (INI.USBCAM_NAME.IndexOf(dsdevice.Name) > -1)
                {
                    m_CameraControl[i] = new CameraControl();
                    var camera_moniker = _CameraChoice.Devices[i].Mon;
                    // Set selected camera to camera control with default resolution
                    m_CameraControl[i].SetCamera(camera_moniker, null);
                }
            }

            return true;
        }
        void _CloseUSBCamera()
        {
            // Close camera. It's safe to call CloseCamera() even if no camera was set.
            //if (m_CameraControl != null)
            //    m_CameraControl.CloseCamera();

            foreach (CameraControl cam in m_CameraControl)
            {
                if (cam != null)
                    cam.CloseCamera();
            }
        }
        public void UsbCAMLive(int index)
        {
            CCDBMP[index].Dispose();
            CCDBMP[index] = m_CameraControl[index].SnapshotOutputImage();
        }
#endif
        public void InitialAISYSInFormLoad()
        {
            bool ret = true;
#if AISYS
            int i = 0;


            i = 0;
            while (i < CCDHead)
            {
                //controls.Add(AISYSCameras[i]);

                bool retx = AISYSCameras[i].QuickCreateChannel();

                if (retx)
                    AISYSCameras[i].Live();

                ret &= retx;

                i++;
            }
#endif
            if (!ret)
                MessageBox.Show("太陽牌像機初使化錯誤。");

        }

        public void Close()
        {
            switch (CCDTYPE)
            {
#if EPIX
                case CCDTYPEEnum.EPIX:
                    pxd_PIXCIclose();
                    break;
#if HIK
                case CCDTYPEEnum.HIK:
                    foreach (CAM_HIKVISION cam in CAM_HIK)
                    {
                        cam.Dispose();
                    }
                    break;
#endif
#endif
#if USBCAM
                case CCDTYPEEnum.USBCAM:
                    _CloseUSBCamera();
                    break;
#endif
#if PTG
                case CCDTYPEEnum.PTG:
                    foreach (ManagedGigECamera cam in PTGCAM)
                    {
                        cam.Disconnect();
                    }
                    break;
#endif
#if TIS || TISUSB
                case CCDTYPEEnum.TIS:
                case CCDTYPEEnum.TISUSB:
                    foreach (ICImagingControl tiscam in TISCAM)
                    {
                        tiscam.LiveStop();
                        tiscam.Dispose();
                    }
                    //TISCAM[CameraNo].ShowPropertyDialog();
                    break;
#endif
#if AISYS
                case CCDTYPEEnum.AISYS:
                    foreach (AxAxAltairU aisyscam in AISYSCameras)
                    {
                        aisyscam.DestroyChannel();
                    }
                    break;
#endif
                default:
                    break;
            }


        }

        public CCDTYPEEnum GetCCDType()
        {
            return CCDTYPE;
        }

        bool IsOCRDeug = false;

        public Bitmap GetImage(int CameraNo)
        {
            switch (CCDTYPE)
            {
                case CCDTYPEEnum.FILE:
                    if (IsOCRDeug && CameraNo == 0)
                        GetBMP(TESTFilePath + "\\" + (CameraNo).ToString("000") + Universal.GlobalImageTypeString, ref CCDBMP[CameraNo]);

                    if (!IsOCRDeug)
                        GetBMP(TESTFilePath + "\\" + (CameraNo + OFFSETIndex).ToString("000") + Universal.GlobalImageTypeString, ref CCDBMP[CameraNo]);

                    break;
#if IDS
                case CCDTYPEEnum.IDS:

                    uEye.Camera camera = iDSCAM[CameraNo];

                    Int32 s32MemID;
                    camera.Memory.GetActive(out s32MemID);
                    camera.Memory.Lock(s32MemID);

                    camera.Memory.ToBitmap(s32MemID, out bmpTmp);

                    if (INI.CCD_ROTATE == 90)
                        bmpTmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                    CCDBMP[CameraNo].Dispose();
                    CCDBMP[CameraNo] = new Bitmap(bmpTmp);
                    //CCDBMP[CameraNo].Save( m_BasePath + @"LOA\IDSLIVE" + Universal.GlobalImageTypeString, Universal.GlobalImageFormat);

                    camera.Memory.Unlock(s32MemID);
                    break;
#endif
#if EPIX
                case CCDTYPEEnum.EPIX:

                    //Need Repair
                    //if (CountErrorRetry[CameraNo] >= CountErrorCount || SideErrorRetry[CameraNo] >= ReConnectionConut)
                    //{
                    //    CCDBMP[CameraNo].Dispose();
                    //    CCDBMP[CameraNo] = new Bitmap(CCDWidth, CCDHeight);

                    //    myJzTools.DrawRect(CCDBMP[CameraNo], myJzTools.SimpleRect(CCDBMP[CameraNo].Size), new SolidBrush(Color.Red));
                    //}
                    //else
                    int rotation = 0;

                    EPIXRender(CameraNo, CCDRotate);

                    break;
#endif
#if HIK
                case CCDTYPEEnum.HIK:

                    CAM_HIK[CameraNo].TriggerSoftwareX();
                    CCDBMP[CameraNo].Dispose();
                    CCDBMP[CameraNo] = new Bitmap(CAM_HIK[CameraNo].CaptureBmp(CCDRotate));

                    break;
#endif
#if(USBCAM)
                case CCDTYPEEnum.USBCAM:

                    UsbCAMLive(CameraNo);

                    break;

#endif
#if TIS || TISUSB
                case CCDTYPEEnum.TIS:
                case CCDTYPEEnum.TISUSB:
                    TISCAM[CameraNo].LiveCapturePause = false;
                    TISCAM[CameraNo].LiveCaptureContinuous = true;


                    TISSoftTrigger[CameraNo].Push();

                    if (TISImageBuffer[CameraNo] != null)
                    {
                        CCDBMP[CameraNo].Dispose();
                        CCDBMP[CameraNo] = new Bitmap(TISImageBuffer[CameraNo].Bitmap);
                    }
                    
                    break;
#endif
#if PTG
                case CCDTYPEEnum.PTG:
                    try
                    {
                        PTGCAM[CameraNo].RetrieveBuffer(PTGRowImage[CameraNo]);

                        lock (this)
                        {
                            PTGRowImage[CameraNo].Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, PTGProcessedImage[CameraNo]);

                            CCDBMP[CameraNo].Dispose();
                            CCDBMP[CameraNo] = new Bitmap(PTGProcessedImage[CameraNo].bitmap);
                        }
                    }
                    catch (FC2Exception ex)
                    {

                    }

                    break;
#endif
#if AISYS
                case CCDTYPEEnum.AISYS:

                    AISYSRender(CameraNo);

                    break;
#endif
            }
            return CCDBMP[CameraNo];
        }
        public void GetImage()
        {
            GetImage(0, CCDHead - 1);
        }
        public void GetImage(int CameraNo, ref Bitmap FromBMP)
        {
            FromBMP.Dispose();
            FromBMP = new Bitmap(GetImage(CameraNo));
        }

        public void GetImage(int FromCameraNo, int ToCameraNo)
        {
            int i = 0;

            while (i < CCDHead)
            {
                if (i >= FromCameraNo && i <= ToCameraNo)
                {
                    GetImage(i);
                }
                i++;
            }
        }

        public int CCDDuriation = 200;
        public void Tick()
        {
            if (myTime.msDuriation > CCDDuriation)
            {
                int i = 0;

                switch (CCDTYPE)
                {
                    case CCDTYPEEnum.EPIX:

                        i = 0;
                        while (i < CCDHead)
                        {
                            CheckConnection(i);
                            i++;
                        }
                        break;
                }


                myTime.Cut();
            }

        }

        public int EPIXCCDMAX = 300;

        public void SetExposure(int CameraNo, int Exposure)
        {
            switch (CCDTYPE)
            {
#if IDS
                case CCDTYPEEnum.IDS:
                    iDSCAM[CameraNo].Timing.Exposure.Set(499d * ((double)Exposure / 100d));
                    break;
#endif
#if EPIX
                case CCDTYPEEnum.EPIX:
                    pxd_SV9M001_setExposureAndDigitalGain(1 << (CameraNo + EPIXOffset), 0, (double)EPIXCCDMAX * ((double)Exposure / 300d), 0, 0, 0, 0);
                    break;
#endif
#if HIK
                case CCDTYPEEnum.HIK:
                    MvCamCtrl.NET.MyCamera.MVCC_FLOATVALUE stParam = new MvCamCtrl.NET.MyCamera.MVCC_FLOATVALUE();
                    CAM_HIK[CameraNo].GetFloatValue_NET(ref stParam);

                    CAM_HIK[CameraNo].SetExposure((float)Exposure / 300f * stParam.fMax);
                    //CAM_HIK[CameraNo].SetExposure((float)Exposure / 100f * 10000 * INI.EPIXCCDMAX);

                    break;
#endif
#if TIS || TISUSB
                case CCDTYPEEnum.TIS:
                case CCDTYPEEnum.TISUSB:
                    TIS_SetExposureAbs(TISCAM[CameraNo], (double)Exposure / 200d);
                    //TISCAM[CameraNo].ShowPropertyDialog();
                    break;
#endif
#if PTG
                case CCDTYPEEnum.PTG:
                    PTGCAMProperty[CameraNo] = PTGCAM[CameraNo].GetProperty(PropertyType.Shutter);
                    PTGCAMProperty[CameraNo].valueA = (uint)(((float)Exposure / 100f) * 2010f);

                    PTGCAM[CameraNo].SetProperty(PTGCAMProperty[CameraNo]);

                    break;
#endif
#if AISYS
                case CCDTYPEEnum.AISYS:

                    AISYSCameras[CameraNo].ElShutter = (int)(2748d * ((double)Exposure / 100d));

                    break;
#endif

                default:
                    break;
            }
        }

        public void SetExposure(string Str)
        {
            string[] str = Str.Split(',');

            int i = 0;

            while (i < CCDHead)
            {
                SetExposure(i, int.Parse(str[i]));
                i++;
            }
        }

        public void GenAllBMP(Size AllViewSize)
        {
            bmpALL.Dispose();
            bmpALL = new Bitmap(AllViewSize.Width, AllViewSize.Height);
        }

        public Bitmap GetAllBMP(bool IsChangeSize, Size AllViewSize, Point[] CCDLocations, int[] CameraNos)
        {
            if (IsChangeSize)
            {
                GenAllBMP(AllViewSize);
            }

            switch (CCDTYPE)
            {
#if IDS || EPIX || TIS || TISUSB || AISYS || PTG
                case CCDTYPEEnum.IDS:
                case CCDTYPEEnum.EPIX:
                case CCDTYPEEnum.TIS:
                case CCDTYPEEnum.TISUSB:
                case CCDTYPEEnum.AISYS:
                case CCDTYPEEnum.PTG:
                    GetImage(LiveCycle);

                    LiveCycle++;

                    if (LiveCycle >= CCDHead)
                        LiveCycle = 0;
                    break;
#endif
                default:
                    break;
            }
            


            Graphics g = Graphics.FromImage(bmpALL);

            int i = 0;
            foreach (Point location in CCDLocations)
            {
                //g.DrawImage(CCDBMP[CameraNos[i]],
                //    new Rectangle(new Point(location.X >> Universal.Ratio, location.Y >> Universal.Ratio), new Size(CCDWidth >> Universal.Ratio, CCDHeight >> Universal.Ratio)),
                //    new Rectangle(0, 0, CCDWidth, CCDHeight),
                //    GraphicsUnit.Pixel);

                g.DrawImage(CCDBMP[CameraNos[i]],
                    new Rectangle(new Point(location.X, location.Y), new Size(CCDWidth, CCDHeight)),
                    new Rectangle(0, 0, CCDWidth, CCDHeight),
                    GraphicsUnit.Pixel);
                
                
                i++;
            }

            g.Dispose();

            return bmpALL;

        }

        const int ReConnectionConut = 5;
        const int CountErrorCount = 10;

        int[] LastCount;
        int[] CountErrorRetry;
        int[] SideErrorRetry;

        StringBuilder m_strErr = new StringBuilder(1024);

        public void CheckConnection(int sideindex)
        {

            int NowCount = pxd_capturedFieldCount(1 << sideindex);    //判?是否掉?

            if (CountErrorRetry[sideindex] < CountErrorCount)
            {
                if (LastCount[sideindex] != NowCount)
                {
                    LastCount[sideindex] = NowCount;
                    CountErrorRetry[sideindex] = 0;
                }
                else
                {
                    CountErrorRetry[sideindex]++;
                    
                    pxd_goUnLive(1 << sideindex);
                    pxd_goLive(1 << sideindex, 1);

                    //ErrorWriter.WriteLine(TimerClass.DateTimeString + "," + "Camera " + ((int)sideindex + 1).ToString() + " Lost Connection " + CountErrorRetry[(int)sideindex].ToString() + " times.");
                    if (CountErrorRetry[sideindex] >= CountErrorCount)
                    {
                        //SetErrorScreen(Side);
                        //MessageBox.Show("Camera " + ((int)Side + 1).ToString() + " Connection Error,Please Check the Cable。", "MAIN", MessageBoxButtons.OK);
                        //ErrorWriter.WriteLine(TimerClass.DateTimeString + "," + "Camera " + ((int)sideindex + 1).ToString() + " Do Reconnection.");

                        //CCDBMP[sideindex].Dispose();
                        //CCDBMP[sideindex] = new Bitmap(CCDWidth, CCDHeight);
                        //ReconnectInit();
                    }
                }
            }

            if (SideErrorRetry[sideindex] < ReConnectionConut)
            {
                int i = pxd_mesgFaultText((1 << sideindex), m_strErr, 1024);
                if (i != 0)
                {
                    SideErrorRetry[sideindex]++;
                    if (SideErrorRetry[sideindex] >= ReConnectionConut)
                    {
                        //CCDBMP[sideindex].Dispose();
                        //CCDBMP[sideindex] = new Bitmap(CCDWidth, CCDHeight);
                    }
                }
            }
            else
            {
                //CCDBMP[sideindex].Dispose();
                //CCDBMP[sideindex] = new Bitmap(CCDWidth, CCDHeight);
            }
        }
    }
}
