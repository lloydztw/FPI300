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
using EzPlc.Hcfa;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTestH4_Hcfa_Device_Factory : UnitTest_Base
    {
        [TestMethod]
        public void Test_01_OpenDevice_SameKey_ShouldReturnSameInstance()
        {
            // Arrange
            var settings = new EzTcpIpSettings { Port= HCFA_IP_PORT, IsSim= HCFA_IS_SIM };

            // Act
            var device1 = EzHcfaFactory.OpenDevice(settings);
            var device2 = EzHcfaFactory.OpenDevice(settings);
            var mem1 = EzHcfaFactory.GetIoMemory(device1);
            var mem2 = EzHcfaFactory.GetIoMemory(device2);
            _TRACE($"device1 = {device1.KeyName}");

            // Assert
            Assert.IsNotNull(device1);
            Assert.AreSame(device1, device2, "相同 Key 的設備應該回傳同一個實體");

            Assert.IsNotNull(mem1, "IoMemory 應該被成功建立");
            Assert.AreSame(mem1, mem2, "針對同一個設備呼叫兩次 GetIoMemory 應該回傳同一個實體");
        }

        [TestMethod]
        public void Test_02_OpenDevice_DifferentKey_ShouldReturnDifferentInstances()
        {
            // Arrange
            var settings1 = new EzTcpIpSettings { Port = 8080, IsSim = true };
            var settings2 = new EzTcpIpSettings { Port = 8081, IsSim = true };

            // Act
            var device1 = EzHcfaFactory.OpenDevice(settings1);
            var device2 = EzHcfaFactory.OpenDevice(settings2);
            _TRACE($"device1 = {device1}");
            _TRACE($"device2 = {device2}");

            var m1 = EzHcfaFactory.GetIoMemory(device1);
            var m2 = EzHcfaFactory.GetIoMemory(device2);

            // Assert
            Assert.AreNotSame(device1, device2, "不同 COM Port 應建立不同的設備實體");
            Assert.AreNotSame(m1, m2, "不同設備應該擁有各自獨立的 IoMemory 實體");
        }

        [TestMethod]
        public void Test_03_OpenDevice_ConcurrentAccess_ShouldOnlyCreateOneInstance()
        {
            // Arrange
            var settings = new EzTcpIpSettings { Port = HCFA_IP_PORT, IsSim = HCFA_IS_SIM };
            int taskCount = 15;
            Task<IoDevice>[] tasks = new Task<IoDevice>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => EzHcfaFactory.OpenDevice(settings));
            }
            Task.WaitAll(tasks);

            // Assert
            var firstInstance = tasks[0].Result;
            foreach (var t in tasks)
            {
                Assert.AreSame(firstInstance, t.Result, "多執行緒併發下，Lazy<T> 應保證實例唯一性");
            }
        }

        [TestMethod]
        public void Test_04_DeviceUnregister_ShouldAlsoClearMemoryFromFactory()
        {
            // Arrange
            var settings = new EzTcpIpSettings { Port = HCFA_IP_PORT, IsSim = HCFA_IS_SIM };
            var device = EzHcfaFactory.OpenDevice(settings);
            var memoryBefore = EzHcfaFactory.GetIoMemory(device);

            // Act
            // 模擬設備被釋放，觸發 Hook 裡面的 Unregister
            // 這會同時執行 _factory.Unregister(key) 與 _factoryMem.Unregister(key)
            ((IDisposable)device).Dispose();

            // 再次獲取設備與記憶體
            var newDevice = EzHcfaFactory.OpenDevice(settings);
            var memoryAfter = EzHcfaFactory.GetIoMemory(newDevice);

            // Assert
            Assert.AreNotSame(device, newDevice, "舊設備應已註銷，應建立新設備");
            Assert.AreNotSame(memoryBefore, memoryAfter, "舊設備註銷時，相關聯的 IoMemory 也應從工廠中被移除");
        }

        [TestMethod]
        public void Test_05_DisposeAll_ShouldClearFactoryCache()
        {
            // Arrange
            var settings = new EzTcpIpSettings { Port = HCFA_IP_PORT, IsSim = HCFA_IS_SIM };
            var deviceBefore = EzHcfaFactory.OpenDevice(settings);

            // Act
            EzHcfaFactory.DisposeAll();
            var deviceAfter = EzHcfaFactory.OpenDevice(settings);

            // Assert
            Assert.AreNotSame(deviceBefore, deviceAfter, "DisposeAll 後，舊的實體應被清除，再次獲取應為新實體");

            EzHcfaFactory.DisposeAll();
        }
    }
}
