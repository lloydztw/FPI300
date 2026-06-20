#region AUTHOR
/*
 * EzGlueDispenser.IO.S3
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-22 revised by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System;
using System.Collections.Generic;
using S3PointEnum = EzGlueDispenser.IO.S3.EzPointEnum;

namespace EzGlueDispenser.IO.S3
{
    public class S3_IoTable
    {
        #region PRIVATE_DATA
        Dictionary<S3PointEnum, IoPoint> _ioTable;
        #endregion

        public byte StationID => 3;

        public void Init(IoMemory ioMem)
        {
            var ioTable = new Dictionary<S3PointEnum, IoPoint>();

            foreach(S3PointEnum id in Enum.GetValues(typeof(S3PointEnum)))
            {
                var attrib = EzPointEnumAttribute.GetAttribute(id);
                if (attrib == null)
                    continue;

                string ezIoName = $"{StationID}:{attrib.EzIoName}";
                IoPoint pt = ioMem[ezIoName];

                pt.SetDescription(attrib.Description);
                pt.SetInverted(!attrib.NormalOpen);

                ioTable.Add(id, pt);
            }

            _ioTable = ioTable;
        }

        public bool IsAutoScanPoint(S3PointEnum pid)
        {
            var attrib = EzPointEnumAttribute.GetAttribute(pid);
            return attrib != null && attrib.AutoScan;
        }

        public IoPoint this[S3PointEnum pid]
        {
            get
            {
                if(_ioTable.TryGetValue(pid, out var point))
                    return point;
                return null;
                    
            }
        }
    }
}