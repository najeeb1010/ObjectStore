using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using X.ObjectStore;

namespace ObjectStore {
    [TestFixture]
    public class Versioning : TestBase {
        [Test]
        public void SanityCheck_VersionComment () {
            CatalogDto priceListDto = new CatalogDto () {
                CatalogTitle = Rand.AnyTitle,
                CatalogDesc = Rand.AnyDescription,
            };

            int numItemsToAdd = 2;
            for (int i = 0; i < numItemsToAdd; ++i) {
                CatalogEntryDto productConfigDto = new CatalogEntryDto () {
                    EntryTitle = Rand.AnyTitle,
                    DisplayPrice = 100,
                    SalePrice = 10,
                }.SetObjectAsReady () as CatalogEntryDto;

                priceListDto.CatalogItems.Add (productConfigDto);
            }

            // First version should get saved here:
            string comment0 = Rand.AnyComment;
            priceListDto.SetObjectAsReady (comment0);

            var versions = OdCepManager.Versioning.GetAllVersions (typeof (CatalogDto), priceListDto.Uuid);
            Assert.AreEqual (1, versions.Count, "Did not retrieve correct number of versions.");

            var kvPair = versions[0];
            Assert.AreEqual (comment0, kvPair.Value, "Version comment not as expected.");
        }

        [Test]
        public void SanityCheck_VersionValues () {
            List<string> names = new List<string> ();
            List<string> comments = new List<string> ();

            string initComment = Rand.AnyComment;

            PersonDto personDto = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady (initComment) as PersonDto;

            comments.Add (initComment);
            names.Add (personDto.FullName);
            Console.WriteLine ("Added person with name: {0}", personDto.FullName);

            int numChangesAfterReady = 3;
            // Each change will result in a new version:
            for (int i = 0; i < numChangesAfterReady; ++i) {
                string name = Rand.AnyName;
                string comment = Rand.AnyComment;

                Console.WriteLine ("Modifying person name value to: {0}", name);

                personDto.VersionComment = comment;
                personDto.FullName = name;

                names.Add (name);
                comments.Add (comment);
            }

            var versions = OdCepManager.Versioning.GetAllVersions (typeof (PersonDto), personDto.Uuid);
            Assert.AreEqual (numChangesAfterReady + 1, versions.Count, "Returned incorrect number of versions.");

            for (int i = 0; i < numChangesAfterReady + 1; ++i) {
                string name = names[i];
                string comment = comments[i];

                var val = versions[i];
                PersonDto retrVerionObj = (PersonDto) val.Key;
                string retrComment = val.Value;

                Assert.AreEqual (name, retrVerionObj.FullName, "Name property not as expected.");
                Assert.AreEqual (comment, retrComment, "Comment not as expected.");
            }
        }

        [Test]
        public void SanityCheck_NethVersions () {
            CatalogDto priceListDto = new CatalogDto () {
                CatalogTitle = Rand.AnyTitle,
                CatalogDesc = Rand.AnyDescription,
            };

            int numItemsToAddBeforeReady = 2;
            // Since object is not ready yet, this will be the zeroeth version
            // regardless of the number of items that we add.
            for (int i = 0; i < numItemsToAddBeforeReady; ++i) {
                CatalogEntryDto productConfigDto = new CatalogEntryDto () {
                    EntryTitle = Rand.AnyTitle,
                    DisplayPrice = 100,
                    SalePrice = 10,
                }.SetObjectAsReady () as CatalogEntryDto;

                priceListDto.CatalogItems.Add (productConfigDto);
            }

            // First version should get saved here:
            priceListDto.SetObjectAsReady ();

            int numItemsToAddAfterReady = 3;
            // Each item addition will result in a new version:
            for (int i = 0; i < numItemsToAddAfterReady; ++i) {
                CatalogEntryDto productConfigDto = new CatalogEntryDto () {
                    EntryTitle = Rand.AnyTitle,
                    DisplayPrice = 100,
                    SalePrice = 10,
                }.SetObjectAsReady () as CatalogEntryDto;

                priceListDto.CatalogItems.Add (productConfigDto);
            }

            // At this point, there should be numItemsToAddAfterReady + 1 versions of PriceListDto.
            // 0eth version should have numItemsToAddBeforeReady items;
            // 1eth version should have numItemsToAddBeforeReady + 1 items;
            // 2eth version should have numItemsToAddBeforeReady + 2 items;
            // 3eth version should have numItemsToAddBeforeReady + 3 items;
            // Head version should have numItemsToAddBeforeReady + 3 items, same as 3eth version.

            string comment;
            CatalogDto p0 = (CatalogDto) OdCepManager.Versioning.GetNEthVersion (typeof (CatalogDto), priceListDto.Uuid, 0, out comment);
            Assert.AreEqual (numItemsToAddBeforeReady, p0.CatalogItems.Count, "Did not return correct number of items.");
            CatalogDto p1 = (CatalogDto) OdCepManager.Versioning.GetNEthVersion (typeof (CatalogDto), priceListDto.Uuid, 1, out comment);
            Assert.AreEqual (numItemsToAddBeforeReady + 1, p1.CatalogItems.Count, "Did not return correct number of items.");
            CatalogDto p2 = (CatalogDto) OdCepManager.Versioning.GetNEthVersion (typeof (CatalogDto), priceListDto.Uuid, 2, out comment);
            Assert.AreEqual (numItemsToAddBeforeReady + 2, p2.CatalogItems.Count, "Did not return correct number of items.");
            CatalogDto p3 = (CatalogDto) OdCepManager.Versioning.GetNEthVersion (typeof (CatalogDto), priceListDto.Uuid, 3, out comment);
            Assert.AreEqual (numItemsToAddBeforeReady + 3, p3.CatalogItems.Count, "Did not return correct number of items.");
            CatalogDto pH = (CatalogDto) OdCepManager.Versioning.GetHeadVersion (typeof (CatalogDto), priceListDto.Uuid, out comment);
            Assert.AreEqual (numItemsToAddBeforeReady + 3, pH.CatalogItems.Count, "Did not return correct number of items.");
        }

        [Test]
        public void AtomicCommit () {
            List<string> comments = new List<string> ();
            // First version:
            string comment = Rand.AnyComment;
            comments.Add (comment);
            WidgetDto widgetDto = new WidgetDto () {
                WidgetTitle = Rand.AnyTitle,
                ImportantStr = Rand.AnyText,
            }.SetObjectAsReady (comment) as WidgetDto;

            // Second version, using lambda:
            comment = Rand.AnyComment;
            comments.Add (comment);
            bool result = widgetDto.ModifyAtomic<WidgetDto> (() => {
                widgetDto.WidgetTitle = Rand.AnyTitle;
                widgetDto.ImportantStr = Rand.AnyText;
                return true;
            }, comment);
            Assert.IsTrue (result, "Did not commit atomic operation.");

            // Third version, using delegate:
            comment = Rand.AnyComment;
            comments.Add (comment);
            result = widgetDto.ModifyAtomic<WidgetDto> (ModifyAtomically, comment);
            Assert.IsTrue (result, "Did not commit atomic operation.");

            var versions = OdCepManager.Versioning.GetAllVersions (typeof (WidgetDto), widgetDto.Uuid);
            Assert.AreEqual (3, versions.Count, "Returned incorrect number of versions.");

            for (int i = 0; i < 3; ++i) {
                var val = versions[i];
                WidgetDto retrVerionObj = (WidgetDto) val.Key;
                string retrComment = val.Value;

                Assert.AreEqual (comments[i], retrComment, "Comment not as expected.");
            }
        }

        private bool ModifyAtomically (WidgetDto widgetDto) {
            widgetDto.WidgetTitle = Rand.AnyTitle;
            widgetDto.ImportantStr = Rand.AnyText;

            return true;
        }
    }
}
