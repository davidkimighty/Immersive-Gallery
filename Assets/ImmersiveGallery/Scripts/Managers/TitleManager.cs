using System.Threading.Tasks;
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

        private async void Awake()
        {
            await _fadeEventChannel.RequestFadeAsync(0, _fadeDuration);

            await Task.Delay(3000);

            await _sceneEventChannel.RequestSceneLoadAsync(_immersiveGalleryScene, false);
        }
    }
}
