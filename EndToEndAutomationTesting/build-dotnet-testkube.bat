@echo off
REM Set Docker to Minikube's daemon
for /f "tokens=*" %%i in ('minikube -p minikube docker-env --shell cmd') do call %%i

echo [INFO] Docker env set to Minikube

REM Build Docker image
docker build -t end-to-end-dotnet-test:latest .

if %errorlevel% neq 0 (
    echo [ERROR] Docker build failed!
    exit /b %errorlevel%
)

REM Create Testkube test
kubectl testkube create test ^
  --name dotnet-integration-test ^
  --type container ^
  --image end-to-end-dotnet-test:latest ^
  --command "dotnet" ^
  --args "OutLineStruct.dll"

REM Run the test
kubectl testkube run test dotnet-integration-test

pause
