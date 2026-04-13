#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GwMotorCommander : UserControl
    {
        JogStickOption _option;
        public GwMotorCommander()
        {
            InitializeComponent();
            SizeChanged += GPaneMotorCommanderXY_SizeChanged;
            HandleCreated += GwMotorCommander_HandleCreated;
            _option = gwMotorJogSticks1.JogStickOption;
            updateVisibility();
            autoLayout();
        }

        #region PRIVATE_FUNCTIONS
        private void GPaneMotorCommanderXY_SizeChanged(object sender, System.EventArgs e)
        {
            autoLayout();
        }
        private void GwMotorCommander_HandleCreated(object sender, System.EventArgs e)
        {
            updateVisibility();
            autoLayout();
        }
        private void autoLayout()
        {
            gwMotorMoveCmdXY.AutoAdjustSize();
            alignCenter(gwMotorMoveCmdXY);
            gwMotorMoveCmd1D.AutoAdjustSize();
            alignCenter(gwMotorMoveCmd1D);
            int gap = gwSpeedHiLoButtons1.Left - groupBox2.Right;
            gwSpeedHiLoButtons1.Width = ClientSize.Width - gap - gwSpeedHiLoButtons1.Left;
        }
        private void alignCenter(Control c)
        {
            var csz = c.Parent.ClientSize;
            c.Left = (csz.Width - c.Width) / 2;
            c.Top = (csz.Height - c.Height) / 2;
        }
        private void updateVisibility()
        {
            if (_option == JogStickOption.XY)
            {
                gwMotorMoveCmdXY.Visible = true;
                gwMotorMoveCmd1D.Visible = false;
                gwMotorMoveCmdXY.Refresh();
            }
            else
            {
                gwMotorMoveCmd1D.Visible = true;
                gwMotorMoveCmdXY.Visible = false;
                gwMotorMoveCmd1D.Refresh();
            }
        }
        #endregion

        public JogStickOption Option
        {
            get
            {
                return _option;
            }
            set
            {
                if (true || _option != value)
                {
                    _option = value;
                    gwMotorJogSticks1.JogStickOption = _option;
                    if (true || IsHandleCreated)
                    {
                        updateVisibility();
                        autoLayout();
                    }
                }
            }
        }
        public string TagName
        {
            get
            {
                return gwMotorMoveCmd1D.lblAxisName.Text;
            }
            set
            {
                gwMotorMoveCmd1D.lblAxisName.Text = value;
            }
        }

        public Button btnConfig 
            => gwSpeedHiLoButtons1.btnConfig;
        public Button btnMoveTo =>
            Option == JogStickOption.XY
            ? gwMotorMoveCmdXY.btnMoveTo
            : gwMotorMoveCmd1D.btnMoveTo;
        public NumericUpDown numMoveTo1 =>
            Option == JogStickOption.XY
            ? gwMotorMoveCmdXY.numMoveTo1
            : gwMotorMoveCmd1D.numMoveTo1;
        public NumericUpDown numMoveTo2 
            => gwMotorMoveCmdXY.numMoveTo2;

        public Button btnStickHome => gwMotorJogSticks1.btnStickHome;
        public Button btnStickUp => gwMotorJogSticks1.btnStickUp;
        public Button btnStickDown => gwMotorJogSticks1.btnStickDown;
        public Button btnStickLeft => gwMotorJogSticks1.btnStickLeft;
        public Button btnStickRight => gwMotorJogSticks1.btnStickRight;
        public Button btnStickBackward => gwMotorJogSticks1.btnBackward;
        public Button btnStickForward => gwMotorJogSticks1.btnForward;

        public Control lblSpeed => gwSpeedHiLoButtons1.lblSpeed;
        public RadioButton rdoSpeedModeHI => gwSpeedHiLoButtons1.rdoSpeedModeHI;
        public RadioButton rdoSpeedModeLO => gwSpeedHiLoButtons1.rdoSpeedModeLO;
        public RadioButton rdoMicroStep => null; // gwSpeedHiLoButtons1.rdoMicroStep;
    }
}
