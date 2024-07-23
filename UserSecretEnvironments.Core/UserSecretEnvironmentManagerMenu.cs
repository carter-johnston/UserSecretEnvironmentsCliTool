using Spectre.Console;

namespace UserSecretEnvironments.Core;

public static class UserSecretEnvironmentManagerMenu
{
   private enum MenuSelection
    {
        List,
        Use,
        Initialize,
        Edit,
        Show,
        Exit,
    }

    private static readonly (string, MenuSelection)[] menuOptions = [
        ("Show all environments.", MenuSelection.List),
        ("Show selected environment.", MenuSelection.Show),
        ("Select an environment.", MenuSelection.Use),
        ("Open editor for selected environment.", MenuSelection.Edit),
        ("Exit.", MenuSelection.Exit)
    ];

    public static void Entry()
    {
        var menuIndex = 0;

        while (true)
        {
            AnsiConsole.MarkupLine("[green]┌ User Secret Environment Manager[/]");
            AnsiConsole.MarkupLine("[green]|[/] Use the arrow up and down keys to navigate. Enter to make a selection.");
            AnsiConsole.MarkupLine("[green]|[/] Enter to make a selection.");
            AnsiConsole.MarkupLine("[green]|[/]");
            AnsiConsole.MarkupLine("[green]|[/] [bold]Menu:[/]");

            for (int i = 0; i < menuOptions.Length; i++)
            {
                var visualSelection = menuIndex == i ? ">" : " ";
                AnsiConsole.MarkupLine($"[green]|[/] {visualSelection} {menuOptions[i].Item1}");
            }

            AnsiConsole.MarkupLine($"[green]└[/]");

            var keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.DownArrow:
                    if (menuIndex < menuOptions.Length - 1)
                        menuIndex++;
                    break;
                case ConsoleKey.UpArrow:
                    if (menuIndex > 0)
                        menuIndex--;
                    break;
                case ConsoleKey.Enter:
                    RunMenuSelection(menuOptions[menuIndex].Item2);
                    Console.Write("Press Any Key to proceed to Menu...");
                    Console.ReadKey();
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
            }

            Console.Clear();
        }
    }

    private static void RunMenuSelection(MenuSelection action)
    {
        switch (action)
        {
            case MenuSelection.Initialize:
                Initialize();
                break;
            case MenuSelection.Use:
                Use();
                break;
            case MenuSelection.Edit:
                Edit();
                break;
            case MenuSelection.List:
                List();
                break;
            case MenuSelection.Show:
                Show();
                break;
        }
    }

    private static void Show()
    {
        // Get currently selected User Secret
        // Display to User
        throw new NotImplementedException();
    }

    private static void Edit()
    {
        // Get list of environments
        // Display a menu to select an environment
        // User selects an env
        UserSecretEnvironmentManager.EditEnvironment("Qa");
    }

    private static void List()
    {
        // Get list of environments
        // Display to User
    }

    private static void Use()
    {
        // Get list of environments
        // Display a menu to select an environment
        // User Selects env
        var result = UserSecretEnvironmentManager.UseEnvironment("Qa");
    }

    private static void Initialize()
    {
        // Let user input environment names
        UserSecretEnvironmentManager.InitializeDefaultEnvironments();
    }
}
