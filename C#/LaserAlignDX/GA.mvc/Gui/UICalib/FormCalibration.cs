using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.FormSpace.FPI30Form
{
    public partial class FormCalibration : Form
    {
        public FormCalibration()
        {
            InitializeComponent();
            rdoCarrier1.CheckedChanged += RdoCarrier1_CheckedChanged;
            rdoSucker1.CheckedChanged += RdoSucker1_CheckedChanged;
        }

        private void RdoSucker1_CheckedChanged(object sender, System.EventArgs e)
        {
            updateForeColor(rdoSucker1);
            updateForeColor(rdoSucker2);
        }

        private void RdoCarrier1_CheckedChanged(object sender, System.EventArgs e)
        {
            updateForeColor(rdoCarrier1);
            updateForeColor(rdoCarrier2);
        }

        void updateForeColor(RadioButton rdo)
        {
            rdo.ForeColor = rdo.Checked ? Color.Black : Color.DimGray;
        }
    }
}
