using System.Threading.Tasks;
using Broccollie.System;
using UnityEngine;

namespace Gallery
{
    public class SystemManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;

        [Header("Scenes")]
        [SerializeField] private SceneAddressablePreset _titleScene = null;

        [Header("Controllers")]
        [SerializeField] private ScreenFader _screenFader = null;

        private async void Awake()
        {
            _sceneEventChannel.OnBeforeSceneUnloadAsync += FadeIn;

            await _sceneEventChannel.RequestSceneLoadAsync(_titleScene, false);
        }

        #region Subscribers
        private async Task FadeIn(SceneAddressablePreset preset) => await _screenFader.FadeAsync(1);

        #endregion
    }
}
