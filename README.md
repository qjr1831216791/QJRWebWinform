# QJR Web Winform

基于 **CefSharp + WPF + Vue 2** 的 Windows 桌面应用程序解决方案。

## 快速开始

### 1. 环境要求

- Windows 10/11
- .NET Framework 4.7.2+
- Visual Studio 2019/2022
- Node.js 14.x+ 和 npm

### 2. 初始化项目

```powershell
# 安装前端依赖
cd frontend
npm install

# 构建前端（首次运行前必须）
npm run build
```

### 3. 运行项目

**开发模式（前后端分离）**:

```powershell
# 终端1: 启动前端开发服务器
cd frontend
npm run serve

# 终端2: 在 Visual Studio 中启动后端（Debug 模式）
# 打开 QJRWebWinform.sln，按 F5 启动
```

**生产模式**:

```powershell
# 构建前端
.\scripts\build-frontend.ps1

# 在 Visual Studio 中构建后端（Release 配置）
```

## 项目结构

```
QJRWebWinform/
├── src/QJRWebWinform.WPF/    # WPF 后端项目
├── frontend/                  # Vue 2 前端项目
├── scripts/                   # 构建脚本
└── docs/                      # 文档目录
```

## 详细文档

请查看 [docs/README.md](docs/README.md) 获取完整的开发文档，包括：

- 环境配置
- 开发指南
- 调试步骤
- 编译和生成
- 部署说明
- 常见问题

## 技术栈

- **前端**: Vue 2.6.14 + Vue CLI
- **后端**: .NET Framework 4.7.2 + WPF
- **浏览器引擎**: CefSharp (Chromium)
- **通信方式**: JavaScript Binding

## 许可证

[在此添加许可证信息]

