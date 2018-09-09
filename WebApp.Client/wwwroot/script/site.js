//init called OnInit() in Blazor
// window.localDB 
init = function () {
    localDB = new PouchDB('localDB-Dev');

    localDB.allDocs({
        include_docs: true,
        startkey: 'user',
        endkey: 'user\ufff0'
    }).then(function (result) {
        console.log(result);
        if (result.total_rows > 0)
            syncWithRemoteDB(result.rows[0].doc.user, result.rows[0].doc.pass);
    }).catch(function (err) {
        console.log(err);
    });
}

//syncWithRemoteDB called init()
// window.remoteDB 
// window.sync
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

    var error = false;
    remoteDB.login(user, pass,
    {
        ajax: {
            cache: false,
            timeout: 30000
        }
    })
    .then(() => {
        sync = localDB.sync(remoteDB, {
            live: true,
            retry: true,
        })
        .on('denied', (err) => {
            error = true;
            console.log('[Database::syncWithRemoteDB()]: Error (1):' + JSON.stringify(err));
        })
        .on('error', (err) => {
            error = true;
            console.log('[Database::syncWithRemoteDB()]: Error (2):' + JSON.stringify(err));
        })
    });

    if (error)
        sync = null;
}

//afterRenderProductsOnOffer called OnAfterRenderAsync() of de page ProductsOnOffer in Blazor
// window.changes
afterRenderProductsOnOffer = function () {

    //check if (sync) else toastrShow info

    var error = false;
    changes = this.localDB.changes({
        since: 'now',
        live: true,
        include_docs: true
    })
    .on('change', function (change) {

    })
    .on('denied', (err) => {
        error = true;
        console.log('[Database::afterRenderProductsOnOffer()]: Error (1):' + JSON.stringify(err));
    })
    .on('error', (err) => {
        error = true;
        console.log('[Database::afterRenderProductsOnOffer()]: Error (2):' + JSON.stringify(err));
    });

    if (error)
        changes = null;

    //changes.cancel(); 
}

String.prototype.convertToHex = function (delim) {
    return this.split("").map(function (c) {
        return ("0" + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(delim || "");
};