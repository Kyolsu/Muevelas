using UnityEngine;

public class MovimientoPersonaje : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;
    private Rigidbody2D rb;

    void Start()
    {
        // Esto conecta el script con el motor de físicas de tu personaje
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Movimiento Horizontal (Flechas Izquierda/Derecha o A/D)
        float movimientoX = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(movimientoX * velocidad, rb.velocity.y);

        // 2. Salto (Barra Espaciadora)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);
        }
    }
}