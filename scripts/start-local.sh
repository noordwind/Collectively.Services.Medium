#!/bin/bash
export ASPNETCORE_ENVIRONMENT=local
cd src/Collectively.Services.Medium
dotnet run --no-restore --urls "http://*:11000"