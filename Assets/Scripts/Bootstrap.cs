using UnityEngine;

namespace Kdevaulo.Match3
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private BackgroundAnimationSettings _animationSettings;
        [SerializeField] private BackgroundAnimationView _animationView;
        [SerializeField] private RectTransform _chipsContainer;
        [SerializeField] private ChipSettings _chipSettings;
        [SerializeField] private GridSettings _gridSettings;

        private BackgroundAnimationController _animationController;
        private GameLoop _gameLoop;

        private void Awake()
        {
            _gameLoop = new GameLoop(_chipSettings, _gridSettings, _chipsContainer);
            _animationController = new BackgroundAnimationController(_animationSettings, _animationView);
        }

        private void Start()
        {
            StartCoroutine(_gameLoop.HandleGameLoop());
            StartCoroutine(_animationController.Animate());
        }
    }
}