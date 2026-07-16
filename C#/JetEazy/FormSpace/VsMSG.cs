using JetEazy.FormSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace JetEazy.BasicSpace
{
    public class VsMSG
    {
        #region SINGLETON
        private static readonly VsMSG _instance = new VsMSG();
        VsMessageBox _messageBox = null;
        #endregion

        public static VsMSG Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 询问视窗
        /// </summary>
        /// <param name="msg">提示信息</param>
        /// <returns>返回OK  和 Cancel </returns>
        public DialogResult Question(string msg)
        {
            _messageBox = new VsMessageBox(msg, false);
            return _messageBox.ShowDialog();
        }
        public void Warning(string msg, bool iswarning = true)
        {
            _messageBox = new VsMessageBox(msg, iswarning);
            _messageBox.ShowDialog();
        }
        public void Tishi(string msg)
        {
            _messageBox = new VsMessageBox(msg, "1");
            _messageBox.ShowDialog();
        }
    }
}
