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
using EzAoiEmptyTrayInspector.Gui;
using EzAoiEmptyTrayInspector.Model;
using JetEazy;
using JetEazy.EzImage;
using JetEazy.ImageViewerEx;
using LeTian.JxProps;
using LeTian.JxRecipesTool.Ctrl;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using GvImageViewerClassT = JetEazy.OpenCV.Viewer.CvMatViewer;

namespace EzAoiEmptyTrayInspector.Ctrl
{
    internal class EzMatchCtrl : BaseUtil, IDisposable
    {
        public event EventHandler OnInitDone;

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
        NLog.ILogger LOG => base._LOG;
        #endregion

        #region GLOBAL_DATA
        IxEmptyTrayInspector _model => Global.AoiModel;
        JxAppSettings _appSettings => Global.AppSettings;
        #endregion

        #region STATIC_DATA
        //static List<EzMatchCtrl> _instances = new List<EzMatchCtrl>();
        #endregion

        #region DUMP_PATH_AND_OUTPUT_FILE_NAME
        string GET_DUMP_PATH(int id, string fileName)
        {
            if (_activeRecipe == null || !_activeRecipe.IsDebugDumpEnabled())
                return null;

            string rcpTag = _activeRecipe != null ? _activeRecipe.Name : $"#{id}";
            if (string.IsNullOrEmpty(fileName))
                return System.IO.Path.Combine(Global.APP_PATH.DumpPath, rcpTag);

            fileName = System.IO.Path.GetFileName(fileName);
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            return System.IO.Path.Combine(Global.APP_PATH.DumpPath, rcpTag, fileName);
        }
        string DUMP_PATH => GET_DUMP_PATH((int)ID, _imgSourceFile?.Value);
        string GET_OUTPUT_IMAGE_FILE_NAME(bool checkDir = true)
        {
            if (!_appSettings.OutputResultImageFile)
                return null;

            string path = _appSettings.OutputDataPath;
            if (string.IsNullOrEmpty(path))
            {
                path = Global.APP_PATH.DumpPath;
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
        JxVisionSource _visionSrc
        {
            get => _appSettings?.GetVisionSrc((int)ID);
        }
        JxPathFile _imgSourceFile
        {
            get => _visionSrc?.ImgFile;
        }
        string _lastPushName;
        bool _isInitDone = false;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        Form _frmOwner;
        Control _lblPassFail;
        IvSingleMatchView _view;
        IvFuncButtonsPanel _funcButtonsPanel;
        CviMatchResultBox _cviMatchResultBox;
        bool _bypassJxEvents = false;
        #endregion

        #region OTHER_CTRLS
        EzMatchRcpEdittingCtrl _rcpEditCtrl;
        #endregion

        public EzMatchCtrl(IvSingleMatchView view, IvFuncButtonsPanel funcButtonsPanel, Control lblPassFail, IRecipesMgrCtrl recipesMgr)
        {
            ID = SideID.A;

            //_TRACE($"{GetType().Name}(view={view.Window.Handle})");
            //_instances.Add(this);
            //System.Diagnostics.Debug.Assert(sideId == _instances.Count - 1);

            _view = view;
            _funcButtonsPanel = funcButtonsPanel;
            _lblPassFail = lblPassFail;

            _frmOwner = _view.Window.FindForm();
            _recipesMgr = recipesMgr;

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
                //_model.SetRecipe(_activeRecipe);
                LoadImage(_imgSourceFile?.Value);
                _rcpEditCtrl = new EzMatchRcpEdittingCtrl((int)ID, _view, _funcButtonsPanel, _recipesMgr);
                _rcpEditCtrl.AttachImageSource(_largeIMG);
            }));
        }
        public void Dispose()
        {
            auto_dump_large_image();
            _largeIMG?.Dispose();
            _largeIMG = null;
        }

        /// <summary>
        /// img 交給 EzMatchCtrl 負責生命週期
        /// </summary>
        public void SetImage(IEzImage img, string name)
        {
            if (img != _largeIMG && img != null)
            {
                var old = _largeIMG;
                _largeIMG = img;
                attach_image_to_viewer(_largeIMG);
                if (!System.IO.File.Exists(name))
                {
                    name = System.IO.Path.ChangeExtension(name, ".jpg");
                    name = System.IO.Path.Combine(Global.APP_PATH.DumpPath, name);
                }
                _lastPushName = name;
                update_image_file_name(name);
                old?.Dispose();
            }
        }

        #region EVENT_HANDLERS
        void init_event_handlers()
        {
            var view = _view;
            var frmOwner = view.Window.FindForm();

            #region GUI_EVENT_HANDLERS
            //frmOwner.Load += FrmOwner_Load;
            frmOwner.FormClosing += FrmOwner_FormClosing;
            frmOwner.FormClosed += FrmOwner_FormClosed;

            var btnOpenFile = _funcButtonsPanel?.btnOpenFile;
            if (btnOpenFile != null)
                btnOpenFile.Click += BtnOpenFile_Click;

            //var btnRunMatch = _funcButtonsPanel?.btnRunMatch;
            //if (btnRunMatch != null)
            //    btnRunMatch.Click += BtnRunMatch_Click;

            var btnResetClear = _funcButtonsPanel?.btnResetClear;
            if (btnResetClear != null)
                btnResetClear.Click += BtnClear_Click;

            //if (view.btnCombine != null)
            //{
            //    view.btnCombine.Visible = false;
            //    view.btnCombine.Click += BtnCombine_Click;
            //}

            var btnRunAll = _funcButtonsPanel?.btnRunAll;
            if (btnRunAll != null)
                btnRunAll.Click += BtnRunAll_Click;

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

            #region MODEL_EVENT_HANDLERS
            _model.OnStateChanged += _model_OnStateChanged;
            _model.OnMatched += _model_OnMatched;
            _model.OnFinalResulted += _model_OnFinalResulted;
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

        private void BtnOpenFile_Click(object sender, EventArgs e)
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
            update_pass_fail("空盤檢測", Color.Blue);
        }
        private void BtnRunAll_Click(object sender, EventArgs e)
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
                update_gui_status(ev);
        }
        private void _model_OnMatched(object sender, MatchResultEventArgs e)
        {
            // 過濾
            if (e == null || e.ID != ID)
                return;

            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((EventHandler<MatchResultEventArgs>)_model_OnMatched, sender, e);
            }
            else
            {
                update_matched_result(e);
            }
        }
        private void _model_OnFinalResulted(object sender, AoiResultEventArgs e)
        {
            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.BeginInvoke((EventHandler<AoiResultEventArgs>)_model_OnFinalResulted, sender, e);
            }
            else
            {
                update_final_result(e);
            }
        }
        #endregion

        #region PRIVATE_RUN_FUNCTIONS
        void _PROMPT_ERROR(ErrCodes err, bool clearAll = false)
        {
            //if (err == ErrCodes.HAS_BEEN_COMBINED)
            //{
            //    _PROMPT_RELOAD(clearAll);
            //}
            //else
            {
                var msg = QxNums.GetEnumDescription(err);
                MessageBox.Show(msg, _frmOwner.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void _PROMPT_RELOAD(bool clearAll = false)
        {
            //var ret = MessageBox.Show($"源圖檔 {SideID.A} 已經被畫上合併結果, 是否重新載入該原始檔案?",
            //                _frmOwner.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            //if (ret == DialogResult.Yes)
            //{
            //    LoadImage(_imgSourceFile.Value);

            //    if (clearAll)
            //    {
            //        foreach (var c in _instances)
            //        {
            //            c.ResetAndClear();
            //            refresh(c._view.ImageViewer as Control);
            //        }
            //    }
            //}
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

            _TRACE($"[載入影像檔] {fileName}");
            var oldCursorV = setCursor(_view?.Window, Cursors.WaitCursor);
            var oldCursorF = setCursor(_frmOwner, Cursors.WaitCursor);
            setEnable(_funcButtonsPanel?.Window, false);

            try
            {
                var tm0 = DateTime.Now;
                _largeIMG?.Dispose();
                _largeIMG = null;

                MirrorMode mirror = _sideSettings != null ? _sideSettings.Mirror.Value : MirrorMode.None;

                // 使用 ImageUtil 載入巨大圖檔
                //>>> _largeIMG = ImageUtil.LoadLargeImage(fileName, mirror);
                _largeIMG = await ImageUtil.LoadLargeImageAsync(fileName, mirror, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

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

                    update_image_file_name(fileName, 1000);
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
                setCursor(_frmOwner, oldCursorF);
                setCursor(_view?.Window, oldCursorV);
                setEnable(_funcButtonsPanel?.Window, true);
            }

            if (_largeIMG == null)
            {
                _TRACE("[載入影像] Failed!", isError: true);
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
            //>>> clearViewportsSyncOffset();

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

        bool _canMatch(bool prompt)
        {
            var err = _model.CanMatch(ID, _largeIMG);
            bool ok = (err == ErrCodes.OK);
            if (!ok && prompt)
                _PROMPT_ERROR(err);
            return ok;
        }
        bool _canRunAll(bool prompt)
        {
#if (OPT_DUAL)
            if (ID == SideID.A)
            {
                var ImgA = _instances[0]?._largeIMG;
                var ImgB = _instances[1]?._largeIMG;
                var err = _model.CanRunAll(ImgA, ImgB);
                bool ok = err == ErrCodes.OK;
                if (!ok && prompt)
                    _PROMPT_ERROR(err, true);
                return ok;
            }
            return false;
#endif
            var err = _model.CanRunAll(_largeIMG);
            bool ok = err == ErrCodes.OK;
            if (!ok && prompt)
                _PROMPT_ERROR(err, true);
            return ok;
        }

        void RunOneMatch()
        {
            if (!_canMatch(true))
                return;

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
                _model.RunMatch(ID, _largeIMG, DUMP_PATH);
                //_TRACE($"[更新GUI影像]");
                refresh(_view.ImageViewer);
            });
        }
        void RunAll()
        {
#if (OPT_DUAL_MATCH)
            if (ID != SideID.A)
                return;

            if (!_canRunAll(true))
                return;

            var ImgA = _instances[0]?._largeIMG;
            var ImgB = _instances[1]?._largeIMG;
            string outputFile = (_appSettings?.CreateCombinedFile) ? GET_COMBINED_FILE_NAME() : null;

            _model.RunAll(ImgA, ImgB, outputFile, wait: false);
#endif
            if (!_canRunAll(true))
                return;

            update_pass_fail("檢測中", Color.Yellow);

            // 如果 goldenBox 有移動, 則自動抓取最新 golden
            if (_rcpEditCtrl != null && _rcpEditCtrl.IsEditting)
            {
                _rcpEditCtrl.AutoCatchGolden();
            }

            string outputFile = (_appSettings?.OutputResultImageFile) ? GET_OUTPUT_IMAGE_FILE_NAME() : null;
            _model.RunAll(_largeIMG, null, outputFile, wait: false);
        }

#if (OPT_DUAL)
        void setupViewportsSyncOffset()
        {
            //if (_cviSyncBox != null && _instances.Count >= 2)
            //{
            //    var trf = _model.BuildDualTransform();
            //    if (trf != null && _cviSyncBox != null)
            //    {
            //        _cviSyncBox.SetOffset(_instances[1]._view.ImageViewer, trf.AveOffset);
            //    }
            //    else
            //    {
            //        _cviSyncBox.ClearOffsets();
            //    }
            //}
        }
        void clearViewportsSyncOffset()
        {
            _cviSyncBox?.ClearOffsets();
        }
#endif
        #endregion

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
                //bool allDone = true;

                //foreach (var ctrl in _instances)
                //    allDone &= ctrl._isInitDone;

                //if (allDone)
                //{
                //    OnInitDone?.Invoke(this, null);

                //    var ctrl0 = _instances[0];
                //    ctrl0.update_button_status();
                //}

                this.update_button_status();
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
        void update_image_file_name(string filename, int delay = 0)
        {
            if (delay > 0)
            {
                new Action<string, int>((f, d) =>
                {
                    Thread.Sleep(d);
                    mark_init_done();
                    update_image_file_name(f, 0);
                }).BeginInvoke(filename, delay, null, null);
                //ThreadPool.QueueUserWorkItem((arg)=>
                //{
                //    Thread.Sleep((int)arg);
                //    mark_init_done();
                //    update_image_file_name(filename, 0);
                //}, delay);
                return;
            }

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke(new Action<string>((fname) =>
                {
                    update_image_file_name(fname, 0);
                }),
                    filename
                );
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
                if (bits > 0)
                {
                    int w = _largeIMG.Width;
                    int h = _largeIMG.Height;
                    srcName += $" ({bits} bit, {w} x {h})";
                    LOG.Info($"[{ID}] srcName= {srcName}");
                }

                _view?.UpdateImageSrcName(srcName);

                _bypassJxEvents = false;
            }
        }
        #endregion

        #region PRIVATE_GUI_FUNCTIONS
        void update_gui_status(MatchStateEventArgs e)
        {
            if (_frmOwner == null)
                return;

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action<MatchStateEventArgs>)update_gui_status, e);
            }
            else
            {
                update_button_status();
                _view?.UpdateMatchState(e?.State);
            }
        }
        void update_button_status(int delay = 0)
        {
            if (_frmOwner == null)
                return;

            #region USE_THREAD_POOL
            if (delay > 0)
            {
                ThreadPool.QueueUserWorkItem((arg) =>
                {
                    Thread.Sleep((int)arg);
                    update_button_status(0);
                }, delay);
                return;
            }
            #endregion

            if (_frmOwner.InvokeRequired)
            {
                _frmOwner.Invoke((Action<int>)update_button_status, 0);
            }
            else
            {
                var view = _view;
                if (view == null)
                    return;

                bool isReady = _model != null ? _model.IsReady() : false;

                //enable(view.btnOpenFile, isReady);
                //enable(view.btnRunMatch, isReady);
                //enable(view.btnResetClear, true);
                //if (ID == SideID.A)
                //{
                //    //enable(view.btnCombine, isReady);
                //    //view.btnCombine.Visible = _canCombine(false);
                //    enable(_btnRunAll, isReady);
                //}

                setEnable(_funcButtonsPanel?.btnRunAll, isReady);
                setEnable(_funcButtonsPanel?.btnOpenFile, isReady);
                setEnable(_funcButtonsPanel?.btnSnapshot, isReady);
                setEnable(_funcButtonsPanel?.btnResetClear, true);

                // OpModesPanel
                var frmAwMain = _frmOwner as FormAwMain;
                var opModesPanel = frmAwMain.GetOpModeButton("Production")?.Parent;
                setEnable(opModesPanel, isReady);
            }
        }
        void update_matched_result(MatchResultEventArgs e)
        {
            if (e == null)
                return;

            bool isClear = e.IsResetting() || e.Result == null;

            if (!isClear)
                LOG.Info("[{0}] {1}", ID, e.Result);

            _cviMatchResultBox.UpdateResult(e?.Result);
            _view.UpdateStatusInfo(e.Result?.ToString());
            _view.Window.Invalidate();

            if (isClear)
            {
                refresh(_view.ImageViewer);
                return;
            }

            //if (_model.AreAllSidesMatched())
            //{
            //    setupViewportsSyncOffset();
            //    _instances[0].update_button_status(1000);
            //}
            //else
            //{
            //    _instances[0].update_button_status();
            //}

            update_button_status();
        }
        void update_final_result(AoiResultEventArgs e, bool showMsgBox = false)
        {
#if (OPT_DUAL_MATCH)
            //refresh(_view.ImageViewer);
            //_instances[0].update_button_status();

            foreach (var ctrl in _instances)
                refresh(ctrl?._view?.ImageViewer);
#else
            refresh(_view.ImageViewer);
#endif

            // 更新 PASS / FAIL
            update_pass_fail(e);

            var result = e.Result;
            string msg;

            if (result != null)
            {
                msg = result.ToString();
                LOG.Info(msg);
                _view?.UpdateStatusInfo(msg);
                //>>> LOG.Info($"[{tag}] 總耗時 = {(int)(result.TotalSeconds * 1000)} ms");
            }
            else
            {
                msg = "無結果!";
            }

            if (showMsgBox)
            {
                var icon = result != null && result.IsPass() ? MessageBoxIcon.Information : MessageBoxIcon.Exclamation;
                MessageBox.Show(msg.Replace(",", "\n\r"), Global.TITLE, MessageBoxButtons.OK, icon);
            }

            // 3 秒後 狀態自動 顯示 Ready
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(3000);
                _view.UpdateMatchState("Ready");
            });
        }
        void update_pass_fail(AoiResultEventArgs e)
        {
            var result = e?.Result;
            if (result != null)
            {
                if (result.IsPass())
                    update_pass_fail("PASS", Color.Lime);
                else
                    update_pass_fail("FAIL", Color.Red);
            }
            else
            {
                update_pass_fail("無結果", Color.Red);
            }
        }
        void update_pass_fail(string msg, Color color)
        {
            if (_lblPassFail != null)
            {
                _lblPassFail.Text = msg;
                _lblPassFail.ForeColor = color;
                _lblPassFail.Refresh();
            }
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
        void auto_dump_large_image()
        {
            if (_largeIMG != null && _lastPushName != null)
            {
                try
                {
                    string fileName = _lastPushName;
                    if (!System.IO.File.Exists(fileName))
                    {
                        _largeIMG.Save(fileName);
                    }
                }
                catch
                {
                }
            }
        }
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
