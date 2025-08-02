using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traveller106;
using VsCommon.ControlSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace LaserAlignDX.FormSpace
{
    public partial class frmLightControl : Form
    {

        protected MachineCollectionClass MACHINECollection
        {
            get
            {
                return Universal.MACHINECollection;
            }
        }
        protected MainFPIX3MachineClass MACHINE
        {
            get { return (MainFPIX3MachineClass)Universal.MACHINECollection.MACHINE; }
        }

        public frmLightControl()
        {
            InitializeComponent();

            this.Text = "光源控制界面";
        }

        private void btn0Open_Click(object sender, EventArgs e)
        {
            LightValue((int)num0.Value, 1);
            LightOnOff(true);
        }

        private void btn0Close_Click(object sender, EventArgs e)
        {
            //LightValue(0, 0);
            LightOnOff(false);
        }

        private void btn1Open_Click(object sender, EventArgs e)
        {
            LightValue((int)num1.Value, 2);
            LightOnOff(true);
        }

        private void btn1Close_Click(object sender, EventArgs e)
        {
            //LightValue(0, 0);
            LightOnOff(false);
        }

        protected void LightValue(int eVal, int eChNum = 1)
        {
            foreach (var machine in MACHINE.LightCollection)
            {
                machine.ChNum = eChNum;
                machine.CstLightValue = eVal;
            }
        }
        protected void LightOnOff(bool eOn)
        {
            foreach (var machine in MACHINE.LightCollection)
            {
                machine.LightONOFF(eOn);
            }
        }
    }
}
