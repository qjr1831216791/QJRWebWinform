<template>
    <div v-if="showPicker" ref="dateBoxClass" :class="dateBoxClass">
        <div class="window-background" :style="[backgroundColorClass]"></div>
        <div class="window-wrapper">
            <div class="today-date-text">
                <div v-show="showCancelBtn" class="text-button" @click="cancle">取消</div>
                <i class="el-icon-arrow-left" @click="toLastMonth"></i>
                <div class="today-date">{{ currentDateText }}</div>
                <i class="el-icon-arrow-right" @click="toNextMonth"></i>
                <div v-show="showConfirmBtn" class="text-button" @click="confirm">确定</div>
            </div>
            <div v-show="showWeekDate" class="week-wrapper">
                <div v-for="week in weekItemArr" class="week-item" :key="'week_' + week">
                    <slot name="week-cell" :data="week"><span>{{ week }}</span></slot>
                </div>
            </div>
            <div class="month-day-wrapper">
                <div @click="handleDay(day)" v-for="day in days" :key="day.dateText" :class="dayClass(day)">
                    <div class="date-text" :style="[dayCellHeight]">
                        <div v-show="showAnniversary && day.hasAnniversarys" class="red-dot"></div>
                        <el-popover :disabled="!showPopover || !day.hasAnniversarys" placement="top-start"
                            :title="day.dateString" width="200" trigger="click">
                            <div>
                                <div v-for="(anni, anniIndex) in day.anniversarys" :key="'anni_' + anniIndex"
                                    class="text item">
                                    <el-tag size="medium" type="success">{{ anni.label }}</el-tag>
                                    <span>{{ anni.description }}</span>
                                </div>
                            </div>
                            <div slot="reference">
                                <slot name="date-cell" :data="day">
                                    <span>{{ day.isToday ? '今天' : day.dateText }}</span>
                                </slot>
                            </div>
                        </el-popover>
                    </div>
                    <slot name="lunar-cell" :data="day">
                        <div v-show="showLunar || showHoliday" class="chinese-date-text">
                            <span>{{ underText(day) }}</span>
                        </div>
                    </slot>
                </div>
                <div class="day-item" style="cursor: default;" v-for="item in (7 - days.length % 7)"
                    :key="'blank_' + item"></div>
            </div>
        </div>
    </div>
</template>

<script>
import { toLunar } from '../../../../public/js/ToLunar.js'

export default {
    name: "SimpleCalendar",
    data() {
        return {
            days: [], // 当前月的所有日期的数据集合
            currentDateText: '', // 头部标题年月
            currentDate: '', // 初始化时为当前日期
            showPicker: true,
            boxHeight: 275,
            holidays: [], // 节假日数据
            anniversarys: [],//纪念日数据
        }
    },
    props: {
        showCancelBtn: {
            type: Boolean,
            default: false
        },
        showConfirmBtn: {
            type: Boolean,
            default: false
        },
        showWeekDate: {
            type: Boolean,
            default: true
        },
        showPopover: {
            type: Boolean,
            default: false
        },
        showAnniversary: {
            type: Boolean,
            default: false
        },
        type: {
            type: String,
            default: 'calendar',//日历表现形式，有datePicker、calendar
        },
        showLunar: {
            type: Boolean,
            default: false
        },
        showHoliday: {
            type: Boolean,
            default: false
        },
        backgroundColor: {
            type: String,
        },
        weekItemArr: {
            type: Array,
            default: () => ['周日', '周一', '周二', '周三', '周四', '周五', '周六']
        },
    },
    created() {
        this.initPicker();
    },
    mounted() {
        this.$nextTick(() => {
            let offsetHeight = this.$refs["dateBoxClass"].offsetHeight
            this.$set(this, "boxHeight", offsetHeight);
        });
    },
    computed: {
        dateBoxClass() {
            if (this.type === 'datePicker') return "window-backdrop";
            else if (this.type === 'calendar') return "window-calendar";
            return "window-backdrop"; // 默认返回值
        },
        backgroundColorClass() {
            return {
                backgroundColor: this.backgroundColor == undefined ? "white" : this.backgroundColor,
            };
        },
        dayCellHeight() {
            let height = (this.boxHeight - 20 - 50 - (this.showWeekDate ? 44 : 0) - 6 * 5 - (this.showLunar || this.showHoliday ? 20 : 0) * 5) / 5;
            return {
                height: height + "px",
                lineHeight: height + "px",
            }
        },
    },
    methods: {
        open() {
            this.showPicker = true
            this.initPicker()
        },
        confirm() {
            this.$emit('confirm', this.getSelectedDay())
        },
        cancle() {
            this.showPicker = false
        },
        getSelectedDay() {
            for (var i = 0; i < this.days.length; i++) {
                if (this.days[i].selected) return this.days[i]
            }
        },
        /**
         * 日历点击事件,更新点击选中状态
         * @param {Object} day
         */
        handleDay(day) {
            if (!day.date) return
            for (var i = 0; i < this.days.length; i++) {
                this.days[i].selected = day.dateText === this.days[i].dateText
            }
            this.updateTitle(day)
        },
        /**
         * 更新标题年月
         * @param {Object} day
         */
        updateTitle(day) {
            this.currentDateText = this.getDateText(day.date)
        },
        /**
         * 下个月
         */
        toLastMonth() {
            this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() - 1, 1)
            this.days = this.getPickerData(this.currentDate)
            this.currentDateText = this.getDateText(this.currentDate)
            this.getHolidays(this.currentDate);
            this.getAnniversarys(this.currentDate);
        },
        /**
         * 上个月
         */
        toNextMonth() {
            this.currentDate = new Date(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, 1)
            this.days = this.getPickerData(this.currentDate)
            this.currentDateText = this.getDateText(this.currentDate)
            this.getHolidays(this.currentDate);
            this.getAnniversarys(this.currentDate);
        },
        /**
         * 设置日历标题
         * @param {Object} day
         */
        getDateText(day) {
            return day.getFullYear() + '年' + (day.getMonth() + 1) + '月'
        },
        /**
         * 转换成周几
         * @param {Object} day
         */
        getWeekText(day) {
            let weekVal = day.getDay()
            let weekText = ''
            switch (weekVal) {
                case 0:
                    weekText = '日'
                    break
                case 1:
                    weekText = '一'
                    break
                case 2:
                    weekText = '二'
                    break
                case 3:
                    weekText = '三'
                    break
                case 4:
                    weekText = '四'
                    break
                case 5:
                    weekText = '五'
                    break
                case 6:
                    weekText = '六'
                    break

            }
            return '周' + weekText
        },
        /**
         * 计算某个月的天数
         * @param {Object} month
         */
        getMonthDaysLength(day, month) {
            day.setMonth(month)
            day.setDate(0)
            return day.getDate()
        },
        /**
         * 根据日期对象计算当前月份1日为星期几,
         * 从而计算日期数组开始需要添加几个空数据
         * @param {Object} day
         */
        getEmptyDays(day) {
            return new Date(day.setDate(1)).getDay()
        },
        /**
         * 根据传入的day对象获取当前月的所有日期数据
         * @param {Object} day
         */
        getPickerData(day) {
            let arr = []
            let emptyLength = this.getEmptyDays(day)
            for (var i = 0; i < emptyLength; i++) {
                arr.push({}) // 根据当前月份1号是星期几向往数组里添加空数据
            }
            let month = day.getMonth() + 1
            let daysLength = this.getMonthDaysLength(day, month) // 计算当前月份一共有多少天
            for (var i = 0; i < daysLength; i++) {
                let obj = {
                    date: new Date(day.setDate(i + 1)), // Date类型的日期数据
                    dateText: i + 1, // 用于显示的日期
                    chineseDateText: toLunar(day.getFullYear(), month, i + 1).day, // 农历日期
                    isToday: new Date().toDateString() === new Date(day.setDate(i + 1)).toDateString(), // 是否是今天
                    selected: new Date().toDateString() === new Date(day.setDate(i + 1))
                        .toDateString(), // 设置今天为选中状态
                    dateString: this.rtcrm.formatDate(new Date(day.setDate(i + 1)), "yyyy-MM-dd"), // 日期字符串
                    hasAnniversarys: false,//是否纪念日，默认为否
                }
                if (!this.rtcrm.isNull(this.holidays) && this.holidays.length > 0) {
                    for (let Holiday of this.holidays) {
                        if (obj.date.getFullYear() === Holiday.year && obj.date.getMonth() + 1 === Holiday.month &&
                            obj.date.getDate() === Holiday.day) {
                            obj["holidayName"] = Holiday.holiday_CN_Name;
                            obj["holidayStatus"] = Holiday.status;
                            break;
                        }
                    }
                }
                if (!this.rtcrm.isNull(this.anniversarys)) {
                    let flag = false;
                    for (let date in this.anniversarys) {
                        let item = this.anniversarys[date];
                        let dt = new Date(date);
                        if (obj.date.getFullYear() === dt.getFullYear() && obj.date.getMonth() === dt.getMonth() &&
                            obj.date.getDate() === dt.getDate()) {
                            obj["anniversarys"] = item;
                            flag = true;
                            break;
                        }
                    }
                    obj["hasAnniversarys"] = flag;
                }
                arr.push(obj)
            }
            return arr
        },
        initPicker() {
            this.currentDate = new Date()
            this.currentDateText = this.getDateText(this.currentDate) // 初始化标题年月
            this.days = this.getPickerData(this.currentDate) // 默认当前日期初始化日历
            this.getHolidays(this.currentDate); // 获取节假日数据
            this.getAnniversarys(this.currentDate); // 获取纪念日数据
        },
        /**
         * 根据传入的day获取当月的节假日数据
         * @param {Object} day
         */
        getHolidays(currentDate) {
            if (!this.showHoliday) return;
            let _this = this;
            let currentDateStr = this.rtcrm.formatDate(currentDate, "yyyy-MM-dd");

            this.jshelper
                .ApiGet("Holiday/GetHolidayList?date=" + currentDateStr)
                .then((res) => {
                    if (_this.rtcrm.isNull(res)) {
                        this.jshelper.openAlertDialog(this, "返回数据为空", "查询节假日数据失败");
                        return;
                    }
                    if (res.isSuccess) {
                        if (!_this.rtcrm.isNull(res.data)) {
                            let data = res.data;
                            if (!_this.rtcrm.isNull(data)) {
                                _this.$set(_this, "holidays", data);
                                _this.days = _this.getPickerData(_this.currentDate) // 默认当前日期初始化日历
                            }
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "查询节假日数据失败");
                    }
                })
                .catch((err) => {
                    _this.jshelper.openAlertDialog(_this, err.message, "查询节假日数据失败");
                });
        },

        /**
         * 根据传入的day获取当月的纪念日数据
         * @param {Object} day
         */
        getAnniversarys(currentDate) {
            if (!this.showAnniversary) return;
            let _this = this;
            let currentDateStr = this.rtcrm.formatDate(currentDate, "yyyy-MM-dd");

            this.jshelper
                .ApiGet("Holiday/GetAnniversaryList?date=" + currentDateStr)
                .then((res) => {
                    if (_this.rtcrm.isNull(res)) {
                        this.jshelper.openAlertDialog(this, "返回数据为空", "查询纪念日数据失败");
                        return;
                    }
                    if (res.isSuccess) {
                        if (!_this.rtcrm.isNull(res.data)) {
                            let data = res.data;
                            if (!_this.rtcrm.isNull(data)) {
                                _this.$set(_this, "anniversarys", data);
                                _this.days = _this.getPickerData(_this.currentDate) // 默认当前日期初始化日历
                            }
                        }
                    } else {
                        this.jshelper.openAlertDialog(this, res.message, "查询纪念日数据失败");
                    }
                })
                .catch((err) => {
                    _this.jshelper.openAlertDialog(_this, err.message, "查询纪念日数据失败");
                });
        },

        dayClass(day) {
            let className = "day-item";
            if (day.selected) className = "day-item-actived";
            else if (!this.rtcrm.isNullOrWhiteSpace(day.holidayName)) {
                if (day.holidayStatus === '1') className = "day-item-holiday";
                else if (day.holidayStatus === '2') className = "day-item-holiday2";
            }
            else if (!this.rtcrm.isNull(day.date) && (day.date.getDay() == 6 || day.date.getDay() == 0)) className = "day-item-weekend";
            return className;
        },

        underText(day) {
            let text = "";
            if (this.showLunar || this.showHoliday) {
                if (this.showHoliday) {
                    if (day.holidayStatus === '1') {
                        text = day.holidayName;
                    }
                    else if (day.holidayStatus === '2') {
                        text = "班";
                    }
                }
                if (this.showLunar) {
                    if (this.rtcrm.isNullOrWhiteSpace(text)) {
                        text = day.chineseDateText;
                    }
                }
            }
            return text;
        },
    }
}
</script>

<style scoped>
* {
    box-sizing: border-box;
}

.window-backdrop {
    position: fixed;
    left: calc(50% - 250px);
    top: calc(50% - 180px);
    width: 500px;
    min-height: 275px;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    overflow: hidden;
}

.window-calendar {
    position: relative;
    width: 100%;
    height: 100%;
    min-height: 275px;
    z-index: 9999;
}

.window-wrapper {
    position: relative;
    top: 0;
    left: 0;
    height: 100%;
    min-width: 300px;
    max-width: 800px;
    padding: 10px 15px;
    border-radius: 10px;
    overflow: hidden;
}

.window-background {
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 100%;
    border-radius: 10px;
    border: 1px #000 solid;
    overflow: hidden;
}

.week-wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-height: 44px;
    font-weight: bold;

    .week-item {
        font-size: 14px;
        flex: 14.2857%;
        text-align: center;
    }
}

.today-date-text {
    display: flex;
    height: 50px;
    align-items: center;
    justify-content: space-between;
    line-height: 100%;
    padding: 0 20px;

    .text-button {
        font-size: 16px;
        color: #000;
    }

    .arrow-img {
        height: 30px;
        width: 30px;
    }

    .today-date {
        font-size: 18px;
        font-weight: bold;
    }
}

.month-day-wrapper {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;

    .day-item,
    .day-item-holiday,
    .day-item-holiday2,
    .day-item-weekend,
    .day-item-actived {
        position: relative;
        flex: 12.2857%;
        font-size: 16px;
        cursor: pointer;
        overflow: hidden;
        font-weight: bold;
        padding-bottom: 3px;
        margin: 0 3px 3px 3px;

        .date-text,
        .chinese-date-text {
            min-height: 17px;
            text-align: center;
        }
    }

    .day-item,
    .day-item-holiday,
    .day-item-actived,
    .day-item-holiday2,
    .day-item-weekend {
        .chinese-date-text {
            font-weight: normal;
            font-size: 14px;
        }
    }

    .day-item {
        color: #333;
    }

    .day-item-actived {
        color: #fff;
        background: #2875dd;
        border-radius: 5px;
    }

    .day-item-holiday {
        color: #F56C6C;
        font-weight: bold;
        background-color: rgba(245, 108, 108, 0.2);
        border-radius: 5px;
    }

    .day-item-holiday2 {
        color: #606266;
        font-weight: bold;
        background-color: rgba(96, 98, 102, 0.1);
        border-radius: 5px;
    }

    .day-item-weekend {
        color: #F56C6C;
    }
}

.red-dot {
    position: absolute;
    left: calc(100% /2 - 3.5px);
    top: calc(100% /2);
    width: 7px;
    height: 7px;
    background-color: red;
    border-radius: 50%;
    display: inline-block;
}
</style>