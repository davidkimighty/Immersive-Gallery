using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CollieMollie.Audio;
using CollieMollie.Helper;
using CollieMollie.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery.Gameboy
{
    public class GameboyButton : MonoBehaviour
    {
        #region Variable Field
        public event Action OnPressed = null;

        [Header("Gameboy Button")]
        [SerializeField] private UIButton _button = null;
        [SerializeField] private float _travelDistance = 0.3f;
        [SerializeField] private float _travelDuration = 0.6f;

        [Space]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;
        [SerializeField] private AudioPreset _audioPreset = null;

        private Vector3 _defaultPosition = Vector3.zero;
        private Task _buttonTask = null;
        #endregion

        private void Awake()
        {
            _button.OnDefault += (eventArgs) => InvokeDefaultAction();
            _button.OnPressed += (eventArgs) => InvokeDownAction();
        }

        private void Start()
        {
            _defaultPosition = transform.localPosition;
        }

        #region Subscribers
        private void InvokeDefaultAction()
        {
            DefaultButton();
        }

        private void InvokeDownAction()
        {
            PressedButton();
        }

        #endregion

        #region Button Behaviors
        private void DefaultButton()
        {
            _buttonTask = PushButtonAsync(_defaultPosition);
        }

        private void PressedButton()
        {
            Vector3 pressedPosition = _defaultPosition;
            pressedPosition.x += _travelDistance;
            _buttonTask = PushButtonAsync(pressedPosition, () =>
            {
                _audioEventChannel.RaisePlayAudioEvent(_audioPreset);
                OnPressed?.Invoke();
            });
        }

        private async Task PushButtonAsync(Vector3 targetPosition, Action done = null)
        {
            await transform.LerpLocalPositionAsync(targetPosition, _travelDuration);
            done?.Invoke();
        }
        #endregion
    }
}
