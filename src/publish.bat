dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd .\bin\Debug\netcoreapp2.2\linux-arm\publish
c:\work\putty\pscp -pw harlequin -v -r .\ExplorerHat* pi@harlequin.local:/home/pi/Work/dotnet/explorerhat
popd