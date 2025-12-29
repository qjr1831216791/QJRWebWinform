<template>
    <div>
        <el-col :span="12">
            <!-- envirFrom -->
            <el-card class="box-card">
                <div slot="header" class="clearfix">
                    <span>{{ envirLabel[0] }}</span>
                </div>
                <div>
                    <el-table :height="tableHeight" :data="tableData.ecFrom" :key="tableKey" border style="width: 100%"
                        v-loading="loading" :default-sort="{ prop: 'new_name', order: 'ascending' }"
                        @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 父实体 -->
                        <el-table-column prop="new_name" label="父实体" show-overflow-tooltip sortable width="160">
                        </el-table-column>
                        <!-- 子实体 -->
                        <el-table-column prop="new_listentity" label="子实体" show-overflow-tooltip width="160">
                        </el-table-column>
                        <!-- 汇总主档字段 -->
                        <el-table-column prop="new_total" label="汇总主档字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 汇总明细字段 -->
                        <el-table-column prop="new_list" label="汇总明细字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 子关系字段 -->
                        <el-table-column prop="new_reffield" label="子关系字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 汇总数据类型 -->
                        <el-table-column prop="new_type" label="汇总数据类型" show-overflow-tooltip width="110">
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
                        <!-- 父实体 -->
                        <el-table-column prop="new_name" label="父实体" show-overflow-tooltip sortable width="160">
                        </el-table-column>
                        <!-- 子实体 -->
                        <el-table-column prop="new_listentity" label="子实体" show-overflow-tooltip width="160">
                        </el-table-column>
                        <!-- 汇总主档字段 -->
                        <el-table-column prop="new_total" label="汇总主档字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 汇总明细字段 -->
                        <el-table-column prop="new_list" label="汇总明细字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 子关系字段 -->
                        <el-table-column prop="new_reffield" label="子关系字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 汇总数据类型 -->
                        <el-table-column prop="new_type" label="汇总数据类型" show-overflow-tooltip width="110">
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'SumRelationshipDetailTable',
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