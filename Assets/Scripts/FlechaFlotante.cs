using UnityEngine;

public class FlechaControlTotal : MonoBehaviour
{
    [Header("Ajustes de Rotación")]
    public float velocidadRotacion = 100f;
    
    public Vector3 ejeRotacion = new Vector3(1, 0, 0);

    [Header("Ajustes de Flote")]
    public Vector3 direccionFlote = new Vector3(0, 1, 0);
    public float amplitudFlote = 0.2f;
    public float velocidadFlote = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // ROTACIÓN PERSONALIZADA
        // Usamos ejeRotacion para que tú decidas si gira en X, Y, Z o combinado
        transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime, Space.Self);

        // FLOTE EN DIRECCIÓN PERSONALIZADA
        // Multiplicamos el vector de dirección por el resultado del Seno y la amplitud
        float movimientoSeno = Mathf.Sin(Time.time * velocidadFlote) * amplitudFlote;
        Vector3 desfase = direccionFlote.normalized * movimientoSeno;

        transform.position = posicionInicial + desfase;
    }
}