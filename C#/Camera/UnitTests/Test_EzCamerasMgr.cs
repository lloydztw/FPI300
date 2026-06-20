using EzCamera.Driver.WebCam;
using EzCamera.GUI;
using EzCamera.Interface;
using EzCamera.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UnitTest_EzCameras
{
    public class Test_Base
    {
        protected static void _SHOW_IMAGE(Bitmap img, string title, int autoCloseDelay = 0)
        {
            _TRACE(img);

            if (img == null)
            {
                //_TRACE("_SHOW_IMAGE(img == null) !");
                return;
            }

            using (var frm = new Form())
            {
                frm.Text = title;
                var panel = new PictureBox();
                panel.BackgroundImageLayout = ImageLayout.Zoom;
                frm.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
                panel.BackgroundImage = img;
                panel.Visible = true;
                frm.Size = new System.Drawing.Size(640, 480);

                if (autoCloseDelay > 0)
                {
                    new Action(() =>
                    {
                        System.Threading.Thread.Sleep(autoCloseDelay);
                        if (frm != null && frm.IsHandleCreated)
                            frm.BeginInvoke((Action)frm.Close);
                    }).BeginInvoke(null, null);
                }

                frm.ShowDialog();
            }
        }
        protected static void _TRACE(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);
        }
        protected static void _TRACE(Bitmap bmp)
        {
            if (bmp != null)
            {
                _TRACE("[Bitmap] {0}x{1} ({2})", bmp.Width, bmp.Height, bmp.PixelFormat);
            }
            else
            {
                _TRACE("[Bitmap] => null");
            }
        }
        protected static string _FORMAT(IEzCamera cam)
        {
            if (cam == null)
                return "";
            return $"[GID={cam.GlobalCamID}] [CamID={cam.CamID}] {cam.DeviceInfo}";
        }
        
        protected static void _VERIFY(IEzCamera cam, int dispTime = 2000)
        {
            Assert.IsNotNull(cam, "Camera is Null!");
            _TRACE(_FORMAT(cam));

            cam.Init();
            System.Threading.Thread.Sleep(100);
            cam.StartLiveMode();
            System.Threading.Thread.Sleep(100);

            using (var bmp = cam.Snapshot())
            {
                Assert.IsNotNull(bmp, "無法 snapshot!");
                _SHOW_IMAGE(bmp, _FORMAT(cam), dispTime);
            }
        }
    }

    [TestClass]
    public class Test_EzCamerasMgr : Test_Base
    {
        [TestMethod]
        public void Test_001_WebCameras()
        {
            var factory = new EzUsbWebCameraFactory();
            var infos = factory.GetAvailableCameraInfos();
            var index = 0;
            foreach (var info in infos)
            {
                _TRACE($"WebCam[{index}] = {info}");
                index++;
            }

            var cameras = new List<IEzCamera>();
            foreach(var info in infos)
            {
                var cam = factory.LoadCamera(info);
                cameras.Add(cam);
            }

            foreach(var cam in cameras)
            {
                _VERIFY(cam);
                cam.Dispose();
            }
        }

        [TestMethod]
        public void Test_002_BrowseConfig()
        {
            var mgr = AppCamerasManager.Instance;

            var camA = mgr.BrowseCameraConfig();
            while (camA == null)
                camA = mgr.BrowseCameraConfig();
            _TRACE("CamA = {0}", _FORMAT(camA));
            
            var camB = mgr.LoadCamera(camA.GlobalCamID);
            _TRACE("CamB = {0}", _FORMAT(camB));

            Assert.IsTrue(_FORMAT(camA) == _FORMAT(camB));
            Assert.IsTrue(camA == camB);

            _VERIFY(camA);

            AppCamerasManager.DisposeAll();
        }

        [TestMethod]
        public void Test_003_LoadLastCamera()
        {
            var mgr = AppCamerasManager.Instance;
            
            var cam = mgr.LoadCamera(-1);
            _TRACE("Last Camera = {0}", _FORMAT(cam));
            _VERIFY(cam);

            cam?.Dispose();
        }

        [TestMethod]
        public void Test_004_BuildMultiCameras()
        {
            int N = 3;
            var mgr = AppCamerasManager.Instance;
            mgr.Build(N);

            for (int gid = 0; gid < N; ++gid)
            {
                var cam = mgr.LoadCamera(gid);
                _VERIFY(cam, 3000);
                cam?.Dispose();
            }
        }
    }
}