using JetEazy.ControlSpace.PLCSpace;
using System;
using System.ComponentModel;
using System.Drawing;

namespace VsCommon.ControlSpace.IOSpace
{
    public interface IPlcIoFPIX3
    {
        void Initial(string path, VsCommPLC[] plc);
        void LoadData();
        void SaveData();

        /// <summary>
        /// 启动软件的ready
        /// </summary>
        bool bSoftwareReady { get; set; }
        bool bSyncClock { get; set; }

        bool bQRJudgeUsed { get; }
        bool bQRUsed { get; }


        /// <summary>
        /// PLC->PC 马达移动到开始位通知pc信号
        /// </summary>
        bool bScanStart { get; set; }
        /// <summary>
        /// PC->PLC 线扫准备就绪
        /// </summary>
        bool bScanReady { get; set; }
        /// <summary>
        /// PC->PLC完成信号和结果一起给
        /// </summary>
        bool bScanDone { get; set; }
        /// <summary>
        /// PLC->PC 线扫状态
        /// 1: 尺寸外观
        /// 2: 读码,
        /// 3: 空载台
        /// </summary>
        int iScanStatus { get; }
        /// <summary>
        /// 1: ok 
        /// 2: ng
        /// </summary>
        int iScanResult { get; set; }
        /// <summary>
        /// PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG
        /// 单颗的线扫结果(预留300个)
        /// PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <param name="singleResults">ARRAY[0..299] OF INT</param>
        void iSingleResult(int[] singleResults);
        /// <summary>
        /// PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到
        /// 单颗产品的读码比对结果(预留300个)
        /// 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <param name="QRResults">ARRAY[0..299] OF INT</param>
        void iQRResult(int[] QRResults);
        /// <summary>
        /// PC->PLC 线扫偏移值XYR
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)
        /// 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <param name="scanOffsets">ARRAY[0..899] OF REAL</param>
        void rScanOffset(float[] scanOffsets);

        string sStripID { get; }
        string sLotID { get; }

        int iFlyStart { get; set; }
        bool bFlyDone { get; set; }
        bool bFlyReady { get; set; }

        /// <summary>
        /// PC->PLC 飞拍结果,1-Ok,2-Ng,3-空 单颗的飞拍结果
        /// </summary>
        /// <param name="flyResults">ARRAY[0..3] OF INT</param>
        void iFlyResult(int[] flyResults);
        /// <summary>
        /// PC->PLC 飞拍补偿(X,Y,R) 单颗的补偿结果([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共4个) 
        /// </summary>
        /// <param name="Offsets">ARRAY[0..11] OF REAL</param>
        void rOffset(float[] Offsets);

        /// <summary>
        /// PC->PLC 线扫配方切换结果1-Ok,2-Ng
        /// </summary>
        int iRecipeNum { get; set; }
        /// <summary>
        /// PLC->PC 线扫使用配方名
        /// </summary>
        string sRecipeName { get; set; }

        /// <summary>
        /// 载台一纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        void SetStage1(int eIndex, PointF pt0, PointF pt1);
        /// <summary>
        /// 载台二纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        void SetStage2(int eIndex, PointF pt0, PointF pt1);
        /// <summary>
        /// 目前 PLC 使用那个线扫平台
        /// 1: 平台一
        /// 2: 平台二
        /// </summary>
        int iScanStage { get; }
    }


    public interface IPlcIoFPIX3Sim : IPlcIoFPIX3
    {
        event EventHandler<DoWorkEventArgs> OnRequestSimLineScan;
        event EventHandler<DoWorkEventArgs> OnRequestSimFlyCam;

        /// <summary>
        /// 模擬目前 載台1 或 載台2
        /// </summary>
        void simActiveStage(int stageId1);
    }
}