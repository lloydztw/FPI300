using AwFramework;
using AwFramework.Gui;
using EzAoiChipLocQC.Gui.Panels;
using System.Windows.Forms;

namespace EzAoiChipLocQC.Gui
{
    internal class EzAoiUI : IvAppUI
    {
        #region PRIVATE_DATA
        FormAwMain _frmMain;
        #endregion

        public EzAoiUI(FormAwMain frmMain)
        {
            _frmMain = frmMain;
        }

        Control IView.Window => _frmMain;
        public FormAwMain frmAwMain => _frmMain;
        public GvProductionPanel wndProductionPanel => _frmMain?.OpDocker.FindPanel<GvProductionPanel>();

        public IvSingleMatchView MatchView => _frmMain?.ClientDocker.FindPanel<GvSingleMatchViewPanel>();
        public IvFuncButtonsPanel FuncButtonsPanel => wndProductionPanel?.FuncButtonsPanel;
        public IvRecipeBriefView RecipeBriefView => wndProductionPanel?.RecipeBriefView;
        public Control lblPassFail => wndProductionPanel?.lblPassFail;
    }
}
