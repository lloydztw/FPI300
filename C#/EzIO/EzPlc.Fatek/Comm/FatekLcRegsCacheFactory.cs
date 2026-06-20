#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-03-21 重整 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;
using System.Collections.Generic;
using ComPortSettings = EzComm.EzUartSettings;
using FatekLcRegsCache = EzPlc.Fatek.Comm.IxFatekComm;
using IFatekLcRegsCache = EzPlc.Fatek.Comm.IxFatekComm;


namespace Fierro.Presso.Drivers.LoadCell.Support
{
    internal partial class FatekLcRegsCacheFactory
    {
        #region PRIVATE_DATA
        static Dictionary<int, IFatekLcRegsCache> _cachesInUse = new Dictionary<int, IFatekLcRegsCache>();
        static List<int> _comPorts = new List<int>();
        static object _sync => _cachesInUse;
        #endregion

        public static IFatekLcRegsCache Instance(int comPort, int channel, ComPortSettings settings)
        {
            lock (_sync)
            {
                if (_cachesInUse.TryGetValue(comPort, out IFatekLcRegsCache cache))
                {
                    cache?.AddRef();
                    cache?.RegisterChannel(channel);
                    return cache;
                }
                else
                {
                    if (_cachesInUse.Count == 0)
                        EzKeyHook.Init();

                    var plc = OpenPLC(comPort, settings);
                    int moduleID = GetModuleID(comPort);
                    var newCache = new FatekLcRegsCache(plc, comPort, moduleID, settings.IsSim);

                    RegisterUsage(comPort, newCache);
                    newCache.OnFinalDisposed += (sender, e) => UnRegisterUsage(sender as IFatekLcRegsCache);
                    newCache.RegisterChannel(channel);
                    return newCache;
                }
            }
        }

        #region PRIVATE_FUNCTIONS
        static int GetModuleID(int comPort)
        {
            lock (_sync)
            {
                int idx = _comPorts.IndexOf(comPort);
                if (idx >= 0)
                    return idx;
                return _comPorts.Count;
            }
        }
        static void RegisterUsage(int comPort, IFatekLcRegsCache cache)
        {
            lock (_sync)
            {
                if (cache != null)
                {
                    if (!_cachesInUse.ContainsKey(comPort))
                    {
                        _cachesInUse.Add(comPort, cache);
                        _comPorts.Add(comPort);
                        cache.OnFinalDisposed += Cache_OnFinalDisposed;
                    }
                }
            }
        }
        static void UnRegisterUsage(IFatekLcRegsCache cache)
        {
            lock (_sync)
            {
                if (cache != null)
                {
                    int comPort = cache.ComPort;
                    if (_cachesInUse.ContainsKey(comPort))
                    {
                        cache.OnFinalDisposed -= Cache_OnFinalDisposed;

                        _cachesInUse.Remove(comPort);
                        _comPorts.Remove(comPort);

                        if (_cachesInUse.Count == 0)
                            EzKeyHook.Dispose();
                    }
                }
            }
        }
        private static void Cache_OnFinalDisposed(object sender, EventArgs e)
        {
            UnRegisterUsage(sender as IFatekLcRegsCache);
        }
        #endregion

        static IxFatekComm OpenPLC(int comPort, ComPortSettings settings)
        {
            try
            {
                if (!settings.IsSim)
                    return JetEazy.Drivers.PLC.FATEK.CFatekPLC.OpenPLC(comPort, settings.BaudRate, settings.QuickTimeout, settings.RetryCount);
                else
                    return JetEazy.Drivers.PLC.FATEK.Sim.CFatekPLC.OpenPLC(comPort, settings.BaudRate, settings.QuickTimeout, settings.RetryCount);
            }
            catch
            {
                throw new Exception($"無法開啟 PLC @ comPort = {comPort}");
            }
        }
    }

    partial class FatekLcRegsCacheFactory
    {
        public static void GetInfo(IFatekLcRegsCache cache, out int comPort, out int moduleID)
        {
            if (cache != null)
            {
                comPort = cache.ComPort;
                moduleID = GetModuleID(comPort);
            }
            else
            {
                comPort = -1;
                moduleID = -1;
            }
        }

        public static void ResetRegsD()
        {
            lock(_sync)
            {
                foreach (var cache in _cachesInUse.Values)
                    cache?.ResetRegsD();
            }
        }
    }
}
