define(["sitecore"], function (Sitecore) {
    var TriggerDetailDialog = Sitecore.Definitions.App.extend({
        initialized: function () {
            var app = this;

            app.cmbScheduleType.on("change:selectedItemId", function () {
                var selectedSchedule = app.cmbScheduleType.get('selectedItem').itemName;
                console.log('selectedItem: ' + selectedSchedule);
                app.ShowHideScheduleInfo(selectedSchedule, app);
            });

        },

        ShowHideScheduleInfo: function (scheduleType, app) {

            //app.durationExpander.set('isVisible', false);
            //app.dailyExpander.set('isVisible', false);
            //app.weeklyExpander.set('isVisible', false);
            //app.monthlyExpander.set('isVisible', false);
            //app.customExpander.set('isVisible', false);

            app.HideAllScheduleDetails(app);

            switch (scheduleType) {
                case "Seconds":
                case "Minutes":
                case "Hours":
                    app.txtDurationLabel.set('text', scheduleType);
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
        }

    });


  return TriggerDetailDialog;
});