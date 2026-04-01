@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "PUBLISH_DIR=%~1"
set "SERVICE_NAME=%~2"
set "HEALTH_URL=%~3"

if "%PUBLISH_DIR%"=="" (
    if exist "%SCRIPT_DIR%TrackingApi.exe" (
        set "PUBLISH_DIR=%SCRIPT_DIR%"
    ) else (
        set "PUBLISH_DIR=C:\Apps\TrackingApi"
    )
)
if "%SERVICE_NAME%"=="" set "SERVICE_NAME=TrackingApi"
if "%HEALTH_URL%"=="" set "HEALTH_URL=http://127.0.0.1:5105/health"

set "APP_EXE=%PUBLISH_DIR%\TrackingApi.exe"
set "LOG_DIR=%PUBLISH_DIR%\logs"
set "PID_FILE=%LOG_DIR%\TrackingApi.pid"
set "STDOUT_LOG=%LOG_DIR%\TrackingApi.stdout.log"
set "STDERR_LOG=%LOG_DIR%\TrackingApi.stderr.log"

echo [INFO] Publish dir : "%PUBLISH_DIR%"
echo [INFO] Service     : "%SERVICE_NAME%"
echo [INFO] Health URL  : "%HEALTH_URL%"
echo.

sc.exe query "%SERVICE_NAME%" >nul 2>&1
if not errorlevel 1060 (
    echo [INFO] Servicio encontrado. Iniciando "%SERVICE_NAME%"...
    sc.exe start "%SERVICE_NAME%"
    if errorlevel 1 (
        echo [ERROR] No se pudo iniciar el servicio. Ejecuta este .bat como Administrador.
        exit /b 1
    )
    goto :health
)

if not exist "%APP_EXE%" (
    echo [ERROR] No existe "%APP_EXE%".
    echo [INFO] Instala el servicio o copia una carpeta publish valida.
    exit /b 1
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

if exist "%PID_FILE%" (
    for /f "usebackq delims=" %%P in ("%PID_FILE%") do set "OLD_PID=%%P"
    if not "!OLD_PID!"=="" taskkill /PID !OLD_PID! /T /F >nul 2>&1
    del /q "%PID_FILE%" >nul 2>&1
)

echo [INFO] Servicio no encontrado. Levantando TrackingApi.exe con Kestrel...
set "PS_COMMAND=$p = Start-Process -FilePath '%APP_EXE%' -WorkingDirectory '%PUBLISH_DIR%' -PassThru -RedirectStandardOutput '%STDOUT_LOG%' -RedirectStandardError '%STDERR_LOG%'; Set-Content -Path '%PID_FILE%' -Value $p.Id"
powershell -NoProfile -ExecutionPolicy Bypass -Command "%PS_COMMAND%"
if errorlevel 1 (
    echo [ERROR] No se pudo iniciar TrackingApi.exe.
    exit /b 1
)

for /f "usebackq delims=" %%P in ("%PID_FILE%") do set "STARTED_PID=%%P"
echo [INFO] PID Kestrel : %STARTED_PID%

:health
echo.
echo [INFO] Probando health...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$url = '%HEALTH_URL%'; try { $r = Invoke-RestMethod -Uri $url -TimeoutSec 8; Write-Host '[OK] Health response:' ($r | ConvertTo-Json -Compress -Depth 5); exit 0 } catch { if ($_.Exception.Response) { Write-Host '[WARN] Health HTTP status:' ([int]$_.Exception.Response.StatusCode) } else { Write-Host '[WARN] Health no respondio.' }; exit 1 }"
exit /b %ERRORLEVEL%
