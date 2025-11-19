using System.Collections.Generic;

namespace Kdevaulo.Match3
{
    public class MatchFinder
    {
        private readonly GridModel _board;
        private readonly SegmentFinder _segmentFinder;
        private readonly ClusterBuilder _clusterBuilder;

        public MatchFinder(GridModel grid)
        {
            _board = grid;

            var shapeAnalyzer = new ClusterShapeAnalyzer();
            _segmentFinder = new SegmentFinder(_board);
            _clusterBuilder = new ClusterBuilder(shapeAnalyzer);
        }

        public List<Cluster> FindMatches(bool checkDirtyOnly = true)
        {
            var horizontalDirty = checkDirtyOnly ? _board.DirtyRows : null;
            var verticalDirty = checkDirtyOnly ? _board.DirtyColumns : null;

            var horizontalSegments =
                _segmentFinder.FindSegments(SegmentOrientation.Horizontal, horizontalDirty);

            var verticalSegments =
                _segmentFinder.FindSegments(SegmentOrientation.Vertical, verticalDirty);

            if (horizontalSegments.Count == 0 && verticalSegments.Count == 0)
                return new List<Cluster>();

            return _clusterBuilder.BuildClusters(horizontalSegments, verticalSegments);
        }
    }
}