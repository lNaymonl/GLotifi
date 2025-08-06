using DotNetEnv;

namespace GLotifi.Core
{
    public static class Configuration
    {
        public static void LoadEnvironment()
        {
            Env.Load(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ".env"));

            var envPath = Environment.GetEnvironmentVariable("TODO_FILE_PATH");
            if (envPath != null) Constants.TodoFilePath = envPath;
            else Directory.CreateDirectory(Constants.DefaultTodoDirectoryPath);

            Constants.GitLabUrl = GetEnvVar("GITLAB_URL");
            Constants.GitLabToken = GetEnvVar("GITLAB_TOKEN");
            Constants.ExecEverySec = int.Parse(GetEnvVar("EXEC_EVERY_SEC"));
            Constants.ExecEverySecSpan = TimeSpan.FromSeconds(Constants.ExecEverySec);
        }

        public static string GetEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} variable must be present in .env file");
    }
}
