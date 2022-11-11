using System;
using System.Collections;
using System.Collections.Generic;
using CollieMollie.Audio;
using CollieMollie.Core;
using CollieMollie.Helper;
using CollieMollie.Interactable;
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
        private IEnumerator _currentButtonAction = null;
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
            if (_currentButtonAction != null)
                StopCoroutine(_currentButtonAction);

            _currentButtonAction = PushButton(_defaultPosition);
            StartCoroutine(_currentButtonAction);
        }

        private void PressedButton()
        {
            if (_currentButtonAction != null)
                StopCoroutine(_currentButtonAction);

            Vector3 pressedPosition = _defaultPosition;
            pressedPosition.x += _travelDistance;
            _currentButtonAction = PushButton(pressedPosition, () =>
            {
                _audioEventChannel.RaisePlayAudioEvent(_audioPreset);
                OnPressed?.Invoke();
            });
            StartCoroutine(_currentButtonAction);
        }

        IEnumerator PushButton(Vector3 targetPosition, Action done = null)
        {
            yield return transform.LerpLocalPosition(targetPosition, _travelDuration);
            done?.Invoke();
        }
        #endregion
    }
}
