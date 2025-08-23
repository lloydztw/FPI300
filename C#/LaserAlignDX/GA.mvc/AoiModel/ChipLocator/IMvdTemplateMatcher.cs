#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-13 開始重整優化 Gaara 原來的 MvdFindClass (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using AUVision;
using JetEazy.Match;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace LaserAlignDX.RunSpace.Supports
{
    public interface IMvdTemplateMatcher : IDisposable
    {
        #region MVD_設定參數
        int xMaxOverlap { get; set; }
        float xMvdAngle { get; set; }
        PointF xMvdFixed { get; set; }
        int xMvdMaxOcc { get; set; }
        float xMvdTolerance { get; set; }
        #endregion

        /// <summary>
        /// 樣板長寬大小 (必須於調用 Train 之後, 才有有效值!)
        /// </summary>
        Size TemplateSize { get; }

        /// <summary>
        /// 結果 (必須於調用 Train 之後, 才有有效值!)
        /// </summary>
        List<xFindResult> xResults { get; }

        /// <summary>
        /// 訓練
        /// </summary>
        bool Train(Bitmap bmpTemplate);

        /// <summary>
        /// 執行匹配 
        /// </summary>
        bool RunMatch(Bitmap bmpScene);

        /// <summary>
        /// 取得定位後 Chip 上面 PAD 的資訊
        /// </summary>
        EzBlocsGrid GetResultPadsGrid();
    }
}
