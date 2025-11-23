namespace Kdevaulo.Match3
{
    public class MoveFinder
    {
        private readonly GridModel _grid;
        private readonly MatchFinder _matchFinder;

        public MoveFinder(GridModel grid, MatchFinder matchFinder)
        {
            _grid = grid;
            _matchFinder = matchFinder;
        }

        public bool HasAnyMove()
        {
            var width = _grid.Width;
            var height = _grid.Height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var chip = _grid.GetChip(x, y);
                    if (chip == Chip.None)
                        continue;

                    if (TrySwapAndCheck(x, y, x + 1, y))
                        return true;

                    if (TrySwapAndCheck(x, y, x, y + 1))
                        return true;
                }
            }

            return false;
        }

        private bool TrySwapAndCheck(int x1, int y1, int x2, int y2)
        {
            var width = _grid.Width;
            var height = _grid.Height;

            if (x2 < 0 || x2 >= width || y2 < 0 || y2 >= height)
                return false;

            var chip2 = _grid.GetChip(x2, y2);
            if (chip2 == Chip.None)
                return false;

            _grid.SwapChips(x1, y1, x2, y2);

            var matches = _matchFinder.FindMatches(true);

            _grid.SwapChips(x1, y1, x2, y2);
            _grid.ClearDirty();

            return matches.Count > 0;
        }
    }
}