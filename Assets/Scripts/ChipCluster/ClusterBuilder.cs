using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class ClusterBuilder
    {
        private readonly ClusterShapeAnalyzer _shapeAnalyzer;

        public ClusterBuilder(ClusterShapeAnalyzer shapeAnalyzer)
        {
            _shapeAnalyzer = shapeAnalyzer;
        }

        public List<Cluster> BuildClusters(List<Segment> horizontalSegments, List<Segment> verticalSegments)
        {
            var result = new List<Cluster>();
            var allSegments = new List<Segment>(horizontalSegments.Count + verticalSegments.Count);

            allSegments.AddRange(horizontalSegments);
            allSegments.AddRange(verticalSegments);

            var visited = new bool[allSegments.Count];

            for (var i = 0; i < allSegments.Count; i++)
            {
                if (visited[i])
                    continue;

                var clusterSegments = CollectConnectedSegments(allSegments, visited, i);
                var cells = BuildClusterCells(clusterSegments);

                if (cells.Count == 0)
                    continue;

                var shape = _shapeAnalyzer.AnalyzeShape(clusterSegments, cells);

                result.Add(new Cluster(clusterSegments[0].Type, shape, cells));
            }

            return result;
        }

        private List<Segment> CollectConnectedSegments(List<Segment> allSegments, bool[] visited, int startIndex)
        {
            var queue = new Queue<int>();
            var clusterSegments = new List<Segment>();

            queue.Enqueue(startIndex);
            visited[startIndex] = true;

            while (queue.Count > 0)
            {
                var idx = queue.Dequeue();
                var s = allSegments[idx];
                clusterSegments.Add(s);

                for (var j = 0; j < allSegments.Count; j++)
                {
                    if (visited[j])
                        continue;

                    if (!s.IsIntersect(allSegments[j]))
                        continue;

                    visited[j] = true;
                    queue.Enqueue(j);
                }
            }

            return clusterSegments;
        }

        private List<Vector2Int> BuildClusterCells(List<Segment> clusterSegments)
        {
            var cellsSet = new HashSet<Vector2Int>();

            foreach (var segment in clusterSegments)
            {
                AddSegmentCells(segment, cellsSet);
            }

            if (cellsSet.Count == 0)
                return new List<Vector2Int>();

            var cells = new List<Vector2Int>(cellsSet);
            cells.Sort((a, b) =>
            {
                var cmpY = a.y.CompareTo(b.y);
                return cmpY != 0 ? cmpY : a.x.CompareTo(b.x);
            });

            return cells;
        }

        private void AddSegmentCells(Segment segment, HashSet<Vector2Int> cellsSet)
        {
            if (segment.Orientation == SegmentOrientation.Horizontal)
            {
                for (var x = segment.X1; x <= segment.X2; x++)
                {
                    cellsSet.Add(new Vector2Int(x, segment.Y1));
                }
            }
            else
            {
                for (var y = segment.Y1; y <= segment.Y2; y++)
                {
                    cellsSet.Add(new Vector2Int(segment.X1, y));
                }
            }
        }
    }
}