using Common.RecipeSpace;
using Eazy_Project_III;
using EzAoiEmptyTrayInspector.Model;
using JetEazy;
using JetEazy.FormSpace;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.AoiModel;
using LaserAlignDX.BasicSpace;
using LaserAlignDX.Mvc.Model.Recipe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
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
            disposeAllTemplates();
        }

        /// <summary>
        /// 2025-09-21 支援不同載台 可以混搭黑白背景
        /// </summary>
        public CarrierEnum ActiveCarrierID
        {
            get;
            private set;
        }
        public bool ChangeActiveCarrier(CarrierEnum C, bool forceToReload = false)
        {
            bool isChanged = false;

            if (ActiveCarrierID != C)
            {
                ActiveCarrierID = C;
                isChanged = true;
            }

            if (isChanged || forceToReload)
                Load();

            return isChanged;
        }

        #region CARRIER_TAG_載台標籤
        string _CARRIER_TAG => ActiveCarrierID != CarrierEnum.C1 ? $"@{ActiveCarrierID}" : "";
        #endregion

        #region RUNTIME_REGION_CELLS
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

        #region BMP_ORG_HOLDERS
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台1 的 bmpOrg
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrg1 = new RcpBmpHolder("org");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台2 的 bmpOrg
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrg2 = new RcpBmpHolder("org2");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 飛拍 的 bmpOrgFly
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrgFly = new RcpBmpHolder("orgFly");
        /// <summary>
        /// 釋放所有的 BmpOrgs (org, org2, orgFly)
        /// </summary>
        void disposeBmpOrgs()
        {
            _bmpHolderOrg1?.Dispose();
            _bmpHolderOrg2?.Dispose();
            _bmpHolderOrgFly?.Dispose();
            _bmpOrgFlyRuntime?.Dispose();
        }
        #endregion

        #region BMP_ORG_載台1_載台2
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
        #endregion

        #region BMP_ORG_飛拍
        /// <summary>
        /// 用來維持 舊接口 相容性 之 使用模式
        /// </summary>
        Bitmap _bmpOrgFlyRuntime = null;
        /// <summary>
        /// 舊接口: Caller 會管理 bmpOrgFly 生命週期
        /// </summary>
        public Bitmap bmpOrgFly
        {
            get
            {
                if (_bmpOrgFlyRuntime == null)
                    _bmpOrgFlyRuntime = (Bitmap)_bmpHolderOrgFly.Peek()?.Clone();
                return _bmpOrgFlyRuntime;
            }
            set
            {
                if (_bmpOrgFlyRuntime != value && value != null)
                {
                    var old = _bmpOrgFlyRuntime;
                    _bmpOrgFlyRuntime = value;
                    _bmpHolderOrgFly.TakeOver((Bitmap)_bmpOrgFlyRuntime.Clone());
                    old?.Dispose();
                }
            }
        }
        /// <summary>
        /// bmpOrgFly 一般只用於參數編輯時期, 跑線時可以釋放
        /// </summary>
        public void ReleaseBmpOrgFly(bool save)
        {
            // bmpOrgFly 一般只用於參數編輯時期, 跑線時可以釋放
            if (save)
            {
                _bmpHolderOrgFly.Save();
            }
            _bmpHolderOrgFly?.Dispose();
        }
        #endregion

        #region 模板區_DTO
        DtoBmpTemplate _dtoGoldenRegionTemplate = new DtoBmpTemplate("bmpPrintTemplate", "Recipe Basic", "xRectRegionPrint");
        DtoBmpTemplate _dtoGoldenChipTemplate = new DtoBmpTemplate("bmpDefectTemplate", "Recipe Basic", "xRegionTrain");
        DtoBmpTemplate _dtoGoldenMaskTemplate = new DtoBmpTemplate("bmpPrintMask", "Recipe Basic", "xRectDummy");
        DtoBmpTemplate _dtoQrCodeTemplate = new DtoBmpTemplate("bmpCode", "Recipe Basic", "xRectCodeRegion");
        DtoBmpTemplate _dtoFlyAoiTemplate = new DtoBmpTemplate("bmpPrintFlyTemplate", "Recipe Basic", "xRectRegionPrintFly");
        void disposeAllTemplates()
        {
            _dtoGoldenRegionTemplate?.Dispose();
            _dtoGoldenRegionTemplate = null;

            _dtoGoldenChipTemplate?.Dispose();
            _dtoGoldenChipTemplate = null;

            _dtoGoldenMaskTemplate?.Dispose();
            _dtoGoldenMaskTemplate = null;

            _dtoQrCodeTemplate?.Dispose();
            _dtoQrCodeTemplate = null;

            _dtoFlyAoiTemplate?.Dispose();
            _dtoFlyAoiTemplate = null;
        }
        #endregion

        #region 模板區_舊接口
        /// <summary>
        /// Golden Region Cell (晶粒區域粗框)
        /// </summary>
        public RectangleF xRectRegionPrint
        {
            get => _dtoGoldenRegionTemplate.RectF;
            set => _dtoGoldenRegionTemplate.RectF = value;
        }
        /// <summary>
        /// Golden Region Bitmap (晶粒區域粗框)
        /// </summary>
        public Bitmap bmpprinttemplate
        {
            get => _dtoGoldenRegionTemplate.Bmp;
            set => _dtoGoldenRegionTemplate.Bmp = value;
        }
        /// <summary>
        /// Golden Chip Rect
        /// 更精確(內縮)的晶粒矩形區域
        /// 位於 Golden Region (xRectRegionPrint) 之內
        /// 相對於 xRectRegionPrint 的左上角為零點
        /// </summary>
        public RectangleF xRegionTrain
        {
            get => _dtoGoldenChipTemplate.RectF;
            set => _dtoGoldenChipTemplate.RectF = value;
        }
        /// <summary>
        /// 這個其實就是 Golden Chip Template Bitmap
        /// </summary>
        public Bitmap bmpDefectTemplate
        {
            get => _dtoGoldenChipTemplate.Bmp;
            set => _dtoGoldenChipTemplate.Bmp = value;
        }
        /// <summary>
        /// Mask Bitmap
        /// </summary>
        public Bitmap bmpprintmask
        {
            get => _dtoGoldenMaskTemplate.Bmp; 
            set => _dtoGoldenMaskTemplate.Bmp = value;
        }
        /// <summary>
        /// 二維碼框選區
        /// </summary>
        public RectangleF xRectCodeRegion
        {
            get => _dtoQrCodeTemplate.RectF;
            set => _dtoQrCodeTemplate.RectF = value;
        }
        /// <summary>
        /// 二維碼影像樣本
        /// </summary>
        public Bitmap bmpcodetemplate
        {
            get => _dtoQrCodeTemplate.Bmp;
            set => _dtoQrCodeTemplate.Bmp = value;
        }
        /// <summary>
        /// 飛拍 矩形區塊
        /// </summary>
        public RectangleF xRectRegionPrintFly
        {
            get => _dtoFlyAoiTemplate.RectF;
            set => _dtoFlyAoiTemplate.RectF = value;
        }
        /// <summary>
        /// 飛拍 模板 圖像
        /// </summary>
        public Bitmap bmpprintFlytemplate
        {
            get => _dtoFlyAoiTemplate.Bmp;
            set => _dtoFlyAoiTemplate.Bmp = value;
        }
        #endregion

        #region 邊線_手拉框區塊_LINE_BORDER_BOXES
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

        internal void LoadLineBorderRects(string carrierTag)
        {
            xLineLeft = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineTop = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineRight = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            xLineBottom = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
        }
        public void SaveLineBorderRects(string carrierTag)
        {
            WriteINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(xLineLeft), INIFILE);
            WriteINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(xLineTop), INIFILE);
            WriteINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(xLineRight), INIFILE);
            WriteINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(xLineBottom), INIFILE);
        }
        #endregion

        public string xLotNoStr = "NONE";

        #region 參數區_PARA_GRID
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

        #region 參數區_其他子群
        public InspectX3ParaClass InspectParams => InspectX3ParaClass.Instance;
        public FlyParaClass FlyAoiParams => FlyParaClass.Instance;
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

            #region NOT_USEFUL
            PassCount = int.Parse(ReadINIValue("Recipe Basic", "PassCount", "0", INIFILE));
            NGCount = int.Parse(ReadINIValue("Recipe Basic", "NGCount", "0", INIFILE));
            #endregion

            xChNum = int.Parse(ReadINIValue("Recipe Basic", "xChNum", "1", INIFILE));
            xChValue = int.Parse(ReadINIValue("Recipe Basic", "xChValue", "255", INIFILE));

            //xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRegionTrain = StringtoRectF(ReadINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectRegionPrintFly = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineLeft = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineTop = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineRight = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineBottom = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));

            loadCamGrids(CarrierEnum.C1, out xCamGrid1);
            loadCamGrids(CarrierEnum.C2, out xCamGrid2);

            //(LD1) 各種模板 (根據載台號 載入不同對應的設定值)
            _dtoGoldenRegionTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoGoldenChipTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoGoldenMaskTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoQrCodeTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            //(LD2) 飛拍模板 (目前不隨著載台顏色改變)
            _dtoFlyAoiTemplate.Load(INIFILE);
            //(LD3) 邊線框 (根據載台號 載入不同對應的設定值)
            LoadLineBorderRects(_CARRIER_TAG);

            if (!eCancel)
            {
                #region 舊代碼_初始化加载图片
                //bmpOrgFly?.Dispose();
                //bmpOrgFly = RcpBmpHolder.LoadRecipeImage("orgFly");
                //this.bmpprinttemplate?.Dispose();
                //this.bmpprinttemplate = loadImage("bmpprinttemplate.bmp");
                //this.bmpDefectTemplate?.Dispose();
                //this.bmpDefectTemplate = loadImage("bmpDefectTemplate.bmp");
                //this.bmpprintFlytemplate?.Dispose();
                //this.bmpprintFlytemplate = loadImage("bmpprintFlytemplate.bmp");
                //this.bmpprintmask?.Dispose();
                //this.bmpprintmask = loadImage("bmpprintmask.bmp");
                //this.bmpcodetemplate?.Dispose();
                //this.bmpcodetemplate = loadImage("bmpcode.bmp");
                #endregion

                //建立所有的 Region Cells (應該放在 Aoi Model)
                CreateViews();
                ViewTrainLoad();
            }

            //(LD4) 晶粒檢測參數 (根據載台號 載入不同對應的設定值)
            InspectParams.Initial(Path, Index, $"Inspect_default_info{_CARRIER_TAG}.ini");
            InspectParams.Load();

            //(LD5) 飛拍檢測參數 (目前不隨著載台顏色改變)
            FlyAoiParams.Initial(Path, Index, "Fly_default_info.ini");
            FlyAoiParams.Load();
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
            WriteINIValue("Recipe Basic", "xChNum", xChNum.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "xChValue", xChValue.ToString(), INIFILE);

            saveCamGrid(CarrierEnum.C1, xCamGrid1);
            saveCamGrid(CarrierEnum.C2, xCamGrid2);

            //==============================================================
            // 以下區塊 是調用 各別保存 函式
            //--------------------------------------------------------------
            ////(1) 各種模板 (根據載台號 載入不同對應的設定值)
            //_dtoGoldenRegionTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            //_dtoGoldenChipTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            //_dtoGoldenMaskTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            //_dtoQrCodeTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            //--------------------------------------------------------------
            ////(2) 飛拍模板 (目前不隨著載台顏色改變)
            //_dtoFlyAoiTemplate.Save(INIFILE);
            //--------------------------------------------------------------
            ////(3) 邊線框 (根據載台號 載入不同對應的設定值)
            //SaveLineBorderRects(_CARRIER_TAG);
            //==============================================================

            _bmpHolderOrg1.Save();
            _bmpHolderOrg2.Save();
            _bmpHolderOrgFly.Save();

            // 建立所有的 Region Cells (應該放在 Aoi Model)
            CreateViews();
            ViewTrainLoad();

            InspectParams.Save();
            FlyAoiParams.Save();
        }

        #region 子項模板參數_保存函式
        public void SaveTemplate(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            name = name.ToUpper();

            if (name.Contains("REGION"))
            {
                // Golden Region 模板 (根據載台號 存入不同對應的設定值)
                _dtoGoldenRegionTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            }
            if (name.Contains("CHIP"))
            {
                // Golden Chip 模板 (根據載台號 存入不同對應的設定值)
                _dtoGoldenChipTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            }
            if (name.Contains("MASK"))
            {
                // Defect Inspect Mask  (根據載台號 存入不同對應的設定值)
                _dtoGoldenMaskTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
                this.InspectParams.SaveMaskRects();
            }
            if (name.Contains("QRCODE"))
            {
                // QR Code 模板 (根據載台號 存入不同對應的設定值)
                _dtoQrCodeTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            }
            if (name.Contains("LINEBORDER"))
            {
                // 邊線框 (根據載台號 存入不同對應的設定值)
                SaveLineBorderRects(_CARRIER_TAG);
            }
            if (name.Contains("FLY"))
            {
                // 飛拍模板 (目前不隨著載台顏色改變)
                _dtoFlyAoiTemplate.Save(INIFILE);
            }
        }
        #endregion

        #region 子項參數_保存函式_舊接口
        /// <summary>
        /// 保存 Golden Region, Mask, and QRCode templates
        /// </summary>
        public void SavePrintTemplate()
        {
            //WriteINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(xRectRegionPrint), INIFILE);
            //saveImage(bmpprinttemplate, "bmpprinttemplate.bmp");
            //saveImage(bmpprintmask, "bmpprintmask.bmp");
            //this.InspectParams.SaveMaskRects();
            //SaveCodeTemplate();

            // 各式模板 (根據載台號 存入不同對應的設定值)
            SaveTemplate("REGION_CHIP_MASK_QRCODE");
        }
        /// <summary>
        /// 保存 Golden Chip Template
        /// </summary>
        public void SavePrintTemplateRegionTrain()
        {
            //WriteINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(xRegionTrain), INIFILE);
            //saveImage(bmpDefectTemplate, "bmpDefectTemplate.bmp");

            // 晶粒模板 (根據載台號 存入不同對應的設定值)
            SaveTemplate("CHIP");
        }
        /// <summary>
        /// 保存 QRCode Template
        /// </summary>
        public void SaveCodeTemplate()
        {
            // QRCODE 模板 (根據載台號 存入不同對應的設定值)
            SaveTemplate("QRCODE");
        }
        /// <summary>
        /// 保存 FlyAoi Template
        /// </summary>
        public void SavePrintFlyTemplate()
        {
            // 飛拍模板 (目前不隨著載台顏色改變)
            SaveTemplate("FLY");
        }
        /// <summary>
        /// 保存 Line Border Rectangles
        /// </summary>
        public void SaveLinesRegion()
        {
            // 邊線框 (根據載台號 存入不同對應的設定值)
            SaveTemplate("LINE_BORDER");
        }
        /// <summary>
        /// 保存 LotNo
        /// </summary>
        public void SaveLotNo()
        {
            WriteINIValue("Collect", "xLotNoStr", xLotNoStr, INIFILE);
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

        #region AOI_FUNCTIONS_FOR_CHIP_LOCATE_TRAIN_AND_RUN_晶粒定位的相關像測函式
        //----------------------------------------------------------------------
        // 這些應該放在 AOI MODEL 
        //----------------------------------------------------------------------
        // Recipe 是配方材料 (食譜食材)
        // AoiModel 才是主角 (廚師)
        //      出各種飯局料理 是 廚師 而不是 食譜食材
        //      一隻雞 會自己剁雞腿 變成 滷雞腿 或 炸雞腿 是非常違反常理的謬異.
        //----------------------------------------------------------------------

        /// <summary>
        /// 這其實等同於 bmpDefectTemplate.Size
        /// </summary>
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
            mvdprinttemp_Find.SetRecipeParams(this.InspectParams);
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

        #region NO_USE_本專案沒用到_但是這應該放在_AOI_RESULT_區域
        private int PassCount = 0;
        private int NGCount = 0;
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
            int err = 0;

            if (err == 0)
            {
                err = PrintTempTrain();
                if (err != 0)
                    VsMessageBox.Warning("加載 (晶粒匹配) 參數 訓練失敗!");
            }

            if (err == 0)
            {
                err = PrintTempFlyTrain();
                if (err != 0)
                    VsMessageBox.Warning("加載 飛拍 參數 訓練失敗!");
            }

            return err;
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

        #region MISC_UTIL_FUNCTIONS
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


    public class FlyParaClass : RecipeBaseClass
    {
        const int POINT_COUNT = 8;

        #region SINGLETON
        protected FlyParaClass()
        {

        }
        private static FlyParaClass _instance = null;
        #endregion

        public static FlyParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlyParaClass();
                return _instance;
            }
        }

        const string _Cat1 = "A01.基础设置";
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A01.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A02.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A03.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendx { get; set; } = 20;
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A04.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(true)]
        public int xExtendy { get; set; } = 20;

        const string _Cat2 = "A02.双头吸嘴找角度设置";
        [CategoryAttribute(_Cat2), DescriptionAttribute("两个吸嘴的计算开启")]
        [DisplayName("A00.开启双头计算")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsOpenMuit { get; set; } = false;

        [CategoryAttribute(_Cat2), DescriptionAttribute("二值化的阈值")]
        [DisplayName("A01.灰阶阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;
        [CategoryAttribute(_Cat2), DescriptionAttribute("寻找的特征选择黑色还是白色 吸嘴是黑色选择黑色")]
        [DisplayName("A02.检测模式")]
        [TypeConverter(typeof(JzEnumConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public BlobMode xBlobMode { get; set; } = BlobMode.Black;
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最小的面积")]
        [DisplayName("A03.Blob面积最小值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMin { get; set; } = 5000;
        [CategoryAttribute(_Cat2), DescriptionAttribute("特征二值化后最大的面积")]
        [DisplayName("A03.Blob面积最大值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 999999999)]
        [Browsable(true)]
        public int xBlobAreaMax { get; set; } = 99999999;

        [CategoryAttribute(_Cat2), DescriptionAttribute("true水平 false垂直")]
        [DisplayName("A04.吸嘴是否水平校正")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public bool xIsShuiPing { get; set; } = true;

        public PointF[] ptsOffset = new PointF[POINT_COUNT];
        public PointF[] ptsOffset2 = new PointF[POINT_COUNT];

        public override void Load(bool eCancel = false)
        {
            xTolerance = float.Parse(ReadINIValue("Fly", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Fly", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Fly", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Fly", "xExtendy", "20", INIFILE));

            xIsOpenMuit = ReadINIValue("Basic", "xIsOpenMuit", "0", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Basic", "xThresholdValue", "128", INIFILE));
            xBlobMode = (BlobMode)int.Parse(ReadINIValue("Basic", "xBlobMode", "1", INIFILE));
            xBlobAreaMin = int.Parse(ReadINIValue("Basic", "xBlobAreaMin", "10", INIFILE));
            xBlobAreaMax = int.Parse(ReadINIValue("Basic", "xBlobAreaMax", "20000", INIFILE));
            xIsShuiPing = ReadINIValue("Basic", "xIsShuiPing", "1", INIFILE) == "1";

            int i = 0;
            while (i < POINT_COUNT)
            {
                ptsOffset[i] = StringtoPointF(ReadINIValue("FlyOffset", $"ptsOffset_{i}", $"0,0", INIFILE));
                ptsOffset2[i] = StringtoPointF(ReadINIValue("FlyOffset2", $"ptsOffset2_{i}", $"0,0", INIFILE));

                i++;
            }
        }
        public override void Save()
        {
            WriteINIValue("Fly", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Fly", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Fly", "xExtendy", xExtendy.ToString(), INIFILE);

            WriteINIValue("Basic", "xIsOpenMuit", (xIsOpenMuit ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobMode", ((int)xBlobMode).ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMin", xBlobAreaMin.ToString(), INIFILE);
            WriteINIValue("Basic", "xBlobAreaMax", xBlobAreaMax.ToString(), INIFILE);
            WriteINIValue("Basic", "xIsShuiPing", (xIsShuiPing ? "1" : "0"), INIFILE);

            int i = 0;
            while (i < POINT_COUNT)
            {
                WriteINIValue("FlyOffset", $"ptsOffset_{i}", PointFtoStringSimple(ptsOffset[i]), INIFILE);
                WriteINIValue("FlyOffset2", $"ptsOffset2_{i}", PointFtoStringSimple(ptsOffset2[i]), INIFILE);

                i++;
            }

        }

    }


    public class InspectX3ParaClass : RecipeBaseClass
    {
        #region SINGLETON
        protected InspectX3ParaClass()
        {

        }
        private static InspectX3ParaClass _instance = null;
        #endregion

        public static InspectX3ParaClass Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InspectX3ParaClass();
                return _instance;
            }
        }

        const string _Cat0 = "A00.启用设置";
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A01.开启尺寸测量")]
        [Browsable(true)]
        public bool bOpenLineMeasure { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A02.开启缺陷检测")]
        //[TypeConverter(typeof(NumericUpDownTypeConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckInspect { get; set; } = false;
        [CategoryAttribute(_Cat0), DescriptionAttribute("")]
        [DisplayName("A03.开启尺寸偏移检测")]
        //[TypeConverter(typeof(JzEnumConverter))]
        //[Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public bool bCheckMeasureOffset { get; set; } = false;

        #region 晶粒定位
        const string _Cat1 = "A01.晶粒定位";
        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的演算法")]
        [DisplayName("A00.演算法")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public MatchAlgorithmEnum xAlgorithm { get; set; } = MatchAlgorithmEnum.GridMatch;

        [CategoryAttribute(_Cat1), DescriptionAttribute("搭配 '格點晶粒' 匹配演算法的 '格點門限值'")]
        [DisplayName("A01.格點門限")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xGridPadThreshold { get; set; } = 0;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的相似程度")]
        [DisplayName("A02.相似度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1, 0.1f, 2)]
        [Browsable(true)]
        public float xTolerance { get; set; } = 0.5f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的允许的角度")]
        [DisplayName("A03.角度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 360, 1f, 2)]
        [Browsable(true)]
        public float xAngle { get; set; } = 30f;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大X方向的像素")]
        [DisplayName("A04.外扩X")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendx { get; set; } = 20;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围扩大Y方向的像素")]
        [DisplayName("A05.外扩Y")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 1f, 2)]
        [Browsable(false)]
        public int xExtendy { get; set; } = 20;

        [CategoryAttribute(_Cat1), DescriptionAttribute("模板轮廓匹配的搜寻范围内重叠率")]
        [DisplayName("A06.匹配重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 100)]
        [Browsable(true)]
        public int xMaxOverlap { get; set; } = 80;

        [CategoryAttribute(_Cat1), DescriptionAttribute("在搜索范围内重合的比例")]
        [DisplayName("A07.格点重叠率")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1)]
        [Browsable(true)]
        public float xChipOverlap { get; set; } = 0.5f;

        #endregion

        #region 找直线的参数

        const string _Cat2 = "A02.尺寸检测参数设置";
        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.测量方式")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(false)]
        public MeasureFindLineType MFLType { get; set; } = MeasureFindLineType.FindLineType_v1;

        [CategoryAttribute(_Cat2)]
        [DisplayName("A00.載台背景")]
        [TypeConverter(typeof(JzEnumConverter))]
        [Browsable(true)]
        public EdgeBackGroundType xCarrierBackground { get; set; }
        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A01.左边查找方向")]
        [Browsable(false)]
        public bool bPositive0 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A02.左边极性")]
        [Browsable(false)]
        public bool bEdgePolarity0 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A03.上边查找方向")]
        [Browsable(false)]
        public bool bPositive1 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A04.上边极性")]
        [Browsable(false)]
        public bool bEdgePolarity1 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从左到右 true正向 false反向")]
        [DisplayName("A05.右边查找方向")]
        [Browsable(false)]
        public bool bPositive2 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A06.右边极性")]
        [Browsable(false)]
        public bool bEdgePolarity2 { get; set; } = true;

        [CategoryAttribute(_Cat2), DescriptionAttribute("从上到下 true正向 false反向")]
        [DisplayName("A07.下边查找方向")]
        [Browsable(false)]
        public bool bPositive3 { get; set; } = true;
        [CategoryAttribute(_Cat2), DescriptionAttribute("true白到黑 false黑到白")]
        [DisplayName("A08.下边极性")]
        [Browsable(false)]
        public bool bEdgePolarity3 { get; set; } = true;


        #endregion

        #region 缺陷检测设置

        const string _Cat3 = "A03.缺陷检测参数设置";

        [CategoryAttribute(_Cat3), DescriptionAttribute("")]
        [DisplayName("A00.二值化阈值")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 255)]
        [Browsable(true)]
        public int xThresholdValue { get; set; } = 128;

        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A01.缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A02.缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A03.缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(true)]
        public float xCharArea { get; set; } = 30.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A04.背景缺陷宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudWidth { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A05.背景缺陷高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudHeight { get; set; } = 15.1f;
        [CategoryAttribute(_Cat3), DescriptionAttribute("单位pixel")]
        [DisplayName("A06.背景缺陷面积")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 2)]
        [Browsable(false)]
        public float xBackgroudArea { get; set; } = 30.1f;

        /// <summary>
        /// Mask Rectangles 的數量
        /// </summary>
        [Browsable(false)]
        public int RoiCount => rectangles.Count;
        /// <summary>
        /// Mask Rectangles
        /// </summary>
        [Browsable(false)]
        public List<RectangleF> rectangles { get; set; } = new List<RectangleF>();

        #endregion

        #region 尺寸宽度spec

        const string _Cat4 = "A04.尺寸规格设置";

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01.标准宽度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthStand { get; set; } = 9f;

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01a.宽度上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthUpper { get; set; } = 0.05f;

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A01b.宽度下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mWidthLower { get; set; } = 0.05f;

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02.标准高度")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightStand { get; set; } = 9.9f;

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02a.高度上公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightUpper { get; set; } = 0.05f;

        [CategoryAttribute(_Cat4), DescriptionAttribute("单位mm")]
        [DisplayName("A02b.高度下公差")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float mHeightLower { get; set; } = 0.05f;

        #endregion

        #region 尺寸偏移spec

        const string _Cat5 = "A05.尺寸偏移规格设置";

        [CategoryAttribute(_Cat5), DescriptionAttribute("单位mm")]
        [DisplayName("A01.X方向偏移")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float XOffset { get; set; } = 0.05f;

        [CategoryAttribute(_Cat5), DescriptionAttribute("单位mm")]
        [DisplayName("A02.Y方向偏移")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 99999999, 0.1f, 3)]
        [Browsable(true)]
        public float YOffset { get; set; } = 0.05f;

        #endregion

        public override void Load(bool eCancel = false)
        {
            xAlgorithm = (MatchAlgorithmEnum)int.Parse(ReadINIValue("Basic", "xAlgorithm", "0", INIFILE));
            xTolerance = float.Parse(ReadINIValue("Basic", "xTolerance", "0.5", INIFILE));
            xAngle = float.Parse(ReadINIValue("Basic", "xAngle", "30", INIFILE));
            xExtendx = int.Parse(ReadINIValue("Basic", "xExtendx", "20", INIFILE));
            xExtendy = int.Parse(ReadINIValue("Basic", "xExtendy", "20", INIFILE));
            xMaxOverlap = int.Parse(ReadINIValue("Basic", "xMaxOverlap", "80", INIFILE));
            xChipOverlap = float.Parse(ReadINIValue("Basic", "xChipOverlap", "0.5", INIFILE));
            xGridPadThreshold = int.Parse(ReadINIValue("Basic", "xGridPadThreshold", "0", INIFILE));

            bCheckInspect = ReadINIValue("Inspect", "bCheckInspect", "1", INIFILE) == "1";
            xThresholdValue = int.Parse(ReadINIValue("Inspect", "xThresholdValue", "128", INIFILE));
            xCharWidth = float.Parse(ReadINIValue("Inspect", "xCharWidth", "15.1", INIFILE));
            xCharHeight = float.Parse(ReadINIValue("Inspect", "xCharHeight", "15.1", INIFILE));
            xCharArea = float.Parse(ReadINIValue("Inspect", "xCharArea", "30.1", INIFILE));
            xBackgroudWidth = float.Parse(ReadINIValue("Inspect", "xBackgroudWidth", "15.1", INIFILE));
            xBackgroudHeight = float.Parse(ReadINIValue("Inspect", "xBackgroudHeight", "15.1", INIFILE));
            xBackgroudArea = float.Parse(ReadINIValue("Inspect", "xBackgroudArea", "30.1", INIFILE));

            //RoiCount = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            //int i = 0;
            //rectangles.Clear();
            //while (i < RoiCount)
            //{
            //    RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
            //    rectangles.Add(rectf);
            //    i++;
            //}

            bOpenLineMeasure = ReadINIValue("Basic", "bOpenLineMeasure", "0", INIFILE) == "1";
            bCheckMeasureOffset = ReadINIValue("Basic", "bCheckMeasureOffset", "0", INIFILE) == "1";

            MFLType = (MeasureFindLineType)int.Parse(ReadINIValue("Basic", "MFLType", "0", INIFILE));
            xCarrierBackground = (EdgeBackGroundType)int.Parse(ReadINIValue("Basic", "CarrierBackground", "0", INIFILE));

            bPositive0 = ReadINIValue("Basic", "bPositive0", "1", INIFILE) == "1";
            bPositive1 = ReadINIValue("Basic", "bPositive1", "1", INIFILE) == "1";
            bPositive2 = ReadINIValue("Basic", "bPositive2", "1", INIFILE) == "1";
            bPositive3 = ReadINIValue("Basic", "bPositive3", "1", INIFILE) == "1";
            bEdgePolarity0 = ReadINIValue("Basic", "bEdgePolarity0", "1", INIFILE) == "1";
            bEdgePolarity1 = ReadINIValue("Basic", "bEdgePolarity1", "1", INIFILE) == "1";
            bEdgePolarity2 = ReadINIValue("Basic", "bEdgePolarity2", "1", INIFILE) == "1";
            bEdgePolarity3 = ReadINIValue("Basic", "bEdgePolarity3", "1", INIFILE) == "1";

            mWidthStand = float.Parse(ReadINIValue("Basic", "mWidthStand", "9", INIFILE));
            mWidthUpper = float.Parse(ReadINIValue("Basic", "mWidthUpper", "0.05", INIFILE));
            mWidthLower = float.Parse(ReadINIValue("Basic", "mWidthLower", "0.05", INIFILE));
            mHeightStand = float.Parse(ReadINIValue("Basic", "mHeightStand", "9.9", INIFILE));
            mHeightUpper = float.Parse(ReadINIValue("Basic", "mHeightUpper", "0.05", INIFILE));
            mHeightLower = float.Parse(ReadINIValue("Basic", "mHeightLower", "0.05", INIFILE));
            XOffset = float.Parse(ReadINIValue("Basic", "XOffset", "0.05", INIFILE));
            YOffset = float.Parse(ReadINIValue("Basic", "YOffset", "0.05", INIFILE));

            LoadMaskRects();
        }
        public override void Save()
        {
            WriteINIValue("Basic", "xAlgorithm", ((int)xAlgorithm).ToString(), INIFILE);
            WriteINIValue("Basic", "xTolerance", xTolerance.ToString(), INIFILE);
            WriteINIValue("Basic", "xAngle", xAngle.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendx", xExtendx.ToString(), INIFILE);
            WriteINIValue("Basic", "xExtendy", xExtendy.ToString(), INIFILE);
            WriteINIValue("Basic", "xMaxOverlap", xMaxOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xChipOverlap", xChipOverlap.ToString(), INIFILE);
            WriteINIValue("Basic", "xGridPadThreshold", xGridPadThreshold.ToString(), INIFILE);

            WriteINIValue("Inspect", "bCheckInspect", (bCheckInspect ? "1" : "0"), INIFILE);
            WriteINIValue("Inspect", "xThresholdValue", xThresholdValue.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharWidth", xCharWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharHeight", xCharHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xCharArea", xCharArea.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudWidth", xBackgroudWidth.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudHeight", xBackgroudHeight.ToString(), INIFILE);
            WriteINIValue("Inspect", "xBackgroudArea", xBackgroudArea.ToString(), INIFILE);

            WriteINIValue("Basic", "bOpenLineMeasure", (bOpenLineMeasure ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bCheckMeasureOffset", (bCheckMeasureOffset ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "MFLType", ((int)MFLType).ToString(), INIFILE);
            WriteINIValue("Basic", "CarrierBackground", ((int)xCarrierBackground).ToString(), INIFILE);
            WriteINIValue("Basic", "bPositive0", (bPositive0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive1", (bPositive1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive2", (bPositive2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bPositive3", (bPositive3 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity0", (bEdgePolarity0 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity1", (bEdgePolarity1 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity2", (bEdgePolarity2 ? "1" : "0"), INIFILE);
            WriteINIValue("Basic", "bEdgePolarity3", (bEdgePolarity3 ? "1" : "0"), INIFILE);

            WriteINIValue("Basic", "mWidthStand", mWidthStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthUpper", mWidthUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mWidthLower", mWidthLower.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightStand", mHeightStand.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightUpper", mHeightUpper.ToString(), INIFILE);
            WriteINIValue("Basic", "mHeightLower", mHeightLower.ToString(), INIFILE);
            WriteINIValue("Basic", "XOffset", XOffset.ToString(), INIFILE);
            WriteINIValue("Basic", "YOffset", YOffset.ToString(), INIFILE);

            //>>> SaveMaskRects();
        }

        internal void LoadMaskRects()
        {
            var maskRects = new List<RectangleF>();
            int count = int.Parse(ReadINIValue("Inspect", "RoiCount", "0", INIFILE));
            for (int i = 0; i < count; i++)
            {
                RectangleF rectf = StringtoRectF(ReadINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(new RectangleF(0, 0, 10, 10)), INIFILE));
                maskRects.Add(rectf);
            }
            this.rectangles = maskRects;
        }
        internal void SaveMaskRects()
        {
            var maskRects = this.rectangles;
            int count = maskRects != null ? maskRects.Count : 0;
            WriteINIValue("Inspect", "RoiCount", count.ToString(), INIFILE);
            for (int i = 0; i < count; i++)
            {
                WriteINIValue("Inspect", $"Roi{i.ToString()}", RectFtoStringSimple(maskRects[i]), INIFILE);
            }
        }
    }


    public class LineScanCalibrateClass : RecipeBaseClass
    {
        const int POINT_COUNT = 4;

        JetEazy.BasicSpace.CAoiCalibration cAoiCalibration = new JetEazy.BasicSpace.CAoiCalibration();
        PointF[,] _v1 = new PointF[2, 2];
        PointF[,] _w1 = new PointF[2, 2];

        //JetEazy.BasicSpace.CAoiCalibration cAoiCalibration2 = new JetEazy.BasicSpace.CAoiCalibration();
        //PointF[,] _v2 = new PointF[2, 2];
        //PointF[,] _w2 = new PointF[2, 2];

        public LineScanCalibrateClass()
        {

        }
        //private static LineScanCalibrateClass _instance = null;
        //public static LineScanCalibrateClass Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //            _instance = new LineScanCalibrateClass();
        //        return _instance;
        //    }
        //}
        public override void Initial(string epath, int ercpindex, string enamefile)
        {
            base.Initial(epath, ercpindex, enamefile);
            string dir = $"{Path}\\Calibration";
            INIFILE = $"{dir}\\{Name}";
        }
        public override void ChangeIndex(int eindex)
        {
            //base.ChangeIndex(eindex);
        }
        public PointF ViewToWorld(PointF ptview)
        {
            PointF ptworld = new PointF(ptview.X, ptview.Y);
            cAoiCalibration.TransformViewToWorld(ptview, out ptworld);
            return ptworld;
        }
        public PointF WorldToView(PointF ptworld)
        {
            PointF ptview = new PointF(ptworld.X, ptworld.Y);
            cAoiCalibration.TransformViewToWorld(ptworld, out ptview);
            return ptview;
        }
        //public PointF T2ViewToWorld(PointF ptview)
        //{
        //    PointF ptworld = new PointF(ptview.X, ptview.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptview, out ptworld);
        //    return ptworld;
        //}
        //public PointF T2WorldToView(PointF ptworld)
        //{
        //    PointF ptview = new PointF(ptworld.X, ptworld.Y);
        //    cAoiCalibration2.TransformViewToWorld(ptworld, out ptview);
        //    return ptview;
        //}

        public PointF[] ptsview = new PointF[POINT_COUNT];
        public PointF[] ptsworld = new PointF[POINT_COUNT];
        //public PointF[] pts2view = new PointF[POINT_COUNT];
        //public PointF[] pts2world = new PointF[POINT_COUNT];

        //public override void Initial(string epath, int ercpindex, string enamefile)
        //{
        //    base.Initial(epath, ercpindex, enamefile);
        //}
        public override void Load(bool eCancel = false)
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                ptsview[i] = StringtoPointF(ReadINIValue("Tray", $"ptsview_{i}", $"{i},{i}", INIFILE));
                ptsworld[i] = StringtoPointF(ReadINIValue("Tray", $"ptsworld_{i}", $"{i},{i}", INIFILE));

                //pts2view[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2view_{i}", $"{i},{i}", INIFILE));
                //pts2world[i] = StringtoPointF(ReadINIValue("Tray2", $"pts2world_{i}", $"{i},{i}", INIFILE));

                i++;
            }

            run();
        }
        public override void Save()
        {
            int i = 0;
            while (i < POINT_COUNT)
            {
                WriteINIValue("Tray", $"ptsview_{i}", PointFtoStringSimple(ptsview[i]), INIFILE);
                WriteINIValue("Tray", $"ptsworld_{i}", PointFtoStringSimple(ptsworld[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2view_{i}", PointFtoStringSimple(pts2view[i]), INIFILE);
                //WriteINIValue("Tray2", $"pts2world_{i}", PointFtoStringSimple(pts2world[i]), INIFILE);

                i++;
            }

            run();
        }

        private void run()
        {
            _v1[0, 1] = ptsview[0];
            _v1[1, 1] = ptsview[1];
            _v1[0, 0] = ptsview[2];
            _v1[1, 0] = ptsview[3];

            _w1[0, 1] = ptsworld[0];
            _w1[1, 1] = ptsworld[1];
            _w1[0, 0] = ptsworld[2];
            _w1[1, 0] = ptsworld[3];
            cAoiCalibration.Dispose();
            cAoiCalibration.SetCalibrationPoints(_v1, _w1);
            cAoiCalibration.CalculateTransformMatrix();

            //_v2[0, 1] = pts2view[0];
            //_v2[1, 1] = pts2view[1];
            //_v2[0, 0] = pts2view[2];
            //_v2[1, 0] = pts2view[3];

            //_w2[0, 1] = pts2world[0];
            //_w2[1, 1] = pts2world[1];
            //_w2[0, 0] = pts2world[2];
            //_w2[1, 0] = pts2world[3];
            //cAoiCalibration2.Dispose();
            //cAoiCalibration2.SetCalibrationPoints(_v2, _w2);
            //cAoiCalibration2.CalculateTransformMatrix();

        }
    }

}
