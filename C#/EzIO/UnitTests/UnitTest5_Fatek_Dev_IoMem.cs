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

using EzComm;
using EzIO.Mem;
using EzPlc.Fatek;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTest5_Fatek_Dev_IoMem : UnitTest_Base
    {
        #region STATIC_MEMBERS
        static IoDevice _ioDevice;
        static IoMemory _ioMem;
        #endregion

        static IoMemory ConfigIoDevice(bool usingHost32, bool force = false)
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
        static void SLEEP(int ms = 0)
        {
            if (ms > 0)
                System.Threading.Thread.Sleep(ms);
        }

        [ClassInitialize]
        public static void GlobalSetup(TestContext context)
        {
            ConfigIoDevice(false);
        }

        [ClassCleanup]
        public static void GlobalTeardown()
        {
            DisposeIoDevice();
        }

        /// <summary>
        /// 直接於 IoPoint 進行讀寫 (M)
        /// </summary>
        [TestMethod]
        public void Test_01_IoPoints()
        {
            var ioMem = ConfigIoDevice(false);
            var ioPoints = new[]
            {
                ioMem["M1"],
                ioMem["M16"],
                ioMem["M100"],
            };

            _TRACE(ioPoints);

            foreach (var ioPoint in ioPoints)
            {
                bool on = !ioPoint.IsOn;
                string symbol = on ? "ON" : "OFF";

                _TRACE($"測試直接寫入 {ioPoint} << {symbol}");
                ioPoint.Set(on, IoPriority.Directly);

                SLEEP();
                ioPoint.Read(IoPriority.Directly);

                _TRACE(ioPoint);
                Assert.AreEqual(on, ioPoint.IsOn);
                Assert.AreEqual(!on, ioPoint.IsOff);
            }
        }

        /// <summary>
        /// 直接於 IoPointReg (16-bit) 進行讀寫 (R, D, WM, WX, WY)
        /// </summary>
        [TestMethod]
        public void Test_02_IoPointRegs()
        {
            var ioMem = ConfigIoDevice(false);

            var ioPoints = new[] {
                ioMem["R100"],
                ioMem["R200"],
                ioMem["WM32"],
                ioMem["WM128"],
            };

            _TRACE(ioPoints);

            foreach (IoPoint ioPoint in ioPoints)
            {
                uint value = (uint)_rnd.Next(5120);

                _TRACE($"測試直接寫入 {ioPoint} <- {value}");
                ioPoint.Write(value, IoPriority.Directly);

                SLEEP();
                ioPoint.Read(IoPriority.Directly);

                _TRACE(ioPoint);
                Assert.AreEqual(value, ioPoint.Data);
            }
        }

        [TestMethod]
        public void Test_03_BitPoints_In_Host16()
        {
            var ioMem = ConfigIoDevice(false);     // 確保使用 WORD 16-bit Host

            // M1 到 M15 應該都映射到 WM0
            IoPoint m1 = ioMem["M1"];
            IoPoint m15 = ioMem["M15"];
            IoPoint m16 = ioMem["M16"];     // 映射到 WM16 

            var ioPoints = new[] { m1, m15, m16 };

            // 列出 HOST
            foreach (var ioPoint in ioPoints)
            {
                _TRACE($"{ioPoint.Address.ToCommString()}, Host.Key = {ioPoint.Host.KeyName}");
            }

            // 驗證位址數字
            for (int i = 0, N = ioPoints.Length; i < N; i++)
            {
                var p1 = ioPoints[i];
                for (int j = i + 1; j < N; j++)
                {
                    var p2 = ioPoints[j];
                    Assert.AreNotEqual(p1.Address.Address, p2.Address.Address,
                        $"{p1} @{p1.Address.Address} 與 {p2} @{p2.Address.Address} 定址必須不相同");
                }
            }

            // 驗證 M0 與 M15 是否屬於同一個 Register 實體
            Assert.AreEqual(m1.Host, m15.Host, $"{m1} 與 {m15} 應屬於同一個 WM0 暫存器");
            Assert.AreNotEqual(m1.Host, m16.Host, $"{m1} 與 {m16} 應屬於不同的暫存器 (WM0 vs WM16)");

            // 驗證寫入 M0 是否會影響到同一個 Word 內的其他位元 (不應互相覆蓋)
            m1.Set(true, IoPriority.Directly);
            m15.Set(false, IoPriority.Directly);

            SLEEP();
            m1.Read(IoPriority.Directly);
            m15.Read(IoPriority.Directly);

            Assert.IsTrue(m1.IsOn);
            Assert.IsFalse(m15.IsOn);
        }

        [TestMethod]
        public void Test_04_IoMemoryBank_Access()
        {
            var ioMem = ConfigIoDevice(false);

            // 建立一個從 R100 開始，長度為 10 的 Bank
            var R = ioMem.GetBank("R100", 10);

            // 測試絕對位址索引
            // 初始狀態下, 可以自動建立實體 (AutoAllocateEnabled==True)
            var r108 = R[108];
            Assert.IsNotNull(r108, "開啟 autoAllocate 後應建立實體");
            Assert.AreEqual(r108.Address.Address, 108, "位址應精確匹配 R108");

            // 測試超出邊界
            var r99 = R[99];
            var r110 = R[110];
            Assert.IsNull(r99, "低於起始位址應回傳 null");
            Assert.IsNull(r110, "超過 Span 範圍應回傳 null");

            // 關閉自動建立
            R.AutoAllocateEnabled = false;
            var r101 = R[101];
            Assert.IsNull(r101, "未開啟 AutoAllocateEnabled 前應回傳 null");
        }

        [TestMethod]
        public void Test_05_MemoryEnumeration_16Bit_Host()
        {
            var ioMem = ConfigIoDevice(false, force: true);      // 確保使用 WORD 16-bit Host

            // 1. 建立點位 (stationID == 2)
            var M = ioMem.GetBank("2:M80", 16);
            var R = ioMem.GetBank("2:R100", 16);
            var m0 = ioMem["2:M0"];
            var m18 = ioMem["2:M18"];
            var m80 = M[80];
            var m95 = M[95];
            var r100 = R[100];
            var r115 = R[115];

            // 2. 使用 LINQ 或 foreach 收集所有已分配的 Key
            var allocatedKeys = ioMem.IterRegs().Select(reg => reg.Address.KeyName).ToList();

            // 3. 驗證特定 Host 是否存在
            // M0 映射後的 Host Key 應為 "2:WM0"
            Assert.IsTrue(allocatedKeys.Contains("2:WM0"), "應該包含 WM0");
            // M18 映射後的 Host Key 應為 "2:WM16"
            Assert.IsTrue(allocatedKeys.Contains("2:WM16"), "應該包含 WM16");
            // M80, M95 映射後的 Host Key 應為 "2:WM80"
            Assert.IsTrue(allocatedKeys.Contains("2:WM80"), "應該包含 WM80");
            // R100 是獨立暫存器，Key 應為 "2:R100"
            Assert.IsTrue(allocatedKeys.Contains("2:R100"), "應該包含 R100");
            // R105 是獨立暫存器，Key 應為 "2:R105"
            Assert.IsTrue(allocatedKeys.Contains("2:R100"), "應該包含 R105");

            // 4. 驗證總數量是否正確
            // count == 1(WM0) + 1(WM16) + 1 WM80) + 2 (R1000,R1015) == 5
            Assert.AreEqual(5, allocatedKeys.Count, "枚舉到的 Host 數量不符預期");
        }

        [TestMethod]
        public void Test_32_00_Hosts_MappingTypes()
        {
            // 測試 16-bit 映射
            var mem16 = ConfigIoDevice(false, force: true);
            mem16.GetPoint("M33");
            var reg16 = mem16.IterRegs().First();
            Assert.AreEqual("WM0032", reg16.Address.ToCommString());
            Assert.AreEqual(16, reg16.Address.Bits);

            // 測試 32-bit 映射
            var mem = ConfigIoDevice(true);
            mem.GetPoint("M33");
            var reg32 = mem.IterRegs().First();
            Assert.AreEqual("DWM0032", reg32.Address.ToCommString());
            Assert.AreEqual(32, reg32.Address.Bits);
        }

        /// <summary>
        /// 直接於 IoPointReg (32-bit) 進行讀寫 (DR, DD, DWM, DWX, DWY)
        /// </summary>
        [TestMethod]
        public void Test_32_01_IoPointRegs32()
        {
            var ioMem = ConfigIoDevice(true, force: true);

            var ioPoints = new[] {
                ioMem["DR100"],
                ioMem["DR200"],
                ioMem["DWM32"],
                ioMem["DWM160"]
            };

            _TRACE(ioPoints);

            foreach (IoPoint ioPoint in ioPoints)
            {
                uint value = (uint)_rnd.Next(5120);

                _TRACE($"測試直接寫入 {ioPoint} <- {value}");
                ioPoint.Write(value, IoPriority.Directly);

                SLEEP();
                ioPoint.Read(IoPriority.Directly);

                _TRACE(ioPoint);
                Assert.AreEqual(value, ioPoint.Data);
            }
        }

        [TestMethod]
        public void Test_32_02_BitPoints_In_Host32()
        {
            var ioMem = ConfigIoDevice(true);      // 確保使用 DWORD 32-bit Host

            // M0 到 M15 應該都映射到 DWM0
            IoPoint m0 = ioMem["M0"];
            IoPoint m31 = ioMem["M31"];
            IoPoint m32 = ioMem["M32"];     // 映射到 DWM32 

            var ioPoints = new[] { m0, m31, m32 };

            // 列出 HOST
            foreach (var ioPoint in ioPoints)
            {
                _TRACE($"{ioPoint.Address.ToCommString()}, Host.Key = {ioPoint.Host.KeyName}");
            }

            // 驗證位址數字
            for (int i = 0, N = ioPoints.Length; i < N; i++)
            {
                var p1 = ioPoints[i];
                for (int j = i + 1; j < N; j++)
                {
                    var p2 = ioPoints[j];
                    Assert.AreNotEqual(p1.Address.Address, p2.Address.Address,
                        $"{p1} @{p1.Address.Address} 與 {p2} @{p2.Address.Address} 定址必須不相同");
                }
            }

            // 驗證 M0 與 M31 是否屬於同一個 Register 實體
            Assert.AreSame(m0.Host, m31.Host, $"{m0} 與 {m31} 應屬於同一個 DWM0 暫存器");
            Assert.AreNotSame(m0.Host, m32.Host, $"{m0} 與 {m32} 應屬於不同的暫存器 (DWM0 vs DWM32)");

            // 驗證寫入 M0 是否會影響到同一個 DWord 內的其他位元 (不應互相覆蓋)
            m0.Set(true, IoPriority.Directly);
            m31.Set(false, IoPriority.Directly);

            SLEEP();
            m0.Read(IoPriority.Directly);
            m31.Read(IoPriority.Directly);

            Assert.IsTrue(m0.IsOn);
            Assert.IsFalse(m31.IsOn);
        }

        [TestMethod]
        public void Test_32_03_Register_Uniqueness()
        {
            // 故意用不同方式請求同一個範圍
            foreach (bool usingHost32 in new bool[] { true, false })
            {
                var mem = ConfigIoDevice(usingHost32, true);

                var m1 = mem["M160"];
                var m2 = mem["M161"];
                var host = usingHost32 ? mem["DWM160"] : mem["WM160"];

                // 驗證枚舉結果
                int hostCount = mem.IterRegs().Count();
                Assert.AreEqual(1, hostCount, $"{m1}, {m2}, {host} 應該全部指向同一個 {host} 實體");
            }
        }

        [TestMethod]
        public void Test_32_04_MemoryEnumeration_32Bit_Host()
        {
            var ioMem = ConfigIoDevice(true);      // 確保使用 DWORD 32-bit Host

            // 1. 建立點位
            var M = ioMem.GetBank("M512", 32);
            var R = ioMem.GetBank("R1000", 32);
            var m0 = ioMem["M0"];
            var m512 = M[512];
            var m530 = M[530];
            var r1024 = R[1024];
            var r1025 = R[1025];

            // 2. 使用 LINQ 或 foreach 收集 ioMem 所有 regs 已分配的 KeyName
            var allocatedKeys = ioMem.IterRegs().Select(reg => reg.KeyName).ToList();

            // 3. 驗證特定 Host 是否存在
            // M0 映射後的 Host Key 應為 "DWM0"
            Assert.IsTrue(allocatedKeys.Contains("DWM0"), "應該包含 DWM0");

            // M512, M530 映射後的 Host Key 應為 "DWM512"
            Assert.IsTrue(allocatedKeys.Contains("DWM512"), "應該包含 DWM512");

            // R1000 是獨立暫存器，Key 應為 "R1000"
            Assert.IsTrue(allocatedKeys.Contains("R1000"), "應該包含 R1000");

            // R1024 是獨立暫存器，Key 應為 "R1024"
            Assert.IsTrue(allocatedKeys.Contains("R1024"), "應該包含 R1024");

            // R1025 是獨立暫存器，Key 應為 "R1025"
            Assert.IsTrue(allocatedKeys.Contains("R1025"), "應該包含 R1025");

            // 4. 驗證 host regs 總數量是否正確
            //      count == 1(WM0) + 1(DWM512) + 3 (R1000,R1024,R1025) == 5
            Assert.AreEqual(5, allocatedKeys.Count, "枚舉到的 Host 數量不符預期");
        }
    }
}