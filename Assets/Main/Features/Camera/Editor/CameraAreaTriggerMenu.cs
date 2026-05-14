using UnityEditor;
using UnityEngine;

public static class CameraAreaTriggerMenu
{
    [MenuItem("GameObject/Fixed Perspective Camera/Camera Area Trigger", false, 10)]
    private static void CreateCameraAreaTrigger(MenuCommand menuCommand)
    {
        GameObject triggerObject = new GameObject("Camera Area Trigger");
        BoxCollider boxCollider = triggerObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector3(4f, 2.5f, 4f);

        triggerObject.AddComponent<CameraAreaTrigger>();
        PlaceObject(triggerObject, menuCommand);
    }

    [MenuItem("GameObject/Fixed Perspective Camera/Camera Controller", false, 11)]
    private static void CreateCameraController(MenuCommand menuCommand)
    {
        GameObject cameraObject = new GameObject("Fixed Perspective Camera");
        cameraObject.AddComponent<UnityEngine.Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<FixedPerspectiveCameraController>();
        PlaceObject(cameraObject, menuCommand);
    }

    private static void PlaceObject(GameObject gameObject, MenuCommand menuCommand)
    {
        GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
        Selection.activeObject = gameObject;
    }
}
