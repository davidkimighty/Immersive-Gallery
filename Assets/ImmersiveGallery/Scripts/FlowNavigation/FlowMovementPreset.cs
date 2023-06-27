using UnityEngine;

namespace Gallery
{
    [CreateAssetMenu(fileName = "MovePreset", menuName = "ImmersiveGallery/Preset/FlowMovementPreset")]
    public class FlowMovementPreset : ScriptableObject
    {
        public float PositionDuration = 0;
        public AnimationCurve PositionCurve = null;

        public float RotationDuration = 0;
        public AnimationCurve RotationCurve = null;

        public float ScaleDuration = 0;
        public AnimationCurve ScaleCurve = null;
    }
}
