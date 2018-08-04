using System;
using System.Collections.Generic;

namespace X.ObjectStore {
    /// <summary>
    /// Object Dependency & Cascading Event Propagation Manager
    /// </summary>
    public static class OdCepManager {
        #region << Object Store Functions >>

        public static class Versioning {
            public static ObjectDto GetHeadVersion (Type objType, string objUuid) {
                // TODO: Add cache check here

                if (!ObjectStore.ObjectExists (objUuid)) {
                    return null;
                }

                return ObjectStore.RetrieveObjectHeadVersion (objType, objUuid);
            }

            public static ObjectDto GetHeadVersion (Type objType, string objUuid, out string comment) {
                // TODO: Add cache check here

                if (!ObjectStore.ObjectExists (objUuid)) {
                    comment = null;
                    return null;
                }

                return ObjectStore.RetrieveObjectHeadVersion (objType, objUuid, out comment);
            }

            public static ObjectDto GetNEthVersion (Type objType, string objUuid, long versionNumber) {
                // TODO: Add cache check here

                if (!ObjectStore.ObjectExists (objUuid)) {
                    return null;
                }

                return ObjectStore.RetrieveObjectNEthVersion (objType, objUuid, versionNumber);
            }

            public static ObjectDto GetNEthVersion (Type objType, string objUuid, long versionNumber, out string comment) {
                // TODO: Add cache check here

                if (!ObjectStore.ObjectExists (objUuid)) {
                    comment = null;
                    return null;
                }

                return ObjectStore.RetrieveObjectNEthVersion (objType, objUuid, versionNumber, out comment);
            }

            public static List<KeyValuePair<ObjectDto, string>> GetAllVersions (Type objType, string objUuid) {
                // TODO: Add cache check here

                if (!ObjectStore.ObjectExists (objUuid)) {
                    return null;
                }

                return ObjectStore.RetrieveAllObjectVersions (objType, objUuid);
            }
        }

        #endregion << Object Store Functions >>

        #region << Object Index Functions >>

        public static class Indexing {
            public static string GetUuidForUniqueValue (Type objType, string uniqueValue) {
                // TODO: Add cache check here

                return ObjectIndexer.GetObjectUuidForUniqueValue (objType, uniqueValue);
            }
        }

        #endregion << Object Index Functions >>
    }
}
