using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Tank tank;
    public enum Status
    {
        Patrol,
        Attack,
        Flee
    }
    private Status status = Status.Patrol;

    private GameObject target;
    private float sightDistance = 30;
    private float lastSearchTargetTime = 0;
    private float searchTargetInterval = 3;

    private Path path = new Path();

    private float lastUpdateWaypointTime = float.MinValue;
    private float updateWaypointInterval = 3;

    private Vector3 lastPetrolPos;
    public void ChangeStatus(Status status)
    {
        this.status = status;
        Debug.Log(status);
        if (status == Status.Patrol)
        {
            PatrolStart();
        }
        else if (status == Status.Attack)
        {
            AttackStart();
        }
        else if (status == Status.Flee)
        {
            FleeStart();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        InitWaypoint();
        lastPetrolPos = tank.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (tank.ctrlType != Tank.CtrlType.computer)
            return;
        TargetUpdate();
        if (status == Status.Patrol)
        {
            PatrolUpdate();
        }
        else if (status == Status.Attack)
        {
            AttackUpdate();
        }
        else if(status == Status.Flee)
        {
            FleeUpdate();
        }

        if (path.IsReach(transform))
        {
            path.NextWaypoint();
        }
    }

    void PatrolStart()
    {
        GameObject obj = GameObject.Find("WaypointContainer");
        {
            int count = obj.transform.childCount;
            if (count == 0) return;
            int index = Random.Range(0, count);
            Vector3 targetPos = obj.transform.GetChild(index).position;
            path.InitByNavMeshPath(transform.position, targetPos);
        }
    }

    void AttackStart(){
        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    void FleeStart()
    {
        Vector3 hideSpot;
        float shortestLength = float.MinValue;
        GameObject obj = GameObject.Find("WaypointContainer");
        int count = obj.transform.childCount;
        if (count == 0) return;
        hideSpot = obj.transform.GetChild(Random.Range(0, count)).position;
        for(int i = 0; i < count; i++)
        {
            Vector3 spot = obj.transform.GetChild(i).position;
            if (!HasSight(spot))
            {
                float length = CalculatePathLength(spot);
                if(length < shortestLength)
                {
                    hideSpot = spot;
                }
            }
        }
        path.InitByNavMeshPath(transform.position, hideSpot);
    }

    void PatrolUpdate()
    {
        if (target != null)
            ChangeStatus(Status.Attack);

        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval)
            return;
        lastUpdateWaypointTime = Time.time;

        if (path.waypoints == null || path.isFinish || (tank.transform.position - lastPetrolPos).magnitude < 2f)
        {
            PatrolStart();
        }
        lastPetrolPos = tank.transform.position;
    }

    void AttackUpdate()
    {
        if (target == null)
        {
            ChangeStatus(Status.Patrol);
            return;
        }
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval)
            return;
        lastUpdateWaypointTime = Time.time;

        Vector3 targetPos = target.transform.position;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    void FleeUpdate()
    {
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval)
            return;
        lastUpdateWaypointTime = Time.time;
        if (path.waypoints == null || path.isFinish)
        {
            ChangeStatus(Status.Patrol);
        }
    }

    void TargetUpdate()
    {
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval)
            return;
        lastSearchTargetTime = Time.time;

        if (target != null)
            HasTarget();
        else
            NoTarget();
    }

    void HasTarget()
    {
        Tank targetTank = target.GetComponent<Tank>();
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        if(targetTank.ctrlType == Tank.CtrlType.none)
        {
            Debug.Log("目标死亡，丢失目标");
            target = null;
        }
        else if (Vector3.Distance(pos, targetPos) > sightDistance)
        {
            Debug.Log("距离过远，丢失目标");
            target = null;
        }
    }

    void NoTarget()
    {
        float minHp = float.MaxValue;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tank");
        for(int i = 0; i < targets.Length; i++)
        {
            Tank tank = targets[i].GetComponent<Tank>();
            if (tank == null)
                continue;
            if (targets[i] == gameObject)
                continue;
            Vector3 pos = transform.position;
            Vector3 targetPos = targets[i].transform.position;
            if (Vector3.Distance(pos, targetPos) > sightDistance)
                continue;
            if (minHp > tank.hp && tank.hp > 0)
                target = tank.gameObject;
        }

        if (target != null)
            Debug.Log("获取目标" + target.name);
    }

    public void OnAttacked(GameObject attackTank)
    {
        target = attackTank;
        ChangeStatus(Status.Flee);
    }

    public Vector3 GetTurretTarget()
    {
        if (target == null)
        {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0, y, 0);
            return rot;
        }
        else
        {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
    }

    public bool IsShoot()
    {
        if(target == null || this.status != Status.Attack)
        {
            return false;
        }
        float turretRoll = tank.turret.eulerAngles.y;
        float angle = turretRoll - GetTurretTarget().y - 180;
        if (angle < 0) 
            angle += 360;
        if (angle < 30 || angle > 330)
            return true;
        else
            return false;
    }

    void InitWaypoint()
    {
        GameObject obj = GameObject.Find("WaypointContainer");
        if(obj && obj.transform.GetChild(0) != null)
        {
            Vector3 targetPos = obj.transform.GetChild(0).position;
            path.InitByNavMeshPath(transform.position, targetPos);
        }
    }

    public float GetSteering()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        //Debug.Log(itp);
        itp *= 1 / itp.z;
        if (itp.x > path.deviation / 10)
            return tank.maxSteeringAngle;
        else if (itp.x < -path.deviation / 10)
            return -tank.maxSteeringAngle;
        else
            return 0;
    }

    public float GetMotor()
    {
        if (tank == null)
            return 0;
        Vector3 itp = transform.InverseTransformPoint(path.waypoint);
        float x = itp.x;
        float z = itp.z;
        float r = 0.15f;
        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.maxMotorTorque;
        else return tank.maxMotorTorque;
    }

    public float GetBrakeTorque()
    {
        if (path.isFinish)
            return tank.maxMotorTorque;
        else
            return 0;
    }

    void OnDrawGizmos()
    {
        path.DrawWaypoints();
    }

    bool HasSight(Vector3 spot)
    {
        //spot.y += 3;
        //Vector3 enemy = target.transform.position;
        //enemy.y += 3;
        Vector3 direction = spot - target.transform.position;
        Ray aimLine = new Ray(spot, direction * 9999);

        RaycastHit hitInfo;
        if(Physics.Raycast(aimLine, out hitInfo, 99999f))
        {
            if (hitInfo.transform.CompareTag("Tank"))
            {
                return true;
            }
            else
                return false;
        }
        return false;
    }

    float CalculatePathLength(Vector3 targetPosition)
    {
        NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
        //nav.CalculatePath(targetPosition, path);
        Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];

        allWayPoints[0] = transform.position;
        allWayPoints[allWayPoints.Length - 1] = targetPosition;

        for(int i = 0; i < path.corners.Length; i++)
        {
            allWayPoints[i + 1] = path.corners[i];
        }

        float pathLength = 0f;
        for(int i = 0; i < allWayPoints.Length - 1; i++)
        {
            pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
        }

        return pathLength;
    }
}
