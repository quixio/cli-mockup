from setuptools import setup, find_packages

setup(
    name='QuixCLI-test',
    version='0.0.6',
    packages=find_packages(),
    include_package_data=True,
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