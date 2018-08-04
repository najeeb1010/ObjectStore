namespace X.ObjectStore {
    public delegate string ConfigMethod (string key);

    public static class ConfigRegistrar {
        private static ConfigMethod _configMethod;

        public static void RegisterConfigMethod (ConfigMethod method) {
            _configMethod = method;
        }

        internal static ConfigMethod GetConfigMethod () {
            return _configMethod;
        }
    }
}
