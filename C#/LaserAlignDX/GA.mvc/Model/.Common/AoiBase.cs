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
        RecipeFPIX3Class _xRecipe
        {
            get => RecipeFPIX3Class.Instance;
        }
        #endregion

        #region PRIVATE_STATISTICS_DATA
        private long m_ElapsedTime = 0;
        private bool m_Running = false;
        private bool m_IsPass = false;
        #endregion

        public long ElapsedTime
        {
            get { return m_ElapsedTime; }
        }
        public bool Running
        {
            get { return m_Running; }
        }
        public bool IsPass
        {
            get { return m_IsPass; }
        }

        public LotData LotData
        {
            get;
            set;
        } = new LotData();
        public string LotId
        {
            //get { return m_LotId; }
            //set { m_LotId = value; }
            get => LotData.LotID;
            set => LotData.LotID = value;
        }
        public string StripId
        {
            //get { return m_StripId; }
            //set { m_StripId = value; }
            get => LotData.StripID;
            set => LotData.StripID = value;
        }
        public string FileName
        {
            get { return GetLotFileName(LotId, ".txt"); }
        }
        public string FileBarcodeStr
        {
            get
            {
                return m_FileBarcodeStr;
            }
            set
            {
                m_FileBarcodeStr = value;
                MarkFileTimeTag();
            }
        }

        #region PRIVATE_PATH_FILE_FUNCTIONS
        string m_FileBarcodeStr = string.Empty;
        DateTime _timeTag = DateTime.Now;
        /// <summary>
        /// 標定統一的存檔時間
        /// </summary>
        void MarkFileTimeTag()
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
        void _LOG_ERROR(Exception ex, string message)
        {
            _NLOG.Error(ex, message);
            GaUtil.LOG($"[異常] {ex.Message}", Color.Red);
        }
        #endregion
    }
}
