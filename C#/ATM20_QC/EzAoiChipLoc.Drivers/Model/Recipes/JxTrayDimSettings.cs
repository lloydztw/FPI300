#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using JetEazy.QMath;
using LeTian.JxProps;
using System.Drawing;

namespace EzAoiChipLocQC.Model
{
    /// <summary>
    /// Tray Dimensions
    /// </summary>
    public class JxTrayDimSettings : JxContainer
    {
        public JxInt FullRows = new JxInt("Full Rows", "滿盤 行數", 10, Range.C255);
        public JxInt FullCols = new JxInt("Full Cols", "滿盤 列數", 5, Range.C255);
        public JxNumber PitchX = new JxNumber("PitchX", "X方向 節距 (mm)", 20m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber PitchY = new JxNumber("PitchY", "Y方向 節距 (mm)", 20m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber PlaceHoldSizeX = new JxNumber("PlaceHoldSizeX", "格位尺寸X (mm)", 10m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber PlaceHoldSizeY = new JxNumber("PlaceHoldSizeY", "格位尺寸Y (mm)", 10m, new Range(-5000m, 5000m, 0.01m, 3));

        public JxInt FovWidth = new JxInt("FovWidth", description: "取像 寬度 (Hidden)");
        public JxInt FovHeight = new JxInt("FovHeight", description: "取像 高度 (Hidden)");
        
        public JxBool DebugDump = new JxBool("Debug Dump", false, description: "輸出調適影像檔 (Hidden)");
        public JxText GoldenGridRawData = new JxText("GoldenGridRawData", "", description: "(Hidden)");

        public JxTrayDimSettings() : base("Tray Settings", "載盤規格")
        {
            //>>> System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 建構");
        }

        public override void OnBindingSubItems()
        {
            // clear cache
            _cacheGoldenGrid?.Dispose();
            _cacheGoldenGrid = null;

            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                FullRows,
                FullCols,
                PitchX,
                PitchY,
                PlaceHoldSizeX,
                PlaceHoldSizeY,
                FovWidth,
                FovHeight,
                DebugDump,
                GoldenGridRawData,
            });
            base.OnBindingSubItems();
        }
        protected override void OnDisposing()
        {
            base.OnDisposing();
            _cacheGoldenGrid?.Dispose();
            _cacheGoldenGrid = null;
            //>>> System.Diagnostics.Debug.WriteLine($"{GetType().Name} [{Name}] 卸載");
        }

        #region GOLDEN_HELPER_FUNCTIONS
        /// <summary>
        /// GoldenGrid 的 Json 由 GoldenGridRawData 處理
        /// </summary>
        public EzBlocsGrid GetGoldenGrid(bool reload = false)
        {
            if (_cacheGoldenGrid == null || reload)
            {
                var old = _cacheGoldenGrid;
                _cacheGoldenGrid = get_goldenGrid_from_jx();
                old?.Dispose();
            }
            return _cacheGoldenGrid;
        }
        /// <summary>
        /// JxTrayGeoSettings 接管 Grid 生命週期
        /// </summary>
        public void SetGoldenGrid(EzBlocsGrid grid)
        {
            if (_cacheGoldenGrid != grid)
            {
                //_cacheGoldenGrid?.Dispose();
                //_cacheGoldenGrid = grid;
                var old = _cacheGoldenGrid;
                _cacheGoldenGrid = grid;
                set_goldenGrid_to_jx(_cacheGoldenGrid);
                old?.Dispose();
            }
        }
        #endregion

        #region PRIVATE_GRID_MEMBERS
        EzBlocsGrid _cacheGoldenGrid = null;
        EzBlocsGrid get_goldenGrid_from_jx()
        {
            var ss = new EzBlocsGridSerializer();
            ss.Deserialize(GoldenGridRawData.Value, out EzBlocsGrid grid);
            return grid;
        }
        void set_goldenGrid_to_jx(EzBlocsGrid grid)
        {
            var ss = new EzBlocsGridSerializer();
            GoldenGridRawData.Value = ss.Serialize(grid);
        }
        #endregion

        /// <summary>
        /// 取得 載盤的外廓尺寸 (mm)
        /// </summary>
        public SizeF GetTrayDimension(bool innerSpan = false)
        {
            decimal paddingX = 25;
            decimal paddingY = 50;
            decimal spanX = PitchX.Value * (FullCols.Value - 1) + PlaceHoldSizeX.Value;
            decimal spanY = PitchY.Value * (FullRows.Value - 1) + PlaceHoldSizeY.Value;
            if (!innerSpan)
            {
                spanX += paddingX;
                spanY += paddingY;
            }
            return new SizeF((float)spanX, (float)spanY);
        }

        /// <summary>
        /// 左上角第一格位座標 (mm)
        /// </summary>
        public QVector GetFirstPlaceHoldCoord()
        {
            var dimSize = GetTrayDimension();
            var innerSpan = GetTrayDimension(innerSpan: true);
            float x = (dimSize.Width - innerSpan.Width) / 2 + (float)PlaceHoldSizeX.Value / 2;
            float y = (dimSize.Height - innerSpan.Height) / 2 + (float)PlaceHoldSizeY.Value / 2;
            return new QVector2(x, y);
        }
    }
}
