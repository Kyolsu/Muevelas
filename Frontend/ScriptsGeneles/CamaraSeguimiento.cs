using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraSeguidora : MonoBehaviour
{
    public Transform objetivo; // A quién va a seguir
    public float suavidad = 5f; // Qué tan fluido es el movimiento

    // IMPORTANTE: En 2D, la cámara DEBE estar en Z = -10 para ver la escena
    public Vector3 compensacion = new Vector3(0, 1.5f, -10f);

    // Usamos LateUpdate para que la cámara se mueva DESPUÉS de que el personaje caminó
    void LateUpdate()
    {
        if (objetivo != null)
        {
            // Calculamos a dónde debería ir la cámara
            Vector3 posicionDeseada = objetivo.position + compensacion;

            // Lerp hace que el movimiento sea suave y no un salto brusco
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavidad * Time.deltaTime);
        }
    }
}
