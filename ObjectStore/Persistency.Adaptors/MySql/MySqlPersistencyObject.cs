using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal class MySqlPersistencyObject : IPersister {
        #region << Object Store Methods >>

        public long AddObjectFirstRecord (string objUuid) {
            string sql = "insert into L_Objects (key_ObjectId) values (@objUuid)";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        int numRows = myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                        return myCommand.LastInsertedId;
                    }
                }
            }
        }

        public bool ObjectExists (string objUuid) {
            string sql = "select key_ObjectId from L_Objects where key_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        return reader.Read ();
                    }
                }
            }
        }

        public long PersistObjectAsVersion (string objUuid, string objJson, long versionNumber, string comment) {
            string newKey = objUuid + ":" + versionNumber;
            string sql = "insert into L_ObjectVersions (key_ObjectVersionId, fkey_ObjectId, ObjectJson, Comment) "
                + "values (@newKey, @objUuid, @objJson, @comment)";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@newKey", newKey);
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);
                    myCommand.Parameters.AddWithValue ("@objJson", objJson);
                    myCommand.Parameters.AddWithValue ("@comment", comment);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                        return myCommand.LastInsertedId;
                    }
                }
            }
        }

        public string RetrieveObjectJsonHeadVersion (string objUuid) {
            long lastVersionId = GetObjectLastVersionNumber (objUuid);
            string versionUuid = objUuid + ":" + lastVersionId;

            string sql = "select ObjectJson from L_ObjectVersions where key_ObjectVersionId=@versionUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@versionUuid", versionUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            string json = reader.GetString (0);
                            return json;
                        }
                    }
                }
            }

            return null;
        }

        public string RetrieveObjectJsonHeadVersion (string objUuid, out string comment) {
            long lastVersionId = GetObjectLastVersionNumber (objUuid);
            string versionUuid = objUuid + ":" + lastVersionId;

            string sql = "select ObjectJson, Comment from L_ObjectVersions where key_ObjectVersionId=@versionUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@versionUuid", versionUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            string json = reader.GetString (0);
                            comment = reader.GetString (1);
                            return json;
                        }
                    }
                }
            }

            comment = null;
            return null;
        }

        public string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber) {
            long lastVersionId = GetObjectLastVersionNumber (objUuid);
            if (versionNumber > lastVersionId) {
                return null;
            }

            string versionUuid = objUuid + ":" + versionNumber;
            string sql = "select ObjectJson from L_ObjectVersions where key_ObjectVersionId=@versionUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@versionUuid", versionUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            string json = reader.GetString (0);
                            return json;
                        }
                    }
                }
            }

            return null;
        }

        public string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber, out string comment) {
            long lastVersionId = GetObjectLastVersionNumber (objUuid);
            if (versionNumber > lastVersionId) {
                comment = null;
                return null;
            }

            string versionUuid = objUuid + ":" + versionNumber;
            string sql = "select ObjectJson, Comment from L_ObjectVersions where key_ObjectVersionId=@versionUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@versionUuid", versionUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            string json = reader.GetString (0);
                            comment = reader.GetString (1);
                            return json;
                        }
                    }
                }
            }

            comment = null;
            return null;
        }

        public List<KeyValuePair<string, string>> RetrieveObjectJsonAllVersions (string objUuid) {
            string sql = "select ObjectJson, Comment from L_ObjectVersions where fkey_ObjectId=@objUuid order by WhenAdded";

            List<KeyValuePair<string, string>> allVersions = new List<KeyValuePair<string, string>> ();
            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        while (reader.Read ()) {
                            string json = reader.GetString (0);
                            string comment = reader.GetString (1);
                            KeyValuePair<string, string> p = new KeyValuePair<string, string> (json, comment);
                            allVersions.Add (p);
                        }
                    }
                }
            }

            return allVersions;
        }

        public long GetObjectLastVersionNumber (string objUuid) {
            string sql = "select count(*) from L_ObjectVersions where fkey_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            return reader.GetInt64 (0) - 1;
                        }
                    }
                }
            }

            return -1;
        }

        public void DeleteAllObjectReferences (string objUuid) {
            DeleteFromDependenciesTable (objUuid);
            DeleteFromIndexTables (objUuid);
            DeleteFromVersionsTable (objUuid);
            DeleteFromObjectsTable (objUuid);
        }

        private void DeleteFromDependenciesTable (string objUuid) {
            string sql = "delete from L_ObjectDependencies where "
                + "fkey_PrincipalObjectId=@principalObjUuid or fkey_DependentObjectId=@dependentObjUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@principalObjUuid", objUuid);
                    myCommand.Parameters.AddWithValue ("@dependentObjUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        private void DeleteFromIndexTables (string objUuid) {
            string sql = "delete from L_ObjectUniqueIndexes where fkey_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        private void DeleteFromVersionsTable (string objUuid) {
            string sql = "delete from L_ObjectVersions where fkey_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        private void DeleteFromObjectsTable (string objUuid) {
            string sql = "delete from L_Objects where key_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        #endregion << Object Store Methods >>

        #region << Object Index Methods >>

        public void IndexObject (string objectTypeFullName, string objUuid, string indexValue) {
            string sql = "insert into L_ObjectUniqueIndexes (fkey_ObjectId, ObjectType, Value) values "
                + "(@objUuid, @objectType, @indexValue)";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);
                    myCommand.Parameters.AddWithValue ("@objectType", objectTypeFullName);
                    myCommand.Parameters.AddWithValue ("@indexValue", indexValue);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        int numRows = myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        public bool UniquePropertyValueExists (string objTypeFullName, string propertyValue) {
            string sql = "select key_EntryId from L_ObjectUniqueIndexes where "
                + "ObjectType=@objectType and Value=@indexValue";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objectType", objTypeFullName);
                    myCommand.Parameters.AddWithValue ("@indexValue", propertyValue);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        return reader.Read ();
                    }
                }
            }
        }

        public string GetObjectUuidForUniqueValue (string objectTypeFullName, string uniqueValue) {
            string sql = "select fkey_ObjectId from L_ObjectUniqueIndexes where Value=@indexValue and ObjectType=@objectType";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@indexValue", uniqueValue);
                    myCommand.Parameters.AddWithValue ("@objectType", objectTypeFullName);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        if (reader.Read ()) {
                            return reader.GetString (0);
                        }
                    }
                }
            }

            return null;
        }

        public void DeletePreviousIndex (string objUuid) {
            string sql = "delete from L_ObjectUniqueIndexes where fkey_ObjectId=@objUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@objUuid", objUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        int numRows = myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }
        }

        #endregion << Object Index Methods >>

        #region << Object Dependency Methods >>

        public bool AddObjectDependency (string principalObjUuid, string dependentObjUuid, string principalType, string dependentType, string optionalArg = null) {
            string sql = "insert into L_ObjectDependencies (fkey_PrincipalObjectId, fkey_DependentObjectId, PrincipalType, DependentType, OptionalArg) "
                + "values (@principalObjUuid, @dependentObjUuid, @principalType, @dependentType, @optionalArg)";

            int numRowsAdded = 0;
            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@principalObjUuid", principalObjUuid);
                    myCommand.Parameters.AddWithValue ("@dependentObjUuid", dependentObjUuid);
                    myCommand.Parameters.AddWithValue ("@principalType", principalType);
                    myCommand.Parameters.AddWithValue ("@dependentType", dependentType);
                    myCommand.Parameters.AddWithValue ("@optionalArg", optionalArg ?? string.Empty);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        numRowsAdded = myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }

            return numRowsAdded == 1;
        }

        public bool RemoveObjectDependency (string principalObjUuid, string dependentObjUuid) {
            string sql = "delete from L_ObjectDependencies where "
                + "fkey_PrincipalObjectId=@principalObjUuid and fkey_DependentObjectId=@dependentObjUuid";

            int numRowsDeleted = 0;
            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForWriting ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@principalObjUuid", principalObjUuid);
                    myCommand.Parameters.AddWithValue ("@dependentObjUuid", dependentObjUuid);

                    using (MySqlTransaction transaction = cnn.BeginTransaction ()) {
                        numRowsDeleted = myCommand.ExecuteNonQuery ();
                        transaction.Commit ();
                    }
                }
            }

            return numRowsDeleted == 1;
        }

        public bool ObjectDependencyExists (string principalObjUuid, string dependentObjUuid) {
            string sql = "select fkey_PrincipalObjectId from L_ObjectDependencies where "
                + "fkey_PrincipalObjectId=@principalObjUuid and fkey_DependentObjectId=@dependentObjUuid";

            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@principalObjUuid", principalObjUuid);
                    myCommand.Parameters.AddWithValue ("@dependentObjUuid", dependentObjUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        return reader.Read ();
                    }
                }
            }
        }

        public List<string[]> GetAllDependentObjectsInfo (string principalObjUuid) {
            string sql = "select PrincipalType, fkey_DependentObjectId, DependentType, OptionalArg "
                + "from L_ObjectDependencies where fkey_PrincipalObjectId=@principalObjUuid";

            List<string[]> allDependents = new List<string[]> ();
            using (MySqlConnection cnn = DbParameter.GetConnectionObjectForReading ()) {
                cnn.Open ();

                using (MySqlCommand myCommand = new MySqlCommand (sql, cnn)) {
                    myCommand.Parameters.AddWithValue ("@principalObjUuid", principalObjUuid);

                    using (MySqlDataReader reader = myCommand.ExecuteReader ()) {
                        while (reader.Read ()) {
                            string[] arr = new string[] {
                                principalObjUuid,
                                reader.GetString (0),
                                reader.GetString (1),
                                reader.GetString (2),
                                reader.GetString (3),
                            };
                            allDependents.Add (arr);
                        }
                    }
                }
            }

            return allDependents;
        }

        #endregion << Object Dependency Methods >>
    }
}
