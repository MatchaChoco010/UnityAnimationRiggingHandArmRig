using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent, AddComponentMenu ("Animation Rigging/Custom/Rotation Blend Slider")]
public class RotationBlendSliderConstraint : RigConstraint<RotationBlendSliderConstraintJob, RotationBlendSliderConstraintData, RotationBlendSliderConstraintBinder> { }

[BurstCompile]
public struct RotationBlendSliderConstraintJob : IWeightedAnimationJob {

    public ReadWriteTransformHandle Target;

    public ReadOnlyTransformHandle SourceA;
    public ReadOnlyTransformHandle SourceB;

    public ReadWriteTransformHandle Slider;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion (AnimationStream stream) { }

    public void ProcessAnimation (AnimationStream stream) {
        float w = jobWeight.Get (stream);

        if (w > 0f) {
            var sliderPos = Slider.GetLocalPosition (stream);
            var t = Mathf.Clamp01 (sliderPos.y);
            Slider.SetLocalPosition (stream, new Vector3 (0, t, 0));

            var rot = Quaternion.Lerp (
                SourceA.GetRotation (stream),
                SourceB.GetRotation (stream),
                t
            );

            var targetRot = Target.GetRotation (stream);
            Target.SetRotation (stream, Quaternion.Lerp (targetRot, rot, w));
        }
    }

}

[System.Serializable]
public struct RotationBlendSliderConstraintData : IAnimationJobData {

    public Transform Target;

    [SyncSceneToStream] public Transform SourceA;
    [SyncSceneToStream] public Transform SourceB;

    [SyncSceneToStream] public Transform Slider;

    public bool IsValid () => !(Target == null || SourceA == null || SourceB == null || Slider == null);

    public void SetDefaultValues () {
        Target = null;
        SourceA = null;
        SourceB = null;
        Slider = null;
    }

}

public class RotationBlendSliderConstraintBinder : AnimationJobBinder<RotationBlendSliderConstraintJob, RotationBlendSliderConstraintData> {

    public override RotationBlendSliderConstraintJob Create (Animator animator, ref RotationBlendSliderConstraintData data, Component component) {
        var job = new RotationBlendSliderConstraintJob ();

        job.Target = ReadWriteTransformHandle.Bind (animator, data.Target);

        job.SourceA = ReadOnlyTransformHandle.Bind (animator, data.SourceA);
        job.SourceB = ReadOnlyTransformHandle.Bind (animator, data.SourceB);

        job.Slider = ReadWriteTransformHandle.Bind (animator, data.Slider);

        return job;
    }

    public override void Destroy (RotationBlendSliderConstraintJob job) { }

}
