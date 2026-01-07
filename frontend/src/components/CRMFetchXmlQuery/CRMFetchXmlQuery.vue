<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">FetchXml 查询</h3>
                <el-divider></el-divider>
            </el-header>
            <el-main>
                <el-form :model="input" size="medium" label-position="left">
                    <!-- 输入行 -->
                    <el-row :gutter="24">
                        <!-- 环境： -->
                        <el-col :span="8">
                            <el-form-item prop="envirFrom" label="环境：" label-width="80px">
                                <el-select size="small" v-model="input.envirFrom" placeholder="请选择" :disabled="loading"
                                    @change="envirFromChange">
                                    <el-option v-for="item in environments" :key="item.key" :label="item.label"
                                        :value="item.key">
                                    </el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>

                        <!-- 实体名称 -->
                        <el-col :span="8">
                            <el-form-item prop="entityName" label="实体名称" label-width="80px">
                                <el-input size="small" :disabled="loading || entityOptionsLoading" readonly
                                    v-model="input.entityName" style="vertical-align: middle;"
                                    class="entityName-input-with-select">
                                    <el-select size="small" slot="prepend" clearable filterable
                                        :disabled="loading || entityOptionsLoading" v-model="input.entityObj"
                                        :filter-method="entitySelectFilter" value-key="key" @change="entityNameChange">
                                        <el-option v-for="item in entityOptions" :key="item.key" :label="item.label"
                                            :value="item">
                                        </el-option>
                                    </el-select>
                                </el-input>
                            </el-form-item>
                        </el-col>

                        <!-- 实体视图 -->
                        <el-col :span="8">
                            <el-form-item label="实体视图" label-width="80px">
                                <div class="entityView-input-with-select" style="display: flex; align-items: center;">
                                    <el-select size="small" clearable filterable
                                        :disabled="loading || entityViewOptionsLoading" v-model="input.entityViewObj"
                                        :filter-method="entityViewSelectFilter" value-key="savedqueryid"
                                        @change="entityViewChange" placeholder="请选择实体视图">
                                        <el-option v-for="item in entityViewOptions" :key="item.savedqueryid"
                                            :label="item.name" :value="item">
                                        </el-option>
                                    </el-select>
                                    <el-button size="small" icon="el-icon-search"
                                        :loading="loading || entityViewOptionsLoading" @click="queryEntityViews"
                                        :disabled="input.objecttypecode === -1">
                                    </el-button>
                                </div>
                            </el-form-item>
                        </el-col>
                    </el-row>
                </el-form>
                <!-- 展示内容 -->
                <el-row>
                    <el-row :gutter="10">
                        <el-col :span="24">
                            <el-card class="box-card">
                                <!-- 数据展示 - 左右分栏布局 -->
                                <el-row :gutter="24" style="margin-top: 15px">
                                    <!-- 左侧表格 -->
                                    <el-col :span="14">
                                        <el-table :height="actualTableHeight" :data="tableData" :key="tableColumnsKey"
                                            border style="width: 100%" v-loading="loading"
                                            :default-sort="{ prop: 'id', order: 'ascending' }" highlight-current-row>
                                            <!-- 行号 -->
                                            <el-table-column v-if="tableColumns != undefined && tableColumns.length > 0"
                                                type="index" align="center">
                                            </el-table-column>
                                            <!-- 动态列 -->
                                            <el-table-column v-for="column in tableColumns" :key="column.prop"
                                                :width="column.width" :prop="column.prop" :label="column.label"
                                                show-overflow-tooltip>
                                                <!-- 日期字段格式化显示 -->
                                                <template slot-scope="scope">
                                                    <span v-if="column.isDateTime && scope.row[column.prop]">
                                                        {{ formatDateTimeValue(scope.row[column.prop],
                                                            column.dateTimeFormat) }}
                                                    </span>
                                                    <span v-else>{{ scope.row[column.prop] }}</span>
                                                </template>
                                            </el-table-column>
                                        </el-table>
                                        <!-- 分页组件 -->
                                        <el-pagination :key="tableKey + '_paging'" :disabled="loading"
                                            @size-change="handleSizeChange" @current-change="handleCurrentChange"
                                            :current-page="input.pageIndex" :page-sizes="PageSizeList"
                                            :page-size="input.pageSize" layout="total, sizes, prev, pager, next, jumper"
                                            :total="tableDataTotalRecord" style="margin-top: 10px;">
                                        </el-pagination>
                                    </el-col>
                                    <!-- 右侧输入面板 -->
                                    <el-col :span="10">
                                        <el-card class="input-panel" :style="{ height: tableHeight }">
                                            <div slot="header" class="detail-header">
                                                <div
                                                    style="display: flex; justify-content: space-between; align-items: center;">
                                                    <el-tooltip v-model="sql2FetchXmlUrlTooltip" effect="dark"
                                                        content="点击跳转Sql2FetchXmlUrl网站" placement="right">
                                                        <a :href="sql2FetchXmlUrl" target="_blank">FetchXml 查询</a>
                                                    </el-tooltip>
                                                    <el-button size="mini" icon="el-icon-sort" @click="formatFetchXml"
                                                        :disabled="loading || !input.fetchXml || input.fetchXml.trim() === ''"
                                                        title="格式化 XML">
                                                        格式化
                                                    </el-button>
                                                </div>
                                            </div>
                                            <!-- 输入区域 -->
                                            <div class="input-section">
                                                <el-form :model="input" size="medium" label-position="top">
                                                    <el-form-item style="margin-bottom: 10px;">
                                                        <el-input type="textarea" :rows="19" v-model="input.fetchXml"
                                                            placeholder="请输入 FetchXml 查询字符串" :disabled="loading">
                                                        </el-input>
                                                    </el-form-item>
                                                    <el-form-item style="margin-bottom: 0;">
                                                        <el-button type="primary" size="medium" :loading="loading"
                                                            @click="executeQuery" style="width: 100%;">
                                                            搜索
                                                        </el-button>
                                                    </el-form-item>
                                                </el-form>
                                            </div>
                                        </el-card>
                                    </el-col>
                                </el-row>
                            </el-card>
                        </el-col>
                    </el-row>
                </el-row>
            </el-main>
        </el-container>
    </div>
</template>

<script>
import { getCache, setCache } from '@/utils/storageCache';

export default {
    name: 'CRMFetchXmlQuery',
    data() {
        return {
            // 环境
            environments: [{ label: "无效环境请刷新", key: "undefined" }],
            // 实体名称
            entityOptions: [],
            entityOptionsCopy: [],
            // 实体视图
            entityViewOptions: [],
            entityViewOptionsCopy: [],
            // 输入
            input: {
                envirFrom: "dev",
                fetchXml: "",
                pageIndex: 1, // 当前页码
                pageSize: 50, // 每页显示数量
                entityObj: null,// 实体对象
                entityName: "",// 实体名称
                objecttypecode: -1,// 实体类型代码
                entityViewObj: null,// 实体视图对象
            },
            sql2FetchXmlUrl: "http://www.sql2fetchxml.com/",
            sql2FetchXmlUrlTooltip: true,
            tableData: [], // 表格数据
            tableDataTotalRecord: 0, // 总记录数
            tableColumns: [], // 动态列
            defaultTableHeight: "555", // 表格高度
            tableKey: 1, // 刷新表格的Key
            tableColumnsKey: 1, // 刷新表格列的Key（仅在列结构变化时更新）
            loading: false, // 是否加载数据中
            entityOptionsLoading: false, // 是否加载实体名称中
            entityViewOptionsLoading: false, // 是否加载实体视图中
            currentEntityViewLayoutXml: null, // 当前选中的实体视图的layoutxml
            fieldMetadataMap: {}, // 字段元数据映射 { "entityName.fieldName": metadata }
            entityMetadataCache: {}, // 实体元数据缓存 { "envir_entityName": metadataArray }
            entityMetadataPromises: {}, // 正在进行的查询Promise缓存 { "envir_entityName": Promise }，避免并发重复查询
            // 分页配置
            PageSizeList: [20, 50, 100, 200],
        };
    },
    created() {
        let that = this;
        // 获取系统参数
        this.getEnvironments();

        // 获取实体名称
        this.getEntityOptions();

        setTimeout(() => {
            that.$set(that, "sql2FetchXmlUrlTooltip", false);
        }, 2000);
    },
    mounted() {
        // 监听环境参数切换
        this.$on('environment-change', this.environmentChange);
    },
    computed: {
        // 表格实际高度（减去分页组件的高度，约50px）
        actualTableHeight() {
            let tableHeight = parseInt(this.tableHeight);
            return tableHeight - 50 + "px";
        },
        // 检测是否为桌面端环境（复用 JsCrmHelper 的方法）
        isDesktop() {
            return this.jshelper && this.jshelper.isDesktopEnvironment
                ? this.jshelper.isDesktopEnvironment()
                : false;
        },
        // Table高度
        tableHeight() {
            let height = parseInt(this.defaultTableHeight);
            if (this.isDesktop) return height + 90 + "px";
            return height + "px";
        },
    },
    methods: {
        // 环境切换事件
        environmentChange: function (envir) {
            if (this.rtcrm.isNullOrWhiteSpace(envir)) return;
            // 优化：合并多个 $set 调用，减少响应式更新次数
            // 清空数据
            this.$set(this, "tableData", []);
            this.$set(this, "tableColumns", []);
            this.$set(this, "input", {
                envirFrom: "dev",
                fetchXml: "",
                pageIndex: 1,
                pageSize: 50,
                entityObj: null,
                entityName: "",
                objecttypecode: -1,
                entityViewObj: null,
            });
            this.$set(this, "tableDataTotalRecord", 0);
            this.$set(this, "entityViewOptions", []);
            this.$set(this, "entityViewOptionsCopy", []);
            this.$set(this, "currentEntityViewLayoutXml", null);
            // 优化：环境切换时列结构会变化，需要更新 tableColumnsKey
            this.$set(this, "tableColumnsKey", this.tableColumnsKey + 1);

            // 重新获取环境参数
            this.getEnvironments();

            // 重新获取实体名称
            this.getEntityOptions();
        },

        // 获取环境参数
        getEnvironments: function () {
            let _this = this;
            this.$set(this, "loading", true);

            this.jshelper
                .invokeHiddenApiAsync(
                    "new_hbxn_common",
                    "SyncConfiguration/GetEnvironments",
                    null
                )
                .then((res) => {
                    _this.$set(_this, "loading", false);
                    if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                        this.jshelper.openAlertDialog(this,
                            "返回数据为空", "获取环境参数"
                        );
                        return;
                    }
                    if (res.isSuccess) {
                        let data = res.data;
                        if (!this.rtcrm.isNull(data)) _this.$set(_this, "environments", data);
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "获取环境参数");
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "获取环境参数");
                });
        },

        // 获取实体名称
        getEntityOptions: function () {
            let _this = this;

            // 生成缓存键（包含环境信息）
            const cacheKey = `entityOptions_${this.input.envirFrom}`;

            // 先尝试从缓存获取
            const cachedData = getCache(cacheKey);
            if (cachedData) {
                _this.$set(_this, "entityOptions", cachedData);
                _this.$set(_this, "entityOptionsCopy", cachedData);
                return;
            }

            // 缓存中没有，从服务器获取
            this.$set(this, "loading", true);
            this.$set(this, "entityOptionsLoading", true);
            this.$set(this, "entityOptions", []);
            this.$set(this, "entityOptionsCopy", []);

            let input = {
                envir: this.input.envirFrom,
                isCustomEntity: false,
            };
            this.jshelper
                .invokeHiddenApiAsync(
                    "new_hbxn_common",
                    "RetrieveEntityMetadata/GetAllEntityMetadata",
                    JSON.stringify(input)
                )
                .then((res) => {
                    _this.$set(_this, "loading", false);
                    _this.$set(_this, "entityOptionsLoading", false);
                    if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                        this.jshelper.openAlertDialog(this,
                            "返回数据为空", "获取实体列表"
                        );
                        return;
                    }
                    if (res.isSuccess) {
                        let data = res.data;
                        if (!this.rtcrm.isNull(data)) {
                            _this.$set(_this, "entityOptions", data);
                            _this.$set(_this, "entityOptionsCopy", data);
                            // 将数据存入缓存
                            setCache(cacheKey, data);
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "获取实体列表");
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.$set(_this, "entityOptionsLoading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "获取实体列表");
                });
        },

        // 实体名称下拉搜索事件
        entitySelectFilter(val) {
            if (val) {
                // 优化：提前转换搜索关键词，避免重复调用 toUpperCase()
                const searchVal = val.toUpperCase();
                this.entityOptions = this.entityOptionsCopy.filter((item) => {
                    const label = item.label || '';
                    const key = item.key || '';
                    // 优化：先尝试直接匹配（区分大小写），失败后再尝试不区分大小写匹配
                    return label.includes(val) || label.toUpperCase().includes(searchVal) ||
                        key.includes(val) || key.toUpperCase().includes(searchVal);
                });
            } else {
                this.entityOptions = this.entityOptionsCopy;
            }
        },

        // envirFrom下拉Change事件
        envirFromChange: function () {
            // 优化：合并多个 $set 调用，减少响应式更新次数
            // 环境切换时清空查询结果和缓存（因为不同环境的元数据可能不同）
            this.$set(this, "tableData", []);
            this.$set(this, "tableColumns", []);
            this.$set(this, "fieldMetadataMap", {});
            this.$set(this, "entityMetadataCache", {}); // 清空缓存
            this.$set(this, "entityMetadataPromises", {}); // 清空正在进行的查询Promise缓存
            this.$set(this, "tableDataTotalRecord", 0);
            this.$set(this.input, "pageIndex", 1);
            // 优化：环境切换时列结构会变化，需要更新 tableColumnsKey
            this.$set(this, "tableColumnsKey", this.tableColumnsKey + 1);
            // 清空实体视图相关数据
            this.$set(this, "entityViewOptions", []);
            this.$set(this, "entityViewOptionsCopy", []);
            this.$set(this.input, "entityViewObj", null);
            this.$set(this, "currentEntityViewLayoutXml", null);

            // 重新获取实体名称
            this.getEntityOptions();
        },

        // 执行查询
        executeQuery: function () {
            let _this = this;

            // 验证输入
            if (this.rtcrm.isNullOrWhiteSpace(this.input.fetchXml)) {
                this.jshelper.openAlertDialog(this, "请输入 FetchXml 查询字符串", "FetchXml 查询");
                return;
            }

            // 查询按钮点击时重置页码为1
            this.$set(this.input, "pageIndex", 1);

            // 清空之前的数据（但不清空缓存）
            this.$set(this, "tableData", []);
            this.$set(this, "tableColumns", []);
            this.$set(this, "fieldMetadataMap", {});
            this.$set(this, "tableDataTotalRecord", 0);
            // 优化：新查询时列结构会变化，需要更新 tableColumnsKey（列会在 generateTableColumns 中更新）

            // 开始查询
            this.$set(this, "loading", true);

            // 解析 FetchXml
            let parsed;
            try {
                parsed = this.parseFetchXml(this.input.fetchXml);
            } catch (error) {
                _this.$set(_this, "loading", false);
                this.jshelper.openAlertDialog(this, "FetchXml 解析失败: " + error.message, "FetchXml 查询");
                return;
            }

            // 收集需要查询元数据的实体和字段
            const entitiesToQuery = [];

            // 添加主实体（如果有字段）
            if (parsed.attributes && parsed.attributes.length > 0) {
                entitiesToQuery.push({
                    entityName: parsed.entityName,
                    alias: null,
                    attributes: parsed.attributes,
                    isLinkEntity: false
                });
            }

            // 递归收集所有有字段的 linkEntity
            const collectLinkEntities = (linkEntities) => {
                linkEntities.forEach(linkEntity => {
                    // 只有当 linkEntity 有字段时才查询
                    if (linkEntity.attributes && linkEntity.attributes.length > 0) {
                        entitiesToQuery.push({
                            entityName: linkEntity.entityName,
                            alias: linkEntity.alias,
                            attributes: linkEntity.attributes,
                            isLinkEntity: true
                        });
                    }
                    // 递归处理嵌套的 linkEntity
                    if (linkEntity.linkEntities && linkEntity.linkEntities.length > 0) {
                        collectLinkEntities(linkEntity.linkEntities);
                    }
                });
            };
            collectLinkEntities(parsed.linkEntities);

            // 如果没有需要查询的实体，直接返回
            if (entitiesToQuery.length === 0) {
                _this.$set(_this, "loading", false);
                this.jshelper.openAlertDialog(this, "FetchXml 中未找到任何字段", "FetchXml 查询");
                return;
            }

            // 并行执行：获取元数据和执行 FetchXml 查询
            Promise.all([
                // 获取所有实体的字段元数据（带缓存检查）
                this.getAllEntitiesMetadata(entitiesToQuery),
                // 执行 FetchXml 查询
                this.executeFetchXmlQuery(parsed)
            ])
                .then(([metadataResults, queryResult]) => {
                    // 优化：构建字段元数据映射，使用 Map 替代 find 操作，提升性能
                    const fieldMetadataMap = {};
                    // 为每个 metadata 数组构建 Map，以 logicalName 为 key
                    const metadataMaps = metadataResults.map(metadata => {
                        if (!metadata || !Array.isArray(metadata)) {
                            return new Map();
                        }
                        const map = new Map();
                        metadata.forEach(m => {
                            if (m.logicalName) {
                                map.set(m.logicalName, m);
                            }
                        });
                        return map;
                    });

                    entitiesToQuery.forEach((entityInfo, index) => {
                        const metadataMap = metadataMaps[index];
                        if (metadataMap && metadataMap.size > 0) {
                            entityInfo.attributes.forEach(attrName => {
                                // 优化：使用 Map.get 替代 find 操作，从 O(n) 降低到 O(1)
                                const attrMetadata = metadataMap.get(attrName);
                                if (attrMetadata) {
                                    // 使用实体名和字段名作为key，如果是linkEntity则使用alias
                                    const key = entityInfo.isLinkEntity && entityInfo.alias
                                        ? `${entityInfo.alias}.${attrName}`
                                        : `${entityInfo.entityName}.${attrName}`;
                                    fieldMetadataMap[key] = {
                                        ...attrMetadata,
                                        entityName: entityInfo.entityName,
                                        alias: entityInfo.alias,
                                        isLinkEntity: entityInfo.isLinkEntity,
                                        fieldName: attrName
                                    };
                                }
                            });
                        }
                    });

                    _this.$set(_this, "fieldMetadataMap", fieldMetadataMap);

                    // 根据元数据生成表格列
                    _this.generateTableColumns(entitiesToQuery, fieldMetadataMap);

                    // 所有操作完成后，绑定数据到表格
                    _this.$set(_this, "loading", false);
                    if (queryResult && queryResult.entities) {
                        // 此时 fieldMetadataMap 已经构建完成，可以用于数据转换
                        const transformedData = _this.transformCRMEntityData(queryResult.entities, parsed, fieldMetadataMap);

                        // 处理分页信息
                        const totalCount = queryResult.totalRecordCount;
                        if (totalCount === -1) {
                            // 如果无法获取准确总数，使用当前页数据量 + 是否有更多记录
                            const currentCount = transformedData.length + (this.input.pageIndex - 1) * this.input.pageSize;
                            const hasMore = queryResult.moreRecords;
                            _this.$set(_this, "tableDataTotalRecord", hasMore ? currentCount + 1 : currentCount);
                        } else {
                            _this.$set(_this, "tableDataTotalRecord", totalCount);
                        }
                        _this.processQueryResult(transformedData);
                    }
                    // 优化：新查询时列结构已在 generateTableColumns 中更新 tableColumnsKey，这里不需要更新
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.jshelper.openAlertDialog(_this, "查询失败: " + err.message, "FetchXml 查询");
                });
        },

        // 获取所有实体的元数据（带缓存和去重）
        getAllEntitiesMetadata: function (entitiesToQuery) {
            let _this = this;
            const envir = this.input.envirFrom;

            // 去重：按实体名去重，确保同一个实体只查询一次
            const uniqueEntities = new Map();
            entitiesToQuery.forEach(entityInfo => {
                const entityKey = entityInfo.entityName;
                if (!uniqueEntities.has(entityKey)) {
                    uniqueEntities.set(entityKey, entityInfo);
                }
            });

            // 检查缓存并构建查询Promise数组（只处理去重后的实体）
            const metadataPromises = Array.from(uniqueEntities.values()).map(entityInfo => {
                const cacheKey = `${envir}_${entityInfo.entityName}`;

                // 检查缓存
                if (this.entityMetadataCache[cacheKey]) {
                    // 从缓存中获取完整的元数据
                    const cachedMetadata = this.entityMetadataCache[cacheKey];
                    // 返回Promise以保持接口一致
                    return Promise.resolve(cachedMetadata);
                }

                // 检查是否有正在进行的查询（避免并发重复查询）
                if (this.entityMetadataPromises[cacheKey]) {
                    // 如果有正在进行的查询，直接返回该Promise
                    return this.entityMetadataPromises[cacheKey];
                }

                // 缓存中没有，也没有正在进行的查询，需要发起新查询
                const queryPromise = this.getEntityAttributeMetadata(entityInfo.entityName, entityInfo.attributes)
                    .then((allMetadata) => {
                        // 查询成功后，缓存完整的元数据
                        _this.entityMetadataCache[cacheKey] = allMetadata;
                        // 清除正在进行的查询Promise缓存
                        delete _this.entityMetadataPromises[cacheKey];
                        return allMetadata;
                    })
                    .catch((err) => {
                        // 查询失败时，也要清除正在进行的查询Promise缓存
                        delete _this.entityMetadataPromises[cacheKey];
                        throw err;
                    });

                // 缓存正在进行的查询Promise
                this.entityMetadataPromises[cacheKey] = queryPromise;
                return queryPromise;
            });

            // 等待所有元数据查询完成（包括缓存和实际查询）
            return Promise.all(metadataPromises).then((results) => {
                // 将去重后的结果映射回原始 entitiesToQuery 的顺序
                const entityMap = new Map();
                Array.from(uniqueEntities.values()).forEach((entityInfo, index) => {
                    entityMap.set(entityInfo.entityName, results[index]);
                });

                // 按照原始 entitiesToQuery 的顺序返回结果
                return entitiesToQuery.map(entityInfo => entityMap.get(entityInfo.entityName));
            });
        },

        // 获取实体的字段元数据（返回完整的实体元数据，不进行字段过滤）
        getEntityAttributeMetadata: function (entityName, attributeNames) {
            let _this = this;
            return new Promise((resolve, reject) => {
                let input = {
                    envir: this.input.envirFrom,
                    entityName: entityName
                };

                this.jshelper
                    .invokeHiddenApiAsync("new_hbxn_common", "RetrieveEntityMetadata/GetAllAttributeMetadataFromEntity", JSON.stringify(input))
                    .then((res) => {
                        if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                            reject(new Error(`获取实体 ${entityName} 的元数据失败：返回数据为空`));
                            return;
                        }
                        if (res.isSuccess) {
                            // 返回完整的实体元数据（用于缓存）
                            let allMetadata = res.data;
                            resolve(allMetadata);
                        } else {
                            reject(new Error(`获取实体 ${entityName} 的元数据失败：${res.message}`));
                        }
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },

        // 根据元数据生成表格列
        generateTableColumns: function (entitiesToQuery, fieldMetadataMap) {
            const columns = [];

            // 找到主实体（用于判断主键字段）
            const mainEntity = entitiesToQuery.find(function (entity) {
                return !entity.isLinkEntity;
            });
            const mainEntityName = mainEntity ? mainEntity.entityName : null;
            // 主实体的主键字段格式：{entityName}id
            const mainEntityPrimaryKey = mainEntityName ? mainEntityName + "id" : null;

            // 优化：构建 entityOptionsMap，避免在循环中重复调用 find
            const entityOptionsMap = new Map();
            if (this.entityOptionsCopy && this.entityOptionsCopy.length > 0) {
                this.entityOptionsCopy.forEach(option => {
                    if (option.key) {
                        entityOptionsMap.set(option.key, option);
                    }
                });
            }

            entitiesToQuery.forEach(entityInfo => {
                entityInfo.attributes.forEach(attrName => {
                    // 如果是主实体的字段，且字段名是主实体的主键字段，则跳过（不显示）
                    if (!entityInfo.isLinkEntity && mainEntityPrimaryKey && attrName === mainEntityPrimaryKey) {
                        return; // 跳过主实体的主键字段
                    }

                    // 生成列的 prop（用于数据绑定）
                    // 主实体的字段通常不带前缀，linkEntity 的字段带 alias 前缀
                    let key;
                    let metadataKey; // 用于查找元数据的 key

                    if (entityInfo.isLinkEntity) {
                        // linkEntity 使用 alias 或实体名作为前缀，使用下划线连接（避免点号导致的绑定问题）
                        const prefix = entityInfo.alias || entityInfo.entityName;
                        key = `${prefix}_${attrName}`;
                        metadataKey = `${prefix}.${attrName}`; // 元数据查找仍使用点号格式
                    } else {
                        // 主实体字段不带前缀（实际数据格式）
                        key = attrName;
                        // 但元数据查找时使用 entityName.attrName
                        metadataKey = `${entityInfo.entityName}.${attrName}`;
                    }

                    const metadata = fieldMetadataMap[metadataKey];

                    // 判断是否为日期字段
                    const isDateTime = metadata && metadata.attributeType === 'DateTime';

                    // 生成列名：使用元数据中的 displayName，如果没有则使用字段名
                    let columnLabel = attrName;
                    if (metadata && metadata.displayName) {
                        columnLabel = metadata.displayName;
                    }

                    // 如果是日期字段，根据日期格式类型在 label 中添加格式说明
                    if (isDateTime && metadata.dateTimeFormat) {
                        // dateTimeFormat 可能的值：DateOnly（仅日期）或 DateAndTime（日期时间）
                        if (metadata.dateTimeFormat === 'DateOnly') {
                            columnLabel = `${columnLabel} `;
                        } else if (metadata.dateTimeFormat === 'DateAndTime') {
                            columnLabel = `${columnLabel} `;
                        }
                    }

                    // 如果是 linkEntity，尝试从 entityOptions 中获取实体显示名
                    if (entityInfo.isLinkEntity) {
                        // 优化：使用 Map.get 替代 find 操作
                        const entityOption = entityOptionsMap.get(entityInfo.entityName);

                        if (entityOption && entityOption.label) {
                            // 如果找到了实体显示名，使用格式：字段名(实体显示名)
                            columnLabel = `${columnLabel}(${entityOption.label})`;
                        } else {
                            // 如果没找到，使用原来的格式：alias.字段名 或 entityName.字段名
                            const prefix = entityInfo.alias || entityInfo.entityName;
                            columnLabel = `${prefix}.${columnLabel}`;
                        }
                    }

                    const column = {
                        prop: key,
                        label: columnLabel,
                        fieldName: attrName,
                        entityName: entityInfo.entityName,
                        alias: entityInfo.alias,
                        isLinkEntity: entityInfo.isLinkEntity,
                        metadata: metadata,
                        isDateTime: isDateTime,
                        dateTimeFormat: metadata ? metadata.dateTimeFormat : null
                    };
                    columns.push(column);
                });
            });

            // 如果存在layoutxml，根据layoutxml中的cell width动态调整列宽
            if (this.currentEntityViewLayoutXml) {
                this.applyLayoutXmlColumnWidths(columns);
            }

            this.$set(this, "tableColumns", columns);
            // 优化：列结构变化时更新 tableColumnsKey，触发表格列重新渲染
            this.$set(this, "tableColumnsKey", this.tableColumnsKey + 1);
        },

        // 根据layoutxml中的cell width动态调整列宽，并按layoutxml中的顺序排序
        applyLayoutXmlColumnWidths: function (columns) {
            try {
                // 解析layoutxml
                const parser = new DOMParser();
                const xmlDoc = parser.parseFromString(this.currentEntityViewLayoutXml, "text/xml");

                // 检查解析错误
                const parseError = xmlDoc.querySelector("parsererror");
                if (parseError) {
                    console.warn("解析layoutxml失败:", parseError.textContent);
                    return;
                }

                // 获取所有cell节点
                const cells = xmlDoc.querySelectorAll("cell");
                const cellWidthMap = {}; // 字段名到宽度的映射
                const cellOrderMap = {}; // 字段名到顺序索引的映射（用于排序）
                const cellNames = []; // 按顺序保存的字段名列表

                // 构建字段名到宽度和顺序的映射
                for (let i = 0; i < cells.length; i++) {
                    const cell = cells[i];
                    const cellName = cell.getAttribute("name");
                    const cellWidth = cell.getAttribute("width");
                    if (cellName) {
                        // 记录顺序
                        cellOrderMap[cellName] = i;
                        cellNames.push(cellName);
                        // 记录宽度
                        if (cellWidth) {
                            const width = parseInt(cellWidth, 10);
                            if (!isNaN(width) && width > 0) {
                                cellWidthMap[cellName] = width;
                            }
                        }
                    }
                }

                // 为每个列找到匹配的cell名称和顺序
                const columnOrderMap = new Map(); // 列到顺序索引的映射
                columns.forEach(column => {
                    let matchedCellName = null;
                    let matchedOrder = null;

                    // 尝试匹配字段名（可能是主实体字段或linkEntity字段）
                    // 1. 直接匹配字段名（主实体字段或linkEntity字段，如果layoutxml中cell的name就是字段名）
                    if (cellOrderMap.hasOwnProperty(column.fieldName)) {
                        matchedCellName = column.fieldName;
                        matchedOrder = cellOrderMap[column.fieldName];
                    }
                    // 2. 如果是linkEntity字段，尝试匹配 alias.fieldName 格式
                    else if (column.isLinkEntity && column.alias) {
                        const aliasFieldName = `${column.alias}.${column.fieldName}`;
                        if (cellOrderMap.hasOwnProperty(aliasFieldName)) {
                            matchedCellName = aliasFieldName;
                            matchedOrder = cellOrderMap[aliasFieldName];
                        }
                    }
                    // 3. 如果是linkEntity字段，尝试匹配 entityName.fieldName 格式（如果没有alias）
                    else if (column.isLinkEntity && !column.alias) {
                        const entityFieldName = `${column.entityName}.${column.fieldName}`;
                        if (cellOrderMap.hasOwnProperty(entityFieldName)) {
                            matchedCellName = entityFieldName;
                            matchedOrder = cellOrderMap[entityFieldName];
                        }
                    }
                    // 4. 尝试匹配列的prop（用于处理可能的其他格式）
                    else if (cellOrderMap.hasOwnProperty(column.prop)) {
                        matchedCellName = column.prop;
                        matchedOrder = cellOrderMap[column.prop];
                    }

                    // 如果找到匹配的cell，记录顺序并应用宽度
                    if (matchedCellName !== null && matchedOrder !== null) {
                        columnOrderMap.set(column, matchedOrder);
                        // 应用列宽
                        if (cellWidthMap.hasOwnProperty(matchedCellName)) {
                            column.width = cellWidthMap[matchedCellName];
                        }
                    } else {
                        // 如果没有匹配到，使用一个很大的索引，确保这些列排在最后
                        columnOrderMap.set(column, 999999);
                    }
                });

                // 根据layoutxml中的顺序对列进行排序
                columns.sort(function (a, b) {
                    const orderA = columnOrderMap.get(a) !== undefined ? columnOrderMap.get(a) : 999999;
                    const orderB = columnOrderMap.get(b) !== undefined ? columnOrderMap.get(b) : 999999;
                    return orderA - orderB;
                });
            } catch (error) {
                console.warn("应用layoutxml列宽和排序失败:", error);
            }
        },

        // 执行 FetchXml 查询（返回Promise）
        executeFetchXmlQuery: function (parsed) {
            let _this = this;

            return new Promise((resolve, reject) => {
                let input = {
                    envir: this.input.envirFrom,
                    fetchXml: this.input.fetchXml,
                    pageIndex: this.input.pageIndex,
                    pageSize: this.input.pageSize
                };

                this.jshelper
                    .invokeHiddenApiAsync("new_hbxn_common", "RetrieveCRMData/RetrieveCRMDataByFetchXml", input)
                    .then((res) => {
                        if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                            reject(new Error("返回数据为空"));
                            return;
                        }
                        if (res.isSuccess) {
                            let data = res.data;

                            // 处理返回的数据格式
                            // 注意：这里返回原始数据，数据转换在调用处进行（需要 fieldMetadataMap）
                            if (data && data.Entities) {
                                // 返回原始数据和处理后的分页信息
                                resolve({
                                    entities: data.Entities,
                                    totalRecordCount: data.TotalRecordCount,
                                    moreRecords: data.MoreRecords
                                });
                            } else if (Array.isArray(data)) {
                                // 直接返回数组数据（兼容旧格式）
                                resolve({
                                    entities: data,
                                    totalRecordCount: data.length,
                                    moreRecords: false
                                });
                            } else {
                                resolve({
                                    entities: [],
                                    totalRecordCount: 0,
                                    moreRecords: false
                                });
                            }
                        } else {
                            reject(new Error(res.message || "查询失败"));
                        }
                    })
                    .catch((err) => {
                        reject(err);
                    });
            });
        },

        // 处理查询结果
        processQueryResult: function (data) {
            if (this.rtcrm.isNull(data) || !Array.isArray(data)) {
                this.jshelper.openAlertDialog(this, "查询结果为空", "FetchXml 查询");
                return;
            }

            // 列已经通过元数据生成，直接绑定数据
            if (this.tableColumns && this.tableColumns.length > 0) {
                this.$set(this, "tableData", data);
            } else {
                // 如果没有列定义（理论上不应该发生），显示错误
                console.error("表格列未定义，无法显示数据");
                this.jshelper.openAlertDialog(this, "表格列未定义，无法显示数据", "FetchXml 查询");
            }
        },

        // CRM实体数据转换工具方法 - 将Attributes数组转换为扁平对象
        transformCRMEntityData: function (entities, parsed, fieldMetadataMap) {
            if (!entities || !Array.isArray(entities)) {
                console.warn("transformCRMEntityData: entities 为空或不是数组", entities);
                return [];
            }

            // 优化：使用 Map 替代对象，提升查找性能
            // 构建字段映射：将返回数据中的 Key 映射到表格列使用的 prop
            // 因为返回数据中的 Key 可能包含空格（如 "s. domainname"），需要规范化
            const fieldKeyMappingObj = this.buildFieldKeyMapping(parsed);
            const fieldKeyMapping = new Map();
            Object.keys(fieldKeyMappingObj).forEach(key => {
                fieldKeyMapping.set(key, fieldKeyMappingObj[key]);
            });

            // 优化：使用 Map 替代对象，并优化日期字段映射构建逻辑
            // 构建日期字段映射（用于判断字段是否为日期类型）
            const dateTimeFieldMap = new Map();
            if (fieldMetadataMap) {
                // 优化：减少循环次数，直接遍历 fieldMetadataMap
                for (const metadataKey in fieldMetadataMap) {
                    const metadata = fieldMetadataMap[metadataKey];
                    if (metadata && metadata.attributeType === 'DateTime') {
                        // 需要找到对应的 prop key（可能是主实体字段或 linkEntity 字段）
                        // 主实体字段：entityName.attrName -> attrName
                        // linkEntity 字段：alias.attrName -> alias_attrName
                        const parts = metadataKey.split('.');
                        if (parts.length === 2) {
                            const [prefix, attrName] = parts;
                            // 检查是否是 linkEntity（通过 isLinkEntity 和 alias 判断）
                            if (metadata.isLinkEntity && metadata.alias) {
                                // linkEntity 字段：使用 alias_attrName
                                const propKey = `${metadata.alias}_${attrName}`;
                                dateTimeFieldMap.set(propKey, {
                                    isDateTime: true,
                                    dateTimeFormat: metadata.dateTimeFormat
                                });
                            } else if (!metadata.isLinkEntity) {
                                // 主实体字段：直接使用 attrName
                                dateTimeFieldMap.set(attrName, {
                                    isDateTime: true,
                                    dateTimeFormat: metadata.dateTimeFormat
                                });
                            }
                        }
                    }
                }
            }

            const transformedData = entities.map(entity => {
                const flatEntity = {
                    Id: entity.Id,
                    LogicalName: entity.LogicalName
                };

                // 优化：合并 FormattedValues 处理，减少重复遍历
                // 先构建 FormattedValues 映射（用于后续优先使用，但日期字段除外）
                const formattedValuesMap = new Map();
                const formattedValuesKeyMap = new Map(); // 缓存规范化 key 到 mappedKey 的映射

                if (entity.FormattedValues && Array.isArray(entity.FormattedValues)) {
                    entity.FormattedValues.forEach(fv => {
                        // 规范化 Key：去除多余空格，统一格式
                        const normalizedKey = this.normalizeFieldKey(fv.Key);
                        // 查找映射关系，如果找到则使用映射后的 key，否则使用规范化后的 key
                        const mappedKey = fieldKeyMapping.get(normalizedKey) || normalizedKey;
                        // 缓存映射关系，避免后续重复计算
                        formattedValuesKeyMap.set(fv.Key, mappedKey);
                        // 如果是日期字段，不添加到 FormattedValues 映射中（日期字段优先使用 Attributes）
                        if (!dateTimeFieldMap.has(mappedKey)) {
                            formattedValuesMap.set(mappedKey, fv.Value);
                        }
                    });
                }

                // 转换Attributes数组为扁平对象
                // 注意：返回数据中的 Key 格式可能包含空格（如 "s. domainname"）
                // 需要映射到表格列使用的格式（如 "s.domainname"）
                // 日期字段优先从 Attributes 获取，其他字段优先使用 FormattedValues
                if (entity.Attributes && Array.isArray(entity.Attributes)) {
                    entity.Attributes.forEach(attr => {
                        // 优化：使用缓存的映射关系（如果 FormattedValues 中已处理过）
                        let mappedKey;
                        if (formattedValuesKeyMap.has(attr.Key)) {
                            mappedKey = formattedValuesKeyMap.get(attr.Key);
                        } else {
                            // 规范化 Key：去除多余空格，统一格式
                            const normalizedKey = this.normalizeFieldKey(attr.Key);
                            // 查找映射关系，如果找到则使用映射后的 key，否则使用规范化后的 key
                            mappedKey = fieldKeyMapping.get(normalizedKey) || normalizedKey;
                        }

                        // 判断是否为日期字段
                        const dateTimeFieldInfo = dateTimeFieldMap.get(mappedKey);
                        const isDateTimeField = !!dateTimeFieldInfo;

                        if (isDateTimeField) {
                            // 日期字段：优先从 Attributes 获取，并格式化
                            let actualValue = attr.Value;
                            if (attr.Value && typeof attr.Value === 'object' && attr.Value.hasOwnProperty('Value')) {
                                actualValue = attr.Value.Value;
                            }
                            // 保存原始日期值（用于格式化显示）
                            flatEntity[mappedKey] = actualValue;
                        } else {
                            // 非日期字段：如果 FormattedValues 中有对应的格式化值，优先使用格式化值
                            if (formattedValuesMap.has(mappedKey)) {
                                flatEntity[mappedKey] = formattedValuesMap.get(mappedKey);
                            } else {
                                // 处理 Attributes 中的原始值
                                // 优先提取易读值：兼容 AliasedValue/EntityReference（带 Name）
                                let actualValue = attr.Value;
                                if (attr.Value && typeof attr.Value === 'object') {
                                    // 1) EntityReference 或 AliasedValue 内含 Name/name
                                    if (attr.Value.hasOwnProperty('Name')) {
                                        actualValue = attr.Value.Name;
                                    } else if (attr.Value.hasOwnProperty('name')) {
                                        actualValue = attr.Value.name;
                                    }
                                    // 2) AliasedValue.Value 里再包 EntityReference/对象
                                    else if (attr.Value.hasOwnProperty('Value')) {
                                        const innerVal = attr.Value.Value;
                                        if (innerVal && typeof innerVal === 'object') {
                                            if (innerVal.hasOwnProperty('Name')) {
                                                actualValue = innerVal.Name;
                                            } else if (innerVal.hasOwnProperty('name')) {
                                                actualValue = innerVal.name;
                                            } else if (innerVal.hasOwnProperty('Value')) {
                                                actualValue = innerVal.Value;
                                            } else {
                                                actualValue = innerVal;
                                            }
                                        } else {
                                            actualValue = innerVal;
                                        }
                                    }
                                }
                                flatEntity[mappedKey] = actualValue;
                            }
                        }
                    });
                }

                // 添加 FormattedValues 中独有的字段（如果 Attributes 中没有对应字段，且不是日期字段）
                if (entity.FormattedValues && Array.isArray(entity.FormattedValues)) {
                    entity.FormattedValues.forEach(fv => {
                        // 优化：使用缓存的映射关系
                        const mappedKey = formattedValuesKeyMap.get(fv.Key);
                        if (mappedKey) {
                            // 如果 Attributes 中没有这个字段，且不是日期字段，则使用 FormattedValues 的值
                            if (!flatEntity.hasOwnProperty(mappedKey) && !dateTimeFieldMap.has(mappedKey)) {
                                flatEntity[mappedKey] = fv.Value;
                            }
                        }
                    });
                }

                return flatEntity;
            });

            return transformedData;
        },

        // 规范化字段 Key（去除多余空格，将点号替换为下划线）
        normalizeFieldKey: function (key) {
            if (!key) return key;
            // 去除所有多余空格，将点号替换为下划线
            // 例如："s. domainname" -> "s_domainname"
            return key.replace(/\s+/g, '').replace(/\.+/g, '_');
        },

        // 构建字段 Key 映射关系（将返回数据的 Key 映射到表格列的 prop）
        buildFieldKeyMapping: function (parsed) {
            const mapping = {};

            // 递归处理 linkEntity 字段映射（主实体字段直接使用字段名，不需要映射）
            const processLinkEntities = (linkEntities) => {
                linkEntities.forEach(linkEntity => {
                    if (linkEntity.attributes && linkEntity.attributes.length > 0) {
                        const prefix = linkEntity.alias || linkEntity.entityName;
                        linkEntity.attributes.forEach(attrName => {
                            // 生成标准格式的 key（表格列使用的格式，使用下划线）
                            const standardKey = `${prefix}_${attrName}`;

                            // 映射可能的返回格式到标准格式（下划线格式）
                            // 1. 标准格式：s.domainname -> s_domainname
                            const normalizedKey = this.normalizeFieldKey(`${prefix}.${attrName}`);
                            mapping[normalizedKey] = standardKey;

                            // 2. 带空格格式：s. domainname -> s_domainname（CRM 可能返回的格式）
                            const spacedKey = `${prefix}. ${attrName}`;
                            mapping[this.normalizeFieldKey(spacedKey)] = standardKey;

                            // 3. 实体名格式：systemuser.domainname -> s_domainname（如果没有 alias）
                            if (linkEntity.alias) {
                                const entityKey = `${linkEntity.entityName}.${attrName}`;
                                mapping[this.normalizeFieldKey(entityKey)] = standardKey;

                                // 4. 实体名带空格格式：systemuser. domainname -> s_domainname
                                const entitySpacedKey = `${linkEntity.entityName}. ${attrName}`;
                                mapping[this.normalizeFieldKey(entitySpacedKey)] = standardKey;
                            }
                        });
                    }
                    // 递归处理嵌套的 linkEntity
                    if (linkEntity.linkEntities && linkEntity.linkEntities.length > 0) {
                        processLinkEntities(linkEntity.linkEntities);
                    }
                });
            };
            processLinkEntities(parsed.linkEntities);

            return mapping;
        },

        // 解析 FetchXml 字符串
        parseFetchXml: function (fetchXmlString) {
            try {
                // 使用 DOMParser 解析 XML
                const parser = new DOMParser();
                const xmlDoc = parser.parseFromString(fetchXmlString, "text/xml");

                // 检查解析错误
                const parseError = xmlDoc.querySelector("parsererror");
                if (parseError) {
                    throw new Error("FetchXml 解析失败：" + parseError.textContent);
                }

                // 获取主 entity 节点
                const entityNode = xmlDoc.querySelector("entity");
                if (!entityNode) {
                    throw new Error("未找到 entity 节点");
                }

                // 解析主实体
                const result = this.parseEntityNode(entityNode, null);
                return result;
            } catch (error) {
                console.error("解析 FetchXml 失败:", error);
                throw error;
            }
        },

        // 递归解析 entity 节点（包括主实体和 link-entity）
        parseEntityNode: function (entityNode) {
            const result = {
                entityName: entityNode.getAttribute("name") || "",
                alias: entityNode.getAttribute("alias") || null,
                attributes: [],
                linkEntities: []
            };

            // 如果是 link-entity，提取额外属性
            if (entityNode.tagName === "link-entity") {
                result.from = entityNode.getAttribute("from") || "";
                result.to = entityNode.getAttribute("to") || "";
                result.linkType = entityNode.getAttribute("link-type") || "outer";
            }

            // 遍历所有子节点，提取直接子节点（不包括嵌套的）
            const childNodes = entityNode.childNodes;
            for (let i = 0; i < childNodes.length; i++) {
                const childNode = childNodes[i];

                // 只处理元素节点
                if (childNode.nodeType !== 1) continue;

                // 提取 attribute 节点（直接子节点）
                if (childNode.tagName === "attribute") {
                    const attrName = childNode.getAttribute("name");
                    if (attrName) {
                        result.attributes.push(attrName);
                    }
                }
                // 提取 link-entity 节点（直接子节点）
                else if (childNode.tagName === "link-entity") {
                    const linkEntity = this.parseEntityNode(childNode);
                    result.linkEntities.push(linkEntity);
                }
            }

            return result;
        },

        // 格式化日期时间值
        formatDateTimeValue: function (value, dateTimeFormat) {
            if (!value) return '';

            try {
                const date = new Date(value);
                if (isNaN(date.getTime())) {
                    return value; // 如果无法解析为日期，返回原值
                }

                // 根据日期格式类型进行格式化
                if (dateTimeFormat === 'DateOnly') {
                    // 仅日期：yyyy-MM-dd
                    return this.rtcrm.formatDate(date, "yyyy-MM-dd");
                } else {
                    // 日期时间：yyyy-MM-dd hh:mm:ss
                    return this.rtcrm.formatDate(date, "yyyy-MM-dd hh:mm:ss");
                }
            } catch (error) {
                return value; // 如果格式化失败，返回原值
            }
        },

        // 分页变更
        handleSizeChange: function (val) {
            this.$set(this.input, "pageSize", val);
            this.$set(this.input, "pageIndex", 1); // 重置到第一页
            // 重新执行查询（只查询数据，不重新获取元数据）
            this.executeQueryForPaging();
        },

        // 页码变更
        handleCurrentChange: function (val) {
            this.$set(this.input, "pageIndex", val);
            // 重新执行查询（只查询数据，不重新获取元数据）
            this.executeQueryForPaging();
        },

        // 分页查询（只查询数据，不重新获取元数据和列定义）
        executeQueryForPaging: function () {
            let _this = this;

            // 验证输入
            if (this.rtcrm.isNullOrWhiteSpace(this.input.fetchXml)) {
                return;
            }
            // 如果列定义不存在，需要完整查询
            if (!this.tableColumns || this.tableColumns.length === 0) {
                this.executeQuery();
                return;
            }

            // 开始查询
            this.$set(this, "loading", true);

            // 解析 FetchXml（用于数据转换）
            let parsed;
            try {
                parsed = this.parseFetchXml(this.input.fetchXml);
            } catch (error) {
                _this.$set(_this, "loading", false);
                this.jshelper.openAlertDialog(this, "FetchXml 解析失败: " + error.message, "FetchXml 查询");
                return;
            }

            // 只执行 FetchXml 查询（不重新获取元数据）
            this.executeFetchXmlQuery(parsed)
                .then((queryResult) => {
                    _this.$set(_this, "loading", false);
                    if (queryResult && queryResult.entities) {
                        // 分页查询时，需要重新转换数据（使用当前的 fieldMetadataMap）
                        const transformedData = _this.transformCRMEntityData(queryResult.entities, parsed, _this.fieldMetadataMap);

                        // 处理分页信息
                        const totalCount = queryResult.totalRecordCount;
                        if (totalCount === -1) {
                            // 如果无法获取准确总数，使用当前页数据量 + 是否有更多记录
                            const currentCount = transformedData.length + (this.input.pageIndex - 1) * this.input.pageSize;
                            const hasMore = queryResult.moreRecords;
                            _this.$set(_this, "tableDataTotalRecord", hasMore ? currentCount + 1 : currentCount);
                        } else {
                            _this.$set(_this, "tableDataTotalRecord", totalCount);
                        }
                        _this.processQueryResult(transformedData);
                        // 优化：分页查询时列结构不变，只更新数据，不需要更新 tableColumnsKey
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "FetchXml 查询");
                });
        },

        // 从实体 Attributes 数组中获取指定 key 的值（优化：提取为方法，避免重复创建函数）
        getAttributeValueFromEntity: function (entity, key) {
            if (!entity || !entity.Attributes || !Array.isArray(entity.Attributes)) return "";
            const attr = entity.Attributes.find(function (a) { return a.Key === key; });
            return attr ? attr.Value : "";
        },

        // 实体名称Change事件
        entityNameChange: function (val) {
            let entityName = "";
            let objecttypecode = -1;
            if (!this.rtcrm.isNull(val)) {
                entityName = val.key;
                objecttypecode = val.objecttypecode;
            }
            // 优化：合并多个 $set 调用
            this.$set(this.input, "entityName", entityName);
            this.$set(this.input, "objecttypecode", objecttypecode);
            // 清空实体视图选择
            this.$set(this.input, "entityViewObj", null);
            this.$set(this, "entityViewOptions", []);
            this.$set(this, "entityViewOptionsCopy", []);
            this.$set(this, "currentEntityViewLayoutXml", null);
        },

        // 查询实体视图
        queryEntityViews: function () {
            let _this = this;

            // 验证实体是否已选择
            if (this.rtcrm.isNullOrWhiteSpace(this.input.entityName) || this.input.objecttypecode === -1) {
                this.jshelper.openAlertDialog(this, "请先选择实体名称", "查询实体视图");
                return;
            }

            // 构建查询实体视图的FetchXml
            const fetchXml = this.buildEntityViewFetchXml();

            // 优化：合并多个 $set 调用
            this.$set(this, "entityViewOptions", []);
            this.$set(this, "entityViewOptionsCopy", []);
            this.$set(this.input, "entityViewObj", null);
            this.$set(this, "currentEntityViewLayoutXml", null);

            // 开始查询
            this.$set(this, "loading", true);
            this.$set(this, "entityViewOptionsLoading", true);

            let input = {
                envir: this.input.envirFrom,
                fetchXml: fetchXml,
                pageIndex: 1,
                pageSize: 5000 // 获取所有视图
            };

            this.jshelper
                .invokeHiddenApiAsync("new_hbxn_common", "RetrieveCRMData/RetrieveCRMDataByFetchXml", input)
                .then((res) => {
                    _this.$set(_this, "loading", false);
                    _this.$set(_this, "entityViewOptionsLoading", false);
                    if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                        this.jshelper.openAlertDialog(this, "返回数据为空", "查询实体视图");
                        return;
                    }
                    if (res.isSuccess) {
                        let data = res.data;
                        let views = [];
                        if (data && data.Entities && Array.isArray(data.Entities)) {
                            // 优化：使用提取的方法替代内联函数
                            views = data.Entities.map(entity => {
                                const view = {
                                    savedqueryid: entity.Id,
                                    name: _this.getAttributeValueFromEntity(entity, "name"),
                                    fetchxml: _this.getAttributeValueFromEntity(entity, "fetchxml"),
                                    layoutxml: _this.getAttributeValueFromEntity(entity, "layoutxml"),
                                    querytype: _this.getAttributeValueFromEntity(entity, "querytype") || null,
                                    returnedtypecode: _this.getAttributeValueFromEntity(entity, "returnedtypecode") || null
                                };
                                return view;
                            }).filter(function (view) { return view.name; }); // 过滤掉没有名称的视图
                        } else if (Array.isArray(data)) {
                            // 兼容旧格式
                            views = data.map(entity => {
                                const view = {
                                    savedqueryid: entity.Id || entity.savedqueryid,
                                    name: entity.name || _this.getAttributeValueFromEntity(entity, "name"),
                                    fetchxml: entity.fetchxml || _this.getAttributeValueFromEntity(entity, "fetchxml"),
                                    layoutxml: entity.layoutxml || _this.getAttributeValueFromEntity(entity, "layoutxml"),
                                    querytype: entity.querytype || _this.getAttributeValueFromEntity(entity, "querytype") || null,
                                    returnedtypecode: entity.returnedtypecode || _this.getAttributeValueFromEntity(entity, "returnedtypecode") || null
                                };
                                return view;
                            }).filter(function (view) { return view.name; });
                        }

                        _this.$set(_this, "entityViewOptions", views);
                        _this.$set(_this, "entityViewOptionsCopy", views);
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "查询实体视图");
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.$set(_this, "entityViewOptionsLoading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "查询实体视图");
                });
        },

        // 构建查询实体视图的FetchXml
        buildEntityViewFetchXml: function () {
            const objecttypecode = this.input.objecttypecode;
            const fetchXml = `<fetch mapping="logical" version="1.0">
  <entity name="savedquery">
    <attribute name="name" />
    <attribute name="fetchxml" />
    <attribute name="layoutxml" />
    <attribute name="querytype" />
    <attribute name="returnedtypecode" />
    <filter>
      <condition attribute="querytype" operator="eq" value="0" />
      <condition attribute="returnedtypecode" operator="eq" value="${objecttypecode}" />
    </filter>
  </entity>
</fetch>`;
            return fetchXml;
        },

        // 实体视图Change事件
        entityViewChange: function (val) {
            if (!this.rtcrm.isNull(val) && val.fetchxml) {
                // 将选中的视图的fetchxml赋值到查询框
                this.$set(this.input, "fetchXml", val.fetchxml);
                // 保存layoutxml，用于动态调整列宽
                this.$set(this, "currentEntityViewLayoutXml", val.layoutxml || null);
            } else {
                // 清空查询框
                this.$set(this.input, "fetchXml", "");
                // 清空layoutxml
                this.$set(this, "currentEntityViewLayoutXml", null);
            }
        },

        // 实体视图下拉搜索事件
        entityViewSelectFilter(val) {
            if (val) {
                // 优化：提前转换搜索关键词，避免重复调用 toUpperCase()
                const searchVal = val.toUpperCase();
                this.entityViewOptions = this.entityViewOptionsCopy.filter((item) => {
                    const name = item.name || '';
                    const savedqueryid = item.savedqueryid || '';
                    // 优化：使用 includes 替代 indexOf，更高效
                    return name.includes(val) || name.toUpperCase().includes(searchVal) ||
                        (savedqueryid && (savedqueryid.includes(val) || savedqueryid.toUpperCase().includes(searchVal)));
                });
            } else {
                this.entityViewOptions = this.entityViewOptionsCopy;
            }
        },

        // 格式化 FetchXml
        formatFetchXml: function () {
            if (this.rtcrm.isNullOrWhiteSpace(this.input.fetchXml)) {
                return;
            }

            try {
                // 使用 DOMParser 解析 XML
                const parser = new DOMParser();
                const xmlDoc = parser.parseFromString(this.input.fetchXml, "text/xml");

                // 检查解析错误
                const parseError = xmlDoc.querySelector("parsererror");
                if (parseError) {
                    this.jshelper.openAlertDialog(this, "XML 格式错误，无法格式化：" + parseError.textContent, "格式化 FetchXml");
                    return;
                }

                // 格式化 XML（添加缩进）
                const formattedXml = this.formatXmlString(xmlDoc.documentElement);

                // 更新输入框
                this.$set(this.input, "fetchXml", formattedXml);

                // 提示成功（可选）
                this.$message.success("FetchXml 格式化成功");
            } catch (error) {
                console.error("格式化 FetchXml 失败:", error);
                this.jshelper.openAlertDialog(this, "格式化失败：" + error.message, "格式化 FetchXml");
            }
        },

        // 格式化 XML 字符串（递归添加缩进）
        formatXmlString: function (node, indent = "") {
            const indentStep = "  "; // 使用 2 个空格作为缩进
            let result = "";

            if (node.nodeType === 1) {
                // 元素节点
                const tagName = node.tagName;
                const attributes = [];

                // 收集属性（按字母顺序排序，使输出更一致）
                if (node.attributes && node.attributes.length > 0) {
                    const attrArray = [];
                    for (let i = 0; i < node.attributes.length; i++) {
                        const attr = node.attributes[i];
                        attrArray.push(attr);
                    }
                    // 按属性名排序（可选，使输出更一致）
                    attrArray.sort((a, b) => a.name.localeCompare(b.name));
                    attrArray.forEach(attr => {
                        attributes.push(`${attr.name}="${attr.value}"`);
                    });
                }

                // 构建开始标签
                const attrString = attributes.length > 0 ? " " + attributes.join(" ") : "";
                const startTag = `<${tagName}${attrString}>`;

                // 处理子节点
                const childNodes = Array.from(node.childNodes);
                const elementChildren = childNodes.filter(n => n.nodeType === 1);
                const textChildren = childNodes.filter(n => n.nodeType === 3 && n.textContent.trim());

                if (elementChildren.length === 0 && textChildren.length === 0) {
                    // 自闭合标签
                    result = `${indent}${startTag.replace(">", " />")}`;
                } else if (elementChildren.length === 0 && textChildren.length > 0) {
                    // 只有文本内容
                    const textContent = textChildren.map(n => n.textContent.trim()).join(" ");
                    result = `${indent}${startTag}${textContent}</${tagName}>`;
                } else {
                    // 有子元素
                    result = `${indent}${startTag}\n`;
                    // 处理子节点（包括文本节点和元素节点）
                    childNodes.forEach(child => {
                        if (child.nodeType === 1) {
                            // 元素节点
                            const formatted = this.formatXmlString(child, indent + indentStep);
                            if (formatted) {
                                result += formatted + "\n";
                            }
                        } else if (child.nodeType === 3 && child.textContent.trim()) {
                            // 文本节点（非空白）
                            result += indent + indentStep + child.textContent.trim() + "\n";
                        }
                    });
                    result += `${indent}</${tagName}>`;
                }
            } else if (node.nodeType === 3) {
                // 文本节点
                const text = node.textContent.trim();
                if (text) {
                    result = indent + text;
                }
            }

            return result;
        },
    },
};
</script>

<style scoped>
.main-container {
    min-height: 100%;
    background-color: white;
}

.credit-title {
    margin-top: 15px;
    text-align: center;
}

.box-card {
    width: 100%;
}

/* 输入面板样式 */
.input-panel {
    overflow-y: auto;
}

.input-panel /deep/ .el-card__header {
    padding: 12px 20px 12px 20px;
}

.detail-header {
    font-weight: bold;
    color: #303133;
}

.detail-header a {
    color: inherit;
    text-decoration: none;
    cursor: pointer;
}

.detail-header a:hover {
    color: #409EFF;
    text-decoration: none;
}

.input-section {
    margin-bottom: 0;
}

.entityName-input-with-select {
    width: 300px;
    vertical-align: middle;

    .el-select {
        width: 160px;
    }
}

.entityName-input-with-select /deep/ .el-input-group__append {
    color: black;
    background-color: #fff;
}

.entityName-input-with-select /deep/ .el-input-group__prepend {
    color: black;
    background-color: #fff;
}

.entityView-input-with-select {
    width: 100%;
    vertical-align: middle;
}

.entityView-input-with-select /deep/ .el-button--small {
    border-radius: 0 3px 3px 0;
}

.entityView-input-with-select /deep/ .el-button--small {
    border-radius: 0 3px 3px 0;
}

.entityView-input-with-select /deep/ .el-input__inner {
    border-radius: 3px 0 0 3px;
}
</style>
