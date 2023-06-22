using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gallery
{
    [CreateAssetMenu(fileName = "ItemPreset", menuName = "ImmersiveGallery/Preset/FlowItemPreset")]
    public class FlowItemPreset : ScriptableObject
    {
        public int Id = -1;
        public string Name = null;
        public AssetReference ItemRef = null;

    }
}
