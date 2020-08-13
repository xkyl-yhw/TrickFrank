using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    public LayerMask grabLayer;
    public GameObject grabObject;
    public float offsetScale;
    public float maxDistance;
    [Range(1,50)]
    public float lerpSpeed;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!grabObject)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance, grabLayer))
                {
                    grabObject = hit.collider.gameObject;
                    grabObject.GetComponent<Collider>().enabled = false;
                    grabObject.GetComponent<Rigidbody>().useGravity = false;
                }
            }
            else
            {
                grabObject.GetComponent<Collider>().enabled = true;
                grabObject.GetComponent<Rigidbody>().useGravity = true;
                grabObject = null;
            }
        }
        if (grabObject)
        {
            grabObject.transform.position = Vector3.Lerp(grabObject.transform.position, Camera.main.transform.position + Camera.main.transform.forward * offsetScale, 1 / lerpSpeed);
        }
    }
}
