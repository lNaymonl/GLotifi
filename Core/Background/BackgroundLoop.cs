using GLotifi.Core;
using GLotifi.Notifications;
using GLotifi.Services;

namespace GLotifi.Background
{
    public static class BackgroundLoop
    {
        public static void Start()
        {
            try
            {
                Configuration.LoadEnvironment();

                if (!File.Exists(Constants.TodoFilePath))
                {
                    File.Create(Constants.TodoFilePath).Close();
                    File.WriteAllText(Constants.TodoFilePath, "[]");
                }

                ToastNotifier.ShowStartupNotification();

                while (true)
                {
                    if (DateTime.Now - Constants.LastExec < Constants.ExecEverySecSpan)
                    {
                        Task.Delay(1000).Wait();
                        continue;
                    }

                    try
                    {
                        var unannounced = GitLabService.GetUnannouncedTodos().Result;

                        foreach (var todo in unannounced)
                            ToastNotifier.ShowTodoNotification(todo);

                        Constants.LastExec = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        ToastNotifier.ShowError($"Error during execution: {ex.Message}");
                        Environment.Exit(1);
                    }

                    Task.Delay(1000).Wait();
                }
            }
            catch (Exception ex)
            {
                ToastNotifier.ShowError($"Startup failed: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
