using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal static class ObjectStore {
        private static IPersister _persistentObj;

        static ObjectStore () {
            _persistentObj = PersistenceFactory.GetPersistentObject (ConfigRegistrar.GetConfigMethod () (ConfigConst.PERSISTENCE_DRIVER));
            if (_persistentObj == null) {
                throw new Exception (LocalConst.ERR_NON_EXISTENT_TYPE);
            }
        }

        public static ObjectDto RetrieveObjectHeadVersion (Type actualType, string objUuid) {
            string json = _persistentObj.RetrieveObjectJsonHeadVersion (objUuid);
            var obj = Jsonizer.Deserialize (json, actualType);
            // This is a hack! If we don't do this, we will have to make Uuid property public,
            // or accessible to outsiders, in which case it is open to modification. Therefore
            // we have made this property internal. Which also means that the Newton Json library
            // will also *not* be able to access it. Thusly, this hack. It hurts, but what the heck!
            obj.Uuid = objUuid;
            obj.VersionIndex = _persistentObj.GetObjectLastVersionNumber (objUuid);
            return obj;
        }

        public static ObjectDto RetrieveObjectHeadVersion (Type actualType, string objUuid, out string comment) {
            string json = _persistentObj.RetrieveObjectJsonHeadVersion (objUuid, out comment);
            var obj = Jsonizer.Deserialize (json, actualType);
            // This is a hack! If we don't do this, we will have to make Uuid property public,
            // or accessible to outsiders, in which case it is open to modification. Therefore
            // we have made this property internal. Which also means that the Newton Json library
            // will also *not* be able to access it. Thusly, this hack. It hurts, but what the heck!
            obj.Uuid = objUuid;
            obj.VersionIndex = _persistentObj.GetObjectLastVersionNumber (objUuid);
            return obj;
        }

        public static ObjectDto RetrieveObjectNEthVersion (Type actualType, string objUuid, long versionNumber) {
            string json = _persistentObj.RetrieveObjectJsonNEthVersion (objUuid, versionNumber);
            if (json == null) {
                return null;
            }

            var obj = Jsonizer.Deserialize (json, actualType);
            // This is a hack! If we don't do this, we will have to make Uuid property public,
            // or accessible to outsiders, in which case it is open to modification. Therefore
            // we have made this property internal. Which also means that the Newton Json library
            // will also *not* be able to access it. Thusly, this hack. It hurts, but what the heck!
            obj.Uuid = objUuid;
            obj.VersionIndex = versionNumber;
            return obj;
        }

        public static ObjectDto RetrieveObjectNEthVersion (Type actualType, string objUuid, long versionNumber, out string comment) {
            string json = _persistentObj.RetrieveObjectJsonNEthVersion (objUuid, versionNumber, out comment);
            if (json == null) {
                return null;
            }

            var obj = Jsonizer.Deserialize (json, actualType);
            // This is a hack! If we don't do this, we will have to make Uuid property public,
            // or accessible to outsiders, in which case it is open to modification. Therefore
            // we have made this property internal. Which also means that the Newton Json library
            // will also *not* be able to access it. Thusly, this hack. It hurts, but what the heck!
            obj.Uuid = objUuid;
            obj.VersionIndex = versionNumber;
            return obj;
        }

        public static List<KeyValuePair<ObjectDto, string>> RetrieveAllObjectVersions (Type actualType, string objUuid) {
            List<KeyValuePair<ObjectDto, string>> allObjs = new List<KeyValuePair<ObjectDto, string>> ();

            long versionNumber = 0;
            foreach (var pair in _persistentObj.RetrieveObjectJsonAllVersions (objUuid)) {
                string json = pair.Key;
                string comment = pair.Value;
                var obj = Jsonizer.Deserialize (json, actualType);
                // This is a hack! If we don't do this, we will have to make Uuid property public,
                // or accessible to outsiders, in which case it is open to modification. Therefore
                // we have made this property internal. Which also means that the Newton Json library
                // will also *not* be able to access it. Thusly, this hack. It hurts, but what the heck!
                obj.Uuid = objUuid;
                obj.VersionIndex = versionNumber++;
                KeyValuePair<ObjectDto, string> p = new KeyValuePair<ObjectDto, string> (obj, comment);
                allObjs.Add (p);
            }

            return allObjs;
        }

        public static bool ObjectExists (string objUuid) {
            return _persistentObj.ObjectExists (objUuid);
        }

        public static void DeleteAllObjectReferences (string objUuid) {
            _persistentObj.DeleteAllObjectReferences (objUuid);
        }

        public static bool SaveNewObject (ObjectDto persistObj, string comment, out string result) {
            string objUuid = persistObj.Uuid;

            if (_persistentObj.ObjectExists (objUuid)) {
                result = LocalConst.ERR_EXISTENT_OBJ;
                return false;
            }

            if (ObjectIndexer.UniquePropertyValueExists (persistObj)) {
                result = LocalConst.ERR_DUPLICATE_VALUE;
                return false;
            }

            persistObj.WhenAdded = DateTime.Now;
            string json = Jsonizer.Serialize (persistObj);

            // First time this object is being persisted.
            _persistentObj.AddObjectFirstRecord (objUuid);

            // Version numbers are *always* zero-indexed!
            long versionNumber = 0;
            _persistentObj.PersistObjectAsVersion (objUuid, json, versionNumber, comment ?? string.Empty);

            // Closing tasks:
            ObjectIndexer.IndexObject (persistObj);

            result = LocalConst.ERR_SUCCESS;
            return true;
        }

        public static bool SaveExistingObject (ObjectDto persistObj, string comment, out string result) {
            string objUuid = persistObj.Uuid;

            if (!_persistentObj.ObjectExists (objUuid)) {
                result = LocalConst.ERR_NON_EXISTENT_OBJ;
                return false;
            }

            // Here we need to check the current unique index value for this object and compare it to the
            // passed, modified object. There can be any one of two possible outcomes:
            // a. Both values are the same; or
            // b. Both values are different.
            // Case (b) first. If it's case (b), we will need to check if it isn't clashing with any other
            // object's index value. If it is, we fail and inform the caller as much, else we go ahead.
            // In case (a), we will again need to figure out one of the two sub-possibilities:
            // Both values are the same because:
            // i. Another property of the object is being changed; or
            // ii. The object is being passed as is.
            // In case (i), we will check the JSON of the existing object to figure out if the two objects
            // are the same or different. If they are different, we save, else we merely return a true,
            // without adding a record.
            // In the latter case (ii), we save because indeed some value seems to have been changed.

            Type objectType = persistObj.GetType ();

            string currentJson = Jsonizer.Serialize (persistObj);

            string existingJson = _persistentObj.RetrieveObjectJsonHeadVersion (persistObj.Uuid, out string existingComment);
            ObjectDto existingPersistObject = Jsonizer.Deserialize (existingJson, objectType);
            existingPersistObject.VersionIndex++;

            // At this point, we have the existing object.

            var props = objectType.GetProperties ().Where (
                prop => Attribute.IsDefined (prop, typeof (UniqueAttribute)));
            PropertyInfo propertyToBeIndexed = null;
            foreach (var p in props) {
                propertyToBeIndexed = p;
                // There is only one unique property allowed per class.
                // So we can safely break:
                break;
            }
            string propertyValueToBeIndexed = (string) propertyToBeIndexed.GetValue (persistObj);
            string propertyValueCurrentlyIndexed = (string) propertyToBeIndexed.GetValue (existingPersistObject);

            // At this point, we have the two values: the unique index value for this object that needs to be indexed,
            // and the same property value that is currently indexed.

            // Question: Are both values the same?
            if (propertyValueToBeIndexed.Equals (propertyValueCurrentlyIndexed)) {
                // Answer: Both are the same, so we check for sub-possibilities (i) and (ii):
                if (JsonUtil.AreObjectsIdentical (currentJson, existingJson)) {
                    // No change in the object, so we return true:
                    result = LocalConst.INFO_NO_MODF;
                    return true;
                } else {
                    // Another property within the object has been changed, so we go ahead.
                    // Do nothing here.
                }
            } else {
                // Answer: Both values are *not* the same. So we do a regular check:
                if (ObjectIndexer.UniquePropertyValueExists (persistObj)) {
                    result = LocalConst.ERR_DUPLICATE_VALUE;
                    return false;
                } else {
                    // No clash of unique index value with that of any other object.
                    // Do nothing here.
                }
            }

            // Version numbers are *always* zero-indexed!
            long versionNumber = _persistentObj.GetObjectLastVersionNumber (objUuid) + 1;

            persistObj.WhenAdded = DateTime.Now;
            string json = Jsonizer.Serialize (persistObj);

            // Save the versioned object:
            _persistentObj.PersistObjectAsVersion (objUuid, json, versionNumber, comment);

            // Closing tasks:
            ObjectIndexer.IndexObject (persistObj);
            ObjectDependencyStore.InformAllDependents (objUuid, persistObj.WhoAmI ());

            result = LocalConst.ERR_SUCCESS;
            return true;
        }

        internal static bool AutoSaveExistingObject (ObjectDto persistObj, string comment) {
            string objUuid = persistObj.Uuid;

            // We can skip this check: if the object did not already exist, we wouldn't have been here, anyway.
            /*if (!_persistentObj.ObjectExists (objUuid)) {
                return false;
            }*/

            // We will save the object afresh for maintaining the object's history, even if it remains unmodified,
            // especially when it is getting auto-modified and auto-saved.

            Type objectType = persistObj.GetType ();
            string currentJson = Jsonizer.Serialize (persistObj);
            string existingJson = _persistentObj.RetrieveObjectJsonHeadVersion (persistObj.Uuid, out string existingComment);
            ObjectDto existingPersistObject = Jsonizer.Deserialize (existingJson, objectType);
            existingPersistObject.VersionIndex++;

            // At this point, we have the existing object.

            var props = objectType.GetProperties ().Where (
                prop => Attribute.IsDefined (prop, typeof (UniqueAttribute)));
            PropertyInfo propertyToBeIndexed = null;
            foreach (var p in props) {
                propertyToBeIndexed = p;
                // There is only one unique property allowed per class.
                // So we can safely break:
                break;
            }
            string propertyValueToBeIndexed = (string) propertyToBeIndexed.GetValue (persistObj);
            string propertyValueCurrentlyIndexed = (string) propertyToBeIndexed.GetValue (existingPersistObject);

            // At this point, we have the two values: the unique index value for this object that needs to be indexed,
            // and the same property value that is currently indexed.

            // Question: Are both values are the same?
            if (propertyValueToBeIndexed.Equals (propertyValueCurrentlyIndexed)) {
                // Answer: Both are the same, so we check for sub-possibilities (i) and (ii):
                if (JsonUtil.AreObjectsIdentical (currentJson, existingJson)) {
                    // We *do* save the new object even if it is identical to the last one (for the reason cited above),
                    //return true;
                } else {
                    // Another property within the object has been changed, so we go ahead.
                    // Do nothing here.
                }
            } else {
                // Answer: Both values are not the same. So we do a regular check:
                if (ObjectIndexer.UniquePropertyValueExists (persistObj)) {
                    return false;
                } else {
                    // No clash of unique index value with that of any other object.
                    // Do nothing here.
                }
            }

            // Version numbers are *always* zero-indexed!
            long versionNumber = _persistentObj.GetObjectLastVersionNumber (objUuid) + 1;

            persistObj.WhenAdded = DateTime.Now;
            string json = Jsonizer.Serialize (persistObj);

            // Save the versioned object:
            _persistentObj.PersistObjectAsVersion (objUuid, json, versionNumber, comment);

            // Closing tasks:
            ObjectIndexer.IndexObject (persistObj);

            return true;
        }
    }
}
