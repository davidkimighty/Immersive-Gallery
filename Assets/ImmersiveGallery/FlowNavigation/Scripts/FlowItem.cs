using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broccollie.Core;
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

        private List<Task> _moveTask = null;
        private Task _scaleTask = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion

        #region Public Functions
        public async void MoveToAnchorAsync(Transform targetAnchor, bool isCenter = false, bool postDisable = false)
        {
            gameObject.SetActive(true);
            transform.parent = targetAnchor; _isCenter = isCenter;

            _cts.Cancel();
            _cts = new CancellationTokenSource();

            _moveTask = new List<Task>();
            _moveTask.Add(transform.LerpLocalPositionAsync(Vector3.zero, _moveDuration, _cts.Token, _moveCurve));
            _moveTask.Add(transform.LerpLocalRotationAsync(Quaternion.identity, _rotateDuration, _cts.Token, _rotateCurve));

            _scaleTask = transform.LerpScaleAsync(isCenter ? Vector3.one * _focusedSize : Vector3.one * _unfocusedSize, _scaleDuration, _cts.Token, _scaleCurve);

            await Task.WhenAll(_moveTask);

            gameObject.SetActive(!postDisable);
        }

        public void MoveToAnchorReady(Transform targetAnchor)
        {
            _moveTask = null;
            _scaleTask = null;

            gameObject.SetActive(false);
            transform.parent = targetAnchor;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one * _unfocusedSize;
        }
        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isCenter) return;

            _scaleTask = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isCenter) return;

            _scaleTask = null;

            if (_isCenter)
            {
                _scaleTask = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
            }
        }
    }
}
