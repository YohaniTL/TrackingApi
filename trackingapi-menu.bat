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

:menu
cls
echo ==========================================
echo        TrackingApi - Menu de Control
echo ==========================================
echo Publish dir : %PUBLISH_DIR%
echo Service     : %SERVICE_NAME%
echo Health URL  : %HEALTH_URL%
echo.
echo 1. Levantar ^(Servicio o Kestrel^)
echo 2. Bajar ^(Servicio y Kestrel^)
echo 3. Probar health
echo 4. Ver estado
echo 5. Publicar e instalar servicio desde codigo fuente
echo 6. Publicar y levantar Kestrel desde codigo fuente
echo 7. Salir
echo.
set /p "CHOICE=Selecciona una opcion: "

if "%CHOICE%"=="1" goto :up
if "%CHOICE%"=="2" goto :down
if "%CHOICE%"=="3" goto :health
if "%CHOICE%"=="4" goto :status
if "%CHOICE%"=="5" goto :deploy_service
if "%CHOICE%"=="6" goto :deploy_kestrel
if "%CHOICE%"=="7" exit /b 0
goto :menu

:up
call "%SCRIPT_DIR%trackingapi-up.bat" "%PUBLISH_DIR%" "%SERVICE_NAME%" "%HEALTH_URL%"
pause
goto :menu

:down
call "%SCRIPT_DIR%trackingapi-down.bat" "%PUBLISH_DIR%" "%SERVICE_NAME%"
pause
goto :menu

:health
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$url = '%HEALTH_URL%'; try { $r = Invoke-RestMethod -Uri $url -TimeoutSec 8; Write-Host '[OK] Health response:' ($r | ConvertTo-Json -Compress -Depth 5) } catch { if ($_.Exception.Response) { Write-Host '[WARN] Health HTTP status:' ([int]$_.Exception.Response.StatusCode) } else { Write-Host '[WARN] Health no respondio.' } }"
pause
goto :menu

:status
echo [INFO] Estado del servicio:
sc.exe query "%SERVICE_NAME%"
echo.
if exist "%PUBLISH_DIR%\logs\TrackingApi.pid" (
    echo [INFO] PID Kestrel:
    type "%PUBLISH_DIR%\logs\TrackingApi.pid"
) else (
    echo [INFO] No hay PID de Kestrel registrado.
)
echo.
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$url = '%HEALTH_URL%'; try { $r = Invoke-RestMethod -Uri $url -TimeoutSec 5; Write-Host '[OK] Health response:' ($r | ConvertTo-Json -Compress -Depth 5) } catch { if ($_.Exception.Response) { Write-Host '[WARN] Health HTTP status:' ([int]$_.Exception.Response.StatusCode) } else { Write-Host '[WARN] Health no respondio.' } }"
pause
goto :menu

:deploy_service
if not exist "%SCRIPT_DIR%deploy-trackingapi.bat" (
    echo [WARN] deploy-trackingapi.bat no existe en esta carpeta.
    echo [INFO] Esta opcion solo funciona si el menu esta junto al repo o si copiaste tambien deploy-trackingapi.bat.
    pause
    goto :menu
)
call "%SCRIPT_DIR%deploy-trackingapi.bat" service "%PUBLISH_DIR%" "%SERVICE_NAME%"
pause
goto :menu

:deploy_kestrel
if not exist "%SCRIPT_DIR%deploy-trackingapi.bat" (
    echo [WARN] deploy-trackingapi.bat no existe en esta carpeta.
    echo [INFO] Esta opcion solo funciona si el menu esta junto al repo o si copiaste tambien deploy-trackingapi.bat.
    pause
    goto :menu
)
call "%SCRIPT_DIR%deploy-trackingapi.bat" kestrel "%PUBLISH_DIR%"
pause
goto :menu
