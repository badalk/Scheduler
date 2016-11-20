require.config(
    {
        paths: {
            entityService: '/sitecore/shell/client/Services/Assets/lib/entityservice'
        }
    });
    
define(["sitecore", "jquery", "underscore", "entityService"], function (Sitecore, $, _, entityService) {
    var TriggerDetail = Sitecore.Definitions.App.extend({
        initialized: function () {
            var app = this;

            app.cmbScheduleType.on("change:selectedItemId", function () {
                var selectedSchedule = app.cmbScheduleType.get('selectedItem').itemName;
                console.log('selectedItem: ' + selectedSchedule);
                app.ShowHideScheduleInfo(selectedSchedule, app);
            });

            app.dsDaysOfWeek.on("change:hasItems", function () {
                //var app = this;
                var selectedJobID = Sitecore.Helpers.url.getQueryParameters(window.location.href)['jd'];
                app.selectedJobGuid.set("text", selectedJobID);
                var selectedTriggerID = Sitecore.Helpers.url.getQueryParameters(window.location.href)['td'];

                if (selectedTriggerID && selectedTriggerID.length > 0) {
                    app.LoadTriggerDetails(selectedTriggerID, app);
                }
            });



        },

        ShowHideScheduleInfo: function (scheduleType, app) {

            app.HideAllScheduleDetails(app);

            switch (scheduleType) {
                case "Seconds":
                case "Minutes":
                case "Hours":
                    app.txtRepeatIntervalLabel.set('text', scheduleType);
                    app.durationExpander.set('isVisible', true);
                    break;
                case "Daily":
                    app.dailyExpander.set('isVisible', true);
                    break;
                case "Weekly":
                    app.lcDaysOfWeek.set('sorting', 'a__Sortorder');
                    app.dsDaysOfWeek.set('rootItemId', '{90E6CA1B-CA4D-4868-8D56-85660DA72569}');
                    app.weeklyExpander.set('isVisible', true);
                    break;
                case "Monthly":
                    app.monthlyExpander.set('isVisible', true);
                    break;
                case "Custom":
                    app.customExpander.set('isVisible', true);
                    break;

            }

        },

        HideAllScheduleDetails: function (app) {
            app.durationExpander.set('isVisible', false);
            app.dailyExpander.set('isVisible', false);
            app.weeklyExpander.set('isVisible', false);
            app.monthlyExpander.set('isVisible', false);
            app.customExpander.set('isVisible', false);
        },

        LoadTriggerDetails: function (triggerID, app) {

            var triggerDetailService = this.GetTriggerDetailEntityService();

            console.log('loading trigger details screen..');
            if (triggerID != null && triggerID.length > 0) {
                console.log('calling Trigger Detail entity service...');

                triggerDetailService.fetchEntity(triggerID).execute().then(function (triggerDetail) {
                    console.log('Trigger Detail received from entity service: ' + triggerDetail);
                    app.PopulateTriggerDetails(triggerDetail, app);
                });
            }
        },

        GetDateString: function(date)
        {
            if (date.getFullYear() > 0)
                return (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
            else
                return "";
        },
        GetTimeString: function (date) {
            if (date.getFullYear() > 0)
                return this.pad(date.getHours()) + ":" + this.pad(date.getMinutes());
            else
                return "";
        },
        pad: function(n) {
            return n<10 ? '0'+n : n;
        },

        GetSelectedScheduleTypeItem: function(scheduleTypeItemId){
            var app = this;
            var items = app.dsScheduleType.get("items");
            console.log("schedule type items: " + items);
            var selectedItem;
            for (var i=0; i < items.length; i++) {
                if (items[i].itemId == scheduleTypeItemId) {
                    selectedItem = items[i];
                    console.log("selected item found: " + items[i].itemName);
                }

            }

            return selectedItem;

        },

        PopulateTriggerDetails: function (triggerDetail, app) {

            try{

                console.log('triggerDetail.TriggerKey : ' + triggerDetail.TriggerKey);
                console.log('triggerDetail.StartTime: ' + triggerDetail.StartTime);
                console.log('triggerDetail.EndTime : ' + triggerDetail.EndTime);
                console.log('triggerDetail.ScheduleType : ' +  triggerDetail.ScheduleType);
                console.log('triggerDetail.DaysOfWeeks : ' +  triggerDetail.DaysOfWeeks);
                console.log('triggerDetail.RepeatInterval : ' +  triggerDetail.RepeatInterval);
                console.log('triggerDetail.RepeatCount : ' +  triggerDetail.RepeatCount);
                console.log('triggerDetail.DayOfMonth : ' +  triggerDetail.DayOfMonth);
                console.log('triggerDetail.CronExpression : ' + triggerDetail.CronExpression);

                app.txtTriggerKey.set("text", triggerDetail.TriggerKey);

                //if (triggerDetail.StartTime)
                //{
                console.log("GetDateString(triggerDetail.StartTime): " + app.GetDateString(triggerDetail.StartTime));
                console.log("triggerDetail.StartTime: " + triggerDetail.StartTime);
                console.log("GetTimeString(triggerDetail.StartTime): " + app.GetTimeString(triggerDetail.StartTime));

                app.dtStartDate.set("formattedDate", app.GetDateString(triggerDetail.StartTime));
                app.dtStartTime.set("formattedTime", app.GetTimeString(triggerDetail.StartTime));

                console.log("GetDateString(triggerDetail.EndTime): " + app.GetDateString(triggerDetail.EndTime));
                console.log("triggerDetail.EndTime: " + triggerDetail.EndTime);
                console.log("GetTimeString(triggerDetail.EndTime): " + app.GetTimeString(triggerDetail.EndTime));

                app.dtEndDate.set("formattedDate", app.GetDateString(triggerDetail.EndTime));
                app.dtEndTime.set("formattedTime", app.GetTimeString(triggerDetail.EndTime));
                app.cmbScheduleType.viewModel.rebind(app.dsScheduleType.get("items"), app.GetSelectedScheduleTypeItem(triggerDetail.ScheduleType), triggerDetail.ScheduleType, "itemName", "itemId");
                //app.lcDaysOfWeek.viewModel.checkItems(triggerDetail.DaysOfWeeks);
                console.log("triggerDetail.DaysOfWeeks : " + triggerDetail.DaysOfWeeks)

                for (var i = 0; i < triggerDetail.DaysOfWeeks.underlying.length; i++) {
                    //console.log("triggerDetail.DaysOfWeeks.underlying[" + i + "] : " + triggerDetail.DaysOfWeeks.underlying[i]);
                    console.log("triggerDetail.DaysOfWeeks.underlying[" + i + "].itemId : " + triggerDetail.DaysOfWeeks.underlying[i].itemId);
                    app.lcDaysOfWeek.viewModel.checkItem(triggerDetail.DaysOfWeeks.underlying[i].itemId);
                }

                //app.lcDaysOfWeek.viewModel.checkItem(triggerDetail.DaysOfWeeks[i].itemId);
                

                //app.lcDaysOfWeek.viewModel.checkItems(triggerDetail.DaysOfWeeks);

                app.txtRepeatInterval.set("text", triggerDetail.RepeatInterval);
                app.txtRepeatCount.set("text", triggerDetail.RepeatCount);
                app.txtDayOfMonth.set("text", triggerDetail.DayOfMonth);
                app.txtCustomCronExpression.set("text", triggerDetail.CronExpression);
            }
            catch (exception) {
                console.log("Error in PopulateTriggerDetails : " + exception);
                return (exception);
            }

        },

        GetTriggerDetailEntityService: function () {
            var triggerDetailService = new entityService({
                url: "/sitecore/api/ssc/sitecore-quartzscheduler-controllers/triggerdetail"
            });
            return triggerDetailService;
        },

    });



  return TriggerDetail;
});