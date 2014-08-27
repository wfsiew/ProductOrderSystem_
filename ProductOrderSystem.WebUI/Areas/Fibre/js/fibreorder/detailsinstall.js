function DetailsInstallCtrl($scope, $http, $timeout, $window, $rootScope, uploadManager) {

    $scope.files = [];
    $scope.fileLinks = [];
    $scope.percentage = 0;

    $scope.model = {
        ID: 0
    };

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = false;

        $scope.initUpload();
    }

    $scope.initID = function (a) {
        $scope.model.ID = a;
    }

    $scope.initStatusInstall = function (a) {
        $scope.StatusInstall = a;
    }

    $scope.initReasonRejectInstall = function (a) {
        $scope.ReasonRejectInstall = a;
    }

    $scope.initInstallDate = function (a) {
        if (a != '')
            $scope.InstallDate = new Date(a);
    }

    $scope.initInstallTime = function (a) {
        if (a != 'N/A')
            $scope.InstallTime = a;
    }

    $scope.initBTUID = function (a) {
        $scope.BTUID = a;
    }

    $scope.initBTUInstalled = function (a) {
        $scope.BTUInstalled = a;
    }

    $scope.initSIPPort = function (a) {
        $scope.SIPPort = a;
    }

    $scope.initOrderFiles = function (a) {
        $scope.fileLinks = files;
    }

    $scope.initUpload = function () {
        $rootScope.$on('fileAdded', function (e, call) {
            $scope.files.push(call);
            $scope.$apply();
        });

        $rootScope.$on('uploadProgress', function (e, call) {
            $scope.percentage = call;
            $scope.$apply();
        });
    }

    $scope.openInstallDate = function () {
        $timeout(function () {
            $scope.openedInstallDate = true;
        });
    }

    $scope.isValidInstallDate = function () {
        var dt = $scope.InstallDate;
        return utils.isValidInstallDate(dt);
    }

    $scope.isValidInstallTime = function () {
        var s = $scope.InstallTime;
        return utils.isValidInstallTime(s);
    }

    $scope.save = function () {
        var valid = $scope.form.$valid;

        if (!valid)
            return;

        valid = $scope.isValidInstallDate();

        if (!valid)
            return;

        valid = $scope.isValidInstallTime();

        if (!valid)
            return;

        $('#form').submit();
    }

    $scope.uploadOption = {
        url: route.fibre.order.uploadfile
    }

    $scope.uploadFormData = function () {
        return { id: $scope.model.ID };
    }

    $scope.upload = function () {
        uploadManager.upload();
    }

    $scope.uploadDone = function (e, data) {
        var result = data.result;

        if (result.error == 1) {
            toastr.error(result.message);
        }

        else if (result.success == 1) {
            var t = _.findWhere($scope.fileLinks, { fileID: result.fileID });
            if (t == null) {
                $scope.fileLinks.push({ filename: result.filename, url: result.url, fileID: result.fileID });
            }

            $scope.files = [];
            uploadManager.clear();
            $scope.$apply();
        }
    }

    $scope.removeFile = function (file) {
        var i = _.indexOf($scope.files, file);
        $scope.files.splice(i, 1);
        $scope.$apply();
    }

    $scope.deleteFile = function (file) {
        var o = {
            id: file.fileID,
            filename: file.filename
        };

        $http.post(route.fibre.order.removefile, o).success(function (data) {
            if (data.success == 1) {
                var i = _.indexOf($scope.fileLinks, file);
                $scope.fileLinks.splice(i, 1);
                toastr.success(data.message);
                $scope.$apply();
            }
        });
    }

    $scope.init();
}