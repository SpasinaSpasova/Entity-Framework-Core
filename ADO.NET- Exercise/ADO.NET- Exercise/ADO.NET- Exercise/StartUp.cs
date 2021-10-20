using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ADO.NET__Exercise
{
    class StartUp
    {
        static void Main(string[] args)
        {
            SqlConnection conn = new SqlConnection(Configuration.CONNECTION_STRING);

            conn.Open();

            using (conn)
            {

                //For problem04
                /*
                 Console.WriteLine("Enter minion info: ");
                string[] minionInfo = Console.ReadLine()?
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();

                string minionName = minionInfo[1];
                int minionAge = int.Parse(minionInfo[2]);
                string townName = minionInfo[3];

                Console.WriteLine("Enter villain info: ");
                string villainName = Console.ReadLine()?
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                */



            }
        }
        private static void problem09(SqlConnection conn)
        {
            SqlCommand command = new SqlCommand(Query.problem09, conn);
            command.ExecuteNonQuery();

            int id = int.Parse(Console.ReadLine());

            command = new SqlCommand($"EXEC usp_GetOlder @id = {id}", conn);
            command.ExecuteNonQuery();
            command = new SqlCommand($"SELECT Name, Age FROM Minions WHERE Id = {id}", conn);

            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"{reader[0]} - {reader[1]} years old");
                }
            }
        }
        private static void problem08(SqlConnection conn)
        {
            List<string> minionsNames = new List<string>();

            SqlCommand command = new SqlCommand(Query.problem08, conn);

            int[] ids = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray();

            foreach (var id in ids)
            {
                SqlParameter parameter = new SqlParameter("@Id", id);

                command.Parameters.Add(parameter);
                command.ExecuteNonQuery();
                command.Parameters.Remove(parameter);
            }

            command = new SqlCommand(Query.problem0801, conn);
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    minionsNames.Add(reader[0].ToString() + " " + reader[1].ToString());
                }
            }

            Console.WriteLine(string.Join(Environment.NewLine, minionsNames));
        }
        private static void problem04(SqlConnection conn, string minionName, int minionAge, string townName, string villainName)
        {
            SqlCommand getTownIdCmd = new SqlCommand(Query.ID_BY_TOWN_NAME, conn);
            getTownIdCmd.Parameters.AddWithValue("@townName", townName);

            object townIdObject = getTownIdCmd.ExecuteScalar();

            if (townIdObject == null)
            {
                SqlCommand insertTownCmd = new SqlCommand(Query.INSERT_TOWN, conn);
                insertTownCmd.Parameters.AddWithValue("@townName", townName);

                int rowsAffectedT = insertTownCmd.ExecuteNonQuery();

                if (rowsAffectedT == 0)
                {
                    Console.WriteLine("Problem occured while inserting new town into the database MinionsDB! Please try again later!");
                    return;
                }

                townIdObject = getTownIdCmd.ExecuteScalar();
                Console.WriteLine($"Town {townName} was added to the database.");
            }

            int townId = (int)townIdObject;

            SqlCommand getVillainIdCmd = new SqlCommand(Query.ID_BY_VILLAIN_NAME, conn);
            getVillainIdCmd.Parameters.AddWithValue("@Name", villainName);

            object villainIdObject = getVillainIdCmd.ExecuteScalar();

            if (villainIdObject == null)
            {
                SqlCommand insertVillainCmd = new SqlCommand(Query.INSERT_VILLAIN, conn);
                insertVillainCmd.Parameters.AddWithValue("@villainName", villainName);

                int rowsAffectedV = insertVillainCmd.ExecuteNonQuery();

                if (rowsAffectedV == 0)
                {
                    Console.WriteLine("Problem occured while inserting new villain into the database MinionsDB! Please try again later!");
                    return;
                }

                villainIdObject = getVillainIdCmd.ExecuteScalar();
                Console.WriteLine($"Villain {villainName} was added to the database.");
            }

            int villainId = (int)villainIdObject;

            SqlCommand insertMinionCmd = new SqlCommand(Query.INSERT_MINION, conn);
            insertMinionCmd.Parameters.AddWithValue("@name", minionName);
            insertMinionCmd.Parameters.AddWithValue("@age", minionAge);
            insertMinionCmd.Parameters.AddWithValue("@townId", townId);

            int rowsAffected = insertMinionCmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                Console.WriteLine("Problem occured while inserting new minion into the database MinionsDB! Please try again later!");
                return;
            }

            SqlCommand getMinionIdCmd = new SqlCommand(Query.ID_BY_MINION_NAME, conn);
            getMinionIdCmd.Parameters.AddWithValue("@Name", minionName);

            int minionId = (int)getMinionIdCmd.ExecuteScalar();

            SqlCommand insertMinionVillainCmd = new SqlCommand(Query.INSERT_MINION_VILLAIN, conn);
            insertMinionVillainCmd.Parameters.AddWithValue("@villainId", villainId);
            insertMinionVillainCmd.Parameters.AddWithValue("@minionId", minionId);

            int rowsAffectedMV = insertMinionVillainCmd.ExecuteNonQuery();

            if (rowsAffectedMV == 0)
            {
                Console.WriteLine("Problem occured while inserting new minion under the control of the given villain! Please try again later!");
                return;
            }

            Console.WriteLine($"Successfully added {minionName} to be minion of {villainName}.");
        }
        private static void Problem03(SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand(Query.problem0301, conn);

            SqlParameter parameter = new SqlParameter("Id", SqlDbType.Int);
            int id = int.Parse(Console.ReadLine());
            parameter.Value = id;
            comm.Parameters.Add(parameter);

            string villainName = (string)(comm.ExecuteScalar());

            if (villainName != null)
            {
                Console.WriteLine($"Villain: {villainName}");
            }
            else
            {
                Console.WriteLine($"No villain with ID {parameter.Value.ToString()} exists in the database.");
                return;
            }


            SqlCommand comm2 = new SqlCommand(Query.problem0302, conn);
            SqlParameter parameter2 = new SqlParameter("Id", SqlDbType.Int);
            parameter2.Value = id;
            comm2.Parameters.Add(parameter2);


            SqlDataReader reader = comm2.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("(no minions)");
            }
            else
            {

                while (reader.Read())
                {
                    Console.WriteLine($"{reader["RowNum"]}. {reader["Name"]} {reader["Age"]}");
                }
            }
        }
        private static void Problem02(SqlConnection conn)
        {
            SqlCommand comm = new SqlCommand(Query.problem02, conn);
            SqlDataReader reader = comm.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["name"] } - {reader["MinionsCount"]}");
            }
        }

        private static void problem05(SqlConnection conn)
        {
            string country = Console.ReadLine();

            SqlCommand cmd = new SqlCommand(Query.problem05, conn);
            cmd.Parameters.AddWithValue("@countryName", country);
            SqlDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                Console.WriteLine("No town names were affected.");
                return;
            }
            reader.Close();
            SqlCommand cmd1 = new SqlCommand(Query.problem0501, conn);
            cmd1.Parameters.AddWithValue("@countryName", country);
            var res = cmd1.ExecuteScalar();
            if (res == null)
            {
                Console.WriteLine("No town names were affected.");
                return;
            }

            SqlCommand cmd2 = new SqlCommand(Query.problem0502, conn);
            cmd2.Parameters.AddWithValue("@countryName", country);
            Console.WriteLine($"{(int)cmd2.ExecuteNonQuery()} town names were affected.");
            SqlDataReader reader2 = cmd.ExecuteReader();
            Console.Write("[");
            while (reader2.Read())
            {

                Console.Write($"{reader2["name"]}, ");
            }
            Console.Write("]");

        }
        public static void problem06(SqlConnection conn)
        {
            int villianId = int.Parse(Console.ReadLine());

            SqlCommand comm = new SqlCommand(Query.problem0601, conn);
            comm.Parameters.AddWithValue("@villainId", villianId);

            int count = (int)comm.ExecuteScalar();

            if (count == 0)
            {
                Console.WriteLine($"No such villain was found.");
            }
            else
            {
                comm = new SqlCommand(Query.problem0604, conn);
                comm.Parameters.AddWithValue("@villainId", villianId);


                string name = comm.ExecuteScalar().ToString();

                comm = new SqlCommand(Query.problem0602, conn);
                comm.Parameters.AddWithValue("@villainId", villianId);


                int affectedRows = comm.ExecuteNonQuery();

                comm = new SqlCommand(Query.problem0603, conn);

                comm.Parameters.AddWithValue("@villainId", villianId);


                comm.ExecuteNonQuery();

                Console.WriteLine($"{name} was deleted.");
                Console.WriteLine($"{affectedRows} minions was released.");
            }
        }

        public static void problem07(SqlConnection conn)
        {
            List<string> minionsNames = new List<string>();

            SqlCommand command = new SqlCommand(Query.problem07, conn);
            SqlDataReader reader = command.ExecuteReader();
            using (reader)
            {
                while (reader.Read())
                {
                    minionsNames.Add(reader["Name"].ToString());
                }
            }

            int counter = 0;

            while (minionsNames.Any())
            {
                if (counter % 2 == 0)
                {
                    Console.WriteLine(minionsNames[0]);

                    minionsNames.RemoveAt(0);
                }
                else
                {
                    Console.WriteLine(minionsNames[minionsNames.Count - 1]);
                    minionsNames.RemoveAt(minionsNames.Count - 1);
                }

                counter++;
            }
        }
    }
   
}

