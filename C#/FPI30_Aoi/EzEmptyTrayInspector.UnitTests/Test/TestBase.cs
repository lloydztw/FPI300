using EzDualMatch.Model;
using JetEazy.EzImage;
using JetEazy.QMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace EzDualMatch.UnitTests
{
    public class TestDemoBase
    {
        protected string _recipeFileName = Path.Combine(Global.AppPath("Recipes"), "v_003.json");
        protected string _imgFileA = "D:\\Lloydz\\ML\\Data\\LargeImages\\v003\\001.jpg";
        protected string _imgFileB = "D:\\Lloydz\\ML\\Data\\LargeImages\\v003\\002.jpg";
        protected string _dumpPath = Global.AppPath("Work", "Dump");
        protected bool _isDump = true;

        protected void _ASSERT_PATH_FILES()
        {
            Assert.IsTrue(System.IO.File.Exists(_recipeFileName), $"參數檔不存在: {_recipeFileName}");
            Assert.IsTrue(System.IO.File.Exists(_imgFileA), $"影像檔不存在: {_imgFileA}");
            Assert.IsTrue(System.IO.File.Exists(_imgFileB), $"影像檔不存在: {_imgFileB}");
        }
        protected void _TRACE(string msg)
        {
            Console.WriteLine(msg);
        }

        protected void initEventHandlers(IxDualMatchModel model)
        {
            model = EzDualMatchModel.Instance;
            model.OnStateChanged += Model_OnStateChanged;
            model.OnMatched += Model_OnMatched;
            model.OnCombined += Model_OnCombined;
            model.OnAllRunCompleted += Model_OnAllRunCompleted;
        }
        protected void loadRecipesAndImages(IxDualMatchModel model, out JxDualMatchRecipe recipe, out IEzImage ImgA, out IEzImage ImgB)
        {
            _TRACE($"載入參數檔 {_recipeFileName} ...");
            recipe = new JxDualMatchRecipe();
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

            _TRACE("使用 ImageUtil 載入大圖檔 ...");
            ImgA = loadLargeImage(_imgFileA, recipeSideA.Mirror);
            ImgB = loadLargeImage(_imgFileB, recipeSideB.Mirror);
        }
        protected IEzImage loadLargeImage(string fileName, MirrorMode mode)
        {
            string fname = Path.GetFileName(fileName);

            _TRACE($"載入大圖檔: {fname} ...");
            var tm0 = DateTime.Now;

            var imgObj = ImageUtil.LoadLargeImage(fileName, mode);

            var ts = DateTime.Now - tm0;
            _TRACE($"載入大圖檔: {fname} 完成 ({(int)ts.TotalMilliseconds} ms)");


            //if (mode != MirrorMode.None)
            //{
            //    _TRACE($"鏡像處理: {fname} ...");
            //    tm0 = DateTime.Now;
            //    ImageUtil.ApplyMirror(imgObj, mode, true);
            //    ts = DateTime.Now - tm0;
            //    _TRACE($"鏡像處理: {fname} 完成 ({(int)ts.TotalMilliseconds} ms)");
            //}

            return imgObj;
        }

        #region EVENT_HANDLERS
        private void Model_OnStateChanged(object sender, EventArgs e)
        {
            var es = e as MatchStateEventArgs;
            if (es != null)
            {
                _TRACE($"$$$$$ [{es.ID}] {es.State}");
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
            _TRACE($"$$$$$ OnCombined : Result = {result}");
        }
        private void Model_OnAllRunCompleted(object sender, DualMatchResultEventArgs e)
        {
            var result = e.Result;
            _TRACE($"$$$$$ OnAllCompleted : Result = {result}");
        }
        #endregion

        protected void _TRACE(EzDualTransform transF)
        {
            // 列印區域位移量
            int rows = transF.Rows;
            int cols = transF.Cols;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    transF.GetCellOffset(r, c, out QVector offset);
                    //_TRACE($"offset[{r},{c}] = {offset}");
                    Assert.IsTrue(offset != null);
                }
            }
        }
    }
}
