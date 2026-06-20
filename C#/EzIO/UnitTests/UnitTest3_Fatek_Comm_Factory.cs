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
using EzPlc.Fatek.Comm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EzIO.UnitTests
{
    [TestClass]
    public class UnitTest3_Fatek_Comm_Factory : UnitTest_Base
    {
        [TestInitialize]
        public void Setup()
        {
            // 每次測試前清空工廠，確保測試隔離性
            FatekCommFactory.DisposeAll();
        }

        [TestMethod]
        public void Test_01_OpenPlcComm_SameSettings_ShouldReturnSameInstance()
        {
            // Arrange
            var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };

            // Act
            var comm1 = FatekCommFactory.OpenPlcComm(settings);
            var comm2 = FatekCommFactory.OpenPlcComm(settings);

            _TRACE($"{comm1}");

            // Assert
            Assert.IsNotNull(comm1);
            Assert.AreSame(comm1, comm2, "相同 COM Port 的請求應回傳同一個快取實例");
        }

        [TestMethod]
        public void Test_02_OpenPlcApi_ShouldCreateNewApiWrapperWithSameComm()
        {
            // Arrange
            var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };

            // Act
            var api1 = FatekCommFactory.OpenPlcApi(settings);
            var api2 = FatekCommFactory.OpenPlcApi(settings);

            _TRACE($"{api1}");

            // Assert
            Assert.IsNotNull(api1);
            Assert.AreNotSame(api1, api2, "OpenPlcApi 每次呼叫應回傳新的 Wrapper 物件");

            _TRACE($"{api1} : isRunning = {api1.IsRunning()}");
            _TRACE(api1.Echo("Hello, Fatek!"));
        }

        [TestMethod]
        public void Test_03_OpenPlcComm_ConcurrentAccess_ShouldBeThreadSafe()
        {
            // Arrange
            var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };
            int taskCount = 10;
            var tasks = new Task<IxFatekComm>[taskCount];

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() => FatekCommFactory.OpenPlcComm(settings));
            }
            Task.WaitAll(tasks);

            // Assert
            var firstResult = tasks[0].Result;
            foreach (var task in tasks)
            {
                Assert.AreSame(firstResult, task.Result, "高併發下 QxObjFactory 應保證初始化唯一性");
            }
        }

        [TestMethod]
        public void Test_04_DisposeOne_And_NewOne()
        {
            var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };
            for (int trial = 0; trial < 3; trial++)
            {
                var comm = FatekCommFactory.OpenPlcComm(settings);
                var api = FatekCommFactory.OpenPlcApi(settings);
                _TRACE(api.Echo($"Hello, Fatek (Test={trial + 1})."));
                comm.Dispose();
            }
        }

        [TestMethod]
        public void Test_05_DisposeAll_ShouldClearInternalFactory()
        {
            // Arrange
            var settings = new EzUartSettings { ComPort = FATEK_COM_PORT, IsSim = FATEK_IS_SIM };
            var commBefore = FatekCommFactory.OpenPlcComm(settings);

            // Act
            FatekCommFactory.DisposeAll();
            var commAfter = FatekCommFactory.OpenPlcComm(settings);

            // Assert
            Assert.AreNotSame(commBefore, commAfter, "DisposeAll 後應釋放資源並清空快取");

            FatekCommFactory.DisposeAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Test_06_OpenPlcComm_ConnectionFailure_ShouldThrowExceptionAfterRetries()
        {
            // Arrange
            // 模擬一個會導致連線失敗的設定（例如無效的 Port）
            var settings = new EzUartSettings { ComPort = 999, IsSim = false };

            // Act
            FatekCommFactory.OpenPlcComm(settings);

            // Assert: 由 ExpectedException 驗證

        }
    }
}
