import cv2
import mediapipe as mp
from mediapipe.tasks.python import vision
from mediapipe.tasks.python import BaseOptions
import time
import socket

UDP_IP = "127.0.0.1" 
UDP_PORT = 5052      
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

result_list = []

def res_callback(result, output_image, timestamp_ms):
     result_list.append(result)

options = vision.HandLandmarkerOptions(
     base_options=BaseOptions(model_asset_path="hand_landmarker.task"),
     running_mode=vision.RunningMode.LIVE_STREAM,
     num_hands=1, 
     min_hand_detection_confidence=0.5,
     min_hand_presence_confidence=0.5,
     min_tracking_confidence=0.5,
     result_callback=res_callback)

landmarker = vision.HandLandmarker.create_from_options(options)
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

print("Cámara encendida. Buscando 1 mano...")

while True:
    ret, frame = cap.read()
    if not ret: break
    
    frame = cv2.flip(frame, 1)
    h, w, _ = frame.shape
    frame_rgb = mp.Image(image_format=mp.ImageFormat.SRGB, data=cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    
    landmarker.detect_async(frame_rgb, time.time_ns() // 1_000_000)

    # --- EL ARREGLO ESTÁ AQUÍ ---
    if len(result_list) > 0:
        resultado_actual = result_list[0]
        
        # Si el resultado tiene manos, enviamos a Unity
        if resultado_actual.hand_landmarks:
            mano = resultado_actual.hand_landmarks[0]
            nudillo = mano[9] 
            
            cx, cy = int(nudillo.x * w), int(nudillo.y * h)
            cv2.circle(frame, (cx, cy), 15, (0, 255, 0), -1)

            mensaje = f"{nudillo.x},{nudillo.y}"
            sock.sendto(mensaje.encode(), (UDP_IP, UDP_PORT))
        
        # IMPORTANTE: Limpiamos la memoria SIEMPRE, haya manos o no
        result_list.clear()

    cv2.imshow('Mano Virtual - Hackathon', frame)
    if cv2.waitKey(1) & 0xFF == 27: break

cap.release()
cv2.destroyAllWindows()