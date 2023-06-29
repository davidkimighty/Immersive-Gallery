using System.Collections;
using Broccollie.System;
using ShaderMagic.Shaders;
using UnityEngine;

namespace Gallery
{
    public class TitleManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private FadeEventChannel _fadeEventChannel = null;

        [SerializeField] private float _fadeDuration = 1f;

        [SerializeField] private SceneAddressablePreset _immersiveGalleryScene = null;

        private void Awake()
        {
            _fadeEventChannel.RequestFade(0, _fadeDuration);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(3);

            _sceneEventChannel.RequestSceneLoadAsync(_immersiveGalleryScene, false);
        }
    }
}
