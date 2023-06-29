using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gallery
{
    [CreateAssetMenu(fileName = "ItemPreset", menuName = "ImmersiveGallery/Preset/FlowItemPreset")]
    public class FlowItemPreset : ScriptableObject
    {
        public int Id = -1;
        public string ItemName = null;
        public AssetReference ItemRef = null;
        public List<FlowItemPreset> SubFlowItems = null;
        public string ItemDescription = null;
    }
}
