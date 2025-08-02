using EzDualMatch.Model;
using JetEazy.EzImage;
using System;
using System.Drawing;
using System.IO;


namespace EzDualMatch.Test
{
    /// <summary>
    /// SampleCode (使用 ImageUtil 載入大圖檔)
    /// </summary>
    internal class TestDemo_SampleCode : TestDemoBase
    {
        public void Run()
        {
            _TRACE("\n\r" + GetType().Name);
            _ASSERT_PATH_FILES();

            IxDualMatchModel model = EzDualMatchModel.Instance;
            model.OnStateChanged += Model_OnStateChanged;

            _TRACE($"載入參數檔 {_recipeFileName} ...");
            var recipe = new JxDualMatchRecipe();
            recipe.Load(_recipeFileName);
            model.SetRecipe(recipe);

            var recipeSideA = recipe.SideA;
            var recipeSideB = recipe.SideB;
            if (_isDump)
            {
                // 快速查看 Golden
                //recipeSideA.Match.GoldenBmp.Value.Save(_dumpPath + "\\goldenA.png");
                //recipeSideB.Match.GoldenBmp.Value.Save(_dumpPath + "\\goldenB.png");
                _SHOW_IMAGE(recipeSideA.Match.GoldenBmp, "Golden A");
                _SHOW_IMAGE(recipeSideB.Match.GoldenBmp, "Golden B");
            }

            // 大圖物件
            IEzImage ImgA = null;
            IEzImage ImgB = null;

            try
            {
                _TRACE("使用 ImageUtil 載入大圖檔 ...");
                ImgA = load_large_image(_imgFileA, recipeSideA.Mirror);
                ImgB = load_large_image(_imgFileB, recipeSideB.Mirror);

                _TRACE("Template Match A ...");
                model.RunMatch(SideID.A, ImgA);

                _TRACE("Template Match B ...");
                model.RunMatch(SideID.B, ImgB);

                // 顯示結果
                var matchResultA = model.GetMatchResult(SideID.A);
                var matchResultB = model.GetMatchResult(SideID.B);
                _TRACE($"Match A: {matchResultA}");
                _TRACE($"Match B: {matchResultB}");

                // 合併
                _TRACE("Combine: A = A ⨁ B");
                var tm0 = DateTime.Now;
                int count = model.Combine(ImgA, ImgB);
                if (count == 0) System.Diagnostics.Debug.Assert(false, "合成失敗!");

                // 計算區域位移量 (Cell Offsets)
                var transF = model.BuildDualTransform();
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

                _SHOW_IMAGE(ImgA, "SampleCode 000 合併圖");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ImgA?.Dispose();
                ImgB?.Dispose();
                recipe?.Dispose();
                model?.Dispose();
            }
        }

        #region EVENT_HANDLERS
        private void Model_OnStateChanged(object sender, EventArgs e)
        {
            var es = e as MatchStateEventArgs;
            if (es != null)
            {
                _TRACE($"[{es.ID}] {es.State}");
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        IEzImage load_large_image(string fileName, MirrorMode mirrorMode)
        {
            string fname = Path.GetFileName(fileName);
            
            _TRACE($"載入大圖檔: {fname} ...");
            var tm0 = DateTime.Now;

            var imgObj = ImageUtil.LoadLargeImage(fileName);

            var ts = DateTime.Now - tm0;
            _TRACE($"載入大圖檔: {fname} 完成 ({(int)ts.TotalMilliseconds} ms)");


            //////if (mirrorMode != MirrorMode.None)
            //////{
            //////    _TRACE($"鏡像處理: {fname} ...");
            //////    tm0 = DateTime.Now;
                
            //////    ImageUtil.ApplyMirror(imgObj, mirrorMode, true);

            //////    ts = DateTime.Now - tm0;
            //////    _TRACE($"鏡像處理: {fname} 完成 ({(int)ts.TotalMilliseconds} ms)");
            //////}

            return imgObj;
        }
        #endregion
    }
}
