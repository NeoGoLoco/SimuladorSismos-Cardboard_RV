using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GestorSimulacion : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    public float tiempoMaximo = 60f;
    private float tiempoActual;
    private bool simulacionActiva = true;

    [Header("Referencias de Interfaz (UI)")]
    public TextMeshProUGUI textoReloj;
    public GameObject panelGameOver;

    [Header("Efectos de Estrés (Audio)")]
    public AudioSource audioTerremoto;
    public float volumenFinal = 1.2f; // Volumen al que llegará al final
    private float volumenInicial;
    private bool surgeIniciado = false;

    [Header("Efectos Visuales (Derrumbe)")]
    public ParticleSystem[] particulasPolvo; // Arrastra aquí tus sistemas de partículas

    [Header("Limpieza Final")]
    public AudioSource[] todosLosAudios; // Arrastra todos tus AudioSources aquí

    void Start()
    {
        tiempoActual = tiempoMaximo;
        if (audioTerremoto != null) volumenInicial = audioTerremoto.volume;

        if (panelGameOver != null) panelGameOver.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (simulacionActiva)
        {
            tiempoActual -= Time.deltaTime;

            // --- CÁLCULO DE MILÉSIMAS ---
            int minutos = Mathf.FloorToInt(tiempoActual / 60);
            int segundos = Mathf.FloorToInt(tiempoActual % 60);
            int milesimas = Mathf.FloorToInt((tiempoActual * 100) % 100); // 2 dígitos para que sea legible

            // Formato: 00:00:00
            textoReloj.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milesimas);

            // --- MOMENTO DE PÁNICO (3 segundos antes) ---
            if (tiempoActual <= 3f && !surgeIniciado)
            {
                surgeIniciado = true;

                // ¡ACTIVER LAS PARTÍCULAS AQUÍ! 
                // Esto sucede justo cuando el reloj marca 00:03:00
                foreach (ParticleSystem ps in particulasPolvo)
                {
                    if (ps != null) ps.Play();
                }
            }

            if (surgeIniciado && audioTerremoto != null)
            {
                // El audio sube y, por consecuencia (FFT), la cámara se sacude más fuerte
                audioTerremoto.volume = Mathf.Lerp(audioTerremoto.volume, volumenFinal, Time.deltaTime * 2f);
            }

            if (tiempoActual <= 0)
            {
                tiempoActual = 0;
                textoReloj.text = "00:00:00";
                TiempoAgotado();
            }
        }
        else if (panelGameOver.activeSelf)
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                ReiniciarNivel();
            }
        }
    }

    void TiempoAgotado()
    {
        simulacionActiva = false;

        DetenerTodosLosSonidos();
        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void DetenerReloj()
    {
        simulacionActiva = false;
        DetenerTodosLosSonidos();
        Time.timeScale = 0f;
    }

    private void DetenerTodosLosSonidos()
    {
        foreach (AudioSource source in todosLosAudios)
        {
            source.Stop();
        }
    }

    void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}