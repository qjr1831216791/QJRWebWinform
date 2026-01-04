<template>
    <div>
        <el-col :span="12">
            <!-- envirFrom -->
            <el-card class="box-card">
                <div slot="header" class="clearfix">
                    <span>{{ envirLabel[0] }}</span>
                </div>
                <div>
                    <el-table :height="pagingTableHeight" :data="tableData.ecFrom" :key="tableKey + '_envirFrom'" border
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
                        <el-table-column prop="new_entity_name" label="实体名" sortable show-overflow-tooltip width="180">
                        </el-table-column>
                        <!-- 字段名 -->
                        <el-table-column prop="new_attribute_name" label="字段名" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 语言 -->
                        <el-table-column prop="new_language_id" label="语言" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 标签值 -->
                        <el-table-column prop="new_value" label="标签值" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 编码 -->
                        <el-table-column prop="new_code" label="编码" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 数据id -->
                        <el-table-column prop="new_data_id" label="数据id" show-overflow-tooltip width="110">
                        </el-table-column>
                        <!-- 语言编码 -->
                        <el-table-column prop="new_language_code" label="语言编码" show-overflow-tooltip width="90">
                        </el-table-column>
                        <!-- 创建时间 -->
                        <el-table-column prop="createdon" label="创建时间" show-overflow-tooltip width="120">
                        </el-table-column>
                    </el-table>
                    <el-pagination :key="tableKey + '_envirFromPaging'" :disabled="loading"
                        @size-change="handleSizeChange" @current-change="handleCurrentChange"
                        :current-page="input.pageIndexEnvirFrom" :page-sizes="PageSizeList"
                        :page-size="input.pageSizeEnvirFrom" layout="total, sizes, prev, pager, next, jumper"
                        :total="tableData.ecFromTotalRecord">
                    </el-pagination>
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
                    <el-table :height="pagingTableHeight" :data="tableData.ecTo" :key="tableKey + '_envirTo'" border
                        style="width: 100%" v-loading="loading"
                        :default-sort="{ prop: 'new_entity_name', order: 'ascending' }">
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名 -->
                        <el-table-column prop="new_entity_name" label="实体名" sortable show-overflow-tooltip width="180">
                        </el-table-column>
                        <!-- 字段名 -->
                        <el-table-column prop="new_attribute_name" label="字段名" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 语言 -->
                        <el-table-column prop="new_language_id" label="语言" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 标签值 -->
                        <el-table-column prop="new_value" label="标签值" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 编码 -->
                        <el-table-column prop="new_code" label="编码" show-overflow-tooltip width="140">
                        </el-table-column>
                        <!-- 数据id -->
                        <el-table-column prop="new_data_id" label="数据id" show-overflow-tooltip width="110">
                        </el-table-column>
                        <!-- 语言编码 -->
                        <el-table-column prop="new_language_code" label="语言编码" show-overflow-tooltip width="90">
                        </el-table-column>
                        <!-- 创建时间 -->
                        <el-table-column prop="createdon" label="创建时间" show-overflow-tooltip width="120">
                        </el-table-column>
                    </el-table>
                    <el-pagination :key="tableKey + '_envirFromPaging'" :disabled="loading"
                        @size-change="handleSizeChange2" @current-change="handleCurrentChange2"
                        :current-page="input.pageIndexEnvirTo" :page-sizes="PageSizeList"
                        :page-size="input.pageSizeEnvirTo" layout="total, sizes, prev, pager, next, jumper"
                        :total="tableData.ecToTotalRecord">
                    </el-pagination>
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
            // 分页配置
            PageSizeList: [20, 50, 100, 200],
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
        input: {
            pageIndexEnvirFrom: 1,
            pageSizeEnvirFrom: 20,
            pageIndexEnvirTo: 1,
            pageSizeEnvirTo: 20,
        },
    },
    created() {
    },
    mounted() {
    },
    computed: {
        // 带有分页的Table的高度
        pagingTableHeight() {
            let tableHeight = parseInt(this.tableHeight);
            return tableHeight - 65 + "px";
        },
    },
    methods: {
        //处理表格多选
        handleSelectionChange(val) {
            this.$emit('handle-selection-change', val);
        },

        //分页变更-envirFrom
        handleSizeChange: function (val) {
            this.$emit("handle-size-change", val);
        },

        //页码变更-envirFrom
        handleCurrentChange: function (val) {
            this.$emit("handle-current-change", val);
        },

        //分页变更-envirTo
        handleSizeChange2: function (val) {
            this.$emit("handle-size-change2", val);
        },

        //页码变更-envirTo
        handleCurrentChange2: function (val) {
            this.$emit("handle-current-change2", val);
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