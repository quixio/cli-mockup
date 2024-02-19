using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices; // Add this line
using Confluent.Kafka;
using System.Text.Json;


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

        if (command == "begin" || command == "init")
        {
            Init();
        }
        else if (command == "create")
        {
            static void PrintLocalHelp(){
                Console.WriteLine("Usage:");
                Console.WriteLine("  create [source|s] [name] - creates a new source app using a sample from the library");
                Console.WriteLine("  create [transformation|t] [name] - creates a new transformation app using a sample from the library");
                Console.WriteLine("  create [destination|sink|d] [name] - creates a new destination app using a sample from the library");
            }

            if (args.Length < 3){
                PrintLocalHelp();
                return;
            }

            var what = args[1];
            if (what == "transformation" || what == "t"){
                var appName = args[2];
                WrapQuixCreateTransformation(appName);
            }
            else if(what == "destination" || what == "sink" || what == "d"){
                var appName = args[2];
                WrapQuixCreateDestination(appName);
            }
            else if(what == "source" || what == "s"){
                var appName = args[2];
                WrapQuixCreateSource(appName);
            }
            else{
                PrintLocalHelp();
            }
        }
        else if (command == "activate")
        {
            Activate();
        }
        else if (command == "topics")
        {
            //quix topics ingest demo-stream
            var action = args[1];
            if (action == "ingest")
            {
                var topic = args[2];

                DemoData(topic);
            }
            else{
                Console.WriteLine("Usage:");
                Console.WriteLine("  ingest [topic-name] - ingest data into a topic of your choosing");
            }
        }
        else if (command == "local")
        {
            if (args.Length > 1){
                var action = args[1];

                if (action == "deploy")
                {
                    var dockerInfo = RunCommand("run cli", output: true);
                }
                else{
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  local deploy - add your app to the quix.yaml");
                }
            }
            else{
                Console.WriteLine("Usage:");
                Console.WriteLine("  local deploy - add your app to the quix.yaml");
            }
        }
        else if( command == "broker"){
            StartBroker();
        }
        else
        {
            Console.WriteLine("Unknown command. Use 'init' to initialize the project or 'create' to create a new app.");
            PrintHelp();
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  begin - Initialize the project");
        Console.WriteLine("  create [transformation|destination|source] <app_name> - Create a new app");
        Console.WriteLine("  topics ingest [topic-name] - Ingest data");
        Console.WriteLine("  local deploy - add your app to the quix.yaml");
    }

    static void StartBroker(){

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


        Console.WriteLine("Which broker would you like to use?");
        string[] options = {"Redpanda", "Aiven", "Confluent Kafka"};
        var chosenOption = SelectOption(options);

        if (chosenOption == "Redpanda")
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
    }

    static void DownloadQuixCliForWindows(){
        string version = "0.0.1-20240215.4";
        string githubUrl = "https://github.com";
        string owner = "quixio";
        string repoName = "quix-cli";
        string target = "win-x64";
        string binDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "bin");

        string resourceUri = string.IsNullOrEmpty(version)
            ? $"{githubUrl}/{owner}/{repoName}/releases/latest/download/{target}.zip"
            : $"{githubUrl}/{owner}/{repoName}/releases/download/{version}/{target}.zip";

        string downloadedZip = Path.Combine(binDir, $"{target}.zip");

        Directory.CreateDirectory(binDir);

        Console.WriteLine($"[1/5] Detected 'x64' architecture");
        Console.WriteLine($"[2/5] Downloading '{target}.zip' to '{binDir}'");

        using (WebClient client = new WebClient())
        {
            client.DownloadFile(resourceUri, downloadedZip);
        }

        Console.WriteLine($"[3/5] Decompressing '{target}.zip' in '{binDir}'");

        ZipFile.ExtractToDirectory(downloadedZip, binDir, true);

        Console.WriteLine($"[4/5] Cleaning '{downloadedZip}'");

        File.Delete(downloadedZip);

        Console.WriteLine("");
        Console.WriteLine("Quix CLI was installed successfully");
        Console.WriteLine("Run 'quix --help' to get started");
    }


    static void Init()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //var installCommand = "$quixCliInstall = (iwr https://github.com/quixio/quix-cli/raw/main/install.ps1 -useb).Content; $version=\"0.0.1-20240215.4\"; iex \"$quixCliInstall\"";
            //RunCommand(installCommand, fileName: "powershell.exe");

            DownloadQuixCliForWindows();

        }
        else
        {
            RunCommand("curl -fsSL https://github.com/quixio/quix-cli/raw/main/install.sh | sudo bash -s -- -v=0.0.1-20240215.4");
        }

        // Create a directory named .quix
        Directory.CreateDirectory(".quix");
        //Console.WriteLine("Directory .quix created.");

        // clone a repo to here
        Console.Write("Please enter the GitHub repo URL (or blank to init a blank repo here): ");
        string repoUrl = Console.ReadLine();
        if(repoUrl == ""){
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");  // Debug print statement

            RunCommand($"init", fileName: "git", verbatimCommand: true);
        }
        else{
            RunCommand($"clone {repoUrl}", fileName: "git", verbatimCommand: true);
        }
        // run cli init
        RunCommand("quix local init");


        // DownloadInitFiles();

        // //CreateVenv();
        
        // // Check if Docker Desktop is running
        // var dockerInfo = RunCommand("docker info", output: false);
        // if (dockerInfo.ExitCode == 0)
        // {
        //     Console.WriteLine("Docker Desktop is running.");
        // }
        // else
        // {
        //     Console.WriteLine("Docker Desktop is not running. Please start Docker Desktop and try again.");
        //     return;
        // }

        // // Ask the user if they want to start a Redpanda instance
        // Console.Write("Would you like to start a Redpanda instance in Docker? (yes/no) ");
        // var startRedpanda = Console.ReadLine();
        // if (startRedpanda.ToLower() == "yes" || startRedpanda.ToLower() == "y")
        // {
        //     // Download the Docker Compose configuration
        //     var dockerComposeUrl = "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/docker-compose.yml";
        //     using (var client = new WebClient())
        //     {
        //         client.DownloadFile(dockerComposeUrl, "docker-compose.yml");
        //     }
        //     Console.WriteLine("Docker Compose configuration downloaded.");

        //     // Run `docker-compose up`
        //     RunCommand("docker-compose up", redirectOutput: true, waitForExit: false);
        // }

        
    }


    static void DownloadInitFiles(){
        
        // Download main.py, requirements.txt, and app.yaml files from the URLs
        var files = new[] { "quix.yaml" };
        var urls = new[]
        {
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/quix.yaml",
        };

        using (var client = new WebClient())
        {
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var url = urls[i];
                //var path = Path.Combine(appName, file);

                client.DownloadFile(url, file);
                Console.WriteLine($"File {file} downloaded.");
            }
        }
    }

    static void CreateDotEnv(string appName){
        // Create an empty .env file
        var envFilePath = Path.Combine(appName, ".env");
        using (var stream = File.Create(envFilePath))
        {
            // Close the FileStream immediately to create an empty file
        }
        Console.WriteLine(".env file created.");
    }

    static void AppCreated(string appType, string appName){
        Console.WriteLine($"{appType} created in {appName}");
        Console.WriteLine($"Change to the {appName} directory, create a virtual environment and activate it.");
        var activateCommandText = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "..\\venv\\Scripts\\activate" : "source ../venv/bin/activate";
        var python = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";
        Console.WriteLine($"hint. Use:");

        Console.ForegroundColor = ConsoleColor.Green; // Change the text color to green
        Console.BackgroundColor = ConsoleColor.Black; // Change the background color to black
        Console.WriteLine($"{python} -m venv venv\n.{activateCommandText}");
        Console.ResetColor(); // Reset the color to the default


    }

    static void WrapQuixCreateTransformation(string appName){
        var libraryItemId = "a00cd311-988d-4966-8eb5-eb50ad71dedd";
        RunCommand(fileName: "quix", command: $"local applications add {appName} -li {libraryItemId}", verbatimCommand: true);
        AppCreated("Transformation", appName);
    }

    static void WrapQuixCreateSource(string appName){
        var libraryItemId = "";
        RunCommand(fileName: "quix", command: $"local applications add {appName} -li {libraryItemId}", verbatimCommand: true);
        AppCreated("Source", appName);
    }
    
    static void WrapQuixCreateDestination(string appName){
        var libraryItemId = "";
        RunCommand(fileName: "quix", command: $"local applications add {appName} -li {libraryItemId}", verbatimCommand: true);
        AppCreated("Destination/Sink", appName);
    }

    static void Create(string appName)
    {
        // Create a directory with the name of the app
        Directory.CreateDirectory(appName);
        Console.WriteLine($"Directory {appName} created.");

        // Download main.py, requirements.txt, and app.yaml files from the URLs
        var files = new[] { "main.py", "requirements.txt", "app.yaml", "dockerfile", ".gitignore" };
        var urls = new[]
        {
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/main.py",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/requirements.txt",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/app.yaml",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/name%20counter/dockerfile",
            "https://raw.githubusercontent.com/SteveRosam/cli-code/tutorial/.gitignore"            
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

        CreateDotEnv(appName);
    }

    static void CreateVenv(){
        
        Console.WriteLine("Creating Virtual environment...");

        // Get the correct Python command
        var pythonCommand = GetPythonCommand();

        if (!Directory.Exists($"venv"))
        {
            // Create a virtual environment in the new app folder
            RunCommand($"{pythonCommand} -m venv venv");
            Console.WriteLine("Virtual environment created.");
        }
    }
    static void Activate()
    {
        Console.WriteLine("Creating virtual environment");
        CreateVenv();
        Console.WriteLine("Activating virtual environment");

        // var scriptFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "activate.bat" : "activate.sh";
        // var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFileName);
        // var chmodCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : $"chmod +x {scriptPath}";
        // RunCommand(chmodCommand);
        // RunCommand($"{scriptPath}", waitForExit: true);

        Console.WriteLine("Creating .env file");

        RunCommand("quix local sync env-variables", waitForExit: true);
        Console.WriteLine("synced");

        //RunCommand($"{activateCommand}");
        //Console.WriteLine(activateCommand);

        //var pythonCommand = GetPythonCommand();
        //var installCommand = $"{pythonCommand} -m pip install -r requirements.txt";
        //RunCommand($"{installCommand}");


        // Console.WriteLine("Virtual environment dependencies installed.");
        var helpText = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "..\\venv\\Scripts\\activate" : "source ../venv/bin/activate";

        Console.WriteLine("Please copy and paste the following command into your shell to execute it:");
        Console.WriteLine(helpText);
        //Console.WriteLine("Then run:");

        //var helpText2 = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "" : "python3 -m ";
        //Console.WriteLine($"{helpText2}pip install -r requirements.txt");

    }

    static void DemoData(string topic){
        var server = "localhost:19092";

        var dataList = new List<Data>
        {
            new Data { Timestamp = 1687516100000000000, Number = 1, Speed = 120, Sector = 1},
            new Data { Timestamp = 1687516095000000000, Number = 2, Speed = 244, Sector = 2},
            new Data { Timestamp = 1687516095000000000, Number = 3, Speed = 68, Sector = 3},
            new Data { Timestamp = 1687516095000000000, Number = 4, Speed = 77, Sector = 4},
            new Data { Timestamp = 1687516095000000000, Number = 5, Speed = 285, Sector = 1}
        };

        foreach(var data in dataList)
            WriteToKafkaTopic(server, topic, data).GetAwaiter().GetResult();

        Console.WriteLine($"Data delivered");
    }

    public class Data
    {
        public long Timestamp { get; set; }
        public int Number { get; set; }
        public float Speed { get; set; }
        public int Sector { get; set; }

    }
    public static async Task WriteToKafkaTopic(string bootstrapServers, string topic, Data data)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            var message = JsonSerializer.Serialize(data);

            try
            {
                var dr = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }

    static Process RunCommand(string command, bool output = true, bool redirectOutput = true, bool waitForExit = true, string fileName = null, bool verbatimCommand=false)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash"),
                Arguments = verbatimCommand ? command : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/c {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = redirectOutput,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        // Console.WriteLine($"Running command: {process.StartInfo.FileName} {process.StartInfo.Arguments}");  // Debug print statement


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
            // Console.WriteLine($"Command exited with code: {process.ExitCode}");  // Debug print statement

        }

        return process;
    }

   static string SelectOption(string[] options)
    {
        int currentSelection = 0;
        ConsoleKeyInfo key;

        // Write the options to the console
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine(options[i]);
        }

        do
        {
            // Move the cursor to the start of the options
            Console.SetCursorPosition(0, Console.CursorTop - options.Length);

            // Write the options to the console, highlighting the current selection
            for (int i = 0; i < options.Length; i++)
            {
                if (i == currentSelection)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(options[i].PadRight(Console.WindowWidth - 1));  // PadRight to overwrite the entire line

                Console.ResetColor();
            }

            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow)
            {
                currentSelection = (currentSelection == 0) ? options.Length - 1 : currentSelection - 1;
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                currentSelection = (currentSelection == options.Length - 1) ? 0 : currentSelection + 1;
            }
        }
        while (key.Key != ConsoleKey.Enter);

        return options[currentSelection];
    }
}