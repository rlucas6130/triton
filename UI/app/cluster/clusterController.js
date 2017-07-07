angular.module('triton').controller('ClusterController', function ($scope, $http) {
    
    $scope.calcs = [];
    $scope.clusters = [];

    $http.get('/Home/LoadMatrixData').then(function () {
        $http.get('/Home/GetClusters?k=12').then(function (response) {

            $.each(response.data.NameList, function (i, names) {

                $scope.clusters.push({
                    nameList: _.map(names, (name) => {
                        return _.replace(name, "D:/Wiki/wiki-small\\en\\articles\\", "");
                    }),
                    SI: response.data.ClusterSiList[i],
                    categoryNames: response.data.CategoryNameMap[i],
                });
            });

            
            $scope.globalSi = response.data.GlobalSI;
            $scope.numClusters = response.data.NumClusters;
            $scope.clusterSiAverage = response.data.ClusterSIAverage;
        });
    });
});