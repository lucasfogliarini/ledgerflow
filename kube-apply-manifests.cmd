@echo off
setlocal

echo ===== Aplicando infraestrutura base do LedgerFlow =====

echo 1. Aplicando namespace e secrets...
kubectl apply -f "manifests\namespace.yaml"
kubectl apply -f "manifests\ledgerflow-secrets.yaml"

echo.
echo 2. Aplicando Keycloak...
kubectl apply -f "manifests\keycloak_deployment.yaml"

echo.
echo 3. Aplicando SQL Server...
kubectl apply -f "manifests\sql_server_deployment.yaml"

echo.
echo 4. Aplicando Redis...
kubectl apply -f "manifests\redis_deployment.yaml"

echo.
echo ===== Aguardando infraestrutura ficar pronta =====
timeout /t 10 /nobreak >nul

echo.
echo ===== Aplicando APIs =====

echo 5. Aplicando Transaction API...
call :apply_app "LedgerFlow.Transactions.WebApi"

echo.
echo 6. Aplicando LedgerFlow API...
call :apply_app "LedgerFlow.LedgerSummaries.WebApi"

echo.
echo ===== Services =====
kubectl get svc -A

echo.
echo ===== Nodes =====
kubectl get nodes -o wide

echo.
pause
exit /b

:apply_app
set "app_folder=%~1\manifests"
echo Aplicando manifests da pasta: %app_folder%
if exist "%app_folder%\deployment.yaml" (
    echo   - Aplicando deployment.yaml
    kubectl apply -f "%app_folder%\deployment.yaml"
) else (
    echo   - deployment.yaml NAO encontrado
)

if exist "%app_folder%\service.yaml" (
    echo   - Aplicando service.yaml
    kubectl apply -f "%app_folder%\service.yaml"
) else (
    echo   - service.yaml NAO encontrado
)

if exist "%app_folder%\hpa.yaml" (
    echo   - Aplicando hpa.yaml
    kubectl apply -f "%app_folder%\hpa.yaml"
) else (
    echo   - hpa.yaml NAO encontrado
)
echo ----------
exit /b
