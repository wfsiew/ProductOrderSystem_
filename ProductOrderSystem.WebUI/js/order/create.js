function CreateOrderCtrl($scope, $http, $timeout, $rootScope, uploadManager) {

    $scope.init = function () {
        $scope.hideIsWithdrawFixedLineReq = true;
        $scope.hideReasonWithdraw = true;
        $scope.hideIsServiceUpgrade = true;

        $scope.initModel();
        $scope.initUpload();
        //$scope.loadSalesPersons();
    }

    $scope.initUpload = function () {
        $rootScope.$on('fileAdded', function (e, call) {
            $scope.model.files.push(call);
            $scope.$apply();
        });

        $rootScope.$on('uploadProgress', function (e, call) {
            $scope.percentage = call;
            $scope.$apply();
        });
    }

    $scope.initModel = function () {
        $scope.model = {
            SalesPersonID: '',
            OrderTypeID: 1,
            ReceivedDate: null,
            ReceivedTime: '',
            CustName: '',
            CustAddr: '',
            CustID: '',
            ContactPerson: '',
            ContactPersonNo: '',
            IsCoverageAvailable: false,
            IsDemandList: false,
            IsReqFixedLine: false,
            IsCeoApproved: false,
            IsWithdrawFixedLineReq: false,
            IsServiceUpgrade: false,
            ReasonWithdraw: '',
            Comments: '',
            BookedInstallDate: null,
            BookedInstallTime: '',
            IsKIV: false,
            IsBTUInstalled: false,
            files: [],
            fileLinks: [],
            percentage: 0,
            ID: 0,
            saveCallback: null,
            uploadCnt: 0
        };
    }

    $scope.initSalesPersonID = function (a) {
        if (a != 0)
            $scope.model.SalesPersonID = a;
    }

    $scope.initOrderTypeID = function (a) {
        if (a != 0) {
            $scope.model.OrderTypeID = a;
            $scope.changeOrderType();
        }
    }

    $scope.initCustName = function (a) {
        if (a != null)
            $scope.model.CustName = a;
    }

    $scope.initCustAddr = function (a) {
        if (a != null)
            $scope.model.CustAddr = a;
    }

    $scope.initCustID = function (a) {
        if (a != null)
            $scope.model.CustID = a;
    }

    $scope.initContactPerson = function (a) {
        if (a != null)
            $scope.model.ContactPerson = a;
    }

    $scope.initContactPersonNo = function (a) {
        if (a != null)
            $scope.model.ContactPersonNo = a;
    }

    $scope.changeOrderType = function () {
        $scope.hideIsCoverageAvailable = false;
        $scope.hideIsDemandList = false;
        $scope.hideIsReqFixedLine = false;
        $scope.hideIsCeoApproved = false;
        $scope.hideIsServiceUpgrade = true;
        //$scope.hideIsWithdrawFixedLineReq = false;
        //$scope.hideReasonWithdraw = false;

        $scope.hideAllocatedFixedLineNo = false;
        $scope.hideRemarksFL = false;

        $scope.hideBTUInstalled = false;
        $scope.hideSIPPort = false;

        if ($scope.model.OrderTypeID == 3) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsServiceUpgrade = false;
            //$scope.hideIsWithdrawFixedLineReq = true;
            //$scope.hideReasonWithdraw = true;
        }

        else if ($scope.model.OrderTypeID == 4) {
            $scope.hideIsCoverageAvailable = true;
            $scope.hideIsDemandList = true;
            $scope.hideIsReqFixedLine = true;
            $scope.hideIsCeoApproved = true;
            $scope.hideIsServiceUpgrade = true;
            //$scope.hideIsWithdrawFixedLineReq = true;
            //$scope.hideReasonWithdraw = true;

            $scope.hideAllocatedFixedLineNo = true;
            $scope.hideRemarksFL = true;

            $scope.hideBTUInstalled = true;
            $scope.hideSIPPort = true;
        }
    }

    $scope.formSubmit = function () {
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
        utils.blockUI();

        $http.post(route.order.create, o).success(function (data) {
            if (data.success == 1) {
                $scope.model.saveCallback = data;
                $scope.model.ID = data.orderid;
                if ($scope.model.files.length > 0)
                    uploadManager.upload();

                else {
                    toastr.success(data.message);
                    $scope.initModel();
                    utils.unblockUI();
                }
            }

            else if (data.error == 1) {
                utils.unblockUI();
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
            SalesPersonID: m.SalesPersonID,
            OrderTypeID: m.OrderTypeID,
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

    $scope.loadSalesPersons = function () {
        $http.get(route.order.salespersons).success(function (data) {
            $scope.SalesPersons = data;
        });
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
        url: route.order.uploadfile
    }

    $scope.uploadFormData = function () {
        return { id: $scope.model.ID };
    }

    $scope.removeFile = function (file) {
        var i = _.indexOf($scope.model.files, file);
        $scope.model.files.splice(i, 1);
        $scope.$apply();
    }

    $scope.uploadDone = function (e, data) {
        var result = data.result;

        if (result.success == 1) {
            ++$scope.model.uploadCnt;
            if ($scope.model.uploadCnt == $scope.model.files.length) {
                toastr.success($scope.model.saveCallback.message);
                $scope.initModel();
                uploadManager.clear();
                $scope.$apply();
                utils.unblockUI();
            }
        }

        else if (result.error == 1) {
            utils.unblockUI();
            toastr.error(result.message);
        }
    }

    $scope.init();
}