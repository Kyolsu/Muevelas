using UnityEngine;

public class PuntoControl : MonoBehaviour
{
    private bool yaTocado = false;
    public ManagerDibujo manager; // Te lo asignaremos desde el manager

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si la mano (pincel) nos toca y no hemos sido tocados antes
        if (!yaTocado && collision.CompareTag("Mano")) // Asegúrate de que tu mano tenga el Tag "Mano"
        {
            yaTocado = true;
            manager.RegistrarPuntoTocado();
            
            // Opcional: Cambiar de color o emitir partículas para que el jugador vea que va bien
            // GetComponent<SpriteRenderer>().color = Color.green; 
        }
    }

    public void ReiniciarPunto()
    {
        yaTocado = false;
        // Regresar color original
    }
}