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
        //使用 Thread-safe 的 Lazy 單例
        private static readonly Lazy<LotDataHolder> _instance
            = new Lazy<LotDataHolder>(() => new LotDataHolder());
        protected LotDataHolder()
        {
        }
        #endregion

        public static LotDataHolder Instance
        {
            get => _instance.Value;
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
            set
            {
                _lotData.LotID = value;
                OnLotDataChanged?.Invoke(this, EventArgs.Empty);
            }
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
                MarkFileTimeTag();
            }
        }

        #region PRIVATE_PATH_FILE_FUNCTIONS
        DateTime _timeTag = DateTime.Now;

        /// <summary>
        /// 帶日期時間尾綴的檔名
        /// </summary>
        internal string GetLotFileName(string tag, string ext)
        {
            //m_FileName = $"{LotId}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.jpg";
            return $"{tag}-{_timeTag:yyyyMMdd_HHmmss}{ext}";
        }

        /// <summary>
        /// 根據 (StripId, LotId) 生成檔名 於 子資料夾 "LineScanImage"
        /// </summary>
        protected string GetDebugBmpFileName(bool pass)
        {
            string folder = "LineScanImage";
            if (_INI.UseOkNgDiffImageFolders)
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
            if (_INI.UseOkNgDiffImageFolders)
            {
                if (!pass) folder += ".NG";
            }
            return GetDebugImgSaveFileName(folder, StripId, LotId);
        }

        /// <summary>
        /// 檔案 _INI.ResultImagePath
        ///         \ subFolder
        ///         \ yyyyMMdd
        ///         \ stripID
        ///         \ lotID-yyyyMMdd_HHmmss.jpg
        /// </summary>
        protected string GetDebugImgSaveFileName(string subFolder, string stripID, string lotID, bool autoCreateDir = true)
        {
            string path = System.IO.Path.Combine(_INI.ResultImagePath, subFolder, _timeTag.ToString("yyyyMMdd"), stripID);
            if (autoCreateDir && !System.IO.Directory.Exists(path))
            {
                // 改用 JetEazy.IO.QxPathUtility.InitDirectory 可以 遞迴深層 創建資料夾.
                // System.IO.Directory.CreateDirectory(path);
                JetEazy.IO.QxPathUtility.InitDirectory(path);
            }

            string file = $"{lotID}-{_timeTag:yyyyMMdd_HHmmss}.jpg";
            return System.IO.Path.Combine(path, file);
        }

        /// <summary>
        /// 指向 [LOG_ROOT]\\Images\\[yyyyMMdd] 資料夾
        /// </summary>
        internal string GetLogPath(string subFolder)
        {
            if (string.IsNullOrEmpty(subFolder))
                subFolder = "Dump";
            return System.IO.Path.Combine(LOG_IMG_PATH, _timeTag.ToString("yyyyMMdd"), subFolder);
        }
        #endregion

        /// <summary>
        /// 標定統一的存檔時間
        /// </summary>
        public void MarkFileTimeTag()
        {
            _timeTag = DateTime.Now;
        }

        /// <summary>
        /// 非同步保存 原圖 (caller 負責 bmpFullfov 生命)
        /// </summary>
        public void AsyncSaveOrgImage(Bitmap bmpFullfov, bool pass)
        {
            if (!_INI.IsSaveDebugOrgBmp && !_INI.IsSaveDebugBmp)
                return;

            if (bmpFullfov == null)
                return;

            var args = new object[]
            {
                bmpFullfov.Clone(),
                pass
            };

            ThreadPool.QueueUserWorkItem(argv =>
            {
                try
                {
                    var argvs = (object[])argv;
                    var cPass = (bool)argvs[1];

                    using (Bitmap bmpBig = (Bitmap)argvs[0])
                    {
                        //(1) 保存壓縮圖檔 (IsSaveDebugBMP)
                        if (_INI.IsSaveDebugBmp)
                        {
                            string fileName = GetDebugBmpFileName(cPass);
                            GaImageUtil.SaveImageWithQuality(bmpBig, fileName, _INI.ImageQuality);
                        }

                        //(2) 保存原始圖檔 (IsSaveDebugOrgBmp)
                        if (_INI.IsSaveDebugOrgBmp)
                        {
                            string fileName = GetDebugOrgBmpFileName(cPass);
                            GaImageUtil.SaveBigImage(fileName, bmpBig);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveOrgImage");
                }
            },
                args
            );
        }

        /// <summary>
        /// 非同步保存 cellBmp.
        /// caller 負責 cellBmp 生命
        /// </summary>
        public void AsyncSaveCellBmp(Bitmap cellBmp, RegionCellX3Class cell)
        {
            if (!_INI.IsSaveTestImage)
                return;

            if (cellBmp == null || cell == null)
                return;

            var dumpPath = GetLogPath(this.FileBarcodeStr);
            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}.bmp";
            string fullFileName = System.IO.Path.Combine(dumpPath, "PositionFix", fname);

            var args = new object[]
            {
                cellBmp.Clone(),
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
                        // 檢查 Path
                        string path = System.IO.Path.GetDirectoryName(fileName);
                        JetEazy.IO.QxPathUtility.InitDirectory(path);

                        // 保存檔案
                        GaImageUtil.SaveBigImage(fileName, bmp);
                    }
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveCellBmp");
                }
            },
                args
            );
        }

        /// <summary>
        /// 非同步保存 邊框數據
        /// </summary>
        public void AsyncDumpLineSegmentsData(RegionCellX3Class cell, ref RectangleF cellRoi)
        {
            if (!_INI.IsSaveTestImage)
                return;

            if (cell == null)
                return;

            var lineBorderBoxes = cell?.ChipData?.LineBorderBoxes;
            var lineSegments = cell?.ChipData?.LineSegments;
            if (lineBorderBoxes == null)
                return;

            // 複製
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
                                // OFFSET
                                var center = lb.Center;
                                var cx = center.X - roi.X;
                                var cy = center.Y - roi.Y;
                                lb.SetCenter(cx, cy);
                                // dump CORNERS
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
                                // OFFSET
                                ls.Offset(-roi.X, -roi.Y);
                                // dump P1 P2
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

                    // 準備資料夾
                    string pathA = System.IO.Path.GetDirectoryName(fileNameA);
                    JetEazy.IO.QxPathUtility.InitDirectory(pathA);

                    // 生成 json 檔案
                    System.IO.File.WriteAllLines(fileNameA, lines, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncDumpLineSegmentsData");
                }
            },
                args
            );
        }

        /// <summary>
        /// 非同步保存 原圖 (caller 負責 bmpFullfov 生命)
        /// </summary>
        public void AsyncSaveFlyCameraImage(FlyID flyID, Bitmap bmpFly, FlyLotData lotData, int[] flyResultCodes)
        {
            if (bmpFly == null)
                return;

            if (!_INI.IsSaveDebugBmp && !_INI.IsSaveDebugOrgBmp)
                return;

            var result = flyResultCodes != null && flyID.flyIndex < flyResultCodes.Length
                       ? (PlcFlyResultCode)flyResultCodes[flyID.flyIndex]
                       : PlcFlyResultCode.NG_EMPTY;

            var args = new object[]
            {
                flyID,
                bmpFly.Clone(),
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
                        int flyShowIndex = flyID.ShowID;
                        string stripID = lotData.StripID;
                        string lotID = lotData.LotID;
                        string code = lotData.CodeStr;

                        // 2026-07-09 泰國版要求分流保存圖檔
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
                    }
                }
                catch (Exception ex)
                {
                    //_LOG($"异常捕获:{ex.Message}", Color.Red);
                    _LOG_ERROR(ex, $"{GetType().Name}.AsyncSaveFlyCameraImage");
                    //GaUtil.LOG()
                }
            },
                args
            );
        }

        /// <summary>
        /// 保存調試用的圖片檔 (包含形態學處理後的結果，利於工程師現場調機)
        /// </summary>
        public void DumpImage(RegionCellX3Class cell, CMvdImage image, string folder, string postfix)
        {
            if (!_INI.IsSaveTestImage)
                return;

            if (cell == null || image == null)
                return;

            //string dumpPath = RegionCellX3Class.SaveDebugPath;
            string dumpPath = GetLogPath(this.FileBarcodeStr);
            string dumpFolder = System.IO.Path.Combine(dumpPath, folder);
            JetEazy.IO.QxPathUtility.InitDirectory(dumpFolder);

            string fname = $"Cell_{cell.Index}@{cell.CellRow}_{cell.CellCol}-{postfix}.bmp";
            string fileName = System.IO.Path.Combine(dumpFolder, fname);

            //_cImageArithmeticTool?.InputImage1?.SaveImage(fileStem + "_Template.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageArithmeticTool?.InputImage2?.SaveImage(fileStem + "_Run.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageBinaryTool?.Result?.OutputImage?.SaveImage(fileStem + "_Diff_Binary.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //_cImageMorphTool?.Result?.OutputImage?.SaveImage(fileStem + "_Diff_MorphOpen.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP); // 新增儲存開運算圖
            //_cBlobFindTool?.RegionImage?.SaveImage(fileStem + "_MaskRegion.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);
            //cBlobFindRes?.BlobImage?.SaveImage(fileStem + "_BlobResult.bmp", MVD_FILE_FORMAT.MVD_FILE_BMP);

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
