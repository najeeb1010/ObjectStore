using System;

namespace X.ObjectStore {
    internal class DependencyInfo {
        public DependencyInfo () {
        }

        public string PrincipalObjectUuid { get; set; }
        public Type PrincipalObjectType { get; set; }
        public string DependentObjectUuid { get; set; }
        public Type DependentObjectType { get; set; }
        public string OptionalArg { get; set; }
    }
}
