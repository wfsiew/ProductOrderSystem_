function HomeCtrl($scope, $http, $timeout) {
    $scope.currentSort = 'LastActionDatetime';
    $scope.currentSort1 = 'LastActionDatetime';

    $scope.init = function () {
        $scope.load();
    }

    $scope.load = function () {
        var o = {
            Page: 1,
            Sort: $scope.currentSort1
        };

        $http.post(route.fibre.order.listoverdue, o).then(function (res) {
            var data = res.data;
            $scope.listoverdue = data.list;
            $scope.pager1 = data.pager;

            var k = {
                Page: 1,
                Sort: $scope.currentSort
            };

            return $http.post(route.fibre.order.listpending, k);
        }).then(function (res) {
            var data = res.data;
            $scope.list = data.list;
            $scope.pager = data.pager;
        });
    }

    $scope.gotoPage = function (page) {
        var o = {
            Page: page,
            Sort: $scope.currentSort
        };

        $http.post(route.fibre.order.listpending, o).success(function (data) {
            $scope.list = data.list;
            $scope.pager = data.pager;
        });
    }

    $scope.gotoPage1 = function (page) {
        var o = {
            Page: page,
            Sort: $scope.currentSort1
        };

        $http.post(route.fibre.order.listoverdue, o).success(function (data) {
            $scope.listoverdue = data.list;
            $scope.pager1 = data.pager;
        });
    }

    $scope.refreshList = function () {
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.refreshList1 = function () {
        $scope.gotoPage1($scope.pager1.PageNum);
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
                data.model['urgent'] = 0;
                $scope.$broadcast('initModel', data.model);
                $scope.edit = true;
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }

            utils.unblockUI();
        });
    }

    $scope.editItem1 = function (o) {
        var params = {
            id: o.ID
        };

        utils.blockUI();
        $http.get(editurl, { params: params }).success(function (data) {
            if (data.success == 1) {
                data.model['urgent'] = 1;
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

    $scope.removeItem1 = function (o) {
        bootbox.confirm('Are you sure to delete this order ?', function (r) {
            if (r)
                $scope.deleteItem1(o);
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

    $scope.deleteItem1 = function (o) {
        var x = {
            id: o.ID
        };

        $http.post(route.fibre.order.del, x).success(function (data) {
            if (data.success == 1) {
                toastr.success(data.message);
                $scope.gotoPage1($scope.pager1.PageNum);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        })
    }

    $scope.sort = function (a) {
        var c = $scope.currentSort;
        $scope.currentSort = $scope.getSort(a, c);
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.sort1 = function (a) {
        var c = $scope.currentSort1;
        $scope.currentSort1 = $scope.getSort(a, c);
        $scope.gotoPage1($scope.pager1.PageNum);
    }

    $scope.getSort = function (a, c) {
        var s = c;

        if (a == 'OrderID') {
            if (c == null || c == 'OrderID')
                s = 'OrderID_desc';

            else
                s = 'OrderID';
        }

        else if (a == 'SalesPerson') {
            if (c == 'SalesPerson')
                s = 'SalesPerson_desc';

            else
                s = 'SalesPerson';
        }

        else if (a == 'OrderType') {
            if (c == 'OrderType')
                s = 'OrderType_desc';

            else
                s = 'OrderType';
        }

        else if (a == 'ReceivedDatetime') {
            if (c == 'ReceivedDatetime')
                s = 'ReceivedDatetime_desc';

            else
                s = 'ReceivedDatetime';
        }

        else if (a == 'InstallDatetime') {
            if (c == 'InstallDatetime')
                s = 'InstallDatetime_desc';

            else
                s = 'InstallDatetime';
        }

        else if (a == 'CustID') {
            if (c == 'CustID')
                s = 'CustID_desc';

            else
                s = 'CustID';
        }

        else if (a == 'CustName') {
            if (c == 'CustName')
                s = 'CustName_desc';

            else
                s = 'CustName';
        }

        else if (a == 'CustAddr') {
            if (c == 'CustAddr')
                s = 'CustAddr_desc';

            else
                s = 'CustAddr';
        }

        else if (a == 'ContactPerson') {
            if (c == 'ContactPerson')
                s = 'ContactPerson_desc';

            else
                s = 'ContactPerson';
        }

        else if (a == 'IsCoverageAvailable') {
            if (c == 'IsCoverageAvailable')
                s = 'IsCoverageAvailable_desc';

            else
                s = 'IsCoverageAvailable';
        }

        else if (a == 'IsDemandList') {
            if (c == 'IsDemandList')
                s = 'IsDemandList_desc';

            else
                s = 'IsDemandList';
        }

        else if (a == 'IsReqFixedLine') {
            if (c == 'IsReqFixedLine')
                s = 'IsReqFixedLine_desc';

            else
                s = 'IsReqFixedLine';
        }

        else if (a == 'IsCeoApproved') {
            if (c == 'IsCeoApproved')
                s = 'IsCeoApproved_desc';

            else
                s = 'IsCeoApproved';
        }

        else if (a == 'IsWithdrawFixedLineReq') {
            if (c == 'IsWithdrawFixedLineReq')
                s = 'IsWithdrawFixedLineReq_desc';

            else
                s = 'IsWithdrawFixedLineReq';
        }

        else if (a == 'IsServiceUpgrade') {
            if (c == 'IsServiceUpgrade')
                s = 'IsServiceUpgrade_desc';

            else
                s = 'IsServiceUpgrade';
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

        return s;
    }

    $scope.getSortCss = function (a) {
        var c = $scope.currentSort;
        return $scope.getSortCss_(a, c);
    }

    $scope.getSortCss1 = function (a) {
        var c = $scope.currentSort1;
        return $scope.getSortCss_(a, c);
    }

    $scope.getSortCss_ = function (a, c) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        if ((c == null || c == '') && a == 'OrderID')
            return up;

        if (c.indexOf(a) == 0) {
            if (c.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.getRowCss = function (o) {
        var a = o.Status;
        var b = o.IsInstallDatePassed;

        var pending = 'info';
        var success = 'success';
        var reject = 'error';
        var warning = 'warning';

        var css = null;

        if (a == 'Pending')
            css = pending;

        else if (a == 'Success')
            css = success;

        else if (a == 'Reject')
            css = reject;

        if (b == true)
            css = warning;

        return css;
    }

    $scope.getRowCss1 = function (o) {
        return 'urgent';
    }

    $scope.getYesNoIcon = function (o) {
        var i = o == true ? 'icon-ok' : 'icon-remove';
        return i;
    }

    $scope.init();
}