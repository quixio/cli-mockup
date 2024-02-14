using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices; // Add this line


class Program
{

    static string GetPythonCommand()
    {
        var pythonCommand = "python";
        var checkCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"where {pythonCommand}" : $"command -v {pythonCommand}";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {checkCommand}" : $"-c \"{checkCommand}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            pythonCommand = "python3";
        }

        Console.WriteLine($"Python command: {pythonCommand}");  // Debug print statement

        return pythonCommand;
    }

    static void Main(string[] args)
    {
        // Check if no arguments were passed
        if (args.Length == 0)
        {
            PrintHelp();
            return;
        }

        // Parse command-line arguments
        var command = args[0];

        if (command == "init")
        {
            Init();
        }
        else if (command == "create")
        {
            var appName = args[1];
            Create(appName);
        }
        else
        {
            Console.WriteLine("Unknown command. Use 'init' to initialize the project or 'create' to create a new app.");
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  init - Initialize the project");
        Console.WriteLine("  create <app_name> - Create a new app");
    }

    static void Init()
    {
        // Check if Docker Desktop is running
        var dockerInfo = RunCommand("docker info", output: false);
        if (dockerInfo.ExitCode == 0)
        {
            Console.WriteLine("Docker Desktop is running.");
        }
        else
        {
            Console.WriteLine("Docker Desktop is not running. Please start Docker Desktop and try again.");
            return;
        }

        // Ask the user if they want to start a Redpanda instance
        Console.Write("Would you like to start a Redpanda instance in Docker? (yes/no) ");
        var startRedpanda = Console.ReadLine();
        if (startRedpanda.ToLower() == "yes" || startRedpanda.ToLower() == "y")
        {
            // Download the Docker Compose configuration
            var dockerComposeUrl = "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/docker-compose.yml";
            using (var client = new WebClient())
            {
                client.DownloadFile(dockerComposeUrl, "docker-compose.yml");
            }
            Console.WriteLine("Docker Compose configuration downloaded.");

            // Run `docker-compose up`
            RunCommand("docker-compose up", redirectOutput: true, waitForExit: false);
        }

        // Create a directory named .quix
        Directory.CreateDirectory(".quix");
        Console.WriteLine("Directory .quix created.");

        // TODO: Download the quix.yaml file
    }


    static void Create(string appName)
    {

        // Get the correct Python command
        var pythonCommand = GetPythonCommand();

        // Create a directory with the name of the app
        Directory.CreateDirectory(appName);
        Console.WriteLine($"Directory {appName} created.");

        // Download main.py, requirements.txt, and app.yaml files from the URLs
        var files = new[] { "main.py", "requirements.txt", "app.yaml" };
        var urls = new[]
        {
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/main.py",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/requirements.txt",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/app.yaml",
        };

        using (var client = new WebClient())
        {
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var url = urls[i];
                var path = Path.Combine(appName, file);

                client.DownloadFile(url, path);
                Console.WriteLine($"File {file} downloaded.");
            }
        }

        // Create a virtual environment in the new app folder
        RunCommand($"{pythonCommand} -m venv {appName}/venv");
        Console.WriteLine("Virtual environment created.");


        // Activate the virtual environment
        var activateCommand = $"source {appName}/venv/bin/activate";
        RunCommand(activateCommand);
        Console.WriteLine("Virtual environment activated.");

        // Activate the virtual environment and install the dependencies
        // var activateCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
        //     $"{appName}\\venv\\Scripts\\activate" : $"source {appName}/venv/bin/activate";
        // var installCommand = $"{pythonCommand} -m pip install -r {appName}/requirements.txt";
        // RunCommand($"{activateCommand} && {installCommand}");
        // Console.WriteLine("Virtual environment activated and dependencies installed.");

        var activateVenvCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
            "activate.bat" : "activate.sh";
        RunCommand($"{activateVenvCommand} {appName}", waitForExit: false);
    }

    static Process RunCommand(string command, bool output = true, bool redirectOutput = true, bool waitForExit = true)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = redirectOutput,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();

        if (output && redirectOutput)
        {
            int lineCount = 0;
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
                lineCount++;
                if (lineCount >= 10)
                {
                    break;
                }
            }
        }

        if (waitForExit)
        {
            process.WaitForExit();
        }

        return process;
    }
}