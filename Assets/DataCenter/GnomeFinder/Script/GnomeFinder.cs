using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GnomeFinder : MonoBehaviour
{

    public List<Mesh> Segments;
    public Vector3 ScannerOfset;
    public Quaternion cameraRotationOffset;

    // Start is called before the first frame update
    void Start()
    {
        ScannerOfset =  new Vector3(0, -0.25f, 0);
        cameraRotationOffset = Quaternion.Euler(new Vector3(0f, 0, 0));
        Segments = new List<Mesh>();
        // Create Vector2 vertices
        var startAngle = 45f;
        var moveAngle = 0.1f;
        var margin = -0.08f;
        var sMargin = 0.01f;
        
        var start = 0.1f;
        var len = 0.05f;

        for (int f = 0; f < 10; f++)
        {
            for (int s = 0; s < 8; s++)
            {
                var sA = startAngle;
                var ma = moveAngle * f;
                Vector2 aDir = new Vector2(Mathf.Cos(startAngle + ma - margin),              Mathf.Sin(startAngle + ma - margin));
                Vector2 bDir = new Vector2(Mathf.Cos(startAngle + ma +  moveAngle + margin), Mathf.Sin(startAngle + ma + moveAngle + margin));

                var sS = start + (len * s);
                var vertices2D = new Vector2[] {
                    new Vector2(0,0) + (aDir * (sS + sMargin)),
                    new Vector2(0,0) + (bDir * (sS + sMargin)),
                    new Vector2(0,0) + (bDir * (sS + len - sMargin)),
                    new Vector2(0,0) + (aDir * (sS + len - sMargin))
                };
                RenderMesh(vertices2D);
            }
        }

        
    }

    private void RenderMesh(Vector2[] vertices2D)
    {
        var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);

        // Use the triangulator to get indices for creating triangles
        var triangulator = new Triangulator(vertices2D);
        var indices = triangulator.Triangulate();

        // Generate a color for each vertex
        var colors = Enumerable.Range(0, vertices3D.Length)
            .Select(i => Color.gray)
            .ToArray();

        // Create the mesh
        var mesh = new Mesh
        {
            vertices = vertices3D,
            triangles = indices,
            colors = colors
        };
        Segments.Add(mesh);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var gm = Instantiate<GameObject>(new GameObject(),transform);
        gm.name = "scannerSegment";
        var meshRenderer = gm.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        var filter = gm.AddComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    void SetSegment(int segment, int row, bool active)
    {
        var rowS = (9 - row) * 8;

        for (int i = 0; i < 8; i++)
        {
            var sIndex = i + rowS;
            Segments[sIndex].colors = Segments[sIndex].colors.Select(s => {
                if (active)
                {
                    if (i > segment)
                    {
                        //return Color.black;
                        return (new List<Color> { Color.black, Color.grey })
                        .OrderBy(y => Guid.NewGuid())
                        .FirstOrDefault();

                    } else
                    {
                        return Color.grey;
                    }
                    
                }
                else
                {
                    return Color.grey;
                }

            }).ToArray();
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        var maxDistance = 15f;

        var scannerPos = transform.position + ScannerOfset;
        var rotationCorrection = -Weapons.GnomeFinder().WeaponLocalRoration;

        
        var correction = Quaternion.Euler(rotationCorrection) * transform.forward;
        var ScannerDirection = correction.normalized;

        var layer = 1 << LayerMask.NameToLayer("GnomeCollider");

        var offSet = -5;
        var offsetMultiplayer = 1.5f;
        for (int i = 0; i < 10; i++)
        {
            var ray = new Ray(scannerPos,Quaternion.AngleAxis((offsetMultiplayer * offSet) + (offsetMultiplayer * i), transform.up) * ScannerDirection * maxDistance);
            var hit = Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance, layer);
            if (hit)
            {
                var distance = Vector3.Distance(scannerPos, raycastHit.point);
                Debug.DrawRay(scannerPos, Quaternion.AngleAxis((offsetMultiplayer * offSet) + (offsetMultiplayer * i), transform.up) * ScannerDirection * distance, Color.yellow);
                Debug.Log(raycastHit.distance);

                var segments = (int)Mathf.Floor((raycastHit.distance / maxDistance) * 8f);
                SetSegment(segments, i, hit);
            }
            else
            {
                Debug.DrawRay(scannerPos, Quaternion.AngleAxis((offsetMultiplayer * offSet) + (offsetMultiplayer * i), transform.up) * ScannerDirection * maxDistance, Color.magenta);
                SetSegment(0, i, hit);
            }
            
        }

    }
}


public class Triangulator
{
    private List<Vector2> m_points = new List<Vector2>();

    public Triangulator(Vector2[] points)
    {
        m_points = new List<Vector2>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area()
    {
        int n = m_points.Count;
        float A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x;
        ay = C.y - B.y;
        bx = A.x - C.x;
        by = A.y - C.y;
        cx = B.x - A.x;
        cy = B.y - A.y;
        apx = P.x - A.x;
        apy = P.y - A.y;
        bpx = P.x - B.x;
        bpy = P.y - B.y;
        cpx = P.x - C.x;
        cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}