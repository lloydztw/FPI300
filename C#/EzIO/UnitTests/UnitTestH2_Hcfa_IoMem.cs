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

using EzIO.Mem;
using EzPlc.Hcfa;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTestH2_Hcfa_IoMemory : UnitTest_Base
    {
        private HcfaIoMemory _ioMem;

        [TestInitialize]
        public void Setup()
        {
            _ioMem = new HcfaIoMemory();
        }

        [TestMethod]
        public void Test01_Hcfa_IoMem_IX()
        {
            int N = 32;
            var baseAddress = new HcfaAddress($"0:IX0.0");
            var addresses = new IAddress[N];
            for (int i = 0; i < N; i++)
                addresses[i] = baseAddress.Offset(i);

            var ioPoints = Array.ConvertAll(addresses, a => _ioMem[a]);
            var testData = Array.ConvertAll(addresses, a => _rnd.NextDouble() > 0.5);

            for (int i = 0; i < N; i++)
            {
                ioPoints[i].Set(testData[i]);

                var addr = ioPoints[i].Address;
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {addr.ToCommString()}");
            }

            _TRACE(ioPoints);

            for (int i = 0; i < N; i++)
            {
                bool on = testData[i];
                var ioPoint = ioPoints[i];
                Assert.IsTrue(ioPoint.IsOn == on);
                Assert.IsTrue(ioPoint.IsOff == !on);
            }

            // Independent Test
            for (int i = 0; i < N; i++)
            {
                var oldDatas = Array.ConvertAll(ioPoints, p => p.Data);
                bool on = !ioPoints[i].IsOn;
                ioPoints[i].Set(on);
                for (int j = 0; j < N; j++)
                {
                    if (j != i)
                        Assert.AreEqual(ioPoints[j].Data, oldDatas[j]);
                }
                Assert.IsTrue(ioPoints[i].IsOn == on);
            }
        }

        [TestMethod]
        public void Test02_Hcfa_IoMem_QX()
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

            int N = addresses.Length;
            var addrs = Array.ConvertAll(addresses, n => new HcfaAddress($"QX{n}.{_rnd.Next() % 8}"));
            var ioPoints = Array.ConvertAll(addrs, a => _ioMem[a]);
            var testData = Array.ConvertAll(addrs, a => _rnd.NextDouble() > 0.5);

            for (int i = 0; i < N; i++)
            {
                ioPoints[i].Set(testData[i]);
                var addr = ioPoints[i].Address;
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {addr.ToCommString()}");
            }

            _TRACE(ioPoints);

            for (int i = 0; i < N; i++)
            {
                bool on = testData[i];
                var ioPoint = ioPoints[i];
                Assert.IsTrue(ioPoint.IsOn == on);
                Assert.IsTrue(ioPoint.IsOff == !on);
            }

            // Independent Test
            for (int i = 0; i < N; i++)
            {
                var oldDatas = Array.ConvertAll(ioPoints, p => p.Data);
                bool on = !ioPoints[i].IsOn;
                ioPoints[i].Set(on);
                for (int j = 0; j < N; j++)
                {
                    if (j != i)
                        Assert.AreEqual(ioPoints[j].Data, oldDatas[j]);
                }
                Assert.IsTrue(ioPoints[i].IsOn == on);
            }
        }

        [TestMethod]
        public void Test03_Hcfa_IoMem_QB()
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

            int N = addresses.Length;
            var addrs = Array.ConvertAll(addresses, n => new HcfaAddress($"QB{n}.{_rnd.Next() % 8}"));
            var ioPoints = Array.ConvertAll(addrs, a => _ioMem[a]);
            var testData = Array.ConvertAll(addrs, a => _rnd.NextDouble() > 0.5);

            for (int i = 0; i < N; i++)
            {
                ioPoints[i].Set(testData[i]);
                var addr = ioPoints[i].Address;
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {addr.ToCommString()}");
            }

            _TRACE(ioPoints);

            for (int i = 0; i < N; i++)
            {
                bool on = testData[i];
                var ioPoint = ioPoints[i];
                Assert.IsTrue(ioPoint.IsOn == on);
                Assert.IsTrue(ioPoint.IsOff == !on);
            }

            // Independent Test
            for (int i = 0; i < N; i++)
            {
                var oldDatas = Array.ConvertAll(ioPoints, p => p.Data);
                bool on = !ioPoints[i].IsOn;
                ioPoints[i].Set(on);
                for (int j = 0; j < N; j++)
                {
                    if (j != i)
                        Assert.AreEqual(ioPoints[j].Data, oldDatas[j]);
                }
                Assert.IsTrue(ioPoints[i].IsOn == on);
            }
        }

        [TestMethod]
        public void Test04_Hcfa_IoMem_MW()
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

            int N = addresses.Length;
            var addrs = Array.ConvertAll(addresses, n => new HcfaAddress($"MW{n}"));
            var ioPoints = Array.ConvertAll(addrs, a => _ioMem[a]);
            var testData = Array.ConvertAll(addrs, a => (uint)(_rnd.Next() & 0xFFFF));

            for (int i = 0; i < N; i++)
            {
                ioPoints[i].Data = testData[i];
                var addr = ioPoints[i].Address;
                _TRACE($"位址= {addr.KeyName}\t=>\tModbus通訊位址= {addr.ToCommString()}");
            }

            _TRACE(ioPoints);

            for (int i = 0; i < N; i++)
            {
                var ioPoint = ioPoints[i];
                Assert.IsTrue(ioPoint.Data == testData[i]);
            }

            // Independent Test
            for (int i = 0; i < N; i++)
            {
                var oldDatas = Array.ConvertAll(ioPoints, p => p.Data);
                var newData = (uint)(_rnd.Next() & 0xFFFF);
                ioPoints[i].Data = newData;
                for (int j = 0; j < N; j++)
                {
                    if (j != i)
                        Assert.AreEqual(ioPoints[j].Data, oldDatas[j]);
                }
                Assert.IsTrue(ioPoints[i].Data == newData);
            }
        }

        [TestMethod]
        public void Test05_Hcfa_IoMemBank_IX()
        {
            var Bank0 = _ioMem.GetBank("IX0.0", 8);
            var Bank1 = _ioMem.GetBank("IX1.0", 8);
            var Bank2 = _ioMem.GetBank("IX2.0", 8);
            var Bank3 = _ioMem.GetBank("IX3.0", 8);
            var Banks = new[] { Bank0, Bank1, Bank2, Bank3 };

            foreach (var Bank in Banks)
            {
                var baseAddr = Bank.AddressBase;
                var prefix = baseAddr.KeyName.Split('.')[0];

                int index = _rnd.Next(8);

                // 相對 offset indexing
                var p = Bank[index];

                // In-SPAN 自動填充
                Assert.IsNotNull(p, "開啟 autoAllocate 後應建立實體");
                Assert.IsTrue(p.Address.KeyName.StartsWith(prefix), $"{p.Address} 不屬於 {prefix}");
                _TRACE($"位址= {p.Address.KeyName}\t=>\tModbus通訊位址= {p.Address.ToCommString()}\t=>\t{p.Data}");

                // Out-SPAN 測試超出邊界
                var q = Bank[Bank.Span];
                Assert.IsNull(q, "超過 Span 範圍應回傳 null");
            }

            // 外部配置
            int N = 32;
            var testData = new bool[N];
            var ioPoints = new IoPoint[N];
            if (true)
            {
                var baseAddr = new HcfaAddress("IX0.0");
                for (int i = 0; i < 32; i++)
                {
                    var addr = baseAddr.Offset(i);
                    bool on = _rnd.NextDouble() > 0.5;
                    testData[i] = on;
                    ioPoints[i] = _ioMem[addr];
                    ioPoints[i].Set(on);
                }
            }

            _TRACE(ioPoints);

            // 重新建構 BANK (Span=32)
            var BANK = _ioMem.GetBank("IX0.0", N);
            for (int i = 0; i < N; i++)
            {
                var p = ioPoints[i];
                var q = BANK[i];
                Assert.AreEqual(p, q, "應該相同!");
            }

            // 模擬 IoDevice 使用 BANK 方式, 更新數據.
            testData = Array.ConvertAll(ioPoints, _ => _rnd.NextDouble() > 0.5);
            for (int i = 0; i < N; i++)
            {
                uint one = testData[i] ? 1u : 0u;
                BANK.UpdateCacheByIndex(i, one);
            }

            // 檢查個別的 IoPoint 數據是否一致.
            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(ioPoints[i].IsOn, testData[i], "On/Off 應該相同!");
            }
        }

        [TestMethod]
        public void Test06_Hcfa_IoMemBank_QX()
        {
            int[] addresses = new int[]
            {
                //0,
                1016,
                1144,
                1272,
                1400,
                1528,
                1656,
                1784,
            };

            int N = addresses.Length;

            // 外部配置
            var ioPoints = Array.ConvertAll(addresses, a => _ioMem[$"QX{a}.{_rnd.Next() % 8}"]);
            var testData = Array.ConvertAll(ioPoints, _ => _rnd.NextDouble() > 0.5);
            
            // 初始值
            for (int i = 0; i < N; i++)
                ioPoints[i].Set(testData[i]);
            _TRACE(ioPoints);


            // address numbers
            var address0 = addresses[0] / 1000 * 1000;
            var addressN = addresses[N - 1];
            var addressD = (addressN - address0 + 99) / 100 * 100;

            // BANK
            int SPAN = addressD * 8;
            var BankQX = _ioMem.GetBank($"QX{address0}", SPAN);

            var baseAddr = BankQX.AddressBase;
            Assert.IsTrue(baseAddr.BitStride == 8);
            Assert.IsTrue(baseAddr.CateID == ioPoints[0].Address.CateID, "Category 必須相等!");

            _TRACE("\n檢查 BANK by IoAddress.BitsDiff");
            foreach (var p in ioPoints)
            {
                Assert.IsTrue(p.Address.BitStride == 8);

                int index = IoAddress.BitsDiff(p.Address, baseAddr);
                int diff = p.Address.UniqueID - baseAddr.UniqueID;
                var q = BankQX[index];

                _TRACE($"{p.Address} : index = {index}");

                Assert.AreEqual(index, diff, "兩者算法應該相同!");
                Assert.AreEqual(p, q, $"{p} {q} 應該相同!");
            }

            _TRACE($"\n枚舉 BANK[{BankQX.AddressBase}] 已經配置的點位 (ActualNumber= {BankQX.ActualNumber}, Span={BankQX.Span}) :");
            foreach (var p in BankQX)
            {
                _TRACE($"位址= {p.Address.KeyName}\t=>\tModbus通訊位址= {p.Address.ToCommString()}\t=>\t{p.Data}");
            }
            Assert.AreEqual(BankQX.Span, SPAN, $"Span 必須是 {SPAN}");
            Assert.AreEqual(BankQX.ActualNumber, N, $"ActualNumber 必須是 {N}");

            // 模擬 IoDevice 使用 BANK 方式, 更新數據.
            testData = Array.ConvertAll(ioPoints, _ => _rnd.NextDouble() > 0.5);
            for (int i = 0; i < N; i++)
            {
                uint one = testData[i] ? 1u : 0u;
                int index = ioPoints[i].Address.UniqueID - BankQX.AddressBase.UniqueID;
                BankQX.UpdateCacheByIndex(index, one);
            }

            // 檢查個別的 IoPoint 數據是否一致.
            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(ioPoints[i].IsOn, testData[i], "On/Off 應該相同!");
            }
        }

        [TestMethod]
        public void Test07_Hcfa_IoMemBank_QB()
        {
            int[] addresses = new int[]
            {
                //0,
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

            int N = addresses.Length;

            // 外部配置
            var ioPoints = Array.ConvertAll(addresses, a => _ioMem[$"QB{a}.{_rnd.Next() % 8}"]);
            var testData = Array.ConvertAll(ioPoints, _ => _rnd.NextDouble() > 0.5);

            // 初始值
            for (int i = 0; i < N; i++)
                ioPoints[i].Set(testData[i]);
            _TRACE(ioPoints);

            // address numbers
            var address0 = addresses[0] / 1000 * 1000;
            var addressN = addresses[N - 1];
            var addressD = (addressN - address0 + 99) / 100 * 100;

            // BANK
            int SPAN = addressD * 8;
            var BankQB = _ioMem.GetBank($"QB{address0}", SPAN);

            var baseAddr = BankQB.AddressBase;
            Assert.IsTrue(baseAddr.BitStride == 8);
            Assert.IsTrue(baseAddr.CateID == ioPoints[0].Address.CateID, "Category 必須相等!");

            _TRACE("\n檢查 BANK by IoAddress.BitsDiff");
            foreach (var p in ioPoints)
            {
                Assert.IsTrue(p.Address.BitStride == 8);

                int index = IoAddress.BitsDiff(p.Address, baseAddr);
                int diff = p.Address.UniqueID - baseAddr.UniqueID;
                var q = BankQB[index];

                _TRACE($"{p.Address} : index = {index}");

                Assert.AreEqual(index, diff, "兩者算法應該相同!");
                Assert.AreEqual(p, q, $"{p} {q} 應該相同!");
            }

            _TRACE($"\n枚舉 BANK[{BankQB.AddressBase}] 已經配置的點位 (ActualNumber= {BankQB.ActualNumber}, Span={BankQB.Span}) :");
            foreach (var p in BankQB)
            {
                _TRACE($"位址= {p.Address.KeyName}\t=>\tModbus通訊位址= {p.Address.ToCommString()}\t=>\t{p.Data}");
            }
            Assert.AreEqual(BankQB.Span, SPAN, $"Span 必須是 {SPAN}");
            Assert.AreEqual(BankQB.ActualNumber, N, $"ActualNumber 必須是 {N}");

            // 模擬 IoDevice 使用 BANK 方式, 更新數據.
            testData = Array.ConvertAll(ioPoints, _ => _rnd.NextDouble() > 0.5);
            for (int i = 0; i < N; i++)
            {
                uint one = testData[i] ? 1u : 0u;
                int index = ioPoints[i].Address.UniqueID - BankQB.AddressBase.UniqueID;
                BankQB.UpdateCacheByIndex(index, one);
            }

            // 檢查個別的 IoPoint 數據是否一致.
            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(ioPoints[i].IsOn, testData[i], "On/Off 應該相同!");
            }
        }

        [TestMethod]
        public void Test08_Hcfa_IoMemBank_MW()
        {
            int[] addresses = new int[]
            {
                //0,
                1000,
                1020,
                1040,
                1060,
                1100,
                1300,
                1340
            };

            int N = addresses.Length;

            // 外部配置
            var ioPoints = Array.ConvertAll(addresses, a => _ioMem[$"MW{a}"]);
            var testData = Array.ConvertAll(ioPoints, _ => (uint)(_rnd.Next() & 0xFFFF));

            // 初始值
            for (int i = 0; i < N; i++)
                ioPoints[i].Data = testData[i];
            _TRACE(ioPoints);

            // address numbers
            var address0 = addresses[0] / 1000 * 1000;
            var addressN = addresses[N - 1];
            var addressD = (addressN - address0 + 99) / 100 * 100;

            // BANK
            int SPAN = addressD;
            var BankMW = _ioMem.GetBank($"MW{address0}", SPAN);

            var baseAddr = BankMW.AddressBase;
            Assert.IsTrue(baseAddr.CateID == ioPoints[0].Address.CateID, "Category 必須相等!");

            _TRACE("\n檢查 BANK by IoAddress.BitsDiff");
            foreach (var p in ioPoints)
            {
                int address = p.Address.Address;
                var q = BankMW[address];
                Assert.AreEqual(p, q, $"{p} {q} 應該相同!");
            }

            _TRACE($"\n枚舉 BANK[{BankMW.AddressBase}] 已經配置的點位 (ActualNumber= {BankMW.ActualNumber}, Span={BankMW.Span}) :");
            foreach (var p in BankMW)
            {
                _TRACE($"位址= {p.Address.KeyName}\t=>\tModbus通訊位址= {p.Address.ToCommString()}\t=>\t{p.Data}");
            }
            Assert.AreEqual(BankMW.Span, SPAN, $"Span 必須是 {SPAN}");
            Assert.AreEqual(BankMW.ActualNumber, N, $"ActualNumber 必須是 {N}");

            // 模擬 IoDevice 使用 BANK 方式, 更新數據.
            testData = Array.ConvertAll(ioPoints, _ => (uint)(_rnd.Next() & 0xFFFF));
            for (int i = 0; i < N; i++)
            {
                int index = ioPoints[i].Address.UniqueID - BankMW.AddressBase.UniqueID;
                BankMW.UpdateCacheByIndex(index, testData[i]);
            }

            // 檢查個別的 IoPoint 數據是否一致.
            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(ioPoints[i].Data, testData[i], "value 應該相同!");
            }
        }
    }
}
