using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

// Este script es el cerebro que maneja todo el ciclo de la simulación:
// controla cuándo empieza el tutorial, cuándo el conteo, y gestiona
// la intensidad del sismo basándose en el tiempo restante.
public class GestorSimulacion : MonoBehaviour
{
    // Definimos los posibles momentos de la experiencia
    public enum EstadoJuego { PreguntaInicio, ViendoTutorial, Conteo, Jugando, Terminado }
    private EstadoJuego estadoActual = EstadoJuego.PreguntaInicio;

    [Header("UI de Introducción")]
    public GameObject panelPreguntaTutorial;
    public GameObject panelTutorialTexto;
    public TextMeshProUGUI textoConteo;

    [Header("Audios Especiales (Intro)")]
    // Usamos música relajante al inicio para crear contraste con el caos posterior
    public AudioSource audioRelajante;     
    public AudioSource audioConteoBeep;    
    public AudioSource audioConteoGo;      

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
    // Arrastramos aquí todos los sonidos que deban callarse al terminar (alarma, ambiente, etc.)
    public AudioSource[] todosLosAudios; 

    void Start()
    {
        // Setup inicial: reloj al máximo y guardamos el volumen base del sismo
        tiempoActual = tiempoMaximo;
        if (audioTerremoto != null) volumenInicial = audioTerremoto.volume;

        // Preparamos el ambiente silencioso y solo encendemos la música relajante
        DetenerTodosLosSonidos();
        if (audioRelajante != null) audioRelajante.Play();

        // Aseguramos que solo el panel de inicio sea visible
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelTutorialTexto != null) panelTutorialTexto.SetActive(false);
        if (textoConteo != null) textoConteo.gameObject.SetActive(false);
        if (panelPreguntaTutorial != null) panelPreguntaTutorial.SetActive(true);

        // Congelamos el tiempo del juego (física, animaciones normales) 
        // para que nada se mueva en el menú.
        Time.timeScale = 0f;
    }

    void Update()
    {
        // Dependiendo del estado, escuchamos inputs o actualizamos el reloj
        if (estadoActual == EstadoJuego.PreguntaInicio)
        {
            // Esperamos a que el jugador elija ver tutorial o empezar
            ManejarEntradaMenuInicio();
        }
        else if (estadoActual == EstadoJuego.ViendoTutorial)
        {
            ManejarEntradaTutorial();
        }
        else if (estadoActual == EstadoJuego.Jugando && simulacionActiva)
        {
            // El núcleo del juego: reducir tiempo y aumentar estrés
            ActualizarReloj();
        }
        else if (estadoActual == EstadoJuego.Terminado)
        {
            ManejarEntradaGameOver();
        }
    }

    // --- LÓGICA DE MENÚS Y ENTRADA (GAMEPAD) ---

    private void ManejarEntradaMenuInicio()
    {
        if (Gamepad.current == null) return;

        // Círculo: El jugador salta el tutorial e inicia el conteo
        if (Gamepad.current.buttonEast.wasPressedThisFrame) 
        {
            panelPreguntaTutorial.SetActive(false);
            IniciarConteo();
        }
        // X: El jugador decide leer las instrucciones primero
        else if (Gamepad.current.buttonSouth.wasPressedThisFrame) 
        {
            panelPreguntaTutorial.SetActive(false);
            panelTutorialTexto.SetActive(true);
            estadoActual = EstadoJuego.ViendoTutorial;
        }
    }

    private void ManejarEntradaTutorial()
    {
        // Al terminar de leer, presionan Círculo para iniciar
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            panelTutorialTexto.SetActive(false);
            IniciarConteo();
        }
    }

    private void ManejarEntradaGameOver()
    {
        // En la pantalla final, X sirve para reiniciar la escena
        if (panelGameOver.activeSelf && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            ReiniciarNivel();
        }
    }

    // --- FLUJO DEL SISMO ---

    void IniciarConteo()
    {
        estadoActual = EstadoJuego.Conteo;
        StartCoroutine(RutinaConteo());
    }

    // Esta rutina maneja el conteo regresivo ignorando la pausa del juego
    IEnumerator RutinaConteo()
    {
        textoConteo.gameObject.SetActive(true);

        // Hacemos el 3, 2, 1 usando Realtime porque el TimeScale sigue en 0
        for (int i = 3; i > 0; i--)
        {
            textoConteo.text = i.ToString();
            if (audioConteoBeep != null) audioConteoBeep.Play();
            yield return new WaitForSecondsRealtime(1f); 
        }

        // Momento de impacto: UI en rojo y sonido de "GO"
        textoConteo.color = Color.red;
        textoConteo.text = "¡ESCAPA!";
        if (audioConteoGo != null) audioConteoGo.Play();

        // Apagamos la calma (música relajante)
        if (audioRelajante != null) audioRelajante.Stop();

        yield return new WaitForSecondsRealtime(1f);
        textoConteo.gameObject.SetActive(false);

        // --- INICIA LA SIMULACIÓN FÍSICA ---
        // Devolvemos el tiempo a la normalidad (activando NavMesh, sismo físico, etc.)
        Time.timeScale = 1f;
        simulacionActiva = true;
        estadoActual = EstadoJuego.Jugando;

        // Encendemos el audio del terremoto y el ambiente de caos
        if (audioTerremoto != null) audioTerremoto.Play();
        IniciarAudiosDeAmbiente();
    }

    // Maneja el temporizador y activa los efectos finales de estrés
    void ActualizarReloj()
    {
        // Restamos tiempo usando deltaTime normal
        tiempoActual -= Time.deltaTime;
        
        // Formateamos para mostrar Minutos:Segundos:Milésimas
        int minutos = Mathf.FloorToInt(tiempoActual / 60);
        int segundos = Mathf.FloorToInt(tiempoActual % 60);
        int milesimas = Mathf.FloorToInt((tiempoActual * 100) % 100);
        textoReloj.text = string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milesimas);

        // Disparador crítico: cuando quedan 3 segundos, inicia el colapso final
        if (tiempoActual <= 3f && !surgeIniciado)
        {
            surgeIniciado = true;
            // Activamos las partículas de derrumbe (polvo)
            foreach (ParticleSystem ps in particulasPolvo) { if (ps != null) ps.Play(); }
        }

        // Si ya inició el colapso, subimos agresivamente el volumen del sismo
        if (surgeIniciado && audioTerremoto != null)
        {
            // Hacemos un Lerp hacia el volumen final para aumentar el pánico
            audioTerremoto.volume = Mathf.Lerp(audioTerremoto.volume, volumenFinal, Time.deltaTime * 2f);
        }

        // Si el tiempo llega a cero, forzamos el final
        if (tiempoActual <= 0)
        {
            tiempoActual = 0;
            textoReloj.text = "00:00:00";
            TiempoAgotado();
        }
    }

    // --- LIMPIEZA Y FINALIZACIÓN ---

    private void DetenerTodosLosSonidos()
    {
        if (audioTerremoto != null) audioTerremoto.Stop();
        foreach (AudioSource source in todosLosAudios) { if (source != null) source.Stop(); }
    }

    private void IniciarAudiosDeAmbiente()
    {
        foreach (AudioSource source in todosLosAudios) { if (source != null) source.Play(); }
    }

    // Se llama cuando el tiempo llega a cero
    void TiempoAgotado()
    {
        TerminarLógicaDeSimulacion();
        panelGameOver.SetActive(true);
    }

    // Se llama externamente si el jugador llega a la meta
    public void DetenerReloj()
    {
        TerminarLógicaDeSimulacion();
    }

    // Lógica común para detener el caos (NavMesh, Audio y Tiempo)
    private void TerminarLógicaDeSimulacion()
    {
        estadoActual = EstadoJuego.Terminado;
        simulacionActiva = false;
        DetenerTodosLosSonidos();
        // Volvemos a congelar el mundo para la pantalla de puntuación
        Time.timeScale = 0f; 
    }

    void ReiniciarNivel()
    {
        // Importante: devolver el tiempo a 1 antes de recargar
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}