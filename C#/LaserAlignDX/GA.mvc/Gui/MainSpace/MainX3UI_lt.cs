using JetEazy.BasicSpace;
using System;
using System.Windows.Forms;
using GaMainCtrl = LaserAlignDX.Mvc.Ctrl.Abs.GaMainCtrl;


namespace LaserAlignDX.UISpace.MainSpace
{
    public partial class MainX3UI : UserControl, IMainUI
    {
        public event EventHandler<MainUiStateEventArgs> OnStateChanged;

        #region PRIVATE_DATA
        GaMainCtrl _mainCtrl;
        #endregion

        public MainX3UI()
        {
            InitializeComponent();
        }

        public Control Window => this;

        public void Init()
        {
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);

            _mainCtrl = GaMvcConfig.CreateMainCtrl();

            _mainCtrl.Attach( new[] { mvsui1, mvsui2 },
                              new[] { DSFly0, DSFly1, DSFly2, DSFly3 },
                              lblFlySerialNumber, 
                              lblMemoryUsage );

            _mainCtrl.OnStateChanged += (s, e) =>
            {
                //OnChangeState?.Invoke(e.Status, e.Tag);
                OnStateChanged(s, e);
            };
        }
        
        public void Tick()
        {
            _mainCtrl?.Tick();
        }

        /// <summary>
        /// 對上層 MainControlUI 所需要的接口
        /// </summary>
        public void ChangeRecipe()
        {
            _mainCtrl?.ChangeRecipe();
        }

        /// <summary>
        /// 對上層 MainControlUI 所需要的接口
        /// </summary>
        public void SetEnable(bool enabled)
        {
            // MainControlUI 調用 此函式的 目的 & 作用 是?
            _mainCtrl?.SetEnable(enabled);
        }

        /// <summary>
        /// 對上層 MainControlUI 所需要的接口
        /// </summary>
        public void SetEnableState(bool enabled)
        {
            //MainControlUI 調用 此函式的 目的 & 作用 是?
            _mainCtrl?.SetEnableState(enabled);
        }
    }
}
