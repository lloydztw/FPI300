#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System.Windows.Forms;
using JetEazy.BasicSpace;
using LaserAlignDX.LT.UnitTest;
using Traveller106;
using VsCommon.ControlSpace.MachineSpace;

namespace UnitTest_FP130
{
    public class Test_OmronPlc
    {
        public void Run()
        {
            CommonLogClass.Instance.LogPath = Universal.LOG_TXT_PATH;
            INI.Instance.Initial();
            bool bOK = Universal.Initial(0);
            if (!bOK)
            {
                return;
            }

            var machine = (MainFPIX3MachineClass)Universal.Machine;
            var frm = new FormOmronPlcViewer();
            frm.Initial(machine);
            Application.Run(frm);

            Universal.Dispose();
        }
    }
}
