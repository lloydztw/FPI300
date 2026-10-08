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

using JetEazy.ControlSpace.PLCSpace;

namespace VsCommon.ControlSpace.IOSpace
{
    // Driver setup belongs to Machine, not the operation contract consumed by Models/UI.
    internal interface IInitializableFpix3Io : IPlcIoFPIX3
    {
        void Initial(string path, VsCommPLC[] plc);
    }
}
