using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace 迷宫控件 {
	public class MazeEventArgs : EventArgs {
		public ReadOnlyCollection<Point> Points { get; private set; }

		public MazeEventArgs(IList<Point> points) {
			this.Points = new ReadOnlyCollection<Point>(points);
		}
	}
}
