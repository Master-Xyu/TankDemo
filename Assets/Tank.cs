using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public enum CtrlType
    {
        none,
        player,
        computer
    }
    public Transform turret;
    private float turretRotSpeed = 0.8f;
    private float turretRotTarget = 0;

    public Transform gun;
    public Transform wheels;
    public Transform tracks;
    private float maxRoll = 10f;
    private float minRoll = -10f;
    private float turretRollTarget = 0;

    public List<AxleInfo> axleInfos;
    private float motor = 0;
    public float maxMotorTorque;

    private float brakeTorque = 0;
    public float maxBrakeTorque;

    private float steering = 0;
    public float maxSteeringAngle = 5f;

    public AudioSource motorAudioSource;
    public AudioClip motorClip;

    public GameObject bullet;
    public float lastShootTime = 0;
    private float shootInterval = 0.5f;

    public CtrlType ctrlType = CtrlType.player;

    public float maxHp = 100;
    public float hp = 100;

    public GameObject destroyFlameEffect;
    public GameObject destroySmokeEffect;
    // Start is called before the first frame update
    void Start()
    {
        turret = transform.Find("turret");
        gun = turret.Find("cannon");
        wheels = transform.Find("wheels");
        tracks = transform.Find("tracks");
        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCtrl();
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            axleInfo.leftWheel.brakeTorque = brakeTorque;
            axleInfo.rightWheel.brakeTorque = brakeTorque;

            if(axleInfos[1] != null && axleInfo == axleInfos[1])
            {
                WheelsRotation(axleInfos[1].leftWheel);
                TrackMove();
            }
        }

        TurretRotation();
        TurretRoll();
        MotorSound();
    }

    public void TurretRotation()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;

        float angle = turret.eulerAngles.y + 180 - turretRotTarget;
        if (angle < 0)
            angle += 360;

        angle %= 360;
        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    public void TurretRoll()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;

        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;

        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;

        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;

        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }

    public void PlayerCtrl()
    {
        if(ctrlType != CtrlType.player)
        {
            return;
        }
        maxMotorTorque = 300f;
        maxSteeringAngle = 25f;
        motor = -maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        turretRollTarget = Camera.main.transform.eulerAngles.x;
        turretRotTarget = Camera.main.transform.eulerAngles.y;

        brakeTorque = 0;
        maxBrakeTorque = 400f;

        foreach(AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            continue;
        }

        if (Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        foreach(Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }

    public void TrackMove()
    {
        if (tracks == null)
            return;

        float offset = 0;
        if (wheels.GetChild(0) != null)
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;
        foreach(Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                continue;
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(offset, 0);
        }
    }

    void MotorSound()
    {
        if ((motor>1 || motor < -1) && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorClip;
            motorAudioSource.Play();
        }
        else if(motor < 1 && motor > -1)
        {
            motorAudioSource.Pause();
        }
    }

    public void Shoot()
    {
        if (Time.time - lastShootTime < shootInterval)
            return;

        if (bullet == null)
            return;
        Vector3 pos = gun.position + gun.forward * 5;
        Instantiate(bullet, pos, gun.rotation);
        lastShootTime = Time.time;
    }
}
