# GLotifi

## Usage
1. Download the latest version from the releases
2. Unzip it
3. Create a `.env` file inside the directory where the `.exe` lives
Example `.env` file:
```.env
GITLAB_URL=https://gitlab.example.com
GITLAB_TOKEN=<my-private-auth-token>
# TODO_FILE_PATH=/some/path/to/todo.json # Optional
EXEC_EVERY_SEC=30
```
4. Start the `.exe`

Now you will find the GLotifi icon in the taskbar menu at the bottom right corner of the screen.

## Enable Autostart
If you right click on the GLotifi icon, you can select `Autostart`.
By clicking on it, you can toggle wether you want to run GLotifi at startup or not.
This is indicated by the checkmark.

<!--
## Installation
> [!NOTE]
> You have to got .NET 9 installed on your PC

1. Open a terminal window and clone the Repo.
```bash
git clone https://github.com/lNaymonl/GLotifi.git
```
2. Create a file called `.env` in the root folder of the repo.
Example .env file:
```env
GITLAB_URL=https://gitlab.example.com
GITLAB_TOKEN=<my-private-auth-token>
TODO_FILE_PATH=/some/path/to/todo.json # Optional
EXEC_EVERY_SEC=30
```
3. Navigate to the repo Folder.
```bash
cd /path/where/you/cloned/the/repo
```
4. Build the application.
```bash
dotnet publish -c Release -o build
```

### Run it in a console
1. Open a terminal
2. Start the application
```bash
./path/to/build/glotifi.exe
```

### Run the app as a service which starts everytime you login
1. Press `Ctrl`+`S`
2. Type `Task Schedular` and open it
3. Create Basic Task on the right-top corner.
4. Give it a good name as well as a desciption. Click `Next >`.
5. Set `Trigger` to `When I log on`. Click `Next >`.
6. Leave the `Action` on its default value. Click `Next >`.
7. Not Click `Browse` and navigate to the location where the executable for `GLotifi` is. Select it. Click `Next >`.
8. Double check that you entered everything correctly. Then click on `Finish`.
Now GLotifi should start everytime you login to your pc.
--!>
