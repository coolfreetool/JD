# JD optimization tool
C# library for optimization

## Setup
Tested with Ubuntu 20.04
~~~bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get install dotnet-sdk-5.0
~~~

## Run tests
~~~bash
SOLVER=CBC dotnet test -v n JD-net5.sln # Run tests using CBC solver

SOLVER=CBC dotnet test JD-net5.sln --filter SubLinExprTest1 -v m # Run a single test
~~~
