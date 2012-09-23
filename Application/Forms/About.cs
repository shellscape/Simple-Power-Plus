using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.WindowsAPI.Dialogs;
using Microsoft.WindowsAPI.Shell;


using Shellscape;

namespace SimplePowerPlus.Forms {
	public partial class About : Shellscape.UI.About {

		private Bitmap _logo;

		public About() : base() {

			this.Icon = Resources.Icons.Window;
			this.Text = "About - Simple Power Plus";
			this._logo = Resources.Images.IconLarge;

			InitializeComponent();

			this._Flow.SuspendLayout();
			this.SuspendLayout();

			String toYear = DateTime.Now.Year.ToString();
			String linkTarget = "Shellscape Software";

			toYear = toYear == "2012" ? String.Empty : String.Concat("-", toYear);

			this._Button.Font = this._ButtonDonate.Font = SystemFonts.MessageBoxFont;
			this._Button.Text = "Sweet, Thanks!";
			this._ButtonDonate.Text = "Please consider donating";
			this._LabelCopyright.Text = String.Join("\n",
				String.Concat("Copyright © 2012", toYear, " Andrew Powell, ", linkTarget, ". All rights reserved."));
			this._LabelCopyright.Links.Add(this._LabelCopyright.Text.IndexOf(linkTarget), linkTarget.Length);
			this._LabelCopyright.Font = SystemFonts.MessageBoxFont;
			this._LabelCopyright.LinkColor = this._LabelCopyright.NormalColor = this._LabelCopyright.HoverColor;
			this._LabelCopyright.LinkBehavior = LinkBehavior.AlwaysUnderline;

			this._Flow.ResumeLayout(true);
			this.ResumeLayout(true);

			this._Button.Click += delegate(object sender, EventArgs e) {
				this.Close();
			};

			this._LabelCopyright.LinkClicked += delegate(object sender, LinkLabelLinkClickedEventArgs e) {
				Help.ShowHelp(this, "http://shellscape.org");
			};
		}

		protected override string DonationDescription {
			get { return "Simple%20Power%20Plus%20Donation"; }
		}

		protected override void OnPaintIcon(Graphics g) {
			float percent = (float)256 / (float)this._logo.Width;
			g.DrawImage(this._logo, 326, 7, this._logo.Width * percent, this._logo.Height * percent);
		}

	}
}
