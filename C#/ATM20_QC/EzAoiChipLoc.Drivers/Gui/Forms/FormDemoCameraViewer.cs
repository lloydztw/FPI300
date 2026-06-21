using AwFramework;
using EzCamera.GUI;
using EzDualMatch.Model;
using JetEazy.ImageViewerEx;
using JetEazy.ImageViewerQx;
using System.Windows.Forms;



namespace EzDualMatch.GUI
{
    public partial class FormDemoCameraViewer : Form, IvAppMainWindow, IvSingleMatchView
    {
        #region PRIVATE_DATA
        #endregion

        public FormDemoCameraViewer()
        {
            InitializeComponent();
        }

        #region GUI_LINKS
        Form IView.frmOwner => this;
        Control IView.Window => this;
        IvCameraViewer IvAppMainWindow.camLiveViewer => qzCameraViewer1;
        IvCameraPanel IvAppMainWindow.camPropertyPanel => jxCameraPropertyPanel1;
        Control IvAppMainWindow.lblCamCoordInfo => this.lblCoordInfo;
        Control IvAppMainWindow.lblCamBlinker => this.lblBlinker;
        Control IvAppMainWindow.lblCamFrameCount => this.lblTitle;
        Button IvAppMainWindow.btnOpenCamera => this.jxCameraPropertyPanel1.btnOpenCamera;

        IvSingleMatchView IvAppMainWindow.matchView => this;
        IvLiveImageViewer IvSingleMatchView.LiveImageViewer => qzCameraViewer1 as IvLiveImageViewer;
        IvImageViewer IvSingleMatchView.ImageViewer => qzCameraViewer1;
        Button IvSingleMatchView.btnOpen => null;
        Button IvSingleMatchView.btnRunMatch => this.jxCameraPropertyPanel1.btnMatch;
        Button IvSingleMatchView.btnClearResult => this.jxCameraPropertyPanel1.btnClear;
        Button IvSingleMatchView.btnCatchGolden => null;
        Control IvSingleMatchView.lblInfo => this.jxCameraPropertyPanel1.lblDeviceInfo;
        #endregion

        void IvSingleMatchView.UpdateMatchState(object state)
        {
        }
        void IvSingleMatchView.UpdateImageSrcName(string srcName)
        {
        }
        void IvSingleMatchView.UpdateMatchResult(MatchResultEventArgs e)
        {
        }
    }
}
