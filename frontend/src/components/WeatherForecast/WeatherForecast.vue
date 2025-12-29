<template>
    <div class="weather-forecast-box" style="height: 100%;position: relative;">
        <el-carousel direction="vertical" autoplay :interval="30000" indicator-position="none">
            <el-carousel-item v-for="(item, index) in menu" :key="index">
                <el-image :src="item.url"></el-image>
            </el-carousel-item>
        </el-carousel>
        <!-- 右侧栏 -->
        <div class="weather-content">
            <!-- Header -->
            <el-row v-if="!weatherLoading" style="margin-top: 15px;">
                <el-col :span="24">
                    <span class="weather-fc-header">未来5天的天气预报</span>
                </el-col>
            </el-row>
            <!-- 未来五天天气预报 -->
            <el-row v-if="!weatherLoading" style="margin-top: 15px;line-height: 17px;"
                v-for="(item, index) in WeatherForecasts" :key="index"
                :title="item.text_day + ' ' + item.low + '° ~' + item.high + '°'">
                <el-col :span="6">
                    <span class="weather-fc-font">{{ rtcrm.formatDate(new Date(item.date), "MM-dd") }}</span>
                </el-col>
                <el-col :span="6">
                    <span class="weather-fc-font">{{ item.week }}</span>
                </el-col>
                <el-col :span="6">
                    <span class="weather-fc-font">{{ item.text_day }}</span>
                </el-col>
                <el-col :span="6">
                    <span class="weather-fc-font">{{ item.low }}° ~ {{ item.high }}°</span>
                </el-col>
            </el-row>
            <!-- 平均气温 -->
            <el-divider v-if="!weatherLoading" content-position="right">平均气温
                <span>{{ WeatherForecastsLowAvg }}°</span> ~
                <span>{{ WeatherForecastsHighAvg }}°</span>
            </el-divider>
            <!-- Chart -->
            <el-row style="margin-top: 15px;">
                <el-col :span="24">
                    <div class="weather-echart-box" ref="weacher-forecast-chart"></div>
                </el-col>
            </el-row>
        </div>
        <!-- Header栏 -->
        <div class="weather-content2" :title="WeatherTemperature + ' ' + WeatherText + ' ' + CityName">
            <div v-if="!weatherLoading" class="weather-temperature">{{ WeatherTemperature }}</div>
            <div v-if="!weatherLoading" class="weather-content2-content">
                <div class="weather-date">
                    <span style="margin-right: 10px;">{{ WeatherText }}</span>
                    <span style="font-size: 25px;">{{ WeatherWind }}</span>
                </div>
                <div class="weather-city" style="position: relative;;">
                    <div class="weather-city-label" @click="cityChange">{{ CityName }}
                    </div>
                    <el-cascader class="city-cascader" ref="city-cascader" size="mini" v-show="true"
                        v-model="input.cityMulySelect" filterable :options="TownShipData" :show-all-levels="false"
                        @change="cityChangeHandle"></el-cascader>
                    <div class="weather-date-label">{{ NowDate }}</div>
                </div>
            </div>
        </div>
        <!-- 日历 -->
        <div class="calendar-box">
            <date-picker ref="datePicker" showLunar showAnniversary showPopover showHoliday
                backgroundColor="rgba(242, 242, 242, 0.75)"></date-picker>
        </div>
    </div>
</template>

<script>
import datePicker from '../Common/Calendar/SimpleCalendar.vue'

export default {
    name: 'WeatherForecast',
    components: {
        datePicker
    },
    data() {
        return {
            msg: "hello world",
            publicKey: 1, //刷新Key
            menu: [
                { id: "morning_0001", url: require("@/assets/img/landscaping/morning_0001.jpg") },
                { id: "evening_0001", url: require("@/assets/img/landscaping/evening_0001.jpg") },
                { id: "evening_0002", url: require("@/assets/img/landscaping/evening_0002.jpg") },
                { id: "nightfall_0001", url: require("@/assets/img/landscaping/nightfall_0001.jpg") },
                { id: "nightfall_0002", url: require("@/assets/img/landscaping/nightfall_0002.jpg") },
            ],
            input: {
                city: "",
                cityMulySelect: [],
            },
            weatherLoading: false,
            weatherData: {},
            weatherChart: null,
            BaiduMapTownShipData: [],
            HistoryTownShipData: {
                label: "历史与热门",
                value: "9999999",
                children: [{
                    label: "广州市",
                    value: "440100",
                }, {
                    label: "深圳市",
                    value: "440300",
                }]
            },
        };
    },
    created() {
        this.getBaiduMapTownShip(true);

        let _this = this;
        setInterval(() => {
            _this.getData()
        }, 3600000);//每小时刷新一次天气数据
    },
    mounted() {
        const weatherChartBox = this.$refs["weacher-forecast-chart"];
        this.weatherChart = this.$echarts.init(weatherChartBox)
    },
    computed: {
        CityName: function () {
            let text = "";
            if (this.rtcrm.isNull(this.weatherData) || this.rtcrm.isNull(this.weatherData.location)) return text;

            text = this.weatherData.location.city;
            return text;
        },
        NowDate: function () {
            return this.jshelper.getWeekday(new Date());
        },
        WeatherText: function () {
            let text = "";
            if (this.rtcrm.isNull(this.weatherData) || this.rtcrm.isNull(this.weatherData.location)) return text;

            text = this.weatherData.now.text;
            return text;
        },
        WeatherTemperature: function () {
            let text = "";
            if (this.rtcrm.isNull(this.weatherData) || this.rtcrm.isNull(this.weatherData.location)) return text;

            text = this.weatherData.now.temp + "°";
            return text;
        },
        WeatherWind: function () {
            let text = "";
            if (this.rtcrm.isNull(this.weatherData) || this.rtcrm.isNull(this.weatherData.location)) return text;

            text = this.weatherData.now.wind_dir + " " + this.weatherData.now.wind_class;
            return text;
        },
        WeatherForecasts: function () {
            let arr = [];
            if (this.rtcrm.isNull(this.weatherData) || this.rtcrm.isNull(this.weatherData.forecasts)) return arr;

            arr = this.weatherData.forecasts;
            return arr;
        },
        WeatherForecastsLowAvg: function () {
            var sum = 0;
            for (let item of this.WeatherForecasts) {
                sum += parseInt(item.low);
            }
            return parseInt(sum / this.WeatherForecasts.length);
        },
        WeatherForecastsHighAvg: function () {
            var sum = 0;
            for (let item of this.WeatherForecasts) {
                sum += parseInt(item.high);
            }
            return parseInt(sum / this.WeatherForecasts.length);
        },
        TownShipData: function () {
            let arr = [];
            arr.push(this.HistoryTownShipData);
            if (!this.rtcrm.isNull(this.BaiduMapTownShipData) && this.BaiduMapTownShipData.length > 0) {
                arr = arr.concat(this.BaiduMapTownShipData)
            }
            return arr;
        },
    },
    methods: {
        //查询
        getData: function () {
            let _this = this;
            this.$set(this, "weatherLoading", true);
            this.$set(this, "weatherLoading", {});

            this.jshelper
                .ApiGet("WeatherForecast/GetWeatherForecast?citycode=" + this.input.city)
                .then((res) => {
                    if (_this.rtcrm.isNull(res)) {
                        this.jshelper.openAlertDialog(this, "返回数据为空", "查询天气数据失败");
                        return;
                    }
                    if (!_this.rtcrm.isNull(res.data) && res.isSuccess) {
                        let data = res.data.result;
                        if (!_this.rtcrm.isNull(data)) {
                            _this.$set(_this, "weatherData", data);

                            //记录当前城市
                            if (_this.rtcrm.isNullOrWhiteSpace(_this.input.city)) {
                                _this.$set(_this.input, "cityMulySelect", ["9999999", _this.weatherData.location.id]);
                            }

                            _this.$set(_this, "weatherLoading", false);
                            _this.$set(_this.input, "city", _this.weatherData.location.id);
                            _this.weatherChartInit();

                            //把查询的城市插入到历史记录中方便用户切换热门城市
                            let mult = false;
                            _this.HistoryTownShipData.children.forEach((item) => {
                                if (item.value === _this.weatherData.location.id) {
                                    mult = true;
                                    return;
                                }
                            });
                            if (!mult) _this.HistoryTownShipData.children.push({
                                label: _this.weatherData.location.city,
                                value: _this.weatherData.location.id,
                            });
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "查询天气数据失败");
                    }
                    _this.$set(_this, "publicKey", _this.publicKey + 1); //刷新
                    // console.log(res);
                })
                .catch((err) => {
                    _this.jshelper.openAlertDialog(_this, err.message, "查询天气数据失败");
                });
        },

        //天气预报图表初始化
        weatherChartInit: function () {
            if (this.rtcrm.isNull(this.weatherChart)) return;

            let x_data = [];
            this.WeatherForecasts.forEach((item) => {
                x_data.push(item.week);
            });

            let low_data = [];
            this.WeatherForecasts.forEach((item) => {
                low_data.push(item.low);
            });

            let high_data = [];
            this.WeatherForecasts.forEach((item) => {
                high_data.push(item.high);
            });

            let option = {
                backgroundColor: 'rgba(128, 128, 128, 0.35)',
                title: {
                    text: '气温趋势',
                    subtext: '',
                    x: 'center',
                    textStyle: {
                        color: "white"
                    },
                },
                xAxis: {
                    data: x_data,
                    axisLine: {
                        show: false, //隐藏轴
                    },
                    axisTick: {
                        show: false, //隐藏刻度线
                    },
                    axisLabel: {
                        show: false, //隐藏刻度值
                    },
                },
                yAxis: {
                    axisLine: {
                        lineStyle: {
                            color: '#EDF2F4',//设置轴颜色
                        },
                    }
                },
                // 提示框
                tooltip: {
                    trigger: 'axis',
                    renderMode: "html",
                    formatter: function (params) {
                        let tooltipText = `<strong>${params[0].axisValue}</strong><br/>`;
                        tooltipText += `<span style='color:#409EFF'>High&nbsp;${params[0].data}</span><br/>`;
                        tooltipText += `<span style='color:#67C23A'>Low&nbsp;${params[1].data}</span><br/>`;
                        return tooltipText;
                    },
                },
                grid: {
                    x: 0, //距离左边
                    x2: 0, //距离右边
                    y: 0, //距离上边
                    y2: 0, //距离下边
                    left: 5,
                    right: 5,
                    top: 5,
                    bottom: 5,
                    containLabel: true
                },
                series: [
                    {
                        data: high_data,
                        type: 'line',
                        stack: 'high',
                        itemStyle: {
                            normal: {
                                label: {
                                    show: false,// 拐点上显示数值
                                },
                                borderColor: 'black',  // 拐点边框颜色
                                lineStyle: {
                                    width: 2,  // 设置线宽
                                    type: 'solid'  //'dotted'虚线 'solid'实线
                                },
                                color: '#409EFF'
                            }
                        }
                    },
                    {
                        data: low_data,
                        type: 'line',
                        stack: 'low',
                        itemStyle: {
                            normal: {
                                color: '#67C23A'
                            }
                        }
                    }
                ]
            };
            this.weatherChart.setOption(option);
        },

        //获取百度地图行政区域数据（省份、城市编码）
        getBaiduMapTownShip: function (isRetrieveData) {
            let _this = this;
            this.$set(this, "weatherLoading", true);
            this.$set(this, "BaiduMapTownShipData", []);

            this.jshelper
                .ApiGet("WeatherForecast/GetBaiduMapTownShip")
                .then((res) => {
                    if (_this.rtcrm.isNull(res)) {
                        this.jshelper.openAlertDialog(this, "返回数据为空", "获取百度地图行政区域数据失败");
                        return;
                    }
                    if (!_this.rtcrm.isNull(res.data) && res.isSuccess) {
                        let data = res.data;
                        if (!_this.rtcrm.isNull(data)) {
                            _this.$set(_this, "BaiduMapTownShipData", data);
                            _this.$set(_this, "weatherLoading", false);
                            _this.getData();//查询当前城市的天气数据
                        }
                        else {
                            this.jshelper.openAlertDialog(this, res.message, "获取百度地图行政区域数据失败");
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "获取百度地图行政区域数据失败");
                    }
                })
                .catch((err) => {
                    _this.jshelper.openAlertDialog(_this, err.message, "获取百度地图行政区域数据失败");
                });
        },

        //城市点击切换事件
        cityChange: function () {
            this.$nextTick(() => {
                const cascader = this.$refs["city-cascader"];
                if (cascader) {
                    cascader.dropDownVisible = true;
                }
            });
        },

        //城市切换事件
        cityChangeHandle: function (value) {
            if (!this.rtcrm.isNull(value) && value.length > 1) {
                this.$set(this.input, "city", value[1]);
                this.getData();
            }
        },
    },
};
</script>

<style scoped>
.el-image {
    width: 100%;
    height: 100%;
}

/* 使用 /deep/ 或 ::v-deep 来穿透 scoped 样式 */
.weather-forecast-box /deep/ .el-carousel {
    width: 100%;
    height: 100%;
}

.weather-forecast-box /deep/ .el-carousel__container {
    width: 100%;
    height: 100%;
}

.weather-forecast-box /deep/ .el-carousel__item {
    height: 100%;
}

.weather-content::before {
    background-color: rgba(242, 242, 242, 0.2);
    background-size: cover;
    width: 100%;
    height: 100%;
    content: "";
    z-index: -1;
    position: absolute;
    top: 0;
    left: 0;
    border-radius: 25px;
}

.weather-content {
    height: 80%;
    width: 400px;
    position: absolute;
    top: 10%;
    right: 85px;
    z-index: 10;
    padding: 0px 15px 15px 15px;
    user-select: none;
}

.weather-fc-font {
    color: #EDF2F4;
    font-size: 20px;
}

.weather-fc-header {
    color: #EDF2F4;
    font-size: 30px;
    font-weight: bolder;
}

.weather-content2 {
    height: 200px;
    line-height: 200px;
    width: 600px;
    position: absolute;
    top: 0;
    left: 0;
    z-index: 10;
    padding: 0 15px 0 35px;
    user-select: none;
}

.weather-temperature {
    display: inline-block;
    color: white;
    font-size: 100px;
    font-weight: bolder;
    margin-right: 25px;
    line-height: 100px;
}

.weather-situation {
    display: inline-block;
    font-size: 30px;
    margin-right: 25px;
    line-height: 30px;
    color: white;
}

.weather-content2-content {
    display: inline-block;
    color: white;
}

.weather-city {
    font-size: 20px;
    height: 30px;
    line-height: 30px;
}

.weather-city-label {
    display: inline;
    margin-right: 10px;
    cursor: pointer;
    z-index: 10;
}

.weather-date-label {
    display: inline;
    z-index: 10;
}

.weather-date {
    font-size: 30px;
    height: 30px;
    line-height: 30px;
}

.el-divider__text {
    background-color: transparent;
    color: white;
    font-weight: bolder;
    font-size: 15px;
}

.weather-echart-box {
    height: 280px;
    overflow: hidden;
    border-radius: 15px;
}

.city-cascader {
    position: absolute;
    top: 0;
    left: 0;
    width: 160px;
}

.city-cascader>>>.el-input {
    z-index: -1;
}

.city-cascader>>>.el-input__inner {
    background-color: transparent;
    border: none;
    visibility: hidden;
}

.city-cascader>>>.el-input__suffix {
    visibility: hidden;
}

.calendar-box {
    position: absolute;
    width: 650px;
    height: 500px;
    top: 200px;
    left: 35px;
}

.calendar-box>>>.window-calendar {
    z-index: 2000;
}

.calendar-box>>>.el-badge {
    display: inline;

    .is-dot {
        right: -3px;
    }
}

.calendar-date-text {
    display: inline;
    height: 50%;
}
</style>