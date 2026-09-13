using Common.RecipeSpace;
using JetEazy.Match;
using JetEazy.Utils;
using LaserAlignDX.Mvc.Model.Recipe;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LaserAlignDX.OPSpace.RecipeSpace
{
    public partial class RecipeFPIX3Class : RecipeBaseClass, IDisposable
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

            //2026-08-11 將所有的 MVD AOI 代碼, 重整至 AoiModel 內!
            //disposeMvdTools();

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
        public readonly List<RegionCellX3Class> xRegionCells = new List<RegionCellX3Class>();
        /// <summary>
        /// 個別 疑似異物區塊 (位於格點範圍外)
        /// 這應該放在 AoiResult 而不是 Recipe 區
        /// </summary>
        public readonly List<Rectangle> xOutBlocs = new List<Rectangle>();
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
        /// 使用 RcpBmpHolder 來動態載入 載台1 的 bmpOrg (對焦在晶粒)
        /// </summary> 
        readonly RcpBmpHolder _bmpHolderOrg1 = new RcpBmpHolder("org");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台2 的 bmpOrg (對焦在晶粒)
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrg2 = new RcpBmpHolder("org2");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台1 的 bmpOrg (對焦在空載台)
        /// </summary>
        readonly RcpBmpHolder _bmpHolderEmpty1 = new RcpBmpHolder("orgE1");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 載台2 的 bmpOrg (對焦在空載台)
        /// </summary>
        readonly RcpBmpHolder _bmpHolderEmpty2 = new RcpBmpHolder("orgE2");
        /// <summary>
        /// 使用 RcpBmpHolder 來動態載入 飛拍 的 bmpOrgFly
        /// </summary>
        readonly RcpBmpHolder _bmpHolderOrgFly = new RcpBmpHolder("orgFly");
        /// <summary>
        /// 釋放所有的 BmpOrgs (org, org2, orgFly)
        /// </summary>
        void disposeBmpOrgs()
        {
            _bmpHolderEmpty1?.Dispose();
            _bmpHolderEmpty2?.Dispose();
            _bmpHolderOrg1?.Dispose();
            _bmpHolderOrg2?.Dispose();
            _bmpHolderOrgFly?.Dispose();
            _bmpOrgFlyRuntime?.Dispose();
        }
        #endregion

        #region BMP_ORG_載台1_載台2
        public Bitmap PeekBmpOrg(CarrierEnum carrierID, bool focusOnEmptyCarrier)
        {
            var holder = focusOnEmptyCarrier ?
                    ((carrierID == CarrierEnum.C1) ? _bmpHolderEmpty1 : _bmpHolderEmpty2) :
                    ((carrierID == CarrierEnum.C1) ? _bmpHolderOrg1 : _bmpHolderOrg2);
            
            var bmp = holder?.Peek();

            // 原有版本, 都是對焦在 chip 表面
            if (bmp == null && focusOnEmptyCarrier)
            {
                holder = (carrierID == CarrierEnum.C1) ? _bmpHolderOrg1 : _bmpHolderOrg2;
                bmp = holder?.Peek();
            }

            return bmp;
        }
        public void TakeInBmpOrg(CarrierEnum carrierID, bool focusOnEmptyCarrier, Bitmap bmp)
        {
            var holder = focusOnEmptyCarrier ?
                    ((carrierID == CarrierEnum.C1) ? _bmpHolderEmpty1 : _bmpHolderEmpty2) :
                    ((carrierID == CarrierEnum.C1) ? _bmpHolderOrg1 : _bmpHolderOrg2);
            holder?.TakeOver(bmp);
        }
        public void ReleaseBmpsOrg(bool save)
        {
            // bmpOrg 一般只用於參數編輯時期, 跑線時可以釋放
            if (save)
            {
                _bmpHolderOrg1?.Save();
                _bmpHolderOrg2?.Save();
                _bmpHolderEmpty1?.Save();
                _bmpHolderEmpty2?.Save();
            }
            _bmpHolderOrg1?.Dispose();
            _bmpHolderOrg2?.Dispose();
            _bmpHolderEmpty1?.Dispose();
            _bmpHolderEmpty2?.Dispose();
        }
        #endregion

        #region BMP_ORG_飛拍
        /// <summary>
        /// 用來維持 舊接口 相容性 之 使用模式
        /// </summary>
        Bitmap _bmpOrgFlyRuntime = null;// new Bitmap(1,1,System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        /// <summary>
        /// 舊接口: Caller 會管理 bmpOrgFly 生命週期
        /// </summary>
        public Bitmap bmpOrgFly
        {
            get
            {
                //if (_bmpOrgFlyRuntime == null)
                    _bmpOrgFlyRuntime = (Bitmap)_bmpHolderOrgFly.Peek()?.Clone();//<<这里不管怎么样都要重新加载一次不然参数页面的图片不对
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

        #region 模板區_新接口
        /// <summary>
        /// Golden Region Cell Rect (晶粒區域粗框)
        /// </summary>
        public RectangleF GoldenRegionCellRect
        {
            get => _dtoGoldenRegionTemplate.RectF;
            set => _dtoGoldenRegionTemplate.RectF = value;
        }

        /// <summary>
        /// Golden Region Bitmap (晶粒區域粗框)
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>
        public Bitmap GoldenRegionCellBmp
        {
            get => _dtoGoldenRegionTemplate.Bmp;
            set => _dtoGoldenRegionTemplate.Bmp = value;
        }

        /// <summary>
        /// Golden Chip Rect
        /// 更精確(內縮)的晶粒矩形區域,
        /// 位於 Golden Region Cell Rect 之內,
        /// 以 GoldenRegionCellRect 的左上角 為相對零點
        /// </summary>
        public RectangleF GoldenChipRect
        {
            get => _dtoGoldenChipTemplate.RectF;
            set => _dtoGoldenChipTemplate.RectF = value;
        }

        /// <summary>
        /// Golden Chip Bitmap 更精確(內縮)的晶粒模板圖像
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>        
        public Bitmap GoldenChipBmp
        {
            get => _dtoGoldenChipTemplate.Bmp;
            set => _dtoGoldenChipTemplate.Bmp = value;
        }

        /// <summary>
        /// Defects Mask Bitmap (瑕疵檢遮罩)
        /// </summary>
        public Bitmap DefectsMaskBmp
        {
            get => _dtoGoldenMaskTemplate.Bmp;
            set => _dtoGoldenMaskTemplate.Bmp = value;
        }

        /// <summary>
        /// 二維碼框選區
        /// </summary>
        /// <remarks>
        /// 位於 Golden Region Cell Rect 之內,
        /// 以 GoldenRegionCellRect 的左上角 為相對零點        
        /// </remarks>
        public RectangleF QrCodeRect
        {
            get => _dtoQrCodeTemplate.RectF;
            set => _dtoQrCodeTemplate.RectF = value;
        }

        /// <summary>
        /// 二維碼影像樣本圖像
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>        
        public Bitmap QrCodeBmp
        {
            get => _dtoQrCodeTemplate.Bmp;
            set => _dtoQrCodeTemplate.Bmp = value;
        }

        /// <summary>
        /// 飛拍 矩形區塊
        /// </summary>
        public RectangleF FlyTemplateRect
        {
            get => _dtoFlyAoiTemplate.RectF;
            set => _dtoFlyAoiTemplate.RectF = value;
        }

        /// <summary>
        /// 飛拍 模板 圖像
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>   
        public Bitmap FlyTemplateBmp
        {
            get => _dtoFlyAoiTemplate.Bmp;
            set => _dtoFlyAoiTemplate.Bmp = value;
        }
        #endregion

        #region 模板區_舊接口_(陸版爛英文)
        /// <summary>
        /// Golden Region Cell (晶粒區域粗框)
        /// </summary>
        public RectangleF xRectRegionPrint
        {
            get => GoldenRegionCellRect;
            set => GoldenRegionCellRect = value;
        }
        /// <summary>
        /// Golden Region Bitmap (晶粒區域粗框)
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>        
        public Bitmap bmpprinttemplate
        {
            get => GoldenRegionCellBmp;
            set => GoldenRegionCellBmp = value;
        }
        /// <summary>
        /// Golden Chip Rect
        /// 更精確(內縮)的晶粒矩形區域
        /// 位於 Golden Region Cell (xRectRegionPrint) 之內
        /// 相對於 Golden Region Cell (xRectRegionPrint) 的左上角為零點
        /// </summary>
        public RectangleF xRegionTrain
        {
            get => GoldenChipRect;
            set => GoldenChipRect = value;
        }
        /// <summary>
        /// 這個其實就是 Golden Chip Template Bitmap
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>
        public Bitmap bmpDefectTemplate
        {
            get => GoldenChipBmp;
            set => GoldenChipBmp = value;
        }
        /// <summary>
        /// Defects Mask Bitmap
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>        
        public Bitmap bmpprintmask
        {
            get => DefectsMaskBmp;
            set => DefectsMaskBmp = value;
        }
        /// <summary>
        /// 二維碼框選區
        /// </summary>
        public RectangleF xRectCodeRegion
        {
            //get => _dtoQrCodeTemplate.RectF;
            //set => _dtoQrCodeTemplate.RectF = value;
            get => QrCodeRect;
            set => QrCodeRect = value;
        }
        /// <summary>
        /// 二維碼影像樣本
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>
        public Bitmap bmpcodetemplate
        {
            //get => _dtoQrCodeTemplate.Bmp;
            //set => _dtoQrCodeTemplate.Bmp = value;
            get => QrCodeBmp;
            set => QrCodeBmp = value;
        }
        /// <summary>
        /// 飛拍 矩形區塊
        /// </summary>
        public RectangleF xRectRegionPrintFly
        {
            //get => _dtoFlyAoiTemplate.RectF;
            //set => _dtoFlyAoiTemplate.RectF = value;
            get => FlyTemplateRect; 
            set => FlyTemplateRect = value;
        }
        /// <summary>
        /// 飛拍 模板 圖像
        /// </summary>
        /// <remarks>
        /// Caller 必須維持 Bitmap 生命週期
        /// </remarks>        
        public Bitmap bmpprintFlytemplate
        {
            //get => _dtoFlyAoiTemplate.Bmp;
            //set => _dtoFlyAoiTemplate.Bmp = value;
            get => FlyTemplateBmp;
            set => FlyTemplateBmp = value;
        }
        #endregion

        #region 參數區_PARAM_GRID
        //internal int xRow = 1;
        //internal int xColumn = 1;
        //internal int xLeftTopX = 1;
        //internal int xLeftTopY = 1;
        //internal float xRowOffset = 2f;
        //internal float xColumnOffset = 2f;
        //internal float xChipWidth = 10;
        //internal float xChipHeight = 10;
        internal int xExtendx
        {
            get => GridParams.xExtendx;
            set => GridParams.xExtendx = value;
        }
        internal int xExtendy
        {
            get => GridParams.xExtendy;
            set => GridParams.xExtendy = value;
        }
        //internal float xAngle = 0;
        internal int xChNum => GridParams.xChNum;
        internal int xChValue => GridParams.xChValue;
        //internal StageNumber xStageNumber = StageNumber.N0;
        #endregion

        #region 參數區_实际矩阵XY_即將被新的座標系統完全取代
        //public float xRealLeftX = 0;
        //public float xRealLeftY = 0;
        public float xRealOffsetX => GridParams.xRealOffsetX;
        public float xRealOffsetY => GridParams.xRealOffsetY;
        #endregion

        #region 參數區_相機工作高度
        /// <summary>
        /// 對焦在載台表面
        /// </summary>
        internal float zFocusOnCarrier;
        /// <summary>
        /// 對焦在晶粒表面
        /// </summary>
        internal float zFocusOnChip;
        #endregion

        #region 參數區_LT_CAM_GRIDS
#if (OPT_REV_2026_0308_LEGACY)
        public EzBlocsGrid xCamGrid1 { get; set; } = null;
        public EzBlocsGrid xCamGrid2 { get; set; } = null;
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
        public void saveCamGrid(CarrierEnum carrierID, EzBlocsGrid grid)
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
#else
        public EzBlocsGrid xCamGrid1
        {
            //*** 交給 TransformsModel 管理 ***
            get => GaMvcConfig.SysModel.TransformsModel.GetCalibCamGrid(CarrierEnum.C1);
            set { }
        }
        public EzBlocsGrid xCamGrid2
        {
            //*** 交給 TransformsModel 管理 ***
            get => GaMvcConfig.SysModel.TransformsModel.GetCalibCamGrid(CarrierEnum.C2);
            set { }
        }
        void loadCamGrids(CarrierEnum carrierID, out EzBlocsGrid grid)
        {
            //*** 交給 TransformsModel 管理 ***

            //var file = System.IO.Path.Combine(PathIndexStr, $"camGrid_{carrierID}.txt");
            //if (System.IO.File.Exists(file))
            //{
            //    string str = System.IO.File.ReadAllText(file);
            //    var ss = new EzBlocsGridSerializer();
            //    ss.Deserialize(str, out grid);
            //    return;
            //}
            //else
            //{
            //    grid = null;
            //}
            //if (carrierID == CarrierEnum.C1) xCamGrid1 = grid;
            //if (carrierID == CarrierEnum.C2) xCamGrid2 = grid;

            grid = carrierID == CarrierEnum.C1 ? xCamGrid1 : xCamGrid2;
        }
        public void saveCamGrid(CarrierEnum carrierID, EzBlocsGrid grid)
        {
            //*** 交給 TransformsModel 管理 ***

            //if (grid == null)
            //    return;
            //var file = System.IO.Path.Combine(PathIndexStr, $"camGrid_{carrierID}.txt");
            //var ss = new EzBlocsGridSerializer();
            //string str = ss.Serialize(grid);
            //System.IO.File.WriteAllText(file, str);
        }
        void disposeCamGrids()
        {
            //*** 交給 TransformsModel 管理 ***
        }
        public void SaveCameraGrids()
        {
            //*** 交給 TransformsModel 管理 ***
            //saveCamGrid(CarrierEnum.C1, xCamGrid1);
            //saveCamGrid(CarrierEnum.C2, xCamGrid2);
        }
#endif
        #endregion

        #region 參數區_其他子群
        internal readonly DtoX3GridParams GridParams = new DtoX3GridParams();
        public InspectX3ParaClass InspectParams => InspectX3ParaClass.Instance;
        public FlyParaClass FlyAoiParams => FlyParaClass.Instance;
        #endregion

        public override void Load(bool eCancel = false)
        {
#if (OPT_NOT_USED_LEGACY_CODE)
            //xLotNoStr = ReadINIValue("Collect", "xLotNoStr", "NONE", INIFILE);

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
#endif

            GridParams.Load(INIFILE);

            zFocusOnCarrier = float.Parse(ReadINIValue("Recipe Basic", "zFocusOnCarrier", "1", INIFILE));
            zFocusOnChip = float.Parse(ReadINIValue("Recipe Basic", "zFocusOnChip", "1", INIFILE));

            #region NOT_USED_LEGACY_CODE
            //xRectRegionPrint = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrint", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRegionTrain = StringtoRectF(ReadINIValue("Recipe Basic", "xRegionTrain", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectRegionPrintFly = StringtoRectF(ReadINIValue("Recipe Basic", "xRectRegionPrintFly", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xRectCodeRegion = StringtoRectF(ReadINIValue("Recipe Basic", "xRectCodeRegion", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineLeft = StringtoRectF(ReadINIValue("Recipe Basic", "xLineLeft", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineTop = StringtoRectF(ReadINIValue("Recipe Basic", "xLineTop", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineRight = StringtoRectF(ReadINIValue("Recipe Basic", "xLineRight", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            //xLineBottom = StringtoRectF(ReadINIValue("Recipe Basic", "xLineBottom", RectFtoStringSimple(new RectangleF(0, 0, 100, 100)), INIFILE));
            #endregion

            loadCamGrids(CarrierEnum.C1, out var grid1);
            loadCamGrids(CarrierEnum.C2, out var grid2);

            //(LD1) 各種模板 (根據載台號 載入不同對應的設定值)
            _dtoGoldenRegionTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoGoldenChipTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoGoldenMaskTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);
            _dtoQrCodeTemplate.SetTag(_CARRIER_TAG).Load(INIFILE);

            //(LD2) 飛拍模板 (兩載台共用一份)
            _dtoFlyAoiTemplate.Load(INIFILE);

            //(LD3) 邊線框 (根據載台號 載入不同對應的設定值)
            LoadLineBorderParams(_CARRIER_TAG);
            LoadGapBorderParams(_CARRIER_TAG);

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
                //CreateViews();
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
#if (OPT_NOT_USED_LEGACY_CODE)
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
#endif
            GridParams.Save(INIFILE);

            WriteINIValue("Recipe Basic", "zFocusOnCarrier", zFocusOnCarrier.ToString(), INIFILE);
            WriteINIValue("Recipe Basic", "zFocusOnChip", zFocusOnChip.ToString(), INIFILE);

            saveCamGrid(CarrierEnum.C1, xCamGrid1);
            saveCamGrid(CarrierEnum.C2, xCamGrid2);

            #region NOT_USED_LEGACY_CODE
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
            #endregion

            _bmpHolderOrg1.Save();
            _bmpHolderOrg2.Save();
            _bmpHolderOrgFly.Save();

            // 建立所有的 Region Cells (應該放在 Aoi Model)
            //CreateViews();
            ViewTrainLoad();

            InspectParams.Save();
            FlyAoiParams.Save();

            // 強制刪除多餘的檔案
            DeleteRedundentFiles(INIFILE);
        }
        public override void ChangeIndex(int eindex)
        {
            base.ChangeIndex(eindex);
            InspectParams.ChangeIndex(eindex);
            FlyAoiParams.ChangeIndex(eindex);
        }
        public override string ToString()
        {
            // 回傳簡要說明
            var sb = new StringBuilder();
            //(1) 晶粒型態
            string chipType = GaUtil.GetEnumDescription(InspectParams.xAlgorithm);
            sb.AppendLine(chipType);
            //(2) Rows x Cols
            int rows = GridParams.xRow;
            int cols = GridParams.xColumn;
            var pitchX = GridParams.xRealOffsetX;
            var pitchY = GridParams.xRealOffsetY;
            sb.AppendLine($"格位數: {rows} x {cols}");
            sb.AppendLine($"格位間距X: {pitchX:0.000} mm");
            sb.AppendLine($"格位間距Y: {pitchY:0.000} mm");
            //(3) Chip Dimentsion
            sb.AppendLine($"晶粒尺寸X : {GridParams.xChipWidth:0.000} mm");
            sb.AppendLine($"晶粒尺寸Y : {GridParams.xChipHeight:0.000} mm");
            //(4) OPTIONS
            sb.AppendLine($"尺寸檢測 : {InspectParams.optChipMeasurement}");
            sb.AppendLine($"邊隙檢測 : {InspectParams.optPadEdgeGapsMeasurement}");
            sb.AppendLine($"瑕疵檢測 : {InspectParams.optChipDefectsInspect}");
            return sb.ToString();
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
                //InspectParams.Save();
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
                this.InspectParams.SaveDefectMaskRects();
            }
            if (name.Contains("BADCONN"))
            {
                this.InspectParams.SaveBadConnRects();
            }
            if (name.Contains("QRCODE"))
            {
                // QR Code 模板 (根據載台號 存入不同對應的設定值)
                _dtoQrCodeTemplate.SetTag(_CARRIER_TAG).Save(INIFILE);
            }
            if (name.Contains("LINEBORDER"))
            {
                // 邊線框
                SaveLineBorderParams(_CARRIER_TAG);
                // 邊隙框
                SaveGapBorderParams(_CARRIER_TAG);
            }
            if (name.Contains("FLY"))
            {
                // 飛拍模板 (目前不隨著載台顏色改變)
                _dtoFlyAoiTemplate.Save(INIFILE);
            }
        }
        #endregion

        #region 子項參數_保存函式_舊接口
#if( OPT_NOT_USED)
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
            //WriteINIValue("Collect", "xLotNoStr", xLotNoStr, INIFILE);
        }
#endif
        #endregion

        #region REDUNDENT_FILES_刪除冗余文件
        public static void DeleteRedundentFiles(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (System.IO.File.Exists(filePath))
                filePath = System.IO.Path.GetDirectoryName(filePath);

            if (!System.IO.Directory.Exists(filePath))
                return;

            foreach (var fname in IterRedundentFiles())
            {
                string fileName = System.IO.Path.Combine(filePath, fname);
                if (System.IO.File.Exists(fileName))
                {
                    try
                    {
                        System.IO.File.Delete(fileName);
                    }
                    catch
                    {
                    }
                }
            }
        }
        public static IEnumerable<string> IterRedundentFiles()
        {
            yield return "bmpprintNoTraytemplate.bmp";
            yield return "orgNoTray.bmp";
            //yield return "camGrid_C1.txt";
            //yield return "camGrid_C2.txt";
            //yield return "NoTray_default_info.ini";
        }
        #endregion
    }
}
