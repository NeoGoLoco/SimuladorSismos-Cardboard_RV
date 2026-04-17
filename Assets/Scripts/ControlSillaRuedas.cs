using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Sincroniza el movimiento de la silla con el jugador, transformando al usuario en el motor de empuje.
public class ControlSillaRuedas : MonoBehaviour
{
    [Header("Interfaz")]
    public Text textoInteraccion;

    [Header("Visuales")]
    // Referencias para intercambiar los brazos del jugador por unos que sujeten la silla
    public GameObject brazosEmpujando;
    public GameObject brazoNormal;

    [Header("Configuración")]
    public Transform puntoDeAgarre; // Posición exacta donde debe quedar el jugador al empujar
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
        // Detectamos el intento de interacción (E o Cuadrado)
        bool intentoEmpujar = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) intentoEmpujar = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) intentoEmpujar = true;

        if (jugadorEnZona && intentoEmpujar)
        {
            if (!empujando) EmpezarAEmpujar();
            else SoltarSilla();
        }

        // Si el estado es activo, la silla toma el control del desplazamiento
        if (empujando)
        {
            MoverSilla();
        }
    }

    // --- DETECCIÓN DE PROXIMIDAD ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnZona = true;
            jugador = other.gameObject;
            
            // Cachamos los componentes del jugador para optimizar el acceso después
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
            if (textoInteraccion != null) textoInteraccion.text = "";

            // Si salimos de la zona sin estar empujando, limpiamos las referencias
            if (!empujando)
            {
                jugador = null;
                scriptMovimiento = null;
                characterController = null;
            }
        }
    }

    // --- LÓGICA DE VINCULACIÓN ---

    void EmpezarAEmpujar()
    {
        empujando = true;
        if (textoInteraccion != null) textoInteraccion.text = "";

        // Desactivamos el control autónomo del jugador
        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = false;
        if (characterController != null) characterController.enabled = false;

        // El truco técnico: "emparentamos" al jugador a la silla para que se muevan como un solo objeto
        jugador.transform.position = puntoDeAgarre.position;
        jugador.transform.rotation = puntoDeAgarre.rotation;
        jugador.transform.SetParent(this.transform);

        // Feedback visual: cambiamos el modelo de los brazos
        if (brazosEmpujando != null) brazosEmpujando.SetActive(true);
        if (brazoNormal != null) brazoNormal.SetActive(false);
    }

    void SoltarSilla()
    {
        empujando = false;

        // Devolvemos al jugador su independencia física
        jugador.transform.SetParent(null);

        if (scriptMovimiento != null) scriptMovimiento.puedeCaminar = true;
        if (characterController != null) characterController.enabled = true;

        if (brazosEmpujando != null) brazosEmpujando.SetActive(false);
        if (brazoNormal != null) brazoNormal.SetActive(true);
    }

    // --- FÍSICA DE MOVIMIENTO ---

    void MoverSilla()
    {
        float avance = 0f;
        float giro = 0f;

        // Leemos inputs de teclado y mando para definir la dirección
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) avance += 1f;
            if (Keyboard.current.sKey.isPressed) avance -= 1f;
            if (Keyboard.current.dKey.isPressed) giro += 1f;
            if (Keyboard.current.aKey.isPressed) giro -= 1f;
        }

        if (Gamepad.current != null)
        {
            avance += Gamepad.current.leftStick.y.ReadValue();
            giro += Gamepad.current.leftStick.x.ReadValue();
        }

        // Aplicamos la rotación primero para orientar el empuje
        transform.Rotate(0, giro * velocidadGiro * Time.deltaTime, 0);

        // Calculamos el vector de movimiento y aplicamos una gravedad constante
        Vector3 movimiento = transform.forward * (avance * velocidadEmpuje);
        movimiento.y = -9.81f; 

        // Movemos el CharacterController de la silla (que ahora arrastra al jugador)
        ccSilla.Move(movimiento * Time.deltaTime);
    }
}