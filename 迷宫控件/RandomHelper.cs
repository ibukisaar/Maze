using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 迷宫控件 {
	/// <summary>
	/// 映射函数：F(x), x∈[0,1)
	/// </summary>
	/// <param name="x">x∈[0,1)</param>
	/// <returns>F(x)</returns>
	public delegate double CumulativeDistributionFunction(double x);

	public static class RandomHelper {
		static Random random = new Random();

		public static void SetSeed(int seed) {
			random = new Random(seed);
		}

		public static T Random<T>(this ICollection<T> list) {
			if (list.Count > 0) {
				int index = random.Next(list.Count);
				return list.ElementAt(index);
			} else {
				return default(T);
			}
		}

		public static T Random<T>(this ICollection<T> list, CumulativeDistributionFunction f) {
			if (list.Count > 0) {
				double x = random.NextDouble();
				double y = f(x);
				int index = (int) (y * list.Count);
				return list.ElementAt(index);
			} else {
				return default(T);
			}
		}

		public static T At<T>(this ICollection<T> list, double value) {
			if (list.Count > 0) {
				return list.ElementAt((int) (list.Count * value));
			} else {
				return default(T);
			}
		}

		public static T Random<T>(this IList<T> list) {
			if (list.Count > 0) {
				int index = random.Next(list.Count);
				return list[index];
			} else {
				return default(T);
			}
		}

		public static T Random<T>(this IList<T> list, CumulativeDistributionFunction f) {
			if (list.Count > 0) {
				double x = random.NextDouble();
				double y = f(x);
				int index = (int) (y * list.Count);
				return list[index];
			} else {
				return default(T);
			}
		}

		public static T At<T>(this IList<T> list, double value) {
			if (list.Count > 0) {
				return list[(int) (list.Count * value)];
			} else {
				return default(T);
			}
		}

		public static double NextDouble(this Random random, CumulativeDistributionFunction f) {
			return f(random.NextDouble());
		}

		public static Rectangle Union(this Rectangle obj, Rectangle rect) {
			int x1 = obj.Left < rect.Left ? obj.Left : rect.Left;
			int y1 = obj.Top < rect.Top ? obj.Top : rect.Top;
			int x2 = obj.Right > rect.Right ? obj.Right : rect.Right;
			int y2 = obj.Bottom > rect.Bottom ? obj.Bottom : rect.Bottom;
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		public static Rectangle Intersection(this Rectangle obj, Rectangle rect) {
			int x1 = obj.Left > rect.Left ? obj.Left : rect.Left;
			int y1 = obj.Top > rect.Top ? obj.Top : rect.Top;
			int x2 = obj.Right < rect.Right ? obj.Right : rect.Right;
			int y2 = obj.Bottom < rect.Bottom ? obj.Bottom : rect.Bottom;
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}

		public static MazeNode.Direction ROL(this MazeNode.Direction direction, int value) {
			int temp = (int) direction << value;
			return (MazeNode.Direction) ((temp >> 4) | temp & 0xF);
		}

		public static MazeNode.Direction ROR(this MazeNode.Direction direction, int value) {
			return direction.ROL(4 - value);
		}

		public static Rectangle ToRectangle(this RectangleF rectf) {
			int x1 = (int) Math.Floor(rectf.Left);
			int x2 = (int) Math.Ceiling(rectf.Right);
			int y1 = (int) Math.Floor(rectf.Top);
			int y2 = (int) Math.Ceiling(rectf.Bottom);
			return new Rectangle(x1, y1, x2 - x1, y2 - y1);
		}
	}
}
