using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float distance = 15;
    public float rot = 0;
    private float roll = 30f * Mathf.PI * 2 / 360;
    private GameObject target;

    public float rotSpeed = 0.2f;

    private float maxRoll = 70f * Mathf.PI * 2 / 360;
    private float minRoll = -10f * Mathf.PI * 2 / 360;

    private float rollSpeed = 0.2f;

    public float maxDistance = 22f;
    public float minDistance = 5f;

    public float zoomSpeed = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        SetTarget(GameObject.Find("Tank"));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target == null)
        {
            return;
        }

        if(Camera.main == null)
        {
            return;
        }

        Vector3 targetPos = target.transform.position;

        Vector3 cameraPos;
        float d = distance * Mathf.Cos(roll);
        float height = distance * Mathf.Sin(roll);
        cameraPos.x = targetPos.x + d * Mathf.Cos(rot);
        cameraPos.z = targetPos.z + d * Mathf.Sin(rot);
        cameraPos.y = targetPos.y + height;
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.LookAt(target.transform);
        Rotate();
        Zoom();
    }

    void Rotate()
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;
        rot -= w;

        w = Input.GetAxis("Mouse Y") * rollSpeed * 0.5f;
        roll -= w;

        if (roll > maxRoll)
            roll = maxRoll;
        if (roll < minRoll)
            roll = minRoll;
    }

    void Zoom()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if(distance > minDistance)
            {
                distance -= zoomSpeed;
            }
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if(distance < maxDistance)
            {
                distance += zoomSpeed;
            }
        }
    }

    void SetTarget(GameObject target)
    {
        if (target.transform.Find("cameraPoint") != null)
            this.target = target.transform.Find("cameraPoint").gameObject;
        else
            this.target = target;
    }
}
