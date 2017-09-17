using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 迷宫控件 {
	internal class HashLinkedList<T> : ICollection<T>, IEnumerable<T> {
		private class HashLinkedListNode<E> {
			public E Data;
			public HashLinkedListNode<E> Front;
			public HashLinkedListNode<E> Next;

			public HashLinkedListNode(E data) {
				this.Data = data;
			}

			public void Add(E data) {
				HashLinkedListNode<E> node = this;
				while (node.Next != null) node = node.Next;
				(node.Next = new HashLinkedListNode<E>(data)).Front = node;
			}

			public HashLinkedListNode<E> Find(Predicate<E> func) {
				HashLinkedListNode<E> node = this;
				while (node != null) {
					if (func(node.Data)) return node;
					node = node.Next;
				}
				return null;
			}

			public E FindElement(Predicate<E> func) {
				HashLinkedListNode<E> node = Find(func);
				return node != null ? node.Data : default(E);
			}

			public bool Action(Predicate<E> func, Action<HashLinkedListNode<E>> action) {
				HashLinkedListNode<E> node = Find(func);
				if (node != null) {
					action(node);
					return true;
				} else {
					return false;
				}
			}
		}

		private const int BaseSize = 4096;
		private HashLinkedListNode<HashLinkedListNode<T>>[] hashtable = new HashLinkedListNode<HashLinkedListNode<T>>[BaseSize];
		private HashLinkedListNode<T> header, last;
		private int count = 0;

		private HashLinkedListNode<T> GetHashListNode(T item) {
			if (item != null) {
				uint index = (uint)item.GetHashCode() % BaseSize;
				if (hashtable[index] != null) {
					return hashtable[index].FindElement(n => n.Data.Equals(item));
				}
			}
			return null;
		}

		public IEnumerator<T> GetEnumerator() {
			HashLinkedListNode<T> node = header;
			while (node != null) {
				yield return node.Data;
				node = node.Next;
			}
		}

		public IEnumerable<T> GetEnumerator(T startItem) {
			HashLinkedListNode<T> node = GetHashListNode(startItem) ?? header;
			while (node != null) {
				yield return node.Data;
				node = node.Next;
			}
		}

		public IEnumerable<T> GetEnumeratorOver(T endItem) {
			if (last != null) {
				HashLinkedListNode<T> end = (GetHashListNode(endItem) ?? last).Next;
				HashLinkedListNode<T> node = header;
				while (node != end) {
					yield return node.Data;
					node = node.Next;
				}
			}
		}

		public IEnumerable<T> GetReverseEnumerator() {
			HashLinkedListNode<T> node = last;
			while (node != null) {
				yield return node.Data;
				node = node.Front;
			}
		}

		public IEnumerable<T> GetReverseEnumerator(T startItem) {
			HashLinkedListNode<T> node = GetHashListNode(startItem) ?? last;
			while (node != null) {
				yield return node.Data;
				node = node.Front;
			}
		}

		public IEnumerable<T> GetReverseEnumeratorOver(T endItem) {
			if (header != null) {
				HashLinkedListNode<T> end = (GetHashListNode(endItem) ?? header).Front;
				HashLinkedListNode<T> node = last;
				while (node != null) {
					yield return node.Data;
					node = node.Front;
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		public void Add(T item) {
			HashLinkedListNode<T> newData = null;
			uint index = (uint)item.GetHashCode() % BaseSize;
			if (hashtable[index] == null) {
				hashtable[index] = new HashLinkedListNode<HashLinkedListNode<T>>(newData = new HashLinkedListNode<T>(item));
			} else if (hashtable[index].Find(node => node.Data.Equals(item)) != null) {
				throw new Exception("不能重复添加项");
			} else {
				hashtable[index].Add(newData = new HashLinkedListNode<T>(item));
			}
			if (header == null) {
				header = last = newData;
			} else {
				last.Next = newData;
				newData.Front = last;
				last = newData;
			}
			count++;
		}

		public void Clear() {
			hashtable = new HashLinkedListNode<HashLinkedListNode<T>>[BaseSize];
			header = last = null;
			count = 0;
		}

		public bool Contains(T item) {
			uint index = (uint)item.GetHashCode() % BaseSize;
			return hashtable[index] != null && hashtable[index].Find(node => node.Data.Equals(item)) != null;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			foreach (T item in this) {
				array[arrayIndex++] = item;
			}
		}

		public int Count {
			get { return count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove(T item) {
			uint index = (uint)item.GetHashCode() % BaseSize;
			if (hashtable[index] != null) {
				return hashtable[index].Action(node => node.Data.Equals(item), node => {
					HashLinkedListNode<T> itemNode = node.Data;
					if (itemNode.Front != null) {
						itemNode.Front.Next = itemNode.Next;
					} else {
						header = itemNode.Next;
					}
					if (itemNode.Next != null) {
						itemNode.Next.Front = itemNode.Front;
					} else {
						last = itemNode.Front;
					}
					if (hashtable[index] == node) {
						hashtable[index] = node.Next;
						if (hashtable[index] != null)
							hashtable[index].Front = null;
					} else {
						node.Front.Next = node.Next;
						if (node.Next != null) {
							node.Next.Front = node.Front;
						}
					}
					count--;
				});
			}
			return false;
		}

		public T First {
			get { return header.Data; }
		}

		public T Last {
			get { return last.Data; }
		}

		public T this[int index] {
			get {
				if (index >= 0 && index < count) {
					if (index <= count >> 1) {
						foreach (T data in this) {
							if (index-- <= 0) {
								return data;
							}
						}
					} else {
						index = count - index;
						foreach (T data in this.GetReverseEnumerator()) {
							if (--index <= 0) {
								return data;
							}
						}
					}
				}
				return default(T);
			}
		}
	}
}
