using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using X.ObjectStore;

namespace ObjectStore {
    [TestFixture]
    public class SimpleSave : TestBase {
        [Test]
        public void SanityCheck () {
            PersonDto p = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;

            // Let's retrieve the object now:
            string retrUuid = OdCepManager.Indexing.GetUuidForUniqueValue (typeof (PersonDto), p.FullName);
            Assert.AreEqual (retrUuid, p.Uuid, "Retrieved object uuid not as expected.");
        }

        [Test]
        public void SanityCheck_WithCollection () {
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

            priceListDto.SetObjectAsReady ();

            // Let's retrieve the object now:
            string retrUuid = OdCepManager.Indexing.GetUuidForUniqueValue (typeof (CatalogDto), priceListDto.CatalogTitle);
            CatalogDto retrObj = (CatalogDto) OdCepManager.Versioning.GetHeadVersion (typeof (CatalogDto), retrUuid, out string comment);

            Assert.AreEqual (numItemsToAdd, retrObj.CatalogItems.Count (), "Does not show same number of objects as saved.");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void UniqueAttributeNotSet () {
            PersonDto p1 = new PersonDto () {
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void UniqueAttributeNullValue () {
            PersonDto p1 = new PersonDto () {
                FullName = null,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void UniqueAttributeReset () {
            PersonDto p1 = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;

            p1.FullName = null;
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void DuplicateValuesForUnique () {
            PersonDto p1 = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;

            new PersonDto () {
                FullName = p1.FullName,
                DoB = DateTime.Now,
            }.SetObjectAsReady ();
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void DuplicateValuesAfterCreation () {
            PersonDto p1 = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;
            Console.WriteLine ("p1: {0}", p1.Uuid);

            PersonDto p2 = new PersonDto () {
                FullName = Rand.AnyName,
                DoB = DateTime.Now,
            }.SetObjectAsReady () as PersonDto;
            Console.WriteLine ("p2: {0}", p2.Uuid);

            p2.FullName = p1.FullName;
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void NonStringUniqueType () {
            StudentDto s1 = new StudentDto () {
                FullName = Rand.AnyName,
                RollNumber = Rand.AnyNumberLargerThanLast,
            }.SetObjectAsReady () as StudentDto;
            Console.WriteLine ("Roll #: " + s1.RollNumber);

            try {
                StudentDto s2 = new StudentDto () {
                    FullName = Rand.AnyName,
                    RollNumber = s1.RollNumber,
                }.SetObjectAsReady () as StudentDto;
            } finally {
                s1.DeleteMe ();
            }
        }

        [Test]
        public void NonStringUniqueType_Retrieve () {
            List<StudentDto> students = new List<StudentDto> ();
            for (int i = 0; i < 10; ++i) {
                StudentDto student = new StudentDto () {
                    FullName = Rand.AnyName,
                    RollNumber = Rand.AnyNumberLargerThanLast,
                }.SetObjectAsReady () as StudentDto;
                students.Add (student);

                string uuid = OdCepManager.Indexing.GetUuidForUniqueValue (typeof (StudentDto), student.RollNumber.ToString ());
                StudentDto s2 = (StudentDto) OdCepManager.Versioning.GetHeadVersion (typeof (StudentDto), uuid);

                Console.WriteLine ("Roll#: {0}", s2.RollNumber);

                Assert.AreEqual (student.FullName, s2.FullName, "Retrieved object properties not as expected.");
                Assert.AreEqual (student.RollNumber, s2.RollNumber, "Retrieved object properties not as expected.");
            }

            foreach (var s in students) {
                s.DeleteMe ();
            }
        }
    }
}
