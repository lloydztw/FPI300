#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;

namespace EzAoiEmptyTrayInspector.Model
{
    public class MatchStateEventArgs : EventArgs
    {
        public SideID ID;
        public string State;
        public MatchStateEventArgs(SideID id, string state)
        {
            ID = id;
            State = state != null ? state.ToString() : "";
        }
        public override string ToString()
        {
            return ID != SideID.All ? State : $"[{ID}] {State}";
        }
    }


    public class MatchResultEventArgs : EventArgs
    {
        public SideID ID
        {
            get; private set;
        }
        public MatchResult Result
        {
            get; internal set;
        }
        public bool IsResetting()
        {
            return Result == null;
        }
        public MatchResultEventArgs(SideID id, MatchResult result)
        {
            ID = id;
            Result = result;
        }
    }


    public class AoiResultEventArgs : EventArgs
    {
        public EzEmptyTrayResult Result
        {
            get; private set;
        }
        public AoiResultEventArgs(EzEmptyTrayResult result)
        {
            Result = result;
        }
    }
}
