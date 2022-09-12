using System;
using System.Collections;
using System.Collections.Generic;
using CollieMollie.Audio;
using CollieMollie.Helper;
using CollieMollie.Interactable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Museum.Gameboy
{
    public class GameboyButton : BaseInteractable
    {
        #region Variable Field
        public event Action OnPressed = null;

        [Header("Gameboy Button")]
        [SerializeField] private float _travelDistance = 0.3f;
        [SerializeField] private float _travelDuration = 0.6f;

        [Space]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;
        [SerializeField] private AudioPreset _audioPreset = null;

        private Vector3 _defaultPosition = Vector3.zero;
        private IEnumerator _currentButtonAction = null;
        #endregion

        private void Start()
        {
            _defaultPosition = transform.localPosition;
        }

        #region Interaction Publishers
        protected override void InvokeExitAction(PointerEventData eventData = null)
        {
            if (!_interactable) return;

            DefaultButton();
        }

        protected override void InvokeDownAction(PointerEventData eventData = null)
        {
            if (!_interactable) return;

            PressedButton();
        }

        protected override void InvokeUpAction(PointerEventData eventData = null)
        {
            if (!_interactable) return;

            DefaultButton();
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
