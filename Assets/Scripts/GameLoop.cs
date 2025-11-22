using System;
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
        private readonly GameEventBus _eventBus;

        private readonly WaitForSeconds _delay;

        private bool _playerMoveRequested;

        public GameLoop(GridModel gridModel, MatchFinder matchFinder, ChipController chipController,
            GridRefillHandler gridRefillHandler, GameEventBus eventBus)
        {
            _gridRefillHandler = gridRefillHandler;
            _chipController = chipController;
            _matchFinder = matchFinder;
            _gridModel = gridModel;
            _eventBus = eventBus;

            _delay = new WaitForSeconds(0.5f);

            _eventBus.PlayerSwapPerformed += OnPlayerSwapPerformed;
        }

        public IEnumerator HandleGameLoop()
        {
            _chipController.SpawnGrid();

            yield return HandleMatches(false);

            while (true)
            {
                if (_playerMoveRequested)
                {
                    _playerMoveRequested = false;
                    yield return HandleMatches(true);
                }

                yield return null;
            }
        }

        private void OnPlayerSwapPerformed(PlayerSwapPerformedEvent evt)
        {
            _playerMoveRequested = true;
        }

        private IEnumerator HandleMatches(bool checkDirtyOnly)
        {
            while (true)
            {
                var matches = _matchFinder.FindMatches(checkDirtyOnly);

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