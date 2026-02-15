FROM python:3.11-slim
# Usa la imagen oficial de Python 3.10 en su versión "slim" (ligera), 
# lo que reduce el tamaño de la imagen final y contiene solo lo esencial de Python.

WORKDIR /app
# Establece el directorio de trabajo dentro del contenedor en /app. 
# Todos los comandos siguientes se ejecutarán desde este directorio.

COPY requirements.txt .
# Copia el archivo de dependencias de Python (requirements.txt) desde tu máquina 
# al directorio de trabajo del contenedor. Esto permite instalar dependencias antes de copiar todo el código.

RUN pip install --no-cache-dir -r requirements.txt
# Instala todas las dependencias listadas en requirements.txt.
# --no-cache-dir evita que pip almacene caché dentro de la imagen, manteniéndola más ligera.

COPY app ./app
# Copia todo el código de tu backend (carpeta app) dentro del contenedor, 
# en el directorio /app/app.

EXPOSE 8000
# Expone el puerto 8000 del contenedor para que pueda recibir tráfico externo.

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
# Comando por defecto al iniciar el contenedor:
# - uvicorn arranca la app FastAPI
# - app.main:app indica el módulo y la instancia de FastAPI
# - --host 0.0.0.0 hace que sea accesible desde fuera del contenedor
# - --port 8000 define el puerto interno



# ERROR: Nunca usar rutas relativas como 'COPY backend/app ./app' si el Dockerfile ya está dentro de 'backend/'.
# CONSECUENCIA: Docker no encuentra la carpeta y falla la construcción de la imagen.
# REGLA: Siempre que uses COPY, la ruta de origen es relativa al Dockerfile, no a la raíz del proyecto.
# SOLUCIÓN: Ajustar COPY según la ubicación real del Dockerfile.

# ERROR: No verificar la compatibilidad de versiones de Python con los paquetes del requirements.txt.
# CONSECUENCIA: pip falla al instalar dependencias por incompatibilidad de Python.
# REGLA: Siempre confirmar que la versión de Python en el Dockerfile soporte todas las dependencias listadas.
# SOLUCIÓN: O actualizar la imagen de Python (ej. python:3.11-slim) o ajustar versiones de los paquetes en requirements.txt.
