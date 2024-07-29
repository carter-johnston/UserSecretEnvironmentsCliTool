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

        if (!result.OperationResult.Equals(OperationType.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        Console.WriteLine("New user secret environments created:");

        foreach (var createdEnvironmentDirectories in result.EnvironmentDirectories)
        {
            AnsiConsole.MarkupLine($"[green]{createdEnvironmentDirectories}[/]");
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

        if (!result.OperationResult.Equals(OperationType.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"User Secret Environment switched to [white][[[green]{environmentName ?? "default".ToUpper()}[/]]][/]");
    }

    [Command("list")]
    public void ListAllEnvironmentsByProject()
    {
        var result = EnvironmentManager.GetEnvironments();

        if (!result.OperationResult.Equals(OperationType.Success))
        {
            AnsiConsole.MarkupLine($"[yellow]{result.ErrorMessage}[/]");
            return;
        }

        Console.WriteLine();
        
        foreach (var environmentDirectory in result.UserSecretEnvironments)
        {
            AnsiConsole.MarkupLine($"[green]{environmentDirectory}[/]");
        }
    }

    [Command("edit")]
    public void EditUserSecretsFile([Argument] string environmentName)
    {
        Console.WriteLine($"Opening [white][[[green]{environmentName}[/]]][/] secrets file to edit.");

        var result = EnvironmentManager.EditEnvironment(environmentName);

        if (!result.Equals(OperationType.Success))
        {
            AnsiConsole.MarkupLine("[yellow]Unable to find a usersecrets file to open.[/]");
        }
    }
}
