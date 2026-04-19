import os
import json
import re
from google import genai
from dotenv import load_dotenv
from crear_vector_almacenamiento import obtener_pregunta_aleatoria

load_dotenv()
#LLAVE API DE GOOGLE#
client = genai.Client(api_key=os.getenv("GOOGLE_API_KEY"))

def generar_pregunta_trivia(tematica):
    fragmento = obtener_pregunta_aleatoria(tematica)
    
    if not fragmento:
        return None
    instrucciones = (
        "Eres un profesor que ACTÚA COMO UN API PURA. " 
        "Genera una pregunta de opción múltiple basada en el texto. "
        "PROHIBIDO: No saludes, no expliques, no des introducciones. "
        "FORMATO: Debes devolver ÚNICAMENTE un objeto JSON válido Donde las opciones tengan máximo 5 palabras sin contar nexos."
        '{"pregunta": "texto", "opciones": ["A", "B", "C", "D"], "correcta": "opcion_exacta"}'
    )

    prompt_usuario = f"{instrucciones}\n\nTexto de referencia: {fragmento}\n\n."

    try:

        response = client.models.generate_content(
            model="gemma-3-27b-it", 
            config=genai.types.GenerateContentConfig(
                #system_instruction=instrucciones,
                #response_mime_type="application/json" 
            ),
            contents=prompt_usuario
        )

        texto_sucio = response.text
        
        texto_limpio = re.sub(r'```json\s*|```', '', texto_sucio).strip()
 
        return json.loads(texto_limpio)

    except Exception as e:
        print(f"Error generando la pregunta: {e}")
        return None
if __name__ == "__main__":
    print("--- GENERANDO PREGUNTA DE HISTORIA ---")
    trivia = generar_pregunta_trivia("historia_mexico")
    
    if trivia:
        print(f"\nPregunta: {trivia['pregunta']}")
        print(f"Opciones: {trivia['opciones']}")
        print(f"Respuesta Correcta: {trivia['correcta']}")