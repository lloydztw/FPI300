using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaserAlignDX.FormSpace
{
    public partial class frmSelectScanInspectMode : Form
    {
        #region PRIVATE_GUI_LINKS
        Button btnOK => button1;
        Button btnCancel => button2;
        #endregion

        public int SelectScanMode
        {
            get
            {
                int iSelIndex = 0;
                if (radioButton1.Checked)
                {
                    iSelIndex = 0;
                }
                else if (radioButton2.Checked)
                {
                    iSelIndex = 1;
                }
                else if (radioButton3.Checked)
                {
                    iSelIndex = 2;
                }

                return iSelIndex;
            }
        }

        public frmSelectScanInspectMode()
        {
            InitializeComponent();
            this.Load += FrmSelectScanInspectMode_Load;
            this.TopMost = true;
        }

        #region EVENT_HANDLERS

        private void FrmSelectScanInspectMode_Load(object sender, EventArgs e)
        {
            this.Text = "选择测试模式";

            //btnOK = button1;
            //btnCancel = button2;
            btnOK.Click += BtnOK_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        #endregion
    }
}
