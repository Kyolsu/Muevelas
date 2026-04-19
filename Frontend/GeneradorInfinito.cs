using UnityEngine;

public class GeneradorInfinito : MonoBehaviour
{
    public GameObject[] prefabsBloques; 
    
    // Qué tanto se puede desviar la pieza hacia los lados desde el Spawner
    public float rangoX = 4f; 

    void Start()
    {
        
    }

    public void CrearPieza()
    {
        int indiceAleatorio = Random.Range(0, prefabsBloques.Length);
        GameObject bloqueElegido = prefabsBloques[indiceAleatorio];

        // --- EL ARREGLO ESTÁ AQUÍ ---
        // Tomamos la posición 'X' y 'Y' exacta de este Spawner en específico
        float posXAleatoria = transform.position.x + Random.Range(-rangoX, rangoX);
        Vector3 pos = new Vector3(posXAleatoria, transform.position.y, 0f);
        
        Instantiate(bloqueElegido, pos, Quaternion.identity);
    }
}