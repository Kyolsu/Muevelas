import os
import concurrent.futures
import uuid
import shutil
from flask import Flask, request, jsonify
from flask_cors import CORS
from generador_preguntas import generar_pregunta_trivia 
from crear_vector_almacenamiento import guardar_libro_con_tematica 
from crear_vector_almacenamiento import get_vector_db 
from administrar_audio import generar_audio_pregunta
from generador_preguntas import generar_pregunta_trivia
app = Flask(__name__)
CORS(app)

cache_preguntas = {}

##
def limpiar_carpeta_audio():
    ruta_audio = os.path.join(os.path.dirname(__file__), 'static', 'audio')
    
    print("--- Realizando Magia XD")
    
    if os.path.exists(ruta_audio):
        try:
            for archivo in os.listdir(ruta_audio):
                ruta_archivo = os.path.join(ruta_audio, archivo)
                if os.path.isfile(ruta_archivo):
                    os.remove(ruta_archivo)
            print("¡Información de audio eliminada!")
        except Exception as e:
            print(f"Aviso: No se pudo limpiar la bodega: {e}")
    else:
        os.makedirs(ruta_audio, exist_ok=True)
        print("Carpeta de audio creada y lista.")


def tarea_generar_completa(tematica):
    pregunta_data = generar_pregunta_trivia(tematica)
    
    if pregunta_data:
        id_unico = str(uuid.uuid4())[:8]
        url_audio = generar_audio_pregunta(pregunta_data['pregunta'], id_unico)
        pregunta_data['audio_url'] = url_audio
        
        return pregunta_data
    return None

def tarea_generar(tematica):
    """Función auxiliar para el hilo paralelo"""
    return generar_pregunta_trivia(tematica)

@app.route('/preparar-partida', methods=['POST'])
def preparar_partida():
    data = request.get_json()
    tematica = data.get('tematica', 'cultura_general').lower()
    cantidad = data.get('cantidad', 10)

    if tematica not in cache_preguntas:
        cache_preguntas[tematica] = []

    print(f"--- Iniciando generación paralela de {cantidad} preguntas para: {tematica} ---")

    with concurrent.futures.ThreadPoolExecutor() as executor:

        futuros = [executor.submit(tarea_generar_completa, tematica) for _ in range(cantidad)]
        
        for f in concurrent.futures.as_completed(futuros):
            pregunta = f.result()
            if pregunta:
                cache_preguntas[tematica].append(pregunta)

    return jsonify({"mensaje": f"Bodega lista con {len(cache_preguntas[tematica])} preguntas"}), 200

@app.route('/ver-bodega', methods=['GET'])
def ver_bodega():
    return jsonify({
        "estado_bodega": {
            tematica: {
                "cantidad": len(preguntas),
                "contenido": preguntas
            } for tematica, preguntas in cache_preguntas.items()
        }
    })

@app.route('/pregunta', methods=['GET'])
def get_trivia():
    tematica = request.args.get('tematica', 'biologia').lower() # <-- Asegúrate de que coincida con tu Unity
    print(f"Solicitud recibida para: {tematica}")

    # 1. Revisar si hay preguntas CON AUDIO listas en la bodega
    if tematica in cache_preguntas and len(cache_preguntas[tematica]) > 0:
        pregunta = cache_preguntas[tematica].pop(0)
        print(f"¡Pregunta con audio sacada de la bodega! Quedan {len(cache_preguntas[tematica])}")
        return jsonify(pregunta), 200

    # 2. Si la bodega está vacía, usamos tarea_generar_completa (que incluye ElevenLabs)
    print("Bodega vacía, generando texto y voz en vivo (tardará unos segundos)...")
    
    # ¡ESTA ES LA LÍNEA MÁGICA QUE FALTABA!
    pregunta = tarea_generar_completa(tematica) 

    if pregunta:
        return jsonify(pregunta), 200
    return jsonify({"error": "No se pudo generar la pregunta"}), 500

@app.route('/agregar-libro', methods=['POST'])
def add_book():
    data = request.get_json()
    archivo = data.get('archivo')
    categoria = data.get('categoria')

    if not archivo or not categoria:
        return jsonify({"error": "Datos incompletos"}), 400

    try:
        guardar_libro_con_tematica(archivo, categoria)
        return jsonify({"mensaje": "Libro procesado exitosamente"}), 201
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    #
    limpiar_carpeta_audio()
    print("Pre-cargando base de datos...")
    get_vector_db() 
    
    app.run(host='0.0.0.0', port=5000, debug=True)