using System.Collections.Generic;

namespace 迷宫控件 {
	public class MazeNode : IEnumerable<MazeNode> {
		public enum MazeNodeType {
			空白, 障碍
		}
		public enum Direction {
			None = 0, Left = 1, Up = 2, Right = 4, Down = 8
		}

		public MazeNode(int x, int y) : this(x, y, MazeNodeType.空白) { }

		public MazeNode(int x, int y, MazeNodeType type) {
			this.x = x;
			this.y = y;
			this.type = type;
		}

		private int x, y;
		private MazeNode parent;
		private int depth = 0;
		//private bool up, down, left, right;
		private List<MazeNode> childs = new List<MazeNode>(4);
		private MazeNodeType type;
		private Direction direction = Direction.None;

		public int X { get { return x; } }
		public int Y { get { return y; } }
		public MazeNode Parent { get { return parent; } }
		public int Depth { get { return depth; } }
		public bool Up { get { return this.direction.HasFlag(Direction.Up); } }
		public bool Down { get { return this.direction.HasFlag(Direction.Down); } }
		public bool Left { get { return this.direction.HasFlag(Direction.Left); } }
		public bool Right { get { return this.direction.HasFlag(Direction.Right); } }
		public List<MazeNode> Childs { get { return childs; } }
		public MazeNodeType Type { get { return type; } set { type = value; } }
		public Direction DirectionFlags { get { return direction; } }

		public void AddChild(MazeNode node) {
			childs.Add(node);
			node.parent = this;

			if (node.x == -1 || node.y == -1) return;

			if (this.x == node.x) {
				if (this.y + 1 == node.y) {
					//this.down = node.up = true;
					this.direction |= Direction.Down;
					node.direction |= Direction.Up;
				} else {
					//this.up = node.down = true;
					this.direction |= Direction.Up;
					node.direction |= Direction.Down;
				}
			} else {
				if (this.x + 1 == node.x) {
					//this.right = node.left = true;
					this.direction |= Direction.Right;
					node.direction |= Direction.Left;
				} else {
					//this.left = node.right = true;
					this.direction |= Direction.Left;
					node.direction |= Direction.Right;
				}
			}

			node.depth = this.depth + 1;
		}

		public void RemoveChild(MazeNode node) {
			if (childs.Remove(node)) {
				node.parent = null;
				if (node.x == -1 || node.y == -1) return;
				if (this.x == node.x) {
					if (this.y + 1 == node.y) {
						//this.down = node.up = false;
						this.direction &= ~Direction.Down;
						node.direction &= ~Direction.Up;
					} else {
						//this.up = node.down = false;
						this.direction &= ~Direction.Up;
						node.direction &= ~Direction.Down;
					}
				} else {
					if (this.x + 1 == node.x) {
						//this.right = node.left = false;
						this.direction &= ~Direction.Right;
						node.direction &= ~Direction.Left;
					} else {
						//this.left = node.right = false;
						this.direction &= ~Direction.Left;
						node.direction &= ~Direction.Right;
					}
				}
			}
		}

		public void Reset() {
			this.parent = null;
			this.childs.Clear();
			this.direction = Direction.None;
			this.depth = 0;
		}

		private delegate void NextPathDelegate(MazeNode node, ref int x, ref int y, ref Direction from);
		private static readonly NextPathDelegate[] NextPathActions = {
			(MazeNode node, ref int x, ref int y, ref Direction from) => { x = node.x - 1; y = node.y; from = Direction.Right; }, // left
			(MazeNode node, ref int x, ref int y, ref Direction from) => { x = node.x; y = node.y - 1; from = Direction.Down; }, // up
			(MazeNode node, ref int x, ref int y, ref Direction from) => { x = node.x + 1; y = node.y; from = Direction.Left; }, // right
			(MazeNode node, ref int x, ref int y, ref Direction from) => { x = node.x; y = node.y + 1; from = Direction.Up; }, // down
		};

		public bool OnlyOnePath(ref int x, ref int y, ref Direction from) {
			int temp = (int)(this.direction ^ from);
			if (temp != 0 && ((temp - 1) & temp) == 0) {
				NextPathActions[(temp >> 1) - (temp >> 3)](this, ref x, ref y, ref from);
				return true;
			} else {
				return false;
			}
		}

		public bool IsOnlyOnePath(Direction from) {
			int temp = (int)(this.direction ^ from);
			return temp != 0 && ((temp - 1) & temp) == 0;
		}

		public Direction ToNodeDirection(MazeNode node) {
			if (node.x == this.x) {
				if (node.y + 1 == this.y) {
					return Direction.Up;
				} else if (node.y - 1 == this.y) {
					return Direction.Down;
				}
			} else if (node.y == this.y) {
				if (node.x + 1 == this.x) {
					return Direction.Left;
				} else if (node.x - 1 == this.x) {
					return Direction.Right;
				}
			}
			return Direction.None;
		}

		public IEnumerable<MazeNode> GetEnumeratorExclude(MazeNode excludeNode) {
			if (this.parent != null) {
				if (this.parent != excludeNode) {
					yield return this.parent;
					foreach (MazeNode node in this.childs) {
						if (node != excludeNode) yield return node;
					}
				} else {
					foreach (MazeNode node in this.childs) {
						yield return node;
					}
				}
			} else {
				foreach (MazeNode node in this.childs) {
					if (node != excludeNode) yield return node;
				}
			}
		}

		public IEnumerable<MazeNode> GetEnumeratorExcludeDirection(Direction from) {
			if (this.parent != null && this.ToNodeDirection(this.parent) != from) yield return this.parent;
			foreach (MazeNode node in this.childs) {
				if (this.ToNodeDirection(node) != from)
					yield return node;
			}
		}

		public IEnumerator<MazeNode> GetEnumerator() {
			if (this.parent != null) yield return this.parent;
			foreach (MazeNode node in this.childs) {
				yield return node;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}
	}
}
