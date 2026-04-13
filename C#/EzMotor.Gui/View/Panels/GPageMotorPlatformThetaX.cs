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
    public partial class GPageMotorPlatformThetaX : UserControl
    {
        public GPageMotorPlatformThetaX()
        {
            InitializeComponent();
            //if (DesignMode)
            //{
            //    gwMotorAxisStatusAndCtrl1.AxisName = "X";
            //    gwMotorAxisStatusAndCtrl2.AxisName = "θ";
            //}
            tableLayoutPanel1.Dock = DockStyle.Fill;
        }

#if (false)
        public void UpdateAxisName(IDrvMotorAxis axis)
        {
            var wnd = axis.ID==0 
                ? gwMotorAxisStatusAndCtrl1 
                : gwMotorAxisStatusAndCtrl2;
            wnd.UpdateAxisName(axis);
        }
        public void UpdateAxisStatus(IDrvMotorAxis axis)
        {
            var wnd = axis.ID == 0
                ? gwMotorAxisStatusAndCtrl1
                : gwMotorAxisStatusAndCtrl2;
            wnd.UpdateAxisStatus(axis);
        }
#endif        
    }
}
