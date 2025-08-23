@echo off
REM QuizForge CLI 测试运行脚本 (Windows版本)
REM 此脚本运行所有测试并生成覆盖率报告

echo 🚀 开始运行 QuizForge CLI 测试套件...

REM 获取脚本所在目录
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%\..\..
set TEST_DIR=%PROJECT_ROOT%\tests\QuizForge.CLI.Tests

echo 📁 项目根目录: %PROJECT_ROOT%
echo 📁 测试目录: %TEST_DIR%

REM 检查 .NET 环境
echo 🔍 检查 .NET 环境...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET CLI 未找到，请安装 .NET 8.0 SDK
    exit /b 1
)

for /f "tokens=1,2 delims=." %%a in ('dotnet --version') do set DOTNET_VERSION=%%a.%%b
echo ✅ .NET 版本: %DOTNET_VERSION%

REM 进入测试目录
cd /d "%TEST_DIR%"

REM 还原依赖
echo 📦 还原测试依赖...
dotnet restore

REM 构建项目
echo 🔨 构建测试项目...
dotnet build --configuration Release

REM 运行单元测试
echo 🧪 运行单元测试...
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Unit" --collect:"XPlat Code Coverage"

REM 运行集成测试
echo 🧪 运行集成测试...
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Integration" --collect:"XPlat Code Coverage"

REM 运行端到端测试
echo 🧪 运行端到端测试...
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~E2E" --collect:"XPlat Code Coverage"

REM 运行性能测试
echo 🧪 运行性能测试...
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Performance" --collect:"XPlat Code Coverage"

REM 合并覆盖率报告
echo 📊 合并覆盖率报告...
where reportgenerator >nul 2>&1
if errorlevel 1 (
    echo ⚠️  ReportGenerator 未安装，跳过覆盖率报告生成
) else (
    reportgenerator -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"TestResults\coverage-report" -reporttypes:HtmlInline_AzurePipelines;Cobertura
    echo ✅ 覆盖率报告已生成: TestResults\coverage-report\index.html
)

REM 生成测试结果摘要
echo 📋 生成测试结果摘要...
if exist "TestResults" (
    echo 📁 测试结果目录内容:
    dir /b TestResults\
    
    REM 查找覆盖率文件
    dir /s /b TestResults\*.cobertura.xml >nul 2>&1
    if not errorlevel 1 (
        echo ✅ 找到覆盖率文件:
        dir /s /b TestResults\*.cobertura.xml
    )
)

REM 运行所有测试并生成完整报告
echo 🎯 运行完整测试套件...
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory "TestResults" --logger "trx"

REM 显示最终结果
echo.
echo 🎉 测试完成！
echo.
echo 📊 测试结果位置:
echo    - 测试结果: %TEST_DIR%\TestResults\
echo    - 覆盖率报告: %TEST_DIR%\TestResults\coverage-report\ (如果已生成)
echo.
echo 🔍 查看测试结果:
echo    - 单元测试: dotnet test --configuration Release --filter "FullyQualifiedName~Unit"
echo    - 集成测试: dotnet test --configuration Release --filter "FullyQualifiedName~Integration"
echo    - 端到端测试: dotnet test --configuration Release --filter "FullyQualifiedName~E2E"
echo    - 性能测试: dotnet test --configuration Release --filter "FullyQualifiedName~Performance"
echo.
echo 📈 查看覆盖率:
echo    - HTML报告: %TEST_DIR%\TestResults\coverage-report\index.html
echo    - Cobertura XML: %TEST_DIR%\TestResults\coverage.cobertura.xml
echo.

REM 检查测试是否全部通过
echo 🔍 检查测试状态...
if exist "TestResults" (
    REM 查找失败的测试
    findstr /s /m "Failed" TestResults\*.trx >nul 2>&1
    if not errorlevel 1 (
        echo ❌ 发现失败的测试
        echo 失败的测试文件:
        findstr /s /m "Failed" TestResults\*.trx
        exit /b 1
    ) else (
        echo ✅ 所有测试通过！
    )
)

echo 🚀 测试套件运行完成！
pause