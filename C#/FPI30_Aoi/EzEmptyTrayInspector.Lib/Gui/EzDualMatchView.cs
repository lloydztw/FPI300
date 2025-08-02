using AwFramework;
using AwFramework.GUI;
using System.Windows.Forms;


namespace EzDualMatch.GUI
{
    internal class EzDualMatchView : IView
    {
        FormAwMain _frmMain;
        public EzDualMatchView(FormAwMain frmMain)
        {
            _frmMain = frmMain;
        }

        Form IView.frmOwner => _frmMain;
        Control IView.Window => _frmMain;
    }
}
