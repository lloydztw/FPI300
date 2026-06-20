using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MediaFoundation;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;

namespace JetEazy.Media
{
    public class Mp4Recorder : IDisposable
    {
        #region PRIVATE_DATA
        private IMFSinkWriter _sinkWriter;
        private int _streamIndex;
        private int _width, _height, _fps;
        private long _frameCount = 0;
        private bool _isStarted = false;
        #endregion


        #region WIN_API
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        #endregion

        public void Init(string outputPath, Size targetSize, double fps, Guid? mediaSubType = null)
        {
            if (targetSize != Size.Empty)
            {
                _width = targetSize.Width;
                _height = targetSize.Height;
            }
            if (fps > 0)
            {
                _fps = (int)Math.Round(fps);
            }

            System.Diagnostics.Trace.Assert(_isStarted == false, $"{GetType().Name} 重複 Init() !");

            MFExtern.MFStartup(0x00020070, MFStartup.Full);

            // 建立 Sink Writer
            MFExtern.MFCreateSinkWriterFromURL(outputPath, null, null, out _sinkWriter);

            // 輸出格式（H.264）
            IMFMediaType outMediaType;
            MFExtern.MFCreateMediaType(out outMediaType);
            outMediaType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            outMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.H264);
            outMediaType.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, 8000000);
            outMediaType.SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, (int)MFVideoInterlaceMode.Progressive);
            MFExtern.MFSetAttributeSize(outMediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, _width, _height);
            MFExtern.MFSetAttributeRatio(outMediaType, MFAttributesClsid.MF_MT_FRAME_RATE, _fps, 1);
            MFExtern.MFSetAttributeRatio(outMediaType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);

            _sinkWriter.AddStream(outMediaType, out _streamIndex);

            // 輸入格式（RGB24）
            IMFMediaType inMediaType;
            MFExtern.MFCreateMediaType(out inMediaType);
            inMediaType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            inMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.RGB24);
            inMediaType.SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, (int)MFVideoInterlaceMode.Progressive);
            MFExtern.MFSetAttributeSize(inMediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, _width, _height);
            MFExtern.MFSetAttributeRatio(inMediaType, MFAttributesClsid.MF_MT_FRAME_RATE, _fps, 1);
            MFExtern.MFSetAttributeRatio(inMediaType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);

            _sinkWriter.SetInputMediaType(_streamIndex, inMediaType, null);
            _sinkWriter.BeginWriting();

            _isStarted = true;
        }

        public void PushFrame(IntPtr rgb24Ptr, int bufferSize)
        {
            if (!_isStarted) return;

            bufferSize = _width * _height * 3;

            // 建立 Media Buffer
            MFExtern.MFCreateMemoryBuffer(bufferSize, out IMFMediaBuffer buffer);

            // 鎖定並複製資料
            buffer.Lock(out IntPtr destPtr, out _, out _);
            CopyMemory(destPtr, rgb24Ptr, (uint)bufferSize);
            buffer.Unlock();
            buffer.SetCurrentLength(bufferSize);

            // 包裝成 Sample
            MFExtern.MFCreateSample(out IMFSample sample);
            sample.AddBuffer(buffer);

            long sampleTime = _frameCount * 10_000_000L / _fps;
            long sampleDuration = 10_000_000L / _fps;

            sample.SetSampleTime(sampleTime);
            sample.SetSampleDuration(sampleDuration);

            _sinkWriter.WriteSample(_streamIndex, sample);

            _frameCount++;
            Marshal.ReleaseComObject(buffer);
            Marshal.ReleaseComObject(sample);
        }

        private void Finish()
        {
            if (_sinkWriter != null && _isStarted)
            {
                _sinkWriter.Finalize_();
                Marshal.ReleaseComObject(_sinkWriter);
                _sinkWriter = null;
                MFExtern.MFShutdown();
                _isStarted = false;
            }
        }

        public void Dispose()
        {
            Finish();
        }
    }
}