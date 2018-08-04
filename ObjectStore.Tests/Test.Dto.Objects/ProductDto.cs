using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class ProductDto : ObjectDto {
        public ProductDto () : base ("P") {
        }

        [Unique]
        public string ProductTitle { get; set; }
        public decimal BaseCost { get; set; }
        public decimal TaxComponent { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / Base cost: `{4}` / Tax: `{5}`";
            return string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, ProductTitle, BaseCost, TaxComponent);
        }

        public override ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg) {
            // Do whatever you need to do here to manage this change.
            // Do NOT attempt to save this object, else it will result in undefined behaviour; just manage
            // the change in the object and that's it. The library will ensure that the object is auto-saved.
            // Avoid doing any disk/network io here.

            if (updatedPrincipalObj is CategoryDto) {
                CategoryDto categoryDto = updatedPrincipalObj as CategoryDto;
                TaxComponent = BaseCost * categoryDto.TaxRate / 100;
            }

            return this;
        }
    }
}
