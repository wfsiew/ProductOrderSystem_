function SearchOrderCtrl($scope, $http, $timeout, $modal) {

    $scope.currentSort = '';

    $scope.init = function () {
        $scope.initModel();
        $scope.gotoPage(1);
    }

    $scope.initModel = function () {
        $scope.model = {
            DateFrom: null,
            DateTo: null,
            CustName: '',
            OrderID: '',
            CustID: '',
            SalesPersonID: '',
            CustAddr: '',
            Stat: '',
            OrderTypeID: '',
            IsDemandList: false
        };

        $scope.disableSalesPersonID = false;
    }

    $scope.initSalesPersonID = function (a) {
        $scope.model.SalesPersonID = a;
        if (a != '')
            $scope.disableSalesPersonID = true;
    }

    $scope.formSubmit = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var o = {
            Page: page,
            Sort: $scope.currentSort,
            CustName: $scope.model.CustName,
            OrderID: $scope.model.OrderID,
            CustID: $scope.model.CustID,
            SalesPersonID: $scope.model.SalesPersonID,
            CustAddr: $scope.model.CustAddr,
            Status: $scope.model.Stat,
            OrderTypeID: $scope.model.OrderTypeID,
            IsDemandList: $scope.model.IsDemandList
        };

        if ($scope.model.DateFrom != null)
            o.DateFrom = utils.getDateStr($scope.model.DateFrom);

        if ($scope.model.DateTo != null)
            o.DateTo = utils.getDateStr($scope.model.DateTo);

        $http.post(route.fibre.order.search, o).success(function (data) {
            $scope.list = data.list;
            $scope.pager = data.pager;
        });
    }

    $scope.refreshList = function () {
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.viewItem = function (o) {
        var params = {
            id: o.ID
        };

        utils.blockUI();
        $http.get(route.fibre.order.view, { params: params }).success(function (data) {
            if (data.success == 1) {
                $scope.$broadcast('initViewModel', data.model);
                $scope.view = true;
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }

            utils.unblockUI();
        });
    }

    $scope.editItem = function (o) {
        var params = {
            id: o.ID
        };

        utils.blockUI();
        $http.get(editurl, { params: params }).success(function (data) {
            if (data.success == 1) {
                $scope.$broadcast('initModel', data.model);
                $scope.edit = true;
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }

            utils.unblockUI();
        });
    }

    $scope.removeItem = function (o) {
        bootbox.confirm('Are you sure to delete this order ?', function (r) {
            if (r)
                $scope.deleteItem(o);
        });
    }

    $scope.deleteItem = function (o) {
        var x = {
            id: o.ID
        };

        $http.post(route.fibre.order.del, x).success(function (data) {
            if (data.success == 1) {
                toastr.success(data.message);
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        })
    }

    $scope.sort = function (a) {
        if (a == 'OrderID') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'OrderID_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'SalesPerson') {
            if ($scope.currentSort == 'SalesPerson')
                $scope.currentSort = 'SalesPerson_desc';

            else
                $scope.currentSort = 'SalesPerson';
        }

        else if (a == 'OrderType') {
            if ($scope.currentSort == 'OrderType')
                $scope.currentSort = 'OrderType_desc';

            else
                $scope.currentSort = 'OrderType';
        }

        else if (a == 'ReceivedDatetime') {
            if ($scope.currentSort == 'ReceivedDatetime')
                $scope.currentSort = 'ReceivedDatetime_desc';

            else
                $scope.currentSort = 'ReceivedDatetime';
        }

        else if (a == 'InstallDatetime') {
            if ($scope.currentSort == 'InstallDatetime')
                $scope.currentSort = 'InstallDatetime_desc';

            else
                $scope.currentSort = 'InstallDatetime';
        }

        else if (a == 'CustID') {
            if ($scope.currentSort == 'CustID')
                $scope.currentSort = 'CustID_desc';

            else
                $scope.currentSort = 'CustID';
        }

        else if (a == 'CustName') {
            if ($scope.currentSort == 'CustName')
                $scope.currentSort = 'CustName_desc';

            else
                $scope.currentSort = 'CustName';
        }

        else if (a == 'CustAddr') {
            if ($scope.currentSort == 'CustAddr')
                $scope.currentSort = 'CustAddr_desc';

            else
                $scope.currentSort = 'CustAddr';
        }

        else if (a == 'ContactPerson') {
            if ($scope.currentSort == 'ContactPerson')
                $scope.currentSort = 'ContactPerson_desc';

            else
                $scope.currentSort = 'ContactPerson';
        }

        else if (a == 'IsCoverageAvailable') {
            if ($scope.currentSort == 'IsCoverageAvailable')
                $scope.currentSort = 'IsCoverageAvailable_desc';

            else
                $scope.currentSort = 'IsCoverageAvailable';
        }

        else if (a == 'IsDemandList') {
            if ($scope.currentSort == 'IsDemandList')
                $scope.currentSort = 'IsDemandList_desc';

            else
                $scope.currentSort = 'IsDemandList';
        }

        else if (a == 'IsReqFixedLine') {
            if ($scope.currentSort == 'IsReqFixedLine')
                $scope.currentSort = 'IsReqFixedLine_desc';

            else
                $scope.currentSort = 'IsReqFixedLine';
        }

        else if (a == 'IsCeoApproved') {
            if ($scope.currentSort == 'IsCeoApproved')
                $scope.currentSort = 'IsCeoApproved_desc';

            else
                $scope.currentSort = 'IsCeoApproved';
        }

        else if (a == 'IsWithdrawFixedLineReq') {
            if ($scope.currentSort == 'IsWithdrawFixedLineReq')
                $scope.currentSort = 'IsWithdrawFixedLineReq_desc';

            else
                $scope.currentSort = 'IsWithdrawFixedLineReq';
        }

        else if (a == "IsServiceUpgrade") {
            if ($scope.currentSort == 'IsServiceUpgrade')
                $scope.currentSort = 'IsServiceUpgrade_desc';

            else
                $scope.currentSort = 'IsServiceUpgrade';
        }

        else if (a == 'Comments') {
            if (c == 'Comments')
                s = 'Comments_desc';

            else
                s = 'Comments';
        }

        else if (a == 'RemarksCC') {
            if (c == 'RemarksCC')
                s = 'RemarksCC_desc';

            else
                s = 'RemarksCC';
        }

        else if (a == 'RemarksFL') {
            if (c == 'RemarksFL')
                s = 'RemarksFL_desc';

            else
                s = 'RemarksFL';
        }

        else if (a == 'RemarksAC') {
            if (c == 'RemarksAC')
                s = 'RemarksAC_desc';

            else
                s = 'RemarksAC';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'OrderID')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.getCollapseCss = function () {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        return $scope.isCollapsed ? down : up;
    }

    $scope.getRowCss = function (o) {
        var a = o.Status;
        var b = o.IsInstallDatePassed;
        var c = o.IsUrgent;

        var pending = 'info';
        var success = 'success';
        var reject = 'error';
        var warning = 'warning';
        var urgent = 'urgent';

        var css = null;

        if (a == 'Pending')
            css = pending;

        else if (a == 'Success')
            css = success;

        else if (a == 'Reject')
            css = reject;

        if (b == true)
            css = warning;

        if (c == true)
            css = urgent;

        return css;
    }

    $scope.getYesNoIcon = function (o) {
        var i = o == true ? 'icon-ok' : 'icon-remove';
        return i;
    }

    $scope.openDateFrom = function () {
        $timeout(function () {
            $scope.openedDateFrom = true;
        });
    }

    $scope.openDateTo = function () {
        $timeout(function () {
            $scope.openedDateTo = true;
        });
    }

    $scope.init();
}