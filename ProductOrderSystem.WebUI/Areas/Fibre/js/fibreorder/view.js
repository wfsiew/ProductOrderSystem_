function ViewCtrl($scope) {

    $scope.files = [];
    $scope.fileLinks = [];

    $scope.init = function () {
        $scope.isCollapsedSC = false;
        $scope.isCollapsedCC = false;
        $scope.isCollapsedFL = false;
        $scope.isCollapsedAC = false;
        $scope.isCollapsedInstall = false;
        $scope.$on('initViewModel', function (e, call) {
            $scope.initModel(call);
        });
    }

    $scope.initModel = function (o) {
        $scope.model = o;
        $scope.fileLinks = $scope.model.OrderFiles;

        $scope.changeOrderType();
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

    $scope.back = function () {
        $scope.$parent.view = false;
    }

    $scope.getCollapseCss = function (x) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        return x ? down : up;
    }
}