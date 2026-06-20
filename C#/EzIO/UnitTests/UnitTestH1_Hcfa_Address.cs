#region AUTHOR
/*
 * EzIO UnitTests
 * Copyright (C) 2026
 * 2026-04-11 created by LeTian Chang
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzPlc.Hcfa;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTestH1_Hcfa_Address : UnitTest_Base
    {
        [TestMethod]
        public void Test000_HcfaAddress_IX()
        {
            var baseAddress = new HcfaAddress($"0:IX0.0");
            int modbusHslAddressN = 0;

            for (int i = 0; i < 32; i++)
            {
                var addr = baseAddress.Offset(i);
                var modbusAddrStr = addr.ToCommString();
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {modbusAddrStr}");

                modbusHslAddressN = i;
                Assert.AreEqual(modbusAddrStr, modbusHslAddressN.ToString());
            }
        }

        [TestMethod]
        public void Test001_HcfaAddress_QX()
        {
            int[] addresses = new int[]
            {
                0,
                1016,
                1144,
                1272,
                1400,
                1528,
                1656,
                1784,
            };

            foreach(int addrN in addresses)
            {
                var addr = new HcfaAddress($"QX{addrN}");
                var modbusAddrStr = addr.ToCommString();
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {modbusAddrStr}");

                int modbusHslAddressN = addrN * 8;
                Assert.AreEqual(modbusAddrStr, modbusHslAddressN.ToString());
            }
        }

        [TestMethod]
        public void Test002_HcfaAddress_QB()
        {
            int[] addresses = new int[]
            {
                0,
                1000,
                1020,
                1040,
                1060,
                1080,
                1200,
                1300,
                1520,
                1521,
                1522,
                1540,
                1544,
                1545,
                1547,   // (也用於 input)
                1548,   // (也用於 input)
                1553,
            };

            foreach (int addrN in addresses)
            {
                var addr = new HcfaAddress($"QB{addrN}");
                var modbusAddrStr = addr.ToCommString();
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {modbusAddrStr}");

                int modbusHslAddressN = addrN * 8;
                Assert.AreEqual(modbusAddrStr, modbusHslAddressN.ToString());
            }
        }

        [TestMethod]
        public void Test003_HcfaAddress_MW()
        {
            int[] addresses = new int[]
            {
                0,
                1000,
                1020,
                1040,
                1060,
                1100,
                1300,
                1340
            };

            foreach (int addrN in addresses)
            {
                var addr = new HcfaAddress($"MW{addrN}");
                var modbusAddrStr = addr.ToCommString();
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {modbusAddrStr}");

                int modbusHslAddressN = addrN;
                Assert.AreEqual(modbusAddrStr, modbusHslAddressN.ToString());
            }
        }

        /// <summary>
        /// 驗證：ToCommString 必須嚴格用於「通訊格式」 (純數字或 Modbus 物理地址，不含點號)
        /// </summary>
        [DataTestMethod]
        [DataRow("0:IX0.0", "0")]               // 0 * 8 + 0 = 0
        [DataRow("0:IX10", "80")]               // 10 * 8 + 0 = 80
        [DataRow("0:MW100.5", "100")]           // 100 (通訊不應包含 .5)
        public void Test01_HcfaAddress_ToCommString_StrictLogic(string ezName, string expectedComm)
        {
            // Arrange
            var addr = new HcfaAddress(ezName);

            var modbusAddrStr = addr.ToCommString();
            _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {modbusAddrStr}");

            // Assert
            Assert.AreEqual(expectedComm, modbusAddrStr,
                $"通訊格式錯誤：{ezName} 應轉換為物理地址 {expectedComm}");
        }

        /// <summary>
        /// 驗證：ToString 必須用於「人看格式」 (保留 Category 與位元偏移以便除錯)
        /// </summary>
        [DataTestMethod]
        [DataRow("1:MW100.5", "MW100")]
        [DataRow("2:QB200", "2:QB200.0")]
        [DataRow("1:QX1200", "QX1200.0")]
        [DataRow("1:IX8", "IX8.0")]
        [DataRow("3:IX3.7", "3:IX3.7")]
        public void Test02_HcfaAddress_ToString_HumanReadable(string ezName, string expectedDisplay)
        {
            // Arrange
            var addr = new HcfaAddress(ezName);

            _TRACE($"輸入= {ezName}\t=>\tKeyName= {addr.KeyName}");

            // Act
            string actualDisplay = addr.KeyName;

            // Assert
            Assert.AreEqual(expectedDisplay, actualDisplay,
                $"顯示格式錯誤：應保留原始易讀標籤 {expectedDisplay}");
        }

        /// <summary>
        /// 驗證位元偏移 (BitOffset) 在物件中的完整性
        /// </summary>
        [TestMethod]
        public void Test04_HcfaAddress_BitOffset_Integrity()
        {
            // Arrange
            var addr = new HcfaAddress("1:MW100.5");

            _TRACE(addr, showDual: true);

            // Assert
            Assert.AreEqual(5, addr.BitOffset, "BitOffset 應正確解析並保存在記憶體中");

            // 根據分離原則：
            Assert.IsFalse(addr.ToCommString().Contains("."), "通訊格式不應有點");
            //Assert.IsTrue(addr.ToString().Contains(".5"), "顯示格式應有點");
        }
    }
}
