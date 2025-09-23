#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-11 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.Drawing;


namespace VsCommon.ControlSpace.IOSpace
{
    /// <summary>
    /// 模擬 德龍 PLC IO
    /// </summary>
    public class MainFPIX3IOSim : GeoIOClass, IPlcIoFPIX3Sim
    {
        public event EventHandler<DoWorkEventArgs> OnRequestSimLineScan;
        public event EventHandler<DoWorkEventArgs> OnRequestSimFlyCam;

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

        //const string m_Format = "0.000";

        //public enum ADRMainFPIX3 : int
        //{
        //    COUNT = 10,

        //    ADR_Heart = 0,
        //    ADR_LineScanRecipe = 1,
        //    ADR_SoftwareReady = 2,
        //    ADR_LineScanStart = 3,
        //    ADR_LineScanReady = 4,
        //    ADR_LineScanDone = 5,
        //    ADR_LineScanResult = 6,
        //    ADR_QRUsed = 7,
        //    ADR_QRJusgeUsed = 8,
        //    ADR_ScanStatus = 9,
        //}

        private int _simRunCount = 0;
        private int _simChipCount = 0;
        private bool _simSoftwareReady = false;
        private int _simScanResult = 0;
        private int _simFlyStart = 0;
        private bool _simFlyDone = false;

        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            //ADDRESSARRAY = new AddressClass[(int)ADRMainFPIX3.COUNT];
            //PLC = plc;
            //INIFILE = path + "\\IO.INI";
            //LoadData();
        }
        public override void LoadData()
        {
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_Heart] = new AddressClass(ReadINIValue("Operation Address", "Heart", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanRecipe] = new AddressClass(ReadINIValue("Operation Address", "LineScanRecipe", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_SoftwareReady] = new AddressClass(ReadINIValue("Operation Address", "SoftwareReady", "", INIFILE));

            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanStart] = new AddressClass(ReadINIValue("Operation Address", "LineScanStart", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanReady] = new AddressClass(ReadINIValue("Operation Address", "LineScanReady", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanDone] = new AddressClass(ReadINIValue("Operation Address", "LineScanDone", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_LineScanResult] = new AddressClass(ReadINIValue("Operation Address", "LineScanResult", "", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRUsed] = new AddressClass(ReadINIValue("Operation Address", "ADR_QRUsed", "0:Gvl_PhotoPC.bQRUsed", INIFILE));

            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_QRJusgeUsed] = new AddressClass(ReadINIValue("Operation Address", "ADR_QRJusgeUsed", "0:Gvl_PhotoPC.bQRJudgeUsed", INIFILE));
            //ADDRESSARRAY[(int)ADRMainFPIX3.ADR_ScanStatus] = new AddressClass(ReadINIValue("Operation Address", "ADR_ScanStatus", "0:Gvl_PhotoPC.iScanStatus", INIFILE));
        }
        public override void SaveData()
        {
        }

        /// <summary>
        /// 启动软件的ready
        /// </summary>
        public bool bSoftwareReady
        {
            get => _simSoftwareReady;
            set
            {
                if (_simSoftwareReady != value)
                {
                    _simSoftwareReady = value;
                    if (_simSoftwareReady)
                    {
                        ((Action)sim_StartScan).BeginInvoke(null, null);
                    }
                }
            }
        }
        /// <summary>
        /// 心跳
        /// </summary>
        public bool bSyncClock
        {
            get;
            set;
        }

        /// <summary>
        /// PLC->PC 马达移动到开始位通知pc信号
        /// </summary>
        public bool bScanStart
        {
            get; set;
        }
        /// <summary>
        /// PC->PLC 线扫准备就绪
        /// </summary>
        public bool bScanReady
        {
            get; set;
        }
        /// <summary>
        /// PC->PLC完成信号和结果一起给
        /// </summary>
        public bool bScanDone
        {
            get; set;
        }

        public bool bQRUsed
        {
            get; set;
        }
        public bool bQRJudgeUsed
        {
            get; set;
        }

        /// <summary>
        /// PLC->PC 线扫状态,1-尺寸外观,2-读码,3-空载台
        /// </summary>
        public int iScanStatus
        {
            get; set;
        }

        /// <summary>
        /// 1：ok 2:ng
        /// </summary>
        public int iScanResult
        {
            get => _simScanResult;
            set
            {
                if (_simScanResult != value)
                {
                    _simScanResult = value;
                    ((Action)sim_StartFly).BeginInvoke(null, null);
                }
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
            //if (singleResults == null)
            //    return;
            //if (singleResults.Length > 0)
            //{
            //    for (int i = 0; i < singleResults.Length; i++)
            //    {
            //        AddressClass address = getCipAdress($"iSingleResult[{i}]");
            //        PLC[address.SiteNo].WriteVari(address.Address0, singleResults[i].ToString());
            //    }
            //}
        }
        /// <summary>
        /// PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到
        /// 单颗产品的读码比对结果(预留300个)
        /// 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <param name="QRResults">ARRAY[0..299] OF INT</param>
        public void iQRResult(int[] QRResults)
        {
            //if (QRResults == null)
            //    return;
            //if (QRResults.Length > 0)
            //{
            //    for (int i = 0; i < QRResults.Length; i++)
            //    {
            //        AddressClass address = getCipAdress($"iQRResult[{i}]");
            //        PLC[address.SiteNo].WriteVari(address.Address0, QRResults[i].ToString());
            //    }
            //}
        }
        /// <summary>
        /// PC->PLC 线扫偏移值XYR
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)
        /// 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <param name="scanOffsets">ARRAY[0..899] OF REAL</param>
        public void rScanOffset(float[] scanOffsets)
        {
            //if (scanOffsets == null)
            //    return;
            //if (scanOffsets.Length > 0)
            //{
            //    for (int i = 0; i < scanOffsets.Length; i++)
            //    {
            //        AddressClass address = getCipAdress($"rScanOffset[{i}]");
            //        PLC[address.SiteNo].WriteVari(address.Address0, scanOffsets[i].ToString());
            //    }
            //}
        }

        public string sLotID
        {
            get;
            set;
        } = "Lot_NONE";
        public string sStripID
        {
            get;
            set;
        } = "Strip_NONE";

        public int iFlyStart
        {
            get => _simFlyStart;
            set => _simFlyStart = value;
        }
        public bool bFlyReady
        {
            get;
            set;
        }
        public bool bFlyDone
        {
            get => _simFlyDone;
            set
            {
                if (_simFlyDone != value)
                {
                    _simFlyDone = value;
                    if (_simFlyDone)
                        ((Action)sim_NextFly).BeginInvoke(null, null);
                }
            }
        }

        /// <summary>
        /// PC->PLC 飞拍结果,1-Ok,2-Ng,3-空 单颗的飞拍结果
        /// </summary>
        /// <param name="flyResults">ARRAY[0..3] OF INT</param>
        public void iFlyResult(int[] flyResults)
        {
            //if (flyResults == null)
            //    return;
            //if (flyResults.Length > 0)
            //{
            //    for (int i = 0; i < flyResults.Length; i++)
            //    {
            //        AddressClass address = getCipAdress($"iFlyResult[{i}]");
            //        PLC[address.SiteNo].WriteVari(address.Address0, flyResults[i].ToString());
            //    }
            //}
        }
        /// <summary>
        /// PC->PLC 飞拍补偿(X,Y,R) 单颗的补偿结果([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共4个) ARRAY[0..11] OF REAL
        /// </summary>
        /// <param name="Offsets">ARRAY[0..11] OF REAL</param>
        public void rOffset(float[] Offsets)
        {
            //if (Offsets == null)
            //    return;
            //if (Offsets.Length > 0)
            //{
            //    for (int i = 0; i < Offsets.Length; i++)
            //    {
            //        AddressClass address = getCipAdress($"rOffset[{i}]");
            //        PLC[address.SiteNo].WriteVari(address.Address0, Offsets[i].ToString());
            //    }
            //}
        }

        /// <summary>
        /// PC->PLC 线扫配方切换结果1-Ok,2-Ng
        /// </summary>
        public int iRecipeNum
        {
            get;
            set;
        }
        /// <summary>
        /// PLC->PC 线扫使用配方名
        /// </summary>
        public string sRecipeName
        {
            get;
            set;
        } = "SIM";

        /// <summary>
        /// 载台一纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        public void SetStage1(int eIndex, PointF pt0, PointF pt1)
        {
            //if (eIndex == 0)
            //{
            //    AddressClass addressX0 = new AddressClass($"0:Gvl_Stage1.X[0]");
            //    PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
            //    AddressClass addressY0 = new AddressClass($"0:Gvl_Stage1.Y[0]");
            //    PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            //}
            //if (eIndex == 1)
            //{
            //    AddressClass addressX1 = new AddressClass($"0:Gvl_Stage1.X[1]");
            //    PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
            //    AddressClass addressY1 = new AddressClass($"0:Gvl_Stage1.Y[1]");
            //    PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            //}
        }
        /// <summary>
        /// 载台二纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        public void SetStage2(int eIndex, PointF pt0, PointF pt1)
        {
            //if (eIndex == 0)
            //{
            //    AddressClass addressX0 = new AddressClass($"0:Gvl_Stage2.X[0]");
            //    PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
            //    AddressClass addressY0 = new AddressClass($"0:Gvl_Stage2.Y[0]");
            //    PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            //}
            //if (eIndex == 1)
            //{
            //    AddressClass addressX1 = new AddressClass($"0:Gvl_Stage2.X[1]");
            //    PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
            //    AddressClass addressY1 = new AddressClass($"0:Gvl_Stage2.Y[1]");
            //    PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            //}
        }

        /// <summary>
        /// 使用那个线扫平台;=1平台一;=2平台二
        /// </summary>
        public int iScanStage
        {
            get;
            private set;
        } = 1;

        void IPlcIoFPIX3Sim.simActiveStage(int stageId1)
        {
            iScanStage = stageId1 == 2 ? 2 : 1;
        }

        void sim_StartScan()
        {
            _simScanResult = -1;

            _simRunCount++;
            sStripID = $"Strip_SIM_{_simRunCount:000}";
            sLotID = $"Lot_SIM_{_simRunCount:000}";

            //*** bFlyReady ***
            bFlyReady = true;   // 根據 Gaara 描述, 一旦啟動 bSoftwareReady, bFlyReady 就馬上 ON

            // 要求載入模擬影像
            System.Threading.Thread.Sleep(10);
            if (OnRequestSimLineScan != null)
            {
                var e = new DoWorkEventArgs(this) { Cancel = false };
                OnRequestSimLineScan(this, e);
                if (e.Cancel)
                    return;
            }

            // 模擬 10 ms 後, 觸發 Line Scan Image
            System.Threading.Thread.Sleep(10);

            _simFlyDone = false;
            _simFlyStart = 0;

            bScanStart = true;
        }
        
        void sim_StartFly()
        {
            // 模擬 1000 ms 後, iFlyStart = 1
            System.Threading.Thread.Sleep(1000);
            _simChipCount = 0;
            _simFlyStart = 1;
            bScanStart = false;
        }
        void sim_NextFly()
        {
            _simChipCount++;

            if (_simChipCount < 154)
            {
                // 模擬 1000 ms 後, 變換 iFlyStart
                System.Threading.Thread.Sleep(100);
                _simFlyDone = false;
                _simFlyStart = _simFlyStart == 1 ? 2 : 1;
            }
            else
            {
                System.Threading.Thread.Sleep(1000);
                _simFlyStart = 0;
                System.Threading.Thread.Sleep(1000);
                _simFlyDone = true;
                bScanStart = false;
            }
        }
    }
}
