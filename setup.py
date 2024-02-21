from setuptools import setup, find_packages

setup(
    name='QuixCLI-test',
    version='0.1.1',
    packages=find_packages(),
    include_package_data=True,
    package_data={
        'QuixCliWrapper': [
            'binaries/win10/*',
            'binaries/osx/*',
            'binaries/osx-arm/*',
            'binaries/linux/*'
        ],
    },
    install_requires=[
        # Add any Python dependencies here
        # For example: 'requests'
    ],
    entry_points={
        'console_scripts': [
            'QuixCliWrapper = QuixCliWrapper.wrapper:main',
        ],
    },
)