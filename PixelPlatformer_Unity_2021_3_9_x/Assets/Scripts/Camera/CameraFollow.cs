using UnityEngine;
using System.Linq;
using System.Collections.Generic;

//Follow player and stay within camera constraints
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public bool FollowPlayer = true;
    public float xMargin = 1f;                              // Distance in the x axis the Player can move before the camera follows.
    public float yMargin = 1f;                              // Distance in the y axis the Player can move before the camera follows.
    public float xSmooth = 8f;                              // How smoothly the camera catches up with it's target movement in the x axis.
    public float ySmooth = 8f;                              // How smoothly the camera catches up with it's target movement in the y axis.    
    
    private PlayerController playerController;               // Reference to player character
    private Camera cam;                                     //This object's camera component, game's main camera    
    private List<ConstraintRect> ConstraintRects;           //Rects extracted from colliders        
    private Rect cameraRect;                                //Camera's viewport as rectangle                           
    private ConstraintRect currentConstraintRect;           //Rect that is currently used to constraint camera's movement
    //private ConstraintRect switchConstraintRect;          //Candidate for next constraint rect
    private List<CameraConstraint> ConstraintColliderBoxes; //Constraint rectangles are extracted from BoxCollider2D (as triggers) for easy setup

    public void Init(PlayerController _playerController)
    {
        playerController = _playerController;
        cam = GetComponent<Camera>();
    }

    public void UpdateCameraConstraints()
    {
        //Find all constraints from scene
        ConstraintColliderBoxes = FindObjectsOfType<CameraConstraint>().ToList();

        if (!ConstraintColliderBoxes.Any())
        { //You need at least one BoxCollider2D to constraint camera movement
            Debug.LogError("No constraint BoxCollider2Ds assigned to CameraFollow!");
            return;
        }

        ConstraintRects = ConstraintBoxesToRects(ConstraintColliderBoxes);
    }

    public void WarpCameraPosition(Vector2 toPosition)
    {        
        FollowPlayer = false;

        cam.transform.position = new Vector3(toPosition.x, toPosition.y, cam.transform.position.z);
        
        ConstraintRect nearestConstraint = GetNearestConstraint(cam.transform.position);
        
        //Fit camera viewport to ConstraintRect rect
        CameraHelper.FitCameraIntoConstraintRect(cam, cam.transform.position, nearestConstraint);

        //After fitting camera into rect, set this rect to be current constraint rect
        currentConstraintRect = nearestConstraint;

        FollowPlayer = true;
    }

    private void LateUpdate()
    {        
        //If following check constraints and move camera
        if (playerController != null && FollowPlayer)
            UpdateCameraPosition();        
    }

    private void UpdateCameraPosition()
    {
        //How much we should move camera if movement is not constraint
        Vector3 camPositionDelta = GetStepTowardsPlayerPosition();

        //Move camera and check that it stays within constraints
        TranslateCamera(cam.transform.position, camPositionDelta, currentConstraintRect); //Move or constraint camera
    }

    private bool CheckXMargin()
    {
        // Returns true if the distance between the camera and the Player in the x axis is greater than the x margin.
        return Mathf.Abs(transform.position.x - playerController.transform.position.x) > xMargin;
    }

    private bool CheckYMargin()
    {
        // Returns true if the distance between the camera and the Player in the y axis is greater than the y margin.
        return Mathf.Abs(transform.position.y - playerController.transform.position.y) > yMargin;
    }

    private Vector3 GetStepTowardsPlayerPosition()
    {
        //If player doesn't move enough on this frame, the target x and y coordinates of the camera are it's current x and y coordinates.
        float targetX = cam.transform.position.x;
        float targetY = cam.transform.position.y;

        //Get step between the camera's current position and the Player's current position for smooth movement.        
        if (CheckXMargin())
            targetX = Mathf.Lerp(cam.transform.position.x, playerController.transform.position.x, Time.deltaTime * xSmooth);

        if (CheckYMargin())
            targetY = Mathf.Lerp(cam.transform.position.y, playerController.transform.position.y, Time.deltaTime * ySmooth);
            
        //Return new position delta
        return new Vector3(targetX - cam.transform.position.x, targetY - cam.transform.position.y, 0f);
    }

    private void TranslateCamera(Vector3 _camPosition, Vector3 _deltaPosition, ConstraintRect _constraintRect)
    {        
        Vector3 nextCameraPosition = _camPosition += _deltaPosition;    
        currentConstraintRect = _constraintRect = GetNextNearestConstraint(_camPosition, _deltaPosition, _constraintRect);

        //Clamp next position inside current constraint rect
        Vector3 clampedCameraPosition = new Vector3(
            Mathf.Clamp(nextCameraPosition.x, _constraintRect.Rect.xMin + cam.orthographicSize * cam.aspect, _constraintRect.Rect.xMax - cam.orthographicSize * cam.aspect),
            Mathf.Clamp(nextCameraPosition.y, _constraintRect.Rect.yMin + cam.orthographicSize, _constraintRect.Rect.yMax - cam.orthographicSize),
            nextCameraPosition.z
            );

        //Move to new valid position
        cam.transform.position = clampedCameraPosition;
    }

    //Get nearest constraint where camera center is within
    private ConstraintRect GetNearestConstraint(Vector3 _camPosition)
    {
        List<ConstraintRect> cameraCenterInRects;
        cameraCenterInRects = ConstraintRects.Where(r => CameraHelper.IsPointWithinConstraintRect(cam.transform.position, r)).ToList();

        float nearestDistance = Mathf.Infinity;
        ConstraintRect nearestRect = cameraCenterInRects.First();

        foreach (ConstraintRect rect in cameraCenterInRects)
        {
            float distanceToRectCenter = Vector3.Distance(_camPosition, rect.Rect.center);
            if (distanceToRectCenter < nearestDistance)
            {
                nearestDistance = distanceToRectCenter;
                nearestRect = rect;
            }
        }

        return nearestRect;
    }

    //Get next nearest constraint in moving direction
    private ConstraintRect GetNextNearestConstraint(Vector3 _camPosition, Vector3 _deltaPosition, ConstraintRect _constraintRect)
    {
        Vector3 nextCameraPosition = _camPosition += _deltaPosition;
        List<ConstraintRect> cameraCenterInRects;

        cameraCenterInRects = ConstraintRects.Where(r => CameraHelper.IsCameraViewportWithinConstraint(cam, nextCameraPosition, r)).ToList();
        
        if (_deltaPosition != Vector3.zero)
        {
        foreach (ConstraintRect rect in cameraCenterInRects)
            {
                float currentDistanceToRectCenter = Vector2.Distance(_camPosition, rect.Rect.center);
                float nextMoveDistanceToRectCenter = Vector2.Distance(_camPosition + _deltaPosition, rect.Rect.center);

                //Moving towards rect
                if (nextMoveDistanceToRectCenter <= currentDistanceToRectCenter)
                {
                    //currentConstraintRect = _constraintRect = rect;
                    Debug.DrawLine(nextCameraPosition, rect.Rect.center, Color.red, .1f);
                    return rect;
                }
            }
        }

        return _constraintRect;
    }

    private List<ConstraintRect> ConstraintBoxesToRects(List<CameraConstraint> constraints)
    {
        List<ConstraintRect> colliderRects = new List<ConstraintRect>();
        //Convert BoxCollider2Ds to ConstraintRects
        constraints.ForEach(constraint =>
        {
            BoxCollider2D boxCollider2D = constraint.GetComponent<BoxCollider2D>();
            //Colliders are only for measure, they need to be disabled
            boxCollider2D.enabled = false;
            //Check possible errors in constraints
            if (boxCollider2D.size.x == 1f && boxCollider2D.size.y == 1f)
                Debug.LogError("Constraint size is 1 x 1, camera can't fit within rectangle!");
            else if (boxCollider2D.transform.localScale != Vector3.one)
                Debug.LogError("Constraint object's scale is not uniform one. Have you scaled transform instead of setting collider size?");

            Rect rect = new Rect(boxCollider2D.transform.position, new Vector2(boxCollider2D.size.x, boxCollider2D.size.y));            
            rect.center = boxCollider2D.transform.position;

            ConstraintRect constraintRect = new ConstraintRect(rect, constraint.gameObject);

            //If this marked as constraintRect
            if (constraint.StartRect || currentConstraintRect == null)
            {                
                currentConstraintRect = constraintRect;
            }            

            colliderRects.Add(constraintRect);
        });

        if (currentConstraintRect == null)
            Debug.LogError("No start rect found!");

        return colliderRects;
    }
}
