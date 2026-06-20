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
using EzGlueDispenser.IO.S3;
using EzIO.Mem;
using EzPlc.Hcfa;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTestH5_Hcfa_Dev_IoMem : UnitTest_Base
    {
        static EzGlueDispenserIoS3 _glueIO;

        [ClassInitialize]
        public static void GlobalSetup(TestContext context)
        {
            EzHcfaFactory.DisposeAll();

            var settings = new EzTcpIpSettings() { 
                Port= HCFA_IP_PORT, 
                IsSim = HCFA_IS_SIM, 
                UsingRandomSim = HCFA_IS_SIM
            };

            _glueIO = new EzGlueDispenserIoS3(settings);
        }

        [ClassCleanup]
        public static void GlobalTeardown()
        {
            _glueIO?.Dispose();
            _glueIO = null;
            EzHcfaFactory.DisposeAll();
        }

        /// <summary>
        /// 直接於 IX, QX, QB 進行讀寫
        /// </summary>
        [TestMethod]
        [DataRow(HcfaCateEnum.IX)]
        [DataRow(HcfaCateEnum.QX)]
        [DataRow(HcfaCateEnum.QB)]
        public void Test_01_IoPoints_1Bit(HcfaCateEnum cate)
        {
            var ioMem = _glueIO.IoMem;

            // 取得 所有相關 點位
            var ioPoints = ioMem.GetCatePoints((int)cate);

            _TRACE($"\n\n單點測試 {cate}:\n");

            foreach (IoPoint ioPoint in ioPoints)
            {
                bool on = _rnd.NextDouble() > 0.5;
                string symbol = on ? "ON" : "OFF";

                // IX 是 唯讀
                if (cate == HcfaCateEnum.IX)
                {
                    _TRACE($"測試讀取 {ioPoint}");
                    ioPoint.Read(IoPriority.Directly);
                    _TRACE(ioPoint);
                }
                else
                {
                    _TRACE($"測試直接寫入 {ioPoint} << {symbol}");
                    ioPoint.Set(on, IoPriority.Directly);
                    ioPoint.Read(IoPriority.Directly);
                    _TRACE(ioPoint);
                    Assert.AreEqual(on, ioPoint.IsOn);
                    Assert.AreEqual(!on, ioPoint.IsOff);
                }
            }
        }

        /// <summary>
        /// 直接於 MW 進行讀寫
        /// </summary>
        [TestMethod]
        public void Test_02_IoPoints_MW()
        {
            var ioMem = _glueIO.IoMem;

            // 取得 所有 MW 點位
            var ioPoints = ioMem.GetCatePoints((int)HcfaCateEnum.MW);

            _TRACE($"\n\n單點測試 {HcfaCateEnum.MW}:\n");

            foreach (IoPoint ioPoint in ioPoints)
            {
                uint testData = (uint)_rnd.Next() & 0xFFFF;

                _TRACE($"測試直接寫入 {ioPoint} << {testData}");
                ioPoint.Write(testData, IoPriority.Directly);
                ioPoint.Read(IoPriority.Directly);
                _TRACE(ioPoint);
                Assert.AreEqual(testData, ioPoint.Data);
            }
        }

        [TestMethod]
        public void Test_03_IoMemoryBank_IX()
        {
            var ioMem = _glueIO.IoMem;

            // 建立一個從 IX0.0 開始，長度為 32 的 Bank
            var IX = ioMem.GetBank("IX0.0", 32);

            // 測試絕對位址索引
            // 初始狀態下, 可以自動建立實體 (AutoAllocateEnabled==True)
            foreach (var bit in new int[] { 0, 7, 8, 15, 16, 23, 24, 31 })
            {
                var ioPoint = IX[bit];
                _TRACE(ioPoint);
                Assert.IsNotNull(ioPoint, "開啟 autoAllocate 後應建立實體");
                string targetStr = $"IX{bit / 8}.{bit % 8}";
                Assert.AreEqual(ioPoint.KeyName, targetStr, $"位址應精確匹配 {targetStr}");
            }

            // 測試超出邊界
            var ix99 = IX[99];
            Assert.IsNull(ix99, "超過 Span 範圍應回傳 null");
        }

        [TestMethod]
        public void Test_04_IoMemoryBank_QX()
        {
            var ioMem = _glueIO.IoMem;

            // 建立一個從 QX0.0 開始，長度為 32 的 Bank
            var QX = ioMem.GetBank("QX0.0", 32);

            // 選擇子集
            var addresses = new int[] { 0, 7, 8, 15, 16, 23, 24, 31 };
            var ioPoints = new Dictionary<int, IoPoint>();
            var testData = new Dictionary<int, bool>();

            // 測試絕對位址索引
            // 初始狀態下, 可以自動建立實體 (AutoAllocateEnabled==True)
            foreach (var addrNumber in addresses)
            {
                var ioPoint = QX[addrNumber];
                
                //模擬初始值
                bool on = _rnd.NextDouble() > 0.5;
                ioPoint.Set(on, IoPriority.Directly);
                testData.Add(addrNumber, on);

                _TRACE(ioPoint);
                Assert.IsNotNull(ioPoint, "開啟 autoAllocate 後應建立實體");
                string ezIoName = $"QX{addrNumber / 8}.{addrNumber % 8}";
                Assert.AreEqual(ioPoint.KeyName, ezIoName, $"位址應精確匹配 {ezIoName}");
                ioPoints.Add(addrNumber, ioMem[ezIoName]);
            }

            // 測試超出邊界
            var qx99 = QX[99];
            Assert.IsNull(qx99, "超過 Span 範圍應回傳 null");

            // 模擬使用 Bank 讀出
            for (int i = 0, N = QX.Span; i < N; i++)
            {
                var ioPoint = QX.AtOffset(i);
                if (ioPoint == null)
                    continue;
                Assert.IsTrue(ioPoints.Values.Contains(ioPoint));
            }

            // 比對
            foreach (int addrNumber in addresses)
            {
                var ioPoint = ioPoints[addrNumber];
                Assert.AreEqual(ioPoint.IsOn, testData[addrNumber]);
            }
        }

        [TestMethod]
        public void Test_05_IoMemoryBank_QB()
        {
            var ioMem = _glueIO.IoMem;
            var ioPoints = ioMem.GetCatePoints((int)HcfaCateEnum.QB);

            //模擬初始值
            foreach (var ioPoint in ioPoints)
            {
                bool on = _rnd.NextDouble() > 0.5;
                ioPoint.Set(on, IoPriority.Directly);
            }

            _TRACE(ioPoints);

            // address numbers
            int N = ioPoints.Count;
            var address0 = ioPoints[0].Address.Address / 1000 * 1000;
            var addressN = ioPoints[N - 1].Address.Address;
            var addressD = (addressN - address0 + 99) / 100 * 100;
            var stationID = ioPoints[0].Address.StationID;

            // BANK
            int SPAN = addressD * 8;
            var BankQB = ioMem.GetBank($"{stationID}:QB{address0}", SPAN);
            _TRACE($"BankQB = {BankQB}");

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
                Assert.AreEqual(p, q, $"{p}, {q} 應該相同!");
            }

            _TRACE($"\n枚舉 BANK[{BankQB.AddressBase}] 已經配置的點位 (ActualNumber= {BankQB.ActualNumber}, Span={BankQB.Span}) :");
            foreach (var p in BankQB)
            {
                _TRACE($"位址= {p.Address.KeyName}\t=>\tModbus通訊位址= {p.Address.ToCommString()}\t=>\t{p.Data}");
            }
            Assert.AreEqual(BankQB.Span, SPAN, $"Span 必須是 {SPAN}");
            Assert.AreEqual(BankQB.ActualNumber, N, $"ActualNumber 必須是 {N}");

            // 模擬 IoDevice 使用 BANK 方式, 更新數據.
            var testData = Array.ConvertAll(ioPoints.ToArray(), _ => _rnd.NextDouble() > 0.5);
            for (int i = 0; i < N; i++)
            {
                uint one = testData[i] ? 1u : 0u;
                int index = ioPoints[i].Address.UniqueID - BankQB.AddressBase.UniqueID;
                ioPoints[i].Write(one, IoPriority.Directly);
                BankQB.UpdateCacheByIndex(index, one);
            }

            // 檢查個別的 IoPoint 數據是否一致.
            for (int i = 0; i < N; i++)
            {
                Assert.AreEqual(ioPoints[i].IsOn, testData[i], "On/Off 應該相同!");
            }
        }

        //[TestMethod]
        public void Test_06_RegisterUniqueness()
        {
            var ioMem = _glueIO.IoMem;

            //// 故意用不同方式請求同一個範圍
            //foreach (bool usingDwordHost in new bool[] { true, false })
            //{
            //    var mem = ConfigFatckIoMemory(usingDwordHost, force: true);
            //    var m320 = mem["M320"];
            //    var m321 = mem["M321"];
            //    var host = usingDwordHost ? mem["DWM320"] : mem["WM320"];

            //    // 驗證枚舉結果
            //    int hostCount = mem.IterRegs().Count();
            //    Assert.AreEqual(1, hostCount, $"{m320}, {m321}, {host} 應該全部指向同一個 {host} 實體");
            //}
        }

        //[TestMethod]
        public void Test_07_MemoryEnumeration_16Bit_Host()
        {
            var ioMem = _glueIO.IoMem;

            //// 1. 建立點位 (stationID == 2)
            //var M = ioMem.GetBank("2:M80", 16);
            //var R = ioMem.GetBank("2:R100", 16);
            //var m0 = ioMem["2:M0"];
            //var m18 = ioMem["2:M18"];
            //var m80 = M[80];
            //var m95 = M[95];
            //var r100 = R[100];
            //var r115 = R[115];

            //// 2. 使用 LINQ 或 foreach 收集所有已分配的 Key
            //var allocatedKeys = ioMem.IterRegs().Select(reg => reg.Address.KeyName).ToList();

            //// 3. 驗證特定 Host 是否存在
            //// M0 映射後的 Host Key 應為 "2:WM0"
            //Assert.IsTrue(allocatedKeys.Contains("2:WM0"), "應該包含 WM0");
            //// M18 映射後的 Host Key 應為 "2:WM16"
            //Assert.IsTrue(allocatedKeys.Contains("2:WM16"), "應該包含 WM16");
            //// M80, M95 映射後的 Host Key 應為 "2:WM80"
            //Assert.IsTrue(allocatedKeys.Contains("2:WM80"), "應該包含 WM80");
            //// R100 是獨立暫存器，Key 應為 "2:R100"
            //Assert.IsTrue(allocatedKeys.Contains("2:R100"), "應該包含 R100");
            //// R105 是獨立暫存器，Key 應為 "2:R105"
            //Assert.IsTrue(allocatedKeys.Contains("2:R100"), "應該包含 R105");

            //// 4. 驗證總數量是否正確
            //// count == 1(WM0) + 1(WM16) + 1 WM80) + 2 (R1000,R1015) == 5
            //Assert.AreEqual(5, allocatedKeys.Count, "枚舉到的 Host 數量不符預期");
        }
    }
}