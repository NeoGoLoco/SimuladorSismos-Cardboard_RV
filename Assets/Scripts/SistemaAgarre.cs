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
    public Color colorResaltado = new Color(1f, 1f, 0f, 2f);

    [Header("Conciencia")]
    public int puntuacionTotal = 0;
    public List<string> datosCuriososRecopilados = new List<string>();
    public Text textoDatoCuriosoInstantaneo;
    public List<string> desglosePuntuacion = new List<string>();

    private GameObject objetoSostenido;
    private Rigidbody rbSostenido;
    private Vector3 escalaOriginal;
    private GameObject objetoMiradoActual;
    private bool mostrandoMensajeTemporal = false;

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

        // TECLA AGARRAR (E en PC, Cuadrado en Dualshock o Dualsense, da igual)
        bool intentoAgarre = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) intentoAgarre = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) intentoAgarre = true;

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

                textoMensaje.text = "¿Guardar " + nombre + "? (Triángulo/G)\n" +
                                    "Presione [Cuadrado/E] para soltar";
            }

            // TECLA GUARDAR (G en PC, Triángulo en control)
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

        //Esto en específico es para la tabla de la Meta final... Para que se visualice el valor de los objetos al final
        inventarioOculto.Add(nombreObjeto);
        //Diseño bonito cuando se despliega
        desglosePuntuacion.Add(nombreObjeto + " .................... " + puntosEsteObjeto + " pts");

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
            if (hit.collider.CompareTag("Agarrable") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactuable"))
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

                    textoInteraccion.text = "Presiona Cuadrado/E para sostener " + nombreMostrar;
                }
            }
            else
            {
                ApagarResaltadoObjetoMirado();
                if (textoInteraccion != null && textoInteraccion.text.Contains("sostener"))
                {
                    textoInteraccion.text = "";
                }
            }
        }
        else
        {
            ApagarResaltadoObjetoMirado();
            if (textoInteraccion != null && textoInteraccion.text.Contains("sostener"))
            {
                textoInteraccion.text = "";
            }
        }
    }

    void EncenderResaltadoObjeto(GameObject obj)
    {
        Renderer[] todosLosRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in todosLosRenderers)
        {
            if (rend != null)
            {
                rend.material.EnableKeyword(EMISSION_KEYWORD);
                rend.material.SetColor(EMISSION_COLOR_NAME, colorResaltado);
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

    void SoltarObjeto()
    {
        objetoSostenido.transform.SetParent(null);
        if (rbSostenido != null) rbSostenido.isKinematic = false;

        objetoSostenido = null;
        rbSostenido = null;
    }
}