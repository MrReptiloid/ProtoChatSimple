cd ProtoChatSimple.Server
dotnet build
Start-Process ./bin/Debug/net9.0/ProtoChatSimple.Server.exe

cd ../ProtoChatSimple.Client
dotnet build

Start-Process ./bin/Debug/net9.0/ProtoChatSimple.Client.exe 3

Start-Process ./bin/Debug/net9.0/ProtoChatSimple.Client.exe 5

Start-Process ./bin/Debug/net9.0/ProtoChatSimple.Client.exe 7