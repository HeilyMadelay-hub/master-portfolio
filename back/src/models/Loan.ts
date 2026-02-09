import { DataTypes, Model, Optional } from "sequelize";
import sequelize from "../service/db";

export interface LoanAttributes {
 id: number;
 usuarioId: number;
 libroId: number;
 fechaPrestamo: Date;
 fechaDevolucionPrevista: Date;
 fechaDevolucionReal?: Date | null;
 createdAt?: Date;
 updatedAt?: Date;
}

type LoanCreationAttributes = Optional<LoanAttributes, "id" | "fechaDevolucionReal" | "createdAt" | "updatedAt">;

export class Loan extends Model<LoanAttributes, LoanCreationAttributes> implements LoanAttributes {
 public id!: number;
 public usuarioId!: number;
 public libroId!: number;
 public fechaPrestamo!: Date;
 public fechaDevolucionPrevista!: Date;
 public fechaDevolucionReal!: Date | null;
 public readonly createdAt!: Date;
 public readonly updatedAt!: Date;
}

Loan.init(
 {
 id: { type: DataTypes.INTEGER, autoIncrement: true, primaryKey: true },
 usuarioId: { type: DataTypes.INTEGER, allowNull: false },
 libroId: { type: DataTypes.INTEGER, allowNull: false },
 fechaPrestamo: { type: DataTypes.DATE, allowNull: false },
 fechaDevolucionPrevista: { type: DataTypes.DATE, allowNull: false },
 fechaDevolucionReal: { type: DataTypes.DATE, allowNull: true },
 },
 { sequelize, tableName: "loans" }
);

export default Loan;// Note: we define associations in index.ts to ensure proper ordering