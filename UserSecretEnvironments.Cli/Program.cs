using Cocona;
using UserSecretEnvironments.Core;

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
        UserSecretEnvironmentManager.Initialize();
    });

app.AddCommand("use", ([Argument] string environmentName) => 
    {
        Console.WriteLine($"switch to this {environmentName}");
        UserSecretEnvironmentManager.UseEnvironment(environmentName);
    });

app.AddCommand("list", () => 
    {
        Console.WriteLine("List all environment groups for a given project file.");

        foreach (var filePath in UserSecretEnvironmentManager.GetEnvironments())
        {
            Console.WriteLine(filePath);
        }
    });

app.AddCommand("edit", ([Argument] string environmentName) => 
    {
        Console.WriteLine($"open secrets file to edit {environmentName}");
        UserSecretEnvironmentManager.EditEnvironment(environmentName);
    });

app.AddCommand("menu", () => 
    {
        Console.WriteLine("┌ User Secret Environment Manager");
        Console.WriteLine("| Use the arrow up and down keys to navigate. Enter to make a selection.");
        Console.WriteLine("| Enter to make a selection.");
        Console.WriteLine("| ");
        Console.WriteLine("| Menu:");

        var menuIndex = 0;

        (int left, int top) = Console.GetCursorPosition();

        while (true)
        {
            Console.SetCursorPosition( left, top );

            for (int i = 0; i < menuOptions.Length; i++)
            {
                var isCurrentSelection = menuIndex == i;
                var visualSelection = menuIndex == i ? ">" : " ";
                Console.WriteLine($"| {visualSelection} {menuOptions[i]}");
            }

            Console.WriteLine($"└");

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
                    Console.WriteLine($"User Chose to {menuOptions[menuIndex]}");
                    //if (menuIndex == menuOptions.Length - 1)
                        Environment.Exit(0);
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
            }

        }
    });

app.Run();