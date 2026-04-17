using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

// Este script actúa como el puente entre el jugador y el NPC.
// Controla la proximidad para activar diálogos, congela el mundo para la toma
// de decisiones y gestiona el sistema de navegación hacia zonas seguras.
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
    // Diferentes zonas de la casa con distinto valor ético/seguridad
    public Transform puntoVentanal;
    public Transform puntoColumnaSegura;
    public Transform puntoColumnaMala;

    [Header("Interfaz y Cámara")]
    public GameObject panelDialogosUI;
    public GameObject botonOpcionCentro; // Botón para forzar el foco del Gamepad
    public MonoBehaviour scriptCamaraJugador;

    [Header("Conexión con Puntuación Final")]
    public SistemaAgarre inventarioJugador;

    private ControladorTerremoto scriptMovimiento;

    void Start()
    {
        // Ocultamos la interfaz de diálogo al inicio
        if (panelDialogosUI != null) panelDialogosUI.SetActive(false);

        // Obtenemos el script de movimiento del jugador para poder congelarlo en diálogos
        if (transformJugador != null)
        {
            scriptMovimiento = transformJugador.GetComponent<ControladorTerremoto>();
        }
    }

    void Update()
    {
        // Sincronización constante: enviamos la velocidad física del NavMesh al Animator.
        // Esto permite que el NPC pase de 'Idle' a 'Walking' de forma automática.
        if (agenteNPC != null && animadorNPC != null)
        {
            animadorNPC.SetFloat("Speed", agenteNPC.velocity.magnitude);
        }

        // Si el NPC ya tiene instrucciones o faltan referencias, no evaluamos proximidad
        if (yaRespondio || transformJugador == null || transformNPC == null) return;

        float distancia = Vector3.Distance(transformJugador.position, transformNPC.position);

        // Control de activación/desactivación automática por distancia
        if (distancia <= distanciaInteraccion && !dialogoActivo)
        {
            ActivarModoDialogo();
        }
        else if (distancia > distanciaInteraccion && dialogoActivo)
        {
            DesactivarModoDialogo();
        }

        // Mantenemos el foco en los botones si el jugador usa Gamepad
        if (dialogoActivo && EventSystem.current.currentSelectedGameObject == null && botonOpcionCentro != null)
        {
            EventSystem.current.SetSelectedGameObject(botonOpcionCentro);
        }
    }

    // Preparamos el entorno para la interacción: liberamos cursor y bloqueamos movimiento
    public void ActivarModoDialogo()
    {
        dialogoActivo = true;
        if (panelDialogosUI != null) panelDialogosUI.SetActive(true);

        // Evitamos que el jugador se mueva mientras decide la suerte del NPC
        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = false;

        // Gestión de foco de UI para navegación con mando
        EventSystem.current.SetSelectedGameObject(null);
        if (botonOpcionCentro != null)
        {
            EventSystem.current.SetSelectedGameObject(botonOpcionCentro);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Desactivamos rotación de cámara para no perder el panel de vista
        if (scriptCamaraJugador != null) scriptCamaraJugador.enabled = false;
    }

    // Restauramos el estado de juego normal
    public void DesactivarModoDialogo()
    {
        dialogoActivo = false;
        if (panelDialogosUI != null) panelDialogosUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (scriptCamaraJugador != null) scriptCamaraJugador.enabled = true;

        // Pequeño margen de tiempo antes de restaurar movimiento para evitar 
        // que una pulsación de botón se detecte en ambos sistemas (UI y Juego)
        Invoke("RestaurarMovimiento", 0.1f);
    }

    private void RestaurarMovimiento()
    {
        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = true;
    }

    // --- MÉTODOS DE RESPUESTA ---
    // Cada opción afecta la puntuación final y define el destino del NPC

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

    // Registramos la decisión en el sistema de inventario para el reporte final
    private void RegistrarAccionEnTabla(string nombreAccion, int puntos)
    {
        if (inventarioJugador != null)
        {
            inventarioJugador.puntuacionTotal += puntos;
            inventarioJugador.desglosePuntuacion.Add(nombreAccion + " .................... " + puntos + " pts");
        }
    }

    // Ejecuta la orden de navegación física hacia el waypoint seleccionado
    private void MoverNPC(Transform destino)
    {
        yaRespondio = true;
        DesactivarModoDialogo();

        if (agenteNPC != null) agenteNPC.SetDestination(destino.position);
    }
}