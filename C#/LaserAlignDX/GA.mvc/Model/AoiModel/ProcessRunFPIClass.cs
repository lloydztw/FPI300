using System;
using VisionDesigner;

namespace LaserAlignDX.AoiModel
{
    using IMPLEMENT = V2.ProcessRunFPIClass;

    public class ProcessRunFPIClass : IProcessRunFPI
    {
        #region PRIVATE_DATA
        static ProcessRunFPIClass _instance;
        IProcessRunFPI _imp;
        #endregion

        #region SINGLETON
        protected ProcessRunFPIClass()
        {
            _imp = IMPLEMENT.Instance;
        }
        #endregion

        public static ProcessRunFPIClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProcessRunFPIClass();
                return _instance;
            }
        }
        public static void DisposeAll()
        {
            ((IDisposable)_instance)?.Dispose();
            _instance = null;
        }
        void IDisposable.Dispose()
        {
            _imp?.Dispose();
        }

        public event EventHandler<GaProgressEventArgs> OnAoiBegin
        {
            add
            {
                _imp.OnAoiBegin += value;
            }

            remove
            {
                _imp.OnAoiBegin -= value;
            }
        }
        public event EventHandler<GaProgressEventArgs> OnAoiEnd
        {
            add
            {
                _imp.OnAoiEnd += value;
            }

            remove
            {
                _imp.OnAoiEnd -= value;
            }
        }
        public event EventHandler<GaProgressEventArgs> OnAoiProgressing
        {
            add
            {
                _imp.OnAoiProgressing += value;
            }

            remove
            {
                _imp.OnAoiProgressing -= value;
            }
        }

        public bool IsPass => _imp.IsPass;
        public long ElapsedTime => _imp.ElapsedTime;
        public string FileBarcodeStr { get => _imp.FileBarcodeStr; set => _imp.FileBarcodeStr = value; }
        public string LotId { get => _imp.LotId; set => _imp.LotId = value; }
        public bool QrJudged { get => _imp.QrJudged; set => _imp.QrJudged = value; }
        public bool QrUsed { get => _imp.QrUsed; set => _imp.QrUsed = value; }
        public string ResultDesc => _imp.ResultDesc;
        public bool Running => _imp.Running;
        public string StripId { get => _imp.StripId; set => _imp.StripId = value; }
        public string FileName { get => _imp.FileName; set => _imp.FileName = value; }

        /// <summary>
        /// 2025-08-28 LETIAN: 巨圖 統一由 LineScanCamImageHolder 保管其生命週期
        /// </summary>
        public GaBigImageHolder LineScanCamImageHolder
        {
            get => _imp.LineScanCamImageHolder;
        }
        public ScanInspectMode xScanInspectMode
        {
            get => _imp.xScanInspectMode;
            set => _imp.xScanInspectMode = value;
        }

        public int[] GetQrResult()
        {
            return _imp.GetQrResult();
        }
        public float[] GetScanOffset()
        {
            return _imp.GetScanOffset();
        }
        public int[] GetSingleResult()
        {
            return _imp.GetSingleResult();
        }

        public void Run()
        {
            _imp.Run();
        }
    }
}