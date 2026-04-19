import os
import random
import shutil
from dotenv import load_dotenv
from langchain_chroma import Chroma
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from fragmentar_pdf import procesar_documento, CARPETA_LIBROS
import threading

load_dotenv()

embeddings_global = GoogleGenerativeAIEmbeddings(
    model="models/gemini-embedding-001",
    task_type="retrieval_query"
)
_vector_db_instance = None
_db_lock = threading.Lock() 

def get_vector_db():
    global _vector_db_instance

    with _db_lock: 
        if _vector_db_instance is None:
            print("--- Inicializando conexión con ChromaDB ---")
            if os.path.exists("./db_conocimiento"):
                _vector_db_instance = Chroma(
                    persist_directory="./db_conocimiento",
                    embedding_function=embeddings_global
                )
            else:
                print("Error: No existe la carpeta db_conocimiento")
                return None
    return _vector_db_instance

def obtener_pregunta_aleatoria(tematica):
    vector_db = get_vector_db()
    
    datos = vector_db.get(where={"tematica": tematica.lower()})
    textos = datos['documents']
    
    if not textos:
        return None
        
    return random.choice(textos)

def guardar_libro_con_tematica(nombre_archivo, tematica):
    ruta_libro = os.path.join(CARPETA_LIBROS, nombre_archivo)
    fragmentos = procesar_documento(ruta_libro)
    
    if not fragmentos:
        print(f"Error: No se pudo procesar {nombre_archivo}")
        return
    for doc in fragmentos:
        doc.metadata = {"tematica": tematica.lower()}

    embeddings = GoogleGenerativeAIEmbeddings(
        model="models/gemini-embedding-001",
        task_type="retrieval_document"
    )

    print(f"--- Añadiendo {nombre_archivo} a la categoría: {tematica} ---")

    vector_db = Chroma.from_documents(
        documents=fragmentos,
        embedding=embeddings,
        persist_directory="./db_conocimiento"
    )
    print(f"¡Éxito! {nombre_archivo} ya es parte de la base de datos.")

if __name__ == "__main__":

    #ARCHIVO A LEER Y SU CATEGORIA#
    archivo_pdf = "preguntastriviawejajalolnomames.pdf" 
    mi_categoria = "cultura_general"

    guardar_libro_con_tematica(archivo_pdf, mi_categoria)
    
    resultado = obtener_pregunta_aleatoria(mi_categoria)
    if resultado:
        print("\n--- FRAGMENTO PARA LA TRIVIA ---")
        print(resultado)