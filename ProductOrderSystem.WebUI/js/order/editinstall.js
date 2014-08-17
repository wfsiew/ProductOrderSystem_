var editurl = route.order.editinstall;

function EditInstallCtrl($scope, $http, $filter, $timeout, $window, $rootScope, uploadManager) {

    $scope.files = [];
    $scope.fileLinks = [];
    $scope.percentage = 0;

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = false;
        $scope.$on('initModel', function (e, call) {
            $scope.initModel(call);
        });
    }

    $scope.initModel = function (o) {
        $scope.model = o;
        $scope.fileLinks = $scope.model.OrderFiles;

        $scope.changeOrderType();
        $scope.initUpload();

        if ($scope.model.StatusInstall == null)
            $scope.model.StatusInstall = model.StatusTypes[0].ID;

        if ($scope.model.InstallDate != null)
            $scope.model.InstallDate = new Date($filter('datefilter')($scope.model.InstallDate));

        if ($scope.model.InstallTime != null) {
            var date = new Date($filter('datefilter')($scope.model.InstallTime));
            $scope.model.InstallTime = utils.getTimeStr(date);
        }
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

    $scope.changeOrderType = function () {
        $scope.hideIsCoverageAvailable = false;
        $scope.hideIsDemandList = false;
        $scope.hideIsReqFixedLine = false;
        $scope.hideIsCeoApproved = false;
        $scope.hideIsWithdrawFixedLineReq = false;
        $scope.hideReasonWithdraw = false;

        $scope.hideAllocatedFixedLineNo = false;
        $scope.hideRemarksFL = false;

        $scope.hideBTUInstalled = false;
        $scope.hideSIPPort = false;

        if ($scope.model.OrderTypeID == 3) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsWithdrawFixedLineReq = true;
            $scope.hideReasonWithdraw = true;
        }

        else if ($scope.model.OrderTypeID == 4) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsReqFixedLine = true;
            $scope.hideIsCeoApproved = true;
            $scope.hideIsWithdrawFixedLineReq = true;
            $scope.hideReasonWithdraw = true;

            $scope.hideAllocatedFixedLineNo = true;
            $scope.hideRemarksFL = true;

            $scope.hideBTUInstalled = true;
            $scope.hideSIPPort = true;
        }
    }

    $scope.save = function () {
        var valid = $scope.form.$valid;

        if (!valid) {
            toastr.error('Form is not valid');
            return;
        }

        valid = $scope.isValidInstallDate();

        if (!valid) {
            toastr.error('Form is not valid');
            return;
        }

        valid = $scope.isValidInstallTime();

        if (!valid) {
            toastr.error('Form is not valid');
            return;
        }

        var o = $scope.getSubmitData();

        $http.post(route.order.editinstall, o).success(function (data) {
            if (data.success == 1) {
                toastr.success(data.message);
                $scope.$parent.edit = false;
                if ($scope.model.urgent == 0)
                    $scope.$parent.refreshList();

                else
                    $scope.$parent.refreshList1();
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.isValidInstallDate = function () {
        var dt = $scope.model.InstallDate;
        return utils.isValidInstallDate(dt);
    }

    $scope.isValidInstallTime = function () {
        var s = $scope.model.InstallTime;
        return utils.isValidInstallTime(s);
    }

    $scope.getSubmitData = function () {
        var m = $scope.model;
        var o = {
            ID: m.ID,
            StatusInstall: m.StatusInstall,
            ReasonRejectInstall: m.ReasonRejectInstall,
            InstallDate: utils.getDateStr(m.InstallDate),
            InstallTime: m.InstallTime,
            BTUID: m.BTUID,
            BTUInstalled: m.BTUInstalled,
            SIPPort: m.SIPPort
        };

        return o;
    }

    $scope.openInstallDate = function () {
        $timeout(function () {
            $scope.openedInstallDate = true;
        });
    }

    $scope.uploadOption = {
        url: route.order.uploadfile
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

        $http.post(route.order.removefile, o).success(function (data) {
            if (data.success == 1) {
                var i = _.indexOf($scope.fileLinks, file);
                $scope.fileLinks.splice(i, 1);
                toastr.success(data.message);
                $scope.$apply();
            }
        });
    }

    $scope.cancelEdit = function () {
        $scope.$parent.edit = false;
    }

    $scope.getCollapseCss = function (x) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        return x ? down : up;
    }

    $scope.init();
}