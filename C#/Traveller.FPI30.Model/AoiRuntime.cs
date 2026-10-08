#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using System;
using LaserAlignDX.Mvc.Model;

namespace LaserAlignDX.AoiModel
{
    /// <summary>
    /// Internal bridge for legacy singleton AOI components. 
    /// Only the model factory binds it.
    /// </summary>
    internal static class AoiRuntime
    {
        #region PRIVATE_DATA
        static readonly object _gate = new object();
        static ITravellerSysModel _sysModel;
        #endregion

        internal static object SyncRoot => _gate;
        internal static bool IsBound 
        { 
            get { lock (_gate) return _sysModel != null; } 
        }

        internal static ITravellerSysModel SystemModel
        {
            get
            {
                lock (_gate)
                    return _sysModel ?? throw new InvalidOperationException("Create a model through TravellerModelFactory before using AOI.");
            }
        }
        internal static IDisposable Bind(ITravellerSysModel system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            lock (_gate)
            {
                if (_sysModel != null) throw new InvalidOperationException("A model session is already active.");
                _sysModel = system;
                return new Binding(system);
            }
        }

        #region PRIVATE_TYPES
        sealed class Binding : IDisposable
        {
            #region PRIVATE_DATA
            ITravellerSysModel _owner;
            #endregion
            internal Binding(ITravellerSysModel owner) { _owner = owner; }
            public void Dispose()
            {
                lock (_gate)
                {
                    if (_owner == null) return;
                    if (ReferenceEquals(_sysModel, _owner)) _sysModel = null;
                    _owner = null;
                }
            }
        }
        #endregion
    }
}
