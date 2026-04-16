using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

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
    public Color colorResaltadoNormal = new Color(1f, 1f, 0f, 2f); // Amarillo
    [ColorUsage(true, true)]
    public Color colorResaltadoMochila = new Color(0f, 1f, 1f, 2f); // Cyan

    [Header("Conciencia")]
    public int puntuacionTotal = 0;
    public List<string> datosCuriososRecopilados = new List<string>();
    public Text textoDatoCuriosoInstantaneo;
    public List<string> desglosePuntuacion = new List<string>();

    [Header("Audio")]
    public AudioSource reproductorAudioAgarre;
    public AudioClip sonidoGuardarObjeto;

    // Estado del objeto
    private GameObject objetoSostenido;
    private Rigidbody rbSostenido;
    private GameObject objetoMiradoActual;
    private bool mostrandoMensajeTemporal = false;

    // Variables de memoria 
    private Vector3 posicionOriginal;
    private Quaternion rotacionOriginal;
    private Vector3 escalaOriginal;
    private bool esObjetoOculto = false;

    private const string EMISSION_KEYWORD = "_EMISSION";
    private const string EMISSION_COLOR_NAME = "_EmissionColor";

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (textoMensaje != null) textoMensaje.text = "";
        if (textoDatoCuriosoInstantaneo != null) textoDatoCuriosoInstantaneo.text = "";
        if (textoInteraccion != null) textoInteraccion.text = "";
    }

    void Update()
    {
        GestionarIndicadorVisual();

        bool intentoAgarre = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) intentoAgarre = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) intentoAgarre = true; // 

        if (intentoAgarre)
        {
            if (objetoSostenido == null) IntentarAgarrar();
            else SoltarObjeto();
        }

        if (objetoSostenido != null)
        {
            if (textoInteraccion != null) textoInteraccion.text = "";

            if (textoMensaje != null && !mostrandoMensajeTemporal)
            {
                string nombre = objetoSostenido.name;
                ValorObjeto val = objetoSostenido.GetComponent<ValorObjeto>();
                if (val != null) nombre = val.nombreMostrado;

                textoMensaje.text = "Guardar " + nombre + "? (Triangulo)\n" +
                                    "Presione [Cuadrado] para soltar";
            }

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
            if (textoMensaje != null && !mostrandoMensajeTemporal && textoMensaje.text != "")
            {
                textoMensaje.text = "";
            }
        }
    }

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

        inventarioOculto.Add(nombreObjeto);
        desglosePuntuacion.Add(nombreObjeto + " .................... " + puntosEsteObjeto + " pts");

       
        if (sonidoGuardarObjeto != null && camaraJugador != null)
        {
            AudioSource.PlayClipAtPoint(sonidoGuardarObjeto, camaraJugador.transform.position, 1f);
        }

        // Ahora sí, destruimos el objeto con total seguridad
        Destroy(objetoSostenido);
        objetoSostenido = null;
        rbSostenido = null;

        if (textoMensaje != null)
        {
            mostrandoMensajeTemporal = true;
            textoMensaje.text = "¡" + nombreObjeto + " Guardado!";
            Invoke("LimpiarMensajeUI", 5f);
        }

        if (textoDatoCuriosoInstantaneo != null && datoIA != "")
        {
            textoDatoCuriosoInstantaneo.text = "CONCIENCIA:\n" + datoIA;
            CancelInvoke("LimpiarDatoCuriosoUI");
            Invoke("LimpiarDatoCuriosoUI", 20f);
        }
    }

    void LimpiarMensajeUI()
    {
        mostrandoMensajeTemporal = false;
        if (textoMensaje != null) textoMensaje.text = "";
    }

    void LimpiarDatoCuriosoUI()
    {
        if (textoDatoCuriosoInstantaneo != null) textoDatoCuriosoInstantaneo.text = "";
    }

    void GestionarIndicadorVisual()
    {
        if (objetoSostenido != null)
        {
            ApagarResaltadoObjetoMirado();
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(camaraJugador.transform.position, camaraJugador.transform.forward, out hit, distanciaAgarre))
        {
            if (hit.collider.CompareTag("Agarrable") || hit.collider.CompareTag("Mochila") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactuable"))
            {
                GameObject objetoDetectado = hit.collider.gameObject;

                if (objetoDetectado != objetoMiradoActual)
                {
                    ApagarResaltadoObjetoMirado();
                    objetoMiradoActual = objetoDetectado;
                    EncenderResaltadoObjeto(objetoMiradoActual);
                }

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

    void EncenderResaltadoObjeto(GameObject obj)
    {
        Color colorAUsar = colorResaltadoNormal;
        if (obj.CompareTag("Mochila") || obj.name.ToLower().Contains("mochila")) colorAUsar = colorResaltadoMochila;

        Renderer[] todosLosRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in todosLosRenderers)
        {
            if (rend != null)
            {
                rend.material.EnableKeyword(EMISSION_KEYWORD);
                rend.material.SetColor(EMISSION_COLOR_NAME, colorAUsar);
            }
        }
    }

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

    void EquiparObjeto(GameObject objeto)
    {
        ApagarResaltadoObjetoMirado();

        objetoSostenido = objeto;
        rbSostenido = objeto.GetComponent<Rigidbody>();

        if (rbSostenido != null) rbSostenido.isKinematic = true;

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