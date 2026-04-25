using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleCameraShake : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private BattleCameraShakeOffset shakeOffset;

    [Header("Shake")]
    [SerializeField, Min(0f)] private float maxAmplitude = 0.01f;
    [SerializeField, Min(0f)] private float frequencyGain = 35f;
    [SerializeField, Min(0f)] private float maxRollAngle = 0f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;
    [SerializeField] private bool restoreInitialTransformOnStop = true;

    private Coroutine shakeRoutine;
    private float currentAmplitude;
    private float currentFrequencyGain;
    private float currentRollAngle;
    private float targetAmplitude;
    private float targetFrequencyGain;
    private float targetRollAngle;
    private float shakeTime;
    private float phaseX;
    private float phaseY;
    private float phaseRoll;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool hasInitialTransform;

    private void Awake()
    {
        ResolveShakeOffset(true);
        CaptureInitialTransform();
        ResetSeeds();
        SetShake(0f, 0f, 0f);
    }

    private void OnDisable()
    {
        StopShakeImmediately();
    }

    private void LateUpdate()
    {
        UpdateShakeOffset();
    }

    private void OnValidate()
    {
        ResolveShakeOffset(false);
    }

    public void StartShake()
    {
        ResetSeeds();
        FadeTo(maxAmplitude, frequencyGain, maxRollAngle, fadeInDuration, false);
    }

    public void StopShake()
    {
        FadeTo(0f, 0f, 0f, fadeOutDuration, restoreInitialTransformOnStop);
    }

    public void StopShakeImmediately()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        targetAmplitude = 0f;
        targetFrequencyGain = 0f;
        targetRollAngle = 0f;
        SetShake(0f, 0f, 0f);
        ApplyShakeOffset(Vector2.zero, 0f);
        RestoreInitialTransformImmediate();
    }

    private void FadeTo(
        float targetAmplitudeValue,
        float targetFrequencyValue,
        float targetRollValue,
        float duration,
        bool restoreInitialTransform)
    {
        ResolveShakeOffset(true);

        if (shakeOffset == null)
        {
            return;
        }

        if (Mathf.Approximately(targetAmplitude, targetAmplitudeValue)
            && Mathf.Approximately(targetFrequencyGain, targetFrequencyValue)
            && Mathf.Approximately(targetRollAngle, targetRollValue))
        {
            if (shakeRoutine != null
                || (Mathf.Approximately(currentAmplitude, targetAmplitudeValue)
                    && Mathf.Approximately(currentFrequencyGain, targetFrequencyValue)
                    && Mathf.Approximately(currentRollAngle, targetRollValue)
                    && (!restoreInitialTransform || IsAtInitialTransform())))
            {
                return;
            }
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        targetAmplitude = targetAmplitudeValue;
        targetFrequencyGain = targetFrequencyValue;
        targetRollAngle = targetRollValue;
        shakeRoutine = StartCoroutine(FadeRoutine(
            targetAmplitudeValue,
            targetFrequencyValue,
            targetRollValue,
            duration,
            restoreInitialTransform));
    }

    private IEnumerator FadeRoutine(
        float amplitudeTarget,
        float frequencyTarget,
        float rollTarget,
        float duration,
        bool restoreInitialTransform)
    {
        float startAmplitude = currentAmplitude;
        float startFrequency = currentFrequencyGain;
        float startRoll = currentRollAngle;
        Transform cameraTransform = GetCameraTransform();
        Vector3 startLocalPosition = cameraTransform != null ? cameraTransform.localPosition : Vector3.zero;
        Quaternion startLocalRotation = cameraTransform != null ? cameraTransform.localRotation : Quaternion.identity;

        if (duration <= 0f)
        {
            SetShake(amplitudeTarget, frequencyTarget, rollTarget);

            if (restoreInitialTransform)
            {
                RestoreInitialTransformImmediate();
            }

            shakeRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            SetShake(
                Mathf.Lerp(startAmplitude, amplitudeTarget, easedT),
                Mathf.Lerp(startFrequency, frequencyTarget, easedT),
                Mathf.Lerp(startRoll, rollTarget, easedT));

            if (restoreInitialTransform)
            {
                RestoreInitialTransform(t, cameraTransform, startLocalPosition, startLocalRotation);
            }

            yield return null;
        }

        SetShake(amplitudeTarget, frequencyTarget, rollTarget);

        if (restoreInitialTransform)
        {
            RestoreInitialTransform(1f, cameraTransform, startLocalPosition, startLocalRotation);
        }

        shakeRoutine = null;
    }

    private void SetShake(float amplitude, float frequency, float roll)
    {
        currentAmplitude = Mathf.Max(0f, amplitude);
        currentFrequencyGain = Mathf.Max(0f, frequency);
        currentRollAngle = Mathf.Max(0f, roll);
    }

    private void UpdateShakeOffset()
    {
        if (shakeOffset == null)
        {
            return;
        }

        if (currentAmplitude <= 0.00001f && currentRollAngle <= 0.00001f)
        {
            ApplyShakeOffset(Vector2.zero, 0f);
            return;
        }

        shakeTime += Time.deltaTime * currentFrequencyGain;

        float x = GetCenteredWave(shakeTime, phaseX, 1f, 2.17f);
        float y = GetCenteredWave(shakeTime, phaseY, 1.31f, 2.63f);
        Vector2 offset = new Vector2(x, y) * currentAmplitude;

        float roll = currentRollAngle <= 0f
            ? 0f
            : Mathf.Sin((shakeTime * 1.47f) + phaseRoll) * currentRollAngle;

        ApplyShakeOffset(offset, roll);
    }

    private float GetCenteredWave(float time, float phase, float primarySpeed, float secondarySpeed)
    {
        float primary = Mathf.Sin((time * primarySpeed) + phase) * 0.75f;
        float secondary = Mathf.Sin((time * secondarySpeed) + (phase * 0.53f)) * 0.25f;
        return primary + secondary;
    }

    private void ApplyShakeOffset(Vector2 offset, float roll)
    {
        if (shakeOffset == null)
        {
            return;
        }

        shakeOffset.PositionOffset = offset;
        shakeOffset.RollOffset = roll;
    }

    private void ResetSeeds()
    {
        shakeTime = 0f;
        phaseX = Random.Range(0f, Mathf.PI * 2f);
        phaseY = phaseX + (Mathf.PI * 0.5f);
        phaseRoll = Random.Range(0f, Mathf.PI * 2f);
    }

    private void CaptureInitialTransform()
    {
        Transform cameraTransform = GetCameraTransform();

        if (cameraTransform == null)
        {
            return;
        }

        initialLocalPosition = cameraTransform.localPosition;
        initialLocalRotation = cameraTransform.localRotation;
        hasInitialTransform = true;
    }

    private void RestoreInitialTransform(
        float t,
        Transform cameraTransform,
        Vector3 startLocalPosition,
        Quaternion startLocalRotation)
    {
        if (!restoreInitialTransformOnStop || !hasInitialTransform || cameraTransform == null)
        {
            return;
        }

        float easedT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        cameraTransform.localPosition = Vector3.Lerp(startLocalPosition, initialLocalPosition, easedT);
        cameraTransform.localRotation = Quaternion.Slerp(startLocalRotation, initialLocalRotation, easedT);
    }

    private void RestoreInitialTransformImmediate()
    {
        Transform cameraTransform = GetCameraTransform();

        if (!hasInitialTransform || cameraTransform == null)
        {
            return;
        }

        cameraTransform.localPosition = initialLocalPosition;
        cameraTransform.localRotation = initialLocalRotation;
    }

    private bool IsAtInitialTransform()
    {
        Transform cameraTransform = GetCameraTransform();

        if (!hasInitialTransform || cameraTransform == null)
        {
            return true;
        }

        return (cameraTransform.localPosition - initialLocalPosition).sqrMagnitude <= 0.000001f
            && Quaternion.Angle(cameraTransform.localRotation, initialLocalRotation) <= 0.01f;
    }

    private Transform GetCameraTransform()
    {
        return cinemachineCamera != null ? cinemachineCamera.transform : null;
    }

    private void ResolveShakeOffset(bool createIfMissing)
    {
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (shakeOffset == null && cinemachineCamera != null)
        {
            shakeOffset = cinemachineCamera.GetComponent<BattleCameraShakeOffset>();
        }

        if (shakeOffset == null && createIfMissing && cinemachineCamera != null)
        {
            shakeOffset = cinemachineCamera.gameObject.AddComponent<BattleCameraShakeOffset>();
        }

        MuteCinemachineNoise();

        if (!hasInitialTransform && cinemachineCamera != null)
        {
            CaptureInitialTransform();
        }
    }

    private void MuteCinemachineNoise()
    {
        if (cinemachineCamera == null)
        {
            return;
        }

        CinemachineBasicMultiChannelPerlin noise = cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise)
            as CinemachineBasicMultiChannelPerlin;

        if (noise == null)
        {
            noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (noise == null)
        {
            return;
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
}
