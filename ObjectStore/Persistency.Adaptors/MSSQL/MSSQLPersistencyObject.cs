using System.Collections.Generic;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal class MSSQLPersistencyObject : IPersister {
        #region << Object Store Methods >>

        public long AddObjectFirstRecord (string objUuid) {
            return -1;
        }

        public bool ObjectExists (string objUuid) {
            return false;
        }

        public long PersistObjectAsVersion (string objUuid, string objJson, long versionNumber, string comment) {
            return -1;
        }

        public string RetrieveObjectJsonHeadVersion (string objUuid) {
            return null;
        }

        public string RetrieveObjectJsonHeadVersion (string objUuid, out string comment) {
            comment = null;
            return null;
        }

        public string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber) {
            return null;
        }

        public string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber, out string comment) {
            comment = null;
            return null;
        }

        public List<KeyValuePair<string, string>> RetrieveObjectJsonAllVersions (string objUuid) {
            return null;
        }

        public long GetObjectLastVersionNumber (string objUuid) {
            return -1;
        }

        public void DeleteAllObjectReferences (string objUuid) {
        }

        #endregion << Object Store Methods >>

        #region << Object Index Methods >>

        public void IndexObject (string objectTypeFullName, string objUuid, string indexValue) {
        }

        public bool UniquePropertyValueExists (string objTypeFullName, string propertyValue) {
            return false;
        }

        public string GetObjectUuidForUniqueValue (string objectTypeFullName, string uniqueValue) {
            return null;
        }

        public void DeletePreviousIndex (string objUuid) {
        }

        #endregion << Object Index Methods >>

        #region << Object Dependency Methods >>

        public bool AddObjectDependency (string principalObjUuid, string dependentObjUuid, string principalType, string dependentType, string optionalArg = null) {
            return false;
        }

        public bool RemoveObjectDependency (string principalObjUuid, string dependentObjUuid) {
            return false;
        }

        public bool ObjectDependencyExists (string principalObjUuid, string dependentObjUuid) {
            return false;
        }

        public List<string[]> GetAllDependentObjectsInfo (string principalObjUuid) {
            return null;
        }

        #endregion << Object Dependency Methods >>
    }
}
