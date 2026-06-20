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
using EzAoiChipLocQC.Gui;
using EzAoiChipLocQC.Model;
using JetEazy;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using LeTian.JxProps.Gui;
using LeTian.JxRecipesTool.Ctrl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CviBoundBox = EzAoiChipLocQC.Ctrl.CviRcpBox;
using CviGoldenBox = EzAoiChipLocQC.Ctrl.CviRcpBox;


namespace EzAoiChipLocQC.Ctrl
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
        IxChipLocator _model => Global.AoiModel;
        //JxAppSettings _appSettings => Global.AppSettings;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        IEzImage _imgSource = null;
        IRecipesMgrCtrl _recipesMgr;
        JxQcRecipe _activeRecipe
        {
            get => _recipesMgr?.ActiveRecipe as JxQcRecipe;
        }
        JxTraySegGrpSettings _segGrpSettings
        {
            get => _activeRecipe?.TraySegGrpSettings;
        }
        JxQcVisionSettings _sideSettings
        {
            get => _activeRecipe?.GetSiteSettings((int)ID);
        }
        JxTempMatchSettings _matchSettings
        {
            get => _sideSettings?.Match;
        }
        bool _isRcpEdittingMode = false;
        bool _bypassJxEvents = false;
        #endregion

        #region PRIVATE_GUI_LINKS
        Form _frmOwner;
        Control _wndRcpHostPanel;
        Control _imgViewerWindow;
        IvImageViewer _imgViewer;
        IvFuncButtonsPanel _funcButtonsPanel;
        Button _btnGolden => _funcButtonsPanel?.btnPickGolden;
        #endregion

        #region PRIVATE_INTERACTORS
        List<CviBoundBox> _cviGroupBoundBoxes;
        CviGoldenBox _cviGoldenBox;
        CviFiltersBox _cviFiltersBox;
        #endregion

        public EzMatchRcpEdittingCtrl(int sideId, IvCommonImageView view, IvFuncButtonsPanel funcPanel, IRecipesMgrCtrl recipesMgr)
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

            _frmOwner.BeginInvoke((Action)update_rcp_editor_gui_status);
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
                move_boxes_to_safe_location(_imgSource);
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
                _imgViewerWindow.KeyDown += Viewer_KeyDown;
            #endregion

            #region MODEL_EVENT
            _model.OnStateChanged += _model_OnStateChanged;
            //_model.OnMatched += _model_OnMatched;
            #endregion
        }
        void connect_recipe_prop_handlers()
        {
            if (_segGrpSettings != null)
            {
                //_segGrpSettings.SegsNumber.OnModified += SegsNumber_OnModified;
                _segGrpSettings.SegsNumber.OnButtonClicked += SegsNumber_OnButtonClicked;
            }

            if (_sideSettings != null)
            {
                _sideSettings.RotAngle.OnModified += Side_RotAngle_OnModified;
            }
        }

        private void _recipesMgr_OnRecipeSelectionChanged(object sender, EventArgs e)
        {
            rebuildPropsView();
            updateActiveRecipe();
        }
        private void _recipesMgr_OnRecipeEditting(object sender, EventArgs e)
        {
            if (!_isRcpEdittingMode)
            {
                enterEdittingMode();
            }
        }
        private void _recipesMgr_OnRecipeBrowsing(object sender, EventArgs e)
        {
            if (_isRcpEdittingMode)
            {
                leaveEdittingMode();
            }

            //GwPanePropsViewerExtension.ExpandTrees(BaseUtil.SearchAwMainForm().OpDocker.Window);
        }

        private void BtnGolden_Click(object sender, EventArgs e)
        {
            if (_isRcpEdittingMode)
            {
                catch_golden_image();
                build_golden_grid_template();
            }
        }
        private void Viewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isRcpEdittingMode && e.KeyCode == Keys.F3)
            {
                autoLayoutCviBoxes();
            }
        }
        private void Side_RotAngle_OnModified(object sender, EventArgs e)
        {
            if (_bypassJxEvents || !_isRcpEdittingMode)
                return;
            showFiltersEffect();
        }
        private void SegsNumber_OnModified(object sender, EventArgs e)
        {
            // RESERVED
            if (_bypassJxEvents || !_isRcpEdittingMode)
                return;
            syncSegGroupsNumber();
        }
        private void SegsNumber_OnButtonClicked(object sender, EventArgs e)
        {
            if (_bypassJxEvents || !_isRcpEdittingMode)
                return;
            openSegDetailSettings();
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
            update_cvi_boxes_to_gui();
            update_rcp_editor_gui_status();
            refresh(_imgViewerWindow);
            _model?.ResetAndClear();        //@<<<  EzMatchRcpEditingCtrl.enterEdittingMode
        }
        void leaveEdittingMode()
        {
            _isRcpEdittingMode = false;
            update_cvi_boxes_to_recipe();
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
        
        void syncSegGroupsNumber()
        {
            if (_segGrpSettings == null)
                return;

            bool isChanged = update_group_boxes_to_gui();

            if (isChanged)
            {
                update_group_boxes_to_recipe();
                rebuildPropsView();
            }
        }
        void autoLayoutCviBoxes()
        {
            move_golden_box_to_default_location();

            if (_cviGroupBoundBoxes != null && _cviGroupBoundBoxes.Count > 0)
            {
                var rect = Rectangle.Round(_imgViewer.GetWorldRect());
                var H = rect.Height / _cviGroupBoundBoxes.Count;
                for (int i = 0; i < _cviGroupBoundBoxes.Count; i++)
                {
                    var cviSegBox = _cviGroupBoundBoxes[i];
                    cviSegBox.Box = new Rectangle(rect.X, rect.Y + (int)(i * H), rect.Width, (int)H);
                }
            }

            _imgViewerWindow?.Invalidate();
        }

        void rebuildPropsView()
        {
#if (OPT_RESERVED)
            var wnd = (_frmOwner as FormAwMain).OpDocker.Window ?? _frmOwner;
            var view = AppUtil.SearchGui<GwPanePropsViewer>(wnd, null) as IxPropsViewer;
            _segGrpSettings.AutoHidden();
            view?.BuildGuiCtrls(_activeRecipe);
#endif
        }
        void openSegDetailSettings()
        {
            using (var dlg = new FormSegOffsetSettings())
            {
                dlg.PitchY = (double)_activeRecipe.TrayDimSettings.PitchY.Value;
                dlg.SegsList = _segGrpSettings.SegsList;

                if (dlg.ShowDialog(_frmOwner) == DialogResult.OK)
                {
                    _segGrpSettings.SegsList = dlg.SegsList;
                    syncSegGroupsNumber();
                }
            }
        }

        #region CVI_BOX_FUNCTIONS
        void init_interactors()
        {
            _cviGroupBoundBoxes = new List<CviBoundBox>();
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
        void update_cvi_boxes_to_gui()
        {
            //var defaultSize = _imgSource != null ? _imgSource.Size : new Size(500, 500);
            //_cviBoundBox.Box = _matchSettings != null ?
            //                   _matchSettings.BoundBox.Value :
            //                   new Rectangle(0, 0, defaultSize.Width, defaultSize.Height);
            //if (_cviBoundBox.Box == Rectangle.Empty)
            //{
            //    _cviBoundBox.Box = new Rectangle(0, 0, defaultSize.Width, defaultSize.Height);
            //}

            update_group_boxes_to_gui();

            _cviGoldenBox.Box = _matchSettings != null ?
                                _matchSettings.GoldenBox.Value :
                                Rectangle.Empty;

            if (_cviGoldenBox.Box == Rectangle.Empty)
                move_golden_box_to_default_location();

            move_boxes_to_safe_location(_imgSource);
        }
        void update_cvi_boxes_to_recipe()
        {
            if (_matchSettings == null)
                return;

            _bypassJxEvents = true;

            var loc = _cviGoldenBox.Box;
            if (_matchSettings.GoldenBox.Value != loc)
                _matchSettings.GoldenBox.Value = loc;

            //loc = _cviBoundBox.Box;
            //if (_matchSettings.BoundBox.Value != loc)
            //    _matchSettings.BoundBox.Value = loc;

            _bypassJxEvents = false;

            update_group_boxes_to_recipe();
        }

        bool update_group_boxes_to_gui()
        {
            bool isNumberChanged = false;

            if (_segGrpSettings == null)
                return isNumberChanged;

            //int targetSegsNum = _segGrpSettings.SegsNumber.Value;
            if (!int.TryParse(_segGrpSettings.SegsNumber.Value, out int targetSegsNum))
                targetSegsNum = 1;

            var jxSegsList = _segGrpSettings?.SegsList;
            if (jxSegsList == null)
                return isNumberChanged;

            if (_imgViewerWindow != null)
                _imgViewerWindow.Enabled = false;

            // 根據需要 生成新的 CviBox
            for (int i = _cviGroupBoundBoxes.Count; i < targetSegsNum; i++)
            {
                var brush = i % 2 == 0 ? Brushes.Blue : Brushes.DarkBlue;
                var cviSegBox = new CviBoundBox(brush, 1, 3)
                {
                    Text = $"Seg{i}"
                };

                cviSegBox.OnChanged += (s, e) => update_group_boxes_to_recipe(s);
                _cviGroupBoundBoxes.Add(cviSegBox);
                _imgViewer?.AddInteractor(cviSegBox);

                isNumberChanged = true;
            }

            // 刪除/隱藏多餘的 CviBox
            if (targetSegsNum < _cviGroupBoundBoxes.Count)
            {
                for (int i = targetSegsNum; i < _cviGroupBoundBoxes.Count; i++)
                {
                    var cviSegBox = _cviGroupBoundBoxes[i];
                    cviSegBox.Visible = false;
                    cviSegBox.Enabled = false;
                    _imgViewer?.RemoveInteractor(cviSegBox);
                }
                _cviGroupBoundBoxes.RemoveRange(targetSegsNum, _cviGroupBoundBoxes.Count - targetSegsNum);
                isNumberChanged = true;
            }

            // 基準矩形
            var ccRect = _imgViewer != null ? Rectangle.Round(_imgViewer.GetWorldRect()) : new Rectangle(0, 0, 500, 500 * targetSegsNum);
            int H = ccRect.Height / Math.Max(targetSegsNum, 1);

            // 更新 CviBox
            for (int i = 0, N = _cviGroupBoundBoxes.Count; i < N; i++)
            {
                var rect = new Rectangle(ccRect.X, ccRect.Y + i * H, ccRect.Width, H - 50);

                if (i < jxSegsList.Count)
                {
                    var jx = jxSegsList[i];
                    if (jx != null)
                    {
                        if (jx.BoundBox.Value == Rectangle.Empty)
                            jx.BoundBox.Value = rect;
                        else
                            rect = jx.BoundBox.Value;
                    }
                }

                var cviSegBox = _cviGroupBoundBoxes[i];
                cviSegBox.Visible = _isRcpEdittingMode;
                cviSegBox.Enabled = _isRcpEdittingMode;
                cviSegBox.Box = rect;
            }

            if (_imgViewerWindow != null)
                _imgViewerWindow.Enabled = true;

            if (jxSegsList.Count != _cviGroupBoundBoxes.Count)
                isNumberChanged = true;

            return isNumberChanged;
        }
        void update_group_boxes_to_recipe(object sender = null)
        {
            if (_segGrpSettings == null || _cviGroupBoundBoxes == null)
                return;

            bool flag = _bypassJxEvents;
            _bypassJxEvents = true;

            //(1) 取得 pitchY
            decimal pitchY = _activeRecipe.TrayDimSettings.PitchY.Value;

            //(2) 根據 sender 來決定要更新哪一個 CviBox，還是全部更新
            int index = -1;
            if (sender is CviBoundBox cviSegBox)
                index = _cviGroupBoundBoxes.IndexOf(cviSegBox);
            int iStart = index >= 0 ? index : 0;
            int iEnd = index >= 0 ? index + 1 : _cviGroupBoundBoxes.Count;

            //(3) 更新 _segGrpSettings 的 JxTraySegItem
            for (int i = iStart; i < iEnd; i++)
            {
                _segGrpSettings.UpdateSegment(i, _cviGroupBoundBoxes[i].Box, pitchY);
            }

            // 刪除多餘的 JxTraySegItem
            int targetSegsNum = _cviGroupBoundBoxes.Count;
            _segGrpSettings.RemoveSegments(targetSegsNum);

            _bypassJxEvents = flag;
        }

        void move_boxes_to_safe_location(Size boundarySize)
        {
            var boxes = new List<CviBoundBox>(_cviGroupBoundBoxes)
            {
                _cviGoldenBox
            };

            foreach (var cviBox in boxes)
            {
                var rect = cviBox.Box;
                rect.Width = Math.Min(rect.Width, boundarySize.Width - 1);
                rect.Height = Math.Min(rect.Height, boundarySize.Height - 1);
                int x = Math.Max(0, rect.X);
                int y = Math.Max(0, rect.Y);
                if (rect.Right > boundarySize.Width)
                    x = boundarySize.Width - rect.Width;
                if (rect.Bottom > boundarySize.Height)
                    y = boundarySize.Height - rect.Height;
                rect.X = x;
                rect.Y = y;
                cviBox.Box = rect;
                //int x2 = Math.Min(boundarySize.Width, rect.Right);
                //int y2 = Math.Min(boundarySize.Height, rect.Bottom);
                //cviBox.Box = new Rectangle(x, y, x2 - x, y2 - y);
            }
        }
        void move_boxes_to_safe_location(IEzImage imgSrc)
        {
            if (imgSrc != null)
            {
                move_boxes_to_safe_location(imgSrc.Size);
            }
        }
        void move_golden_box_to_default_location()
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
            
            _model.ResetAndClear();         //@<<<  EzMatchRcpEditingCtrl.catch_golden_image

            // 閃綠色
            var oldBrush = _cviGoldenBox.BoxBrush;
            _cviGoldenBox.BoxBrush = Brushes.Lime;
            refresh(_imgViewerWindow);

            _bypassJxEvents = true;
            try
            {
                move_boxes_to_safe_location(_imgSource);
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

            getRecipeRowsCols(out int targetRows, out int targetCols);

            while (true)
            {
                (var err, var suggestRows, var suggestCols) = _model.BuildGoldenGridTemplate(SideID.A, _imgSource, targetRows, targetCols);

                if (err != ErrCodes.OK)
                {
                    var msg = QxNums.GetEnumDescription(err);
                    if (err == ErrCodes.GRID_ROWS_COLS_ARE_NOT_THE_SAME_AS_USER_INPUT)
                    {
                        msg += $"\n\r\n\r視覺辨識: rows={suggestRows} , cols={suggestCols}";
                        msg += $"\n\r\n\r";
                        msg += "\n\r是否自動更新參數設定的 (rows, cols)";
                        msg += "\n\r再自動重新抓取?";
                        var ret = MessageBox.Show(msg, _frmOwner.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (ret == DialogResult.Yes)
                        {
                            adjustRecipeRowsCols(suggestRows, suggestCols);
                            _wndRcpHostPanel.Invalidate();
                            continue;
                        }
                    }
                    else
                    {
                        MessageBox.Show(msg, _frmOwner.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                break;
            }
        }
        void getRecipeRowsCols(out int rows, out int cols)
        {
            var recipe = _recipesMgr?.ActiveRecipe as JxQcRecipe;
            var settings = recipe?.TrayDimSettings;
            rows = settings!=null ? settings.FullRows.Value : 0;
            cols = settings!=null ? settings.FullCols.Value : 0;
        }
        void adjustRecipeRowsCols(int rows, int cols)
        {
            var recipe = _recipesMgr?.ActiveRecipe as JxQcRecipe;
            var settings = recipe?.TrayDimSettings;
            if (settings != null)
            {
                settings.FullRows.Value = rows;
                settings.FullCols.Value = cols;
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
                //_cviBoundBox.Visible = _isRcpEdittingMode;
                //_cviBoundBox.Enabled = _isRcpEdittingMode;
                _cviGoldenBox.Visible = _isRcpEdittingMode;
                _cviGoldenBox.Enabled = _isRcpEdittingMode;
                _cviFiltersBox.Visible = _isRcpEdittingMode;
                _cviFiltersBox.Enabled = _isRcpEdittingMode;
                foreach(var cviSegBox in _cviGroupBoundBoxes)
                {
                    cviSegBox.Visible = _isRcpEdittingMode;
                    cviSegBox.Enabled = _isRcpEdittingMode;
                }
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
