using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CollieMollie.Audio;
using CollieMollie.Helper;
using CollieMollie.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery.Gameboy
{
    public class GameboyAxisButton : MonoBehaviour
    {
        #region Variable Field
        public event Action OnPressed = null;

        [Header("Gameboy Axis Button")]
        [SerializeField] private UIButton _button = null;
        [SerializeField] private Transform _axisButton = null;
        [SerializeField] private Vector2 _dir = Vector2.zero;
        [SerializeField] private float _travelDistance = 0.3f;
        [SerializeField] private float _travelDuration = 0.6f;

        [Space]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;
        [SerializeField] private AudioPreset _audioPreset = null;

        private Quaternion _defaultRotation = Quaternion.identity;
        private Task _buttonTask = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion

        private void Awake()
        {
            _button.OnDefault += (eventArgs) => InvokeDefaultAction();
            _button.OnPressed += (eventArgs) => InvokeDownAction();
        }

        private void Start()
        {
            _defaultRotation = transform.localRotation;
        }

        #region Interaction Publishers
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
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            _buttonTask = PushButtonAsync(_defaultRotation, _cts.Token);
        }

        private void PressedButton()
        {
            Quaternion pressedRotation = _dir switch
            {
                Vector2 dir when dir.y > 0 => _defaultRotation * Quaternion.Euler(-Vector3.forward * _travelDistance),
                Vector2 dir when dir.y < 0 => _defaultRotation * Quaternion.Euler(Vector3.forward * _travelDistance),
                Vector2 dir when dir.x < 0 => _defaultRotation * Quaternion.Euler(Vector3.up * _travelDistance),
                Vector2 dir when dir.x > 0 => _defaultRotation * Quaternion.Euler(-Vector3.up * _travelDistance),
                _ => Quaternion.identity
            };

            _cts.Cancel();
            _cts = new CancellationTokenSource();

            _buttonTask = PushButtonAsync(pressedRotation, _cts.Token, () =>
            {
                _audioEventChannel.RaisePlayAudioEvent(_audioPreset);
                OnPressed?.Invoke();
            });
        }

        private async Task PushButtonAsync(Quaternion targetRotation, CancellationToken token, Action done = null)
        {
            await _axisButton.LerpLocalRotationAsync(targetRotation, _travelDuration, token);
            done?.Invoke();
        }
        #endregion
    }
}
