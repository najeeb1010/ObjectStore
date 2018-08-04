using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace X.ObjectStore {
    [Serializable]
    public class NotifierList<T> : List<T> {
        private ObjectDto _dtoObject;

        public NotifierList (ObjectDto dtoObject) : base () {
            _dtoObject = dtoObject;
        }

        public NotifierList (ObjectDto dtoObject, int capacity) : base (capacity) {
            _dtoObject = dtoObject;
        }

        public NotifierList (ObjectDto dtoObject, IEnumerable<T> items) : base (items) {
            _dtoObject = dtoObject;
        }

        public void Add (T item, bool quietly = false) {
            base.Add (item);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }

        public void Clear (bool quietly = false) {
            base.Clear ();
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }

        public void AddRange (IEnumerable<T> items, bool quietly = false) {
            base.AddRange (items);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }

        public void InsertRange (int index, IEnumerable<T> items, bool quietly = false) {
            base.InsertRange (index, items);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }

        public bool Remove (T item, bool quietly = false) {
            bool success = base.Remove (item);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
            return success;
        }

        public int RemoveAll (Predicate<T> match, bool quietly = false) {
            int num = base.RemoveAll (match);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
            return num;
        }

        public void RemoveAt (int index, bool quietly = false) {
            base.RemoveAt (index);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }

        public void RemoveRange (int index, int count, bool quietly = false) {
            base.RemoveRange (index, count);
            if (!quietly) {
                _dtoObject.OnPropertyModified (MethodBase.GetCurrentMethod ().Name);
            }
        }
    }
}
