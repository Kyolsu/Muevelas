using UnityEngine;

public class SonidoColision : MonoBehaviour
{
    private AudioSource audioFuente;
    public AudioClip sonidoConcreto;

    void Start()
    {
        // Agregamos el AudioSource automáticamente por código para no hacerlo a mano
        audioFuente = gameObject.AddComponent<AudioSource>();
        audioFuente.playOnAwake = false;
        
        // Configuramos para que el sonido no se escuche igual de fuerte en toda la pantalla (Opcional, da efecto 3D)
        audioFuente.spatialBlend = 0.5f; 
    }

    void OnCollisionEnter2D(Collision2D colision)
    {
        // EL SECRETO: relativeVelocity mide qué tan fuerte fue el impacto. 
        // Si es mayor a 1, fue una caída real. Si es casi 0, solo están apoyados.
        if (colision.relativeVelocity.magnitude > 1f)
        {
            // PRO TIP: Variamos un poquito el "tono" (pitch) al azar. 
            // Así, aunque uses el mismo mp3, cada golpe sonará ligeramente diferente y no hartará al jugador.
            audioFuente.pitch = Random.Range(0.8f, 1.2f);
            
            // Reproducimos el sonido modulando el volumen según la fuerza del golpe
            float volumenFuerza = Mathf.Clamp(colision.relativeVelocity.magnitude / 5f, 0.2f, 1f);
            audioFuente.PlayOneShot(sonidoConcreto, volumenFuerza);
        }
    }
}