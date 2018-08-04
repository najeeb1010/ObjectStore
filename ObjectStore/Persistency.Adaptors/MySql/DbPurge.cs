using MySql.Data.MySqlClient;

namespace X.ObjectStore {
    public static class DbPurge {
        public static void _PurgeDb (string passcode) {
            if (!passcode.Equals ("QWEDSAZXC")) {
                return;
            }

            string[] tableNames = new string[] {
                "L_ObjectUniqueIndexes", "L_ObjectDependencies", "L_ObjectVersions", "L_Objects"
            };

            foreach (string tableName in tableNames) {
                string sql = string.Format ("delete from {0}", tableName);

                int numRowsDeleted = 0;
                using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                    cnn.Open ();

                    using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {

                        using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                            numRowsDeleted = myCommand.ExecuteNonQuery ();
                            transaction.Commit ();
                        }
                    }
                }
            }
        }
    }
}
