﻿var route = (function () {

    var user = {
        form: utils.getUrl('/ngview/user/form.html'),
        search: utils.getUrl('/User/Search'),
        confirmdelete: utils.getUrl('/ngview/user/confirm_delete.html'),
        create: utils.getUrl('/User/Create'),
        edit: utils.getUrl('/User/Edit'),
        del: utils.getUrl('/User/Delete'),
        roles: utils.getUrl('/User/Roles'),
        unregisteredUsers: utils.getUrl('/User/UnregisteredGoogleUsers')
    };

    var fibre = {
        order: {
            create: utils.getUrl('/Fibre/Order/Create'),
            uploadfile: utils.getUrl('/Fibre/Order/UploadFile'),
            removefile: utils.getUrl('/Fibre/Order/RemoveFile'),
            search: utils.getUrl('/Fibre/Order/Search'),
            list: utils.getUrl('/Fibre/Order/List'),
            listoverdue: utils.getUrl('/Fibre/Order/ListOverdue'),
            listpending: utils.getUrl('/Fibre/Order/ListPending'),
            del: utils.getUrl('/Fibre/Order/Delete'),
            view: utils.getUrl('/Fibre/Order/View'),
            editsc: utils.getUrl('/Fibre/Order/EditSC'),
            editcc: utils.getUrl('/Fibre/Order/EditCC'),
            editfl: utils.getUrl('/Fibre/Order/EditFL'),
            editac: utils.getUrl('/Fibre/Order/EditAC'),
            editinstall: utils.getUrl('/Fibre/Order/EditInstall'),
            withdraw: utils.getUrl('/Fibre/Order/Withdraw'),
            terminate: utils.getUrl('/Fibre/Order/Terminate'),
            variation: utils.getUrl('/Fibre/Order/Variation'),
            salespersons: utils.getUrl('/Fibre/Order/SalesPersons')
        },
        audit: {
            runexport: utils.getUrl('/Fibre/Audit/Export')
        }
    };

    var audit = {
        runexport: utils.getUrl('/Audit/Export')
    };

    return {
        user: user,
        fibre: fibre
    };
}());