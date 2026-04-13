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
    public partial class GwMotorSimpleGoPanel : UserControl
    {
        public GwMotorSimpleGoPanel()
        {
            InitializeComponent();
        }

        public string AxisName
        {
            get => lblAxisName.Text; 
            set => lblAxisName.Text = value;
        }
    }
}
