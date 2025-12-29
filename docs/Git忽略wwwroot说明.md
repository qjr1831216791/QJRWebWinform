# Git 忽略 wwwroot 目录说明

## 为什么需要忽略 wwwroot？

`wwwroot` 目录包含的是前端构建后的产物（由 `frontend` 项目通过 `npm run build` 生成），这些文件：

1. **是自动生成的**：可以通过构建脚本重新生成
2. **会增加仓库大小**：构建产物通常比较大
3. **可能导致合并冲突**：每次构建文件名可能变化（如 `app.1a9c6b33.js`）
4. **不是源代码**：应该只提交源代码，而不是构建产物

## 已完成的配置

已在 `.gitignore` 文件中添加了以下规则：

```
# Frontend build output (generated from frontend build)
**/wwwroot/
wwwroot/
```

这将忽略：
- 项目根目录下的 `wwwroot/`
- 任何子目录下的 `wwwroot/`（如 `src/QJRWebWinform.WPF/wwwroot/`）

## 如果 wwwroot 已经被 Git 跟踪

如果 `wwwroot` 目录已经被 Git 跟踪（之前提交过），仅仅添加到 `.gitignore` 是不够的。需要从 Git 索引中移除：

### 方法 1：使用 Git 命令（推荐）

```powershell
# 从 Git 索引中移除 wwwroot（但保留本地文件）
git rm -r --cached src/QJRWebWinform.WPF/wwwroot

# 提交更改
git commit -m "移除 wwwroot 目录，添加到 .gitignore"
```

### 方法 2：如果只想移除特定文件

```powershell
# 移除所有 wwwroot 目录
git rm -r --cached **/wwwroot

# 或者指定具体路径
git rm -r --cached src/QJRWebWinform.WPF/wwwroot
```

### 验证

移除后，可以使用以下命令验证：

```powershell
# 检查 wwwroot 是否还在 Git 跟踪中
git ls-files | Select-String "wwwroot"

# 如果没有任何输出，说明已成功移除
```

## 注意事项

1. **`git rm --cached`** 只会从 Git 索引中移除，**不会删除本地文件**
2. 移除后，`wwwroot` 目录仍然存在于本地，只是不再被 Git 跟踪
3. 其他开发者拉取代码后，需要运行 `npm run build` 来生成 `wwwroot` 目录
4. 确保构建脚本（`scripts/build-frontend.ps1`）正常工作，以便其他开发者可以轻松构建

## 构建流程

其他开发者克隆项目后，需要：

1. 安装前端依赖：
   ```powershell
   cd frontend
   npm install
   ```

2. 构建前端：
   ```powershell
   npm run build
   # 或使用构建脚本
   ..\scripts\build-frontend.ps1
   ```

3. 构建后端（Visual Studio 会自动复制 wwwroot 到输出目录）

## 总结

- ✅ `wwwroot` 已添加到 `.gitignore`
- ⚠️ 如果已被跟踪，需要从 Git 索引中移除
- ✅ 本地文件不会被删除
- ✅ 其他开发者可以通过构建脚本重新生成

