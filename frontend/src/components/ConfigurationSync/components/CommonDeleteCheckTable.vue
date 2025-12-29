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
                        style="width: 100%" v-loading="loading" :default-sort="{
                            prop: 'primaryobjecttypecode',
                            order: 'ascending',
                        }" @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- Step Name -->
                        <el-table-column prop="name" label="Step Name" show-overflow-tooltip width="100">
                        </el-table-column>
                        <!-- Entity -->
                        <el-table-column prop="primaryobjecttypecode" label="Entity" show-overflow-tooltip sortable
                            width="160">
                        </el-table-column>
                        <!-- Message -->
                        <el-table-column prop="sdkmessageid" label="Message" show-overflow-tooltip width="80">
                        </el-table-column>
                        <!-- Stage -->
                        <el-table-column prop="stage" label="Stage" show-overflow-tooltip width="60">
                        </el-table-column>
                        <!-- Configuration -->
                        <el-table-column prop="configuration" label="Configuration" show-overflow-tooltip width="220">
                        </el-table-column>
                        <!-- SecureConfig -->
                        <el-table-column prop="secureconfig" label="SecureConfig" show-overflow-tooltip width="220">
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
                        style="width: 100%" v-loading="loading" :default-sort="{
                            prop: 'primaryobjecttypecode',
                            order: 'ascending',
                        }">
                        <!-- 行号 -->
                        <el-table-column type="index" show-overflow-tooltip>
                        </el-table-column>
                        <!-- Step Name -->
                        <el-table-column prop="name" label="Step Name" show-overflow-tooltip width="100">
                        </el-table-column>
                        <!-- Entity -->
                        <el-table-column prop="primaryobjecttypecode" label="Entity" show-overflow-tooltip sortable
                            width="160">
                        </el-table-column>
                        <!-- Message -->
                        <el-table-column prop="sdkmessageid" label="Message" show-overflow-tooltip width="80">
                        </el-table-column>
                        <!-- Stage -->
                        <el-table-column prop="stage" label="Stage" show-overflow-tooltip width="60">
                        </el-table-column>
                        <!-- Configuration -->
                        <el-table-column prop="configuration" label="Configuration" show-overflow-tooltip width="220">
                        </el-table-column>
                        <!-- SecureConfig -->
                        <el-table-column prop="secureconfig" label="SecureConfig" show-overflow-tooltip width="220">
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'CommonDeleteCheckTable',
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