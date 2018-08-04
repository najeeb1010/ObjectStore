using System;
using X.PersistenceBase;

namespace X.ObjectStore {
    internal static class PersistenceFactory {
        public static IPersister GetPersistentObject (string concreteType) {
            if (string.IsNullOrWhiteSpace (concreteType)) {
                throw new ArgumentNullException ("Null or whitespace passed as persistence typename argument.");
            }

            Type ct = Type.GetType (concreteType.Trim ());
            if (ct == null) {
                throw new ArgumentException ("Invalid string passed as persistence typename argument.");
            }

            IPersister obj = null;

            try {
                obj = (IPersister) Activator.CreateInstance (ct);
            } catch (Exception) {
                throw new ArgumentException ("Wrong persistence typename passed as argument.");
            }

            return obj;
        }
    }
}
