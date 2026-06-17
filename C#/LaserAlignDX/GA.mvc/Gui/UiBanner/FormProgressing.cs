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

using System;
using System.Reflection;
using System.Windows.Forms;


namespace Eazy_Project_III.FormSpace
{
    public partial class FormProgressing : Form
    {
        public FormProgressing()
        {
            InitializeComponent();
            lblVersion.Text = _VERSION();
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

        public void SetTotalSteps(int total)
        {
            progressBar1.Maximum = total;
        }
        public void UpdateProgress(int current)
        {
            current = Math.Min(current, progressBar1.Maximum);
            progressBar1.Value = current;
            progressBar1.Refresh();
        }
        public void UpdateMessage(string message)
        {
            if(!string.IsNullOrEmpty(message))
            {
                lblMessage.Text = message;
                lblMessage.Visible = true;
            }
        }
    }
}