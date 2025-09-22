using JetEazy.ControlSpace.PLCSpace;
using System.Drawing;

namespace VsCommon.ControlSpace.IOSpace
{
    public interface IPlcIoFPIX3
    {
        bool bFlyDone { get; set; }
        bool bFlyReady { get; set; }
        bool bQRJudgeUsed { get; }
        bool bQRUsed { get; }
        bool bScanDone { get; set; }
        bool bScanReady { get; set; }
        bool bScanStart { get; }
        bool bSoftwareReady { get; set; }
        bool bSyncClock { get; set; }
        int iFlyStart { get; }
        int iRecipeNum { get; set; }
        int iScanResult { get; set; }

        /// <summary>
        /// 目前 PLC 使用那个线扫平台
        /// 1: 平台一
        /// 2: 平台二
        /// </summary>
        int iScanStage { get; }

        int iScanStatus { get; }
        string sLotID { get; }
        string sRecipeName { get; set; }
        string sStripID { get; }



        void Initial(string path, VsCommPLC[] plc);
        void iQRResult(int[] QRResults);
        void iSingleResult(int[] singleResults);
        void LoadData();

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
        
        void rScanOffset(float[] scanOffsets);
        void SaveData();
        void SetStage1(int eIndex, PointF pt0, PointF pt1);
        void SetStage2(int eIndex, PointF pt0, PointF pt1);

        /// <summary>
        /// 模擬目前 載台1 或 載台2
        /// </summary>
        /// <param name="stageId1">1 or 2</param>
        void simActiveStage(int stageId1);
    }
}