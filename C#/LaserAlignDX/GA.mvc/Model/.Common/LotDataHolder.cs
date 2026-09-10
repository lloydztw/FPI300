#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-07-28 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QvMath;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.OPSpace;
using LeTian.AoiLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Traveller106;
using VisionDesigner;
using INI = Traveller106.INI;
using PlcFlyResultCode = LaserAlignDX.PlcResultCode;

namespace LaserAlignDX.Model
{
    public class LotDataHolder
    {
        public event EventHandler OnLotDataChanged;

        #region PRIVATE_STATIC_MEMBERS
        static string LOG_IMG_PATH => Universal.LOG_IMG_PATH;
        static INI _INI => INI.Instance;
        #endregion

        #region SINGLETON
        private static readonly Lazy<LotDataHolder> _instance
            = new Lazy<LotDataHolder>(() => new LotDataHolder());
        protected LotDataHolder()
        {
            _ftpUploader.OnLog = msg => GaUtil.LOG(msg);
        }
        #endregion

        public static LotDataHolder Instance => _instance.Value;

        #region LOT_DATA
        LotData _lotData = new LotData();
        string _fileBarcodeStr = string.Empty;
        #endregion

        #region FTP_UPLOADER
        readonly FtpUploader _ftpUploader = new FtpUploader(_INI.FtpSettings);
        #endregion

        public LotData LotData
        {
            get => _lotData;
            set => _lotData = value;
        }
        public string LotId
        {
            get => _lotData.LotID;
            set
            {
                _lotData.LotID = value;
                OnLotDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public string StripId
        {
            get => _lotData.StripID;
            set => _lotData.StripID = value;
        }
        public string FileName => GetLotFileName(LotId, ".txt");
        public string FileBarcodeStr
        {
            get => _fileBarcodeStr;
            set
            {
                _fileBarcodeStr = value;
                MarkFileTimeTag();
            }
        }

        #region PRIVATE_PATH_FILE_FUNCTIONS
        DateTime _timeTag = DateTime.Now;

        internal DateTime TimeTag => _timeTag;

        internal string GetLotFileName(string tag, string ext)
        {
            return $"{tag}-{_timeTag:yyyyMMdd_HHmmss}{ext}";
        }

        protected string GetDebugBmpFileName(bool pass, DateTime timeTag)
        {
            string folder = "LineScanImage";
            if (_INI.UseOkNgDiffImageFolders && !pass)
            {
                folder += ".NG";
            }
            return GetDebugImgSaveFileName(folder, StripId, LotId, timeTag);
        }

        protected string GetDebugOrgBmpFileName(bool pass, DateTime timeTag)
        {
            string folder = "LineScanImageOrg";
            if (_INI.UseOkNgDiffImageFolders && !pass)
            {
                folder += ".NG";
            }
            return GetDebugImgSaveFileName(folder, StripId, LotId, timeTag);
        }

        protected string GetDebugImgSaveFileName(string subFolder, string stripID, string lotID, DateTime timeTag, bool autoCreateDir = true)
        {
            string path = System.IO.Path.Combine(_INI.ResultImagePath, subFolder, timeTag.ToString("yyyyMMdd"), stripID);
            if (autoCreateDir && !System.IO.Directory.Exists(path))
            {
                JetEazy.IO.QxPathUtility.InitDirectory(path);
            }

            string file = $"{lotID}-{timeTag:yyyyMMdd_HHmmss}.jpg";
            return System.IO.Path.Combine(path, file);
        }

        internal string GetLogPath(string subFolder)
        {
            if (string.IsNullOrEmpty(subFolder))
                subFolder = "Dump";
            var timeTag = _timeTag;
            return System.IO.Path.Combine(LOG_IMG_PATH, timeTag.ToString("yyyyMMdd"), subFolder);
        }
        #endregion

        public void MarkFileTimeTag()
        {
            _timeTag = DateTime.Now;
        }

        /// <summary>
        /// 非同步保存 原圖 (線程安全防鎖死版)
        /// </summary>
        public void AsyncSaveOrgImage(Bitmap bmpFullfov, bool pass)
        {
            if (!_INI.IsSaveDebugOrgBmp && !_INI.IsSaveDebugBmp)
                return;

            if (bmpFullfov == null)
                return;

            // 1. 安全複製 Bitmap 防止 GDI+ 競態
            Bitmap clonedBmp;
            lock (bmpFullfov)
            {
                clonedBmp = (Bitmap)bmpFullfov.Clone();
            }

            // 2. 凍結當前的時間戳，傳給背景 Thread
            DateTime taskTimeTag = _timeTag;

            var args = new object[]
            {
                clonedBmp,
                pass,
                taskTimeTag
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    var argvs = (object[])argv;
                    var cPass = (bool)argvs[1];
                    var cTimeTag = (DateTime)argvs[2];

                    string fileNameOrg = null;

                    using (Bitmap bmpBig = (Bitmap)argvs[0])
                    {
                        if (_INI.IsSaveDebugBmp)
                        {
                            var fileName = GetDebugBmpFileName(cPass, cTimeTag);
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, _INI.ImageQuality);
                        }

                        if (_INI.IsSaveDebugOrgBmp)
                        {
                            fileNameOrg = GetDebugOrgBmpFileName(cPass, cTimeTag);
                            GaImageUtil.SaveBigImage(fileNameOrg, bmpBig);
                        }
                    }

                    if (fileNameOrg != null && _ftpUploader.Enabled)
                    {
                        _ftpUploader.UploadFile(fileNameOrg);
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveOrgImage");
                }
            }, args);
        }

        /// <summary>
        /// 非同步保存 cellBmp
        /// </summary>
        public void AsyncSaveCellBmp(Bitmap cellBmp, RegionCellX3Class cell)
        {
            if (!_INI.IsSaveTestImage || cellBmp == null || cell == null)
                return;

            Bitmap clonedBmp;
            lock (cellBmp)
            {
                clonedBmp = (Bitmap)cellBmp.Clone();
            }

            var dumpPath = GetLogPath(this.FileBarcodeStr);
            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}.bmp";
            string fullFileName = System.IO.Path.Combine(dumpPath, "PositionFix", fname);

            var args = new object[]
            {
                clonedBmp,
                fullFileName,
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    object[] argvs = (object[])argv;
                    string fileName = argvs[1] as string;
                    using (Bitmap bmp = argvs[0] as Bitmap)
                    {
                        string path = System.IO.Path.GetDirectoryName(fileName);
                        JetEazy.IO.QxPathUtility.InitDirectory(path);

                        GaImageUtil.SaveBigImage(fileName, bmp);
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveCellBmp");
                }
            }, args);
        }

        /// <summary>
        /// 非同步保存 邊框數據
        /// </summary>
        public void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi)
        {
#if (OPT_RESERVED)
            if (!_INI.IsSaveTestImage || cell == null)
                return;

            var lineBorderBoxes = cell?.ChipData?.LineBorderBoxes;
            var lineSegments = cell?.ChipData?.LineSegments;
            if (lineBorderBoxes == null)
                return;

            lineBorderBoxes = Array.ConvertAll(lineBorderBoxes, lb => lb?.Clone());
            lineSegments = lineSegments != null ? Array.ConvertAll(lineSegments, ls => ls?.Clone()) : null;

            string dumpPath = GetLogPath(this.FileBarcodeStr);
            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}_lines.json";
            string fullFileName = System.IO.Path.Combine(dumpPath, "PositionFix", fname);

            var args = new object[]
            {
                fullFileName,
                lineBorderBoxes,
                lineSegments,
                cellRoi,
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    object[] argvs = (object[])argv;
                    string fileNameA = (string)argvs[0];
                    var lineBorderBoxesA = (QvBox2D[])argvs[1];
                    var lineSegmentsA = (EzLSD.LineSegment[])argvs[2];
                    var roi = (RectangleF)argvs[3];

                    var lines = new List<string>();
                    if (lineBorderBoxesA != null)
                    {
                        int idx = 0;
                        foreach (var lb in lineBorderBoxesA)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"\"lineBorderBox_{idx}\" : [");
                            if (lb != null)
                            {
                                var center = lb.Center;
                                var cx = center.X - roi.X;
                                var cy = center.Y - roi.Y;
                                lb.SetCenter(cx, cy);
                                var pts = lb.Corners;
                                foreach (var pt in pts)
                                {
                                    sb.Append(pt.X).Append(",").Append(pt.Y).Append(",");
                                }
                            }
                            lines.Add(sb.ToString().TrimEnd(',') + "],");
                            idx++;
                        }
                    }

                    if (lineSegmentsA != null)
                    {
                        int idx = 0;
                        foreach (var ls in lineSegmentsA)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"\"lineSegment_{idx}\" : [");
                            if (ls != null)
                            {
                                ls.Offset(-roi.X, -roi.Y);
                                if (ls.P1 != null && ls.P2 != null)
                                {
                                    foreach (var pt in new[] { ls.P1, ls.P2 })
                                    {
                                        sb.Append(pt.X).Append(",").Append(pt.Y).Append(",");
                                    }
                                }
                            }
                            lines.Add(sb.ToString().TrimEnd(',') + "],");
                            idx++;
                        }
                    }

                    if (lines.Count > 0)
                    {
                        lines[lines.Count - 1] = lines[lines.Count - 1].TrimEnd(',');
                        lines.Insert(0, "{");
                        lines.Add("}");
                    }

                    string pathA = System.IO.Path.GetDirectoryName(fileNameA);
                    JetEazy.IO.QxPathUtility.InitDirectory(pathA);

                    System.IO.File.WriteAllLines(fileNameA, lines, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncDumpLineSegmentsData");
                }
            }, args);
#endif
        }

        /// <summary>
        /// 非同步保存 原圖 (安全修正變數作用域)
        /// </summary>
        public void AsyncSaveFlyCameraImage(FlyID flyID, Bitmap bmpFly, FlyLotData lotData, int[] flyResultCodes)
        {
            if (bmpFly == null || (!_INI.IsSaveDebugBmp && !_INI.IsSaveDebugOrgBmp))
                return;

            Bitmap clonedBmp;
            lock (bmpFly)
            {
                clonedBmp = (Bitmap)bmpFly.Clone();
            }

            var result = flyResultCodes != null && flyID.flyIndex < flyResultCodes.Length
                       ? (PlcFlyResultCode)flyResultCodes[flyID.flyIndex]
                       : PlcFlyResultCode.NG_EMPTY;

            var args = new object[]
            {
                flyID,
                clonedBmp,
                lotData.Clone(),
                result,
                DateTime.Now,
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    var argvs = (object[])argv;
                    var cFlyID = (FlyID)argvs[0];
                    var cLotData = (FlyLotData)argvs[2];
                    var cResult = (PlcFlyResultCode)argvs[3];
                    var tm = (DateTime)argvs[4];

                    using (Bitmap bmpBigAsync = (Bitmap)argvs[1])
                    {
                        // 修正：全部改使用解包後的 cFlyID 與 cLotData，避免作用域抓錯變數
                        int flyShowIndex = cFlyID.ShowID;
                        string stripID = cLotData.StripID;
                        string lotID = cLotData.LotID;
                        string code = cLotData.CodeStr;

                        string folder = "flyImage";
                        if (_INI.UseOkNgDiffImageFolders)
                        {
                            bool isPass = cResult == PlcFlyResultCode.OK || cResult == PlcFlyResultCode.NG_EMPTY;
                            if (!isPass) folder += ".NG";
                        }

                        string path = System.IO.Path.Combine(_INI.ResultImagePath, folder, tm.ToString("yyyyMMdd"), stripID);
                        if (!Directory.Exists(path))
                            JetEazy.IO.QxPathUtility.InitDirectory(path);

                        string fileName = $"{lotID}-[{flyShowIndex}]-[{code}]-{tm:yyyyMMdd_HHmmssfff}.jpg";
                        fileName = System.IO.Path.Combine(path, fileName);

                        GaImageUtil.SaveBigImage(fileName, bmpBigAsync);

                        if (fileName != null && _ftpUploader.Enabled)
                        {
                            _ftpUploader.UploadFile(fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveFlyCameraImage");
                }
            }, args);
        }

        public void DumpImage(RegionCellX3Class cell, CMvdImage image, string folder, string postfix)
        {
            if (!_INI.IsSaveTestImage || cell == null || image == null)
                return;

            string dumpPath = GetLogPath(this.FileBarcodeStr);
            string dumpFolder = System.IO.Path.Combine(dumpPath, folder);
            JetEazy.IO.QxPathUtility.InitDirectory(dumpFolder);

            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}-{postfix}.bmp";
            string fileName = System.IO.Path.Combine(dumpFolder, fname);

            image.SaveImage(fileName, MVD_FILE_FORMAT.MVD_FILE_BMP);
        }

        #region LOG_FUNCTIONS
        static void _LOG_ERROR(Exception ex, string message)
        {
            LtDebug.LOG.Error(ex, message);
            GaUtil.LOG($"[Error] {ex.Message}", Color.Red);
        }
        #endregion
    }
}
