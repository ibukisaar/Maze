using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using 迷宫控件;
using System.IO;

namespace 迷宫 {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {
			//maze1.StartPointRandom = true;
			//maze1.EndPointRandom = true;
			maze1.CreateMazeMap();
			this.LoadConfig();
		}

		private void LoadConfig() {
			try {
				this.numWidth.Value = maze1.MazeWidth;
				this.numHeight.Value = maze1.MazeHeight;
				this.ChangedPointRange();
				this.chkStartRandom.Checked = maze1.StartPointRandom;
				this.chkEndRandom.Checked = maze1.EndPointRandom;
				this.numStartX.Value = maze1.StartPoint.X;
				this.numStartX.Value = maze1.StartPoint.Y;
				this.numEndX.Value = maze1.EndPoint.X;
				this.numEndY.Value = maze1.EndPoint.Y;
				this.chkMouseOperator.Checked = maze1.IsDiscernPath;
				this.chkFindBranch.Checked = maze1.IsFindBranch;
				this.numDiscernPathAbility.Value = maze1.DiscernPathAbility;
				this.picBackColor.BackColor = maze1.BackColor;
				this.picForeColor.BackColor = maze1.ForeColor;
				this.picStartColor.BackColor = maze1.StartPointColor;
				this.picEndColor.BackColor = maze1.EndPointColor;
				this.picDiscernColor.BackColor = maze1.DiscernPathColor;
				this.picCursorColor.BackColor = maze1.CursorColor;
				this.picUserColor.BackColor = maze1.UserColor;
				this.picScrollColor.BackColor = maze1.ScrollColor;
			} catch (Exception e) {
				MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void ChangedPointRange() {
			this.numStartX.Maximum = this.numWidth.Value - 1;
			this.numStartY.Maximum = this.numHeight.Value - 1;
			this.numEndX.Maximum = this.numWidth.Value - 1;
			this.numEndY.Maximum = this.numHeight.Value - 1;
		}

		private void ApplyConfig() {
			try {
				maze1.MazeWidth = (int) this.numWidth.Value;
				maze1.MazeHeight = (int) this.numHeight.Value;
				this.ChangedPointRange();
				maze1.StartPointRandom = this.chkStartRandom.Checked;
				maze1.StartPoint = new Point((int) this.numStartX.Value, (int) this.numStartY.Value);
				maze1.EndPointRandom = this.chkEndRandom.Checked;
				maze1.EndPoint = new Point((int) this.numEndX.Value, (int) this.numEndY.Value);
				maze1.RandomSeed = this.txtRandomSeed.Text;
				maze1.RandomMapper = this.txtFunction.Text;
				maze1.CreateMazeMap();
			} catch (Exception e) {
				MessageBox.Show(e.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void maze1_Click(object sender, EventArgs e) {
			Console.WriteLine("Click");
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e) {
			Console.WriteLine("form : " + e.KeyCode);
		}

		private void Form1_DoubleClick(object sender, EventArgs e) {
			Console.WriteLine("Double Click");
		}

		private void Form1_Click(object sender, EventArgs e) {
			Console.WriteLine("Click");
		}

		private void maze1_DoubleClick(object sender, EventArgs e) {
			//maze1.ToBitmap().Save("maze.jpg");
			maze1.Answer();
		}

		private void maze1_MazeCompleted(object sender, EventArgs e) {
			MessageBox.Show("完成");
		}

		private void maze1_MazeChanged(object sender, MazeEventArgs e) {
			Console.WriteLine(e.Points.Count);
			foreach (var point in e.Points) {
				Console.WriteLine(point);
			}
		}

		private void btnApply_Click(object sender, EventArgs e) {
			this.ApplyConfig();
		}

		private void chkMouseOperator_CheckedChanged(object sender, EventArgs e) {
			maze1.IsDiscernPath = this.chkMouseOperator.Checked;
		}

		private void chkFindBranch_CheckedChanged(object sender, EventArgs e) {
			maze1.IsFindBranch = this.chkFindBranch.Checked;
		}

		private void picBackColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picBackColor.BackColor = this.colorDialog1.Color;
				maze1.BackColor = this.colorDialog1.Color;
			}
		}

		private void picForeColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picForeColor.BackColor = this.colorDialog1.Color;
				maze1.ForeColor = this.colorDialog1.Color;
			}
		}

		private void picStartColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picStartColor.BackColor = this.colorDialog1.Color;
				maze1.StartPointColor = this.colorDialog1.Color;
			}
		}

		private void picEndColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picEndColor.BackColor = this.colorDialog1.Color;
				maze1.EndPointColor = this.colorDialog1.Color;
			}
		}

		private void picDiscernColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picDiscernColor.BackColor = this.colorDialog1.Color;
				maze1.DiscernPathColor = Color.FromArgb(80, this.colorDialog1.Color);
			}
		}

		private void picUserColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picUserColor.BackColor = this.colorDialog1.Color;
				maze1.UserColor = this.colorDialog1.Color;
			}
		}

		private void picCursorColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picCursorColor.BackColor = this.colorDialog1.Color;
				maze1.CursorColor = Color.FromArgb(128, this.colorDialog1.Color);
			}
		}

		private void picScrollColor_Click(object sender, EventArgs e) {
			if (this.colorDialog1.ShowDialog() == DialogResult.OK) {
				this.picScrollColor.BackColor = this.colorDialog1.Color;
				maze1.ScrollColor = Color.FromArgb(128, this.colorDialog1.Color);
			}
		}

		private void Form1_Shown(object sender, EventArgs e) {
			this.maze1.Focus();
		}

		private void numDiscernPathAbility_ValueChanged(object sender, EventArgs e) {
			this.maze1.DiscernPathAbility = (int) this.numDiscernPathAbility.Value;
		}
	}
}
