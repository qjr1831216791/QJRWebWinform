<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">CRM插件跟踪日志</h3>
                <el-divider></el-divider>
            </el-header>
            <el-main>
                <el-form :model="input" :rules="rules" ref="form" size="medium" label-position="left">
                    <!-- 输入行 -->
                    <el-row :gutter="24">
                        <!-- 环境： -->
                        <el-col :span="6">
                            <el-form-item prop="envirFrom" label="环境：" label-width="80px">
                                <el-select size="small" v-model="input.envirFrom" placeholder="请选择" :disabled="loading"
                                    @change="envirFromChange">
                                    <el-option v-for="item in environments" :key="item.key" :label="item.label"
                                        :value="item.key">
                                    </el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>

                        <!-- 插件名称 -->
                        <el-col :span="6">
                            <el-form-item prop="pluginName" label="插件名称" label-width="80px">
                                <el-input size="small" v-model="input.pluginName" :disabled="loading">
                                </el-input>
                            </el-form-item>
                        </el-col>
                        <el-col :span="5">
                            <el-form-item prop="pluginName2">
                                <el-input size="small" v-model="input.pluginName2" :disabled="loading">
                                </el-input>
                            </el-form-item>
                        </el-col>

                        <!-- 按钮 -->
                        <el-col :span="5">
                            <el-form-item>
                                <el-button size="small" :loading="loading" @click="getDataButton()">查询</el-button>
                            </el-form-item>
                        </el-col>
                    </el-row>
                    <!-- 过滤行 -->
                    <el-row :gutter="24" style="min-height: 28px">
                        <el-col :span="10" v-show="createdonRangeShow">
                            <!-- 创建、修改时间 -->
                            <el-form-item prop="createdonDR" label="创建、修改时间：" required>
                                <el-date-picker size="small" v-model="input.createdonDR" type="datetimerange"
                                    range-separator="至" start-placeholder="开始日期时间" end-placeholder="结束日期时间"
                                    :default-time="['00:00:00', '23:59:59']" :picker-options="pickerOptions"
                                    :disabled="loading" @focus="handleDatePickerFocus">
                                </el-date-picker>
                            </el-form-item>
                        </el-col>

                        <!-- 实体名称 -->
                        <el-col :span="6">
                            <el-form-item prop="entityName" label="实体名称" label-width="80px">
                                <el-input size="small" v-model="input.entityName" :disabled="loading">
                                </el-input>
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
                                    <el-col :offset="20" :span="4">
                                        <el-input v-model="input.search" :disabled="loading" size="mini" clearable
                                            placeholder="输入关键字搜索" @change="dataSearchOnChange" />
                                    </el-col>
                                </el-row>
                                <!-- 数据展示 - 左右分栏布局 -->
                                <el-row :gutter="24" style="margin-top: 13px">
                                    <!-- 左侧表格 -->
                                    <el-col :span="12">
                                        <el-table :height="pagingTableHeight" :data="tableData.ecFrom"
                                            :key="tableKey + '_envirFrom'" border style="width: 100%"
                                            v-loading="loading" @row-click="handleRowClick" highlight-current-row>
                                            <!-- 行号 -->
                                            <el-table-column type="index" align="center" show-overflow-tooltip
                                                width="60">
                                            </el-table-column>
                                            <!-- 插件名称 -->
                                            <el-table-column prop="typename" label="插件名称" show-overflow-tooltip
                                                width="360">
                                                <template slot-scope="scope">
                                                    <div class="text-content text-ellipsis">
                                                        <div class="text-inner text-ellipsis">
                                                            <span>{{ scope.row.typename }}</span>
                                                        </div>
                                                        <i v-show="!rtcrm.isNullOrWhiteSpace(scope.row.typename)"
                                                            class="el-icon-copy-document text-icon"
                                                            style="cursor: pointer;" title="复制"
                                                            @click="copyTypeNameToClipboard(scope.row.typename)"></i>
                                                    </div>
                                                </template>
                                            </el-table-column>
                                            <!-- 消息名称 -->
                                            <el-table-column prop="messagename" label="消息名称" show-overflow-tooltip
                                                width="160">
                                            </el-table-column>
                                            <!-- 创建时间 -->
                                            <el-table-column prop="createdOn" label="创建时间" show-overflow-tooltip>
                                                <template slot-scope="scope">
                                                    {{ rtcrm.formatDate(new Date(scope.row.createdon), "yyyy-MM-dd hh:mm:ss") }}
                                                </template>
                                            </el-table-column>
                                        </el-table>
                                        <!-- 分页组件 -->
                                        <el-pagination :key="tableKey + '_paging'" :disabled="loading"
                                            @size-change="handleSizeChange" @current-change="handleCurrentChange"
                                            :current-page="input.pageIndex" :page-sizes="PageSizeList"
                                            :page-size="input.pageSize" layout="total, sizes, prev, pager, next, jumper"
                                            :total="tableData.ecFromTotalRecord">
                                        </el-pagination>
                                    </el-col>
                                    <!-- 右侧详细信息面板 -->
                                    <el-col :span="12">
                                        <el-card class="detail-panel" v-if="selectedRow"
                                            :id="'detail-panel-' + tableKey" :style="{ height: pagingTableHeight }">
                                            <div slot="header" class="detail-header">
                                                <span>日志详细信息</span>
                                            </div>
                                            <!-- 基础信息 -->
                                            <div class="detail-content">
                                                <div class="detail-item">
                                                    <label>插件名称：</label>
                                                    <span>{{ selectedRow.typename || '无' }}</span>
                                                </div>
                                                <div class="detail-item">
                                                    <label>消息名称：</label>
                                                    <span>{{ selectedRow.messagename || '无' }}</span>
                                                </div>
                                                <div class="detail-item">
                                                    <label>执行状态：</label>
                                                    <span>{{ selectedRow.mode_Formatted || 'Unknown' }}</span>
                                                </div>
                                                <div class="detail-item">
                                                    <label>执行时间：</label>
                                                    <span>{{ rtcrm.formatDate(new Date(selectedRow.createdon),
                                                        "yyyy-MM-dd hh:mm:ss") }}</span>
                                                </div>
                                                <div class="detail-item">
                                                    <label>执行时长：</label>
                                                    <span>{{ selectedRow.performanceexecutionduration || 0 }} ms</span>
                                                </div>
                                                <div class="detail-item">
                                                    <label>实体名称：</label>
                                                    <span>{{ selectedRow.primaryentity || '无' }}</span>
                                                </div>
                                                <div class="detail-item full-width">
                                                    <label>消息内容：</label>
                                                    <div class="message-content">
                                                        <el-input type="textarea" :id="'message-content-' + tableKey"
                                                            :value="selectedRow.messageblock || '无'" :rows="15"
                                                            readonly>
                                                        </el-input>
                                                        <el-button size="mini" type="primary"
                                                            @click="copyToClipboard(selectedRow.messageblock || '')"
                                                            style="margin-top: 5px;">
                                                            复制消息内容
                                                        </el-button>
                                                    </div>
                                                </div>
                                            </div>
                                        </el-card>
                                        <el-card v-else class="detail-panel" :style="{ height: pagingTableHeight }">
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
    name: 'CRMPluginTraceLog',
    data() {
        return {
            msg: "hello world",
            // 环境
            environments: [{ label: "无效环境请刷新", key: "undefined" }],
            // 输入
            input: {
                envirFrom: "dev",
                pluginName: "RekTec.",
                pluginName2: "",
                entityName: "",//实体名称
                search: "",
                createdonDR: null, // 日期范围
                pageIndex: 1, // 当前页码
                pageSize: 50, // 每页显示数量
            },
            tableData: {
                ecFrom: [],
                ecFromCopy: [],
                ecFromTotalRecord: 0,
            }, //数据
            defaultTableHeight: "470", //表格高度
            tableKey: 1, //刷新表格的Key
            loading: false, //是否加载数据中
            selectedRow: null, //选中的行数据
            createdonRangeShow: true, // 是否显示日期范围
            // 分页配置
            PageSizeList: [20, 50, 100, 200],
            // 表单校验规则
            rules: {
                // 可以添加必要的校验规则
            },
            // 日期选择器快捷选项
            pickerOptions: {
                shortcuts: [
                    {
                        text: '最近一小时',
                        onClick(picker) {
                            const end = new Date();
                            const start = new Date();
                            start.setTime(end.getTime() - 3600 * 1000); // 减去1小时（毫秒）
                            // 结束时间往后推3分钟，以覆盖异步队列写入延迟
                            end.setTime(end.getTime() + 3 * 60 * 1000); // 增加3分钟（180000毫秒）
                            // 设置开始时间的秒数为00
                            start.setSeconds(0, 0);
                            // 设置结束时间的秒数为59
                            end.setSeconds(59, 999);
                            picker.$emit('pick', [start, end]);
                        },
                    },
                    {
                        text: '最近一天',
                        onClick(picker) {
                            const end = new Date();
                            const start = new Date();
                            start.setTime(end.getTime() - 24 * 3600 * 1000); // 减去1天
                            // 设置开始时间的时分秒为00:00:00
                            start.setHours(0, 0, 0, 0);
                            // 设置结束时间为当前日期的23:59:59
                            end.setHours(23, 59, 59, 999);
                            picker.$emit('pick', [start, end]);
                        },
                    },
                    {
                        text: '最近一周',
                        onClick(picker) {
                            const end = new Date();
                            const start = new Date();
                            start.setTime(end.getTime() - 7 * 24 * 3600 * 1000); // 减去7天
                            // 设置开始时间的时分秒为00:00:00
                            start.setHours(0, 0, 0, 0);
                            // 设置结束时间为当前日期的23:59:59
                            end.setHours(23, 59, 59, 999);
                            picker.$emit('pick', [start, end]);
                        },
                    },
                ],
            },
        };
    },
    created() {
        //获取系统参数
        this.getEnvironments();
        //设置默认日期范围为昨天到今天
        this.setDefaultDateRange();
    },
    mounted() {
        //监听环境参数切换
        this.$on('environment-change', this.environmentChange);
    },
    computed: {
        // 带有分页的Table的高度
        pagingTableHeight() {
            let tableHeight = parseInt(this.tableHeight);
            return tableHeight - 30 + "px";
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
            if (this.isDesktop) return height + 90 + "px";
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
                pluginName: "",
                search: "",
                createdonDR: null,
                pageIndex: 1,
                pageSize: 20,
            });
            // 重新设置默认日期范围
            this.setDefaultDateRange();
            this.$set(this, "selectedRow", null); //清空选中的行
            this.$set(this, "tableKey", this.tableKey + 1); //刷新Table

            //重新获取环境参数
            this.getEnvironments();
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

        //查询按钮
        getDataButton: function () {
            this.$set(this.input, "pageIndex", 1);
            this.getData();
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

                    // 构建FetchXML查询语句
                    const fetchXml = this.buildFetchXml();

                    let input = {
                        envir: this.input.envirFrom,
                        fetchXml: fetchXml,
                        pageIndex: this.input.pageIndex,
                        pageSize: this.input.pageSize
                    };
                    this.jshelper
                        .invokeHiddenApiAsync("new_hbxn_common", "RetrieveCRMData/RetrieveCRMDataByFetchXml", input)
                        .then((res) => {
                            _this.$set(_this, "loading", false);
                            if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                                this.jshelper.openAlertDialog(this,
                                    "返回数据为空", "CRM插件跟踪日志查询"
                                );
                                return;
                            }
                            if (res.isSuccess) {
                                let data = res.data;
                                if (data && data.Entities) {
                                    // 使用工具方法转换数据格式：将Attributes数组转换为扁平对象
                                    const transformedData = this.transformCRMEntityData(data.Entities);

                                    _this.$set(_this.tableData, "ecFrom", transformedData);
                                    _this.$set(_this.tableData, "ecFromCopy", transformedData);

                                    // 处理分页信息
                                    const totalCount = data.TotalRecordCount;
                                    if (totalCount === -1) {
                                        // 如果无法获取准确总数，使用当前页数据量 + 是否有更多记录
                                        const currentCount = transformedData.length + (this.input.pageIndex - 1) * this.input.pageSize;
                                        const hasMore = data.MoreRecords;
                                        _this.$set(_this.tableData, "ecFromTotalRecord", hasMore ? currentCount + 1 : currentCount);
                                    } else {
                                        _this.$set(_this.tableData, "ecFromTotalRecord", totalCount);
                                    }
                                } else if (Array.isArray(data)) {
                                    // 直接返回数组数据（兼容旧格式）
                                    _this.$set(_this.tableData, "ecFrom", data);
                                    _this.$set(_this.tableData, "ecFromCopy", data);
                                } else {
                                    _this.$set(_this.tableData, "ecFrom", []);
                                    _this.$set(_this.tableData, "ecFromCopy", []);
                                    _this.$set(_this.tableData, "ecFromTotalRecord", 0);
                                }
                            } else {
                                this.jshelper.openAlertDialog(this, res.message, "CRM插件跟踪日志查询");
                            }
                            _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
                        })
                        .catch((err) => {
                            _this.$set(_this, "loading", false);
                            _this.jshelper.openAlertDialog(_this, err.message, "CRM插件跟踪日志查询");
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

        //CRM实体数据转换工具方法 - 将Attributes数组转换为扁平对象
        transformCRMEntityData(entities) {
            if (!entities || !Array.isArray(entities)) {
                return [];
            }

            return entities.map(entity => {
                const flatEntity = {
                    Id: entity.Id,
                    LogicalName: entity.LogicalName
                };

                // 转换Attributes数组为扁平对象
                if (entity.Attributes && Array.isArray(entity.Attributes)) {
                    entity.Attributes.forEach(attr => {
                        flatEntity[attr.Key] = attr.Value;
                    });
                }

                // 添加FormattedValues
                if (entity.FormattedValues && Array.isArray(entity.FormattedValues)) {
                    entity.FormattedValues.forEach(fv => {
                        flatEntity[`${fv.Key}_Formatted`] = fv.Value;
                    });
                }

                return flatEntity;
            });
        },

        //envirFrom下拉Change事件
        envirFromChange: function () {
            // 可以在这里添加环境切换时的逻辑
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$message({
                message: message,
                dangerouslyUseHTMLString: dangerouslyUseHTMLString,
                type: type,
            });
            if (type === "error") {
                console.error(message);
            }
        },

        //table模糊搜索事件
        dataSearchOnChange(val) {
            if (this.rtcrm.isNullOrWhiteSpace(val)) {
                this.$set(this.tableData, "ecFrom", this.tableData.ecFromCopy);
            }
            else {
                this.$set(this.tableData, "ecFrom", []);//置空再重新填充数据
                for (let item of this.tableData.ecFromCopy) {
                    if ((item.typename && item.typename.indexOf(val) !== -1) ||
                        (item.messagename && item.messagename.indexOf(val) !== -1) ||
                        (item.messageblock && item.messageblock.indexOf(val) !== -1) ||
                        (item.primaryentity && item.primaryentity.indexOf(val) !== -1)) {
                        this.tableData.ecFrom.push(item);
                    }
                }
            }
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

        copyTypeNameToClipboard(val) {
            let typeNameSplit = val.split(',');
            if (!this.rtcrm.isNull(typeNameSplit) && typeNameSplit.length > 1) {
                val = this.rtcrm.trim(typeNameSplit[1]);
            }
            this.copyToClipboard(val);
        },

        //处理行点击事件
        handleRowClick(row) {
            this.selectedRow = row;

            // 重置详细信息面板和消息内容的滚动条到顶端
            this.$nextTick(() => {
                // 重置详细信息面板滚动条
                const detailPanel = document.getElementById(`detail-panel-${this.tableKey}`);
                if (detailPanel) {
                    detailPanel.scrollTop = 0;
                }

                // 重置消息内容textarea滚动条
                const messageContent = document.getElementById(`message-content-${this.tableKey}`);
                if (messageContent) {
                    messageContent.scrollTop = 0;
                }
            });
        },

        //设置默认日期范围为昨天到今天
        setDefaultDateRange() {
            const today = new Date();
            const yesterday = new Date();
            yesterday.setDate(today.getDate() - 1);

            // 设置昨天为开始时间 00:00:00
            yesterday.setHours(0, 0, 0, 0);
            // 设置今天为结束时间 23:59:59
            today.setHours(23, 59, 59, 999);

            this.$set(this.input, 'createdonDR', [yesterday, today]);
        },

        //构建FetchXML查询语句
        buildFetchXml() {

            // 动态添加过滤条件
            const filters = [];
            const pluginNameList = [];

            // 添加插件名称过滤
            if (!this.rtcrm.isNullOrWhiteSpace(this.input.pluginName)) {
                pluginNameList.push(this.input.pluginName);
            }
            if (!this.rtcrm.isNullOrWhiteSpace(this.input.pluginName2)) {
                pluginNameList.push(this.input.pluginName2);
            }
            if (!this.rtcrm.isNull(pluginNameList) && pluginNameList.length > 0) {
                let pluginNameFilter = "";
                for (let i of pluginNameList) {
                    pluginNameFilter += `<condition attribute="typename" operator="like" value="${i}%" />`;
                }
                filters.push(`<filter type="or">${pluginNameFilter}</filter>`);
            }

            // 添加日期范围过滤
            if (this.input.createdonDR && this.input.createdonDR.length === 2) {
                // Fetch XML 会将日期过滤转换为 UTC 时间，因此需要将本地时间转换为 UTC 时间
                const startDate = new Date(this.input.createdonDR[0]);
                const endDate = new Date(this.input.createdonDR[1]);

                // 获取时区偏移量（分钟），getTimezoneOffset() 返回本地时间与 UTC 的差值
                // 例如 UTC+8 返回 -480，UTC-5 返回 300
                const timezoneOffset = startDate.getTimezoneOffset();

                // 将本地时间转换为 UTC 时间（减去时区偏移量，因为 offset 是负数）
                // UTC 时间 = 本地时间 - offset * 60 * 1000（毫秒）
                const startDateUTC = new Date(startDate.getTime() + timezoneOffset * 60 * 1000);
                const endDateUTC = new Date(endDate.getTime() + timezoneOffset * 60 * 1000);

                const startDateStr = this.rtcrm.formatDate(startDateUTC, "yyyy-MM-dd hh:mm:ss");
                const endDateStr = this.rtcrm.formatDate(endDateUTC, "yyyy-MM-dd hh:mm:ss");

                filters.push(`<condition attribute="createdon" operator="between"><value>${startDateStr}</value><value>${endDateStr}</value></condition>`);
            }

            // 实体名称
            if (!this.rtcrm.isNullOrWhiteSpace(this.input.entityName)) {
                filters.push(`<condition attribute="primaryentity" operator="eq" value="${this.input.entityName}" />`);
            }

            let fetchXml = `<fetch mapping="logical" version="1.0" return-total-record-count="true" >
  <entity name="plugintracelog">
    <attribute name="createdon" />
    <attribute name="typename" />
    <attribute name="performanceexecutionduration" />
    <attribute name="messagename" />
    <attribute name="primaryentity" />
    <attribute name="mode" />
    <attribute name="messageblock" />
    <order attribute="createdon" descending="true" />
    <filter type="and">
        ${filters.length > 0 ? filters.join('\n') : ''}
    </filter>
  </entity>
</fetch>`;

            return fetchXml;
        },

        //分页变更
        handleSizeChange: function (val) {
            this.$set(this.input, "pageSize", val);
            this.$set(this.input, "pageIndex", 1); // 重置到第一页
            this.getData();
        },

        //页码变更
        handleCurrentChange: function (val) {
            this.$set(this.input, "pageIndex", val);
            this.getData();
        },

        //日期选择器focus事件
        handleDatePickerFocus: function () {
            // 如果已有日期范围，设置开始时间的时分秒为00:00:00，结束时间的时分秒为23:59:59
            if (this.input.createdonDR && Array.isArray(this.input.createdonDR) && this.input.createdonDR.length === 2) {
                const startDate = new Date(this.input.createdonDR[0]);
                const endDate = new Date(this.input.createdonDR[1]);

                // 设置开始时间为当天的00:00:00
                startDate.setHours(0, 0, 0, 0);
                // 设置结束时间为当天的23:59:59
                endDate.setHours(23, 59, 59, 999);

                // 更新日期范围
                this.$set(this.input, 'createdonDR', [startDate, endDate]);
            }
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

/* 详情面板新增样式 */
.detail-item.full-width {
    flex-direction: column;
    align-items: flex-start;
}

.detail-item.full-width label {
    margin-bottom: 8px;
    min-width: auto;
}

.message-content {
    width: 100%;
}

.message-content .el-textarea {
    margin-bottom: 8px;
}
</style>
