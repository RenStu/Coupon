//init called OnInit() in Blazor
// window.localDB 
init = function () {
    db = new PouchDB('Coupon');
    dataSet = null;
    dbChanges = null;
    user = null;
    remoteDB = null;
    sync = null;
    syncing = false;
}

//syncWithRemoteDB
syncWithRemoteDB = function (user, pass) {
    remoteDB = new PouchDB('http://localhost:5984/userdb-' + user.convertToHex(), {
        skipSetup: true,
        auth: {
            username: user,
            password: pass,
        },
        ajax: {
            cache: false,
            timeout: 30000,
            //headers: {
            //    Authorization: 'Basic ' + window.btoa(user + ':' + pass)
            //}
        }
    });

    syncing = true;
    remoteDB.login(user, pass,
        {
            ajax: {
                skipSetup: true,
                cache: false,
                timeout: 30000
            }
        })
        .then(() => {
            sync = db.sync(remoteDB, {
                live: true,
                retry: true,
            })
                .on('denied', (err) => {
                    sync = null;
                    syncing = false;
                    console.log('[Database::syncWithRemoteDB()]: Error (1):' + JSON.stringify(err));
                })
                .on('error', (err) => {
                    sync = null;
                    syncing = false;
                    console.log('[Database::syncWithRemoteDB()]: Error (2):' + JSON.stringify(err));
                })
        });
}


String.prototype.convertToHex = function (delim) {
    return this.split("").map(function (c) {
        return ("0" + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(delim || "");
};

HttpCodes = {
    success: 200,
    notFound: 404
};

//function should called in all afterRender function
userAndSync = function () {
    db.allDocs({
        include_docs: true,
        startkey: 'org.couchdb.user',
        endkey: 'org.couchdb.user\ufff0'
    }).then(function (result) {
        console.log(result);
        if (result.total_rows > 0) {
            user = result.rows[0].doc;
            if (!syncing)
                syncWithRemoteDB(user.email, user.password);

        }
        UpdateComponentsOnLogin();

    }).catch(function (err) {
        console.log(err);
    });
}

function UpdateComponentsOnLogin() {
    try {
        remoteDB.getSession(function (err, response) {
            if (err || !response.userCtx.name) {

                if (user == null) {
                    // nobody's logged in
                    showLogin(true);
                } else {
                    // unconnected user 
                    showLogin(false);
                }
            } else {
                // connected user -> response.userCtx.name  
                if (user != null) {
                    // synchronized user
                    showLogin(false);
                } else {
                    // unsynchronized user
                    showLogin(false);
                }
            }
        });
    } catch (err) {
        showLogin(true);
    }
}

function showLogin(show) {
    if (show) {
        $('a[href="login"]').parent().show();
        $('a[href="javascript:logout()"]').parent().hide();

    } else {
        $('a[href="login"]').parent().hide();
        $('a[href="javascript:logout()"]').parent().show();
    }

    showShopkeeper();
}

function showShopkeeper() {
    if (user == null || user.isShopkeeper) {
        $('#couponOffered').show();
        $('a[href="offers"]').parent().show();
    } else {
        $('#couponOffered').hide();
        $('a[href="offers"]').parent().hide();
    }
}

function logout() {
    remoteDB.logOut(function (err, response) {
        if (err) {
            return;
        }
        db.destroy().then(function (response) {
            dataSet = null;
            dbChanges = null;
            user = null;
            sync = null;
            syncing = false;
        }).catch(function (err) {
            console.log(err);
        });

        UpdateComponentsOnLogin();
    });
    remoteDB = null;
}

$.postJSON = function (url, data) {
    return jQuery.ajax({
        'type': 'POST',
        'url': url,
        'contentType': 'application/json',
        'data': JSON.stringify(data),
        'dataType': 'json'
    });
};

function toHome() {
    $('#indexLink')[0].click();
}


//###########################################################################################################################################################################
//afterRenderIndex called OnAfterRenderAsync() of de page Index in Blazor
afterRenderIndex = function () {
    userAndSync();

    tableOffers = $('#tableOffers').DataTable({
        order: [[1, 'asc']],
        columns: [
            {
                target: 0, title: "Product", width: "10%", "orderable": false, className: "td-vcenter td-no-padding", render: function (data, type, row, meta) {
                    return "<img src='" + row.image + "' style='width:100%; min-width:30px;'>";
                }
            },
            { title: "", data: "name", render: function (data, type, row, meta) { return `${row.name} <br> ${row.shopName} - ${row.offerName}` } },
            { title: "Value", width: "25%", data: "value", render: function (data, type, row, meta) { return `${row.value} <br> ${row.remainingCoupon} of  ${row.amountCoupon} - until ${moment(row.endDate.substr(0, 10), 'YYYY-MM-DD').format('YYYY-MM-DD')}` } },
            {
                target: -1, title: '<span class="oi oi-command" title="Commands"></span>', width: "10%", data: "couponAvailable", createdCell: function (td, cellData, rowData, row, col) {
                    if (rowData.couponAvailable) {
                        $(td).html("<button id ='btnCoupon' title='Get coupon' class='btn btn-primary oi oi-aperture' style='padding-bottom:10px'></button>");
                    } else if (rowData.userCoupon != null) {
                        $(td).html("<button id ='btnCoupon' title='Get coupon' class='btn oi oi-aperture' style='padding-bottom:10px' disabled></button>");
                    } else {
                        $(td).html("");
                    }
                }
            }
        ]
    });

    $('#tableOffers tbody').on('click', '#btnCoupon', function () {
        var data = tableOffers.row($(this).parents('tr')).data();

        var requestCoupon = {
            idOffer: data.offerId,
            guidProduct: data.productGuid,
            userEmail: user.email
        };
        var command = {
            service: "Offers",
            commandName: "Commands.RequestCoupon",
            commandJSON: JSON.stringify(requestCoupon),
            type: "command",
        };

        command._id = sha256(JSON.stringify(command));
        db.get(command._id).then(function (doc) {
            toastr.info("command already sent!");
        }).catch(function (err) {
            if (err.status == HttpCodes.notFound) {
                db.put(command).then(function (response) {
                    console.log(response);
                });
            }
        });
    });

    load();

    function load() {
        db.allDocs({
            include_docs: true
        }).then(function (result) {
            dataSet = result.rows;
            initialize();
        });

        if (dbChanges != null) {
            dbChanges.cancel();
        }

        dbChanges = db.changes({
            since: 'now',
            live: true
        }).on('change', function (change) {
            load();
        }).on('complete', function (info) {
            // changes() was canceled
        });
    }

    function initialize() {

        productsGrid = [];
        dataSet.map((obj) => {
            var doc = obj.doc;
            if (doc.cqrsType == "query" && doc.type == "CreateOffer" && moment() > moment(doc.effectiveEndDate.substr(0, 10), 'YYYY-MM-DD')) {
                var offer = doc;
                offer.listProduct.map((product) => {
                    var productItem = new Object();
                    productItem.offerId = offer._id;
                    productItem.productGuid = product.guid;
                    productItem.shopName = offer.shopName;
                    productItem.image = product.image;
                    productItem.name = product.name;
                    productItem.offerName = offer.name;
                    productItem.isCoupon = product.isCoupon;
                    productItem.value = '$' + product.value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
                    productItem.amountCoupon = product.amountCoupon;
                    productItem.remainingCoupon = product.remainingCoupon;
                    productItem.endDate = offer.effectiveEndDate;
                    productItem.userCoupon = null;
                    product.listUserCoupon.map((userCoupon) => {
                        if (userCoupon.userEmail.toLowerCase() == user.email.toLowerCase())
                            productItem.userCoupon = userCoupon;
                    });
                    productItem.couponAvailable = productItem.isCoupon && productItem.userCoupon == null && productItem.remainingCoupon > 0;
                    productsGrid.push(productItem);
                });
            }
        });
        tableOffers.clear();
        tableOffers.rows.add(productsGrid).draw();

    }
}

//afterRenderOffers called OnAfterRenderAsync() of de page Offers in Blazor
afterRenderOffers = function () {
    userAndSync();

    $('.button-checkbox').each(function () {
        var $widget = $(this),
            $button = $widget.find('button'),
            $checkbox = $widget.find('input:checkbox'),
            color = $button.data('color'),
            settings = {
                on: {
                    icon: 'oi oi-check'
                },
                off: {
                    icon: ''
                }
            };

        $button.on('click', function () {
            $checkbox.prop('checked', !$checkbox.is(':checked'));
            $checkbox.triggerHandler('change');
            updateDisplay();
        });

        $checkbox.on('change', function () {
            updateDisplay();
        });

        function updateDisplay() {
            var isChecked = $checkbox.is(':checked');
            // Set the button's state
            $button.data('state', (isChecked) ? "on" : "off");

            // Set the button's icon
            $button.find('.state-icon')
                .removeClass()
                .addClass('state-icon ' + settings[$button.data('state')].icon);

            // Update the button's color
            if (isChecked) {
                $button
                    .removeClass('btn-outline-info')
                    .addClass('btn-' + color + ' active');
                $(amountCoupon).show();
            }
            else {
                $button
                    .removeClass('btn-' + color + ' active')
                    .addClass('btn-outline-info');
                $(amountCoupon).hide();
            }
        }
        function init() {
            updateDisplay();
            // Inject the icon if applicable
            if ($button.find('.state-icon').length == 0) {
                $button.prepend('<i class="state-icon ' + settings[$button.data('state')].icon + '"></i> ');
            }
        }
        init();
    });

    tableProductList = $('#tableProductList').DataTable({
        order: [[1, 'asc']],
        ordering: false,
        info: false,
        paging: false,
        searching: false,
        columns: [
            {
                target: 0, title: "Product", width: "10%", orderable: false, className: "td-vcenter td-no-padding", render: function (data, type, row, meta) {
                    return "<img src='" + row.image + "' style='width:100%; min-width:30px;'>";
                }
            },
            { title: "", data: "name", render: function (data, type, row, meta) { return `${row.name} <br> ${row.shopName} - ${row.offerName}` } },
            { title: "Value", width: "25%", data: "value", render: function (data, type, row, meta) { return row.value + (row.isCoupon ? '<br> Coupon: ' + row.amountCoupon : '') } },
            {
                target: -1, title: '<span class="oi oi-command" title="Commands"></span>', width: "25%", orderable: false, data: null, createdCell: function (td, cellData, rowData, row, col) {
                    $(td).html("<button id ='btnRemove' title='Remove Product' class='btn btn-danger oi oi-circle-x' style='padding-bottom:10px; margin:3px;'></button>");
                }
            }
        ]
    });

    $(addProduct).click(() => {
        if ($('#productForm').valid()) {
            tableProductList.row.add({
                image: $($("#productImage option:selected")[0]).data("img-src"),
                name: $('#productName').val(),
                value: '$' + parseFloat($('#productValue').val()).toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,"),
                offerName: $('#nameOffer').val(),
                shopName: "Mudar",
                isCoupon: $('#isCoupon').is(':checked'),
                amountCoupon: $('#isCoupon').is(':checked') ? $('#amountCoupon').val() : 0
            }).draw();

            $(productImage).data('picker').destroy();
            $(productImage).find('option').remove()
            $('.image_picker_selector').empty()
            $(productImage).hide();

            $(addProduct).prop('disabled', true);

            if ($('.button-checkbox').find('input:checkbox').is(':checked'))
                $('.button-checkbox').find('button')[0].click();

            $('#productForm')[0].reset();

        }
    });

    $(effectiveStartDate).datepicker({
        format: "yyyy-MM-dd",
        autoHide: true
    });
    $(effectiveEndDate).datepicker({
        format: "yyyy-MM-dd",
        autoHide: true
    });

    function initialize() {

        if (user == null) {
            db.allDocs({
                include_docs: true,
                startkey: 'org.couchdb.user',
                endkey: 'org.couchdb.user\ufff0'
            }).then(function (result) {
                if (result.total_rows > 0)
                    user = result.rows[0];
            });
        }

        $(loader).hide();
        $(addProduct).prop('disabled', false);
        googleSearchImg.listUrlImages.map((imageUrl, index) => {
            $(productImage).append(`<option data-img-src="${imageUrl}" value="${index}" ${index == 0 ? "selected" : ""}>${index}<\/option>`);
        });
        $(productImage).imagepicker();

    }

    load();

    function load() {

        db.get('googleSearchImg').then(function (doc) {
            if (doc.search == $('#productName').val()) {
                googleSearchImg = doc;
                initialize();
            }
        });

        if (dbChanges != null) {
            dbChanges.cancel();
        }

        dbChanges = db.changes({
            since: 'now',
            live: true
        }).on('change', function (change) {
            load();
        }).on('complete', function (info) {
            // changes() was canceled
        });

    }

    $(productName).blur(() => {
        var googleSearch = {
            Search: $('#productName').val()
        };
        var command = {
            service: "PyGoogleImg",
            commandName: "Commands.GoogleSearch",
            commandJSON: JSON.stringify(googleSearch),
            type: "command",
        };

        db.post(command).then(function (response) {
            console.log(response);
        });

        $(addProduct).prop('disabled', true);

        //console.log(command);

        $(loader).show();
    });

    $(saveOffer).click(() => {
        products = tableProductList.rows().data().toArray();
        if ($('#offerForm').valid()) {
            if (products.length > 0) {
                var createOffer = {
                    name: $('#productName').val(),
                    location: user.location,
                    shopName: user.shopName,
                    userShopEmail: user.email,
                    effectiveStartDate: $('#effectiveStartDate').val(),
                    effectiveEndDate: $('#effectiveEndDate').val(),
                    ListProduct: products
                };
                var command = {
                    service: "Offers",
                    commandName: "Commands.CreateOffer",
                    commandJSON: JSON.stringify(createOffer),
                    type: "command",
                };

                command._id = sha256(JSON.stringify(command));
                //console.log(command);

                db.get(command._id).then(function (doc) {
                    toastr.info("command already sent!");
                }).catch(function (err) {
                    if (err.status == HttpCodes.notFound) {
                        db.put(command).then(function (response) {
                            console.log(response);
                        });
                    }
                });
            } else {
                toastr.warning("the list of products offered cannot be empty!");
            }

        }
    });
}

//afterRenderRegister called OnAfterRenderAsync() of de page Register in Blazor
afterRenderRegister = function () {
    userAndSync();

    $('.button-checkbox').each(function () {
        var $widget = $(this),
            $button = $widget.find('button'),
            $checkbox = $widget.find('input:checkbox'),
            color = $button.data('color'),
            settings = {
                on: {
                    icon: 'oi oi-check'
                },
                off: {
                    icon: ''
                }
            };

        $button.on('click', function () {
            $checkbox.prop('checked', !$checkbox.is(':checked'));
            $checkbox.triggerHandler('change');
            updateDisplay();
        });

        $checkbox.on('change', function () {
            updateDisplay();
        });

        function updateDisplay() {
            var isChecked = $checkbox.is(':checked');
            // Set the button's state
            $button.data('state', (isChecked) ? "on" : "off");

            // Set the button's icon
            $button.find('.state-icon')
                .removeClass()
                .addClass('state-icon ' + settings[$button.data('state')].icon);

            // Update the button's color
            if (isChecked) {
                $button
                    .removeClass('btn-outline-info')
                    .addClass('btn-' + color + ' active');
                $(shopName).show();
            }
            else {
                $button
                    .removeClass('btn-' + color + ' active')
                    .addClass('btn-outline-info');
                $(shopName).hide();
            }
        }
        function init() {
            updateDisplay();
            // Inject the icon if applicable
            if ($button.find('.state-icon').length == 0) {
                $button.prepend('<i class="state-icon ' + settings[$button.data('state')].icon + '"></i> ');
            }
        }
        init();
    });

    $('#btnRegister').click((event) => {
        event.preventDefault();
        if ($('form').valid()) {
            var createUser = {
                _id: "org.couchdb.user:" + $('#email').val(),
                name: $('#email').val(),
                email: $('#email').val(),
                password: $('#password').val(),
                location: $('#location').val(),
                roles: [],
                isShopkeeper: $('input:checkbox').is(':checked'),
                shopName: $('#shopName').val()
            };

            var command = {
                service: "Register",
                commandName: "Commands.CreateUser",
                commandJSON: JSON.stringify(createUser),
                type: "command",
            };

            $.postJSON("http://localhost:8712/api/register", { change: command })
                .done(function () {
                    toastr.success("user created");
                    syncWithRemoteDB(createUser.email, createUser.password);
                    toHome();
                })
                .fail(function () {
                    toastr.error("error creating user");
                });
        }
    });
}

//afterRenderCoupon called OnAfterRenderAsync() of de page Coupon in Blazor
afterRenderCoupon = function () {
    userAndSync();

    tableCouponsOffered = $('#tableCouponsOffered').DataTable({
        order: [[1, 'asc']],
        columns: [
            {
                target: 0, title: "Product", width: "10%", orderable: false, className: "td-vcenter td-no-padding", render: function (data, type, row, meta) {
                    return "<img src='" + row.image + "' style='width:100%; min-width:30px;'>";
                }
            },
            { title: "", data: "name", render: function (data, type, row, meta) { return `${row.name} <br> ${row.shopName} - ${row.offerName}` } },
            { title: "User", width: "25%", data: "value", render: function (data, type, row, meta) { return `${row.value} <br> ${row.userEmail} - until ${moment(row.endDate.substr(0, 10), 'YYYY-MM-DD').format('YYYY-MM-DD')}` } },
            {
                target: -1, title: '<span class="oi oi-command" title="Commands"></span>', width: "25%", orderable: false, data: null, createdCell: function (td, cellData, rowData, row, col) {
                    var html = "";
                    if (rowData.userCoupon.inStock == null) {
                        html += ("<button id ='btnInStock' title='In Stock' class='btn btn-primary oi oi-cart' style='padding-bottom:10px; margin:3px;'></button>");
                        html += ("<button id ='btnOutOfStock' title='Out Of Stock' class='btn btn-warning oi oi-link-broken' style='padding-bottom:10px; margin:3px;'></button>");
                    } else if (rowData.userCoupon.inStock) {
                        html += ("<button id ='btnInStock' title='In Stock' class='btn btn-outline-primary oi oi-cart' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    } else {
                        html += ("<button id ='btnOutOfStock' title='Out Of Stock' class='btn btn-outline-warning oi oi-link-broken' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }

                    if (!rowData.userCoupon.isDelivered && rowData.userCoupon.inStock) {
                        html += ("<button id ='btnIsDelivered' title='Is Delivered' class='btn btn-success oi oi-share-boxed' style='padding-bottom:10px; margin:3px;'></button>");
                    } else if (rowData.userCoupon.inStock) {
                        html += ("<button id ='btnIsDelivered' title='Is Delivered' class='btn btn-outline-success oi oi-share-boxed' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }

                    if (!rowData.userCoupon.isCancelled && !rowData.userCoupon.isDelivered && rowData.userCoupon.inStock) {
                        html += ("<button id ='btnIsCancelled' title='In Cancelled' class='btn btn-danger oi oi oi-circle-x' style='padding-bottom:10px; margin:3px;'></button>");
                    } else if (rowData.userCoupon.isCancelled && !rowData.userCoupon.isDelivered) {
                        html += ("<button id ='btnIsCancelled' title='Is Cancelled' class='btn btn-outline-danger oi oi-circle-x' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }
                    $(td).html(html);
                }
            }
        ]
    });

    $('#tableCouponsOffered tbody').on('click', '#btnInStock', function () {
        var data = tableOffers.row($(this).parents('tr')).data();

        var inStock = {
            idOffer: data.offerId,
            guidProduct: data.productGuid,
            userEmail: user.email
        };
        var command = {
            service: "Offers",
            commandName: "Commands.InStock",
            commandJSON: JSON.stringify(inStock),
            type: "command",
        };

        command._id = sha256(JSON.stringify(command));
        db.get(command._id).then(function (doc) {
            toastr.info("command already sent!");
        }).catch(function (err) {
            if (err.status == HttpCodes.notFound) {
                db.put(command).then(function (response) {
                    console.log(response);
                });
            }
        });
    });

    $('#tableCouponsOffered tbody').on('click', '#btnOutOfStock', function () {
        var data = tableOffers.row($(this).parents('tr')).data();

        var finishedStock = {
            idOffer: data.offerId,
            guidProduct: data.productGuid,
            userEmail: user.email
        };
        var command = {
            service: "Offers",
            commandName: "Commands.FinishedStock",
            commandJSON: JSON.stringify(finishedStock),
            type: "command",
        };

        command._id = sha256(JSON.stringify(command));
        db.get(command._id).then(function (doc) {
            toastr.info("command already sent!");
        }).catch(function (err) {
            if (err.status == HttpCodes.notFound) {
                db.put(command).then(function (response) {
                    console.log(response);
                });
            }
        });
    });

    $('#tableCouponsOffered tbody').on('click', '#btnIsDelivered', function () {
        var data = tableOffers.row($(this).parents('tr')).data();

        var delivered = {
            idOffer: data.offerId,
            guidProduct: data.productGuid,
            userEmail: user.email
        };
        var command = {
            service: "Offers",
            commandName: "Commands.Delivered",
            commandJSON: JSON.stringify(delivered),
            type: "command",
        };

        command._id = sha256(JSON.stringify(command));
        db.get(command._id).then(function (doc) {
            toastr.info("command already sent!");
        }).catch(function (err) {
            if (err.status == HttpCodes.notFound) {
                db.put(command).then(function (response) {
                    console.log(response);
                });
            }
        });
    });

    $('#tableCouponsOffered tbody').on('click', '#btnIsCancelled', function () {
        var data = tableOffers.row($(this).parents('tr')).data();

        var cancelled = {
            idOffer: data.offerId,
            guidProduct: data.productGuid,
            userEmail: user.email
        };
        var command = {
            service: "Offers",
            commandName: "Commands.Cancelled",
            commandJSON: JSON.stringify(cancelled),
            type: "command",
        };

        command._id = sha256(JSON.stringify(command));
        db.get(command._id).then(function (doc) {
            toastr.info("command already sent!");
        }).catch(function (err) {
            if (err.status == HttpCodes.notFound) {
                db.put(command).then(function (response) {
                    console.log(response);
                });
            }
        });
    });

    tableMyCoupons = $('#tableMyCoupons').DataTable({
        order: [[1, 'asc']],
        columns: [
            {
                target: 0, title: "Product", width: "10%", orderable: false, className: "td-vcenter td-no-padding", render: function (data, type, row, meta) {
                    return "<img src='" + row.image + "' style='width:100%; min-width:30px;'>";
                }
            },
            { title: "", data: "name", render: function (data, type, row, meta) { return `${row.name} <br> ${row.shopName} - ${row.offerName}` } },
            { title: "Value", width: "25%", data: "value", render: function (data, type, row, meta) { return `${row.value} <br> Until ${moment(row.endDate.substr(0, 10), 'YYYY-MM-DD').format('YYYY-MM-DD')}` } },
            {
                target: -1, title: '<span class="oi oi-command" title="Commands"></span>', width: "25%", orderable: false, data: null, createdCell: function (td, cellData, rowData, row, col) {
                    var html = "";
                    if (rowData.userCoupon.inStock) {
                        html += ("<button id ='btnInStock' title='In Stock' class='btn btn-outline-primary btn-primary oi oi-cart' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    } else if (rowData.userCoupon.inStock == false) {
                        html += ("<button id ='btnOutOfStock' title='Out Of Stock' class='btn btn-outline-warning btn-warning oi oi-link-broken' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }

                    if (rowData.userCoupon.isDelivered) {
                        html += ("<button id ='btnIsDelivered' title='Is Delivered' class='btn btn-outline-success btn-success oi oi-share-boxed' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }

                    if (rowData.userCoupon.isCancelled) {
                        html += ("<button id ='btnIsCancelled' title='Is Cancelled' class='btn  btn-outline-danger btn-danger oi oi oi-circle-x' style='padding-bottom:10px; margin:3px;' disabled></button>");
                    }
                    $(td).html(html);
                }
            }
        ]
    });



    load();

    function load() {
        db.allDocs({
            include_docs: true
        }).then(function (result) {
            dataSet = result.rows;
            initialize();
        });

        if (dbChanges != null) {
            dbChanges.cancel();
        }

        dbChanges = db.changes({
            since: 'now',
            live: true
        }).on('change', function (change) {
            load();
        }).on('complete', function (info) {
            // changes() was canceled
        });
    }

    function initialize() {

        productsGridOffered = [];
        dataSet.map((obj) => {
            var doc = obj.doc;
            if (doc.cqrsType == "query" && doc.type == "CreateOffer") {
                var offer = doc;
                offer.listProduct.map((product) => {
                    if (offer.userShopEmail.toLowerCase() == user.email.toLowerCase()) {
                        product.listUserCoupon.map((userCoupon) => {
                            if (!userCoupon.isOutOfRange) {
                                var productItem = new Object();
                                productItem.offerId = offer._id;
                                productItem.productGuid = product.guid;
                                productItem.shopName = offer.shopName;
                                productItem.image = product.image;
                                productItem.name = product.name;
                                productItem.offerName = offer.name;
                                productItem.isCoupon = product.isCoupon;
                                productItem.value = '$' + product.value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
                                productItem.amountCoupon = product.amountCoupon;
                                productItem.remainingCoupon = product.remainingCoupon;
                                productItem.endDate = offer.effectiveEndDate;
                                productItem.userCoupon = userCoupon;
                                productItem.userEmail = userCoupon.userEmail;
                                productsGridOffered.push(productItem);
                            }
                        });
                    }
                });
            }
        });

        myProductsGrid = [];
        dataSet.map((obj) => {
            var doc = obj.doc;
            if (doc.cqrsType == "query" && doc.type == "CreateOffer") {
                var offer = doc;
                offer.listProduct.map((product) => {
                    product.listUserCoupon.map((userCoupon) => {
                        if (!userCoupon.isOutOfRange && userCoupon.userEmail.toLowerCase() == user.email.toLowerCase()) {
                            var productItem = new Object();
                            productItem.offerId = offer._id;
                            productItem.productGuid = product.guid;
                            productItem.shopName = offer.shopName;
                            productItem.image = product.image;
                            productItem.name = product.name;
                            productItem.offerName = offer.name;
                            productItem.isCoupon = product.isCoupon;
                            productItem.value = '$' + product.value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,");
                            productItem.amountCoupon = product.amountCoupon;
                            productItem.remainingCoupon = product.remainingCoupon;
                            productItem.endDate = offer.effectiveEndDate;
                            productItem.userCoupon = userCoupon;
                            productItem.userEmail = userCoupon.userEmail;
                            myProductsGrid.push(productItem);
                        }
                    });
                });
            }
        });

        tableCouponsOffered.clear();
        tableCouponsOffered.rows.add(productsGridOffered).draw();

        tableMyCoupons.clear();
        tableMyCoupons.rows.add(myProductsGrid).draw();
    }
}

//afterRenderLogin called OnAfterRenderAsync() of de page Login in Blazor
afterRenderLogin = function () {
    $('#btnLogin').click((event) => {
        if ($('form').valid()) {
            syncWithRemoteDB($('#email').val(), $('#password').val());
            UpdateComponentsOnLogin();

            remoteDB.getSession(function (err, response) {
                if (err || !response.userCtx.name) {
                    // nobody's logged in
                    toastr.warning("login failed");
                } else {
                    // logged
                    toastr.success("user logged");
                    toHome();
                }
            });
        }
    });
}