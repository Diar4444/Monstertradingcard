using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class DBinitRepository
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";

        public DBinitRepository()
        {
            DropTable(connectionString, "packages");
            DropTable(connectionString, "users");
            DropTable(connectionString, "cards");

            CreateTable(connectionString, "users", "CREATE TABLE IF NOT EXISTS users (token varchar(255) PRIMARY KEY, username VARCHAR(255) NOT NULL, password VARCHAR(255) NOT NULL);");
            CreateTable(connectionString, "packages", "CREATE TABLE IF NOT EXISTS packages (package_id SERIAL PRIMARY KEY, bought BOOLEAN NOT NULL);");
            CreateTable(connectionString, "cards", "CREATE TABLE IF NOT EXISTS cards (id VARCHAR(255) PRIMARY KEY, name VARCHAR(255) NOT NULL, damage DOUBLE PRECISION NOT NULL, package_id INTEGER REFERENCES packages(package_id));");
        }

        private void DropTable(string connectionString, string tableName)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
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
            }
        }

        private void CreateTable(string connectionString, string tableName, string createTableSql)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
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
            }
        }


    }
}
