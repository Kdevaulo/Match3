using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class ClusterShapeAnalyzer
    {
        private const int MinMatchLength = 3;

        public ClusterShape AnalyzeShape(List<Segment> segments, List<Vector2Int> cells)
        {
            AnalyzeOrientations(segments, out var hasHorizontal, out var hasVertical);

            if (!hasHorizontal || !hasVertical)
                return ClusterShape.Line;

            var intersections = FindIntersections(segments);

            if (intersections.Count == 0)
                return ClusterShape.Complex;

            var cellSet = new HashSet<Vector2Int>(cells);

            return AnalyzeComplexShape(intersections, cellSet);
        }

        private void AnalyzeOrientations(List<Segment> segments, out bool hasHorizontal, out bool hasVertical)
        {
            hasHorizontal = false;
            hasVertical = false;

            foreach (var segment in segments)
            {
                if (segment.Orientation == SegmentOrientation.Horizontal)
                    hasHorizontal = true;
                else
                    hasVertical = true;

                if (hasHorizontal && hasVertical)
                    break;
            }
        }

        private ClusterShape AnalyzeComplexShape(List<Vector2Int> intersections, HashSet<Vector2Int> cellSet)
        {
            foreach (var center in intersections)
            {
                AnalyzeArms(center, cellSet,
                    out var left,
                    out var right,
                    out var up,
                    out var down);

                var dirs = (left > 0 ? 1 : 0) +
                           (right > 0 ? 1 : 0) +
                           (up > 0 ? 1 : 0) +
                           (down > 0 ? 1 : 0);

                var horizLen = left + 1 + right;
                var vertLen = up + 1 + down;

                if (IsCross(left, right, up, down, horizLen, vertLen))
                    return ClusterShape.Cross;

                if (IsTShape(dirs, horizLen, vertLen))
                    return ClusterShape.T;

                if (IsLShape(left, right, up, down, dirs, horizLen, vertLen))
                    return ClusterShape.L;
            }

            return ClusterShape.Complex;
        }

        private void AnalyzeArms(Vector2Int center, HashSet<Vector2Int> cellSet,
            out int left,
            out int right,
            out int up,
            out int down)
        {
            left = CountInDirection(center, cellSet, -1, 0);
            right = CountInDirection(center, cellSet, 1, 0);
            down = CountInDirection(center, cellSet, 0, -1);
            up = CountInDirection(center, cellSet, 0, 1);
        }

        private int CountInDirection(Vector2Int center, HashSet<Vector2Int> cellSet, int dx, int dy)
        {
            var count = 0;

            var pos = new Vector2Int(center.x + dx, center.y + dy);

            while (cellSet.Contains(pos))
            {
                count++;

                pos.x += dx;
                pos.y += dy;
            }

            return count;
        }

        private bool IsCross(int left, int right, int up, int down,
            int horizLen,
            int vertLen)
        {
            return left > 0 && right > 0 && up > 0 && down > 0 &&
                   horizLen >= MinMatchLength && vertLen >= MinMatchLength;
        }

        private bool IsTShape(int dirs,
            int horizLen, int vertLen)
        {
            return dirs == 3 && (horizLen >= MinMatchLength || vertLen >= MinMatchLength);
        }

        private bool IsLShape(int left, int right, int up, int down,
            int dirs,
            int horizLen,
            int vertLen)
        {
            if (dirs != 2)
                return false;

            var horiz = left > 0 || right > 0;
            var vert = up > 0 || down > 0;

            return horiz && vert && (horizLen >= MinMatchLength || vertLen >= MinMatchLength);
        }

        private List<Vector2Int> FindIntersections(List<Segment> segments)
        {
            var result = new List<Vector2Int>();

            for (var i = 0; i < segments.Count; i++)
            {
                for (var j = i + 1; j < segments.Count; j++)
                {
                    var a = segments[i];
                    var b = segments[j];

                    if (!a.IsIntersect(b))
                        continue;

                    if (a.Orientation == SegmentOrientation.Horizontal &&
                        b.Orientation == SegmentOrientation.Vertical)
                    {
                        var x = b.X1;
                        var y = a.Y1;
                        result.Add(new Vector2Int(x, y));
                    }
                    else if (a.Orientation == SegmentOrientation.Vertical &&
                             b.Orientation == SegmentOrientation.Horizontal)
                    {
                        var x = a.X1;
                        var y = b.Y1;
                        result.Add(new Vector2Int(x, y));
                    }
                }
            }

            return result;
        }
    }
}