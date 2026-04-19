using UnityEngine;

// Mudamos esta clase aquí para que TODO el juego sepa qué es un "Personaje"
[System.Serializable]
public class Personaje
{
    public string nombre;
    public Sprite imagen;
}

public class DatosPersonaje : MonoBehaviour
{
    // Esta "Instancia" mágica es la que permite que otros scripts le hablen sin hacer conexiones manuales
    public static DatosPersonaje Instancia;

    [Header("Base de Datos GLOBAL")]
    public Personaje[] baseDeDatosGlobal;

    [Header("Datos de la Partida Actual")]
    public int idP1 = 0;
    public int idP2 = 1;
    public int victoriasP1 = 0;
    public int victoriasP2 = 0;
    public int rondaActual = 1;

    void Awake()
    {
        // Si no hay un cerebro todavía, yo me convierto en el cerebro
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject); // ¡MAGIA! No me destruyas al cambiar de escena
        }
        else
        {
            // Si ya existe un cerebro (porque regresamos al menú tras jugar), me destruyo para no haber clones
            Destroy(gameObject);
        }
    }

    public void ReiniciarJuego()
    {
        victoriasP1 = 0;
        victoriasP2 = 0;
        rondaActual = 1;
    }
}