import { DataTypes, Model, Optional } from "sequelize";
import sequelize from "../service/db";

export interface BookAttributes {
 id: number;
 titulo: string;
 autor: string;
 stock: number;
 createdAt?: Date;
 updatedAt?: Date;
}

type BookCreationAttributes = Optional<BookAttributes, "id" | "stock" | "createdAt" | "updatedAt">;

export class Book extends Model<BookAttributes, BookCreationAttributes> implements BookAttributes {
 public id!: number;
 public titulo!: string;
 public autor!: string;
 public stock!: number;
 public readonly createdAt!: Date;
 public readonly updatedAt!: Date;
}

Book.init(
 {
 id: { type: DataTypes.INTEGER, autoIncrement: true, primaryKey: true },
 titulo: { type: DataTypes.STRING, allowNull: false },
 autor: { type: DataTypes.STRING, allowNull: false },
 stock: { type: DataTypes.INTEGER, allowNull: false, defaultValue:0 },
 },
 { sequelize, tableName: "books" }
);

export default Book;