using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MetaNivel : MonoBehaviour
{
    [Header("Interfaz de la Meta")]
    public GameObject panelTablaPuntuacion;
    public Text textoListaObjetos;
    public Text textoPuntajeTotal;

    [Header("Referencias del Jugador")]
    public MonoBehaviour scriptMovimiento;
    public SistemaAgarre scriptInventario;

    // Variable para rastrear la silla - Si está ya en la meta... O no.
    private bool sillaSalvada = false;

    void Start()
    {
        if (panelTablaPuntuacion != null) panelTablaPuntuacion.SetActive(false);
    }

    void OnTriggerEnter(Collider otro)
    {
        // Detectar si la silla entra a la meta
        if (otro.CompareTag("SillaRuedas"))
        {
            sillaSalvada = true;
        }

        // Detectar si el jugador entra a la meta
        if (otro.CompareTag("Player"))
        {
            TerminarNivel();
        }
    }

    // Detectar si la silla sale por error antes de que acabe el juego
    void OnTriggerExit(Collider otro)
    {
        if (otro.CompareTag("SillaRuedas"))
        {
            sillaSalvada = false;
        }
    }

    void TerminarNivel()
    {
        // Detenemos al jugador
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        // Mostramos la interfaz
        if (panelTablaPuntuacion != null) panelTablaPuntuacion.SetActive(true);

        // Liberamos el ratón
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Llenamos los datos del recibo
        if (scriptInventario != null)
        {
            string textoArmado = "";
            // Tomamos los puntos base que ya recolectaste con la mochila
            int puntajeFinalCalculado = scriptInventario.puntuacionTotal;

            if (scriptInventario.desglosePuntuacion.Count > 0)
            {
                foreach (string renglon in scriptInventario.desglosePuntuacion)
                {
                    textoArmado += renglon + "\n";
                }
            }
            else
            {
                textoArmado = "No recogiste ningún objeto de valor.\n";
            }

            // Agregamos la evaluación del rescate (La abuelaxd)
            textoArmado += "\n--- REPORTE DE RESCATE ---\n";

            if (sillaSalvada)
            {
                textoArmado += "Evacuación de abuela: ÉXITO (+1000 pts)";
                puntajeFinalCalculado += 1000;
            }
            else
            {
                textoArmado += "Evacuación de abuela: FALLIDA - No llegaste con ella al punto de reunión (-1000 pts)";
                puntajeFinalCalculado -= 1000;
            }

            // Mostramos el texto y el puntaje final calculado
            if (textoListaObjetos != null) textoListaObjetos.text = textoArmado;
            if (textoPuntajeTotal != null) textoPuntajeTotal.text = "PUNTUACIÓN TOTAL: " + puntajeFinalCalculado + " PTS";
        }

        // Llama a la función "ReiniciarJuego" después de 15 segundos.
        Invoke("ReiniciarJuego", 15f);
    }

    void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}