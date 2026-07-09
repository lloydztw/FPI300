#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-22 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Windows.Forms;

namespace JetEazy.Lang
{
    /// <summary>
    /// 多語系 MessageBox
    /// </summary>
    public class QMessageBox
    {
        public static DialogResult Show(
            string msg,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            title = title == null ? QMSG.TITLE : QMSG.SmartTranslate("prompts", title);
            msg = QMSG.SmartTranslate("prompts", msg);
            return MessageBox.Show(msg, title, btn, icon);
        }

        public static DialogResult Show(
            Enum prompt,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information,
            params string[] args)
        {
            title = title == null ? QMSG.TITLE : QMSG.SmartTranslate("prompts", title);
            string msg = QMSG.Text(prompt, args);
            return MessageBox.Show(msg, title, btn, icon);
        }

        public static DialogResult Info(
            Enum prompt,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.OK,
            params string[] args)
        {
            return Show(prompt, title, btn, MessageBoxIcon.Information, args);
        }

        public static DialogResult Question(
            Enum prompt,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.YesNo,
            params string[] args)
        {
            return Show(prompt, title, btn, MessageBoxIcon.Question, args);
        }

        public static DialogResult Warning(
            Enum prompt,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.OK,
            params string[] args)
        {
            return Show(prompt, title, btn, MessageBoxIcon.Exclamation, args);
        }

        public static DialogResult Error(
            Enum prompt,
            string title = null,
            MessageBoxButtons btn = MessageBoxButtons.OK,
            params string[] args)
        {
            return Show(prompt, title, btn, MessageBoxIcon.Error, args);
        }
    }
}
