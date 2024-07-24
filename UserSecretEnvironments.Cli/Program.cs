using Cocona;
using Spectre.Console;
using System.Diagnostics;
using UserSecretEnvironments.Core;
using static UserSecretEnvironments.Core.UserSecretEnvironmentManager;

var app = CoconaApp.Create();

app.AddCommand("init", (string[]? environmentNames) => 
    {
        Console.WriteLine($"creates default empty environments.");

        var environmentNamesSpecified = environmentNames != null && environmentNames.Length > 0;

        var result = environmentNamesSpecified 
            ? InitializeEnvironments(environmentNames!)
            : InitializeDefaultEnvironments();
    });

app.AddCommand("use", ([Argument] string? environmentName) => 
    {
        var result = UseEnvironment(environmentName);

        var userMessage = result.OperationResult switch
        {
            UserSecretEnvironmentOperationResult.Success
                =>  $"User Secret Environment switched to [white][[[green]{environmentName ?? "default".ToUpper()}[/]]][/]",

            UserSecretEnvironmentOperationResult.UnableToFindUserSecrets
                =>  $"[yellow]User Secrets do not seem to be configured correctly. " +
                    $" Make sure you are running this command within a project folder (containing a .csproj file)." +
                    $" Also Ensure your project has User Secrets set up (i.e. dotnet user-secrets init)[/]",

            UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory
                =>  $"[yellow]secrets.json was not found for the selected for the environment provided." +
                    " Ensure the UserSecrets folder was created correctly" +
                    $" (i.e. {Process.GetCurrentProcess().ProcessName} init {environmentName})[/]",

            _ => $"[yellow]Something went wrong and the User Secret Environment was not able to be properly set.[/]",
        };

        AnsiConsole.MarkupLine(userMessage);
    });



app.AddCommand("list", () => 
    {
        Console.WriteLine("List all environment groups for a given project file.");

        var result = GetEnvironments();

        foreach (var environmentName in result.EnvironmentNames)
        {
            Console.WriteLine(environmentName);
        }
    });

app.AddCommand("edit", ([Argument] string environmentName) => 
    {
        Console.WriteLine($"open secrets file to edit {environmentName}");

        var result = EditEnvironment(environmentName);
    });

app.AddCommand("test-directory", () =>
    {
        Console.WriteLine("Current Directory: " + Environment.CurrentDirectory);

        string binDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
        Console.WriteLine("bin directory: " + binDirectory);

        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
        Console.WriteLine("bin directory: " + projectDirectory);
    })
    .WithMetadata(new HiddenAttribute());

app.AddCommand("test-appdomain", () =>
    {
        Console.WriteLine(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]));
        Console.WriteLine(Process.GetCurrentProcess().ProcessName);
    })
    .WithMetadata(new HiddenAttribute());

app.AddCommand("menu", () => UserSecretEnvironmentManagerMenu.Entry());

app.Run();