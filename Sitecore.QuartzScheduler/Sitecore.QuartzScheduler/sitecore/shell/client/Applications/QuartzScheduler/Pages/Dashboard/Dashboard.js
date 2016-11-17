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

          this.ChartDataProviderJobPerformance.on("error", function (errorObject) {
              console.log('Error in ChartDataProviderJobPerformance: ' + errorObject);
          });

          this.ChartDataProviderSummary.on("error", function (errorObject) {
              console.log('Error in ChartDataProviderSummary: ' + errorObject);
          });

          console.log('Getting Job Performance Summary...');
          this.ChartDataProviderSummary.viewModel.getData(requestOptions);

          console.log('Getting Performance data...');
          this.ChartDataProviderJobPerformance.viewModel.getData(requestOptions);
         // this.ChartDataProvider2.viewModel.getData(requestOptions);
          console.log('Attempted to get Performance data...');

      },

      getDataCallback: function (data) {
          var app = this;
          console.log('data: ' + data);
          //console.log('app.ChartDataProviderJobPerformance.get("data"): ' + app.ChartDataProviderJobPerformance.get("data"));
      },

      getJobPerformanceData: function () {
          window.open("/api/sitecore/ReportData/GetJobWisePerformanceData", '_blank');
      },

      loadChartData: function () {
          var app = this;
          $.getJSON(app.txtJsonPath.viewModel.text(), function (data) { alert(data);});

      }


  });

  return Dashboard;
});