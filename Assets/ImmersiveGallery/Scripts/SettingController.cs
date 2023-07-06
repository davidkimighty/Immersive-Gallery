using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Broccollie.System;
using Broccollie.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Gallery
{
    public class SettingController : MonoBehaviour
    {
        private const string s_musicKey = "MusicVolume";
        private const string s_sfxKey = "SFXVolume";
        private const string s_ambientKey = "AmbientVolume";

        [SerializeField] private SettingEventChannel _settingEventChannel = null;
        [SerializeField] private SceneAddressableEventChannel _sceneEventChannel = null;
        [SerializeField] private List<SceneAddressablePreset> _turnOnScenes = null;

        [SerializeField] private AudioMixer _mixer = null;
        [SerializeField] private Slider _musicSlider = null;
        [SerializeField] private Slider _sfxSlider = null;
        [SerializeField] private Slider _ambientSlider = null;
        [SerializeField] private TextMeshProUGUI _titleText = null;
        [SerializeField] private Image _screenFader = null;
        [SerializeField] private float _fadeStrength = 0.9f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private AnimationCurve _fadeCurve = null;
        [SerializeField] private CanvasGroup _canvasGroup = null;

        [SerializeField] private ButtonUI _settingButton = null;
        [SerializeField] private PanelUI _settingPanel = null;

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener((volume) => ChangeVolume(s_musicKey, volume));
            _sfxSlider.onValueChanged.AddListener((volume) => ChangeVolume(s_sfxKey, volume));
            _ambientSlider.onValueChanged.AddListener((volume) => ChangeVolume(s_ambientKey, volume));

            _settingButton.OnClick += OpenSettingsPanel;
            _settingButton.OnDefault += HideSettingsPanel;

            _settingButton.SetActive(false);
            _titleText.enabled = false;
            _screenFader.enabled = false;

            ChangeVolume(s_musicKey, 0.3f);
            ChangeVolume(s_sfxKey, 0.5f);
            ChangeVolume(s_ambientKey, 0.3f);

            _musicSlider.value = 0.3f;
            _sfxSlider.value = 0.5f;
            _ambientSlider.value = 0.3f;
        }

        private void OnEnable()
        {
            _sceneEventChannel.OnAfterSceneLoad += EnableSettingButton;
        }

        private void OnDisable()
        {
            _sceneEventChannel.OnAfterSceneLoad -= EnableSettingButton;
        }

        #region Subscribers
        private void ChangeVolume(string name, float volume) => _mixer.SetFloat(name, Mathf.Log10(volume) * 20);

        private void EnableSettingButton(SceneAddressablePreset preset)
        {
            if (_turnOnScenes == null) return;

            bool turnOn = _turnOnScenes.Contains(preset);
            _settingButton.SetActive(turnOn);
        }

        private void OpenSettingsPanel(BaseUI sender, EventArgs args)
        {
            _settingPanel.ChangeState(UIStates.Show.ToString());
            _titleText.enabled = true;
            _screenFader.enabled = true;

            FadeAsync(_fadeStrength);
            _settingEventChannel.RaiseSettingOpen();
        }

        private void HideSettingsPanel(BaseUI sender, EventArgs args)
        {
            _settingPanel.ChangeState(UIStates.Hide.ToString());
            _titleText.enabled = false;

            FadeAsync(0, () => _screenFader.enabled = false);
            _settingEventChannel.RaiseSettingClose();
        }

        #endregion

        private async void FadeAsync(float alpha, Action done = null)
        {
            Color startColor = _screenFader.color;
            Color targetColor = startColor;
            targetColor.a = alpha;
            float startAlpha = _canvasGroup.alpha;

            float elapsedTime = 0f;
            while (elapsedTime < _fadeDuration)
            {
                _screenFader.color = Color.Lerp(startColor, targetColor, _fadeCurve.Evaluate(elapsedTime / _fadeDuration));
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, _fadeCurve.Evaluate(elapsedTime / _fadeDuration));
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }
            _screenFader.color = targetColor;
            done?.Invoke();
        }
    }
}
