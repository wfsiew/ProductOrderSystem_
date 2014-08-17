function DetailsACCtrl($scope) {

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = false;
        $scope.isCollapsedInstall = true;
    }

    $scope.initStatusAC = function (a) {
        $scope.StatusAC = a;
    }

    $scope.initReasonRejectAC = function (a) {
        $scope.ReasonRejectAC = a;
    }

    $scope.initRemarksAC = function (a) {
        $scope.RemarksAC = a;
    }

    $scope.initIsFormReceived = function (a) {
        $scope.IsFormReceived = a == 1 ? true : false;
    }

    $scope.getCollapseCss = function (x) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        return x ? down : up;
    }

    $scope.save = function () {
        var valid = $scope.form.$valid;

        if (!valid)
            return;

        $('#form').submit();
    }

    $scope.init();
}