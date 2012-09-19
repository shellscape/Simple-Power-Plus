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

namespace SimplePowerPlus {
  public partial class Main : Form {

	private NotifyIcon _trayIcon = new NotifyIcon();

	[DllImport("user32.dll")]
	public static extern void LockWorkStation();

	[DllImport("user32.dll")]
	public static extern int ExitWindowsEx(int uFlags, int dwReason);

	[DllImport("wtsapi32.dll", SetLastError = true)]
	static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

	private const int WTS_CURRENT_SESSION = -1;
	private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

	public Main() {
	  InitializeComponent();

	  this.Text = "Simple Power Plus";
	  this.ShowInTaskbar = false;
	  this.Location = new Point(-10000, -10000);

	  _trayIcon.Icon = this.Icon;
	  _trayIcon.Visible = true;

	  var @switch  = new ToolStripMenuItem("Switch User", null, new EventHandler(delegate(object sender, EventArgs e){
		WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, false);
	  }));

	  var logoff = new ToolStripMenuItem("Logoff", null, new EventHandler(delegate(object sender, EventArgs e) {
		ExitWindowsEx(4, 0);
	  }));

	  var @lock = new ToolStripMenuItem("Lock", null, new EventHandler(delegate(object sender, EventArgs e) {
		LockWorkStation();
	  }));

	  var sleep = new ToolStripMenuItem("Sleep", null, new EventHandler(delegate(object sender, EventArgs e) {
		Application.SetSuspendState(PowerState.Suspend, true, true);
	  }));

	  var hibernate = new ToolStripMenuItem("Hibernate", null, new EventHandler(delegate(object sender, EventArgs e) {
		Application.SetSuspendState(PowerState.Hibernate, true, true);
	  }));

	  var restart = new ToolStripMenuItem("Restart", null, new EventHandler(delegate(object sender, EventArgs e) {
		ExitWindowsEx(2, 0); 
	  }));

	  var shutdown = new ToolStripMenuItem("Shutdown", null, new EventHandler(delegate(object sender, EventArgs e) {
		ExitWindowsEx(1, 0);
	  }));

	  _trayIcon.ContextMenuStrip = new ContextMenuStrip();
	  _trayIcon.MouseClick += delegate(object sender, MouseEventArgs e) {
		if (e.Button == MouseButtons.Left) {
		  MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
		  mi.Invoke(_trayIcon, null);
		}
	  };

	  _trayIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[]{ 
		@switch, logoff, @lock, 
		new ToolStripSeparator(), 
		sleep, hibernate, 
		new ToolStripSeparator(), 
		restart, shutdown 
	  });
	}
  }
}
