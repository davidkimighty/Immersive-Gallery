using System.Collections;
using Broccollie.System;
using UnityEngine;

namespace Gallery
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        
        [SerializeField] private SceneAddressablePreset _immersiveGalleryScene = null;

        private ScreenFader _screenFader = null;

        private void Awake()
        {
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
