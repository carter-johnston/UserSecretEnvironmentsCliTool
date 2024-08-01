using Cocona;
using Spectre.Console;
using UserSecretEnvironments.Core;

namespace UserSecretEnvironments.Cli.Commands;

public class EnvironmentManagerCommands
{
    [Command("init", Description = "Creates secret.json files from a list of environment names.")]
    public void Initialize(string[]? environmentNames)
    {
        var environmentNamesSpecified = environmentNames != null 
            && environmentNames.Length > 0;

        var result = environmentNamesSpecified
            ? EnvironmentManager.InitializeEnvironments(environmentNames!)
            : EnvironmentManager.InitializeDefaultEnvironments();

        if (!result.OperationResult.Equals(OperationStatus.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        foreach (var userSecretEnvironment in result.UserSecretEnvironments)
        {
            AnsiConsole.MarkupLine($"[green]{userSecretEnvironment.Name} {userSecretEnvironment.FilePath}[/]");
        }
    }

    //[Command("menu")]
    [Ignore]
    public void RunInteractiveMenu() {
        AnsiConsole.MarkupLine("[yellow]This feature is a WIP. You may experience non-functional features or bugs.[/]");
        EnvironmentManagerMenu.Run(); 
    }

    [Command("use")]
    public void SelectNewEnvironment([Argument] string? environmentName)
    {
        var result = EnvironmentManager.UseEnvironment(environmentName);

        if (!result.OperationResult.Equals(OperationStatus.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"User Secret Environment switched to [white][[[green]{environmentName ?? "default".ToUpper()}[/]]][/]");
    }

    [Command("list")]
    public static void ListAllEnvironments()
    {
        var result = EnvironmentManager.GetEnvironments();

        if (!result.OperationResult.Equals(OperationStatus.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        Console.WriteLine();
        foreach (var environmentDirectory in result.UserSecretEnvironments)
        {
            AnsiConsole.MarkupLine($"* [green]{environmentDirectory.Name} {environmentDirectory.FilePath}[/]");
        }

        Console.WriteLine();
    }

    [Command("edit")]
    public static void EditUserSecretsFile([Argument] string environmentName)
    {
        AnsiConsole.MarkupLine($"Opening [white][[[green]{environmentName}[/]]][/] secrets file to edit.");

        var result = EnvironmentManager.EditEnvironment(environmentName);

        if (!result.Equals(OperationStatus.Success))
        {
            AnsiConsole.MarkupLine("[yellow]Unable to find a usersecrets file to open.[/]");
        }
    }
}
