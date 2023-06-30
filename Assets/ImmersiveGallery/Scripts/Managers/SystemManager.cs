using System.Threading.Tasks;
using Broccollie.System;
using ShaderMagic.Shaders;
using UnityEngine;

namespace Gallery
{
    public class SystemManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private FadeEventChannel _fadeEventChannel = null;
        [SerializeField] private float _fadeDuration = 1f;

        [Header("Scenes")]
        [SerializeField] private SceneAddressablePreset _titleScene = null;

        private async void Awake()
        {
            _sceneEventChannel.OnBeforeSceneUnloadAsync += FadeIn;

            await _sceneEventChannel.RequestSceneLoadAsync(_titleScene, false);
        }

        #region Subscribers
        private async Task FadeIn(SceneAddressablePreset preset) => await _fadeEventChannel.RequestFadeAsync(1f, _fadeDuration);

        #endregion
    }
}
