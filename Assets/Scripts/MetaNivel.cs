using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class MetaNivel : MonoBehaviour
{
    [Header("Interfaz de la Meta")]
    public GameObject panelTablaPuntuacion;
    public Transform tituloPanel;
    public Text textoListaObjetos;
    public Text textoPuntajeTotal;
    public GameObject contenedorBotones;

    [Header("El Cuarto Seguro (VR)")]
    [Tooltip("Crea un GameObject vacío lejos del mapa (ej. Y: -50) y arrástralo aquí")]
    public Transform puntoSeguroUI;

    [Header("Referencias del Jugador")]
    public MonoBehaviour scriptMovimiento;
    public SistemaAgarre scriptInventario;

    [Header("Solución Matemática")]
    public Transform jugadorTransform;
    public Transform sillaTransform;
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

    // Variables de control
    private bool sillaSalvada = false;
    private bool nivelTerminado = false;
    private bool esperandoRespuesta = false;

    void Start()
    {
        if (panelTablaPuntuacion != null)
        {
            panelTablaPuntuacion.SetActive(false);
        }

        // Nos aseguramos de que el estado inicial de las puertas sea el correcto
        if (modeloPuertaAbierta != null) modeloPuertaAbierta.SetActive(false);
        if (modeloPuertaCerrada != null) modeloPuertaCerrada.SetActive(true);
    }

    void Update()
    {
        // 1. ESCUCHADOR DE BOTONES FINALES
        if (esperandoRespuesta)
        {
            bool presionoSi = false;
            bool presionoNo = false;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.xKey.wasPressedThisFrame) presionoSi = true;
                if (Keyboard.current.oKey.wasPressedThisFrame) presionoNo = true;
            }

            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame) presionoSi = true; // X
                if (Gamepad.current.buttonEast.wasPressedThisFrame) presionoNo = true;  // Círculo
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

        // 2. LÓGICA DE DETECCIÓN DE META
        if (nivelTerminado == true || jugadorTransform == null || sillaTransform == null)
        {
            return;
        }

        float distanciaSilla = Vector3.Distance(transform.position, sillaTransform.position);
        sillaSalvada = (distanciaSilla <= radioDeMeta);

        float distanciaJugador = Vector3.Distance(transform.position, jugadorTransform.position);
        if (distanciaJugador <= radioDeMeta)
        {
            nivelTerminado = true;

            if (GetComponent<GestorSimulacion>() != null)
            {
                GetComponent<GestorSimulacion>().DetenerReloj();
            }

            TerminarNivel();
        }
    }

    public void RegistrarInteraccionNPC()
    {
        // Solo ejecutamos el cambio si no habíamos hablado con él antes
        if (!habloConFamiliar)
        {
            habloConFamiliar = true;

            // Intercambio de modelos de puerta
            if (modeloPuertaCerrada != null) modeloPuertaCerrada.SetActive(false);

            if (modeloPuertaAbierta != null)
            {
                modeloPuertaAbierta.SetActive(true);

                // Sonido espacial "fantasma" en la posición de la puerta
                if (sonidoAbrirPuerta != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoAbrirPuerta, modeloPuertaAbierta.transform.position, 1f);
                }
            }
        }
    }

    void TerminarNivel()
    {
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        if (puntoSeguroUI != null)
        {
            jugadorTransform.position = puntoSeguroUI.position;
            jugadorTransform.rotation = puntoSeguroUI.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string textoRecibo = CalcularTextoFinal();
        int puntosExtra = CalcularPuntosExtra();
        int total = scriptInventario.puntuacionTotal + puntosExtra;

        StartCoroutine(AnimacionFinalJuicy(textoRecibo, total));
    }

    string CalcularTextoFinal()
    {
        string texto = "";

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

    int CalcularPuntosExtra()
    {
        int extra = 0;
        if (habloConFamiliar == true) extra += 500; else extra -= 2000;
        if (sillaSalvada == true) extra += 1000; else extra -= 1000;
        return extra;
    }

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

        if (reproductorAudioMeta != null && reproductorAudioMeta.clip != null)
        {
            reproductorAudioMeta.Play();
        }

        if (reproductorAudioMeta != null && sonidoAparicionTitulo != null)
        {
            reproductorAudioMeta.PlayOneShot(sonidoAparicionTitulo);
        }

        float tiempo = 0;
        float duracionPop = 0.5f;

        while (tiempo < duracionPop)
        {
            tiempo += Time.unscaledDeltaTime;
            tituloPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, tiempo / duracionPop));
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        foreach (char letra in textoRecibo)
        {
            textoListaObjetos.text += letra;
            yield return new WaitForSecondsRealtime(letra == '\n' ? 0.15f : 0.01f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

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