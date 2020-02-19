//
//SpringCollider for unity-chan!
//
//Original Script is here:
//ricopin / SpringCollider.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
using UnityEngine;
using System.Collections;

public class SpringCollider : MonoBehaviour {
    public enum Shape {
        Sphere = 0,
        Capsule,
    }

    public enum Direction { X = 0, Y, Z }

    public Shape shape;
    public Vector3 center;
    public float radius = 0.5f;

    public float height = 2;
    public Direction direction = Direction.Y;

    public bool debug = true;

    Matrix4x4 localToWorldMatrix;
    Matrix4x4 worldToLocalMatrix;
    Matrix4x4 colliderMatrix;
    Matrix4x4 colliderMatrixInverse;

    void Awake() {
        UpdateColliderMatrix();
    }


    void UpdateColliderMatrix() {
        if (shape == Shape.Capsule) {
            if (direction == Direction.X)
                colliderMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, 90), Vector3.one);
            else if (direction == Direction.Y)
                colliderMatrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
            else
                colliderMatrix = Matrix4x4.TRS(center, Quaternion.Euler(90, 0, 0), Vector3.one);
            colliderMatrixInverse = colliderMatrix.inverse;
        } else {
            //Sphere模式下没有计算Inverser矩阵
            colliderMatrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
        }
    }

    void UpdateMatrix() {
#if UNITY_EDITOR
        UpdateColliderMatrix();
#endif

        localToWorldMatrix = transform.localToWorldMatrix;
        localToWorldMatrix *= colliderMatrix;

        if (shape == Shape.Capsule) {
            worldToLocalMatrix = transform.worldToLocalMatrix;
            worldToLocalMatrix = colliderMatrixInverse * worldToLocalMatrix;
        }
    }


    public bool CheckCollision(Vector3 sphereWorldPosition, float sphereRadius, out Vector3 centerWorldPos) {
        centerWorldPos = Vector3.zero;

        UpdateMatrix();

        if (shape == Shape.Capsule)
            return CheckCapsuleCollision(sphereWorldPosition, sphereRadius, ref centerWorldPos);
        else
            return CheckSphereCollision(sphereWorldPosition, sphereRadius, ref centerWorldPos);
    }

    bool CheckCapsuleCollision(Vector3 sphereWorldPosition, float sphereRadius, ref Vector3 centerWorldPos) {
        bool collision = false;
        Vector3 center = Vector3.zero;

        float capsuleHalfHeight = height / 2;

        Vector3 spherePosLocal = worldToLocalMatrix.MultiplyPoint(sphereWorldPosition);
        spherePosLocal *= this.transform.lossyScale.x;
        if (spherePosLocal.y > -capsuleHalfHeight - sphereRadius && spherePosLocal.y < capsuleHalfHeight + sphereRadius) {
            float distSq = radius + sphereRadius;
            distSq *= distSq;

            float cylinderHalfHeight = Mathf.Max(0, capsuleHalfHeight - radius);
            if (cylinderHalfHeight > 0 && spherePosLocal.y > -cylinderHalfHeight && spherePosLocal.y < cylinderHalfHeight) {
                if (Vector2.SqrMagnitude(new Vector2(spherePosLocal.x, spherePosLocal.z)) < distSq) {
                    collision = true;
                    center = new Vector3(0, spherePosLocal.y, 0);
                }
            } else {
                if (spherePosLocal.y > cylinderHalfHeight) {
                    if (Vector3.SqrMagnitude(spherePosLocal - new Vector3(0, cylinderHalfHeight, 0)) < distSq) {
                        collision = true;
                        center = new Vector3(0, cylinderHalfHeight, 0);
                    }
                } else {
                    if (Vector3.SqrMagnitude(spherePosLocal - new Vector3(0, -cylinderHalfHeight, 0)) < distSq) {
                        collision = true;
                        center = new Vector3(0, -cylinderHalfHeight, 0);
                    }
                }
            }
        }

        if (collision) {
            centerWorldPos = localToWorldMatrix.MultiplyPoint(center);
            return true;
        }

        return false;
    }

    bool CheckSphereCollision(Vector3 sphereWorldPosition, float sphereRadius, ref Vector3 centerWorldPos) {
        float distSq = radius + sphereRadius;
        distSq *= distSq;

        Vector3 colliderWorldPos = new Vector3(localToWorldMatrix.m03, localToWorldMatrix.m13, localToWorldMatrix.m23);
        if (Vector3.SqrMagnitude(colliderWorldPos - sphereWorldPosition) < distSq) {
            centerWorldPos = colliderWorldPos;
            return true;
        }

        return false;
    }


    private void OnDrawGizmos() {
        if (debug) {
            UpdateMatrix();

            Gizmos.color = Color.green;
            Gizmos.matrix = localToWorldMatrix;

            if (shape == Shape.Capsule) {
                float h = Mathf.Max(height, radius * 2);
                DrawCapsule(new Vector3(0, -h / 2, 0), new Vector3(0, h / 2, 0), Gizmos.color, radius);
            } else {
                Gizmos.DrawWireSphere(Vector3.zero, radius);
            }
        }
    }

    /// <summary>
    ///     - Draws a capsule.
    /// </summary>
    /// <param name='start'>
    ///     - The position of one end of the capsule.
    /// </param>
    /// <param name='end'>
    ///     - The position of the other end of the capsule.
    /// </param>
    /// <param name='color'>
    ///     - The color of the capsule.
    /// </param>
    /// <param name='radius'>
    ///     - The radius of the capsule.
    /// </param>
    public static void DrawCapsule(Vector3 start, Vector3 end, Color color, float radius = 1) {
        Vector3 up = (end - start).normalized * radius;
        Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 right = Vector3.Cross(up, forward).normalized * radius;

        Color oldColor = Gizmos.color;
        Gizmos.color = color;

        float height = (start - end).magnitude;
        float sideLength = Mathf.Max(0, (height * 0.5f) - radius);
        Vector3 middle = (end + start) * 0.5f;

        start = middle + ((start - middle).normalized * sideLength);
        end = middle + ((end - middle).normalized * sideLength);

        //Radial circles
        DrawCircle(start, up, color, radius);
        DrawCircle(end, -up, color, radius);

        //Side lines
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);

        Gizmos.DrawLine(start + forward, end + forward);
        Gizmos.DrawLine(start - forward, end - forward);

        for (int i = 1; i < 26; i++) {

            //Start endcap
            Gizmos.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + start, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + start, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + start, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + start);
            Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + start, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + start);

            //End endcap
            Gizmos.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + end, Vector3.Slerp(right, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + end, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + end, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + end);
            Gizmos.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + end, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + end);
        }

        Gizmos.color = oldColor;
    }
    public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f) {
        up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
        Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
        Vector3 _right = Vector3.Cross(up, _forward).normalized * radius;

        Matrix4x4 matrix = new Matrix4x4();

        matrix[0] = _right.x;
        matrix[1] = _right.y;
        matrix[2] = _right.z;

        matrix[4] = up.x;
        matrix[5] = up.y;
        matrix[6] = up.z;

        matrix[8] = _forward.x;
        matrix[9] = _forward.y;
        matrix[10] = _forward.z;

        Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        Vector3 _nextPoint = Vector3.zero;

        Color oldColor = Gizmos.color;
        Gizmos.color = (color == default(Color)) ? Color.white : color;

        for (var i = 0; i < 91; i++) {
            _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
            _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
            _nextPoint.y = 0;

            _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

            Gizmos.DrawLine(_lastPoint, _nextPoint);
            _lastPoint = _nextPoint;
        }

        Gizmos.color = oldColor;
    }
}
