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

using EzComm;
using EzIO.Mem;
using EzPlc.Fatek;
using EzPlc.Fatek.AutoScan;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTest6_Fatek_Dev_AutoScan : UnitTest_Base
    {
        #region PRIVATE_MEMBERS
        private FatekScanCmdsBuilder _builder = new FatekScanCmdsBuilder();
        static IoDevice _ioDevice;
        static IoMemory _ioMem;
        #endregion

        static IoMemory ConfigIoDevice(bool usingHost32, bool force)
        {
            if (force)
            {
                DisposeIoDevice();
            }

            if (_ioMem is FatekIoMemory fkIoMem)
            {
                if (fkIoMem.OPT_USING_HOST_32BIT != usingHost32)
                    DisposeIoDevice();
            }

            if (_ioMem == null)
            {
                // 生成
                var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };
                _ioDevice = EzFatekFactory.OpenDevice(settings);
                _ioMem = EzFatekFactory.GetIoMemory(_ioDevice, usingHost32);
                string tag = usingHost32 ? " (32-bit)" : " (16-bit)";
                _TRACE($"[生成] {_ioDevice.KeyName}{tag}");
            }

            return _ioMem;
        }
        static void DisposeIoDevice()
        {
            if (_ioDevice != null)
            {
                _TRACE($"[釋放] {_ioDevice.KeyName}");
            }

            EzFatekFactory.DisposeAll();
            _ioDevice = null;
            _ioMem = null;
        }

        [ClassInitialize]
        public static void GlobalSetup(TestContext context)
        {
            DisposeIoDevice();
        }

        [ClassCleanup]
        public static void GlobalTeardown()
        {
            DisposeIoDevice();
        }

        /// <summary>
        /// 測試點位合併：相鄰的 R 16-bit 點位應合併在同一個 Bank
        /// </summary>
        [TestMethod]
        public void Test_01_BuildBanks_MergeContinuousPoints()
        {
            var ioMem = ConfigIoDevice(false, true);

            // Arrange: 建立 R100, R101, R102, R116
            var regs = new[] {
                ioMem["R100"],
                ioMem["R101"],
                ioMem["R102"],
            };

            // TRACE
            foreach (var reg in regs)
                _TRACE(reg.Address);

            // Act: 設定 gap 為預設 (16)
            var banks = _builder.BuildBanks(ioMem, regs, gap: 16);

            // TRACE
            foreach (var bank in banks)
                _TRACE(bank);

            // Assert
            Assert.AreEqual(1, banks.Length, "連續點位應合併為 1 個 Bank");
            Assert.AreEqual(100, banks[0].AddressBase.Address);
            Assert.AreEqual(3, banks[0].Span, "R100-R102 跨度應為 3");

            _TRACE("\nAutoScan cmds:");
            var cmds = _builder.BuildCmds(_ioDevice, regs, gap: 16);
            foreach (var cmd in cmds)
                _TRACE(cmd.ToString());
        }

        /// <summary>
        /// 測試點位合併：相鄰的 WM (16-bit) 點位, 應合併在同一個 Bank.
        /// </summary>
        [TestMethod]
        public void Test_02_BuildBanks_MergeContinuousPoints_WM()
        {
            var ioMem = ConfigIoDevice(false, true);

            // Arrange: 建立 WM16, WM32, ..., WM128
            var regs = new[] {
                ioMem["WM16"],
                ioMem["WM32"],
                //_ioMem["WM48"],   // GAP
                //_ioMem["WM64"],   // GAP
                ioMem["WM80"],
                ioMem["WM96"],
                ioMem["WM128"],
            };

            // TRACE
            foreach (var reg in regs)
                _TRACE(reg.Address);

            // Act: 設定 gap 為 3
            var banks = _builder.BuildBanks(ioMem, regs, gap: 3);

            // TRACE
            foreach (var bank in banks)
                _TRACE(bank);

            // Assert
            int startAddrN = regs[0].Address.Address;
            int endAddrN = regs[regs.Length - 1].Address.Address;
            int targetSpan = (endAddrN - startAddrN) / 16 + 1;
            int actualCount = regs.Length;
            Assert.AreEqual(1, banks.Length, "連續點位應合併為 1 個 Bank");
            Assert.AreEqual(startAddrN, banks[0].AddressBase.Address);
            Assert.AreEqual(targetSpan, banks[0].Span, $"跨度 應為 {targetSpan}");
            Assert.AreEqual(actualCount, banks[0].ActualNumber, $"實際數量 應為 {actualCount}");

            _TRACE("\nAutoScan cmds:");
            var cmds = _builder.BuildCmds(_ioDevice, regs, gap: 3);
            foreach (var cmd in cmds)
                _TRACE(cmd.ToString());
        }

        /// <summary>
        /// 測試 Gap 拆分：間距大於設定值時應自動拆分 Bank
        /// </summary>
        [TestMethod]
        public void Test_03_BuildBanks_SplitByGap()
        {
            var ioMem = ConfigIoDevice(false, true);

            // Arrange: 建立 WM16, WM32, ..., WM128
            var regs = new[] {
                ioMem["WM16"],
                ioMem["WM32"],
                //_ioMem["WM48"],   // GAP
                //_ioMem["WM64"],   // GAP
                ioMem["WM80"],
                ioMem["WM96"],
                //_ioMem["WM112"],  // GAP
                ioMem["WM128"],
            };

            // TRACE
            foreach (var reg in regs)
                _TRACE(reg.Address);

            // Act: 設定 gap 為 2
            var banks = _builder.BuildBanks(ioMem, regs, gap: 2);

            // TRACE
            foreach (var bank in banks)
                _TRACE(bank);

            // Assert
            Assert.AreEqual(2, banks.Length, "連續點位應合併為 2 個 Bank");
            // Bank0: WM16 ~ WM32
            Assert.AreEqual(16, banks[0].AddressBase.Address);
            Assert.AreEqual(2, banks[0].Span, $"跨度 應為 2");
            Assert.AreEqual(2, banks[0].ActualNumber, $"實際數量 應為 2");
            // bank1: WM80 ~ WM128
            Assert.AreEqual(80, banks[1].AddressBase.Address);
            Assert.AreEqual(4, banks[1].Span, $"跨度 應為 4");
            Assert.AreEqual(3, banks[1].ActualNumber, $"實際數量 應為 3");
        }

        /// <summary>
        /// 測試不同 Category 隔離：M 區與 R 區不應合併
        /// </summary>
        [TestMethod]
        public void Test_04_BuildBanks_CategoryIsolation()
        {
            var ioMem = ConfigIoDevice(false, true);

            // Arrange
            var regs = new []
            {
                ioMem["WM128"],
                ioMem["R128"]
            };

            // TRACE
            foreach (var reg in regs)
                _TRACE(reg.Address);

            // Act
            var banks = _builder.BuildBanks(ioMem, regs, gap: 100);

            // TRACE
            foreach (var bank in banks)
                _TRACE(bank);

            // Assert
            int N = regs.Length;
            Assert.AreEqual(N, banks.Length, "不同 Category 必須拆分 Bank");
        }

        /// <summary>
        /// 測試 32-bit 對齊與跨度計算：驗證 + (wordSpan - 1) 邏輯
        /// </summary>
        [TestMethod]
        public void Test_05_BuildBanks_DWordSpanAndAlignment()
        {
            var ioMem = ConfigIoDevice(usingHost32: true, force: true);

            // Arrange: 
            // 1. DR100 (32-bit, 佔用 100, 101)
            // 2. DR102 (32-bit, 占用 102, 103)
            // 3. R103 (16-bit)
            var regs = new[]
            {
                ioMem["DR100"],    // 32-bit 佔 100, 101
                ioMem["DR102"],    // 32-bit 佔 102, 103
                ioMem["R103"],     // 16-bit
                //_ioMem["R104"],   // 16-bit (GAP)
                ioMem["R105"],     // 16-bit
            };

            // TRACE
            foreach (var reg in regs)
                _TRACE(reg.Address);

            // Act
            // 目前 FatekIoMemory 實作, 會把 DR 與 R 分開建立不同的 banks
            // Bank0 : DR0100, Span=2, Actual=2
            // Bank1 : R00103, Span=3, Actual=2
            var banks = _builder.BuildBanks(ioMem, regs, gap: 5);

            foreach (var bank in banks)
                _TRACE(bank);

            // Assert
            Assert.AreEqual(2, banks.Length);
            // Bank0: DR100 ~ DR103
            Assert.AreEqual(100, banks[0].AddressBase.Address);
            Assert.AreEqual(2, banks[0].Span, $"跨度 應為 2");
            Assert.AreEqual(2, banks[0].ActualNumber, $"實際數量 應為 2");
            // bank1: R103 ~ R103
            Assert.AreEqual(103, banks[1].AddressBase.Address);
            Assert.AreEqual(3, banks[1].Span, $"跨度 應為 3");
            Assert.AreEqual(2, banks[1].ActualNumber, $"實際數量 應為 2");

            _TRACE("\nAutoScan cmds:");
            var cmds = _builder.BuildCmds(_ioDevice, regs, gap: 5);
            foreach (var cmd in cmds)
                _TRACE(cmd.ToString());
        }
    }
}
