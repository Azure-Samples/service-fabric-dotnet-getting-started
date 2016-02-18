(function(){
    'use strict';
    angular.module('wordCountApp')
        .service('wordCountService', ['$q', '$http', 'appConfig', WordCountService]);
    var baseUrl = '';

    function WordCountService($q, $http, appConfig) {
        var RandomWordFun = function () {
            var text = "";
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            for (var i = 0; i < 5; i++)
                text += possible.charAt(Math.floor(Math.random() * possible.length));

            return text;
        };

        var SendWords = function () {
            var word = RandomWordFun();
            return $http.post(baseUrl + '/wordcount/AddWord/' + word)
                    .then(function (response) {
                        return response.data;
                    });
        };

        var Count = function () {
            return $http.get(baseUrl + '/wordcount/Count?c=' + Math.random())
                   .then(function (response) {
                       return response.data;
                   });
        };

        return {
            SendWords: SendWords,
            Count: Count,
            RandomWordFun: RandomWordFun
        }
    }

})();

