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
using System.Drawing;
using System.Drawing.Imaging;
using JetEazy.Utils;
using LaserAlignDX.GA.BasicSpace;
using Traveller.FPI30.Model;

namespace LaserAlignDX.AoiModel.V3
{
    public partial class AoiModel_FlyCam : IAoiFlyCamRunner
    {
        #region PRIVATE_MEMBERS
        readonly object _flyRunGate = new object();
        readonly IAoiQrDecoder _runnerDecoder;
        bool _flyDisposed;
        #endregion

        public AoiModel_FlyCam() { }

        /// <summary>Borrows the same decoder used by the main AOI; does not dispose it.</summary>
        public AoiModel_FlyCam(IAoiQrDecoder decoder)
        {
            _runnerDecoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        }

        public FlyAoiResult Run(FlyID flyId, Bitmap image, FlyAoiRunOptions options)
        {
            lock (_flyRunGate)
            {
                if (_flyDisposed) throw new ObjectDisposedException(nameof(AoiModel_FlyCam));
                return RunCore(this, _runnerDecoder, flyId, image, options);
            }
        }

        #region PRIVATE_FUNCTIONS
        // Kept independent of SDK construction so the pipeline can be verified with substitute tools.
        internal static FlyAoiResult RunCore(IAoiFlyCamMatcher matcher, IAoiQrDecoder decoder,
            FlyID flyId, Bitmap image, FlyAoiRunOptions options)
        {
            if (matcher == null) throw new ArgumentNullException(nameof(matcher));
            if (flyId == null) throw new ArgumentNullException(nameof(flyId));
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if ((flyId.flyStart != 1 && flyId.flyStart != 2) || flyId.flyIndex < 0 || flyId.flyIndex >= 4)
                throw new ArgumentOutOfRangeException(nameof(flyId));

            var roi = options.TemplateRegion;
            var centerOrg = JetEazy.Qcvt.Center(ref roi);
            roi.Inflate(options.ExtendX, options.ExtendY);
            ModelUtilities.Clip(ref roi, image.Width, image.Height);
            if (roi.Width <= 0 || roi.Height <= 0 || float.IsNaN(roi.Width) || float.IsNaN(roi.Height))
                throw new ArgumentException("Fly AOI region does not intersect the image.", nameof(options));

            var meta = new FlyMetaData
            {
                AlgorithmName = options.UseSpecialAngle ? "MVD_CheckSpecialAngle" : "MVD_TemplateMatch",
                roiRect = roi, bmpFly = image, flyID = flyId
            };
            var result = new FlyAoiResult { MetaData = meta, Code = PlcResultCode.NG, CodeStr = string.Empty };
            meta.flyAoiResult = result;
            using (var crop = image.Clone(roi, PixelFormat.Format8bppIndexed))
            {
                if (options.UseSpecialAngle)
                {
                    bool ok = matcher.CheckSpecialAngle(crop, out var blobs, out float angle, out PointF center);
                    result.Code = ok ? PlcResultCode.OK : PlcResultCode.NG;
                    result.OffsetAngle = ok ? angle : 0;
                    meta.xBlobs = blobs == null ? null : Array.ConvertAll(blobs.ToArray(), b => b.RectInfo.ToBox2D());
                    FlyMetaData.Offset(meta.xBlobs, roi.X, roi.Y);
                }
                else
                {
                    matcher.Run(crop);
                    var rectangles = matcher.MatchResultRects;
                    // Preserve the existing non-null result criterion, including an empty list.
                    result.Code = rectangles != null ? PlcResultCode.OK : PlcResultCode.NG;
                    if (rectangles != null && rectangles.Count > 0)
                    {
                        meta.xResultBox2D = rectangles[0]?.ToBox2D();
                        result.OffsetAngle = matcher.MatchResultAngle;
                    }
                    FlyMetaData.Offset(meta.xResultBox2D, roi.X, roi.Y);
                    if (options.DecodeCode)
                    {
                        if (decoder == null) throw new InvalidOperationException("A shared QR decoder is required for fly decoding.");
                        result.CodeStr = decoder.TryDecode(crop) ?? string.Empty;
                    }
                    meta.xTemplateRect = options.TemplateRegion;
                    if (result.Code == PlcResultCode.OK)
                        CalculateOffsets(result, centerOrg, meta.xCentroid, options);
                }
            }
            return result;
        }

        static void CalculateOffsets(FlyAoiResult result, PointF templateCenter, PointF currentCenter, FlyAoiRunOptions options)
        {
            var template = new TemplatePositioningCalculator2D.TemplateData(
                new TemplatePositioningCalculator2D.Vector2(templateCenter.X, templateCenter.Y), 0);
            var current = new TemplatePositioningCalculator2D.TemplateData(
                new TemplatePositioningCalculator2D.Vector2(currentCenter.X, currentCenter.Y), result.OffsetAngle);
            // This was the final (comp2) calculation in GaPlcFlyCameraCtrl.
            var compensation = TemplatePositioningCalculator2D.CalculateCompensation(template, current,
                TemplatePositioningCalculator2D.ApplyOrder.TranslateThenRotate);
            double x = compensation.Translation.X * options.ImageResolution + options.PositionOffset.X;
            double y = compensation.Translation.Y * options.ImageResolution + options.PositionOffset.Y;
            if (result.OffsetAngle < -90 || result.OffsetAngle > 90) { x = -x; y = -y; }
            result.OffsetX = (float)x;
            result.OffsetY = (float)y;
        }
        #endregion
    }
}
