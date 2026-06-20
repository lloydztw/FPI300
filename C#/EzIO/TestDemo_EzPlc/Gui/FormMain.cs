#region AUTHOR
/*
 * EzIO Demo
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-11 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;

namespace TestDemo_EzPLC.Gui
{
    public partial class FormMain : Form, IvPressoDemoView
    {
        public FormMain()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Form IvPressoDemoView.frmMain => this;
        Control IvPressoDemoView.lblInfo => lblInfo;
        Control IvPressoDemoView.lblEMG => lblEMG;
        Control IvPressoDemoView.lblCurrentForce => lblForce;
        NumericUpDown IvPressoDemoView.numProtectForce => numericUpDown1;
        Button IvPressoDemoView.btnWriteProtectForce => button1;
        Button IvPressoDemoView.btnStartTest => button2;
        Button IvPressoDemoView.btnStopTest => button3;
        #endregion
    }
}
