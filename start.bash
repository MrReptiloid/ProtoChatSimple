#!/bin/bash

cd ProtoChatSimple.Server
dotnet build
open -a Terminal "./bin/Debug/net9.0/ProtoChatSimple.Server"

cd ../ProtoChatSimple.Client
dotnet build

open -a Terminal "./bin/Debug/net9.0/ProtoChatSimple.Client 3"
open -a Terminal "./bin/Debug/net9.0/ProtoChatSimple.Client 5"
open -a Terminal "./bin/Debug/net9.0/ProtoChatSimple.Client 7"