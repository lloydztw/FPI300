using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using OpenCvSharp;
using JetEazy.QMath;
using CvMat = OpenCvSharp.Mat;
using MatrixType = OpenCvSharp.MatType;
using CvScalar = OpenCvSharp.Scalar;
using CvPoint2D32f = OpenCvSharp.Point2f;
using CvFileStorage = OpenCvSharp.FileStorage;


namespace JetEazy.Aoi.TransformD
{
    partial class CxCamCoordTransform
    {
        public void LoadBin(string fileName)
        {
            m_name = System.IO.Path.GetFileName(fileName);

            BinaryFormatter bformatter = new BinaryFormatter();
            Stream stream = null;

            try
            {
                stream = File.Open(fileName, FileMode.Open, FileAccess.Read);

                // JetEazy.Win32.Win32Ini.Load(ref str, fileName, "Scale", "World");
                // if (!string.IsNullOrEmpty(str)) double.TryParse(str, out SCALE_OF_WORLD);
                // JetEazy.Win32.Win32Ini.Load(ref str, fileName, "Scale", "View");
                // if (!string.IsNullOrEmpty(str)) double.TryParse(str, out SCALE_OF_VIEW);
                SCALE_OF_WORLD = (double)bformatter.Deserialize(stream);
                SCALE_OF_VIEW = (double)bformatter.Deserialize(stream);
                if (SCALE_OF_WORLD <= 0) SCALE_OF_WORLD = 1.0;
                if (SCALE_OF_VIEW <= 0) SCALE_OF_VIEW = 1.0;
                //var dummy1 = (double)bformatter.Deserialize(stream);
                //var dummy2 = (double)bformatter.Deserialize(stream);

                // JetEazy.Win32.Win32Ini.Load(ref m_iRows, fileName, "Dimensions", "Rows");
                // JetEazy.Win32.Win32Ini.Load(ref m_iCols, fileName, "Dimensions", "Cols");
                m_iRows = (int)bformatter.Deserialize(stream);
                m_iCols = (int)bformatter.Deserialize(stream);

                bool bEmptyMatrix = false;
                if (m_iRows < 2) { m_iRows = 2; bEmptyMatrix = true; }
                if (m_iCols < 2) { m_iCols = 2; bEmptyMatrix = true; }

                _initArrays();

                if (!bEmptyMatrix)
                {
                    //_load(m_matriceW2V, fs, "MATRIX");
                    //_load(m_matriceV2W, fs, "MATRIX_V2W");
                    _load(m_matriceW2V, stream, bformatter);
                    _load(m_matriceV2W, stream, bformatter);
                }

                //_load(m_pointsInView, fs, "ViewPoints");
                //_load(m_pointsInWorld, fs, "WorldPoints");
                _load(m_pointsInView, stream, bformatter);
                _load(m_pointsInWorld, stream, bformatter);

                stream.Close();
                stream.Dispose();
                stream = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                _LOG(ex);
                if (stream != null)
                    stream.Dispose();
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
        public void SaveBin(string fileName)
        {
            BinaryFormatter bformatter = new BinaryFormatter();
            Stream stream = null;

            try
            {
                stream = File.Open(fileName, FileMode.Create, FileAccess.Write);

                //JetEazy.Win32.Win32Ini.Save(SCALE_OF_WORLD, fileName, "Scale", "World");
                //JetEazy.Win32.Win32Ini.Save(SCALE_OF_VIEW, fileName, "Scale", "View");
                //JetEazy.Win32.Win32Ini.Save(m_iRows, fileName, "Dimensions", "Rows");
                //JetEazy.Win32.Win32Ini.Save(m_iCols, fileName, "Dimensions", "Cols");
                bformatter.Serialize(stream, SCALE_OF_WORLD);
                bformatter.Serialize(stream, SCALE_OF_VIEW);
                bformatter.Serialize(stream, m_iRows);
                bformatter.Serialize(stream, m_iCols);

                //_save(m_matriceW2V, fs, "MATRIX");
                //_save(m_matriceV2W, fs, "MATRIX_V2W");
                //_save(m_pointsInView, fs, "ViewPoints");
                //_save(m_pointsInWorld, fs, "WorldPoints");
                _save(m_matriceW2V, stream, bformatter);
                _save(m_matriceV2W, stream, bformatter);
                _save(m_pointsInView, stream, bformatter);
                _save(m_pointsInWorld, stream, bformatter);

                stream.Close();
                stream.Dispose();
                stream = null;
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                _LOG(ex);
                if (stream != null)
                    stream.Dispose();
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        private void _load(QVector[,] pts, Stream fs, BinaryFormatter bformatter)
        {
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
                {
#if(false)
                    QVector v = new QVector(2);
                    double x = 0;
                    double y = 0;
                    _loadItem(ref x, strIniFileName, strAppName, "X", i, j);
                    _loadItem(ref y, strIniFileName, strAppName, "Y", i, j);
                    v.x = x;
                    v.y = y;
                    pts[i, j] = v;
#endif
                    QVector v = new QVector(2);
                    v.x = (double)bformatter.Deserialize(fs);
                    v.y = (double)bformatter.Deserialize(fs);
                    pts[i, j] = v;
                }
            }
        }
        private void _save(QVector[,] pts, Stream fs, BinaryFormatter bformatter)
        {
            for (int i = 0; i < m_iRows; i++)
            {
                for (int j = 0; j < m_iCols; j++)
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
        private void _load(CvMat[,] mxx, Stream fs, BinaryFormatter bformatter)
        {
            try
            {
                for (int i = 0; i < m_iRows - 1; i++)
                {
                    for (int j = 0; j < m_iCols - 1; j++)
                    {
                        //_load(mxx[i, j], strIniFileName, strAppName + "_" + i + "_" + j);
                        _load(mxx[i, j], fs, bformatter);
                    }
                }
            }
            catch (Exception ex)
            {
                //JetEazy.LoggerClass.Instance.WriteException(ex);
                _LOG(ex);
                // NOT READY YET!
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
        private void _save(CvMat[,] mxx, Stream fs, BinaryFormatter bformatter)
        {
            try
            {
                for (int i = 0; i < m_iRows - 1; i++)
                {
                    for (int j = 0; j < m_iCols - 1; j++)
                    {
                        _save(mxx[i, j], fs, bformatter);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
        private void _load(CvMat mx, Stream fs, BinaryFormatter bformatter)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    //> _loadItem(ref value, strIniFileName, strAppName, "M", i, j);
                    double value = (double)bformatter.Deserialize(fs);
                    mx.At<float>(i, j) = (float)value;
                }
            }
        }
        private void _save(CvMat mx, Stream fs, BinaryFormatter bformatter)
        {
            int iRows = mx.Rows;
            int iCols = mx.Cols;
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    double value = mx.At<float>(i, j);
                    //> _saveItem(value, strIniFileName, strAppName, "M", i, j);
                    bformatter.Serialize(fs, value);
                }
            }
        }
    }
}
