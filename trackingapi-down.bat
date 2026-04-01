@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "PUBLISH_DIR=%~1"
set "SERVICE_NAME=%~2"

if "%PUBLISH_DIR%"=="" (
    if exist "%SCRIPT_DIR%TrackingApi.exe" (
        set "PUBLISH_DIR=%SCRIPT_DIR%"
    ) else (
        set "PUBLISH_DIR=C:\Apps\TrackingApi"
    )
)
if "%SERVICE_NAME%"=="" set "SERVICE_NAME=TrackingApi"

set "PID_FILE=%PUBLISH_DIR%\logs\TrackingApi.pid"

echo [INFO] Publish dir : "%PUBLISH_DIR%"
echo [INFO] Service     : "%SERVICE_NAME%"
echo.

sc.exe query "%SERVICE_NAME%" >nul 2>&1
if not errorlevel 1060 (
    echo [INFO] Deteniendo servicio "%SERVICE_NAME%"...
    sc.exe stop "%SERVICE_NAME%"
    if errorlevel 1 (
        echo [WARN] No se pudo detener el servicio o ya estaba detenido.
    ) else (
        powershell -NoProfile -ExecutionPolicy Bypass -Command "$svc = Get-Service -Name '%SERVICE_NAME%' -ErrorAction SilentlyContinue; if ($svc -and $svc.Status -ne 'Stopped') { try { $svc.WaitForStatus('Stopped','00:00:30') } catch {} }" >nul 2>&1
        echo [OK] Servicio detenido.
    )
)

if exist "%PID_FILE%" (
    for /f "usebackq delims=" %%P in ("%PID_FILE%") do set "OLD_PID=%%P"
    if not "!OLD_PID!"=="" (
        echo [INFO] Deteniendo proceso Kestrel PID !OLD_PID!...
        taskkill /PID !OLD_PID! /T /F >nul 2>&1
        if errorlevel 1 (
            echo [WARN] No se pudo detener el PID !OLD_PID! o ya no existe.
        ) else (
            echo [OK] Kestrel detenido.
        )
    )
    del /q "%PID_FILE%" >nul 2>&1
)

echo [OK] Proceso de apagado finalizado.
exit /b 0
