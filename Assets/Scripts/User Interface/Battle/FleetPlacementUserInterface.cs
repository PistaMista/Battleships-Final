﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUserInterface : BoardViewUserInterface
{
    public Material shipDrawerMaterial;
    public Waypoint cameraWaypoint;
    public GameObject[] defaultShipLoadout;
    public float shipPaletteGroupPadding;
    public float shipDrawerFlatSize;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.ENABLING:
                break;
        }
    }

    protected override void DeployWorldElements()
    {
        base.DeployWorldElements();
        cameraWaypoint.transform.position = Battle.main.attacker.boardCameraPoint.transform.position;
        cameraWaypoint.transform.rotation = Battle.main.attacker.boardCameraPoint.transform.rotation;
        CameraControl.GoToWaypoint(cameraWaypoint, MiscellaneousVariables.it.playerCameraTransitionTime);

        ResetWorldSpaceParent();
        MakeShipDrawer();
    }

    struct ShipRectangleGroup
    {
        public bool vertical;
        public Ship[] ships;
        public Rect rect;
        public Vector2[] horizontalCorners;
        public Vector2[] verticalCorners;
        public Vector2[] Corners
        {
            get { return vertical ? verticalCorners : horizontalCorners; }
        }
    }

    ShipRectangleGroup[] groups;
    GameObject shipDrawer;
    void MakeShipDrawer()
    {
        Destroy(shipDrawer);
        shipDrawer = new GameObject("Ship Drawer");
        shipDrawer.transform.SetParent(worldSpaceParent);

        List<ShipRectangleGroup> unfinishedGroups = new List<ShipRectangleGroup>();
        List<Ship> toAdd = new List<Ship>();

        ShipRectangleGroup currentGroup = new ShipRectangleGroup();
        for (int i = 0; i < defaultShipLoadout.Length; i++)
        {
            Ship ship = Instantiate(defaultShipLoadout[i]).GetComponent<Ship>();

            ship.owner = Battle.main.attacker;
            ship.transform.SetParent(shipDrawer.transform);

            if (toAdd.Count == 0)
            {
                toAdd.Add(ship);
                continue;
            }

            if (toAdd[0].type == ship.type)
            {
                toAdd.Add(ship);
            }
            else
            {
                currentGroup.ships = toAdd.ToArray();
                toAdd = new List<Ship>();
                toAdd.Add(ship);
                unfinishedGroups.Add(currentGroup);
                currentGroup = new ShipRectangleGroup();
            }
        }

        currentGroup.ships = toAdd.ToArray();
        unfinishedGroups.Add(currentGroup);

        groups = unfinishedGroups.ToArray();

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].rect.height = groups[i].ships[0].health + shipPaletteGroupPadding;
            groups[i].rect.width = (groups[i].ships[0].transform.localScale.x + 0.5f) * groups[i].ships.Length + shipPaletteGroupPadding;
            groups[i].horizontalCorners = CalculateCorners(groups[i].rect, false);
            groups[i].verticalCorners = CalculateCorners(groups[i].rect, true);
        }


        ArrangeShipGroupsOnSquarePlane();

        // GameObject drawerFlatpanel = GameObject.CreatePrimitive(PrimitiveType.Quad);
        // drawerFlatpanel.transform.SetParent(shipDrawer.transform);
        // drawerFlatpanel.transform.localPosition = Vector3.zero;
        // drawerFlatpanel.transform.localScale = new Vector3(shipDrawerFlatSize, shipDrawerFlatSize, 1.0f);
        // drawerFlatpanel.transform.localRotation = new Quaternion(1, 0, 0, 1);

        MoldDrawerMeshes();
    }

    void MoldDrawerMeshes()
    {
        List<Dictionary<Vector3, List<Vector3>>> flatpanelHoles = new List<Dictionary<Vector3, List<Vector3>>>();
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            for (int shipIndex = 0; shipIndex < groups[groupIndex].ships.Length; shipIndex++)
            {
                //Initialize an object to store the hole this mesh is going to make in the flatpanel
                Dictionary<Vector3, List<Vector3>> hole = new Dictionary<Vector3, List<Vector3>>();
                //Get the mesh we are going to be molding
                MeshFilter moldedShipMesh = groups[groupIndex].ships[shipIndex].GetComponentInChildren<MeshFilter>();

                Vector3 positionRelativeToDrawer = groups[groupIndex].ships[shipIndex].transform.position - shipDrawer.transform.position;
                Vector3 positionMod = moldedShipMesh.gameObject.transform.position - groups[groupIndex].ships[shipIndex].transform.position;
                Vector3 scale = moldedShipMesh.gameObject.transform.lossyScale;

                //Assemble the new mesh
                Vector3[] originalVertices = moldedShipMesh.mesh.vertices;

                //Position the vertices correctly
                for (int vertexID = 0; vertexID < originalVertices.Length; vertexID++)
                {
                    originalVertices[vertexID] = moldedShipMesh.gameObject.transform.rotation * Vector3.Scale(originalVertices[vertexID], scale) + positionMod + positionRelativeToDrawer;
                }

                int[] originalTriangles = moldedShipMesh.mesh.triangles;


                List<Vector3> newVerticesList = new List<Vector3>();
                List<int> newTrianglesList = new List<int>();

                //Add the triangles along with their vertices
                for (int triangle = 0; triangle <= originalTriangles.Length - 3; triangle += 3)
                {
                    int[] triangleVertices = new int[] { originalTriangles[triangle], originalTriangles[triangle + 1], originalTriangles[triangle + 2] };

                    List<int> upperVertexIDs = new List<int>();
                    List<int> lowerVertexIDs = new List<int>();
                    for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                    {
                        Vector3 vertexPosition = originalVertices[triangleVertices[vertexID]];
                        if (vertexPosition.y > 0)
                        {
                            upperVertexIDs.Add(vertexID);
                        }
                        else
                        {
                            lowerVertexIDs.Add(vertexID);
                        }
                    }

                    Vector3[] surfacePair = new Vector3[2] { Vector3.up * 999, Vector3.up * 999 };

                    switch (upperVertexIDs.Count)
                    {
                        case 0:
                            for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                            {
                                Vector3 finalPosition = originalVertices[triangleVertices[vertexID]];

                                newVerticesList.Add(finalPosition);
                            }

                            newTrianglesList.Add(newVerticesList.Count - 1);
                            newTrianglesList.Add(newVerticesList.Count - 2);
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                        case 1:
                            Vector3 originalTipPosition = originalVertices[triangleVertices[upperVertexIDs[0]]];
                            List<Vector3> retractedPoints = new List<Vector3>();

                            //Add the points to and unordered list
                            int surfaceID = 0;
                            foreach (int vertexID in lowerVertexIDs)
                            {
                                Vector3 position = originalVertices[triangleVertices[vertexID]];

                                Vector3 relativePosition = originalTipPosition - position;

                                Vector3 normalizationAgent = relativePosition.normalized / relativePosition.normalized.y;
                                Vector3 retractedPointPosition = -normalizationAgent * position.y + position;

                                newVerticesList.Add(position);
                                retractedPoints.Add(retractedPointPosition);

                                surfacePair[surfaceID] = retractedPointPosition;
                                surfaceID++;
                            }

                            newVerticesList.AddRange(retractedPoints);

                            bool invert = lowerVertexIDs[1] - lowerVertexIDs[0] > 1;
                            //Add the first triangle of the quad
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 3 : 2));
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 2 : 3));
                            newTrianglesList.Add(newVerticesList.Count - 4);

                            //Add the seconds triangle of the quad
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 1 : 2));
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 2 : 1));
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                        case 2:
                            surfaceID = 0;
                            for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                            {
                                Vector3 finalPosition = originalVertices[triangleVertices[vertexID]];
                                if (upperVertexIDs.Contains(vertexID))
                                {
                                    Vector3 linkedPosition = originalVertices[triangleVertices[lowerVertexIDs[0]]];
                                    Vector3 relativePosition = finalPosition - linkedPosition;

                                    Vector3 normalizationAgent = relativePosition.normalized / relativePosition.normalized.y;
                                    finalPosition = -normalizationAgent * linkedPosition.y + linkedPosition;

                                    surfacePair[surfaceID] = finalPosition;
                                    surfaceID++;
                                }

                                newVerticesList.Add(finalPosition);
                            }

                            newTrianglesList.Add(newVerticesList.Count - 1);
                            newTrianglesList.Add(newVerticesList.Count - 2);
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                    }

                    //If this triangle is on the surface add its intersection to the hole
                    if (upperVertexIDs.Count == 1 || upperVertexIDs.Count == 2)
                    {
                        for (int point = 0; point < 2; point++)
                        {
                            int connection = (point + 1) % 2;
                            if (!hole.ContainsKey(surfacePair[point]))
                            {
                                hole.Add(surfacePair[point], new List<Vector3>());
                            }

                            hole[surfacePair[point]].Add(surfacePair[connection]);
                        }
                    }
                }

                //Add the hole this mesh has made in the flatpanel
                flatpanelHoles.Add(hole);

                Mesh finalMesh = new Mesh();
                finalMesh.vertices = newVerticesList.ToArray();
                finalMesh.triangles = newTrianglesList.ToArray();
                finalMesh.RecalculateNormals();

                GameObject shipMold = new GameObject("Ship Mold");
                shipMold.transform.SetParent(shipDrawer.transform);
                //shipMold.transform.position = moldedShipMesh.gameObject.transform.position + Vector3.up * 10;
                //shipMold.transform.rotation = moldedShipMesh.gameObject.transform.rotation;

                MeshFilter meshFilter = shipMold.AddComponent<MeshFilter>();
                shipMold.AddComponent<MeshRenderer>();
                meshFilter.mesh = finalMesh;

                shipMold.GetComponent<Renderer>().material = shipDrawerMaterial;
            }
        }

        //Order all of the vertices of each hole in a counter-clockwise direction
        Vector3[][] orderedHoles = new Vector3[flatpanelHoles.Count][];
        int finalHoleID = 0;
        foreach (Dictionary<Vector3, List<Vector3>> hole in flatpanelHoles)
        {
            Vector3[] vertices = new Vector3[hole.Keys.Count];
            hole.Keys.CopyTo(vertices, 0);

            //Get a list of vertices, where each vertex connects to the next one in the list - a connected hole
            List<Vector3> connectedHole = new List<Vector3>();
            Vector3 currentPosition = vertices[0];
            for (int i = 0; i < vertices.Length; i++)
            {
                foreach (Vector3 connection in hole[currentPosition])
                {
                    if (!connectedHole.Contains(connection))
                    {
                        connectedHole.Add(connection);
                        currentPosition = connection;
                        break;
                    }
                }
            }

            //Determine if the connected hole is now clockwise or counter-clockwise
            float deterministicSum = 0;
            for (int i = 0; i < connectedHole.Count; i++)
            {
                Vector3 first = connectedHole[i];
                Vector3 second = connectedHole[(i + 1) % connectedHole.Count];

                deterministicSum += (second.x - first.x) * (second.z + first.z);
            }

            //If its clockwise make it counter-clockwise
            if (deterministicSum > 0)
            {
                connectedHole.Reverse();
            }

            //Add it to the final array
            orderedHoles[finalHoleID] = connectedHole.ToArray();
            finalHoleID++;
        }

        //Assemble a simple polygon out of a plane and these ordered holes
        float halfSize = shipDrawerFlatSize / 2.0f;
        List<Vector3> processedPolygon = new List<Vector3>() { new Vector3(-halfSize, 0, halfSize), new Vector3(halfSize, 0, halfSize), new Vector3(halfSize, 0, -halfSize), new Vector3(-halfSize, 0, -halfSize) };

        for (int holeID = 0; holeID < orderedHoles.Length; holeID++)
        {
            //Find the vertex of the hole, that is furthest to the right
            int firstVertexInHoleID = 0;
            Vector3 firstVertexInHolePosition = orderedHoles[holeID][0];
            for (int holePointID = 0; holePointID < orderedHoles[holeID].Length; holePointID++)
            {
                Vector3 candidatePosition = orderedHoles[holeID][holePointID];
                if (candidatePosition.x > firstVertexInHolePosition.x)
                {
                    firstVertexInHoleID = holePointID;
                    firstVertexInHolePosition = candidatePosition;
                }
            }

            //Find a vertex on the edge of the existing polygon to connect the hole with
            int injectionPointID = 0;
            Vector3 edgeConnector = Vector3.right * Mathf.Infinity;
            for (int polygonVertexID = 0; polygonVertexID < processedPolygon.Count; polygonVertexID++)
            {
                Vector3 firstVertexRelative = processedPolygon[polygonVertexID] - firstVertexInHolePosition;
                Vector3 secondVertexRelative = processedPolygon[(polygonVertexID + 1) % processedPolygon.Count] - firstVertexInHolePosition;

                //If one point is below the line and the other above
                if (firstVertexRelative.z * secondVertexRelative.z <= 0)
                {
                    Vector3 directional = (secondVertexRelative - firstVertexRelative).normalized;
                    Vector3 normalizationAgent = directional / directional.z;

                    Vector3 potentialEdgeConnector = firstVertexRelative - normalizationAgent * firstVertexRelative.z + firstVertexInHolePosition;

                    if (potentialEdgeConnector.x > 0 && potentialEdgeConnector.x < edgeConnector.x)
                    {
                        injectionPointID = polygonVertexID;
                        edgeConnector = potentialEdgeConnector;
                    }
                }
            }

            //Inject the hole with a coincident edge defined by the two vertices
            List<Vector3> toInject = new List<Vector3>();
            toInject.Add(edgeConnector);
            for (int holeIDOffset = 0; holeIDOffset < orderedHoles[holeID].Length + 1; holeIDOffset++)
            {
                int actualID = (firstVertexInHoleID + holeIDOffset) % orderedHoles[holeID].Length;
                toInject.Add(orderedHoles[holeID][actualID]);
            }
            toInject.Add(edgeConnector);

            processedPolygon.InsertRange((injectionPointID + 1) % processedPolygon.Count, toInject);
        }

        //Triangulate the resulting polygon
        Vector3[] polygon = processedPolygon.ToArray();

        List<Vector3> finalVertices = new List<Vector3>();
        List<int> finalTriangles = new List<int>();

        //Add the initial edges
        List<int> edges = new List<int>();
        for (int i = 0; i < polygon.Length; i++)
        {
            edges.Add(i);
        }


        for (int i = 0; i < polygon.Length; i++)
        {
            //TEST
            Vector3 tcP = polygon[i];
            Vector3 tnP = polygon[(i + 1) % polygon.Length];

            Debug.DrawLine(tcP, tnP, Color.red, Mathf.Infinity, false);
            //TEST
            for (int edge = 0; edge < edges.Count; edge++)
            {
                int currentPointID = edges[edge];
                Vector3 currentPoint = polygon[currentPointID];

                int previousPointID = edges[(edge + edges.Count - 1) % edges.Count];
                Vector3 previousPointRelative = polygon[previousPointID] - currentPoint;

                int nextPointID = edges[(edge + 1) % edges.Count];
                Vector3 nextPointRelative = polygon[nextPointID] - currentPoint;

                Vector3 previousPointNormal = new Vector3(-previousPointRelative.z, previousPointRelative.x).normalized;
                Vector3 nextPointNormal = new Vector3(nextPointRelative.z, 0, -nextPointRelative.x).normalized;


                //Determine whether this edge is convex
                if (Vector3.Distance(previousPointRelative, nextPointNormal) < Vector3.Distance(previousPointRelative, previousPointNormal))
                {
                    float triangleArea = CalculateTriangleArea(Vector3.zero, nextPointRelative, previousPointRelative);

                    bool intersected = false;
                    //Determine whether this triangle has any edges intersecting into it
                    foreach (int potentialIntersector in edges)
                    {
                        //If the potential intersector point is not one of the three points of the triangle
                        if (potentialIntersector != currentPointID && potentialIntersector != previousPointID && potentialIntersector != nextPointID)
                        {
                            //Check if its inside the triangle - AREA TEST
                            Vector3 intersectorRelativePosition = polygon[potentialIntersector] - currentPoint;

                            float area1 = CalculateTriangleArea(intersectorRelativePosition, previousPointRelative, nextPointRelative);
                            float area2 = CalculateTriangleArea(intersectorRelativePosition, Vector3.zero, previousPointRelative);
                            float area3 = CalculateTriangleArea(intersectorRelativePosition, Vector3.zero, nextPointRelative);

                            if (!((area1 + area2 + area3) > triangleArea))
                            {
                                intersected = true;
                                break;
                            }
                        }
                    }

                    if (!intersected)
                    {
                        finalVertices.Add(previousPointRelative + currentPoint);
                        finalVertices.Add(currentPoint);
                        finalVertices.Add(nextPointRelative + currentPoint);

                        finalTriangles.Add(finalVertices.Count - 3);
                        finalTriangles.Add(finalVertices.Count - 2);
                        finalTriangles.Add(finalVertices.Count - 1);

                        edges.RemoveAt(edge);
                        break;
                    }
                }
            }
        }



        //Add this polygon into the drawer
        Mesh drawerFlatpanelMesh = new Mesh();
        drawerFlatpanelMesh.vertices = finalVertices.ToArray();
        drawerFlatpanelMesh.triangles = finalTriangles.ToArray();
        drawerFlatpanelMesh.RecalculateNormals();

        GameObject drawerFlatpanel = new GameObject("Drawer Flatpanel");
        drawerFlatpanel.transform.SetParent(shipDrawer.transform);
        Renderer flatpanelRenderer = drawerFlatpanel.AddComponent<MeshRenderer>();
        flatpanelRenderer.material = shipDrawerMaterial;
        drawerFlatpanel.AddComponent<MeshFilter>().mesh = drawerFlatpanelMesh;

        drawerFlatpanel.transform.Translate(Vector3.up * 10);
    }

    float CalculateTriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Abs((a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z)) / 2.0f);
    }

    struct AttachmentPoint
    {
        public Vector2 position;
        public Vector2[] quadrantSizeLimits;

        public AttachmentPoint(Vector2 position, Vector2[] quadrantSizeLimits)
        {
            this.position = position;
            this.quadrantSizeLimits = quadrantSizeLimits;
        }
    }

    struct NextStepCandidate
    {
        public int groupID;
        public Vector2 position;
        public bool vertical;
        public Vector2 wholeFootprint;
        public float balance;

        public NextStepCandidate(int groupID, Vector2 position, bool vertical, Vector2 wholeFootprint, float balance)
        {
            this.groupID = groupID;
            this.position = position;
            this.vertical = vertical;
            this.wholeFootprint = wholeFootprint;
            this.balance = balance;
        }
    }

    void ArrangeShipGroupsOnSquarePlane()
    {
        Vector2 footprint = Vector2.zero;
        List<int> addedGroupIDs = new List<int>();

        List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
        attachmentPoints.Add(new AttachmentPoint(Vector2.zero, new Vector2[] { Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity }));

        NextStepCandidate bestCandidate = new NextStepCandidate(0, Vector2.zero, false, Vector2.one, Mathf.Infinity);
        for (int i = 0; i < groups.Length; i++)
        {
            //DETERMINE BEST CANDIDATE STEP
            for (int candidateGroupID = 0; candidateGroupID < groups.Length; candidateGroupID++)
            {
                if (!addedGroupIDs.Contains(candidateGroupID))
                {
                    for (int verticalIndex = 0; verticalIndex < 2; verticalIndex++)
                    {
                        //CALCULATE CORNER POSITIONS
                        Vector2[] groupCorners = verticalIndex == 1 ? groups[candidateGroupID].verticalCorners : groups[candidateGroupID].horizontalCorners;
                        //CALCULATE FOOTPRINT
                        Vector2 size = groupCorners[2] - groupCorners[1];

                        //TRY ALL POSITIONING OPTIONS
                        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
                        {
                            for (int examinedCorner = 0; examinedCorner < 4; examinedCorner++)
                            {
                                Vector2 sizeLimitation = attachmentPoint.quadrantSizeLimits[examinedCorner];

                                if (size.x <= sizeLimitation.x && size.y <= sizeLimitation.y)
                                {
                                    NextStepCandidate newCandidate;
                                    newCandidate.groupID = candidateGroupID;
                                    newCandidate.position = attachmentPoint.position - groupCorners[examinedCorner];
                                    newCandidate.vertical = verticalIndex == 1;

                                    Vector4 boundaries = Vector4.zero;
                                    for (int newCandidateCornerID = 0; newCandidateCornerID < groupCorners.Length; newCandidateCornerID++)
                                    {
                                        boundaries = PushBoundaries(boundaries, newCandidate.position + groupCorners[newCandidateCornerID]);
                                    }

                                    foreach (int addedGroupID in addedGroupIDs)
                                    {
                                        Vector2[] addedGroupCorners = groups[addedGroupID].Corners;
                                        for (int cornerID = 0; cornerID < 4; cornerID++)
                                        {
                                            boundaries = PushBoundaries(boundaries, groups[addedGroupID].rect.position + addedGroupCorners[cornerID]);
                                        }
                                    }

                                    newCandidate.wholeFootprint = new Vector2(Mathf.Abs(boundaries.x - boundaries.z), Mathf.Abs(boundaries.y - boundaries.w));
                                    newCandidate.balance = newCandidate.wholeFootprint.x / newCandidate.wholeFootprint.y;

                                    if (newCandidate.wholeFootprint.x < 0.9f * shipDrawerFlatSize && newCandidate.wholeFootprint.y < 0.9f * shipDrawerFlatSize)
                                    {
                                        if (Mathf.Abs(1 - newCandidate.balance) < Mathf.Abs(1 - bestCandidate.balance))
                                        {
                                            bestCandidate = newCandidate;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            //APPLY BEST STEP
            addedGroupIDs.Add(bestCandidate.groupID);
            ShipRectangleGroup positionedGroup = groups[bestCandidate.groupID];
            positionedGroup.rect.position = bestCandidate.position;
            positionedGroup.vertical = bestCandidate.vertical;
            groups[bestCandidate.groupID] = positionedGroup;
            footprint = bestCandidate.wholeFootprint;

            // Debug.Log("Cycle: " + i);
            // Debug.Log("Group ID: " + bestCandidate.groupID);
            // Debug.Log("Position: " + positionedGroup.rect.position);
            // Debug.Log("Vertical: " + positionedGroup.vertical);

            bestCandidate = new NextStepCandidate(0, Vector2.zero, false, Vector2.one, Mathf.Infinity);



            //RECALCULATE ATTACHMENT POINTS
            attachmentPoints = new List<AttachmentPoint>();
            foreach (int groupID in addedGroupIDs)
            {
                ShipRectangleGroup managedGroup = groups[groupID];
                for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++)
                {
                    AttachmentPoint potentialAttachmentPoint = new AttachmentPoint(Vector2.zero, new Vector2[4]);
                    potentialAttachmentPoint.position = managedGroup.rect.position + managedGroup.Corners[cornerIndex];

                    Vector2[] calculatedQuadrants = new Vector2[4];
                    for (int quadrantID = 0; quadrantID < 4; quadrantID++)
                    {
                        Vector2 quadrantDirectional = new Vector2((quadrantID == 0 || quadrantID == 1) ? 1 : -1, (quadrantID == 1 || quadrantID == 3) ? 1 : -1);
                        Vector2 size = Vector2.one * Mathf.Infinity;

                        foreach (int potentialIntersectorIndex in addedGroupIDs)
                        {
                            if (potentialIntersectorIndex == 0 && groupID == 0 && cornerIndex == 3 && quadrantID == 3)
                            {
                                Debug.Log("BREAK");
                            }

                            ShipRectangleGroup potentialIntersector = groups[potentialIntersectorIndex];
                            for (int intersectorCornerIndex = 0; intersectorCornerIndex < 4; intersectorCornerIndex++)
                            {
                                Vector2 cornerGlobalPosition = potentialIntersector.rect.position + potentialIntersector.Corners[intersectorCornerIndex];
                                Vector2 cornerPositionRelativeToAttachmentPoint = cornerGlobalPosition - potentialAttachmentPoint.position;
                                Vector2 cornerNormalizedQuadrantPosition = Vector2.Scale(cornerPositionRelativeToAttachmentPoint, quadrantDirectional);

                                if (cornerNormalizedQuadrantPosition.x >= -0.000015f && cornerNormalizedQuadrantPosition.y >= -0.000015f && cornerNormalizedQuadrantPosition.x <= size.x && cornerNormalizedQuadrantPosition.y <= size.y)
                                {
                                    Vector2 oppositeCornerNormalizedQuadrantPosition = Vector2.Scale(potentialIntersector.rect.position - potentialIntersector.Corners[intersectorCornerIndex] - potentialAttachmentPoint.position, quadrantDirectional);
                                    Vector2 sides = oppositeCornerNormalizedQuadrantPosition - cornerNormalizedQuadrantPosition;
                                    sides = sides - Vector2.Scale(new Vector2(Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.x, 0, Mathf.Abs(sides.x)), Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.y, 0, Mathf.Abs(sides.y))), new Vector2(Mathf.Sign(sides.x), Mathf.Sign(sides.y)));

                                    if (sides.y != 0 && sides.x != 0)
                                    {
                                        if (sides.x > 0)
                                        {
                                            size.x = cornerNormalizedQuadrantPosition.x < size.x ? cornerNormalizedQuadrantPosition.x : size.x;
                                        }
                                        else
                                        {
                                            size.y = cornerNormalizedQuadrantPosition.y < size.y ? cornerNormalizedQuadrantPosition.y : size.y;
                                        }
                                    }
                                }
                            }
                        }

                        //TEST
                        // Debug.Log("Cycle: " + i);
                        // Debug.Log("Group: " + groupID + " Corner: " + cornerIndex + " Quadrant: " + quadrantID);
                        // Debug.Log("Size: " + size);
                        //TEST
                        // if (groupID == 3 && cornerIndex == 2 && quadrantID == 2)
                        // {
                        //     Debug.Log("Conflicting Quadrant Size: " + size);
                        // }

                        calculatedQuadrants[quadrantID] = size;
                    }

                    potentialAttachmentPoint.quadrantSizeLimits = calculatedQuadrants;
                    attachmentPoints.Add(potentialAttachmentPoint);
                }
            }
        }


        //NORMALIZE RECTANGLE POSITIONS
        Vector2 topRightCorner = Vector2.one * Mathf.NegativeInfinity;
        Vector2 bottomLeftCorner = Vector2.one * Mathf.Infinity;
        for (int i = 0; i < groups.Length; i++)
        {
            Vector2[] corners = groups[i].Corners;
            Vector2 groupTopRightCorner = corners[2] + groups[i].rect.position;
            Vector2 groupBottomLeftCorner = corners[1] + groups[i].rect.position;

            topRightCorner.x = groupTopRightCorner.x > topRightCorner.x ? groupTopRightCorner.x : topRightCorner.x;
            topRightCorner.y = groupTopRightCorner.y > topRightCorner.y ? groupTopRightCorner.y : topRightCorner.y;

            bottomLeftCorner.x = groupBottomLeftCorner.x < bottomLeftCorner.x ? groupBottomLeftCorner.x : bottomLeftCorner.x;
            bottomLeftCorner.y = groupBottomLeftCorner.y < bottomLeftCorner.y ? groupBottomLeftCorner.y : bottomLeftCorner.y;
        }

        Vector2 positionAdjustment = (bottomLeftCorner + topRightCorner) / 2.0f;

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].rect.position -= positionAdjustment;
        }


        //POSITION SHIPS ON RECTANGLE GROUPS
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            ShipRectangleGroup group = groups[groupIndex];
            float shipSpacing = group.rect.width / group.ships.Length * 0.8f;
            float reservedSpace = shipSpacing * (group.ships.Length - 1);
            Vector3 startingPosition = new Vector3(group.rect.x, 0, group.rect.y) + (group.vertical ? Vector3.forward : Vector3.right) * (reservedSpace / 2.0f);
            Vector3 positionStep = (group.vertical ? Vector3.back : Vector3.left) * shipSpacing;
            for (int shipIndex = 0; shipIndex < group.ships.Length; shipIndex++)
            {
                Ship ship = group.ships[shipIndex];
                ship.transform.position = startingPosition + positionStep * shipIndex;
                ship.transform.rotation = new Quaternion(0, 1, 0, group.vertical ? 1 : 0);
            }

            // GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // tmp.transform.localScale = new Vector3(group.vertical ? group.rect.height : group.rect.width, 0.1f, group.vertical ? group.rect.width : group.rect.height);
            // tmp.transform.position = new Vector3(group.rect.x, 0, group.rect.y);
        }
    }

    Vector4 PushBoundaries(Vector4 boundaries, Vector2 position)
    {
        if (position.x > boundaries.x)
        {
            boundaries.x = position.x;
        }
        else if (position.x < boundaries.z)
        {
            boundaries.z = position.x;
        }

        if (position.y > boundaries.y)
        {
            boundaries.y = position.y;
        }
        else if (position.y < boundaries.w)
        {
            boundaries.w = position.y;
        }

        return boundaries;
    }

    Vector2[] CalculateCorners(Rect rect, bool invert)
    {
        float CWC = rect.width / 2.0f; //CORNER WIDTH COMPONENT
        float CHC = rect.height / 2.0f; //CORNER HEIGHT COMPONENT

        Vector2[] corners = new Vector2[] { new Vector2(-CWC, CHC), new Vector2(-CWC, -CHC), new Vector2(CWC, CHC), new Vector2(CWC, -CHC) };
        if (invert)
        {
            for (int groupCornerID = 0; groupCornerID < corners.Length; groupCornerID++)
            {
                Vector2 initialState = corners[groupCornerID];
                corners[groupCornerID] = new Vector2(Mathf.Abs(initialState.y) * Mathf.Sign(initialState.x), Mathf.Abs(initialState.x) * Mathf.Sign(initialState.y));
            }
        }

        return corners;
    }
}
