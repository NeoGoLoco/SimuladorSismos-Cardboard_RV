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
    public Transform puntoSeguroUI;

    [Header("Referencias del Jugador")]
    public MonoBehaviour scriptMovimiento;
    public SistemaAgarre scriptInventario;

    [Header("Solución Matemática")]
    public Transform jugadorTransform;
    public Transform sillaTransform;
    public float radioDeMeta = 3.0f;

    [Header("Lógica de Empatía")]
    public bool habloConFamiliar = false;

    private bool sillaSalvada = false;
    private bool nivelTerminado = false;
    private bool esperandoRespuesta = false;

    void Start()
    {
        if (panelTablaPuntuacion != null) panelTablaPuntuacion.SetActive(false);
    }

    void Update()
    {
        // 1. ESCUCHADOR DE BOTONES FINALES (Usando el New Input System)
        if (esperandoRespuesta)
        {
            bool presionoSi = false;
            bool presionoNo = false;

            // Revisamos el teclado (para pruebas en PC)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.xKey.wasPressedThisFrame) presionoSi = true;
                if (Keyboard.current.oKey.wasPressedThisFrame) presionoNo = true;
            }

            // Revisamos el control Bluetooth / PS5
            if (Gamepad.current != null)
            {
                // buttonSouth = X en PS5, A en Xbox
                if (Gamepad.current.buttonSouth.wasPressedThisFrame) presionoSi = true;

                // buttonEast = Círculo en PS5, B en Xbox
                if (Gamepad.current.buttonEast.wasPressedThisFrame) presionoNo = true;
            }

            // Ejecutamos la acción
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
            return; // Ignora todo el código de abajo si estamos en el menú final
        }

        // 2. LÓGICA DE DETECCIÓN DE META (Sin cambios)
        if (nivelTerminado || jugadorTransform == null || sillaTransform == null) return;

        float distanciaSilla = Vector3.Distance(transform.position, sillaTransform.position);
        sillaSalvada = (distanciaSilla <= radioDeMeta);

        float distanciaJugador = Vector3.Distance(transform.position, jugadorTransform.position);

        if (distanciaJugador <= radioDeMeta)
        {
            nivelTerminado = true;
            if (GetComponent<GestorSimulacion>() != null)
                GetComponent<GestorSimulacion>().DetenerReloj();

            TerminarNivel();
        }
    }

    public void RegistrarInteraccionNPC()
    {
        habloConFamiliar = true;
    }

    void TerminarNivel()
    {
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        // Teletransporte al Cuarto Seguro para evitar ver el interior de los modelos
        if (puntoSeguroUI != null)
        {
            jugadorTransform.position = puntoSeguroUI.position;
            jugadorTransform.rotation = puntoSeguroUI.rotation;
        }

        // Mostramos el panel pero con los elementos vacíos para la animación
        string textoRecibo = CalcularTextoFinal();
        StartCoroutine(AnimacionFinalJuicy(textoRecibo, scriptInventario.puntuacionTotal + CalcularPuntosExtra()));
    }

    string CalcularTextoFinal()
    {
        string texto = "";
        if (scriptInventario != null && scriptInventario.desglosePuntuacion.Count > 0)
        {
            foreach (string renglon in scriptInventario.desglosePuntuacion)
                texto += renglon + "\n";
        }
        else texto = "No recogiste ningún objeto de valor.\n";

        texto += "\n--- EVALUACIÓN DE PROTOCOLO ---\n";
        if (habloConFamiliar) texto += "Aviso a terceros: CUMPLIDO (+500 pts)\n";
        else texto += "ALERTA: Abandono de civil. PENALIZACIÓN (-2000 pts)\n";

        texto += "\n--- REPORTE DE RESCATE ---\n";
        if (sillaSalvada) texto += "Evacuación de familiar: ÉXITO (+1000 pts)";
        else texto += "Evacuación de familiar: FALLIDA (-1000 pts)";

        return texto;
    }

    int CalcularPuntosExtra()
    {
        int extra = 0;
        if (habloConFamiliar) extra += 500; else extra -= 2000;
        if (sillaSalvada) extra += 1000; else extra -= 1000;
        return extra;
    }

    IEnumerator AnimacionFinalJuicy(string textoRecibo, int puntajeTotal)
    {
        panelTablaPuntuacion.SetActive(true);
        contenedorBotones.SetActive(false);
        textoListaObjetos.text = "";
        textoPuntajeTotal.text = "";
        tituloPanel.localScale = Vector3.zero;

        // 1. POP TÍTULO
        float tiempo = 0;
        float duracionPop = 0.5f;
        while (tiempo < duracionPop)
        {
            tiempo += Time.unscaledDeltaTime;
            tituloPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, tiempo / duracionPop));
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 2. MÁQUINA DE ESCRIBIR (Recibo)
        foreach (char letra in textoRecibo)
        {
            textoListaObjetos.text += letra;
            yield return new WaitForSecondsRealtime(letra == '\n' ? 0.15f : 0.01f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 3. MÁQUINA DE ESCRIBIR (Puntaje)
        string textoFinal = "PUNTUACIÓN TOTAL: " + puntajeTotal + " PTS";
        foreach (char letra in textoFinal)
        {
            textoPuntajeTotal.text += letra;
            yield return new WaitForSecondsRealtime(0.03f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 4. ACTIVAR ESCUCHA DE CONTROL
        contenedorBotones.SetActive(true);
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