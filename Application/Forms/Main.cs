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

		#region .    PInvoke

		[DllImport("user32.dll")]
		public static extern void LockWorkStation();

		[DllImport("user32.dll")]
		public static extern int ExitWindowsEx(int uFlags, int dwReason);

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

			var @switch = new ToolStripMenuItem("Switch User", null, new EventHandler(delegate(object sender, EventArgs e) {
				WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, false);
			}));
			@switch.ToolTipText = "Switch to a different user, without logging off";

			var logoff = new ToolStripMenuItem("Logoff", null, new EventHandler(delegate(object sender, EventArgs e) {
				if(Ask("Logoff", "Logoff, and grab a drink")) {
					ExitWindowsEx(4, 0);
				}
			}));

			var @lock = new ToolStripMenuItem("Lock", null, new EventHandler(delegate(object sender, EventArgs e) {
				LockWorkStation();
			}));
			@lock.ToolTipText = "Lock the computer, so nosey people can't meddle";

			var screensaver = new ToolStripMenuItem("Start Screensaver", null, new EventHandler(delegate(object sender, EventArgs e) {
				SendMessage(GetDesktopWindow(), WM_SYSCOMMAND, new IntPtr(SC_SCREENSAVE), IntPtr.Zero);
			}));

			var sleep = new ToolStripMenuItem("Sleep", null, new EventHandler(delegate(object sender, EventArgs e) {
				Application.SetSuspendState(PowerState.Suspend, true, true);
			}));
			sleep.ToolTipText = "Give your computer a rest, and put it to sleep.";

			var hibernate = new ToolStripMenuItem("Hibernate", null, new EventHandler(delegate(object sender, EventArgs e) {
				if(Ask("Hibernate", "Put the computer into hibernation, without turning it off")) {
					Application.SetSuspendState(PowerState.Hibernate, true, true);
				}
			}));
			hibernate.ToolTipText = "Put the computer into hibernation, without turning it off";

			var restart = new ToolStripMenuItem("Restart", null, new EventHandler(delegate(object sender, EventArgs e) {
				if(Ask("Restart", "Restart the computer")) {
					ExitWindowsEx(2, 0);
				}
			}));

			var shutdown = new ToolStripMenuItem("Shutdown", Resources.Images.IconSmall, new EventHandler(delegate(object sender, EventArgs e) {
				if(Ask("Shutdown", "Shutdown the computer")) {
					ExitWindowsEx(1, 0);
				}
			}));

			shutdown.Font = new Font(shutdown.Font, shutdown.Font.Style | FontStyle.Bold);

			var about = new ToolStripMenuItem("About Simple Power Plus", null, new EventHandler(delegate(object sender, EventArgs e) {
				About aboutWin = Shellscape.Program.FindForm(typeof(About)) as About ?? new About();

				MethodInvoker method = delegate() { // yes, all this ugly is necessary.
					aboutWin.Show();
					aboutWin.TopMost = true;
					aboutWin.BringToFront();
					aboutWin.Focus();
					aboutWin.TopMost = false;
				};

				if(aboutWin.InvokeRequired) {
					aboutWin.Invoke(method);
				}
				else {
					method();
				}
			}));

			var exit = new ToolStripMenuItem("Exit", null, new EventHandler(delegate(object sender, EventArgs e) {
				_trayIcon.Visible = false;
				Application.Exit();
			}));

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
				about, exit
			});
		}

		private Boolean Ask(String function, String yes) {

			using(TaskDialog dialog = new TaskDialog() { Icon = TaskDialogStandardIcon.Information }) {

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

		public static bool ScreenSaverActive() {

			try {
				RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

				if(key != null) {
					return key.GetValue("SCRNSAVE.EXE", null) != null;
				}
			}
			catch(System.Security.SecurityException) { }
			catch(System.UnauthorizedAccessException) { }

			return false;
		}

	}
}
