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
                            prop: 'name',
                            order: 'ascending',
                        }" @selection-change="handleSelectionChange">
                        <!-- 全选 -->
                        <el-table-column type="selection" label="全选" align="center" width="40" fixed>
                        </el-table-column>
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 模板名称 -->
                        <el-table-column prop="name" label="模板名称" show-overflow-tooltip width="160" sortable>
                        </el-table-column>
                        <!-- 模板类型 -->
                        <el-table-column prop="documenttype" label="模板类型" width="130" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 关联实体 -->
                        <el-table-column prop="associatedentityName" label="关联实体" show-overflow-tooltip width="120">
                        </el-table-column>
                        <!-- 语言 -->
                        <el-table-column prop="languageName" label="语言" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 操作 -->
                        <el-table-column fixed="right" label="操作" show-overflow-tooltip>
                            <template slot-scope="scope">
                                <el-button @click="
                                    downloadTemplate('envirFrom', scope.row)" type=" text" size="small">下载</el-button>
                            </template>
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
                            prop: 'name',
                            order: 'ascending',
                        }">
                        <!-- 行号 -->
                        <el-table-column type="index" align="center" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 模板名称 -->
                        <el-table-column prop="name" label="模板名称" show-overflow-tooltip width="160" sortable>
                        </el-table-column>
                        <!-- 模板类型 -->
                        <el-table-column prop="documenttype" label="模板类型" width="130" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 关联实体 -->
                        <el-table-column prop="associatedentityName" label="关联实体" show-overflow-tooltip width="120">
                        </el-table-column>
                        <!-- 语言 -->
                        <el-table-column prop="languageName" label="语言" show-overflow-tooltip>
                        </el-table-column>
                        <!-- 操作 -->
                        <el-table-column fixed="right" label="操作" show-overflow-tooltip>
                            <template slot-scope="scope">
                                <el-button @click="
                                    downloadTemplate('envirTo', scope.row)" type=" text" size="small">下载</el-button>
                            </template>
                        </el-table-column>
                    </el-table>
                </div>
            </el-card>
        </el-col>
    </div>
</template>

<script>
export default {
    name: 'DocumentTemplateTable',
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
        input: {
            envirFrom: "dev",
            envirTo: "uat",
        },
    },
    created() {
    },
    mounted() {
    },
    computed: {
        // 检测是否为桌面端环境（复用 JsCrmHelper 的方法）
        isDesktop() {
            return this.jshelper && this.jshelper.isDesktopEnvironment 
                ? this.jshelper.isDesktopEnvironment() 
                : false;
        },
    },
    methods: {
        //处理表格多选
        handleSelectionChange(val) {
            this.$emit('handle-selection-change', val);
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$emit('show-message', message, type, dangerouslyUseHTMLString);
        },

        //增加MessageTab
        addMessageTab: function (message) {
            this.$emit("add-message-tab", message);
        },

        //唤起Loading
        formLoading: function () {
            this.$emit('form-loading');
        },

        formLoadingClose: function () {
            this.$emit('form-loading-close');
        },

        //系统模板-操作-下载
        downloadTemplate: function (envir, row) {
            let _this = this;
            try {
                this.formLoading();
                let input = {
                    envir: this.input[envir],
                    templateId: row.Id,
                };
                console.log(input);

                //#region 下载模板
                this.jshelper
                    .invokeHiddenApiAsync(
                        "new_hbxn_common",
                        "SyncConfiguration/DownloadTemplate",
                        JSON.stringify(input)
                    )
                    .then((res) => {
                        _this.formLoadingClose();
                        if (this.rtcrm.isNull(res)) {
                            this.rtcrm.openAlertDialog(
                                "返回数据为空"
                            );
                            return;
                        }
                        if (res.isSuccess) {
                            // 根据环境选择下载方式
                            if (_this.isDesktop) {
                                // 桌面端：使用 C# 方法保存文件（内部会显示消息）
                                _this.saveFileDesktop(res.data, res.message);
                            } else {
                                // Web 端：使用 JavaScript 下载
                                _this.base64ToFile(res.data, res.message);
                                _this.showMessage("请求成功", "success", true);
                            }
                        } else {
                            _this.showMessage("请求失败", "error", true);
                        }
                    })
                    .catch((err) => {
                        _this.formLoadingClose();
                        this.jshelper.openAlertDialog(_this, err.message, "系统模板-操作-下载");
                        _this.addMessageTab(err.message);
                    });
                //#endregion

            } catch (err) {
                _this.formLoadingClose();
                this.jshelper.openAlertDialog(_this, err.message, "系统模板-操作-下载");
                _this.addMessageTab(err.message);
            }
        },

        //桌面端保存文件（通过 C# 方法）
        saveFileDesktop: function (base64, fileName) {
            let _this = this;
            try {
                // 检查 nativeHost 是否可用
                if (typeof window === 'undefined' || !window.nativeHost) {
                    _this.showMessage("桌面端环境未初始化，无法保存文件", "error", true);
                    return;
                }

                var input = {
                    base64Data: base64,
                    fileName: fileName
                };

                // 调用 C# 方法保存文件
                window.nativeHost.executeCommandAsync(
                    'System/SaveFile',
                    JSON.stringify(input),
                    function (success, result) {
                        try {
                            var resultObj = typeof result === 'string' ? JSON.parse(result) : result;
                            if (success && resultObj && resultObj.success) {
                                _this.showMessage(resultObj.message || "文件保存成功", "success", true);
                            } else {
                                var errorMsg = resultObj && resultObj.error ? resultObj.error : (result || "文件保存失败");
                                _this.showMessage(errorMsg, "error", true);
                            }
                        } catch (e) {
                            // 如果解析失败，直接使用原始结果
                            if (success) {
                                _this.showMessage("文件保存成功", "success", true);
                            } else {
                                _this.showMessage("文件保存失败: " + (result || e.message), "error", true);
                            }
                        }
                    }
                );
            } catch (err) {
                _this.showMessage("保存文件时发生错误: " + err.message, "error", true);
            }
        },

        //#region base64转文件下载（Web端使用）
        base64ToFile: function (base64, fileName) {
            var myBlob = this.dataURLtoBlob(base64, fileName);
            var myUrl = URL.createObjectURL(myBlob);
            this.downloadFile(myUrl, fileName);
            // 延迟释放 URL，确保下载完成后再释放
            setTimeout(() => {
                URL.revokeObjectURL(myUrl);
            }, 100);
        },

        dataURLtoBlob: function (dataurl, fileName) {
            var arr = dataurl.split(",");
            var mime = arr[0].match(/:(.*?);/);
            if (!this.rtcrm.isNull(mime) && mime.length > 0) {
                mime = mime[1];
            } else {
                let ext = this.getFileExtension(fileName);
                switch (ext) {
                    case "xlsx":
                        mime =
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    case "docx":
                        mime =
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;
                    default:
                        this.showMessage("无法识别的扩展", "error");
                        break;
                }
            }
            var base64 = !this.rtcrm.isNull(arr) && arr.length > 1 ? arr[1] : arr[0];
            var bstr = null;
            if (this.safeAtob(base64)) {
                bstr = atob(base64);
            } else if (this.safeAtob(window.atob(base64))) {
                bstr = atob(window.atob(base64));
            } else {
                bstr = atob(base64);
            }
            var n = bstr.length;
            var u8arr = new Uint8Array(n);
            while (n--) {
                u8arr[n] = bstr.charCodeAt(n);
            }
            return new Blob([u8arr], { type: mime });
        },

        downloadFile: function (url, name) {
            var a = document.createElement("a");
            a.href = url;
            a.download = name;
            a.style.display = "none";
            
            // 将元素添加到 DOM（某些环境要求元素在 DOM 中才能触发下载）
            document.body.appendChild(a);
            
            // 使用现代的 click() 方法，兼容性更好
            try {
                a.click();
            } catch (err) {
                // 如果 click() 失败，尝试使用 MouseEvent（作为后备方案）
                var clickEvent = new MouseEvent("click", {
                    view: window,
                    bubbles: true,
                    cancelable: true
                });
                a.dispatchEvent(clickEvent);
            }
            
            // 延迟移除元素，确保下载已触发
            setTimeout(() => {
                document.body.removeChild(a);
            }, 100);
        },

        //检查是否为安全的base64字符串
        safeAtob: function (base64Str) {
            if (this.rtcrm.isNullOrWhiteSpace(base64Str))
                throw new Error("base64Str字符串不能为空");

            // 检查输入字符串是否是有效的Base64编码
            const base64Regex = /^[A-Za-z0-9+/]+={0,2}$/;
            if (!base64Regex.test(base64Str)) {
                return false;
            }

            // 如果输入字符串长度不是4的倍数，添加等号'='
            while (base64Str.length % 4 !== 0) {
                base64Str += "=";
            }

            return true;
        },
        //#endregion

        //获取文件名后缀
        getFileExtension: function (filename) {
            // 获取文件名中最后一个点的位置
            const lastDot = filename.lastIndexOf(".");
            // 如果存在点，且点位置不是在文件名的开头，则获取后缀名
            if (lastDot !== -1 && lastDot !== 0) {
                return filename.substring(lastDot + 1);
            } else {
                // 没有点或者点在开头表示没有后缀名
                return "";
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
</style>