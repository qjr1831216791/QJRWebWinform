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
                        <!-- 实体名称 -->
                        <el-table-column prop="new_name" label="实体名称" show-overflow-tooltip sortable width="160">
                        </el-table-column>
                        <!-- 单号字段 -->
                        <el-table-column prop="new_nofieldname" label="单号字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 使用表达式 -->
                        <el-table-column prop="new_useexpression" label="使用表达式" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 前缀 -->
                        <el-table-column prop="new_prefix" label="前缀" show-overflow-tooltip width="80">
                        </el-table-column>
                        <!-- 是否包含年 -->
                        <el-table-column prop="new_isyear" label="是否包含年" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否包含月 -->
                        <el-table-column prop="new_ismonth" label="是否包含月" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否两位年 -->
                        <el-table-column prop="new_istwo" label="是否两位年" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否包含日 -->
                        <el-table-column prop="new_isday" label="是否包含日" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 流水号长度 -->
                        <el-table-column prop="new_length" label="流水号长度" show-overflow-tooltip width="95">
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
                <!-- 自动编号 -->
                <div>
                    <el-table :height="tableHeight" :data="tableData.ecTo" :key="tableKey + '_envirTo'" border
                        style="width: 100%" v-loading="loading"
                        :default-sort="{ prop: 'new_name', order: 'ascending' }">
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名称 -->
                        <el-table-column prop="new_name" label="实体名称" sortable show-overflow-tooltip width="160">
                        </el-table-column>
                        <!-- 单号字段 -->
                        <el-table-column prop="new_nofieldname" label="单号字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 使用表达式 -->
                        <el-table-column prop="new_useexpression" label="使用表达式" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 前缀 -->
                        <el-table-column prop="new_prefix" label="前缀" show-overflow-tooltip width="80">
                        </el-table-column>
                        <!-- 是否包含年 -->
                        <el-table-column prop="new_isyear" label="是否包含年" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否包含月 -->
                        <el-table-column prop="new_ismonth" label="是否包含月" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否两位年 -->
                        <el-table-column prop="new_istwo" label="是否两位年" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 是否包含日 -->
                        <el-table-column prop="new_isday" label="是否包含日" show-overflow-tooltip width="95">
                        </el-table-column>
                        <!-- 流水号长度 -->
                        <el-table-column prop="new_length" label="流水号长度" show-overflow-tooltip width="95">
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'AutoNumberTable',
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
</style>