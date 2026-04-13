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
using JetEazy.Drivers.Motor;


namespace AX.Gui
{
    public partial class GwMotorSpeedSettings : UserControl
    {
        public GwMotorSpeedSettings()
        {
            InitializeComponent();
        }

        public string AxisName
        {
            get { return groupBox7.Text; }
            set { groupBox7.Text = value; }
        }
        public void UpdateAxisData(IDrvMotorAxis axis, bool bToDriver)
        {
            if (axis != null)
            {
                if (bToDriver)
                {
                    axis.SetHomeSpeeds((double)numHomeHigh.Value, (double)numHomeLow.Value, MotorUnitMode.Physical);
                    axis.SetMotionSpeeds((double)numHighSpeed.Value, (double)numLowSpeed.Value, MotorUnitMode.Physical);
                }
                else
                {
                    double dHigh;
                    double dLow;

                    axis.GetHomeSpeeds(out dHigh, out dLow, MotorUnitMode.Physical);
                    _setNum(numHomeHigh, dHigh);
                    _setNum(numHomeLow, dLow);

                    axis.GetMotionSpeeds(out dHigh, out dLow, MotorUnitMode.Physical);
                    _setNum(numHighSpeed, dHigh);
                    _setNum(numLowSpeed, dLow);
                }
            }
        }

        #region PRIVATE_FUNCTIONS
        private void _setNum(NumericUpDown num, double dValue)
        {
            if (dValue > (double)num.Maximum)
                num.Value = num.Maximum;
            else if (dValue < (double)num.Minimum)
                num.Value = num.Minimum;
            else
                num.Value = (decimal)dValue;
        }
        #endregion
    }
}
