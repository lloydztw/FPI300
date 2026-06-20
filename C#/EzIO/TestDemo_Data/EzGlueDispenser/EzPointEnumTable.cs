using EzIO.Mem;
using System;
using System.Collections.Generic;

namespace Ez.IOCtrl.Common
{
    public class EzPointEnumTable<KEY> where KEY : Enum
    {
        #region CLASS_PointItem
        class PointItem
        {
            public IoPoint ioPoint;
            public bool autoScannable;
            public PointItem(IoPoint ioPoint, bool autoScannable)
            {
                this.ioPoint = ioPoint;
                this.autoScannable = autoScannable;
            }
        }
        #endregion

        #region PRIVATE_DATA
        Dictionary<Enum, PointItem> m_ioTable = new Dictionary<Enum, PointItem>();
        //EzIoPointSectionGroup m_ioGrp = null;
        Func<string, IAddress> m_addressCreate = null;
        #endregion

        public EzPointEnumTable(Func<string, IAddress> addressCreateFunc)
        {
            m_addressCreate = addressCreateFunc;
        }

        public int TotalPoints
        {
            get { return m_ioTable != null ? m_ioTable.Count : 0; }
        }
        public IoPoint this[Enum key]
        {
            get
            {
                if (m_ioTable.TryGetValue(key, out PointItem item))
                    return item.ioPoint;
                else
                    return null;
            }
        }
        public IEnumerable<KEY> EnumAllKeys()
        {
            foreach (var kp in m_ioTable)
            {
                yield return (KEY)kp.Key;
            }
        }
        public IEnumerable<IoPoint> EnumAllPoints()
        {
            foreach (var kv in m_ioTable)
            {
                yield return kv.Value.ioPoint;
            }
        }
        public IEnumerable<IoPoint> EnumSortedPoints()
        {
            var list = ToList(true);
            foreach (var pt in list)
                yield return pt;
        }
        public IEnumerable<IoPoint> EnumAutoScanPoints()
        {
            foreach (var kv in m_ioTable)
            {
                if (kv.Value.autoScannable)
                    yield return kv.Value.ioPoint;
            }
        }
        public IEnumerable<KeyValuePair<KEY, IoPoint>> EnumKeyValues()
        {
            foreach (var kp in m_ioTable)
            {
                yield return
                    new KeyValuePair<KEY, IoPoint>((KEY)kp.Key, kp.Value.ioPoint);
            }
        }
        IEnumerable<KeyValuePair<Enum, IoPoint>> IoPointsTable.EnumKeyValues()
        {
            foreach (var kp in m_ioTable)
            {
                yield return
                    new KeyValuePair<Enum, IoPoint>(kp.Key, kp.Value.ioPoint);
            }
        }

        public IoAddrSectionGroup<IoPoint> GetGroup(bool rebuild = false)
        {
            if (rebuild || m_ioGrp == null)
            {
                m_ioGrp = new EzIoPointSectionGroup();
                foreach (var kv in m_ioTable)
                {
                    var pt = kv.Value.ioPoint;
                    m_ioGrp.Append(pt.Address, pt);
                }
            }
            return m_ioGrp;
        }
        public IoAddrSectionGroup<IoPoint> GetAutoScanGroup(bool rebuild = false)
        {
            var ioGrp = new EzAddrSectionGroup<IoPoint>();
            //>> if (rebuild || m_ioGrp == null)
            {
                foreach (var kv in m_ioTable)
                {
                    if (kv.Value.autoScannable)
                    {
                        var pt = kv.Value.ioPoint;
                        ioGrp.Append(pt.Address, pt);
                    }
                }
            }
            return ioGrp;
        }

        #region IEnumerator_FUCTIONS
        private IList<IoPoint> ToList(bool sort = false)
        {
            var list = new List<IoPoint>();
            foreach (var kv in m_ioTable)
                list.Add(kv.Value.ioPoint);
            if (sort)
                list.Sort(EzIoGlobal.CompareOrder);
            return list;
        }
#if(OPT_RESERVED)
        public IEnumerator<IoPoint> GetEnumerator()
        {
            var list = ToList(true);
            return list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
#endif
        #endregion


        public virtual void LoadIni(string iniFileName, string sectionName = null)
        {
            if (string.IsNullOrEmpty(sectionName))
                sectionName = GetType().Name;

            m_ioTable.Clear();  // = new Dictionary<EnumT, PointItem>();
            m_ioGrp = null;

            var srcEnumValues = Enum.GetValues(typeof(KEY));
            foreach (KEY key in srcEnumValues)
            {
                var enumAttr = EzPointEnumAttribute.GetAttribute(key);
                if (enumAttr == null)
                    continue;

                string description = enumAttr.Description;
                string ezIoName = enumAttr.EzIoName;
                bool normalOpen = enumAttr.NormalOpen;
                bool autoScan = enumAttr.AutoScan;


                IoPoint pt = _loadPoint(iniFileName, sectionName, key);
                if (pt != null)
                {
                    if (!m_ioTable.ContainsKey(key))
                    {
                        m_ioTable.Add(key, new PointItem(pt, autoScan));
                        //> m_points.Add(pt);
                    }
                }
            }
            //> m_points.Sort(EzIoGlobal.CompareOrder);
        }
        public virtual void SaveIniFile(string iniFileName, string sectionName = null)
        {
            // RESERVED
            var keys = new List<Enum>();
            foreach (var key in m_ioTable.Keys)
                keys.Add(key);
            keys.Sort();
            foreach(var key in keys)
            {
                var pt = this[key];
                string str = $"{key} = {pt.Address}";
                System.Diagnostics.Trace.WriteLine(str);
            }
        }


        #region PRIVATE_FUNCTIONS
        IoPoint _loadPoint(string iniFileName, string sectionName, KEY eKey)
        {
            var enumAttr = EzPointEnumAttribute.GetAttribute(eKey);
            if (enumAttr == null)
                return null;

            string description = enumAttr.Description;
            string ezIoName = enumAttr.EzIoName;
            bool normalOpen = enumAttr.NormalOpen;

            IoAddress addr = null;

            #region TRY_INI_FILE
            string ezIoName2 = loadEzIoName(iniFileName, sectionName, eKey);
            if (!string.IsNullOrEmpty(ezIoName2))
                addr = createAddress(description, ezIoName2);
            #endregion

            if (addr == null)
                addr = createAddress(description, ezIoName);

            if (addr != null)
            {
                IoPoint pt = _createPoint(addr, normalOpen, null);
                return pt;
            }
            return null;
        }
        IoPoint _createPoint(IoAddress addr, bool normalOpen, IList<IoPoint> collector = null)
        {
            if (addr == null)
                return null;
            if (addr.Bits == 1)
                return new EzIoPointBit(addr, normalOpen, collector);
            else
                return new EzIoPointReg(addr, collector);
        }
        #endregion

        #region VIRTUAL_FUNCTIONS
        private string loadEzIoName(string iniFileName, string sectionName, KEY eKey)
        {
            if (!string.IsNullOrEmpty(iniFileName))
            {
                getIniSectionKeyNames(eKey, out string sect2, out string key2);
                if (!string.IsNullOrEmpty(sect2))
                    sectionName = sect2;
                string keyName = string.IsNullOrEmpty(key2) ? eKey.ToString() : key2;

                string valueStr = Utils.Win32Ini.Read(sectionName, keyName, "", iniFileName);
                //> JetEazy.Win32.Win32Ini.Load(ref valueStr, iniFileName, sectionName, keyName);

                if (!string.IsNullOrEmpty(valueStr))
                    return valueStr.Trim();
            }
            return null;
        }
        protected virtual void getIniSectionKeyNames(KEY key, out string sectionName, out string keyName)
        {
            sectionName = null;
            keyName = key.ToString();
        }
        protected virtual IoAddress createAddress(string description, string ezIoName)
        {
            var addr = m_addressCreate != null ?
                m_addressCreate(ezIoName) :
                EzAddress.FromString(ezIoName);

            if (addr != null)
                addr.Name = description;

            return addr;
        }
        protected bool appendPoint(KEY eKey, IoAddress addr, bool normalOpen = true, bool autoScan = false)
        {
            if (addr == null)
                return false;
            var pt = _createPoint(addr, normalOpen);
            if (pt == null)
                return false;
            if (m_ioTable.ContainsKey(eKey))
                return false;

            m_ioTable.Add(eKey, new PointItem(pt, autoScan));
            m_ioGrp = null;

            return true;
        }
        protected bool isAutoScannable(Enum eKey)
        {
            if (m_ioTable.TryGetValue(eKey, out var item))
                return item.autoScannable;
            return false;
        }
        #endregion


        /// <summary>
        /// 取出子集 <br/>
        /// (trial)
        /// </summary>
        public EzPointEnumTable<KEY> SliceTable(KEY[] keys)
        {
            var ioDict = new Dictionary<Enum, PointItem>();
            foreach (var key in keys)
            {
                m_ioTable.TryGetValue(key, out PointItem item);

                if (ioDict.ContainsKey(key))
                {
                    EzIoGlobal.LOG.Error($"重複 key {key}");
                    continue;
                }

                ioDict.Add(key, item);
            }
            var newTable = new EzPointEnumTable<KEY>(m_addressCreate);
            newTable.m_ioTable = ioDict;
            return newTable;
        }
        /// <summary>
        /// 取出子集 <br/>
        /// (trial)
        /// </summary>
        IoPointsTable IoPointsTable.SliceTable(Enum[] keys)
        {
            var ioDict = new Dictionary<Enum, PointItem>();
            foreach (var key in keys)
            {
                m_ioTable.TryGetValue(key, out PointItem item);

                if (ioDict.ContainsKey(key))
                {
                    EzIoGlobal.LOG.Error($"重複 key {key}");
                    continue;
                }

                ioDict.Add(key, item);
            }
            var newTable = new EzPointEnumTable<KEY>(m_addressCreate);
            newTable.m_ioTable = ioDict;
            return newTable;
        }
    }
}
