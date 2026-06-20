#region AUTHOR
/*
 * 
 * Copyright (c) 2024-2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-12 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.EzImage;
using JetEazy.ImageViewerQx;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EzQuickImage = JetEazy.EzImage.EzQuickImage;

namespace JetEazy.OpenCV.Viewer.Develop
{
    using ImageViewerClassT = CvMatViewer;
    using Mat = OpenCvSharp.Mat;

    public partial class CvzQuickImageViewPanel : UserControl, IvImageContainer<Mat>
    {
        public event EventHandler OnImageSrcChanged;

        #region NLOG
        // NOTE:
        // 不要做靜態初始化，而是要等到窗體的Load事件時才初始化Logger物件，
        // 且保證該窗體是 【首個使用】NLog 的 Class !!!
        // 這是因為NLog是在首次被使用時，才載入配置文件的。
        static NLog.ILogger _logger = null;
        protected NLog.ILogger _LOG
        {
            get
            {
                if (_logger == null)
                    _logger = NLog.LogManager.GetCurrentClassLogger();
                return _logger;
            }
        }
        #endregion

        #region STATUS_MESSAGES
        enum Status : int
        {
            [Description("[Ready]")]
            Ready,
            [Description("[載入圖檔]")]
            Loading,
            [Description("[載入圖檔] 完成")]
            Loaded_OK,
            [Description("[載入圖檔] 失敗!")]
            Loaded_Failed,
        }
        #endregion

        #region PRIVATE_DATA
        string _lastImageFileName;
        string _srcName;
        bool _isDirty;
        #endregion

        #region PRIVATE_GUI_MEMBERS
        ToolTip toolTip = new ToolTip();
        bool _isTitleBarVisible = true;
        bool _isCoordInfoVisible = true;
        #endregion

        public CvzQuickImageViewPanel()
        {
            InitializeComponent();
            initGui();
        }

        #region PRIVATE_EVENT_HANDLERS
        void initGui()
        {
            cvMatViewer1.Attach(lblCoordInfo, lblBlinker);
            _updateTitleWidgetGrpSize();

            _isTitleBarVisible = panel1.Visible;
            _isCoordInfoVisible = panel2.Visible;

            if (!DesignMode)
            {
                btnOpen.Click += BtnOpen_Click;
                HandleCreated += Panel_HandleCreated;
            }
        }
        void Panel_HandleCreated(object sender, EventArgs e)
        {
            var frm = FindForm();
            if (frm != null)
            {
                frm.Load += (s2, e2) => loadIniAsync();
                frm.FormClosed += (s3, e3) => saveIni();
            }
        }
        async void BtnOpen_Click(object sender, EventArgs e)
        {
            var ret = await BrowseFile();
        }
        #endregion

        #region $$$ PUBLIC_GUI_LINKS $$$
        [Browsable(false)]
        public CvMatViewer MatViewer
        {
            get => cvMatViewer1;
        }

        [Browsable(false)]
        public ImageViewerClassT ImageViewer
        {
            get => cvMatViewer1;
        }

        [Browsable(false)]
        public Control TitleBar => panel1;

        [Browsable(false)]
        public Control StatusBar => panel2;
        #endregion

        #region $$$ PUBLIC_PROPERTIES $$$
        public int ViewID
        {
            get;
            set;
        }

        [Browsable(false)]
        public string ImageFileName
        {
            get => _lastImageFileName;
        }

        [Browsable(false)]
        public string SrcName
        {
            get => _srcName;
        }

        [Browsable(false)]
        public System.Drawing.Image Icon
        {
            get => picIcon.Image;
            set => picIcon.Image = value;
        }

        [Browsable(false)]
        public int SRC_UPDATE_DELAY = 2000;

        public bool OptAutoPersistLastFile
        {
            get;
            set;
        }

        public bool OptTitleBarVisible
        {
            get => _isTitleBarVisible;
            set
            {
                if (_isTitleBarVisible != value)
                {
                    _isTitleBarVisible = value;
                    _updatePanelsLayout();
                }
            }
        }

        public bool OptCoordInfoVisible
        {
            get => _isCoordInfoVisible;
            set
            {
                if (_isCoordInfoVisible != value)
                {
                    _isCoordInfoVisible = value;
                    _updatePanelsLayout();
                }
            }
        }
        #endregion

        #region $$$ PUBLIC_IvImageContainer $$$
        public Mat Image
        {
            get => cvMatViewer1.Image;
            set => Attach(value);
        }
        public bool IsImageOwner()
        {
            return cvMatViewer1.IsImageOwner();
        }
        public bool Attach(Mat src)
        {
            bool ok= cvMatViewer1.Attach(src);
            if (ok)
            {
                updateImageSrcName("[Image]");
            } 
            return ok;
        }
        public bool CopyFrom(Mat src)
        {
            bool ok = cvMatViewer1.CopyFrom(src);
            if (ok)
            {
                updateImageSrcName("[Image (copy)]");
            }
            return ok;
        }
        public bool LoadImage(string fileName)
        {
            var imgQ = _loadImage(fileName);
            return imgQ?.Image != null;
        }
        #endregion

        #region PRIVATE_LOAD_FUNCTIONS
        Func<string, IEzImage> _externLoadFunc;
        IEzImage _loadImage(string fileName, bool skipFirstMsg = false)
        {
            try
            {
                if (!skipFirstMsg)
                    updateStatusInfo(Status.Loading, fileName);

                var tm0 = DateTime.Now;

                IEzImage imgQ = cvMatViewer1.SetSource(null);
                var extLoad = _externLoadFunc;
                if (extLoad != null)
                {
                    imgQ?.Dispose();
                    imgQ = extLoad(fileName);
                }
                else
                {
                    if (imgQ == null)
                        imgQ = new EzQuickImage();
                    imgQ.Load(fileName);
                }
                Invoke(new Action<IEzImage>((im) => cvMatViewer1.SetSource(im)), imgQ);

                var ts = DateTime.Now - tm0;

                // 更新成員資料
                _lastImageFileName = fileName;
                _isDirty = true;

                // 更新狀態訊息
                bool ok = (imgQ?.Image != null);
                var status = ok ? Status.Loaded_OK : Status.Loaded_Failed;
                var msg = ok ? $"{(int)ts.TotalMilliseconds} ms" : null;
                updateStatusInfo(status, msg);

                if (ok)
                {
                    updateImageSrcName(_lastImageFileName, SRC_UPDATE_DELAY);
                }

                return imgQ;
            }
            catch (Exception ex)
            {
                _handleLoadFailed(ex, fileName);
                return null;
            }
        }
        private void _handleLoadFailed(Exception ex, string fileName)
        {
            if (InvokeRequired)
            {
                Invoke((Action<Exception, string>)_handleLoadFailed, ex, fileName);
            }
            else
            {
                MessageBox.Show(ex.Message, FindForm().Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                updateStatusInfo(Status.Loaded_Failed, fileName);
                if (_lastImageFileName != null)
                    updateImageSrcName(_lastImageFileName, SRC_UPDATE_DELAY);
            }
        }
        #endregion

        #region $$$ PUBLIC_LOAD_FUNCTIONS $$$
        public void SetExternalLoader(Func<string, IEzImage> loadFunc)
        {
            _externLoadFunc = loadFunc;
        }
        public async Task<IEzImage> LoadImageAsync(string fileName)
        {
            // 使用 Task.Run 將同步操作轉換為異步
            updateStatusInfo(Status.Loading, fileName);
            return await Task.Run(() => _loadImage(fileName, skipFirstMsg: true));
        }
        public async Task<IEzImage> BrowseFile(string lastFileName = null, string filter = null, string ext = null)
        {
            if (lastFileName == null)
                lastFileName = _lastImageFileName;

            if (filter == null)
                filter = "JPG Files(*.jpg)|*.jpg|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png|All Files(*.*)|*.*";

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Image";
                dlg.Filter = filter;

                if (ext != null)
                    dlg.FileName = "*" + ext;

                if (lastFileName != null)
                {
                    try
                    {
                        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(lastFileName);
                    }
                    catch
                    {
                    }
                }

                if (DialogResult.OK != dlg.ShowDialog())
                    return null;

                lastFileName = dlg.FileName;
            }

            if (lastFileName != null && System.IO.File.Exists(lastFileName))
            {
                var imgQ = await LoadImageAsync(lastFileName);
                return imgQ;
            }

            return null;
        }
        public IEzImage GetSource()
        {
            // ImageViewer 負責 IEzImage 的生命週期 !!! 
            return this.ImageViewer.GetSource();
        }
        #endregion

        #region $$$ PUBLIC_TITLE_WIDGET_FUNCTIONS $$$
        private void AddTitleWidget(Control wnd, int colId = -1, int width = -1)
        {
            if (wnd == null)
                return;

            int cols = tblTitleWidgetsGrp.ColumnCount;
            colId = colId < 0 ? cols : Math.Min(Math.Max(0, colId), cols);

            tblTitleWidgetsGrp.ColumnCount = cols + 1;
            this.tblTitleWidgetsGrp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblTitleWidgetsGrp.Controls.Add(wnd, colId, 0);
            _updateTitleWidgetGrpSize(width);
            //wnd.Dock = DockStyle.Fill;
        }
        public Button AddTitleWidgetButton(string text, int colId = -1, int width = -1)
        {
            Button btn = (Button)_cloneControl(btnOpen, false);
            int N = tblTitleWidgetsGrp.Controls.Count + 1;
            btn.Name = $"btnTitleWidget{N}";
            btn.Text = text;
            AddTitleWidget(btn, colId, width);
            return btn;
        }
        public Control RemoveTitleWidget(int colId)
        {
            if (colId < 0 || colId >= tblTitleWidgetsGrp.ColumnCount)
                return null;
            Control wnd = tblTitleWidgetsGrp.GetControlFromPosition(colId, 0);
            tblTitleWidgetsGrp.ColumnStyles.RemoveAt(colId);
            if (wnd != null)
                tblTitleWidgetsGrp.Controls.Remove(wnd);
            wnd.Dock = DockStyle.None;
            return wnd;
        }
        public void UpdateTitleBarText(string text, Color? color = null)
        {
            if (_isTitleBarVisible && lblTitle != null)
            {
                if (text == null)
                    text = _srcName;
                set_text(lblTitle, text, color);
            }
        }
        #endregion

        #region PRIVATE_LAYOUT_FUNCTIONS
        void _updatePanelsLayout()
        {
            panel1.Visible = _isTitleBarVisible;
            panel2.Visible = _isCoordInfoVisible;
        }
        void _updateTitleWidgetGrpSize(int adjWidth = -1)
        {
            int widthSum = 0;
            int cols = tblTitleWidgetsGrp.ColumnCount;
            for (int col = 0; col < cols; col++)
            {
                Control c = tblTitleWidgetsGrp.GetControlFromPosition(col, 0);
                if (c == null)
                    continue;
                
                if (adjWidth > 0)
                {
                    c.Dock = DockStyle.None;
                    c.Width = adjWidth;
                }

                if (true && c.Visible)
                {
                    widthSum += c.Width + 1;
                    c.Dock = DockStyle.Fill;
                }
            }
            tblTitleWidgetsGrp.Width = widthSum;
        }
        #endregion

        #region PRIVATE_GUI_CLONE_FUNCTIONS
        private Control _cloneControl(Control srcCtl, bool recursive)
        {
            var cloned = Activator.CreateInstance(srcCtl.GetType()) as Control;
            var binding = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo prop in srcCtl.GetType().GetProperties(binding))
            {
                if (_isClonable(prop))
                {
                    object val = prop.GetValue(srcCtl);
                    prop.SetValue(cloned, val, null);
                }
            }

            if (recursive)
            {
                foreach (Control ctl in srcCtl.Controls)
                {
                    cloned.Controls.Add(_cloneControl(ctl, recursive));
                }
            }

            return cloned;
        }
        private bool _isClonable(PropertyInfo prop)
        {
            var browsableAttr = prop.GetCustomAttribute(typeof(BrowsableAttribute), true) as BrowsableAttribute;
            var editorBrowsableAttr = prop.GetCustomAttribute(typeof(EditorBrowsableAttribute), true) as EditorBrowsableAttribute;
            return prop.CanWrite
                && (browsableAttr == null || browsableAttr.Browsable == true)
                && (editorBrowsableAttr == null || editorBrowsableAttr.State != EditorBrowsableState.Advanced);
        }
        #endregion

        #region PRIVATE_INI_FUNCTIONS
        string getIniFileName()
        {
            var fname = AppDomain.CurrentDomain.FriendlyName;
            fname = System.IO.Path.ChangeExtension(fname, "_last.ini");
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), fname);
        }
        string getSectionName()
        {
            string sectionName = "";
            Control c = this;
            while (c != null)
            {
                sectionName = c.Name + "_" + sectionName;
                c = c.Parent;
            }
            return sectionName.Trim('_');
        }
        async void loadIniAsync()
        {
            if (!OptAutoPersistLastFile)
                return;

            loadIni();

            if (string.IsNullOrEmpty(_lastImageFileName) ||
                !System.IO.File.Exists(_lastImageFileName))
                _lastImageFileName = null;

            if (_lastImageFileName != null)
            {
                //var frm = FindForm();
                //frm.Refresh();
                var imgQ = await LoadImageAsync(_lastImageFileName);
                _isDirty = imgQ?.Image != null;
            }

            _updateTitleWidgetGrpSize();
        }
        void loadIni()
        {
            if (!OptAutoPersistLastFile)
                return;

            string iniFileName = getIniFileName();
            string iniSectionName = getSectionName();
            JetEazy.Win32.Win32Ini.Load(ref _lastImageFileName, iniFileName, iniSectionName, "LastImageFileName");
            if (string.IsNullOrEmpty(_lastImageFileName) || !System.IO.File.Exists(_lastImageFileName))
                _lastImageFileName = null;
            _isDirty = false;
        }
        void saveIni()
        {
            if (!OptAutoPersistLastFile)
                return;

            if (_isDirty)
            {
                string iniFileName = getIniFileName();
                string iniSectionName = getSectionName();
                string lastFileName = _lastImageFileName != null ? _lastImageFileName : "";
                JetEazy.Win32.Win32Ini.Save(lastFileName, iniFileName, iniSectionName, "LastImageFileName");
                _isDirty = false;
            }
        }
        #endregion

        #region PRIVATE_STATUS_FUNCTIONS

        //Status _status = Status.Ready;
        //void changeState(Status status, string tag)
        //{
        //    if (_status != status || tag != null)
        //    {
        //        _status = status;
        //        updateGuiStatus(status);
        //        updateStatusInfo(status, tag);
        //    }
        //}

        void updateStatusInfo(Status status, string msg = null)
        {
            if (InvokeRequired)
            {
                Invoke((Action<Status, string>)updateStatusInfo, status, msg);
                return;
            }

            string stateTxt = JetEazy.QxNums.GetEnumDescription(status);
            if (msg != null)
                stateTxt = $"{stateTxt} {msg}";

            bool isError = status == Status.Loaded_Failed;
            _TRACE(stateTxt, isError);

            if (_isTitleBarVisible && lblTitle != null)
                set_text(lblTitle, stateTxt, isError ? Color.Red : Color.White);

            updateGuiStatus(status);
        }
        void updateImageSrcName(string fileName, int delay = 0)
        {
            //------------------------------------------------------
            // 此函式 updateImageSrcName
            // 只進行 update gui text
            // 但是不 LOG.
            //------------------------------------------------------

            if (!_isTitleBarVisible || lblTitle == null)
                return;

            if (delay > 0)
            {
                // 使用 background thread 來延遲更新
                new Action<string, int>((f, d) =>
                {
                    Thread.Sleep(delay);
                    updateImageSrcName(f, 0);
                }).BeginInvoke(fileName, delay, null, null);
                return;
            }

            if (InvokeRequired)
            {
                Invoke((Action<string, int>)updateImageSrcName, fileName, 0);
            }
            else
            {
                string pixelFmtTxt = "";
                Mat img = (Mat)Image;

                if (img != null)
                {
                    //>>> int bits = System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat);
                    int bits = img.Channels() * 8;
                    pixelFmtTxt = $"({bits} bits)";
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    set_text(lblTitle, _srcName = pixelFmtTxt, Color.White);
                    toolTip.SetToolTip(lblTitle, "");
                    toolTip.Hide(lblTitle);
                }
                else
                {
                    string fname = System.IO.Path.GetFileName(fileName);
                    set_text(lblTitle, _srcName = $"{fname} {pixelFmtTxt}", Color.White);
                    toolTip.SetToolTip(lblTitle, fileName);
                }

                notify_image_src_changed();
            }
        }
        void updateGuiStatus(Status status)
        {
            if (InvokeRequired)
            {
                Invoke((Action<Status>)updateGuiStatus, status);
            }
            else
            {
                bool isBusy = (status == Status.Loading);
                tblTitleWidgetsGrp.Enabled = !isBusy;
                FindForm().Cursor = isBusy ? Cursors.WaitCursor : Cursors.Default;
            }
        }
        void set_text(Control wnd, string text, Color? color = null)
        {
            if (wnd != null)
            {
                if (InvokeRequired)
                {
                    Invoke((Action<Control, string, Color?>)set_text, wnd, text, color);
                }
                else
                {
                    if (color != null)
                        wnd.ForeColor = color.Value;
                    wnd.Text = text;
                    wnd.Refresh();
                }
            }
        }
        void notify_image_src_changed()
        {
            OnImageSrcChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region TRACE
        void _TRACE(string msg, bool isError)
        {
            if (isError)
                _LOG.Error(msg);
            else
                _LOG.Info(msg);
        }
        #endregion
    }
}
