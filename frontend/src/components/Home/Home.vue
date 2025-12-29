<template>
    <div :style="bgColorStyle" class="fullscreen-div">
        <transition name="el-fade-in" mode="out-in">
            <SelfIntroduction v-show="showSelf" title="七之小站"></SelfIntroduction>
        </transition>
        <el-container v-if="showMain">
            <!-- 侧边栏 -->
            <el-aside :style="[asideWide]">
                <div class="logo-box" @click="isCollapse = !isCollapse">My Web</div>
                <el-menu class="el-menu-vertical-demo" text-color="#606266" :unique-opened="false"
                    :collapse="isCollapse" :collapse-transition="false" default-active="0-1">
                    <!-- 环境选择器 -->
                    <el-select v-show="!isCollapse" class="el-select-custom" v-model="selectEnvironment" filterable
                        size="mini" placeholder="请选择CRM环境" @change="environmentSelectChange">
                        <el-option v-for="item in environments" :key="item.value" :label="item.label"
                            :value="item.value">
                        </el-option>
                    </el-select>
                    <!-- 菜单目录 -->
                    <el-submenu v-for="(main) in menuGroup" :index="main.index" :key="main.index" :title="main.name">
                        <template slot="title">
                            <i :class="main.icon"></i>
                            <span slot="title">{{ main.name }}</span>
                        </template>
                        <el-menu-item v-for="(secondMenu) in main.items" :index="secondMenu.index"
                            :key="secondMenu.index" @click="navigateTo(secondMenu)" :title="secondMenu.name">
                            {{ secondMenu.name }}
                        </el-menu-item>
                    </el-submenu>
                </el-menu>
            </el-aside>
            <!-- 主要显示区 -->
            <el-main>
                <div class="main-content scrollable-area">
                    <div v-if="currentComponent" ref="mainComponent" :is="currentComponent"></div>
                </div>
            </el-main>
        </el-container>
    </div>
</template>

<script>
import BaseData from './BaseData.json';
export default {
    name: 'Home',
    components: {
        SelfIntroduction: () => import('../Common/SelfIntroduction/SelfIntroduction.vue')
    },
    data() {
        return {
            rootPath: "/qjrweb",
            isCollapse: false,//侧边栏是否收起
            componentsMapPath: {},
            componentsMapVuePath: {},
            currentComponent: null,
            defaultPage: "",
            componentMap: {},
            menuGroup: [],
            showSelf: true,
            showMain: false,
            environments: [],
            selectEnvironment: "",
        }
    },
    created() {
        this.BaseDataInit();
        this.componentMapInit().then(() => {
            this.getCurrentComponent();
        });
    },
    mounted() {
        let _this = this;
        setTimeout(() => {
            _this.GetCRMEnvironments();
        }, 300);
    },
    computed: {
        asideWide() {
            return {
                maxWidth: "240px",
                width: this.isCollapse ? "auto" : "240px",
            };
        },
        bgColorStyle() {
            return {
                '--color-light_apricot': "#FAF3E3",//浅杏色
                '--color-morning_yellow': "#F7EEDD",//晨曦黄
                '--color-light_beige': "#F5E6D3",//浅米色
                '--color-light_coral_beige': "#F7D9C4",//浅珊瑚米色
            }
        }
    },
    methods: {
        //配置文件初始化
        BaseDataInit: function () {
            console.log("BaseData", BaseData);
            if (!this.rtcrm.isNull(BaseData)) {
                if (!this.rtcrm.isNull(BaseData.componentsMapPath)) {
                    this.$set(this, "componentsMapPath", BaseData.componentsMapPath);
                }
                if (!this.rtcrm.isNull(BaseData.componentsMapVuePath)) {
                    this.$set(this, "componentsMapVuePath", BaseData.componentsMapVuePath);
                }
                if (!this.rtcrm.isNull(BaseData.menuGroup) && BaseData.menuGroup.length > 0) {
                    this.$set(this, "menuGroup", BaseData.menuGroup);
                }
                if (!this.rtcrm.isNullOrWhiteSpace(BaseData.defaultPage)) {
                    this.$set(this, "defaultPage", BaseData.defaultPage);
                }
            }
        },

        //侧边栏导航
        navigateTo: function (menu) {
            let route = this.rootPath + "/" + this.componentsMapPath[menu.componentName];
            this.$router.push({ path: route }, () => {
                this.getCurrentComponent();
            });
            console.log("menu", menu);
        },

        //获取CRM环境列表
        GetCRMEnvironments: function () {
            let _this = this;
            this.jshelper.ApiGet("Default/GetCRMEnvironments").then((resp) => {
                if (resp && resp.isSuccess) {
                    this.$set(_this, "environments", resp.data);
                    if (resp.data != null && resp.data.length > 0) {
                        this.$set(this, "selectEnvironment", resp.data[0].value);
                        this.environmentSelectChange(this.selectEnvironment);
                    }
                }

                this.$set(this, "showMain", true);
                setTimeout(() => {
                    this.$set(this, "showSelf", false);
                }, 3000);
            }).catch((err) => {
                this.jshelper.openAlertDialog(this, err.message, "获取CRM环境列表");
            }).finally(() => {
                this.jshelper.closeLoading();
            });
        },

        //环境选择器OnChange
        environmentSelectChange: function (value) {
            let tips = `当前CRM环境为：${value}`;
            console.log(tips);
            this.showMessage(tips, "success");

            if (!this.$globalVar) this.$globalVar = {};

            this.$globalVar["selectEnv"] = value;

            //通知子组件环境切换
            if (this.$refs.mainComponent)
                this.$refs.mainComponent.$emit("environment-change", value);
        },

        //showMessage
        showMessage(message, type, dangerouslyUseHTMLString = false) {
            this.$message({
                message: message,
                dangerouslyUseHTMLString: dangerouslyUseHTMLString,
                type: type,
            });
            if (type === "error") {
                console.log(message);
            }
        },

        //componentMap初始化
        componentMapInit: async function () {
            console.log(this.$options.components);
            let obj = {};
            for (let name in this.componentsMapPath) {
                let path = this.componentsMapPath[name];
                let vuePath = this.componentsMapVuePath[name];
                await import(`../${vuePath}`).then(module => {
                    obj[path] = module.default || module;
                }).catch(error => {
                    this.showMessage(`Error loading component(${name}):${error}`, "error");
                });
            }
            this.$set(this, "componentMap", obj);
            console.log("componentMap", this.componentMap);
        },

        //获取跳转组件
        getCurrentComponent() {
            // 根据路由的path返回对应的组件名称
            let regex = new RegExp(this.rootPath + '/(.*)');

            let componentResult = this.$route.path.match(regex);
            if (componentResult) {
                // 获取当前路由
                const currentPath = componentResult[1];
                console.log("currentPath", currentPath);
                // 返回与当前路由匹配的组件名称，如果没有匹配，返回null
                if (this.componentMap[currentPath]) {
                    this.$set(this, "currentComponent", this.componentMap[currentPath]);
                }
                else if (this.componentMap[this.componentsMapPath[this.defaultPage]]) {
                    this.$set(this, "currentComponent", this.componentMap[this.componentsMapPath[this.defaultPage]]);
                }
            }
            else {
                this.$set(this, "currentComponent", this.componentMap[this.componentsMapPath[this.defaultPage]]);
            }
            console.log("currentComponent", this.currentComponent);
        },
    },
}
</script>

<style scoped>
.el-main {
    padding: 0 !important;
}

.el-aside {
    text-align: left;
    margin-right: 1px;
    background-color: var(--color-light_beige) !important;
}

.logo-box {
    height: 20px;
    line-height: 20px;
    text-align: center;
    padding: 7px 0;
    cursor: pointer;
    color: #409EFF;
    font-weight: bolder;
    user-select: none
}

.el-submenu__title {
    white-space: nowrap;
    /* 确保文本在一行内显示 */
    overflow: hidden;
    /* 隐藏超出容器的文本 */
    text-overflow: ellipsis;
    /* 使用省略号表示被截断的文本 */
}

.el-menu-vertical-demo {
    margin-left: 5px;
    height: calc(100% - 34px);
    background-color: var(--color-light_beige) !important;
}

.el-menu-vertical-demo>>>.el-menu-item:hover {
    background-color: #EBEEF5 !important;
    color: #67C23A !important;
}

.el-menu-vertical-demo>>>.el-submenu__title:hover {
    background-color: #EBEEF5 !important;
    color: #67C23A !important;
}

.el-menu-vertical-demo>>>.el-menu-item {
    font-size: 14px;
    background-color: var(--color-light_beige) !important;
}

.el-menu-vertical-demo>>>.el-submenu__title {
    font-size: 14px;
    background-color: var(--color-light_beige) !important;
}

.el-menu-item {
    white-space: nowrap;
    /* 确保文本在一行内显示 */
    overflow: hidden;
    /* 隐藏超出容器的文本 */
    text-overflow: ellipsis;
    /* 使用省略号表示被截断的文本 */
}

.el-container {
    height: 100%;
}

.fullscreen-div {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
}

.main-content {
    height: calc(100% - 20px);
    /* 计算元素的高度为父元素高度减去内边距 */
    background-color: var(--color-light_coral_beige);
    padding: 10px;
}

.scrollable-area {
    /* 设置溢出行为，使滚动条出现 */
    overflow-y: auto;

    /* 针对Webkit内核的浏览器 */
    &::-webkit-scrollbar {
        /* 设置滚动条的宽度 */
        width: 8px;
    }

    /* 滚动条轨道 - 背景颜色/白底 */
    &::-webkit-scrollbar-track {
        background: #fff;
        border-radius: 10px;
    }

    /* 滚动条的滑块部分 */
    &::-webkit-scrollbar-thumb {
        background: rgba(144, 147, 153, .2);
        border-radius: 10px;
    }

    /* 当鼠标悬停在滚动条滑块上时改变颜色 */
    &::-webkit-scrollbar-thumb:hover {
        background: rgba(144, 147, 153, .4);
    }
}

.el-select {
    display: block;
}

.el-select-custom {
    margin: 3px 2px 5px 2px;
}

.el-select-custom /deep/ .el-input__inner {
    background-color: var(--color-light_beige);
    font-weight: bolder;
    color: #409EFF;
    -webkit-text-fill-color: #409EFF;
}
</style>