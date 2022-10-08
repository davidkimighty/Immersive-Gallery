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
    public class FlowItem : BasePointerInteractable
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

        public string Name = null;
        public int Id = -1;

        private Operation _moveOperation = new Operation();
        private bool _interactable = true;
        #endregion

        #region Public Functions
        public void MoveToAnchor(Transform targetAnchor, bool isCenter = false, bool postDisable = false)
        {
            _moveOperation.Stop(this);

            gameObject.SetActive(true);
            transform.parent = targetAnchor;

            _moveOperation.Add(transform.LerpScale(isCenter ? Vector3.one * _focusedSize : Vector3.one * _unfocusedSize, _scaleDuration, _scaleCurve));
            _moveOperation.Add(transform.LerpLocalPosition(Vector3.zero, _moveDuration, _moveCurve));
            _moveOperation.Add(transform.LerpLocalRotation(Quaternion.identity, _rotateDuration, _rotateCurve));
            _moveOperation.Start(this, _moveDuration, () =>
            {
                gameObject.SetActive(!postDisable);
            });
        }

        public void MoveToAnchorReady(Transform targetAnchor)
        {
            _moveOperation.Stop(this);

            gameObject.SetActive(false);
            transform.parent = targetAnchor;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * _unfocusedSize;
        }
        #endregion

        protected override void InvokeEnterAction(PointerEventData eventData = null)
        {
            base.InvokeEnterAction(eventData);
        }

        protected override void InvokeExitAction(PointerEventData eventData = null)
        {
            base.InvokeExitAction(eventData);
        }
    }
}
