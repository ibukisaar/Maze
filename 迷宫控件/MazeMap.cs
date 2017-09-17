using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace 迷宫控件 {
	public class MazeMap {
		private class DiscernPathResult {
			public HashSet<MazeNode> Paths;
			public Rectangle Rectangle;
			public DiscernPathResult(HashSet<MazeNode> paths, Rectangle rectangle) {
				this.Paths = paths;
				this.Rectangle = rectangle;
			}
		}

		public event Action Completed;
		public event Action<MazeEventArgs> Changed;

		private static Random random = new Random();

		private int width, height, weight;
		private bool resize = true;
		private MazeNode[,] map;
		private Bitmap mazeImage;
		private List<MazeNode> userPath;
		private int userX, userY;
		private int rootX, rootY;
		private bool userChanged = true;
		private bool reupdate = true;
		private Color foreColor = Color.FromArgb(255, 64, 255);
		private Color backColor = Color.White;
		private Color startPointColor = Color.FromArgb(128, 255, 0, 0);
		private Color endPointColor = Color.FromArgb(128, 0, 255, 0);
		private Point[] lines;
		private DiscernPathResult result;
		private Rectangle[] rects;
		private bool isDiscernPath = true;
		private int discernPathAbility = 250;
		private CumulativeDistributionFunction randomMapper = x => x;

		public int Width {
			get { return width; }
			set { width = value; resize = true; reupdate = true; }
		}
		public int Height {
			get { return height; }
			set { height = value; resize = true; reupdate = true; }
		}
		public int Weight {
			get { return weight; }
			set {
				if (weight != value) {
					weight = value;
					userChanged = true;
					reupdate = true;
				}
			}
		}
		public int StartX { get; set; }
		public int StartY { get; set; }
		public int EndX { get; set; }
		public int EndY { get; set; }
		public int RootX { get { return rootX; } }
		public int RootY { get { return rootY; } }
		public int UserX { get { return userX; } }
		public int UserY { get { return userY; } }
		public Color ForeColor { get { return foreColor; } set { foreColor = value; reupdate = true; } }
		public Color BackColor { get { return backColor; } set { backColor = value; reupdate = true; } }
		public Color UserColor { get; set; }
		public Color DiscernPathColor { get; set; }
		public Color StartPointColor { get { return startPointColor; } set { startPointColor = value; reupdate = true; } }
		public Color EndPointColor { get { return endPointColor; } set { endPointColor = value; reupdate = true; } }
		public bool IsDiscernPath {
			get {
				return isDiscernPath;
			}
			set {
				if (value) {
					userChanged = true;
					if (IsCreated) result = this.DiscernPath();
				}
				isDiscernPath = value;
			}
		}
		public bool IsFindBranch { get; set; }
		public int DiscernPathAbility {
			get { return discernPathAbility; }
			set {
				if (isDiscernPath && discernPathAbility != value) {
					discernPathAbility = value;
					userChanged = true;
					if (IsCreated) result = this.DiscernPath();
				} else {
					discernPathAbility = value;
				}
			}
		}
		public Rectangle DiscernPathRectangle {
			get { return result == null ? Rectangle.Empty : result.Rectangle; }
		}
		public bool IsShowStartPoint { get; set; }
		public bool IsShowEndPoint { get; set; }
		public bool IsCreated { get; private set; }
		public string RandomSeed { get; set; }
		public CumulativeDistributionFunction RandomMapper {
			get { return randomMapper; }
			set { randomMapper = value; }
		}

		public MazeMap(int width, int height, int weight) {
			this.width = width;
			this.height = height;
			this.weight = weight;
			this.UserColor = Color.Blue;
			this.DiscernPathColor = Color.FromArgb(80, 255, 128, 128);
			this.userPath = new List<MazeNode>((width + height) * 10);
			this.StartX = 0;
			this.StartY = 0;
			this.EndX = width - 1;
			this.EndY = height - 1;
			//this.RootX = width / 2;
			//this.RootY = height / 2;
			this.IsDiscernPath = true;
			this.DiscernPathAbility = 250;
			this.IsShowStartPoint = true;
			this.IsShowEndPoint = true;
			this.IsFindBranch = true;
		}

		private void AddString(string text, Point offset, float size) {
			GraphicsPath path = new GraphicsPath();
			path.AddString(text, new FontFamily("Consolas"), 0, size, offset, new StringFormat());
			Rectangle rect = new Rectangle(0, 0, width, height);
			rect.Intersect(path.GetBounds().ToRectangle());
			if (text != "❤") {
				for (int x = rect.Left; x < rect.Right; x++) {
					for (int y = rect.Top; y < rect.Bottom; y++) {
						if (path.IsVisible(x, y))
							map[x, y].Type = MazeNode.MazeNodeType.障碍;
					}
				}
			} else {
				int x2 = (rect.Left + rect.Right) / 2;
				int y2 = (rect.Top + rect.Bottom) / 2;
				for (int x = rect.Left; x < rect.Right; x++) {
					for (int y = rect.Top; y < rect.Bottom; y++) {
						if (x != x2 && path.IsVisible(x, y))
							map[x, y].Type = MazeNode.MazeNodeType.障碍;
					}
				}
			}
		}

		private void Resize() {
			if (resize) {
				map = new MazeNode[width, height];
				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						map[x, y] = new MazeNode(x, y);
					}
				}
				resize = false;
			} else {
				for (int x = 0; x < width; x++) {
					for (int y = 0; y < height; y++) {
						map[x, y].Reset();
					}
				}
			}
		}

		public void SetRandomSeed() {
			if (!string.IsNullOrEmpty(this.RandomSeed)) {
				int seed = this.RandomSeed.GetHashCode();
				random = new Random(seed);
				RandomHelper.SetSeed(seed);
			}
		}

		public void Create() {
			this.SetRandomSeed();
			Create(random.Next(width), random.Next(height), false);
		}

		public void Create(int rootX, int rootY, bool setSeed = true) {
			this.rootX = rootX;
			this.rootY = rootY;

			Resize();

			if (setSeed) this.SetRandomSeed();

			DisorderList<MazeNode> tempNodes = new DisorderList<MazeNode>(width * height);	 // 已记录节点
			MazeNode header = new MazeNode(-1, -1);
			MazeNode root = map[RootX, RootY];
			header.AddChild(root);
			tempNodes.Add(root);
			double count = height;
			while (tempNodes.Count > 0) {
				int index = (int) (random.NextDouble(randomMapper) * tempNodes.Count);
				MazeNode randomNode = tempNodes[index];
				List<MazeNode> childs = GetSurroundNodes(randomNode);
				if (childs.Count == 0) {
					tempNodes.RemoveAt(index);
				} else {
					MazeNode next = childs.Random();
					randomNode.AddChild(next);
					tempNodes.Add(next);
				}
			}

			header.RemoveChild(root);
			userPath.Clear();
			userPath.Add(map[StartX, StartY]);
			userX = StartX;
			userY = StartY;
			result = DiscernPath();
			userChanged = true;
			reupdate = true;
			IsCreated = true;
		}

		public void Draw(Graphics graphics, Rectangle updateRectangle) {
			if (map != null) {
				int x1 = updateRectangle.Left / weight; if (x1 < 0) x1 = 0;
				int x2 = updateRectangle.Right / weight; if (x2 >= width) x2 = width - 1;
				int y1 = updateRectangle.Top / weight; if (y1 < 0) y1 = 0;
				int y2 = updateRectangle.Bottom / weight; if (y2 >= height) y2 = height - 1;

				if (weight <= 10) {
					if (reupdate) {
						mazeImage = new Bitmap(width * weight + 1, height * weight + 1);
						Draw(Graphics.FromImage(mazeImage), 0, 0, width - 1, height - 1);
						reupdate = false;
					}
					graphics.DrawImageUnscaled(mazeImage, Point.Empty);
				} else {
					mazeImage = null;
					Draw(graphics, x1, y1, x2, y2);
				}
			}
		}

		private void Draw(Graphics graphics, int x1, int y1, int x2, int y2) {
			graphics.Clear(this.BackColor);
			Pen pen = new Pen(this.ForeColor);
			SolidBrush brush = new SolidBrush(pen.Color);

			if (IsShowStartPoint && StartX >= x1 && StartX <= x2 && StartY >= y1 && StartY <= y2) {
				graphics.FillRectangle(new SolidBrush(this.StartPointColor), new Rectangle(StartX * weight, StartY * weight, weight, weight));
			}
			if (IsShowEndPoint && EndX >= x1 && EndX <= x2 && EndY >= y1 && EndY <= y2) {
				graphics.FillRectangle(new SolidBrush(this.EndPointColor), new Rectangle(EndX * weight, EndY * weight, weight, weight));
			}

			int wx0 = x1 * weight, wy0 = y1 * weight;
			for (int x = x1, wx = wx0; x <= x2; x++, wx += weight) {
				for (int y = y1, wy = wy0; y <= y2; y++, wy += weight) {
					MazeNode node = map[x, y];
					Point point = new Point(wx, wy);
					if (node.Type == MazeNode.MazeNodeType.空白) {
						//if (node.Parent == null && node != map[rootX, rootY]) continue;
						if (!node.Up) graphics.DrawLine(pen, point, new Point(point.X + weight, point.Y));
						if (!node.Left) graphics.DrawLine(pen, point, new Point(point.X, point.Y + weight));
					} else {
						graphics.FillRectangle(brush, point.X, point.Y, weight, weight);
					}
				}
			}
			graphics.DrawLine(pen, new Point(0, height * weight), new Point(width * weight, height * weight));
			graphics.DrawLine(pen, new Point(width * weight, 0), new Point(width * weight, height * weight));
		}

		public List<MazeNode> GetSurroundNodes(MazeNode node) {
			List<MazeNode> output = new List<MazeNode>();
			if (node.X > 0) {
				MazeNode n = map[node.X - 1, node.Y];
				if (n.Parent == null && n.Type == MazeNode.MazeNodeType.空白) {
					output.Add(n);
				}
			}
			if (node.Y > 0) {
				MazeNode n = map[node.X, node.Y - 1];
				if (n.Parent == null && n.Type == MazeNode.MazeNodeType.空白) {
					output.Add(n);
				}
			}
			if (node.X < width - 1) {
				MazeNode n = map[node.X + 1, node.Y];
				if (n.Parent == null && n.Type == MazeNode.MazeNodeType.空白) {
					output.Add(n);
				}
			}
			if (node.Y < height - 1) {
				MazeNode n = map[node.X, node.Y + 1];
				if (n.Parent == null && n.Type == MazeNode.MazeNodeType.空白) {
					output.Add(n);
				}
			}
			return output;
		}

		public bool SetNode(int x, int y, MazeNode.MazeNodeType type) {
			if (x < 0 || x >= width || y < 0 || y >= height) return false;
			map[x, y].Type = type;
			return true;
		}

		private MazeNode GotoNode(MazeNode node, MazeNode.Direction from) {
			switch (from) {
				case MazeNode.Direction.Left: return node.Right ? map[node.X + 1, node.Y] : null;
				case MazeNode.Direction.Right: return node.Left ? map[node.X - 1, node.Y] : null;
				case MazeNode.Direction.Up: return node.Down ? map[node.X, node.Y + 1] : null;
				case MazeNode.Direction.Down: return node.Up ? map[node.X, node.Y - 1] : null;
				default: return null;
			}
		}

		public bool Goto(MazeNode.Direction from) {
			List<Point> eventArgs = null;
			if (this.Changed != null) eventArgs = new List<Point>() { new Point(userX, userY) };

			MazeNode temp = GotoNode(map[userX, userY], from);
			if (temp != null) {
				userX = temp.X;
				userY = temp.Y;
			} else {
				return false;
			}

			MazeNode next;
			do {
				next = map[userX, userY];
				if (userPath.Count >= 2 && userPath[userPath.Count - 2] == next) {
					userPath.RemoveAt(userPath.Count - 1);
				} else {
					userPath.Add(next);
				}

				if (userX == EndX && userY == EndY && this.Completed != null) {
					this.Completed();
				}

				if (this.Changed != null) eventArgs.Add(new Point(userX, userY));
			} while (IsFindBranch && next.OnlyOnePath(ref userX, ref userY, ref from));

			userChanged = true;
			result = DiscernPath();
			if (this.Changed != null) {
				this.Changed(new MazeEventArgs(eventArgs));
			}
			return true;
		}

		public bool WiseGoto(MazeNode.Direction from) {
			if (Goto(from)) {
				return true;
			} else {
				MazeNode user = map[userX, userY];
				MazeNode.Direction direction1 = from.ROL(1);
				MazeNode.Direction direction2 = from.ROL(3);
				MazeNode node1 = GotoNode(user, direction1);
				MazeNode node2 = GotoNode(user, direction2);
				if (node1 != null) {
					if (node2 != null) {
						MazeNode node3 = GotoNode(node1, from);
						MazeNode node4 = GotoNode(node2, from);
						if (node3 != null && node4 == null) {
							return Goto(direction1);
						}
						if (node3 == null && node4 != null) {
							return Goto(direction2);
						}
						return false;
					} else {
						return Goto(direction1);
					}
				} else {
					if (node2 != null) {
						return Goto(direction2);
					} else {
						return false;
					}
				}
			}
		}

		public void Back() {
			if (userPath.Count >= 2) {
				Goto(userPath[userPath.Count - 2].ToNodeDirection(userPath[userPath.Count - 1]));
			}
		}

		public void Reset() {
			userPath.Clear();
			userPath.Add(map[StartX, StartY]);
			userX = StartX;
			userY = StartY;
			result = DiscernPath();
			userChanged = true;
		}

		public void DrawUserPath(Graphics g) {
			if (result != null && this.IsDiscernPath) {
				if (userChanged) {
					rects = result.Paths.Select(node => GetNodeRectangle(node.X, node.Y)).ToArray();
				}
				g.FillRectangles(new SolidBrush(this.DiscernPathColor), rects);
			}
			if (userPath.Count >= 2) {
				if (userChanged) {
					lines = userPath.Select<MazeNode, Point>(node => new Point(node.X * weight + (weight >> 1), node.Y * weight + (weight >> 1))).ToArray();
				}
				g.DrawLines(new Pen(this.UserColor), lines);
			}
			userChanged = false;
		}

		private List<MazeNode> Answer(MazeNode start, MazeNode end) {
			List<MazeNode> startPath = new List<MazeNode>(start.Depth + end.Depth);
			List<MazeNode> endPath = new List<MazeNode>(end.Depth);
			if (start.Depth < end.Depth) {
				while (start.Depth != end.Depth) {
					endPath.Add(end);
					end = end.Parent;
				}

			} else if (start.Depth > end.Depth) {
				while (start.Depth != end.Depth) {
					startPath.Add(start);
					start = start.Parent;
				}
			}

			while (start != end) {
				startPath.Add(start);
				endPath.Add(end);
				start = start.Parent;
				end = end.Parent;
			}
			startPath.Add(start);
			startPath.AddRange(endPath.Reverse<MazeNode>());
			return startPath;
		}

		public void Answer() {
			this.Answer(EndX, EndY);
		}

		public void Answer(int x, int y) {
			userPath = Answer(map[StartX, StartY], map[x, y]);
			userX = x;
			userY = y;
			result = DiscernPath();
			userChanged = true;
		}

		private DiscernPathResult DiscernPath() {
			MazeNode userNode = map[userX, userY];
			DiscernPathResult result = new DiscernPathResult(new HashSet<MazeNode>() { userNode }, new Rectangle(userX, userY, 1, 1));
			foreach (MazeNode node in userNode) {
				DiscernPath(result, userNode, node, node.ToNodeDirection(userNode), 0, 1);
			}
			return result;
		}

		private void DiscernPath(DiscernPathResult result, MazeNode parent, MazeNode node, MazeNode.Direction direction, int ability, int deep) {
			result.Rectangle = result.Rectangle.Union(new Rectangle(node.X, node.Y, 1, 1));
			result.Paths.Add(node);
			foreach (MazeNode next in node.GetEnumeratorExclude(parent)) {
				int ability2 = ability;
				MazeNode.Direction direction2 = next.ToNodeDirection(node);
				if (direction != direction2) {	 // 非直线
					if (next.IsOnlyOnePath(direction2)) {	// 无分支
						ability2++;
					} else {
						ability2 += deep;
					}
				} else {
					if (!next.IsOnlyOnePath(direction2)) ability2++;
				}

				if (ability2 <= DiscernPathAbility) {
					DiscernPath(result, node, next, direction2, ability2, deep + 1);
				}
			}
		}

		public Rectangle GetNodeRectangle(int x, int y) {
			return new Rectangle(x * weight, y * weight, weight, weight);
		}

		public Rectangle GetMapRectangle(Rectangle rect) {
			return new Rectangle(rect.Left * weight, rect.Top * weight, rect.Width * weight, rect.Height * weight);
		}

		public bool ToDiscernNode(int x, int y) {
			if (result != null && result.Paths.Contains(map[x, y])) {
				List<MazeNode> path = Answer(map[userX, userY], map[x, y]);
				if (this.Changed != null && path.Count >= 2) {
					this.Changed(new MazeEventArgs(path.Select(node => new Point(node.X, node.Y)).ToList()));
				}
				int removeCount = 0;
				foreach (MazeNode node in path) {
					if (userPath.Count > removeCount) {
						if (userPath[userPath.Count - 1 - removeCount] == node) {
							removeCount++;
						} else {
							userPath.RemoveRange(userPath.Count - removeCount, removeCount);
							path.RemoveRange(0, removeCount - 1);
							userPath.AddRange(path);
							goto Result;
						}
					} else {
						path.RemoveRange(0, userPath.Count - 1);
						userPath.Clear();
						userPath.AddRange(path);
						goto Result;
					}
				}
				userPath.RemoveRange(userPath.Count - removeCount + 1, removeCount - 1);
				Result:
				userX = x;
				userY = y;
				result = DiscernPath();
				userChanged = true;

				if (userX == EndX && userY == EndY && this.Completed != null) {
					this.Completed();
				}

				return true;
			} else {
				return false;
			}
		}

		public int ScreenWidth { get { return width * weight + 1; } }
		public int ScreenHeight { get { return height * weight + 1; } }

		public Bitmap ToBitmap() {
			string s = "Saar";
			Console.WriteLine(new string(s.Reverse().ToArray()));
			Bitmap bitmap = new Bitmap(this.ScreenWidth, this.ScreenHeight);
			Graphics g = Graphics.FromImage(bitmap);
			this.Draw(g, 0, 0, width - 1, height - 1);
			this.DrawUserPath(g);
			return bitmap;
		}
	}
}
