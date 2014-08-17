var editurl = route.fibre.order.editac;

function EditACCtrl($scope, $http, $filter, $timeout) {

    $scope.files = [];
    $scope.fileLinks = [];

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = false;
        $scope.isCollapsedInstall = true;
        $scope.$on('initModel', function (e, call) {
            $scope.initModel(call);
        });
    }

    $scope.initModel = function (o) {
        $scope.model = o;
        $scope.fileLinks = $scope.model.OrderFiles;

        $scope.changeOrderType();

        if ($scope.model.StatusAC == null)
            $scope.model.StatusAC = model.StatusTypes[0].ID;
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

        var o = $scope.getSubmitData();

        $http.post(route.fibre.order.editac, o).success(function (data) {
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

    $scope.getSubmitData = function () {
        var m = $scope.model;
        var o = {
            ID: m.ID,
            StatusAC: m.StatusAC,
            ReasonRejectAC: m.ReasonRejectAC,
            RemarksAC: m.RemarksAC,
            IsFormReceived: m.IsFormReceived
        };

        return o;
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