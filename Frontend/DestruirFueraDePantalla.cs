using UnityEngine;

public class DestruirFueraDePantalla : MonoBehaviour
{
    // Update se ejecuta en cada frame del juego
    void Update()
    {
        // Si la posición en el eje Y de este objeto es menor a -10 (fuera de la cámara abajo)
        if (transform.position.y < -10f) 
        { 
            // Destruye el objeto para liberar memoria (Garbage Collection)
            Destroy(gameObject); 
        }
    }
}