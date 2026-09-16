using JetEazy.FormSpace;
using System;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    partial class RecipeFPIX3Class
    {
        /// <summary>
        /// 只被 Load function 內部調用 海康的 Train functions. 
        /// (1) PrintTempTrain
        /// (2) PrintTempFlyTrain
        /// </summary>
        private bool ViewTrainLoad()
        {
            try
            {
                var sysModel = GaMvcConfig.SysModel;
                sysModel?.AoiModel?.Train();
                return true;
            }
            catch (Exception ex)
            {
                VsMessageBox.Warning(ex.Message);
                return false;
            }

#if (OPT_OLD_CODE)
            //-----------------------------------------------------------------------------
            // 這應該設計在 AoiModel.SetRecipe(RecipeFPIX3Class recipe) 內,
            // 不應該由 Recipe 自己調用 !
            //-----------------------------------------------------------------------------
            int err = 0;
            if (err == 0)
            {
                err = PrintTempTrain();
                if (err != 0)
                {
                    //VsMessageBox.Warning("加載 (晶粒匹配) 參數 訓練失敗!\n\r該參數尚未建立\n\r或未插入 Dongle.");
                    VsMessageBox.Warning(QMSG.Text(Mvc.Model.ErrorCodes.AoiErr_Template_Train_Failed));
                }
            }
#endif

#if (OPT_OLD_FLY_CAM_AOI)
            if (err == 0)
            {
                err = PrintTempFlyTrain();
                if (err != 0)
                {
                    //VsMessageBox.Warning("加載 飛拍 參數 訓練失敗!\n\r該參數尚未建立\n\r或未插入 Dongle.");
                    VsMessageBox.Warning(QMSG.Text(Mvc.Model.ErrorCodes.AoiErr_FlyCam_Train_Failed));
                }
            }

            return err == 0;
#endif
        }

        #region NOT_USED_LEGACY_MVD_AOI_TOOLS_RUNTIME_海康工具相關成員_放在這裡非常不妥
#if (OPT_OLD_MVD_AOI_CODE)
        public MVD_CHIP_MATCHER mvdprinttemp_Find = new MVD_CHIP_MATCHER();
        public MvdFindClass mvdprintFlytemp_Find = new MvdFindClass();
        public Mvd2DReaderClass mvd2DReader = new Mvd2DReaderClass();
        public Mvd2DReaderClass fly2DReader = new Mvd2DReaderClass();
        void disposeMvdTools()
        {
            mvdprinttemp_Find?.Dispose();
            mvdprinttemp_Find = null;
            mvdprintFlytemp_Find?.Dispose();
            mvdprintFlytemp_Find = null;
            mvd2DReader?.Dispose();
            mvd2DReader = null;
            fly2DReader?.Dispose();
            fly2DReader = null;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_MVD_AOI_FUNCTIONS_FOR_CHIP_LOCATE_TRAIN_AND_RUN_晶粒定位的相關像測函式_放在這裡非常不妥
        //----------------------------------------------------------------------
        // 這些應該放在 AOI MODEL 
        //----------------------------------------------------------------------
        // Recipe 是配方材料 (食譜食材)
        // AoiModel 才是主角 (廚師)
        //      出各種飯局料理 是 廚師 而不是 食譜食材
        //      一隻雞 會自己剁雞腿 變成 滷雞腿 或 炸雞腿 是非常違反常理的謬異.
        //----------------------------------------------------------------------

#if (OPT_OLD_CODE)
        /// <summary>
        /// 這其實等同於 bmpDefectTemplate.Size
        /// </summary>
        public Size PrintTemplateSize
        {
            get
            {
                //// LETIAN: Revised for multithread
                //if (mvdprinttemp_Find != null)
                //    return mvdprinttemp_Find.TemplateSize;
                //return new Size(1, 1);

                if (GoldenChipBmp != null)
                    return GoldenChipBmp.Size;
                return new Size(1, 1);  
            }
        }
#endif

#if (OPT_OLD_CODE)
        public int PrintTempTrain(bool showGoldenVisualizedFeature = false)
        {
            // LETIAN: Revised for multithread
            mvdprinttemp_Find.SetRecipeParams(this.InspectParams);
            bool bOK = mvdprinttemp_Find.Train(this.bmpDefectTemplate);
            
            mvdprinttemp_Find.ShowGoldenTemplateVisualizer(showGoldenVisualizedFeature);

            return (bOK ? 0 : -1);
        }
#endif

#if (OPT_OLD_CODE)
        public int PrintTempRun(Bitmap ebmpInput)
        {
#if (OPT_OLD)
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;

            mvdprinttemp_Find.bmpRun_Image?.Dispose();
            mvdprinttemp_Find.bmpRun_Image = (Bitmap)ebmpInput.Clone();

            bool bOK = mvdprinttemp_Find.HikRunBmp();
#endif
            bool bOK = mvdprinttemp_Find.RunMatch(ebmpInput);
            return (bOK ? 0 : -1);
        }
#endif

#if (OPT_OLD_TEMPLATE_MATCH)
        public int PrintTempRun(CMvdImage eMvdInput)
        {
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            mvdprinttemp_Find.xMvdRun_Image?.Dispose();
            mvdprinttemp_Find.xMvdRun_Image = (CMvdImage)eMvdInput.Clone();
            bool bOK = mvdprinttemp_Find.HikRun2();
            return (bOK ? 0 : -1);
        }
        public int PrintTempRun(CMvdImage eMvdInput, RectangleF eRectF)
        {
            mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
            mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
            mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
            mvdprinttemp_Find.xMvdRun_Image?.Dispose();
            mvdprinttemp_Find.xMvdRun_Image = (CMvdImage)eMvdInput.Clone();
            bool bOK = mvdprinttemp_Find.HikRun3(eRectF);
            return (bOK ? 0 : -1);
        }
#endif

#if (OPT_OLD_FLY_CAM_AOI)
        public int PrintTempFlyTrain()
        {
            mvdprintFlytemp_Find.bmpObj_Image?.Dispose();
            mvdprintFlytemp_Find.bmpObj_Image = (Bitmap)bmpprintFlytemplate.Clone();
            bool bOK = mvdprintFlytemp_Find.HikTrainBmp();
            return (bOK ? 0 : -1);
        }
        public int PrintTempFlyRun(Bitmap ebmpInput)
        {
            mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
            mvdprintFlytemp_Find.xMvdTolerance = 1 - FlyParaClass.Instance.xTolerance;//<<<=这里把数据反过来不知为什么明明越大越严但是实际是反过来的
            mvdprintFlytemp_Find.bmpRun_Image?.Dispose();
            mvdprintFlytemp_Find.bmpRun_Image = (Bitmap)ebmpInput.Clone();
            bool bOK = mvdprintFlytemp_Find.HikRunBmp();
            return (bOK ? 0 : -1);
        }

        //public int PrintTempFlyRun(CMvdImage eMvdInput)
        //{
        //    mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
        //    mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
        //    mvdprintFlytemp_Find.xMvdRun_Image?.Dispose();
        //    mvdprintFlytemp_Find.xMvdRun_Image = eMvdInput;
        //    bool bOK = mvdprintFlytemp_Find.HikRun2();
        //    return (bOK ? 0 : -1);
        //}
        //public int PrintTempFlyRun(CMvdImage eMvdInput, RectangleF eRectF)
        //{
        //    mvdprintFlytemp_Find.xMvdAngle = FlyParaClass.Instance.xAngle;
        //    mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
        //    mvdprintFlytemp_Find.xMvdRun_Image?.Dispose();
        //    mvdprintFlytemp_Find.xMvdRun_Image = eMvdInput;
        //    bool bOK = mvdprintFlytemp_Find.HikRun3(eRectF);
        //    return (bOK ? 0 : -1);
        //}
        public bool CheckSpecialAngle(Bitmap ebmpInput, out List<CBlobInfo> retBlobs, out float retAngle, out PointF retCenter)
        {

            bool bOK = false;
            retAngle = 0;
            retCenter = new PointF();

            VisionDesigner.ImageBinary.CImageBinaryTool cImageBinaryToolObj = null;
            VisionDesigner.BlobFind.CBlobFindTool cBlobFindToolObj = null;

            if (cImageBinaryToolObj == null)
                cImageBinaryToolObj = new VisionDesigner.ImageBinary.CImageBinaryTool();
            if (cBlobFindToolObj == null)
                cBlobFindToolObj = new VisionDesigner.BlobFind.CBlobFindTool();

            //二值化
            cImageBinaryToolObj.InputImage?.Dispose();
            cImageBinaryToolObj.InputImage = GaImageUtil.BitmapToCMvdImage(ebmpInput);
            cImageBinaryToolObj.ROI = null;
            cImageBinaryToolObj.SetRunParam("LowThreshold", FlyParaClass.Instance.xThresholdValue.ToString());
            //cImageArithmeticToolObj.SetRunParam("HighThreshold", BlobHighThreshold.ToString());
            cImageBinaryToolObj.Run();

            //blob
            cBlobFindToolObj.InputImage?.Dispose();
            cBlobFindToolObj.InputImage = cImageBinaryToolObj.Result.OutputImage;
            //cBlobFindToolObj.RegionImage = BitmapToCMvdImage(eBmpMask);
            cBlobFindToolObj.ROI = null;
            if (FlyParaClass.Instance.xBlobMode == Eazy_Project_III.BlobMode.White)
                cBlobFindToolObj.SetRunParam("Polarity", "BrightObject");
            else
                cBlobFindToolObj.SetRunParam("Polarity", "DarkObject");
            cBlobFindToolObj.BasicParam.ShowBlobImageStatus = true;
            cBlobFindToolObj.Run();
            VisionDesigner.BlobFind.CBlobFindResult cBlobFindRes = cBlobFindToolObj.Result;

            retBlobs = new List<CBlobInfo>();
            foreach (var blob in cBlobFindToolObj.Result.BlobInfo)
            {
                if (blob.AreaF >= FlyParaClass.Instance.xBlobAreaMin && blob.AreaF <= FlyParaClass.Instance.xBlobAreaMax)
                {
                    retBlobs.Add(blob);
                }
            }

            if (retBlobs.Count >= 2)
            {
                CBlobInfo b0 = retBlobs[0];
                CBlobInfo b1 = retBlobs[1];

                // CreateInstance

                VisionDesigner.P2PMeasure.CP2PMeasureTool cP2PMeasureToolObj = new VisionDesigner.P2PMeasure.CP2PMeasureTool();

                // Set basic parameter

                cP2PMeasureToolObj.BasicParam.Point1 = new MVD_POINT_F(b0.RectInfo.CenterX, b0.RectInfo.CenterY);

                cP2PMeasureToolObj.BasicParam.Point2 = new MVD_POINT_F(b1.RectInfo.CenterX, b1.RectInfo.CenterY);

                // Running

                cP2PMeasureToolObj.Run();

                // Get the result

                VisionDesigner.P2PMeasure.CP2PMeasureResult cP2PMeasureRes = cP2PMeasureToolObj.Result;

                if (FlyParaClass.Instance.xIsShuiPing)
                {
                    retAngle = cP2PMeasureRes.Angle;
                }
                else
                {
                    retAngle = cP2PMeasureRes.Angle + 90;
                }

                //if (cP2PMeasureRes.Angle < 0)
                //{
                //    if (FlyParaClass.Instance.xIsShuiPing)
                //    {
                //        retAngle = -cP2PMeasureRes.Angle;
                //    }
                //    else
                //    {
                //        retAngle = -cP2PMeasureRes.Angle + 90;
                //    }
                //}
                //else
                //{
                //    if (FlyParaClass.Instance.xIsShuiPing)
                //    {
                //        retAngle = cP2PMeasureRes.Angle;
                //    }
                //    else
                //    {
                //        retAngle = cP2PMeasureRes.Angle - 90;
                //    }
                //}

                retCenter = new PointF(cP2PMeasureRes.MidPoint.fX, cP2PMeasureRes.MidPoint.fY);
                //Console.WriteLine("Angle: {0}", cP2PMeasureRes.Angle);

                //Console.WriteLine("Distance: {0}", cP2PMeasureRes.Dist);

                cP2PMeasureToolObj.Dispose();
                cP2PMeasureToolObj = null;

                bOK = true;
            }

            cImageBinaryToolObj.Dispose();
            cBlobFindToolObj.Dispose();

            cImageBinaryToolObj = null;
            cBlobFindToolObj = null;
            return bOK;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_TCP_DATA_沒用到
#if (OPT_TCP_DATA)
        public int SetByPass(bool[] eBypass)
        {
            if (eBypass == null)
                return -1;
            if (eBypass.Length != xRegionCells.Count)
                return -2;
            int i = 0;
            while (i < eBypass.Length)
            {
                xRegionCells[i].ByPass = eBypass[i];
                i++;
            }
            return 0;
        }
        public int SetBarcode(string[] eBarcodeStr)
        {
            if (eBarcodeStr == null)
                return -1;
            if (eBarcodeStr.Length != xRegionCells.Count)
                return -2;
            int i = 0;
            while (i < eBarcodeStr.Length)
            {
                xRegionCells[i].SetBarcodeStr = eBarcodeStr[i];
                i++;
            }
            return 0;
        }
        public bool RunRepeatCode()
        {
            bool isgood = true;
            ////收集所有页面读取到的二维码
            //List<string> _collectCodeList = new List<string>();
            //_collectCodeList.Clear();
            //foreach (RegionCellX2Class cell in xRegionCells)
            //{
            //    if (cell.RunCodeInfo == null)
            //        continue;
            //    string barcodeStr = cell.RunCodeInfo.Content;
            //    if (!string.IsNullOrEmpty(barcodeStr))
            //        _collectCodeList.Add(barcodeStr);
            //}

            //List<string> _collectRepeatCodeList = new List<string>();
            ////查询数据库中所有的条码
            //isgood = JzCheckRepeatClass.Instance.MySqlTableQuery(_collectCodeList, ref _collectRepeatCodeList) <= 0;
            //List<string> _collectNoRepeatCodeList = new List<string>();
            ////匹配到各个分支
            //if (!isgood)
            //{
            //    _collectNoRepeatCodeList.Clear();
            //    foreach (RegionCellX2Class cell in xRegionCells)
            //    {
            //        if (cell.RunCodeInfo == null)
            //            continue;
            //        string barcodeStr = cell.RunCodeInfo.Content;
            //        if (!string.IsNullOrEmpty(barcodeStr))
            //        {
            //            bool bOK = cell.CheckRepeatCode(_collectRepeatCodeList, 0);
            //            isgood &= bOK;
            //            if (bOK)
            //                _collectNoRepeatCodeList.Add(barcodeStr);
            //        }
            //    }
            //    JzCheckRepeatClass.Instance.MySqlTableInsert(_collectNoRepeatCodeList);
            //}
            //else
            //{
            //    JzCheckRepeatClass.Instance.MySqlTableInsert(_collectCodeList);
            //}

            ////比对同一片的重复码
            //foreach (RegionCellX2Class cell in xRegionCells)
            //{
            //    bool bOK = cell.CheckRepeatCode(_collectCodeList);
            //    isgood &= bOK;
            //}
            return isgood;
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_本專案沒用到_但是這應該放在_AOI_RESULT_區域
        //private int PassCount = 0;
        //private int NGCount = 0;
        #endregion

        #region NOT_USED_LEGACY_统计数据_沒用到
#if (OPT_NOT_USED)
        public float[] AnalyzeDatas = new float[9];
#endif
        public void AnalyzeDatasData()
        {
#if (OPT_NOT_USED)
            int count = Enum.GetValues(typeof(InspectReason)).Length;
            AnalyzeDatas = new float[count + 2];
            int i = 0;
            while (i < AnalyzeDatas.Length)
            {
                AnalyzeDatas[i] = 0;
                i++;
            }
#endif
        }
        /// <summary>
        /// 分析数据 返回bool
        /// </summary>
        /// <returns>true:PASS false:FAIL</returns>
        public bool AnalyzeDatasRun()
        {
#if (OPT_NOT_USED)
            //foreach (RegionCellX3Class cell in xRegionCells)
            //{
            //    if (cell.inspectReasons.Count == 0)
            //    {
            //        AnalyzeDatas[(int)InspectReason.PASS]++;
            //        continue;
            //    }
            //    foreach (InspectReason reason in cell.inspectReasons)
            //    {
            //        AnalyzeDatas[(int)reason]++;
            //    }
            //}
            ////芯片数
            //AnalyzeDatas[AnalyzeDatas.Length - 2] = xRow * xColumn;
            ////良率
            //AnalyzeDatas[AnalyzeDatas.Length - 1] = AnalyzeDatas[(int)InspectReason.PASS] * 1.0f / AnalyzeDatas[AnalyzeDatas.Length - 2] * 100;
            //float ins_count = AnalyzeDatas[(int)InspectReason.PASS] + AnalyzeDatas[(int)InspectReason.INS_NOOPEN];
            //bool bOK = ins_count == xRow * xColumn;

            //Add(bOK);
            //return bOK;
#endif
            return true;
        }

#if (OPT_NOT_USED)
        //public int PassCount = 0;
        //public int NGCount = 0;
        public void ResetZero()
        {
            PassCount = 0;
            NGCount = 0;
            SaveLotCount();
        }
        void Add(bool eIsPass = true)
        {
            if (eIsPass)
            {
                PassCount++;
            }
            else
            {
                NGCount++;
            }
            SaveLotCount();
        }
        void SaveLotCount()
        {
            WriteINIValue("Recipe Basic", "PassCount", PassCount.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "NGCount", NGCount.ToString(), INIFILE);
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_校正與座標轉換_GAARA_版本
#if (OPT_NOT_USED_LEGACY)
        /// <summary>
        /// 校正與座標轉換: 第一吸嘴排. (GAARA版)
        /// (載台: 由 xStageNumber Runtime 決定)
        /// </summary>
        public LineScanCalibrateClass lineScanCalibrate
        {
            get
            {
                switch (xStageNumber)
                {
                    case StageNumber.N1:
                        return Traveller106.Universal.LineScanCalibrateClasses[2];  //Carrier2 Sucker1
                                                                                    //break;
                    default:
                        return Traveller106.Universal.LineScanCalibrateClasses[0];  //Carrier1 Sucker1
                        //break;
                }
            }
        }
        /// <summary>
        /// 校正與座標轉換: 第一吸嘴排. (GAARA版)
        /// (載台: 由 xStageNumber Runtime 決定)
        /// </summary>
        public LineScanCalibrateClass lineScanCalibrate2
        {
            get
            {
                switch (xStageNumber)
                {
                    case StageNumber.N1:
                        return Traveller106.Universal.LineScanCalibrateClasses[3];  //Carrier2 Sucker2
                    //break;
                    default:
                        return Traveller106.Universal.LineScanCalibrateClasses[1];  //Carrier1 Sucker2
                        //break;
                }
            }
        }
        /// <summary>
        /// Golden Chip 中心點 pixel coordinates, 以 Golden Region (xRectRegionPrint) 左上角 當相對原點.
        /// (用於 GAARA版 校正與座標轉換)
        /// </summary>
        PointF LeftTopRectCenter
        {
            get
            {
                var LeftTopRect = xRegionTrain;
                PointF ptCenter
                    = new PointF(LeftTopRect.X + LeftTopRect.Width / 2 + xRectRegionPrint.X,
                                 LeftTopRect.Y + LeftTopRect.Height / 2 + xRectRegionPrint.Y);
                return ptCenter;
            }
        }
#endif
        #endregion

        #region NOT_USED_LEGACY_CODE
        /// <summary>
        /// 創建 RegionCells.
        /// (這個應該是放在 AOI MODEL 內)
        /// </summary>
        private void CreateViews()
        {
            //-----------------------------------------------------------------------------
            // 這應該設計在 AoiModel.SetRecipe(RecipeFPIX3Class recipe) 內,
            // 不應該由 Recipe 自己調用 !
            //-----------------------------------------------------------------------------
            // 已經交由 SysModel.ApplyRecipe 自動建立 Region Cells !!!
            // GaMvcConfig.SysModel.ApplyRecipe(optWritebackToRecipe: true);
            return;
        }
        /// <summary>
        /// 創建 RegionCells. (Gaara 版)
        /// (這個應該是放在 AOI MODEL 內)
        /// </summary>
        void CreateViews_Gaara()
        {
#if (OPT_OLD_CODE)
            //-----------------------------------------------------------------------------
            // 這應該設計在 AoiModel.SetRecipe(RecipeFPIX3Class recipe) 內,
            // 不應該由 Recipe 自己調用 !
            //-----------------------------------------------------------------------------

            xRegionCells.Clear();

            PointF ptworld = lineScanCalibrate.ViewToWorld(LeftTopRectCenter);
            xRealLeftX = ptworld.X;
            xRealLeftY = ptworld.Y;

            RectangleF _baserect = new RectangleF(xLeftTopX,
                xLeftTopY,
                xChipWidth / INI.Instance.ImageResolution,
                xChipHeight / INI.Instance.ImageResolution);
            float _rowoffset = xRowOffset / INI.Instance.ImageResolution;
            float _coloffset = xColumnOffset / INI.Instance.ImageResolution;

            int _index = 0;
            for (int i = 0; i < xRow; i++)
            {
                if (i % 2 == 1)
                {
                    for (int j = xColumn - 1; j > -1; j--)
                    {
                        RegionCellX3Class _cell = new RegionCellX3Class();
                        _cell.Index = _index;
                        _cell.CellRow = i;
                        _cell.CellCol = j;
                        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);

                        _cell.OrgX = xRealLeftX + j * xRealOffsetX;
                        _cell.OrgY = xRealLeftY + i * xRealOffsetY;

                        xRegionCells.Add(_cell);
                        _index++;
                    }
                }
                else
                {
                    for (int j = 0; j < xColumn; j++)
                    {
                        RegionCellX3Class _cell = new RegionCellX3Class();
                        _cell.Index = _index;
                        _cell.CellRow = i;
                        _cell.CellCol = j;
                        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
                        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);

                        _cell.OrgX = xRealLeftX + j * xRealOffsetX;
                        _cell.OrgY = xRealLeftY + i * xRealOffsetY;

                        xRegionCells.Add(_cell);
                        _index++;
                    }
                }
            }

            float a = _baserect.X;
            float b = _baserect.Y;

            var cellBase = xRegionCells[0];
            int ix = 0;
            while (ix < xRegionCells.Count)
            {
                var cell = xRegionCells[ix];
                var pt = RotatePointAroundPivot(cell.viewRectF.Location,
                                                cellBase.viewRectF.Location,
                                                xAngle);

                cell.viewRectF.X = pt.X;
                cell.viewRectF.Y = pt.Y;

                ix++;
            }

            //AnalyzeDatasData();

            //原始的顺序排列
            //int _index = 0;
            //for (int i = 0; i < xRow; i++)
            //{
            //    for (int j = 0; j < xColumn; j++)
            //    {
            //        RegionCellX3Class _cell = new RegionCellX3Class();
            //        _cell.Index = _index;
            //        _cell.CellRow = i;
            //        _cell.CellCol = j;
            //        _cell.lblName = "ROW" + i.ToString("000") + "-COL" + j.ToString("000");
            //        _cell.viewRectF = new RectangleF(_baserect.X + j * _coloffset, _baserect.Y + i * _rowoffset, _baserect.Width, _baserect.Height);
            //        xRegionCells.Add(_cell);
            //        _index++;
            //    }
            //}
#endif
        }
        #endregion

        #region NOT_USED_LEGACY_MISC_UTIL_FUNCTIONS
#if (OPT_NOT_USED_CODE)
        /// <summary>
        /// 绕任意点旋转一个点 (這個應該放在 Util 模塊內)
        /// </summary>
        /// <param name="pointToRotate">要旋转的点</param>
        /// <param name="pivotPoint">旋转中心点</param>
        /// <param name="angleDegrees">旋转角度(度)</param>
        /// <returns>旋转后的新点</returns>
        static PointF RotatePointAroundPivot(PointF pointToRotate, PointF pivotPoint, double angleDegrees)
        {
            // 将角度转换为弧度
            double angleRadians = angleDegrees * Math.PI / 180.0;
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            // 将点平移到原点周围
            PointF translatedPoint = new PointF(
                pointToRotate.X - pivotPoint.X,
                pointToRotate.Y - pivotPoint.Y);

            // 执行旋转
            PointF rotatedPoint = new PointF(
                (float)(translatedPoint.X * cosTheta - translatedPoint.Y * sinTheta),
                (float)(translatedPoint.X * sinTheta + translatedPoint.Y * cosTheta));

            // 平移回原位置
            PointF finalPoint = new PointF(
                rotatedPoint.X + pivotPoint.X,
                rotatedPoint.Y + pivotPoint.Y);

            return finalPoint;
        }
#endif
        #endregion
    }
}
