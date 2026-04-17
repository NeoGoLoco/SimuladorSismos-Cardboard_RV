using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

// Este script gestiona el final de la experiencia: detecta la llegada a la zona segura,
// evalúa el desempeño ético del usuario y presenta los resultados finales en VR.
public class MetaNivel : MonoBehaviour
{
    [Header("Interfaz de la Meta")]
    public GameObject panelTablaPuntuacion;
    public Transform tituloPanel;
    public Text textoListaObjetos;
    public Text textoPuntajeTotal;
    public GameObject contenedorBotones;

    [Header("El Cuarto Seguro (VR)")]
    // Espacio reservado para teletransportar la cámara y mostrar la UI sin obstrucciones del mapa
    [Tooltip("Crea un GameObject vacío lejos del mapa (ej. Y: -50) y arrástralo aquí")]
    public Transform puntoSeguroUI;

    [Header("Referencias del Jugador")]
    public MonoBehaviour scriptMovimiento;
    public SistemaAgarre scriptInventario;

    [Header("Solución Matemática")]
    public Transform jugadorTransform;
    public Transform sillaTransform; // Representa al familiar o el objetivo a escoltar
    public float radioDeMeta = 3.0f;

    [Header("Lógica de Empatía / Puerta")]
    public bool habloConFamiliar = false;
    public GameObject modeloPuertaCerrada;
    public GameObject modeloPuertaAbierta;
    public AudioClip sonidoAbrirPuerta;

    [Header("Audio Game Juice")]
    public AudioSource reproductorAudioMeta;
    public AudioClip sonidoAparicionTitulo;
    public AudioClip sonidoPuntuacionFinal;

    // Controladores de estado interno
    private bool sillaSalvada = false;
    private bool nivelTerminado = false;
    private bool esperandoRespuesta = false;

    void Start()
    {
        // Nos aseguramos de que la tabla de resultados esté oculta al iniciar
        if (panelTablaPuntuacion != null)
        {
            panelTablaPuntuacion.SetActive(false);
        }

        // Estado inicial de la ruta de salida: debe empezar bloqueada
        if (modeloPuertaAbierta != null) modeloPuertaAbierta.SetActive(false);
        if (modeloPuertaCerrada != null) modeloPuertaCerrada.SetActive(true);
    }

    void Update()
    {
        // Si el nivel ya terminó, pasamos a escuchar la entrada para reiniciar o salir
        if (esperandoRespuesta)
        {
            bool presionoSi = false;
            bool presionoNo = false;

            // Soporte para teclado (X para sí, O para no)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.xKey.wasPressedThisFrame) presionoSi = true;
                if (Keyboard.current.oKey.wasPressedThisFrame) presionoNo = true;
            }

            // Soporte para Gamepad (X/South para sí, Círculo/East para no)
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame) presionoSi = true; 
                if (Gamepad.current.buttonEast.wasPressedThisFrame) presionoNo = true;  
            }

            if (presionoSi)
            {
                esperandoRespuesta = false;
                ReiniciarJuego();
            }
            else if (presionoNo)
            {
                esperandoRespuesta = false;
                SalirDelJuego();
            }

            return;
        }

        // Monitoreo constante de la zona de meta
        if (nivelTerminado == true || jugadorTransform == null || sillaTransform == null)
        {
            return;
        }

        // Calculamos si el familiar logró entrar en el radio de seguridad
        float distanciaSilla = Vector3.Distance(transform.position, sillaTransform.position);
        sillaSalvada = (distanciaSilla <= radioDeMeta);

        // Verificamos si el jugador mismo alcanzó el objetivo
        float distanciaJugador = Vector3.Distance(transform.position, jugadorTransform.position);
        if (distanciaJugador <= radioDeMeta)
        {
            nivelTerminado = true;

            // Frenamos el cronómetro global antes de pasar a la pantalla final
            if (GetComponent<GestorSimulacion>() != null)
            {
                GetComponent<GestorSimulacion>().DetenerReloj();
            }

            TerminarNivel();
        }
    }

    // Se dispara cuando el jugador completa la interacción de diálogo con el NPC
    public void RegistrarInteraccionNPC()
    {
        if (!habloConFamiliar)
        {
            habloConFamiliar = true;

            // Feedback visual: abrimos la puerta para permitir la evacuación
            if (modeloPuertaCerrada != null) modeloPuertaCerrada.SetActive(false);

            if (modeloPuertaAbierta != null)
            {
                modeloPuertaAbierta.SetActive(true);

                // Disparamos un sonido espacial en la ubicación física de la puerta
                if (sonidoAbrirPuerta != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoAbrirPuerta, modeloPuertaAbierta.transform.position, 1f);
                }
            }
        }
    }

    // Prepara el entorno para mostrar los resultados sin distracciones
    void TerminarNivel()
    {
        // Inmovilizamos al jugador
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        // Movemos la cámara al cuarto seguro para una lectura de UI más cómoda
        if (puntoSeguroUI != null)
        {
            jugadorTransform.position = puntoSeguroUI.position;
            jugadorTransform.rotation = puntoSeguroUI.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Recopilamos los datos del inventario y los bonos éticos
        string textoRecibo = CalcularTextoFinal();
        int puntosExtra = CalcularPuntosExtra();
        int total = scriptInventario.puntuacionTotal + puntosExtra;

        // Iniciamos la presentación secuencial de la puntuación
        StartCoroutine(AnimacionFinalJuicy(textoRecibo, total));
    }

    // Construye la cadena de texto con el desglose de acciones y puntos
    string CalcularTextoFinal()
    {
        string texto = "";

        // Listamos los suministros rescatados del inventario
        if (scriptInventario != null && scriptInventario.desglosePuntuacion.Count > 0)
        {
            foreach (string renglon in scriptInventario.desglosePuntuacion)
            {
                texto += renglon + "\n";
            }
        }
        else
        {
            texto = "No recogiste ningún objeto de valor.\n";
        }

        // Evaluación de conducta social y protocolos de seguridad
        texto += "\n--- EVALUACIÓN DE PROTOCOLO ---\n";

        if (habloConFamiliar == true)
        {
            texto += "Aviso a terceros: CUMPLIDO (+500 pts)\n";
        }
        else
        {
            texto += "ALERTA: Abandono de civil. PENALIZACIÓN (-2000 pts)\n";
        }

        texto += "\n--- REPORTE DE RESCATE ---\n";

        if (sillaSalvada == true)
        {
            texto += "Evacuación de familiar: ÉXITO (+1000 pts)";
        }
        else
        {
            texto += "Evacuación de familiar: FALLIDA (-1000 pts)";
        }

        return texto;
    }

    // Lógica matemática pura para los bonos de la meta
    int CalcularPuntosExtra()
    {
        int extra = 0;
        if (habloConFamiliar == true) extra += 500; else extra -= 2000;
        if (sillaSalvada == true) extra += 1000; else extra -= 1000;
        return extra;
    }

    // Controla la coreografía visual de la tabla de puntuación (ignora la pausa del juego)
    IEnumerator AnimacionFinalJuicy(string textoRecibo, int puntajeTotal)
    {
        panelTablaPuntuacion.SetActive(true);

        if (contenedorBotones != null)
        {
            contenedorBotones.SetActive(false);
        }

        textoListaObjetos.text = "";
        textoPuntajeTotal.text = "";
        tituloPanel.localScale = Vector3.zero;

        // Iniciamos la ambientación sonora de la meta
        if (reproductorAudioMeta != null && reproductorAudioMeta.clip != null)
        {
            reproductorAudioMeta.Play();
        }

        if (reproductorAudioMeta != null && sonidoAparicionTitulo != null)
        {
            reproductorAudioMeta.PlayOneShot(sonidoAparicionTitulo);
        }

        // Efecto visual de entrada para el título (Pop-up)
        float tiempo = 0;
        float duracionPop = 0.5f;

        while (tiempo < duracionPop)
        {
            tiempo += Time.unscaledDeltaTime;
            tituloPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, tiempo / duracionPop));
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Imprimimos el desglose letra por letra para un efecto de reporte en tiempo real
        foreach (char letra in textoRecibo)
        {
            textoListaObjetos.text += letra;
            yield return new WaitForSecondsRealtime(letra == '\n' ? 0.15f : 0.01f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Revelación del puntaje final con sonido de impacto
        if (reproductorAudioMeta != null && sonidoPuntuacionFinal != null)
        {
            reproductorAudioMeta.PlayOneShot(sonidoPuntuacionFinal);
        }

        string textoFinal = "PUNTUACIÓN TOTAL: " + puntajeTotal + " PTS";

        foreach (char letra in textoFinal)
        {
            textoPuntajeTotal.text += letra;
            yield return new WaitForSecondsRealtime(0.03f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Habilitamos los botones una vez terminada la animación
        if (contenedorBotones != null)
        {
            contenedorBotones.SetActive(true);
        }

        esperandoRespuesta = true;
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}