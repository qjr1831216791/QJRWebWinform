# QJR Web Winform 生产版本构建脚本
# 用途：构建前端和后端，准备发布

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "QJR Web Winform 生产版本构建" -ForegroundColor Cyan
Write-Host "版本: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 Node.js
Write-Host "检查 Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "Node.js 版本: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "错误: 未找到 Node.js，请先安装 Node.js" -ForegroundColor Red
    exit 1
}

# 检查 MSBuild
Write-Host "检查 MSBuild..." -ForegroundColor Yellow
$msbuildPath = ""
$vsPath = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if (Test-Path $vsPath) {
    $msbuildPath = $vsPath
    Write-Host "找到 MSBuild: $msbuildPath" -ForegroundColor Green
} else {
    $vsPath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    if (Test-Path $vsPath) {
        $msbuildPath = $vsPath
        Write-Host "找到 MSBuild: $msbuildPath" -ForegroundColor Green
    } else {
        Write-Host "警告: 未找到 MSBuild，将尝试使用系统 PATH 中的 MSBuild" -ForegroundColor Yellow
        $msbuildPath = "msbuild"
    }
}

# 步骤 1: 构建前端
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "步骤 1: 构建前端" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Push-Location frontend

# 检查 node_modules
if (-not (Test-Path "node_modules")) {
    Write-Host "安装前端依赖..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "错误: npm install 失败" -ForegroundColor Red
        Pop-Location
        exit 1
    }
}

# 构建前端
Write-Host "构建前端生产版本..." -ForegroundColor Yellow
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 前端构建失败" -ForegroundColor Red
    Pop-Location
    exit 1
}

# 验证构建结果
$wwwrootPath = "..\src\QJRWebWinform.WPF\wwwroot"
if (-not (Test-Path $wwwrootPath)) {
    Write-Host "错误: 前端构建输出目录不存在: $wwwrootPath" -ForegroundColor Red
    Pop-Location
    exit 1
}

if (-not (Test-Path "$wwwrootPath\index.html")) {
    Write-Host "错误: 前端构建输出不完整，缺少 index.html" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "前端构建成功！" -ForegroundColor Green
Pop-Location

# 步骤 2: 构建后端
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "步骤 2: 构建后端 (Release)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "构建后端 Release 版本..." -ForegroundColor Yellow
& $msbuildPath "QJRWebWinform.sln" /p:Configuration=Release /p:Platform="Any CPU" /t:Rebuild /v:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 后端构建失败" -ForegroundColor Red
    exit 1
}

# 验证构建结果
$releasePath = "src\QJRWebWinform.WPF\bin\Release"
if (-not (Test-Path "$releasePath\QJRWebWinform.WPF.exe")) {
    Write-Host "错误: 后端构建输出不完整，缺少 QJRWebWinform.WPF.exe" -ForegroundColor Red
    exit 1
}

Write-Host "后端构建成功！" -ForegroundColor Green

# 步骤 3: 验证文件
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "步骤 3: 验证构建结果" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$requiredFiles = @(
    "$releasePath\QJRWebWinform.WPF.exe",
    "$releasePath\QJRWebWinform.WPF.exe.config",
    "$releasePath\wwwroot\index.html",
    "$releasePath\CefSharp.dll",
    "$releasePath\CefSharp.Wpf.dll"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file (缺失)" -ForegroundColor Red
        $allFilesExist = $false
    }
}

if (-not $allFilesExist) {
    Write-Host ""
    Write-Host "警告: 部分必需文件缺失" -ForegroundColor Yellow
}

# 输出构建信息
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "构建完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "输出目录: $releasePath" -ForegroundColor Yellow
Write-Host ""
Write-Host "下一步: 运行 .\scripts\package-release.ps1 -Version `"$Version`" 来打包发布文件" -ForegroundColor Cyan
Write-Host ""


