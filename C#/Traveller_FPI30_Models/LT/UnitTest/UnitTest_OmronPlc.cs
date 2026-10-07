using JetEazy.BasicSpace;
using LaserAlignDX;
using LaserAlignDX.LT.UnitTest;
using System;
using System.Windows.Forms;
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
                Environment.Exit(0);
            }

            var machine = (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE;
            var frm = new FormOmronPlcViewer();
            frm.Initial(machine);
            Application.Run(frm);

            Universal.Dispose();
        }
    }
}
