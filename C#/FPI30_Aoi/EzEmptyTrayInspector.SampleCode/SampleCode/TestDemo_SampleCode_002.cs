using EzDualMatch.Model;
using JetEazy.Image;
using System;
using System.Drawing;


namespace EzDualMatch.Test
{
    /// <summary>
    /// SampleCode_001 (使用 ImageSourceModel 載入大圖檔)
    /// </summary>
    internal class TestDemo_SampleCode_002 : TestDemoBase
    {
        public void Run()
        {
            _ASSERT_PATH_FILES();

            _TRACE("使用 ImageSourceModel 載入大圖 ...");
            ImageSourceModel imgSourceA = new ImageSourceModel(0);
            ImageSourceModel imgSourceB = new ImageSourceModel(1);
            IxDualMatchModel model = EzDualMatchModel.Instance;

            _TRACE("建立 EventHandlers ...");
            imgSourceA.OnStateChanged += ImgSource_OnStateChanged;
            imgSourceB.OnStateChanged += ImgSource_OnStateChanged;
            model.OnStateChanged += Model_OnStateChanged;

            _TRACE($"載入參數檔 {_recipeFileName} ...");
            var recipe = new JxDualMatchRecipe();
            recipe.Load(_recipeFileName);
            model.SetRecipe(recipe);

            var recipeSideA = recipe.SideA;
            var recipeSideB = recipe.SideB;
            if (_isDump)
            {
                // 查看 Golden
                recipeSideA.Match.GoldenBmp.Value.Save(_dumpPath + "\\goldenA.png");
                recipeSideB.Match.GoldenBmp.Value.Save(_dumpPath + "\\goldenB.png");
            }

            try
            {
                // 載入大圖檔
                IEzImage ImgA = imgSourceA.LoadImage(_imgFileA, recipeSideA.Mirror);
                IEzImage ImgB = imgSourceB.LoadImage(_imgFileB, recipeSideB.Mirror);
                System.Diagnostics.Trace.Assert(ImgA != null && ImgA.Image != null, "圖檔A 載入失敗!");
                System.Diagnostics.Trace.Assert(ImgB != null && ImgB.Image != null, "圖檔B 載入失敗!");

                _TRACE("Template Match A ...");
                model.RunMatch(SideID.A, ImgA);

                _TRACE("Template Match B ...");
                model.RunMatch(SideID.B, ImgB);

                // 顯示結果
                var matchResultA = model.MatchResults[0];
                var matchResultB = model.MatchResults[1];
                _TRACE($"Match A: {matchResultA}");
                _TRACE($"Match B: {matchResultB}");

                // 合併
                _TRACE("Combine: A = A ⨁ B");
                var tm0 = DateTime.Now;
                int count = model.Combine(ImgA, ImgB);
                if (count == 0) System.Diagnostics.Debug.Assert(false, "合成失敗!");

                // 計算區域位移量 (Cell Offsets)
                var transF = model.BuildTransform(model.MatchResults);
                var ts = DateTime.Now - tm0;

                // 列印區域位移量
                int rows = transF.Rows;
                int cols = transF.Cols;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        transF.GetCellOffset(r, c, out Point offset);
                        _TRACE($"offset[{r},{c}] = {offset}");
                    }
                }
                _TRACE($"合併耗時 ({(int)ts.TotalMilliseconds} ms)");

                // 存檔: 保存合成結果 (耗時)
                if (_isDump)
                {
                    _TRACE("儲存合併後大圖檔 ...");
                    tm0 = DateTime.Now;
                    ImgA.Save(_dumpPath + "\\CombineA.jpg");
                    ts = DateTime.Now - tm0;
                    _TRACE($"儲存合併後大圖檔 完成 ({(int)ts.TotalMilliseconds} ms)");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                imgSourceA?.Dispose();
                imgSourceB?.Dispose();
                recipe?.Dispose();
                model?.Dispose();
            }
        }

        #region EVENT_HANDLERS
        private void ImgSource_OnStateChanged(object sender, EventArgs e)
        {
            var es = e as MatchStateEventArgs;
            if (es != null)
            {
                _TRACE($"ImageSource[{es.ID}] {es.State}");
            }
        }
        private void Model_OnStateChanged(object sender, EventArgs e)
        {
            var es = e as MatchStateEventArgs;
            if (es != null)
            {
                _TRACE($"[{es.ID}] {es.State}");
            }
        }
        #endregion
    }
}
