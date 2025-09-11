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
        int iScanStage { get; }
        int iScanStatus { get; }
        string sLotID { get; }
        string sRecipeName { get; set; }
        string sStripID { get; }

        void iFlyResult(int[] flyResults);
        void Initial(string path, VsCommPLC[] plc);
        void iQRResult(int[] QRResults);
        void iSingleResult(int[] singleResults);
        void LoadData();
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