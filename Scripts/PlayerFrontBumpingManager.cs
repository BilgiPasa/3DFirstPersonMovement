using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class PlayerFrontBumpingManager : MonoBehaviour
{
    //* Attach this script to the FrontBumpingDetector game object.
    //* In Unity Editor, layer 3 should be "Static Normal Layer".
    //* In Unity Editor, layer 6 should be "Static Bouncy Layer".

    public static bool frontBumping;
    BoxCollider frontBumpingDetectorBoxCollider;

    void Start()
    {
        frontBumpingDetectorBoxCollider = GetComponent<BoxCollider>();
        frontBumpingDetectorBoxCollider.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6)
        {
            frontBumping = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6)
        {
            frontBumping = false;
        }
    }
}
