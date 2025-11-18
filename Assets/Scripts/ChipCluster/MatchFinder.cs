using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public enum SegmentOrientation
    {
        Horizontal,
        Vertical
    }

    public readonly struct Segment
    {
        public SegmentOrientation Orientation { get; }
        public Chip Type { get; }
        public int X1 { get; }
        public int Y1 { get; }
        public int X2 { get; }
        public int Y2 { get; }
        public int Length => Orientation == SegmentOrientation.Horizontal
            ? X2 - X1 + 1
            : Y2 - Y1 + 1;

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

    public class MatchFinder
    {
        private readonly GridModel _board;

        public MatchFinder(GridModel grid)
        {
            _board = grid;
        }

        public List<Cluster> FindMatches(bool checkDirtyOnly = true)
        {
            var result = new List<Cluster>();

            var width = _board.Width;
            var height = _board.Height;

            var horizontalSegments = new List<Segment>();
            var verticalSegments = new List<Segment>();

            var dirtyRows = checkDirtyOnly ? _board.DirtyRows : null;
            var dirtyCols = checkDirtyOnly ? _board.DirtyColumns : null;

            // 1. Горизонтальные отрезки 3+
            for (var y = 0; y < height; y++)
            {
                if (checkDirtyOnly && !dirtyRows[y])
                    continue;

                var x = 0;

                while (x < width)
                {
                    var type = _board.GetChip(x, y);

                    if (type == Chip.None)
                    {
                        x++;
                        continue;
                    }

                    var startX = x;
                    var endX = x;

                    x++;

                    while (x < width && _board.GetChip(x, y) == type)
                    {
                        endX = x;
                        x++;
                    }

                    var length = endX - startX + 1;

                    if (length >= 3)
                    {
                        horizontalSegments.Add(new Segment(
                            SegmentOrientation.Horizontal,
                            type,
                            startX, y,
                            endX, y));
                    }
                }
            }

            // 2. Вертикальные отрезки 3+
            for (var x = 0; x < width; x++)
            {
                if (checkDirtyOnly && !dirtyCols[x])
                    continue;

                var y = 0;

                while (y < height)
                {
                    var type = _board.GetChip(x, y);

                    if (type == Chip.None)
                    {
                        y++;
                        continue;
                    }

                    var startY = y;
                    var endY = y;

                    y++;

                    while (y < height && _board.GetChip(x, y) == type)
                    {
                        endY = y;
                        y++;
                    }

                    var length = endY - startY + 1;

                    if (length >= 3)
                    {
                        verticalSegments.Add(new Segment(
                            SegmentOrientation.Vertical,
                            type,
                            x, startY,
                            x, endY));
                    }
                }
            }

            if (horizontalSegments.Count == 0 && verticalSegments.Count == 0)
                return result;

            // 3. Объединяем отрезки в кластеры по пересечениям
            var allSegments = new List<Segment>(horizontalSegments.Count + verticalSegments.Count);
            allSegments.AddRange(horizontalSegments);
            allSegments.AddRange(verticalSegments);

            var visited = new bool[allSegments.Count];

            for (var i = 0; i < allSegments.Count; i++)
            {
                if (visited[i])
                    continue;

                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited[i] = true;

                var clusterSegments = new List<Segment>();

                while (queue.Count > 0)
                {
                    var idx = queue.Dequeue();
                    var s = allSegments[idx];
                    clusterSegments.Add(s);

                    for (var j = 0; j < allSegments.Count; j++)
                    {
                        if (visited[j])
                            continue;

                        var other = allSegments[j];

                        if (s.Type != other.Type)
                            continue;

                        if (SegmentsIntersect(s, other))
                        {
                            visited[j] = true;
                            queue.Enqueue(j);
                        }
                    }
                }

                // 4. Собираем уникальные клетки кластера
                var cellsSet = new HashSet<Vector2Int>();

                foreach (var seg in clusterSegments)
                {
                    if (seg.Orientation == SegmentOrientation.Horizontal)
                    {
                        for (var x = seg.X1; x <= seg.X2; x++)
                        {
                            cellsSet.Add(new Vector2Int(x, seg.Y1));
                        }
                    }
                    else
                    {
                        for (var y = seg.Y1; y <= seg.Y2; y++)
                        {
                            cellsSet.Add(new Vector2Int(seg.X1, y));
                        }
                    }
                }

                if (cellsSet.Count == 0)
                    continue;

                var cells = new List<Vector2Int>(cellsSet);
                cells.Sort((a, b) =>
                {
                    var cmpY = a.y.CompareTo(b.y);
                    return cmpY != 0 ? cmpY : a.x.CompareTo(b.x);
                });

                var shape = AnalyzeShape(clusterSegments, cells);

                result.Add(new Cluster(clusterSegments[0].Type, shape, cells));
            }

            return result;
        }

        private bool SegmentsIntersect(Segment a, Segment b)
        {
            if (a.Orientation == SegmentOrientation.Horizontal &&
                b.Orientation == SegmentOrientation.Horizontal)
            {
                if (a.Y1 != b.Y1)
                    return false;

                return a.X1 <= b.X2 && b.X1 <= a.X2;
            }

            if (a.Orientation == SegmentOrientation.Vertical &&
                b.Orientation == SegmentOrientation.Vertical)
            {
                if (a.X1 != b.X1)
                    return false;

                return a.Y1 <= b.Y2 && b.Y1 <= a.Y2;
            }

            // Пересечение горизонтального и вертикального
            var hSegment = a.Orientation == SegmentOrientation.Horizontal ? a : b;
            var vSegment = a.Orientation == SegmentOrientation.Vertical ? a : b;

            var isXInside = vSegment.X1 >= hSegment.X1 && vSegment.X1 <= hSegment.X2;
            var isYInside = hSegment.Y1 >= vSegment.Y1 && hSegment.Y1 <= vSegment.Y2;

            return isXInside && isYInside;
        }

        private ClusterShape AnalyzeShape(List<Segment> segments, List<Vector2Int> cells)
        {
            var hasHorizontal = false;
            var hasVertical = false;

            var maxHorizLen = 0;
            var maxVertLen = 0;

            foreach (var segment in segments)
            {
                if (segment.Orientation == SegmentOrientation.Horizontal)
                {
                    hasHorizontal = true;
                    if (segment.Length > maxHorizLen)
                        maxHorizLen = segment.Length;
                }
                else
                {
                    hasVertical = true;
                    if (segment.Length > maxVertLen)
                        maxVertLen = segment.Length;
                }
            }

            // Только линия (одна ориентация)
            if (!hasHorizontal || !hasVertical)
                return ClusterShape.Line;

            // Есть и горизонтали, и вертикали — возможны L / T / Cross.
            var intersections = FindIntersections(segments);

            foreach (var center in intersections)
            {
                AnalyzeArms(center, segments[0].Type,
                    out var left, out var right, out var up, out var down);

                var dirs =
                    (left > 0 ? 1 : 0) +
                    (right > 0 ? 1 : 0) +
                    (up > 0 ? 1 : 0) +
                    (down > 0 ? 1 : 0);

                var horizLen = left + 1 + right;
                var vertLen = up + 1 + down;

                // X / крест
                if (left > 0 && right > 0 && up > 0 && down > 0 &&
                    horizLen >= 3 && vertLen >= 3)
                {
                    return ClusterShape.Cross;
                }

                // T-образная: три луча от центра
                if (dirs == 3 && (horizLen >= 3 || vertLen >= 3))
                {
                    return ClusterShape.T;
                }

                // Г-образная: два луча под прямым углом
                if (dirs == 2)
                {
                    var horiz = left > 0 || right > 0;
                    var vert = up > 0 || down > 0;

                    if (horiz && vert && (horizLen >= 3 || vertLen >= 3))
                    {
                        return ClusterShape.L;
                    }
                }
            }

            // Сюда попадут более экзотические объединения (но это всё равно матч)
            return ClusterShape.Complex;
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

                    if (!SegmentsIntersect(a, b))
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

        private void AnalyzeArms(Vector2Int center, Chip type,
            out int left, out int right, out int up, out int down)
        {
            left = right = up = down = 0;

            // left
            for (var x = center.x - 1; x >= 0; x--)
            {
                if (_board.GetChip(x, center.y) != type)
                    break;

                left++;
            }

            // right
            for (var x = center.x + 1; x < _board.Width; x++)
            {
                if (_board.GetChip(x, center.y) != type)
                    break;

                right++;
            }

            // down
            for (var y = center.y - 1; y >= 0; y--)
            {
                if (_board.GetChip(center.x, y) != type)
                    break;

                down++;
            }

            // up
            for (var y = center.y + 1; y < _board.Height; y++)
            {
                if (_board.GetChip(center.x, y) != type)
                    break;

                up++;
            }
        }
    }
}