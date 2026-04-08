using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.Mvc.Gui
{
    public partial class GvRecipeBtnsPanel : UserControl
    {
        #region PRIVATE_DATA
        Color _colorActive;
        Color _colorPassive;
        Button[] _majorButtons;
        #endregion

        public GvRecipeBtnsPanel()
        {
            InitializeComponent();
            initDataGridView();

            SizeChanged += (s, e) => autoLayout();

            _majorButtons = new[]
            {
                btnSwitchToEmptyTray,
                btnSwitchToChipTemplate,
                btnOpenFlyCamRcpWindow,
                btnOpenLightCtrlWindow,
            };

            _colorActive = _majorButtons[0].BackColor;
            _colorPassive = _majorButtons[1].BackColor;

            foreach (var btnMajor in _majorButtons)
                btnMajor.Click += (s, e) => switchTo(s as Button);

            var rdoCarriers = new[] { rdoCarrier1, rdoCarrier2 };
            foreach (var rdoCarrier in rdoCarriers)
                rdoCarrier.CheckedChanged += (s, e) => updateRdoColor(s as RadioButton);
        }

        void initDataGridView()
        {
            DataGridView dgv = gvCalibPointsDataGridView1.DataGridView;

            while (dgv.Rows.Count > 2)
                dgv.Rows.RemoveAt(dgv.Rows.Count - 1);

            dgv.Rows[0].Cells[0].Value = "吸嘴1";
            dgv.Rows[1].Cells[0].Value = "吸嘴2";

            int col = dgv.Columns.Count - 2;
            foreach (string coordName in new[] { "World X", "World Y" })
            {
                //dgv.Columns[col].HeaderText = coordName;
                dgv.Columns[col].ReadOnly = true;
                col++;
            }

            dgv.Columns[0].DefaultCellStyle.SelectionBackColor = dgv.DefaultCellStyle.BackColor;
            dgv.BackgroundColor = dgv.Parent.BackColor;

            //// 暫時隱藏
            //dgv.Visible = false;
            //btnWriteCoordsToPlc.Visible = false;
        }
        void autoLayout()
        {
            var ccSize = ClientSize;
            panel2.Height = ccSize.Height - tableLayoutPanel1.Height;
        }
        void updateRdoColor(RadioButton rdo)
        {
            rdo.ForeColor = rdo.Checked ? Color.Black : Color.DimGray;
        }
        void switchTo(Button btnTarget)
        {
            if (btnTarget == null)
                return;

            foreach (var btn in _majorButtons)
            {
                btn.BackColor = (btn == btnTarget) ? _colorActive : _colorPassive;
            }

            if (btnTarget == btnSwitchToEmptyTray)
            {
                btnOpenEmptyTrayWindow.Visible = true;
                btnCreateCellRegions.Visible = true;
                btnOpenTemplateMatchWindow.Visible = false;
                btnPickGoldenRegion.Visible = false;
                btnWriteCoordsToPlc.Visible = true;
                panel2.Enabled = true;
            }
            else if (btnTarget == btnSwitchToChipTemplate)
            {
                btnOpenEmptyTrayWindow.Visible = false;
                btnCreateCellRegions.Visible = false;
                btnOpenTemplateMatchWindow.Visible = true;
                btnPickGoldenRegion.Visible = true;
                btnWriteCoordsToPlc.Visible = true;
                panel2.Enabled = true;
            }
            else
            {
                btnWriteCoordsToPlc.Visible = false;
                panel2.Enabled = false;
            }
        }
    }
}
