using UnityEngine;
using UnityEngine.UI;

namespace Kdevaulo.Match3
{
    public class ChipView : MonoBehaviour
    {
        [field: SerializeField] public ChipInput Input { get; private set; }
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [SerializeField] private Image _icon;

        public Vector2Int Cell { get; private set; }

        public void SetSprite(Sprite sprite)
        {
            _icon.sprite = sprite;
        }

        public void SetColor(Color color)
        {
            _icon.color = color;
        }

        public void SetCell(Vector2Int cell)
        {
            Cell = cell;
        }
    }
}