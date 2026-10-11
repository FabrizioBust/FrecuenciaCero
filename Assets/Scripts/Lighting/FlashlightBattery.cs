using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(200)] // Consume after the controller processes F in this frame.
    [DisallowMultipleComponent, RequireComponent(typeof(Light2D))]
    public sealed class FlashlightBattery : MonoBehaviour
    {
        [SerializeField, Range(0f, 100f)] private float batteryPercent = 100f;
        [SerializeField, Min(0.1f)] private float fullBatteryDuration = 120f;
        [SerializeField, Min(0.01f)] private float minimumIntensity = 0.2f;
        [SerializeField, Min(0.01f)] private float maximumIntensity = 1.6f;
        [SerializeField, Min(0.1f)] private float minimumRange = 0.9f;
        [SerializeField, Min(0.1f)] private float maximumRange = 5.5f;
        [SerializeField, Range(0f, 1f)] private float innerRadiusFraction = 0.35f;

        private Light2D beam;
        public float Percent => batteryPercent;
        public float NormalizedCharge => Mathf.Clamp01(batteryPercent / 100f);

        private void Awake()
        {
            beam = GetComponent<Light2D>();
            ValidateSettings();
            ApplyDegradation();
        }

        private void Update()
        {
            batteryPercent = BatteryCharge.Drain(batteryPercent, Time.deltaTime,
                fullBatteryDuration, beam.enabled);
            ApplyDegradation();
        }

        // Future recharge mechanics can supply energy without changing the light's manual state.
        public void AddCharge(float percentagePoints)
        {
            batteryPercent = Mathf.Clamp(batteryPercent + Mathf.Max(0f, percentagePoints), 0f, 100f);
            if (beam != null) ApplyDegradation();
        }

        // Scene travel restores the existing charge instead of creating a fresh battery.
        public void RestoreCharge(float percent)
        {
            batteryPercent = Mathf.Clamp(percent, 0f, 100f);
            if (beam != null) ApplyDegradation();
        }

        private void ApplyDegradation()
        {
            beam.intensity = BatteryCharge.Interpolate(minimumIntensity, maximumIntensity, batteryPercent);
            beam.pointLightOuterRadius = BatteryCharge.Interpolate(minimumRange, maximumRange, batteryPercent);
            beam.pointLightInnerRadius = beam.pointLightOuterRadius * innerRadiusFraction;
            // Never disable the beam at 0%; only the controller handles manual on/off.
        }

        private void OnValidate()
        {
            ValidateSettings();
            if (Application.isPlaying && beam != null) ApplyDegradation();
        }

        private void ValidateSettings()
        {
            batteryPercent = Mathf.Clamp(batteryPercent, 0f, 100f);
            fullBatteryDuration = Mathf.Max(0.1f, fullBatteryDuration);
            minimumIntensity = Mathf.Max(0.01f, minimumIntensity);
            maximumIntensity = Mathf.Max(minimumIntensity, maximumIntensity);
            minimumRange = Mathf.Max(0.1f, minimumRange);
            maximumRange = Mathf.Max(minimumRange, maximumRange);
            innerRadiusFraction = Mathf.Clamp01(innerRadiusFraction);
        }
    }
}
