using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement
{
    public class DatabaseConnection
    {
        private static MySqlConnection? _instance;

        private DatabaseConnection() { }

        public static MySqlConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MySqlConnection("Server=localhost;Uid=root;Database=clubmanagement;port=3306;pwd=root");
                }
                return _instance;
            }
        }
    }

}
