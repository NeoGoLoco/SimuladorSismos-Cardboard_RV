using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlSillaRuedas : MonoBehaviour
{
    [Header("Interfaz")]
    public Text textoInteraccion;

    [Header("Visuales")]
    public GameObject brazosEmpujando;
    public GameObject brazoNormal;

    [Header("Configuración")]
    public Transform puntoDeAgarre;
    public float velocidadEmpuje = 3.0f;
    public float velocidadGiro = 60.0f;

    private bool jugadorEnZona = false;
    private bool empujando = false;
    private GameObject jugador;

    private ControladorTerremoto scriptMovimiento;
    private CharacterController characterController;
    private CharacterController ccSilla;

    void Start()
    {
        ccSilla = GetComponent<CharacterController>();
    }

    void Update()
    {
        
        bool intentoEmpujar = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) intentoEmpujar = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) intentoEmpujar = true;

        if (jugadorEnZona && intentoEmpujar)
        {
            if (!empujando) EmpezarAEmpujar();
            else SoltarSilla();
        }

        if (empujando)
        {
            MoverSilla();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = true;
            jugador = other.gameObject;
            scriptMovimiento = jugador.GetComponent<ControladorTerremoto>();
            characterController = jugador.GetComponent<CharacterController>();

            if (!empujando && textoInteraccion != null)
            {
                textoInteraccion.text = "Presiona [Cuadrado] para empujar la silla";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = false;

            if (textoInteraccion != null)
            {
                textoInteraccion.text = "";
            }

            if (!empujando)
            {
                jugador = null;
                scriptMovimiento = null;
                characterController = null;
            }
        }
    }

    void EmpezarAEmpujar()
    {
        empujando = true;

        if (textoInteraccion != null) textoInteraccion.text = "";

        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = false;
        if (characterController != null) characterController.enabled = false;

        jugador.transform.position = puntoDeAgarre.position;
        jugador.transform.rotation = puntoDeAgarre.rotation;
        jugador.transform.SetParent(this.transform);

        if (brazosEmpujando != null) brazosEmpujando.SetActive(true);
        if (brazoNormal != null) brazoNormal.SetActive(false);
    }

    void SoltarSilla()
    {
        empujando = false;

        jugador.transform.SetParent(null);

        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = true;
        if (characterController != null) characterController.enabled = true;

        if (brazosEmpujando != null) brazosEmpujando.SetActive(false);
        if (brazoNormal != null) brazoNormal.SetActive(true);
    }

    void MoverSilla()
    {
        float avance = 0f;
        float giro = 0f;

        // LECTURA DE TECLADO en la PC... O incluso en celularxd
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) avance += 1f;
            if (Keyboard.current.sKey.isPressed) avance -= 1f;
            if (Keyboard.current.dKey.isPressed) giro += 1f;
            if (Keyboard.current.aKey.isPressed) giro -= 1f;
        }

        // LECTURA DE CONTROL 
        if (Gamepad.current != null)
        {
            avance += Gamepad.current.leftStick.y.ReadValue();
            giro += Gamepad.current.leftStick.x.ReadValue();
        }

        transform.Rotate(0, giro * velocidadGiro * Time.deltaTime, 0);

        Vector3 movimiento = transform.forward * (avance * velocidadEmpuje);
        movimiento.y = -9.81f;

        ccSilla.Move(movimiento * Time.deltaTime);
    }
}