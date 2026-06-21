#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-21 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;

namespace EzAoiChipLocQC.Drivers.IO
{
	/// <summary>
	/// 需要 高速頻繁讀取的 IO點位
	/// </summary>
    public enum BasePointEnum : int
    {
        [EzPointEnumAttribute("心跳", "Gvl_PhotoPC", autoScan: true)]
        bSyncClock,

        [EzPointEnumAttribute("軟件準備", "Gvl_PhotoPC", autoScan: true)]
        bSoftwareReady,

        [EzPointEnumAttribute("參數確認碼", "Gvl_PhotoPC", autoScan: true)]
        iRecipeNum,

        [EzPointEnumAttribute("載台號", "Gvl_PhotoPC", autoScan: true)]
        iScanStage,

        [EzPointEnumAttribute("線掃開始", "Gvl_PhotoPC", autoScan: true)]
        bScanStart,

        [EzPointEnumAttribute("線掃Ready", "Gvl_PhotoPC", autoScan: true)]
        bScanReady,

        [EzPointEnumAttribute("線掃完成", "Gvl_PhotoPC", autoScan: true)]
        bScanDone,

        [EzPointEnumAttribute("線掃狀態", "Gvl_PhotoPC", autoScan: true)]
        iScanStatus,

        [EzPointEnumAttribute("線掃結果", "Gvl_PhotoPC", autoScan: true)]
        iScanResult,

        [EzPointEnumAttribute("QR啟用", "Gvl_PhotoPC", autoScan: true)]
        bQRUsed,

        [EzPointEnumAttribute("QR判定啟用", "Gvl_PhotoPC", autoScan: true)]
        bQRJudgeUsed,

        [EzPointEnumAttribute("飛拍開始", "Gvl_PhotoPC", autoScan: true)]
        iFlyStart,

        [EzPointEnumAttribute("飛拍Ready", "Gvl_PhotoPC", autoScan: true)]
        bFlyReady,

        [EzPointEnumAttribute("飛拍完成", "Gvl_PhotoPC", autoScan: true)]
        bFlyDone,
    }


    /// <summary>
    /// 比較耗時的 字串型 點位
    /// </summary>
    public enum MiscPointEnum : int
    {
        [EzPointEnumAttribute("參數名稱", "Gvl_PhotoPC", autoScan: false)]
        sRecipeName,

        [EzPointEnumAttribute("Lot ID", "Gvl_PhotoPC", autoScan: false)]
        sLotID,

        [EzPointEnumAttribute("Strip ID", "Gvl_PhotoPC", autoScan: false)]
        sStripID,
    }


    /// <summary>
    /// 軸控點位
    /// </summary>
    public enum AxisPointEnum : int
    {
        [EzPointEnumAttribute("軸{0}.Home", "Axis[{0}]", autoScan: false)]
        bHomed,
        
        [EzPointEnumAttribute("軸{0}.Home", "Axis[{0}]", autoScan: false)]
        bAbsMoveDone,
        
        [EzPointEnumAttribute("軸{0}.正極限", "Axis[{0}]", autoScan: false)]
        bLimitP,
        
        [EzPointEnumAttribute("軸{0}.負極限", "Axis[{0}]", autoScan: false)]
        bLimitN,
        
        [EzPointEnumAttribute("軸{0}.原點", "Axis[{0}]", autoScan: false)]
        bCalibrationCam,
        
        [EzPointEnumAttribute("軸{0}.啟用", "Axis[{0}]", autoScan: false)]
        bEnable,
        
        [EzPointEnumAttribute("軸{0}.異常", "Axis[{0}]", autoScan: false)]
        bError,
        
        [EzPointEnumAttribute("軸{0}.開始移動", "Axis[{0}]", autoScan: false)]
        bStart,
        
        [EzPointEnumAttribute("軸{0}.開始回Home", "Axis[{0}]", autoScan: false)]
        bHomeStart,
        
        [EzPointEnumAttribute("軸{0}.JOG(前進)", "Axis[{0}]", autoScan: false)]
        bJogFW,
        
        [EzPointEnumAttribute("軸{0}.JOG(後退)", "Axis[{0}]", autoScan: false)]
        bJogNW,
        
        [EzPointEnumAttribute("軸{0}.速度", "Axis[{0}]", autoScan: false)]
        rSped,
        
        [EzPointEnumAttribute("軸{0}.位置(前進)", "Axis[{0}]", autoScan: false)]
        rFPOS,
        
        [EzPointEnumAttribute("軸{0}.位置(後退)", "Axis[{0}]", autoScan: false)]
        rRPOS,
    }
}
