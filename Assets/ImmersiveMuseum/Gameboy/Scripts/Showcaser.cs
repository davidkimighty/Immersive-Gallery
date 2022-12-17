using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CollieMollie.Helper;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery.Gameboy
{
    public class Showcaser : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region Variable field
        [SerializeField] private Transform _showcaseObject = null;
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private float _rotationDamping = 0.3f;
        [SerializeField] private float _resetWaitTime = 3f;
        [SerializeField] private float _resetRotationTime = 0.3f;
        [SerializeField] private bool _isDragging = false;

        private Quaternion _originalRotation = Quaternion.identity;
        private float _rotationVelocityX = 0f;
        private float _rotationVelocityY = 0f;
        private float _currentResetWaitTime = 0f;
        private Task _resetTask = null;
        #endregion

        private void Start()
        {
            if (_showcaseObject == null) return;
            _originalRotation = _showcaseObject.localRotation;
        }

        #region Public Functions
        public void SetShowcaseObject(Transform showcaseObject, bool resetRatation = false)
        {
            _showcaseObject = showcaseObject;
            _originalRotation = resetRatation ? Quaternion.identity : _showcaseObject.localRotation;
        }

        public void ResetMovement()
        {
            _resetTask = null;
            _currentResetWaitTime = 0f;
            _rotationVelocityX = 0f;
            _rotationVelocityY = 0f;
        }
        #endregion

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //if (eventData.pointerId != 2) return;

            float rotDeltaX = eventData.delta.x * _rotationSpeed * Time.deltaTime;
            float rotDeltaY = eventData.delta.y * _rotationSpeed * Time.deltaTime;

            _rotationVelocityX = Mathf.Lerp(_rotationVelocityX, rotDeltaX, Time.deltaTime);
            _rotationVelocityY = Mathf.Lerp(_rotationVelocityY, rotDeltaY, Time.deltaTime);

            _showcaseObject.Rotate(Vector3.up, -_rotationVelocityX, Space.Self);
            _showcaseObject.Rotate(Vector3.forward, -_rotationVelocityY, Space.Self);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }

        private void Update()
        {
            if (_showcaseObject == null) return;

            VelocityXMovement();
            VelocityYMovement();
            BackToOriginalRotation();
        }

        #region Showcase Features
        private void VelocityXMovement()
        {
            if (!_isDragging && !Mathf.Approximately(_rotationVelocityX, 0))
            {
                float deltaVelocity = Mathf.Min(
                    Mathf.Sign(_rotationVelocityX) * _rotationDamping * Time.deltaTime,
                    Mathf.Sign(_rotationVelocityX) * _rotationVelocityX
                );
                _rotationVelocityX -= deltaVelocity;
                _showcaseObject.Rotate(Vector3.up, -_rotationVelocityX, Space.Self);
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
                _showcaseObject.Rotate(Vector3.forward, -_rotationVelocityY, Space.Self);
            }
        }

        private void BackToOriginalRotation()
        {
            if (CanReset())
            {
                _currentResetWaitTime += Time.deltaTime;
                if (_currentResetWaitTime < _resetWaitTime || _resetTask != null) return;

                _resetTask = ResetRotationAsync();
            }
            else
            {
                _currentResetWaitTime = 0f;
            }

            async Task ResetRotationAsync()
            {
                await _showcaseObject.LerpLocalRotationAsync(_originalRotation, _resetRotationTime);
                _currentResetWaitTime = _rotationVelocityX = _rotationVelocityY = 0f;
                _resetTask = null;
            }

            bool CanReset()
            {
                return !_isDragging && Mathf.Approximately(_rotationVelocityX, 0)
                    && Mathf.Approximately(_rotationVelocityY, 0)
                    && _showcaseObject.localRotation != _originalRotation;
            }
        }
        #endregion
    }
}
