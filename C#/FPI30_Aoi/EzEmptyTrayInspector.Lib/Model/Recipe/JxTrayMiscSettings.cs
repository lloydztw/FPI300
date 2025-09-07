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
using LeTian.JxProps;


namespace EzAoiEmptyTrayInspector.Model
{
    public class JxTrayMiscSettings : JxContainer
    {
        public JxInt FullRows = new JxInt("Full Rows", "滿盤 行數", 10, Range.C255);
        public JxInt FullCols = new JxInt("Full Cols", "滿盤 列數", 5, Range.C255);
        public JxNumber PitchX = new JxNumber("PitchX", "X方向 間距 mm", 10m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxNumber PitchY = new JxNumber("PitchY", "Y方向 間距 mm", 10m, new Range(-5000m, 5000m, 0.01m, 3));
        public JxInt FovWidth = new JxInt("FovWidth", description: "取像 寬度 (Hidden)");
        public JxInt FovHeight = new JxInt("FovHeight", description: "取像 高度 (Hidden)");
        public JxBool DebugDump = new JxBool("Debug Dump", false, description: "輸出調適影像檔 (Hidden)");
        public JxText GoldenGridRawData = new JxText("GoldenGridRawData", "", description: "(Hidden)");

        public JxTrayMiscSettings() : base("Tray Settings", "空盤 全域設定")
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

        /// <summary>
        /// GoldenGrid 的 Json 由 GoldenGridRawData 處理
        /// </summary>
        public EzBlocsGrid GetGoldenGrid(bool reload = false)
        {
            if (_cacheGoldenGrid == null || reload)
            {
                var old = _cacheGoldenGrid;
                _cacheGoldenGrid = get_goldenGrid_from_jx();
            }
            return _cacheGoldenGrid;
        }
        /// <summary>
        /// JxTrayMiscSettings 接管 Grid 生命週期
        /// </summary>
        public void SetGoldenGrid(EzBlocsGrid grid)
        {
            if (_cacheGoldenGrid != grid)
            {
                _cacheGoldenGrid?.Dispose();
                _cacheGoldenGrid = grid;
                set_goldenGrid_to_jx(_cacheGoldenGrid);
            }
        }


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
    }
}
