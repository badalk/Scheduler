define(["sitecore", "jquery", "underscore"], function (Sitecore, jquery, underscore) {
    var JobList = Sitecore.Definitions.App.extend({
        initialized: function () {

            var app = this;
            console.log('JobList page code executing..');

            var jobs = app.GetJobList();
            app.lcJobs.set("items", JSON.parse(jobs));
 
        },

 
        onEditJobDetails: function () {
            console.log('Editing Job Details');
            window.location.replace('/sitecore/client/Applications/QuartzScheduler/Pages/JobDetails?jd=' + this.lcJobs.get('selectedItemId'));
        },

        GetJobList: function (group) {
            var app = this;
            var jobs = "";
            console.log('Getting list of jobs');
            $.ajax({
                url: '/api/sitecore/ReportData/GetJobDefinitions',
                type: 'GET',
                async: false,
                cache: false,
                data: {
                },
                success: function (data) {
                    console.log('Job List returned : ' + data);
                    console.dir(data);
                    if ((!data) || (data == "")) {
                        //app.msgNotifications.addMessage("notification", { text: "No Jobs executing currently", actions: [], closable: true, temporary: true })
                        console.log('No jobs currently defined..');
                    }
                    jobs = data;
                },
                error: function () {
                    console.log("There was error while retrieving the current job definitions");
                }
            });

            return jobs;
        },

        onExecuteJob: function () {

            var app = this;
            var rawItem =  app.lcJobs.get("selectedItem").get("$fields")[0].item;
            console.log("Executing Job: ID = '" + this.lcJobs.get('selectedItemId') + "' and JobName: " + rawItem["Job Key"]);
            var jobKey = rawItem["Job Key"];
            var group = rawItem["Group"];

            if (confirm("Are you sure you want to execute - \"" + jobKey + "\" ?")) {

                $.ajax({
                    url: '/api/sitecore/Utility/ExecuteJob',
                    type: 'POST',
                    async: false,
                    cache: false,
                    data: {
                        'JobKey': jobKey,
                        'Group': group
                    },
                    success: function (data) {
                        console.log('Job Executed with success');
                        alert('Job : "' + jobKey + '" executed successfully');
                    },
                    error: function () {
                        console.log("There was an error executing the job");
                        alert('Error occured while executing Job : "' + jobKey + '"');
                    }
                });
            }

        }

    });

    return JobList;
});
