dotnet publish ..\src\ExplorerHat.ObstacleAvoidance\ExplorerHat.ObstacleAvoidance.csproj -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd ..\src\ExplorerHat.ObstacleAvoidance\bin\Debug\netcoreapp3.1\linux-arm\publish
c:\work\putty\pscp -pw harlequin -v -r .\*ExplorerHat* pi@harlequin.local:/home/pi/Work/dotnet/explorerhat
popd