using AwFramework;
using JetEazy.Lang;
using System.Windows.Forms;

namespace EzAoiEmptyTrayInspector.Gui.Panels
{
    public partial class GwFuncButtonsPanel : UserControl, IvFuncButtonsPanel
    {
        public GwFuncButtonsPanel()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                HandleCreated += (s, e) =>
                {
                    toolTip1.SetToolTip(btnRunAll, btnRunAll.Text = QMSG.Text("檢測"));
                    toolTip1.SetToolTip(btnOpenFile, btnOpenFile.Text = QMSG.Text("讀取圖檔"));
                    toolTip1.SetToolTip(btnSnapshot, btnSnapshot.Text = QMSG.Text("擷取影像"));
                    toolTip1.SetToolTip(btnResetClear, btnResetClear.Text = QMSG.Text("清除"));
                    toolTip1.SetToolTip(btnPickGolden, btnPickGolden.Text = QMSG.Text("擷取樣本"));
                };
            }
        }

        Control IView.Window => this;
        Button IvFuncButtonsPanel.btnRunAll => btnRunAll;
        Button IvFuncButtonsPanel.btnOpenFile => btnOpenFile;
        Button IvFuncButtonsPanel.btnSnapshot => btnSnapshot;
        Button IvFuncButtonsPanel.btnResetClear => btnResetClear;
        Button IvFuncButtonsPanel.btnPickGolden => btnPickGolden;

        /// <summary>
        /// 讓 this 與 target 連動
        /// </summary>
        void IvFuncButtonsPanel.HookTo(IvFuncButtonsPanel target)
        {
            if (target == null)
                return;

            var srcButtons = new Button[]
            {
                btnRunAll, btnOpenFile, btnSnapshot, btnResetClear, btnPickGolden,
            };

            var dstButtons = new Button[]
            {
                target.btnRunAll, target.btnOpenFile, target.btnSnapshot, target.btnResetClear, target.btnPickGolden,
            };

            for(int i=0, count = srcButtons.Length; i < count; i++)
            {
                var btn = srcButtons[i];
                var btnTgt = dstButtons[i];
                if (btn != null && btnTgt != null)
                {
                    btn.Tag = btnTgt;
                    btn.Click += Btn_Click;
                    btn.EnabledChanged += Btn_EnabledChanged;
                    btn.VisibleChanged += Btn_VisibleChanged;
                }
            }
        }
        private void Btn_Click(object sender, System.EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Button target)
            {
                target.PerformClick();
            }
        }
        private void Btn_EnabledChanged(object sender, System.EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Button target)
            {
                btn.Enabled = target.Enabled;
            }
        }
        private void Btn_VisibleChanged(object sender, System.EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Button target)
            {
                btn.Visible = target.Visible;
            }
        }
    }
}
