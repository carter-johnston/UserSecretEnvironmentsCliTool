using Cocona;
using System.Diagnostics;
using UserSecretEnvironments.Cli.Commands;

var builder = CoconaApp.CreateBuilder();
var app = builder.Build();

app.AddCommands<EnvironmentManagerCommands>();

app.AddCommand("test-directory", () =>
    {
        string binDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
        
        Console.WriteLine("current directory: " + Environment.CurrentDirectory);
        Console.WriteLine("bin directory: " + binDirectory);
        Console.WriteLine("project directory: " + projectDirectory);
    }
)
.WithMetadata(new HiddenAttribute());

app.AddCommand("test-appdomain", () =>
    {
        Console.WriteLine("FileName w/o extension" + Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]));
        Console.WriteLine("Process Name" + Process.GetCurrentProcess().ProcessName);
    }
)
.WithMetadata(new HiddenAttribute());

app.Run();