import subprocess
import sys
import platform
import os


def list_files_recursively(directory, indent=0):
    for root, dirs, files in os.walk(directory):
        level = root.replace(directory, '').count(os.sep)
        indent_sub = ' ' * 4 * (level)
        print('{}{}/'.format(indent_sub, os.path.basename(root)))
        subindent = ' ' * 4 * (level + 1)
        for f in files:
            print('{}{}'.format(subindent, f))


def main():
    print("RUNNING MAIN...")

    # Usage
    list_files_recursively(".")

    # Determine the current platform
    current_platform = platform.system().lower()

    # Determine the path to the C# executable based on the platform
    if current_platform == 'windows':
        exe_path = './bin/Debug/net7.0/win10-x64/cs-cli.exe'
    elif current_platform == 'darwin':
        if platform.processor() == 'arm':
            exe_path = './bin/Debug/net7.0/osx-arm64/executable'
        else:
            exe_path = './bin/Debug/net7.0/osx-x64/executable'
    elif current_platform == 'linux':
        exe_path = './bin/Debug/net7.0/linux-x64/executable'
    else:
        print(f'Unsupported platform: {current_platform}')
        sys.exit(1)

    # Call the C# executable, passing any command-line arguments
    subprocess.run([exe_path] + sys.argv[1:])

if __name__ == '__main__':
    main()