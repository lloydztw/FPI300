using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionDesigner;

namespace LaserAlignDX.UISpace.UIMVC
{
    public partial class MVSUI : UserControl
    {
        public MVSUI()
        {
            InitializeComponent();

            //删除矩形菜单项，右键菜单中对应项会被删除
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdAddShape),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdFile),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdZoom),
              System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdRotate),
               System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdEraser),
               System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);
            mvdRenderActivex1.SetMenuState(System.Convert.ToUInt32(MVD_MENU_ID.MvdShapeMenuPaste),
                System.Convert.ToUInt32(MVD_MENU_CMD.MvdMenuDelete), null);

            //CMvdImage cMvdImage = new CMvdImage();
            //cMvdImage.InitImage(1000, 1000, MVD_PIXEL_FORMAT.MVD_PIXEL_MONO_08);
            //mvdRenderActivex1.LoadImageFromObject(cMvdImage);
            //SizeChanged += MVSUI_SizeChanged;
        }
        public void AddCross()
        {
            //CMvdLineSegmentF x1 =
            //    new CMvdLineSegmentF(new MVD_POINT_F(0, this.Height / 2), new MVD_POINT_F(this.Width, this.Height / 2));
            //CMvdLineSegmentF x2 =
            //    new CMvdLineSegmentF(new MVD_POINT_F(this.Width / 2, 0), new MVD_POINT_F(this.Width / 2, this.Height));
            //mvdRenderActivex1.AddShape(x1);
            //mvdRenderActivex1.AddShape(x2);
        }

        private void MVSUI_SizeChanged(object sender, EventArgs e)
        {
            //CMvdLineSegmentF x1 = 
            //    new CMvdLineSegmentF(new MVD_POINT_F(0, this.Height / 2), new MVD_POINT_F(this.Width, this.Height / 2));
            //CMvdLineSegmentF x2 =
            //    new CMvdLineSegmentF(new MVD_POINT_F(this.Width / 2, 0), new MVD_POINT_F(this.Width / 2, this.Height));
            //mvdRenderActivex1.AddShape(x1);
            //mvdRenderActivex1.AddShape(x2);
            //mvdRenderActivex1.Width = this.Width;
            //mvdRenderActivex1.Height = this.Height;
            //mvdRenderActivex1.Refresh();
            //mvdRenderActivex1.Display();
        }
    }
}
