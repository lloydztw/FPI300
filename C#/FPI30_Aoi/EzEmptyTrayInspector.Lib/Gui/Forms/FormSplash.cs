#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 ªì½Z (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using System.Windows.Forms;


namespace EzEmptyTrayInspector.Gui
{
    public partial class FormSplash : Form, IvSplashView
    {
        public FormSplash()
        {
            InitializeComponent();
            lblVersion.Text = Application.ProductVersion.ToString();
        }

        //Form IView.frmOwner => this;
        Control IView.Window => this;
        void IvSplashView.TraceProgress(string message)
        {
            this.lblMessage.Text = message;
            this.lblMessage.Visible = message != null;
            this.lblMessage.Refresh();
        }
    }
}