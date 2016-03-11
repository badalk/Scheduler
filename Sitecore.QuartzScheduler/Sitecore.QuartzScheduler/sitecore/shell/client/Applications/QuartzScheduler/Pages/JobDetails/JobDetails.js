require.config(
    {
        paths: {
            entityService: '/sitecore/shell/client/Services/Assets/lib/entityservice'
        }
    });

define(["sitecore", "jquery", "underscore", "entityService" ], function (Sitecore, $, _, entityService) {
    var JobDetails = Sitecore.Definitions.App.extend({

        initialized: function () {
            console.log('Job Details page initialized');

            var app = this;

            var selectedJobID = Sitecore.Helpers.url.getQueryParameters(window.location.href)['jd'];
            console.log('Job ID retrieved : ' + selectedJobID);

            if (selectedJobID && selectedJobID.length > 0) {
                this.LoadJobDetails(selectedJobID, app);
            }

            this.lcJobDataMap.on('change:selectedItemId', function (component, value) {
                app.smPnlJobDataMap.set("isOpen", false);
            });
        },

        LoadJobDetails: function (selectedJobID, app) {
            var jobDetailService = this.GetJobDetailEntityService();

            console.log('loading job details screen..');
            if (selectedJobID != null && selectedJobID.length > 0) {
                // remember the selectedGuid in the hidden SelectedGuid text control
                console.log('calling JobDetail entity service...');
                app.selectedJobGuid.set("text", selectedJobID);

                jobDetailService.fetchEntity(selectedJobID).execute().then(function (jobDetail) {
                    console.log('Job Detail received from entity service: ' + jobDetail);
                    app.txtJobKey.set("text", jobDetail.JobKey);
                    app.txtareaDescription.set("text", jobDetail.Description);
                    app.txtGroup.set("text", jobDetail.Group);
                    app.txtJobType.set("text", jobDetail.Type);

                    console.log('jobDetail.Data => ' + jobDetail.JobData);
                    var jsonJobDataMap = app.GetJobDataMapJson(jobDetail.JobData);
                    console.log('jsonjobDataMap' + jsonJobDataMap);
                    app.lcJobDataMap.set("items", jsonJobDataMap);
                    app.lcJobDataMap.viewModel.set("selectedItemId", jsonJobDataMap[0].ItemId);
                });

            }

        },

        CreateJobDetail: function(){
            var self = this;
            var jobDetailService = this.GetJobDetailEntityService();
            var jobDataMapString = this.GetJobDataMapString(self.lcJobDataMap.get("items"));
            console.log('jobDataMapString: ' + jobDataMapString);

            //create JobDetail entity
            var jobDetail = {
                JobKey: self.txtJobKey.viewModel.text(),
                Description: self.txtareaDescription.viewModel.text(),
                Group: self.txtGroup.viewModel.text(),
                Type: self.txtJobType.viewModel.text(),
                JobData: jobDataMapString
            };

            console.log('jobDetail to be saved: ' + jobDetail.toString());

            jobDetailService.create(jobDetail).execute().then(function (newJobDetail) {
                console.log('entity returned: ' + newJobDetail.toString());
                self.msgNotifications.addMessage("notification", { text: "Job Details created successfully !", actions: [], closable: true, temporary: true });
            }).fail(function (error) {
                self.msgNotifications.addMessage("error", { text: error.message, actions: [], closable: true, temporary: true });
            });
        },

        UpdateJobDetail: function(id){
            var self = this;
            var jobDetailService = this.GetJobDetailEntityService();
            var jobDataMapString = this.GetJobDataMapString(self.lcJobDataMap.get("items"));
            console.log('jobDataMapString: ' + jobDataMapString);

            jobDetailService.fetchEntity(id).execute().then(function (jobDetail) {
                console.log('entity to update fethed..' + jobDetail);
                try{
                    jobDetail.JobKey = self.txtJobKey.viewModel.text();
                    jobDetail.Description = self.txtareaDescription.viewModel.text();
                    jobDetail.Group = self.txtGroup.viewModel.text();
                    jobDetail.Type = self.txtJobType.viewModel.text();
                    jobDetail.JobData = jobDataMapString;
                }
                catch (error) {
                    console.log('error while updating attributes..' + error.message);
                }
            
                console.log('updated entity (ready to save) : ' + jobDetail.toString());

                jobDetail.on('save', function () {
                    console.log('save function on entity service called');
                    self.UpdateSuccessful(self);
                });

                try{
                    jobDetail.save().execute();
                }
                catch (err) {
                    console.log('Error occured while updating the JobDetail: ' + err.message);
                }

            });
        },

        UpdateSuccessful: function (self) {
            console.log('update successful.');
            self.msgNotifications.addMessage("notification", { text: "Job Details updated successfully !", actions: [], closable: true, temporary: true});

        },

        SaveJobDetails: function(){
            var jobId = this.selectedJobGuid.viewModel.text();

            if (jobId != "") {
                console.log('calling update function for job id: ' + jobId);
                this.UpdateJobDetail(jobId);
            }
            else {
                console.log('calling create function');
                this.CreateJobDetail();
            }
        },

        GetJobDataMapJson: function (jobDataMap) {

            //var jsonObjectStream = "";
            var jsonJobDataMap = [];
            var arrKeyValue = jobDataMap.split("&");
            console.log('Array splitted');
            if (arrKeyValue != null && arrKeyValue.length > 0) {
                for (i = 0; i < arrKeyValue.length; i++) {
                    //TODO: Need to change the implementation to use JSON Object
                    var arrKeyValuePair = arrKeyValue[i].split("=");
                    //if (i == 0)
                    //    jsonObjectStream = jsonObjectStream + "[";
                    //jsonObjectStream = jsonObjectStream + "{\"itemId\":\"" + arrKeyValuePair[0] + "\",  \"Key\":\"" + arrKeyValuePair[0] + "\", \"Value\": \"" + arrKeyValuePair[1] + "\"}";

                    //if (i < arrKeyValue.length - 1)
                    //    jsonObjectStream = jsonObjectStream + ", ";

                    //if (i == arrKeyValue.length - 1)
                    //    jsonObjectStream = jsonObjectStream + "]";

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

        GetJobDataMapString: function (jsonJobDataMap) {
            console.log('In GetJobDataMapString with ' + jsonJobDataMap);
            if (jsonJobDataMap != null && jsonJobDataMap.length > 0) {
                var jobDataMapString = "";
                for (i = 0; i < jsonJobDataMap.length; i++) {
                    jobDataMapString = jobDataMapString + jsonJobDataMap[i].Key + "=" + jsonJobDataMap[i].Value;

                    if (i < jsonJobDataMap.length - 1)
                        jobDataMapString = jobDataMapString + "&";
                }
                return jobDataMapString;
            }
        },

        GetJobDetailEntityService: function () {
            var jobDetailService = new entityService({
                url: "/sitecore/api/ssc/sitecore-quartzscheduler-controllers/jobmanager"
            });
            return jobDetailService;
        },

        onGoBack: function () {
            console.log('Going back to previous page');
            window.location.replace('/sitecore/client/Applications/QuartzScheduler/Pages/JobList');
        },

        onEditJobDataMap: function() {
            var app = this;
            console.log('Editing Job Data Map')
            var rawItem = app.lcJobDataMap.get("selectedItem").attributes;
            app.txtOriginalJobDataMapKey.set("text", rawItem["itemId"]);
            app.txtJobDataMapKey.set("text", rawItem["Key"]);
            app.txtJobDataMapValue.set("text", rawItem["Value"]);

            app.smPnlJobDataMap.set("isOpen", true);
        },

        onAddJobDataMap: function(){
            var app = this;
            console.log('Adding new Job Data Map')
            app.txtOriginalJobDataMapKey.set("text", "");
            app.txtJobDataMapKey.set("text", "");
            app.txtJobDataMapValue.set("text", "");

            app.smPnlJobDataMap.set("isOpen", true);
        },

        onDeleteJobDataMap: function () {
            var app = this;
            var selectedJobDataMap = app.lcJobDataMap.viewModel.selectedItemId();
            console.log('Deleting selected item ' + selectedJobDataMap);
            var jsonJobDataMap = app.lcJobDataMap.get("items");
            if (selectedJobDataMap != null && selectedJobDataMap != "") {
                for (var i = 0; i < jsonJobDataMap.length; i++) {
                    if (jsonJobDataMap[i].itemId == selectedJobDataMap) {
                        console.log('deleteing... ' + jsonJobDataMap[i].itemId);
                        jsonJobDataMap.splice(i, 1);
                    }
                }
            }
            this.resetJobDataMapListControl(this.lcJobDataMap, jsonJobDataMap);
        },

        onSaveJobDataMap: function () {
            var app = this;

            var editedJobDataMap = app.txtOriginalJobDataMapKey.viewModel.text();
            console.log('editedJobDataMap: ' + editedJobDataMap);

            var jsonJobDataMap = app.lcJobDataMap.get("items");
            console.log('Whats in the JobDataMap list control: ' + jsonJobDataMap);

            var newJsonJobDataMap = app.GetUpdatedJobDataMap(editedJobDataMap, jsonJobDataMap);
            console.log('new json data map: ' + JSON.stringify(newJsonJobDataMap));
            this.resetJobDataMapListControl(app.lcJobDataMap, newJsonJobDataMap);
            app.smPnlJobDataMap.set("isOpen", false);
        },

        resetJobDataMapListControl: function (listcontrol, newJsonJobDataMap) {
            console.log('listcontrol:' + listcontrol);
            listcontrol.set("items", null); //clear
            listcontrol.set("items", newJsonJobDataMap);
        },

        GetUpdatedJobDataMap: function (editedJobDataMap, jsonJobDataMap) {

            //if (jsonJobDataMap != null && jsonJobDataMap.length > 0) {

            console.log('editedJobDataMap:' + editedJobDataMap);
                //edit existing job data map parameter
            if (editedJobDataMap != null && editedJobDataMap != "") {
                for (var i = 0; i < jsonJobDataMap.length; i++) {
                    if (jsonJobDataMap[i].itemId == editedJobDataMap) {
                        jsonJobDataMap[i].Key = this.txtJobDataMapKey.viewModel.text();
                        jsonJobDataMap[i].Value = this.txtJobDataMapValue.viewModel.text();
                    }

                }
            }
            else { //new parameters
                console.log('Adding new Job Data Map');
                var newJobDataMap = {
                    "itemId": this.txtJobDataMapKey.viewModel.text(),
                    "Key": this.txtJobDataMapKey.viewModel.text(),
                    "Value": this.txtJobDataMapValue.viewModel.text()
                };

                if (jsonJobDataMap == null) {
                    jsonJobDataMap = [];
                }
                jsonJobDataMap.push(newJobDataMap);
                console.log('New Job Data Map after adding new values:' + JSON.stringify(jsonJobDataMap));

            }

            return jsonJobDataMap;
        },

  });
    
  return JobDetails;
});