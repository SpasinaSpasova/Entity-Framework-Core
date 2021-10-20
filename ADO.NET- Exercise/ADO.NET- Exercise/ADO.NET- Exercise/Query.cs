using System;
using System.Collections.Generic;
using System.Text;

namespace ADO.NET__Exercise
{
    public static class Query
    {

        public const string problem02 = @"SELECT v.Name, 
                                                 COUNT(mv.VillainId) AS MinionsCount  
                                                FROM Villains AS v 
                                                JOIN MinionsVillains AS mv ON v.Id = mv.VillainId 
                                                GROUP BY v.Id, v.Name 
                                                HAVING COUNT(mv.VillainId) > 3 
                                            ORDER BY COUNT(mv.VillainId)";

        public const string problem0301 = @"SELECT Name FROM VILLAINS WHERE Id=@id";

        public const string problem0302 = @"SELECT ROW_NUMBER() OVER (ORDER BY m.Name) as RowNum,
                                         m.Name, 
                                         m.Age
                                    FROM MinionsVillains AS mv
                                    JOIN Minions As m ON mv.MinionId = m.Id
                                   WHERE mv.VillainId = @Id
                                ORDER BY m.Name";

        public const string ID_BY_TOWN_NAME = @"SELECT Id FROM Towns WHERE Name = @townName";

        public const string INSERT_TOWN = @"INSERT INTO Towns (Name) VALUES (@townName)";

        public const string ID_BY_VILLAIN_NAME = @"SELECT Id FROM Villains WHERE Name = @Name";

        public const string INSERT_VILLAIN = @"INSERT INTO Villains (Name, EvilnessFactorId)  VALUES (@villainName, 4)";

        public const string INSERT_MINION = @"INSERT INTO Minions (Name, Age, TownId) VALUES (@nam, @age, @townId)";

        public const string ID_BY_MINION_NAME = @"SELECT Id FROM Minions WHERE Name = @Name";

        public const string INSERT_MINION_VILLAIN = @"INSERT INTO MinionsVillains (MinionId, VillainId) VALUES (@villainId, @minionId)";

        public const string problem05 = @" SELECT t.Name 
                                           FROM Towns as t
                                           JOIN Countries AS c ON c.Id = t.CountryCode
                                          WHERE c.Name = @countryName";
        public const string problem0501 = @"SELECT Name FROM Countries  WHERE Name = @countryName";

        public const string problem0502 = @"UPDATE Towns
                                           SET Name = UPPER(Name)
                                         WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name = @countryName)";

        public const string problem0601 = @"SELECT COUNT(*) FROM Villains WHERE Id = @villainId";
        public const string problem0602 = @"DELETE FROM MinionsVillains
                                                WHERE VillainId = @villainId";

        public const string problem0603 = @"DELETE FROM Villains
                                                 WHERE Id = @villainId";

        public const string problem0604 = @"SELECT Name FROM Villains WHERE Id = @villainId";

        public const string problem07 = @"SELECT Name FROM Minions";

        public const string problem08 = @"UPDATE Minions
                                           SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1
                                         WHERE Id = @Id";
        public const string problem0801 = @"SELECT Name, Age FROM Minions";

        public const string problem09= @"CREATE PROC usp_GetOlder @id INT
                                                    AS
                                                    UPDATE Minions
                                                       SET Age += 1
                                                     WHERE Id = @id";
    }
}
