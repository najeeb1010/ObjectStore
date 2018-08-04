using MySql.Data.MySqlClient;

namespace X.ObjectStore {
    internal static class DbParameter {
        #region << Class Construction >>

        static DbParameter () {
            _dbHost = ConfigRegistrar.GetConfigMethod () (ConfigConst.DB_HOST);
            _dbName = ConfigRegistrar.GetConfigMethod () (ConfigConst.DB_NAME);
            _dbUserID = ConfigRegistrar.GetConfigMethod () (ConfigConst.DB_USERNAME);
            _dbPassword = ConfigRegistrar.GetConfigMethod () (ConfigConst.DB_PASSWORD);

            _connectionStrForReading = "Data Source=" + _dbHost + ";Database=" + _dbName
                      + ";User ID=" + _dbUserID + ";Password=" + _dbPassword
                      + ";Min Pool Size=3;Max Pool Size=5";

            _connectionStrForWriting = "Data Source=" + _dbHost + ";Database=" + _dbName
                      + ";User ID=" + _dbUserID + ";Password=" + _dbPassword
                      + ";Min Pool Size=3;Max Pool Size=5";
        }

        #endregion << Class Construction >>

        #region << Connection Parameters >>

        private static string _dbHost;
        private static string _dbName;
        private static string _dbUserID;
        private static string _dbPassword;

        #endregion << Connection Parameters >>

        #region << Internal Class Variables >>

        private static string _connectionStrForReading;
        private static string _connectionStrForWriting;

        #endregion << Internal Class Variables >>

        #region << Connectivity Methods >>

        public static MySqlConnection GetConnectionObjectForReading () {
            return new MySqlConnection (_connectionStrForReading);
        }

        public static MySqlConnection GetConnectionObjectForWriting () {
            return new MySqlConnection (_connectionStrForWriting);
        }

        #endregion << Connectivity Methods >>
    }
}
