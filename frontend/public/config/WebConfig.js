var WebConfig = {
    //VUE_APP_DEV_API_URL: "http://localhost:44321/",// 调试环境
    VUE_APP_DEV_API_URL: "http://localhost:8098/",// 本地环境
    
    // API 调用模式配置
    // 'webapi': 通过 WebAPI 调用（用于单独前端调试）
    // 'nativehost': 通过 NativeHost 调用 WPF API（用于 VS 调试和生产环境）
    // 'auto': 自动检测，如果 NativeHost 可用则使用，否则使用 WebAPI
    API_MODE: 'auto', // 'webapi' | 'nativehost' | 'auto'
    
    // 是否强制使用 NativeHost（生产环境必须为 true）
    FORCE_NATIVE_HOST: false, // 生产环境应设置为 true
};

export { WebConfig }

