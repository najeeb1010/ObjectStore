using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class TaxDto : ObjectDto {
        public TaxDto () : base ("T") {
        }

        [Unique]
        public string TaxTitle { get; set; }
        public decimal Rate { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / Rate: `{4}`";
            return string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, TaxTitle, Rate);
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
