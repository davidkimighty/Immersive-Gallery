using Broccollie.System;
using ShaderMagic.Shaders;
using UnityEngine;

namespace Gallery
{
    public class ImmersiveGalleryManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private FadeEventChannel _fadeEventChannel = null;

        [SerializeField] private float _fadeDuration = 1f;

        [Header("Flow Navigation")]
        [SerializeField] private FlowNavigationController _flowNavController = null;

        private async void Awake()
        {
            await _flowNavController.InitializeAsync();

            await _fadeEventChannel.RequestFadeAsync(0, _fadeDuration);
        }
    }
}
