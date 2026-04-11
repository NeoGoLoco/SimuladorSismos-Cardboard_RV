using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Efectos de Sonido")]
    public AudioSource reproductorSonidos;
    public AudioClip sonidoClic;
    public AudioClip sonidoHover;

    void Start() { }
    void Update() { }

    public void ReproducirSonidoHover()
    {
        if (reproductorSonidos != null && sonidoHover != null)
        {
            reproductorSonidos.PlayOneShot(sonidoHover);
        }
    }

    public void ReproducirSonidoBoton()
    {
        if (reproductorSonidos != null && sonidoClic != null)
        {
            reproductorSonidos.PlayOneShot(sonidoClic);
        }
    }

    public void EmpezarJuego()
    {
        StartCoroutine(CargarJuegoConRetraso());
    }

    IEnumerator CargarJuegoConRetraso()
    {
        ReproducirSonidoBoton();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void CerrarJuego()
    {
        StartCoroutine(CerrarJuegoConRetraso());
    }

    IEnumerator CerrarJuegoConRetraso()
    {
        ReproducirSonidoBoton();
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
        Debug.Log("Salir");
    }
}