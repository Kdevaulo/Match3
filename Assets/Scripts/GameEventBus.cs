using System;

using UnityEngine;

namespace Kdevaulo.Match3
{
    public struct PlayerSwapPerformedEvent
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public PlayerSwapPerformedEvent(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }
    }

    public class GameEventBus
    {
        public event Action<PlayerSwapPerformedEvent> PlayerSwapPerformed;

        public void Publish(PlayerSwapPerformedEvent evt)
        {
            PlayerSwapPerformed?.Invoke(evt);
        }
    }
}