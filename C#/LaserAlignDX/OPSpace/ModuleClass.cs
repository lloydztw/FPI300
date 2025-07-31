using Eazy_Project_III;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traveller106.ControlSpace.MachineSpace;
using VsCommon.ControlSpace.MachineSpace;

namespace TravellerMINIX6.OPSpace
{
    //public class ModuleBaseClass
    //{
    //    public MainLSMachineClass MACHINE = null;
    //    public virtual void SetMachine(MainLSMachineClass eMachine)
    //    {
    //        MACHINE = eMachine;
    //    }

    //    public virtual string Name
    //    {
    //        get { return GetType().Name; }
    //    }
    //}
    public class ModuleClass
    {

        private int m_bit_index = -1;
        private string m_name = string.Empty;
        private TrackSingleModule m_module = TrackSingleModule.NONE;
        //private GeoMachineClass machine = null;
        private MiniX6MachineClass MACHINE = null;
        private string _format = "0000";

        public string Name
        {
            get { return m_name; }
        }
        public int BitIndex
        { get { return m_bit_index; } }
        public TrackSingleModule Module
        { get { return m_module; } }

        public ModuleClass()
        {

        }

        public ModuleClass(TrackSingleModule module, MiniX6MachineClass eMachine, int bit_index)
        {
            m_module = module;
            m_bit_index = bit_index;
            MACHINE = eMachine;
        }
        public ModuleClass(string eName, MiniX6MachineClass eMachine, int bit_index)
        {
            m_name = eName;
            m_bit_index = bit_index;
            MACHINE = eMachine;
        }
        public bool Start
        {
            get { return getBit(m_bit_index); }
            set { setBit(m_bit_index, value); }
        }
        public bool Running
        {
            get { return getBit(m_bit_index + 1); }
            //set { _setQB(m_bit_index+1, value); }
        }
        public bool Complete
        {
            get { return getBit(m_bit_index + 2) && !Running; }
            //set { _setQB(m_bit_index+2, value); }
        }
        //public bool ForceStop
        //{
        //    get { return getBit(m_bit_index + 3); }
        //    set { setBit(m_bit_index + 3, value); }
        //}
        public int bitStart
        { get { return m_bit_index; } }
        public int bitRunning
        { get { return m_bit_index + 1; } }
        public int bitComplete
        { get { return m_bit_index + 2; } }
        //public int bitForceStop
        //{ get { return m_bit_index + 3; } }

        bool getBit(int index)
        {
            if (index < 0)
                return false;
            if (MACHINE == null)
                return false;

            string addr = "0:M" + index.ToString(_format);
            return MACHINE.PLCIO.GetBit(addr);
        }
        void setBit(int index, bool ison)
        {
            if (index < 0)
                return;
            if (MACHINE == null)
                return;
            string addr = "0:M" + index.ToString(_format);
            MACHINE.PLCIO.SetBit(addr, ison);
        }
    }
}
