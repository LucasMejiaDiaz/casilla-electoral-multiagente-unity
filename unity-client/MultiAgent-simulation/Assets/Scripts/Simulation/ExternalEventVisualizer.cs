using UnityEngine;
using UnityEngine.UI;

namespace PollingStation.Simulation
{
    public sealed class ExternalEventVisualizer : MonoBehaviour
    {
        [SerializeField] private Light mainLight;
        [SerializeField] private GameObject banner;
        [SerializeField] private Text bannerText;
        private float normalLightIntensity = 1f;

        public void Configure(Light lightSource, GameObject eventBanner, Text eventText)
        {
            mainLight = lightSource;
            banner = eventBanner;
            bannerText = eventText;
            normalLightIntensity = mainLight == null ? 1f : mainLight.intensity;
            Apply(null, false);
        }

        public void Apply(ExternalEventSnapshot externalEvent, bool paused)
        {
            bool active = externalEvent != null && externalEvent.active;
            if (banner != null)
            {
                banner.SetActive(active || paused);
            }

            string kind = externalEvent == null ? "" : externalEvent.kind;
            if (bannerText != null)
            {
                float remaining = externalEvent == null ? 0f : externalEvent.remaining;
                bannerText.text = active
                    ? $"EVENTO EXTERNO: {FriendlyName(kind)}  |  {remaining:0.0} min restantes"
                    : "SIMULACIÓN EN PAUSA";
            }

            if (mainLight != null)
            {
                mainLight.intensity = active && kind == "corte_de_luz"
                    ? normalLightIntensity * 0.15f
                    : normalLightIntensity;
            }
        }

        private static string FriendlyName(string kind)
        {
            switch (kind)
            {
                case "corte_de_luz": return "CORTE DE LUZ";
                case "temblor": return "TEMBLOR";
                case "aguacero": return "AGUACERO";
                default: return string.IsNullOrWhiteSpace(kind) ? "INTERRUPCIÓN" : kind.ToUpperInvariant();
            }
        }
    }
}
