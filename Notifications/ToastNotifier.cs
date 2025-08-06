using GLotifi.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GLotifi.Notifications
{
    public static class ToastNotifier
    {
        private static string IconPath => Path.Join("file:///", AppDomain.CurrentDomain.BaseDirectory, "GLotifi.png");

        public static void ShowStartupNotification()
        {
            new ToastContentBuilder()
                .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                .AddAppLogoOverride(new Uri(IconPath))
                .AddText("GLotifi")
                .AddText("GLotifi has started successfully in the background.\n\nAnd now you can enjoy your GitLab todos in peace!")
                .SetProtocolActivation(new Uri("https://github.com/lNaymonl/GLotifi"))
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }

        public static void ShowTodoNotification(Todo todo)
        {
            new ToastContentBuilder()
                .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                .AddAppLogoOverride(new Uri(IconPath))
                .AddText(todo.Target.Title)
                .AddText(todo.Target.Description)
                .SetProtocolActivation(new Uri(todo.TargetUrl))
                .SetToastDuration(ToastDuration.Long)
                .Show();
        }

        public static void ShowError(string message)
        {
            new ToastContentBuilder()
                .AddToastActivationInfo("action=viewDetails", ToastActivationType.Foreground)
                .AddAppLogoOverride(new Uri(IconPath))
                .AddText("GLotifi Error")
                .AddText(message)
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }
    }
}
