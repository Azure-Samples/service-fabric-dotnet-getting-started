# Visual Objects
This sample project uses WebGL to render a set of objects. Each object is represented by an Actor, where the location and trajectory of each is calculated on the server side by the Actor representing the object. 

## How to run Visual Objects
Press ctrl+F5 in Visual Studio to run (or just F5 to debug). Once the application has started, go to [http://localhost:8081/visualobjects](http://localhost:8081/visualobjects). Try opening it in multiple browser windows or on multiple machines to see how the server-side calculation produces the same result on every screen.

## How to Perform an Upgrade
Look inside of the StatefulVisualObjectActor.cs file for the MoveObject method. You should see a line "visualObject.Move(false);" Comment out this line and replace it with the one below where the value is set to true. Then right click on the VisualObjectsApplication project and select publish. Change the version of the code package inside the VisualObjects.ActorService package, and ensure that all of the versions for the service package and application type are changed as well. Click Save, and then ensure that you are targeting the correct cluster, then click publish. 