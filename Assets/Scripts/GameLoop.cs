using System.Collections;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GameLoop
    {
        private readonly GridModel _gridModel;
        private readonly MatchFinder _matchFinder;
        private readonly ChipController _chipController;

        public GameLoop(GridModel gridModel, MatchFinder matchFinder, ChipController chipController)
        {
            _gridModel = gridModel;
            _matchFinder = matchFinder;
            _chipController = chipController;
        }

        public IEnumerator HandleGameLoop()
        {
            _chipController.SpawnGrid();

            yield return HandleInitialMatches();
        }

        private IEnumerator HandleInitialMatches()
        {
            var matches = _matchFinder.FindMatches(false);

            if (matches.Count == 0)
                yield break;

            foreach (var cluster in matches)
            {
                _chipController.HighlightCells(cluster.Cells, Color.green);
            }

            yield return new WaitForSeconds(1f);

            foreach (var cluster in matches)
            {
                _chipController.ClearCells(cluster.Cells);
            }
        }
    }
}