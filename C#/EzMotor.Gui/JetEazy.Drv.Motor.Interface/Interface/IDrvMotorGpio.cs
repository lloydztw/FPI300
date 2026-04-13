#region AUTHOR
/*
 * 
 * Copyright (c) 2023 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2023-09-11 LeTian Chang, Revision.
 *      2008-12-01 LeTian Chang, Creation.
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using System;


namespace JetEazy.Drivers.Motor
{
    public interface IDrvMotorGpio
    {
        // TEMPORAL
        void SetSlaveID(int slaveID);
        void WriteIO(int portNo, ushort data);
        ushort ReadIO(int portNo);
    }


    public interface IDrvFastDIO : IDisposable
    {
        int TotalOutputChannels { get; }
        int TotalInputChannels { get; }
        void SetDO(int iChannel, bool bValue);
        bool GetDO(int iChannel);
        bool GetDI(int iChannel);
    }
}
