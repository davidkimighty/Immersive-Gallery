using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gallery
{
    [CreateAssetMenu(fileName = "ItemPreset", menuName = "ImmersiveGallery/Preset/FlowItemPreset")]
    public class FlowItemPreset : ScriptableObject
    {
        #region Variable Field
        public int Id = -1;
        public string Name = null;
        public AssetReference ItemRef = null;

        #endregion
    }
}
