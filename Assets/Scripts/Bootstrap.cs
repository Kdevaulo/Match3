using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private RectTransform _chipsContainer;
        [SerializeField] private ChipSettings _chipSettings;
        [SerializeField] private GridSettings _gridSettings;

        private GridRefillHandler _gridRefillHandler;
        private ChipController _chipController;
        private GameEventBus _eventBus;
        private MatchFinder _matchFinder;
        private GridModel _gridModel;
        private GameLoop _gameLoop;

        private void Awake()
        {
            _gridModel = new GridModel(_gridSettings.GridSize);

            _eventBus = new GameEventBus();

            _chipController = new ChipController(_chipsContainer, _gridSettings, _chipSettings, _gridModel, _eventBus);
            _matchFinder = new MatchFinder(_gridModel);
            _gridRefillHandler = new GridRefillHandler(_gridModel, _chipSettings);
            _gameLoop = new GameLoop(_gridModel, _matchFinder, _chipController, _gridRefillHandler, _eventBus);
        }

        private void Start()
        {
            StartCoroutine(_gameLoop.HandleGameLoop());
        }
    }
}