using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class CategoryDto : ObjectDto {
        public CategoryDto () : base ("C") {
        }

        [Unique]
        public string CategoryTitle { get; set; }
        public string CategoryDesc { get; set; }
        public decimal TaxRate { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / Desc: `{4}` / Tax rate: `{5}`";
            return string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, CategoryTitle, CategoryDesc, TaxRate);
        }

        public override ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg) {
            // Do whatever you need to do here to manage this change.
            // Do NOT attempt to save this object, else it will result in undefined behaviour; just manage
            // the change in the object and that's it. The library will ensure that the object is auto-saved.
            // Avoid doing any disk/network io here.

            if (updatedPrincipalObj is TaxDto) {
                TaxDto taxDto = updatedPrincipalObj as TaxDto;
                TaxRate = taxDto.Rate;
            }

            return this;
        }
    }
}
