using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GestorSimulacion : MonoBehaviour
{
    public enum EstadoJuego { PreguntaInicio, ViendoTutorial, Conteo, Jugando, Terminado }
    private EstadoJuego estadoActual = EstadoJuego.PreguntaInicio;

    [Header("UI de Introducción")]
    public GameObject panelPreguntaTutorial;
    public GameObject panelTutorialTexto;
    public TextMeshProUGUI textoConteo;

    [Header("Audios Especiales (Intro)")]
    public AudioSource audioRelajante;     // Música de elevador/relajante
    public AudioSource audioConteoBeep;    // El "Beep" del 3, 2, 1
    public AudioSource audioConteoGo;      // El sonido de "¡ESCAPA!"

    [Header("Configuración de Tiempo")]
    public float tiempoMaximo = 60f;
    private float tiempoActual;
    private bool simulacionActiva = false;

    [Header("Referencias de Interfaz (UI)")]
    public TextMeshProUGUI textoReloj;
    public GameObject panelGameOver;

    [Header("Efectos de Estrés (Audio)")]
    public AudioSource audioTerremoto;
    public float volumenFinal = 1.2f;
    private float volumenInicial;
    private bool surgeIniciado = false;

    [Header("Efectos Visuales (Derrumbe)")]
    public ParticleSystem[] particulasPolvo;

    [Header("Limpieza Final")]
    public AudioSource[] todosLosAudios; // Incluye aquí alarma, cubiertos, etc.

    void Start()
    {
        tiempoActual = tiempoMaximo;
        if (audioTerremoto != null) volumenInicial = audioTerremoto.volume;

        // 1. Silenciamos TODO lo que no sea el audio relajante
        DetenerTodosLosSonidos();

        // 2. Iniciamos la música relajante
        if (audioRelajante != null) audioRelajante.Play();

        // 3. Setup de UI
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelTutorialTexto != null) panelTutorialTexto.SetActive(false);
        if (textoConteo != null) textoConteo.gameObject.SetActive(false);
        if (panelPreguntaTutorial != null) panelPreguntaTutorial.SetActive(true);

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (estadoActual == EstadoJuego.PreguntaInicio)
        {
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonEast.wasPressedThisFrame) // Círculo
                {
                    panelPreguntaTutorial.SetActive(false);
                    IniciarConteo();
                }
                else if (Gamepad.current.buttonSouth.wasPressedThisFrame) // X
                {
                    panelPreguntaTutorial.SetActive(false);
                    panelTutorialTexto.SetActive(true);
                    estadoActual = EstadoJuego.ViendoTutorial;
                }
            }
        }
        else if (estadoActual == EstadoJuego.ViendoTutorial)
        {
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                panelTutorialTexto.SetActive(false);
                IniciarConteo();
            }
        }
        else if (estadoActual == EstadoJuego.Jugando && simulacionActiva)
        {
            // ... (Lógica de tiempo y milésimas que ya tenías)
            ActualizarReloj();
        }
        else if (estadoActual == EstadoJuego.Terminado)
        {
            if (panelGameOver.activeSelf && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                ReiniciarNivel();
            }
        }
    }

    void IniciarConteo()
    {
        estadoActual = EstadoJuego.Conteo;
        StartCoroutine(RutinaConteo());
    }

    IEnumerator RutinaConteo()
    {
        textoConteo.gameObject.SetActive(true);

        // --- CONTEO ESTILO WIPEOUT ---
        for (int i = 3; i > 0; i--)
        {
            textoConteo.text = i.ToString();
            if (audioConteoBeep != null) audioConteoBeep.Play();
            yield return new WaitForSecondsRealtime(1f);
        }

        textoConteo.color = Color.red;
        textoConteo.text = "¡ESCAPA!";
        if (audioConteoGo != null) audioConteoGo.Play();

        // Apagamos la música relajante justo antes de que empiece el caos
        if (audioRelajante != null) audioRelajante.Stop();

        yield return new WaitForSecondsRealtime(1f);
        textoConteo.gameObject.SetActive(false);

        // --- INICIA EL JUEGO REAL ---
        Time.timeScale = 1f;
        simulacionActiva = true;
        estadoActual = EstadoJuego.Jugando;

        // Encendemos el sismo y demás audios de ambiente
        if (audioTerremoto != null) audioTerremoto.Play();
        IniciarAudiosDeAmbiente();
    }

    // Funciones de apoyo
    void ActualizarReloj()
    {
        tiempoActual -= Time.deltaTime;
        int minutos = Mathf.FloorToInt(tiempoActual / 60);
        int segundos = Mathf.FloorToInt(tiempoActual % 60);
        int milesimas = Mathf.FloorToInt((tiempoActual * 100) % 100);
        textoReloj.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milesimas);

        if (tiempoActual <= 3f && !surgeIniciado)
        {
            surgeIniciado = true;
            foreach (ParticleSystem ps in particulasPolvo) { if (ps != null) ps.Play(); }
        }

        if (surgeIniciado && audioTerremoto != null)
        {
            audioTerremoto.volume = Mathf.Lerp(audioTerremoto.volume, volumenFinal, Time.deltaTime * 2f);
        }

        if (tiempoActual <= 0)
        {
            tiempoActual = 0;
            textoReloj.text = "00:00:00";
            TiempoAgotado();
        }
    }

    private void DetenerTodosLosSonidos()
    {
        if (audioTerremoto != null) audioTerremoto.Stop();
        foreach (AudioSource source in todosLosAudios) { if (source != null) source.Stop(); }
    }

    private void IniciarAudiosDeAmbiente()
    {
        foreach (AudioSource source in todosLosAudios) { if (source != null) source.Play(); }
    }

    void TiempoAgotado()
    {
        estadoActual = EstadoJuego.Terminado;
        simulacionActiva = false;
        DetenerTodosLosSonidos();
        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void DetenerReloj()
    {
        estadoActual = EstadoJuego.Terminado;
        simulacionActiva = false;
        DetenerTodosLosSonidos();
        Time.timeScale = 0f;
    }

    void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}