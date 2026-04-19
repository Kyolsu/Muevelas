using UnityEngine;
using UnityEngine.SceneManagement; 
using static DatosPersonaje; // Importamos el cerebro global

public class MovimientoYCambioEscena : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 100f;
    public float unidadesAAvanzar = 15f;

    private Rigidbody2D rb;
    private float posicionInicialX;
    private bool tocandoPiso = false;
    private bool yaLlego = false;
    private static bool escenaCargando = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        escenaCargando = false; 
    }

    void OnCollisionEnter2D(Collision2D colision)
    {
        if (colision.gameObject.name == "PisoInvisible" && !tocandoPiso)
        {
            tocandoPiso = true;
            posicionInicialX = transform.position.x;
        }
    }

    void FixedUpdate()
    {
        if (tocandoPiso && !yaLlego)
        {
            rb.velocity = new Vector2(velocidad, rb.velocity.y);

            if (transform.position.x >= posicionInicialX + unidadesAAvanzar)
            {
                yaLlego = true;
                rb.velocity = new Vector2(0, rb.velocity.y); 

                if (!escenaCargando)
                {
                    escenaCargando = true;
                    CargarSiguienteMinijuego();
                }
            }
        }
    }

    // --- LA MAGIA DEL ENRUTAMIENTO ---
    void CargarSiguienteMinijuego()
    {
        string escenaDestino = "";

        // Dependiendo de la ronda en la que vayamos, cargamos una escena distinta
        if (DatosPersonaje.Instancia.rondaActual == 1) escenaDestino = "SampleScene";
        else if (DatosPersonaje.Instancia.rondaActual == 2) escenaDestino = "RetoTrivia";
        else if (DatosPersonaje.Instancia.rondaActual == 3) escenaDestino = "RetoDibujo";
        
        // (Asegúrate de que tus escenas se llamen exactamente así en Build Settings)
        SceneManager.LoadScene(escenaDestino);
    }
}