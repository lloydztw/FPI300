#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-06 重新設計校正架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.AoiModel;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public interface IvFlyCamViewUI
    {
        Control Window { get; }
        void Update(FlyMetaData flyMetaData, FlyLotData lotData);
    }
}
