using MonsterTradingCardGame.Objects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class PackageRepository
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";

        public PackageRepository() { }

        public int GetPackageId()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                int rowCount = 0;
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM packages;", connection))
                {
                    rowCount = Convert.ToInt32(command.ExecuteScalar());
                }
                connection.Close();

                return rowCount;
            }
        }
        public void AddPackage(Package package)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Save the package
                        SavePackage(connection, transaction, package);

                        // Save each card in the package
                        foreach (var card in package.Cards)
                        {
                            SaveCard(connection, transaction, card, package.PackageId);
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving package and cards: {ex.Message}");
                        transaction.Rollback();
                    }
                }

                connection.Close();
            }
        }


        private void SavePackage(NpgsqlConnection connection, NpgsqlTransaction transaction, Package package)
        {
            using (var command = new NpgsqlCommand("INSERT INTO packages (package_id, bought) VALUES (@package_id, @bought)", connection, transaction))
            {
                command.Parameters.AddWithValue("@package_id", package.PackageId);
                command.Parameters.AddWithValue("@bought", package.Bought);

                command.ExecuteNonQuery();
            }
        }

        private void SaveCard(NpgsqlConnection connection, NpgsqlTransaction transaction, Card card, int packageId)
        {
            using (var command = new NpgsqlCommand("INSERT INTO cards (id, name, damage, package_id) VALUES (@Id, @name, @damage, @packageId)", connection, transaction))
            {
                command.Parameters.AddWithValue("@Id", card.Id);
                command.Parameters.AddWithValue("@name", card.Name);
                command.Parameters.AddWithValue("@damage", card.Damage);
                command.Parameters.AddWithValue("@packageId", packageId);

                command.ExecuteNonQuery();
            }
        }


    }
}
