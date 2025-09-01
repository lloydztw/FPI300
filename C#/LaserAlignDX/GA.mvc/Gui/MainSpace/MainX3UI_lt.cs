using Eazy_Project_III;
using JetEazy.BasicSpace;
using System.Windows.Forms;

using GaMainCtrl = LaserAlignDX.Mvc.Ctrl.GaMainCtrl;


namespace LaserAlignDX.UISpace.MainSpace
{
    public partial class MainX3UI : UserControl //, IMainUI
    {
        //public event EventHandler<MainUIStateChangedEventArgs> OnStateChanged;
        public delegate void ChangeStateHandler(MainS1State status, object tag = null);
        public event ChangeStateHandler OnChangeState;

        #region PRIVATE_DATA
        GaMainCtrl _mainCtrl;
        #endregion

        public MainX3UI()
        {
            InitializeComponent();
        }

        public void Init()
        {
            CommonLogClass.Instance.SetRichTextBox(richTextBox1);

            _mainCtrl = new GaMainCtrl();

            _mainCtrl.Attach( new[] { mvsui1, mvsui2 },
                              new[] { DSFly0, DSFly1, DSFly2, DSFly3 },
                              label1 );

            //_mainCtrl.OnStateChanged += (s, e) => OnStateChanged?.Invoke(s, e);
            _mainCtrl.OnStateChanged += (s, e) => OnChangeState?.Invoke(e.Status, e.Tag);
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
