function UserCtrl($scope, $http, $modal) {
    'use strict';

    $scope.currentSort = '';

    $scope.init = function () {
        $scope.gotoPage(1);
    }

    $scope.search = function () {
        $scope.gotoPage(1);
    }

    $scope.gotoPage = function (page) {
        var params = {
            SearchString: ($scope.SearchString == '' ? null : $scope.SearchString),
            sortOrder: $scope.currentSort,
            page: page
        };

        $http.get(route.user.search, { params: params }).success(function (data) {
            if (data.error == 1) {
                toastr.error(data.message);
                $scope.pager = null;
                $scope.model = null;
            }

            else {
                $scope.pager = data.pager;
                $scope.model = data.model;
            }
        });
    }

    $scope.sort = function (a) {
        if (a == 'Name') {
            if ($scope.currentSort == null || $scope.currentSort == '')
                $scope.currentSort = 'Name_desc';

            else
                $scope.currentSort = '';
        }

        else if (a == 'Email') {
            if ($scope.currentSort == 'Email')
                $scope.currentSort = 'Email_desc';

            else
                $scope.currentSort = 'Email';
        }

        else if (a == 'PhoneNo') {
            if ($scope.currentSort == 'PhoneNo')
                $scope.currentSort = 'PhoneNo_desc';

            else
                $scope.currentSort = 'PhoneNo';
        }

        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.getSortCss = function (a) {
        var up = 'icon-chevron-up';
        var down = 'icon-chevron-down';

        if (($scope.currentSort == null || $scope.currentSort == '') && a == 'Name')
            return up;

        if ($scope.currentSort.indexOf(a) == 0) {
            if ($scope.currentSort.indexOf('desc') > 0)
                return down;

            else
                return up;
        }

        return null;
    }

    $scope.addNew = function () {
        var mi = $modal.open({
            templateUrl: route.user.form,
            controller: UserCreateCtrl,
            resolve: {
                items: function () {
                    return {};
                }
            }
        });

        mi.result.then(function (x) {
            $scope.saveCreate(x);
        }, function () {
        });
    }

    $scope.editItem = function (o) {
        utils.blockUI();
        $http.get(route.user.edit + '/' + o.ID).success(function (data) {
            if (data.error == 1) {
                toastr.error(data.message);
            }

            else {
                $scope.showEdit(data);
            }

            utils.unblockUI();
        });
    }

    $scope.showEdit = function (o) {
        var mi = $modal.open({
            templateUrl: route.user.form,
            controller: UserEditCtrl,
            resolve: {
                items: function () {
                    return o;
                }
            }
        });

        mi.result.then(function (x) {
            $scope.saveEdit(x);
        }, function () {
        });
    }

    $scope.removeItem = function (o) {
        bootbox.confirm('Are you sure to delete this user ?', function (r) {
            if (r)
                $scope.deleteItem(o);
        });
    }

    $scope.saveCreate = function (o) {
        toastr.success(o.message);
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.saveEdit = function (o) {
        toastr.success(o.message);
        $scope.gotoPage($scope.pager.PageNum);
    }

    $scope.deleteItem = function (o) {
        $http.post(route.user.del, { id: o.ID }).success(function (data) {
            if (data.success == 1) {
                toastr.success(data.message);
                $scope.gotoPage($scope.pager.PageNum);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.init();
}

function UserCreateCtrl($scope, $http, $modalInstance, items) {
    'use strict';

    $scope.title = 'Create User';

    $scope.save = function () {
        var _selectedRoles = _.where($scope.Roles, { Assigned: true });
        var selectedRoles = _.map(_selectedRoles, function (o) {
            return o.RoleID;
        });

        var o = {
            Email: $scope.model.Email,
            PhoneNo: $scope.model.PhoneNo,
            selectedRoles: selectedRoles
        };

        $http.post(route.user.create, o).success(function (data) {
            if (data.success == 1) {
                items = data;
                $modalInstance.close(items);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }

    $scope.dismissAlert = function () {
        $scope.success = false;
        $scope.error = false;
    }

    $scope.model = {};

    $http.get(route.user.roles).success(function (data) {
        $scope.Roles = data;
    });

    $http.get(route.user.unregisteredUsers).success(function (data) {
        $scope.Emails = data;
    });
}

function UserEditCtrl($scope, $http, $modalInstance, items) {
    'use strict';

    $scope.title = 'Edit User';

    $scope.save = function () {
        var _selectedRoles = _.where($scope.Roles, { Assigned: true });
        var selectedRoles = _.map(_selectedRoles, function (o) {
            return o.RoleID;
        });

        var o = {
            ID: $scope.model.ID,
            Email: $scope.model.Email,
            Name: $scope.model.Name,
            PhoneNo: $scope.model.PhoneNo,
            selectedRoles: selectedRoles,
            Roles: selectedRoles
        };

        $http.post(route.user.edit, o).success(function (data) {
            if (data.success == 1) {
                items = data;
                $modalInstance.close(data);
            }

            else if (data.error == 1) {
                toastr.error(data.message);
            }
        });
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }

    $scope.model = items;
    $scope.Roles = items.Roles;
    $scope.Emails = items.Emails;
}