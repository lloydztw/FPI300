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

using System;
using System.ComponentModel;

namespace EzAoiChipLocQC.Drivers.IO
{
    public interface IPlcAtm20Sim : IPlcAtm20
    {
        event EventHandler<DoWorkEventArgs> OnRequestSimLineScan;
        event EventHandler<DoWorkEventArgs> OnRequestSimFlyCam;

        /// <summary>
        /// 模擬目前 載台1 或 載台2
        /// </summary>
        void simActiveStage(int stageId1);
        void simStripID(string stripID);
        void simLotID(string lotID);
    }
}