# Research Topic: Generating A Navigation Mesh From Scratch Using Level Geometries In Unity
For my project I decided to generate 2D meshes from scratch. I wanted to get a truly in-depth look at every part of the process and thus investigated every type of algorithm along the way. I do believe this manner of investigation worthwhile. Knowledge of algorithms and ideas in one field might unexpectedly transfer to solve issues in another field. Navigation meshes themselves are a good example of an idea transferring, as it originated in the field of robotics, to be translated and popularized in game AI’s at the turning of the millennium. 

#### What is a Navigation Mesh?
**A navigation mesh (or NavMesh, as the cool kids say it) is a collection of two-dimensional polygons, which define areas of the map traversable by (AI) agents**. A pathfinding algorithm, such as A* can then be used to traverse said mesh. NavMeshes can be created manually or automatically, or through a combination of both. Most commonly, NavMeshes are static and immutable, which in plain English means that the NavMesh doesn't change during runtime. Dynamic NavMeshes also exist, but are harder to implement and NavMesh recalculation might be costly (GET REFERENCE AND POSSIBLY CORRECT THIS IF WRONG). **For this project I will solely be focusing on static NavMeshes.**

## 2D NavMesh
#### Polygon Expansion
For the first iteration of my NavMesh project I only worked with a single floor geometry. The first challenge was to expand all level geometries to take in account a potential AI agent's size, given by a certain radius. I devised my own simple algorithm that should work for any convex polygon. The idea was as follows. For every single vertex of the polygon, I generate three new points. Two of these points would be respectively perpendicular to each of the sides adjacent to the vertex, at a distance of the previously mentioned radius. The third point would be exactly inbetween the two aforementioned points, also at the given distance*. These three points represented that vertex of the polygon expanded. So for every vertex of the polygon, these three points were added to an array representing the expanded polygon.

(*: while this is not exactly what happens on the programming side, it provides a more concise explanation.)

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/VertexExpand.gif" 
     width="400" 
     height=auto />

<sub>*Visualisation of the polygon expansion technique*</sub>

#### Vertex Sorting
Naturally, it couldn’t be that simple. Unity doesn’t hold vertices of a 2D polygon as a simple array of vertices in a particular winding order. Polygons in Unity are built out of triangle, just as typical meshes are, using a triangle list structure. This means that there is an array holding all the vertices of the shape in a seemingly random order (the vertex array), and a second array of which every three elements represent a triangle (the triangle array). These three elements can each be used as indices in the vertex array, and the the resultring three retrieved vertices represent a triangle. But because of this random vector order I had to find a way to get the vertices in an order by which would you encounter when tracing along the outer edge of the polygon.

To solve this problem, some pen & paper analysis of the problem revealed a pattern that could be used to sort the vertices in a consecutive fashion. Every triangle described in the triangle array is adjacent to the next triangle described. This means that two of the indices it holds, are identical to two indices held by the next triangle. To start off, you copy over the first three indices of the triangle array to a new container, describing the first triangle. Next, look at the next triangle and find the element that is not in the container yet and then insert that element in the new container, inbetween the two elements that this container and your current triangle have in common.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/VertexSort.gif" 
     width="400" 
     height=auto />

<sub>*Visualisation of the vertex sorting technique*</sub>

After successfully implementing both of the above algorithm, the final polygon expansions for respectively a triangle, square, and hexagon, looked like this:

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/FinalExpansion.png" 
     width="400" 
     height=auto />
     
<sub>*Final result after vertex sorting and polygon expansion for a triangle, square, and hexagon. Expansion size set to 0.5*</sub>

#### Punching holes
The next obstacle (pun intended) was to cut the expanded obstacle polygons out of the ground plane. To do this, holes had to be treated as if they were part of the outer bounds of the polygon. This was achieved by creating a link between a vertex of the ground plane with a vertex of the hole. For this I utilized a brute force algorithm. First I found the combination of base shape vertex and hole vertex with the shortest distance between them. Next I looped through the base shape vertices, and upon encountering the aforementioned base shape vertex, I inserted the hole vertices in reverse winding order, starting from the found hole vertex. Finally, I respectively added the found hole vertex and base vertex (in that order!) once again, before adding the remaining base shape vertices.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/HolePunching.gif" 
     width="400" 
     height=auto />
     
<sub>*Step by step explanation of the hole punching algorithm*</sub>

This algorithm to add holes to a polygon was implemented recursively. After joining a hole polygon with the bass polygon, the hole polygon is removed from the list and the algorithm checks if there are still holes remaining in the list. If so, the algorithm passes the resulting polygon and the remaining holes on to itself again. There are probably better ways to implement this algorithm, as the brute force approach has a time of O(n * m) or O(n²) in standard big O notation. I mention no source here, because I devised of this method myself, but after researching the next topic I realized that the adding of holes to a polygon in this way is a long standing way of working.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/HolePuncherResult.png" 
     width="400" 
     height=auto />
     
<sub>*Result of recursive 'hole punching' algorithm*</sub>

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
