using System.Text;
using System.Text.Json;
using DotNetEnv;
using GLotifi.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GLotifi
{
    static class Program
    {
        private static readonly string DEFAULT_TODO_DIRECTORY_PATH = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GLotifi");
        private static string TODO_FILE_PATH = Path.Join(DEFAULT_TODO_DIRECTORY_PATH, "alreadyAnnounced.json");
        private static string GITLAB_URL = "";
        private static string GITLAB_TOKEN = "";
        private static int EXEC_EVERY_SEC = 30;
        private static DateTime LastExec = DateTime.Now;
        private static TimeSpan ExecEverySec = TimeSpan.Zero;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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

            Task.Run(() => StartBackgroundLoop());

            new ToastContentBuilder()
                .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                .AddAppLogoOverride(new Uri(Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png")))
                .AddText("GLotifi")
                .AddText("GLotifi got successfully started in the background!\n\nStart observing your todo list!")
                .SetProtocolActivation(new Uri("https://github.com/lNaymonl/GLotifi"))
                .SetToastDuration(ToastDuration.Short)
                .Show();

            Application.Run(); // Starting application
        }

        static void StartBackgroundLoop()
        {
            try
            {
                Env.Load(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".env"));

                var envTodoFilePath = Environment.GetEnvironmentVariable("TODO_FILE_PATH");
                if (envTodoFilePath != null) TODO_FILE_PATH = envTodoFilePath;
                else Directory.CreateDirectory(DEFAULT_TODO_DIRECTORY_PATH);

                GITLAB_URL = GetEnvVar("GITLAB_URL");
                GITLAB_TOKEN = GetEnvVar("GITLAB_TOKEN");
                EXEC_EVERY_SEC = int.Parse(GetEnvVar("EXEC_EVERY_SEC"));
                ExecEverySec = TimeSpan.FromSeconds(EXEC_EVERY_SEC);

                if (!File.Exists(TODO_FILE_PATH))
                {
                    File.Create(TODO_FILE_PATH).Close();
                    File.WriteAllText(TODO_FILE_PATH, "[]");
                }

                while (true)
                {
                    if (DateTime.Now - LastExec >= ExecEverySec)
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

                    Task.Delay(1000).Wait();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}", "GLotifi Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
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
    }
}
