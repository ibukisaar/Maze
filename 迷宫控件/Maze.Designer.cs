namespace 迷宫控件
{
    partial class Maze
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.timerDrawScrollbar = new System.Windows.Forms.Timer(this.components);
			this.timerScroll = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// timerDrawScrollbar
			// 
			this.timerDrawScrollbar.Interval = 50;
			this.timerDrawScrollbar.Tick += new System.EventHandler(this.timerDrawScrollbar_Tick);
			// 
			// timerScroll
			// 
			this.timerScroll.Interval = 25;
			this.timerScroll.Tick += new System.EventHandler(this.timerScroll_Tick);
			// 
			// Maze
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.DoubleBuffered = true;
			this.Name = "Maze";
			this.Size = new System.Drawing.Size(503, 332);
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.Timer timerDrawScrollbar;
		private System.Windows.Forms.Timer timerScroll;
    }
}
