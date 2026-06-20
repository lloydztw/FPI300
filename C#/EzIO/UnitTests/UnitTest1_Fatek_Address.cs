#region AUTHOR
/*
 * EzIO UnitTests
 * Copyright (C) 2026
 * 2026-04-05 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzPlc.Fatek;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTest1_Fatek_Address : UnitTest_Base
    {
        [TestMethod]
        public void Test_01_ValidAddresses()
        {
            // 測試標準 16-bit R 暫存器
            var addrR = new FatekAddr("R100");
            Assert.AreEqual(16, addrR.Bits);
            Assert.AreEqual("R00100", addrR.ToCommString());

            // 測試 32-bit DR 暫存器 (必須對齊 2)
            var addrDR = new FatekAddr("DR2");
            Assert.AreEqual(32, addrDR.Bits);

            var cates = new[] { "M", "X", "Y", 
                                "WM", "WX", "WY", "R", "D",
                                "DWM", "DWX", "DWY", "DR", "DD" };

            foreach (var cate in cates)
            {
                var number = _rnd.Next(1000);

                if (cate.StartsWith("DW"))
                    number = number / 32 * 32;
                else if (cate.StartsWith("W"))
                    number = number / 16 * 16;
                else if (cate == "DR" || cate == "DD")
                    number = number / 2 * 2;

                var addr = new FatekAddr($"{cate}{number}");
                var addr1 = new FatekAddr($"1:{cate}{number}");
                var addr2 = new FatekAddr($"2:{cate}{number}");
                _TRACE($"{addr.ToCommString()} = {addr.Bits}-Bit");

                Assert.AreEqual(addr.Address, number);
                Assert.AreEqual(addr1.Address, number);
                Assert.AreEqual(addr2.Address, number);
                Assert.AreEqual(addr.KeyName, addr1.KeyName);
                Assert.AreNotEqual(addr.KeyName, addr2.KeyName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Test_02_InvalidAlignment_ShouldFail()
        {
            // WM 必須是 16 倍數
            new FatekAddr("WM100");

            // DR 必須是對齊 2 的倍數，傳入 3 應該拋出異常
            new FatekAddr("1:DR3");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Test_03_AddressOverLimit_ShouldFail()
        {
            // M 點位上限在 GetFatekAddressConstraints 定義為 9999
            new FatekAddr("1:M10000");
        }

        [TestMethod]
        public void Test_04_StringWithComma_ShouldWarnAndParse()
        {
            // 程式碼中有 if (ezIoName.Contains(",")) 邏輯
            var addr = new FatekAddr("2:R500,OptionalDescription");
            Assert.AreEqual(2, addr.StationID);
            Assert.AreEqual(500, addr.Address);
            Assert.AreEqual("R", addr.Category);
        }
    }
}
