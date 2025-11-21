using System.Collections;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GameLoop
    {
        private readonly ChipController _chipController;
        private readonly MatchFinder _matchFinder;
        private readonly GridModel _gridModel;

        private readonly WaitForSeconds _delay;

        public GameLoop(GridModel gridModel, MatchFinder matchFinder, ChipController chipController)
        {
            _gridModel = gridModel;
            _matchFinder = matchFinder;
            _chipController = chipController;

            _delay = new WaitForSeconds(1);
        }

        public IEnumerator HandleGameLoop()
        {
            _chipController.SpawnGrid();

            yield return HandleInitialMatches();
        }

        private IEnumerator HandleInitialMatches()
        {
            while (true)
            {
                var matches = _matchFinder.FindMatches(true);

                if (matches.Count == 0)
                    yield break;

                _gridModel.ClearDirty();

                foreach (var cluster in matches)
                {
                    _chipController.HighlightCells(cluster.Cells, Color.green);
                }

                yield return _delay;

                foreach (var cluster in matches)
                {
                    _chipController.ClearCells(cluster.Cells);
                }

                yield return _delay;

                _chipController.ApplyGravity();

                yield return _delay;

                _chipController.SpawnMissingChipsAboveGrid();

                yield return _delay;

                _chipController.MoveSpawnedChipsDown();
            }
        }
    }
}