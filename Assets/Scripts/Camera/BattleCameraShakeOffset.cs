using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleCameraShakeOffset : CinemachineExtension
{
    public Vector2 PositionOffset { get; set; }
    public float RollOffset { get; set; }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
        {
            return;
        }

        if (PositionOffset != Vector2.zero)
        {
            state.PositionCorrection += state.GetCorrectedOrientation()
                * new Vector3(PositionOffset.x, PositionOffset.y, 0f);
        }

        if (!Mathf.Approximately(RollOffset, 0f))
        {
            state.OrientationCorrection *= Quaternion.Euler(0f, 0f, RollOffset);
        }
    }
}
