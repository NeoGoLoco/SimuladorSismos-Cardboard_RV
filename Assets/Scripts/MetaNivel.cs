using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para usar Corrutinas

public class MetaNivel : MonoBehaviour
{
    [Header("Interfaz de la Meta")]
    public GameObject panelTablaPuntuacion;
    public Text textoListaObjetos;
    public Text textoPuntajeTotal;

    [Header("Referencias del Jugador")]
    public MonoBehaviour scriptMovimiento;
    public SistemaAgarre scriptInventario;

    [Header("Lógica de Empatía")]
    public bool habloConFamiliar = false; // Se activará con los botones de diálogo

    // Variable para rastrear la silla
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
            // AQUÍ NO DETENEMOS EL RELOJ, SOLO MARCAMOS QUE ESTÁ A SALVO
        }

        // Detectar si el jugador entra a la meta
        if (otro.CompareTag("Player"))
        {
            GetComponent<GestorSimulacion>().DetenerReloj(); // Congela el universo
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

    // --- NUEVA FUNCIÓN: Llama a esta desde los botones de diálogo ---
    public void RegistrarInteraccionNPC()
    {
        habloConFamiliar = true;
        Debug.Log("Interacción con NPC registrada. Penalización anulada.");
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

            // --- NUEVO REPORTE DE PROTOCOLO DE COMUNICACIÓN ---
            textoArmado += "\n--- EVALUACIÓN DE PROTOCOLO ---\n";

            if (habloConFamiliar)
            {
                textoArmado += "Aviso a terceros: CUMPLIDO (+500 pts)\n";
                puntajeFinalCalculado += 500;
            }
            else
            {
                // Castigo severo por abandonar al NPC
                textoArmado += "ALERTA: Abandono de civil en zona de riesgo.\n";
                textoArmado += "PENALIZACIÓN: Falta de comunicación ética (-2000 pts)\n";
                puntajeFinalCalculado -= 2000;
            }

            // --- EVALUACIÓN DEL RESCATE FÍSICO ---
            textoArmado += "\n--- REPORTE DE RESCATE ---\n";

            if (sillaSalvada)
            {
                textoArmado += "Evacuación de familiar: ÉXITO (+1000 pts)";
                puntajeFinalCalculado += 1000;
            }
            else
            {
                textoArmado += "Evacuación de familiar: FALLIDA - No llegaste con ella al punto (-1000 pts)";
                puntajeFinalCalculado -= 1000;
            }

            // Mostramos el texto y el puntaje final calculado
            if (textoListaObjetos != null) textoListaObjetos.text = textoArmado;
            if (textoPuntajeTotal != null) textoPuntajeTotal.text = "PUNTUACIÓN TOTAL: " + puntajeFinalCalculado + " PTS";
        }

        // Iniciamos el conteo en tiempo real (Corrutina)
        StartCoroutine(RutinaReinicio(15f));
    }

    // Esta es la rutina especial que ignora que el juego esté congelado
    IEnumerator RutinaReinicio(float tiempoEspera)
    {
        yield return new WaitForSecondsRealtime(tiempoEspera);

        // Importante: descongelar el tiempo antes de recargar la escena
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}