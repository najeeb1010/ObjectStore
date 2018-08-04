using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using PropertyChanged;
using System.Collections;
using System.Collections.Generic;

namespace X.ObjectStore {
    [Serializable]
    public abstract class ObjectDto : INotifyPropertyChanged {
        #region << Object Construction >>

        public ObjectDto (string append) {
            if (append == null) {
                Uuid = StringUtils.Generate12DigitUniqueID (append: string.Empty, uppercase: true);
            } else {
                Uuid = StringUtils.Generate12DigitUniqueID (append: append, uppercase: true);
            }

            VersionComment = null;
        }

        internal ObjectDto () {
            VersionComment = null;
        }

        #endregion << Object Construction >>

        #region << Info Properties & Methods >>

        [DoNotNotify]
        public string Uuid { get; internal set; }
        //[JsonIgnore] // We are not bothering about it anyway. This value will be written to it from the db value.
        [DoNotNotify]
        public long VersionIndex { get; internal set; }

        // No, we cannot make the set of the following property internal :(
        // Doesn't matter, anyway. Even if the client makes modifications
        // to this property, it will have no effect on the object when it
        // is subsequently saved.
        [DoNotNotify]
        public DateTime WhenAdded { get; set; }

        public string WhoAmI () {
            return GetType () + "/" + Uuid;
        }

        #endregion << Info Properties & Methods >>

        #region << Object Ready Methods >>

        [JsonIgnore]
        [DoNotNotify]
        private bool _objectInReadyState = false;

        public ObjectDto SetObjectAsReady (string comment = null) {
            _objectInReadyState = true;
            bool success = Save (comment ?? LocalConst.AUTO_SAVE_COMMENT_OBJECT_READY);
            if (success) {
                return this;
            }
            return null;
        }

        private bool IsObjectReady () {
            return _objectInReadyState;
        }

        #endregion << Object Ready Methods >>

        #region << Modification Events >>

#pragma warning disable 0168
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0168

        public abstract ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg);

        [JsonIgnore]
        [DoNotNotify]
        private bool _IsDirty = false;
        [JsonIgnore]
        [DoNotNotify]
        private bool _IsInsideJob = false;

        public virtual void OnPropertyModified (string propertyName) {
            if (!IsObjectReady ()) {
                return;
            }
            if (SuspendNotifications) {
                return;
            }

            _IsDirty = _IsInsideJob = true;
            string c = VersionComment ?? string.Format (LocalConst.AUTO_SAVE_COMMENT_PROPERTY_MODF_TEMPLATE, propertyName);
            VersionComment = null;
            Save (c);
        }

        #endregion << Modification Events >>

        #region << Persist Methods >>

        /// <summary>
        /// Always returns an updated object. So clients must ensure that they receive
        /// the return value of this method and use it as the updated object.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private bool Save (string comment = null) {
            bool success = false;

            if (!IsObjectReady ()) {
                return success;
            }

            if (_IsInsideJob && !_IsDirty) {
                return success;
            }

            bool nascent = !ObjectStore.ObjectExists (Uuid);

            #region << Unique Attribute Check >>

            // Check if unique attribute set:
            var props = GetType ().GetProperties ().Where (
                prop => Attribute.IsDefined (prop, typeof (UniqueAttribute)));

            PropertyInfo propertyToBeUniquelyIndexed = null;
            foreach (var p in props) {
                propertyToBeUniquelyIndexed = p;
                // There is only one unique property allowed per class.
                // So we can safely break:
                break;
            }

            object propertyValueToBeIndexed = propertyToBeUniquelyIndexed.GetValue (this);
            if (propertyValueToBeIndexed == null) {
                throw new ArgumentException (
                    string.Format ("Value for unique property `{0}.{1}` not set or null.", GetType ().Name, propertyToBeUniquelyIndexed.Name)
                );
            }

            string uuidOfSameIndex = OdCepManager.Indexing.GetUuidForUniqueValue (GetType (), propertyValueToBeIndexed.ToString ());

            bool objWithSameUniqueValueAlreadyExists = uuidOfSameIndex != null;
            bool currentObjIsSameAsOneWithSameUniqueValue = Uuid.Equals (uuidOfSameIndex);

            if (nascent) {
                // This is a new object.
                if (objWithSameUniqueValueAlreadyExists) {
                    throw new ArgumentException (
                        string.Format ("Duplicate value for unique property `{0}.{1}`: `{2}`",
                        GetType ().Name, propertyToBeUniquelyIndexed.Name, propertyValueToBeIndexed)
                    );
                }
            } else {
                // This is an already existing object.
                if (objWithSameUniqueValueAlreadyExists && !currentObjIsSameAsOneWithSameUniqueValue) {
                    // Currently indexed object is not this object.
                    throw new ArgumentException (
                        string.Format ("Duplicate value for unique property `{0}.{1}`: `{2}`",
                        GetType ().Name, propertyToBeUniquelyIndexed.Name, propertyValueToBeIndexed)
                    );
                }
            }

            #endregion << Unique Attribute Check >>

            if (nascent) {
                success = SaveAsNew (comment ?? string.Empty);
            } else {
                success = SaveAsExisting (comment ?? string.Empty);
            }

            _IsDirty = _IsInsideJob = false;
            return success;
        }

        private bool SaveAsNew (string comment) {
            bool success = ObjectStore.SaveNewObject (this, comment, out string res);
            LastResult = res;
            return success;
        }

        private bool SaveAsExisting (string comment) {
            bool success = ObjectStore.SaveExistingObject (this, comment, out string res);
            LastResult = res;
            return success;
        }

        public void DeleteMe () {
            ObjectStore.DeleteAllObjectReferences (Uuid);
        }

        #endregion << Persist Methods >>

        #region << Dependency Methods >>

        public bool AddPrincipalDependency (ObjectDto principal, string optionalArg = null) {
            if (!ObjectStore.ObjectExists (principal.Uuid) || !ObjectStore.ObjectExists (Uuid)) {
                return false;
            }

            return ObjectDependencyStore.AddObjectDependency (
                principal.Uuid, principal.GetType ().AssemblyQualifiedName,
                Uuid, GetType ().AssemblyQualifiedName, optionalArg
            );
        }

        public bool DeletePrincipalDependency (ObjectDto principal) {
            if (!ObjectStore.ObjectExists (principal.Uuid) || !ObjectStore.ObjectExists (Uuid)) {
                return false;
            }

            return ObjectDependencyStore.RemoveObjectDependency (principal.Uuid, Uuid);
        }

        public bool PrincipalDependencyExists (ObjectDto principal) {
            return ObjectDependencyStore.ObjectDependencyExists (principal.Uuid, Uuid);
        }

        #endregion << Dependency Methods >>

        #region << Atomic Commits >>

        [JsonIgnore]
        internal bool SuspendNotifications = false;

        public delegate bool AtomicModifyMethod<T> (T concreteDto);

        public bool ModifyAtomic<T> (AtomicModifyMethod<T> AtomicMethod, string comment = null) where T: ObjectDto {
            SuspendNotifications = true;
            T concrete = (T) this;
            bool result = AtomicMethod (concrete);
            SuspendNotifications = false;
            VersionComment = comment ?? LocalConst.AUTO_SAVE_COMMENT_ATOMIC_MODIFICATION;
            OnPropertyModified (VersionComment);
            return result;
        }

        public delegate bool AtomicModifyMethodNoParameters ();

        public bool ModifyAtomic<T> (AtomicModifyMethodNoParameters AtomicMethod, string comment = null) where T: ObjectDto {
            SuspendNotifications = true;
            bool result = AtomicMethod ();
            SuspendNotifications = false;
            VersionComment = comment ?? LocalConst.AUTO_SAVE_COMMENT_ATOMIC_MODIFICATION;
            OnPropertyModified (VersionComment);
            return result;
        }

        #endregion << Atomic Commits >>

        #region << Version Comment Property >>

        [JsonIgnore]
        [DoNotNotify]
        public string VersionComment { get; set; }

        #endregion << Version Comment Property >>

        #region << Last Operation Result Method >>

        [JsonIgnore]
        [DoNotNotify]
        public string LastResult { get; private set; }

        #endregion << Last Operation Result Method >>
    }
}
