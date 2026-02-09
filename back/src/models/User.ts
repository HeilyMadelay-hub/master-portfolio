import { DataTypes, Model, Optional } from "sequelize";
import sequelize from "../service/db";

export interface UserAttributes {
 id: number;
 nombre: string;
 email: string;
 activo: boolean;
 createdAt?: Date;
 updatedAt?: Date;
}

type UserCreationAttributes = Optional<UserAttributes, "id" | "activo" | "createdAt" | "updatedAt">;

export class User extends Model<UserAttributes, UserCreationAttributes> implements UserAttributes {
 public id!: number;
 public nombre!: string;
 public email!: string;
 public activo!: boolean;
 public readonly createdAt!: Date;
 public readonly updatedAt!: Date;
}

User.init(
 {
 id: { type: DataTypes.INTEGER, autoIncrement: true, primaryKey: true },
 nombre: { type: DataTypes.STRING, allowNull: false },
 email: { type: DataTypes.STRING, allowNull: false, unique: true },
 activo: { type: DataTypes.BOOLEAN, allowNull: false, defaultValue: true },
 },
 { sequelize, tableName: "users" }
);

export default User;
