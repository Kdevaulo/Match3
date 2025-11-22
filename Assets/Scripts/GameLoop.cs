using System.Collections;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public class GameLoop
    {
        private readonly ChipController _chipController;
        private readonly MatchFinder _matchFinder;
        private readonly GridModel _gridModel;
        private readonly GridRefillHandler _gridRefillHandler;

        private readonly WaitForSeconds _delay;

        public GameLoop(GridModel gridModel, MatchFinder matchFinder, ChipController chipController,
            GridRefillHandler gridRefillHandler)
        {
            _gridRefillHandler = gridRefillHandler;
            _chipController = chipController;
            _matchFinder = matchFinder;
            _gridModel = gridModel;

            _delay = new WaitForSeconds(0.5f);
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
                var matches = _matchFinder.FindMatches(true); // change to optimize calculation

                _gridModel.ClearDirty();

                if (matches.Count == 0)
                    yield break;

                yield return _delay;

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

                var moveParams = _gridRefillHandler.ApplyGravity();
                _chipController.MoveByGravity(moveParams);

                yield return _delay;

                var spawns = _gridRefillHandler.SpawnMissingChips();
                _chipController.SpawnMissingChipsAboveGrid(spawns);

                yield return _delay;

                _chipController.MoveSpawnedChipsDown();
            }
        }
    }
}