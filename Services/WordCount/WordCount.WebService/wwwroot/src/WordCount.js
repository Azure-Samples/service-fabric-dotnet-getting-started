(function(){
  'use strict';

  var wordCountApp = angular
            .module('wordCountApp', ['ngRoute', 'ngMaterial', 'ngMdIcons', 'googlechart']).constant(
                "appConfig", {
                    asfAPIUrl: '/wordcount'
                })
            .config(function ($mdThemingProvider, $mdIconProvider) {

                $mdIconProvider
                    .defaultIconSet("./assets/svg/avatars.svg", 128)
                    .icon("menu", "./assets/svg/menu.svg", 24)
                    .icon("share", "./assets/svg/share.svg", 24)
                    .icon("google_plus", "./assets/svg/google_plus.svg", 512)
                    .icon("hangouts", "./assets/svg/hangouts.svg", 512)
                    .icon("twitter", "./assets/svg/twitter.svg", 512)
                    .icon("phone", "./assets/svg/phone.svg", 512);

                $mdThemingProvider.theme('docs-dark', 'default')
                    .primaryPalette('light-blue')
                        .accentPalette('red');
            });
})();
