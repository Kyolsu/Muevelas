using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Súper importante para poder cambiar de escenas

public class PantallaInicio : MonoBehaviour
{
    // Este método lo conectaremos al botón "Iniciar"
    public void IniciarJuego()
    {
        // Asegúrate de que el nombre coincida exactamente con cómo guardaste tu escena
        SceneManager.LoadScene("SeleccionDePersonaje");
    }

    // Este método lo conectaremos al botón "Salir"
    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego..."); // Mensaje en consola para confirmar

        // Esto cierra el juego cuando ya está compilado (.exe, .apk, etc)
        Application.Quit();

        // Esto es un truquito extra para que el botón de salir funcione también dentro del Editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
