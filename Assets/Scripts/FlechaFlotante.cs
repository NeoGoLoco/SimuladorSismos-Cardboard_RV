using UnityEngine;

// Crea un efecto visual dinámico de rotación y flotación para indicadores o ítems de interés.
public class FlechaControlTotal : MonoBehaviour
{
    [Header("Ajustes de Rotación")]
    public float velocidadRotacion = 100f;
    
    // Define el eje sobre el que girará el objeto (ej. Y para un giro tipo moneda)
    public Vector3 ejeRotacion = new Vector3(1, 0, 0);

    [Header("Ajustes de Flote")]
    public Vector3 direccionFlote = new Vector3(0, 1, 0);
    public float amplitudFlote = 0.2f;
    public float velocidadFlote = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        // Guardamos el punto de origen para que el flote siempre sea relativo a la posición inicial
        posicionInicial = transform.position;
    }

    void Update()
    {
        // ROTACIÓN
        // Usamos Space.Self para que el objeto gire sobre su propio eje, sin importar cómo esté orientado
        transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime, Space.Self);

        // Utilizamos una función Seno para generar un vaivén suave y repetitivo.
        // Multiplicamos la dirección normalizada por el resultado del seno para definir el desplazamiento.
        float movimientoSeno = Mathf.Sin(Time.time * velocidadFlote) * amplitudFlote;
        Vector3 desfase = direccionFlote.normalized * movimientoSeno;

        // Actualizamos la posición sumando el desfase calculado al punto de origen
        transform.position = posicionInicial + desfase;
    }
}