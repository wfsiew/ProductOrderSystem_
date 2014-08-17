function DetailsCCCtrl($scope) {

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = false;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = true;
    }

    $scope.initStatusCC = function (a) {
        $scope.StatusCC = a;
    }

    $scope.initReasonRejectCC = function (a) {
        $scope.ReasonRejectCC = a;
    }

    $scope.initRemarksCC = function (a) {
        $scope.RemarksCC = a;
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