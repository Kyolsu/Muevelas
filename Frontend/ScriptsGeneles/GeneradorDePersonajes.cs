using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorDePersonajes : MonoBehaviour
{
    public GameObject[] personajesPrefabs;
    public Transform puntoDeAparicion1;
    public Transform puntoDeAparicion2;

    // ¡NUEVO! Aquí conectaremos a la cámara
    public CamaraSeguidora camaraScript;

    void Start()
    {
        int idPersonaje1 = PlayerPrefs.GetInt("PersonajeElegido1", 0);
        int idPersonaje2 = PlayerPrefs.GetInt("PersonajeElegido2", 1);

        // Creamos al primer personaje y lo GUARDAMOS en una variable (jugador1)
        GameObject jugador1 = Instantiate(personajesPrefabs[idPersonaje1], puntoDeAparicion1.position, Quaternion.identity);

        // Creamos al segundo personaje
        Instantiate(personajesPrefabs[idPersonaje2], puntoDeAparicion2.position, Quaternion.identity);

        // ¡NUEVO! Le decimos a la cámara que siga al jugador1
        if (camaraScript != null)
        {
            camaraScript.objetivo = jugador1.transform;
        }
    }
}
