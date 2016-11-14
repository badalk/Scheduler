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

          this.ChartDataProvider1.on("error", function (errorObject) {
              console.log('Error in ChartDataProvider1: ' + errorObject);
          });

          this.LineChartPerformanceData.on("error", function (errorObject) {
              console.log('Error in BarChart1: ' + errorObject);
          });

          //this.LineChart1.viewModel.toggleProgressIndicator(true);


          console.log('Getting Performance data...');
          this.ChartDataProvider1.viewModel.getData(requestOptions);
          this.ChartDataProvider2.viewModel.getData(requestOptions);
          console.log('Attempted to get Performance data...');

      },

      getDataCallback: function (data) {
          var app = this;
          //console.log('app: ' + app);
          console.log('data: ' + data);
          //app.ChartDataProvider1.set("data", data);
          console.log('app.ChartDataProvider1.get("data"): ' + app.ChartDataProvider1.get("data"));
          //console.log('app.BarChart1.get("data"): ' + app.LineChart1.get("data"));
          //app.BarChart1.
      }

  });

  return Dashboard;
});