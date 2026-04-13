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
    public partial class GwMotorAxisStatusCmd : UserControl
    {
        public GwMotorAxisStatusCmd()
        {
            InitializeComponent();
        }

        public string AxisName
        {
            get => gwMotorAxisStatus1.AxisName;
            set => gwMotorAxisStatus1.AxisName = value;
        }
    }
}
