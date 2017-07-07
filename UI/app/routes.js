angular.module('triton').config(function ($routeProvider, $locationProvider) {

    // Setup HTML 5 mode, for better user experience
    $locationProvider.html5Mode({
        enabled: true,
        requireBase: false,
        rewriteLinks: true
    }).hashPrefix('!');

    $routeProvider
        .when('/', {
            controller: 'ClusterController',
            templateUrl: function () {
                return '/app/cluster/cluster.html';
            }
        })
    .otherwise({
        controller: 'ErrorController',
        templateUrl: '/app/error/error.html'
    });
});