using System.Collections;
using Broccollie.System;
using UnityEngine;

namespace Gallery
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        
        [SerializeField] private SceneAddressablePreset _immersiveGalleryScene = null;

        [SerializeField] private Canvas _canvas = null;

        private ScreenFader _screenFader = null;

        private void Awake()
        {
            if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = Camera.main;

            _screenFader = FindObjectOfType<ScreenFader>();
            _screenFader.FadeAsync(0);
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(3);

            _sceneEventChannel.RequestSceneLoadAsync(_immersiveGalleryScene, false);
        }
    }
}
