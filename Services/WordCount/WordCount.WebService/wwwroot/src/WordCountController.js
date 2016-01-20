(function(){
    var app = angular.module('wordCountApp');
    app.controller("WordCountController", WordCountController);

    function WordCountController(wordCountService, $mdSidenav, $mdBottomSheet, $log, $q, $scope, $mdDialog,
        $interval, appConfig) {
        $scope.partitionData = null;
        $scope.chartObject = {};
        $scope.partitionChartType = "ColumnChart";
        $scope.SendWordsResponse = null;

        onError = function (reason) {
            $scope.message = "Failed!!  " + reason.statusText;
        }

        $scope.Draw = function()
        {
            wordCountService
                .Count()
                .then(onDrawChart, onError);
        };

        var onDrawChart = function (data) {
            $scope.partitionData = data;
            $log.info($scope.partitionData);

            if ($scope.partitionChartType === null) {
                $scope.partitionChartType = "ColumnChart";
            }

            if ($scope.partitionData != null && $scope.partitionData.Infos != null) {
                $scope.chartObject.type = $scope.partitionChartType;
                $scope.chartObject.data = {
                    "cols": [
                        { id: "p", label: "Partition", type: "string" },
                        { id: "k", label: "Words", type: "number" }
                    ],
                    "rows": []
                };

                $scope.partitionData.Infos.forEach(function (entry, idx) {
                    var row = {
                        c: [
                           { v: $scope.partitionData.Infos[idx].LowKey + "-" + $scope.partitionData.Infos[idx].HighKey },
                           { v: $scope.partitionData.Infos[idx].Hits },
                        ]
                    }
                    $scope.chartObject.data.rows.push(row);
                });

                $scope.chartObject.options = {
                    'title': 'Words per partition',
                    isStacked: true,
                    vAxis: { minValue: 0 }
                };
            }
        }
        
        $scope.SendWords = function () {
            wordCountService
               .SendWords()
               .then(function (data) {
                   $scope.SendWordsResponse = data;
                   $log.info($scope.SendWordsResponse);
               }, onError);
        }

        $scope.partitionChartTypeChange = function(item) {
            $scope.partitionChartType = item;
            $scope.Draw();
        };

        var addInterval = null;
        var addWordTask = null;
        var startInterval = function () {
            addInterval = $interval($scope.Draw, 350);
            addWordTask = $interval($scope.SendWords, 200);
        }

        $scope.partitionChartType = "ColumnChart";
        startInterval();
    }
})();
