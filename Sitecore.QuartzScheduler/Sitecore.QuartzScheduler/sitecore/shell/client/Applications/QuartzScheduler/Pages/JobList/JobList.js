define(["sitecore", "jquery", "underscore"], function (Sitecore, jquery, underscore) {
    var JobList = Sitecore.Definitions.App.extend({
        initialized: function () {

            console.log('JobList page code executing..');

            //approac 1
            //this.getAllJobs();


            //approach 2
            // Function to handle user selecting a job
            this.lcJobs.on("change:selectedItemId", function () {
                //// Check to see if this is already the ListControl with a selected item
                //// if not, clear the selection in the other ListControl
                //this.lcTriggers.set("selectedItemId", null);

                console.log('Starting to populate triggers..');
                // Populate the smart control with info from the selected order
                populateTriggers(this, this.lcJobs);

                console.log('Starting to populate Smart Panel for additional job details..');
                populateSmartPanel(this, this.lcJobs, this.spnlAddnlJobDetails);

            }, this);

            function populateTriggers(that, listControl) {
                var app = that;
                console.log('pupulating triggers list ')
                var selectedGuid = listControl.get("selectedItemId");
                var triggersList = app.GetTriggersForJob(selectedGuid);
                if (triggersList != "") {
                    app.lcTriggers.set("items", JSON.parse(triggersList));
                    console.log('Triggers Populated for Job ' + selectedGuid);
                }
            }

            function populateSmartPanel(that, listControl, smartPanel) {
                console.log('populating smart panel... ')
                // get the selected Item Id
                var selectedGuid = listControl.get("selectedItemId");

                if (selectedGuid && selectedGuid.length > 0) {
                    // remember the selectedGuid in the hidden SelectedGuid text control
                    that.selectedJobGuid.set("text", selectedGuid);

                    // show the smart panel
                    smartPanel.set("isOpen", true);

                    // get the selected raw item
                    var rawItem = listControl.get("selectedItem").get("$fields")[0].item;
                    var jsonJobDataMap = that.GetJobDataMapJson(rawItem["Job Data Map"]);
                    console.log('Job Data Map : ' + jsonJobDataMap);

                    //that.txtLastRunDateValue.set("text", Date.now().toString());
                    //that.txtNextRunDateValue.set("text", "Next Value");
                    that.lcJobDataMap.set("items", jsonJobDataMap);

                }
                else {
                    smartPanel.set("isOpen", false);
                }
            }
        },

        GetTriggersForJob: function (jobId) {
            var self = this;
            var triggers = "";
            console.log('Getting triggers for job  : ' + jobId);
            $.ajax({
                url: '/api/sitecore/ReportData/GetJobTriggerList',
                type: 'GET',
                async: false,
                cache: false,
                data: {
                    'jobId': jobId
                },
                success: function (data) {
                    console.log('Job Triggers returned : ' + data);
                    if (!data) {
                        self.msgNotifications.addMessage("error", { text: "No triggers defined for the job.", actions: [], closable: true, temporary: false })
                        console.log('No triggers returned for a job..');
                    }
                    triggers = data;
                },
                error: function () {
                    console.log("There was error while retrieving the triggers for a job " + jobId);
                }
            });

            return triggers;
        },

        GetJobDataMapJson: function (jobDataMap) {
            var jsonJobDataMap = [];
            var arrKeyValue = jobDataMap.split("&");

            if (arrKeyValue != null && arrKeyValue.length > 0) {
                for (i = 0; i < arrKeyValue.length; i++) {
                    var arrKeyValuePair = arrKeyValue[i].split("=");

                    var newJobDataMap = {
                        "itemId": arrKeyValuePair[0],
                        "Key": arrKeyValuePair[0],
                        "Value": arrKeyValuePair[1]
                    };

                    jsonJobDataMap.push(newJobDataMap);
                }
            }

            console.log('Json Job Data Map as string : ' + JSON.stringify(jsonJobDataMap));
            return jsonJobDataMap;
        },

        onEditJobDetails: function () {
            console.log('Editing Job Details');
            window.location.replace('/sitecore/client/Applications/QuartzScheduler/Pages/JobDetails?jd=' + this.lcJobs.get('selectedItemId'));
        },

        onExecuteJob: function () {
            var app = this;
            var rawItem =  app.lcJobs.get("selectedItem").get("$fields")[0].item;
            console.log("Executing Job: ID = '" + this.lcJobs.get('selectedItemId') + "' and JobName: " + rawItem["Job Key"]);
            var jobKey = rawItem["Job Key"];
            var group = rawItem["Group"];

            $.ajax({
                url: '/api/sitecore/Utility/ExecuteJob',
                type: 'POST',
                async: false,
                cache: false,
                data: {
                    'JobKey': jobKey,
                    'Group' : group
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

    });

    return JobList;
});


/*
        //getAllJobs : function()
        //{
        //    console.log('Getting All Jobs');

        //    var app = this;

        //    jQuery.ajax({
        //        type: "GET",
        //        datatype: "json",
        //        url: "http://sitecorexp8/api/sitecore/JobManager/GetAllJobs",
        //        cache: false,
        //        success: function (data) {
        //            app.populateForm(data);
        //        },
        //        error: function () {
        //            console.log("There was an error. Try again please !!")
        //        }
        //    });
        //},

        //populateForm: function (data) {
        //    console.log('Populating the JobList speak page.')
        //    var app = this;
        //    console.log('setting data source');
        //    app.srchDSJobs.viewModel.items(data);

        //    console.log('List control is bound');
        //    // var srchFacets = app.srchDSJobs.f
        //    console.log('Data: ' + data.Group);
        //    console.log('Data: ' + data['Group']);
        //    app.fcJobs.viewModel.facets([{ 'Name': 'Group', 'Values': data['Group'] }, { 'Name': '', 'Values': data['Job Key'] }]);
        //    console.log('filtercontrol facets: ' + app.fcJobs.viewModel.facets);

        //}
*/