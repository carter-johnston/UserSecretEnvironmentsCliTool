using Cocona;
using Spectre.Console;
using UserSecretEnvironments.Core;
using static UserSecretEnvironments.Core.UserSecretEnvironmentManager;

var app = CoconaApp.Create();

var menuOptions = new[] {
    "Show all environments.",
    "Show selected environment.",
    "Select an environment.",
    "Open editor for selected environment.",
    "Exit."
};

app.AddCommand("init", (string[]? environmentNames) => 
    {
        Console.WriteLine($"creates default empty environments.");

        var environmentNamesSpecified = environmentNames != null && environmentNames.Length > 0;

        var result = environmentNamesSpecified 
            ? InitializeEnvironments(environmentNames!)
            : InitializeDefaultEnvironments();
    });

app.AddCommand("use", ([Argument] string environmentName) => 
    {
        var result = UseEnvironment(environmentName);

        if (result.Equals(UserSecretEnvironmentOperationResult.Success))
            AnsiConsole.MarkupLine($"User Secret Environment switched to [white][[[green]{environmentName.ToUpper()}[/]]][/]");
        else
            AnsiConsole.MarkupLine($"[yellow]Something Went Wrong![/]");
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

app.AddCommand("menu", () => UserSecretEnvironmentManagerMenu.Entry());

app.Run();