#define USE_ASYNC_FOR_DISPATCH

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal static class ObjectDependencyStore {
        private static IPersister _persistentObj;

        static ObjectDependencyStore () {
            _persistentObj = PersistenceFactory.GetPersistentObject (ConfigRegistrar.GetConfigMethod () (ConfigConst.PERSISTENCE_DRIVER));
            if (_persistentObj == null) {
                throw new Exception (LocalConst.ERR_NON_EXISTENT_TYPE);
            }
        }

        public static bool AddObjectDependency (string principalObjUuid, string principalType, string dependentObjUuid, string dependentType, string optionalArg) {
            bool success = _persistentObj.AddObjectDependency (principalObjUuid, dependentObjUuid, principalType, dependentType, optionalArg);

            ObjectDto principalObj = OdCepManager.Versioning.GetHeadVersion (Type.GetType (principalType), principalObjUuid, out string comment);
            ObjectDto dependentObj = OdCepManager.Versioning.GetHeadVersion (Type.GetType (dependentType), dependentObjUuid, out comment);

            ObjectDto updatedObj = dependentObj.OnPrincipalObjectUpdated (principalObj, optionalArg);
            updatedObj.VersionIndex++;

            string c = string.Format (LocalConst.AUTO_SAVE_COMMENT_PRINCIPAL_MODF_TEMPLATE, principalObj.WhoAmI ());
            ObjectStore.AutoSaveExistingObject (updatedObj, c);

            return success;
        }

        public static bool RemoveObjectDependency (string principalObjUuid, string dependentObjUuid) {
            return _persistentObj.RemoveObjectDependency (principalObjUuid, dependentObjUuid);
        }

        public static bool ObjectDependencyExists (string principalObjUuid, string dependentObjUuid) {
            return _persistentObj.ObjectDependencyExists (principalObjUuid, dependentObjUuid);
        }

        #region << Dependents Update >>

        internal static void InformAllDependents (string principalObjUuid, string dependentObjUuid) {
            var affectedDependents = GetAllDependents (principalObjUuid);
            InformAffectedObjects (affectedDependents, dependentObjUuid);
        }

        private static List<DependencyInfo> GetAllDependents (string principalObjUuid) {
            List<DependencyInfo> allDependentObjs = new List<DependencyInfo> ();
            foreach (var vals in _persistentObj.GetAllDependentObjectsInfo (principalObjUuid)) {
                DependencyInfo effectInfo = new DependencyInfo () {
                    PrincipalObjectUuid = vals[0],
                    PrincipalObjectType = Type.GetType (vals[1]),
                    DependentObjectUuid = vals[2],
                    DependentObjectType = Type.GetType (vals[3]),
                    OptionalArg = vals[4],
                };
                allDependentObjs.Add (effectInfo);
            }

            return allDependentObjs;
        }

        private static void InformAffectedObjects (List<DependencyInfo> dependenciesInfo, string dependentTypeInfo) {
            if (dependenciesInfo == null || dependenciesInfo.Count == 0) {
                // Nothing to do.
                return;
            }

            foreach (var dependencyInfo in dependenciesInfo) {
                ObjectDto principalObj = OdCepManager.Versioning.GetHeadVersion (dependencyInfo.PrincipalObjectType, dependencyInfo.PrincipalObjectUuid);
                ObjectDto dependentObj = OdCepManager.Versioning.GetHeadVersion (dependencyInfo.DependentObjectType, dependencyInfo.DependentObjectUuid);
                dependentObj.VersionIndex++;

                dependentObj.SuspendNotifications = true;
#if USE_ASYNC_FOR_DISPATCH
                Task<ObjectDto> task = DispatchToObject (dependentObj.OnPrincipalObjectUpdated, principalObj, dependencyInfo.OptionalArg);
                ObjectDto newDependentObj = task.Result;
#else
                ObjectDto newDependentObj = dependentObj.OnPrincipalObjectUpdated (principalObj, effectInfo.OptionalArg);
#endif
                dependentObj.SuspendNotifications = false;
                string c = string.Format (LocalConst.AUTO_SAVE_COMMENT_PRINCIPAL_MODF_TEMPLATE, dependentTypeInfo);
                ObjectStore.AutoSaveExistingObject (newDependentObj, c);
            }

            foreach (var dependencyInfo in dependenciesInfo) {
                ObjectDto objectDto = (ObjectDto) Activator.CreateInstance (dependencyInfo.PrincipalObjectType);
                InformAllDependents (dependencyInfo.DependentObjectUuid, objectDto.WhoAmI ());
            }
        }

#if USE_ASYNC_FOR_DISPATCH
        delegate ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string dependencyType);

        static async Task<ObjectDto> DispatchToObject (OnPrincipalObjectUpdated onPrincipalObjectUpdated, ObjectDto dependentObj, string optionalArg) {
            Task<ObjectDto> task = Task<ObjectDto>.Factory.StartNew (() => {
                return onPrincipalObjectUpdated (dependentObj, optionalArg);
            });

            return await task;
        }
#endif

        #endregion << Dependents Update >>
    }
}
