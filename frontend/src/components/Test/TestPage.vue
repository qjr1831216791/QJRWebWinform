<template>
    <div class="main-container">
        <el-container>
            <el-header>
                <h3 class="credit-title">系统功能测试页面</h3>
                <el-divider></el-divider>
            </el-header>
            <el-main>
                <!-- 按钮 -->
                <el-row class="el-row-testalign" :gutter="15">
                    <!-- 测试网站是否联通 -->
                    <el-col :span="8">
                        <div><el-button size="medium" @click="TestAPIRun">测试网站是否联通</el-button></div>
                    </el-col>
                    <!-- 测试目标环境组织服务是否联通 -->
                    <el-col :span="8">
                        <div><el-button size="medium" @click="TestCRMService">测试目标环境组织服务是否联通</el-button></div>
                    </el-col>
                    <!-- 测试API Post入参 -->
                    <el-col :span="8">
                        <div><el-button size="medium" @click="TestAPIPost">测试API Post入参</el-button></div>
                    </el-col>
                </el-row>
                <!-- 测试结果 -->
                <el-row class="el-row-testalign el-row-margin">
                    <el-col :span="24">
                        <el-input type="textarea" placeholder="测试结果" readonly :autosize="{ minRows: 6, maxRows: 12 }"
                            v-model="testResult">
                        </el-input>
                    </el-col>
                </el-row>
            </el-main>
        </el-container>
    </div>
</template>

<script>
export default {
    name: 'TestPage',
    components: {

    },
    data() {
        return {
            testResult: "",//测试结果
        }
    },
    methods: {
        TestAPIRun: function () {
            this.$set(this, "testResult", null);
            this.jshelper.showLoading();
            const apiMode = this.jshelper._getApiMode();
            if (apiMode === 'nativehost') {
                this.jshelper.invokeHiddenApiAsync("new_hbxn_common", "Default/TestAPIRun", null).then((resp) => {
                    this.$set(this, "testResult", JSON.stringify(resp));
                }).catch(e => {
                    this.$set(this, "testResult", JSON.stringify(e));
                }).finally(() => {
                    this.jshelper.closeLoading();
                });
            }
            else {
                this.jshelper.ApiGet("Default/TestAPIRun").then((resp) => {
                    this.$set(this, "testResult", JSON.stringify(resp));
                }).catch(e => {
                    this.$set(this, "testResult", JSON.stringify(e));
                }).finally(() => {
                    this.jshelper.closeLoading();
                });
            }
        },
        TestCRMService: function () {
            this.$set(this, "testResult", null);
            this.jshelper.showLoading();
            const apiMode = this.jshelper._getApiMode();
            if (apiMode === 'nativehost') {
                this.jshelper.invokeHiddenApiAsync("new_hbxn_common", "Default/TestCRMService", null).then((resp) => {
                    this.$set(this, "testResult", JSON.stringify(resp));
                }).catch(e => {
                    this.$set(this, "testResult", JSON.stringify(e));
                }).finally(() => {
                    this.jshelper.closeLoading();
                });
            }
            else {
                this.jshelper.ApiGet("Default/TestCRMService").then((resp) => {
                    this.$set(this, "testResult", JSON.stringify(resp));
                }).catch(e => {
                    this.$set(this, "testResult", JSON.stringify(e));
                }).finally(() => {
                    this.jshelper.closeLoading();
                });
            }
        },
        TestAPIPost: function () {
            this.$set(this, "testResult", null);
            this.jshelper.showLoading();
            let input = {
                input: "Hello World",
            };
            this.jshelper.ApiPost("Default/TestAPIPost3", input).then((resp) => {
                this.$set(this, "testResult", JSON.stringify(resp));
            }).catch(e => {
                this.$set(this, "testResult", JSON.stringify(e));
            }).finally(() => {
                this.jshelper.closeLoading();
            });
        }
    },
}
</script>

<style scoped>
.main-container {
    width: 100%;
    min-height: 100%;
    background-color: white;
}

.credit-title {
    margin-top: 15px;
    text-align: center;
}

.el-row-margin {
    margin-top: 15px;
}

.el-row-testalign {
    text-align: left;
}
</style>