namespace Kdevaulo.Match3
{
    public enum SegmentOrientation
    {
        Horizontal,
        Vertical
    }

    public struct Segment
    {
        public SegmentOrientation Orientation { get; }
        public Chip Type { get; }
        public int X1 { get; }
        public int Y1 { get; }
        public int X2 { get; }
        public int Y2 { get; }

        public Segment(SegmentOrientation orientation, Chip type, int x1, int y1, int x2, int y2)
        {
            Orientation = orientation;
            Type = type;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }

    public static class SegmentExtensions
    {
        public static bool IsIntersect(this Segment a, Segment b)
        {
            if (a.Orientation == SegmentOrientation.Horizontal && b.Orientation == SegmentOrientation.Horizontal)
            {
                if (a.Y1 != b.Y1)
                    return false;

                return a.X1 <= b.X2 && b.X1 <= a.X2;
            }

            if (a.Orientation == SegmentOrientation.Vertical && b.Orientation == SegmentOrientation.Vertical)
            {
                if (a.X1 != b.X1)
                    return false;

                return a.Y1 <= b.Y2 && b.Y1 <= a.Y2;
            }

            var hSegment = a.Orientation == SegmentOrientation.Horizontal ? a : b;
            var vSegment = a.Orientation == SegmentOrientation.Vertical ? a : b;

            var isXInside = vSegment.X1 >= hSegment.X1 && vSegment.X1 <= hSegment.X2;
            var isYInside = hSegment.Y1 >= vSegment.Y1 && hSegment.Y1 <= vSegment.Y2;

            return isXInside && isYInside;
        }
    }
}