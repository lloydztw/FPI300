using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Traveller106.FormSpace
{
    public partial class frmWaring : Form
    {
        private static frmWaring m_Instance = null;
        public static frmWaring Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new frmWaring();
                return m_Instance;
            }
        }
        public void WarningStr(string Str)
        {
            label1.Text = Str;
        }
        public frmWaring()
        {
            InitializeComponent();
            label1.Text = "";
            this.Text = "即时报警信息显示窗口";
        }
    }
}
