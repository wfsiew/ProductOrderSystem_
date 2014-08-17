var editurl = route.fibre.order.editsc;

function EditSCCtrl($scope, $http, $filter, $timeout, $window, $rootScope, uploadManager) {

    $scope.files = [];
    $scope.fileLinks = [];
    $scope.percentage = 0;

    $scope.init = function () {
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = true;
        $scope.$on('initModel', function (e, call) {
            $scope.initModel(call);
        });
    }

    $scope.initModel = function (o) {
        $scope.model = o;
        $scope.fileLinks = $scope.model.OrderFiles;

        if ($scope.model.ReceivedDate != null)
            $scope.model.ReceivedDate = new Date($filter('datefilter')($scope.model.ReceivedDate));

        if ($scope.model.ReceivedTime != null) {
            var date = new Date($filter('datefilter')($scope.model.ReceivedTime));
            $scope.model.ReceivedTime = utils.getTimeStr(date);
        }

        if ($scope.model.BookedInstallDate != null)
            $scope.model.BookedInstallDate = new Date($filter('datefilter')($scope.model.BookedInstallDate));

        if ($scope.model.BookedInstallTime != null) {
            var date = new Date($filter('datefilter')($scope.model.BookedInstallTime));
            $scope.model.BookedInstallTime = utils.getTimeStr(date);
        }

        $scope.changeOrderType();
        $scope.initUpload();
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
        $scope.hideIsServiceUpgrade = true;

        $scope.hideAllocatedFixedLineNo = false;
        $scope.hideRemarksFL = false;

        $scope.hideBTUInstalled = false;
        $scope.hideSIPPort = false;

        if ($scope.model.OrderTypeID == 3) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsWithdrawFixedLineReq = true;
            $scope.hideReasonWithdraw = true;
            $scope.hideIsServiceUpgrade = false;
        }

        else if ($scope.model.OrderTypeID == 4) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsReqFixedLine = true;
            $scope.hideIsCeoApproved = true;
            $scope.hideIsWithdrawFixedLineReq = true;
            $scope.hideReasonWithdraw = true;
            $scope.hideIsServiceUpgrade = true;

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

        $http.post(route.fibre.order.editsc, o).success(function (data) {
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

    // not used
    $scope.variation = function () {
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

        $http.post(route.fibre.order.variation, o).success(function (data) {
            if (data.success == 1) {
                var u = route.order.create + '/' + $scope.model.ID + '/3';
                $window.location.href = u;
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    // not used
    $scope.terminate = function () {
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

        $http.post(route.fibre.order.terminate, o).success(function (data) {
            if (data.success == 1) {
                var u = route.order.create + '/' + $scope.model.ID + '/4';
                $window.location.href = u;
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.withdraw = function () {
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

        $http.post(route.fibre.order.withdraw, o).success(function (data) {
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
        var dt = $scope.model.BookedInstallDate;
        return utils.isValidInstallDate(dt);
    }

    $scope.isValidInstallTime = function () {
        var s = $scope.model.BookedInstallTime;
        return utils.isValidInstallTime(s);
    }

    $scope.getSubmitData = function () {
        var m = $scope.model;
        var o = {
            ID: m.ID,
            SalesPersonID: m.SalesPersonID,
            OrderTypeID: 1,
            StatusSC: 0,
            ReceivedDate: utils.getDateStr(m.ReceivedDate),
            ReceivedTime: m.ReceivedTime,
            CustID: m.CustID,
            CustName: m.CustName,
            CustAddr: m.CustAddr,
            ContactPerson: m.ContactPerson,
            ContactPersonNo: m.ContactPersonNo,
            IsCoverageAvailable: m.IsCoverageAvailable,
            IsDemandList: m.IsDemandList,
            IsReqFixedLine: m.IsReqFixedLine,
            IsCeoApproved: m.IsCeoApproved,
            IsWithdrawFixedLineReq: m.IsWithdrawFixedLineReq,
            IsServiceUpgrade: m.IsServiceUpgrade,
            ReasonWithdraw: m.ReasonWithdraw,
            Comments: m.Comments,
            BookedInstallDate: utils.getDateStr(m.BookedInstallDate),
            BookedInstallTime: m.BookedInstallTime,
            IsKIV: m.IsKIV,
            IsBTUInstalled: m.IsBTUInstalled
        };

        return o;
    }

    $scope.openReceivedDate = function () {
        $timeout(function () {
            $scope.openedReceivedDate = true;
        });
    }

    $scope.openBookedInstallDate = function () {
        $timeout(function () {
            $scope.openedBookedInstallDate = true;
        });
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