using EzCamera.GUI;
using JetEazy;
using JetEazy.BasicSpace;
using JetEazy.ImageViewerEx;
using JzDisplay.Interface;
using JzDisplay.OPSpace;
using MoveGraphLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace JzDisplay.UISpace
{
    public partial class DispUI : UserControl, IDispUI
    {
        #region GUI_LINKS
        public Label lblInformation => label1;
        Label lblTestInformation => label2;
        Control picDisplay => ezImageViewer;  // 改成綁定 LeTian's Viewer
        #endregion

        #region PRIVATE_DATA
        JzTimes myJzTimes = new JzTimes();
        OPDisplay OPDISP;
        #endregion

        public DispUI()
        {
            InitializeComponent();

            picDisplay.Dock = DockStyle.Bottom;
            this.SizeChanged += DispUI_SizeChanged;
            if (!DesignMode)
                Initial();
        }

        /// <summary>
        /// 可以使用此接口, 把 EzCamera 橋接過來
        /// </summary>
        public IvCameraViewer CameraViewer
        {
            get
            {
                if (this.ezImageViewer is IvCameraViewer camViewer)
                    return camViewer;
                return null;
            }
        }
        public IvImageViewer ImageViewer
        {
            get { return ezImageViewer; }
        }

        public bool ISMOUSEDOWN
        {
            get
            {
                return OPDISP.ISMOUSEDOWN;
            }
        }
        public int DisplayWidth
        {
            get
            {
                return picDisplay.Width;
            }
        }
        public int DisplayHeight
        {
            get
            {
                return picDisplay.Height;
            }
        }

        public void Initial(float maxratio = 10f, float minratio = 0.1f)
        {
            if (OPDISP != null)
                return;

            OPDISP = new OPDisplay(picDisplay, lblInformation);
            OPDISP.MoverAction += OPDISP_MoverAction;
            OPDISP.AdjustAction += OPDISP_AdjustAction;
            OPDISP.DebugAction += OPDISP_DebugAction;
            OPDISP.CaptureAction += OPDISP_CaptureAction;
            picDisplay.MouseDown += picDisplay_MouseDown;
        }
        public bool DispUIload(Form myForm = null)
        {
#if(OPT_BYPASS_DONGLE)
            return true;
#else
            ProjectForAllinone.ProjectClass project = new ProjectForAllinone.ProjectClass();
            bool isok = project.GetDecode(JetEazy.Universal.MYDECODE);
            if (!isok)
            {
                myForm?.Close();
                OPDISP = null;
            }
            return isok;
#endif
        }

        #region EVENT_HANDLERS
        private void DispUI_SizeChanged(object sender, EventArgs e)
        {
            autoLayout();
        }
        private void picDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            picDisplay.Focus();
        }
        private void OPDISP_DebugAction(string opstring)
        {
            //lblTestInformation.Text = opstring;
            lblTestInformation.Invalidate();

            OnDebug(opstring);
        }
        private void OPDISP_AdjustAction(PointF ptfoffset)
        {
            OnAdjustAction(ptfoffset);
        }
        private void OPDISP_MoverAction(MoverOpEnum moverop, string opstring)
        {
            OnMover(moverop, opstring);
        }
        private void OPDISP_CaptureAction(RectangleF rectf)
        {
            OnCapture(rectf);
        }
        #endregion

        #region OVERRIDES
        public override void Refresh()
        {
            autoLayout();
            base.Refresh();
        }
        protected override void OnLoad(EventArgs e)
        {
            autoLayout();
            base.OnLoad(e);
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            Suicide();
            base.OnHandleDestroyed(e);
        }
        void autoLayout()
        {
            if (this.Width == 0 || this.Height == 0)
                return;

            int h = 23;
            if (lblInformation != null && lblInformation.Visible)
            {
                lblInformation.Location = new Point(0, 0);
                lblInformation.Width = this.Width;
                lblInformation.Height = h;
            }
            else
            {
                h = 0;
            }

            picDisplay.Location = new Point(0, h);
            picDisplay.Height = this.Height - h;

            if (picDisplay.Dock == DockStyle.None)
            {
                picDisplay.Width = this.Width;
            }
        }
        #endregion

        #region Normal Functions
        public void ClearAll()
        {
            OPDISP.ClearDisplay();
        }
        public void ClearMover()
        {
            OPDISP.ClearMover();
        }
        public void SaveStatus(string pathname)
        {
            //>>> Bitmap bmp = new Bitmap(OPDISP.GetPaintImage());
            // GetScreen 會把 Mover 與 GeoFigure 都畫下來.
            using (Bitmap bmp = OPDISP.GetScreen())
            {
                bmp.Save(pathname + "\\TEST.png", ImageFormat.Png);

                string statusstring = OPDISP.ToStatusString();

                using (StreamWriter sw = new StreamWriter(pathname + "\\Status.jdb"))
                {
                    sw.Write(statusstring);
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        public void LoadStatus(string pathname)
        {
            Bitmap bmp = new Bitmap(pathname + "\\TEST.png");

            OPDISP.ReplaceDisplayImage(bmp, true);

            StreamReader sr = new StreamReader(pathname + "\\Status.jdb");

            string statusstring = sr.ReadToEnd();

            OPDISP.FromStatusString(statusstring);

            sr.Close();
            sr.Dispose();

            bmp.Dispose();
        }

        public void SetDisplayImage(Bitmap bmp)
        {
            OPDISP.ReplaceDisplayImage(bmp);
        }
        public void ReplaceDisplayImage(Bitmap bmp = null, bool IsResetMover = false)
        {
            OPDISP.ReplaceDisplayImage(bmp, IsResetMover);
        }

#if(OPT_OLD_VICTOR)
        public void ReplaceDisplayImage()
        {
            OPDISP.ReplaceDisplayImage(null, false);
        }
#endif

        public void ReplaceDisplayImage(string fileName)
        {
            Bitmap bmp = new Bitmap(1, 1);
            GetBMP(fileName, ref bmp);
            OPDISP.ReplaceDisplayImage(bmp, true);
            bmp?.Dispose();
        }

        public void ReplaceDisplayImage(Bitmap bmp)
        {
            OPDISP.ReplaceDisplayImage(bmp);
        }
        public void RefreshDisplayShape()
        {
          
            OPDISP.RefreshDisplayShape();
          
        }

        public void DefaultView()
        {
            OPDISP.DefaultView();

            //lblInformation.Location = new Point(0, 0);
            //picDisplay.Location = new Point(0, 23);

            //lblInformation.Width = this.Width;
            //lblInformation.Height = 23;

            //picDisplay.Width = this.Width;
            //picDisplay.Height = this.Height - lblInformation.Height;

            //this.Refresh();


        }
        public void SetDisplayType(DisplayTypeEnum displaytype)
        {
            OPDISP.DISPLAYTYPE = displaytype;
        }
        public void SetMover(Mover mover)
        {
            OPDISP.SetMover(mover);
        }
        public void SetStaticMover(Mover staticmover)
        {
            OPDISP.SetStaticMover(staticmover);
        }
        public void ClearStaticMover()
        {
            OPDISP.ClearStaticMover();
        }
        public void SetImode(int i = 0)
        {
            OPDISP.iMode = i;
        }

        public void SaveScreen()
        {
            OPDISP.SaveScreen();
        }

        /// <summary>
        /// 此函式會生成 【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        public Bitmap GetScreen()
        {
            return OPDISP.GetScreen();
        }
        public Bitmap GetScreen(float penwidth,float fontsize)
        {
            OPDISP.PenWidth = penwidth;
            OPDISP.FontSize = fontsize;
            return OPDISP.GetScreen();
        }

        public void BackupImage()
        {
            OPDISP.BackupImage();
        }
        public void RestoreImage()
        {
            OPDISP.RestoreImage();
        }
        public void Suicide()
        {
            OPDISP?.Suicide();
            OPDISP = null;
        }
        void GetBMP(string bmpstring, ref Bitmap bmp)
        {
            Bitmap bmptmp = new Bitmap(bmpstring);

            bmp.Dispose();
            bmp = new Bitmap(bmptmp);

            bmptmp.Dispose();
        }

        public void MoveMover(int x,int y)
        {
            OPDISP.MoveMover(x, y);
            OPDISP.MappingShape();

        }
        public void SizeMover(int x,int y)
        {
            OPDISP.SizeMover(x, y);
        }
        public void Lock(int level,bool isonly)
        {
            OPDISP.Lock(level, isonly);
        }
        public void SetMatching(Bitmap bmp,MatchMethodEnum matchmethod)
        {
            OPDISP.SetMatching(bmp, matchmethod);
        }

        public void GenSearchImage(ref Bitmap bmp)
        {
            OPDISP.GenSearchImage(ref bmp);
        }
        public void ReDraw()
        {
            OPDISP.ReDraw();
        }

        /// <summary>
        /// 此函式直接參考 bmpOrg.
        /// <br/> 調用者 【不可以】直接將其 Dispose() !!!
        /// </summary>
        public Bitmap GetOrgBMP()
        {
            return OPDISP.GetOrgBMP();
        }

        /// <summary>
        /// 在Capture時使用的抓圖方式
        /// <br/> 此函式會生成 【新的 bmp】.
        /// <br/> 調用者必須維持此新的 bmp 生命週期 !!!
        /// </summary>
        public Bitmap GetOrgBMP(RectangleF cropRect)
        {
            return OPDISP?.GetOrgBMP(cropRect);
        }

        public void SaveOrgBMP(string savefilename)
        {
            OPDISP.GetOrgBMP().Save(savefilename);
        }

        #endregion

        #region TEST Functions
        public void SimWheel()
        {
            OPDISP.SimWheel(1);
        }
        public void AddRect()
        {
            OPDISP.AddRect();
        }
        public void AddShape(ShapeEnum shape)
        {
            OPDISP.AddShape(shape);
        }
        public void DelShape()
        {
            OPDISP.DelShape();
        }
        public void HoldSelect()
        {
            OPDISP.IsHoldForSelct = true;
        }
        public void ReleaseSelect()
        {
            OPDISP.IsHoldForSelct = false;
        }
        public void GetMask(int outrangex, int outrangey)
        {
            OPDISP.GetMask(outrangex, outrangey);

        }
        public void MappingLsbSelect(List<int> lsbselectlist)
        {
            OPDISP.MappingLsbSelect(lsbselectlist);
        }
        public void MappingSelect()
        {
            OPDISP.MappingSelect();
        }
        #endregion

        #region EVENT_LAUNCHERs
        //public delegate void MoverHandler(MoverOpEnum moverop, string opstring);
        public event MoverHandler MoverAction;
        public void OnMover(MoverOpEnum moverop, string opstring)
        {
            if (MoverAction != null)
            {
                MoverAction(moverop, opstring);
            }
        }

        //public delegate void AdjustHandler(PointF ptfoffset);
        public event AdjustHandler AdjustAction;
        public void OnAdjustAction(PointF ptfoffset)
        {
            if (AdjustAction != null)
            {
                AdjustAction(ptfoffset);
            }
        }

        //public delegate void CaputreHandler(RectangleF rectf);
        public event CaputreHandler CaptureAction;
        public void OnCapture(RectangleF rectf)
        {
            if (CaptureAction != null)
            {
                CaptureAction(rectf);
            }
        }

        //public delegate void DebugHandler(string opstring);
        public event DebugHandler DebugAction;
        public void OnDebug(string opstring)
        {
            if (DebugAction != null)
            {
                DebugAction(opstring);
            }
        }
        #endregion
    }
}
