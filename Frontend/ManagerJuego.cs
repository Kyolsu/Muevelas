using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escenas

public class ManagerJuego : MonoBehaviour
{
    void Update()
    {
        // Si presionas la tecla 'R', se reinicia el nivel
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}