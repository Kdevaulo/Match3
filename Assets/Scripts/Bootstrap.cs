using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private RectTransform _chipsContainer;
        [SerializeField] private ChipSettings _chipSettings;
        [SerializeField] private GridSettings _gridSettings;

        private ChipController _chipController;
        private MatchFinder _matchFinder;
        private GridModel _gridModel;
        private GameLoop _gameLoop;
        private GridRefillHandler _gridRefillHandler;

        private void Awake()
        {
            _gridModel = new GridModel(_gridSettings.GridSize);

            _chipController = new ChipController(_chipsContainer, _gridSettings, _chipSettings, _gridModel);
            _matchFinder = new MatchFinder(_gridModel);
            _gridRefillHandler = new GridRefillHandler(_gridModel, () => _chipSettings.GetRandomType());
            _gameLoop = new GameLoop(_gridModel, _matchFinder, _chipController, _gridRefillHandler);
        }

        private void Start()
        {
            StartCoroutine(_gameLoop.HandleGameLoop());
        }
    }
}