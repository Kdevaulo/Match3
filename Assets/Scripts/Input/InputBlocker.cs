using System.Collections.Generic;

using UnityEngine.EventSystems;

namespace Kdevaulo.Match3
{
    public class InputBlocker
    {
        private readonly HashSet<ChipInput> _inputs = new HashSet<ChipInput>();
        private bool _isInputEnabled = true;

        public bool IsInputEnabled => _isInputEnabled;

        public void Register(ChipInput input)
        {
            if (input == null)
                return;

            _inputs.Add(input);
        }

        public void Unregister(ChipInput input)
        {
            if (input == null)
                return;

            _inputs.Remove(input);
        }

        public void SetBlocked(bool blocked)
        {
            if (_isInputEnabled == !blocked)
                return;

            _isInputEnabled = !blocked;

            if (blocked)
            {
                foreach (var input in _inputs)
                {
                    input.ResetInputStateFromBlocker();
                }

                var es = EventSystem.current;

                if (es != null)
                {
                    es.SetSelectedGameObject(null);
                }
            }
        }
    }
}