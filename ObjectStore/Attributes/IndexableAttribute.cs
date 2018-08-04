using System;

namespace X.ObjectStore {
    [AttributeUsage (AttributeTargets.Property, Inherited = true)]
    public class IndexableAttribute : Attribute {
    }
}
