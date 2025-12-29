<template>
    <div class="self-introduction-box">
        <div class="self-myinfo-box">
            <div class="self-avatar-box">
                <el-image class="self-avatar" fit="fill" :src="avatars[0].url"></el-image>
            </div>
            <div>
                <h2>{{ title }}</h2>
            </div>
            <transition name="el-fade-in-linear">
                <div v-show="showAphorism" class="self-aphorism-box">
                    <blockquote>
                        {{ aphorisms[aphorismIndex].description }}
                    </blockquote>
                    <cite class="self-aphorism-author">
                        — {{ aphorisms[aphorismIndex].author }}
                    </cite>
                </div>
            </transition>
        </div>
    </div>
</template>

<script>
import BaseData from './BaseData.json';
export default {
    name: "SelfIntroduction",
    data() {
        return {
            avatars: [
                { id: "avatar_0001", url: require("@/assets/img/avatar/cat1.png") },
            ],
            aphorisms: [],
            aphorismIndex: 19,
            showAphorism: true,
        }
    },
    props: {
        title: {
            type: String,
            default: "标题"
        }
    },
    created() {
        this.BaseDataInit();
        this.$set(this, 'aphorismIndex', this.getRandomNumber());
        setInterval(() => {
            this.aphorismsSwitch();
        }, 15000);
    },
    mounted() {
    },
    computed: {
    },
    methods: {
        //配置文件初始化
        BaseDataInit: function () {
            console.log("BaseData", BaseData);
            if (!this.rtcrm.isNull(BaseData)) {
                if (!this.rtcrm.isNull(BaseData.aphorisms) && BaseData.aphorisms.length > 0) {
                    this.$set(this, "aphorisms", BaseData.aphorisms);
                }
            }
        },
        aphorismsSwitch() {
            this.$set(this, 'showAphorism', false);
            setTimeout(() => {
                let index = this.aphorismIndex + 1;
                index = index > this.aphorisms.length - 1 ? 0 : index;
                this.$set(this, 'aphorismIndex', index);
                this.$set(this, 'showAphorism', true);
            }, 300);
        },
        //生成一个随机数
        getRandomNumber() {
            return Math.floor(Math.random() * (this.aphorisms.length - 1));
        },
    }
}
</script>

<style scoped>
.self-introduction-box {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 3000;
    background-color: #FEEBB1;
}

.self-myinfo-box {
    position: relative;
    top: calc(50% - 150px);
    left: calc(50% - 250px);
    width: 500px;
    height: 300px;
    background-color: white;
    border-radius: 15px;
    box-shadow: 2px 2px #E6A23C;
    text-align: center;
    padding: 20px 0;
}

.self-avatar-box {
    position: relative;
    user-select: none;

    .el-image__error {
        border-radius: 50%;
    }
}

.self-avatar {
    width: 100px;
    height: 100px;
}

@keyframes spin {
    from {
        transform: rotate(0deg);
    }

    to {
        transform: rotate(360deg);
    }
}

.rotate-element {
    animation: spin 3s linear infinite;
}

.self-aphorism-author {
    margin-left: 200px;
}
</style>