using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal static class ObjectIndexer {
        private static IPersister _persistentObj;

        static ObjectIndexer () {
            _persistentObj = PersistenceFactory.GetPersistentObject (ConfigRegistrar.GetConfigMethod () (ConfigConst.PERSISTENCE_DRIVER));
            if (_persistentObj == null) {
                throw new Exception (LocalConst.ERR_NON_EXISTENT_TYPE);
            }
        }

        public static void IndexObject (ObjectDto persistObj) {
            var t = persistObj.GetType ();

            var props = t.GetProperties ().Where (
                prop => Attribute.IsDefined (prop, typeof (UniqueAttribute)));

            PropertyInfo propertyToBeUniquelyIndexed = null;
            foreach (var p in props) {
                propertyToBeUniquelyIndexed = p;
                // There is only one unique property allowed per class.
                // So we can safely break:
                break;
            }

            object propertyValueToBeIndexed = propertyToBeUniquelyIndexed.GetValue (persistObj);

            _persistentObj.DeletePreviousIndex (persistObj.Uuid);
            _persistentObj.IndexObject (persistObj.GetType ().AssemblyQualifiedName, persistObj.Uuid, propertyValueToBeIndexed.ToString ());
        }

        public static string GetObjectUuidForUniqueValue (Type objectType, string uniqueValue) {
            return _persistentObj.GetObjectUuidForUniqueValue (objectType.AssemblyQualifiedName, uniqueValue);
        }

        public static bool UniquePropertyValueExists (ObjectDto persistObj) {
            Type objectType = persistObj.GetType ();

            var props = objectType.GetProperties ().Where (
                prop => Attribute.IsDefined (prop, typeof (UniqueAttribute)));
            PropertyInfo propertyToBeIndexed = null;
            foreach (var p in props) {
                propertyToBeIndexed = p;
                // There is only one unique property allowed per class.
                // So we can safely break:
                break;
            }

            if (propertyToBeIndexed == null) {
                return false;
            }

            object propertyValueToBeIndexed = propertyToBeIndexed.GetValue (persistObj);

            string val = null;
            if (propertyValueToBeIndexed.GetType () == typeof (ObjectDto)) {
                val = (propertyValueToBeIndexed as ObjectDto).WhoAmI ();
            } else {
                val = ((object) propertyValueToBeIndexed).ToString ();
            }

            return _persistentObj.UniquePropertyValueExists (objectType.AssemblyQualifiedName, val);
        }
    }
}
