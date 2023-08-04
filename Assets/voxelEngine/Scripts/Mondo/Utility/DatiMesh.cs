using UnityEngine;
using System.Collections.Generic;   // per le List

public class DatiMesh
{
    //vertici e triangoli per la creazione della mesh
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    //uv e colori per colorare i blocchi
    public List<Vector2> uv = new List<Vector2>();
    //public List<Color32> colors = new List<Color32>();

    //vertici e triangoli per la creazione del mesh collider
    public List<Vector3> colVertices = new List<Vector3>();
    public List<int> colTriangles = new List<int>();

    //costruttore base DatiMesh
    public DatiMesh() { }

    //aggiungere i singoli vertici per fare la faccia del blocco
    public void AddVertex(Vector3 vertex, bool collisions)
    {
        vertices.Add(vertex);

        if (collisions)
        {
            AddColVertex(vertex);
        }
    }

    //aggiungere il vertice per il mesh collider
    public void AddColVertex(Vector3 vertex)
    {
        colVertices.Add(vertex);
    }

    //usa i vertici per creare i triangoli della mesh della faccia
    public void AddQuadTriangles(bool collisions)
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        if (collisions)
        {
            AddColQuadTriangles();
        }
    }

    //aggiungere i triangoli per il mesh collider
    public void AddColQuadTriangles()
    {
        colTriangles.Add(colVertices.Count - 4);
        colTriangles.Add(colVertices.Count - 3);
        colTriangles.Add(colVertices.Count - 2);
        colTriangles.Add(colVertices.Count - 4);
        colTriangles.Add(colVertices.Count - 2);
        colTriangles.Add(colVertices.Count - 1);
    }

    /*
    // passa il singolo vertice da aggiungere a triangles invece di aggiungerli tutti e 6
    public void AddTriangle(int tri)
    {
        triangles.Add(tri, bool collisions);

        if (collisions)
        {
            AddColTriangle(tri);
        }
    }

    public void AddColTriangle(int tri)
    {
        colTriangles.Add(tri - (vertices.Count - colVertices.Count));
    }
    */
}
