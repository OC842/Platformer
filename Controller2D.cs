using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{

    public LayerMask collisionMask;

    public Text Above;
    public Text Below;
    public Text Left;
    public Text Right;
    public Text Leftslope;
    public Text Rightslope;

    const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collide;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;

    void Start()
    {
        collide = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();

    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

       /* if(velocity.x != 0 && velocity.y != 0)
        {
            SlopeCollisions(ref velocity);
        }*/

        transform.Translate(velocity);
        SurfaceCollisions();
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            Vector2 Normal = hit.normal;

            if (hit)
            {
                if (Normal == Vector2.right || Normal == Vector2.left)
                {

                    SlopeCollisions(ref velocity, Normal);
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                else
                {
                    SlopeCollisions(ref velocity, Normal);

                    velocity = (hit.distance - skinWidth) * collisions.slopeUnitVector;
                    rayLength = hit.distance;

                    if (collisions.leftSlope)
                    {
                        collisions.left = false;
                    }else if(collisions.rightSlope){

                        collisions.right = false;
                    }
                    /*velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;*/
                }
            }

        }    
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin; //= (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;

            if (directionY == -1)
            {
                rayOrigin = raycastOrigins.bottomLeft;
            }
            else
            {
                rayOrigin = raycastOrigins.topLeft;
            }


            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            Vector2 Normal = hit.normal;

            if (hit)
            {
                if (Normal == Vector2.up || Normal == Vector2.down)
                {

                    SlopeCollisions(ref velocity, Normal);
                    velocity.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;

                }
                else
                {
                    SlopeCollisions(ref velocity, Normal);
                    velocity = (hit.distance - skinWidth) * collisions.slopeUnitVector;
                    rayLength = hit.distance;

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }
            }
        }
    }

    void SlopeCollisions(ref Vector3 velocity, Vector2 surfaceNormal)
    {
        Debug.Log(surfaceNormal);

        if (surfaceNormal != Vector2.up)
        {
            float xComp = surfaceNormal.x;
            float yComp = surfaceNormal.y;

            float sign = Mathf.Sign(velocity.x);

            collisions.slopeUnitVector = new Vector2(sign * yComp, -1 * sign * xComp);

            Debug.Log(collisions.slopeUnitVector);

            collisions.rightSlope = Mathf.Sign(Vector3.Cross(Vector3.up, surfaceNormal).z) == 1;
            collisions.leftSlope = Mathf.Sign(Vector3.Cross(Vector3.up, surfaceNormal).z) == -1;
            // velocity = velocity.magnitude * slopeUnitVector;
        }
        else
        {
            
        }

    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = collide.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = collide.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool leftSlope, rightSlope;
        //public bool leftSlopeVertical, rightSlopeVertical;

        public Vector2 slopeUnitVector;


        public void Reset()
        {
            above = below = false;
            left = right = false;
            leftSlope = rightSlope = false;
            //leftSlopeVertical = rightSlopeVertical = false;
        }
    }

    void SurfaceCollisions()
    {
        Above.text = "Above: " + collisions.above.ToString();
        Below.text = "Below: " + collisions.below.ToString();
        Left.text = "Left: " + collisions.left.ToString();
        Right.text = "Right: " + collisions.right.ToString();
        Leftslope.text = "LeftSlope: " + collisions.leftSlope.ToString();
        Rightslope.text = "RightSlope: " + collisions.rightSlope.ToString();

    }

}