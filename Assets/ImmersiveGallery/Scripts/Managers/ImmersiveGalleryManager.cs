using Broccollie.Audio;
using Broccollie.System;
using UnityEngine;

namespace Gallery
{
    public class ImmersiveGalleryManager : MonoBehaviour
    {
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private AudioEventChannel _audioEventChannel = null;

        [SerializeField] private FlowNavigationController _flowNavController = null;

        [SerializeField] private AudioPreset _ambientPreset = null;

        private ScreenFader _screenFader = null;

        private async void Awake()
        {
            await _flowNavController.InitializeAsync();

            _audioEventChannel.RaisePlayAudioEvent(_ambientPreset);

            _screenFader = FindObjectOfType<ScreenFader>();
            await _screenFader.FadeAsync(0);
        }
    }
}
