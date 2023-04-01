using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitsLaw.Class
{
    internal class Trail
    {
        
        
        DBConnection dbConnection = new DBConnection();
        
        
        public void generateRandomTrail()
        {

            dbConnection.Open();
            Random random = new Random();
            string query;

            for (int i = 1; i <= 15; i++)
            {
                int diameter = random.Next(30, 101);
                int distance = random.Next(100, 501);
                string direction = random.Next(2) == 0 ? "left" : "right";
                query = $"INSERT INTO `tasks` (`diameter`,`distance`,`direction`) VALUES ('{diameter}','{distance}','{direction}');";
                dbConnection.ExecuteNonQuery(query);
            }
            dbConnection.Close();
        }
    }
}
