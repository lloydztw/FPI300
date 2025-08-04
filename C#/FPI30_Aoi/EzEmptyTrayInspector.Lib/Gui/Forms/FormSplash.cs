#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-08-23 初稿 (by LeTian Chang)
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


namespace EzAoiEmptyTrayInspector.Gui
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
        /// 版本號
        /// </summary>
        static string _VERSION(bool useOwnerAppVersion = false)
        {
            if (useOwnerAppVersion)
            {
                return Application.ProductVersion.ToString();
            }
            else
            {
                // 取得正在執行的組件
                Assembly assembly = Assembly.GetExecutingAssembly();
                // 取得組件的版本
                Version version = assembly.GetName().Version;
                // 將版本號轉換為字串
                return version.ToString();
            }
        }
    }
}