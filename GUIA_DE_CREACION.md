Guía de Estructura para API Node.js + TypeScript

Creamos la carpeta 

mkdir event-ticketing-api
cd event-ticketing-api

Mergeamos el gitignore de la rama principal 

git fetch origin
git checkout origin/main -- .gitignore

Hacemos el commit 

git add .gitignore
git commit -m "Merge .gitignore from main"

Subimos a la rama

git push origin event-ticketing-api

Inicializar npm

npm init -y

Instalar TypeScript y configuración base

npm install -D typescript @types/node ts-node nodemon
npm install -D eslint prettier eslint-config-prettier
npm install -D @typescript-eslint/parser @typescript-eslint/eslint-plugin

Crear tsconfig.json

npx tsc --init

Y rellenarlo con 

{
  "compilerOptions": {
    // Versión de JavaScript que generará TypeScript
    "target": "ES2020", 

    // Sistema de módulos que se usará en el JS compilado
    "module": "commonjs",

    // Librerías de tipos disponibles (APIs de ES2020)
    "lib": ["ES2020"],

    // Carpeta donde se guardará el JS compilado
    "outDir": "./dist",

    // Carpeta donde está tu código fuente
    "rootDir": "./src",

    // Activa todas las comprobaciones estrictas de TypeScript
    "strict": true,

    // Permite importar módulos CommonJS con sintaxis moderna
    "esModuleInterop": true,

    // Omite verificación de tipos en node_modules (acelera compilación)
    "skipLibCheck": true,

    // Obliga a usar mayúsculas/minúsculas consistentes en imports
    "forceConsistentCasingInFileNames": true,

    // Permite importar archivos JSON directamente
    "resolveJsonModule": true,

    // Estrategia para resolver módulos (igual que Node.js)
    "moduleResolution": "node",

    // Error si hay variables declaradas y no usadas
    "noUnusedLocals": true,

    // Error si hay parámetros que no se usan
    "noUnusedParameters": true,

    // Todas las rutas de una función deben devolver algo
    "noImplicitReturns": true,

    // Evita olvidos en switch (fallthrough)
    "noFallthroughCasesInSwitch": true
  },

  // Archivos que se incluirán en la compilación
  "include": ["src/**/*"],

  // Archivos/carpetas que se excluirán
  "exclude": ["node_modules", "dist"]
}

# 📚 Guía Completa: Estructura para API Node.js + TypeScript

## 🎯 Estructura Base Recomendada

```
project-name/
│
├── src/
│   ├── config/          # Configuración (env, DB, etc.)
│   ├── db/              # Cliente de base de datos (opcional)
│   ├── models/          # Modelos de datos (ORM)
│   ├── repositories/    # Acceso a datos
│   ├── services/        # Lógica de negocio
│   ├── controllers/     # Manejo de peticiones HTTP
│   ├── routes/          # Definición de endpoints
│   ├── middlewares/     # Auth, validación, CORS, etc.
│   ├── utils/           # Funciones reutilizables
│   ├── validators/      # Validaciones de datos
│   ├── errors/          # Manejo centralizado de errores
│   ├── types/           # Interfaces y tipos TypeScript
│   ├── constants/       # Constantes (roles, estados, etc.)
│   ├── jobs/            # Background jobs (opcional)
│   ├── websocket/       # WebSockets (opcional)
│   ├── app.ts           # Configuración de Express
│   └── main.ts          # Punto de entrada del servidor
│
├── tests/               # Tests unitarios e integración
├── dist/                # Código compilado (JS)
├── node_modules/
├── .env
├── .gitignore
├── package.json
├── tsconfig.json
└── README.md
```

---

## 📦 Responsabilidad de cada carpeta

### 🔴 **CARPETAS OBLIGATORIAS** (toda API debe tenerlas)

| Carpeta | Propósito | Ejemplo | ¿Por qué es obligatoria? |
|---------|-----------|---------|--------------------------|
| **models/** | Definición de entidades (Prisma, Sequelize, Mongoose) | `User.model.ts` | Sin modelos no hay estructura de datos |
| **services/** | Lógica de negocio compleja | `AuthService.ts` | Separar lógica de controllers |
| **controllers/** | Recibe request, devuelve response (sin lógica) | `UserController.ts` | Punto de entrada HTTP |
| **routes/** | Define endpoints (`/users`, `/auth`) | `user.routes.ts` | Necesario para Express |
| **middlewares/** | Funciones intermedias (auth, logs, CORS) | `authMiddleware.ts` | Auth, validación, errores |
| **config/** | Variables de entorno, config DB | `database.config.ts` | Centralizar configuración |
| **utils/** | Helpers generales (JWT, bcrypt, logger) | `jwt.util.ts` | Funciones reutilizables |
| **types/** | Interfaces y tipos TS | `express.d.ts`, `enums.ts` | TypeScript necesita tipos |
| **errors/** | Clases de error personalizadas | `AppError.ts` | Manejo centralizado de errores |

### 🟡 **CARPETAS MUY RECOMENDADAS** (según el proyecto)

| Carpeta | Cuándo usarla | Ejemplo | Tipo de proyecto |
|---------|---------------|---------|------------------|
| **repositories/** | Cuando tienes consultas complejas a BD | `UserRepository.ts` | APIs medianas/grandes, operaciones atómicas |
| **validators/** | Validaciones con Zod/Joi | `userValidator.ts` | Todas las APIs (mejor que validar en controllers) |
| **constants/** | Evitar valores mágicos | `roles.ts`, `orderTimeout.ts` | Cuando tienes muchos valores fijos |
| **jobs/** o **queues/** | Background tasks | `orderExpirationJob.ts` | APIs con tareas asíncronas (emails, limpieza) |
| **websocket/** | Comunicación en tiempo real | `handlers.ts`, `rooms.ts` | APIs con notificaciones en vivo |
| **db/** | Conexión separada a BD | `connection.ts` | Si manejas múltiples conexiones |

### 🟢 **CARPETAS OPCIONALES** (casos específicos)

| Carpeta | Cuándo usarla | Ejemplo |
|---------|---------------|---------|
| **decorators/** | Solo con clases + metadata | `@Role('admin')` |
| **tests/** | Siempre recomendable | `user.test.ts` |
| **docs/** | Documentación API | `swagger.yaml` |
| **scripts/** | Automatizaciones | `seed.ts`, `migrate.ts` |

---

## 📊 Matriz de decisión: ¿Qué carpetas necesito?

### **API Pequeña** (CRUD básico, < 10 endpoints)
```
✅ models, services, controllers, routes, middlewares
✅ config, utils, types, errors
❌ repositories, validators, constants
❌ jobs, websocket, decorators
```

### **API Mediana** (Sistema completo, 10-30 endpoints)
```
✅ TODO lo anterior +
✅ repositories, validators, constants
🟡 jobs (si tienes tareas programadas)
❌ websocket, decorators
```

### **API Compleja** (Tiempo real, concurrencia, background jobs)
```
✅ TODO lo anterior +
✅ jobs, websocket, tests
✅ db (si múltiples conexiones)
🟡 decorators (solo si usas clases)
```

---

## 🔄 Flujo de una petición (arquitectura)

```
Request HTTP
  ↓
Routes (define endpoint)
  ↓
Middlewares (auth, validación)
  ↓
Validators (Zod/Joi schemas)
  ↓
Controllers (recibe y delega)
  ↓
Services (lógica de negocio)
  ↓
Repositories (acceso a BD)
  ↓
Models (ORM)
  ↓
Database
```

**Reglas de flujo:**
- **Controllers** → Solo delegan, no tienen lógica
- **Services** → Toda la lógica de negocio
- **Repositories** → Solo queries a BD (sin lógica)
- **Models** → Solo estructura de datos

---

## 🎯 Ejemplos prácticos por tipo de proyecto

### **Ejemplo 1: API de Autenticación simple**
```
src/
├── config/          # DB + JWT config
├── models/          # User.ts
├── services/        # authService.ts
├── controllers/     # authController.ts
├── routes/          # authRoutes.ts
├── middlewares/     # authMiddleware.ts
├── utils/           # jwt.ts, bcrypt.ts
├── types/           # user.interface.ts
├── errors/          # AppError.ts
├── app.ts
└── main.ts
```
**NO necesitas:** repositories, validators, constants, jobs, websocket

---

### **Ejemplo 2: E-commerce con inventario**
```
src/
├── config/
├── models/          # Product, Order, User
├── repositories/    # productRepository.ts (stock atómico)
├── services/        # orderService.ts, inventoryService.ts
├── controllers/
├── routes/
├── middlewares/
├── validators/      # orderValidator.ts (Zod)
├── utils/
├── types/           # enums.ts (OrderStatus)
├── constants/       # orderStates.ts
├── errors/
├── app.ts
└── main.ts
```
**Añades:** repositories (operaciones atómicas), validators, constants

---

### **Ejemplo 3: Sistema de tickets con tiempo real** (tu caso)
```
src/
├── config/          # DB, JWT, WebSocket
├── models/          # Event, Order, Ticket, User
├── repositories/    # orderRepository.ts ($inc atómico)
├── services/        # orderService.ts, ticketService.ts
├── controllers/
├── routes/
├── middlewares/
├── validators/      # orderValidator.ts
├── jobs/            # orderExpirationJob.ts
├── websocket/       # handlers.ts, rooms.ts
├── utils/
├── types/           # enums.ts (OrderStatus, EventStatus)
├── constants/       # orderTimeout.ts, roles.ts
├── errors/
├── app.ts
└── main.ts
```
**Añades:** repositories, validators, constants, jobs, websocket

---

## ⚙️ Configuración TypeScript obligatoria

**tsconfig.json**
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "commonjs",
    "outDir": "./dist",
    "rootDir": "./src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "experimentalDecorators": true,  // Solo si usas decorators
    "emitDecoratorMetadata": false   // false por defecto
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist"]
}
```

---

## 🔴 Elementos ESENCIALES (implementación)

### 1️⃣ Manejo de errores centralizado
```typescript
// errors/AppError.ts
export class AppError extends Error {
  constructor(
    public message: string, 
    public statusCode: number = 500,
    public code?: string
  ) {
    super(message);
    this.name = 'AppError';
  }
}

// errors/errorCodes.ts
export const ErrorCodes = {
  VALIDATION_ERROR: 'VALIDATION_ERROR',
  UNAUTHORIZED: 'UNAUTHORIZED',
  NOT_FOUND: 'NOT_FOUND',
  INSUFFICIENT_STOCK: 'INSUFFICIENT_STOCK'
} as const;
```

### 2️⃣ Utilidades JWT
```typescript
// utils/jwt.ts
import jwt from 'jsonwebtoken';

export const generateToken = (payload: object): string => {
  return jwt.sign(payload, process.env.JWT_SECRET!, {
    expiresIn: process.env.JWT_EXPIRES_IN || '7d'
  });
};

export const verifyToken = (token: string) => {
  return jwt.verify(token, process.env.JWT_SECRET!);
};
```

### 3️⃣ Tipos personalizados
```typescript
// types/express.d.ts
import { User } from '../models/User';

declare global {
  namespace Express {
    interface Request {
      user?: User;
    }
  }
}

// types/enums.ts
export enum OrderStatus {
  PENDING = 'PENDING',
  RESERVED = 'RESERVED',
  CONFIRMED = 'CONFIRMED',
  CANCELLED = 'CANCELLED',
  EXPIRED = 'EXPIRED'
}
```

---

## 🟡 Decorators vs Middlewares: ¿Cuándo usar cada uno?

### **Usa Middlewares cuando:**
✅ Lógica a nivel de ruta (auth, CORS, body-parser)  
✅ API funcional (sin clases)  
✅ Express estándar

```typescript
// middlewares/authMiddleware.ts
export const authenticate = (req, res, next) => {
  const token = req.headers.authorization?.split(' ')[1];
  if (!token) throw new AppError('Unauthorized', 401);
  
  req.user = verifyToken(token);
  next();
};
```

### **Usa Decorators cuando:**
✅ Trabajas con **clases** (controllers como clases)  
✅ Usas frameworks como **NestJS** o **TypeORM**  
✅ Necesitas metadata avanzada (roles, permisos)

```typescript
// decorators/role.decorator.ts
export function RequireRole(role: string) {
  return function (target: any, propertyKey: string, descriptor: PropertyDescriptor) {
    const originalMethod = descriptor.value;
    descriptor.value = async function (...args: any[]) {
      const req = args[0];
      if (req.user.role !== role) {
        throw new AppError('Forbidden', 403);
      }
      return originalMethod.apply(this, args);
    };
  };
}

// Uso:
class UserController {
  @RequireRole('ADMIN')
  async deleteUser(req, res) { ... }
}
```

**Tabla comparativa:**

| Concepto | Uso | Sintaxis | Complejidad |
|----------|-----|----------|-------------|
| **Middlewares** | Lógica a nivel de ruta | Funciones | Simple |
| **Decorators** | Metadata a nivel de clase/método | `@Decorator` | Avanzada |

**Recomendación:** Empieza con **middlewares**. Solo usa decorators si ya trabajas con clases.

---

## 🚀 Scripts de PowerShell para crear estructura

### **Script 1: Estructura básica (API pequeña/mediana)**
```powershell
# Carpetas principales
"config", "models", "services", "controllers", "routes", 
"middlewares", "utils", "types", "errors" | 
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

# Archivos principales
New-Item -ItemType File -Path "src\app.ts" -Force
New-Item -ItemType File -Path "src\main.ts" -Force

# Archivos de configuración
New-Item -ItemType File -Path ".env.example" -Force
New-Item -ItemType File -Path ".gitignore" -Force
New-Item -ItemType File -Path "tsconfig.json" -Force
```

### **Script 2: Estructura completa (API compleja)**
```powershell
# Carpetas base
"config", "models", "repositories", "services", "controllers", 
"routes", "middlewares", "validators", "utils", "types", 
"errors", "constants" | 
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

# Carpetas avanzadas (tiempo real, jobs)
"jobs", "websocket" | 
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

# Tests
"unit", "integration" | 
ForEach-Object { New-Item -ItemType Directory -Path "tests\$_" -Force }

# Archivos principales
New-Item -ItemType File -Path "src\app.ts" -Force
New-Item -ItemType File -Path "src\main.ts" -Force

# Configuración
New-Item -ItemType File -Path ".env.example" -Force
New-Item -ItemType File -Path ".gitignore" -Force
New-Item -ItemType File -Path "tsconfig.json" -Force
New-Item -ItemType File -Path "README.md" -Force
```

---

## 💡 Reglas de oro

1. **Empieza simple** → Agrega carpetas solo cuando las necesites
2. **Controllers sin lógica** → Delegan todo a services
3. **Services sin SQL** → Usan repositories
4. **Un archivo, una responsabilidad**
5. **Constantes > strings mágicos**
6. **Tipos > any**
7. **Errores centralizados** → No uses `throw new Error()` directamente

---

## 🎯 Checklist: ¿Qué carpetas necesito?

**Pregúntate esto:**

- ✅ **¿Tengo operaciones atómicas en BD?** → Añade `repositories/`
- ✅ **¿Uso Zod/Joi para validar?** → Añade `validators/`
- ✅ **¿Tengo muchos valores fijos?** → Añade `constants/`
- ✅ **¿Necesito tareas programadas?** → Añade `jobs/`
- ✅ **¿Uso WebSockets?** → Añade `websocket/`
- ✅ **¿Trabajo con clases + metadata?** → Añade `decorators/`
- ✅ **¿Múltiples conexiones a BD?** → Añade `db/`

---

## 📌 Resumen visual rápido

```
🔴 OBLIGATORIO (toda API):
   models, services, controllers, routes, middlewares
   config, utils, types, errors

🟡 MUY RECOMENDADO (según proyecto):
   repositories, validators, constants
   jobs, websocket

🟢 OPCIONAL (casos específicos):
   decorators (solo con clases), tests, docs, db
```

---

## 🚦 Siguiente paso

1. **Identifica el tipo de tu proyecto** (pequeña/mediana/compleja)
2. **Crea la estructura base** con el script correspondiente
3. **Define tus enums y tipos** en `types/`
4. **Implementa modelos** en `models/`
5. **Empieza por un servicio simple** en `services/`


¿Dónde meter el Docker en una API pequeña/mediana/compleja?

# 🐳 Docker por Tipo de API: La Guía Definitiva

Como senior, te voy a dar **la respuesta práctica** basada en experiencia real, no teoría.

---

## 🎯 Respuesta Directa

| Tipo de API | ¿Docker desde día 1? | ¿Qué dockerizar? | Momento de creación |
|-------------|---------------------|------------------|---------------------|
| **API Pequeña** | 🟡 **Opcional** | Solo MongoDB/PostgreSQL | Día 3-5 (cuando funcione local) |
| **API Mediana** | ✅ **Recomendado** | DB + API (desarrollo) | Día 1-2 (con setup inicial) |
| **API Compleja** | 🔴 **OBLIGATORIO** | DB + API + Redis/RabbitMQ | Día 0 (antes de escribir código) |

---

## 📊 Análisis Detallado por Tipo

### 🟢 **API PEQUEÑA** (CRUD básico, < 10 endpoints)

#### **Estructura sin Docker:**
```
project-name/
├── src/
│   ├── config/
│   ├── models/
│   ├── services/
│   ├── controllers/
│   ├── routes/
│   ├── middlewares/
│   ├── utils/
│   ├── types/
│   ├── errors/
│   ├── app.ts
│   └── main.ts
├── .env.example
├── tsconfig.json
└── package.json
```

#### **¿Cuándo añadir Docker?**

**Opción A: Sin Docker (desarrollo local)**
```bash
# Día 1
npm install
npm run dev

# Conexión a MongoDB Atlas (cloud) o instalación local
MONGODB_URI=mongodb+srv://user:pass@cluster.mongodb.net/mydb
```

**✅ Ventajas:**
- Setup en 5 minutos
- No necesitas entender Docker
- Ideal para prototipos rápidos

**❌ Desventajas:**
- Cada dev instala MongoDB diferente
- "En mi máquina funciona" 🤷

---

**Opción B: Docker solo para base de datos (recomendado)**

```
project-name/
├── src/
├── docker-compose.yml          # ⚠️ SOLO BD
├── .dockerignore
└── package.json
```

**docker-compose.yml** (versión mínima):
```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: admin123

volumes:
  mongo-data:
```

**Workflow:**
```bash
# Una vez
docker-compose up -d mongodb

# Desarrollar normalmente
npm run dev
```

**.env:**
```env
MONGODB_URI=mongodb://admin:admin123@localhost:27017/mydb?authSource=admin
```

**✅ Cuándo usar esta opción:**
- Equipo de 2-5 personas
- No necesitas transacciones
- Quieres consistencia de BD sin complicar

---

### 🟡 **API MEDIANA** (10-30 endpoints, lógica de negocio)

#### **Estructura recomendada:**
```
project-name/
├── src/
│   ├── config/
│   ├── models/
│   ├── repositories/        # ⚠️ NUEVO
│   ├── services/
│   ├── controllers/
│   ├── routes/
│   ├── middlewares/
│   ├── validators/          # ⚠️ NUEVO
│   ├── utils/
│   ├── types/
│   ├── constants/           # ⚠️ NUEVO
│   ├── errors/
│   ├── app.ts
│   └── main.ts
├── docker/                  # ⚠️ NUEVO
│   ├── development/
│   │   └── Dockerfile.dev
│   └── mongodb/
│       └── init-replica-set.sh
├── docker-compose.yml       # ⚠️ DB + API
├── .dockerignore
├── tsconfig.json
└── package.json
```

#### **¿Cuándo añadir Docker?**

**DÍA 1-2:** Setup completo con Docker

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    command: ["--replSet", "rs0", "--bind_ip_all"]  # ⚠️ Replica set opcional
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: admin123

  api:                        # ⚠️ NUEVO
    build:
      context: .
      dockerfile: docker/development/Dockerfile.dev
    ports:
      - "3000:3000"
    volumes:
      - ./src:/app/src        # Hot reload
      - /app/node_modules
    environment:
      NODE_ENV: development
      MONGODB_URI: mongodb://admin:admin123@mongodb:27017/mydb?authSource=admin
      JWT_SECRET: dev-secret
      PORT: 3000
    depends_on:
      - mongodb
    command: npm run dev

volumes:
  mongo-data:
```

**docker/development/Dockerfile.dev:**
```dockerfile
FROM node:20-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]
```

**Workflow:**
```bash
# Cada día
docker-compose up -d

# Los cambios en src/ se reflejan automáticamente
code src/services/userService.ts

# Ver logs
docker-compose logs -f api

# Apagar
docker-compose down
```

**✅ Cuándo usar Docker completo:**
- Equipo de 3+ personas
- Necesitas consistencia total
- API con lógica compleja de negocio
- Preparación para producción

---

### 🔴 **API COMPLEJA** (Tiempo real, concurrencia, background jobs)

#### **Estructura obligatoria:**
```
event-ticketing-api/
├── src/
│   ├── config/
│   │   ├── database.ts
│   │   ├── websocket.ts     # ⚠️ Socket.io
│   │   └── redis.ts         # ⚠️ NUEVO (opcional)
│   ├── models/
│   ├── repositories/        # ⚠️ Operaciones atómicas
│   ├── services/
│   ├── controllers/
│   ├── routes/
│   ├── middlewares/
│   ├── validators/
│   ├── jobs/                # ⚠️ Background tasks
│   ├── websocket/           # ⚠️ Real-time handlers
│   ├── utils/
│   ├── types/
│   ├── constants/
│   ├── errors/
│   ├── app.ts
│   └── main.ts
├── docker/                  # ⚠️ CRÍTICO
│   ├── development/
│   │   └── Dockerfile.dev
│   ├── production/
│   │   └── Dockerfile       # Multi-stage
│   ├── mongodb/
│   │   └── init-replica-set.sh
│   └── nginx/               # Opcional (reverse proxy)
│       └── nginx.conf
├── tests/
│   ├── unit/
│   └── integration/
├── docker-compose.yml       # ⚠️ Desarrollo
├── docker-compose.prod.yml  # ⚠️ Producción
├── docker-compose.test.yml  # ⚠️ Tests
├── .dockerignore
├── tsconfig.json
└── package.json
```

#### **Docker desde DÍA 0 (OBLIGATORIO)**

**¿Por qué?**

```
❌ Sin Docker:
- MongoDB sin replica set → Transacciones fallan
- No puedes testear concurrencia
- Redis/RabbitMQ manual en cada máquina
- WebSockets: diferentes puertos/configs

✅ Con Docker desde día 0:
- Replica set automático
- Tests de concurrencia desde día 1
- Redis + MongoDB juntos
- Todo el equipo: entorno idéntico
```

---

**docker-compose.yml** (completo):
```yaml
version: '3.8'

services:
  # MongoDB con Replica Set (OBLIGATORIO para transacciones)
  mongodb:
    image: mongo:7.0
    container_name: ticketing-mongo
    command: ["--replSet", "rs0", "--bind_ip_all"]
    ports:
      - "27017:27017"
    volumes:
      - mongo-data:/data/db
      - ./docker/mongodb/init-replica-set.sh:/docker-entrypoint-initdb.d/init-replica-set.sh
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: admin123
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis (opcional, para caché/sessions)
  redis:
    image: redis:7-alpine
    container_name: ticketing-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data

  # API Node.js
  api:
    build:
      context: .
      dockerfile: docker/development/Dockerfile.dev
    container_name: ticketing-api
    ports:
      - "3000:3000"
      - "9229:9229"  # Debugging
    volumes:
      - ./src:/app/src
      - /app/node_modules
    environment:
      NODE_ENV: development
      MONGODB_URI: mongodb://admin:admin123@mongodb:27017/ticketing?authSource=admin&replicaSet=rs0
      REDIS_URL: redis://redis:6379
      JWT_SECRET: dev-secret
      PORT: 3000
    depends_on:
      mongodb:
        condition: service_healthy
      redis:
        condition: service_started
    command: npm run dev

volumes:
  mongo-data:
  redis-data:
```

**docker/mongodb/init-replica-set.sh:**
```bash
#!/bin/bash
sleep 10

mongosh --host localhost:27017 -u admin -p admin123 --authenticationDatabase admin <<EOF
rs.initiate({
  _id: "rs0",
  members: [{ _id: 0, host: "mongodb:27017" }]
});
EOF

echo "✅ Replica set inicializado"
```

**docker/development/Dockerfile.dev:**
```dockerfile
FROM node:20-alpine

RUN apk add --no-cache git

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

EXPOSE 3000 9229

CMD ["npm", "run", "dev"]
```

**docker/production/Dockerfile:**
```dockerfile
FROM node:20-alpine AS builder

WORKDIR /app

COPY package*.json ./
RUN npm ci --only=production

COPY tsconfig.json ./
COPY src ./src

RUN npm run build

# Stage 2: Runtime
FROM node:20-alpine

WORKDIR /app

COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/dist ./dist
COPY package*.json ./

RUN addgroup -g 1001 -S nodejs && \
    adduser -S nodejs -u 1001
USER nodejs

EXPOSE 3000

CMD ["node", "dist/main.js"]
```

---

## 🎯 Matriz de Decisión: ¿Qué dockerizar y cuándo?

### **Criterios de decisión:**

| Pregunta | Respuesta | Acción |
|----------|-----------|--------|
| ¿Necesitas transacciones MongoDB? | ✅ Sí | Docker desde día 0 (replica set) |
| ¿Tienes background jobs? | ✅ Sí | Docker desde día 1 |
| ¿Usas WebSockets? | ✅ Sí | Docker desde día 1 |
| ¿Equipo de 3+ personas? | ✅ Sí | Docker desde día 1-2 |
| ¿Solo CRUD básico? | ❌ No | Docker opcional (solo BD) |
| ¿Prototipo temporal? | ❌ No | Sin Docker |

---

## 📅 Timeline de Implementación

### **API PEQUEÑA:**
```
Día 1: Setup proyecto + npm install
Día 2-5: Desarrollar en local
Día 6: (Opcional) Añadir docker-compose.yml solo para BD
```

### **API MEDIANA:**
```
Día 1: Setup proyecto + docker-compose.yml (BD)
Día 2: Añadir servicio API a Docker
Día 3+: Desarrollar con docker-compose up
```

### **API COMPLEJA:**
```
Día 0: Setup Docker COMPLETO antes de código
  - docker-compose.yml (MongoDB + Redis)
  - Dockerfile.dev
  - init-replica-set.sh
Día 1: Probar replica set funciona
Día 2+: Desarrollar con docker-compose up
```

---

## 🚀 Scripts de PowerShell para Cada Caso

### **Script 1: API Pequeña (sin Docker)**
```powershell
# Estructura base
"config", "models", "services", "controllers", "routes", 
"middlewares", "utils", "types", "errors" |
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

New-Item -ItemType File -Path "src\app.ts" -Force
New-Item -ItemType File -Path "src\main.ts" -Force
New-Item -ItemType File -Path ".env.example" -Force
New-Item -ItemType File -Path "tsconfig.json" -Force
New-Item -ItemType File -Path ".gitignore" -Force

Write-Host "✅ API pequeña creada (sin Docker)" -ForegroundColor Green
```

### **Script 2: API Mediana (Docker básico)**
```powershell
# Estructura base
"config", "models", "repositories", "services", "controllers", 
"routes", "middlewares", "validators", "utils", "types", 
"constants", "errors" |
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

# Docker
New-Item -ItemType Directory -Path "docker\development" -Force
New-Item -ItemType File -Path "docker\development\Dockerfile.dev" -Force
New-Item -ItemType File -Path "docker-compose.yml" -Force
New-Item -ItemType File -Path ".dockerignore" -Force

# Archivos principales
New-Item -ItemType File -Path "src\app.ts" -Force
New-Item -ItemType File -Path "src\main.ts" -Force
New-Item -ItemType File -Path ".env.example" -Force
New-Item -ItemType File -Path "tsconfig.json" -Force
New-Item -ItemType File -Path ".gitignore" -Force

Write-Host "✅ API mediana creada (Docker básico)" -ForegroundColor Green
```

### **Script 3: API Compleja (Docker completo)**
```powershell
# Estructura completa
"config", "models", "repositories", "services", "controllers", 
"routes", "middlewares", "validators", "jobs", "websocket",
"utils", "types", "constants", "errors" |
ForEach-Object { New-Item -ItemType Directory -Path "src\$_" -Force }

# Docker completo
New-Item -ItemType Directory -Path "docker\development" -Force
New-Item -ItemType Directory -Path "docker\production" -Force
New-Item -ItemType Directory -Path "docker\mongodb" -Force

New-Item -ItemType File -Path "docker\development\Dockerfile.dev" -Force
New-Item -ItemType File -Path "docker\production\Dockerfile" -Force
New-Item -ItemType File -Path "docker\mongodb\init-replica-set.sh" -Force

New-Item -ItemType File -Path "docker-compose.yml" -Force
New-Item -ItemType File -Path "docker-compose.prod.yml" -Force
New-Item -ItemType File -Path "docker-compose.test.yml" -Force
New-Item -ItemType File -Path ".dockerignore" -Force

# Tests
New-Item -ItemType Directory -Path "tests\unit" -Force
New-Item -ItemType Directory -Path "tests\integration" -Force

# Archivos principales
New-Item -ItemType File -Path "src\app.ts" -Force
New-Item -ItemType File -Path "src\main.ts" -Force
New-Item -ItemType File -Path ".env.example" -Force
New-Item -ItemType File -Path "tsconfig.json" -Force
New-Item -ItemType File -Path ".gitignore" -Force
New-Item -ItemType File -Path "README.md" -Force

Write-Host "✅ API compleja creada (Docker completo)" -ForegroundColor Green
```

---

## ✅ Checklist por Tipo de API

### **API PEQUEÑA:**
- [ ] `docker-compose.yml` (solo si equipo > 2 personas)
- [ ] Solo MongoDB dockerizado
- [ ] API corre en local con `npm run dev`

### **API MEDIANA:**
- [ ] `docker-compose.yml` (MongoDB + API)
- [ ] `docker/development/Dockerfile.dev`
- [ ] `.dockerignore`
- [ ] Hot reload funcionando

### **API COMPLEJA:**
- [ ] `docker-compose.yml` (MongoDB + Redis + API)
- [ ] `docker/development/Dockerfile.dev`
- [ ] `docker/production/Dockerfile` (multi-stage)
- [ ] `docker/mongodb/init-replica-set.sh`
- [ ] `docker-compose.test.yml`
- [ ] `.dockerignore`
- [ ] Health checks configurados

---

## 🎓 Resumen Ejecutivo

| Tipo de API | Docker | Ubicación | Cuándo crear |
|-------------|--------|-----------|--------------|
| **Pequeña** | 🟡 Opcional | Solo `docker-compose.yml` en raíz | Día 3-5 (si necesitas) |
| **Mediana** | ✅ Recomendado | `docker/` + `docker-compose.yml` | Día 1-2 |
| **Compleja** | 🔴 Obligatorio | `docker/` completo + múltiples compose | Día 0 |

---

## 💡 Regla de Oro

> **"Docker no es por el tamaño del proyecto, es por la COMPLEJIDAD de la infraestructura"**

**Dockeriza desde día 0 si necesitas:**
- ✅ Replica set de MongoDB (transacciones)
- ✅ Múltiples servicios (Redis, RabbitMQ)
- ✅ Background jobs
- ✅ WebSockets
- ✅ Tests de concurrencia

**Dockeriza después (o nunca) si:**
- ❌ CRUD simple sin transacciones
- ❌ Prototipo temporal
- ❌ Equipo de 1 persona
- ❌ MongoDB Atlas en la nube

---

**Respuesta directa a tu pregunta:**

En tu caso específico (sistema de tickets con concurrencia y tiempo real) = **API COMPLEJA** → Docker va en **DÍA 0**, estructura completa con `docker/` y múltiples compose files.