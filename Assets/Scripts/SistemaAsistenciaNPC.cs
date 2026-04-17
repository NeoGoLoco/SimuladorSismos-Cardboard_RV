using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class SistemaAsistenciaNPC : MonoBehaviour
{
    [Header("Referencias del NPC")]
    public NavMeshAgent agenteNPC;
    public Transform transformNPC;
    public Animator animadorNPC; 

    [Header("Interacción por Proximidad")]
    public Transform transformJugador;
    public float distanciaInteraccion = 3f;

    private bool dialogoActivo = false;
    private bool yaRespondio = false;

    [Header("Puntos de Destino (Waypoints)")]
    public Transform puntoVentanal;
    public Transform puntoColumnaSegura;
    public Transform puntoColumnaMala;

    [Header("Interfaz y Cámara")]
    public GameObject panelDialogosUI;
    public GameObject botonOpcionCentro;
    public MonoBehaviour scriptCamaraJugador;

    [Header("Conexión con Puntuación Final")]
    public SistemaAgarre inventarioJugador;

    private ControladorTerremoto scriptMovimiento;

    void Start()
    {
        if (panelDialogosUI != null) panelDialogosUI.SetActive(false);

        if (transformJugador != null)
        {
            scriptMovimiento = transformJugador.GetComponent<ControladorTerremoto>();
        }
    }

    void Update()
    {
        // <-- NUEVO: Le enviamos la velocidad del agente al Animator todo el tiempo
        if (agenteNPC != null && animadorNPC != null)
        {
            animadorNPC.SetFloat("Speed", agenteNPC.velocity.magnitude);
        }

        if (yaRespondio || transformJugador == null || transformNPC == null) return;

        float distancia = Vector3.Distance(transformJugador.position, transformNPC.position);

        if (distancia <= distanciaInteraccion && !dialogoActivo)
        {
            ActivarModoDialogo();
        }
        else if (distancia > distanciaInteraccion && dialogoActivo)
        {
            DesactivarModoDialogo();
        }

        if (dialogoActivo && EventSystem.current.currentSelectedGameObject == null && botonOpcionCentro != null)
        {
            EventSystem.current.SetSelectedGameObject(botonOpcionCentro);
        }
    }

    public void ActivarModoDialogo()
    {
        dialogoActivo = true;
        if (panelDialogosUI != null) panelDialogosUI.SetActive(true);

        // Congelamos al jugador inmediatamente
        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = false;

        EventSystem.current.SetSelectedGameObject(null);
        if (botonOpcionCentro != null)
        {
            EventSystem.current.SetSelectedGameObject(botonOpcionCentro);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scriptCamaraJugador != null) scriptCamaraJugador.enabled = false;
    }

    public void DesactivarModoDialogo()
    {
        dialogoActivo = false;
        if (panelDialogosUI != null) panelDialogosUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (scriptCamaraJugador != null) scriptCamaraJugador.enabled = true;

        // Esperamos 0.1 segundos (100 milisegundos) antes de devolverle el movimiento, para asegurarnos de que Unity ya olvidó que presionamos la X.
        Invoke("RestaurarMovimiento", 0.1f);
    }

    // Esta funcion es llamada automáticamente por el Invoke de arribaxd
    private void RestaurarMovimiento()
    {
        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = true;
    }

    public void ElegirOpcionVentanal()
    {
        RegistrarAccionEnTabla("Asistencia a Familiar (Esconderse en la cochera)", -500);
        MoverNPC(puntoVentanal);
    }

    public void ElegirOpcionColumnaSegura()
    {
        RegistrarAccionEnTabla("Asistencia a Familiar (Llegar al punto de reunión)", 1000);
        MoverNPC(puntoColumnaSegura);
    }

    public void ElegirOpcionColumnaMala()
    {
        RegistrarAccionEnTabla("Asistencia a Familiar (Decirle que se fuera por la cochera)", -100);
        MoverNPC(puntoColumnaMala);
    }

    private void RegistrarAccionEnTabla(string nombreAccion, int puntos)
    {
        if (inventarioJugador != null)
        {
            inventarioJugador.puntuacionTotal += puntos;
            inventarioJugador.desglosePuntuacion.Add(nombreAccion + " .................... " + puntos + " pts");
        }
    }

    private void MoverNPC(Transform destino)
    {
        yaRespondio = true;
        DesactivarModoDialogo();

        if (agenteNPC != null) agenteNPC.SetDestination(destino.position);
    }
}