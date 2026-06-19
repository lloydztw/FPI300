#region AUTHOR
/*
 * UnitTest1
 * Copyright (C) 2021
 * 2021-12-23 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

/*   
 *  參考
 *  https://blog.yowko.com/mstest-nunit-xunit/
 *  https://docs.microsoft.com/zh-tw/visualstudio/test/getting-started-with-unit-testing?view=vs-2022&tabs=dotnet%2Cmstest
 * 
 */

using JetEazy.Match;
using JetEazy.QMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EzAoiEmptyTray.UnitTest
{
    [TestClass]
    public class UnitTest0_EzBlocsGridBuildere
    {
        [TestMethod]
        [DataRow(9,8, @"d:\paso.log\GridBuilder\blocs_NG_Grid.json")]
        public void Test_GridBuilder(int target_rows, int target_cols, string jsFileName)
        {
            //int target_rows = 9;
            //int target_cols = 8;
            //string jsFileName = @"d:\paso.log\blocs_failed_@EzBlocsGridBuilder.json";

            var blocs = EzBlocsStorage.LoadFromFile(jsFileName);
            checkCenterDeviations(blocs);

            var builder = new EzBlocsGridBuilder();
            var grid = builder.Build(blocs);

            if (grid != null)
            {
                grid.RowMin = 0;
                grid.ColMin = 0;
            }

            _TRACE($"\n{grid} <== {jsFileName}");
            _TRACE($"Pitch = {grid.GetPitch()}");
            _TRACE("");

            Assert.AreEqual(target_rows, grid.Rows);
            Assert.AreEqual(target_cols, grid.Cols);
        }

        void checkCenterDeviations(IList<EzBloc> blocs)
        {
            for(int i=0, N = blocs.Count; i<N;i++)
            {
                var b = blocs[i];
                var rect = b.Rect;
                var xCenter = (rect.X + rect.Right) / 2.0;
                var yCenter = (rect.Y + rect.Bottom) / 2.0;
                var centerI = new QVector2(xCenter, yCenter);
                var centerV = b.Center;
                var delta = centerI - centerV;
                if (delta.NormLength > 0.5)
                {
                    _TRACE($"Bloc[{i}] center={centerV} : delta = ({delta.X:0.0}, {delta.Y:0.0})");
                    //>>> b.Center = centerI;
                }
            }
        }

        void _TRACE(string msg)
        {
            System.Console.WriteLine(msg);
        }
    }
}
