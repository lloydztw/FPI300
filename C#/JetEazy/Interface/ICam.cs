namespace JetEazy.Interface
{
    public interface ICam
    {
        bool IsSim();
        int Initial(string inipara);
        void SetExposure(int val);
        void SetExposure(float val);
        void SetGain(float val);
        void StartCapture();
        void StopCapture();
        void Snap();
        System.Drawing.Bitmap GetSnap(int msec = 1000);
        int RotateAngle { get; set; }
    }
}
