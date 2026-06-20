#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-23 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    public partial class FormMainS3 : Form, IvGlueDemoView
    {
        public FormMainS3()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Form IvGlueDemoView.frmMain => this;
        Control IvGlueDemoView.lblInfo => lblInfo;
        Control IvGlueDemoView.lblEMG => lblEMG;
        NumericUpDown IvGlueDemoView.numGlueTime => numericUpDown1;
        NumericUpDown IvGlueDemoView.numUVTime => numericUpDown2;
        Button IvGlueDemoView.btnStartTest => button1;
        Button IvGlueDemoView.btnStopTest => button2;
        Button IvGlueDemoView.btnWrite => button3;
        #endregion
    }
}
