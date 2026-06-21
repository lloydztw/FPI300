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

using EzDualMatch.Gui;
using EzDualMatch.Model;
using JetEazy.Image;
using JetEazy.ImageViewerEx;
using JetEazy.OpenCV.Viewer;
using LeTian.JxProps;
using LeTian.JxRecipesTool.Ctrl;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Interop;
using GvImageViewerClassT = JetEazy.OpenCV.Viewer.CvMatViewer;


namespace EzDualMatch.Ctrl.TryQ
{
    internal class EzMatchCtrl: IDisposable
    {
        public event EventHandler OnInitDone;

        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        protected NLog.ILogger LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region GLOBAL_DATA
        IxDualMatchModel _model => Global.Model;
        JxAppSettings _appSettings => Global.AppSettings;
        #endregion

        #region STATIC_DATA
        static List<EzMatchCtrl> _instances = new List<EzMatchCtrl>();
        #endregion

        #region DUMP_PATH
        string GET_DUMP_PATH(int id, string fileName)
        {
            if (!_activeRecipe?.Misc.DebugDump)
                return null;

            string rcpTag = _activeRecipe != null ? _activeRecipe.Name : $"#{id}";
            if (string.IsNullOrEmpty(fileName))
                return Global.AppPath("Work", $"Dump\\{rcpTag}");

            fileName = System.IO.Path.GetFileName(fileName);
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            return Global.AppPath("Work", $"Dump\\{rcpTag}\\{fileName}");
        }
        string DUMP_PATH => GET_DUMP_PATH((int)ID, _imgSourceFile?.Value);
        #endregion

        #region COMBINED_FILE_NAME
        string GET_COMBINED_FILE_NAME(bool checkDir = true)
        {
            if (!_appSettings.CreateCombinedFile)
                return null;
            
            string path = _appSettings.OutputDataPath;
            if (string.IsNullOrEmpty(path))
            {
                path = Global.AppPath("Work", "Dump");
                _appSettings.OutputDataPath.Value = path;
            }

            if (checkDir)
                JetEazy.IO.QxPathUtility.InitDirectory(path);

            var tm0 = DateTime.Now;
            string fileName = $"combine_{tm0:yyyyMMdd_HHmmss}.jpg";
            return System.IO.Path.Combine(path, fileName);
        }
        #endregion

        #region PRIVATE_RUNTIME_DATA
        IEzImage _largeIMG = null;
        IRecipesMgrCtrl _recipesMgr;
        JxDualMatchRecipe _activeRecipe => _recipesMgr?.ActiveRecipe as JxDualMatchRecipe;
        JxDualMatchSideSettings _sideSettings => _activeRecipe?.GetSiteSettings((int)ID);
        JxTempMatchSettings _matchSettings => _sideSettings?.Match;
        JxVisionSource _visionSrc => _appSettings?.GetVisionSrc((int)ID);
        JxPathFile _imgSourceFile => _visionSrc?.ImgFile;
        bool _isInitDone = false;
        bool _hasBeenCombined = false;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Form _frmOwner;
        Button _btnRunAll;
        IvSingleMatchView _view;
        CviMatchResultBox _cviMatchResultBox;
        EzMatchRcpEdittingCtrl _rcpEditCtrl;
        bool _bypassJxEvents = false;
        #endregion

        public EzMatchCtrl(int sideId, IvSingleMatchView view, IRecipesMgrCtrl recipesMgr, Button btnRunAll = null)
        {
            ID = (SideID)sideId;

            _TRACE($"{GetType().Name}(view={view.Window.Handle})");
            _instances.Add(this);
            System.Diagnostics.Debug.Assert(sideId == _instances.Count - 1);

            _view = view;
            _frmOwner = _view.Window.FindForm();
            _recipesMgr = recipesMgr;
            _btnRunAll = ID == 0 ? btnRunAll : null;

            setup_image_loader_for_quick_view_panel();
            init_interactors();
            init_event_handlers();
        }
        public SideID ID
        {
            get;
            private set;
        }
        public void PostInit()
        {
            _frmOwner.BeginInvoke(new Action(() =>
            {
                _frmOwner.Refresh();
                _model.SetRecipe(_activeRecipe);
                LoadImage(_imgSourceFile?.Value);
                _rcpEditCtrl = new EzMatchRcpEdittingCtrl((int)ID, _view, _recipesMgr);
                _rcpEditCtrl.AttachImageSource(_largeIMG);
            }));
        }
        public void Dispose()
        {
            _largeIMG?.Dispose();
            _largeIMG = null;
        }


        #region EVENT_HANDLERS
        void setup_image_loader_for_quick_view_panel()
        {
            var quickImageViewPanel = _view?.quickImageViewPanel;
            quickImageViewPanel?.SetExternalLoader((fileName) =>
            {
                LOG.Trace("[使用 ImageUtil 載入圖檔] {0}", fileName);
                MirrorMode mirror = _sideSettings != null ? _sideSettings.Mirror.Value : MirrorMode.None;
                _largeIMG = ImageUtil.LoadLargeImage(fileName, mirror);
                return _largeIMG;
            });
            quickImageViewPanel.OnImageSrcChanged += (s, e) => mark_init_done();
        }
        void init_event_handlers()
        {
            var view = _view;
            var frmOwner = view.frmOwner;

            #region GUI_EVENT_HANDLERS
            //frmOwner.Load += FrmOwner_Load;
            frmOwner.FormClosing += FrmOwner_FormClosing;
            frmOwner.FormClosed += FrmOwner_FormClosed;

            var btnOpen = view.btnOpen;
            if (btnOpen != null)
                btnOpen.Click += BtnOpen_Click;

            var btnRunMatch = view.btnRunMatch;
            if (btnRunMatch != null)
                btnRunMatch.Click += BtnRunMatch_Click;

            var btnResetClear = view.btnResetClear;
            if (btnResetClear != null)
                btnResetClear.Click += BtnClear_Click;

            if (view.btnCombine != null)
            {
                view.btnCombine.Visible = false;
                view.btnCombine.Click += BtnCombine_Click;
            }

            if(_btnRunAll!=null)
                _btnRunAll.Click += btnRunAll_Click;
            #endregion

            #region SYS_SETTINGS_EVENT_HANDLERS
            if (_visionSrc != null)
            {
                _visionSrc.ImgFile.OnModified += visionSrc_File_OnModified;
                //_visionSrc.Mirror.OnModified += visionSrc_Mirror_OnModified;
            }
            #endregion

            #region RECIPE_MGR_EVENT_HANDLERS
            _recipesMgr.OnRecipeSelectionChanged += _recipesMgr_OnRecipeSelectionChanged;
            _recipesMgr.OnRecipeBrowsing += _recipesMgr_OnRecipeBrowsing;
            _recipesMgr.OnRecipeEditting += _recipesMgr_OnRecipeEditting;
            connect_recipe_prop_handlers();
            #endregion

            #region MODEL_EVENT
            _model.OnStateChanged += _model_OnStateChanged;
            _model.OnMatched += _model_OnMatched;
            #endregion
        }
        void connect_recipe_prop_handlers()
        {
            if (_sideSettings != null)
            {
                _sideSettings.Mirror.OnModified += side_Mirror_OnModified;
            }
        }

        private void FrmOwner_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool ok = _model == null || _model.IsSafeToExit();
            if (!ok)
            {
                if (MessageBox.Show("系統忙碌中, 是否強制退出?", 
                                    Application.ProductName, 
                                    MessageBoxButtons.YesNo, 
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                    e.Cancel = true;
            }
        }
        private void FrmOwner_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            BrowseFile();
        }
        private void BtnRunMatch_Click(object sender, EventArgs e)
        {
            RunOneMatch();
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            ResetAndClear();
        }
        private void BtnCombine_Click(object sender, EventArgs e)
        {
            RunCombine();
        }
        private void btnRunAll_Click(object sender, EventArgs e)
        {
            RunAll();
        }

        private void visionSrc_File_OnModified(object sender, EventArgs e)
        {
            if (!_bypassJxEvents && _visionSrc != null)
            {
                LoadImage(_visionSrc?.ImgFile.Value);
            }
        }
        private void side_Mirror_OnModified(object sender, EventArgs e)
        {
            if (!_bypassJxEvents && _matchSettings != null)
            {
                _frmOwner.BeginInvoke(new Action(() => applyMirrorMode(true)));
            }
        }

        private void _recipesMgr_OnRecipeSelectionChanged(object sender, EventArgs e)
        {
            applyActiveRecipe();
        }
        private void _recipesMgr_OnRecipeEditting(object sender, EventArgs e)
        {
            // 由 EzMatchRcpEditCtrl 處理
        }
        private void _recipesMgr_OnRecipeBrowsing(object sender, EventArgs e)
        {
            // 由 EzMatchRcpEditCtrl 處理
        }

        private void _model_OnStateChanged(object sender, EventArgs e)
        {
            // 過濾
            var ev = (MatchStateEventArgs)e;
            if (ev != null && ev.ID != SideID.All && ev.ID != ID)
                return;

            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
                _frmOwner.Invoke((EventHandler)_model_OnStateChanged, sender, e);
            else
                update_model_state(ev);
        }
        private void _model_OnMatched(object sender, MatchResultEventArgs e)
        {
            // 過濾
            if (e == null || e.ID != ID)
                return;

            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
                _frmOwner.Invoke((EventHandler<MatchResultEventArgs>)_model_OnMatched, sender, e);
            else
                update_matched_result(e);
        }
        #endregion


        void _PROMPT_RELOAD(bool clearAll = false)
        {
            var ret = MessageBox.Show($"源圖檔 {SideID.A} 已經被畫上合併結果, 是否重新載入該原始檔案?",
                            _frmOwner.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (ret == DialogResult.Yes)
            {
                LoadImage(_imgSourceFile.Value);

                if (clearAll)
                {
                    foreach (var c in _instances)
                    {
                        c.ResetAndClear();
                        refresh(c._view.ImageViewer as Control);
                    }
                }
            }
        }
        void applyActiveRecipe()
        {
            _model.SetRecipe(_activeRecipe);
            connect_recipe_prop_handlers();
            _view.Window.Invalidate();
            _frmOwner.BeginInvoke(new Action(() => applyMirrorMode(true)));
        }
        void applyMirrorMode(bool updateGui)
        {
            bool isChanged;
            _bypassJxEvents = true;

            if (_largeIMG != null &&
                _sideSettings != null &&
                _sideSettings.Mirror != ImageUtil.GetMirrorTag(_largeIMG))
            {
                _TRACE($"[影像鏡像] 處理中 ...");
                var tm0 = DateTime.Now;
                isChanged = ImageUtil.ApplyMirror(_largeIMG, _sideSettings.Mirror, markingDirtyPixel: true);

                var ts = DateTime.Now - tm0;
                if (isChanged && updateGui)
                {
                    _TRACE($"[影像鏡像完成 ({(int)ts.TotalMilliseconds} ms)] 更新GUI影像中 ... ");
                    refresh(_view.ImageViewer);
                }

                ts = DateTime.Now - tm0;
                _TRACE($"[影像鏡像完成 {(int)ts.TotalMilliseconds} ms]");
            }

            _bypassJxEvents = false;
        }

        async void LoadImage(string fileName)
        {
            if (fileName == null || !System.IO.File.Exists(fileName))
            {
                //>>> MessageBox.Show("[檔案不存在] " + fileName);
                _TRACE($"Error : 無檔案 {fileName}", isError: true);
                mark_init_done();
                return;
            }
            
            _largeIMG = await _view?.quickImageViewPanel.LoadImageAsync(fileName);
            return;

            try
            {
                _TRACE($"[載入影像檔] {fileName}");
                _frmOwner.Cursor = Cursors.WaitCursor;
                
                var tm0 = DateTime.Now;
                _largeIMG?.Dispose();
                _largeIMG = null;

                MirrorMode mirror = _sideSettings != null ? _sideSettings.Mirror.Value : MirrorMode.None;

                // 使用 ImageUtil 載入巨大圖檔
                _largeIMG = await ImageUtil.LoadLargeImageAsync(fileName, mirror);
                //>>> _largeIMG = ImageUtil.LoadLargeImage(fileName, mirror);

                var ts = DateTime.Now - tm0;
                _TRACE($"[讀檔完成 {(int)ts.TotalMilliseconds} ms]");


                if (_largeIMG != null)
                {
                    //_TRACE($"[讀檔完成 {(int)ts.TotalMilliseconds} ms] 更新GUI影像中 ...");
                    //var tm1 = DateTime.Now;
                    attach_image_to_viewer(_largeIMG);

                    //var ts1 = DateTime.Now - tm1;
                    //_TRACE($"[更新GUI影像完成 {(int)ts1.TotalMilliseconds} ms]");

                    ts = DateTime.Now - tm0;
                    _TRACE($"[載入影像檔完成 {(int)ts.TotalMilliseconds} ms]");

                    _hasBeenCombined = false;
                    update_image_file_name_async(fileName, 1000);
                    ResetAndClear();                    
                }
            }
            catch (Exception ex)
            {
                _frmOwner.Cursor = Cursors.Default;
                _largeIMG?.Dispose();
                _largeIMG = null;
                attach_image_to_viewer(null);
                mark_init_done();
                MessageBox.Show($"[{ID}] {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            finally
            {
                _frmOwner.Cursor = Cursors.Default;
            }

            if (_largeIMG == null)
            {
                _TRACE("[載入影像] Failed!");
            }
        }
        void BrowseFile()
        {
            string fileName = _imgSourceFile?.Value;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Image";
                dlg.Filter = "JPG Files(*.jpg)|*.jpg|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png";
                dlg.FileName = "*.jpg";

                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(fileName);
                    }
                    catch
                    {

                    }
                }

                if (DialogResult.OK == dlg.ShowDialog())
                {
                    ResetAndClear();
                    fileName = dlg.FileName;
                }
                else
                {
                    fileName = null;
                }
            }
            if (fileName != null)
            {
                LoadImage(fileName);
            }
        }

        void ResetAndClear()
        {
            if (_model != null)
            {
                _model?.ResetAndClear((SideID)ID);
            }
            else
            {
                _frmOwner.Invoke(new Action(() =>
                {
                    update_matched_result(null);
                    _cviMatchResultBox.Reset();
                    refresh(_view.ImageViewer);
                }));
            }
        }
        void RunOneMatch()
        {
            if (_model == null || !_model.IsReady())
                return;

            if (_hasBeenCombined && ID == 0)
            {
                _PROMPT_RELOAD();
                return;
            }

            // 如果 goldenBox 有移動, 則自動抓取最新 golden
            if (_rcpEditCtrl != null && _rcpEditCtrl.IsEditting)
            {
                _rcpEditCtrl.AutoCatchGolden();
            }

            //------------------------------------------------------------------
            //NOTE: activeRecipe 內容 應該與 model 同步
            //------------------------------------------------------------------
            // if (_activeRecipe.Modified)
            //    _model.SetRecipe(_activeRecipe);
            //------------------------------------------------------------------
            //NOTE: model 會自動發出 Null Result Event 來驅動 GUI 清除顯示.
            //------------------------------------------------------------------
            //      此處可以不調用 ClearResult();
            //------------------------------------------------------------------

            ThreadPool.QueueUserWorkItem(delegate
            {
                _model.RunMatch((SideID)ID, _largeIMG, DUMP_PATH);
                //_TRACE($"[更新GUI影像]");
                refresh(_view.ImageViewer);
            });
        }
        void RunCombine()
        {
            if (ID != 0)
                return;

            if (_hasBeenCombined)
            {
                _PROMPT_RELOAD();
                return;
            }

            ThreadPool.QueueUserWorkItem(delegate { run_combine(); });
        }
        void run_combine()
        {
            if (ID != 0 && _hasBeenCombined)
                return;

            var ImgA = _instances[0]._largeIMG;
            var ImgB = _instances[1]._largeIMG;
            string outputFile = (_appSettings?.CreateCombinedFile) ? GET_COMBINED_FILE_NAME() : null;

            int count = _model.Combine(ImgA, ImgB, outputFile);

            _hasBeenCombined = count > 0;

            // _TRACE("[更新GUI影像]");
            refresh(_view.ImageViewer);

            //if (_appSettings?.CreateCombinedFile)
            //    create_combined_file(ImgA);
            _view?.UpdateMatchState("Ready");
        }

        void RunAll()
        {
            if (!canRunAll())
                return;

            if (_hasBeenCombined)
            {
                _PROMPT_RELOAD(true);
                return;
            }

            LOG.Info("[一鍵執行] 開始 ... ");

            foreach (var c in _instances)
            {
                c.ResetAndClear();
                refresh(c._view.ImageViewer);
            }

            ThreadPool.QueueUserWorkItem(delegate { run_all(); });
        }
        void run_all()
        { 
            // 初始化一個 CountdownEvent，計數器設置為 2，表示要等待兩個工作完成
            CountdownEvent countdownEvent = new CountdownEvent(2);
            try
            {
                var tm0 = DateTime.Now;

                for (int i = 0; i < 2; i++)
                {
                    // 使用 ThreadPool 啟動線呈
                    ThreadPool.QueueUserWorkItem((obj) =>
                    {
                        var idx = (int)obj;
                        var ctrl = _instances[idx];
                        _model.RunMatch((SideID)ctrl.ID, ctrl._largeIMG, ctrl.DUMP_PATH);
                        refresh(ctrl._view.ImageViewer);
                        countdownEvent.Signal(); // 完成後減少計數器
                    }, i);
                }

                // 等待所有工作完成
                bool ok = countdownEvent.Wait(1000 * 60);
                if (ok)
                {
                    run_combine();

                    var ts = DateTime.Now - tm0;
                    LOG.Info("[一鍵執行完成] 總耗時 {0} ms", (int)ts.TotalMilliseconds);
                }
                else
                {
                    LOG.Error("Match 逾時!");
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex);
            }
            finally
            {
                countdownEvent?.Dispose();
            }
        }
        bool canRunAll()
        {
            //if (_hasBeenCombined)
            //    return false;

            if (_btnRunAll == null)
                return false;

            if (_activeRecipe == null)
                return false;
            
            if (_instances.Count < 2)
                return false;

            var imgA = _instances[0]?._largeIMG?.Image as OpenCvSharp.Mat;
            var imgB = _instances[1]?._largeIMG?.Image as OpenCvSharp.Mat;
            if (imgA == null || imgB == null)
                return false;

            if (imgA.Width != imgB.Width || imgA.Height != imgB.Height)
                return false;

            return true;
        }


        #region INTERACTOR_FUNCTIONS
        void init_interactors()
        {
            _cviMatchResultBox = new CviMatchResultBox();
            if (_view.ImageViewer is IvImageViewer imgViewer)
            {
                imgViewer.AddInteractor(_cviMatchResultBox);
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        void mark_init_done()
        {
            if (!_isInitDone)
            {
                _isInitDone = true;
                bool allDone = true;
                
                foreach (var ctrl in _instances)
                    allDone &= ctrl._isInitDone;
                
                if (allDone)
                {
                    OnInitDone?.Invoke(this, null);

                    var ctrl0 = _instances[0];
                    ctrl0.update_button_status();
                }
            }
        }
        void attach_image_to_viewer(IEzImage img, bool resetViewport = false)
        {
            if (_view.ImageViewer is GvImageViewerClassT imgViewer && imgViewer != null)
            {
                imgViewer.SetSource(img);
            }
            _rcpEditCtrl.AttachImageSource(img);
        }
        void update_image_file_name_async(string filename, int delay)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                System.Threading.Thread.Sleep(delay);
                mark_init_done();
                update_image_file_name(filename);
            });
        }

        void update_image_file_name(string filename)
        {
            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke(new Action(() => update_image_file_name(filename)));
            }
            else
            {
                _bypassJxEvents = true;

                if (_imgSourceFile.Value != filename)
                    _imgSourceFile.Value = filename;

                string srcName = "";
                if (filename != null)
                {
                    srcName = System.IO.Path.GetFileName(filename);
                }

                int bits = ImageUtil.GetPixelBits(_largeIMG);
                if(bits > 0)
                {
                    srcName += $" ({bits}bit)";
                    LOG.Info($"[{ID}] srcName= {srcName}");
                }

                _view?.UpdateImageSrcName(srcName);

                _bypassJxEvents = false;
            }
        }
        void update_matched_result(MatchResultEventArgs e)
        {
            if (e != null && e.Result != null)
                LOG.Info("[{0}] {1}", ID, e.Result);

            _cviMatchResultBox.UpdateResult(e?.Result);
            _view.UpdateMatchResult(e);
            _view.Window.Invalidate();

            bool isReadyToCombine = (_model != null && _model.IsReadyToCombine());
            if (isReadyToCombine)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    System.Threading.Thread.Sleep(1000);
                    _instances[0].update_button_status();
                });
            }
            else
            {
                _instances[0].update_button_status();
            }
        }
        void update_model_state(MatchStateEventArgs e)
        {
            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action<MatchStateEventArgs>)update_model_state, e);
            }
            else
            {
                if (e != null)
                    LOG.Info(e);
                update_button_status();
                _view?.UpdateMatchState(_model?.State);
            }
        }
        void update_button_status()
        {
            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action)update_button_status);
            }
            else
            {
                var view = _view;
                if (view == null)
                    return;

                bool isReady = _model != null ? _model.IsReady() : false;
                enable(view.btnOpen, isReady);
                enable(view.btnRunMatch, isReady);
                enable(view.btnResetClear, true);
                enable(view.btnCombine, isReady);

                if (ID == SideID.A)
                {
                    view.btnCombine.Visible = (_model != null && _model.IsReadyToCombine() && !_hasBeenCombined);
                    enable(_btnRunAll, isReady && canRunAll());
                }
            }
        }
        void enable(Control c, bool enable)
        {
            if (c != null)
                c.Enabled = enable;
        }
        void refresh(object c)
        {
            if (c != null && c is Control wnd)
            {
                if (_frmOwner == null)
                    return;

                if (_frmOwner.InvokeRequired)
                {
                    _frmOwner.Invoke((Action)wnd.Refresh);
                }
                else
                {
                    _view.Window.Enabled = false;
                    wnd.Refresh();
                    _view.Window.Enabled = true;
                }
            }
        }
        #endregion

        #region TRACE_AND_LOG
        void _TRACE(string msg, bool isError = false)
        {
            if (isError)
                LOG.Error("[{0}] {1}", ID, msg);
            else
                LOG.Info("[{0}] {1}", ID, msg);

            _view?.UpdateMatchState(msg);
        }
        #endregion
    }
}
