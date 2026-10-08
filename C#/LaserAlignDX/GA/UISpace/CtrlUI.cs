#region AUTHOR
/*
 *
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 *
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整
 *
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 *
 */
#endregion

using System.Drawing;
using System.Windows.Forms;
using JetEazy;
using LaserAlignDX.UISpace.CtrlSpace;

namespace PhotoMachine.UISpace
{
    public partial class CtrlUI : UserControl
    {

        IoPanelUI _ioPanel;

        public CtrlUI()
        {
            InitializeComponent();
            
            if(!DesignMode)
            {
                HandleDestroyed += (s, e) => _ioPanel = null;
            }
        }

        public void Initial(VersionEnum version, OptionEnum option, Traveller.FPI30.Contracts.IFpix3Machine machine)
        {

            _ioPanel = IoPanelFactory.CreateIoPanelUI(version, option, machine);
            if (_ioPanel != null)
            {
                var window = _ioPanel.Window;
                window.Location = new Point(0, 0);
                this.Controls.Add(window);
                window.Dock = DockStyle.Fill;
            }
        }
        public void Tick()
        {
            _ioPanel?.Tick();
        }
        public void SetEnable(bool isenable)
        {
            _ioPanel?.SetEnable(isenable);
        }
        public void MyDispose()
        {

            //------------------------------------------------------------------
            // 註:
            // mainX3.MyDispose()
            // 改在 自己在 HandleDestroyed 的 EventHandler 中, 自己釋放資源
            //------------------------------------------------------------------
        }

#if (OPT_NOT_USED)
        public delegate void TriggerHandler(ActionEnum action, string opstr);
        public event TriggerHandler TriggerAction;
        public void OnTrigger(ActionEnum action, string opstr)
        {
            if (TriggerAction != null)
            {
                TriggerAction(action, opstr);
            }
        }
#endif

    }
}
