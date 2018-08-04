namespace X.ObjectStore {
    internal static class JsonUtil {
        public static bool AreObjectsIdentical (string left, string right) {
            // TODO: A better method for comparing Json objects.
            return left.Equals (right);
        }

        public static string DoObjectDiff (string left, string right) {
            // TODO: A better method for diffing Json objects.
            string patch = right;
            return patch;
        }

        public static string DoObjectMerge (string left, string patch) {
            // TODO: A better method for patching Json objects.
            return patch;
        }
    }
}
