using GLotifi.Background;
using GLotifi.Tray;

namespace GLotifi
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TrayIconManager.SetupTrayIcon();

            Task.Run(() => BackgroundLoop.Start());
            Application.Run();
        }
    }
}
