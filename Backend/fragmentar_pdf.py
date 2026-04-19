import os
import pdfplumber
from langchain_text_splitters import RecursiveCharacterTextSplitter

# --- CONFIGURACIÓN DE RUTAS ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
CARPETA_LIBROS = os.path.join(BASE_DIR, "libros")

def listar_libros():
    if not os.path.exists(CARPETA_LIBROS):
        print(f"Error: No existe la carpeta {CARPETA_LIBROS}")
        return []

    archivos = [f for f in os.listdir(CARPETA_LIBROS) if f.endswith(('.pdf', '.txt'))]
    return archivos

def procesar_documento(ruta_archivo):
    texto_acumulado = ""
    
    print(f"--- Iniciando extracción: {os.path.basename(ruta_archivo)} ---")
    
    try:
        if ruta_archivo.endswith('.pdf'):
            with pdfplumber.open(ruta_archivo) as pdf:
                print(f"Páginas detectadas: {len(pdf.pages)}")
                for i, pagina in enumerate(pdf.pages):
                    texto_pag = pagina.extract_text()
                    if texto_pag:
                        texto_acumulado += texto_pag + "\n"
                    else:
                        print(f"Advertencia: La página {i+1} parece ser una imagen.")
        else:
            with open(ruta_archivo, 'r', encoding='utf-8') as f:
                texto_acumulado = f.read()

        if not texto_acumulado.strip():
            print("El PDF es una imagen")
            return []

        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=1000,
            chunk_overlap=150,
            length_function=len
        )
        fragmentos = text_splitter.create_documents([texto_acumulado])
        
        print(f"Extracción exitosa. Fragmentos generados: {len(fragmentos)}")
        return fragmentos

    except Exception as e:
        print(f"Ocurrió un error procesando el archivo: {e}")
        return []
if __name__ == "__main__":
    libros = listar_libros()
    
    if libros:
        ruta_final = os.path.join(CARPETA_LIBROS, libros[0])
        mis_fragmentos = procesar_documento(ruta_final)
        if len(mis_fragmentos) > 0:
            print("\n--- VISTA PREVIA DEL PRIMER FRAGMENTO ---")
            print(mis_fragmentos[0].page_content[:500] + "...") 
        else:
            print("\n[!] El sistema no pudo generar fragmentos.")
    else:
        print("No hay archivos en la carpeta /libros/")