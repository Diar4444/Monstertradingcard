﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class DBinitRepository
    {
        private string host = "localhost";
        private string username = "postgres";
        private string password = "Halamadrid1";
        private string database = "postgres";

        public DBinitRepository()
        {
            DropTable("user_packages");
            DropTable("cards");
            DropTable("packages");
            DropTable("users");

            CreateTable("users", "CREATE TABLE IF NOT EXISTS users (token varchar(255) ,username VARCHAR(255) NOT NULL PRIMARY KEY UNIQUE,password VARCHAR(255) NOT NULL,coins int NOT NULL);");
            CreateTable("packages", "CREATE TABLE IF NOT EXISTS packages (package_id SERIAL PRIMARY KEY, bought BOOLEAN NOT NULL);");
            CreateTable("cards", "CREATE TABLE IF NOT EXISTS cards (id VARCHAR(255) PRIMARY KEY, name VARCHAR(255) NOT NULL, damage DOUBLE PRECISION NOT NULL, package_id INTEGER REFERENCES packages(package_id));");
            CreateTable("user_packages", "CREATE TABLE IF NOT EXISTS user_packages (username VARCHAR(255) REFERENCES users(username), package_id INT REFERENCES packages(package_id), PRIMARY KEY (username, package_id));");
        }

        private string getConnectionString()
        {
            return "Host="+host+";Username="+username+";Password="+password+";Database="+database;
        }

        private void DropTable(string tableName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand($"DROP TABLE IF EXISTS {tableName};", connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error dropping table '{tableName}': {ex.Message}");
                    }
                }

                connection.Close();
            }
        }

        private void CreateTable(string tableName, string createTableSql)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(createTableSql, connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating table '{tableName}': {ex.Message}");
                    }
                }

                connection.Close();
            }
        }
    }
}