function DetailsFLCtrl($scope) {

    $scope.init = function () {
        $scope.isCollapsedSC = true;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = false;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = true;
    }

    $scope.initStatusFL = function (a) {
        $scope.StatusFL = a;
    }

    $scope.initReasonRejectFL = function (a) {
        $scope.ReasonRejectFL = a;
    }

    $scope.initAllocatedFixedLineNo = function (a) {
        $scope.AllocatedFixedLineNo = a;
    }

    $scope.initRemarksFL = function (a) {
        $scope.RemarksFL = a;
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
}