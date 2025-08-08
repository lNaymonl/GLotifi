using System.Text;
using System.Text.Json;
using DotNetEnv;
using GLotifi.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System.Threading;

namespace GLotifi
{
    internal static class Program
    {
        private static readonly string DEFAULT_TODO_DIRECTORY_PATH = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GLotifi");
        private static string TODO_FILE_PATH = Path.Join(DEFAULT_TODO_DIRECTORY_PATH, "alreadyAnnounced.json");
        private static string GITLAB_URL = "";
        private static string GITLAB_TOKEN = "";
        private static int EXEC_EVERY_SEC = 30;
        private static DateTime LastExec = DateTime.Now;
        private static TimeSpan EXEC_EVERY_SEC_TSPAN = TimeSpan.FromSeconds(EXEC_EVERY_SEC);
        private const string AUTOSTART_REG_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "GLotifi";
        private const string MUTEX_NAME = "GLotifi_SingleInstanceMutex";

        [STAThread]
        static void Main()
        {

            using (Mutex mutex = new(true, MUTEX_NAME, out bool createdNew))
            {
                if (!createdNew)
                {
                    new ToastContentBuilder()
                        .AddToastActivationInfo("action=alreadyStarted", ToastActivationType.Foreground)
                        .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                        .AddText("GLotifi")
                        .AddText("GLotifi is already running.")
                        .SetToastDuration(ToastDuration.Short)
                        .Show();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string envPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".env");

                if (!EnvSetup.CheckAndSetupEnv(envPath, TODO_FILE_PATH))
                {
                    new ToastContentBuilder()
                        .AddToastActivationInfo("action=setupAborted", ToastActivationType.Foreground)
                        .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                        .AddText("GLotifi")
                        .AddText("The setup got aborted. Application will exit.")
                        .SetToastDuration(ToastDuration.Short)
                        .Show();
                    return;
                }

                var envTodoFilePath = Environment.GetEnvironmentVariable("TODO_FILE_PATH");
                if (envTodoFilePath != null) TODO_FILE_PATH = envTodoFilePath;

                GITLAB_URL = Environment.GetEnvironmentVariable("GITLAB_URL")!;
                GITLAB_TOKEN = Environment.GetEnvironmentVariable("GITLAB_TOKEN")!;
                EXEC_EVERY_SEC = int.Parse(Environment.GetEnvironmentVariable("EXEC_EVERY_SEC")!);
                EXEC_EVERY_SEC_TSPAN = TimeSpan.FromSeconds(EXEC_EVERY_SEC);

                var notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "GLotifi.ico")),
                    Visible = true,
                    Text = "GLotifi"
                };

                var contextMenu = new ContextMenuStrip();

                var autostartItem = new ToolStripMenuItem("Autostart")
                {
                    CheckOnClick = true,
                    Checked = IsAutostartEnabled()
                };

                autostartItem.CheckedChanged += (s, e) =>
                {
                    if (autostartItem.Checked)
                    {
                        EnableAutostart();
                        new ToastContentBuilder()
                            .AddToastActivationInfo("action=enableAutostart", ToastActivationType.Foreground)
                            .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                            .AddText("GLotifi")
                            .AddText("Autostart is enabled!")
                            .SetToastDuration(ToastDuration.Short)
                            .Show();
                    }
                    else
                    {
                        DisableAutostart();
                        new ToastContentBuilder()
                            .AddToastActivationInfo("action=enableAutostart", ToastActivationType.Foreground)
                            .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                            .AddText("GLotifi")
                            .AddText("Autostart is disabled!")
                            .SetToastDuration(ToastDuration.Short)
                            .Show();
                    }
                };

                contextMenu.Items.Add(autostartItem);
                contextMenu.Items.Add(new ToolStripSeparator());

                var exitItem = new ToolStripMenuItem("Exit GLotifi");
                exitItem.Click += (s, e) =>
                {
                    notifyIcon.Visible = false;
                    Application.Exit();
                    Environment.Exit(0);
                };

                contextMenu.Items.Add(exitItem);
                notifyIcon.ContextMenuStrip = contextMenu;

                Task.Run(() => StartBackgroundLoop());
                Application.Run();
            }

            static void StartBackgroundLoop()
            {
                try
                {
                    Env.Load(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".env"));

                    if (Environment.GetEnvironmentVariable("TODO_FILE_PATH") is string envTodoFilePath)
                        TODO_FILE_PATH = envTodoFilePath;
                    else
                        Directory.CreateDirectory(DEFAULT_TODO_DIRECTORY_PATH);

                    GITLAB_URL = GetEnvVar("GITLAB_URL");
                    GITLAB_TOKEN = GetEnvVar("GITLAB_TOKEN");
                    EXEC_EVERY_SEC = int.Parse(GetEnvVar("EXEC_EVERY_SEC"));
                    EXEC_EVERY_SEC_TSPAN = TimeSpan.FromSeconds(EXEC_EVERY_SEC);

                    if (!File.Exists(TODO_FILE_PATH))
                    {
                        File.Create(TODO_FILE_PATH).Close();
                        File.WriteAllText(TODO_FILE_PATH, "[]");
                    }

                    new ToastContentBuilder()
                        .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                        .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                        .AddText("GLotifi")
                        .AddText("GLotifi got successfully started in the background!\n\nStart observing your todo list!")
                        .SetProtocolActivation(new Uri("https://github.com/lNaymonl/GLotifi"))
                        .SetToastDuration(ToastDuration.Short)
                        .Show();

                    while (true)
                    {
                        if (DateTime.Now - LastExec < EXEC_EVERY_SEC_TSPAN)
                        {
                            Task.Delay(1000).Wait();
                            continue;
                        }

                        try
                        {
                            var task = GetUnanouncedTodos();
                            task.Wait();

                            var unannounced = task.Result.ToList();
                            foreach (var todo in unannounced)
                            {
                                new ToastContentBuilder()
                                    .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                                    .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                                    .AddText(todo.Target.Title)
                                    .AddText(todo.Target.Description)
                                    .SetProtocolActivation(new Uri(todo.TargetUrl))
                                    .SetToastDuration(ToastDuration.Long)
                                    .Show();
                            }

                            LastExec = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            new ToastContentBuilder()
                                .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                                .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                                .AddText("An error occurred while running GLotifi: " + ex.Message)
                                .SetToastDuration(ToastDuration.Short)
                                .Show();
                            Environment.Exit(1);
                        }

                        Task.Delay(1000).Wait();
                    }
                }
                catch (Exception ex)
                {
                    new ToastContentBuilder()
                        .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                        .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                        .AddText("An error occurred while starting GLotifi: " + ex.Message)
                        .SetToastDuration(ToastDuration.Short)
                        .Show();
                    Environment.Exit(1);
                }
            }
        }

        public static string GetEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} variable must be present in .env file");

        public static async Task<IEnumerable<Todo>> GetTodos()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", GITLAB_TOKEN);
            var res = await httpClient.GetAsync($"{GITLAB_URL}/api/v4/todos");

            var content = await res.Content.ReadAsByteArrayAsync();
            var contentString = Encoding.UTF8.GetString(content);
            var todos = JsonSerializer.Deserialize<Todo[]>(contentString) ?? throw new NullReferenceException("Could not parse todos");

            return todos;
        }

        public static async Task<IEnumerable<Todo>> GetUnanouncedTodos()
        {
            string alreadyAnnouncedTodosStr = File.ReadAllText(TODO_FILE_PATH);
            List<int> alreadyAnnouncedTodos = [.. JsonSerializer.Deserialize<int[]>(alreadyAnnouncedTodosStr) ?? throw new NullReferenceException("Given file content could not be parsed into int array!")];

            var todos = await GetTodos();
            File.WriteAllText(TODO_FILE_PATH, JsonSerializer.Serialize(todos.Select(todo => todo.Id)));

            return todos.Where(todo => !alreadyAnnouncedTodos.Contains(todo.Id)).ToList();
        }

        private static void EnableAutostart()
        {
            string exePath = Application.ExecutablePath;
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(AUTOSTART_REG_PATH, true)!;
            key.SetValue(APP_NAME, $"\"{exePath}\"");
        }

        private static void DisableAutostart()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(AUTOSTART_REG_PATH, true)!;
            key.DeleteValue(APP_NAME, false);
        }

        private static bool IsAutostartEnabled()
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(AUTOSTART_REG_PATH, false)!;
            return key.GetValue(APP_NAME) != null;
        }
    }
}
