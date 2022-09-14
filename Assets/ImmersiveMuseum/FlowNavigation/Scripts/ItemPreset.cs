using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gallery.FlowNavigation
{
    [CreateAssetMenu(fileName = "ItemPreset", menuName = "CollieMollie/FlowNavigation/ItemPreset")]
    public class ItemPreset : ScriptableObject
    {
        #region Variable Field
        public string Name = null;
        public AssetReference ItemRef = null;

        #endregion
    }
}
