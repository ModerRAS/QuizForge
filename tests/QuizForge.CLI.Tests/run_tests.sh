#!/bin/bash

# QuizForge CLI 测试运行脚本
# 此脚本运行所有测试并生成覆盖率报告

set -e

echo "🚀 开始运行 QuizForge CLI 测试套件..."

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
TEST_DIR="$PROJECT_ROOT/tests/QuizForge.CLI.Tests"

echo "📁 项目根目录: $PROJECT_ROOT"
echo "📁 测试目录: $TEST_DIR"

# 检查 .NET 环境
echo "🔍 检查 .NET 环境..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET CLI 未找到，请安装 .NET 8.0 SDK${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version | cut -d. -f1,2)
echo "✅ .NET 版本: $DOTNET_VERSION"

# 还原依赖
echo "📦 还原测试依赖..."
cd "$TEST_DIR"
dotnet restore

# 构建项目
echo "🔨 构建测试项目..."
dotnet build --configuration Release

# 运行单元测试
echo -e "${BLUE}🧪 运行单元测试...${NC}"
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Unit" --collect:"XPlat Code Coverage"

# 运行集成测试
echo -e "${BLUE}🧪 运行集成测试...${NC}"
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Integration" --collect:"XPlat Code Coverage"

# 运行端到端测试
echo -e "${BLUE}🧪 运行端到端测试...${NC}"
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~E2E" --collect:"XPlat Code Coverage"

# 运行性能测试
echo -e "${BLUE}🧪 运行性能测试...${NC}"
dotnet test --configuration Release --logger "console;verbosity=detailed" --filter "FullyQualifiedName~Performance" --collect:"XPlat Code Coverage"

# 合并覆盖率报告
echo -e "${BLUE}📊 合并覆盖率报告...${NC}"
if command -v reportgenerator &> /dev/null; then
    reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/coverage-report" -reporttypes:HtmlInline_AzurePipelines;Cobertura
    echo -e "${GREEN}✅ 覆盖率报告已生成: TestResults/coverage-report/index.html${NC}"
else
    echo -e "${YELLOW}⚠️  ReportGenerator 未安装，跳过覆盖率报告生成${NC}"
fi

# 生成测试结果摘要
echo -e "${BLUE}📋 生成测试结果摘要...${NC}"
if [ -d "TestResults" ]; then
    echo "📁 测试结果目录内容:"
    ls -la TestResults/
    
    # 查找覆盖率文件
    COVERAGE_FILES=$(find TestResults -name "*.cobertura.xml" 2>/dev/null || true)
    if [ -n "$COVERAGE_FILES" ]; then
        echo -e "${GREEN}✅ 找到覆盖率文件:${NC}"
        echo "$COVERAGE_FILES"
    fi
fi

# 运行所有测试并生成完整报告
echo -e "${BLUE}🎯 运行完整测试套件...${NC}"
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory "TestResults" --logger "trx"

# 显示最终结果
echo ""
echo -e "${GREEN}🎉 测试完成！${NC}"
echo ""
echo "📊 测试结果位置:"
echo "   - 测试结果: $TEST_DIR/TestResults/"
echo "   - 覆盖率报告: $TEST_DIR/TestResults/coverage-report/ (如果已生成)"
echo ""
echo "🔍 查看测试结果:"
echo "   - 单元测试: dotnet test --configuration Release --filter \"FullyQualifiedName~Unit\""
echo "   - 集成测试: dotnet test --configuration Release --filter \"FullyQualifiedName~Integration\""
echo "   - 端到端测试: dotnet test --configuration Release --filter \"FullyQualifiedName~E2E\""
echo "   - 性能测试: dotnet test --configuration Release --filter \"FullyQualifiedName~Performance\""
echo ""
echo "📈 查看覆盖率:"
echo "   - HTML报告: $TEST_DIR/TestResults/coverage-report/index.html"
echo "   - Cobertura XML: $TEST_DIR/TestResults/coverage.cobertura.xml"
echo ""

# 检查测试是否全部通过
echo -e "${YELLOW}🔍 检查测试状态...${NC}"
if [ -d "TestResults" ]; then
    # 查找失败的测试
    FAILED_TESTS=$(find TestResults -name "*.trx" -exec grep -l "Failed" {} \; 2>/dev/null || true)
    if [ -n "$FAILED_TESTS" ]; then
        echo -e "${RED}❌ 发现失败的测试${NC}"
        echo "失败的测试文件:"
        echo "$FAILED_TESTS"
        exit 1
    else
        echo -e "${GREEN}✅ 所有测试通过！${NC}"
    fi
fi

echo -e "${GREEN}🚀 测试套件运行完成！${NC}"