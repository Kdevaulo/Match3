using System.Collections.Generic;

namespace Kdevaulo.Match3
{
    public class SegmentFinder
    {
        private const int MinMatchLength = 3;

        private readonly GridModel _board;

        public SegmentFinder(GridModel board)
        {
            _board = board;
        }

        public List<Segment> FindSegments(SegmentOrientation orientation, bool[] dirtyLines)
        {
            var result = new List<Segment>();

            var primaryLength = GetPrimaryLength(orientation);
            var secondaryLength = GetSecondaryLength(orientation);

            for (var lineIndex = 0; lineIndex < secondaryLength; lineIndex++)
            {
                if (dirtyLines != null && !dirtyLines[lineIndex])
                    continue;

                FindSegmentsOnLine(orientation, lineIndex, primaryLength, result);
            }

            return result;
        }

        private int GetPrimaryLength(SegmentOrientation orientation)
        {
            return orientation == SegmentOrientation.Horizontal
                ? _board.Width
                : _board.Height;
        }

        private int GetSecondaryLength(SegmentOrientation orientation)
        {
            return orientation == SegmentOrientation.Horizontal
                ? _board.Height
                : _board.Width;
        }

        private void FindSegmentsOnLine(SegmentOrientation orientation, int lineIndex, int primaryLength,
            List<Segment> result)
        {
            var primary = 0;

            while (primary < primaryLength)
            {
                var type = GetChipByOrientation(orientation, primary, lineIndex);

                if (type == Chip.None)
                {
                    primary++;
                    continue;
                }

                var start = primary;
                var end = primary;

                while (end + 1 < primaryLength)
                {
                    var nextType = GetChipByOrientation(orientation, end + 1, lineIndex);
                    if (nextType != type)
                        break;

                    end++;
                }

                primary = end + 1;

                var length = end - start + 1;

                if (length >= MinMatchLength)
                {
                    var segment = CreateSegment(orientation, type, start, end, lineIndex);
                    result.Add(segment);
                }
            }
        }

        private Chip GetChipByOrientation(SegmentOrientation orientation, int primaryIndex, int secondaryIndex)
        {
            if (orientation == SegmentOrientation.Horizontal)
                return _board.GetChip(primaryIndex, secondaryIndex);

            return _board.GetChip(secondaryIndex, primaryIndex);
        }

        private Segment CreateSegment(SegmentOrientation orientation, Chip type,
            int start, int end, int lineIndex)
        {
            if (orientation == SegmentOrientation.Horizontal)
            {
                return new Segment(SegmentOrientation.Horizontal,
                    type,
                    start, lineIndex,
                    end, lineIndex);
            }

            return new Segment(SegmentOrientation.Vertical,
                type,
                lineIndex, start,
                lineIndex, end);
        }
    }
}