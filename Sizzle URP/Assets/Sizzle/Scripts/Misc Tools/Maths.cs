using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Maths
{
    public struct Shape
    {
        private Vector3 root;
        private List<Vector3> vertices;

        public Vector3 Root { get { return root; } }

        public List<Vector3> Verticies { get { return vertices; } }


        public Shape(Vector3 root, List<Vector3> vertices)
        {
            this.root = root;
            this.vertices = vertices;
        }
    }

    public static bool ShapeContainsPoint(Vector3 point, Shape shape)
    {
        // Check if there are enough verticies
        if(shape.Verticies.Count <= 2)
        {
            return false;
        }

        // Project to xz plane - Won't work if quad is directly up but it should not be in that situation 
        List<Vector3> projectedVerticies = new List<Vector3>();
        foreach (Vector3 vertex in shape.Verticies)
        {
            projectedVerticies.Add(Vector3.ProjectOnPlane(vertex, Vector3.up));
        }



        return true;
    }

    public static Vector3 GetIntersectionPoint(Vector3 originA, Vector3 offsetFromOriginA, Vector3 originB, Vector3 offsetFromOriginB)
    {

        // Now we have the closest side towards the origin 
        // Since we know for a fact that when this is called there is an intersection we can find
        // it here 

        // startA.x + t * offsetA.x = startB.x + u * offsetB.x
        // startA.z + t * offsetA.z = startB.z + u * offsetB.z

        // offsetA.x(t) - offsetB.x(u) = startB.x - startA.x
        // offsetA.z(t) - OffsetB.z(u) = startB.z - startA.z

        float[,] matrix2x2 = new float[2, 2];

        matrix2x2[0, 0] = offsetFromOriginA.x; // A
        matrix2x2[1, 0] = -offsetFromOriginB.x;// B
        matrix2x2[0, 1] = offsetFromOriginA.z; // C
        matrix2x2[1, 1] = -offsetFromOriginB.z;// D

        // Vector on other side of equation 
        Vector2 B = new Vector2(originB.x - originA.x, originB.z - originA.z);


        // Inverses the matrix 
        InverseMatrix2x2(matrix2x2);

        // Matrix x Vector 
        // (m[0, 0] * B[0]) + (m[0, 1] * b[1]) = t 
        // (m[1, 0] * B[0]) + (m[1, 1] * b[1]) = u

        float t = (matrix2x2[0, 0] * B[0]) + (matrix2x2[0, 1] * B[1]);
        float u = (matrix2x2[1, 0] * B[0]) + (matrix2x2[1, 1] * B[1]);

        // Makes sure that it actually leads to the correct point 
        if (((originA + offsetFromOriginA * t) - (originB + offsetFromOriginB * u)).magnitude <= 0.01f)
        {
            // Unless they intersect direction at (0, 0, 0) this should work 
            return Vector3.zero;
        }

        return originA + offsetFromOriginA * t;
    }

    /// <summary>
    /// Updates the inputed matrix to be its inverse 
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="det"></param>
    public static void InverseMatrix2x2(float[,] matrix)
    {
        float det = (matrix[0, 0] * matrix[1, 1]) - (matrix[1, 0] * matrix[0, 1]);

        // Swap A and D 
        float hold = matrix[0, 0];
        matrix[0, 0] = matrix[1, 1] / det;
        matrix[1, 1] = hold / det;

        // Make B and C negative 
        matrix[1, 0] = -matrix[1, 0] / det;
        matrix[0, 1] = -matrix[0, 1] / det;
    }


    /// <summary>
    /// If a point is within the plane formed by this quad.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static bool PlaneContainsPoint(Transform[] vertexPoints, Vector3 planeNormal, Vector3 point)
    {

        // See if within bounds of points 

        // At this stage every point has been projected onto the plane 

        // Solve by splitting the plane into two triangles and finding if the point is within them 
        Vector3[] triangleA = new Vector3[]
        {
            Vector3.ProjectOnPlane(vertexPoints[0].position, planeNormal),
            Vector3.ProjectOnPlane(vertexPoints[1].position, planeNormal),
            Vector3.ProjectOnPlane(vertexPoints[2].position, planeNormal)
        };


        Vector3 AB = triangleA[0] - triangleA[1];
        Vector3 AM = triangleA[0] - point;
        Vector3 BC = triangleA[1] - triangleA[2];
        Vector3 BM = triangleA[1] - point;

        float dotABAM = Vector3.Dot(AB, AM);
        float dotABAB = Vector3.Dot(AB, AB);
        float dotBCBM = Vector3.Dot(BC, BM);
        float dotBCBC = Vector3.Dot(BC, BC);

        return 0 <= dotABAM && dotABAM <= dotABAB && 0 <= dotBCBM && dotBCBM <= dotBCBC;
    }

    public static float DetArea(Vector2 A, Vector2 B, Vector2 C)
    {
        return (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);
    }

    public static Vector3 RotateVectorXZ(Vector3 vector, float angle)
    {
        return new Vector3
            (
                vector.x * Mathf.Cos(angle) - vector.z * Mathf.Sin(angle),
                0,
                vector.x * Mathf.Sin(angle) + vector.z * Mathf.Cos(angle)
            );
    }

    public static Vector3 RotateVectorOnXAxis(Vector3 vector, float angle)
    {
        return new Vector3
            (
                vector.x,
                vector.y * Mathf.Cos(angle) * vector.z * Mathf.Sin(angle),
                vector.y * -Mathf.Sin(angle) * vector.z * Mathf.Cos(angle)
            );
    }

    public static Vector3 ProjectToXZPlane(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sideLengthA">Length of one side adjacent to angle</param>
    /// <param name="sideLengthB">Length of one side adjacent to angle</param>
    /// <param name="sideLengthC">Length of one side opposite to angle</param>
    /// <returns></returns>
    public static float CosineRule(float sideLengthA, float sideLengthB, float sideLengthC)
    {
        float numerator = Mathf.Pow(sideLengthA, 2) + Mathf.Pow(sideLengthB, 2) - Mathf.Pow(sideLengthC, 2);
        float denominator = 2 * sideLengthA * sideLengthB;

        return Mathf.Acos(numerator / denominator);
    }

    public static float Roof(float value)
    {
        return Mathf.Floor(value + 1);
    }

    /// <summary>
    /// Checks if a given value is equal to a check within a certain tolerance 
    /// </summary>W
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static bool EqualWithinRange(float A, float B, float tolerance)
    {
        return Mathf.Abs(A - B) <= tolerance;
    }

    public static Vector3 MulVecElements(Vector3 A, Vector3 B)
    {
        // I'm sure there is an actual name for this I just can't find it
        // ( I am dumby :< )
        return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
    }

    public static float InverseLerp(Vector4 a, Vector4 b, Vector4 value)
    {
        Vector4 AB = b - a;
        Vector4 AV = value - a;
        return Vector4.Dot(AV, AB) / Vector4.Dot(AB, AB);
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public static float InverseLerp(Vector2 a, Vector2 b, Vector2 value)
    {
        Vector2 AB = b - a;
        Vector2 AV = value - a;
        return Vector2.Dot(AV, AB) / Vector2.Dot(AB, AB);
    }


    public static Vector2 Proj2D(Vector2 u, Vector2 v)
    {
        return (Vector2.Dot(u, v) / (u.magnitude * v.magnitude)) * v;
    }
    
    /// <summary>
    /// Get a set of points that form a cube from origin with a specified size
    /// </summary>
    /// <returns></returns>
    public static Vector3[] FormPlaneFromSize(Vector2 size)
    {
        return new Vector3[]
        {
            new Vector3(size.x / 2, 0, size.y / 2),
            new Vector3(size.x / 2, 0, -size.y / 2),
            new Vector3(-size.x / 2, 0, -size.y / 2),
            new Vector3(-size.x / 2, 0, size.y / 2)
        };
    }

    public static float[,] CrossProduct(float[,] A, float[,] B)
    {

        // Sizes do not match up
        if(A.GetLength(1) == B.GetLength(0))
        {
            return new float[,] { { 0,0} };
        }

        float[,] P = new float[A.GetLength(0), B.GetLength(1)];
        for (int i = 0; i < A.GetLength(1); i++)
        {
            for (int j = 0; j < B.GetLength(0); j++)
            {
                // Initializes the value 
                P[i, j] = 0;
                for (int k = 0; k < A.GetLength(0); k++)
                {
                    P[i, j] += A[i, k] * B[k, j];
                }
            }
        }

        return P;
    }

    public static bool IsPointWithinRect(Vector3 point, Vector3[] rect)
    {
        // Turns values into easier to use 2d vectors 
        Vector2 point2D = new Vector2(point.x, point.z);
        Vector2[] rect2D = new Vector2[rect.Length];
        for (int i = 0; i < rect.Length; i++)
        {
            rect2D[i] = new Vector2(rect[i].x, rect[i].z);
        }

        // Find division value between two pairs of the projected point
        // CANNOT be diagonal lines being used 

        // If the point is greater than side then it should be outta the line
        // Do twice to check both sides

        Vector2 A = rect[0] - rect[1];
        Vector2 B = rect[0] - rect[3];

        float[,] matrix = { { A.x, A.y }, { B.x, B.y } };
        InverseMatrix2x2(matrix);

        float[,] P = CrossProduct(matrix, new float[,] { {point2D.x, point2D.y} } );

        return (P[0, 0] <= 1) && (P[1, 0] <= 1);
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public static Vector3 MirrorOnY(Vector3 vec)
    {
        return new Vector3(-vec.x, vec.y, vec.z);
    }
}
