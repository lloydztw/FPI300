
//#define FX3U
//#define FATEK
//#define CIP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JetEazy.ControlSpace.PLCSpace
{

    //public interface IPLCInterface
    //{
    //    IODataClass IOData { get; }
    //    void SetData(string data, string ioname);
    //    void SetIO(bool IsOn, string ioname);
    //}

    //public class VsCommPLC : Modbus_From_HSL
    //{

    //}

    //public class VsCommPLC : ModbusTcpFromNNModbus4
    //{

    //}

#if FATEK
    public class VsCommPLC : FatekPLCClass
    {

    }
#endif

#if FX3U
    public class VsCommPLC : MITFX3U
    {

    }
#endif

#if CIP
    public class VsCommPLC : CipCompoletClass
    {

    }
#endif
    public class VsLight : CstLightClass
    {

    }

}
