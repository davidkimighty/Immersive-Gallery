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
        private List<Task> _moveTask = null;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            //_ = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //_ = transform.LerpScaleAsync(Vector3.one * _hoveredSize, _hoveredDuration, _cts.Token, _hoveredCurve);
        }

        #region Public Functions
        public void Initialize(FlowItemPreset preset)
        {
            _injectedPreset = preset;
        }

        public async Task MoveToAnchorAsync(Transform targetAnchor, FlowMovementPreset preset)
        {
            try
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();

                _moveTask = new List<Task>
                {
                    transform.LerpPositionAsync(targetAnchor.position, preset.PositionDuration, _cts.Token, preset.PositionCurve),
                    transform.LerpRotationAsync(targetAnchor.rotation, preset.RotationDuration, _cts.Token, preset.RotationCurve),
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
