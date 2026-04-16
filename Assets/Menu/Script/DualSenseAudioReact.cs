using UnityEngine;
using UnityEngine.InputSystem.DualShock;

public class DualSenseAudioReact : MonoBehaviour
{
    public enum RangoFrecuencia { Bajos, Medios, Agudos }

    [Header("Referencias")]
    public AudioSource musicaBase;

    [Header("Configuración del Ritmo")]
    public RangoFrecuencia frecuenciaAEscuchar = RangoFrecuencia.Medios;
    public float sensibilidad = 80f;
    public float velocidadSuavizado = 15f;

    [Header("Colores (Base vs Golpe)")]
    public Color colorTranquilo = Color.black;
    public Color colorGolpe = Color.cyan;

    private float[] samplesAudio = new float[512];
    private Color colorActualLED;

    void Start()
    {
        if (musicaBase == null) musicaBase = GetComponent<AudioSource>();
        colorActualLED = colorTranquilo;
    }

    void Update()
    {
        // CÓDIGO EXCLUSIVO PARA PC (Se ignora en Android)
#if UNITY_EDITOR || UNITY_STANDALONE
        var dualSense = DualSenseGamepadHID.current;

        if (dualSense == null || musicaBase == null || !musicaBase.isPlaying) return;

        musicaBase.GetSpectrumData(samplesAudio, 0, FFTWindow.Blackman);

        float promedioEnergia = 0f;

        switch (frecuenciaAEscuchar)
        {
            case RangoFrecuencia.Bajos:
                for (int i = 0; i < 10; i++) promedioEnergia += samplesAudio[i];
                promedioEnergia /= 10f;
                break;

            case RangoFrecuencia.Medios:
                for (int i = 10; i < 250; i++) promedioEnergia += samplesAudio[i];
                promedioEnergia /= 240f;
                break;

            case RangoFrecuencia.Agudos:
                for (int i = 250; i < 512; i++) promedioEnergia += samplesAudio[i];
                promedioEnergia /= 262f;
                break;
        }

        float intensidad = Mathf.Clamp01(promedioEnergia * sensibilidad);
        Color colorDestino = Color.Lerp(colorTranquilo, colorGolpe, intensidad);
        colorActualLED = Color.Lerp(colorActualLED, colorDestino, Time.deltaTime * velocidadSuavizado);

        dualSense.SetLightBarColor(colorActualLED);
#endif
    }
}