using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Point of the source window an absolute region is anchored to (a corner, the
    /// middle of a border or the center). When the source window is resized, the
    /// region keeps its size and its distance from the anchor, so it follows content
    /// aligned to that point.
    /// </summary>
    /// <remarks>
    /// Values are ordered row by row: value % 3 is the horizontal edge and
    /// value / 3 the vertical edge (see <see cref="AnchorEdge"/>).
    /// </remarks>
    public enum RegionAnchor {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Edge of the source an anchored region is aligned to, along one axis.
    /// </summary>
    public enum AnchorEdge {
        /// <summary>Left or top border: the offset is the distance from that border.</summary>
        Start,
        /// <summary>Center: the offset is the displacement of the region's center from the source's center.</summary>
        Center,
        /// <summary>Right or bottom border: the offset is the distance from that border.</summary>
        End
    }

    /// <summary>
    /// Represents a thumbnail region.
    /// </summary>
    /// <remarks>
    /// A ThumbnailRegion can work in absolute or in relative mode.
    /// In absolute mode, the region is expressed in absolute pixel values, as expressed by the value of the
    /// <see cref="Bounds"/> property. The X and Y values are the offsets from the edges given by
    /// <see cref="Anchor"/>: with a right (bottom) anchor, X (Y) is the distance between the right (bottom)
    /// border of the source and the right (bottom) border of the region; with a centered anchor, X (Y) is
    /// the displacement of the region's center from the source's center (positive towards the right/bottom).
    /// In relative mode, the region is expressed in padding pixels from the borders of the source. Internally this
    /// is still represented by the <see cref="Bounds"/> property. Properties of the Rectangle value are mapped as follows:
    /// Rectangle.X = Padding.Left
    /// Rectangle.Y = Padding.Top
    /// Rectangle.Width = Padding.Right
    /// Rectangle.Height = Padding.Bottom
    /// </remarks>
    public class ThumbnailRegion {

        /// <summary>
        /// Creates a ThumbnailRegion from a padding value relative to the thumbnail borders.
        /// </summary>
        public ThumbnailRegion(Padding padding) {
            _bounds = new Rectangle {
                X = padding.Left,
                Y = padding.Top,
                Width = padding.Right,
                Height = padding.Bottom
            };
            Relative = true;
        }

        /// <summary>
        /// Creates a ThumbnailRegion from a bounds rectangle (in absolute terms).
        /// </summary>
        public ThumbnailRegion(Rectangle rectangle) {
            _bounds = rectangle;
            Relative = false;
        }

        /// <summary>
        /// Creates a ThumbnailRegion from a rectangle, either expressing values in relative or in absolute terms.
        /// </summary>
        public ThumbnailRegion(Rectangle paddingOrBounds, bool relative) {
            _bounds = paddingOrBounds;
            Relative = relative;
        }

        /// <summary>
        /// Creates an absolute ThumbnailRegion whose bounds are expressed from the edges of an anchor.
        /// </summary>
        public ThumbnailRegion(Rectangle bounds, RegionAnchor anchor) {
            _bounds = bounds;
            Relative = false;
            Anchor = anchor;
        }

        private Rectangle _bounds;

        /// <summary>
        /// Gets or sets the bounds of the thumbnail region.
        /// </summary>
        /// <remarks>
        /// X and Y are expressed from the edges given by <see cref="Anchor"/>.
        /// Use <see cref="ComputeRegionRectangle"/> to get the effective rectangle.
        /// </remarks>
        public Rectangle Bounds {
            get {
#if DEBUG
                if (Relative)
                    throw new InvalidOperationException("Not allowed to use ThumbnailRegion Bounds as Rectangle value (in relative mode).");
#endif

                return _bounds;
            }
            set {
                _bounds = value;
                Relative = false;
            }
        }

        /// <summary>
        /// Gets or sets whether the bounds are expressed relative to the thumbnail borders.
        /// </summary>
        public bool Relative {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the corner the region is anchored to (absolute mode only).
        /// </summary>
        public RegionAnchor Anchor {
            get;
            set;
        }

        /// <summary>
        /// Gets the horizontal edge an anchor aligns the region to.
        /// </summary>
        public static AnchorEdge GetHorizontalEdge(RegionAnchor anchor) {
            return (AnchorEdge)((int)anchor % 3);
        }

        /// <summary>
        /// Gets the vertical edge an anchor aligns the region to.
        /// </summary>
        public static AnchorEdge GetVerticalEdge(RegionAnchor anchor) {
            return (AnchorEdge)((int)anchor / 3);
        }

        /// <summary>
        /// Converts an offset from an anchor edge to a position inside the source.
        /// </summary>
        static int OffsetToPosition(int offset, int size, int sourceSize, AnchorEdge edge) {
            switch (edge) {
                case AnchorEdge.End:
                    return sourceSize - offset - size;
                case AnchorEdge.Center:
                    return (sourceSize - size) / 2 + offset;
                default:
                    return offset;
            }
        }

        /// <summary>
        /// Converts a position inside the source to an offset from an anchor edge
        /// (inverse of <see cref="OffsetToPosition"/>).
        /// </summary>
        static int PositionToOffset(int position, int size, int sourceSize, AnchorEdge edge) {
            switch (edge) {
                case AnchorEdge.End:
                    return sourceSize - position - size;
                case AnchorEdge.Center:
                    return position - (sourceSize - size) / 2;
                default:
                    return position;
            }
        }

        /// <summary>
        /// Gets whether the effective region depends on the size of the source.
        /// </summary>
        public bool DependsOnSourceSize {
            get { return Relative || Anchor != RegionAnchor.TopLeft; }
        }

        /// <summary>
        /// Sets the relative bounds of the region. Switches to relative mode.
        /// </summary>
        /// <param name="padding">Padding in relative terms from the borders.</param>
        public void SetRelativeBounds(Padding padding) {
            Bounds = new Rectangle {
                X = padding.Left,
                Y = padding.Top,
                Width = padding.Right,
                Height = padding.Bottom
            };
            Relative = true;
        }

        /// <summary>
        /// Gets the bounds of the thumbnail region as relative padding from the thumbnail borders.
        /// </summary>
        /// <remarks>Makes sense only in relative mode.</remarks>
        public Padding BoundsAsPadding {
            get {
#if DEBUG
                if (!Relative)
                    throw new InvalidOperationException("Not allowed to use ThumbnailRegion Bounds as Padding value (not in relative mode).");
#endif

                return new Padding {
                    Left = _bounds.X,
                    Top = _bounds.Y,
                    Right = _bounds.Width,
                    Bottom = _bounds.Height
                };
            }
        }

        const int MinimumRegionSize = 8;

        /// <summary>
        /// Computes the region inside a source of a certain size, without constraining it to the source bounds.
        /// </summary>
        private Rectangle ComputeUnconstrainedRegion(Size sourceSize) {
            if (Relative) {
                return new Rectangle {
                    X = _bounds.X,
                    Y = _bounds.Y,
                    Width = sourceSize.Width - _bounds.X - _bounds.Width,
                    Height = sourceSize.Height - _bounds.Y - _bounds.Height
                };
            }

            return new Rectangle {
                X = OffsetToPosition(_bounds.X, _bounds.Width, sourceSize.Width, GetHorizontalEdge(Anchor)),
                Y = OffsetToPosition(_bounds.Y, _bounds.Height, sourceSize.Height, GetVerticalEdge(Anchor)),
                Width = _bounds.Width,
                Height = _bounds.Height
            };
        }

        /// <summary>
        /// Computes the effective region representing the bounds inside a source thumbnail of a certain size.
        /// </summary>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        /// <returns>Bounds inside the thumbnail.</returns>
        protected Rectangle ComputeRegion(Size sourceSize) {
            Rectangle ret = ComputeUnconstrainedRegion(sourceSize);

            //Constrain to bounds (a right/bottom or center anchored region moves past
            //the left/top border when the source becomes smaller than the region)
            if (ret.X < 0) {
                ret.Width += ret.X;
                ret.X = 0;
            }
            if (ret.Y < 0) {
                ret.Height += ret.Y;
                ret.Y = 0;
            }
            if (ret.X + ret.Width > sourceSize.Width)
                ret.Width = sourceSize.Width - ret.X;
            if (ret.Y + ret.Height > sourceSize.Height)
                ret.Height = sourceSize.Height - ret.Y;

            return ret;
        }

        /// <summary>
        /// Computes a rectangle representing the bounds of the region inside a source thumbnail of a certain size.
        /// </summary>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        /// <returns>Bounds inside the thumbnail.</returns>
        public Rectangle ComputeRegionRectangle(Size sourceSize) {
            return ComputeRegion(sourceSize);
        }

        /// <summary>
        /// Computes a value representing the size of the region inside a source thumbnail of a certain size.
        /// </summary>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        /// <returns>Size of the bounds inside the thumbnail.</returns>
        public Size ComputeRegionSize(Size sourceSize) {
            return ComputeRegion(sourceSize).Size;
        }

        /// <summary>
        /// Switches the region to relative mode, according to a source thumbnail of a given size.
        /// </summary>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        public void SwitchToRelative(Size sourceSize) {
            if (Relative)
                return;

            var rect = ComputeUnconstrainedRegion(sourceSize);
            var relativeBounds = new Padding {
                Left = rect.X,
                Top = rect.Y,
                Right = sourceSize.Width - (rect.X + rect.Width),
                Bottom = sourceSize.Height - (rect.Y + rect.Height)
            };

            this.SetRelativeBounds(relativeBounds);
            Anchor = RegionAnchor.TopLeft;
        }

        /// <summary>
        /// Switches the region to absolute mode, according to a source thumbnail of a given size.
        /// </summary>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        public void SwitchToAbsolute(Size sourceSize) {
            SwitchToAnchor(RegionAnchor.TopLeft, sourceSize);
        }

        /// <summary>
        /// Switches the region to absolute mode anchored to a given point, keeping the region
        /// at the same place inside a source thumbnail of a given size.
        /// </summary>
        /// <param name="anchor">New anchor of the region.</param>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        public void SwitchToAnchor(RegionAnchor anchor, Size sourceSize) {
            if (!Relative && Anchor == anchor)
                return;

            var rect = ComputeUnconstrainedRegion(sourceSize);
            Relative = false;
            Anchor = anchor;
            _bounds = new Rectangle {
                X = PositionToOffset(rect.X, rect.Width, sourceSize.Width, GetHorizontalEdge(anchor)),
                Y = PositionToOffset(rect.Y, rect.Height, sourceSize.Height, GetVerticalEdge(anchor)),
                Width = rect.Width,
                Height = rect.Height
            };
        }

        /// <summary>
        /// Switches the region to the same mode (relative or anchor) as another region, keeping
        /// the region at the same place inside a source thumbnail of a given size.
        /// </summary>
        /// <param name="other">Region whose mode is adopted.</param>
        /// <param name="sourceSize">Size of the full thumbnail source.</param>
        public void SwitchToModeOf(ThumbnailRegion other, Size sourceSize) {
            if (other.Relative)
                SwitchToRelative(sourceSize);
            else
                SwitchToAnchor(other.Anchor, sourceSize);
        }

        public override string ToString() {
            return string.Format("({0}, {1})", _bounds, (Relative) ? "relative" : "absolute " + Anchor);
        }

    }

}
