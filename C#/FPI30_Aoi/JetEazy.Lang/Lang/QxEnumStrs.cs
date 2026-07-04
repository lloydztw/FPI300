using System;
using System.Reflection;
using System.ComponentModel;

namespace JetEazy
{
    public class QxNums
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static bool TryGetIndex(Type enumT, string key, out int idx)
        {
            string[] names = Enum.GetNames(enumT);
            
            key = key.Trim();

            for (int i = 0, len = names.Length; i < len; i++)
            {
                if (string.Compare(names[i], key, true) == 0)
                {
                    int[] values = (int[])Enum.GetValues(enumT);
                    idx = (int)values[i];
                    return true;
                }
            }
            idx = -1;
            return false;
        }
    }
}
