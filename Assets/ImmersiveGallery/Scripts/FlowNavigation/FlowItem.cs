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
        [SerializeField] private Showcaser _showcaser = null;

        [Header("Hover Interaction")]
        [SerializeField] private float _hoveredSize = 1.3f;
        [SerializeField] private float _hoveredDuration = 0.3f;
        [SerializeField] private AnimationCurve _hoveredCurve = null;

        [Header("Random Float")]
        [SerializeField] private float _newPositionInterval = 5f;
        [SerializeField] private float _newRotationInterval = 3f;
        [SerializeField] private float _moveSpeed = 0.001f;
        [SerializeField] private float _rotationSpeed = 0.01f;

        [SerializeField] private float _followSpeed = 0.5f;

        private GameObject _rotateIndicators = null;
        private FlowItemPreset _injectedPreset = null;
        public FlowItemPreset InjectedPreset
        {
            get => _injectedPreset;
        }
        private bool _showcaserEnabled = false;
        private Vector3 _originScale = Vector3.one;
        private IEnumerator _hoverScaleCoroutine = null;
        private IEnumerator _randomFloatCoroutine = null;
        private CancellationTokenSource _cts = new();

        private void Awake()
        {
            _showcaser.OnEndDrag += BackToOriginalScale;
        }

        #region Subscribers
        private void BackToOriginalScale()
        {
            if (_hoverScaleCoroutine != null)
                StopCoroutine(_hoverScaleCoroutine);
            _hoverScaleCoroutine = transform.LerpScale(_originScale, _hoveredDuration, _hoveredCurve);
            StartCoroutine(_hoverScaleCoroutine);

            _rotateIndicators.SetActive(false);
        }

        #endregion

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_randomFloatCoroutine != null)
                StopCoroutine(_randomFloatCoroutine);

            if (_showcaserEnabled)
            {
                if (_hoverScaleCoroutine != null)
                    StopCoroutine(_hoverScaleCoroutine);
                _hoverScaleCoroutine = transform.LerpScale(_originScale * _hoveredSize, _hoveredDuration, _hoveredCurve);
                StartCoroutine(_hoverScaleCoroutine);

                _rotateIndicators.SetActive(true);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (gameObject.activeSelf)
            {
                _randomFloatCoroutine = FloatingRandomly();
                StartCoroutine(_randomFloatCoroutine);
            }

            if (!_showcaser.IsDragging && _showcaserEnabled)
            {
                BackToOriginalScale();
            }
        }

        #region Public Functions
        public void Initialize(FlowItemPreset preset, GameObject rotateIndicator)
        {
            _injectedPreset = preset;
            _rotateIndicators = rotateIndicator;
        }

        public async Task MoveToAnchorAsync(Transform targetAnchor, FlowMovementPreset preset)
        {
            if (_randomFloatCoroutine != null)
                StopCoroutine(_randomFloatCoroutine);

            _originScale = targetAnchor.localScale;
            _showcaser.ResetMovement();
            gameObject.SetActive(true);

            try
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();

                List<Task> moveTask = new List<Task>
                {
                    transform.LerpPositionAsync(targetAnchor.position, preset.PositionDuration, _cts.Token, preset.PositionCurve),
                    transform.LerpRotationAsync(targetAnchor.rotation, preset.RotationDuration, _cts.Token, preset.RotationCurve),
                    transform.LerpScaleAsync(targetAnchor.localScale, preset.ScaleDuration, _cts.Token, preset.ScaleCurve)
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

        public void SetReady(bool state, Transform targetAnchor = null)
        {
            if (_randomFloatCoroutine != null)
                StopCoroutine(_randomFloatCoroutine);

            _showcaser.ResetMovement();
            gameObject.SetActive(state);

            if (targetAnchor != null)
            {
                transform.position = targetAnchor.position;
                transform.localScale = targetAnchor.localScale;
            }

            if (state)
            {
                _randomFloatCoroutine = FloatingRandomly();
                StartCoroutine(_randomFloatCoroutine);
            }
        }

        public void EnableShowcaser(bool state, Transform origin)
        {
            _showcaser.EnableShowcase(state, origin);
            _showcaserEnabled = state;
        }

        #endregion

        private IEnumerator FloatingRandomly()
        {
            float rotElapsedTime = 0;
            Quaternion randomRotation = GetRandomRoation();

            while (true)
            {
                if (rotElapsedTime > _newRotationInterval)
                {
                    randomRotation = GetRandomRoation();
                    rotElapsedTime = 0;
                }

                transform.rotation = Quaternion.Lerp(transform.rotation, randomRotation, _rotationSpeed * Time.deltaTime);
                rotElapsedTime += Time.deltaTime;
                yield return null;
            }

            //Vector3 GetRandomPosition() => transform.position + UnityEngine.Random.insideUnitSphere * 0.1f;
            Quaternion GetRandomRoation() => UnityEngine.Random.rotationUniform;
        }
    }
}
