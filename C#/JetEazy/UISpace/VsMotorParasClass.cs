using JetEazy;
using JetEazy.ControlSpace.MotionSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace ComtactAnglePlus.FromCommon
{
    public class VsMotorParasClass
    {
        PLCMotionClass m_MotionUse = null;

        public VsMotorParasClass()
        {

        }
        public VsMotorParasClass(PLCMotionClass ePlcMotion)
        {
            m_MotionUse = ePlcMotion;
        }

        [Category("Motor Speed Control"), Description("定位速度高速")]
        [DisplayName("定位速度高速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        [Browsable(true)]
        public double GOSPEED
        {
            get { return m_MotionUse.GOSPEED; }
            set
            {
                m_MotionUse.GOSPEED = value;
            }
        }
        [Category("Motor Speed Control"), Description("定位速度低速")]
        [DisplayName("定位速度低速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        [Browsable(true)]
        public double GOSLOWSPEED
        {
            get { return m_MotionUse.GOSLOWSPEED; }
            set
            {
                m_MotionUse.GOSLOWSPEED = value;
            }
        }
        [Category("Motor Speed Control"), Description("手动速度高速")]
        [DisplayName("手动速度高速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        [Browsable(true)]
        public double MANUALSPEED
        {
            get { return m_MotionUse.MANUALSPEED; }
            set
            {
                m_MotionUse.MANUALSPEED = value;
            }
        }
        [Category("Motor Speed Control"), Description("手动速度低速")]
        [DisplayName("手动速度低速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        [Browsable(true)]
        public double MANUALSLOWSPEED
        {
            get { return m_MotionUse.MANUALSLOWSPEED; }
            set
            {
                m_MotionUse.MANUALSLOWSPEED = value;
            }
        }
        [Category("Motor Speed Control"), Description("回原速度高速")]
        [DisplayName("回原速度高速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        //[Browsable(false)]
        public double HOMEHIGHSPEED
        {
            get { return m_MotionUse.HOMEHIGHSPEED; }
            set
            {
                m_MotionUse.HOMEHIGHSPEED = value;
            }
        }
        [Category("Motor Speed Control"), Description("回原速度低速")]
        [DisplayName("回原速度低速")]
        [TypeConverter(typeof(NumericUpDownTypeConverter))]
        [Editor(typeof(NumericUpDownTypeEditor), typeof(UITypeEditor)), MinMax(0, 1000000, 0.5f, 3)]
        //[Browsable(false)]
        public double HOMESLOWSPEED
        {
            get { return m_MotionUse.HOMESLOWSPEED; }
            set
            {
                m_MotionUse.HOMESLOWSPEED = value;
            }
        }

        [Category("Motor Position Control"), Description("POS1(待命位置)")]
        [DisplayName("POS1(待命位置)")]
        public float READYPOSITION
        {
            get { return m_MotionUse.READYPOSITION; }
            set
            {
                m_MotionUse.READYPOSITION = value;
            }
        }

        [Category("Motor Position Control"), Description("POS2(测试位置)")]
        [DisplayName("POS2(测试位置)")]
        public float TESTPOSITION
        {
            get { return m_MotionUse.TESTPOSITION; }
            set
            {
                m_MotionUse.TESTPOSITION = value;
            }
        }


    }
}
