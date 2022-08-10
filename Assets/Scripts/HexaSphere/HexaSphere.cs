using System;
using System.Collections.Generic;
using System.Linq;
using Gravity;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Object;

namespace Sphere
{
    [RequireComponent(typeof(MeshFilter))]
    public class HexaSphere : MonoBehaviour
    {
        [SerializeField] private float _size = 60f;
        
        [Tooltip("If the inside is bigger than the outside, increase this")]
        public float subtract = 1;
    
        [Range(0, 6)]
        public int subdivisions = 2;
        public float offset = 6;

        public Material insideMaterial;

        public Cell _cellPrefab;

        public Cell[] Cells { get; private set; }
        public UnityEvent<Cell, Vector3> OnCellShown; 
        public UnityEvent<Cell, Vector3> OnSphereClicked; 
        
        public void GenerateCells()
        {
            var stopwatch = new System.Diagnostics.Stopwatch();

            var inside = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            inside.transform.parent = transform;
            inside.transform.position = transform.position;
            inside.transform.localScale = Vector3.one * (_size-2) * 0.5f;
            
            inside.AddComponent<GravityAttractor>();
            var insideRenderer = inside.GetComponent<MeshRenderer>();
            insideRenderer.material = insideMaterial;
            insideRenderer.receiveShadows = false;
            
            Debug.Log("Generating inside(second) mesh. operation took: " + stopwatch.ElapsedMilliseconds + "ms");
            
            stopwatch.Start();
            Cells = CreateHexasphere((_size * 1 / 2), offset, subdivisions, _cellPrefab, transform, insideMaterial).ToArray();
            foreach (var cell in Cells)
            {
                cell.OnShow += OnCellShown.Invoke;
                cell.OnSpherClicked += OnSphereClicked.Invoke;
            }
            stopwatch.Stop();   
            Debug.Log("Generating first mesh. operation took: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private void OnDestroy()
        { 
            foreach (var cell in Cells)
            {
                cell.OnShow -= OnCellShown.Invoke;
            }
        }

        private static IEnumerable<Cell> CreateHexasphere(float size, float offset, int subdivisions, Cell cellPrefab, Transform parent, Material material)
        { 
            var hex = new HexagonSphere((size - offset) / 2, subdivisions, offset);
           return hex.GetCells(cellPrefab, parent, material);
        }
    }
    public class HexagonSphere
    {
        private float size;
        private List<FinalFace> finalFaces;
        private Face[] faces;
        private Vector3[] centroidPoints;
        public HexagonSphere(float size, int subdivisions, float offset)
        {
            this.size = size;
            finalFaces = new List<FinalFace>();

            var storage = new Storage();

            FillFaces();
            SubdivideFaces(subdivisions);


            foreach (var face in faces)
            {
                face.FixRadius(size);
                face.SetOffset(offset);
            }
        
            foreach (var face in faces)
            {
                face.StorePoints(storage);
            }
            finalFaces = storage.FindShapeFaces();
        }
        
        public IEnumerable<Cell> GetCells(Cell cellPrefab, Transform parent, Material material)
        {
            var faceIds = new Dictionary<FinalFace, int>();
            var cells = new Cell[finalFaces.Count];

            for (var index = 0; index < finalFaces.Count; index++)
            {
                var finalFace = finalFaces[index];
                faceIds[finalFace] = index;
                cells[index] = Instantiate(cellPrefab);
            }

            var meshes = finalFaces.Select(face => (face, face.GetMesh())).ToArray();
            FinalFaceStorage.CalculateNeighbours(meshes);

            for (var i = 0; i < meshes.Length; i++)
            {
                var (finalFace, preMesh) = meshes[i];
                var normals = new Vector3[preMesh.verts.Length];
                var neighbours = finalFace.GetNeighbours();
                var cellNeighbours = neighbours.ToDictionary(
                    neighbour => neighbour.Item1,
                    neighbour => cells[faceIds[neighbour.Item2]]);

                for (var index = 0; index < preMesh.verts.Length; index++)
                {
                    normals[index] = finalFace.GetNormal();
                }


                var cell = cells[i];
                
                cell.transform.SetParent(parent, false);
                cell.transform.rotation = Quaternion.FromToRotation(cell.transform.up, finalFace.GetNormal()) * cell.transform.rotation;
                cell.transform.localPosition = finalFace.CenterPoint;
                cell.gameObject.GetComponent<MeshRenderer>().material = material;
                var meshCollider = cell.gameObject.GetComponent<MeshCollider>();
                var meshFilter = cell.gameObject.GetComponent<MeshFilter>();
                for (var i1 = 0; i1 < preMesh.verts.Length; i1++)
                {
                    preMesh.verts[i1] = cell.transform.InverseTransformPoint(preMesh.verts[i1] + parent.transform.position);
                }

                var mesh = new Mesh
                {
                    vertices = preMesh.verts,
                    triangles = preMesh.tris,
                    normals = normals
                };
                meshCollider.sharedMesh = mesh;
                meshFilter.mesh = mesh;
                cell.Init(cellNeighbours, finalFace.CenterPoint);
            }
 
            return cells;
        }
        
        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            return rotation * (point - pivot) + pivot;
        }
    
        private void SubdivideFaces(int subdivisionAmt)
        {
            for (int i = 0; i < subdivisionAmt; i++)
            {
                var newFaces = new Face[(faces.Length * 4)];
                for (int f = 0; f < faces.Length; f++)
                {
                    var face = faces[f];
                    var subdivided = face.Subdivide();
                    for (int l = 0; l < subdivided.Length; l++)
                    {
                        newFaces[(f * 4) + l] = subdivided[l];
                    }
                }
                faces = newFaces;
            }
            centroidPoints = new Vector3[faces.Length];
            for (var i = 0; i < faces.Length; i++)
            {
                centroidPoints[i] = faces[i].GetCentroidPoint();
            }
        }

        private void FillFaces()
        {
            var vertices = new Vector3[12];
            var tau = 1.61803399f;
            vertices[0] = new Vector3(size, tau * size, 0);
            vertices[1] = new Vector3(-size, tau * size, 0);
            vertices[2] = new Vector3(size, -tau * size, 0);
            vertices[3] = new Vector3(-size, -tau * size, 0);
            vertices[4] = new Vector3(0, size, tau * size);
            vertices[5] = new Vector3(0, -size, tau * size);
            vertices[6] = new Vector3(0, size, -tau * size);
            vertices[7] = new Vector3(0, -size, -tau * size);
            vertices[8] = new Vector3(tau * size, 0, size);
            vertices[9] = new Vector3(-tau * size, 0, size);
            vertices[10] = new Vector3(tau * size, 0, -size);
            vertices[11] = new Vector3(-tau * size, 0, -size);

            faces = new Face[20];
            faces[0] = new Face(vertices[0], vertices[1], vertices[4]);
            faces[1] = new Face(vertices[1], vertices[9], vertices[4]);
            faces[2] = new Face(vertices[4], vertices[9], vertices[5]);
            faces[3] = new Face(vertices[5], vertices[9], vertices[3]);
            faces[4] = new Face(vertices[2], vertices[3], vertices[7]);
            faces[5] = new Face(vertices[3], vertices[2], vertices[5]);
            faces[6] = new Face(vertices[7], vertices[10], vertices[2]);
            faces[7] = new Face(vertices[0], vertices[8], vertices[10]);
            faces[8] = new Face(vertices[0], vertices[4], vertices[8]);
            faces[9] = new Face(vertices[8], vertices[2], vertices[10]);
            faces[10] = new Face(vertices[8], vertices[4], vertices[5]);
            faces[11] = new Face(vertices[8], vertices[5], vertices[2]);
            faces[12] = new Face(vertices[1], vertices[0], vertices[6]);
            faces[13] = new Face(vertices[11], vertices[1], vertices[6]);
            faces[14] = new Face(vertices[3], vertices[9], vertices[11]);
            faces[15] = new Face(vertices[6], vertices[10], vertices[7]);
            faces[16] = new Face(vertices[3], vertices[11], vertices[7]);
            faces[17] = new Face(vertices[11], vertices[6], vertices[7]);
            faces[18] = new Face(vertices[6], vertices[0], vertices[10]);
            faces[19] = new Face(vertices[9], vertices[1], vertices[11]);
        }
    }
    
    
    public class Storage
    {
        private Dictionary<Vector3, List<Face>> data = new();


        public void AddPoint(Vector3 point, Face face)
        {
            if (data.ContainsKey(point))
            {
                data[point].Add(face);
            }
            else
            {
                var al = new List<Face> { face };
                data.Add(point, al);
            }
        }

        public List<FinalFace> FindShapeFaces()
        {
            var finalFaces = new List<FinalFace>();
            foreach (var (key, val) in data)
            {
                var size = val.Count;
                if (size < 5) continue;
            
                var finalFace = new FinalFace(val, key);
                finalFaces.Add(finalFace);

            }
            return finalFaces;

        }
    }
    public class FinalFace
    {
        private List<Face> faces;
        private Vector3 normal;
        public Vector3 CenterPoint { get; set; }
        public Dictionary<Vector3, FinalFace> Neighbours { get; }
        
        public FinalFace(List<Face> faces, Vector3 centerPoint)
        {
            this.faces = faces;
            RearangeFaces();
            CenterPoint = centerPoint;
            normal = new Vector3(centerPoint.x, centerPoint.y, centerPoint.z).normalized; // probably buggy
            Neighbours = new Dictionary<Vector3, FinalFace>();
        }
        public Vector3 GetNormal() => normal;
        public IEnumerable<(Vector3, FinalFace)> GetNeighbours() => Neighbours.Select(keyVal=> (keyVal.Key, keyVal.Value));

        public (Vector3[] verts, int[] tris) GetMesh()
        {
            var tris = new List<int>();

            var frontFace = false;
            
            var P = faces[2].OffsetCentroid(this);
            var Q = faces[1].OffsetCentroid(this);
            var R = faces[4].OffsetCentroid(this);

            var PR = R - P;
            var PQ = Q - P;

            var cross = Vector3.Cross(PR, PQ);

        
            frontFace =  Vector3.Dot(cross, normal) > 0.005f;

            var verts = faces.Select(t => t.OffsetCentroid(this)).ToArray();


            switch (faces.Count)
            {
                case 5 when frontFace:
                    tris.Add(0);
                    tris.Add(1);
                    tris.Add(2); // first triangle

                    tris.Add(2);
                    tris.Add(3);
                    tris.Add(4); // second triangle

                    tris.Add(4);
                    tris.Add(0);
                    tris.Add(2); // third triangle
                    break;
                case 5:
                    tris.Add(0);
                    tris.Add(4);
                    tris.Add(1); // first triangle

                    tris.Add(4);
                    tris.Add(3);
                    tris.Add(2); // second triangle

                    tris.Add(2);
                    tris.Add(1);
                    tris.Add(4); // third triangle
                    break;
                case 6 when frontFace:
                    tris.Add(0);
                    tris.Add(1);
                    tris.Add(2); // first triangle

                    tris.Add(2);
                    tris.Add(3);
                    tris.Add(0); // I really don't think these comments

                    tris.Add(3);
                    tris.Add(4);
                    tris.Add(5); // are necessary anymore

                    tris.Add(5);
                    tris.Add(0);
                    tris.Add(3); // fourth triangle
                    break;
                case 6:
                    // second s.Add(0);
                    tris.Add(0);
                    tris.Add(5);
                    tris.Add(1); // first triangle

                    tris.Add(5);
                    tris.Add(4);
                    tris.Add(3); // second triangle

                    tris.Add(3);
                    tris.Add(2);
                    tris.Add(1); // third triangle

                    tris.Add(1);
                    tris.Add(5);
                    tris.Add(3); // fourth triangle
                    break;
            }

            return (verts.ToArray(), tris.ToArray());
        }

        private void RearangeFaces()
        {
            var rearanged = new List<Face> { faces[0] };
            var lastFace = faces[0];
            var firstFace = lastFace;
            faces.Remove(lastFace);
            while(faces.Count > 0)
            {
                foreach(var face in faces)
                {
                    Vector3[] lastFacePoints = {lastFace.P1, lastFace.P2, lastFace.P3};
                    var sharedPoints = 0;
                    if (lastFacePoints.Contains(face.P1))
                        ++sharedPoints;
                    if (lastFacePoints.Contains(face.P2))
                        ++sharedPoints;
                    if (lastFacePoints.Contains(face.P3))
                        ++sharedPoints;
                    if (sharedPoints != 2) continue;
                    rearanged.Add(face);
                    faces.Remove(face);
                    lastFace = face;
                    break;
                }
            }
            faces = rearanged;
        }
        
        public Vector3 OffsetToRadius(Vector3 p, float sphereRadius)
        {
            var currentDistance = (p.x * p.x) + (p.y * p.y) + (p.z * p.z);
            var adjustment = (sphereRadius * sphereRadius) / currentDistance;
            return new Vector3(p.x * adjustment, p.y * adjustment, p.z * adjustment);
        }
    }
    public class Face
    {
        private float _offset = 5;
        public Vector3 P1;
        public Vector3 P2;
        public Vector3 P3;
        
        public void SetOffset(float offset)
        {
            _offset = offset;
        }
        public Face(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            P1 = point1;
            P2 = point2;
            P3 = point3;
        }

        public Face[] Subdivide()
        {
            var m1 = new Vector3((P1.x + P2.x) / 2, (P1.y + P2.y) / 2, (P1.z + P2.z) / 2);
            var m2 = new Vector3((P2.x + P3.x) / 2, (P2.y + P3.y) / 2, (P2.z + P3.z) / 2);
            var m3 = new Vector3((P3.x + P1.x) / 2, (P3.y + P1.y) / 2, (P3.z + P1.z) / 2);

            var array = new Face[4];
            array[0] = new Face(m1, P1, m3);
            array[1] = new Face(m3, P3, m2);
            array[2] = new Face(m2, m1, m3);
            array[3] = new Face(P2, m1, m2);

            return array;
        }

        public Vector3 GetCentroidPoint()
        {
            return MultiplyVector(new Vector3((P1.x + P2.x + P3.x) / 3, (P1.y + P2.y + P3.y) / 3, (P1.z + P2.z + P3.z) / 3), 1.07f);
        }

        public Vector3 OffsetCentroid(FinalFace f)
        {
            return GetCentroidPoint() + f.GetNormal() * _offset;
        }

        private static float CorrectToRadius(float sphereRadius, Vector3 p)
        {
            var currentDistance = Mathf.Sqrt((p.x * p.x) + (p.y * p.y) + (p.z * p.z));
            var adjustment = sphereRadius / currentDistance;
            return adjustment;
        }
        public void FixRadius(float radius)
        {
            P1 = MultiplyVector(P1, CorrectToRadius(radius, P1));
            P2 = MultiplyVector(P2, CorrectToRadius(radius, P2));
            P3 = MultiplyVector(P3, CorrectToRadius(radius, P3));
        }

        private static Vector3 MultiplyVector(Vector3 point, float multiplication)
        {
            return new Vector3(point.x * multiplication, point.y * multiplication, point.z * multiplication);
        }
        public void StorePoints(Storage storage)
        {
            storage.AddPoint(P1, this);
            storage.AddPoint(P2, this);
            storage.AddPoint(P3, this);
        }
    }
    public static class FinalFaceStorage
    {
        private static void SortPair(ref (Vector3 a, Vector3 b) pair)
        {
            if (pair.a.x > pair.b. x  || (Math.Abs(pair.a.x - pair.b. x) < 1e-5 && (
                    pair.a.y > pair.b.y ||
                    (Math.Abs(pair.a.y - pair.b.y) < 1e-5  && pair.a.z > pair.b.z))))
            {
                (pair.b, pair.a) = (pair.a, pair.b);
            }
        }
        
        private static void AddToDictionary<T>(T key, IDictionary<T, int> dictionary)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key]++;
            else dictionary.Add(key, 1);
        }

        private static void SortAndAdd((Vector3, Vector3) pair, IDictionary<(Vector3, Vector3), int> dictionary)
        {
            SortPair(ref pair);
            AddToDictionary(pair, dictionary);
        }
        
        public static void CalculateNeighbours((FinalFace face, (Vector3[] verts, int[] tris))[] meshes)
        {
            var bufDictionary = new Dictionary<(Vector3, Vector3), int>();
            var neighbourEdges = new Dictionary<(Vector3, Vector3), List<FinalFace>>();

            foreach (var mesh in meshes)
            {
                var tris = mesh.Item2.tris;
                var verts = mesh.Item2.verts;
                bufDictionary.Clear();
                for (var index = 0; index < tris.Length; index+=3)
                {
                    SortAndAdd((verts[tris[index + 0]], verts[tris[index + 1]]), bufDictionary);
                    SortAndAdd((verts[tris[index + 0]], verts[tris[index + 2]]), bufDictionary);
                    SortAndAdd((verts[tris[index + 1]], verts[tris[index + 2]]), bufDictionary);
                }

                foreach (var (key,val) in bufDictionary.Where(keyVal => keyVal.Value == 1))
                {
                    if(!neighbourEdges.ContainsKey(key))
                        neighbourEdges.Add(key, new List<FinalFace>());
                    neighbourEdges[key].Add(mesh.face);
                }
            }

            var filteredNeighbourEdges = neighbourEdges.Where(pair => pair.Value.Count == 2);
            foreach (var (positions, faces) in filteredNeighbourEdges)
            {
                var neighbour1 = faces[0];
                var neighbour2 = faces[1];
                var middle = (positions.Item1 + positions.Item2) / 2;
                if(!neighbour1.Neighbours.ContainsKey(middle))
                    neighbour1.Neighbours.Add(middle, neighbour2);
                if(!neighbour2.Neighbours.ContainsKey(middle))
                    neighbour2.Neighbours.Add(middle, neighbour1);
            }
        }
    }
}