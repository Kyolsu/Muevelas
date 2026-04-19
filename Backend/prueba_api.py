import os
from google import genai
from dotenv import load_dotenv


load_dotenv()
clave = os.getenv("GOOGLE_API_KEY")

client = genai.Client(api_key=clave)

def generar_pregunta(texto_fuente, dificultad):
    prompt = f"""
    Actúa como un profesor. Basándote ÚNICAMENTE en el siguiente texto, 
    genera una pregunta de trivia con 4 opciones.
    
    Texto fuente: {texto_fuente}
    Dificultad: {dificultad}
    
    Responde estrictamente en este formato JSON:
    {{
      "pregunta": "texto de la pregunta",
      "opciones": ["opcion A", "opcion B", "opcion C", "opcion D"],
      "correcta_indice": 0
    }}
    """

    response = client.models.generate_content(
        model="gemini-2.5-flash", 
        contents=prompt
    )
    
    return response.text

try:
    texto_ejemplo = "El protocolo I2C usa dos cables: SDA y SCL."
    resultado = generar_pregunta(texto_ejemplo, "fácil")
    print("--- Respuesta de Gemini ---")
    print(resultado)
except Exception as e:
    print(f"Error detectado: {e}")