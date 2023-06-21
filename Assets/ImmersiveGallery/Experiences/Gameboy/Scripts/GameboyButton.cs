using System;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Audio;
using Broccollie.Core;
using Broccollie.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery.Gameboy
{
    public class GameboyButton : MonoBehaviour
    {
        #region Variable Field
        public event Action OnPressed = null;

        [Header("Gameboy Button")]
        [SerializeField] private ButtonUI _button = null;
        [SerializeField] private float _travelDistance = 0.3f;
        [SerializeField] private float _travelDuration = 0.6f;

        [Space]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;
        [SerializeField] private AudioPreset _audioPreset = null;

        private Vector3 _defaultPosition = Vector3.zero;
        private Task _buttonTask = null;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        #endregion

        private void Awake()
        {
            _button.OnDefault += (sender, eventArgs) => InvokeDefaultAction();
            _button.OnPress += (sender, eventArgs) => InvokeDownAction();
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
            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();

            _buttonTask = PushButtonAsync(_defaultPosition, _tokenSource.Token);
        }

        private void PressedButton()
        {
            Vector3 pressedPosition = _defaultPosition;
            pressedPosition.x += _travelDistance;

            _tokenSource.Cancel();
            _tokenSource = new CancellationTokenSource();

            _buttonTask = PushButtonAsync(pressedPosition, _tokenSource.Token, () =>
            {
                _audioEventChannel.RaisePlayAudioEvent(_audioPreset);
                OnPressed?.Invoke();
            });
        }

        private async Task PushButtonAsync(Vector3 targetPosition, CancellationToken token, Action done = null)
        {
            await transform.LerpLocalPositionAsync(targetPosition, _travelDuration, token);
            done?.Invoke();
        }
        #endregion
    }
}
