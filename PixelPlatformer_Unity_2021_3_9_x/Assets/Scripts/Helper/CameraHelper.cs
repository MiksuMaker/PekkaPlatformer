using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraHelper
{
    //Check if camera rect fits inside constraint rect
    public static bool IsCameraViewportWithinConstraint(Camera cam, Vector3 nextCameraPosition, ConstraintRect constraintRect)
    {
        Rect cameraRect = GetCameraRect(cam, nextCameraPosition);

        float allowedRoundingError = 0.03f;

        bool x1 = cameraRect.xMin >= constraintRect.Rect.xMin - allowedRoundingError;
        bool x2 = cameraRect.xMax <= constraintRect.Rect.xMax + allowedRoundingError;
        bool y1 = cameraRect.yMin >= constraintRect.Rect.yMin - allowedRoundingError;
        bool y2 = cameraRect.yMax <= constraintRect.Rect.yMax + allowedRoundingError;

        if (x1 && x2 && y1 && y2) //if within bounds return rect to switch
            return true;
        else
            return false;
    }

    public static void FitCameraIntoConstraintRect(Camera cam, Vector3 nextCameraPosition, ConstraintRect constraintRect)
    {
        Rect cameraRect = GetCameraRect(cam, nextCameraPosition);
        Vector3 correctionOffset = new Vector3();

        if (cameraRect.xMin < constraintRect.Rect.xMin)
            correctionOffset.x = constraintRect.Rect.xMin - cameraRect.xMin;
        else if (cameraRect.xMax > constraintRect.Rect.xMax)
            correctionOffset.x = constraintRect.Rect.xMax - cameraRect.xMax;

        if (cameraRect.yMin < constraintRect.Rect.yMin)
            correctionOffset.y = constraintRect.Rect.yMin - cameraRect.yMin;
        else if (cameraRect.yMax > constraintRect.Rect.yMax)
            correctionOffset.y = constraintRect.Rect.yMax - cameraRect.yMax;

        cam.transform.position += correctionOffset;
    }

    public static bool IsPointWithinConstraintRect(Vector2 point, ConstraintRect constraintRect)
    {
        //Is point within constraintRect
        return constraintRect.Rect.Contains(point);
    }

    public static bool IsPointWithinViewport(Camera cam, Vector3 nextCameraPosition, Vector2 point)
    {
        Rect cameraRect = GetCameraRect(cam, nextCameraPosition);

        //Is point within camera viewport
        return cameraRect.Contains(point);
    }

    public static Rect GetCameraRect(Camera cam, Vector3 nextCameraPosition)
    {
        Rect cameraRect = new Rect(nextCameraPosition, new Vector2(cam.orthographicSize * 2f * cam.aspect, cam.orthographicSize * 2f));
        cameraRect.center = nextCameraPosition;
        return cameraRect;
    }
}

//ConstraintRect class to create nullable rect (for CheckConstraints() method) instead of using Rect directly
public class ConstraintRect
{
    public Rect Rect;
    public GameObject GameObject;

    public ConstraintRect(Rect _rect, GameObject _gameObject)
    {
        Rect = _rect;
        GameObject = _gameObject;
    }
}
