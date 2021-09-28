#!/bin/bash

cd Livekit.Client/
dotnet pack /p:Version=$1 -c Release
cd pkg/
dotnet nuget push Livekit.Client.$1.nupkg --api-key $(cat ../../.nuget) --source https://api.nuget.org/v3/index.json