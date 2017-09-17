using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 迷宫控件 {
	[DefaultEvent("MazeChanged")]
	public partial class Maze : UserControl {
		public delegate void MazeCompletedHandle(object sender, EventArgs e);
		public delegate void MazeChangedHandle(object sender, MazeEventArgs e);

		public event MazeCompletedHandle MazeCompleted;
		public event MazeChangedHandle MazeChanged;

		private static readonly Random random = new Random();

		private MazeMap map;
		private Point offset = Point.Empty;

		public Maze()
			: this(200, 150, 4) {
		}

		public Maze(int width, int height, int weight) {
			InitializeComponent();

			this.SetStyle(ControlStyles.ContainerControl
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.Opaque
				| ControlStyles.Selectable
				| ControlStyles.SupportsTransparentBackColor
				| ControlStyles.UserPaint
				| ControlStyles.AllPaintingInWmPaint, true);

			this.SetStyle(ControlStyles.StandardClick
				| ControlStyles.StandardDoubleClick, false);

			map = new MazeMap(width, height, weight);
			map.Completed += () => {
				if (this.MazeCompleted != null) {
					this.MazeCompleted(this, EventArgs.Empty);
				}
			};
			map.Changed += e => {
				if (this.MazeChanged != null) {
					this.MazeChanged(this, e);
				}
			};

			this.ScrollColor = Color.Blue;
			this.ScrollTransparent = 192;
			this.RootPointRandom = true;
			this.StartPointRandom = true;
			this.EndPointRandom = true;
			this.StartPoint = this.StartPointRandom ? new Point(random.Next(width), random.Next(height)) : new Point(0, 0);
			this.EndPoint = this.EndPointRandom ? new Point(random.Next(width), random.Next(height)) : new Point(width - 1, height - 1);
			this.RootPoint = new Point(width / 2, height / 2);
		}

		protected override void OnLoad(EventArgs e) {
			if (!this.DesignMode) {
				this.maxSize = this.Size;
			}
			base.OnLoad(e);
		}

		private Point downLocation, _offset, mousePoint = new Point(-1, -1);
		private bool isMove;
		protected override void OnMouseDown(MouseEventArgs e) {
			if (e.Button.HasFlag(MouseButtons.Left)) {
				downLocation = e.Location;
				_offset = offset;
				isMove = false;
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			if (e.Button.HasFlag(MouseButtons.Left)) {
				if (isMove || Math.Abs(e.X - downLocation.X) >= 5 || Math.Abs(e.Y - downLocation.Y) >= 5) {
					isMove = true;
					offset.X = _offset.X + e.X - downLocation.X;
					offset.Y = _offset.Y + e.Y - downLocation.Y;
					Redress();
					ShowScroll();
					this.Invalidate();
				}
			} else {
				// 清除上一次光标
				// 在此函数(OnMouseMove)返回之前，this.Invalidate方法并不会绘制，仅仅只是发送了通知
				this.DrawCursor();
				// 此处光标位置已改变，所以之前的绘制光标函数变成了清除上一次光标的作用
				mousePoint.X = (e.X - offset.X) / map.Weight;
				mousePoint.Y = (e.Y - offset.Y) / map.Weight;
				// 绘制光标
				this.DrawCursor();
			}
			base.OnMouseMove(e);
		}

		private void Redress() {
			if (offset.X >= 0 || map.ScreenWidth <= this.Width) {
				offset.X = 0;
			} else if (offset.X + map.ScreenWidth < this.Width) {
				offset.X = this.Width - map.ScreenWidth;
			}
			if (offset.Y >= 0 || map.ScreenHeight <= this.Height) {
				offset.Y = 0;
			} else if (offset.Y + map.ScreenHeight < this.Height) {
				offset.Y = this.Height - map.ScreenHeight;
			}
		}

		protected override void OnMouseLeave(EventArgs e) {
			this.DrawCursor();
			mousePoint = new Point(-1, -1);
			base.OnMouseLeave(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			int weight = map.Weight;
			if (e.Delta > 0) {
				map.Weight++;
			} else if (e.Delta < 0) {
				map.Weight--;
			} else {
				goto Result;
			}

			if (map.Weight <= 0) map.Weight = 1;

			offset.X = e.X + (offset.X - e.X) * map.Weight / weight;
			offset.Y = e.Y + (offset.Y - e.Y) * map.Weight / weight;

			this.AdjustSize();
			this.Redress();
			this.Invalidate();
			Result:
			base.OnMouseWheel(e);
		}

		private bool firstClicked = false;
		private Point firstPoint;
		private System.Diagnostics.Stopwatch clickCounter = new System.Diagnostics.Stopwatch();
		private const long ClickTimeSpanMilliseconds = 500;

		protected override void OnMouseUp(MouseEventArgs e) {
			if (!isMove) {
				if (firstClicked && e.X / map.Weight == firstPoint.X && e.Y / map.Weight == firstPoint.Y) {
					if (clickCounter.ElapsedMilliseconds < ClickTimeSpanMilliseconds) {
						base.OnMouseDoubleClick(e);
						if (e.Button.HasFlag(MouseButtons.Left)) base.OnDoubleClick(new EventArgs());
						firstClicked = false;
						return;
					}
				}
				base.OnMouseClick(e);
				if (e.Button.HasFlag(MouseButtons.Left)) this.OnClick(new EventArgs());
				firstClicked = true;
				firstPoint = new Point(e.X / map.Weight, e.Y / map.Weight);
				clickCounter.Restart();
			} else {
				if (e.Button.HasFlag(MouseButtons.Left)) isMove = false;
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.SetClip(e.ClipRectangle);
			e.Graphics.TranslateTransform(offset.X, offset.Y);
			Rectangle rect = e.ClipRectangle;
			rect.Offset(-offset.X, -offset.Y);
			map.Draw(e.Graphics, rect);
			map.DrawUserPath(e.Graphics);
			if (mousePoint.X >= 0 && mousePoint.Y >= 0)
				e.Graphics.FillRectangle(new SolidBrush(this.cursorColor), new Rectangle(mousePoint.X * map.Weight, mousePoint.Y * map.Weight, map.Weight, map.Weight));
			DrawScrollbar(e.Graphics);
			base.OnPaint(e);
		}

		private int scrollTransparent = 0;
		protected const int ScrollWeight = 4;

		protected Rectangle GetScrollXRectangle() {
			if (this.Width < map.ScreenWidth) {
				int width = this.Width * this.Width / map.ScreenWidth;
				int x = (this.Width - width) * -offset.X / (map.ScreenWidth - this.Width);
				return new Rectangle(x, this.Height - ScrollWeight, width, ScrollWeight);
			} else {
				return default(Rectangle);
			}
		}
		protected Rectangle GetScrollYRectangle() {
			if (this.Height < map.ScreenHeight) {
				int height = this.Height * this.Height / map.ScreenHeight;
				int y = (this.Height - height) * -offset.Y / (map.ScreenHeight - this.Height);
				return new Rectangle(this.Width - ScrollWeight, y, ScrollWeight, height);
			} else {
				return default(Rectangle);
			}
		}

		private void DrawScrollbar(Graphics g) {
			SolidBrush brush = new SolidBrush(Color.FromArgb(scrollTransparent, this.ScrollColor));
			g.ResetTransform();
			if (map.ScreenWidth > this.Width) {
				g.FillRectangle(brush, GetScrollXRectangle());
			}
			if (map.ScreenHeight > this.Height) {
				g.FillRectangle(brush, GetScrollYRectangle());
			}
		}

		private void DrawCursor() {
			Rectangle rect = new Rectangle(mousePoint.X * map.Weight, mousePoint.Y * map.Weight, map.Weight, map.Weight);
			rect.Offset(offset);
			this.Invalidate(rect);
		}

		public void CreateMazeMap() {
			if (this.StartPointRandom) this.StartPoint = new Point(random.Next(this.MazeWidth), random.Next(this.MazeHeight));
			if (this.EndPointRandom) this.EndPoint = new Point(random.Next(this.MazeWidth), random.Next(this.MazeHeight));
			if (this.RootPointRandom) {
				map.Create();
				this.RootPoint = new Point(map.RootX, map.RootY);
			} else {
				map.Create(this.RootPoint.X, this.RootPoint.Y);
			}
			this.AdjustSize();
			this.Invalidate();
		}

		protected void AdjustSize() {
			if (this.Dock == DockStyle.None) {
				base.Width = map.ScreenWidth < this.MaxWidth ? map.ScreenWidth : this.MaxWidth;
				base.Height = map.ScreenHeight < this.MaxHeight ? map.ScreenHeight : this.MaxHeight;
			}
		}

		private void timerDrawScrollbar_Tick(object sender, EventArgs e) {
			if (scrollTransparent > 8) {
				scrollTransparent -= 8;
			} else {
				scrollTransparent = 0;
				timerDrawScrollbar.Enabled = false;
			}
			this.Invalidate(GetScrollXRectangle());
			this.Invalidate(GetScrollYRectangle());
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			if (e.Modifiers == Keys.None) {
				//Rectangle pathRectangle;
				MazeNode.Direction direction = MazeNode.Direction.None;
				switch (e.KeyCode) {
					case Keys.Left: direction = MazeNode.Direction.Right; break;
					case Keys.Right: direction = MazeNode.Direction.Left; break;
					case Keys.Up: direction = MazeNode.Direction.Down; break;
					case Keys.Down: direction = MazeNode.Direction.Up; break;
					case Keys.Back:
						map.Back();
						StartScroll();
						this.Invalidate();
						goto Result;
					default: goto Result;
				}

				map.WiseGoto(direction);
				StartScroll();
				this.Invalidate();
			} else if (e.Control) {
				int weight = map.Weight;
				switch (e.KeyCode) {
					case Keys.Up: map.Weight++; break;
					case Keys.Down: map.Weight--; break;
					default: goto Result;
				}

				if (map.Weight <= 0) map.Weight = 1;

				offset.X = map.UserX * map.Weight + offset.X + (-map.UserX * map.Weight) * map.Weight / weight;
				offset.Y = map.UserY * map.Weight + offset.Y + (-map.UserY * map.Weight) * map.Weight / weight;

				this.AdjustSize();
				this.Redress();
				this.Invalidate();
			}

			Result:
			base.OnKeyDown(e);
		}

		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
			if (e.Modifiers == Keys.None) {
				switch (e.KeyCode) {
					case Keys.Left:
					case Keys.Right:
					case Keys.Up:
					case Keys.Down:
						e.IsInputKey = true;
						break;
				}
			} else if (e.Control) {
				switch (e.KeyCode) {
					case Keys.Up:
					case Keys.Down:
						e.IsInputKey = true;
						break;
				}
			}
			base.OnPreviewKeyDown(e);
		}

		public void Answer() {
			map.Answer();
			this.Invalidate();
		}

		public void Answer(int x, int y) {
			map.Answer(x, y);
			this.Invalidate();
		}

		private const int Range = 10;
		public bool StartScroll() {
			int rangeX = this.Width / 3;
			int rangeY = this.Height / 3;
			bool scrollX = false, scrollY = false;
			Point userPoint = new Point(map.UserX * map.Weight, map.UserY * map.Weight);
			if (map.ScreenWidth > this.Width) {
				if (offset.X != 0 && userPoint.X + offset.X < rangeX) {
					endOffset.X = rangeX - userPoint.X;
					if (endOffset.X > 0) endOffset.X = 0;
					scrollX = true;
				} else if (offset.X != this.Width - map.ScreenWidth && this.Width - offset.X - userPoint.X < rangeX) {
					endOffset.X = this.Width - rangeX - userPoint.X;
					if (endOffset.X < this.Width - map.ScreenWidth) endOffset.X = this.Width - map.ScreenWidth;
					scrollX = true;
				}
			}
			if (map.ScreenHeight > this.Height) {
				if (offset.Y != 0 && userPoint.Y + offset.Y < rangeY) {
					endOffset.Y = rangeY - userPoint.Y;
					if (endOffset.Y > 0) endOffset.Y = 0;
					scrollY = true;
				} else if (offset.Y != this.Height - map.ScreenHeight && this.Height - offset.Y - userPoint.Y < rangeY) {
					endOffset.Y = this.Height - rangeY - userPoint.Y;
					if (endOffset.Y < this.Height - map.ScreenHeight) endOffset.Y = this.Height - map.ScreenHeight;
					scrollY = true;
				}
			}
			if (scrollX) {
				offsetSpeed.X = (endOffset.X - offset.X) / (200 / this.timerScroll.Interval);
				//offsetSpeed.X = (int)Math.Sqrt(this.MazeWeight * this.MazeWeight + temp * temp);
				//if (temp < 0) offsetSpeed.X = -offsetSpeed.X;
				//offsetSpeed.X = endOffset.X > offset.X ? map.Weight : -map.Weight;
				if (offsetSpeed.X == 0) offsetSpeed.X = endOffset.X > offset.X ? 1 : -1;
			} else {
				offsetSpeed.X = 0;
			}
			if (scrollY) {
				offsetSpeed.Y = (endOffset.Y - offset.Y) / (200 / this.timerScroll.Interval);
				//offsetSpeed.Y = (int)Math.Sqrt(this.MazeWeight * this.MazeWeight + temp * temp);
				//if (temp < 0) offsetSpeed.Y = -offsetSpeed.Y;
				//offsetSpeed.Y = endOffset.Y > offset.Y ? map.Weight : -map.Weight;
				if (offsetSpeed.Y == 0) offsetSpeed.Y = endOffset.Y > offset.Y ? 1 : -1;
			} else {
				offsetSpeed.Y = 0;
			}
			Console.WriteLine(offsetSpeed);
			if (scrollX || scrollY) {
				this.timerScroll.Enabled = true;
				return true;
			} else {
				return false;
			}
		}

		public void ShowScroll() {
			scrollTransparent = this.ScrollTransparent;
			this.timerDrawScrollbar.Enabled = true;
		}

		private Point endOffset, offsetSpeed;
		private void timerScroll_Tick(object sender, EventArgs e) {
			bool endX = true, endY = true;
			if (offsetSpeed.X != 0) {
				// endOffset.X < offset.X + offsetSpeed.X && offsetSpeed.X < 0
				// endOffset.X > offset.X + offsetSpeed.X && offsetSpeed.X > 0
				if (offset.X != endOffset.X && endOffset.X.CompareTo(offset.X + offsetSpeed.X) == offsetSpeed.X.CompareTo(0)) {
					offset.X += offsetSpeed.X;
					endX = false;
				} else {
					offset.X = endOffset.X;
				}
			}
			if (offsetSpeed.Y != 0) {
				// endOffset.Y < offset.Y + offsetSpeed.Y && offsetSpeed.Y < 0
				// endOffset.Y > offset.Y + offsetSpeed.Y && offsetSpeed.Y > 0
				if (offset.Y != endOffset.Y && endOffset.Y.CompareTo(offset.Y + offsetSpeed.Y) == offsetSpeed.Y.CompareTo(0)) {
					offset.Y += offsetSpeed.Y;
					endY = false;
				} else {
					offset.Y = endOffset.Y;
				}
			}
			if (endX && endY) this.timerScroll.Enabled = false;
			ShowScroll();
			this.Invalidate();
		}

		protected override void OnClick(EventArgs e) {
			if (mousePoint.X >= 0 && mousePoint.Y >= 0 && mousePoint.X < map.Width && mousePoint.Y < map.Height) {
				if (this.IsDiscernPath && map.ToDiscernNode(mousePoint.X, mousePoint.Y)) {
					StartScroll();
					this.Invalidate();
				}
			}
			base.OnClick(e);
		}

		public void Reset() {
			map.Reset();
			this.Invalidate();
		}

		public Bitmap ToBitmap() {
			return map.ToBitmap();
		}

		private Size maxSize = new Size(600, 400);
		private Color cursorColor = Color.FromArgb(60, 255, 255, 255);

		[Category("布局")]
		[DefaultValue(typeof(Size), "600,400")]
		[Browsable(true)]
		[Description("设置或获取显示屏幕的最大大小")]
		public virtual Size MaxSize {
			get { return maxSize; }
			set {
				if (value.Width <= 0 || value.Height <= 0) return;
				maxSize = value;
			}
		}
		[Browsable(false)]
		public virtual int MaxWidth { get { return MaxSize.Width; } }
		[Browsable(false)]
		public virtual int MaxHeight { get { return MaxSize.Height; } }
		[Category("参数")]
		[DefaultValue(200)]
		[Browsable(true)]
		[Description("设置或获取迷宫的宽度")]
		public virtual int MazeWidth { get { return map.Width; } set { map.Width = value; } }
		[Category("参数")]
		[DefaultValue(150)]
		[Browsable(true)]
		[Description("设置或获取迷宫的高度")]
		public virtual int MazeHeight { get { return map.Height; } set { map.Height = value; } }
		[Category("参数")]
		[DefaultValue(4)]
		[Browsable(true)]
		[Description("设置或获取迷宫的路径宽度")]
		public virtual int MazeWeight { get { return map.Weight; } set { map.Weight = value; this.Invalidate(); } }
		[Browsable(false)]
		public virtual int MazeScreenWidth { get { return map.ScreenWidth; } }
		[Browsable(false)]
		public virtual int MazeScreenHeight { get { return map.ScreenHeight; } }
		[Category("参数")]
		[Browsable(true)]
		[DefaultValue(typeof(Color), "")]
		[Description("设置或获取迷宫的前景色")]
		public new virtual Color ForeColor { get { return map.ForeColor; } set { map.ForeColor = value; this.Invalidate(); } }
		[Category("参数")]
		[Browsable(true)]
		[DefaultValue(typeof(Color), "")]
		[Description("设置或获取迷宫的背景色")]
		public new virtual Color BackColor { get { return map.BackColor; } set { map.BackColor = value; this.Invalidate(); } }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取用户标记颜色")]
		public virtual Color UserColor { get { return map.UserColor; } set { map.UserColor = value; this.Invalidate(); } }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取滚动条颜色")]
		public virtual Color ScrollColor { get; set; }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取寻路标记颜色")]
		public virtual Color DiscernPathColor { get { return map.DiscernPathColor; } set { map.DiscernPathColor = value; this.Invalidate(); } }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取光标颜色")]
		public virtual Color CursorColor { get { return this.cursorColor; } set { this.cursorColor = value; this.DrawCursor(); } }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取起点颜色")]
		public virtual Color StartPointColor {
			get { return map.StartPointColor; }
			set { map.StartPointColor = value; this.Invalidate(new Rectangle(map.StartX * map.Weight, map.StartY * map.Weight, map.Weight, map.Weight)); }
		}
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取终点颜色")]
		public virtual Color EndPointColor {
			get { return map.EndPointColor; }
			set { map.EndPointColor = value; this.Invalidate(new Rectangle(map.EndX * map.Weight, map.EndY * map.Weight, map.Weight, map.Weight)); }
		}
		[Category("参数")]
		[DefaultValue(192)]
		[Browsable(true)]
		[Description("设置或获取滚动条透明度")]
		public virtual int ScrollTransparent { get; set; }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取迷宫根节点位置")]
		public virtual Point RootPoint { get; set; }
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取迷宫起点位置")]
		public virtual Point StartPoint {
			get { return new Point(map.StartX, map.StartY); }
			set { map.StartX = value.X; map.StartY = value.Y; }
		}
		[Category("参数")]
		[Browsable(true)]
		[Description("设置或获取迷宫终点位置")]
		public virtual Point EndPoint {
			get { return new Point(map.EndX, map.EndY); }
			set { map.EndX = value.X; map.EndY = value.Y; }
		}
		[Browsable(false)]
		public virtual Point UserPoint { get { return new Point(map.UserX, map.UserY); } }
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取迷宫根节点位置是否随机")]
		public virtual bool RootPointRandom { get; set; }
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取迷宫起始位置是否随机")]
		public virtual bool StartPointRandom { get; set; }
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取迷宫终点位置是否随机")]
		public virtual bool EndPointRandom { get; set; }
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取是否显示起始位置")]
		public virtual bool IsShowStartPoint {
			get { return map.IsShowStartPoint; }
			set { map.IsShowStartPoint = value; }
		}
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取是否显示终点位置")]
		public virtual bool IsShowEndPoint {
			get { return map.IsShowEndPoint; }
			set { map.IsShowEndPoint = value; }
		}
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取是否开启智能寻路提示")]
		public virtual bool IsDiscernPath {
			get { return map.IsDiscernPath; }
			set {
				this.Invalidate(map.GetMapRectangle(map.DiscernPathRectangle));
				map.IsDiscernPath = value;
				this.Invalidate(map.GetMapRectangle(map.DiscernPathRectangle));
			}
		}
		[Category("参数")]
		[DefaultValue(500)]
		[Browsable(true)]
		[Description("设置或获取智能提示的寻路能力值")]
		public virtual int DiscernPathAbility {
			get { return map.DiscernPathAbility; }
			set { map.DiscernPathAbility = value; if (map.IsDiscernPath) this.Invalidate(map.GetMapRectangle(map.DiscernPathRectangle)); }
		}
		[Category("参数")]
		[DefaultValue(true)]
		[Browsable(true)]
		[Description("设置或获取是否开启分支查找")]
		public virtual bool IsFindBranch {
			get { return map.IsFindBranch; }
			set { map.IsFindBranch = value; }
		}
		[Category("参数")]
		[DefaultValue("")]
		[Browsable(true)]
		[Description("设置随机数的映射函数")]
		public virtual string RandomMapper {
			set {
				try {
					if (!string.IsNullOrWhiteSpace(value))
						map.RandomMapper = RandomMapperGenerator.Generate(value);
				} catch (Exception e) {
					MessageBox.Show(e.Message);
				}
			}
			get { return ""; }
		}
		[Category("参数")]
		[DefaultValue("")]
		[Browsable(true)]
		[Description("设置或获取随机数种子")]
		public virtual string RandomSeed {
			get { return map.RandomSeed; }
			set { map.RandomSeed = value; }
		}
	}
}
