
//#define FATEK
//#define FX3U

using Eazy_Project_III;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.CCDSpace;
using JetEazy.ControlSpace;
using JetEazy.DBSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.OPSpace;
using JetEazy.PropertyGridSpace;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace.ParaSpace;
using LaserAlignDX.ControlSpace.MachineSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.IO;
using TravellerMINIX6.OPSpace;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;


namespace Traveller106
{
    public class Universal : JetEazy.Universal
    {
        public static readonly bool N_THREADS_ENABLED = true;
        public static readonly int N_THREADS = 16;

        public static bool IsNoUseCCD = false;
        public static bool IsNoUseIO = IsNoUseCCD;
        public static bool IsNoUseMotor = IsNoUseIO;
        public static bool IsSilentMode = IsNoUseIO;
        public static bool IsAutoLogin = IsNoUseCCD;

        public const string VersionDate = "2025/09/17";

        public const VersionEnum VERSION = VersionEnum.LASER;
        public const OptionEnum OPTION = OptionEnum.MAIN_FPIX3;
        public static FactoryName FACTORYNAME = FactoryName.NONE;

        /// <summary>
        /// 这个用来区分是否在图片上画图
        /// </summary>
        public static bool IsDrawImage = true;
        public static bool IsOfflineDataVerifty = false;
        public static bool IsOfflineAutoCaliTest = false;

        /// <summary>
        /// 是否打开飞拍界面  如果打开了 则主程序不要飞拍测试
        /// </summary>
        public static bool IsOpenFlyForm = false;

        public static string APP_ROOT_PATH
        {
            get
            {
                switch (OPTION)
                {
                    default:
                        // 直接指定成 最後佈署的資料夾
                        // 這樣 原代碼 C# 專案, 
                        //      才能放在任意資料夾
                        //      不需要 依附於 最後佈署的資料夾 
                        //return "D:\\AUTOMATION\\Eazy FPI30\\_BIN_";
                        //return "D:\\AUTOMATION\\Eazy FPI30\\_M04_";
                        return "D:\\AUTOMATION\\Eazy FPI30\\_V03_";
                }
            }
        }

        public static string UIPATH
        {
            get { return System.IO.Path.Combine(APP_ROOT_PATH, "UI"); }
        }

        //public static string CODEPATH = @"D:\AUTOMATION";
        public static string VEROPT => VERSION.ToString() + "-" + OPTION.ToString();
        public static string MAINPATH => APP_ROOT_PATH + @"\" + VEROPT;

        public static string DBPATH => MAINPATH + @"\DB";
        public static string RCPPATH => MAINPATH + @"\PIC";
        //public static string UIPATH = CODEPATH + @"\" + VERSION.ToString() + "UI";

        public static string LOG_ROOT
        {
            get
            {
                switch (OPTION)
                {
                    default:
                        return @"D:\log\" + OPTION.ToString();
                }
            }
        }
        public static string LOG_TXT_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Logs"); }
        }
        public static string LOG_SQL_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Sqls"); }
        }
        public static string LOG_IMG_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Images"); }
        }
        public static string LOG_ALARM_EVENT_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Alarms"); }
        }
        public static string LOG_INFO_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Infos"); }
        }
        public static string LOG_TCP_PATH
        {
            get { return System.IO.Path.Combine(LOG_ROOT, "Tcps"); }
        }

        //public static string MAPPINGDATA => MAINPATH + @"\MAPPINGDATA";                       //存储tray的数据
        public static string HISTORY => MAINPATH + @"\HISTORYDATA";                             //存储tray的数据
        public static string COLLECT => MAINPATH + @"\COLLECT";
        //public static string BACKUPDBPATH => MAINPATH + @"\BACKUPDB";
        public static string WORKPATH => MAINPATH + @"\WORK";
        //public static string DEBUGRAWPATH => MAINPATH + @"\ORG";                              //偵錯儲存的原圖位置
        public static string DEBUGRESULTPATH => MAINPATH + @"\DEBUG";                           //偵錯結果圖位置
        //public static string TESTRESULTPATH => @"D:\COPYDATA";                                //偵錯結果圖位置
        public static string DEBUGSRCPATH => MAINPATH + @"\SRCDEBUG";                           //離線測試用的原圖位置
        //public static string OCRIMAGEPATH => @"D:\LOA\OCR\";                                  //保存的OCR测试图位置  
        //public static string BarcodeIMAGEPATH => @"D:\LOA\Barcode\";                          //保存的OCR测试图位置  
        //public static string DEBUG_DATA_IMAGE => @"D:\01测试镭雕引导定位存储图片";              //保存的OCR测试图位置  
        public static string PATH_CALI => MAINPATH + @"\CALI";

        /// <summary>
        /// 跑线时读到SN.txt里的东西
        /// </summary>
        public static string DATASNTXT = "";

#if (OPT_GAARA_RESERVED)
        public static string RELATECOLORSTR = "";
        public static string SHOWBMPSTRING => "view.png";
        public static string PlayerPASSPATH => WORKPATH + @"\TADA.wav";
        public static string PlayerFAILPATH => WORKPATH + @"\RoutingNG.wav";
        public static string PlayerOPPWRATPATH => WORKPATH + @"\OPPWRAP.wav";
        public static string RunDebugOrRelease => "";
        public static string FAILBARCODE = "";
#endif

#if (OPT_DATA_CNN)
        //public static string MainX6_Path = "D:\\CollectPictures\\Inspection\\";
        static string DATACNNSTRING => "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DBPATH + @"\DATA.mdb;Jet OLEDB:Database Password=12892414;";
#endif
        static int LanguageIndex = 0;

        public static string InitialErrorString = "";
        public static System.Drawing.Point MainFormLocation = new System.Drawing.Point(0, 0);
        /// <summary>
        /// 离线模式自动登入账户admin
        /// </summary>
        public static bool IsOfflineUserAutoLogin = false;

        public static CAMERAClass[] CAMERAS
        {
            get; 
            private set;
        }
        //public static CCDCollectionClass CCDCollection;
        public static CCDCollectionClass CCDCollection
        {
            get;
            private set;
        }
        public static MachineCollectionClass MACHINECollection
        {
            get;
            private set;
        }

        //public static GdxCameraDpiCalibrator[] CAMDpi_Cali;
        /// <summary>
        /// 线扫相机
        /// </summary>
        public static IxLineScanCam IxLineScan = null;
        public static ModuleClass[] Modules = null;
        //public static ModuleClass ModulesLinescanEx = null;
        public static ChannelBarcodeClass[] ChannelBarcode = null;

#if (OPT_GA_RESERVED)
        public static FreeImageAPI.FreeImageBitmap bmpGlobalFreeImage = new FreeImageAPI.FreeImageBitmap(1, 1);
#endif

        public static CommonLogClass COMMON_LOG_INFOS = new CommonLogClass();
        public static PropGrid_CaliClass CaliClass = new PropGrid_CaliClass();
        public static LineScanCalibrateClass[] LineScanCalibrateClasses = null;

        /// <summary>
        /// 飞拍相机
        /// </summary>
        public static IxLineScanCam IxFlyAreaCam = null;

        public static AccDBClass ACCDB;
        public static EsssDBClass ESSDB;
        public static RCPDBClass RCPDB;
        public static RUNDBClass RUNDB;
        public static CalibrationPlateClass CALIBRATIONPLATE;

        public static ClientSocket X6_LASER_CLIENT = null;      //>>> FPI30 沒用到
        public static ClientSocket X6_HANDLE_CLIENT = null;     //>>> FPI30 沒用到

        public static bool Initial(int langIndex)
        {
            //int[] ints = new int[2] { 2, 2 };
            //string str = new List<int>(ints).ToString();

            bool ret = true;
            //WORKPATH = MAINPATH + @"\WORK";

            try
            {
                FACTORYNAME = (FactoryName)INI.Instance.FactoryNameIndex;
            }
            catch
            {
                FACTORYNAME = FactoryName.NONE;
            }
            //switch (INI.Instance.FactoryNameIndex)
            //{
            //    case 1:
            //        break;
            //    default:
            //        break;
            //}

            ////初始化语言
            //JetEazy.BasicSpace.LanguageExClass.Instance.Load(WORKPATH);
            //JetEazy.BasicSpace.LanguageExClass.Instance.LanguageIndex = 1;
            ////JetEazy.BasicSpace.LanguageExClass.Instance.FirstCsv = true;
            LanguageIndex = langIndex;

            COMMON_LOG_INFOS.LogPath = LOG_INFO_PATH;

            //myTestProgramme();


            CreateDebugDirectories();

            ACCDB = new AccDBClass(DBPATH + @"\ACCDB.jdb");
            ESSDB = new EsssDBClass(DBPATH + @"\ESSDB.jdb");
            RCPDB = new RCPDBClass(DBPATH + @"\RCPDB.jdb", RCPPATH, ESSDB.LastRecipeIndex);
            //RCPDB = new RCPDBClass(DBPATH + @"\RCPDB.jdb", RCPPATH, 1);//總是加載第一個參數 即為正常模式
            RUNDB = new RUNDBClass(DBPATH + @"\RUNDB.jdb");

            LineScanCalibrateClasses = new LineScanCalibrateClass[4];
            int i = 0;
            while (i < 4)
            {
                LineScanCalibrateClasses[i] = new LineScanCalibrateClass();
                LineScanCalibrateClasses[i].Initial(WORKPATH, 0, $"Calibrate_default_info{i}.ini");
                LineScanCalibrateClasses[i].Load();
                i++;
            }

            RecipeFPIX3Class.Instance.Initial(RCPPATH, ESSDB.LastRecipeIndex, "Strip_default_info.ini");
            RecipeFPIX3Class.Instance.Load();
            //CaliClass.FromingStr(INI.Instance.cali_paras);

            MvdFindCircleClass.Instance.Initial(WORKPATH, 0, $"CalibrateForm_default_info.ini");
            MvdFindCircleClass.Instance.Load();

            //LineScanCalibrateClass.Instance.Initial(WORKPATH, 0, "Calibrate_default_info.ini");
            //LineScanCalibrateClass.Instance.Load();
            //LineScanCalibrateClass.Instance.Save();

#if (OPT_MAIN_X2)
            switch (Universal.OPTION)
            {
                case OptionEnum.MAIN_X2:

                    //if (InspectX2Class.Instance.bCheckRepeatCode)
                    {
                        JzCheckRepeatClass.Instance.mysql_server_ip = INI.Instance.mysql_server_ip;
                        JzCheckRepeatClass.Instance.mysql_server_port = INI.Instance.mysql_server_port;
                        JzCheckRepeatClass.Instance.mysql_server_user = INI.Instance.mysql_server_user;
                        JzCheckRepeatClass.Instance.mysql_server_pwd = INI.Instance.mysql_server_pwd;
                        JzCheckRepeatClass.Instance.mysql_server_db = INI.Instance.mysql_server_db;
                        JzCheckRepeatClass.Instance.SetLogPath(LOG_SQL_PATH);
                        JzCheckRepeatClass.Instance.OpenDB();
                    }

                    break;
            }
#endif

            ret &= InitialMachineCollection();

            if (!ret)
            {
                //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
                //JetEazy.BasicSpace.VsMSG.Instance.Warning("plc连接错误，请检查。");
                VsMessageBox.Warning("PLC 连接错误，请检查設定!");
                //return false;
            }

            ret &= InitialCCD();

            if (!ret)
            {
                //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
                //return false;
                //JetEazy.BasicSpace.VsMSG.Instance.Warning("CCD连接错误，请检查。");
                VsMessageBox.Warning("CCD 连接错误，请检查設定!");
            }

            //ret &= MyTcpSocketInitial();
            MyTcpSocketInitial();
            //if (!ret)
            //{
            //    //InitialErrorString = myLanguage.Messages("msg1", LanguageIndex);
            //    //return false;
            //    JetEazy.BasicSpace.VsMSG.Instance.Warning("连接打标服务器错误，请检查。");
            //}
            return ret;
        }
        public static void SetLanguage(int langindex)
        {
            LanguageIndex = langindex;
        }

        static bool InitialMachineCollection()
        {
            bool ret = true;

            string opstr = "";

            switch (VERSION)
            {
                case VersionEnum.LASER:

                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X1:

                            opstr += "1,";  //1個 PLC  
                            opstr += "0,";   //14個軸
                            opstr += $"{INI.Instance.LedControlCount},";   //0 Projector
                            opstr += "0,";   //4 barcode sacn

                            MainX1MachineClass machine = new MainX1MachineClass(Machine_EA.MAIN_X1, opstr, WORKPATH, IsNoUseIO);
                            ret = machine.Initial(IsNoUseIO, IsNoUseMotor);

                            MACHINECollection = new MachineCollectionClass();
                            MACHINECollection.Intial(VERSION, OPTION, machine);


                            break;
                        case OptionEnum.MAIN_FPIX3:

                            opstr += "1,";  //1個 PLC  
                            opstr += "0,";   //14個軸
                            opstr += $"2,";   //0 Projector
                            opstr += "0,";   //4 barcode sacn

                            MainFPIX3MachineClass machineFPIX3 = new MainFPIX3MachineClass(Machine_EA.MAIN_FPIX3, opstr, WORKPATH, IsNoUseIO);
                            ret = machineFPIX3.Initial(IsNoUseIO, IsNoUseMotor);

                            MACHINECollection = new MachineCollectionClass();
                            MACHINECollection.Intial(VERSION, OPTION, machineFPIX3);


                            break;
                    }
                    break;
                case VersionEnum.AOI:
                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:

                            opstr += "1,";  //1個 PLC  
                            opstr += "0,";   //14個軸
                            opstr += $"0,";   //0 Projector
                            opstr += "0,";   //4 barcode sacn

                            MainX2MachineClass machine = new MainX2MachineClass(Machine_EA.MAIN_X2, opstr, WORKPATH, IsNoUseIO);
                            ret = machine.Initial(IsNoUseIO, IsNoUseMotor);

                            MACHINECollection = new MachineCollectionClass();
                            MACHINECollection.Intial(VERSION, OPTION, machine);


                            break;
                    }
                    break;
                default:
                    break;
            }

            return ret;
        }

#if (OPT_GA_DISPENSING_點膠機)
        static bool InitialMeasureHeight()
        {
            bool ret = true;

            string opstr = "";

            switch (VERSION)
            {
                case VersionEnum.PROJECT:

                    switch (OPTION)
                    {
                        case OptionEnum.DISPENSING:

                            //LEClass.Instance.Init(INI.Instance.CfgPath, INI.Instance.HWCPath, !INI.Instance.IsUseMeasureHeight);
                            //ret = LEClass.Instance.Open() == 0;

                            break;
                    }

                    break;

                case VersionEnum.ALLINONE:
                    break;
                case VersionEnum.AUDIX:
                    break;
                default:
                    break;
            }

            return ret;
        }
        static int measureInitial()
        {
            //CAMDpi_Cali = new GdxCameraDpiCalibrator[CameraConfig.Instance.COUNT];
            //int i = 0;
            //while (i < CameraConfig.Instance.COUNT)
            //{
            //    string _path_name = Universal.WORKPATH + "\\" + "Cam" + i.ToString() + ".cali";
            //    if (File.Exists(_path_name))
            //    {
            //        ArrayList array = new ArrayList();
            //        Read(out array, _path_name);
            //        if (array != null)
            //        {
            //            CAMDpi_Cali[i] = new GdxCameraDpiCalibrator();
            //            CAMDpi_Cali[i].FromString((string)array[0]);
            //            CAMDpi_Cali[i].bmpBase1 = (Bitmap)array[1];
            //            CAMDpi_Cali[i].bmpBase2 = (Bitmap)array[2];
            //        }
            //        else
            //        {
            //            if (CAMDpi_Cali[i] == null)
            //                CAMDpi_Cali[i] = new GdxCameraDpiCalibrator();
            //        }
            //    }
            //    else
            //    {
            //        if (CAMDpi_Cali[i] == null)
            //            CAMDpi_Cali[i] = new GdxCameraDpiCalibrator();

            //        ArrayList array = new ArrayList();
            //        array.Add(CAMDpi_Cali[i].ToString());
            //        array.Add(CAMDpi_Cali[i].bmpBase1);
            //        array.Add(CAMDpi_Cali[i].bmpBase2);
            //        Write(array, _path_name);
            //    }
            //    i++;
            //}

            return 0;
        }
#endif

        static bool InitialCCD()
        {
#if (OPT_GA_OLD)
            bool ret = true;
            CameraConfig.Instance.Initial(WORKPATH);
            switch (CameraConfig.Instance.cameras[0].CameraType)
            {
                case "HUARUI":
                    IxLineScan = new LINESCAN_HUARUI();
                    break;
                case "DVP2":
                    IxLineScan = new Linescan_Dvp2();
                    break;
                case "ITK":
                    IxLineScan = new Linescan_iTK();
                    break;
                case "MIND":
                    IxLineScan = new Linescan_Mind();
                    break;
            }
            IxLineScan.Init(CameraConfig.Instance.cameras[0].IsDebug, CameraConfig.Instance.cameras[0].ToCameraString());
            ret = IxLineScan.Open();
            if (ret)
            {
                switch (CameraConfig.Instance.cameras[1].CameraType)
                {
                    case "HUARUI":
                        IxFlyAreaCam = new LINESCAN_HUARUI();
                        break;
                    case "DVP2":
                        IxFlyAreaCam = new Linescan_Dvp2();
                        break;
                    case "ITK":
                        IxFlyAreaCam = new Linescan_iTK();
                        break;
                    case "MIND":
                        IxFlyAreaCam = new Linescan_Mind();
                        break;
                }
                IxFlyAreaCam.Init(CameraConfig.Instance.cameras[1].IsDebug, CameraConfig.Instance.cameras[1].ToCameraString());
                ret = IxFlyAreaCam.Open();
                if (ret)
                {
                    IxFlyAreaCam.StartGrab();
                }
            }
            //if (ret)
            //{
            //    IxLineScan.StartGrab();
            //}
            return ret;
#endif

            var config = CameraConfig.Instance;
            config.Initial(WORKPATH);

            if (IsNoUseCCD)
            {
                foreach (var camParams in config.cameras)
                    camParams.IsDebug = true;
            }

            IxLineScan = GaCameraFactory.LoadLineScanCamera(config.cameras[0]);
            bool ok = IxLineScan != null && IxLineScan.Open();

            if (ok)
            {
                IxFlyAreaCam = GaCameraFactory.LoadLineScanCamera(config.cameras[1]);
                ok = IxFlyAreaCam != null && IxFlyAreaCam.Open();
            }

            if (ok)
            {
                IxFlyAreaCam.StartGrab();
                //>>> IxLineScan.StartGrab();
            }

            return ok;
        }
        static bool MyTcpSocketInitial()
        {
#if (OPT_GA_MAIN_X2)
            bool ret = true;

            switch (VERSION)
            {
                case VersionEnum.AOI:

                    switch (OPTION)
                    {
                        case OptionEnum.MAIN_X2:
                            if (X6_LASER_CLIENT == null)
                            {
                                X6_LASER_CLIENT = new ClientSocket("laser");
                            }
                            X6_LASER_CLIENT.Host = INI.Instance.tcp_ip;
                            X6_LASER_CLIENT.Port = INI.Instance.tcp_port;
                            int iret = X6_LASER_CLIENT.ConnectServer();
                            //ret = iret == 0;
                            if (iret != 0)
                            {
                                MessageBox.Show(ToChangeLanguage("连接打标服务器错误请检查。") + "ip=" + INI.Instance.tcp_ip + ",port=" + INI.Instance.tcp_port.ToString(), ToChangeLanguage("初始化"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            if (IsNoUseCCD)
                            {
                                if (X6_HANDLE_CLIENT == null)
                                    X6_HANDLE_CLIENT = new ClientSocket("handle");
                                //if (X6_HANDLE_CLIENT == null)
                                //    X6_HANDLE_CLIENT = new ClientSocket("handle32002");
                            }
                            else
                            {
                                if (X6_HANDLE_CLIENT == null)
                                    X6_HANDLE_CLIENT = new ClientSocket("handle");
                            }

                            X6_HANDLE_CLIENT.Host = INI.Instance.tcp_handle_ip;
                            X6_HANDLE_CLIENT.Port = INI.Instance.tcp_handle_port;
                            iret = X6_HANDLE_CLIENT.ConnectServer(!INI.Instance.tcp_handle_open);
                            if (iret != 0)
                            {
                                MessageBox.Show("连接handle服务器错误，请检查。" + "ip=" + INI.Instance.tcp_handle_ip + ",port=" + INI.Instance.tcp_handle_port.ToString(), "初始化", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            X6_LASER_CLIENT.Log.LogPath = LOG_TCP_PATH;
                            X6_LASER_CLIENT.Log.LogFilename = "laserServer_tcp";

                            X6_HANDLE_CLIENT.Log.LogPath = LOG_TCP_PATH;
                            X6_HANDLE_CLIENT.Log.LogFilename = "handleServer_tcp";

                            break;
                    }

                    break;

            }

            return ret;
#else
            return false;
#endif
        }
        static void CreateDebugDirectories()
        {
            //if (!Directory.Exists(MAPPINGDATA))
            //    Directory.CreateDirectory(MAPPINGDATA);
            //if (!Directory.Exists(HISTORY))
            //    Directory.CreateDirectory(HISTORY);
            //if (!Directory.Exists(COLLECT))
            //    Directory.CreateDirectory(COLLECT);
            if (!Directory.Exists(DEBUGSRCPATH))
                Directory.CreateDirectory(DEBUGSRCPATH);
            if (!Directory.Exists(DEBUGRESULTPATH))
                Directory.CreateDirectory(DEBUGRESULTPATH);
            //if (!Directory.Exists(PATH_CALI))
            //    Directory.CreateDirectory(PATH_CALI);
        }

        public static void Dispose()
        {
            // Traveller160 沒有用到 JzCheckRepeatClass
            JzCheckRepeatClass.Instance.CloseDB();
            
            MACHINECollection?.Close();
            MACHINECollection = null;

            IxLineScan?.Close();
            //IxLineScan?.Dispose();
            IxLineScan = null;

            IxFlyAreaCam?.Close();
            //IxFlyAreaCam?.Dispose();
            IxFlyAreaCam = null;

            RecipeFPIX3Class.DisposeAll();
            ProcessRunFPIClass.DisposeAll();
            LtAoiFactory.DisposeAll();
        }
        public static void Close()
        {
            Dispose();
        }

#if (OPT_GA_RESERVED)
        /// <summary>
        /// 读出参数
        /// </summary>
        /// <param name="myArray">out 传入的集合</param>
        /// <param name="st_File">读哪个文件</param>
        /// <returns></returns>
        static bool Read(out ArrayList myArray, string st_File)
        {
            try
            {
                System.Runtime.Serialization.IFormatter formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Stream stream = new FileStream(st_File, FileMode.Open);
                myArray = (ArrayList)formater.Deserialize(stream);

                stream.Close();
                stream.Dispose();

                GC.Collect();//强制进行拉圾回收
                return true;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                myArray = null;
                GC.Collect();//强制进行拉圾回收
                return false;
            }

        }
        /// <summary>
        /// 记录参数
        /// </summary>
        /// <param name="mylist">需记录的集合</param>
        /// <param name="st_File">存放的路径</param>
        /// <returns></returns>
        static bool Write(ArrayList mylist, string st_File)
        {
            try
            {
                FileStream fs = new FileStream(st_File, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, mylist);
                fs.Close();

                fs.Dispose();
                GC.Collect();//强制进行拉圾回收
                return true;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                GC.Collect();//强制进行拉圾回收
                return false;
            }
        }

        static double GetAngle(PointF xP1World, PointF xP2World)
        {
            double angleOfLine = 0;
            if (xP2World.X > xP1World.X)
                angleOfLine = Math.Atan2((xP2World.Y - xP1World.Y), (xP2World.X - xP1World.X)) * 180 / Math.PI;
            else
                angleOfLine = Math.Atan2((xP1World.Y - xP2World.Y), (xP1World.X - xP2World.X)) * 180 / Math.PI;
            return angleOfLine;
        }
        static void myTestProgramme()
        {
            test_laser_dot_q0_phi();


            PointF p0 = new PointF(0, 0);
            PointF p1 = new PointF(-1,-1);
            double angle_x = GetAngle(p0, p1);

            //p0 = new PointF(1, 1);
            //p1 = new PointF(2, 2);
            //angle_x = GetAngle(p0, p1);


            JetEazy.QMath.QVector q0 = BuildVector(p0, p1);
            JetEazy.QMath.QVector q1 = BuildVector(p0, new PointF(p0.X + 1, p0.Y));

            // 計算向量夾角 A
            double dotProduct = q0 * q1;
            // 計算夾角的cos值
            double cosValue = dotProduct / (q0.NormLength * q1.NormLength);
            // 確保 cosValue 在 [-1, 1] 範圍內，避免浮點數精度問題
            cosValue = Math.Max(-1, Math.Min(1, cosValue));
            // 計算角度（以弧度表示）
            double _A = Math.Acos(cosValue);

            // 叉积判断方向，如果 b 在 a 的左边，取反
            if (q0.x * q1.y - q0.y * q1.x > 0)
            {
                _A = -_A;
            }

            double an = _A * 180 / Math.PI;

            #region 生成图片

            //Bitmap bmp = new Bitmap(6000, 6000);
            //Graphics g = Graphics.FromImage(bmp);
            //g.Clear(Color.Black);
            //g.FillRectangle(Brushes.White, 100, 5000, 10, 10);
            //g.FillRectangle(Brushes.White, 5000, 1000, 10, 10);
            //g.FillRectangle(Brushes.White, 4000, 4000, 10, 10);
            //g.Dispose();
            //bmp.Save("D:\\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            //PointF q2 = new PointF();
            //double phi = 0;
            //LaserDotCoordinate laserDotCoordinate = new LaserDotCoordinate();

            //PointF k0 = new PointF(100 + 5, -5005);
            //PointF k1 = new PointF(5000 + 5, -1005);
            //PointF k2 = new PointF(4000 + 5, -4005);
            //PointF q0 = new PointF(405f, -5005f);
            //PointF q1 = new PointF(5305f, -1005f);

            //k0 = new PointF(0, 110);
            //k1 = new PointF(10, 120);
            //k2 = new PointF(10, 110);

            //k0 = new PointF(0, 0);
            //k1 = new PointF(10, 10);
            //k2 = new PointF(10, 0);

            //k0 = new PointF(0.0178f, 109.874f);
            //k1 = new PointF(10.0063f, 119.8637f);
            //k2 = new PointF(9.9967f, 109.8726f);

            //CAoiCalibration MSRCalibrationUse = new CAoiCalibration();
            ////这里判断是否使用标定档转换坐标
            //string _pathMsr = OpenFilePicker("MSR Files (*.msr)|*.MSR|" + "All files (*.*)|*.*", "");
            //if (!string.IsNullOrEmpty(_pathMsr))
            //{
            //    if (File.Exists(_pathMsr))
            //    {
            //        MSRCalibrationUse.LoadBin(_pathMsr);
            //        MSRCalibrationUse.CalculateTransformMatrix();
            //    }
            //}

            //List<MSRItemClass> mSRItemClasses = new List<MSRItemClass>();
            //MSRCalibrationUse.GetCalibrationPoints(out PointF[,] views, out PointF[,] worlds);
            //for (int i = 0; i < 13; i++)
            //{
            //    for (int j = 0; j < 27; j++)
            //    {
            //        MSRItemClass mSRItem = new MSRItemClass();
            //        mSRItem.CenterPointF = views[i, j];
            //        mSRItem.RelatePointF = worlds[i, j];

            //        MSRCalibrationUse.TransformViewToWorld(mSRItem.CenterPointF, out mSRItem.RelatePointFViewToWorld);
            //        MSRCalibrationUse.TransformWorldToView(mSRItem.RelatePointF, out mSRItem.RelatePointFWorldToView);

            //        mSRItemClasses.Add(mSRItem);
            //    }
            //}

            //string reportstr = ",,,CXV,CYV,RXV,RYV,RW,RH,,CXW,CYW" + Environment.NewLine;
            //int indexreport = 1;
            //foreach (var item in mSRItemClasses)
            //{
            //    reportstr += $"{item.ReportIndex},{item.ToReportString()},{Environment.NewLine}";
            //    indexreport++;
            //}
            //if (!System.IO.Directory.Exists("D:\\report"))
            //    System.IO.Directory.CreateDirectory("D:\\report");

            ////SaveDataEXD(reportstr, "D:\\report\\MsrData_" + "All" + ".csv");
            //SaveDataEXD(reportstr, "D:\\report\\MsrData_" + JzTimes.DateTimeSerialStringFFF + ".csv");

            ////k0 = new PointF(5942f, 2644f);
            ////MSRCalibrationUse.TransformViewToWorld(k0, out PointF kk0);

            ////k1 = new PointF(30, 120);
            ////MSRCalibrationUse.TransformWorldToView(k1, out PointF kk1);

            //System.Diagnostics.Trace.WriteLine("");
            //MSRCalibrationUse.TransformViewToWorld(k1, out PointF kk1);
            //MSRCalibrationUse.TransformViewToWorld(k2, out PointF kk2);

            //laserDotCoordinate.SetKeyPoints(kk0, kk1, kk2);

            //laserDotCoordinate.SetKeyPoints(k0, k1, k2);

            //q0 = new PointF(647.04f, -5477.4f);
            //q1 = new PointF(4778.03f, -687.3f);
            //q0 = new PointF(0, 100);
            //q1 = new PointF(20, 120);

            //q0 = new PointF(3165, -3560);
            //q1 = new PointF(4093, -2650);

            ////q0 = new PointF(26264, -13450);
            ////q1 = new PointF(27191, -12542);

            //laserDotCoordinate.CalcPointQ2(q0, q1, out q2, out phi);
            ////laserDotCoordinate.CalcPointQ2(k0, k1, k2, q0, q1, out q2, out phi);
            //System.Diagnostics.Trace.WriteLine(q2);
            #endregion

        }

        /// <summary>
        /// 使用 p1, p2 建立向量
        /// </summary>
        static QVector BuildVector(PointF p1, PointF p2)
        {
            return new QVector(p2.X - p1.X, p2.Y - p1.Y);
        }

        static void test_laser_dot_q0_phi()
        {
            var ldc = new LaserDotCoordinate();
            var k0 = new PointF(0, 0);
            var k1 = new PointF(50f, 40f);
            var k2 = new PointF(10, 10);
            var q0 = new PointF(10f, 10f);
            var q1 = new PointF(60f, 50f);
            ldc.CalcPointQ2(k0, k1, k2, q0, q1, out PointF q2, out double phi);
            System.Diagnostics.Debug.WriteLine($"q2= {q2.X}, {q2.Y}");
            System.Diagnostics.Debug.WriteLine($"phi= {phi}");
            System.Diagnostics.Debug.WriteLine($"phi= {phi * 180 / Math.PI}");
        }
        static string OpenFilePicker(string DefaultPath, string DefaultName)
        {
            string retStr = "";

            OpenFileDialog dlg = new OpenFileDialog();

            //dlg.Filter = "BMP Files (*.bmp)|*.BMP|" + "All files (*.*)|*.*";
            dlg.Filter = DefaultPath;
            dlg.FileName = DefaultName;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                retStr = dlg.FileName;
            }
            return retStr;
        }
        static void SaveDataEXD(string DataStr, string FileName)
        {
            System.IO.StreamWriter stm = null;

            try
            {
                stm = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.Default);
                stm.WriteLine(DataStr);
                stm.Flush();
                stm.Close();
                stm.Dispose();
                stm = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
            }

            if (stm != null)
                stm.Dispose();
        }
#endif

        static string ToChangeLanguage(string eText)
        {
            string retStr = eText;
            retStr = LanguageExClass.Instance.GetLanguageText(eText);
            return retStr;
        }
    }
}
