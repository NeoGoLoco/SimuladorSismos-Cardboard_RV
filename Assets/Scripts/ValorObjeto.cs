using UnityEngine;
//El Valor del objeto está oculto y desde el Inspector se puede agregar algún valor o descripción... Desde ahíxd
//Y gracias a las funciones en el SistemaAgarre, se te acumulan los puntos
public class ValorObjeto : MonoBehaviour
{
    [Header("Datos para el Inventario")]
    public string nombreMostrado = "Objeto Desconocido";
    public int puntosQueOtorga = 10;

    [TextArea(2, 5)] // Hace que el cuadro de texto en Unity sea más grande
    public string datoCuriosoIA = "Dato interesante sobre este objeto.";

    // Hace que se pueda modificar su posicionamiento
    [Header("Ajustes en la Mano")]
    public Vector3 posicionEnMano;
    public Vector3 rotacionEnMano;
}