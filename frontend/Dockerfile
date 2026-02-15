FROM node:20-alpine
# Usa la imagen oficial de Node.js 20 en versión Alpine (ligera).

WORKDIR /app
# Define /app como directorio de trabajo dentro del contenedor.
# Todos los comandos siguientes se ejecutarán aquí.

COPY package*.json ./
# Copia los archivos package.json y package-lock.json (si existe) del host al contenedor.
# Esto permite instalar dependencias sin copiar todo el código todavía.

RUN npm install
# Instala las dependencias listadas en package.json dentro del contenedor.

COPY . ./
# Copia todo el contenido de la carpeta frontend del host al contenedor.

RUN npm run build
# Genera la versión de producción del frontend (build optimizado para Vite + React).

EXPOSE 4173
# Expone el puerto 4173 para acceder al servidor de preview.

CMD ["npm", "run", "preview", "--", "--host"]
# Comando que se ejecuta al iniciar el contenedor: levanta el frontend en modo "preview" de Vite.

# ERROR: Nunca usar rutas como 'COPY frontend/ ./', si el Dockerfile ya está dentro de 'frontend/'.
# CONSECUENCIA: Docker no encuentra los archivos y falla la construcción de la imagen.
# REGLA: Siempre que uses COPY, la ruta de origen es relativa a la ubicación del Dockerfile, no a la raíz del proyecto.
# SOLUCIÓN: Usar 'COPY . ./' para copiar todo el contenido de la carpeta del Dockerfile al contenedor.

# ERROR: Vite dentro del contenedor estaba escuchando solo en 'localhost' (127.0.0.1) por defecto.
#         Esto significa que solo aceptaba conexiones desde dentro del contenedor, no desde tu máquina host.
# CONSECUENCIA: Cuando intentabas abrir http://localhost:4173 en tu navegador, el host no podía conectarse al contenedor,
#              por eso recibías ERR_EMPTY_RESPONSE.
# REGLA: Siempre que un contenedor exponga un servidor accesible desde fuera, el servidor debe escuchar en '0.0.0.0',
#        que indica "acepta conexiones desde cualquier interfaz de red dentro del contenedor".
# SOLUCIÓN: Modificar el comando de Vite para escuchar en todas las interfaces usando --host:
#           CMD ["npm", "run", "preview", "--", "--host"]
#           Esto hace que el frontend dentro del contenedor sea accesible desde tu máquina host
#           a través del puerto expuesto en Docker (4173).
