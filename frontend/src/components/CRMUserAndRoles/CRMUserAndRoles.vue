<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">CRM用户与安全角色</h3>
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
                                <el-row :gutter="4">
                                    <el-col :offset="17" :span="4">
                                        <el-input v-model="input.search" :disabled="loading" size="mini" clearable
                                            placeholder="输入关键字搜索" @change="dataSearchOnChange" />
                                    </el-col>
                                    <el-col :span="3">
                                        <el-select v-model="input.isHasSecurityRole" :disabled="loading" size="mini"
                                            clearable @change="isHasSecurityRoleChange">
                                            <el-option v-for="item in isHasSecurityRoleOptions" :key="item.value"
                                                :label="item.label" :value="item.value">
                                            </el-option>
                                        </el-select>
                                    </el-col>
                                </el-row>
                                <!-- 数据展示 -->
                                <el-row :gutter="24" style="margin-top: 15px">
                                    <el-col :span="24">
                                        <el-table :height="tableHeight" :data="tableData.ecFrom"
                                            :key="tableKey + '_envirFrom'" border style="width: 100%"
                                            v-loading="loading"
                                            :default-sort="{ prop: 'domainname', order: 'ascending' }">
                                            <!-- 行号 -->
                                            <el-table-column type="index" align="center" show-overflow-tooltip>
                                            </el-table-column>
                                            <!-- 用户名称 -->
                                            <el-table-column prop="fullname" label="用户名称" sortable show-overflow-tooltip
                                                width="160">
                                            </el-table-column>
                                            <!-- 账户名称 -->
                                            <el-table-column prop="domainname" label="账户名称" sortable
                                                show-overflow-tooltip width="220">
                                            </el-table-column>
                                            <!-- 主要电子邮件 -->
                                            <el-table-column prop="internalemailaddress" label="主要电子邮件"
                                                show-overflow-tooltip width="250">
                                                <template slot-scope="scope">
                                                    <div class="text-content text-ellipsis">
                                                        <div class="text-inner text-ellipsis">
                                                            <span>{{ scope.row.internalemailaddress }}</span>
                                                        </div>
                                                        <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.internalemailaddress)"
                                                            class="el-icon-copy-document text-icon"
                                                            style="cursor: pointer;" title="复制"
                                                            @click="copyToClipboard(scope.row.internalemailaddress)"></i>
                                                    </div>
                                                </template>
                                            </el-table-column>
                                            <!-- 移动电话 -->
                                            <el-table-column prop="mobilephone" label="移动电话" show-overflow-tooltip
                                                width="150">
                                                <template slot-scope="scope">
                                                    <div class="text-content text-ellipsis">
                                                        <div class="text-inner text-ellipsis">
                                                            <span>{{ scope.row.mobilephone }}</span>
                                                        </div>
                                                        <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.mobilephone)"
                                                            class="el-icon-copy-document text-icon"
                                                            style="cursor: pointer;" title="复制"
                                                            @click="copyToClipboard(scope.row.mobilephone)"></i>
                                                    </div>
                                                </template>
                                            </el-table-column>
                                            <!-- 部门 -->
                                            <el-table-column prop="businessunitid" label="部门" show-overflow-tooltip
                                                width="180">
                                            </el-table-column>
                                            <!-- 安全角色 -->
                                            <el-table-column prop="securityRolesStr" label="安全角色" show-overflow-tooltip
                                                width="400">
                                                <template slot-scope="scope">
                                                    <div class="text-content text-ellipsis">
                                                        <div class="text-inner text-ellipsis">
                                                            <span>{{ scope.row.securityRolesStr }}</span>
                                                        </div>
                                                        <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.securityRolesStr)"
                                                            class="el-icon-copy-document text-icon"
                                                            style="cursor: pointer;" title="复制"
                                                            @click="copyToClipboard(scope.row.securityRolesStr)"></i>
                                                    </div>
                                                </template>
                                            </el-table-column>
                                            <!-- 自定义字段 -->
                                            <el-table-column v-if="myUserThirtyFields.length > 0"
                                                v-for="item in myUserThirtyFields" :key="item.logicalName"
                                                :prop="item.logicalName" :label="item.displayName" show-overflow-tooltip
                                                :width="item.width">
                                            </el-table-column>
                                        </el-table>
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
import UserThirtyFields from './UserThirtyFields.json';
export default {
    name: 'CRMUserAndRoles',
    data() {
        return {
            msg: "hello world",
            // 环境
            environments: [{ label: "无效环境请刷新", key: "undefined" }],
            // 输入
            input: {
                envirFrom: "dev",
                search: "",
                isHasSecurityRole: "1",//默认过滤含有安全角色
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
            // 表单校验规则
            rules: {
            },
            UserThirtyFields: {},//用户自定义字段
            myUserThirtyFields: [],//当前环境的用户自定义字段
            isHasSecurityRoleOptions: [
                {
                    value: "1", label: "含有安全角色",
                },
            ],//是否含有安全角色的选项
        };
    },
    created() {
        //用户自定义字段配置初始化
        this.UserThirtyFieldsConfigInit();

        //获取系统参数
        this.getEnvironments();
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
        //配置文件初始化
        UserThirtyFieldsConfigInit: function () {
            if (!this.rtcrm.isNull(UserThirtyFields)) {
                this.$set(this, "UserThirtyFields", UserThirtyFields);
            }
            else {
                this.$set(this, "UserThirtyFields", null);
            }

            if (this.rtcrm.isNull(this.UserThirtyFields))
                this.$set(this, "myUserThirtyFields", []);
            else if (!this.UserThirtyFields.hasOwnProperty(this.$globalVar["selectEnv"]))
                this.$set(this, "myUserThirtyFields", []);
            else
                this.$set(this, "myUserThirtyFields", this.UserThirtyFields[this.$globalVar["selectEnv"]]);;

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
                entityName: "",
                attributeType: "",
                search: "",
            });
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table

            //重新获取环境参数
            this.getEnvironments();

            //重新获取配置文件
            this.UserThirtyFieldsConfigInit();
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
        getData: function (envir) {
            let _this = this;
            this.$refs["form"].validate(async (valid, error) => {
                if (valid) {
                    //#region 清空数据
                    this.$set(this.tableData, "ecFrom", []);
                    this.$set(this.tableData, "ecFromCopy", []);
                    this.$set(this.tableData, "ecFromTotalRecord", 0);
                    this.$set(this.input, "search", "");
                    this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
                    //#endregion

                    //#region 查询数据
                    this.$set(this, "loading", true);
                    let inputParam = {
                        envir: this.input.envirFrom,
                        customFields: this.myUserThirtyFields,
                    };
                    this.jshelper
                        .invokeHiddenApiAsync("new_hbxn_common", "RetrieveUserAndRoles/GetUserAndRoles", inputParam)
                        .then((res) => {
                            _this.$set(_this, "loading", false);
                            if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                                this.jshelper.openAlertDialog(this,
                                    "返回数据为空", "CRM用户与安全角色查询"
                                );
                                return;
                            }
                            if (res.isSuccess) {
                                let data = res.data;
                                let filterData = _this.isHasSecurityRoleFilter(data.data, _this.input.isHasSecurityRole);
                                _this.$set(_this.tableData, "ecFrom", filterData);
                                _this.$set(_this.tableData, "ecFromCopy", data.data);
                            } else {
                                this.jshelper.openAlertDialog(this, res.message, "CRM用户与安全角色查询");
                            }
                            _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
                        })
                        .catch((err) => {
                            _this.$set(_this, "loading", false);
                            _this.jshelper.openAlertDialog(_this, err.message, "CRM用户与安全角色查询");
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
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$message({
                message: message,
                dangerouslyUseHTMLString: dangerouslyUseHTMLString,
                type: type,
            });
        },

        //table模糊搜索事件
        dataSearchOnChange(val) {
            let filterResult = this.dataSearchOnFilter(this.tableData.ecFromCopy, val);
            filterResult = this.isHasSecurityRoleFilter(filterResult, this.input.isHasSecurityRole);

            this.$set(this.tableData, "ecFrom", filterResult);
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
        },

        dataSearchOnFilter(data, val) {
            if (this.rtcrm.isNullOrWhiteSpace(val)) {
                return data;
            }
            else {
                let result = []
                for (let item of data) {
                    if (item.fullname.indexOf(val) !== -1 || item.domainname.indexOf(val) !== -1 ||
                        item.securityRolesStr.indexOf(val) !== -1 || item.businessunitid.indexOf(val) !== -1)
                        result.push(item);
                }
                return result;
            }
        },

        //是否含有安全角色过滤
        isHasSecurityRoleChange: function (val) {
            let filterResult = this.isHasSecurityRoleFilter(this.tableData.ecFromCopy, val);
            filterResult = this.dataSearchOnFilter(filterResult, this.input.search);

            this.$set(this.tableData, "ecFrom", filterResult);
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
        },

        isHasSecurityRoleFilter: function (data, val) {
            if (this.rtcrm.isNullOrWhiteSpace(val)) {
                return data;
            }
            else {
                let result = []
                for (let item of data) {
                    if (val == 1 && !this.rtcrm.isNull(item.securityRoles) && item.securityRoles.length > 0)
                        result.push(item);
                }
                return result;
            }
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