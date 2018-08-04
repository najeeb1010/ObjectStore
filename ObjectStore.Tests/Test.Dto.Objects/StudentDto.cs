using System;
using System.Linq;
using System.Text;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class StudentDto : ObjectDto {
        public StudentDto () : base ("U") {
        }

        [Unique]
        public int RollNumber { get; set; }
        public string FullName { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Roll number: `{3}` / Full name: `{4}`";
            return string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, RollNumber, FullName);
        }

        public override ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg) {
            // Do whatever you need to do here to manage this change.
            // Do NOT attempt to save this object, else it will result in undefined behaviour; just manage
            // the change in the object and that's it. The library will ensure that the object is auto-saved.
            // Avoid doing any disk/network io here.

            return this;
        }
    }
}
