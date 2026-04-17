using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

// Este sistema gestiona la interacción física del jugador con los objetos del entorno.
// Se encarga de detectar qué estamos mirando, resaltarlo visualmente, permitir
// el agarre y procesar el guardado de suministros para la puntuación final.
public class SistemaAgarre : MonoBehaviour
{
    [Header("Interfaz")]
    public Text textoInteraccion;

    [Header("Configuración de Agarre")]
    public Transform puntoMano;
    public Camera camaraJugador;
    public float distanciaAgarre = 3.5f;

    [Header("Inventario y UI")]
    public Text textoMensaje;
    public List<string> inventarioOculto = new List<string>();

    [Header("Configuración de Resaltado")]
    [ColorUsage(true, true)]
    public Color colorResaltadoNormal = new Color(1f, 1f, 0f, 2f); // Amarillo para suministros
    [ColorUsage(true, true)]
    public Color colorResaltadoMochila = new Color(0f, 1f, 1f, 2f); // Cyan para mochilas/especiales

    [Header("Conciencia")]
    public int puntuacionTotal = 0;
    public List<string> datosCuriososRecopilados = new List<string>();
    public Text textoDatoCuriosoInstantaneo;
    public List<string> desglosePuntuacion = new List<string>();

    [Header("Audio")]
    public AudioSource reproductorAudioAgarre;
    public AudioClip sonidoGuardarObjeto;

    // Control de estado del objeto actual
    private GameObject objetoSostenido;
    private Rigidbody rbSostenido;
    private GameObject objetoMiradoActual;
    private bool mostrandoMensajeTemporal = false;

    // Memoria del objeto para cuando necesitemos soltarlo o restaurar su estado
    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Vector3 escalaOriginal;
    private bool esObjetoOculto = false;

    // Constantes para manipular el material (Emission) sin errores de escritura
    private const string EMISSION_KEYWORD = "_EMISSION";
    private const string EMISSION_COLOR_NAME = "_EmissionColor";

    void Start()
    {
        // Configuramos el cursor para que no estorbe en la experiencia VR/Primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Limpiamos los textos de la interfaz al iniciar
        if (textoMensaje != null) textoMensaje.text = "";
        if (textoDatoCuriosoInstantaneo != null) textoDatoCuriosoInstantaneo.text = "";
        if (textoInteraccion != null) textoInteraccion.text = "";
    }

    void Update()
    {
        // Primero detectamos qué tiene el jugador frente a sus ojos
        GestionarIndicadorVisual();

        // Escuchamos el intento de agarrar o soltar (E en teclado o Cuadrado/West en mando)
        bool intentoAgarre = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) intentoAgarre = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) intentoAgarre = true; 

        if (intentoAgarre)
        {
            if (objetoSostenido == null) IntentarAgarrar();
            else SoltarObjeto();
        }

        // Si ya tenemos algo en la mano, gestionamos la UI de guardado y el input de inventario
        if (objetoSostenido != null)
        {
            if (textoInteraccion != null) textoInteraccion.text = "";

            if (textoMensaje != null && !mostrandoMensajeTemporal)
            {
                // Buscamos el nombre amigable del objeto si tiene el script ValorObjeto
                string nombre = objetoSostenido.name;
                ValorObjeto val = objetoSostenido.GetComponent<ValorObjeto>();
                if (val != null) nombre = val.nombreMostrado;

                textoMensaje.text = "Guardar " + nombre + "? (Triangulo)\n" +
                                    "Presione [Cuadrado] para soltar";
            }

            // Input para guardar en la mochila (G o Triángulo/North)
            bool intentoGuardar = false;
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame) intentoGuardar = true;
            if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame) intentoGuardar = true; 

            if (intentoGuardar)
            {
                GuardarEnInventario();
            }
        }
        else
        {
            // Limpiamos el mensaje de UI si no sostenemos nada
            if (textoMensaje != null && !mostrandoMensajeTemporal && textoMensaje.text != "")
            {
                textoMensaje.text = "";
            }
        }
    }

    // Procesa la lógica de puntos, sonidos y datos curiosos al guardar un suministro
    void GuardarEnInventario()
    {
        string nombreObjeto = objetoSostenido.name;
        string datoIA = "";
        ValorObjeto valores = objetoSostenido.GetComponent<ValorObjeto>();
        int puntosEsteObjeto = 0;

        if (valores != null)
        {
            nombreObjeto = valores.nombreMostrado;
            datoIA = valores.datoCuriosoIA;
            puntosEsteObjeto = valores.puntosQueOtorga;
            puntuacionTotal += puntosEsteObjeto;
        }

        // Añadimos al registro para el reporte final de la Meta
        inventarioOculto.Add(nombreObjeto);
        desglosePuntuacion.Add(nombreObjeto + " .................... " + puntosEsteObjeto + " pts");

        // Feedback auditivo espacial
        if (sonidoGuardarObjeto != null && camaraJugador != null)
        {
            AudioSource.PlayClipAtPoint(sonidoGuardarObjeto, camaraJugador.transform.position, 1f);
        }

        // Eliminamos el objeto físico del mundo una vez procesado
        Destroy(objetoSostenido);
        objetoSostenido = null;
        rbSostenido = null;

        // Feedback visual en la UI de que el objeto se guardó con éxito
        if (textoMensaje != null)
        {
            mostrandoMensajeTemporal = true;
            textoMensaje.text = "¡" + nombreObjeto + " Guardado!";
            Invoke("LimpiarMensajeUI", 5f);
        }

        // Mostramos el "Dato de Conciencia" de la IA sobre la importancia de ese objeto
        if (textoDatoCuriosoInstantaneo != null && datoIA != "")
        {
            textoDatoCuriosoInstantaneo.text = "CONCIENCIA:\n" + datoIA;
            CancelInvoke("LimpiarDatoCuriosoUI");
            Invoke("LimpiarDatoCuriosoUI", 20f);
        }
    }

    // Funciones simples para limpiar la interfaz después de un tiempo
    void LimpiarMensajeUI()
    {
        mostrandoMensajeTemporal = false;
        if (textoMensaje != null) textoMensaje.text = "";
    }

    void LimpiarDatoCuriosoUI()
    {
        if (textoDatoCuriosoInstantaneo != null) textoDatoCuriosoInstantaneo.text = "";
    }

    // Lanza un Raycast para detectar objetos interactuables frente al jugador
    void GestionarIndicadorVisual()
    {
        if (objetoSostenido != null)
        {
            ApagarResaltadoObjetoMirado();
            return;
        }

        RaycastHit hit;
        // Buscamos colisiones en el rango de interacción
        if (Physics.Raycast(camaraJugador.transform.position, camaraJugador.transform.forward, out hit, distanciaAgarre))
        {
            // Verificamos si es algo que nos interesa (Tags o Layer específica)
            if (hit.collider.CompareTag("Agarrable") || hit.collider.CompareTag("Mochila") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactuable"))
            {
                GameObject objetoDetectado = hit.collider.gameObject;

                // Solo encendemos el resaltado si cambiamos de objeto mirado
                if (objetoDetectado != objetoMiradoActual)
                {
                    ApagarResaltadoObjetoMirado();
                    objetoMiradoActual = objetoDetectado;
                    EncenderResaltadoObjeto(objetoMiradoActual);
                }

                // Actualizamos el texto de instrucción en pantalla
                if (textoInteraccion != null)
                {
                    string nombreMostrar = objetoDetectado.name;
                    ValorObjeto val = objetoDetectado.GetComponent<ValorObjeto>();
                    if (val != null) nombreMostrar = val.nombreMostrado;

                    textoInteraccion.text = "Presiona [Cuadrado] para sostener " + nombreMostrar;
                }
            }
            else
            {
                ApagarResaltadoObjetoMirado();
                if (textoInteraccion != null && textoInteraccion.text.Contains("sostener")) textoInteraccion.text = "";
            }
        }
        else
        {
            ApagarResaltadoObjetoMirado();
            if (textoInteraccion != null && textoInteraccion.text.Contains("sostener")) textoInteraccion.text = "";
        }
    }

    // Activa la emisión del material para dar feedback visual de interactividad
    void EncenderResaltadoObjeto(GameObject obj)
    {
        Color colorAUsar = colorResaltadoNormal;
        // Las mochilas tienen un color de resaltado diferente (Cyan)
        if (obj.CompareTag("Mochila") || obj.name.ToLower().Contains("mochila")) colorAUsar = colorResaltadoMochila;

        Renderer[] todosLosRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in todosLosRenderers)
        {
            if (rend != null)
            {
                // Activamos la keyword de emisión en el shader
                rend.material.EnableKeyword(EMISSION_KEYWORD);
                rend.material.SetColor(EMISSION_COLOR_NAME, colorAUsar);
            }
        }
    }

    // Apaga la emisión cuando dejamos de mirar el objeto
    void ApagarResaltadoObjetoMirado()
    {
        if (objetoMiradoActual != null)
        {
            Renderer[] todosLosRenderers = objetoMiradoActual.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in todosLosRenderers)
            {
                if (rend != null)
                {
                    rend.material.DisableKeyword(EMISSION_KEYWORD);
                    rend.material.SetColor(EMISSION_COLOR_NAME, Color.black);
                }
            }
            objetoMiradoActual = null;
        }
    }

    void IntentarAgarrar()
    {
        if (objetoMiradoActual != null) EquiparObjeto(objetoMiradoActual);
    }

    // Vincula físicamente el objeto a la mano del jugador
    void EquiparObjeto(GameObject objeto)
    {
        ApagarResaltadoObjetoMirado();

        objetoSostenido = objeto;
        rbSostenido = objeto.GetComponent<Rigidbody>();

        // Desactivamos la física para que no colisione con el jugador mientras lo carga
        if (rbSostenido != null) rbSostenido.isKinematic = true;

        // Lógica especial para la mochila (se oculta en lugar de llevarse en la mano)
        if (objeto.name.ToLower().Contains("mochila") || objeto.CompareTag("Mochila"))
        {
            esObjetoOculto = true;
            posicionOriginal = objeto.transform.position;
            rotacionOriginal = objeto.transform.rotation;
            objeto.SetActive(false);
        }
        else
        {
            esObjetoOculto = false;
            escalaOriginal = objeto.transform.localScale;
            objeto.transform.SetParent(puntoMano);

            // Ajustamos posición y rotación según lo configurado en ValorObjeto
            ValorObjeto val = objeto.GetComponent<ValorObjeto>();
            if (val != null)
            {
                objeto.transform.localPosition = val.posicionEnMano;
                objeto.transform.localEulerAngles = val.rotacionEnMano;
            }
            else
            {
                objeto.transform.localPosition = Vector3.zero;
                objeto.transform.localRotation = Quaternion.identity;
            }

            objeto.transform.localScale = escalaOriginal;
        }
    }

    // Devuelve el objeto al mundo físico
    void SoltarObjeto()
    {
        if (esObjetoOculto)
        {
            objetoSostenido.transform.position = posicionOriginal;
            objetoSostenido.transform.rotation = rotacionOriginal;
            objetoSostenido.SetActive(true);
        }
        else
        {
            objetoSostenido.transform.SetParent(null);
        }

        // Reactivamos la física y limpiamos cualquier velocidad residual
        if (rbSostenido != null)
        {
            rbSostenido.isKinematic = false;
            rbSostenido.linearVelocity = Vector3.zero;
            rbSostenido.angularVelocity = Vector3.zero;
        }

        objetoSostenido = null;
        rbSostenido = null;
    }
}