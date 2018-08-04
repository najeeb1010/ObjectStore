using System;
using System.Linq;
using System.Text;
using X.ObjectStore;

namespace ObjectStore {
    [Serializable]
    public class CatalogEntryDto : ObjectDto {
        public CatalogEntryDto () : base ("E") {
            ProductCombo = new NotifierList<ProductDto> (this);
        }

        [Unique]
        public string EntryTitle { get; set; }
        public decimal DisplayPrice { get; set; }
        public decimal SalePrice { get; set; }
        public NotifierList<ProductDto> ProductCombo { get; set; }

        public override string ToString () {
            string ftr = "Obj: `{0}` / Version#: `{1}` / WhenAdded: `{2}` / Title: `{3}` / Display Price: `{4}` / Sale Price: `{5}`";
            return string.Format (ftr, WhoAmI (), VersionIndex, WhenAdded, EntryTitle, DisplayPrice, SalePrice);
        }

        public override ObjectDto OnPrincipalObjectUpdated (ObjectDto updatedPrincipalObj, string optionalArg) {
            // Do whatever you need to do here to manage this change.
            // Do NOT attempt to save this object, else it will result in undefined behaviour; just manage
            // the change in the object and that's it. The library will ensure that the object is auto-saved.
            // Avoid doing any disk/network io here.

            if (updatedPrincipalObj is ProductDto) {
                ProductDto productDto = updatedPrincipalObj as ProductDto;
                ProductCombo.Remove (productDto, quietly: true);
                ProductCombo.Add (productDto, quietly: true);

                DisplayPrice = SalePrice = 0;
                foreach (var product in ProductCombo) {
                    decimal productVal = product.BaseCost + product.BaseCost * product.TaxComponent / 100;
                    decimal markedup = productVal + (productVal / 100 * 10); // Add a 10% markup
                    decimal discounted = markedup - (productVal / 100 * 5); // subtract a 5% discount
                    DisplayPrice += markedup;
                    SalePrice += discounted;
                }
            }

            return this;
        }
    }
}
