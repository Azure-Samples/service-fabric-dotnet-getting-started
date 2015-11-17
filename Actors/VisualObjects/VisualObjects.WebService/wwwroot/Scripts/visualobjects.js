var gl;

function initGL(canvas) {
    try {
        var C = 0.8;        // canvas width to viewport width ratio
        var W_TO_H = 1 / 1;   // canvas width to canvas height ratio
        var viewportWidth = document.body.clientWidth;
        var viewportHeight = document.body.clientHeight;
        //var viewportWidth = window.innerWidth;
        //var viewportHeight = window.innerHeight;

        var canvasWidth = viewportWidth * C;
        var canvasHeight = canvasWidth / W_TO_H;
        canvas.style.position = "fixed";
        canvas.setAttribute("width", canvasWidth);
        canvas.setAttribute("height", canvasHeight);
        canvas.style.top = (viewportHeight - canvasHeight) / 2;
        canvas.style.left = (viewportWidth - canvasWidth) / 2;
        canvas.style.border = "#00ff00 3px solid";

        gl = canvas.getContext("webgl");
        if (!gl) { gl = canvas.getContext("experimental-webgl"); }
        gl.viewportWidth = viewportWidth;
        gl.viewportHeight = viewportHeight;
        //gl.viewportHeight = canvas.height;
        //gl.viewportWidth = canvas.width;
    } catch (e) {
        alert(e);
    }
    if (!gl) {
        alert("Could not initialize WebGL");
    }
}


function getShader(gl, id) {
    var shaderScript = document.getElementById(id);
    if (!shaderScript) {
        return null;
    }

    var str = "";
    var k = shaderScript.firstChild;
    while (k) {
        if (k.nodeType == 3) {
            str += k.textContent;
        }
        k = k.nextSibling;
    }

    var shader;
    if (shaderScript.type == "x-shader/x-fragment") {
        shader = gl.createShader(gl.FRAGMENT_SHADER);
    } else if (shaderScript.type == "x-shader/x-vertex") {
        shader = gl.createShader(gl.VERTEX_SHADER);
    } else {
        return null;
    }

    gl.shaderSource(shader, str);
    gl.compileShader(shader);

    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        alert(gl.getShaderInfoLog(shader));
        return null;
    }

    return shader;
}


var shaderProgram;

function initShaders() {
    var fragmentShader = getShader(gl, "shader-fs");
    var vertexShader = getShader(gl, "shader-vs");

    shaderProgram = gl.createProgram();
    gl.attachShader(shaderProgram, vertexShader);
    gl.attachShader(shaderProgram, fragmentShader);
    gl.linkProgram(shaderProgram);

    if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
        alert("Could not initialise shaders");
    }

    gl.useProgram(shaderProgram);

    shaderProgram.vertexPositionAttribute = gl.getAttribLocation(shaderProgram, "aVertexPosition");
    gl.enableVertexAttribArray(shaderProgram.vertexPositionAttribute);

    shaderProgram.vertexColorAttribute = gl.getAttribLocation(shaderProgram, "aVertexColor");
    gl.enableVertexAttribArray(shaderProgram.vertexColorAttribute);

    shaderProgram.pMatrixUniform = gl.getUniformLocation(shaderProgram, "uPMatrix");
    shaderProgram.mvMatrixUniform = gl.getUniformLocation(shaderProgram, "uMVMatrix");
}


var mvMatrix = mat4.create();
var mvMatrixStack = [];
var pMatrix = mat4.create();

function mvPushMatrix() {
    var copy = mat4.create();
    mat4.set(mvMatrix, copy);
    mvMatrixStack.push(copy);
}

function mvPopMatrix() {
    if (mvMatrixStack.length == 0) {
        throw "Invalid popMatrix!";
    }
    mvMatrix = mvMatrixStack.pop();
}


function setMatrixUniforms() {
    gl.uniformMatrix4fv(shaderProgram.pMatrixUniform, false, pMatrix);
    gl.uniformMatrix4fv(shaderProgram.mvMatrixUniform, false, mvMatrix);
}


function degToRad(degrees) {
    return degrees * Math.PI / 180;
}

var currentlyPressedKeys = {};

function handleKeyDown(event) {
    currentlyPressedKeys[event.keyCode] = true;
}

function handleKeyUp(event) {
    currentlyPressedKeys[event.keyCode] = false;
}

var xMove = 0;
var yMove = 0;
var zMove = 0;
var xRotate = 0;
var yRotate = 0;
var zRotate = 0;

function handleKeys() {
    if (currentlyPressedKeys[33]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            zRotate -= 1;
        }
        else {
            // Page Up
            zMove -= 0.05;
        }
    }
    if (currentlyPressedKeys[34]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            zRotate += 1;
        }
        else {
            // Page Down
            zMove += 0.05;
        }
    }
    if (currentlyPressedKeys[37]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            yRotate -= 1;
        }
        else {
            // Left cursor key
            xMove -= 0.05;
        }
    }
    if (currentlyPressedKeys[39]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            yRotate += 1;
        }
        else {
            // Right cursor key
            xMove += 0.05;
        }
    }
    if (currentlyPressedKeys[38]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            xRotate -= 1;
        }
        else {
            // Up cursor key
            yMove += 0.05;
        }
    }
    if (currentlyPressedKeys[40]) {
        if (currentlyPressedKeys[32]) {
            // Ctrl
            xRotate += 1;
        }
        else {
            // Down cursor key
            yMove -= 0.05;
        }
    }
}

var cubeVertexPositionBuffer;
var cubeVertexColorBuffer;

function initCubeBuffers() {
    cubeVertexPositionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, cubeVertexPositionBuffer);
    var vertices = [
         1.0, 1.0, 1.0,
         1.0, 1.0, -1.0,
        -1.0, 1.0, -1.0,
        -1.0, 1.0, 1.0,
         1.0, 1.0, 1.0,

         1.0, -1.0, 1.0,
        -1.0, -1.0, 1.0,
        -1.0, -1.0, -1.0,
         1.0, -1.0, -1.0,
         1.0, -1.0, 1.0
    ];
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
    cubeVertexPositionBuffer.itemSize = 3;
    cubeVertexPositionBuffer.itemCount = 10;

    cubeVertexColorBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, cubeVertexColorBuffer);
    var colors = new Array();
    for (var cx = 0; cx < cubeVertexPositionBuffer.itemCount; ++cx) {
        colors[cx * 4 + 0] = 0.0;
        colors[cx * 4 + 1] = 1.0;
        colors[cx * 4 + 2] = 0.0;
        colors[cx * 4 + 3] = 0.0;
    }
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.STATIC_DRAW);
    cubeVertexColorBuffer.itemSize = 4;
    cubeVertexColorBuffer.itemCount = cubeVertexPositionBuffer.itemCount;
}

var nodes = null;
var nodeBuffersUpdated = false;
var nodesToRender = null;
var nodeSize = 0.05;

function updateNodeBuffers() {
    for (var ix = 0; ix < nodes.length; ++ix) {
        nodes[ix].currentBuffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nodes[ix].currentBuffer);
        var vertices = [
            -nodeSize * 0.866, -nodeSize * 0.5, +nodeSize * 0.5,
            +nodeSize * 0.866, -nodeSize * 0.5, +nodeSize * 0.5,
            0, +nodeSize, 0,
            0, -nodeSize * 0.5, -nodeSize * 0.866,
            +nodeSize * 0.866, -nodeSize * 0.5, +nodeSize * 0.5,
            -nodeSize * 0.866, -nodeSize * 0.5, +nodeSize * 0.5,
            0, +nodeSize, 0,
            0, -nodeSize * 0.5, -nodeSize * 0.866,
        ];
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
        nodes[ix].currentBufferItemSize = 3;
        nodes[ix].currentBufferItemCount = 8;

        nodes[ix].currentColorBuffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nodes[ix].currentColorBuffer);
        var colors = new Array();
        for (var cx = 0; cx < nodes[ix].currentBufferItemCount; ++cx) {
            colors[cx * 4 + 0] = nodes[ix].currentColor.r;
            colors[cx * 4 + 1] = nodes[ix].currentColor.g;
            colors[cx * 4 + 2] = nodes[ix].currentColor.b;
            colors[cx * 4 + 3] = nodes[ix].currentColor.a;
        }
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.STATIC_DRAW);
        nodes[ix].currentColorBufferItemSize = 4;
        nodes[ix].currentColorBufferItemCount = nodes[ix].currentBufferItemCount;

        nodes[ix].historyBuffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nodes[ix].historyBuffer);
        vertices = new Array();
        nodes[ix].historyBufferItemSize = 3;
        nodes[ix].historyBufferItemCount = 0;
        for (var jx = 0; jx < nodes[ix].history.length; ++jx) {
            vertices[jx * 3 + 0] = nodes[ix].history[jx].x;
            vertices[jx * 3 + 1] = nodes[ix].history[jx].y;
            vertices[jx * 3 + 2] = nodes[ix].history[jx].z;

            nodes[ix].historyBufferItemCount++;
        }
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);

        nodes[ix].historyColorBuffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, nodes[ix].historyColorBuffer);
        colors = new Array();
        for (var cx = 0; cx < nodes[ix].historyBufferItemCount; ++cx) {
            colors[cx * 4 + 0] = nodes[ix].historyColor.r * cx / nodes[ix].historyBufferItemCount;
            colors[cx * 4 + 1] = nodes[ix].historyColor.g * cx / nodes[ix].historyBufferItemCount;
            colors[cx * 4 + 2] = nodes[ix].historyColor.b * cx / nodes[ix].historyBufferItemCount;
            colors[cx * 4 + 3] = nodes[ix].historyColor.a * cx / nodes[ix].historyBufferItemCount;
        }
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.STATIC_DRAW);
        nodes[ix].historyColorBufferItemSize = 4;
        nodes[ix].historyColorBufferItemCount = nodes[ix].historyBufferItemCount;
    }

    nodeBuffersUpdated = true;
}

function updateNodeBuffersToRender() {
    if (nodeBuffersUpdated) {
        nodesToRender = new Array();
        for (var ix = 0; ix < nodes.length; ++ix) {
            nodesToRender[ix] = nodes[ix];
        }

        nodeBuffersUpdated = false;
    }
}

function drawScene() {
    gl.viewport(0, 0, gl.viewportWidth, gl.viewportHeight);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    mat4.perspective(45, gl.viewportWidth / gl.viewportHeight, 0.1, 100.0, pMatrix);

    mat4.identity(mvMatrix);
    mat4.translate(mvMatrix, [0.0, 0.0, -4.0]);

    mat4.translate(mvMatrix, [xMove, yMove, zMove]);
    mat4.rotate(mvMatrix, degToRad(xRotate), [1, 0, 0]);
    mat4.rotate(mvMatrix, degToRad(yRotate), [0, 1, 0]);
    mat4.rotate(mvMatrix, degToRad(zRotate), [0, 0, 1]);

    gl.bindBuffer(gl.ARRAY_BUFFER, cubeVertexPositionBuffer);
    gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, cubeVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);

    gl.bindBuffer(gl.ARRAY_BUFFER, cubeVertexColorBuffer);
    gl.vertexAttribPointer(shaderProgram.vertexColorAttribute, cubeVertexColorBuffer.itemSize, gl.FLOAT, false, 0, 0);

    setMatrixUniforms();
    gl.drawArrays(gl.LINE_STRIP, 0, cubeVertexPositionBuffer.itemCount);

    updateNodeBuffersToRender();

    if (nodesToRender != null) {
        for (ix = 0; ix < nodesToRender.length; ++ix) {
            mvPushMatrix();

            mat4.translate(mvMatrix, [nodesToRender[ix].current.x, nodesToRender[ix].current.y, nodesToRender[ix].current.z]);

            if (nodesToRender[ix].rotation) {
                mat4.rotate(mvMatrix, degToRad(nodesToRender[ix].rotation), [0, 1, 1]);
            }

            gl.bindBuffer(gl.ARRAY_BUFFER, nodesToRender[ix].currentBuffer);
            gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, nodesToRender[ix].currentBufferItemSize, gl.FLOAT, false, 0, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, nodesToRender[ix].currentColorBuffer);
            gl.vertexAttribPointer(shaderProgram.vertexColorAttribute, nodesToRender[ix].currentColorBufferItemSize, gl.FLOAT, false, 0, 0);

            setMatrixUniforms();
            gl.drawArrays(gl.LINE_LOOP, 0, nodesToRender[ix].currentBufferItemCount);

            mvPopMatrix();

            gl.bindBuffer(gl.ARRAY_BUFFER, nodesToRender[ix].historyBuffer);
            gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, nodesToRender[ix].historyBufferItemSize, gl.FLOAT, false, 0, 0);

            gl.bindBuffer(gl.ARRAY_BUFFER, nodesToRender[ix].historyColorBuffer);
            gl.vertexAttribPointer(shaderProgram.vertexColorAttribute, nodesToRender[ix].historyColorBufferItemSize, gl.FLOAT, false, 0, 0);

            setMatrixUniforms();
            gl.drawArrays(gl.LINE_STRIP, 0, nodesToRender[ix].historyBufferItemCount);
        }
    }
}

var websocket;

function initWebSocket()
{
    websocket = new WebSocket("ws://" + window.location.host + "/visualobjects/data/");

    websocket.onopen = function () {
    };
    websocket.onmessage = function (args) {
        nodes = JSON.parse(args.data);
        updateNodeBuffers();
    };
    websocket.onclose = function (args) {
        setTimeout(initWebSocket, 500);
    };
    websocket.onerror = function (error) {
        websocket.close();
    }

}


function tick() {
    requestAnimFrame(tick);
    handleKeys();
    drawScene();
}

function webGLStart() {
    var canvas = document.getElementById("canvas");
    initGL(canvas);
    initShaders();
    initCubeBuffers();
    initWebSocket();

    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.enable(gl.DEPTH_TEST);

    document.onkeydown = handleKeyDown;
    document.onkeyup = handleKeyUp;

    tick();
}