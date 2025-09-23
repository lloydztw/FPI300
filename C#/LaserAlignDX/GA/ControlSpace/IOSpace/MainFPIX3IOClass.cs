
using JetEazy.ControlSpace;
using System.Drawing;

namespace VsCommon.ControlSpace.IOSpace
{
    public class MainFPIX3IOClass : GeoIOClass, IPlcIoFPIX3
    {

        #region 发送数据的格式排列注释
        /*
         * 线扫相机触发方式
         * 通过读数头采集AB相的方式触发相机采图。
         * 实际两个载台需要线扫，对应一个相机取图。
         * 每颗产品相机给结果的顺序应该以S型给PLC,比如X方向5颗，Y方向10颗，每一颗位置的定义应该如下所示：
            [0]   [1]   [2]   [3]   [4]
            [9]   [8]   [7]   [6]   [5]
            [10]  [11]  [12]  [13]  [14]
            ……
         * 
         * 飞拍相机触发方式
         * 通过位置比对高速输出信号触发飞拍相机取像，
         * 实际两组飞拍轴对应一个相机取图。取像时光源闪亮，未触发时不亮。
            因为轴是从左向右移动触发飞拍，所以最新拍到的是最右边的吸嘴（吸嘴4），
            但相机给结果和偏移值应该是按从左到右的顺序给PLC:
            偏移   吸嘴1         吸嘴2          吸嘴3          吸嘴4
              X        [0]                [3]               [6]              [9]   
              Y        [1]                [4]               [7]              [10]
              R        [2]                [5]               [8]              [11]
         * 
         * 
         * 
         * 
         */
        #endregion

        const string m_Format = "0.000";

        public enum ADRMainFPIX3 : int
        {
            COUNT = 10,

            ADR_Heart = 0,
            ADR_LineScanRecipe = 1,
            ADR_SoftwareReady = 2,
            ADR_LineScanStart = 3,
            ADR_LineScanReady = 4,
            ADR_LineScanDone = 5,
            ADR_LineScanResult = 6,
            ADR_QRUsed = 7,
            ADR_QRJusgeUsed = 8,
            ADR_ScanStatus = 9,
        }

        public MainFPIX3IOClass()
        {

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            ADDRESSARRAY = new AddressClass[(int)ADRMainFPIX3.COUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_Heart] = new AddressClass(ReadINIValue("Operation Address", "Heart", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe] = new AddressClass(ReadINIValue("Operation Address", "LineScanRecipe", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_SoftwareReady] = new AddressClass(ReadINIValue("Operation Address", "SoftwareReady", "", INIFILE));

            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanStart] = new AddressClass(ReadINIValue("Operation Address", "LineScanStart", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanReady] = new AddressClass(ReadINIValue("Operation Address", "LineScanReady", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanDone] = new AddressClass(ReadINIValue("Operation Address", "LineScanDone", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanResult] = new AddressClass(ReadINIValue("Operation Address", "LineScanResult", "", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRUsed] = new AddressClass(ReadINIValue("Operation Address", "ADR_QRUsed", "0:Gvl_PhotoPC.bQRUsed", INIFILE));

            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRJusgeUsed] = new AddressClass(ReadINIValue("Operation Address", "ADR_QRJusgeUsed", "0:Gvl_PhotoPC.bQRJudgeUsed", INIFILE));
            ADDRESSARRAY[(int)ADRMainFPIX3.ADR_ScanStatus] = new AddressClass(ReadINIValue("Operation Address", "ADR_ScanStatus", "0:Gvl_PhotoPC.iScanStatus", INIFILE));



        }
        public override void SaveData()
        {

        }

        /// <summary>
        /// 启动软件的ready
        /// </summary>
        public bool bSoftwareReady
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_SoftwareReady];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_SoftwareReady];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        public bool bSyncClock
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_Heart];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_Heart];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }

        /// <summary>
        /// PLC->PC 马达移动到开始位通知pc信号
        /// </summary>
        public bool bScanStart
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanStart];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
        }
        /// <summary>
        /// PC->PLC 线扫准备就绪
        /// </summary>
        public bool bScanReady
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanReady];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanReady];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        /// <summary>
        /// PC->PLC完成信号和结果一起给
        /// </summary>
        public bool bScanDone
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanDone];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanDone];
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        public bool bQRUsed
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRUsed];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRUsed];
            //    PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            //}
        }
        public bool bQRJudgeUsed
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRJusgeUsed];
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRJusgeUsed];
            //    PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            //}
        }
        /// <summary>
        /// PLC->PC 线扫状态,1-尺寸外观,2-读码,3-空载台
        /// </summary>
        public int iScanStatus
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_ScanStatus];
                int iret = 0;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainX1.ADR_LineScannWhatFor];
            //    PLC[address.SiteNo].WriteVari(address.Address0, value);
            //}
        }

        /// <summary>
        /// 1：ok 2:ng
        /// </summary>
        public int iScanResult
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanResult];
                int iret = 1;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanResult];
                PLC[address.SiteNo].WriteVari(address.Address0, value.ToString());
            }
        }

        /// <summary>
        /// PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG
        /// 单颗的线扫结果(预留300个)
        /// PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <param name="singleResults">ARRAY[0..299] OF INT</param>
        public void iSingleResult(int[] singleResults)
        {
            if (singleResults == null)
                return;
            if (singleResults.Length > 0)
            {
                for (int i = 0; i < singleResults.Length; i++)
                {
                    AddressClass address = getCipAdress($"iSingleResult[{i}]");
                    PLC[address.SiteNo].WriteVari(address.Address0, singleResults[i].ToString());
                }
            }
        }
        /// <summary>
        /// PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到
        /// 单颗产品的读码比对结果(预留300个)
        /// 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <param name="QRResults">ARRAY[0..299] OF INT</param>
        public void iQRResult(int[] QRResults)
        {
            if (QRResults == null)
                return;
            if (QRResults.Length > 0)
            {
                for (int i = 0; i < QRResults.Length; i++)
                {
                    AddressClass address = getCipAdress($"iQRResult[{i}]");
                    PLC[address.SiteNo].WriteVari(address.Address0, QRResults[i].ToString());
                }
            }
        }
        /// <summary>
        /// PC->PLC 线扫偏移值XYR
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)
        /// 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <param name="scanOffsets">ARRAY[0..899] OF REAL</param>
        public void rScanOffset(float[] scanOffsets)
        {
            if (scanOffsets == null)
                return;
            if (scanOffsets.Length > 0)
            {
                for (int i = 0; i < scanOffsets.Length; i++)
                {
                    AddressClass address = getCipAdress($"rScanOffset[{i}]");
                    PLC[address.SiteNo].WriteVari(address.Address0, scanOffsets[i].ToString());
                }
            }
        }


        public string sLotID
        {
            get
            {
                AddressClass address = getCipAdress("sLotID");
                string ret = PLC[address.SiteNo].ReadVari(address.Address0);
                if (string.IsNullOrEmpty(ret))
                {
                    ret = "Lot_NONE";
                }
                return ret;
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe];
            //    PLC[address.SiteNo].WriteVari(address.Address0, value);
            //}
        }
        public string sStripID
        {
            get
            {
                AddressClass address = getCipAdress("sStripID");
                string ret = PLC[address.SiteNo].ReadVari(address.Address0);
                if (string.IsNullOrEmpty(ret))
                {
                    ret = "Strip_NONE";
                }
                return ret;
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe];
            //    PLC[address.SiteNo].WriteVari(address.Address0, value);
            //}
        }
        public int iFlyStart
        {
            get
            {
                AddressClass address = getCipAdress("iFlyStart");
                int iret = 1;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
            //set
            //{
            //    AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanResult];
            //    PLC[address.SiteNo].WriteVari(address.Address0, value.ToString());
            //}
        }
        public bool bFlyReady
        {
            get
            {
                AddressClass address = getCipAdress("bFlyReady");
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = getCipAdress("bFlyReady");
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        public bool bFlyDone
        {
            get
            {
                AddressClass address = getCipAdress("bFlyDone");
                return PLC[address.SiteNo].ReadVari(address.Address0).ToLower() == "true";
            }
            set
            {
                AddressClass address = getCipAdress("bFlyDone");
                PLC[address.SiteNo].WriteVari(address.Address0, (value ? "true" : "false"));
            }
        }
        /// <summary>
        /// PC->PLC 飞拍结果,1-Ok,2-Ng,3-空 单颗的飞拍结果
        /// </summary>
        /// <param name="flyResults">ARRAY[0..3] OF INT</param>
        public void iFlyResult(int[] flyResults)
        {
            if (flyResults == null)
                return;
            if (flyResults.Length > 0)
            {
                for (int i = 0; i < flyResults.Length; i++)
                {
                    AddressClass address = getCipAdress($"iFlyResult[{i}]");
                    PLC[address.SiteNo].WriteVari(address.Address0, flyResults[i].ToString());
                }
            }
        }
        /// <summary>
        /// PC->PLC 飞拍补偿(X,Y,R) 单颗的补偿结果([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共4个) ARRAY[0..11] OF REAL
        /// </summary>
        /// <param name="Offsets">ARRAY[0..11] OF REAL</param>
        public void rOffset(float[] Offsets)
        {
            if (Offsets == null)
                return;
            if (Offsets.Length > 0)
            {
                for (int i = 0; i < Offsets.Length; i++)
                {
                    AddressClass address = getCipAdress($"rOffset[{i}]");
                    PLC[address.SiteNo].WriteVari(address.Address0, Offsets[i].ToString());
                }
            }
        }

        /// <summary>
        /// PC->PLC 线扫配方切换结果1-Ok,2-Ng
        /// </summary>
        public int iRecipeNum
        {
            get
            {
                AddressClass address = getCipAdress("iRecipeNum");
                int iret = 1;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
            set
            {
                AddressClass address = getCipAdress("iRecipeNum");
                PLC[address.SiteNo].WriteVari(address.Address0, value.ToString());
            }
        }
        /// <summary>
        /// PLC->PC 线扫使用配方名
        /// </summary>
        public string sRecipeName
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe];
                return PLC[address.SiteNo].ReadVari(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe];
                PLC[address.SiteNo].WriteVari(address.Address0, value);
            }
        }

        /// <summary>
        /// 载台一纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        public void SetStage1(int eIndex, PointF pt0, PointF pt1)
        {
            if (eIndex == 0)
            {
                AddressClass addressX0 = new AddressClass($"0:Gvl_Stage1.X[0]");
                PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
                AddressClass addressY0 = new AddressClass($"0:Gvl_Stage1.Y[0]");
                PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            }
            if (eIndex == 1)
            {
                AddressClass addressX1 = new AddressClass($"0:Gvl_Stage1.X[1]");
                PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
                AddressClass addressY1 = new AddressClass($"0:Gvl_Stage1.Y[1]");
                PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            }

        }
        /// <summary>
        /// 载台二纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        public void SetStage2(int eIndex, PointF pt0, PointF pt1)
        {
            if (eIndex == 0)
            {
                AddressClass addressX0 = new AddressClass($"0:Gvl_Stage2.X[0]");
                PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
                AddressClass addressY0 = new AddressClass($"0:Gvl_Stage2.Y[0]");
                PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            }
            if (eIndex == 1)
            {
                AddressClass addressX1 = new AddressClass($"0:Gvl_Stage2.X[1]");
                PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
                AddressClass addressY1 = new AddressClass($"0:Gvl_Stage2.Y[1]");
                PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            }
        }

        /// <summary>
        /// 使用那个线扫平台;=1平台一;=2平台二
        /// </summary>
        public int iScanStage
        {
            get
            {
                AddressClass address = getCipAdress("iScanStage");
                int iret = 1;
                string str = PLC[address.SiteNo].ReadVari(address.Address0).ToLower();
                int.TryParse(str, out iret);
                return iret;
            }
        }

        AddressClass getCipAdress(string eAdrStr)
        {
            AddressClass address = new AddressClass($"0:Gvl_PhotoPC.{eAdrStr}");
            return address;
        }

    }
}
