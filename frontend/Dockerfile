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

CMD ["npm", "run", "preview"]
# Comando que se ejecuta al iniciar el contenedor: levanta el frontend en modo "preview" de Vite.

# ERROR: Nunca usar rutas como 'COPY frontend/ ./', si el Dockerfile ya está dentro de 'frontend/'.
# CONSECUENCIA: Docker no encuentra los archivos y falla la construcción de la imagen.
# REGLA: Siempre que uses COPY, la ruta de origen es relativa a la ubicación del Dockerfile, no a la raíz del proyecto.
# SOLUCIÓN: Usar 'COPY . ./' para copiar todo el contenido de la carpeta del Dockerfile al contenedor.
