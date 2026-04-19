import os
from elevenlabs.client import ElevenLabs
from dotenv import load_dotenv

load_dotenv()

api_key = os.getenv("ELEVEN_API_KEY")
client_11 = ElevenLabs(api_key=api_key)

def generar_audio_pregunta(texto, id_pregunta):
    try:
        ruta_base = os.path.dirname(os.path.abspath(__file__))
        ruta_carpeta = os.path.join(ruta_base, "static", "audio")
        
        if not os.path.exists(ruta_carpeta):
            os.makedirs(ruta_carpeta, exist_ok=True)

        MI_VOICE_ID = "ClNifCEVq1smkl4M3aTk" 

        print(f"Generando audio para: {id_pregunta}...")
        texto = texto.strip() + "..."
        audio_generator = client_11.text_to_speech.convert(
            voice_id=MI_VOICE_ID,
            text=texto,
            model_id="eleven_multilingual_v2", 
            output_format="mp3_44100_128",  
            voice_settings={
                "stability": 0.55,           
                "similarity_boost": 0.75,   
                "style": 0.0,                
                "use_speaker_boost": True    
            }
        )

        nombre_archivo = f"pregunta_{id_pregunta}.mp3"
        ruta_completa = os.path.join(ruta_carpeta, nombre_archivo)
        
        with open(ruta_completa, "wb") as f:
            for chunk in audio_generator:
                f.write(chunk)
        
        print(f"¡Éxito! Archivo guardado en: {ruta_completa}")
        return f"http://127.0.0.1:5000/static/audio/{nombre_archivo}"
    
    except Exception as e:
        print(f"¡ERROR EN ELEVENLABS!: {e}")
        return None

if __name__ == "__main__":
    if not api_key:
        print("ERROR: Falta ELEVEN_API_KEY en el .env")
    else:
        generar_audio_pregunta("Prueba de voz exitosa .", "test_v2")