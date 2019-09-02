using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnomeFinder : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var maxDistance = 15f;
 
        var layer = 1 << LayerMask.NameToLayer("Gnome");


        for (int i = -5; i < 10; i++)
        {
            RaycastHit RaycastHit;
            var ray = new Ray(transform.position, Quaternion.AngleAxis(i, transform.up) * transform.forward * maxDistance);
            var hit = Physics.Raycast(ray, out RaycastHit, maxDistance, layer);
            if (hit)
            {
                var distance = Vector3.Distance(transform.position, RaycastHit.point);
                Debug.DrawRay(transform.position, Quaternion.AngleAxis(i, transform.up) * transform.forward * distance, Color.yellow);
                Debug.Log(RaycastHit.distance);
            }
            else
            {
                //Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.magenta);

                Debug.DrawRay(transform.position, Quaternion.AngleAxis(i, transform.up) * transform.forward * maxDistance, Color.magenta);

            }
            
        }

    }
}
