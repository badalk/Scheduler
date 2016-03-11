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
                console.log('pupulating triggers list ')
                // get the selected Item Id
                var selectedGuid = listControl.get("selectedItemId");
                that.srchDSTriggers.set('rootItemId', selectedGuid);
                console.log('Triggers Populated for Job ' + selectedGuid);
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
                    var jsonJobDataMap = GetJobDataMapJson(rawItem["Job Data Map"]);
                    console.log('Job Data Map : ' + jsonJobDataMap);

                    that.txtLastRunDateValue.set("text", Date.now().toString());
                    that.txtNextRunDateValue.set("text", "Next Value");
                    that.lcJobDataMap.set("items", jsonJobDataMap);

                }
                else {
                    smartPanel.set("isOpen", false);
                }
            }

            function GetJobDataMapJson(jobDataMap) {

                var jsonObjectStream = "";
                var arrKeyValue = jobDataMap.split("&");
                if (arrKeyValue != null && arrKeyValue.length > 0) {
                    for (i = 0; i < arrKeyValue.length; i++) {
                        var arrKeyValuePair = arrKeyValue[i].split("=");
                        if (i == 0)
                            jsonObjectStream = jsonObjectStream + "[";
                        jsonObjectStream = jsonObjectStream + "{\"Key\":\"" + arrKeyValuePair[0] + "\", \"Value\": \"" + arrKeyValuePair[1] + "\"}";

                        if (i < arrKeyValue.length - 1)
                            jsonObjectStream = jsonObjectStream + ", ";

                        if (i == arrKeyValue.length - 1)
                            jsonObjectStream = jsonObjectStream + "]";
                    }
                }

                console.log('Json Job Data Map as string : ' + jsonObjectStream);
                return JSON.parse(jsonObjectStream);
            }

        },

        onEditJobDetails: function () {
            console.log('Editing Job Details');
            window.location.replace('/sitecore/client/Applications/QuartzScheduler/Pages/JobDetails?jd=' + this.lcJobs.get('selectedItemId'));
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