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
                        row-key="Id" @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 扩展Table -->
                        <el-table-column type="expand">
                            <template slot-scope="props">
                                <el-table :data="props.row.detail" border style="width: 100%">
                                    <!-- 行号 -->
                                    <el-table-column type="index" :index="indexMethodLower" align="center"
                                        show-overflow-tooltip>
                                    </el-table-column>
                                    <!-- 字段名称 -->
                                    <el-table-column prop="new_name" label="字段名称" show-overflow-tooltip>
                                    </el-table-column>
                                    <!-- NULL值不相等 -->
                                    <el-table-column prop="new_null_ne_null" label="NULL值不相等" show-overflow-tooltip>
                                    </el-table-column>
                                </el-table>
                            </template>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名称 -->
                        <el-table-column prop="new_name" label="实体名称" sortable show-overflow-tooltip>
                        </el-table-column>
                        <!-- 重复记录提示信息 -->
                        <el-table-column prop="new_message" label="重复记录提示信息" show-overflow-tooltip>
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
                <!-- 重复性检测 -->
                <div>
                    <el-table :height="tableHeight" :data="tableData.ecTo" :key="tableKey + '_envirTo'" border
                        style="width: 100%" v-loading="loading"
                        :default-sort="{ prop: 'new_name', order: 'ascending' }">
                        <!-- 扩展Table -->
                        <el-table-column type="expand">
                            <template slot-scope="props">
                                <el-table :data="props.row.detail" border style="width: 100%">
                                    <!-- 行号 -->
                                    <el-table-column type="index" :index="indexMethodLower" align="center"
                                        show-overflow-tooltip>
                                    </el-table-column>
                                    <!-- 字段名称 -->
                                    <el-table-column prop="new_name" label="字段名称" show-overflow-tooltip>
                                    </el-table-column>
                                    <!-- NULL值不相等 -->
                                    <el-table-column prop="new_null_ne_null" label="NULL值不相等" show-overflow-tooltip>
                                    </el-table-column>
                                </el-table>
                            </template>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 实体名称 -->
                        <el-table-column prop="new_name" label="实体名称" sortable show-overflow-tooltip>
                        </el-table-column>
                        <!-- 重复记录提示信息 -->
                        <el-table-column prop="new_message" label="重复记录提示信息" show-overflow-tooltip>
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'DuplicateDetectTable',
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

        //序号方法-字母
        indexMethodLower: function (index) {
            if (index < 0) index = 1;
            else index = index + 1;
            let s = "";
            while (index > 0) {
                let m = index % 26;
                m = m === 0 ? (m = 26) : m;
                s = String.fromCharCode(96 + parseInt(m)) + s;
                index = (index - m) / 26;
            }
            return s;
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