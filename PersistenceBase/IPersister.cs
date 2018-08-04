using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X.PersistenceBase {
    public interface IPersister {
        #region << Object Store Methods >>

        long AddObjectFirstRecord (string objUuid);
        bool ObjectExists (string objUuid);
        long PersistObjectAsVersion (string objUuid, string objJson, long versionNumber, string comment);
        string RetrieveObjectJsonHeadVersion (string objUuid);
        string RetrieveObjectJsonHeadVersion (string objUuid, out string comment);
        string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber);
        string RetrieveObjectJsonNEthVersion (string objUuid, long versionNumber, out string comment);
        List<KeyValuePair<string, string>> RetrieveObjectJsonAllVersions (string objUuid);
        long GetObjectLastVersionNumber (string objUuid);
        void DeleteAllObjectReferences (string objUuid);

        #endregion << Object Store Methods >>

        #region << Object Index Methods >>

        void IndexObject (string objectTypeFullName, string objUuid, string indexValue);
        bool UniquePropertyValueExists (string objTypeFullName, string propertyValue);
        string GetObjectUuidForUniqueValue (string objectTypeFullName, string uniqueValue);
        void DeletePreviousIndex (string objUuid);

        #endregion << Object Index Methods >>

        #region << Object Dependency Methods >>

        bool AddObjectDependency (string principalObjUuid, string dependentObjUuid, string principalType, string dependentType, string optionalArg = null);
        bool RemoveObjectDependency (string principalObjUuid, string dependentObjUuid);
        bool ObjectDependencyExists (string principalObjUuid, string dependentObjUuid);
        List<string[]> GetAllDependentObjectsInfo (string principalObjUuid);

        #endregion << Object Dependency Methods >>
    }
}
