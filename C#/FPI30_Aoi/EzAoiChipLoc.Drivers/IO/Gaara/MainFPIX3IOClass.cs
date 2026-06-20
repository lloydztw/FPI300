
using EzComm;
using LaserAlignDX;
using System;
using System.Drawing;
using Traveller106.IO;

namespace VsCommon.ControlSpace.IOSpace
{
    /// <summary>
    /// 使用 FPIX3_IO 來實作 IPlcIoFPIX3
    /// </summary>
    public class MainFPIX3IOClass : IPlcIO, IDisposable
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

        #region FORMAT
        const string m_Format = "0.000";
        #endregion

        #region PRIVATE_KERNEL_DATA
        private FPIX3_IO _IO;
        private FPIX3_IO.BaseIO _baseIO => _IO?.GetBaseIO();
        #endregion

        public MainFPIX3IOClass()
        {
        }
        public void Init(EzTcpIpSettings settings)
        {
            _IO = new FPIX3_IO(settings);
        }
        public void Dispose()
        {
            _IO?.Dispose();
            _IO = null;
        }

        public void ResetPlc(bool resetRecipeNum = false)
        {
            var plcIO = this;

            plcIO.bScanStart = false;
            plcIO.bScanReady = false;
            plcIO.bScanDone = false;
            plcIO.bFlyDone = false;

            if (plcIO.bSoftwareReady)
            {
                plcIO.bFlyReady = true;

                if (resetRecipeNum)
                    plcIO.iRecipeNum = 0;
            }
        }

        /// <summary>
        /// 启动软件的ready
        /// </summary>
        public bool bSoftwareReady
        {
            get => _baseIO != null ? _baseIO.bSoftwareReady.Value : false;
            set => _baseIO?.bSoftwareReady?.Set(value);
        }

        public bool bSyncClock
        {
            get => _baseIO != null ? _baseIO.bSyncClock.Value : false;
            set => _baseIO?.bSyncClock?.Set(value);
        }

        /// <summary>
        /// PLC->PC 马达移动到开始位通知pc信号
        /// </summary>
        public bool bScanStart
        {
            get => _baseIO != null ? _baseIO.bScanStart.Value : false;
            set => _baseIO?.bScanStart?.Set(value);
        }

        /// <summary>
        /// PC->PLC 线扫准备就绪
        /// </summary>
        public bool bScanReady
        {
            get => _baseIO != null ? _baseIO.bScanReady.Value : false;
            set => _baseIO?.bScanReady?.Set(value);
        }

        /// <summary>
        /// PC->PLC完成信号和结果一起给
        /// </summary>
        public bool bScanDone
        {
            get => _baseIO != null ? _baseIO.bScanDone.Value : false;
            set => _baseIO?.bScanDone?.Set(value);
        }

        public bool bQRUsed
        {
            get => _baseIO != null ? _baseIO.bQRUsed.Value : false;
            //set => _baseIO?.bQRUsed?.Set(value);
        }

        public bool bQRJudgeUsed
        {
            get => _baseIO != null ? _baseIO.bQRJudgeUsed.Value : false;
            //set => _baseIO?.bQRJudgeUsed?.Set(value);
        }

        /// <summary>
        /// PLC->PC 线扫状态,1-尺寸外观,2-读码,3-空载台
        /// </summary>
        public int iScanStatus
        {
            get => _baseIO != null ? _baseIO.iScanStatus.Value : 0;
            //set => _baseIO?.iScanStatus?.Set(value);
        }

        /// <summary>
        /// 1：ok 2:ng
        /// </summary>
        public int iScanResult
        {
            get => _baseIO != null ? _baseIO.iScanResult.Value : 0;
            set => _baseIO?.iScanResult?.Set(value);
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

            //for (int i = 0; i < singleResults.Length; i++)
            //{
            //    AddressClass address = getCipAdress($"iSingleResult[{i}]");
            //    PLC[address.SiteNo].WriteVari(address.Address0, singleResults[i].ToString());
            //}

            _IO.WriteArray("iSingleResult", singleResults);
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

            _IO.WriteArray("iQRResult", QRResults);
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

            _IO?.WriteArray("rScanOffset", scanOffsets);
        }

        public string sLotID
        {
            get => _IO.GetMiscIO()?.sLotID.Value;
            //set => _IO.GetMiscIO()?.sLotID.Set(value);
        }
        public string sStripID
        {
            get => _IO.GetMiscIO()?.sStripID.Value;
            //set => _IO.GetMiscIO()?.sStripID.Set(value);
        }

        public int iFlyStart
        {
            get => _baseIO != null ? _baseIO.iFlyStart.Value : 0;
            set => _baseIO.iFlyStart?.Set(value);
        }
        public bool bFlyReady
        {
            get => _baseIO != null ? _baseIO.bFlyReady.Value : false;
            set => _baseIO.bFlyReady?.Set(value);
        }
        public bool bFlyDone
        {
            get => _baseIO != null ? _baseIO.bFlyDone.Value : false;
            set => _baseIO.bFlyDone?.Set(value);
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

            _IO.WriteArray("iFlyResult", flyResults);
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

            _IO.WriteArray("rOffset", Offsets);
        }

        /// <summary>
        /// PC->PLC 线扫配方切换结果1-Ok,2-Ng
        /// </summary>
        public int iRecipeNum
        {
            get => _baseIO != null ? _baseIO.iRecipeNum.Value : 0;
            set => _baseIO?.iRecipeNum?.Set(value);
        }
        
        /// <summary>
        /// PLC->PC 线扫使用配方名
        /// </summary>
        public string sRecipeName
        {
            get => _IO?.GetMiscIO()?.sRecipeName.Value;
            set => _IO?.GetMiscIO()?.sRecipeName.Set(value);
        }

        /// <summary>
        /// 载台一纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        public void SetStage1(int eIndex, PointF pt0, PointF pt1)
        {
            //if (eIndex == 0)
            //{
            //    //AddressClass addressX0 = new AddressClass($"0:Gvl_Stage1.X[0]");
            //    //PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
            //    //AddressClass addressY0 = new AddressClass($"0:Gvl_Stage1.Y[0]");
            //    //PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage1.X[0]", pt0.X.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage1.Y[0]", pt0.Y.ToString(m_Format));
            //}
            //if (eIndex == 1)
            //{
            //    //AddressClass addressX1 = new AddressClass($"0:Gvl_Stage1.X[1]");
            //    //PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
            //    //AddressClass addressY1 = new AddressClass($"0:Gvl_Stage1.Y[1]");
            //    //PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage1.X[1]", pt1.X.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage1.Y[1]", pt1.Y.ToString(m_Format));
            //}

            int id = eIndex == 0 ? 0 : 1;
            var pt = eIndex == 0 ? pt0 : pt1;
            _IO.WriteOne($"Gvl_Stage1.X[{id}]", pt.X.ToString(m_Format));
            _IO.WriteOne($"Gvl_Stage1.Y[{id}]", pt.Y.ToString(m_Format));
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
            //    //AddressClass addressX0 = new AddressClass($"0:Gvl_Stage2.X[0]");
            //    //PLC[addressX0.SiteNo].WriteVari(addressX0.Address0, pt0.X.ToString(m_Format));
            //    //AddressClass addressY0 = new AddressClass($"0:Gvl_Stage2.Y[0]");
            //    //PLC[addressY0.SiteNo].WriteVari(addressY0.Address0, pt0.Y.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage2.X[0]", pt0.X.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage2.Y[0]", pt0.Y.ToString(m_Format));
            //}
            //if (eIndex == 1)
            //{
            //    //AddressClass addressX1 = new AddressClass($"0:Gvl_Stage2.X[1]");
            //    //PLC[addressX1.SiteNo].WriteVari(addressX1.Address0, pt1.X.ToString(m_Format));
            //    //AddressClass addressY1 = new AddressClass($"0:Gvl_Stage2.Y[1]");
            //    //PLC[addressY1.SiteNo].WriteVari(addressY1.Address0, pt1.Y.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage2.X[1]", pt1.X.ToString(m_Format));
            //    _IO.WriteOne("Gvl_Stage2.Y[1]", pt1.Y.ToString(m_Format));
            //}

            int id = eIndex == 0 ? 0 : 1;
            var pt = eIndex == 0 ? pt0 : pt1;
            _IO.WriteOne($"Gvl_Stage2.X[{id}]", pt.X.ToString(m_Format));
            _IO.WriteOne($"Gvl_Stage2.Y[{id}]", pt.Y.ToString(m_Format));
        }

        /// <summary>
        /// 使用那个线扫平台;=1平台一;=2平台二
        /// </summary>
        public int iScanStage
        {
            get => _baseIO != null ? _baseIO.iScanStage.Value : 1;
        }

        /// <summary>
        /// 控制 吸嘴排1的真空 ON / OFF
        /// </summary>
        public bool VacuumSucker1
        {
            // GAARA_NEEDS_TO_IMPLEMENT
            get => throw new NotImplementedException("等待萬子實作");
            set => throw new NotImplementedException("等待萬子實作");
        }

        /// <summary>
        /// 取得 吸嘴排的安全高度Z (PLC 配方設定)
        /// </summary>
        public double GetSafeZ(SuckerRowEnum suckerID)
        {
            // GAARA_NEEDS_TO_IMPLEMENT
            throw new NotImplementedException("等待萬子實作");
            return 0;
        }

        /// <summary>
        /// 取得 飛拍相機的對焦高度Z (PLC 配方設定)
        /// </summary>
        public double GetFlyCamFocusZ()
        {
            // GAARA_NEEDS_TO_IMPLEMENT
            throw new NotImplementedException("等待萬子實作");
            return 0;
        }

        /// <summary>
        /// 取得 Sucker1 (或 Sucker2) 的 飛拍相機 觸發位置 (PLC 配方設定)
        /// </summary>
        public double GetFlyCamTriggerX(SuckerRowEnum suckerID = SuckerRowEnum.S1)
        {
            // GAARA_NEEDS_TO_IMPLEMENT
            throw new NotImplementedException("等待萬子實作");
            return 0;
        }

        /// <summary>
        /// 取得 飛拍相機 拍照時的 Y軸 位置 (PLC 配方設定)
        /// </summary>
        public double GetFlyCamSnapshotY()
        {
            // GAARA_NEEDS_TO_IMPLEMENT
            throw new NotImplementedException("等待萬子實作");
            return 0;
        }
    }
}
