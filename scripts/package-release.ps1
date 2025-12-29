# QJR Web Winform 发布包打包脚本
# 用途：将 Release 构建结果打包为可分发的发布包

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "QJR Web Winform 发布包打包" -ForegroundColor Cyan
Write-Host "版本: $Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$sourceDir = "src\QJRWebWinform.WPF\bin\Release"
$targetDir = "release\QJRWebWinform-v$Version"

# 检查源目录
if (-not (Test-Path $sourceDir)) {
    Write-Host "错误: 源目录不存在: $sourceDir" -ForegroundColor Red
    Write-Host "请先运行 .\scripts\build-release.ps1 构建项目" -ForegroundColor Yellow
    exit 1
}

# 检查主程序
if (-not (Test-Path "$sourceDir\QJRWebWinform.WPF.exe")) {
    Write-Host "错误: 主程序不存在，请先构建 Release 版本" -ForegroundColor Red
    exit 1
}

# 创建目标目录
Write-Host "创建发布目录: $targetDir" -ForegroundColor Yellow
if (Test-Path $targetDir) {
    Write-Host "目录已存在，正在清理..." -ForegroundColor Yellow
    Remove-Item -Path $targetDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

# 定义需要复制的文件模式
$filePatterns = @(
    # 主程序
    "QJRWebWinform.WPF.exe",
    "QJRWebWinform.WPF.exe.config",
    
    # CefSharp 核心文件
    "CefSharp.BrowserSubprocess.Core.dll",
    "CefSharp.BrowserSubprocess.exe",
    "CefSharp.Core.dll",
    "CefSharp.Core.Runtime.dll",
    "CefSharp.dll",
    "CefSharp.Wpf.dll",
    
    # Chromium 核心文件
    "chrome_elf.dll",
    "libcef.dll",
    "libEGL.dll",
    "libGLESv2.dll",
    "vulkan-1.dll",
    "vk_swiftshader.dll",
    "vk_swiftshader_icd.json",
    "d3dcompiler_47.dll",
    "icudtl.dat",
    "resources.pak",
    "snapshot_blob.bin",
    "v8_context_snapshot.bin",
    "chrome_100_percent.pak",
    "chrome_200_percent.pak",
    
    # 其他依赖
    "Newtonsoft.Json.dll",
    
    # 文档
    "LICENSE.txt",
    "README.txt"
)

# 复制文件
Write-Host ""
Write-Host "复制文件..." -ForegroundColor Yellow
$copiedCount = 0
$missingFiles = @()

foreach ($pattern in $filePatterns) {
    $sourceFile = Join-Path $sourceDir $pattern
    if (Test-Path $sourceFile) {
        Copy-Item -Path $sourceFile -Destination $targetDir -Force
        $copiedCount++
        Write-Host "  ✓ $pattern" -ForegroundColor Green
    } else {
        $missingFiles += $pattern
        Write-Host "  ⚠ $pattern (未找到，跳过)" -ForegroundColor Yellow
    }
}

# 复制目录
Write-Host ""
Write-Host "复制目录..." -ForegroundColor Yellow

# wwwroot 目录
$wwwrootSource = Join-Path $sourceDir "wwwroot"
if (Test-Path $wwwrootSource) {
    $wwwrootTarget = Join-Path $targetDir "wwwroot"
    Copy-Item -Path $wwwrootSource -Destination $wwwrootTarget -Recurse -Force
    Write-Host "  ✓ wwwroot\" -ForegroundColor Green
} else {
    Write-Host "  ✗ wwwroot\ (缺失)" -ForegroundColor Red
}

# locales 目录
$localesSource = Join-Path $sourceDir "locales"
if (Test-Path $localesSource) {
    $localesTarget = Join-Path $targetDir "locales"
    Copy-Item -Path $localesSource -Destination $localesTarget -Recurse -Force
    Write-Host "  ✓ locales\" -ForegroundColor Green
} else {
    Write-Host "  ⚠ locales\ (未找到，跳过)" -ForegroundColor Yellow
}

# 计算文件大小
Write-Host ""
Write-Host "计算发布包大小..." -ForegroundColor Yellow
$totalSize = (Get-ChildItem -Path $targetDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
$totalSizeMB = [math]::Round($totalSize / 1MB, 2)
Write-Host "总大小: $totalSizeMB MB" -ForegroundColor Cyan

# 创建 ZIP 压缩包
Write-Host ""
Write-Host "创建 ZIP 压缩包..." -ForegroundColor Yellow
$zipPath = "release\QJRWebWinform-v$Version.zip"
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}

Compress-Archive -Path "$targetDir\*" -DestinationPath $zipPath -Force
$zipSize = (Get-Item $zipPath).Length / 1MB
$zipSizeMB = [math]::Round($zipSize, 2)

Write-Host "压缩包大小: $zipSizeMB MB" -ForegroundColor Cyan

# 输出摘要
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "打包完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "发布目录: $targetDir" -ForegroundColor Yellow
Write-Host "压缩包: $zipPath" -ForegroundColor Yellow
Write-Host "总文件数: $copiedCount" -ForegroundColor Yellow
Write-Host "发布包大小: $totalSizeMB MB" -ForegroundColor Yellow
Write-Host "压缩包大小: $zipSizeMB MB" -ForegroundColor Yellow
Write-Host ""

if ($missingFiles.Count -gt 0) {
    Write-Host "警告: 以下文件未找到（可能不影响运行）:" -ForegroundColor Yellow
    foreach ($file in $missingFiles) {
        Write-Host "  - $file" -ForegroundColor Yellow
    }
    Write-Host ""
}

Write-Host "发布包已准备就绪！" -ForegroundColor Green
Write-Host ""


