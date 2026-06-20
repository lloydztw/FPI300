using System.Collections.Generic;

namespace EzCamera.Driver.Dvp.Huang
{
    public class CCDSerial
    {


        public CCDSerial()
        {

            List<string> sectionsList = INI.ReadSections(Universal.CCDINI);

            foreach (string section in sectionsList)
            {

                if (section.IndexOf("CCD") != -1)
                {

                }
            }

        }

    }
    /// <summary>
    /// 相机
    /// </summary>
    public class CCD
    {
        public CCDType ccdType { get; }
        public int ccdCount { get; }
    }

}
