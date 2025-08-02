using Eazy_Project_III;
using JetEazy.BasicSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Traveller106.UISpace
{
    public partial class LSDirUI : UserControl
    {
        const int COUNT = 10;
        Label[] lbl = new Label[COUNT];
        private int m_dirIndex = 0;
        private int m_RunningIndex = 0;


        public LSDirUI()
        {
            InitializeComponent();
            InitUI();
        }

        void InitUI()
        {
            int i = 0;
            while (i < COUNT)
            {
                lbl[i] = new Label();

                lbl[i].BackColor = System.Drawing.Color.Lime;
                lbl[i].BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                lbl[i].Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
                lbl[i].Location = new System.Drawing.Point(0, i * 20);
                lbl[i].Name = "lbl" + i.ToString();
                lbl[i].Size = new System.Drawing.Size(84, 20);
                lbl[i].Text = "$$$";
                lbl[i].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.Controls.Add(lbl[i]);

                i++;
            }
        }

        public int DirIndex
        {
            get { return m_dirIndex; }
            set {
                m_dirIndex = value;

                switch (m_dirIndex)
                {
                    case 0:
                    case 2:
                        m_RunProcess1.Start();
                        break;
                    case 1:
                        m_RunProcess2.Start();
                        break;

                }

                    }
        }
        public void Stop()
        {
            m_RunProcess1.Stop();
            m_RunProcess2.Stop();
            int i = 0;
            while (i < COUNT)
            {
                lbl[i].BackColor = Control.DefaultBackColor;
                i++;
            }
        }
        public void Tick()
        {
            t1Tick();
            t2Tick();
        }
        ProcessClass m_RunProcess1 = new ProcessClass(); 
        void t1Tick()
        {
            ProcessClass Process = m_RunProcess1;

            if (!Process.IsOn)
                return;

            switch (Process.ID)
            {
                case 5:
                    m_RunProcess2.Stop();
                    int i = 0;
                    while (i < COUNT)
                    {
                        lbl[i].BackColor = Control.DefaultBackColor;
                        i++;
                    }
                    Process.NextDuriation = 500;
                    Process.ID = 10;

                    break;
                case 10:
                    if (Process.IsTimeup)
                    {
                        lbl[m_RunningIndex].BackColor = Color.Lime;
                        Process.NextDuriation = 200;
                        Process.ID = 15;

                        m_RunningIndex++;
                    }
                    break;
                case 15:
                    if (Process.IsTimeup)
                    {
                        Process.NextDuriation = 200;
                        Process.ID = 10;
                        if (m_RunningIndex < COUNT)
                        {
                            //Process.NextDuriation = 200;
                            //Process.ID = 10;
                        }
                        else
                        {
                            m_RunningIndex = 0;
                            i = 0;
                            while (i < COUNT)
                            {
                                lbl[i].BackColor = Control.DefaultBackColor;
                                i++;
                            }
                        }
                        
                    }
                    break;
            }
        }
        ProcessClass m_RunProcess2 = new ProcessClass();
        void t2Tick()
        {
            ProcessClass Process = m_RunProcess2;

            if (!Process.IsOn)
                return;

            switch (Process.ID)
            {
                case 5:
                    m_RunProcess1.Stop();
                    int i = 0;
                    while (i < COUNT)
                    {
                        lbl[i].BackColor = Control.DefaultBackColor;
                        i++;
                    }
                    Process.NextDuriation = 500;
                    Process.ID = 10;
                    m_RunningIndex = COUNT - 1;
                    break;
                case 10:
                    if (Process.IsTimeup)
                    {
                        lbl[m_RunningIndex].BackColor = Color.Lime;
                        Process.NextDuriation = 200;
                        Process.ID = 15;

                        m_RunningIndex--;
                    }
                    break;
                case 15:
                    if (Process.IsTimeup)
                    {
                        Process.NextDuriation = 200;
                        Process.ID = 10;
                        if (m_RunningIndex > 0)
                        {
                            //Process.NextDuriation = 200;
                            //Process.ID = 10;
                        }
                        else
                        {
                            m_RunningIndex = COUNT - 1;
                            i = 0;
                            while (i < COUNT)
                            {
                                lbl[i].BackColor = Control.DefaultBackColor;
                                i++;
                            }
                        }

                    }
                    break;
            }
        }
    }
}
