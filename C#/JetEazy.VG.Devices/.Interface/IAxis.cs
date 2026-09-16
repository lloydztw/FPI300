namespace JetEazy.Interface
{
    public interface IAxis
    {
        bool IsError { get; }
        bool IsOK { get; }
        bool IsHome { get; }

        void Home();
        void Forward();
        void Backward();
        void Stop();
        void Go(double frompos, double offset);
        void SetManualSpeed(int val);
        void SetActionSpeed(int val);
        double GetPos();
        string GetStatus();

        double GetInitPosition();

        void Go(int posindex, float position);
        void SetPos(int posindex, float position);
        void GoPos(int posindex);
        bool IsOnSitePos(int posindex);
        float GetSetPos(int posindex);

    }
}
