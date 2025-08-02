using EzDualMatch.Model;
using JetEazy.EzImage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace EzDualMatch.UnitTests
{
    [TestClass]
    public class UnitTest1 : TestDemoBase
    {
        protected void Run_Test(int level)
        {
            _ASSERT_PATH_FILES();

            IxDualMatchModel model = EzDualMatchModel.Instance;
            initEventHandlers(model);

            JxDualMatchRecipe recipe = null;
            IEzImage ImgA = null;
            IEzImage ImgB = null;

            // 測試 CanMatch
            Assert.IsTrue(ErrCodes.NO_RECIPE == model.CanMatch(SideID.A, ImgA));
            Assert.IsTrue(ErrCodes.NO_RECIPE == model.CanMatch(SideID.B, ImgB));
            Assert.IsTrue(ErrCodes.NO_RECIPE == model.CanCombine(ImgA, ImgB));

            try
            {
                loadRecipesAndImages(model, out recipe, out ImgA, out ImgB);

                Assert.IsTrue(ErrCodes.NO_IMAGE_A == model.CanMatch(SideID.A, null));
                Assert.IsTrue(ErrCodes.NO_IMAGE_B == model.CanMatch(SideID.B, null));
                Assert.IsTrue(ErrCodes.NO_IMAGE_A == model.CanCombine(null, ImgB));
                Assert.IsTrue(ErrCodes.NO_IMAGE_B == model.CanCombine(ImgA, null));
                Assert.IsTrue(ErrCodes.OK == model.CanMatch(SideID.A, ImgA));
                Assert.IsTrue(ErrCodes.OK == model.CanMatch(SideID.B, ImgB));
                Assert.IsTrue(ErrCodes.NOT_MATCHED_YET_A == model.CanCombine(ImgA, ImgB));

                if (level >= 1)
                {
                    _TRACE(">>> Template Match A ...");
                    model.RunMatch(SideID.A, ImgA);
                    Assert.IsTrue(ErrCodes.NOT_MATCHED_YET_B == model.CanCombine(ImgA, ImgB));

                    _TRACE(">>> Template Match B ...");
                    model.RunMatch(SideID.B, ImgB);

                    // 顯示 Match 結果
                    var matchResultA = model.GetMatchResult(SideID.A);
                    var matchResultB = model.GetMatchResult(SideID.B);

                    Assert.IsTrue(matchResultA != null);
                    Assert.IsTrue(matchResultB != null);
                    Assert.IsTrue(model.AreAllSidesMatched());
                    //_TRACE($"@@@ Match A: {matchResultA}");
                    //_TRACE($"@@@ Match B: {matchResultB}");
                    Assert.IsTrue(model.IsReady());
                }

                if (level >= 2)
                {
                    // 計算區域位移量 (Cell Offsets)
                    var transF = model.BuildDualTransform();
                    Assert.IsTrue(transF == model.GetDualTransform());
                    Assert.IsTrue(transF != null);
                    _TRACE(transF);

                    // 合併
                    _TRACE(">>> Combine: A = A ⨁ B");
                    Assert.IsTrue(ErrCodes.OK == model.CanCombine(ImgA, ImgB));

                    //var tm0 = DateTime.Now;

                    string outputFile = _isDump ? _dumpPath + "\\CombineA.jpg" : null;
                    int count = model.Combine(ImgA, ImgB, outputFile);
                    Assert.IsTrue(count > 0, "合併失敗!");

                    //var ts = DateTime.Now - tm0;
                    //_TRACE($"@@@ 合併耗時 ({(int)ts.TotalMilliseconds} ms)");

                    Assert.IsTrue(ErrCodes.HAS_BEEN_COMBINED == model.CanMatch(SideID.A, ImgA));

                    Assert.IsTrue(model.IsReady());
                }
            }
            catch (Exception ex)
            {
                Assert.IsTrue(false, ex.Message);
            }
            finally
            {
                ImgA?.Dispose();
                ImgB?.Dispose();
                recipe?.Dispose();
                model?.Dispose();
            }
        }

        protected void Run_All()
        {
            _ASSERT_PATH_FILES();

            IxDualMatchModel model = EzDualMatchModel.Instance;
            initEventHandlers(model);

            JxDualMatchRecipe recipe = null;
            IEzImage ImgA = null;
            IEzImage ImgB = null;

            try
            {
                loadRecipesAndImages(model, out recipe, out ImgA, out ImgB);
                Assert.IsTrue(ErrCodes.OK == model.CanRunAll(ImgA, ImgB));

                string outputFile = _isDump ? _dumpPath + "\\CombineAll.jpg" : null;
                model.RunAll(ImgA, ImgB, outputFile, wait: true);

                Assert.IsTrue(model.IsReady());
                Assert.IsTrue(ErrCodes.HAS_BEEN_COMBINED == model.CanRunAll(ImgA, ImgB));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(false, ex.Message);
            }
            finally
            {
                ImgA?.Dispose();
                ImgB?.Dispose();
                recipe?.Dispose();
                model?.Dispose();
            }
        }
    
        [TestMethod]
        public void Test_000()
        {
            Run_Test(0);
        }

        [TestMethod]
        public void Test_001()
        {
            Run_Test(1);
        }

        [TestMethod]
        public void Test_002()
        {
            Run_Test(2);
        }

        [TestMethod]
        public void Test_003()
        {
            Run_All();
        }
    }
}
