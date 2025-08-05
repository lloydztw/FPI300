#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AwFramework.Gui;
using AwFramework.Util;
using EzAoiEmptyTrayInspector.Gui;
using EzAoiEmptyTrayInspector.Model;
using JetEazy;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using LeTian.JxRecipesTool.Ctrl;
using System;
using System.Drawing;
using System.Windows.Forms;
using CviGoldenBox = JetEazy.ImageViewerEx.Interactors.CvImageViewerRectBox;


namespace EzAoiEmptyTrayInspector.Ctrl
{
    internal class EzMatchRcpEdittingCtrl : BaseUtil
    {
        #region NLOG
        //// NOTE:
        //// 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        //// 且保證該窗體是 【首個使用】NLog 的 Class !!!
        //// 這是因為NLog是在首次被使用時，才載入配置文件的。
        //static NLog.ILogger _logger = null;
        //protected NLog.ILogger LOG
        //{
        //    get
        //    {
        //        if (_logger == null)
        //            _logger = NLog.LogManager.GetCurrentClassLogger();
        //        return _logger;
        //    }
        //}
        #endregion

        #region GLOBAL_DATA
        IxEmptyTrayInspector _model => Global.AoiModel;
        JxAppSettings _appSettings => Global.AppSettings;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        IEzImage _imgSource = null;
        IRecipesMgrCtrl _recipesMgr;
        JxAoiRecipe _activeRecipe
        {
            get => _recipesMgr?.ActiveRecipe as JxAoiRecipe;
        }
        JxTrayVisionSettings _sideSettings
        {
            get => _activeRecipe?.GetSiteSettings((int)ID);
        }
        JxTempMatchSettings _matchSettings
        {
            get => _sideSettings?.Match;
        }
        bool _isRcpEdittingMode = false;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Form _frmOwner;
        Control _wndRcpHostPanel;
        Control _imgViewerWindow;
        IvImageViewer _imgViewer;
        IvFuncButtonsPanel _funcButtonsPanel;
        Button _btnGolden => _funcButtonsPanel?.btnPickGolden;
        CviGoldenBox _cviGoldenBox;
        CviFiltersBox _cviFiltersBox;
        bool _bypassJxEvents = false;
        #endregion

        public EzMatchRcpEdittingCtrl(int sideId, IvSingleMatchView view, IvFuncButtonsPanel funcPanel, IRecipesMgrCtrl recipesMgr)
        {
            ID = (SideID)sideId;

            //_TRACE($"{GetType().Name}(view={view.Window.Handle})");

            _recipesMgr = recipesMgr;
            _frmOwner = view.Window.FindForm();
            _imgViewer = view.ImageViewer;
            _imgViewerWindow = _imgViewer as Control;
            _funcButtonsPanel = funcPanel;
            //_btnGolden = funcPanel?.btnPickGolden;

            var frmMain = _frmOwner as FormAwMain;
            _wndRcpHostPanel = frmMain?.OpDocker.FindPanel<AwFramework.Gui.DefaultPanels.GvRecipeDockPanel>();
            if (_wndRcpHostPanel == null)
                _wndRcpHostPanel = view.Window;

            init_interactors();
            init_event_handlers();

            _frmOwner.BeginInvoke(new Action(() =>
            {
                update_rcp_editor_gui_status();
            }));
        }
        public SideID ID
        {
            get;
            private set;
        }

        public void AttachImageSource(IEzImage imgSrc)
        {
            _imgSource = imgSrc;
            if (_isRcpEdittingMode)
                move_box_to_safe_location(_imgSource);
        }

        public bool IsEditting
        {
            get => _isRcpEdittingMode;
        }

        internal void AutoCatchGolden()
        {
            if (_isRcpEdittingMode)
            {
                if (_cviGoldenBox.Box != _matchSettings.GoldenBox.Value)
                    catch_golden_image();
            }
        }

        #region EVENT_HANDLERS
        void init_event_handlers()
        {
            #region RECIPE_MGR_EVENT_HANDLERS
            EzAppForDll.Instance.opModesCtrl.OnOpModeChanged += OpModesCtrl_OnOpModeChanged;
            _recipesMgr.OnRecipeSelectionChanged += _recipesMgr_OnRecipeSelectionChanged;
            _recipesMgr.OnRecipeBrowsing += _recipesMgr_OnRecipeBrowsing;
            _recipesMgr.OnRecipeEditting += _recipesMgr_OnRecipeEditting;
            connect_recipe_prop_handlers();
            #endregion

            #region GUI_EVENT_HANDLERS
            if (_btnGolden != null)
                _btnGolden.Click += BtnGolden_Click;
            if (_imgViewerWindow!=null)
                _imgViewerWindow.KeyDown += viewer_KeyDown;
            #endregion

            #region MODEL_EVENT
            _model.OnStateChanged += _model_OnStateChanged;
            //_model.OnMatched += _model_OnMatched;
            #endregion
        }
        void connect_recipe_prop_handlers()
        {
            if (_sideSettings != null)
            {
                _sideSettings.RotAngle.OnModified += side_RotAngle_OnModified;
            }
        }

        private void _recipesMgr_OnRecipeSelectionChanged(object sender, EventArgs e)
        {
            //_model.SetRecipe(_activeRecipe);
            //connect_recipe_prop_handlers();
            //_view.Window.Invalidate();
            updateActiveRecipe();
        }
        private void _recipesMgr_OnRecipeEditting(object sender, EventArgs e)
        {
            //_isRcpEdittingMode = true;
            //load_golden_box();
            //update_rcp_editor_gui_status();
            //refresh(_view.ImageViewer);

            enterEdittingMode();
        }
        private void _recipesMgr_OnRecipeBrowsing(object sender, EventArgs e)
        {
            //_isRcpEdittingMode = false;
            //update_golden_box();
            //update_rcp_editor_gui_status();
            //refresh(_view.ImageViewer);

            leaveEdittingMode();
        }

        private void BtnGolden_Click(object sender, EventArgs e)
        {
            if (_isRcpEdittingMode)
            {
                catch_golden_image();
                build_golden_grid_template();
            }
        }
        private void viewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isRcpEdittingMode && e.KeyCode == Keys.F3)
                move_box_to_default_location();
        }
        private void side_RotAngle_OnModified(object sender, EventArgs e)
        {
            if (_bypassJxEvents)
                return;
            if (_isRcpEdittingMode)
                showFiltersEffect();
        }

        private void _model_OnStateChanged(object sender, EventArgs e)
        {
            if (!_isRcpEdittingMode)
                return;

            // 過濾
            var ev = (MatchStateEventArgs)e;
            if (ev != null && ev.ID != SideID.All && ev.ID != ID)
                return;

            update_model_state(ev);
        }
        private void _model_OnMatched(object sender, MatchResultEventArgs e)
        {
            // RESERVED
        }
        #endregion

        void updateActiveRecipe()
        {
            if (_isRcpEdittingMode)
            {
                //_model.SetRecipe(_activeRecipe);
                connect_recipe_prop_handlers();
                _wndRcpHostPanel?.Invalidate();
            }
        }
        void enterEdittingMode()
        {
            _isRcpEdittingMode = true;
            update_golden_box_to_gui();
            update_rcp_editor_gui_status();
            refresh(_imgViewerWindow);
        }
        void leaveEdittingMode()
        {
            _isRcpEdittingMode = false;
            update_golden_box_to_recipe();
            update_rcp_editor_gui_status();
            refresh(_imgViewerWindow);
        }
        void showFiltersEffect()
        {
            if (_isRcpEdittingMode)
            {
                _cviFiltersBox.ApplyFilters(ID, _imgSource, _sideSettings.RotAngle, _imgViewerWindow);
            }
        }

        #region CVI_BOX_FUNCTIONS
        void init_interactors()
        {
            _cviGoldenBox = new CviGoldenBox(Brushes.Orange, 1, 3);
            _cviFiltersBox = new CviFiltersBox(Brushes.Lime, 1, 3);

            if (_imgViewer != null)
            {
                _imgViewer.AddInteractor(_cviGoldenBox);
                _cviGoldenBox.Enabled = false;
                _cviGoldenBox.Visible = false;

                _imgViewer.AddInteractor(_cviFiltersBox);
                _cviFiltersBox.Enabled = false;
                _cviFiltersBox.Visible = false;
            }
        }
        void update_golden_box_to_gui()
        {
            Rectangle loc = _matchSettings != null ?
                            _matchSettings.GoldenBox.Value :
                            Rectangle.Empty;

            if (loc == Rectangle.Empty)
            {
                move_box_to_default_location();
            }
            else
            {
                _cviGoldenBox.Box = loc;
                move_box_to_safe_location(_imgSource);
            }
        }
        void update_golden_box_to_recipe()
        {
            if (_matchSettings == null)
                return;
            _bypassJxEvents = true;
            var loc = _cviGoldenBox.Box;
            if (_matchSettings.GoldenBox.Value != loc)
                _matchSettings.GoldenBox.Value = loc;
            _bypassJxEvents = false;
        }
        void move_box_to_safe_location(Size boundarySize)
        {
            var rect = _cviGoldenBox.Box;
            rect.Width = Math.Min(rect.Width, boundarySize.Width - 1);
            rect.Height = Math.Min(rect.Height, boundarySize.Height - 1);
            int x = Math.Max(0, rect.X);
            int y = Math.Max(0, rect.Y);
            if (rect.Right > boundarySize.Width)
                x = boundarySize.Width - rect.Width;
            if  (rect.Bottom > boundarySize.Height)
                y = boundarySize.Height - rect.Height;
            rect.X = x;
            rect.Y = y;
            _cviGoldenBox.Box = rect;
            //int x2 = Math.Min(boundarySize.Width, rect.Right);
            //int y2 = Math.Min(boundarySize.Height, rect.Bottom);
            //_cviGoldenBox.Box = new Rectangle(x, y, x2 - x, y2 - y);
        }
        void move_box_to_safe_location(IEzImage imgSrc)
        {
            if (imgSrc != null)
            {
                move_box_to_safe_location(imgSrc.Size);
            }
        }
        void move_box_to_default_location()
        {
            if (_imgViewer == null)
                return;
            var boundary = _imgViewer.GetWorldRect();
            int x = (int)boundary.Width / 5;
            int y = (int)boundary.Height / 5;
            int w = (int)boundary.Width / 20;
            int h = (int)boundary.Height / 20;
            int sz = Math.Min(w, h);
            _cviGoldenBox.Box = new Rectangle(x, y, sz, sz);
        }
        #endregion

        #region GOLDEN_BUILDING_FUNCTIONS
        void catch_golden_image()
        {
            if (_imgViewer == null || _imgSource == null)
                return;

            // 閃綠色
            var oldBrush = _cviGoldenBox.BoxBrush;
            _cviGoldenBox.BoxBrush = Brushes.Lime;
            refresh(_imgViewerWindow);

            _bypassJxEvents = true;
            try
            {
                move_box_to_safe_location(_imgSource);
                //var rect = _cviGoldenBox.Box;
                //var golden = ImageUtil.CropBmp(_imgSource, rect);
                //_matchSettings.GoldenBox.Value = rect;
                //_matchSettings.GoldenBmp.Value = golden;
                _model.CropGoldenTemplate(ID, _imgSource, _cviGoldenBox.Box);
            }
            catch
            {
            }
            _bypassJxEvents = false;

            // 回復顏色
            _cviGoldenBox.BoxBrush = oldBrush;
            refresh(_recipesMgr?.Window);
            refresh(_imgViewerWindow);
        }
        void build_golden_grid_template()
        {
            if (_imgViewer == null || _imgSource == null || _model == null)
                return;

            var err = _model.BuildGoldenGridTemplate(SideID.A, _imgSource);
            if (err != ErrCodes.OK)
            {
                var msg = QxNums.GetEnumDescription(err);
                MessageBox.Show(msg, _frmOwner.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void update_model_state(MatchStateEventArgs e)
        {
            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action<MatchStateEventArgs>)update_model_state, e);
                return;
            }

            bool isReady = _model != null ? _model.IsReady() : false;
            setEnable(_btnGolden, isReady && _isRcpEdittingMode);
        }
        void update_rcp_editor_gui_status()
        {
            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action)update_rcp_editor_gui_status);
            }
            else
            {
                //setVisible(_btnGolden, _isRcpEdittingMode);
                setEnable(_btnGolden, _isRcpEdittingMode);
                _btnGolden.BackColor = _isRcpEdittingMode ? Color.Gold : Color.DarkGray;
                _cviGoldenBox.Visible = _isRcpEdittingMode;
                _cviGoldenBox.Enabled = _isRcpEdittingMode;
                _cviFiltersBox.Visible = _isRcpEdittingMode;
                _cviFiltersBox.Enabled = _isRcpEdittingMode;
            }
        }
        void refresh(object c)
        {
            if (c != null && c is Control wnd)
            {
                if (_frmOwner.InvokeRequired)
                {
                    _frmOwner.Invoke((Action)wnd.Refresh);
                }
                else
                {
                    wnd.Refresh();
                }
            }
        }
        #endregion

        #region PRIVATE_SWAP_FUNCTIONS
        private void OpModesCtrl_OnOpModeChanged(object sender, EventArgs e)
        {
            if (EzAppForDll.Instance.opModesCtrl.OpMode == "Recipe")
            {
                swap_func_buttons_panel(true);
            }
            else
            {
                swap_func_buttons_panel(false);
            }
        }
        void swap_func_buttons_panel(bool toTop)
        {
            var _frmAwMain = _frmOwner as FormAwMain;
            var funcPanel = _funcButtonsPanel?.Window;
            var logoPanel = _frmAwMain?.wndLogoPanel;

            if (logoPanel == null || funcPanel == null)
                return;

            if (toTop && funcPanel.Parent == logoPanel)
                return;

            if (!toTop && funcPanel.Parent != logoPanel)
                return;

            _frmAwMain?.BeginInvoke(new Action(() =>
            {
                AppUtil.SwapGui(funcPanel, logoPanel.picLogo);
            }));
        }
        #endregion

        #region TRACE_AND_LOG
        void _TRACE(string msg, bool isError = false)
        {
            //if (isError)
            //    LOG.Error("[{0}] {1}", _ID, msg);
            //else
            //    LOG.Info("[{0}] {1}", _ID, msg);
        }
        #endregion
    }
}
