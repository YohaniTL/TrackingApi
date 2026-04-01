@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "MODE=%~1"
set "CUSTOM_PUBLISH_DIR=%~2"
set "CUSTOM_SERVICE_NAME=%~3"

if "%MODE%"=="" goto :usage

set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%TrackingApi"
set "PROJECT_FILE=%PROJECT_DIR%\TrackingApi.csproj"
set "PUBLISH_DIR=%SCRIPT_DIR%publish\TrackingApi"
set "SERVICE_NAME=TrackingApi"
set "DISPLAY_NAME=Tracking API"
set "LOG_DIR=%PUBLISH_DIR%\logs"
set "PID_FILE=%LOG_DIR%\TrackingApi.pid"
set "STDOUT_LOG=%LOG_DIR%\TrackingApi.stdout.log"
set "STDERR_LOG=%LOG_DIR%\TrackingApi.stderr.log"

if not "%CUSTOM_PUBLISH_DIR%"=="" set "PUBLISH_DIR=%CUSTOM_PUBLISH_DIR%"
if not "%CUSTOM_SERVICE_NAME%"=="" set "SERVICE_NAME=%CUSTOM_SERVICE_NAME%"

set "APP_EXE=%PUBLISH_DIR%\TrackingApi.exe"

if /I "%MODE%"=="service" goto :publish_and_service
if /I "%MODE%"=="kestrel" goto :publish_and_kestrel
if /I "%MODE%"=="stop-kestrel" goto :stop_kestrel
if /I "%MODE%"=="remove-service" goto :remove_service

echo [ERROR] Modo no soportado: %MODE%
goto :usage

:publish_and_service
call :require_admin || exit /b 1
call :publish || exit /b 1
call :install_or_update_service || exit /b 1
echo.
echo [OK] Servicio "%SERVICE_NAME%" instalado y levantado.
echo [INFO] Ejecutable: "%APP_EXE%"
echo [INFO] Revisa la URL configurada en appsettings.json ^(ApiBinding:Url^).
exit /b 0

:publish_and_kestrel
call :publish || exit /b 1
call :start_kestrel || exit /b 1
echo.
echo [OK] TrackingApi levantada en modo Kestrel.
echo [INFO] Ejecutable: "%APP_EXE%"
echo [INFO] Logs: "%LOG_DIR%"
echo [INFO] Revisa la URL configurada en appsettings.json ^(ApiBinding:Url^).
exit /b 0

:publish
if not exist "%PROJECT_FILE%" (
    echo [ERROR] No se encontro el proyecto en "%PROJECT_FILE%".
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] dotnet no esta instalado o no esta en PATH.
    exit /b 1
)

echo [INFO] Publicando TrackingApi en "%PUBLISH_DIR%"...
dotnet publish "%PROJECT_FILE%" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo [ERROR] Fallo dotnet publish.
    exit /b 1
)

if not exist "%APP_EXE%" (
    echo [ERROR] No se genero "%APP_EXE%".
    exit /b 1
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"
exit /b 0

:install_or_update_service
echo [INFO] Instalando/actualizando servicio "%SERVICE_NAME%"...

sc.exe query "%SERVICE_NAME%" >nul 2>&1
if errorlevel 1060 (
    sc.exe create "%SERVICE_NAME%" binPath= "\"%APP_EXE%\"" start= auto DisplayName= "%DISPLAY_NAME%"
    if errorlevel 1 (
        echo [ERROR] No se pudo crear el servicio.
        exit /b 1
    )
    sc.exe description "%SERVICE_NAME%" "Tracking API .NET 10"
) else (
    sc.exe stop "%SERVICE_NAME%" >nul 2>&1
    powershell -NoProfile -ExecutionPolicy Bypass -Command "$svc = Get-Service -Name '%SERVICE_NAME%' -ErrorAction SilentlyContinue; if ($svc -and $svc.Status -ne 'Stopped') { try { $svc.WaitForStatus('Stopped','00:00:30') } catch {} }" >nul 2>&1
    sc.exe config "%SERVICE_NAME%" binPath= "\"%APP_EXE%\"" start= auto DisplayName= "%DISPLAY_NAME%"
    if errorlevel 1 (
        echo [ERROR] No se pudo actualizar el servicio.
        exit /b 1
    )
)

sc.exe failure "%SERVICE_NAME%" reset= 86400 actions= restart/5000/restart/5000/restart/5000 >nul 2>&1
sc.exe start "%SERVICE_NAME%"
if errorlevel 1 (
    echo [ERROR] El servicio fue instalado/actualizado, pero no se pudo iniciar.
    exit /b 1
)

exit /b 0

:start_kestrel
if not exist "%APP_EXE%" (
    echo [ERROR] No se encontro "%APP_EXE%".
    exit /b 1
)

call :stop_kestrel >nul 2>&1

echo [INFO] Iniciando TrackingApi.exe en segundo plano...
set "PS_COMMAND=$p = Start-Process -FilePath '%APP_EXE%' -WorkingDirectory '%PUBLISH_DIR%' -PassThru -RedirectStandardOutput '%STDOUT_LOG%' -RedirectStandardError '%STDERR_LOG%'; Set-Content -Path '%PID_FILE%' -Value $p.Id"
powershell -NoProfile -ExecutionPolicy Bypass -Command "%PS_COMMAND%"
if errorlevel 1 (
    echo [ERROR] No se pudo iniciar TrackingApi.exe.
    exit /b 1
)

for /f "usebackq delims=" %%P in ("%PID_FILE%") do set "STARTED_PID=%%P"
echo [INFO] PID: %STARTED_PID%
exit /b 0

:stop_kestrel
if not exist "%PID_FILE%" exit /b 0

for /f "usebackq delims=" %%P in ("%PID_FILE%") do set "OLD_PID=%%P"
if "%OLD_PID%"=="" (
    del /q "%PID_FILE%" >nul 2>&1
    exit /b 0
)

taskkill /PID %OLD_PID% /T /F >nul 2>&1
del /q "%PID_FILE%" >nul 2>&1
exit /b 0

:remove_service
call :require_admin || exit /b 1
sc.exe query "%SERVICE_NAME%" >nul 2>&1
if errorlevel 1060 (
    echo [INFO] El servicio "%SERVICE_NAME%" no existe.
    exit /b 0
)

sc.exe stop "%SERVICE_NAME%" >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command "$svc = Get-Service -Name '%SERVICE_NAME%' -ErrorAction SilentlyContinue; if ($svc -and $svc.Status -ne 'Stopped') { try { $svc.WaitForStatus('Stopped','00:00:30') } catch {} }" >nul 2>&1
sc.exe delete "%SERVICE_NAME%"
if errorlevel 1 (
    echo [ERROR] No se pudo eliminar el servicio.
    exit /b 1
)

echo [OK] Servicio "%SERVICE_NAME%" eliminado.
exit /b 0

:require_admin
net session >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Ejecuta este .bat como Administrador.
    exit /b 1
)
exit /b 0

:usage
echo Uso:
echo   deploy-trackingapi.bat service [publish_dir] [service_name]
echo   deploy-trackingapi.bat kestrel [publish_dir]
echo   deploy-trackingapi.bat stop-kestrel [publish_dir]
echo   deploy-trackingapi.bat remove-service [publish_dir] [service_name]
echo.
echo Ejemplos:
echo   deploy-trackingapi.bat service
echo   deploy-trackingapi.bat service "C:\Apps\TrackingApi" "TrackingApi"
echo   deploy-trackingapi.bat kestrel
exit /b 1
