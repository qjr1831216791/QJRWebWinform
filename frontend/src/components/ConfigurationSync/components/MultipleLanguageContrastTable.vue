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
                        style="width: 100%" v-loading="loading"
                        :default-sort="{ prop: 'new_entity_name', order: 'ascending' }"
                        @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名 -->
                        <el-table-column prop="new_entity_name" label="实体名" sortable show-overflow-tooltip width="160">
                        </el-table-column>
                        <!-- 多语言字段列表 -->
                        <el-table-column prop="new_multi_attribute_name" label="多语言字段列表" show-overflow-tooltip
                            width="140">
                        </el-table-column>
                        <!-- 编码对应字段 -->
                        <el-table-column prop="new_code" label="编码对应字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 创建时间 -->
                        <el-table-column prop="createdon" label="创建时间" show-overflow-tooltip>
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
                        :default-sort="{ prop: 'new_entity_name', order: 'ascending' }">
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名 -->
                        <el-table-column prop="new_entity_name" label="实体名" sortable show-overflow-tooltip width="160">
                        </el-table-column>
                        <!-- 多语言字段列表 -->
                        <el-table-column prop="new_multi_attribute_name" label="多语言字段列表" show-overflow-tooltip
                            width="140">
                        </el-table-column>
                        <!-- 编码对应字段 -->
                        <el-table-column prop="new_code" label="编码对应字段" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 创建时间 -->
                        <el-table-column prop="createdon" label="创建时间" show-overflow-tooltip>
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