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
    public class GameboyAxisButton : BaseInteractable
    {
        #region Variable Field
        public event Action OnPressed = null;

        [Header("Gameboy Axis Button")]
        [SerializeField] private Transform _axisButton = null;
        [SerializeField] private Vector2 _dir = Vector2.zero;
        [SerializeField] private float _travelDistance = 0.3f;
        [SerializeField] private float _travelDuration = 0.6f;

        [Space]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;
        [SerializeField] private AudioPreset _audioPreset = null;

        private Quaternion _defaultRotation = Quaternion.identity;
        private IEnumerator _currentButtonAction = null;
        #endregion

        private void Start()
        {
            _defaultRotation = transform.localRotation;
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

            _currentButtonAction = PushButton(_defaultRotation);
            StartCoroutine(_currentButtonAction);
        }

        private void PressedButton()
        {
            if (_currentButtonAction != null)
                StopCoroutine(_currentButtonAction);

            Quaternion pressedRotation = _dir switch
            {
                Vector2 dir when dir.y > 0 => _defaultRotation * Quaternion.Euler(-Vector3.forward * _travelDistance),
                Vector2 dir when dir.y < 0 => _defaultRotation * Quaternion.Euler(Vector3.forward * _travelDistance),
                Vector2 dir when dir.x < 0 => _defaultRotation * Quaternion.Euler(Vector3.up * _travelDistance),
                Vector2 dir when dir.x > 0 => _defaultRotation * Quaternion.Euler(-Vector3.up * _travelDistance),
                _ => Quaternion.identity
            };

            _currentButtonAction = PushButton(pressedRotation, () =>
            {
                _audioEventChannel.RaisePlayAudioEvent(_audioPreset);
                OnPressed?.Invoke();
            });
            StartCoroutine(_currentButtonAction);
        }

        IEnumerator PushButton(Quaternion targetRotation, Action done = null)
        {
            yield return _axisButton.LerpLocalRotation(targetRotation, _travelDuration);
            done?.Invoke();
        }
        #endregion
    }
}
