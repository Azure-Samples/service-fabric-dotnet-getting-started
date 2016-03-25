var nodeBuffersUpdated = false;
var nodesToRender = new Array();
var triangles = new Array();
var triangleHistories = new Array();

function updateNodeBuffers(nodes) {

    for (var incomingNodes = 0; incomingNodes < nodes.length; ++incomingNodes) {
        var node = nodes[incomingNodes];
        nodesToRender[incomingNodes] = node;

        if(triangles[incomingNodes] == null)
        {
            var t = new Path.RegularPolygon(new Point(0, 0), 3, 20);
            t.fillColor = new Color(node.currentColor.r, node.currentColor.g, node.currentColor.b);		
            triangles[incomingNodes] = t;

            var numHistory = node.history.length;
            triangleHistories[incomingNodes] = new Array();

            for(historyEntry = 0; historyEntry < numHistory; ++historyEntry)
            {
                var h = new Path.RegularPolygon(new Point(0, 0), 3, (20 - (2*(numHistory-historyEntry))));
                h.fillColor = new Color(node.currentColor.r, node.currentColor.g, node.currentColor.b);	
                h.fillColor.alpha = 1 - (0.11 * (numHistory - historyEntry));
                triangleHistories[incomingNodes][historyEntry] = h;
            }
            
        }

    }

    nodeBuffersUpdated = true;
}

function drawScene() 
{ 
    if (nodeBuffersUpdated) 
    {
        var numNodes = nodesToRender.length;

        for (nodeToRender = 0; nodeToRender < numNodes; ++nodeToRender) 
        {
            var node = nodesToRender[nodeToRender];
                
            triangles[nodeToRender].position = scalePosToViewport(node.current.x, node.current.y);
            triangles[nodeToRender].rotation = node.rotation;

            var historyCount = node.history.length;

            for(historyEntry = 0; historyEntry < historyCount; ++historyEntry)
            {
                var historyNodeData = node.history[historyEntry];
                var historyTriangle = triangleHistories[nodeToRender][historyEntry];
                historyTriangle.position = scalePosToViewport(historyNodeData.x, historyNodeData.y);
                historyTriangle.rotation = node.rotation;
            }
                    
        } 			

        nodeBuffersUpdated = false;
    }
}

function scalePosToViewport(nodex, nodey)
{
    var xfactor = view.viewSize.width / 2;
    var yfactor = view.viewSize.height / 2;

    var xval = nodex + 1;
    var yval = nodey + 1;

    //scaling factor is width or height over 2.
    //2 = width or height, 0 = 0;

    return new Point(xval * xfactor, yval * yfactor);	
}

function startDrawing() {
    var canvas = document.getElementById("canvas");

    canvas.style.border = "#00ff00 3px solid";

    paper.install(window);
    paper.setup('canvas');

    initWebSocket();

    view.onFrame = function(event) {
        drawScene();
    }
}


var websocket;

function initWebSocket() {
    websocket = new WebSocket("ws://" + window.location.host + "/visualobjects/data/");

    websocket.onopen = function () {};

    websocket.onmessage = function (args) {
        nodes = JSON.parse(args.data);
        updateNodeBuffers(nodes);
    };

    websocket.onclose = function (args) {
        setTimeout(initWebSocket, 100);
    };

    websocket.onerror = function (error) {
        websocket.close();
    }
}