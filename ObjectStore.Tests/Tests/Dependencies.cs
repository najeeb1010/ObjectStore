using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using X.ObjectStore;

namespace ObjectStore {
    [TestFixture]
    public class Dependencies : TestBase {
        [Test]
        public void AddDependency () {
            // Dependency triggers:
            // Tax => Categories => Products

            // First create a Tax object:
            TaxDto taxDto = new TaxDto () {
                TaxTitle = Rand.AnyTitle,
                Rate = 10,
            }.SetObjectAsReady () as TaxDto;

            // Category next:
            CategoryDto categoryDto = new CategoryDto () {
                CategoryTitle = Rand.AnyTitle,
                CategoryDesc = Rand.AnyDescription,
            }.SetObjectAsReady () as CategoryDto;

            // And also a product:
            ProductDto productDto = new ProductDto () {
                ProductTitle = Rand.AnyTitle,
                BaseCost = 100,
                //TaxComponent = 0, // Don't bother, it will be overwritten
            }.SetObjectAsReady () as ProductDto;

            // Declare dependencies:
            categoryDto.AddPrincipalDependency (taxDto);
            productDto.AddPrincipalDependency (categoryDto);

            // We need to manually update to head version:
            categoryDto = (CategoryDto) OdCepManager.Versioning.GetHeadVersion (typeof (CategoryDto), categoryDto.Uuid);
            productDto = (ProductDto) OdCepManager.Versioning.GetHeadVersion (typeof (ProductDto), productDto.Uuid);

            decimal taxRateInCat = taxDto.Rate;
            Assert.AreEqual (taxRateInCat, categoryDto.TaxRate, "Category tax rate not as expected.");
            decimal expectedTaxAmountInProduct = productDto.BaseCost * taxRateInCat / 100;
            Assert.AreEqual (expectedTaxAmountInProduct, productDto.TaxComponent, "Product tax component not as expected.");
        }

        [Test]
        public void DependencyCascade () {
            // Dependency triggers:
            // Tax => Categories => Products

            // First create a Tax object:
            TaxDto taxDto = new TaxDto () {
                TaxTitle = Rand.AnyTitle,
                Rate = 10,
            }.SetObjectAsReady () as TaxDto;

            // Category next:
            CategoryDto categoryDto = new CategoryDto () {
                CategoryTitle = Rand.AnyTitle,
                CategoryDesc = Rand.AnyDescription,
            }.SetObjectAsReady () as CategoryDto;

            // And also a product:
            ProductDto productDto = new ProductDto () {
                ProductTitle = Rand.AnyTitle,
                BaseCost = 100,
                //TaxComponent = 0, // Don't bother, it will be overwritten
            }.SetObjectAsReady () as ProductDto;

            // Declare dependencies:
            categoryDto.AddPrincipalDependency (taxDto);
            productDto.AddPrincipalDependency (categoryDto);

            taxDto.Rate = 20;

            // We need to manually update to head version:
            categoryDto = (CategoryDto) OdCepManager.Versioning.GetHeadVersion (typeof (CategoryDto), categoryDto.Uuid);
            productDto = (ProductDto) OdCepManager.Versioning.GetHeadVersion (typeof (ProductDto), productDto.Uuid);

            decimal taxRateInCat = taxDto.Rate;
            Assert.AreEqual (taxRateInCat, categoryDto.TaxRate, "Category tax rate not as expected.");
            decimal expectedTaxAmountInProduct = productDto.BaseCost * taxRateInCat / 100;
            Assert.AreEqual (expectedTaxAmountInProduct, productDto.TaxComponent, "Product tax component not as expected.");

            taxDto.Rate = 50;

            categoryDto = (CategoryDto) OdCepManager.Versioning.GetHeadVersion (typeof (CategoryDto), categoryDto.Uuid, out string c);
            productDto = (ProductDto) OdCepManager.Versioning.GetHeadVersion (typeof (ProductDto), productDto.Uuid, out c);

            taxRateInCat = taxDto.Rate;
            Assert.AreEqual (taxRateInCat, categoryDto.TaxRate, "Category tax rate not as expected.");
            expectedTaxAmountInProduct = productDto.BaseCost * taxRateInCat / 100;
            Assert.AreEqual (expectedTaxAmountInProduct, productDto.TaxComponent, "Product tax component not as expected.");
        }
    }
}
