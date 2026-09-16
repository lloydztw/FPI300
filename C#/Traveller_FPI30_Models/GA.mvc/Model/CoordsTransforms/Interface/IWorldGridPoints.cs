using JetEazy.QMath;

namespace LaserAlignDX.Model.Coords
{
    /// <summary>
    /// 理想的格點 (mm)
    /// </summary>
    public interface IWorldGridPoints
    {
        int Rows { get; }
        int Cols { get; }
        
        double PitchX { get; }
        double PitchY { get; }
        
        void Config(int rows, int cols, double pitchX, double pitchY);
        QVector Get(int row, int col);

        void Load(string iniFile, string sectName);
        void Save(string iniFile, string sectName);
    }
}