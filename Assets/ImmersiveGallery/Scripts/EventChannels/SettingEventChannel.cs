using System;
using UnityEngine;

namespace Gallery
{
    [CreateAssetMenu(fileName = "EventChannel_Setting", menuName = "ImmersiveGallery/Event Channels/Setting")]
    public class SettingEventChannel : ScriptableObject
    {
        public event Action OnSettingOpen = null;
        public event Action OnSettingClose = null;

        public void RaiseSettingOpen() => OnSettingOpen?.Invoke();

        public void RaiseSettingClose() => OnSettingClose?.Invoke();
    }
}
