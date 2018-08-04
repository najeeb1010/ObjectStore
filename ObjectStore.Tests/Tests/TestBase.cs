using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using X.ObjectStore;

namespace ObjectStore {
    public class TestBase {
        [TestFixtureSetUp]
        protected void TestFixtureSetUp () {
            ConfigRegistrar.RegisterConfigMethod (AppConfigSimulator);
        }

        [TestFixtureTearDown]
        protected void TeaTestFixtureSetUpDown () {
        }

        public string AppConfigSimulator (string key) {
            switch (key) {
                case "PersistenceDriver":
                    return "X.ObjectStore.MySqlPersistencyObject";

                case "Host":
                    return "127.0.0.1";

                case "PortNo":
                    return "3306";

                case "DbName":
                    return "ObjectStoreDb";

                case "Username":
                    return "root";

                case "Password":
                    return "helloworld";

                case "Extra1":
                    return "";

                case "Extra2":
                    return "";
            }

            return null;
        }
    }
}
