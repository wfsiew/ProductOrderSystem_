function DetailsCtrl($scope) {

    $scope.init = function () {
        $scope.isCollapsedSC = false;
        $scope.isCollapsedCC = true;
        $scope.isCollapsedFL = true;
        $scope.isCollapsedAC = true;
        $scope.isCollapsedInstall = true;
    }

    $scope.getCollapseCss = function (x) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        return x ? down : up;
    }
}