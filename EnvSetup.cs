
namespace GLotifi
{
    internal static class EnvSetup
    {
        private static readonly string[] RequiredKeys = { "GITLAB_URL", "GITLAB_TOKEN", "EXEC_EVERY_SEC" };

        public static bool CheckAndSetupEnv(string envPath, string defaultTodoPath)
        {
            bool envExists = File.Exists(envPath);

            if (!envExists || !HasAllRequiredVariables(envPath))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                while (true)
                {
                    using var form = new SetupForm(envPath, defaultTodoPath);
                    var result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        DotNetEnv.Env.Load(envPath);
                        return true;
                    }
                    else
                    {
                        var exit = MessageBox.Show(
                            "Setup got aborted. Do you want to exit the application?",
                            "Confirm cancellation",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (exit == DialogResult.Yes)
                            return false;
                    }
                }
            }

            DotNetEnv.Env.Load(envPath);
            return true;
        }

        private static bool HasAllRequiredVariables(string envPath)
        {
            if (!File.Exists(envPath))
                return false;

            try
            {
                var lines = File.ReadAllLines(envPath);
                var envDict = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
                    .Select(line => line.Split('=', 2))
                    .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);

                return RequiredKeys.All(key => envDict.ContainsKey(key) && !string.IsNullOrWhiteSpace(envDict[key]));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    internal class SetupForm : Form
        {
            private TextBox? txtGitlabUrl;
            private TextBox? txtGitlabToken;
            private TextBox? txtExecEverySec;
            private TextBox? txtTodoFilePath;
            private Button? btnBrowse;
            private Button? btnSave;
            private Button? btnCancel;

            private readonly string _envPath;
            private readonly string _defaultTodoPath;

            private const int WM_NCHITTEST = 0x84;
            private const int HTCAPTION = 0x2;
            private const int HTCLIENT = 0x1;

            public SetupForm(string envPath, string defaultTodoPath)
            {
                _envPath = envPath ?? throw new ArgumentNullException(nameof(envPath));
                _defaultTodoPath = defaultTodoPath ?? throw new ArgumentNullException(nameof(defaultTodoPath));

                InitializeComponents();
                LoadExistingValues();
            }

            private void InitializeComponents()
            {
                Text = "GLotifi Setup";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = true;
                StartPosition = FormStartPosition.CenterScreen;
                ClientSize = new Size(450, 300);

                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GLotifi.ico");
                if (File.Exists(iconPath))
                {
                    Icon = new Icon(iconPath);
                }

                CreateControls();
                SetupEventHandlers();
                SetTabOrder();

                AcceptButton = btnSave;
                CancelButton = btnCancel;
            }

            private void CreateControls()
            {
                const int labelWidth = 120;
                const int controlLeft = labelWidth + 20;
                const int margin = 20;
                int currentTop = margin;
                const int rowHeight = 40;

                // GitLab URL
                var lblGitlabUrl = new Label
                {
                    Text = "GitLab URL:",
                    Left = margin,
                    Top = currentTop + 3,
                    Width = labelWidth,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                txtGitlabUrl = new TextBox
                {
                    Left = controlLeft,
                    Top = currentTop,
                    Width = 300,
                    Text = ""
                };

                currentTop += rowHeight;

                // GitLab Token
                var lblGitlabToken = new Label
                {
                    Text = "GitLab Token:",
                    Left = margin,
                    Top = currentTop + 3,
                    Width = labelWidth,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                txtGitlabToken = new TextBox
                {
                    Left = controlLeft,
                    Top = currentTop,
                    Width = 300,
                    UseSystemPasswordChar = true,
                    Text = ""
                };

                currentTop += rowHeight;

                // Execution Interval
                var lblExecEverySec = new Label
                {
                    Text = "Interval (seconds):",
                    Left = margin,
                    Top = currentTop + 3,
                    Width = labelWidth,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                txtExecEverySec = new TextBox
                {
                    Left = controlLeft,
                    Top = currentTop,
                    Width = 100,
                    Text = "30"
                };

                currentTop += rowHeight;

                // Todo File Path
                var lblTodoFilePath = new Label
                {
                    Text = "Todo File Path:",
                    Left = margin,
                    Top = currentTop + 3,
                    Width = labelWidth,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                txtTodoFilePath = new TextBox
                {
                    Left = controlLeft,
                    Top = currentTop,
                    Width = 250,
                    Text = _defaultTodoPath
                };

                btnBrowse = new Button
                {
                    Text = "...",
                    Left = controlLeft + 260,
                    Top = currentTop,
                    Width = 40,
                    Height = txtTodoFilePath.Height,
                    FlatStyle = FlatStyle.System,
                    UseVisualStyleBackColor = true
                };

                currentTop += rowHeight + 20;

                // Action buttons
                btnSave = new Button
                {
                    Text = "Save",
                    Left = controlLeft,
                    Top = currentTop,
                    Width = 100,
                    Height = 30,
                    UseVisualStyleBackColor = true,
                };

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Left = controlLeft + 110,
                    Top = currentTop,
                    Width = 100,
                    Height = 30,
                    UseVisualStyleBackColor = true,
                    DialogResult = DialogResult.Cancel
                };

                // Add all controls to form
                Controls.AddRange(
                [
                    lblGitlabUrl, txtGitlabUrl!,
                lblGitlabToken, txtGitlabToken!,
                lblExecEverySec, txtExecEverySec!,
                lblTodoFilePath, txtTodoFilePath!, btnBrowse!,
                btnSave!, btnCancel!
                ]);
            }

            private void SetupEventHandlers()
            {
                btnBrowse!.Click += BtnBrowse_Click;
                btnSave!.Click += BtnSave_Click;
            }

            private void SetTabOrder()
            {
                txtGitlabUrl!.TabIndex = 0;
                txtGitlabToken!.TabIndex = 1;
                txtExecEverySec!.TabIndex = 2;
                txtTodoFilePath!.TabIndex = 3;
                btnBrowse!.TabIndex = 4;
                btnSave!.TabIndex = 5;
                btnCancel!.TabIndex = 6;
            }

            private void LoadExistingValues()
            {
                if (!File.Exists(_envPath))
                    return;

                try
                {
                    var lines = File.ReadAllLines(_envPath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || !line.Contains('='))
                            continue;

                        var parts = line.Split('=', 2);
                        if (parts.Length != 2)
                            continue;

                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        switch (key.ToUpperInvariant())
                        {
                            case "GITLAB_URL":
                                txtGitlabUrl.Text = value;
                                break;
                            case "GITLAB_TOKEN":
                                txtGitlabToken.Text = value;
                                break;
                            case "EXEC_EVERY_SEC":
                                txtExecEverySec.Text = value;
                                break;
                            case "TODO_FILE_PATH":
                                txtTodoFilePath.Text = value;
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Warning: Could not load existing configuration:\n{ex.Message}",
                        "Configuration Load Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            private void BtnBrowse_Click(object? sender, EventArgs e)
            {
                using var dlg = new SaveFileDialog
                {
                    Title = "Select or create Todo JSON file",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = Path.GetFileName(txtTodoFilePath.Text),
                    InitialDirectory = GetInitialDirectory()
                };

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtTodoFilePath.Text = dlg.FileName;
                }
            }

            private string GetInitialDirectory()
            {
                var currentPath = txtTodoFilePath!.Text;

                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    var directory = Path.GetDirectoryName(currentPath);
                    if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                        return directory;
                }

                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            private void BtnSave_Click(object? sender, EventArgs e)
            {
                if (!ValidateInputs())
                    return;

                try
                {
                    SaveConfiguration();
                    DotNetEnv.Env.Load(_envPath);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save configuration file:\n{ex.Message}",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            private bool ValidateInputs()
            {
                if (string.IsNullOrWhiteSpace(txtGitlabUrl!.Text))
                {
                    ShowValidationError("Please enter a valid GitLab URL.", txtGitlabUrl);
                    return false;
                }

                if (!IsValidUrl(txtGitlabUrl.Text.Trim()))
                {
                    ShowValidationError("Please enter a valid URL format (e.g., https://gitlab.example.com).", txtGitlabUrl);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtGitlabToken!.Text))
                {
                    ShowValidationError("Please enter your GitLab access token.", txtGitlabToken);
                    return false;
                }

                if (!int.TryParse(txtExecEverySec!.Text, out int interval) || interval < 5)
                {
                    ShowValidationError("Execution interval must be a number greater than or equal to 5 seconds.", txtExecEverySec);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtTodoFilePath!.Text))
                {
                    ShowValidationError("Please specify a path for the Todo file.", txtTodoFilePath);
                    return false;
                }

                var todoDirectory = Path.GetDirectoryName(txtTodoFilePath.Text.Trim());
                if (!string.IsNullOrEmpty(todoDirectory) && !Directory.Exists(todoDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(todoDirectory);
                    }
                    catch (Exception)
                    {
                        ShowValidationError("The specified directory path is invalid or cannot be created.", txtTodoFilePath);
                        return false;
                    }
                }

                return true;
            }

            private static void ShowValidationError(string message, Control focusControl)
            {
                MessageBox.Show(message, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                focusControl?.Focus();
            }

            private static bool IsValidUrl(string url)
            {
                return Uri.TryCreate(url, UriKind.Absolute, out var result)
                       && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            }

            private void SaveConfiguration()
            {
                var envLines = new[]
                {
                    $"GITLAB_URL={txtGitlabUrl!.Text.Trim()}",
                    $"GITLAB_TOKEN={txtGitlabToken!.Text.Trim()}",
                    $"EXEC_EVERY_SEC={txtExecEverySec!.Text.Trim()}",
                    $"TODO_FILE_PATH={txtTodoFilePath!.Text.Trim()}"
                };

                // Ensure directory exists
                var directory = Path.GetDirectoryName(_envPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(_envPath, envLines);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    base.WndProc(ref m);
                    if ((int)m.Result == HTCAPTION)
                    {
                        m.Result = HTCLIENT;
                        return;
                    }
                    return;
                }
                base.WndProc(ref m);
            }
        }
}
