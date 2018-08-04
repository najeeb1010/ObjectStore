using System;
using System.Linq;
using System.Text;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class CatalogDto : ObjectDto {
        public CatalogDto () : base ("L") {
            CatalogItems = new NotifierList<CatalogEntryDto> (this);
        }

        [Unique]
        public string CatalogTitle { get; set; }
        public string CatalogDesc { get; set; }
        public NotifierList<CatalogEntryDto> CatalogItems { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / Desc: `{4}` with the following items:\n";
            StringBuilder opStr = new StringBuilder ();
            opStr.Append (string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, CatalogTitle, CatalogDesc));

            int ctr = 0;
            foreach (var item in CatalogItems) {
                opStr.Append ("\t" + ++ctr + ". -> " + item.ToString ());
                opStr.AppendLine ();
            }

            return opStr.ToString ();
        }

        public override ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg) {
            // Do whatever you need to do here to manage this change.
            // Do NOT attempt to save this object, else it will result in undefined behaviour; just manage
            // the change in the object and that's it. The library will ensure that the object is auto-saved.
            // Avoid doing any disk/network io here.

            if (updatedPrincipalObj is CatalogEntryDto) {
                CatalogEntryDto catalogEntryDto = updatedPrincipalObj as CatalogEntryDto;
                CatalogItems.Remove (catalogEntryDto, quietly: true);
                CatalogItems.Add (catalogEntryDto, quietly: true);
            }

            return this;
        }
    }
}
