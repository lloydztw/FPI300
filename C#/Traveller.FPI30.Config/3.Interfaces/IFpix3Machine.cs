#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using VsCommon.ControlSpace.IOSpace;

namespace Traveller.FPI30.Config
{
    /// <summary>
    /// Driver-independent access to an FPIX3 machine. The host owns initialization,
    /// polling and closing; construction does not open hardware connections.
    /// </summary>
    public interface IFpix3Machine
    {
        IPlcIoFPIX3 PLCIO { get; }
        bool Initial(bool isnouseio, bool isnousemotor);
        void Tick();
        void PLCRetry();
        string PLCFps();
        void Close();
    }
}
