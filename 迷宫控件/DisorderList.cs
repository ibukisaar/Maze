using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 迷宫控件 {
	internal class DisorderList<T> : IList<T> {
		private int count = 0;
		private T[] data;

		public DisorderList() : this(4) { }
		public DisorderList(int size) {
			data = new T[size];
		}

		public int IndexOf(T item) {
			for (int i = 0; i < count; i++) {
				if (data[i].Equals(item)) {
					return i;
				}
			}
			return -1;
		}

		public void Insert(int index, T item) {
			Resize();
			data[count++] = data[index];
			data[index] = item;
		}

		public void RemoveAt(int index) {
			data[index] = data[--count];
		}

		public T this[int index] {
			get {
				return data[index];
			}
			set {
				data[index] = value;
			}
		}

		public void Add(T item) {
			Resize();
			data[count++] = item;
		}

		public void Clear() {
			count = 0;
		}

		public bool Contains(T item) {
			return IndexOf(item) != -1;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			data.CopyTo(array, arrayIndex);
		}

		public int Count {
			get { return count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove(T item) {
			int index = IndexOf(item);
			if (index >= 0 && index < count) {
				RemoveAt(index);
				return true;
			} else {
				return false;
			}
		}

		public IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < count; i++) {
				yield return data[i];
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		private void Resize() {
			if (data.Length <= count) {
				Array.Resize<T>(ref data, count * 2 + 1);
			}
		}
	}
}
