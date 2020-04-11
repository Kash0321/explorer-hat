dotnet publish ../src/ExplorerHat.LineTracker/ExplorerHat.LineTracker.csproj -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd ../src/ExplorerHat.LineTracker/bin/Debug/netcoreapp3.1/linux-arm/publish
pscp -pw harlequin -v -r ./* pi@harlequin.local:/home/pi/Work/dotnet/explorerhat
popd