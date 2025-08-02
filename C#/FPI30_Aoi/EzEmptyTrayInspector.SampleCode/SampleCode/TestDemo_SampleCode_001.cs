using EzDualMatch.Model;
using JetEazy.EzImage;
using System;
using System.IO;


namespace EzDualMatch.Test
{
    /// <summary>
    /// SampleCode_001 (一鍵執行)
    /// </summary>
    internal class TestDemo_SampleCode_001 : TestDemoBase
    {
        public void Run()
        {
            _TRACE("\n\r" + GetType().Name);

            _ASSERT_PATH_FILES();

            IxDualMatchModel model = EzDualMatchModel.Instance;
            initEventHandlers(model);

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

                ErrCodes err = model.CanRunAll(ImgA, ImgB);
                System.Diagnostics.Trace.Assert(err == ErrCodes.OK, JetEazy.QxNums.GetEnumDescription(err));

                string combineFile = _isDump ? _dumpPath + "\\CombineAll.jpg" : null;
                model.RunAll(ImgA, ImgB, combineFile, wait:true);

                _SHOW_IMAGE(ImgA, "SampleCode 001 合併圖");
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
        protected void initEventHandlers(IxDualMatchModel model)
        {
            model.OnStateChanged += Model_OnStateChanged;
            model.OnMatched += Model_OnMatched;
            model.OnCombined += Model_OnCombined;
            model.OnAllRunCompleted += Model_OnAllRunCompleted;
        }
        private void Model_OnStateChanged(object sender, EventArgs e)
        {
            var es = e as MatchStateEventArgs;
            if (es != null)
            {
                _TRACE($"[{es.ID}] {es.State}");
            }
        }
        private void Model_OnMatched(object sender, MatchResultEventArgs e)
        {
            string txt = !e.IsResetting() ? e.Result?.ToString() : "(clear)";
            _TRACE($"$$$$$ OnMatched [{e.ID}] {txt}");
        }
        private void Model_OnCombined(object sender, DualMatchResultEventArgs e)
        {
            var result = e.Result;
            _TRACE($"$$$$$ 合併完成 : Result = {result}");
        }
        private void Model_OnAllRunCompleted(object sender, DualMatchResultEventArgs e)
        {
            var result = e.Result;
            //_TRACE($"$$$$$ OnAllCompleted : Result = {result}");
            _TRACE($"$$$$$ 一鍵執行完成 : Result = {result}");
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        IEzImage load_large_image(string fileName, MirrorMode mirrorMode)
        {
            string fname = Path.GetFileName(fileName);
            
            _TRACE($"載入大圖檔: {fname} ...");
            var tm0 = DateTime.Now;

            var imgObj = ImageUtil.LoadLargeImage(fileName, mirrorMode);

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
