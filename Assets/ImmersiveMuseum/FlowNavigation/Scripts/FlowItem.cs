using System.Collections;
using System.Collections.Generic;
using CollieMollie.Core;
using CollieMollie.Helper;
using CollieMollie.Interactable;
using CollieMollie.System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery.FlowNavigation
{
    public class FlowItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Variable Field
        [SerializeField] private float _scaleDuration = 0.6f;
        [SerializeField] private AnimationCurve _scaleCurve = null;
        [SerializeField] private float _moveDuration = 0.6f;
        [SerializeField] private AnimationCurve _moveCurve = null;
        [SerializeField] private float _rotateDuration = 0.6f;
        [SerializeField] private AnimationCurve _rotateCurve = null;
        [Range(0.1f, 2f)]
        [SerializeField] private float _focusedSize = 1f;
        [Range(0.1f, 1f)]
        [SerializeField] private float _unfocusedSize = 0.8f;

        [SerializeField] private float _hoveredSize = 1.3f;
        [SerializeField] private float _hoveredDuration = 0.6f;
        [SerializeField] private AnimationCurve _hoveredCurve = null;

        public string Name = null;
        public int Id = -1;

        private bool _interactable = true;
        private bool _isCenter = false;
        private Operation _moveOperation = new Operation();
        private Operation _scaleOperation = new Operation();
        #endregion

        #region Public Functions
        public void MoveToAnchor(Transform targetAnchor, bool isCenter = false, bool postDisable = false)
        {
            _moveOperation.Stop(this);
            _scaleOperation.Stop(this);

            gameObject.SetActive(true);
            transform.parent = targetAnchor; _isCenter = isCenter;

            _moveOperation.Add(transform.LerpLocalPosition(Vector3.zero, _moveDuration, _moveCurve));
            _moveOperation.Add(transform.LerpLocalRotation(Quaternion.identity, _rotateDuration, _rotateCurve));
            _moveOperation.Start(this, _moveDuration, () =>
            {
                gameObject.SetActive(!postDisable);
            });

            _scaleOperation.Add(transform.LerpScale(isCenter ? Vector3.one * _focusedSize : Vector3.one * _unfocusedSize, _scaleDuration, _scaleCurve));
            _scaleOperation.Start(this, _scaleDuration);
        }

        public void MoveToAnchorReady(Transform targetAnchor)
        {
            _moveOperation.Stop(this);
            _scaleOperation.Stop(this);

            gameObject.SetActive(false);
            transform.parent = targetAnchor;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * _unfocusedSize;
        }
        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isCenter) return;

            _scaleOperation.Stop(this);

            _scaleOperation.Add(transform.LerpScale(Vector3.one * _hoveredSize, _hoveredDuration, _hoveredCurve));
            _scaleOperation.Start(this, _scaleDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isCenter) return;

            _scaleOperation.Stop(this);

            if (_isCenter)
            {
                _scaleOperation.Add(transform.LerpScale(Vector3.one * _focusedSize, _hoveredDuration, _hoveredCurve));
                _scaleOperation.Start(this, _scaleDuration);
            }
        }
    }
}
