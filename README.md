# Research Topic: Generating A Navigation Mesh From Scratch Using Level Geometries In Unity
## Description
For my project I decided to generate 2D meshes from scratch. I wanted to get a truly in-depth look at every part of the process and thus investigated every type of algorithm along the way. I do believe this manner of investigation worthwhile. Knowledge of algorithms and ideas in one field might unexpectedly transfer to solve issues in another field. Navigation meshes themselves are a good example of an idea transferring, as it originated in the field of robotics<sup>1</sup>, to be translated and popularized in game AI’s at the turn of the millennium<sup>2</sup>. 

#### What is a Navigation Mesh?
**A navigation mesh (or NavMesh, as the cool kids say it) is a collection of two-dimensional polygons, which define areas of the map traversable by (AI) agents<sup>3</sup>**. A pathfinding algorithm, such as A* can then be used to traverse said mesh. NavMeshes can be created manually or automatically, or through a combination of both. Most commonly, NavMeshes are static and immutable, which in plain English means that the NavMesh doesn't change during runtime. Dynamic NavMeshes also exist, but are harder to implement and NavMesh recalculation might be costly, depending on the manner of implementation<sup>4</sup>. **For this project I solely be focused on static NavMeshes.**

## 2D NavMesh: Design & Implementation
#### Polygon Expansion
For the current iteration of my NavMesh project I only worked with a **single floor geometry and convex, non-overlapping polygons**. As a starting point, I broadly followed the plan of steps outlined by Romstöck <sup>5</sup>. The first challenge was to **expand all level geometries to take in account a potential AI agent's size**, given by a certain radius. I devised my own simple algorithm that should work for any convex polygon. The idea was as follows: For every single vertex of the polygon, I generate three new points. Two of these points would be respectively perpendicular to each of the sides adjacent to the vertex, at a distance of the previously mentioned radius. The third point would be exactly inbetween the two aforementioned points, also at the given distance*. These three points represented that vertex of the polygon expanded. So for every vertex of the polygon, these three points were added to an array representing the expanded polygon.

*(\*: while this is not exactly what happens on the programming side, it provides a more concise explanation.)*

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/VertexExpand.gif" 
     width="400" 
     height=auto />

<sub>*Visualisation of the polygon expansion technique*</sub>

#### Vertex Sorting
Naturally, it couldn’t be that simple. **Unity doesn’t hold vertices of a 2D polygon as a simple array of vertices in a particular winding order**. Polygons in Unity are built out of triangles, just as typical meshes are, using a triangle list structure. This means that there is an array holding all the vertices of the shape in a seemingly random order (the vertex array), and a second array of which every three elements represent a triangle (the triangle array). These three elements can each be used as indices in the vertex array, and the the resulting three retrieved vertices represent a triangle. But because of this random vector order I had to find a way to get the vertices in an order by which would you encounter when tracing along the outer edge of the polygon.

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
The next obstacle (pun intended) was to **cut the expanded obstacle polygons out of the ground plane**. To do this, holes had to be treated as if they were part of the outer bounds of the polygon. This was achieved by creating a link between a vertex of the ground plane with a vertex of the hole. For this I utilized a **brute force algorithm**. First I found the combination of base shape vertex and hole vertex with the shortest distance between them. Next I looped through the base shape vertices, and upon encountering the aforementioned base shape vertex, I inserted the hole vertices in reverse winding order, starting from the found hole vertex. Finally, I respectively added the found hole vertex and base vertex - in that exact order - once again, before adding the remaining base shape vertices.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/HolePunching.gif" 
     width="400" 
     height=auto />
     
<sub>*Step by step explanation of the hole punching algorithm*</sub>

This algorithm to add holes to a polygon was **implemented recursively**. After joining a hole polygon with the base polygon, the hole polygon is removed from the list and the algorithm checks if there are still holes remaining in the list. If so, the algorithm passes the resulting polygon and the remaining holes on to itself again. There are probably better ways to implement this algorithm, as the brute force approach has a time of O(n * m) or **O(n²)** in standard big O notation. I mention no source here, because I devised of this method myself, but after researching the next topic I realized that the adding of holes to a polygon in this way is a long standing way of working<sup>6</sup>.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/HolePuncherResult.png" 
     width="400" 
     height=auto />
     
<sub>*Result of recursive 'hole punching' algorithm*</sub>

#### Ear clipping
The final step to create a usable polygon to generate a Navigation Graph from, was to **triangulate the resulting polygon from the previous step**. For this I implemented an ear clipping algorithm <sup>6</sup>. An ear clipping algorithm works by finding an ear, storing the triangle of that ear in a list of triangles and then removing the vertex on the tip of that ear from the vertex order. An ear (in my implementation) is defined as a set of three consecutive vertices in the vertex order with two properties. The first being that the lines between the first and the second vertex, and the second and third, do not form a concave interior angle. The second being that no other vertices lie within the bounds of the triangle formed by the ear vertices. Finally, when only three vertices remain, add these final three vertices as the last triangle in the list of triangles.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/EarClipping.gif" 
     width="400" 
     height=auto />

<sub>*Demonstration of the ear clipping algorithm*</sub>

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/EarClippingResult.png" 
     width="400" 
     height=auto />  


<sub>*Final triangulated Navigation Mesh polygon after running ear clipping algorithm*</sub>

#### Finally, a NavGraph (Result)
With the setup done, I decided to also create a **mockup of a navigation graph**, the actual data that would be **used in a pathfinding algorithm** next to the mesh geometry. **A navigation graph consists of a set of nodes, placed on lines between two adjacent triangles, and the connections between them**. I must admit that this graph is only a mockup. I implemented the NavGraph generation with a recursive algorithm that has a crucial design flaw. When encountering a triangle for which nodes and connections have already been created, it creates another node on the line adjacent to that triangle, instead of linking to the already existing one. While this results in a visually identical looking NavGraph, it is not practically functional. The problem lays in that the process does not store which triangles the nodes and connections belong to. This could be solved by revisioning my data structures, but unfortunately due to the deadline for this topic and not wanting to risk breaking my current iteration, I have not solved this issue.

<img src="https://github.com/rutger-hertoghe/GPP_ResearchProject/blob/master/FinalNavGraph.png" 
     width=auto 
     height=auto />  


<sub>*Example of the final NavMesh & NavGraph. The white area represents the floor plane, the red areas obstacles. The generated NavMesh is indicated by the green lines. NavGraph nodes are indicated by magenta diamonds, NavGraph connections by lines varying between blue & red. The variation in connection colors represent the cost. Lines closer to blue represent a low cost connection, whereas lines closer to red represent a high cost connection.*</sub>

## Closing remarks
#### Polygon expansion
I do realize that the method by which I expanded the obstacle polygons is flawed. The **expansion does not extend quite far enough at the angles of shapes, thus agents could possibly get stuck** if they were to traverse the NavMesh. While this could quite easily be handled in the way the expansion is done, time constraints forced me to focus on more important aspects of NavMesh generation. In the current iteration of this project, setting the radius by which to expand the shape slightly higher than the radius of the agent for whom the mesh is intended, should suffice.

#### NavMesh Generation Limitations
At this moment, the **NavMesh generation has several requirements** to work. Firstly, there can only be a **single floorplane**. Secondly, do the expanded obstacle **polygons need to be fully contained within the bounds of this floorplane**. Furthermore, **obstacle polygons**, whether expanded or not, **are not allowed to overlap and must be convex**. To support overlap, the expanded polygons would need to be merged or clipped depending on the situation. This would require testing for intersections between all the line segments in the scene. Functionality to support this could be implemented using either an inefficent naive approach or a sweep line algorithm<sup>7, 8, 9</sup>.
Line intersection checking and retrieving the location of this intersection require an algorithm in and of itself, involving vector math<sup>10</sup>.

The **naive approach**, operating in O(n²) time, would require comparing every line segment in the scene, with every other line segment. An optimization for this would be to first compare overlap between Axis-Aligned Bounding Boxes (AABB's) for every polygon first, and if two polygons overlap, check for intersections between their respective line segments. 

The **sweep line approach** (O(log(n) * n time) is more efficient and works by sweep an imaginary line from one side of the scene to the opposing side and comparing only the line segments that are currently next to each other in a data container called the sweep line state. Note that there is actually not really a line sweeping over the screen, but that the sweep line checks at discrete points (if sweeping over the y-axis, these are y-coordinates), stored in what is called the event queue. For efficiency, both the sweep line state and the event queue are binary search trees. This method thus requires the implementation of a data structure that does not exist by default in C#.

At the time of writing, I was unfortunately pressed for time, and not confident in my ability to properly implement either of the above algorithms and data structures in the remaining time frame.

#### 3D NavMeshes
While I initially set out to generate 3D NavMeshes, I felt that both the needed basic concepts and advanced techniques to generate 3D NavMeshes eluded me. Hence my fallback to 2D NavMeshes to first get a firm grip on the concept in a simpler space. I do however realize that 2D NavMeshes and 3D NavMeshes differ quite a lot in terms of generation, the latter being a lot more advanced. **3D NavMeshes work by first voxelizing the scene**. Depending on the needs of the implementation, this voxelized scene is then **used to identify and extract the different walkable layers**<sup>11</sup> and/or a heightfield<sup>12</sup>. Finally, the **original source meshes are generally used again along with the voxelized scene to clip the correct NavMesh polygons** and link different layers to one another if needed. Developing this whole process from scratch and implementing every necessary algorithm myself would be a project taking weeks, if not months.

## Sources in order of first appearance:
1) Arkin, R. C. (1986). "Path Planning for a Vision-Based Autonomous Robot", University of Massachusetts.
2) Snook, G. (2000). "Simplified 3D Movement and Pathfinding Using Navigation Meshes". In DeLoura, Mark (ed.). Game Programming Gems. Charles River Media. pp. 288–304. ISBN 1-58450-049-2.
3) Wikipedia (2022), accessed 7 january 2023 <https://en.wikipedia.org/wiki/Navigation_mesh#:~:text=A%20navigation%20mesh%20is%20a,are%20part%20of%20the%20environment>.
4) Van Toll, W. & Geraerts, R. (2012) "A Navigation Mesh for Dynamic Environments". In Computer Animation and Virtual Worlds 23(6):535-546, <DOI:10.1002/cav.1468>.
5) Romstöck, C. (2014). "Generating 2D Navmeshes", accessed 7 january 2023 <https://www.gamedev.net/tutorials/programming/artificial-intelligence/generating-2d-navmeshes-r3393/>.
6) Eberly, D. (2002). "Triangulation by Ear Clipping".
7) Skiena. S. S. (2008). "The Algorithm Design Manual".
8) O'Rourke, J. (1998). "Intersection of NonConvex Polygons.", In Computational Geometry in C.
9) Kindermann, P. (2021). "Sweep-Line Algorithm for Line Intersection", accessed 10 january 2023, <https://www.youtube.com/playlist?list=PLubYOWSl9mItBLmB2WiFU0A_WINUSLtGH>.
10) Cormen, T. H., Leiserson, C. E., Rivest, R. L., & Stein, C. (2009). "Introduction to algorithms (3rd ed.)." The MIT Press.
11) Oliva R., Pelechano, N. (2013). "NEOGEN: Near Optimal Generator of Navigation Meshes for 3D Multi-Layered Environments". In Computers & Graphics 37(5):403-412, <https://doi.org/10.1016/j.cag.2013.03.004>
12) Lazzaroni, S. (2020). "Navigation Mesh Generation", accessed  7 january 2023, <https://www.stefanolazzaroni.com/navigation-mesh-generation>.
