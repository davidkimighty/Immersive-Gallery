using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gallery
{
    public class FlowItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
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

        private FlowItemPreset _injectedPreset = null;
        public FlowItemPreset InjectedPreset
        {
            get => _injectedPreset;
        }
        private bool _interactable = true;
        private bool _isCenter = false;

        private List<Task> _moveTask = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_isCenter) return;

            _ = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_isCenter) return;

            if (_isCenter)
            {
                _ = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
            }
        }

        #region Public Functions
        public void Initialize(FlowItemPreset preset)
        {
            _injectedPreset = preset;
        }

        public async Task MoveToAnchorAsync(Transform targetAnchor, bool isCenter = false)
        {
            _isCenter = isCenter;
            gameObject.SetActive(true);
            try
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();

                _moveTask = new List<Task>
                {
                    transform.LerpPositionAsync(targetAnchor.position, _moveDuration, _cts.Token, _moveCurve),
                    transform.LerpRotationAsync(targetAnchor.rotation, _rotateDuration, _cts.Token, _rotateCurve),
                    transform.LerpScaleAsync(isCenter ? Vector3.one * _focusedSize : Vector3.one * _unfocusedSize, _scaleDuration, _cts.Token, _scaleCurve)
                };
                await Task.WhenAll(_moveTask);
            }
            catch (OperationCanceledException) { }
        }

        public void MoveToAnchorReady(Transform targetAnchor)
        {
            gameObject.SetActive(true);
            transform.position = targetAnchor.position;
            transform.localScale = Vector3.one * _unfocusedSize;
        }

        public void Disable() => gameObject.SetActive(false);

        #endregion
    }
}
