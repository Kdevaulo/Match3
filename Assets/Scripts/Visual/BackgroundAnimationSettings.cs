using System;

using UnityEngine;

namespace Kdevaulo.Match3
{
    [Serializable]
    public struct HorizontalMoveSettings
    {
        public string Key;
        public float Speed;
        public float PositionY;
        public Vector2 PositionRange;
    }

    [Serializable]
    public struct ScaleAnimationSettings
    {
        public string Key;
        public float Speed;
        public Vector2 ScaleRange;
    }

    [Serializable]
    public struct RotationAnimationSettings
    {
        public string Key;
        public float Speed;
    }

    [CreateAssetMenu(menuName = nameof(Match3) + "/" + nameof(BackgroundAnimationSettings),
        fileName = nameof(BackgroundAnimationSettings))]
    public class BackgroundAnimationSettings : ScriptableObject
    {
        [field: SerializeField] public HorizontalMoveSettings[] HorizontalMoves { get; private set; }
        [field: SerializeField] public ScaleAnimationSettings[] ScaleAnimations { get; private set; }
        [field: SerializeField] public RotationAnimationSettings[] RotationAnimations { get; private set; }
    }
}