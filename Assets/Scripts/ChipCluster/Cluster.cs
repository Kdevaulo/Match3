using System.Collections.Generic;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Cluster
    {
        public Chip Type { get; }
        public ClusterShape Shape { get; }
        public IReadOnlyList<Vector2Int> Cells => _cells;

        private readonly List<Vector2Int> _cells;

        public Cluster(Chip type, ClusterShape shape, List<Vector2Int> cells)
        {
            Type = type;
            Shape = shape;
            _cells = cells;
        }
    }
}