using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery
{
    public class Showcaser : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public event Action OnBeginDrag = null;
        public event Action OnEndDrag = null;

        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private float _rotationDamping = 0.3f;
        [SerializeField] private float _resetWaitTime = 3f;
        [SerializeField] private float _resetTime = 0.3f;

        private bool _showcaseEnabled = false;
        private bool _isDragging = false;
        public bool IsDragging
        {
            get => _isDragging;
        }
        private bool _needsReset = false;
        private Transform _origin = null;
        private float _rotationVelocityX = 0f;
        private float _rotationVelocityY = 0f;
        private float _currentResetWaitTime = 0f;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _needsReset = _isDragging = true;
            OnBeginDrag?.Invoke();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            OnEndDrag?.Invoke();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!_showcaseEnabled) return;
            //if (eventData.pointerId != 2) return;

            float rotDeltaX = eventData.delta.x * _rotationSpeed * Time.deltaTime;
            float rotDeltaY = eventData.delta.y * _rotationSpeed * Time.deltaTime;

            _rotationVelocityX = Mathf.Lerp(_rotationVelocityX, rotDeltaX, Time.deltaTime);
            _rotationVelocityY = Mathf.Lerp(_rotationVelocityY, rotDeltaY, Time.deltaTime);

            transform.Rotate(Vector3.up, -_rotationVelocityX);
            transform.Rotate(Vector3.forward, -_rotationVelocityY);
        }

        #region Public Functions
        public void EnableShowcase(bool state, Transform origin)
        {
            _showcaseEnabled = state;
            _origin = origin;
        }

        public void ResetMovement()
        {
            _currentResetWaitTime = 0f;
            _rotationVelocityX = 0f;
            _rotationVelocityY = 0f;
        }

        #endregion

        private void Update()
        {
            if (transform == null || !_showcaseEnabled) return;

            VelocityXMovement();
            VelocityYMovement();
            BackToOrigin();
        }

        private void VelocityXMovement()
        {
            if (!_isDragging && !Mathf.Approximately(_rotationVelocityX, 0))
            {
                float deltaVelocity = Mathf.Min(
                    Mathf.Sign(_rotationVelocityX) * _rotationDamping * Time.deltaTime,
                    Mathf.Sign(_rotationVelocityX) * _rotationVelocityX
                );
                _rotationVelocityX -= deltaVelocity;
                transform.Rotate(Vector3.up, -_rotationVelocityX);
            }
        }

        private void VelocityYMovement()
        {
            if (!_isDragging && !Mathf.Approximately(_rotationVelocityY, 0))
            {
                float deltaVelocity = Mathf.Min(
                    Mathf.Sign(_rotationVelocityY) * _rotationDamping * Time.deltaTime,
                    Mathf.Sign(_rotationVelocityY) * _rotationVelocityY
                );
                _rotationVelocityY -= deltaVelocity;
                transform.Rotate(Vector3.forward, -_rotationVelocityY);
            }
        }

        private async void BackToOrigin()
        {
            if (CanReset())
            {
                _currentResetWaitTime += Time.deltaTime;
                if (_currentResetWaitTime < _resetWaitTime) return;
                
                try
                {
                    _cts.Cancel();
                    _cts = new CancellationTokenSource();

                    List<Task> originTasks = new List<Task>
                    {
                        transform.LerpRotationAsync(_origin.rotation, _resetTime, _cts.Token),
                        transform.LerpPositionAsync(_origin.position, _resetTime, _cts.Token)
                    };
                    await Task.WhenAll(originTasks);

                    _currentResetWaitTime = _rotationVelocityX = _rotationVelocityY = 0f;
                    _needsReset = false;
                }
                catch (OperationCanceledException) { }
            }
            else
                _currentResetWaitTime = 0f;

            bool CanReset()
            {
                return !_isDragging && Mathf.Approximately(_rotationVelocityX, 0)
                    && Mathf.Approximately(_rotationVelocityY, 0)
                    && transform.localRotation != _origin.rotation
                    && _needsReset;
            }
        }
    }
}
