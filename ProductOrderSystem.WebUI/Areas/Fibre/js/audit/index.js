function AuditCtrl($scope, $http, $timeout) {

    $scope.init = function () {
        $scope.initModel();
    }

    $scope.initModel = function () {
        $scope.model = {
            DateFrom: null,
            DateTo: null
        };
    }

    $scope.formSubmit = function () {
        var dateFrom = $scope.model.DateFrom;
        var dateTo = $scope.model.DateTo;

        if (dateFrom == null || dateTo == null) {
            bootbox.alert('Date From and Date To are required');
            return;
        }

        var _dateFrom = utils.getDateStr(dateFrom);
        var _dateTo = utils.getDateStr(dateTo);

        var a = [
            'from=' + _dateFrom,
            'to=' + _dateTo
        ].join('&');
        var q = '?' + a;

        var url = route.audit.runexport + '/' + q;

        $('#exportFrame').attr('src', url);
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