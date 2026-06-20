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
    /// 集合型 位址
    /// </summary>
    public enum CompositePointEnum : int
    {
        [EzPointEnumAttribute("載台{0}吸嘴{1} 糾偏位置 X", "Gvl_Stage{0}.X[{1}]", autoScan: false)]
        StageX,
        [EzPointEnumAttribute("載台{0}吸嘴{1} 糾偏位置 Y", "Gvl_Stage{0}.Y[{1}]", autoScan: false)]
        StageY,

        [EzPointEnumAttribute("線掃檢測結果", "Gvl_PhotoPC.iSingleResult[{0}]", autoScan: false)]
        iSingleResult,
        [EzPointEnumAttribute("線掃QR結果", "Gvl_PhotoPC.iQRResult[{0}]", autoScan: false)]
        iQRResult,
        [EzPointEnumAttribute("線掃定位結果(補償量)", "Gvl_PhotoPC.rScanOffset[{0}]", autoScan: false)]
        rScanOffset,

        [EzPointEnumAttribute("飛拍結果", "Gvl_PhotoPC.iFlyResult[{0}]", autoScan: false)]
        iFlyResult,
        [EzPointEnumAttribute("飛拍定位結果(補償量)", "Gvl_PhotoPC.rOffset[{0}]", autoScan: false)]
        rOffset,
    }
}
