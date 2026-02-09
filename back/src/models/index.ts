// src/models/index.ts
import Book from "./Book";
import Loan from "./Loan";
import User from "./User";

// Set up associations
User.hasMany(Loan, { foreignKey: "usuarioId", as: "prestamos" });
Book.hasMany(Loan, { foreignKey: "libroId", as: "prestamos" });
Loan.belongsTo(User, { foreignKey: "usuarioId", as: "usuario" });
Loan.belongsTo(Book, { foreignKey: "libroId", as: "libro" });

// Exporta los modelos DESPUÉS de configurar las asociaciones
export { Book, Loan, User };
export default { Book, Loan, User };