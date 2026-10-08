#region AUTHOR
/*
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * REVISION:
 *      2026-10-08 使用 ChatGPT Codex  重整 (by LeTian Chang)
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using LaserAlignDX;
using LaserAlignDX.AoiModel;
using LaserAlignDX.AoiModel.V3;
using VisionDesigner;
using VisionDesigner.BlobFind;

internal static class FlyRunnerChecks
{
    public static void Run()
    {
        Check(typeof(IAoiFlyCamRunner).IsAssignableFrom(typeof(AoiModel_FlyCam)), "Fly model does not implement runner.");
        Check(typeof(IAoiFlyCamMatcher).IsAssignableFrom(typeof(AoiModel_FlyCam)), "Legacy matcher interface was lost.");
        var matcher = new Matcher();
        var decoder = new Decoder();
        var options = new FlyAoiRunOptions
        {
            TemplateRegion = new RectangleF(10, 20, 20, 20), ExtendX = 2, ExtendY = 4,
            ImageResolution = .5f, PositionOffset = new PointF(3, 7), DecodeCode = true
        };
        using (var image = new Bitmap(80, 80))
        {
            foreach (var angle in new[] { 0f, 90f, -90f, 91f, -91f, 180f })
            {
                matcher.MatchResultAngle = angle;
                var result = Execute(matcher, decoder, new FlyID(2, 3), image, options);
                float sign = Math.Abs(angle) > 90 ? -1 : 1;
                Check(result.Code == PlcResultCode.OK && result.OffsetX == sign * 2 && result.OffsetY == sign * 5 && result.OffsetAngle == angle,
                    "Legacy compensation changed at angle " + angle);
                Check(result.CodeStr == "QR" && decoder.LastSize == new Size(24, 28), "Shared decoder or crop changed.");
                Check(result.MetaData.xCentroid == new PointF(22, 34) && result.MetaData.flyID.ShowID == 8,
                    "Full-image match geometry or second-batch ID changed.");
                Check(ReferenceEquals(result.MetaData.bmpFly, image) && ReferenceEquals(result.MetaData.flyAoiResult, result), "Borrowed image/result metadata changed.");
            }
            matcher.MatchResultRects = null;
            var ng = Execute(matcher, decoder, new FlyID(1, 0), image, options);
            Check(ng.Code == PlcResultCode.NG && ng.OffsetX == 0 && ng.OffsetY == 0 && ng.OffsetAngle == 0, "NG retained old compensation.");
            matcher.MatchResultRects = new List<CMvdRectangleF>();
            Check(Execute(matcher, decoder, new FlyID(1, 0), image, options).Code == PlcResultCode.OK, "Legacy empty-list criterion changed.");
            int decodeCalls = decoder.Calls;
            options.UseSpecialAngle = true;
            matcher.SpecialOk = true;
            var special = Execute(matcher, decoder, new FlyID(1, 0), image, options);
            Check(special.Code == PlcResultCode.OK && special.OffsetAngle == 37 && special.OffsetX == 0 && special.OffsetY == 0 &&
                special.CodeStr == "" && decoder.Calls == decodeCalls, "Special-angle branch changed.");
            matcher.SpecialOk = false;
            Check(Execute(matcher, decoder, new FlyID(1, 0), image, options).OffsetAngle == 0, "Special NG retained an angle.");
            options.TemplateRegion = new RectangleF(-5, -6, 20, 20); options.ExtendX = options.ExtendY = 0;
            Check(Execute(matcher, decoder, new FlyID(1, 0), image, options).MetaData.roiRect == new RectangleF(0, 0, 15, 14), "ROI clipping changed.");
            options.TemplateRegion = new RectangleF(100, 100, 10, 10);
            Throws<ArgumentException>(() => Execute(matcher, decoder, new FlyID(1, 0), image, options));
            Throws<ArgumentOutOfRangeException>(() => Execute(matcher, decoder, new FlyID(3, 0), image, options));
            options.TemplateRegion = new RectangleF(10, 20, 20, 20); options.UseSpecialAngle = false;
            Throws<InvalidOperationException>(() => Execute(matcher, null, new FlyID(1, 0), image, options));
            matcher.Fail = true;
            Throws<InvalidOperationException>(() => Execute(matcher, decoder, new FlyID(1, 0), image, options));
            Check(image.Width == 80 && !matcher.Disposed && !decoder.Disposed, "Runner disposed caller-owned resources.");
        }
        Console.WriteLine("PASS fly runner core: ROI, shared QR, template/special modes, compensation at +/-90, NG/exception isolation and borrowed ownership");
    }

    #region PRIVATE_METHODS
    static FlyAoiResult Execute(IAoiFlyCamMatcher matcher, IAoiQrDecoder decoder, FlyID id, Bitmap image, FlyAoiRunOptions options)
    {
        // Exercise the production pipeline without constructing licensed MVD algorithm objects.
        try
        {
            return (FlyAoiResult)typeof(AoiModel_FlyCam).GetMethod("RunCore", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { matcher, decoder, id, image, options });
        }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }
    static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    #endregion

    #region PRIVATE_TYPES
    sealed class Matcher : IAoiFlyCamMatcher
    {
        public List<CMvdRectangleF> MatchResultRects { get; set; } = new List<CMvdRectangleF> { new CMvdRectangleF(14, 18, 6, 8) };
        public float MatchResultAngle { get; set; }
        public bool SpecialOk { get; set; }
        public bool Fail { get; set; }
        public bool Disposed { get; private set; }
        public void Run(Bitmap image) { if (Fail) throw new InvalidOperationException("matcher failure"); }
        public bool Train(Bitmap image, params object[] args) { throw new InvalidOperationException("Unexpected training"); }
        public bool CheckSpecialAngle(Bitmap image, out List<CBlobInfo> blobs, out float angle, out PointF center)
        { blobs = null; angle = 37; center = new PointF(5, 6); return SpecialOk; }
        public void Dispose() { Disposed = true; }
    }
    sealed class Decoder : IAoiQrDecoder
    {
        public bool QrUsed { get; set; }
        public bool QrJudged { get; set; }
        public int Calls { get; private set; }
        public Size LastSize { get; private set; }
        public bool Disposed { get; private set; }
        public string TryDecode(Bitmap image, Rectangle? roi = null) { Calls++; LastSize = image.Size; return "QR"; }
        public void Run(Bitmap image = null) { throw new InvalidOperationException("Unexpected full QR run"); }
        public void SetCellGroups(GaCellsGroup[] groups) { throw new InvalidOperationException("Unexpected grouping"); }
        public void Dispose() { Disposed = true; }
    }
    #endregion
}
