namespace GLotifi.Core
{
    public static class Constants
    {
        public static readonly string DefaultTodoDirectoryPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GLotifi");
        public static string TodoFilePath = Path.Join(DefaultTodoDirectoryPath, "alreadyAnnounced.json");

        public static int ExecEverySec = 30;
        public static TimeSpan ExecEverySecSpan = TimeSpan.FromSeconds(ExecEverySec);

        public static string GitLabUrl = "";
        public static string GitLabToken = "";

        public static DateTime LastExec = DateTime.Now;
    }
}
