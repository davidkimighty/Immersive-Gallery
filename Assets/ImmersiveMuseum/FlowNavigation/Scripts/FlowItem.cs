using System.Collections;
using System.Collections.Generic;
using CollieMollie.Helper;
using UnityEngine;

namespace Gallery.FlowNavigation
{
    public class FlowItem : MonoBehaviour
    {
        #region Variable Field
        [SerializeField] private float _moveSpeed = 0.6f;
        [SerializeField] private AnimationCurve _moveCurve = null;
        [SerializeField] private float _floatSpeed = 0.3f;

        public string Name = null;
        public int Id = -1;

        private IEnumerator _moveAction = null;
        #endregion

        #region Public Functions
        public void MoveToAnchor(Transform targetAnchor, bool postDisable = false)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (_moveAction != null)
                StopCoroutine(_moveAction);


            transform.parent = targetAnchor;
            _moveAction = transform.LerpLocalPosition(Vector3.zero, _moveSpeed, _moveCurve, () =>
            {
                gameObject.SetActive(!postDisable);
            });
            StartCoroutine(_moveAction);
        }

        public void MoveToAnchorReady(Transform targetAnchor)
        {
            if (_moveAction != null)
                StopCoroutine(_moveAction);

            gameObject.SetActive(false);
            transform.parent = targetAnchor;
            transform.localPosition = Vector3.zero;
        }
        #endregion
    }
}
