using UnityEngine;

// Define la identidad, valor educativo y ajustes de agarre específicos para cada objeto interactuable.
public class ValorObjeto : MonoBehaviour
{
    [Header("Atributos del Objeto")]
    public string nombreMostrado = "Objeto Desconocido";
    public int puntosQueOtorga = 10;

    // Este campo de texto expandido en el Inspector permite redactar el mensaje de conciencia que la IA mostrará al jugador.
    [TextArea(2, 5)] 
    public string datoCuriosoIA = "Dato interesante sobre este objeto.";

    [Header("Ajustes de Empuñadura")]
    // Permiten corregir manualmente el desfase de posición y rotación para que el objeto encaje perfectamente en la mano del jugador.
    public Vector3 posicionEnMano;
    public Vector3 rotacionEnMano;
}