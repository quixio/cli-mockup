import subprocess
import sys
import platform
import os
import site

def list_files_recursively(directory, indent=0):
    for root, dirs, files in os.walk(directory):
        level = root.replace(directory, '').count(os.sep)
        indent_sub = ' ' * 4 * (level)
        print('{}{}/'.format(indent_sub, os.path.basename(root)))
        subindent = ' ' * 4 * (level + 1)
        for f in files:
            print('{}{}'.format(subindent, f))


def main():
    # print("RUNNING MAIN...")
    print(site.getsitepackages())

    # Usage
    #list_files_recursively(".")

    # Determine the current platform
    current_platform = platform.system().lower()
    # print("PLATFORM: " + current_platform)
    # Determine the path to the C# executable based on the platform
    script_dir = os.path.dirname(os.path.realpath(__file__))
    # print("script_dir:" + script_dir)
    if current_platform == 'windows':
        exe_path = os.path.join(script_dir, './binaries/win10/cs-cli.exe')
        print(exe_path)
    elif current_platform == 'darwin':
        if platform.processor() == 'arm':
            exe_path = os.path.join(script_dir, 'binaries/osx-arm/cs-cli.dll')
        else:
            exe_path = os.path.join(script_dir, 'binaries/osx/cs-cli.dll')
    elif current_platform == 'linux':
        exe_path = os.path.join(script_dir, 'binaries/linux/cs-cli.dll')
    else:
        print(f'Unsupported platform: {current_platform}')
        sys.exit(1)

    print(exe_path)
    
    # Debug print lines
    # print("Checking if dotnet is available:")
    # print(subprocess.run(['which', 'dotnet'], capture_output=True, text=True))
    # print("Checking if the DLL file exists:")
    # print(subprocess.run(['ls', '-l', exe_path], capture_output=True, text=True))

    # Call the C# executable, passing any command-line arguments
    if current_platform == 'windows':
        subprocess.run([exe_path] + sys.argv[1:])
    else:
        subprocess.run(['dotnet', exe_path] + sys.argv[1:])

if __name__ == '__main__':
    main()