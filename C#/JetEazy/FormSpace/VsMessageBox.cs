#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-09-17 重整 (by LeTian Chang)
 *
 */
#endregion

using JetEazy.Lang;
using JetEazy.Lang.Gui;
using System.Windows.Forms;

namespace JetEazy.FormSpace
{
    /// <summary>
    /// 橋接舊的用法到統合的 QMessageBox
    /// </summary>
    public static class VsMessageBox
    {
        public static DialogResult Info(string msg)
        {
            return QMessageBox.Info(msg, translate: false);
        }
        
        public static DialogResult Warning(string msg)
        {
            return QMessageBox.Warning(msg, translate: false);
        }
        
        public static DialogResult Question(string msg)
        {
            return QMessageBox.Question(msg, translate: false);
        }

        public static Form InfoForm(string msg)
        {
            Form frm = new FormMessageBox(msg, QMSG.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //frm.Show();
            //frm.Refresh();
            return frm;
        }

        public static Form WarningForm(string msg)
        {
            Form frm = new FormMessageBox(msg, QMSG.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //frm.Show();
            //frm.Refresh();
            return frm;
        }
    }
}
