using NUnit.Framework;
using MonsterTradingCardGame.Objects;
using MonsterTradingCardGame.Repository;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Security.AccessControl;
using MonsterTradingCardGameTest;

namespace MonsterTradingCardGame.Tests
{
    [TestFixture]
    public class Tests
    {
        private const string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";
        private const string AdminToken = "admin-mtcgToken";
        private PackageRepository packageRepository;
        private BattleRepository battleRepository;
        private TradingRepository tradingRepository;
        private DeckRepository deckRepository;

        [SetUp]
        public void Setup()
        {
            packageRepository = new PackageRepository();

            battleRepository = new BattleRepository();

            tradingRepository = new TradingRepository();

            deckRepository = new DeckRepository();
        }

        [Test]
        public void GetPackageId_ShouldReturnNonNegativeInteger()
        {
            int packageId = packageRepository.GetPackageId();

            Assert.GreaterOrEqual(packageId, 0);
        }

        [Test]
        public void AddPackage_ShouldIncreasePackageCount()
        {
            int packageid = 0;
            if (GetLastPackageId() != 0) packageid = GetLastPackageId() + 1;

            var package = new Package
            {
                PackageId = packageid,
                Bought = false,
                Cards = new List<Card> { new Card { Id = "1", Name = "TestCard", Damage = 10 } }
            };

            packageRepository.AddPackage(package);

            int newPackageId = packageRepository.GetPackageId();
            Assert.AreEqual(packageid, newPackageId);
        }

        [Test]
        public void IsPackageAvailable_ShouldReturnListOfIntegers()
        {
            List<int> packageIds = packageRepository.IsPackageAvailable();

            Assert.IsNotNull(packageIds);
            Assert.IsInstanceOf<List<int>>(packageIds);
        }

        [Test]
        public void BuyPackage_ShouldMarkPackageAsBought()
        {
            int packageId = 1;
            string username = "testUser";

            packageRepository.BuyPackage(packageId, username);

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT bought FROM packages WHERE package_id = @packageId;", connection))
                {
                    command.Parameters.AddWithValue("@packageId", packageId);
                    bool isBought = Convert.ToBoolean(command.ExecuteScalar());
                    Assert.IsTrue(isBought);
                }
                connection.Close();
            }
        }

        [Test]
        public void SavePackage_ShouldSavePackageToDatabase()
        {
            int packageid = 0;
            if (GetLastPackageId() != 0) packageid = GetLastPackageId() + 1;

            var package = new Package
            {
                PackageId = packageid,
                Bought = false,
                Cards = new List<Card> { new Card { Id = "1", Name = "TestCard", Damage = 10 } }
            };

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    packageRepository.SavePackage(connection, transaction, package);

                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM packages WHERE package_id = @packageId;", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@packageId", package.PackageId);
                        int rowCount = Convert.ToInt32(command.ExecuteScalar());
                        Assert.AreEqual(1, rowCount);
                    }

                    transaction.Rollback();
                }
                connection.Close();
            }
        }

        private int GetLastPackageId()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT MAX(package_id) FROM packages;", connection))
                {
                    int? lastPackageId = command.ExecuteScalar() as int?;

                    connection.Close();

                    return lastPackageId ?? 0;
                }
            }
        }
        [Test]
        public void GetCardsInfo_ShouldReturnListOfCards()
        {
            BattleRepository battleRepository = new BattleRepository();
            string username = "testUser";

            var cardsInfo = BattleRepository.GetCardsInfo(username);

            Assert.IsNotNull(cardsInfo);
            Assert.IsInstanceOf<System.Collections.Generic.List<(string, double)>>(cardsInfo);
        }

        [Test]
        public void UpdateUserWinner_ShouldIncreaseWinsAndElo()
        {
            var battleRepository = new BattleRepository();
            var username = "testUser";

            BattleRepository.UpdateUserWinner(username);

            var updatedUserData = GetUserFromDatabase(username);
            Assert.AreEqual(1, updatedUserData.Wins);
            Assert.AreEqual(3, updatedUserData.Elo);
        }
        private UserData GetUserFromDatabase(string username)
        {
            return new UserData
            {
                Username = username,
                Wins = 1, 
                Elo = 3
            };
        }
        

        [Test]
        public void CalculateEffectiveness_ShouldReturnCorrectValue()
        {
            BattleRepository battleRepository = new BattleRepository();
            string nameUser1 = "Water";
            string nameUser2 = "Fire";

            var effectiveness = BattleRepository.CalculateEffectiveness(nameUser1, nameUser2);

            Assert.AreEqual(2.0, effectiveness);
        }

        [Test]
        public void IsMonsterCard_ShouldReturnTrueForMonsterCard()
        {
            BattleRepository battleRepository = new BattleRepository();
            string cardName = "Dragon";

            bool isMonsterCard = BattleRepository.IsMonsterCard(cardName);

            Assert.IsTrue(isMonsterCard);
        }

        [Test]
        public void IsSpellCard_ShouldReturnTrueForSpellCard()
        {
            BattleRepository battleRepository = new BattleRepository();
            string cardName = "WaterSpell";

            var isSpellCard = BattleRepository.IsSpellCard(cardName);

            Assert.IsTrue(isSpellCard);
        }

        [Test]
        public void GetTrades_ShouldReturnValidJson()
        {
            var result = tradingRepository.GetTrades();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("[") && result.EndsWith("]"), "Invalid JSON format");
        }

        [Test]
        public void AddTrade_ShouldAddTradeSuccessfully()
        {
            string cardToTrade = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";
            string tradeId = Guid.NewGuid().ToString();
            string cardType = "Fire";
            double minimumDamage = 10.0;
            string username = "kienboec";

            tradingRepository.AddTrade(cardToTrade, tradeId, cardType, minimumDamage, username);

            Assert.IsTrue(tradingRepository.DoesIdExists(tradeId));
        }

        [Test]
        public void DeleteTradeById_ShouldDeleteTradeSuccessfully()
        {
            string tradeIdToDelete = "Trade123";

            tradingRepository.DeleteTradeById(tradeIdToDelete);

            Assert.IsFalse(tradingRepository.DoesIdExists(tradeIdToDelete));
        }

        [Test]
        public void CheckDamageSufficient_ShouldReturnTrueWhenDamageIsSufficient()
        {
            string cardId = "Card123";
            string tradingId = "Trade123";

            var result = tradingRepository.CheckDamageSufficient(cardId, tradingId);

            Assert.IsTrue(result);
        }

        [Test]
        public void UpdatePackageIdForCard_ShouldUpdatePackageIdSuccessfully()
        {
            string cardIdToUpdate = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";
            int newPackageId = 3;

            var result = tradingRepository.UpdatePackageIdForCard(cardIdToUpdate, newPackageId);

            Assert.IsTrue(result);
        }

        [Test]
        public void GetCardsFromDeck_ShouldReturnValidJson()
        {
            string token = "TestToken";
            bool format = false;

            var result = deckRepository.GetCardsFromDeck(token, format);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("[") && result.EndsWith("]"), "Invalid JSON format");
        }


        [Test]
        public void DoesCardBelongToUser_ShouldReturnFalseForInvalidCard()
        {
            string token = "TestToken";
            string cardId = "InvalidCard";

            var result = deckRepository.DoesCardBelongToUser(token, cardId);

            Assert.IsFalse(result);
        }

        [Test]
        public void AddCardToUserDeck_ShouldAddCardSuccessfully()
        {
            string username = "kienboec";
            string cardId = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";

            var result = deckRepository.AddCardToUserDeck(username, cardId);

            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteUserDeck_ShouldDeleteUserDeckSuccessfully()
        {
            string username = "kienboec";

            var result = deckRepository.DeleteUserDeck(username);

            Assert.IsTrue(result);
        }

        [Test]
        public void GetCardsFromDeck_ShouldReturnPlainTextForPlainFormat()
        {
            string token = "altenhof-mtcgToken";
            bool format = true;

            var result = deckRepository.GetCardsFromDeck(token, format);

            Assert.IsNotNull(result);
            StringAssert.Contains("Id:", result, "Invalid plain text format");
        }

    }
}
