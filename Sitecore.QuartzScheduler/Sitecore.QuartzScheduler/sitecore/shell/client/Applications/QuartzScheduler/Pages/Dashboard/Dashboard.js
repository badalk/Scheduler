define(["sitecore"], function (Sitecore) {
  var Dashboard = Sitecore.Definitions.App.extend({
      initialized: function () {
          var app = this;
          console.log('Dashboard page is initializing');
          var requestOptions = {
              parameters: "",
              onSuccess: function (result) {
                  app.getDataCallback.apply(app, arguments);
              }, 
          };

          app.TabControl1.on("change:selectedTabIndex", function (component, value) {
              switch (value) {
                  case 0:
                      app.ChartDataProviderSummary.viewModel.getData(requestOptions);
                      break;
                  case 1:
                      app.ChartDataProviderJobPerformance.viewModel.getData(requestOptions);
                      break;
                  case 2:
                      app.LoadCurrentJobStatus();
                      app.LoadFireTimes();
                      break;
              }
          });

          this.ChartDataProviderJobPerformance.on("error", function (errorObject) {
              console.log('Error in ChartDataProviderJobPerformance: ' + console.dir(errorObject));
          });

          this.ChartDataProviderSummary.on("error", function (errorObject) {
              console.log('Error in ChartDataProviderSummary: ' + console.dir(errorObject));
          });

          console.log('Getting Job Performance Summary...');
          this.ChartDataProviderSummary.viewModel.getData(requestOptions);

          console.log('Getting Performance data...');
          this.ChartDataProviderJobPerformance.viewModel.getData(requestOptions);


      },
      LoadCurrentJobStatus: function () {
          var app = this;
          console.log('pupulating job execution status ')
          var jobStatusList = app.GetJobExecutionStatus();
          if (jobStatusList != "") {
              app.lcJobExecutionStatus.set("items", JSON.parse(jobStatusList));
              console.log('Job Execution Status populated ');
          }
      },

      LoadFireTimes: function () {
          var app = this;
          console.log('pupulating job fire times')
          var jobStatusList = app.GetJobFireTimes();
          if (jobStatusList != "") {
              app.lcJobFireTimes.set("items", JSON.parse(jobStatusList));
              console.log('Job Fire Times  populated ');
          }
      },

      getDataCallback: function (data) {
          var app = this;
          console.log('data: ' + data);
      },

      getJobPerformanceData: function () {
          window.open("/api/sitecore/ReportData/GetJobWisePerformanceData", '_blank');
      },

      GetJobExecutionStatus: function () {
          var app = this;
          var jobs = "";
          console.log('Getting Currently executing Jobs status');
          $.ajax({
              url: '/api/sitecore/ReportData/GetCurrentJobExecutionStatus',
              type: 'GET',
              async: false,
              cache: false,
              data: {
              },
              success: function (data) {
                  console.log('Job Stutus returned : ' + console.dir(data));
                  if ((!data) || (data == "")) {
                      app.msgNotifications.addMessage("notification", { text: "No Jobs executing currently", actions: [], closable: true, temporary: true })
                      console.log('No jobs currently running..');
                  }
                  jobs = data;
              },
              error: function () {
                  console.log("There was error while retrieving the current job execution status");
              }
          });

          return jobs;
      },

      GetJobFireTimes: function () {
          var app = this;
          var jobs = "";
          console.log('Getting Job Fire Times');
          $.ajax({
              url: '/api/sitecore/ReportData/GetJobFireTimes',
              type: 'GET',
              async: false,
              cache: false,
              data: {
              },
              success: function (data) {
                  console.log('Job Fire Times returned : ' + console.dir(data));
                  if ((!data) || (data == "")) {
                      console.log('No jobs currently scheduled..');
                  }
                  jobs = data;
              },
              error: function () {
                  console.log("There was error while retrieving the current job fire times");
              }
          });

          return jobs;
      },


  });

  return Dashboard;
});