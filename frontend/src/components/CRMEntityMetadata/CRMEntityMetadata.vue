<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">CRM实体元数据查询</h3>
                <el-divider></el-divider>
            </el-header>
            <el-main>
                <el-form :model="input" :rules="rules" ref="form" size="medium" label-position="left">
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
                            <el-form-item prop="entityName" label="实体名称" label-width="80px" required>
                                <el-input size="small" placeholder="" :disabled="loading || entityOptionsLoading"
                                    readonly v-model="input.entityName" style="vertical-align: middle;"
                                    class="entityName-input-with-select">
                                    <el-select size="small" slot="prepend" clearable filterable
                                        :loading="loading || entityOptionsLoading" v-model="input.entityName"
                                        placeholder="请选择" :filter-method="entitySelectFilter">
                                        <el-option v-for="item in entityOptions" :key="item.key" :label="item.label"
                                            :value="item.key">
                                        </el-option>
                                    </el-select>
                                </el-input>
                            </el-form-item>
                        </el-col>
                        <!-- 按钮 -->
                        <el-col :span="4">
                            <el-form-item>
                                <el-button size="small" :loading="loading" @click="getData(null)">查询</el-button>
                            </el-form-item>
                        </el-col>
                    </el-row>
                </el-form>
                <!-- 展示内容 -->
                <el-row>
                    <el-row :gutter="10">
                        <el-col :span="24">
                            <el-card class="box-card">
                                <!-- 过滤行 -->
                                <el-row :gutter="24">
                                    <el-col :offset="16" :span="4">
                                        <el-input v-model="input.search" :disabled="loading" size="mini" clearable
                                            placeholder="输入关键字搜索" @change="dataSearchOnChange" />
                                    </el-col>
                                    <!-- 字段类型 -->
                                    <el-col :span="4">
                                        <el-select size="mini" clearable v-model="input.attributeType"
                                            placeholder="请选择字段类型" :disabled="loading || fieldOptionsLoading"
                                            @change="onFieldTypeChange">
                                            <el-option v-for="item in fieldOptions" :key="item.key" :label="item.label"
                                                :value="item.key">
                                            </el-option>
                                        </el-select>
                                    </el-col>
                                </el-row>
                                <!-- 数据展示 - 左右分栏布局 -->
                                <el-row :gutter="24" style="margin-top: 15px">
                                    <!-- 左侧表格 -->
                                    <el-col :span="14">
                                        <el-table :height="tableHeight" :data="tableData.ecFrom"
                                            :key="tableKey + '_envirFrom'" border style="width: 100%"
                                            v-loading="loading"
                                            :default-sort="{ prop: 'logicalName', order: 'ascending' }"
                                            @row-click="handleRowClick" highlight-current-row>
                                            <!-- 行号 -->
                                            <el-table-column type="index" align="center" show-overflow-tooltip
                                                width="60">
                                            </el-table-column>
                                            <!-- 逻辑名 -->
                                            <el-table-column prop="logicalName" label="逻辑名" sortable
                                                show-overflow-tooltip width="280">
                                            </el-table-column>
                                            <!-- 显示名称 -->
                                            <el-table-column prop="displayName" label="显示名称" show-overflow-tooltip
                                                width="260">
                                            </el-table-column>
                                            <!-- 字段类型 -->
                                            <el-table-column prop="attributeType" label="字段类型" show-overflow-tooltip
                                                width="100">
                                            </el-table-column>
                                            <!-- 字段要求 -->
                                            <el-table-column prop="requiredLevel" label="字段要求" show-overflow-tooltip>
                                            </el-table-column>
                                        </el-table>
                                    </el-col>
                                    <!-- 右侧详细信息面板 -->
                                    <el-col :span="10">
                                        <el-card class="detail-panel" v-if="selectedRow"
                                            :style="{ height: tableHeight }">
                                            <div slot="header" class="detail-header">
                                                <span>字段详细信息</span>
                                            </div>
                                            <!-- 基础信息 -->
                                            <div class="detail-section">
                                                <h4>基础信息</h4>
                                                <el-row :gutter="10">
                                                    <el-col :span="12">
                                                        <div class="detail-item">
                                                            <label>逻辑名:</label>
                                                            <span>{{ selectedRow.logicalName }}</span>
                                                        </div>
                                                    </el-col>
                                                    <el-col :span="12">
                                                        <div class="detail-item">
                                                            <label>显示名称:</label>
                                                            <span>{{ selectedRow.displayName }}</span>
                                                        </div>
                                                    </el-col>
                                                </el-row>
                                                <el-row :gutter="10">
                                                    <el-col :span="12">
                                                        <div class="detail-item">
                                                            <label>字段类型:</label>
                                                            <span>{{ selectedRow.attributeType }}</span>
                                                        </div>
                                                    </el-col>
                                                    <el-col :span="12">
                                                        <div class="detail-item">
                                                            <label>字段要求:</label>
                                                            <span>{{ selectedRow.requiredLevel }}</span>
                                                        </div>
                                                    </el-col>
                                                </el-row>
                                            </div>
                                            <!-- 详细信息 -->
                                            <div class="detail-section">
                                                <h4>详细信息</h4>
                                                <div v-if="selectedRow.attributeType === 'Boolean'"
                                                    class="detail-content">
                                                    <!-- Boolean类型 -->
                                                    <div class="detail-item">
                                                        <label>默认值:</label>
                                                        <span>{{ selectedRow.defaultValueStr || '无' }}</span>
                                                    </div>
                                                </div>
                                                <div v-else-if="selectedRow.attributeType === 'DateTime'"
                                                    class="detail-content">
                                                    <!-- DateTime类型 -->
                                                    <div class="detail-item">
                                                        <label>格式:</label>
                                                        <span>{{ selectedRow.dateTimeFormat || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>行为:</label>
                                                        <span>{{ selectedRow.dateTimeBehavior || '无' }}</span>
                                                    </div>
                                                </div>
                                                <div v-else-if="['Decimal', 'Integer', 'Double', 'Money'].includes(selectedRow.attributeType)"
                                                    class="detail-content">
                                                    <!-- Number类型 -->
                                                    <div class="detail-item">
                                                        <label>精度:</label>
                                                        <span>{{ selectedRow.precision || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>最小值:</label>
                                                        <span>{{ selectedRow.minimum || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>最大值:</label>
                                                        <span>{{ selectedRow.maximum || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>是否Money:</label>
                                                        <span>{{ selectedRow.isMonryStr || '无' }}</span>
                                                    </div>
                                                </div>
                                                <div v-else-if="selectedRow.attributeType === 'Lookup'"
                                                    class="detail-content">
                                                    <!-- Lookup类型 -->
                                                    <div class="detail-item">
                                                        <label>关联实体逻辑名:</label>
                                                        <span>{{ selectedRow.linkedEntityName || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>关联实体名称:</label>
                                                        <span>{{ selectedRow.displayName || '无' }}</span>
                                                    </div>
                                                </div>
                                                <div v-else-if="['Picklist', 'Status', 'State', 'MultiSelectPicklist'].includes(selectedRow.attributeType)"
                                                    class="detail-content">
                                                    <!-- Picklist类型 -->
                                                    <div class="detail-item">
                                                        <label>选项值:</label>
                                                        <div class="text-content text-ellipsis"
                                                            style="white-space:normal;">
                                                            <div class="text-inner text-ellipsis"
                                                                style="white-space:normal;">
                                                                <span>{{ selectedRow.optionsStr || '无' }}</span>
                                                            </div>
                                                            <i v-show="!rtcrm.isNullOrWhiteSpace(selectedRow.optionsStr)"
                                                                class="el-icon-copy-document text-icon"
                                                                style="cursor: pointer;" title="复制"
                                                                @click="copyToClipboard(selectedRow.optionsStr)"></i>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div v-else-if="['Memo', 'String'].includes(selectedRow.attributeType)"
                                                    class="detail-content">
                                                    <!-- String类型 -->
                                                    <div class="detail-item">
                                                        <label>最大长度:</label>
                                                        <span>{{ selectedRow.strLength || '无' }}</span>
                                                    </div>
                                                    <div class="detail-item">
                                                        <label>格式:</label>
                                                        <span>{{ selectedRow.strFormat || '无' }}</span>
                                                    </div>
                                                </div>
                                                <div v-else class="detail-content">
                                                    <p class="no-detail">该字段类型暂无详细信息</p>
                                                </div>
                                            </div>
                                        </el-card>
                                        <el-card v-else class="detail-panel" :style="{ height: tableHeight }">
                                            <div class="no-selection">
                                                <i class="el-icon-info"></i>
                                                <p>请点击左侧表格中的行查看详细信息</p>
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
export default {
    name: 'CRMEntityMetadata',
    data() {
        // 实体名称不能为空
        const entityName_rule = (rule, value, callback) => {
            if (this.rtcrm.isNullOrWhiteSpace(value)) {
                callback(new Error("实体名称不能为空不能为空！"));
            } else {
                callback();
            }
        };
        return {
            msg: "hello world",
            // 环境
            environments: [{ label: "无效环境请刷新", key: "undefined" }],
            // 实体名称
            entityOptions: [],
            entityOptionsCopy: [],
            // 字段类型
            fieldOptions: [],
            // 输入
            input: {
                envirFrom: "dev",
                entityName: "",
                attributeType: "",
                search: "",
            },
            tableData: {
                ecFrom: [],
                ecFromCopy: [],
                ecFromTotalRecord: 0,
            }, //数据
            defaultTableHeight: "520", //表格高度
            tableKey: 1, //刷新表格的Key
            loading: false, //是否加载数据中
            entityOptionsLoading: false, //是否加载实体名称中
            fieldOptionsLoading: false, //是否加载字段类型中
            selectedRow: null, //选中的行数据
            // 表单校验规则
            rules: {
                entityName: [{ validator: entityName_rule, trigger: "change" }],
            },
        };
    },
    created() {
        //获取系统参数
        this.getEnvironments();

        //获取实体名称
        this.getEntityOptions();

        //获取字段类型
        this.getFieldOptions();
    },
    mounted() {
        //监听环境参数切换
        this.$on('environment-change', this.environmentChange);
    },
    computed: {
        // 检测是否为桌面端环境（复用 JsCrmHelper 的方法）
        isDesktop() {
            return this.jshelper && this.jshelper.isDesktopEnvironment
                ? this.jshelper.isDesktopEnvironment()
                : false;
        },
        // Table高度
        tableHeight() {
            let height = parseInt(this.defaultTableHeight);
            if (this.isDesktop) return height + 100 + "px";
            return height + "px";
        },
    },
    methods: {
        //环境切换事件
        environmentChange: function (envir) {
            if (this.rtcrm.isNullOrWhiteSpace(envir)) return;

            //清空数据
            this.$set(this.tableData, "ecFrom", []);
            this.$set(this.tableData, "ecFromTotalRecord", 0);
            this.$set(this, "input", {
                envirFrom: "dev",
                entityName: "",
                attributeType: "",
                search: "",
            });
            this.$set(this, "selectedRow", null); //清空选中的行
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table

            //重新获取环境参数
            this.getEnvironments();

            //重新获取实体名称
            this.getEntityOptions();
        },

        //获取环境参数
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

        //获取实体名称
        getEntityOptions: function () {
            let _this = this;
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
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "获取实体列表");
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "获取实体列表");
                });
        },

        //获取字段类型
        getFieldOptions: function () {
            let _this = this;
            this.$set(this, "loading", true);
            this.$set(this, "fieldOptionsLoading", true);
            this.$set(this, "fieldOptions", []);

            this.jshelper
                .invokeHiddenApiAsync(
                    "new_hbxn_common",
                    "RetrieveEntityMetadata/GetAttributeTypeList",
                    null
                )
                .then((res) => {
                    _this.$set(_this, "loading", false);
                    _this.$set(_this, "fieldOptionsLoading", false);
                    if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                        this.jshelper.openAlertDialog(this,
                            "返回数据为空", "获取字段类型"
                        );
                        return;
                    }
                    if (res.isSuccess) {
                        let data = res.data;
                        if (!this.rtcrm.isNull(data)) _this.$set(_this, "fieldOptions", data);
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "获取字段类型");
                    }
                })
                .catch((err) => {
                    _this.$set(_this, "loading", false);
                    _this.jshelper.openAlertDialog(_this, err.message, "获取字段类型");
                });
        },

        //查询
        getData: function (envir) {
            let _this = this;
            this.$refs["form"].validate(async (valid, error) => {
                if (valid) {
                    //#region 清空数据
                    this.$set(this.tableData, "ecFrom", []);
                    this.$set(this.tableData, "ecFromCopy", []);
                    this.$set(this.tableData, "ecFromTotalRecord", 0);
                    this.$set(this.input, "search", "");
                    this.$set(this, "selectedRow", null); //清空选中的行
                    this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
                    //#endregion

                    //#region 查询数据
                    this.$set(this, "loading", true);
                    let input = {
                        envir: this.input.envirFrom,
                        entityName: this.input.entityName,
                        attributeType: this.input.attributeType
                    };
                    this.jshelper
                        .invokeHiddenApiAsync("new_hbxn_common", "RetrieveEntityMetadata/GetAllAttributeMetadataFromEntity", JSON.stringify(input))
                        .then((res) => {
                            _this.$set(_this, "loading", false);
                            if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                                this.jshelper.openAlertDialog(this,
                                    "返回数据为空", "CRM实体字段元数据查询"
                                );
                                return;
                            }
                            if (res.isSuccess) {
                                let data = res.data;
                                _this.$set(_this.tableData, "ecFrom", data);
                                _this.$set(_this.tableData, "ecFromCopy", data);
                            } else {
                                this.jshelper.openAlertDialog(this, res.message, "CRM实体字段元数据查询");
                            }
                            _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
                        })
                        .catch((err) => {
                            _this.$set(_this, "loading", false);
                            _this.jshelper.openAlertDialog(_this, err.message, "CRM实体字段元数据查询");
                        });
                    //#endregion
                }
            });
        },

        //获取环境的Label
        GetEnvirLabel: function (val) {
            let obj = { label: "无效环境请刷新", key: "undefined" };
            let _obj = this.environments.find((item) => {
                return item.key === val;
            });
            return !this.rtcrm.isNull(_obj) ? _obj : obj;
        },

        //envirFrom下拉Change事件
        envirFromChange: function () {
            //重新获取实体名称
            this.getEntityOptions();
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$message({
                message: message,
                dangerouslyUseHTMLString: dangerouslyUseHTMLString,
                type: type,
            });
        },

        //实体名称下拉搜索事件
        entitySelectFilter(val) {
            if (val) {
                this.entityOptions = this.entityOptionsCopy.filter((item) => {
                    if (!!~item.label.indexOf(val) || !!~item.label.toUpperCase().indexOf(val.toUpperCase())) {
                        return true
                    }
                    else if (!!~item.key.indexOf(val) || !!~item.key.toUpperCase().indexOf(val.toUpperCase())) {
                        return true
                    }
                })
            } else {
                this.entityOptions = this.entityOptionsCopy;
            }
        },

        //table模糊搜索事件
        dataSearchOnChange(val) {
            let filteredData = this.tableData.ecFromCopy;

            // 关键字过滤
            if (!this.rtcrm.isNullOrWhiteSpace(val)) {
                filteredData = filteredData.filter(item => {
                    return item.logicalName.indexOf(val) !== -1 ||
                        item.displayName.indexOf(val) !== -1;
                });
            }

            // 字段类型过滤
            if (!this.rtcrm.isNull(this.input.attributeType) &&
                !this.rtcrm.isNullOrWhiteSpace(this.input.attributeType.toString())) {
                filteredData = filteredData.filter(item => {
                    return this.matchFieldType(item.attributeType, this.input.attributeType);
                });
            }

            this.$set(this.tableData, "ecFrom", filteredData);
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
        },

        //复制到剪贴板
        copyToClipboard(val) {
            var textarea = document.createElement('textarea');
            textarea.style.position = 'fixed';
            textarea.style.opacity = 0;
            textarea.value = val;
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);

            this.showMessage("复制成功", "success");
        },

        //处理行点击事件
        handleRowClick(row) {
            this.selectedRow = row;
        },

        //字段类型匹配方法
        matchFieldType(actualType, selectedType) {
            // 字段类型映射关系 - 使用数字键对应getFieldOptions返回的key值
            const typeMapping = {
                0: ['Boolean'],                                // Boolean
                1: ['DateTime'],                               // DateTime
                2: ['Decimal', 'Integer', 'Double', 'Money'],  // Number
                3: ['Lookup'],                                 // Lookup
                4: ['Picklist', 'Status', 'State', 'MultiSelectPicklist'], // Picklist
                5: ['Memo', 'String']                          // String
            };

            // 通过映射关系匹配
            if (typeMapping[selectedType] && typeMapping[selectedType].includes(actualType)) {
                return true;
            }

            return false;
        },

        //字段类型变化处理方法
        onFieldTypeChange() {
            // 触发数据过滤
            this.dataSearchOnChange(this.input.search);
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

.required {
    color: red;
}

.text {
    font-size: 14px;
}

.item {
    margin-bottom: 18px;
}

.clearfix:before,
.clearfix:after {
    display: table;
    content: "";
}

.clearfix:after {
    clear: both;
}

.box-card {
    width: 100%;
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

.text-content {
    position: relative;
    width: 100%;
}

.text-ellipsis {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.text-inner {
    display: inline-block;
    width: calc(95% - 20px);
}

.text-icon {
    width: 4%;
    text-align: right;
    vertical-align: super;
}

.text-icon:hover {
    color: #409EFF;
}

/* 详细信息面板样式 */
.detail-panel {
    overflow-y: auto;
}

.detail-header {
    font-weight: bold;
    color: #303133;
}

.detail-section {
    margin-bottom: 20px;
}

.detail-section h4 {
    margin: 0 0 15px 0;
    color: #606266;
    font-size: 14px;
    border-bottom: 1px solid #EBEEF5;
    padding-bottom: 8px;
}

.detail-item {
    margin-bottom: 10px;
    display: flex;
    align-items: center;
}

.detail-item label {
    font-weight: bold;
    color: #606266;
    min-width: 100px;
    margin-right: 10px;
    font-size: 13px;
}

.detail-item span {
    color: #303133;
    font-size: 13px;
    flex: 1;
}

.detail-content {
    padding: 10px 0;
}

.no-detail {
    color: #909399;
    font-style: italic;
    text-align: center;
    margin: 20px 0;
}

.no-selection {
    text-align: center;
    padding: 60px 20px;
    color: #909399;
}

.no-selection i {
    font-size: 48px;
    margin-bottom: 20px;
    display: block;
}

.no-selection p {
    margin: 0;
    font-size: 14px;
}
</style>