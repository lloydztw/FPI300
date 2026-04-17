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

using EzMotor.Gui.View.Wrappers;
using System.Windows.Forms;


namespace AX.Gui
{
    public partial class GvPaneMotorJogXY : UserControl
    {
        public GvPaneMotorJogXY()
        {
            InitializeComponent();
        }
        public IvMotorJogView GetJogViewX()
        {
            return new UiMotorJogView(gwMotorAxisStatusCmd1, gwMotorJogSticks1, JogStickOption.Horiz);
        }
        public IvMotorJogView GetJogViewY()
        {
            return new UiMotorJogView(gwMotorAxisStatusCmd2, gwMotorJogSticks1, JogStickOption.Vert);
        }
        public string AxisName1
        {
            get => gwMotorAxisStatusCmd1.AxisName;
            set => gwMotorAxisStatusCmd1.AxisName = value;
        }
        public string AxisName2
        {
            get => gwMotorAxisStatusCmd2.AxisName;
            set => gwMotorAxisStatusCmd2.AxisName = value;
        }
    }
}
