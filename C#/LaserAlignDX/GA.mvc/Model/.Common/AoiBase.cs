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
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Model;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using LeTian.AoiLib;
using NeedleX.ProcessSpace;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Traveller106;


namespace LaserAlignDX.AoiModel
{
    public abstract class AoiBase
    {
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing;
        public event EventHandler<GaProgressEventArgs> OnAoiBegin;
        public event EventHandler<GaProgressEventArgs> OnAoiEnd;
        public event EventHandler<ProcessEventArgs> OnError;

        #region NLOG
        NLog.Logger _NLOG => LtDebug.LOG;
        #endregion

        #region GLOBAL_MESS
        protected RecipeFPIX3Class _xRecipe
        {
            get => RecipeFPIX3Class.Instance;
        }
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
        LotData _lotData = new LotData();
        string _fileBarcodeStr = string.Empty;
        #endregion

        public LotData LotData
        {
            get => _lotData;
            set => _lotData = value;
        }
        public string LotId
        {
            //get { return m_LotId; }
            //set { m_LotId = value; }
            get => _lotData.LotID;
            set => _lotData.LotID = value;
        }
        public string StripId
        {
            //get { return m_StripId; }
            //set { m_StripId = value; }
            get => _lotData.StripID;
            set => _lotData.StripID = value;
        }
        public string FileName
        {
            get { return GetLotFileName(LotId, ".txt"); }
        }
        public string FileBarcodeStr
        {
            get
            {
                return _fileBarcodeStr;
            }
            set
            {
                _fileBarcodeStr = value;
                markFileTimeTag();
            }
        }

        #region PRIVATE_PATH_FILE_FUNCTIONS
        DateTime _timeTag = DateTime.Now;
        /// <summary>
        /// 標定統一的存檔時間
        /// </summary>
        protected void markFileTimeTag()
        {
            _timeTag = DateTime.Now;
        }
        protected string GetLotFileName(string tag, string ext)
        {
            //m_FileName = $"{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
            return $"{tag}-{_timeTag:yyyyMMddHHmmss}{ext}";
        }
        protected string GetDebugBmpFileName()
        {
            return GetDebugImgSaveFileName("LineScanImage", StripId, LotId);
        }
        protected string GetDebugOrgBmpFileName()
        {
            return GetDebugImgSaveFileName("LineScanImageOrg", StripId, LotId);
        }
        protected string GetDebugImgSaveFileName(string subFolder, string stripID, string lotID, bool autoCreateDir = true)
        {
            //>>> m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";
            //>>> m_PicResultOrgPath = $"{INI.Instance.ResultImagePath}\\linescanImageOrg\\{DateTime.Now.ToString("yyyyMMdd")}\\{StripId}";

            string path = System.IO.Path.Combine(INI.Instance.ResultImagePath, subFolder, _timeTag.ToString("yyyyMMdd"), stripID);
            if (autoCreateDir && !System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string file = $"{lotID}-{_timeTag:yyyyMMddHHmmss}.jpg";
            return System.IO.Path.Combine(path, file);
        }
        protected string GetLogPath(string subFolder)
        {
            return System.IO.Path.Combine(Universal.LOG_IMG_PATH, _timeTag.ToString("yyyyMMdd"), subFolder);
        }
        #endregion

        #region EVENT_FUNCTIONS
        int _progressCount = 0;
        protected void fire_AoiBegin()
        {
            _progressCount = 0;
            if (OnAoiBegin != null)
            {
                int total = _xRecipe.xRegionCells.Count;
                OnAoiBegin?.Invoke(this, new GaProgressEventArgs(total, 0));
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
        protected void fire_AoiError(ErrCodes err, string message)
        {
            OnError?.Invoke(this, new ProcessEventArgs(message, err));
        }
        #endregion

        #region LOG_FUNCTIONS
        protected void _LOG_ERROR(Exception ex, string message)
        {
            _NLOG.Error(ex, message);
            GaUtil.LOG($"[異常] {ex.Message}", Color.Red);
        }
        #endregion

        public abstract void Run();

        /// <summary>
        /// 复位所有数据
        /// </summary>
        protected void ResetCellsResultData()
        {
            _xRecipe.xOutBlocs?.Clear();
            var cells = _xRecipe.xRegionCells;
            if (cells != null)
                foreach (var cell in cells)
                    cell?.Reset();
        }
    }
}
