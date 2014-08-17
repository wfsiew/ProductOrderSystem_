'use strict';

/* http://cgeers.com/2013/05/03/angularjs-file-upload/ */
app.factory('uploadManager', function ($rootScope) {
    var _files = [];
    return {
        add: function (file) {
            _files.push(file);
            $rootScope.$broadcast('fileAdded', file.files[0].name);
        },
        clear: function () {
            _files = [];
        },
        files: function () {
            var fileNames = [];
            $.each(_files, function (index, file) {
                fileNames.push(file.files[0].name);
            });
            return fileNames;
        },
        upload: function () {
            $.each(_files, function (index, file) {
                file.submit();
            });
            this.clear();
        },
        setProgress: function (percentage) {
            $rootScope.$broadcast('uploadProgress', percentage);
        }
    };
});

app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('noCacheInterceptor');
}])
.factory('noCacheInterceptor', function () {
    return {
        request: function (config) {
            if (config.method == 'GET' && (config.url.indexOf('ngview/') < 0 && config.url.indexOf('.html') < 0)) {
                var sep = config.url.indexOf('?') === -1 ? '?' : '&';
                config.url = config.url + sep + 'noCache=' + new Date().getTime();
            }

            return config;
        }
    };
});