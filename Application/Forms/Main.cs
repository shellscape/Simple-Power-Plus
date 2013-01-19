using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;
using Microsoft.WindowsAPI.Dialogs;

namespace SimplePowerPlus.Forms {
	public partial class Main : Form {

		private NotifyIcon _trayIcon = new NotifyIcon();
		private Boolean _ask = true;

		#region .    PInvoke

		[DllImport("user32.dll")]
		public static extern void LockWorkStation();

		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

		[DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
		private static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		private const int SC_SCREENSAVE = 0xF140;
		private const int WM_SYSCOMMAND = 0x0112;
		private const int WTS_CURRENT_SESSION = -1;
		private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

		#endregion

		public Main() {
			InitializeComponent();

			this.Text = "Simple Power Plus";
			this.ShowInTaskbar = false;
			this.Location = new Point(-10000, -10000);

			_trayIcon.Icon = Resources.Icons.Tray;
			_trayIcon.Visible = true;
			_trayIcon.Text = "Simple Power Plus";

			var @switch = new ToolStripMenuItem("Switch User", null, (object sender, EventArgs e) => {
				WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, false);
			});
			@switch.ToolTipText = "Switch to a different user, without logging off";

			var logoff = new ToolStripMenuItem("Logoff", null, (object sender, EventArgs e) => {
				if (Ask("Logoff", "Logoff, and grab a drink")) {
					Shutdown("/l");
				}
			});

			var @lock = new ToolStripMenuItem("Lock", null, (object sender, EventArgs e) => {
				LockWorkStation();
			});
			@lock.ToolTipText = "Lock the computer, so nosey people can't meddle";

			var screensaver = new ToolStripMenuItem("Start Screensaver", null, (object sender, EventArgs e) => {
				SendMessage(GetDesktopWindow(), WM_SYSCOMMAND, new IntPtr(SC_SCREENSAVE), IntPtr.Zero);
			});

			var sleep = new ToolStripMenuItem("Sleep", null, (object sender, EventArgs e) => {
				Application.SetSuspendState(PowerState.Suspend, true, true);
			});
			sleep.ToolTipText = "Give your computer a rest, and put it to sleep.";

			var hibernate = new ToolStripMenuItem("Hibernate", null, (object sender, EventArgs e) => {
				if (Ask("Hibernate", "Put the computer into hibernation, without turning it off")) {
					Application.SetSuspendState(PowerState.Hibernate, true, true);
				}
			});
			hibernate.ToolTipText = "Put the computer into hibernation, without turning it off";

			var restart = new ToolStripMenuItem("Restart", null, (object sender, EventArgs e) => {
				if (Ask("Restart", "Restart the computer")) {
					Shutdown("/r /t 0");
				}
			});

			var shutdown = new ToolStripMenuItem("Shutdown", Resources.Images.IconSmall, (object sender, EventArgs e) => {
				if (Ask("Shutdown", "Shutdown the computer")) {
					Shutdown("/s /t 0");
				}
			});

			shutdown.Font = new Font(shutdown.Font, shutdown.Font.Style | FontStyle.Bold);

			var ask = new ToolStripMenuItem("Ask before action", null, (object sender, EventArgs e) => {
				var item = sender as ToolStripMenuItem;
				item.Checked = Config.Current.Ask = !Config.Current.Ask;
				Config.Current.Save();
			});

			ask.Checked = Config.Current.Ask;

			var about = new ToolStripMenuItem("About Simple Power Plus", null, (object sender, EventArgs e) => {
				About aboutWin = Shellscape.Program.FindForm(typeof(About)) as About ?? new About();

				MethodInvoker method = delegate() { // yes, all this ugly is necessary.
					aboutWin.Show();
					aboutWin.TopMost = true;
					aboutWin.BringToFront();
					aboutWin.Focus();
					aboutWin.TopMost = false;
				};

				if (aboutWin.InvokeRequired) {
					aboutWin.Invoke(method);
				}
				else {
					method();
				}
			});

			var exit = new ToolStripMenuItem("Exit", null, (object sender, EventArgs e) => {
				_trayIcon.Visible = false;
				Application.Exit();
			});

			_trayIcon.ContextMenuStrip = new ContextMenuStrip();
			_trayIcon.MouseClick += delegate(object sender, MouseEventArgs e) {

				screensaver.Enabled = ScreenSaverActive();

				MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
				mi.Invoke(_trayIcon, null);
			};

			_trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]{ 
				@switch, logoff, @lock, screensaver,
				new ToolStripSeparator(), 
				sleep, hibernate, 
				new ToolStripSeparator(), 
				restart, shutdown,
				new ToolStripSeparator(),
				ask, about, exit
			});
		}

		private Boolean Ask(String function, String yes) {

			if (!Config.Current.Ask) {
				return true;
			}

			using (TaskDialog dialog = new TaskDialog() { Icon = TaskDialogStandardIcon.Information }) {

				TaskDialogCommandLink yesButton = new TaskDialogCommandLink("yes", "Yes, " + function, yes);
				yesButton.Click += delegate(object s, EventArgs ea) {
					dialog.Close(TaskDialogResult.Yes);
				};

				TaskDialogCommandLink noButton = new TaskDialogCommandLink("no", "No", "Get me out of here...");
				noButton.Click += delegate(object s, EventArgs ea) {
					dialog.Close(TaskDialogResult.No);
				};

				dialog.InstructionText = "Are you sure you want to " + function + "?";
				dialog.Caption = "Simple Power Plus - " + function;
				dialog.Controls.Add(yesButton);
				dialog.Controls.Add(noButton);

				var result = dialog.Show();

				return result == TaskDialogResult.Yes;
			}
		}

		private bool ScreenSaverActive() {

			try {
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

				if (key != null) {
					return key.GetValue("SCRNSAVE.EXE", null) != null;
				}
			}
			catch (System.Security.SecurityException) { }
			catch (System.UnauthorizedAccessException) { }

			return false;
		}

		private void Shutdown(String arguments) {
			
			using (var process = new System.Diagnostics.Process()) {
				process.StartInfo.FileName = "shutdown";
				process.StartInfo.Arguments = arguments;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
				process.StartInfo.RedirectStandardOutput = true;

				process.Start();
			}
		}

	}
}
