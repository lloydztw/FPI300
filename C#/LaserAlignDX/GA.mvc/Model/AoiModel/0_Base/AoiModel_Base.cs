#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-08-01 開始重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Utils;
using LaserAlignDX.Model;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using INI = Traveller106.INI;

namespace LaserAlignDX.AoiModel
{
    public abstract class AoiModelBase : IDisposable
    {
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        public event EventHandler<GaProgressEventArgs> OnAoiBegin;
        public event EventHandler<GaProgressEventArgs> OnAoiEnd;
        public event EventHandler<ProcessEventArgs> OnError;
        public event EventHandler OnLotDataChanged;

        #region NLOG
        NLog.Logger _NLOG => LtDebug.LOG;
        #endregion

        #region GLOBAL_MESS
        protected RecipeFPIX3Class _xRecipe
        {
            get => RecipeFPIX3Class.Instance;
        }
        protected static RegionCellsDataCollection _regionCells;
        #endregion

        #region KENERL_MEMBERS
        /// <summary>
        /// 2025-08-28 LETIAN: 巨圖 統一由 TravellerBigImagesHolder 保管其生命週期
        /// </summary>
        public GaBigImageHolder LineScanCamImageHolder => _sysModel.LineScanImageHolder;
        /// <summary>
        /// SystemModel
        /// </summary>
        protected ITravelerModel _sysModel => GaMvcConfig.SysModel;
        /// <summary>
        /// 當下的載台
        /// </summary>
        protected CarrierEnum getActiveCarrierID()
        {
            return _sysModel.ActiveCarrierID;
        }
        #endregion

        #region PRIVATE_STATISTICS_DATA
        private long _elapsedTime = 0;
        private bool _isRunning = false;
        private bool _isPass = false;
        private Stopwatch _stopwatch = new Stopwatch();
        #endregion

        #region PROTECTED_STATISTICS_FUNCTIONS
        protected void markRunStart()
        {
            _elapsedTime = 0;
            _isRunning = true;
            _isPass = false;
            _stopwatch.Restart();
        }
        protected void markRunEnd(bool pass)
        {
            _stopwatch.Stop();
            _elapsedTime = _stopwatch.ElapsedMilliseconds;
            _isPass = pass;
            _isRunning = false;
        }
        #endregion

        public long ElapsedTime
        {
            get { return _elapsedTime; }
        }
        public bool Running
        {
            get { return _isRunning; }
        }
        public bool IsPass
        {
            get { return _isPass; }
        }

        #region LOT_DATA
        protected LotDataHolder _lotDataHolder => LotDataHolder.Instance;
        #endregion

        public LotData LotData
        {
            get => _lotDataHolder.LotData;
            set
            {
                _lotDataHolder.LotData = value;
            }
        }
        public string LotId
        {
            get => _lotDataHolder.LotData.LotID;
            set
            {
                _lotDataHolder.LotData.LotID = value;
                OnLotDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string StripId
        {
            get => _lotDataHolder.LotData.StripID;
            set => _lotDataHolder.LotData.StripID = value;
        }
        public string FileName
        {
            get => _lotDataHolder.GetLotFileName(LotId, ".txt");
        }
        public string FileBarcodeStr
        {
            get
            {
                return _lotDataHolder.FileBarcodeStr;
            }
            set
            {
                //_fileBarcodeStr = value;
                //markFileTimeTag();
                _lotDataHolder.FileBarcodeStr = value;
            }
        }

        #region PRIVATE_PATH_FILE_FUNCTIONS

        /// <summary>
        /// 標定統一的存檔時間
        /// </summary>
        protected void markFileTimeTag()
        {
            //_timeTag = DateTime.Now;
            _lotDataHolder?.MarkFileTimeTag();
        }

#if (OPT_LEGACY || true)
        DateTime _timeTag => _lotDataHolder.TimeTag;
        
        /// <summary>
        /// 帶日期時間尾綴的檔名
        /// </summary>
        protected string GetLotFileName(string tag, string ext)
        {
            //m_FileName = $"{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
            //return $"{tag}-{_timeTag:yyyyMMdd_HHmmss}{ext}";
            return _lotDataHolder.GetLotFileName(tag, ext);
        }

        /// <summary>
        /// 根據 (StripId, LotId) 生成檔名 於 子資料夾 "LineScanImage"
        /// </summary>
        protected string GetDebugBmpFileName(bool pass)
        {
            string folder = "LineScanImage";
            if(INI.Instance.UseOkNgDiffImageFolders)
            {
                if (!pass) folder += ".NG";
            }
            return GetDebugImgSaveFileName(folder, StripId, LotId);
        }

        /// <summary>
        /// 根據 (StripId, LotId) 生成檔名 於 子資料夾 "LineScanImageOrg"
        /// </summary>
        protected string GetDebugOrgBmpFileName(bool pass)
        {
            string folder = "LineScanImageOrg";
            if (INI.Instance.UseOkNgDiffImageFolders)
            {
                if (!pass) folder += ".NG";
            }
            return GetDebugImgSaveFileName(folder, StripId, LotId);
        }

        /// <summary>
        /// 檔案 INI.Instance.ResultImagePath
        ///         \ subFolder
        ///         \ yyyyMMdd
        ///         \ stripID
        ///         \ lotID-yyyyMMdd_HHmmss.jpg
        /// </summary>
        protected string GetDebugImgSaveFileName(string subFolder, string stripID, string lotID, bool autoCreateDir = true)
        {
            //string path = System.IO.Path.Combine(INI.Instance.ResultImagePath, subFolder, _timeTag.ToString("yyyyMMdd"), stripID);
            string path = _lotDataHolder.GetLogPath(lotID);
            if (autoCreateDir && !System.IO.Directory.Exists(path))
            {
                // 改用 JetEazy.IO.QxPathUtility.InitDirectory 可以 遞迴深層 創建資料夾.
                // System.IO.Directory.CreateDirectory(path);
                JetEazy.IO.QxPathUtility.InitDirectory(path);
            }
            string file = $"{lotID}-{_timeTag:yyyyMMdd_HHmmss}.jpg";
            return System.IO.Path.Combine(path, file);
        }
#endif

        /// <summary>
        /// 指向 [LOG_ROOT]\\Images\\[yyyyMMdd] 資料夾
        /// </summary>
        protected string GetLogPath(string subFolder)
        {
            // return System.IO.Path.Combine(Universal.LOG_IMG_PATH, _timeTag.ToString("yyyyMMdd"), subFolder);
            return _lotDataHolder.GetLogPath(subFolder);
        }
        #endregion

        #region EVENT_FUNCTIONS
        int _progressCount = 0;
        protected void fire_AoiBegin(string message = null)
        {
            _progressCount = 0;
            if (OnAoiBegin != null)
            {
                int total = _xRecipe.xRegionCells.Count;
                OnAoiBegin?.Invoke(this, new GaProgressEventArgs(total, 0, message));
            }
        }
        protected void fire_AoiEnd()
        {
            if (OnAoiEnd != null)
            {
                int total = _xRecipe.xRegionCells.Count;
                OnAoiEnd?.Invoke(this, new GaProgressEventArgs(total, total));
            }
        }
        protected void fire_AoiProgressing(RegionCellX3Class cell)
        {
            //int currentStep = cell.CellCol + cell.CellRow * xRecipe.xColumn;
            //if (currentStep <= _progressCount)
            //    return;
            //_progressCount = currentStep;

            Interlocked.Increment(ref _progressCount);
            int currentStep = _progressCount;

            if (OnAoiProgressing != null)
            {
                int total = _xRecipe.xRegionCells.Count;
                OnAoiProgressing?.Invoke(this, new GaProgressEventArgs(total, currentStep));
            }
        }
        protected void fire_AoiProgressing(object sender, GaProgressEventArgs e)
        {
            OnAoiProgressing?.Invoke(sender, e);
        }
        protected bool fire_AoiError(ErrorCodes err, string message, bool cancel = true)
        {
            var ev = new ProcessEventArgs(message, err) { Cancel = cancel };
            OnError?.Invoke(this, ev);
            return ev.Cancel;
        }
        protected void fire_AoiError(object sender, ProcessEventArgs e)
        {
            OnError?.Invoke(sender, e);
        }
        #endregion

        #region LOG_FUNCTIONS
        protected void _LOG_ERROR(Exception ex, string message)
        {
            _NLOG.Error(ex, message);
            GaUtil.LOG($"[Error] {ex.Message}", Color.Red);
        }
        #endregion

        public virtual void Dispose()
        {
            _regionCells?.Dispose();
            _regionCells = null;
        }

        /// <summary>
        /// 訓練 AOI (目前是為了 MVD套件)
        /// </summary>
        public virtual bool Train(Bitmap goldenBmp, params object[] args)
        {
            return false;
        }

        /// <summary>
        /// 執行 AOI 運算
        /// </summary>
        /// <remarks>
        /// 默認 sceneBmp = null, 代表使用 GaBigImageHolder 當輸入影像
        /// </remarks>
        public abstract void Run(Bitmap sceneBmp = null);

        /// <summary>
        /// 复位所有数据
        /// </summary>
        protected void ResetCellsResultData()
        {
            //---------------------------------------------------------------------------------------------
            // 2026-04-04 重構前的程式碼，已改為使用 RegionCellsDataCollection 來管理 _xRecipe.xRegionCells
            //---------------------------------------------------------------------------------------------
            // _xRecipe.xOutBlocs?.Clear();
            // foreach (var cell in _xRecipe.xRegionCells)
            //    cell?.Reset();

            //---------------------------------------------------------------------------------------------
            // 2026-04-04
            // 使用 RegionCellsDataCollection 來管理 _xRecipe.xRegionCells
            // 以維持 PASS/NG 統計數量的一致性 !
            //---------------------------------------------------------------------------------------------
            _regionCells?.Dispose();
            _regionCells = new RegionCellsDataCollection(_xRecipe.xRegionCells);
            _regionCells.Reset();

            // 自動設定 Extend
            if (true)
            {
                var c00 = _regionCells.GetGridCell(0, 0);
                var c11 = _regionCells.GetGridCell(1, 1);
                if (c00 != null && c11 != null)
                {
                    var pitchX = Math.Abs(c00.viewRectF.X - c11.viewRectF.X);
                    var pitchY = Math.Abs(c00.viewRectF.Y - c11.viewRectF.Y);
                    var extendX = (2f * pitchX - c00.viewRectF.Width) / 2f * 1.025f;
                    var extendY = (2f * pitchY - c00.viewRectF.Height) / 2f * 1.025f;
                    if (extendX > 0) _xRecipe.xExtendx = (int)extendX;
                    if (extendY > 0) _xRecipe.xExtendy = (int)extendY;
                }
            }
        }

        protected string GetDeepExceptionMessage(Exception ex)
        {
            var msg = "";
            while(ex != null)
            {
                msg += ex.Message + "\n\r";
                ex = ex.InnerException;
            }
            return msg;
        }

        protected void HandleAoiException(Exception ex)
        {
            markRunEnd(false);
            fire_AoiEnd();

            var errCode = ErrorCodes.EXCEPTION_AT_AOI_RUN;
            string errMsg = GaUtil.GetEnumDescription(errCode)
                            + "\n\r\n\r" + GetType().Name
                            + "\n\r\n\r" + GetDeepExceptionMessage(ex);
            GaUtil.LOG(errMsg, Color.Red);

            _LOG_ERROR(ex, $"Error @ {GetType().Name}.Run");
            fire_AoiError(errCode, errMsg);
        }
    }
}
