# Research Topic: Generating A Navigation Mesh From Scratch Using Level Geometries In Unity
For my project I decided to generate 2D meshes from scratch. I wanted to get a truly in-depth look at every part of the process and thus investigated every type of algorithm along the way. I do believe this manner of investigation worthwhile. Knowledge of algorithms and ideas in one field might unexpectedly transfer to solve issues in another field. Navigation meshes themselves are a good example of an idea transferring, as it originated in the field of robotics, to be translated and popularized in game AI’s at the turning of the millennium. 

#### What is a Navigation Mesh?
A navigation mesh (or NavMesh, as the cool kids say) is a collection of two-dimensional polygons, which define areas of the map traversable by (AI) agents. A pathfinding algorithm, such as A*, can then be used to traverse said mesh. NavMeshes can be created manually or automatically, or through a combination of both. Most commonly, NavMeshes are static and immutable, which in plain English means that the NavMesh doesn't change during runtime. Dynamic NavMeshes also exist, but are harder to implement and might be costly (GET REFERENCE AND POSSIBLY CORRECT THIS IF WRONG). For this project I will solely be focusing on static NavMeshes.

// WRITE A SEGUE HERE
2D Nav Meshes – First identify areas (~polygons) that can be walked upon. Merge adjacent (~somewhat overlapping) walkable polygons and create a container holding the walkable polygons. Next take in account blocked areas (~wall, obstacles, …). Since we’re working in a 2D space, these blocked areas will be other polygons. 
 
CLEAN THIS UP!!
## 2D NavMesh
For the first iteration of my NavMesh project I only worked with a single floor geometry. The first challenge was to expand all level geometries to take in account a potential AI agent's size, given by a certain radius. I devised my own simple algorithm that should work for any convex polygon. The idea was as follows. For every single vertex of the polygon, I generate three new points. Two of these points would be respectively perpendicular to each of the sides adjacent to the vertex, at a distance of the previously mentioned radius. The third point would be exactly inbetween the two aforementioned points, also at the given distance*. These three points represented that vertex of the polygon expanded. So for every vertex of the polygon, these three points were added to an array representing the expanded polygon.

(*: while this is not exactly what happens on the programming side, it provides a more concise explanation.)

The resulting expansion was meant to look like this:



![](https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/VertexSort.gif)

VERTALEN NAAR ENGELS

Mijn originele plan was om te leren hoe Nav Mesh te creëren uit een 3D level, maar door een hele hoop technische onderwerpen waar ik nog niet vertrouwd mee ben, leek dit onderwerp al snel out of scope. Mijn plan is nu om eerst te leren hoe een nav mesh in 2D wordt gegenereerd. 

Sources:
GA TERUG OVER BRONNEN EN MAAK GEDETAILLEERDE LOGS
- Book title unknown: C11 Advanced Navmesh Generation (find out title!)
    --> Explains workings of NavMeshes in unity. Voxel based 3D approach, interesting, but probably too out of scope and does not go deep into the technicalities.
- https://github.com/recastnavigation/recastnavigation
    --> NavMesh generation in 3D as well, using voxels based on the existing level geometry.
- https://accu.org/journals/overload/21/117/golodetz_1838/
- https://www.gamedev.net/tutorials/programming/artificial-intelligence/generating-2d-navmeshes-r3393/
- https://en.wikipedia.org/wiki/Navigation_mesh#:~:text=A%20navigation%20mesh%20is%20a,are%20part%20of%20the%20environment.
