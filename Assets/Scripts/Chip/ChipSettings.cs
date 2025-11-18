using System;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Kdevaulo.Match3
{
    [Serializable]
    public struct ChipVisual
    {
        public Chip Type;
        public Sprite Sprite;
    }

    [CreateAssetMenu(menuName = nameof(Match3) + "/" + nameof(ChipSettings), fileName = nameof(ChipSettings))]
    public class ChipSettings : ScriptableObject
    {
        [field: Header("References")]
        [field: SerializeField] public ChipView ChipViewPrefab { get; private set; }

        [SerializeField] private ChipVisual[] _chips;

        public Chip GetRandomType()
        {
            if (_chips == null || _chips.Length == 0)
                return Chip.None;

            var index = Random.Range(0, _chips.Length);
            return _chips[index].Type;
        }

        public Sprite GetSprite(Chip type)
        {
            if (_chips == null)
                return null;

            for (var i = 0; i < _chips.Length; i++)
            {
                if (_chips[i].Type == type)
                    return _chips[i].Sprite;
            }

            return null;
        }
    }
}