using System;
using System.Collections.Generic;
using Broccollie.System;
using Broccollie.UI;
using ShaderMagic.Shaders;
using UnityEngine;

namespace Gallery
{
    public class SettingController : MonoBehaviour
    {
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private List<SceneAddressablePreset> _turnOnScenes = null;

        [SerializeField] private BlurEventChannel _blurEventChannel = null;
        [SerializeField] private float _blurStrength = 30f;
        [SerializeField] private float _blurDuration = 0.7f;

        [SerializeField] private FadeEventChannel _fadeEventChannel = null;
        [SerializeField] private float _fadeStrength = 0.7f;
        [SerializeField] private float _fadeDuration = 1f;

        [SerializeField] private ButtonUI _settingButton = null;
        [SerializeField] private PanelUI _settingPanel = null;

        private void Awake()
        {
            _sceneEventChannel.OnAfterSceneLoad += EnableSettingButton;

            _settingButton.OnClick += OpenSettingsPanel;
            _settingButton.OnDefault += HideSettingsPanel;

            _settingButton.SetActive(false);
        }

        #region Subscribers
        private void EnableSettingButton(SceneAddressablePreset preset)
        {
            if (_turnOnScenes == null) return;

            bool turnOn = _turnOnScenes.Contains(preset);
            _settingButton.SetActive(turnOn);
        }

        private void OpenSettingsPanel(BaseUI sender, EventArgs args)
        {
            _settingPanel.ChangeState(UIStates.Show.ToString());
            _blurEventChannel.RequestBlurAsync(_blurStrength, _blurDuration);
            _fadeEventChannel.RequestFadeAsync(_fadeStrength, _fadeDuration);
        }

        private void HideSettingsPanel(BaseUI sender, EventArgs args)
        {
            _settingPanel.ChangeState(UIStates.Hide.ToString());
            _blurEventChannel.RequestBlurAsync(0, _blurDuration);
            _fadeEventChannel.RequestFadeAsync(0, _fadeDuration);
        }

        #endregion

    }
}
