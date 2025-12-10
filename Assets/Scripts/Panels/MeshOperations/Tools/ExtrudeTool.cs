// ExtrudeTool.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class ExtrudeTool : OperationTool
{
    [Header("UI Elements")]
    [SerializeField] private SliderUI slider;
    [SerializeField] private SliderUI subdivisionsSlider;
    [SerializeField] private Toggle byGroupToggle;

    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private const float Epsilon = 0.001f;
        public bool Equals(Vector3 a, Vector3 b) { return Vector3.Distance(a, b) < Epsilon; }
        public int GetHashCode(Vector3 obj) { return 0; }
    }

    private struct SpatialEdge
    {
        public int idA;
        public int idB;
        public SpatialEdge(int a, int b)
        {
            idA = Mathf.Min(a, b);
            idB = Mathf.Max(a, b);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is SpatialEdge)) return false;
            SpatialEdge other = (SpatialEdge)obj;
            return idA == other.idA && idB == other.idB;
        }
        public override int GetHashCode() { return idA * 397 ^ idB; }
    }

    public void Extrude()
    {
        if (!CanPerformTool()) return;

        var selection = SelectionManager.Instance.GetFirstSelected();
        ProBuilderMesh mesh = selection.GetEditModel();

        List<Face> selectedFaces = ModelEditingPanel.Instance.GetControlPoints()
            .Select(cp => ((FaceControlPoint)cp).GetFace()).ToList();

        if (mesh == null || selectedFaces.Count == 0) return;

        List<Vector3> positions = new List<Vector3>(mesh.positions);
        List<Face> faces = new List<Face>(mesh.faces);

        float totalDist = slider.GetValueAsFloat();
        int subdivisions = (int)subdivisionsSlider.GetValueAsFloat() + 1;
        float distPerSub = totalDist / subdivisions;

        //SPATIAL MAPPING
        Dictionary<Vector3, int> posToID = new Dictionary<Vector3, int>(new Vector3Comparer());
        Dictionary<int, int> rawToUnique = new Dictionary<int, int>();
        int uniqueCounter = 0;

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            if (!posToID.ContainsKey(pos))
                posToID.Add(pos, uniqueCounter++);
            rawToUnique.Add(i, posToID[pos]);
        }

        //EXTRUSION LOOP
        for (int sub = 0; sub < subdivisions; sub++)
        {
            List<Face> nextStepFaces = new List<Face>();
            Dictionary<int, int> globalOldUniqueToNewRaw = new Dictionary<int, int>();
            Dictionary<SpatialEdge, int> edgeCounts = new Dictionary<SpatialEdge, int>();

            //Count Edges
            foreach (Face face in selectedFaces)
            {
                IList<int> idxs = face.indexes;
                for (int k = 0; k < idxs.Count; k += 3)
                {
                    AddEdgeCount(edgeCounts, rawToUnique[idxs[k]], rawToUnique[idxs[k + 1]]);
                    AddEdgeCount(edgeCounts, rawToUnique[idxs[k + 1]], rawToUnique[idxs[k + 2]]);
                    AddEdgeCount(edgeCounts, rawToUnique[idxs[k + 2]], rawToUnique[idxs[k]]);
                }
            }

            //Extrude Faces
            foreach (Face face in selectedFaces)
            {
                Vector3 normal = Vector3.zero;
                IList<int> idxs = face.indexes;
                for (int k = 0; k < idxs.Count; k += 3)
                {
                    Vector3 p0 = positions[idxs[k]];
                    Vector3 p1 = positions[idxs[k + 1]];
                    Vector3 p2 = positions[idxs[k + 2]];
                    normal += Vector3.Cross(p1 - p0, p2 - p0);
                }
                normal.Normalize(); 

                for (int k = 0; k < idxs.Count; k += 3)
                {
                    CheckAndBuildWall(idxs[k], idxs[k + 1], normal, distPerSub, edgeCounts, rawToUnique, positions, faces);
                    CheckAndBuildWall(idxs[k + 1], idxs[k + 2], normal, distPerSub, edgeCounts, rawToUnique, positions, faces);
                    CheckAndBuildWall(idxs[k + 2], idxs[k], normal, distPerSub, edgeCounts, rawToUnique, positions, faces);
                }

                List<int> newFaceIndices = new List<int>();
                foreach (int oldRawIdx in face.indexes)
                {
                    int uniqueID = rawToUnique[oldRawIdx];
                    if (!globalOldUniqueToNewRaw.ContainsKey(uniqueID))
                    {
                        positions.Add(positions[oldRawIdx] + (normal * distPerSub));
                        int newRawIdx = positions.Count - 1;
                        globalOldUniqueToNewRaw.Add(uniqueID, newRawIdx);

                        int newUniqueID = uniqueCounter++;
                        if (!rawToUnique.ContainsKey(newRawIdx)) rawToUnique.Add(newRawIdx, newUniqueID);
                    }
                    newFaceIndices.Add(globalOldUniqueToNewRaw[uniqueID]);
                }

                face.SetIndexes(newFaceIndices);
                nextStepFaces.Add(face);
            }
            selectedFaces = nextStepFaces;
        }

        mesh.positions = positions;
        mesh.faces = faces;

        Dictionary<Vector3, int> lookup = new Dictionary<Vector3, int>(new Vector3Comparer());
        List<SharedVertex> newShared = new List<SharedVertex>();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            if (lookup.ContainsKey(pos))
                newShared[lookup[pos]].Add(i);
            else
            {
                newShared.Add(new SharedVertex(new int[] { i }));
                lookup.Add(pos, newShared.Count - 1);
            }
        }
        mesh.sharedVertices = newShared;

        //WeldAllVertices(mesh);

        mesh.ToMesh();
        mesh.Refresh();
        selection.UpdateMeshEdit();
        ModelEditingPanel.Instance.UpdateEditModel();
    }


    ///// <summary>
    ///// Welds coincident vertices together based on distance.
    ///// </summary>
    //public void WeldAllVertices(ProBuilderMesh mesh)
    //{

    //    IEnumerable<int> allIndices = mesh.faces.SelectMany(f => f.distinctIndexes);

    //    VertexEditing.WeldVertices(mesh, allIndices, 0.0001f);


    //}


    private void AddEdgeCount(Dictionary<SpatialEdge, int> counts, int u1, int u2)
    {
        if (u1 == u2) return;
        SpatialEdge key = new SpatialEdge(u1, u2);
        if (counts.ContainsKey(key)) counts[key]++;
        else counts.Add(key, 1);
    }

    private void CheckAndBuildWall(int rawA, int rawB, Vector3 normal, float dist,
                                   Dictionary<SpatialEdge, int> counts, Dictionary<int, int> rawToUnique,
                                   List<Vector3> positions, List<Face> faces)
    {
        int u1 = rawToUnique[rawA];
        int u2 = rawToUnique[rawB];
        if (u1 == u2) return;

        if (counts[new SpatialEdge(u1, u2)] == 1)
        {
            positions.Add(positions[rawA]); 
            positions.Add(positions[rawB]);
            positions.Add(positions[rawA] + (normal * dist)); 
            positions.Add(positions[rawB] + (normal * dist)); 

            int v = positions.Count;


            faces.Add(new Face(new int[] {
                v-4, v-2, v-1, 
                v-4, v-1, v-3    
            }));
        }
    }

    public override bool CanShowTool() => ModelEditingPanel.Instance.GetEditMode() == EditMode.Face;
    public override bool CanPerformTool() => ModelEditingPanel.Instance.GetControlPoints().Count > 0 && ModelEditingPanel.Instance.GetEditMode() == EditMode.Face;
}