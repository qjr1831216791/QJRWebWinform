<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">CRM树形数据图表</h3>
                <el-divider></el-divider>
            </el-header>
            <el-main>
                <el-form :model="input" :rules="rules" ref="form" label-position="left">
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

                        <!-- 实体： -->
                        <el-col :span="8">
                            <el-form-item prop="entityName" label="实体：" label-width="80px">
                                <el-select size="small" v-model="input.entityName" placeholder="请选择" :disabled="loading"
                                    @change="entityNameChange">
                                    <el-option v-for="item in entityListForTree" :key="item.key" :label="item.label"
                                        :value="item.key">
                                    </el-option>
                                </el-select>
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
                            <el-card class="box-card" :key="tableKey">
                                <!-- 数据展示 -->
                                <el-container ref="dataContainer" id="dataContainer">
                                    <!-- 树形图 -->
                                    <el-aside width="450px" :style="{ maxHeight: tableHeight }">
                                        <el-tree :style="[treeWidthStyle]" :data="tableData.treeData"
                                            :expand-on-click-node="false" :highlight-current="true"
                                            :props="treeDefaultProps" :disabled="loading" :default-expand-all="true"
                                            @node-click="handleNodeClick"></el-tree>
                                    </el-aside>
                                    <!-- 数据详细信息 -->
                                    <el-main>
                                        <el-form ref="detailForm" :inline="true" size="mini"
                                            :model="tableData.selectTreeNode" label-width="120px" label-position="left">
                                            <el-row v-for="(cfRow, cfRowInd) in tableData.selectTreeNodeForm"
                                                :key="'cfRowInd_' + cfRowInd">
                                                <el-col :span="8" :key="'cfColInd_' + cfColInd"
                                                    v-for="(cfCol, cfColInd) in cfRow">
                                                    <el-form-item :label="cfCol.displayName">
                                                        <el-input
                                                            v-if="tableData.selectTreeNode[cfCol.logicalName + '_formatted']"
                                                            readonly size="mini"
                                                            v-model="tableData.selectTreeNode[cfCol.logicalName + '_formatted']"></el-input>
                                                        <el-input v-else readonly size="mini"
                                                            v-model="tableData.selectTreeNode[cfCol.logicalName]"></el-input>
                                                    </el-form-item>
                                                </el-col>
                                            </el-row>
                                        </el-form>
                                    </el-main>
                                </el-container>
                            </el-card>
                        </el-col>
                    </el-row>
                </el-row>
            </el-main>
        </el-container>
    </div>
</template>

<script>
import { Loading } from 'element-ui';
import BaseData from './BaseData.json';
export default {
    name: 'CRMTreeGraph',
    data() {
        return {
            // 环境
            environments: [{ label: "无效环境请刷新", key: "undefined" }],
            // 输入
            input: {
                envirFrom: "dev",
                entityName: "businessunit",
            },
            tableData: {
                ecFrom: [],
                ecFromCopy: [],
                ecFromTotalRecord: 0,
                treeData: [],
                selectTreeNode: {},
                selectTreeNodeForm: [],
                treeDepth: 10,
            }, //数据
            treeDefaultProps: {
                children: 'children',
                label: 'label'
            },
            defaultTableHeight: "550", //表格高度
            tableKey: 1, //刷新表格的Key
            loading: false, //是否加载数据中
            loadingInstance: null,//loading实例
            detailTableConfig: {
                columnCount: 3,
                columnCountArray: [],
                rowCountArray: [],
            },//详情表单配置
            // 表单校验规则
            rules: {
            },
            BaseData: null,//基础配置
            myBaseData: null,//当前环境的基础配置
        };
    },
    created() {
        //获取基础配置
        this.BaseDataInit();

        //获取系统参数
        this.getEnvironments();
    },
    mounted() {
        //监听环境参数切换
        this.$on('environment-change', this.environmentChange);
    },
    computed: {
        //可选的树结构实体
        entityListForTree: function () {
            // 优化：使用 map 替代 for...of 循环，代码更简洁
            if (this.rtcrm.isNull(this.myBaseData) || this.myBaseData.length === 0) return [];

            return this.myBaseData.map(item => ({
                label: item.displayName,
                key: item.logicalName,
            }));
        },

        //当前实体的配置
        currentEntData: function () {
            // 优化：使用 find 替代 for...of 循环，代码更简洁
            if (this.rtcrm.isNull(this.myBaseData) || this.myBaseData.length === 0) return {};

            return this.myBaseData.find(item => item.logicalName === this.input.entityName) || {};
        },

        //计算树形图的宽度
        treeWidthStyle: function () {
            return {
                minWidth: this.tableData.treeDepth * 18 + 100 + "px",
            };
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
            if (this.isDesktop) return height + 110 + "px";
            return height + "px";
        },
    },
    watch: {
        // 监听 loading 属性
        loading(newVal, oldVal) {
            if (newVal === true && this.$refs.dataContainer) {
                this.$nextTick(() => {
                    let loadingInstance = Loading.service({ target: this.$refs.dataContainer["$el"], lock: true });
                    this.$set(this, "loadingInstance", loadingInstance)
                });
            }
            else if (newVal === false && this.loadingInstance) {
                this.$nextTick(() => {
                    // 以服务的方式调用的 Loading 需要异步关闭
                    this.loadingInstance.close();
                });
            }
        }
    },
    methods: {
        //配置文件初始化
        BaseDataInit: function () {
            if (!this.rtcrm.isNull(BaseData)) {
                this.$set(this, "BaseData", BaseData);
            }
            else {
                this.$set(this, "BaseData", null);
            }

            // 优化：使用 Object.prototype.hasOwnProperty.call 替代直接使用 hasOwnProperty，更安全
            if (this.rtcrm.isNull(this.BaseData))
                this.$set(this, "myBaseData", []);
            else if (!Object.prototype.hasOwnProperty.call(this.BaseData, this.$globalVar["selectEnv"]))
                this.$set(this, "myBaseData", []);
            else
                this.$set(this, "myBaseData", this.BaseData[this.$globalVar["selectEnv"]]);

            //计算当前详情表单的行列数
            this.getDetailTableSize();

            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
        },

        //环境切换事件
        environmentChange: function (envir) {
            if (this.rtcrm.isNullOrWhiteSpace(envir)) return;

            //清空数据
            this.$set(this.tableData, "ecFrom", []);
            this.$set(this.tableData, "ecFromTotalRecord", 0);
            this.$set(this, "input", {
                envirFrom: "dev",
            });
            this.$set(this.tableData, "selectTreeNode", {});
            this.$set(this.tableData, "selectTreeNodeForm", []);
            this.$set(this.tableData, "treeData", []);
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table

            //重新获取环境参数
            this.getEnvironments();

            //重新获取配置文件
            this.BaseDataInit();
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

        //查询
        getData: function () {
            let _this = this;
            this.$refs["form"].validate(async (valid, error) => {
                if (valid) {
                    //#region 清空数据
                    this.$set(this.tableData, "ecFrom", []);
                    this.$set(this.tableData, "ecFromCopy", []);
                    this.$set(this.tableData, "selectTreeNode", {});
                    this.$set(this.tableData, "treeData", []);
                    this.$set(this.tableData, "ecFromTotalRecord", 0);
                    this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
                    //#endregion

                    //#region 查询数据
                    this.$set(this, "loading", true);
                    let inputParam = {
                        envir: this.input.envirFrom,
                        entityName: this.input.entityName,
                        customFields: this.currentEntData.customFields,
                    };
                    // 优化：移除生产代码中的 console.log
                    this.jshelper
                        .invokeHiddenApiAsync("new_hbxn_common", "RetrieveCRMData/RetrieveCRMData", inputParam)
                        .then((res) => {
                            _this.$set(_this, "loading", false);
                            if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                                this.jshelper.openAlertDialog(this,
                                    "返回数据为空", "CRM树结构图表"
                                );
                                return;
                            }
                            if (res.isSuccess) {
                                let data = res.data;
                                _this.handleTreeData(data.data);
                                _this.$set(_this.tableData, "ecFrom", data.data);
                                _this.$set(_this.tableData, "ecFromCopy", data.data);
                            } else {
                                this.jshelper.openAlertDialog(this, res.message, "CRM树结构图表");
                            }
                            _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
                        })
                        .catch((err) => {
                            _this.$set(_this, "loading", false);
                            _this.jshelper.openAlertDialog(_this, err.message, "CRM树结构图表");
                        });
                    //#endregion
                }
            });
        },

        //envirFrom下拉Change事件
        envirFromChange: function () {
            //清空树数据
            this.$set(this.tableData, "treeData", []);
        },

        //实体下拉Change事件
        entityNameChange: function (val) {
            //清空树数据
            this.$set(this.tableData, "selectTreeNode", {});
            this.$set(this.tableData, "selectTreeNodeForm", []);
            this.$set(this.tableData, "treeData", []);

            //计算当前详情表单的行列数
            this.getDetailTableSize();
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$message({
                message: message,
                dangerouslyUseHTMLString: dangerouslyUseHTMLString,
                type: type,
            });
        },

        //复制到剪贴板
        copyToClipboard(val) {
            // 优化：使用现代 Clipboard API，替代已废弃的 document.execCommand
            if (navigator.clipboard && navigator.clipboard.writeText) {
                // 使用现代 Clipboard API
                navigator.clipboard.writeText(val)
                    .then(() => {
                        this.showMessage("复制成功", "success");
                    })
                    .catch((err) => {
                        console.error("复制失败:", err);
                        // 降级到传统方法
                        this.fallbackCopyToClipboard(val);
                    });
            } else {
                // 降级到传统方法（兼容旧浏览器）
                this.fallbackCopyToClipboard(val);
            }
        },

        // 降级复制方法（兼容旧浏览器）
        fallbackCopyToClipboard(val) {
            const textarea = document.createElement('textarea');
            textarea.style.position = 'fixed';
            textarea.style.opacity = '0';
            textarea.value = val;
            document.body.appendChild(textarea);
            textarea.select();
            try {
                const successful = document.execCommand('copy');
                if (successful) {
                    this.showMessage("复制成功", "success");
                } else {
                    this.showMessage("复制失败，请手动复制", "error");
                }
            } catch (err) {
                console.error("复制失败:", err);
                this.showMessage("复制失败，请手动复制", "error");
            } finally {
                document.body.removeChild(textarea);
            }
        },

        //树形图节点点击
        handleNodeClick: function (val) {
            this.$set(this.tableData, "selectTreeNode", val)
        },

        //处理为树形结构数据
        handleTreeData: function (data, config = {}) {
            this.$set(this.tableData, "treeData", []);
            if (this.rtcrm.isNull(data) || data.length === 0) return;
            // 优化：使用 Object.prototype.hasOwnProperty.call 替代直接使用 hasOwnProperty，更安全
            let isLookupParentField = typeof (data[0][this.currentEntData.parentField]) === "object" &&
                Object.prototype.hasOwnProperty.call(data[0][this.currentEntData.parentField], "Id");

            //#region 处理为树形结构数据
            const {
                idKey = `${this.currentEntData.logicalName}id`,
                parentKey = this.currentEntData.parentField,
                childrenKey = this.treeDefaultProps.children
            } = config;

            const nodeMap = {};
            let maxDepth = 0;

            // 创建虚拟根节点，收集所有无父节点的项
            const rootNode = { [childrenKey]: [] };
            nodeMap['VIRTUAL_ROOT'] = rootNode;

            data.forEach(item => {
                const nodeId = item[idKey];
                const parentId = isLookupParentField ? (item[parentKey] ? item[parentKey]["Id"] : null) : item[parentKey];

                // 创建节点的映射表
                if (!nodeMap[nodeId]) {
                    nodeMap[nodeId] = {
                        ...item,
                        [childrenKey]: [],
                        ["label"]: item[`${this.currentEntData.nameField}`],
                        _depth: 1,//缓存深度为1
                    };
                    if (nodeMap[nodeId]["_depth"] > maxDepth) {
                        maxDepth = nodeMap[nodeId]["_depth"]
                    }
                } else {
                    // 如果已存在，合并属性（保留已有的children）
                    nodeMap[nodeId] = {
                        ...nodeMap[nodeId],
                        ...item,
                        [childrenKey]: nodeMap[nodeId][childrenKey] || [],
                        ["label"]: item[`${this.currentEntData.nameField}`],
                        _depth: nodeMap[nodeId]["_depth"] ? nodeMap[nodeId]["_depth"] + 1 : 1,//缓存深度为1
                    };
                    if (nodeMap[nodeId]["_depth"] > maxDepth) {
                        maxDepth = nodeMap[nodeId]["_depth"]
                    }
                }

                // 构建树形结构
                // 处理父节点
                const parentNode = parentId !== null && parentId !== undefined
                    ? (nodeMap[parentId] || (nodeMap[parentId] = { [childrenKey]: [], _depth: 1 }))
                    : nodeMap['VIRTUAL_ROOT'];

                // 确保节点只被添加一次
                if (!parentNode[childrenKey].some(child => child[idKey] === nodeId)) {
                    parentNode[childrenKey].push(nodeMap[nodeId]);
                    nodeMap[nodeId]["_depth"] = parentNode["_depth"] + 1;
                    if (nodeMap[nodeId]["_depth"] > maxDepth) {
                        maxDepth = nodeMap[nodeId]["_depth"]
                    }
                }
            });
            //#endregion

            this.$set(this.tableData, "treeData", rootNode[childrenKey]);
            this.$set(this.tableData, "treeDepth", maxDepth);
        },

        //获取指定行列的Item
        getCustomFieldItem: function (row, col) {
            // 优化：简化条件判断逻辑
            if (this.rtcrm.isNull(this.currentEntData) || this.rtcrm.isNull(this.currentEntData.customFields)) {
                return null;
            }
            const index = row * this.detailTableConfig.columnCount + col;
            return index < this.currentEntData.customFields.length ? this.currentEntData.customFields[index] : null;
        },

        //计算当前详情表单的行列数
        getDetailTableSize: function () {
            //计算详情表单行列数
            let _rowCount = 0;
            let _rowCountArray = [];
            if (this.rtcrm.isNull(this.currentEntData) || this.rtcrm.isNull(this.currentEntData.customFields)) _rowCount = 0;
            else _rowCount = parseInt(this.currentEntData.customFields.length / this.detailTableConfig.columnCount);
            // 优化：修复计算错误，使用 columnCount 而不是硬编码的 2
            const remainder = this.currentEntData.customFields.length % this.detailTableConfig.columnCount;
            _rowCountArray = Array.from({ length: remainder === 0 ? _rowCount : _rowCount + 1 }, (_, i) => i);
            this.$set(this.detailTableConfig, "rowCountArray", _rowCountArray);

            let _columnCountArray = Array.from({ length: this.detailTableConfig.columnCount }, (_, i) => i);
            this.$set(this.detailTableConfig, "columnCountArray", _columnCountArray);

            // 优化：使用 map 和 filter 替代 for...of 循环，代码更简洁
            const selectTreeNodeForm = _rowCountArray.map(rowInd => {
                return _columnCountArray
                    .map(colInd => this.getCustomFieldItem(rowInd, colInd))
                    .filter(item => !this.rtcrm.isNull(item));
            });
            this.$set(this.tableData, "selectTreeNodeForm", selectTreeNodeForm);
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

    .el-input-group__append,
    .el-input-group__prepend {
        color: black;
        background-color: #fff;
    }
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
</style>