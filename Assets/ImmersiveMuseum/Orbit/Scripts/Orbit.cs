using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gallery.Orbit
{
    public class Orbit : MonoBehaviour
    {
        #region Variable Field
        [Header("Revolution")]
        [SerializeField] private Transform _target = null;
        [SerializeField] private float _revolutionSpeed = 3f;
        [SerializeField] private Vector3 _revolutionAngle = new Vector3(0, 1, 0);

        [Header("Rotation")]
        [SerializeField] private float _rotSpeed = 1f;
        [SerializeField] private Vector3 _rotAngle = new Vector3(1, 1, 0);

        private Vector3 _dir = Vector3.zero;
        private float _dstFromTarget = 0;

        #endregion

        private void Start()
        {
            _dir = (transform.position - _target.position).normalized;
            _dstFromTarget = Vector3.Distance(_target.position, transform.position);
        }

        private void Update()
        {
            Rotation();
            Revolution();
        }

        #region Private Functions
        private void Rotation()
        {
            transform.Rotate(_rotAngle, _rotSpeed);
        }

        private void Revolution()
        {
            _dir = Quaternion.Euler(_revolutionAngle * _revolutionSpeed * Time.deltaTime) * _dir;
            transform.position = _target.position + _dir * _dstFromTarget;
        }

        #endregion
    }
}
