/* This function calls the StatelessBackendController's HTTP GET method to get the current count from the StatelessBackendService */
function getStatelessBackendCount() {
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                returnData = JSON.parse(http.responseText);
                if (returnData) {
                    countDisplay.innerHTML = returnData.count;
                    updateFooter(http, (end - start));
                }
            } else {
                updateFooter(http, (end - start));
            }
        }
    };
    start = new Date().getTime();
    http.open("GET", "/api/StatelessBackendService/?c=" + start);
    http.send();
}

/* This function calls the GuestExeBackendService's HTTP GET method to get the current count from the GuestExeBackendService */
function getGuestExeBackendService() {
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                if (http.responseText) {
                    countDisplay.innerHTML = "This node is named " + http.responseText;
                    updateFooter(http, (end - start));
                }
            } else {
                updateFooter(http, (end - start));
            }
        }
    };
    start = new Date().getTime();
    http.open("GET", "/api/GuestExeBackendService/?c=" + start);
    http.send();
}

/* This function calls the StatefulBackendController's HTTP GET method to get a collection of KeyValuePairs from the reliable dictionary in the StatefulBackendService */
function getStatefulBackendServiceDictionary() {
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                returnData = JSON.parse(http.responseText);
                if (returnData) {
                    renderStatefulBackendServiceDictionary(returnData);
                    updateFooter(http, (end - start));
                    //postMessage("Got all KeyValuePairs in  " + (end - start).toString() + "ms.", "success", true);
                }
            } else {
                updateFooter(http, (end - start));
                //postMessage(http.statusText, "danger", true);
            }
        }
    };
    start = new Date().getTime();
    http.open("GET", "/api/StatefulBackendService/?c=" + start);
    http.send();
}

/* This function calls the StatefulBackendController's HTTP PUT method to insert a KeyValuePair in the reliable dictionary in the StatefulBackendService */
function addStatefulBackendServiceKeyValuePair() {
    var keyValue = {
        key: keyInput.value,
        value: valueInput.value
    };
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                keyInput.value = '';
                valueInput.value = '';
                keyInput.focus();
                updateFooter(http, (end - start));
            } else {
                keyInput.focus();
                updateFooter(http, (end - start));
            }
        }
    };
    start = new Date().getTime();
    http.open("PUT", "/api/StatefulBackendService/?c=" + start);
    http.setRequestHeader("content-type", "application/json");
    http.send(JSON.stringify(keyValue));
}

/* This function calls the ActorBackendController's HTTP GET method to get the number of actors in the ActorBackendService */
function getActorCount() {
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                returnData = JSON.parse(http.responseText);
                if (returnData) {
                    countDisplay.innerHTML = returnData.count;
                    updateFooter(http, (end - start));
                }
            } else {
                updateFooter(http, (end - start));
            }
        }
    };
    start = new Date().getTime();
    http.open("GET", "/api/ActorBackendService/?c=" + start);
    http.send();
}

/* This function calls the ActorBackendController's HTTP POST method to create a new actor in the ActorBackendService */
function newActor() {
    var http = new XMLHttpRequest();
    http.onreadystatechange = function () {
        if (http.readyState === 4) {
            end = new Date().getTime();
            if (http.status < 400) {
                returnData = JSON.parse(http.responseText);
                if (returnData) {
                    updateFooter(http, (end - start));
                }
            } else {
                updateFooter(http, (end - start));
            }
        }
    };
    start = new Date().getTime();
    http.open("POST", "/api/ActorBackendService/?c=" + start);
    http.send();
}

/* UI Helper fuctions */

/* This function renders the output of the call to the Stateful Backend Service in a table */
function renderStatefulBackendServiceDictionary(dictionary) {
    var table = document.getElementById('statefulBackendServiceTable').childNodes[1];

    while (table.childElementCount > 1) {
        table.removeChild(table.lastChild);
    }

    for (var i = 0; i < dictionary.length; i++) {
        var tr = document.createElement('tr');
        var tdKey = document.createElement('td');
        tdKey.appendChild(document.createTextNode(dictionary[i].key));
        tr.appendChild(tdKey);
        var tdValue = document.createElement('td');
        tdValue.appendChild(document.createTextNode(dictionary[i].value));
        tr.appendChild(tdValue);
        table.appendChild(tr);
    }
}

/* This function highlights the current nav tab */
function navTab() {
    toggleFooter(0);
    var pathName = document.location.pathname.substring(6);
    switch (pathName) {
        case "":
            document.getElementById('navHome').className = "active";
            break;
        case "Stateless":
            document.getElementById('navStateless').className = "active";
            break;
        case "Stateful":
            document.getElementById('navStateful').className = "active";
            break;
        case "Actor":
            document.getElementById('navActor').className = "active";
            break;
        case "Guest":
            document.getElementById('navGuest').className = "active";
            break;
    }
}

/*This function hides the footer*/
function toggleFooter(option) {
    var footer = document.getElementById('footer');
    switch (option) {
        case 0:
            footer.hidden = true;
            break;
        case 1:
            footer.hidden = false;
            break;
    }
}

/*This function puts HTTP result in the footer */
function updateFooter(http, timeTaken) {
    toggleFooter(1);
    if (http.status < 299) {
        statusPanel.className = 'panel panel-success';
        statusPanelHeading.innerHTML = http.status + ' ' + http.statusText;
        statusPanelBody.innerHTML = 'Result returned in ' + timeTaken.toString() + ' ms from ' + http.responseURL;
    }
    else {
        statusPanel.className = 'panel panel-danger';
        statusPanelHeading.innerHTML = http.status + ' ' + http.statusText;
        statusPanelBody.innerHTML = http.responseText;
    }
}

function handleEnter() {
    var pathName = document.location.pathname.substring(6);
    switch (pathName) {
        case "Stateless":
            onkeyup = function (e) {
                if (e.keyCode == 13) {
                    getStatelessBackendCount();
                    return false;
                }
            };
            break;
        case "Stateful":
            keyInput.onkeyup = function (e) {
                if (e.keyCode == 13) {
                    addStatefulBackendServiceKeyValuePair();
                    return false;
                }
            };
            valueInput.onkeyup = function (e) {
                if (e.keyCode == 13) {
                    addStatefulBackendServiceKeyValuePair();
                    return false;
                }
            };
            break;
    }
}