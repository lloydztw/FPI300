#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-25 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    public partial class FormMainT3 : Form, IvTravellerDemoView
    {
        public FormMainT3()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Form IvTravellerDemoView.frmMain => this;
        Control IvTravellerDemoView.lblInfo => lblInfo;
        Control IvTravellerDemoView.lblSync => lblSync;
        NumericUpDown IvTravellerDemoView.numFPOS => numericUpDown1;
        NumericUpDown IvTravellerDemoView.numRPOS => numericUpDown2;
        Button IvTravellerDemoView.btnStartTest => button1;
        Button IvTravellerDemoView.btnStopTest => button2;
        Button IvTravellerDemoView.btnWrite => button3;
        #endregion
    }
}
