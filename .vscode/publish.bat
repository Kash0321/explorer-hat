dotnet publish ExplorerHat.ObstacleAvoidance.csproj -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd bin\Debug\net6.0\linux-arm\publish
c:\work\putty\pscp -pw harlequin -v -r .\*ExplorerHat* pi@harlequin.local:/home/pi/Work/dotnet/explorerhat
popd