#region AUTHOR
/*
 * Traveller106.IO
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-24 created by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;
using System;

namespace Traveller106.IO
{
    /// <summary>
    /// 德龍機台 OMRON PLC 變量名稱 之 管理
    /// </summary>
    /// <remarks>
    /// 全字串格式為: $"{stationID}:{prefix}.{suffix}"
    /// <br/> 但是傳給 PLC 需要把 $"{stationID}:" 移除.
    /// </remarks>
    internal static class DronVariables
    {
        public static string BasePrefix => "Gvl_PhotoPC";
        public static string AxisPrefix(int id) => $"Axis[{id}]";

        public static string TrimStationID(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            else if (name.Contains(":"))
                return name.Split(':')[1].Trim();
            else
                return name;
        }
        public static string VarName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            else if (name.Contains("."))
                return TrimStationID(name);
            else
                return BaseVarName(name);
        }
        public static string BaseVarName(string name)
        {
            //return BasePrefix + "." + TrimStationID(name);
            return $"{BasePrefix}.{TrimStationID(name)}";
        }
        public static string AxisVarName(int axisId, string name)
        {
            //return AxisPrefix(axisId) + "." + TrimStationID(name);
            return $"{AxisPrefix(axisId)}.{TrimStationID(name)}";
        }

        public static string VarName(BasePointEnum e)
        {
            Parse(e, BasePrefix, out string varName, out var _, out var _);
            return varName;
        }
        public static string VarName(MiscPointEnum e)
        {
            Parse(e, BasePrefix, out string varName, out var _, out var _);
            return varName;
        }
        public static string VarName(int axisId, AxisPointEnum e)
        {
            return $"{AxisPrefix(axisId)}.{e}";
        }
        public static string VarName(CompositePointEnum e, params object[] args)
        {
            Parse(e, BasePrefix, out string varName, out var _, out var _);
            if (varName.Contains("{0}") && args.Length > 0)
                varName = string.Format(varName, args);
            return varName;
        }

        public static IoPoint OmronPoint(this IoMemory ioMem, Enum e)
        {
            if (ioMem == null)
                return null;

            if (!(e is BasePointEnum || e is MiscPointEnum))
                return null;

            Parse(e, BasePrefix, out string varName, out string desc, out bool invert);

            var ioPoint = ioMem[varName]?.SetDescription(desc)?.SetInverted(invert);
            
            return ioPoint;
        }
        public static IoPoint OmronPoint(this IoMemory ioMem, int axisId, AxisPointEnum e)
        {
            if (ioMem == null)
                return null;

            Parse(e, AxisPrefix(axisId), out string varName, out string desc, out bool invert);

            if (varName.Contains("{0}"))
                varName = string.Format(varName, axisId);

            if (desc != null && desc.Contains("{0}"))
                desc = string.Format(desc, axisId);

            var ioPoint = ioMem[varName]?.SetDescription(desc)?.SetInverted(invert);
            return ioPoint;
        }

        public static EzPointEnumAttribute Parse(Enum e, string prefix, out string varName, out string desc, out bool invert)
        {
            var attr = EzPointEnumAttribute.GetAttribute(e);
            if (attr != null)
            {
                string prefixA = null;
                string suffixA = null;

                var nameA = TrimStationID(attr.EzIoName);
                if (nameA.Contains("."))
                {
                    var strs = nameA.Split('.');
                    if (strs.Length > 0)
                        prefixA = strs[0].Trim();
                    if (strs.Length > 1)
                        suffixA = strs[1].Trim();
                }
                else
                {
                    prefixA = nameA.Trim();
                }

                if (string.IsNullOrEmpty(prefixA))
                    prefixA = prefix;
                if (string.IsNullOrEmpty(suffixA))
                    suffixA = e.ToString();

                // varName
                varName = $"{prefixA}.{suffixA}";

                // description
                desc = attr.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = JetEazy.QxNums.GetEnumDescription(e);

                // invert
                invert = !attr.NormalOpen;
                return attr;
            }
            else
            {
                // varName
                varName = $"{prefix}.{e}";

                // description
                desc = JetEazy.QxNums.GetEnumDescription(e);

                // invert
                invert = false;
                return null;
            }
        }
    }
}
