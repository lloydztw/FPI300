using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.FormSpace;
using JetEazy.ControlSpace;

using System.Collections;
using System.Media;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace Eazy_Project_III.OPSpace.ResultSpace
{
    public class JzBaseResultClass : GeoResultClass
    {
        public string BARCODE = "";
        public string VER = "";

        YzBaseMachineClass MACHINE;

        JzToolsClass JzTools = new JzToolsClass();
        SoundPlayer PlayerPass = new SoundPlayer();
        SoundPlayer PlayerFail = new SoundPlayer();

        public JzBaseResultClass(Result_EA resultea,VersionEnum version,OptionEnum option,MachineCollectionClass machinecollection)
        {
            myResultEA = resultea;
            VERSION = version;
            OPTION = option;

            DUP = new DupClass();

            MACHINE = (YzBaseMachineClass)machinecollection.MACHINE;
            
            MainProcess = new ProcessClass();
        }


        public override void GetStart(CCDCollectionClass ccdcollection, TestMethodEnum testmethod, bool isnouseccd)
        {
            if (MainProcess.IsOn)
            {
                MainProcess.Stop();
                OnTrigger(ResultStatusEnum.FORECEEND);

                return;
            }

            OnTrigger(ResultStatusEnum.CALSTART);

            //AlbumWork = albumwork;
            //if (AlbumWork != null && AlbumWork.CPD != null)
            //    AlbumWork.CPD.bmpOCRCheckErr = null;
            CCDCollection = ccdcollection;

            TestMethod = testmethod;
            IsNoUseCCD = isnouseccd;

            ResetData(-1);

            MainProcess.Start();

        }
        public override void Tick()
        {
            //if(!IsNoUseCCD && MACHINE.PLCIO.IsUPSError)
            //{
            //    if (MainProcess.IsOn)
            //    {
            //        MainProcess.Stop();
            //        IsStopNormalTick = false;
            //        MessageBox.Show("UPS Error 请检查UPS 或相关接线！");
            //    }
            //}
            
            MainProcessTick();
        }
        public override void GenReport()
        {

        }
        public override void SetDelayTime()
        {
            //DelayTime[0] = INI.DELAYTIME;
        }

        JzTimes TestTimer = new JzTimes();
        int[] Testms = new int[100];
        DateTime m_input_time = DateTime.Now;

        protected override void MainProcessTick()
        {
            switch(OPTION)
            {
                case OptionEnum.DISPENSING:

                    break;
            }


        }
        public override void FillProcessImage()
        {
           
        }
        public void FillProcessImage(string opstr)
        {
            //int i = 0;

            //EnvClass env = AlbumWork.ENVList[EnvIndex];

            //foreach (PageClass page in env.PageList)
            //{
            //    if((opstr + ",").IndexOf(page.No.ToString(PageClass.ORGPAGENOSTRING) + ",") >-1 )
            //        page.SetbmpRUN(PageOPTypeEnum.P00, CCDCollection.GetBMP(page.CamIndex, false));

            //    i++;
            //}
        }
        public void FillProcessImage(string opstr,Bitmap bmp)
        {
            //int i = 0;

            //EnvClass env = AlbumWork.ENVList[EnvIndex];

            //foreach (PageClass page in env.PageList)
            //{
            //    if ((opstr + ",").IndexOf(page.No.ToString(PageClass.ORGPAGENOSTRING) + ",") > -1)
            //        page.SetbmpRUN(PageOPTypeEnum.P00, bmp);

            //    i++;
            //}
        }
        public override void ResetData(int operationindex)
        {
            //if (operationindex == -1)
            //{
            //    AlbumWork.ResetRunStatus();

            //    EnvIndex = 0;
            //    AlbumWork.SetEnvRunIndex(EnvIndex);

            //    RunStatusCollection.Clear();

            //    SetDelayTime();
            //    SetSaveDirectory(Universal.DEBUGRAWPATH);

            //}
            //else
            //{
            //    EnvIndex = operationindex;
            //    AlbumWork.SetEnvRunIndex(EnvIndex);
            //}
        }
       
    }
}
