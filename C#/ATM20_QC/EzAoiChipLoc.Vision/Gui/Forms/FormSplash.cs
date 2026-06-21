#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 ��Z (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework;
using System;
using System.Reflection;
using System.Windows.Forms;


namespace EzAoiChipLocQC.Gui
{
    public partial class FormSplash : Form, IvSplashView
    {
        public FormSplash()
        {
            InitializeComponent();
            lblVersion.Text = _VERSION();
        }

        Control IView.Window => this;
        void IvSplashView.TraceProgress(string message)
        {
            this.lblMessage.Text = message;
            this.lblMessage.Visible = message != null;
            this.lblMessage.Refresh();
        }

        /// <summary>
        /// ������
        /// </summary>
        static string _VERSION(bool useOwnerAppVersion = false)
        {
            if (useOwnerAppVersion)
            {
                return Application.ProductVersion.ToString();
            }
            else
            {
                // ���o���b���檺�ե�
                Assembly assembly = Assembly.GetExecutingAssembly();
                // ���o�ե󪺪���
                Version version = assembly.GetName().Version;
                // �N�������ഫ���r��
                return version.ToString();
            }
        }
    }
}