using UnityEngine;

public class GeometryFromColliderScript : MonoBehaviour
{
    public float outlineOffset;

    public float meshDepth;
    public float outlineDepth;

    public void GenerateGeometry()
    {
        var mf = GetComponent<MeshFilter>();
        var lr = GetComponent<LineRenderer>();

        var poly = GetComponent<PolygonCollider2D>();

        var mesh = new Mesh();

        // Create the Vector3 vertices
        var vertices = new Vector3[poly.points.Length];
        for (var i = 0; i < vertices.Length; i++)
            vertices[i] = new Vector3(poly.points[i].x, poly.points[i].y, meshDepth);

        var tr = new Triangulator(poly.points);
        var indices = tr.Triangulate();

        mesh.vertices = vertices;
        mesh.triangles = indices;

        if (mf) mf.mesh = mesh;

        for (var i = 0; i < vertices.Length; i++)
        {
            var prev = i > 0 ? i - 1 : vertices.Length - 1;
            var next = i < vertices.Length - 1 ? i + 1 : 0;

            var from = (vertices[i] - vertices[prev]).normalized;
            var to = (vertices[i] - vertices[next]).normalized;
            var norm = Quaternion.Euler(new Vector3(0, 0, 90)) * (to - from).normalized;

//			Debug.DrawRay (transform.position + vertices [i], norm, Color.red, 0.5f);

            vertices[i] += norm * outlineOffset;
            vertices[i].z = outlineDepth;
        }

        if (lr)
        {
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = vertices.Length;
            lr.SetPositions(vertices);
        }
    }
}