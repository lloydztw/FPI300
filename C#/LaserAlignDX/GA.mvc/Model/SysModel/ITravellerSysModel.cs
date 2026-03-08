#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-28 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using LaserAlignDX.AoiModel;
using LaserAlignDX.Model.Coords;
using NeedleX.ProcessSpace;
using System;
using System.Drawing;

namespace LaserAlignDX.Mvc.Model
{
    public interface ITravelerModel : IDisposable
    {
        event EventHandler<ProcessEventArgs> OnError;

        /// <summary>
        /// 主要的 AoiModel
        /// 負責 ChipLocate, QrCode, EmptyTrayInspect
        /// </summary>
        IProcessRunFPI AoiModel { get; }

        /// <summary>
        /// 校正用的像測
        /// </summary>
        ICalibAoiModel CalibAoiModel { get; }
        
        /// <summary>
        /// 空盤檢測 AOI
        /// </summary>
        IxEmptyTrayInspector EmptyTrayAoiModel { get; }

        /// <summary>
        /// 線性遷移後 的 座標轉換系統
        /// </summary>
        ITravellerTransforms TransformsModel { get; }

        QMicroChipTransform GetMicroTransform(CarrierEnum carrierID);
        
        /// <summary>
        /// 跑線巨圖管理者
        /// </summary>
        GaBigImageHolder LineScanImageHolder { get; }
        
        /// <summary>
        /// 保留
        /// </summary>
        object GetCurrentRecipe();

        /// <summary>
        /// 當前載台
        /// </summary>
        CarrierEnum ActiveCarrierID { get; set; }

        /// <summary>
        /// 套用 Gaara 參數
        /// (必須先指定 ActiveCarrierID)
        /// </summary>
        void ApplyRecipe(params object[] args);

        /// <summary>
        /// 個別參數 自動抓取陣列 並 進行 座標系統 線性遷移
        /// <br/> 用於 參數編輯模式
        /// <br/> 必須曾經執行過 BuildTransformAndRegionCells
        /// </summary>
        bool AutoBuildRegionCells(CarrierEnum carrierID, Bitmap fullfovBmp, bool migrate, out MatchResult result);

        /// <summary>
        /// 單筆寫入 座標數據 至 PLC
        /// </summary>
        bool WriteCoordsToPlc(CarrierEnum carrierID, SuckerRowEnum suckerRowID, out PointF camCoord, out PointF suckerCoord, out string msg);

        /// <summary>
        /// 一鍵寫入 所有座標數據 至 PLC
        /// </summary>
        bool WriteAllCoordsToPlc(out string msg);
    }
}
