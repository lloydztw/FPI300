namespace EzCamera.Driver.Hikvision.Gaara
{
    public interface ICam
    {
        bool IsSim();
        void Initial(string inipara);
        void SetExposure(int val);
        void SetGain(float val);
        void StartCapture();
        void StopCapture();
        void Snap();
        System.Drawing.Bitmap GetSnap(int msec = 1000);
        int RotateAngle { get; set; }
    }
}
