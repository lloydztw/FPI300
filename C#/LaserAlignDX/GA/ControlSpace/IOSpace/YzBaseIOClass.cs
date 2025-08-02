
using JetEazy.ControlSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using VsCommon.ControlSpace.IOSpace;

namespace VsCommon.ControlSpace.IOSpace
{
    public enum RBaseAddressEnum : int
    {
        COUNT = 6,

        ADR_ISSTART = 0,
        ADR_RED = 1,
        ADR_YELLOW = 2,
        ADR_GREEN = 3,
        ADR_BLUE = 4,
        ADR_WHITE = 5,
    }
    public class YzBaseIOClass : GeoIOClass
    {

        public YzBaseIOClass()
        {   

        }
        public void Initial(string path, JetEazy.ControlSpace.PLCSpace.VsCommPLC[] plc)
        {
            ADDRESSARRAY = new AddressClass[(int)RBaseAddressEnum.COUNT];

            PLC = plc;

            INIFILE = path + "\\IO.INI";

            LoadData();

        }
        public override void LoadData()
        {
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_ISSTART] = new AddressClass(ReadINIValue("Status Address", RBaseAddressEnum.ADR_ISSTART.ToString(), "", INIFILE));
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_RED] = new AddressClass(ReadINIValue("Operation Address", RBaseAddressEnum.ADR_RED.ToString(), "", INIFILE));
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_YELLOW] = new AddressClass(ReadINIValue("Operation Address", RBaseAddressEnum.ADR_YELLOW.ToString(), "", INIFILE));
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_GREEN] = new AddressClass(ReadINIValue("Operation Address", RBaseAddressEnum.ADR_GREEN.ToString(), "", INIFILE));
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_BLUE] = new AddressClass(ReadINIValue("Operation Address", RBaseAddressEnum.ADR_BLUE.ToString(), "", INIFILE));
            ADDRESSARRAY[(int)RBaseAddressEnum.ADR_WHITE] = new AddressClass(ReadINIValue("Operation Address", RBaseAddressEnum.ADR_WHITE.ToString(), "", INIFILE));
        }

        public override void SaveData()
        {
            
        }

        public bool Red
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_RED];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_RED];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool Yellow
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_YELLOW];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_YELLOW];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool Green
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_GREEN];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_GREEN];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool IsStart
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_ISSTART];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
        }
        public bool Blue
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_BLUE];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_BLUE];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
        public bool White
        {
            get
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_WHITE];
                return PLC[address.SiteNo].IOData.GetBit(address.Address0);
            }
            set
            {
                AddressClass address = ADDRESSARRAY[(int)RBaseAddressEnum.ADR_WHITE];
                PLC[address.SiteNo].SetIO(value, address.Address0);
            }
        }
    }
}
