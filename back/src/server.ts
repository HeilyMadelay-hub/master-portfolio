import dotenv from "dotenv";
import express from "express";
import cors from "cors";
import path from "path";
import sequelize from "./service/db";
import { Book, User, Loan } from "./models/index";
import swaggerUi from "swagger-ui-express";
import swaggerJsdoc from "swagger-jsdoc";
import { swaggerOptions } from "./swagger/swagger";
import { runSeedIfEmpty } from "./seed/autoSeed";
import userRoutes from "./routes/userRoutes";
import bookRoutes from "./routes/bookRoutes";
import loanRoutes from "./routes/loanRoutes";

dotenv.config();

const app = express();
const swaggerSpec = swaggerJsdoc(swaggerOptions);

// Middleware
app.use(cors());
app.use(express.json());
app.use("/api-docs", swaggerUi.serve, swaggerUi.setup(swaggerSpec));

// Rutas de la API
app.use("/api/books", bookRoutes);
app.use("/api/users", userRoutes);
app.use("/api/loans", loanRoutes);

// Servir archivos estáticos del frontend
app.use(express.static(path.join(__dirname, '../../front/dist')));

// Ruta catch-all para SPA (debe ir al final)
app.get('{*path}', (req, res) => {
  res.sendFile(path.join(__dirname, '../../front/dist/index.html'));
});

const PORT = Number(process.env.PORT) || 4000;

const start = async () => {
    try {
        await sequelize.authenticate();
        console.log("Conexión a SQLite OK");

        await sequelize.sync({ alter: true });
        await runSeedIfEmpty();
        console.log("Modelos sincronizados");

        // ✅ CORRECCIÓN: Añadir host '0.0.0.0' y arreglar console.log
        app.listen(PORT, '0.0.0.0', () => {
            console.log(`Servidor corriendo en el puerto ${PORT}`);
        });
    } catch (error) {
        console.error("Error iniciando la app:", error);
        process.exit(1);
    }
};

start();

