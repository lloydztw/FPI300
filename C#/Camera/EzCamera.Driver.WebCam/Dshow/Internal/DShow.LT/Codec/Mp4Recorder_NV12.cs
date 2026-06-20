using MediaFoundation;
using MediaFoundation.ReadWrite;
using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace JetEazy.Media
{
    public class Mp4RecorderNV12 : IDisposable
    {
        #region PRIVATE_DATA
        private IMFSinkWriter _sinkWriter;
        private int _streamIndex;
        private long _rtStart;
        private bool _isMfStarted = false;
        private int VIDEO_WIDTH = 1280;
        private int VIDEO_HEIGHT = 720;
        private int FRAME_RATE_NUM = 60;
        private const int FRAME_RATE_DEN = 1;
        private const int BIT_RATE = 10000000; // 10 Mbps
        #endregion

        #region WIN_API
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        #endregion

        public void Init(string outputPath, Size targetSize, double fps, Guid? mediaSubType = null)
        {
            if (targetSize!=Size.Empty)
            {
                VIDEO_WIDTH = targetSize.Width;
                VIDEO_HEIGHT = targetSize.Height;
            }
            if (fps > 0)
            {
                FRAME_RATE_NUM = (int)Math.Round(fps);
            }

            System.Diagnostics.Trace.Assert(_isMfStarted == false, "Mp4Recorder 重複 Init");
            MFExtern.MFStartup(0x20070, MFStartup.Full);
            _isMfStarted = true;

            IMFAttributes attr;
            MFExtern.MFCreateAttributes(out attr, 1);
            attr.SetUINT32(MFAttributesClsid.MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, 1);
            MFExtern.MFCreateSinkWriterFromURL(outputPath, null, attr, out _sinkWriter);

            IMFMediaType outMediaType;
            MFExtern.MFCreateMediaType(out outMediaType);
            outMediaType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            outMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.H264);
            outMediaType.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, BIT_RATE);
            MFExtern.MFSetAttributeSize(outMediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
            MFExtern.MFSetAttributeRatio(outMediaType, MFAttributesClsid.MF_MT_FRAME_RATE, FRAME_RATE_NUM, FRAME_RATE_DEN);
            MFExtern.MFSetAttributeRatio(outMediaType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
            _sinkWriter.AddStream(outMediaType, out _streamIndex);

            IMFMediaType inMediaType;
            MFExtern.MFCreateMediaType(out inMediaType);
            inMediaType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            if (mediaSubType == null)
            {
                //inMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.NV12);
                inMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.RGB24);
            }
            else
            {
                inMediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, mediaSubType.Value);
            }

            MFExtern.MFSetAttributeSize(inMediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, VIDEO_WIDTH, VIDEO_HEIGHT);
            MFExtern.MFSetAttributeRatio(inMediaType, MFAttributesClsid.MF_MT_FRAME_RATE, FRAME_RATE_NUM, FRAME_RATE_DEN);
            MFExtern.MFSetAttributeRatio(inMediaType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);

            _sinkWriter.SetInputMediaType(_streamIndex, inMediaType, null);
            _sinkWriter.BeginWriting();
            _rtStart = 0;
        }

        private void FinalizeRecording()
        {
            if (_sinkWriter != null)
            {
                _sinkWriter.Finalize_();
                Marshal.ReleaseComObject(_sinkWriter);
            }
            if (_isMfStarted)
            {
                _isMfStarted = false;
                MFExtern.MFShutdown();
            }
        }

        public void Dispose()
        {
            FinalizeRecording();
        }

        public void PushFrame(IntPtr bufferPtr, int bufferSize)
        {
            if (!_isMfStarted || _sinkWriter == null)
                return;

            IMFSample sample;
            IMFMediaBuffer mediaBuffer;
            MFExtern.MFCreateMemoryBuffer(bufferSize, out mediaBuffer);
            IntPtr pBuffer;
            int maxLen, currentLen;
            mediaBuffer.Lock(out pBuffer, out maxLen, out currentLen);
            //Marshal.Copy(bufferPtr, 0, pBuffer, bufferSize);
            CopyMemory(pBuffer, bufferPtr, (uint)bufferSize);
            mediaBuffer.Unlock();
            mediaBuffer.SetCurrentLength(bufferSize);
            MFExtern.MFCreateSample(out sample);

            sample.AddBuffer(mediaBuffer);
            sample.SetSampleTime(_rtStart);
            sample.SetSampleDuration(10000000 / FRAME_RATE_NUM);
            _sinkWriter.WriteSample(_streamIndex, sample);
            _rtStart += 10000000 / FRAME_RATE_NUM;
        }
    }
}
