using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class WidgetDto : ObjectDto {
        public WidgetDto () : base ("W") {
            Items = new NotifierList<string> (this);
        }

        [Unique]
        public string WidgetTitle { get; set; }
        public string ImportantStr { get; set; }
        public NotifierList<string> Items { get; set; }

        public override string ToString () {
            StringBuilder ftr = new StringBuilder ();

            ftr.Append (string.Format (
                "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / ImportantStr: `{4}`. It has the following items:\n",
                WhoAmI (), VersionIndex, WhenAdded, WidgetTitle, ImportantStr
            ));

            if (Items.Count > 0) {
                int ctr = 0;
                foreach (var itm in Items) {
                    ftr.Append (string.Format (ftr + "{0} -> {1}\n", ++ctr, itm));
                }
            } else {
                ftr.Append ("None");
            }
            ftr.AppendLine ();

            return ftr.ToString ();
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
