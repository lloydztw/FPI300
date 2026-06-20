#region AUTHOR
/*
 * Traveller106.IO
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.ComponentModel;

namespace Traveller106.IO.V0
{
	/// <summary>
	/// 需要 高速頻繁讀取的 IO點位
	/// </summary>
    public enum BasePointEnum : int
    {
		[Description("心跳")]
		bSyncClock,

        [Description("參數編號")]
        iRecipeNum,

        [Description("軟件準備")]
        bSoftwareReady,

        [Description("載台號")]
        iScanStage,

        //[Description("bScanStart")]
        bScanStart,
        //[Description("bScanReady")]
        bScanReady,
        //[Description("bScanDone")]
        bScanDone,
        //[Description("iScanStatus")]
        iScanStatus,
        //[Description("iScanResult")]
        iScanResult,
        //[Description("bQRUsed")]
        bQRUsed,
        //[Description("bQRJudgeUsed")]
        bQRJudgeUsed,
        //[Description("iFlyStart")]
        iFlyStart,
        //[Description("bFlyReady")]
        bFlyReady,
        //[Description("bFlyDone")]
        bFlyDone,
    }

	/// <summary>
	/// 比較耗時的 字串型 點位
	/// </summary>
    public enum MiscPointEnum : int
    {
        sRecipeName,
        sLotID,
        sStripID,
    }

	/// <summary>
	/// 軸控點位
	/// </summary>
    public enum AxisPointEnum : int
    {
		bHomed,
		bAbsMoveDone,
		bLimitP,
		bLimitN,
		bCalibrationCam,
		bEnable,
		bError,
		bStart,
		bHomeStart,
		bJogFW,
		bJogNW,
		rSped,
		rFPOS,
		rRPOS,
    }
}
