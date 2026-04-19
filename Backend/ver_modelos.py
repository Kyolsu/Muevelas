import os
from google import genai
from dotenv import load_dotenv

load_dotenv()
client = genai.Client(api_key=os.getenv("GOOGLE_API_KEY"))

print("--- Listado completo de modelos en tu cuenta ---")
try:
    for m in client.models.list():
        # Imprimimos el nombre y lo que soporta (usando el diccionario interno para no fallar)
        metodos = getattr(m, 'supported_actions', 'No definido')
        print(f"ID: {m.name} | Métodos: {metodos}")
except Exception as e:
    print(f"Error al listar: {e}")