/**
 * 基于 sessionStorage 的缓存工具
 * 特点：
 * 1. 关闭标签页/窗口时自动清除（sessionStorage 特性）
 * 2. 支持设置过期时间（默认10分钟）
 * 3. 自动清理过期缓存
 */

const CACHE_PREFIX = 'crm_cache_';
const DEFAULT_EXPIRE_TIME = 10 * 60 * 1000; // 默认10分钟（毫秒）

/**
 * 生成缓存键
 * @param {string} key - 缓存键
 * @returns {string} 完整的缓存键
 */
function getCacheKey(key) {
    return `${CACHE_PREFIX}${key}`;
}

/**
 * 获取缓存数据
 * @param {string} key - 缓存键
 * @returns {any|null} 缓存的数据，如果不存在或已过期则返回 null
 */
export function getCache(key) {
    try {
        const cacheKey = getCacheKey(key);
        const cacheStr = sessionStorage.getItem(cacheKey);
        
        if (!cacheStr) {
            return null;
        }

        const cache = JSON.parse(cacheStr);
        const now = Date.now();

        // 检查是否过期
        if (cache.expireTime && now > cache.expireTime) {
            // 已过期，清除缓存
            sessionStorage.removeItem(cacheKey);
            return null;
        }

        return cache.data;
    } catch (error) {
        console.warn('获取缓存失败:', error);
        return null;
    }
}

/**
 * 设置缓存数据
 * @param {string} key - 缓存键
 * @param {any} data - 要缓存的数据
 * @param {number} expireTime - 过期时间（毫秒），默认10分钟
 */
export function setCache(key, data, expireTime = DEFAULT_EXPIRE_TIME) {
    try {
        const cacheKey = getCacheKey(key);
        const now = Date.now();
        const cache = {
            data: data,
            expireTime: now + expireTime,
            timestamp: now
        };
        sessionStorage.setItem(cacheKey, JSON.stringify(cache));
    } catch (error) {
        console.warn('设置缓存失败:', error);
        // 如果存储空间不足，尝试清理过期缓存
        if (error.name === 'QuotaExceededError') {
            clearExpiredCache();
            // 清理后重试一次
            try {
                sessionStorage.setItem(getCacheKey(key), JSON.stringify(cache));
            } catch (retryError) {
                console.error('重试设置缓存失败:', retryError);
            }
        }
    }
}

/**
 * 清除指定缓存
 * @param {string} key - 缓存键
 */
export function removeCache(key) {
    try {
        const cacheKey = getCacheKey(key);
        sessionStorage.removeItem(cacheKey);
    } catch (error) {
        console.warn('清除缓存失败:', error);
    }
}

/**
 * 清除所有过期缓存
 */
export function clearExpiredCache() {
    try {
        const now = Date.now();
        const keysToRemove = [];

        // 遍历所有 sessionStorage 项
        for (let i = 0; i < sessionStorage.length; i++) {
            const key = sessionStorage.key(i);
            if (key && key.startsWith(CACHE_PREFIX)) {
                try {
                    const cacheStr = sessionStorage.getItem(key);
                    if (cacheStr) {
                        const cache = JSON.parse(cacheStr);
                        if (cache.expireTime && now > cache.expireTime) {
                            keysToRemove.push(key);
                        }
                    }
                } catch (error) {
                    // 解析失败，可能是无效的缓存项，也加入清除列表
                    keysToRemove.push(key);
                }
            }
        }

        // 清除过期缓存
        keysToRemove.forEach(key => {
            sessionStorage.removeItem(key);
        });
    } catch (error) {
        console.warn('清理过期缓存失败:', error);
    }
}

/**
 * 清除所有缓存（包括未过期的）
 */
export function clearAllCache() {
    try {
        const keysToRemove = [];
        for (let i = 0; i < sessionStorage.length; i++) {
            const key = sessionStorage.key(i);
            if (key && key.startsWith(CACHE_PREFIX)) {
                keysToRemove.push(key);
            }
        }
        keysToRemove.forEach(key => {
            sessionStorage.removeItem(key);
        });
    } catch (error) {
        console.warn('清除所有缓存失败:', error);
    }
}

