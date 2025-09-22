#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-22 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AUVision;
using System.Collections.Generic;
using System.Drawing;
using VisionDesigner.BlobFind;


namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// 用來讓 MVD 畫圖的額外資料
    /// </summary>
    public class FlyMetaData
    {
        public string AlgorithmName;
        public Bitmap bmpFly;
        public RectangleF roiRect;
        public RectangleF xTemplateRect;
        public xFindResult xResult;
        public PointF xCentroid;
        public List<CBlobInfo> xBlobs;
    };
}
