using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Museum.Gameboy
{
    public class ParallaxBackground : MonoBehaviour
    {
        #region Variable Field
        [SerializeField] private float _scrollSpeed = 3f;
        [SerializeField] private BackgroundLayer[] _layers = null;
        [SerializeField] private Material _material = null;
        #endregion

        private void Start()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i].Background.material = new Material(_material);
            }
        }

        private void Update()
        {
            ParallaxMovement();
        }

        #region Features
        private void ParallaxMovement()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                Vector2 offset = _layers[i].Background.materialForRendering.mainTextureOffset;
                offset.x += (_scrollSpeed * Time.deltaTime) * _layers[i].ParallaxValue;
                /* using layer.material won't work if the Image component is child to the Mask Component. */
                _layers[i].Background.materialForRendering.mainTextureOffset = offset;
            }
        }
        #endregion
    }

    [Serializable]
    public class BackgroundLayer
    {
        public Image Background = null;
        public float ParallaxValue = 0.1f;
    }
}
