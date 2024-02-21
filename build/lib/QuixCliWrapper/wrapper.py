import subprocess
import sys
import platform
import os
import site
import argparse


def list_files_recursively(directory, indent=0):
    for root, dirs, files in os.walk(directory):
        level = root.replace(directory, '').count(os.sep)
        indent_sub = ' ' * 4 * (level)
        print('{}{}/'.format(indent_sub, os.path.basename(root)))
        subindent = ' ' * 4 * (level + 1)
        for f in files:
            print('{}{}'.format(subindent, f))


def main():

    verbose_mode = False

    # Create a new argument parser
    parser = argparse.ArgumentParser(add_help=False)  # add_help=False prevents argparse from handling -h or --help
    parser.add_argument('--verbose', action='store_true', default=False)

    # Parse only known arguments
    # Unknown arguments are returned separately and can be passed through to the C# executable
    args, unknown_args = parser.parse_known_args()

    if args.verbose:
        print('Verbose mode is on.')
        verbose_mode = True

    # Convert known arguments back into a list
    known_args = []
    if args.verbose:
        known_args.append('--verbose')

    if verbose_mode:
        print("RUNNING MAIN...")
        print(site.getsitepackages())
        list_files_recursively(".")

    # Determine the current platform
    current_platform = platform.system().lower()
    if verbose_mode:
        print("PLATFORM: " + current_platform)
    # Determine the path to the C# executable based on the platform
    script_dir = os.path.dirname(os.path.realpath(__file__))
    
    if verbose_mode:
        print("script_dir:" + script_dir)

    if current_platform == 'windows':
        exe_path = os.path.join(script_dir, './binaries/win10/cs-cli.exe')
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

    if verbose_mode:
        print(exe_path)
    
    if verbose_mode:
        print("Checking if dotnet is available:")
        print(subprocess.run(['which', 'dotnet'], capture_output=True, text=True))
        print("Checking if the DLL file exists:")
        print(subprocess.run(['ls', '-l', exe_path], capture_output=True, text=True))

    # Call the C# executable, passing both known and unknown command-line arguments
    if current_platform == 'windows':
        subprocess.run([exe_path] + known_args + unknown_args)
    else:
        subprocess.run(['dotnet', exe_path] + known_args + unknown_args)

if __name__ == '__main__':
    main()