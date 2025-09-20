using Common.RecipeSpace;
using Eazy_Project_III;
using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.BasicSpace;
using System;
using System.Collections.Generic;
using System.Drawing;
using Traveller106;
using VisionDesigner;
using VisionDesigner.BlobFind;
using MVD_CHIP_MATCHER = LaserAlignDX.AoiModel.MvdCompositeChipMatcher;


namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public class RecipeFPIX3Class : RecipeBaseClass, IDisposable
    {
        #region SINGLETON
        protected RecipeFPIX3Class()
        {
            RcpBmpHolder.CommonPathFunc = new Func<string>(() => PathIndexStr);
        }
        private static RecipeFPIX3Class _instance = null;
        #endregion

        public static RecipeFPIX3Class Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RecipeFPIX3Class();
                return _instance;
            }
        }
        public static void DisposeAll()
        {
            _instance?.Dispose();
            _instance = null;
        }
        public void Dispose()
        {
            // 2025-09-14 LETIAN: 初步整理出需要釋放資源的子模塊
            disposeRegionCells();
            disposeCamGrids();

            disposeMvdTools();

            disposeBmpOrgs();
            disposeBmpOrgFly();

            disposeGoldenRegionTemplate();
            disposeDefectInspectTemplate();
            disposeFlyCamTemplate();
            disposeCodeTemplate();
        }

        #region REGION_CELLS
        /// <summary>
        /// 個別 晶粒區域 (位於格點範圍)
        /// 這應該放在 AoiResult 而不是 Recipe 區
        /// </summary>
        public List<RegionCellX3Class> xRegionCells = new List<RegionCellX3Class>();
        /// <summary>
        /// 個別 疑似異物區塊 (位於格點範圍外)
        /// 這應該放在 AoiResult 而不是 Recipe 區
        /// </summary>
        public List<Rectangle> xOutBlocs = new List<Rectangle>();
        /// <summary>
        /// 釋放資源
        /// </summary>
        void disposeRegionCells()
        {
            foreach (var cell in xRegionCells)
                cell?.Dispose();
            xRegionCells.Clear();
        }
        #endregion

        #region PRIVATE_個別載台的_BMP_HOLDERS
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台1 的 bmpOrg
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrg1 = new RcpBmpHolder("org");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台2 的 bmpOrg
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrg2 = new RcpBmpHolder("org2");
        #endregion

        #region 載台的_BMP_ORG_接口
        public Bitmap PeekBmpOrg(CarrierEnum carrierID)
        {
            return carrierID == CarrierEnum.C1 ? _bmpHolderOrg1.Peek() : _bmpHolderOrg2.Peek();
        }
        public void TakeInBmpOrg(CarrierEnum carrierID, Bitmap bmp)
        {
            if (carrierID == CarrierEnum.C1)
                _bmpHolderOrg1.TakeOver(bmp);
            else
                _bmpHolderOrg2?.TakeOver(bmp);
        }
        public void ReleaseBmpsOrg(bool save)
        {
            // bmpOrg 一般只用於參數編輯時期, 跑線時可以釋放
            if (save)
            {
                _bmpHolderOrg1.Save();
                _bmpHolderOrg2.Save();
            }
            _bmpHolderOrg1?.Dispose();
            _bmpHolderOrg2?.Dispose();
        }
        void disposeBmpOrgs()
        {
            _bmpHolderOrg1?.Dispose();
            _bmpHolderOrg2?.Dispose();
        }
        #endregion

        #region 飛拍_BMP_ORG_接口
        //public Bitmap PeekBmpFlyOrg()
        //{
        //    return _bmpHolderOrgFly.Peek();
        //}
        //public void TakeInBmpFlyOrg(Bitmap bmp)
        //{
        //    _bmpHolderOrgFly.TakeOver(bmp);
        //}
        //public void ReleaseBmpFlyOrg(bool save)
        //{
        //    // bmpOrgFly 一般只用於參數編輯時期, 跑線時可以釋放
        //    if (save)
        //    {
        //        _bmpHolderOrgFly.Save();
        //    }
        //    _bmpHolderOrgFly?.Dispose();
        //}
        public Bitmap bmpOrgFly = new Bitmap(1, 1);
        void disposeBmpOrgFly()
        {
            //_bmpHolderOrgFly?.Dispose();
            bmpOrgFly?.Dispose();
            bmpOrgFly = null;
        }
        #endregion

        #region GOLDEN_REGION_TEMPLATE_晶粒區域樣本
        /// <summary>
        /// Golden Region Cell (晶粒區域粗框)
        /// </summary>
        public RectangleF xRectRegionPrint = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// Golden Region Bitmap (晶粒區域粗框)
        /// </summary>
        public Bitmap bmpprinttemplate = new Bitmap(1, 1);
        /// <summary>
        /// Golden Chip Rect
        /// 更精確(內縮)的晶粒矩形區域
        /// 位於 Golden Region (xRectRegionPrint) 之內
        /// 相對於 xRectRegionPrint 的左上角為零點
        /// </summary>
        public RectangleF xRegionTrain = new RectangleF(0, 0, 100, 100);
        void disposeGoldenRegionTemplate()
        {
            bmpcodetemplate?.Dispose();
            bmpcodetemplate = null;
        }
        #endregion

        #region DEFECT_INSPECTOR_TEMPLATE_瑕疵檢所用到的樣本
        /// <summary>
        /// 實際上就是 Golden Chip Template
        /// </summary>
        public Bitmap bmpDefectTemplate = new Bitmap(1, 1);
        /// <summary>
        /// 瑕疵檢查的 Mask
        /// </summary>
        public Bitmap bmpprintmask = new Bitmap(1, 1);
        void disposeDefectInspectTemplate()
        {
            this.bmpprintmask?.Dispose();
            this.bmpprintmask = null;
            this.bmpDefectTemplate?.Dispose();
            this.bmpDefectTemplate = null;
        }
        #endregion

        #region LINE_BORDER_BOXES_邊線區塊_手拉框
        /// <summary>
        /// 邊線框(左)
        /// </summary>
        public RectangleF xLineLeft = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(上)
        /// </summary>
        public RectangleF xLineTop = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(右)
        /// </summary>
        public RectangleF xLineRight = new RectangleF(0, 0, 100, 100);
        /// <summary>
        /// 邊線框(下)
        /// </summary>
        public RectangleF xLineBottom = new RectangleF(0, 0, 100, 100);
        #endregion

        #region FLY_CAMERA_TEMPLATE_飛拍樣本
        public RectangleF xRectRegionPrintFly = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpprintFlytemplate = new Bitmap(1, 1);
        void disposeFlyCamTemplate()
        {
            this.bmpprintFlytemplate?.Dispose();
            this.bmpprintFlytemplate = null;
        }
        #endregion

        #region MVD_AOI_TOOLS_RUNTIME_海康工具相關成員
        public MVD_CHIP_MATCHER mvdprinttemp_Find = new MVD_CHIP_MATCHER();
        public MvdFindClass mvdprintFlytemp_Find = new MvdFindClass();
        public Mvd2DReaderClass mvd2DReader = new Mvd2DReaderClass();
        void disposeMvdTools()
        {
            mvdprinttemp_Find?.Dispose();
            mvdprinttemp_Find = null;
            mvdprintFlytemp_Find?.Dispose();
            mvdprintFlytemp_Find = null;
            mvd2DReader?.Dispose();
            mvd2DReader = null;
        }
        #endregion

        #region QR_CODE_TEMPLATE
        public RectangleF xRectCodeRegion = new RectangleF(0, 0, 100, 100);
        public Bitmap bmpcodetemplate = new Bitmap(1, 1);
        void disposeCodeTemplate()
        {
            bmpcodetemplate?.Dispose();
            bmpcodetemplate = null;
        }
        #endregion

        #region 參數區_RecipeParaGridClass
        // 以下成員, 是讓 RecipeParaGridClass 來進行 ini 存取 
        internal int xRow = 1;
        internal int xColumn = 1;
        internal int xLeftTopX = 1;
        internal int xLeftTopY = 1;
        internal float xRowOffset = 2f;
        internal float xColumnOffset = 2f;
        internal float xChipWidth = 10;
        internal float xChipHeight = 10;
        internal int xExtendx = 100;
        internal int xExtendy = 100;
        internal float xAngle = 0;
        internal int xChNum = 1;
        internal int xChValue = 255;
        //public int xUseStageNo = 0;
        internal StageNumber xStageNumber = StageNumber.N0;
        #endregion

        public string xLotNoStr = "NONE";

        #region NO_USE_本專案沒用到_但是這應該放在_AOI_RESULT_區域
        private int PassCount = 0;
        private int NGCount = 0;
        #endregion

        #region 參數區_实际矩阵XY
        public float xRealLeftX = 0;
        public float xRealLeftY = 0;
        public float xRealOffsetX = 1;
        public float xRealOffsetY = 1;
        #endregion

        #region 參數區_LT_CAM_GRIDS
        public EzBlocsGrid xCamGrid1 = null;
        public EzBlocsGrid xCamGrid2 = null;
        void loadCamGrids(CarrierEnum carrierID, out EzBlocsGrid grid)
        {
            var file = System.IO.Path.Combine(PathIndexStr, $"camGrid_{carrierID}.txt");
            if (System.IO.File.Exists(file))
            {
                string str = System.IO.File.ReadAllText(file);
                var ss = new EzBlocsGridSerializer();
                ss.Deserialize(str, out grid);
                return;
            }
            else
            {
                grid = null;
            }
        }
        void saveCamGrid(CarrierEnum carrierID, EzBlocsGrid grid)
        {
            if (grid == null)
                return;

            var file = System.IO.Path.Combine(PathIndexStr, $"camGrid_{carrierID}.txt");
            var ss = new EzBlocsGridSerializer();
            string str = ss.Serialize(grid);
            System.IO.File.WriteAllText(file, str);
        }
        void disposeCamGrids()
        {
            xCamGrid1?.Dispose();
            xCamGrid1 = null;
            xCamGrid2?.Dispose();
            xCamGrid2 = null;
        }
        public void SaveCameraGrids()
        {
            saveCamGrid(CarrierEnum.C1, xCamGrid1);
            saveCamGrid(CarrierEnum.C2, xCamGrid2);
        }
        #endregion

        #region PRIVATE_LOCAL_BMP_HELPER_FUNCTIONS
        Bitmap loadImage(string fname)
        {
            string fileName = System.IO.Path.Combine(PathIndexStr, fname);
            if (System.IO.File.Exists(fileName))
            {
                return GaImageUtil.LoadBigImage(fileName);
            }
            return new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        }
        void saveImage(Bitmap bmp, string fname)
        {
            if (bmp == null) return;
            string fileName = System.IO.Path.Combine(PathIndexStr, fname);
            GaImageUtil.SaveBigImage(fileName, bmp);
        }
        #endregion

        public override void Load(bool eCancel = false)
        {
            xLotNoStr = ReadINIValue("Collect", "xLotNoStr", "NONE", INIFILE);

            xRow = int.Parse(ReadINIValue("Recipe Basic", "xRow", "1", INIFILE));
            xColumn = int.Parse(ReadINIValue("Recipe Basic", "xColumn", "1", INIFILE));
            xLeftTopX = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopX", "1", INIFILE));
            xLeftTopY = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopY", "1", INIFILE));
            xAngle = float.Parse(ReadINIValue("Recipe Basic", "xAngle", "0", INIFILE));
            xRowOffset = float.Parse(ReadINIValue("Recipe Basic", "xRowOffset", "2", INIFILE));
            xColumnOffset = float.Parse(ReadINIValue("Recipe Basic", "xColumnOffset", "2", INIFILE));
            xChipWidth = float.Parse(ReadINIValue("Recipe Basic", "xChipWidth", "10", INIFILE));
            xChipHeight = float.Parse(ReadINIValue("Recipe Basic", "xChipHeight", "10", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Recipe Basic", "xExtendx", "100", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Recipe Basic", "xExtendy", "100", INIFILE));
            //xRealLeftX = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftX", "0", INIFILE));
            //xRealLeftY = float.Parse(ReadINIValue("Recipe Basic", "xRealLeftY", "0", INIFILE));
            xRealOffsetX = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetX", "1", INIFILE));
            xRealOffsetY = float.Parse(ReadINIValue("Recipe Basic", "xRealOffsetY", "1", INIFILE));
            xStageNumber = (StageNumber)int.Parse(ReadINIValue("Recipe Basic", "xStageNumber", "0", INIFILE));

            PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", "0", INIFILE));
            NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", "0", INIFILE));

            xChNum = int.Parse(ReadINIValue("Recipe Basic", "xChNum", "1", INIFILE));
            xChValue = int.Parse(ReadINIValue("Recipe Basic", "xChValue", "255", INIFILE));

            //xRectRegionBase0 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectRegionBase1 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //template0Center = StringtoPointF(ReadINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            //template1Center = StringtoPointF(ReadINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            //distancebase0tobase1 = StringtoPointF(ReadINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(new System.Drawing.PointF(0, 0)), INIFILE));
            xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xRegionTrain = StringtoRectF(ReadINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            //xRectRegionPrintNoTray = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintNoTray", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xRectRegionPrintFly = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            //ptPrinttemp = StringtoPointF(ReadINIValue("Recipe Basic", "ptPrinttemp", PointFtoStringSimple(new PointF(-1, -1)), INIFILE));
            //ptPrintFlytemp = StringtoPointF(ReadINIValue("Recipe Basic", "ptPrintFlytemp", PointFtoStringSimple(new PointF(-1, -1)), INIFILE));


            xLineLeft = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineTop = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineRight = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineBottom = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));


            //xRow = int.Parse(ReadINIValue("Recipe Basic", "xRow", xRow.ToString(), INIFILE));
            //xColumn = int.Parse(ReadINIValue("Recipe Basic", "xColumn", xColumn.ToString(), INIFILE));
            //xLeftTopX = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopX", xLeftTopX.ToString(), INIFILE));
            //xLeftTopY = int.Parse(ReadINIValue("Recipe Basic", "xLeftTopY", xLeftTopY.ToString(), INIFILE));
            //xRowOffset = float.Parse(ReadINIValue("Recipe Basic", "xRowOffset", xRowOffset.ToString(), INIFILE));
            //xColumnOffset = float.Parse(ReadINIValue("Recipe Basic", "xColumnOffset", xColumnOffset.ToString(), INIFILE));
            //xChipWidth = float.Parse(ReadINIValue("Recipe Basic", "xChipWidth", xChipWidth.ToString(), INIFILE));
            //xChipHeight = float.Parse(ReadINIValue("Recipe Basic", "xChipHeight", xChipHeight.ToString(), INIFILE));

            //PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", PassCount.ToString(), INIFILE));
            //NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", NGCount.ToString(), INIFILE));

            //xRectRegionBase0 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase0", RectFtoStringSimple(xRectRegionBase0), INIFILE));
            //xRectRegionBase1 = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionBase1", RectFtoStringSimple(xRectRegionBase1), INIFILE));
            //template0Center = StringtoPointF(ReadINIValue("Recipe Basic", "template0Center", PointFtoStringSimple(template0Center), INIFILE));
            //template1Center = StringtoPointF(ReadINIValue("Recipe Basic", "template1Center", PointFtoStringSimple(template1Center), INIFILE));
            //distancebase0tobase1 = StringtoPointF(ReadINIValue("Recipe Basic", "distancebase0tobase1", PointFtoStringSimple(distancebase0tobase1), INIFILE));
            //xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE));

            //xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(xRectCodeRegion), INIFILE));
            
            loadCamGrids(CarrierEnum.C1, out xCamGrid1);
            loadCamGrids(CarrierEnum.C2, out xCamGrid2);

            if (!eCancel)
            {
                #region 初始化加载图片

                //string bmporgpath = $"{PathIndexStr}\\org.bmp";
                //if (System.IO.File.Exists(bmporgpath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgpath);
                //    bmpOrg?.Dispose();
                //    bmpOrg = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                //string bmporgNoTraypath = $"{PathIndexStr}\\orgNoTray.bmp";
                //if (System.IO.File.Exists(bmporgNoTraypath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgNoTraypath);
                //    bmpOrgNoTray.Dispose();
                //    bmpOrgNoTray = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                //string bmporgFlypath = $"{PathIndexStr}\\orgFly.bmp";
                //if (System.IO.File.Exists(bmporgFlypath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmporgFlypath);
                //    bmpOrgFly.Dispose();
                //    bmpOrgFly = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                bmpOrgFly?.Dispose();
                bmpOrgFly = loadImage("orgFly.bmp");

                //string bmpbase0path = $"{PathIndexStr}\\base0.bmp";
                //if (System.IO.File.Exists(bmpbase0path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase0path);
                //    bmpbase0.Dispose();
                //    bmpbase0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmpbase1path = $"{PathIndexStr}\\base1.bmp";
                //if (System.IO.File.Exists(bmpbase1path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpbase1path);
                //    bmpbase1.Dispose();
                //    bmpbase1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmptemplate0path = $"{PathIndexStr}\\template0.bmp";
                //if (System.IO.File.Exists(bmptemplate0path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate0path);
                //    bmptemplate0.Dispose();
                //    bmptemplate0 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmptemplate1path = $"{PathIndexStr}\\template1.bmp";
                //if (System.IO.File.Exists(bmptemplate1path))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmptemplate1path);
                //    bmptemplate1.Dispose();
                //    bmptemplate1 = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                //string bmpprinttemplatepath = $"{PathIndexStr}\\bmpprinttemplate.bmp";
                //if (System.IO.File.Exists(bmpprinttemplatepath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprinttemplatepath);
                //    bmpprinttemplate.Dispose();
                //    bmpprinttemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                this.bmpprinttemplate?.Dispose();
                this.bmpprinttemplate = loadImage("bmpprinttemplate.bmp");

                //string bmpDefectTemplatepath = $"{PathIndexStr}\\bmpDefectTemplate.bmp";
                //if (System.IO.File.Exists(bmpDefectTemplatepath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpDefectTemplatepath);
                //    bmpDefectTemplate.Dispose();
                //    bmpDefectTemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                this.bmpDefectTemplate?.Dispose();
                this.bmpDefectTemplate = loadImage("bmpDefectTemplate.bmp");

                //string bmpprintNoTraytemplatepath = $"{PathIndexStr}\\bmpprintNoTraytemplate.bmp";
                //if (System.IO.File.Exists(bmpprintNoTraytemplatepath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintNoTraytemplatepath);
                //    bmpprintNoTraytemplate.Dispose();
                //    bmpprintNoTraytemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}
                //string bmpprintFlytemplatepath = $"{PathIndexStr}\\bmpprintFlytemplate.bmp";
                //if (System.IO.File.Exists(bmpprintFlytemplatepath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintFlytemplatepath);
                //    bmpprintFlytemplate.Dispose();
                //    bmpprintFlytemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                this.bmpprintFlytemplate?.Dispose();
                this.bmpprintFlytemplate = loadImage("bmpprintFlytemplate.bmp");

                //string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
                //if (System.IO.File.Exists(bmpprintmaskpath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpprintmaskpath);
                //    bmpprintmask.Dispose();
                //    bmpprintmask = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                this.bmpprintmask?.Dispose();
                this.bmpprintmask = loadImage("bmpprintmask.bmp");

                //string bmpcodepath = $"{PathIndexStr}\\bmpcode.bmp";
                //if (System.IO.File.Exists(bmpcodepath))
                //{
                //    FreeImageBitmap freeImageBitmap = new FreeImageBitmap(bmpcodepath);
                //    bmpcodetemplate.Dispose();
                //    bmpcodetemplate = (Bitmap)freeImageBitmap.ToBitmap().Clone(
                //                                               new Rectangle(0, 0, freeImageBitmap.Width, freeImageBitmap.Height),
                //                                               freeImageBitmap.PixelFormat);
                //    freeImageBitmap.Dispose();
                //}

                this.bmpcodetemplate?.Dispose();
                this.bmpcodetemplate = loadImage("bmpcode.bmp");
                #endregion

                //建立所有的 Region Cells
                CreateViews();

                int iOK = ViewTrainLoad();

                if (iOK != 0)
                    JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载参数训练失败！");
            }

            InspectX3ParaClass.Instance.Initial(Path, Index, "Inspect_default_info.ini");
            InspectX3ParaClass.Instance.Load();

            FlyParaClass.Instance.Initial(Path, Index, "Fly_default_info.ini");
            FlyParaClass.Instance.Load();
        }
        public override void Save()
        {
            WriteINIValue("Recipe Basic", "xRow", xRow.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xColumn", xColumn.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xLeftTopX", xLeftTopX.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xLeftTopY", xLeftTopY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRowOffset", xRowOffset.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xColumnOffset", xColumnOffset.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChipWidth", xChipWidth.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChipHeight", xChipHeight.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            //WriteINIValue("Recipe Basic", "xRealLeftX", xRealLeftX.ToString(), INIFILE);
            //WriteINIValue("Recipe Basic", "xRealLeftY", xRealLeftY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealOffsetX", xRealOffsetX.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xRealOffsetY", xRealOffsetY.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xStageNumber", ((int)xStageNumber).ToString(), INIFILE);

            //WriteINIValue("Recipe Basic", "ptPrinttemp", PointFtoStringSimple(ptPrinttemp), INIFILE);
            //WriteINIValue("Recipe Basic", "ptPrintFlytemp", PointFtoStringSimple(ptPrintFlytemp), INIFILE);

            WriteINIValue("Recipe Basic", "xChNum", xChNum.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChValue", xChValue.ToString(), INIFILE);


            saveCamGrid(CarrierEnum.C1, xCamGrid1);
            saveCamGrid(CarrierEnum.C2, xCamGrid2);

            _bmpHolderOrg1.Save();
            _bmpHolderOrg2.Save();
            //_bmpHolderOrgFly.Save();
            saveImage(bmpOrgFly, "orgFly.bmp");

            // 建立所有的 Region Cells
            CreateViews();

            int iOK = ViewTrainLoad();
            if (iOK != 0)
                JetEazy.BasicSpace.VsMSG.Instance.Warning($"加载参数训练失败！");

            //InspectX2Class.Instance.Save();
            InspectX3ParaClass.Instance.Save();
            FlyParaClass.Instance.Save();
            //NoTrayParaClass.Instance.Save();
        }

        #region 各別子項的保存函式
        /// <summary>
        /// 保存 Region, Mask, 與 QRCode templates (包含 roi)
        /// </summary>
        public void SavePrintTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE);
            //string bmpprinttemplatepath = $"{PathIndexStr}\\bmpprinttemplate.bmp";
            //bmpprinttemplate.Save(bmpprinttemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            saveImage(bmpprinttemplate, "bmpprinttemplate.bmp");
            //string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            //bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            saveImage(bmpprintmask, "bmpprintmask.bmp");
            InspectX3ParaClass.Instance.SaveRoi();
            SaveCodeTemplate();
        }
        /// <summary>
        /// 保存 Golden Chip Template (包含 roi)
        /// </summary>
        public void SavePrintTemplateRegionTrain()
        {
            WriteINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(xRegionTrain), INIFILE);
            //string bmpDefectTemplatepath = $"{PathIndexStr}\\bmpDefectTemplate.bmp";
            //bmpDefectTemplate.Save(bmpDefectTemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            saveImage(bmpDefectTemplate, "bmpDefectTemplate.bmp");
        }
        /// <summary>
        /// 保存 邊線手拉框
        /// </summary>
        public void SaveLinesRegion()
        {
            WriteINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(xLineLeft), INIFILE);
            WriteINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(xLineTop), INIFILE);
            WriteINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(xLineRight), INIFILE);
            WriteINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(xLineBottom), INIFILE);
        }
        /// <summary>
        /// 保存 飛拍 Template (包含 roi)
        /// </summary>
        public void SavePrintFlyTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(xRectRegionPrintFly), INIFILE);
            //string bmpprintFlytemplatepath = $"{PathIndexStr}\\bmpprintFlytemplate.bmp";
            //bmpprintFlytemplate.Save(bmpprintFlytemplatepath, System.Drawing.Imaging.ImageFormat.Bmp);
            saveImage(bmpprintFlytemplate, "bmpprintFlytemplate.bmp");
            ////string bmpprintmaskpath = $"{PathIndexStr}\\bmpprintmask.bmp";
            ////bmpprintmask.Save(bmpprintmaskpath, System.Drawing.Imaging.ImageFormat.Bmp);
            ////InspectX2Class.Instance.SaveRoi();
            ////SaveCodeTemplate();
        }
        /// <summary>
        /// 保存 QrCode Template (包含 roi)
        /// </summary>
        public void SaveCodeTemplate()
        {
            WriteINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(xRectCodeRegion), INIFILE);
            //string bmpcodepath = $"{PathIndexStr}\\bmpcode.bmp";
            //bmpcodetemplate.Save(bmpcodepath, System.Drawing.Imaging.ImageFormat.Bmp);
            saveImage(bmpcodetemplate, "bmpcode.bmp");
        }
        /// <summary>
        /// 保存 LotNo
        /// </summary>
        public void SaveLotNo()
        {
            WriteINIValue("Collect", "xLotNoStr", xLotNoStr, INIFILE);
        }
        #endregion

        #region AOI_FUNCTIONS_FOR_CHIP_LOCATE_TRAIN_AND_RUN_晶粒定位的相關像測函式
        //----------------------------------------------------------------------
        // 這些應該放在 AOI MODEL 
        //----------------------------------------------------------------------
        // Recipe 是配方材料 (食譜食材)
        // AoiModel 才是主角 (廚師)
        //      出各種飯局料理 是 廚師 而不是 食譜食材
        //      一隻雞 會自己剁雞腿 變成 滷雞腿 或 炸雞腿 是非常違反常理的謬異.
        //----------------------------------------------------------------------

        public Size PrintTemplateSize
        {
            get
            {
                // LETIAN: Revised for multithread
                if (mvdprinttemp_Find != null)
                    return mvdprinttemp_Find.TemplateSize;
                return new Size(1, 1);
            }
        }
        public int PrintTempTrain()
        {
#if (OPT_OLD)
            mvdprinttemp_Find.bmpObj_Image?.Dispose();
            mvdprinttemp_Find.bmpObj_Image = (Bitmap)bmpDefectTemplate.Clone();

            //CMvdRectangleF cMvd = new CMvdRectangleF(
            //    xRegionTrain.Width / 2,
            //    xRegionTrain.Height / 2,
            //    xRegionTrain.Width,
            //    xRegionTrain.Height);

            bool bOK = mvdprinttemp_Find.HikTrainBmp();
#endif
            // LETIAN: Revised for multithread
            mvdprinttemp_Find.SetRecipeParams(InspectX3ParaClass.Instance);
            bool bOK = mvdprinttemp_Find.Train(this.bmpDefectTemplate);
            return (bOK ? 0 : -1);
        }
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

        //public int PrintTempRun(CMvdImage eMvdInput)
        //{
        //    mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
        //    mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
        //    mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
        //    mvdprinttemp_Find.xMvdRun_Image?.Dispose();
        //    mvdprinttemp_Find.xMvdRun_Image = (CMvdImage)eMvdInput.Clone();
        //    bool bOK = mvdprinttemp_Find.HikRun2();
        //    return (bOK ? 0 : -1);
        //}
        //public int PrintTempRun(CMvdImage eMvdInput, RectangleF eRectF)
        //{
        //    mvdprinttemp_Find.xMvdAngle = InspectX3ParaClass.Instance.xAngle;
        //    mvdprinttemp_Find.xMvdTolerance = InspectX3ParaClass.Instance.xTolerance;
        //    mvdprinttemp_Find.xMaxOverlap = InspectX3ParaClass.Instance.xMaxOverlap;
        //    mvdprinttemp_Find.xMvdRun_Image?.Dispose();
        //    mvdprinttemp_Find.xMvdRun_Image = (CMvdImage)eMvdInput.Clone();
        //    bool bOK = mvdprinttemp_Find.HikRun3(eRectF);
        //    return (bOK ? 0 : -1);
        //}

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
            mvdprintFlytemp_Find.xMvdTolerance = FlyParaClass.Instance.xTolerance;
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

        public bool CheckSpecialAngle(Bitmap ebmpInput, out List<CBlobInfo> m_list, out float retAngle, out PointF retCenter)
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

            m_list = new List<CBlobInfo>();
            foreach (var blob in cBlobFindToolObj.Result.BlobInfo)
            {
                if (blob.AreaF >= FlyParaClass.Instance.xBlobAreaMin && blob.AreaF <= FlyParaClass.Instance.xBlobAreaMax)
                {
                    m_list.Add(blob);
                }
            }

            if (m_list.Count == 2)
            {
                CBlobInfo b0 = m_list[0];
                CBlobInfo b1 = m_list[1];

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
        #endregion

        #region TCP_DATA_沒用到
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

        #region 统计数据
        public float[] AnalyzeDatas = new float[9];
        public void AnalyzeDatasData()
        {
            int count = Enum.GetValues(typeof(InspectReason)).Length;
            AnalyzeDatas = new float[count + 2];
            int i = 0;
            while (i < AnalyzeDatas.Length)
            {
                AnalyzeDatas[i] = 0;
                i++;
            }
        }
        /// <summary>
        /// 分析数据 返回bool
        /// </summary>
        /// <returns>true:PASS false:FAIL</returns>
        public bool AnalyzeDatasRun()
        {
            foreach (RegionCellX3Class cell in xRegionCells)
            {
                if (cell.inspectReasons.Count == 0)
                {
                    AnalyzeDatas[(int)InspectReason.PASS]++;
                    continue;
                }
                foreach (InspectReason reason in cell.inspectReasons)
                {
                    AnalyzeDatas[(int)reason]++;
                }
            }
            //芯片数
            AnalyzeDatas[AnalyzeDatas.Length - 2] = xRow * xColumn;
            //良率
            AnalyzeDatas[AnalyzeDatas.Length - 1] = AnalyzeDatas[(int)InspectReason.PASS] * 1.0f / AnalyzeDatas[AnalyzeDatas.Length - 2] * 100;
            float ins_count = AnalyzeDatas[(int)InspectReason.PASS] + AnalyzeDatas[(int)InspectReason.INS_NOOPEN];
            bool bOK = ins_count == xRow * xColumn;

            Add(bOK);
            return bOK;
        }

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
        #endregion

        #region 校正與座標轉換_GAARA_版本
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
        #endregion

        /// <summary>
        /// 只被 Load function 內部調用 海康的 Train functions. 
        /// (1) PrintTempTrain
        /// (2) PrintTempFlyTrain
        /// </summary>
        private int ViewTrainLoad()
        {
            //-----------------------------------------------------------------------------
            // 這應該設計在 AoiModel.SetRecipe(RecipeFPIX3Class recipe) 內,
            // 不應該由 Recipe 自己調用 !
            //-----------------------------------------------------------------------------
            int iret = 0; // Base0Train();

            //if (iret == 0)
            //    iret = Base1Train();

            if (iret == 0)
                iret = PrintTempTrain();

            //if (iret == 0)
            //{
            //    //计算初始位置
            //    ProcessRunFPIClass.Instance.cMvdInput = BitmapToCMvdImage(bmpOrg);
            //    ProcessRunFPIClass.Instance.RunRecipe();
            //    bool bFoundALIGNERR = false;
            //    foreach (RegionCellX3Class cell in xRegionCells)
            //    {
            //        if (cell.inspectReason == InspectReason.INS_ALIGNERR)
            //        {
            //            bFoundALIGNERR = true;
            //            break;
            //        }
            //    }
            //    if (bFoundALIGNERR)
            //    {
            //        iret = -1;
            //    }
            //}

            if (iret == 0)
                iret = PrintTempFlyTrain();

            return iret;
        }

        /// <summary>
        /// 創建 RegionCells.
        /// (這個應該是放在 AOI MODEL 內)
        /// </summary>
        public void CreateViews()
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
        }

        #region OLD_CODE_沒用到
#if (GAARA_OLD_BACKUP)
        public void CreateViews()
        {
            xRegionCells.Clear();

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

        }

#endif
        #endregion

        #region PRIVATE_UTIL_FUNCTIONS
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
        #endregion
    }
}
