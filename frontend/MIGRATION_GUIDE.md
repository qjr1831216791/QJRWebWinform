# VueWebProj 迁移到 QJRWebWinform Frontend 指南

## 📋 迁移步骤

### 1. 安装缺失的依赖

```bash
cd frontend
npm install element-ui@^2.15.14
npm install axios@^1.5.0
npm install echarts@^5.5.1
npm install vue-router@^3.0.1
```

### 2. 复制静态资源

将 VueWebProj 的 `public` 目录下的文件复制到 QJRWebWinform 的 `frontend/public`：

```
VueWebProj/vue-web/public/
├── config/
│   └── WebConfig.js
└── js/
    ├── JsCrmHelper.js
    ├── rtcrm.min.js
    └── ToLunar.js
```

复制到：
```
QJRWebWinform/frontend/public/
├── config/
│   └── WebConfig.js
└── js/
    ├── JsCrmHelper.js
    ├── rtcrm.min.js
    └── ToLunar.js
```

### 3. 复制源代码

将 VueWebProj 的 `src` 目录内容复制到 QJRWebWinform 的 `frontend/src`：

- `src/components/` - 所有组件
- `src/router/` - 路由配置
- `src/assets/` - 静态资源（图片等）

### 4. 修改 main.js

需要修改 `frontend/src/main.js`，使其：
1. 等待 NativeHost 初始化
2. 导入所有必要的依赖
3. 初始化 Vue 应用

### 5. 更新 vue.config.js

确保 `vue.config.js` 配置正确，特别是：
- 静态资源路径
- 构建输出目录
- 开发服务器配置

### 6. 处理 API 调用

如果 VueWebProj 使用外部 API，需要考虑：
- 是否继续使用 axios 调用外部 API
- 或者通过 NativeHost 调用后端 C# 方法
- 或者两者结合使用

## ⚠️ 注意事项

1. **Vue 版本差异**：VueWebProj 使用 2.5.2，QJRWebWinform 使用 2.6.14，应该兼容
2. **路由模式**：确保路由配置正确，特别是 base 路径
3. **Element UI 样式**：确保 Element UI 的 CSS 正确加载
4. **静态资源**：确保 public 目录下的文件在构建时正确复制
5. **环境变量**：WebConfig.js 中的 API 地址可能需要调整

## 🔧 可能需要的调整

1. **API 地址配置**：如果使用 NativeHost，可能需要调整 API 调用方式
2. **路由 base 路径**：可能需要设置 `base: '/'` 或 `base: './'`
3. **构建输出**：确保构建输出到正确的 `wwwroot` 目录

