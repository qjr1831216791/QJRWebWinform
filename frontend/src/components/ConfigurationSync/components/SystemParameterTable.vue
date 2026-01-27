<template>
    <div>
        <el-col :span="12">
            <!-- envirFrom -->
            <el-card class="box-card">
                <div slot="header" class="clearfix">
                    <span>{{ envirLabel[0] }}</span>
                </div>
                <div>
                    <el-table :height="tableHeight" :data="tableData.ecFrom" :key="tableKey + '_envirFrom'" border
                        style="width: 100%" v-loading="loading" :default-sort="{ prop: 'new_name', order: 'ascending' }"
                        @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- Name -->
                        <el-table-column prop="new_name" label="名称" sortable show-overflow-tooltip width="180">
                        </el-table-column>
                        <!-- Value -->
                        <el-table-column prop="new_value" label="Value" show-overflow-tooltip width="180">
                            <template slot-scope="scope">
                                <div class="text-content text-ellipsis">
                                    <div class="text-inner text-ellipsis">
                                        <span>{{ scope.row.new_value }}</span>
                                    </div>
                                    <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.new_value)"
                                        class="el-icon-copy-document text-icon"
                                        style="cursor: pointer;" title="复制"
                                        @click="copyToClipboard(scope.row.new_value)"></i>
                                </div>
                            </template>
                        </el-table-column>
                        <!-- 描述 -->
                        <el-table-column prop="new_desc" label="描述" show-overflow-tooltip width="250">
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
        <el-col :span="12">
            <!-- envirTo -->
            <el-card class="box-card">
                <div slot="header" class="clearfix">
                    <span>{{ envirLabel[1] }}</span>
                </div>
                <div>
                    <el-table :height="tableHeight" :data="tableData.ecTo" :key="tableKey + '_envirTo'" border
                        style="width: 100%" v-loading="loading"
                        :default-sort="{ prop: 'new_name', order: 'ascending' }">
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- Name -->
                        <el-table-column prop="new_name" label="名称" show-overflow-tooltip sortable width="180">
                        </el-table-column>
                        <!-- Value -->
                        <el-table-column prop="new_value" label="Value" show-overflow-tooltip width="180">
                            <template slot-scope="scope">
                                <div class="text-content text-ellipsis">
                                    <div class="text-inner text-ellipsis">
                                        <span>{{ scope.row.new_value }}</span>
                                    </div>
                                    <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.new_value)"
                                        class="el-icon-copy-document text-icon"
                                        style="cursor: pointer;" title="复制"
                                        @click="copyToClipboard(scope.row.new_value)"></i>
                                </div>
                            </template>
                        </el-table-column>
                        <!-- 描述 -->
                        <el-table-column prop="new_desc" label="描述" show-overflow-tooltip width="250">
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'SystemParameterTable',
    data() {
        return {
            msg: "hello rektec",
        };
    },
    props: {
        tableData: {
            ecFrom: [],
            ecFromTotalRecord: 0,
            ecTo: [],
            ecToTotalRecord: 0,
        }, //数据
        tableHeight: {
            type: String,
            default: "380px",
        }, //表格高度
        loading: false, //是否加载数据中
        envirLabel: {
            type: Array,
            default: () => ["DEV", "UAT"], //环境标签
        },
        tableKey: {
            type: Number,
            default: 1,
        }, //刷新表格的Key
    },
    created() {
    },
    mounted() {
    },
    computed: {
    },
    methods: {
        //处理表格多选
        handleSelectionChange(val) {
            this.$emit('handle-selection-change', val);
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
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(val)
                    .then(() => {
                        this.showMessage("复制成功", "success");
                    })
                    .catch((err) => {
                        console.error("复制失败:", err);
                        this.fallbackCopyToClipboard(val);
                    });
            } else {
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
    },
};
</script>

<style scoped>
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