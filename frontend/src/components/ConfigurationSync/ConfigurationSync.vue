<template>
  <div class="main-container">
    <el-container>
      <el-header>
        <h3 class="credit-title">系统配置自动同步</h3>
        <el-divider></el-divider>
      </el-header>
      <el-main>
        <el-form :model="input" :rules="rules" ref="form" size="medium" label-position="left">
          <!-- 输入行 -->
          <el-row :gutter="24">
            <!-- 从环境： -->
            <el-col :span="8">
              <el-form-item prop="envirFrom" label="从环境：" label-width="160px">
                <el-select size="small" v-model="input.envirFrom" placeholder="请选择" :disabled="loading"
                  @change="envirFromChange">
                  <el-option v-for="item in environments" :key="item.key" :label="item.label" :value="item.key">
                  </el-option>
                </el-select>
              </el-form-item>
            </el-col>

            <!-- 同步到环境： -->
            <el-col :span="8">
              <el-form-item prop="envirTo" label="同步到环境：" label-width="160px">
                <el-select size="small" v-model="input.envirTo" placeholder="请选择" :disabled="loading"
                  @change="envirToChange">
                  <el-option v-for="item in environments" :key="item.key" :label="item.label" :value="item.key">
                  </el-option>
                </el-select>
              </el-form-item>
            </el-col>

            <!-- 按钮 -->
            <el-col :span="4">
              <el-form-item>
                <el-button size="small" :loading="loading" @click="getData(null)">查询</el-button>
                <el-button size="small" :loading="loading" @click="handlerConfirm" type="primary"> 确认</el-button>
              </el-form-item>
            </el-col>
          </el-row>
          <!-- 过滤行 -->
          <el-row :gutter="24" style="min-height: 28px">
            <el-col :span="9" v-show="createdonRangeShow">
              <!-- 创建、修改时间 -->
              <el-form-item prop="createdonDR" label="创建、修改时间：" required>
                <el-date-picker size="small" v-model="input.createdonDR" type="daterange" range-separator="至"
                  start-placeholder="开始日期" end-placeholder="结束日期" :disabled="loading">
                </el-date-picker>
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
        <!-- 展示内容 -->
        <el-row>
          <el-tabs type="border-card" v-model="nowTabName" @tab-click="handleTabClick" @tab-remove="tabRemove">
            <el-tab-pane v-for="(item) in tabs" :label="item['label']" :disabled="loading" :closable="item.closable"
              :name="item.entityName" :key="item.entityName">
              <!-- 处理结果 -->
              <el-row v-show="!item.isSysConfig" :gutter="10">
                <el-col :span="24">
                  <el-input type="textarea" readonly resize="none" :autosize="{ minRows: 16 }" v-model="requestMessage">
                  </el-input>
                </el-col>
              </el-row>
              <!-- 系统配置 -->
              <el-row v-show="item.isSysConfig" :gutter="10">
                <!-- 表格过滤控件 -->
                <el-row :gutter="24" style="margin-bottom: 15px;">
                  <el-col :offset="20" :span="4">
                    <el-input size="small" v-model="input.tableFilter" placeholder="输入关键字搜索" clearable
                      :disabled="loading" @change="handleTableFilter">
                    </el-input>
                  </el-col>
                </el-row>
                <!-- 系统参数 -->
                <system-parameter-table v-if="item.entityName === 'new_systemparameter'" :tableData="filteredTableData"
                  :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel" :tableKey="tableKey"
                  :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></system-parameter-table>
                <!-- 自动编号 -->
                <auto-number-table v-else-if="item.entityName === 'new_autonumber'" :tableData="filteredTableData"
                  :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel" :tableKey="tableKey"
                  :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></auto-number-table>
                <!-- 明细汇总 -->
                <sum-relationship-detail-table v-else-if="item.entityName === 'new_sumrelationshipdetail'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></sum-relationship-detail-table>
                <!-- 自定义按钮 -->
                <ribbon-table v-else-if="item.entityName === 'new_ribbon'" :tableData="filteredTableData"
                  :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel" :tableKey="tableKey"
                  :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></ribbon-table>
                <!-- 重复性检测 -->
                <duplicate-detect-table v-else-if="item.entityName === 'new_duplicatedetect'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></duplicate-detect-table>
                <!-- 数据导入 -->
                <import-table v-else-if="item.entityName === 'new_import'" :tableData="filteredTableData"
                  :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel" :tableKey="tableKey"
                  :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></import-table>
                <!-- CommonDeleteCheck -->
                <common-delete-check-table v-else-if="item.entityName === 'commondeletecheck'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></common-delete-check-table>
                <!-- 系统模板 -->
                <document-template-table v-else-if="item.entityName === 'documenttemplate'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection" :input="input" @form-loading="formLoading"
                  @form-loading-close="formLoadingClose" @add-message-tab="addMessageTab" @show-message="showMessage"
                  @handle-selection-change="handleSelectionChange"></document-template-table>
                <!-- 语言配置 -->
                <language-config-table v-else-if="item.entityName === 'new_languageconfig'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection" :input="input" @form-loading="formLoading"
                  @form-loading-close="formLoadingClose" @add-message-tab="addMessageTab" @show-message="showMessage"
                  @handle-selection-change="handleSelectionChange" @handle-size-change="handleSizeChange"
                  @handle-current-change="handleCurrentChange" @handle-size-change2="handleSizeChange2"
                  @handle-current-change2="handleCurrentChange2"></language-config-table>
                <!-- 多语言数据字段对应 -->
                <multiple-language-contrast-table v-else-if="item.entityName === 'new_multiple_language_contrast'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection"
                  @handle-selection-change="handleSelectionChange"></multiple-language-contrast-table>
                <!-- 数据多语言 -->
                <data-language-config-table v-else-if="item.entityName === 'new_data_languageconfig'"
                  :tableData="filteredTableData" :tableHeight="tableHeight" :loading="loading" :envirLabel="envirLabel"
                  :tableKey="tableKey" :multipleSelection="multipleSelection" :input="input" @form-loading="formLoading"
                  @form-loading-close="formLoadingClose" @add-message-tab="addMessageTab" @show-message="showMessage"
                  @handle-selection-change="handleSelectionChange" @handle-size-change="handleSizeChange"
                  @handle-current-change="handleCurrentChange" @handle-size-change2="handleSizeChange2"
                  @handle-current-change2="handleCurrentChange2"></data-language-config-table>
              </el-row>
            </el-tab-pane>
          </el-tabs>
        </el-row>
      </el-main>
    </el-container>
  </div>
</template>

<script>
import BaseData from './BaseData.json';
export default {
  name: 'ConfigurationSync',
  data() {
    // 创建、修改时间
    const createdonDR_rule = (rule, value, callback) => {
      if (
        this.nowTabName === "new_languageconfig" &&
        (this.rtcrm.isNull(value) || value.length !== 2)
      ) {
        callback(new Error("创建、修改时间区间不能为空！"));
      } else {
        callback();
      }
    };
    return {
      // 配置项
      tabs: [],
      nowTab: null, //当前选中的Tab
      nowTabName: "", //当前选中Tab的entityName
      // 环境
      environments: [{ label: "无效环境请刷新", key: "undefined" }],
      // 输入
      input: {
        envirFrom: "dev",
        envirTo: "uat",
        pageIndexEnvirFrom: 1,
        pageSizeEnvirFrom: 20,
        pageIndexEnvirTo: 1,
        pageSizeEnvirTo: 20,
        createdonDR: [],
        tableFilter: "", // 表格过滤关键字
      },
      // 优化：按 tab (entityName) 存储数据，确保每个 tab 的数据独立
      tableDataByTab: {}, // 按 entityName 存储原始数据
      filteredTableDataByTab: {}, // 按 entityName 存储过滤后的数据
      defaultTableHeight: "370", //表格高度
      tableKey: 1, //刷新表格的Key
      multipleSelection: [], //表格选中
      loading: false, //是否加载数据中
      retrieveEntityName: "", //上次查询的实体名
      requestMessage: "", //执行接口后的日志
      envirLabel: [],//当前环境标签
      // 优化：缓存 tableHeight 计算结果
      cachedTableHeight: "",
      // 优化：记录上次过滤文本，避免重复过滤
      lastFilterText: "",
      // 表单校验规则
      rules: {
        // 创建、修改时间
        createdonDR: [{ validator: createdonDR_rule, trigger: "change" }],
      },
    };
  },
  components: {
    SystemParameterTable: () => import('@/components/ConfigurationSync/components/SystemParameterTable'),
    AutoNumberTable: () => import('@/components/ConfigurationSync/components/AutoNumberTable'),
    SumRelationshipDetailTable: () => import('@/components/ConfigurationSync/components/SumRelationshipDetailTable'),
    RibbonTable: () => import('@/components/ConfigurationSync/components/RibbonTable'),
    DuplicateDetectTable: () => import('@/components/ConfigurationSync/components/DuplicateDetectTable'),
    ImportTable: () => import('@/components/ConfigurationSync/components/ImportTable'),
    CommonDeleteCheckTable: () => import('@/components/ConfigurationSync/components/CommonDeleteCheckTable'),
    DocumentTemplateTable: () => import('@/components/ConfigurationSync/components/DocumentTemplateTable'),
    LanguageConfigTable: () => import('@/components/ConfigurationSync/components/LanguageConfigTable'),
    MultipleLanguageContrastTable: () => import('@/components/ConfigurationSync/components/MultipleLanguageContrastTable'),
    DataLanguageConfigTable: () => import('@/components/ConfigurationSync/components/DataLanguageConfigTable'),
  },
  created() {
    //配置文件初始化
    this.BaseDataInit();

    //初始化
    // 优化：添加空值检查，避免 tabs 为空时出错
    if (this.tabs && this.tabs.length > 0) {
      this.$set(this, "nowTab", this.tabs[0]);
      this.$set(this, "nowTabName", this.nowTab.entityName);
    }

    //#region 初始化-创建、修改时间区间
    let nowDT = new Date(this.rtcrm.formatDate(new Date(), "yyyy-MM-dd"));
    if (nowDT.getDate() > 15)
      nowDT.setMonth(nowDT.getMonth(), 1);
    else
      nowDT.setMonth(nowDT.getMonth() - 1, 1);
    this.input.createdonDR.push(nowDT);
    this.input.createdonDR.push(
      new Date(this.rtcrm.formatDate(new Date(), "yyyy-MM-dd"))
    );
    //#endregion

    //获取系统参数
    this.getEnvironments();
  },
  mounted() {
    //监听环境参数切换
    this.$on('environment-change', this.environmentChange);
  },
  watch: {
    // 优化：监听 nowTabName 变化，确保数据结构存在
    nowTabName: {
      handler(newVal) {
        if (newVal) {
          // 确保当前 tab 的数据结构存在
          if (!this.tableDataByTab[newVal]) {
            this.$set(this.tableDataByTab, newVal, {
              ecFrom: [],
              ecFromTotalRecord: 0,
              ecTo: [],
              ecToTotalRecord: 0,
            });
          }
          if (!this.filteredTableDataByTab[newVal]) {
            this.$set(this.filteredTableDataByTab, newVal, {
              ecFrom: [],
              ecTo: [],
            });
          }
        }
      },
      immediate: true,
    },
    // 优化：监听 isDesktop 变化，更新 tableHeight 缓存
    isDesktop: {
      handler() {
        this.cachedTableHeight = "";
      },
    },
  },
  computed: {
    // 选中行格式化为Guid List
    configIdList() {
      let array = [];
      this.multipleSelection.forEach((item) => {
        if (!this.rtcrm.isNullOrWhiteSpace(item.Id)) array.push(item.Id);
        else if (!this.rtcrm.isNullOrWhiteSpace(item.id)) array.push(item.id);
      });
      return array;
    },
    // 创建、修改时间区间格式化为字符串
    createdonRange() {
      if (
        this.rtcrm.isNull(this.input.createdonDR) ||
        this.input.createdonDR.length !== 2
      )
        return "";
      else {
        return (
          this.rtcrm.formatDate(this.input.createdonDR[0], "yyyy-MM-dd") +
          ";" +
          this.rtcrm.formatDate(this.input.createdonDR[1], "yyyy-MM-dd")
        );
      }
    },
    // 是否显示{创建、修改时间区间}
    createdonRangeShow() {
      return ["new_languageconfig", "new_data_languageconfig"].includes(this.nowTabName);
    },
    // 检测是否为桌面端环境（复用 JsCrmHelper 的方法）
    isDesktop() {
      return this.jshelper && this.jshelper.isDesktopEnvironment
        ? this.jshelper.isDesktopEnvironment()
        : false;
    },
    // Table高度 - 优化：使用缓存避免重复计算
    tableHeight() {
      if (this.cachedTableHeight) {
        return this.cachedTableHeight;
      }
      let height = parseInt(this.defaultTableHeight);
      const result = this.isDesktop ? height + 95 + "px" : height + "px";
      this.cachedTableHeight = result;
      return result;
    },
    // 获取当前 tab 的原始数据 - 优化：移除 computed 中的副作用
    tableData() {
      const entityName = this.nowTabName;
      if (!entityName) {
        return {
          ecFrom: [],
          ecFromTotalRecord: 0,
          ecTo: [],
          ecToTotalRecord: 0,
        };
      }
      // 优化：不在 computed 中使用 $set，直接返回或返回空对象
      return this.tableDataByTab[entityName] || {
        ecFrom: [],
        ecFromTotalRecord: 0,
        ecTo: [],
        ecToTotalRecord: 0,
      };
    },
    // 获取当前 tab 的过滤后数据 - 优化：移除 computed 中的副作用
    filteredTableData() {
      const entityName = this.nowTabName;
      if (!entityName) {
        return {
          ecFrom: [],
          ecTo: [],
        };
      }
      // 优化：不在 computed 中使用 $set，直接返回或返回空对象
      return this.filteredTableDataByTab[entityName] || {
        ecFrom: [],
        ecTo: [],
      };
    },
  },
  methods: {
    //配置文件初始化
    BaseDataInit: function () {
      if (!this.rtcrm.isNull(BaseData)) {
        if (!this.rtcrm.isNull(BaseData.tabs) && BaseData.tabs.length > 0) {
          this.$set(this, "tabs", BaseData.tabs);
        }
      }
    },

    //环境切换事件
    environmentChange: function (envir) {
      if (this.rtcrm.isNullOrWhiteSpace(envir)) return;

      //清空所有 tab 的数据
      this.$set(this, "tableDataByTab", {});
      this.$set(this, "filteredTableDataByTab", {});
      this.$set(this, "multipleSelection", []); //清空数据
      this.$set(this.input, "tableFilter", ""); //清空过滤条件
      // 优化：清空过滤文本缓存
      this.lastFilterText = "";
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
          _this.GetEnvirLabels();
        })
        .catch((err) => {
          _this.$set(_this, "loading", false);
          _this.jshelper.openAlertDialog(_this, err.message, "获取环境参数");
        });
    },

    //查询
    getData: function (envir) {
      let input = {};
      let route = "";
      let _this = this;
      this.$refs["form"].validate(async (valid, error) => {
        if (valid) {
          //#region 清空数据
          const entityName = this.nowTab.entityName;
          // 优化：数据结构初始化已由 watch 处理，这里不需要重复初始化
          // 清空当前 tab 的数据
          if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirFrom") {
            this.$set(this.tableDataByTab[entityName], "ecFrom", []);
            this.$set(this.tableDataByTab[entityName], "ecFromTotalRecord", 0);
          }
          if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirTo") {
            this.$set(this.tableDataByTab[entityName], "ecTo", []);
            this.$set(this.tableDataByTab[entityName], "ecToTotalRecord", 0);
          }
          this.$set(this, "multipleSelection", []); //清空数据
          this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
          //#endregion

          //#region 入参格式化
          switch (this.nowTab.entityName) {
            case "new_systemparameter":
            case "new_autonumber":
            case "new_duplicatedetect":
            case "new_sumrelationshipdetail":
            case "new_ribbon":
            case "commondeletecheck":
            case "new_import":
            case "documenttemplate":
            case "new_multiple_language_contrast":
              input["entityName"] = this.nowTab.entityName;
              input["envirFrom"] = this.input.envirFrom;
              input["envirTo"] = this.input.envirTo;
              if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirFrom")
                this.$set(this.input, "pageIndexEnvirFrom", 1); //还原分页页码
              if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirTo")
                this.$set(this.input, "pageIndexEnvirTo", 1); //还原分页页码
              break;
            case "new_languageconfig":
            case "new_data_languageconfig":
              input["entityName"] = this.nowTab.entityName;
              input["envirFrom"] = this.input.envirFrom;
              input["envirTo"] = this.input.envirTo;
              input["createdonRange"] = this.createdonRange;
              if (this.rtcrm.isNullOrWhiteSpace(envir)) {
                this.$set(this.input, "pageIndexEnvirFrom", 1); //还原分页页码
                this.$set(this.input, "pageIndexEnvirTo", 1); //还原分页页码
              }
              input["pageIndexEnvirFrom"] = this.input.pageIndexEnvirFrom;
              input["pageSizeEnvirFrom"] = this.input.pageSizeEnvirFrom;
              input["pageIndexEnvirTo"] = this.input.pageIndexEnvirTo;
              input["pageSizeEnvirTo"] = this.input.pageSizeEnvirTo;
              break;
          }
          switch (this.nowTab.entityName) {
            case "new_systemparameter":
            case "new_autonumber":
            case "new_duplicatedetect":
            case "new_sumrelationshipdetail":
            case "new_ribbon":
            case "commondeletecheck":
            case "new_import":
            case "documenttemplate":
            case "new_multiple_language_contrast":
              route = "SyncConfiguration/GetSystemConfigs";
              break;
            case "new_languageconfig":
            case "new_data_languageconfig":
              route = "SyncConfiguration/GetSystemConfigsByDateRange";
              break;
          }
          if (this.rtcrm.isNullOrWhiteSpace(route)) return;
          //#endregion

          //#region 查询数据
          this.$set(this, "loading", true);
          // 优化：移除生产代码中的 console.log
          this.jshelper
            .invokeHiddenApiAsync("new_hbxn_common", route, JSON.stringify(input))
            .then((res) => {
              _this.$set(_this, "loading", false);
              _this.$set(_this, "retrieveEntityName", _this.nowTab.entityName); //记录上次查询的实体名
              if (this.rtcrm.isNull(res) || this.rtcrm.isNull(res.data)) {
                this.jshelper.openAlertDialog(this,
                  "返回数据为空", "系统配置查询"
                );
                return;
              }
              if (res.isSuccess) {
                let data = res.data;
                const entityName = _this.nowTab.entityName;
                // 优化：数据结构初始化已由 watch 处理，这里不需要重复初始化
                // 优化：将数据存储到当前 tab 的 key 下
                if (Array.isArray(data.ecFrom) && Array.isArray(data.ecTo)) {
                  if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirFrom")
                    _this.$set(_this.tableDataByTab[entityName], "ecFrom", data.ecFrom);
                  if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirTo")
                    _this.$set(_this.tableDataByTab[entityName], "ecTo", data.ecTo);
                } else {
                  if (
                    this.rtcrm.isNullOrWhiteSpace(envir) ||
                    envir === "envirFrom"
                  ) {
                    _this.$set(_this.tableDataByTab[entityName], "ecFrom", data.ecFrom.data);
                    _this.$set(
                      _this.tableDataByTab[entityName],
                      "ecFromTotalRecord",
                      data.ecFrom.TotalRecordCount
                    );
                  }
                  if (this.rtcrm.isNullOrWhiteSpace(envir) || envir === "envirTo") {
                    _this.$set(_this.tableDataByTab[entityName], "ecTo", data.ecTo.data);
                    _this.$set(
                      _this.tableDataByTab[entityName],
                      "ecToTotalRecord",
                      data.ecTo.TotalRecordCount
                    );
                  }
                }
              } else {
                this.jshelper.openAlertDialog(this, res.message, "系统配置查询");
              }
              // 应用表格过滤
              _this.filterTableData();
              _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
            })
            .catch((err) => {
              _this.$set(_this, "loading", false);
              _this.jshelper.openAlertDialog(_this, err.message, "系统配置查询");
            });
          //#endregion
        }
      });
    },

    //处理表格多选
    handleSelectionChange(val) {
      this.$set(this, "multipleSelection", val); //清空
    },

    //确认
    handlerConfirm: function () {
      //#region 校验提示
      if (this.rtcrm.isNull(this.configIdList) || this.configIdList.length === 0) {
        this.showMessage("请选择至少一条数据！", "warning");
        return;
      } else if (this.retrieveEntityName !== this.nowTab.entityName) {
        this.showMessage("切换配置项需要及时点击查询刷新页面", "warning");
        return;
      }
      //#endregion

      let _this = this;
      this.$set(this.input, "pageIndexEnvirFrom", 1); //还原分页页码
      this.$set(this.input, "pageIndexEnvirTo", 1); //还原分页页码
      let tips = this.getTipsFromConfig(this.nowTab.entityName); //获取配置项提示
      if (!this.rtcrm.isNullOrWhiteSpace(tips)) {
        this.jshelper.openConfirmDialog(tips, {}, (parms) => {
          if (!parms.confirmed) return;

          this.jshelper.openConfirmDialog("确认同步？", {}, (parms2) => {
            if (!parms2.confirmed) return;

            this.jshelper.showLoading();
            this.$set(_this, "loading", true);
            let input = {
              envirFrom: _this.input.envirFrom,
              envirTo: _this.input.envirTo,
              entityName: _this.nowTab.entityName,
              configList: _this.configIdList,
            };

            //#region 同步系统配置
            this.jshelper
              .invokeHiddenApiAsync(
                "new_hbxn_common",
                "SyncConfiguration/SyncSystemConfigs",
                input
              )
              .then((res) => {
                this.jshelper.closeLoading();
                this.$set(_this, "loading", false);
                if (_this.rtcrm.isNull(res)) {
                  this.rtcrm.openAlertDialog(
                    "返回数据为空"
                  );
                  return;
                }
                if (res.isSuccess) {
                  _this.showMessage("处理完成", "success", true);
                  _this.getData(); //刷新数据
                } else {
                  this.jshelper.openAlertDialog(this, res.message, "系统配置同步");
                }
                if (!this.rtcrm.isNullOrWhiteSpace(res.message))
                  _this.addMessageTab(res.message);

                _this.$set(_this, "multipleSelection", []); //清空数据
                _this.$set(_this, "tableKey", _this.tableKey + 1); //刷新Table
              })
              .catch((err) => {
                _this.$set(_this, "loading", false);
                _this.jshelper.closeLoading();
                _this.jshelper.openAlertDialog(_this, err.ExceptionMessage ? err.ExceptionMessage : err.message, "系统配置同步");
                _this.addMessageTab(err.message);
              });
            //#endregion
          });
        });
      }
    },

    //Tab Click事件
    handleTabClick: function (tab, event) {
      if (this.rtcrm.isNull(tab)) return;
      // 优化：切换 tab 时，加载对应 tab 的数据，确保数据独立
      const newEntityName = this.tabs[tab.index].entityName;
      this.$set(this, "nowTab", this.tabs[tab.index]);
      this.$set(this, "nowTabName", newEntityName);
      // 重置过滤文本缓存，确保切换 tab 后可以正确过滤
      this.lastFilterText = "";
      // 优化：使用 $nextTick 确保 watch 执行完成后再调用 filterTableData
      this.$nextTick(() => {
        // 应用当前 tab 的过滤条件
        this.filterTableData();
        this.$set(this, "multipleSelection", []); //清空选中
        this.$set(this, "tableKey", this.tableKey + 1); //刷新Table
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

    GetEnvirLabels: function () {
      this.$set(this, "envirLabel", []);
      let labels = [];
      labels.push(this.GetEnvirLabel(this.input.envirFrom).label);
      labels.push(this.GetEnvirLabel(this.input.envirTo).label);
      this.$set(this, "envirLabel", labels);
    },

    //获取环境的Label
    GetTabLabel: function (val) {
      let obj = {};
      obj = this.tabs.find((item) => {
        return item.entityName === val;
      });
      return obj;
    },

    //envirFrom下拉Change事件
    envirFromChange: function () {
      this.showMessage("切换环境需要及时点击查询刷新页面", "warning");
      this.GetEnvirLabels();
    },

    //envirTo下拉Change事件
    envirToChange: function () {
      this.showMessage("切换环境需要及时点击查询刷新页面", "warning");
      this.GetEnvirLabels();
    },

    //各配置项同步提示
    getTipsFromConfig(entityName) {
      // 优化：添加空值检查，避免 GetTabLabel 返回 undefined 时出错
      const tabLabel = this.GetTabLabel(entityName);
      if (!tabLabel || !tabLabel.label) {
        return "";
      }
      let tips =
        "同步" + tabLabel.label + "，请注意以下事项：<br>";
      switch (entityName) {
        case "new_systemparameter":
          tips +=
            "1、请勿全选同步，以免覆盖账号密码等关键信息！<br>2、注释只同步附件，会删除目标环境的附件后再同步，如有必要请先备份！";
          break;
        case "new_autonumber":
        case "new_duplicatedetect":
        case "new_sumrelationshipdetail":
        case "new_multiple_language_contrast":
          tips += "1、同步后请到目标环境点击【初始化】按钮！<br>";
          break;
        case "new_ribbon":
          break;
        case "commondeletecheck":
          tips +=
            "1、该功能仅能同步Config和Secure Config的信息，请先通过解决方案导入步骤后再同步！<br>";
          break;
        case "new_languageconfig":
          tips += "1、该功能仅限于小批量数据同步，为了避免执行超时，请勿一次性选中超过50条数据！<br>";
          break;
        case "new_data_languageconfig":
          tips += "1、同步后请到目标环境点击【初始化】按钮！<br>";
          tips += "1、该功能仅限于小批量数据同步，为了避免执行超时，请勿一次性选中超过50条数据！<br>";
          break;
      }
      return tips;
    },

    //showMessage
    showMessage(message, type, dangerouslyUseHTMLString = false) {
      this.$message({
        message: message,
        dangerouslyUseHTMLString: dangerouslyUseHTMLString,
        type: type,
      });
    },

    //增加MessageTab
    addMessageTab: function (message) {
      // 优化：使用 find 直接返回结果，简化逻辑
      this.$set(this, "requestMessage", message);
      const messageTab = this.tabs.find(item => item.entityName === "requestMessage");
      if (this.rtcrm.isNull(messageTab)) {
        this.tabs.push({
          label: "处理结果",
          entityName: "requestMessage",
          closable: true,
          isSysConfig: false,
        });
      }
    },

    //移除Tab
    tabRemove: function (targetName) {
      // 优化：使用 findIndex 替代 find + 手动索引，代码更简洁高效
      const index = this.tabs.findIndex(item => item.entityName === targetName);
      if (index !== -1) {
        this.tabs.splice(index, 1);
      }
    },

    //分页变更-envirFrom
    handleSizeChange: function (val) {
      this.$set(this.input, "pageSizeEnvirFrom", val);
    },

    //页码变更-envirFrom
    handleCurrentChange: function (val) {
      this.$set(this.input, "pageIndexEnvirFrom", val);
      this.getData("envirFrom");
    },

    //分页变更-envirTo
    handleSizeChange2: function (val) {
      this.$set(this.input, "pageSizeEnvirTo", val);
    },

    //页码变更-envirTo
    handleCurrentChange2: function (val) {
      this.$set(this.input, "pageIndexEnvirTo", val);
      this.getData("envirTo");
    },

    //唤起Loading
    formLoading: function () {
      this.jshelper.showLoading();
      this.$set(this, "loading", true);
    },

    formLoadingClose: function () {
      this.jshelper.closeLoading();
      this.$set(this, "loading", false);
    },

    //表格过滤方法
    handleTableFilter: function () {
      this.filterTableData();
    },


    //过滤表格数据
    filterTableData: function () {
      // 优化：使用当前 tab 的数据进行过滤，确保每个 tab 的数据独立
      const entityName = this.nowTabName;
      if (!entityName) return;

      // 优化：确保数据结构存在（watch 可能还未执行，需要同步检查）
      if (!this.tableDataByTab[entityName]) {
        this.$set(this.tableDataByTab, entityName, {
          ecFrom: [],
          ecFromTotalRecord: 0,
          ecTo: [],
          ecToTotalRecord: 0,
        });
      }
      if (!this.filteredTableDataByTab[entityName]) {
        this.$set(this.filteredTableDataByTab, entityName, {
          ecFrom: [],
          ecTo: [],
        });
      }

      const currentTableData = this.tableDataByTab[entityName];
      const filterText = this.input.tableFilter ? this.input.tableFilter.toLowerCase() : "";
      const currentFiltered = this.filteredTableDataByTab[entityName];

      // 优化：如果过滤文本没有变化，且已有过滤结果，则不需要重新过滤
      if (filterText === this.lastFilterText && currentFiltered && currentFiltered.ecFrom && currentFiltered.ecFrom.length > 0) {
        return;
      }
      this.lastFilterText = filterText;

      let needUpdate = false;
      let newFilteredEcFrom = [];
      let newFilteredEcTo = [];

      if (this.rtcrm.isNullOrWhiteSpace(filterText)) {
        // 如果没有过滤条件，显示所有数据
        newFilteredEcFrom = currentTableData.ecFrom || [];
        newFilteredEcTo = currentTableData.ecTo || [];
      } else {
        // 过滤ecFrom数据
        newFilteredEcFrom = (currentTableData.ecFrom || []).filter(item => {
          return this.matchFilter(item, filterText);
        });

        // 过滤ecTo数据
        newFilteredEcTo = (currentTableData.ecTo || []).filter(item => {
          return this.matchFilter(item, filterText);
        });
      }

      // 优化：只在数据真正变化时更新
      if (currentFiltered && currentFiltered.ecFrom && currentFiltered.ecTo) {
        if (currentFiltered.ecFrom.length !== newFilteredEcFrom.length ||
          currentFiltered.ecTo.length !== newFilteredEcTo.length ||
          currentFiltered.ecFrom !== newFilteredEcFrom ||
          currentFiltered.ecTo !== newFilteredEcTo) {
          needUpdate = true;
        }
      } else {
        needUpdate = true;
      }

      if (needUpdate) {
        this.$set(this.filteredTableDataByTab[entityName], "ecFrom", newFilteredEcFrom);
        this.$set(this.filteredTableDataByTab[entityName], "ecTo", newFilteredEcTo);
        // 优化：只在数据真正变化时更新 tableKey
        this.$set(this, "tableKey", this.tableKey + 1);
      }
    },

    //匹配过滤条件
    matchFilter: function (item, filterText) {
      if (!item) return false;

      // 根据当前配置项类型匹配不同的字段
      switch (this.nowTabName) {
        case "new_systemparameter":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_value, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_autonumber":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_nofieldname, filterText) ||
            this.matchString(item.new_prefix, filterText);
        case "new_sumrelationshipdetail":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_ribbon":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_duplicatedetect":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_import":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "commondeletecheck":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "documenttemplate":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_languageconfig":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_content, filterText) ||
            this.matchString(item.new_note, filterText);
        case "new_multiple_language_contrast":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_desc, filterText);
        case "new_data_languageconfig":
          return this.matchString(item.new_name, filterText) ||
            this.matchString(item.new_content, filterText) ||
            this.matchString(item.new_note, filterText);
        default:
          // 通用匹配，尝试匹配所有可能的字符串字段
          return Object.values(item).some(value =>
            typeof value === 'string' && this.matchString(value, filterText)
          );
      }
    },

    //字符串匹配
    matchString: function (value, filterText) {
      // 优化：使用 includes 替代 indexOf !== -1，代码更简洁
      if (this.rtcrm.isNullOrWhiteSpace(value)) return false;
      return value.toLowerCase().includes(filterText);
    }
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
</style>
