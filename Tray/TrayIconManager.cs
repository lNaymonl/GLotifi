namespace GLotifi.Tray
{
    public static class TrayIconManager
    {
        public static void SetupTrayIcon()
        {
            var notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "GLotifi.ico")),
                Visible = true,
                Text = "GLotifi"
            };

            var contextMenu = new ContextMenuStrip();
            var exitItem = new ToolStripMenuItem("Exit GLotifi");

            exitItem.Click += (s, e) =>
            {
                notifyIcon.Visible = false;
                Application.Exit();
                Environment.Exit(0);
            };

            contextMenu.Items.Add(exitItem);
            notifyIcon.ContextMenuStrip = contextMenu;
        }
    }
}
