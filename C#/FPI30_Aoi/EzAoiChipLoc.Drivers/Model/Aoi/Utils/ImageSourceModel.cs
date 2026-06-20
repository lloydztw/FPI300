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

using JetEazy.EzImage;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;


namespace EzEmptyTrayInspector.Model
{
    public partial class ImageSourceModel : IxCommonModel
    {
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

        public event EventHandler OnStateChanged;
        public event EventHandler OnSrcImageChanged;

        public ImageSourceModel(int id)
        {
            ID = id;
        }
        public void Dispose()
        {
            safe_dispose_large_image();
        }
        public int ID
        {
            get;
            private set;
        }

        #region PRIVATE_STATE_MEMBERS
        string _state = "Ready";
        void changeState(string state, bool force = false, int delay = 0)
        {
            if (state == null)
                state = "Unknown";

            if (_state != state || force)
            {
                _state = state;

                if (IsError())
                    _LOG.Error("[{0}] {1}", ID, _state);
                else
                    _LOG.Info("[{0}] {1}", ID, _state);

                if (delay <= 0)
                {
                    OnStateChanged?.Invoke(this, new MatchStateEventArgs((SideID)ID, _state));
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(delay);
                        OnStateChanged?.Invoke(this, new MatchStateEventArgs((SideID)ID, _state));
                    });
                }
            }
        }
        #endregion

        #region PUBLIC_STATE_FUNCTIONS
        public object State
        {
            get => _state;
        }
        public bool IsReady()
        {
            return _state.Contains("Ready");
        }
        public bool IsError()
        {
            return _state.Contains("Error");
        }
        public bool IsSafeToExit()
        {
            return IsReady() || IsError();
        }
        #endregion
    }


    partial class ImageSourceModel
    {
        #region PRIVATE_RUNTIME_DATA
        string _lastFileName;
        IEzImage _largeIMG;
        #endregion


        public string LastFileName
        {
            get => _lastFileName;
        }

        public IEzImage ImageSource
        {
            get => _largeIMG;
        }

        public async Task<IEzImage> LoadImageAsync(string fileName, MirrorMode mirrorMode = MirrorMode.None)
        {
            return await Task.Run(() => LoadImage(fileName, mirrorMode));
        }

        public IEzImage LoadImage(string fileName, MirrorMode mirrorMode = MirrorMode.None)
        {
            if (!IsSafeToExit())
                return null;

            if (fileName == null || !System.IO.File.Exists(fileName))
            {
                changeState($"[Error] 無檔案 {fileName}");
                return null;
            }

            try
            {
                changeState($"[載入影像檔] {fileName}");
                safe_dispose_large_image();
                var tm0 = DateTime.Now;

                // 使用 ImageUtil 載入巨大圖檔
                var img = ImageUtil.LoadLargeImage(fileName, mirrorMode);

                var ts = DateTime.Now - tm0;
                changeState($"[讀檔完成 {(int)ts.TotalMilliseconds} ms]");

                update_image_source(img, fileName);
                changeState("[Ready]");

                return img;
            }
            catch (Exception ex)
            {
                safe_dispose_large_image();
                update_image_source(null, "");
                changeState("[Error] 載入影像失敗!");
                throw ex;
            }
        }

        public IEzImage SetImage(IEzImage img, bool disposeOld = true)
        {
            if (!IsSafeToExit())
                return null;

            var old = _largeIMG;
            if (img == old)
                return old;

            if (disposeOld)
            {
                safe_dispose_large_image();
                old = null;
            }

            update_image_source(img, "[Memory]");
            return old;
        }

        public bool ApplyMirror(IEzImage img, MirrorMode mirrorMode)
        {
            if (!IsSafeToExit())
                return false;

            if (img == null)
                img = _largeIMG;

            bool isChanged = false;

            if (img != null && ImageUtil.GetMirrorTag(img) != mirrorMode)
            {
                changeState($"[影像鏡像] 處理中 ...");
                var tm0 = DateTime.Now;

                isChanged = ImageUtil.ApplyMirror(img, mirrorMode, markingDirtyPixel: true);

                var ts = DateTime.Now - tm0;
                changeState($"[影像鏡像完成 {(int)ts.TotalMilliseconds} ms]");

                changeState("Ready");
            }

            return isChanged;
        }

        public Bitmap CropBmp(IEzImage img, Rectangle cropRect)
        {
            //if (!IsSafeToExit())
            //    return null;

            if (img == null)
                img = _largeIMG;

            return ImageUtil.CropBmp(img, cropRect);
        }


        #region PRIVATE_FUNCTIONS
        void update_image_source(IEzImage img, string fileName)
        {
            if (_largeIMG != img || _lastFileName != fileName)
            {
                _largeIMG = img;
                _lastFileName = fileName;
                OnSrcImageChanged?.Invoke(this, null);
            }
        }
        void safe_dispose_large_image()
        {
            var old = _largeIMG;
            _largeIMG = null;
            try { old?.Dispose(); } catch { }
        }
        #endregion
    }
}
