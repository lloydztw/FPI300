using JetEazy.QMath;
using OpenCvSharp;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace JetEazy.Transform
{
    partial class QTransform
    {
        public void LoadBin(string fileName)
        {
            var m_name = System.IO.Path.GetFileName(fileName);

            BinaryFormatter bformatter = new BinaryFormatter();

            try
            {
                using (Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    //(1) _srcPoints and _dstPoints
                    int rows = (int)bformatter.Deserialize(stream);
                    int cols = (int)bformatter.Deserialize(stream);
                    if (rows < 2 || cols < 2)
                    {
                        //_srcPoints = initDefaultPoints(100, "pix");
                        //_dstPoints = initDefaultPoints(100, "mm");
                        return;
                    }
                    else
                    {
                        _srcPoints = new QVector[rows, cols];
                        _dstPoints = new QVector[rows, cols];
                        _load(_srcPoints, stream, bformatter);
                        _load(_dstPoints, stream, bformatter);
                    }

                    //(2) _mat and _matInv
                    int dummy_rows = (int)bformatter.Deserialize(stream);
                    int dummy_cols = (int)bformatter.Deserialize(stream);
                    _load(_mat, stream, bformatter);
                    _load(_matInv, stream, bformatter);

                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                _ERROR(ex, "LoadBin");
            }
        }
        public void SaveBin(string fileName)
        {
            BinaryFormatter bformatter = new BinaryFormatter();

            try
            {
                using (Stream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                {
                    //(1) _srcPoints and _dstPoints
                    getRowsCols(_dstPoints, out int rows, out int cols);
                    bformatter.Serialize(stream, rows);
                    bformatter.Serialize(stream, cols);
                    _save(_srcPoints, stream, bformatter);
                    _save(_dstPoints, stream, bformatter);

                    //(2) _mat and _matInv
                    bformatter.Serialize(stream, 1);    // dummy rows == 1
                    bformatter.Serialize(stream, 1);    // dummy cols == 1
                    _save(_mat, stream, bformatter);
                    _save(_matInv, stream, bformatter);

                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                _ERROR(ex, "SaveBin");
            }
        }

        private void _load(QVector[,] pts, Stream fs, BinaryFormatter bformatter)
        {
            int rows = pts.GetLength(0);
            int cols = pts.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    QVector v = new QVector(2);
                    v.x = (double)bformatter.Deserialize(fs);
                    v.y = (double)bformatter.Deserialize(fs);
                    pts[i, j] = v;
                }
            }
        }
        private void _save(QVector[,] pts, Stream fs, BinaryFormatter bformatter)
        {
            int rows = pts.GetLength(0);
            int cols = pts.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    /*
                    double x = pts[i, j].x;
                    double y = pts[i, j].y;
                    _saveItem(x, strIniFileName, strAppName, "X", i, j);
                    _saveItem(y, strIniFileName, strAppName, "Y", i, j);
                    */
                    QVector v = pts[i, j];
                    bformatter.Serialize(fs, v[0]);
                    bformatter.Serialize(fs, v[1]);
                }
            }
        }
        private void _load(Mat mx, Stream fs, BinaryFormatter bformatter)
        {
            int rows = mx.Rows;
            int cols = mx.Cols;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //> _loadItem(ref value, strIniFileName, strAppName, "M", i, j);
                    double value = (double)bformatter.Deserialize(fs);
                    mx.At<double>(i, j) = value;
                }
            }
        }
        private void _save(Mat mx, Stream fs, BinaryFormatter bformatter)
        {
            int rows = mx.Rows;
            int cols = mx.Cols;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double value = mx.At<double>(i, j);
                    //> _saveItem(value, strIniFileName, strAppName, "M", i, j);
                    bformatter.Serialize(fs, value);
                }
            }
        }

        #region LOG
        static NLog.Logger _LOG = NLog.LogManager.GetCurrentClassLogger();
        static void _ERROR(Exception ex, string tag)
        {
            _LOG.Error(ex, tag);
        }
        #endregion
    }
}
