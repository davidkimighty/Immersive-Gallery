using System;
using System.Collections;
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

        [SerializeField] private float _newPositionInterval = 5f;
        [SerializeField] private float _newRotationInterval = 3f;
        [SerializeField] private float _moveSpeed = 0.001f;
        [SerializeField] private float _rotationSpeed = 0.01f;

        private FlowItemPreset _injectedPreset = null;
        public FlowItemPreset InjectedPreset
        {
            get => _injectedPreset;
        }
        private bool _interactable = true;
        private IEnumerator _randomFloatCoroutine = null;
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
            if (_randomFloatCoroutine != null)
                StopCoroutine(_randomFloatCoroutine);
            gameObject.SetActive(true);
            try
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();

                List<Task> moveTask = new List<Task>
                {
                    transform.LerpPositionAsync(targetAnchor.position, preset.PositionDuration, _cts.Token, preset.PositionCurve),
                    transform.LerpRotationAsync(targetAnchor.rotation, preset.RotationDuration, _cts.Token, preset.RotationCurve),
                };
                await Task.WhenAll(moveTask);

                if (gameObject.activeSelf)
                {
                    _randomFloatCoroutine = FloatingRandomly();
                    StartCoroutine(_randomFloatCoroutine);
                }
            }
            catch (OperationCanceledException) { }
        }

        public void SetReady(bool state, Transform targetAnchor)
        {
            gameObject.SetActive(state);
            transform.position = targetAnchor.position;
            transform.localScale = Vector3.one * _unfocusedSize;

            if (state)
            {
                if (_randomFloatCoroutine != null)
                    StopCoroutine(_randomFloatCoroutine);
                _randomFloatCoroutine = FloatingRandomly();
                StartCoroutine(_randomFloatCoroutine);
            }
        }

        #endregion

        private IEnumerator FloatingRandomly()
        {
            float posElapsedTime = Mathf.Infinity;
            float rotElapsedTime = Mathf.Infinity;
            Vector3 randomPosition = Vector3.zero;
            Quaternion randomRotation = Quaternion.identity;

            while (true)
            {
                if (posElapsedTime > _newPositionInterval)
                {
                    randomPosition = UnityEngine.Random.insideUnitSphere * 0.05f;
                    posElapsedTime = 0;
                }

                if (rotElapsedTime > _newRotationInterval)
                {
                    randomRotation = UnityEngine.Random.rotationUniform;
                    rotElapsedTime = 0;
                }
                transform.position = Vector3.Slerp(transform.position, randomPosition, _moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, randomRotation, _rotationSpeed * Time.deltaTime);
                posElapsedTime += Time.deltaTime;
                rotElapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
