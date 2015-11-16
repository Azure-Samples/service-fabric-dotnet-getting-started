# Visual Objects
This sample project uses WebGL to render a set of objects in 3D. Each object is represented by an Actor, where the location and trajectory of each is calculated on the server side by the Actor representing the object. 

## How to run Visual Objects
To run this sample, you'll need to download a couple WebGL JavaScript files and place them in the VisualObjects.WebService project.

### gl-matrix
1. Get [gl-matrix v1.3.7](https://github.com/toji/gl-matrix/releases/tag/v1.3.7)
2. Place **gl-matrix-min.js** in **VisualObjects.WebService\wwwroot\Scripts\gl-matrix-min.js.**
3. Make sure the **Build Action** on the file is set to **Content** in the file properties (right-click in Solution Explorer, select Properties)

### webgl-utils
1. Get [webgl-utils.js](https://github.com/KhronosGroup/WebGL/blob/master/sdk/demos/common/webgl-utils.js)
2. Place **webgl-utils.js** in **VisualObjects.WebService\wwwroot\Scripts\webgl-utils.js.**
3. Make sure the **Build Action** on this file is set to **Content** in the file properties (right-click in Solution Explorer, select Properties)


With these JS files in place, press ctrl+F5 in Visual Studio to run (or just F5 to debug). Once the application has started, go to [http://localhost:8081/visualobjects](http://localhost:8081/visualobjects). Try opening it in multiple browser windows or on multiple machines to see how the server-side calculation produces the same result on every screen.