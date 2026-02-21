Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Auth.Service\Auth.Service.Presentation'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\Weather.Current\Weather.Current.Presentation'; dotnet run"
