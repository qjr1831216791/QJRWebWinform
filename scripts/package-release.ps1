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

# 定义需要复制的特定文件（非 DLL 文件）
$specificFiles = @(
    # 主程序
    "QJRWebWinform.WPF.exe",
    "QJRWebWinform.WPF.exe.config",
    
    # 应用程序图标
    "app.ico",
    
    # CefSharp 子进程
    "CefSharp.BrowserSubprocess.exe",
    
    # Chromium 核心文件
    "chrome_elf.dll",
    "libcef.dll",
    # GPU 相关文件（已禁用 GPU 加速，这些文件不需要）
    # "libEGL.dll",              # 已禁用 GPU，排除
    # "libGLESv2.dll",            # 已禁用 GPU，排除
    # "vulkan-1.dll",             # 已禁用 GPU，排除
    # "vk_swiftshader.dll",       # 已禁用 GPU，排除
    # "vk_swiftshader_icd.json",  # 已禁用 GPU，排除
    # "d3dcompiler_47.dll",       # 已禁用 GPU，排除
    "icudtl.dat",
    "resources.pak",
    "snapshot_blob.bin",
    "v8_context_snapshot.bin",
    "chrome_100_percent.pak",
    "chrome_200_percent.pak",
    
    # 文档
    "LICENSE.txt",
    "README.txt"
)

# 复制特定文件
Write-Host ""
Write-Host "复制特定文件..." -ForegroundColor Yellow
$copiedCount = 0
$missingFiles = @()

foreach ($file in $specificFiles) {
    $sourceFile = Join-Path $sourceDir $file
    if (Test-Path $sourceFile) {
        Copy-Item -Path $sourceFile -Destination $targetDir -Force
        $copiedCount++
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        $missingFiles += $file
        Write-Host "  ⚠ $file (未找到，跳过)" -ForegroundColor Yellow
    }
}

# 复制所有 DLL 文件（排除已在特定文件列表中的 DLL）
Write-Host ""
Write-Host "复制 DLL 文件..." -ForegroundColor Yellow
$specificFileNames = $specificFiles | ForEach-Object { [System.IO.Path]::GetFileName($_) }
$dllFiles = Get-ChildItem -Path $sourceDir -Filter "*.dll" -File | Where-Object {
    # 排除已在特定文件列表中的 DLL（如 chrome_elf.dll, libcef.dll 等）
    $_.Name -notin $specificFileNames
}

foreach ($dll in $dllFiles) {
    $destFile = Join-Path $targetDir $dll.Name
    Copy-Item -Path $dll.FullName -Destination $destFile -Force
    $copiedCount++
    Write-Host "  ✓ $($dll.Name)" -ForegroundColor Green
}

# 复制所有 DLL 配置文件
Write-Host ""
Write-Host "复制配置文件..." -ForegroundColor Yellow
$configFiles = Get-ChildItem -Path $sourceDir -Filter "*.dll.config" -File
foreach ($config in $configFiles) {
    $destFile = Join-Path $targetDir $config.Name
    Copy-Item -Path $config.FullName -Destination $destFile -Force
    $copiedCount++
    Write-Host "  ✓ $($config.Name)" -ForegroundColor Green
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

# locales 目录（仅复制中英文语言包以减小体积）
# 说明：根据 App.xaml.cs 中的 AcceptLanguageList 配置，只复制需要的语言文件
# 可减少空间：约 10-20MB（默认包含 100+ 种语言，只保留 4 个文件）
$localesSource = Join-Path $sourceDir "locales"
if (Test-Path $localesSource) {
    $localesTarget = Join-Path $targetDir "locales"
    New-Item -ItemType Directory -Force -Path $localesTarget | Out-Null
    
    # 只复制中英文语言包（zh-CN, zh, en-US, en）
    $requiredLocales = @("zh-CN.pak", "zh.pak", "en-US.pak", "en.pak")
    $copiedLocaleCount = 0
    foreach ($locale in $requiredLocales) {
        $localeFile = Join-Path $localesSource $locale
        if (Test-Path $localeFile) {
            Copy-Item -Path $localeFile -Destination $localesTarget -Force
            $copiedLocaleCount++
        }
    }
    
    if ($copiedLocaleCount -gt 0) {
        Write-Host "  ✓ locales\ (仅包含 $copiedLocaleCount 个语言包: 中文、英文)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ locales\ (未找到需要的语言包)" -ForegroundColor Yellow
    }
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
Write-Host "已排除的文件类型:" -ForegroundColor Cyan
Write-Host "  - *.pdb (调试符号文件)" -ForegroundColor Gray
Write-Host "  - *.xml (XML 文档文件)" -ForegroundColor Gray
Write-Host "  - DawnCache, GPUCache, Logs (运行时生成目录)" -ForegroundColor Gray
Write-Host "  - GPU 相关文件 (已禁用 GPU 加速): libEGL.dll, libGLESv2.dll, vulkan-1.dll, vk_swiftshader.dll, d3dcompiler_47.dll 等" -ForegroundColor Gray
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


