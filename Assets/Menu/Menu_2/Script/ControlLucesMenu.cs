using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class ControlLucesMenu : MonoBehaviour
{
    [Header("Botones del Menú")]
    public GameObject botonJugar;
    public GameObject botonSalir;

    [Header("Colores del DualSense")]
    public Color colorJugar = Color.cyan;
    public Color colorSalir = Color.red;
    public Color colorPorDefecto = Color.white;

    void Update()
    {
        // --- EL SEGURO o SSISTEMA ANTI-MOUSE
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(botonJugar);
        }

        // CÓDIGO EXCLUSIVO PARA PC (Se ignora en Android)
#if UNITY_EDITOR || UNITY_STANDALONE
        var dualSense = DualSenseGamepadHID.current;
        var dualShock = DualShockGamepad.current;

        if (dualSense != null)
        {
            ActualizarColorLED(dualSense);
        }
        else if (dualShock != null)
        {
            ActualizarColorLED(dualShock);
        }
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void ActualizarColorLED(DualShockGamepad controlActivo)
    {
        GameObject botonActual = EventSystem.current.currentSelectedGameObject;

        if (botonActual == botonJugar)
        {
            controlActivo.SetLightBarColor(colorJugar);
        }
        else if (botonActual == botonSalir)
        {
            controlActivo.SetLightBarColor(colorSalir);
        }
        else
        {
            controlActivo.SetLightBarColor(colorPorDefecto);
        }
    }
#endif
}