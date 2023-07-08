using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gallery.BigMacOS
{
    public class CursorController : MonoBehaviour
    {
        #region Variable Field
        [SerializeField] private PlayerInput _playerInput = null;

        [SerializeField] private Image _cursor = null;
        [SerializeField] private Image _interactiveCursor = null;
        [SerializeField] private float _followSpeed = 0.3f;

        [SerializeField] private float _awakeDuration = 0.2f;
        [SerializeField] private float _sleepDuration = 0.6f;
        [SerializeField] private float _autoSleepWaitTime = 6f;

        private BigMacInput _bigMacInput = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private float _autoSleepElapsedTime = 0;
        private bool _sleep = false;
        #endregion

        private void Awake()
        {
            Cursor.visible = false;

            _bigMacInput = new BigMacInput();
        }

        private void OnEnable()
        {
            _bigMacInput.UI.Enable();
        }

        private void OnDisable()
        {
            _bigMacInput.UI.Disable();
        }

        private void Update()
        {
            UpdateCursorPosition();
            AutoSleep();
        }

        #region Private Functions
        private void UpdateCursorPosition()
        {
            _cursor.transform.position = _bigMacInput.UI.Point.ReadValue<Vector2>();
            _interactiveCursor.transform.position = Vector2.Lerp(_interactiveCursor.transform.position, _cursor.transform.position, _followSpeed);
        }

        private void AutoSleep()
        {
            if (_bigMacInput.UI.Point.triggered)
            {
                if (!_sleep) return;
                _sleep = false;
                _autoSleepElapsedTime = 0f;

                _cts.Cancel();
                _cts = new CancellationTokenSource();
                List<Task> fadeTasks = new List<Task>();
                fadeTasks.Add(Fade(_cursor, 1, _awakeDuration, _cts.Token));
                fadeTasks.Add(Fade(_interactiveCursor, 1, _awakeDuration, _cts.Token));
            }
            else
            {
                if (_sleep) return;
                _autoSleepElapsedTime += Time.deltaTime;
                if (_autoSleepElapsedTime > _autoSleepWaitTime)
                {
                    _sleep = true;
                    _cts.Cancel();
                    _cts = new CancellationTokenSource();
                    List<Task> fadeTasks = new List<Task>();
                    fadeTasks.Add(Fade(_cursor, 0, _sleepDuration, _cts.Token));
                    fadeTasks.Add(Fade(_interactiveCursor, 0, _sleepDuration, _cts.Token));
                }
            }

            async Task Fade(Image cursor, float targetValue, float duration, CancellationToken token)
            {
                Color targetColor = cursor.color;
                targetColor.a = targetValue;
                await cursor.LerpColorAsync(targetColor, duration, _cts.Token);
            }
        }
        #endregion
    }
}
