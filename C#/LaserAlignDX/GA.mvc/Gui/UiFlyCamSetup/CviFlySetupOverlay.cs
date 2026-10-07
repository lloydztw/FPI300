using JetEazy.ImageViewerEx;
using JetEazy.ImageViewerEx.Interactors;
using JetEazy.QvMath;
using LaserAlignDX.Mvc.Gui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LaserAlignDX.FormSpace
{
    /// <summary>
    /// Fly-camera drawing and ROI interaction, independent of camera and recipe state.
    /// The panel owns its image; this interactor only owns its event subscriptions.
    /// </summary>
    internal sealed class CviFlySetupOverlay : CvImageViewerInteractor, IDisposable
    {
        private readonly JezTransImageViewPanel _panel;
        private readonly List<CviRotRectBox> _resultBoxes = new List<CviRotRectBox>();
        private readonly CviCross _cross = new CviCross(Color.Yellow);
        private bool _selectionEnabled;
        private bool _dragging;
        private bool _disposed;
        private PointF _start;
        private PointF _end;

        public CviFlySetupOverlay(JezTransImageViewPanel panel)
        {
            _panel = panel;
            Enabled = true;
            Visible = true;
            _cross.Enabled = true;
            _cross.Visible = true;
            panel.MatViewer.AddInteractor(_cross);
            panel.MatViewer.AddInteractor(this);
            panel.MatViewer.MouseCaptureChanged += OnCaptureChanged;
            panel.MatViewer.MouseDoubleClick += OnDoubleClick;
        }

        public event Action<RectangleF> RegionSelected;

        public bool SelectionEnabled
        {
            get => _selectionEnabled;
            set
            {
                _selectionEnabled = value;
                if (!value)
                    CancelSelection();
            }
        }

        public void ClearResults()
        {
            foreach (var box in _resultBoxes)
                _panel.MatViewer.RemoveInteractor(box);
            _resultBoxes.Clear();
            _panel.MatViewer.Invalidate();
        }

        public void AddResultBox(QvBox2D geometry)
        {
            if (geometry == null)
                return;

            var box = new CviRotRectBox(geometry, Color.Lime)
            {
                Enabled = true,
                Visible = true
            };
            _resultBoxes.Add(box);
            _panel.MatViewer.AddInteractor(box);
            _panel.MatViewer.Invalidate();
        }

        public override void OnDraw(CvImageViewer viewer, Graphics graphics)
        {
            // Crosshairs and results are separate viewer interactors.
            // This interactor only draws the in-progress ROI selection.
            if (!Visible || !_dragging || _panel.Image == null)
                return;

            bool wasWorld = viewer.IsInWorldCoordinate();
            try
            {
                if (!wasWorld)
                    viewer.SwitchToWorldCoordinate(graphics);
                var pen = viewer.GetOnePixelPen(Color.Lime);
                var roi = SelectionRectangle;
                graphics.DrawRectangle(pen, roi.X, roi.Y, roi.Width, roi.Height);
            }
            finally
            {
                if (!wasWorld)
                    viewer.SwitchToViewportCoordinate(graphics);
            }
        }

        public override bool OnMouseDown(CvImageViewer viewer, MouseEventArgs e)
        {
            if (!Enabled || !SelectionEnabled || e.Button != MouseButtons.Left || _panel.Image == null)
                return false;

            _start = _end = ToImagePoint(viewer, e);
            _dragging = true;
            _panel.MatViewer.Focus();
            _panel.MatViewer.Capture = true;
            _panel.MatViewer.Invalidate();
            return true;
        }

        public override bool OnMouseMove(CvImageViewer viewer, MouseEventArgs e)
        {
            if (!_dragging)
                return false;

            _end = ToImagePoint(viewer, e);
            _panel.MatViewer.Invalidate();
            return true;
        }

        public override bool OnMouseUp(CvImageViewer viewer, MouseEventArgs e)
        {
            if (!_dragging || e.Button != MouseButtons.Left)
                return false;

            _end = ToImagePoint(viewer, e);
            var roi = SelectionRectangle;
            CancelSelection();
            RegionSelected?.Invoke(roi);
            return true;
        }

        private RectangleF SelectionRectangle => RectangleF.FromLTRB(
            Math.Min(_start.X, _end.X), Math.Min(_start.Y, _end.Y),
            Math.Max(_start.X, _end.X), Math.Max(_start.Y, _end.Y));

        private static PointF ToImagePoint(CvImageViewer viewer, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            viewer.TransViewportToWorld(ref x, ref y);
            return new PointF(x, y);
        }

        private void CancelSelection()
        {
            if (!_dragging)
                return;
            _dragging = false;
            _panel.MatViewer.Capture = false;
            _panel.MatViewer.Invalidate();
        }

        private void OnCaptureChanged(object sender, EventArgs e)
        {
            if (!_panel.MatViewer.Capture)
                CancelSelection();
        }

        private void OnDoubleClick(object sender, MouseEventArgs e)
        {
            if (!SelectionEnabled && e.Button == MouseButtons.Left)
                _panel.MatViewer.RebuildViewport();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            CancelSelection();
            _panel.MatViewer.MouseCaptureChanged -= OnCaptureChanged;
            _panel.MatViewer.MouseDoubleClick -= OnDoubleClick;
            _panel.MatViewer.RemoveInteractor(this);
            _panel.MatViewer.RemoveInteractor(_cross);
            ClearResults();
            RegionSelected = null;
        }
    }
}
