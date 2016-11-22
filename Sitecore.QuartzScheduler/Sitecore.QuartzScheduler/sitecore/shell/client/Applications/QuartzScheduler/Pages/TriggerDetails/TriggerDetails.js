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
                    //console.log("triggerDetail.DaysOfWeeks.underlying[" + i + "].itemId : " + triggerDetail.DaysOfWeeks.underlying[i].itemId);
                    app.lcDaysOfWeek.viewModel.checkItem(triggerDetail.DaysOfWeeks.underlying[i].itemId);
                }


                app.txtRepeatInterval.set("text", (triggerDetail.RepeatInterval == 0 ? "" : triggerDetail.RepeatInterval));
                app.txtRepeatCount.set("text", (triggerDetail.RepeatCount == 0 ? "" : triggerDetail.RepeatCount));
                app.txtDayOfMonth.set("text", (triggerDetail.DayOfMonth == 0 ? "" : triggerDetail.DayOfMonth));
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

        GetJobDetailEntityService: function () {
            var jobDetailService = new entityService({
                url: "/sitecore/api/ssc/sitecore-quartzscheduler-controllers/jobmanager"
            });
            return jobDetailService;
        },


        onSaveTriggerDetails: function () {
            var app = this;
            var jobId = Sitecore.Helpers.url.getQueryParameters(window.location.href)['jd'];
            var triggerId = Sitecore.Helpers.url.getQueryParameters(window.location.href)['td'];

            if (jobId != ""){

                if (triggerId != "") {
                    console.log("calling update function for trigger details " +  triggerId + " under job " + jobId);
                    app.UpdateTrigerDetail(jobId, triggerId);
                }
                else {
                    console.log('Creating a trigger under job : ' + jobId);
                    app.CreateTriggerDetail(jobId);
                }
            }
            else {
                app.msgNotifications.addMessage("error", { text: "You are not in the valid context of Trigger Details. Trigger ID is missing.", actions: [], closable: true, temporary: false })
            }
        },

        UpdateTrigerDetail: function (jobId, triggerId) {
            var self = this;
            var triggerDetailService = this.GetTriggerDetailEntityService();

            triggerDetailService.fetchEntity(triggerId).execute().then(function (trigger) {
                console.log('entity to update fethed..' + trigger);

                try {

                    if (self.IsValidEntity()) {

                        trigger = self.GetTriggerDetailUpdated(trigger);
                         console.log('updated entity (ready to save) : ' + trigger.toString());

                        trigger.on('save', function () {
                            console.log('save function on entity service called');
                            self.UpdateSuccessful(self);
                        });

                        //trigger.save().execute();
                    }
                }
                catch (error) {
                    console.log('Error occured while updating the Trigger Detail: ' + error.message);
                }
            });
        },

        CreateTriggerDetail: function(jobId){

        },

        IsValidEntity: function () {
            var isValid = true;
            var self = this;
            self.msgNotifications.removeMessages();

            console.log("IsValidEntity - self.txtTriggerKey.viewModel.text().trim() : " + self.txtTriggerKey.viewModel.text().trim());
            if (self.txtTriggerKey.viewModel.text().trim().length == 0) {
                self.msgNotifications.addMessage("error", { text: "Trigger Key is required.", actions: [], closable: true, temporary: false })
                console.log('Trigger key is needed..');
                isValid = false;
            }
            console.log("IsValidEntity - self.cmbScheduleType.get(\"selectedItemId\") : " + self.cmbScheduleType.get("selectedItemId"));
            if (self.cmbScheduleType.get("selectedItemId") == "{14126F4B-CB96-4038-A199-B46681530C00}") {
                self.msgNotifications.addMessage("error", { text: "Select valid Schedule Type", actions: [], closable: true, temporary: false })
                console.log('Schedule Type is not selected..');
                isValid = false;
            }

            var scheduleType = self.cmbScheduleType.get('selectedItem').itemName.toLowerCase();
            var repeatInterval = self.txtRepeatInterval.viewModel.text();
            var repeatCount = self.txtRepeatCount.viewModel.text();
            var dayOfMonth = self.txtDayOfMonth.viewModel.text();

            console.log("IsValidEntity - scheduleType : " + scheduleType);
            switch (scheduleType) {
                case "hours":
                case "minutes":
                case "seconds":
                    if (isNaN(repeatInterval)) {
                        self.msgNotifications.addMessage("error", { text: "Repeat Interval should be a number", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }
                    else {
                        var iRptInterval = parseInt(repeatInterval);

                        if ((scheduleType == "seconds") || (scheduleType == "minutes")) {
                            if ((iRptInterval <= 0) || (iRptInterval > 59)) {
                                self.msgNotifications.addMessage("error", { text: "Repeat Interval should be between 1 to 59", actions: [], closable: true, temporary: false });
                                isValid = false;

                            }
                        }
                        else if (scheduleType == "hours") {
                            if ((iRptInterval <= 0) || (iRptInterval > 23)) {
                                self.msgNotifications.addMessage("error", { text: "Repeat Interval should be between 1 to 23", actions: [], closable: true, temporary: false });
                                isValid = false;
                            }
                        }
                    }

                    if (isNaN(repeatCount)) {
                        self.msgNotifications.addMessage("error", { text: "Repeat Count should be a positivie number", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }
                    else if (parseInt(repeatCount) < 0) {
                        self.msgNotifications.addMessage("error", { text: "Repeat Count should be a positivie number", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }
                    break;
                case "monthly":

                    if (isNaN(dayOfMonth)) {
                        self.msgNotifications.addMessage("error", { text: "Day of Month should be a positivie number", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }
                    else if ((parseInt(dayOfMonth) <= 0) || (parseInt(dayOfMonth) > 31)) {
                        self.msgNotifications.addMessage("error", { text: "Day of Month has to be beween 1 to 31", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }
                    break;
                case "weekly":
                    console.log("IsValidEntity - self.lcDaysOfWeek.viewModel.checkedItems().length: " + self.lcDaysOfWeek.viewModel.checkedItems().length);
                    if (self.lcDaysOfWeek.viewModel.checkedItems().length <=0){
                        self.msgNotifications.addMessage("error", { text: "Please select at least one day of the week", actions: [], closable: true, temporary: false });
                        isValid = false;
                    }

                    break;
                case "custom":
                    console.log("self.txtCustomCronExpression.viewModel.text().trim()")
                    isValid = self.IsValidCronExpression(self.txtCustomCronExpression.viewModel.text().trim())
                    break;

            }
            console.log("IsValiEntity: " + isValid);
            return isValid;

        },

        GetTriggerDetailUpdated: function (trigger) {
            var self = this;
            console.log("Start date: " + self.dtStartDate.get("date"));
            console.log("Start time: " + self.dtStartTime.get("time"));
            console.log("End Date : " + self.dtEndDate.get("date"));
            console.log("End Time : " + self.dtEndTime.get("time"));
            console.log("Schedule Type : " + self.cmbScheduleType.get("selectedItemId"));
            console.log("Repeat Interval : " + self.txtRepeatInterval.viewModel.text());
            console.log("Repeat Count : " + self.txtRepeatCount.viewModel.text());
            console.log("Cron Expression: " + self.txtCustomCronExpression.viewModel.text().trim());

            trigger.TriggerKey = self.txtTriggerKey.viewModel.text();
            //trigger.StartTime = self.dtStartDate.get("date")
            //trigger.EndTime = self.dtEndDate.get("date")
            trigger.RepeatInterval = parseInt(self.txtRepeatInterval.viewModel.text());
            trigger.RepeatCount = parseInt(self.txtRepeatCount.viewModel.text());
            trigger.ScheduleType = self.cmbScheduleType.get("selectedItemId");
            trigger.DayOfMonth = parseInt(self.txtDayOfMonth.viewModel.text());
            trigger.CronExpression = parseInt(self.txtCustomCronExpression.viewModel.text().trim());

            trigger.DaysOfWeeks.underlying = []; //resetting the DayOfWeeks selections to set the new ones
            console.log("No of items checked: " + self.lcDaysOfWeek.viewModel.checkedItems().length);

            for (var i = 0; i < self.lcDaysOfWeek.viewModel.checkedItems().length; i++) {
                //console.log(" triggerDetail.DaysOfWeeks.underlying[i]: " + triggerDetail.DaysOfWeeks.underlying[i]);
                var item = self.lcDaysOfWeek.viewModel.checkedItems()[i];
                var day = new {
                    itemId : item.itemId,
                    itemName : item.itemName
                };
                console.log("self.lcDaysOfWeek.viewModel.checkedItems[" + i + "].itemId : " + item.itemId);
                //trigger.DaysOfWeeks.underlying[i].itemId = item.itemId;
                //trigger.DaysOfWeeks.underlying[i].itemName = item.itemName;
                trigger.DaysOfWeeks.underlying[i] = day;
                console.log("triggerDetail.DaysOfWeeks.underlying[" + i + "]: " + trigger.DaysOfWeeks.underlying[i]);
            }

            return trigger;
        },

        IsValidCronExpression: function (cronExpression) {
            var self = this;
            var isValidCronExpression = true;
            console.log('Cron expression being validated..' + cronExpression);
            $.ajax({
                url: '/api/sitecore/Utility/IsValidCronExpression',
                type: 'GET',
                async: false,
                cache: false,
                data: {
                    'cronExpression': cronExpression
                },
                success: function (data) {
                    console.log('IsValidCronExpression returned: ' + data);
                    if (!data) {
                        self.msgNotifications.addMessage("error", { text: "CronExpression provided is not valid. Refer cronexpression guide http://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontrigger.html", actions: [], closable: true, temporary: false })
                        console.log('Cron Expression is not valid..');
                        isValidCronExpression = false;
                    }
                },
                error: function () {
                    isValidCronExpression = true;
                    console.log("There was an error while validating the Cron Expression. Still trying to update but your job may not work as expected!");
                }
            });

            return isValidCronExpression;
        },



    });



  return TriggerDetail;
});