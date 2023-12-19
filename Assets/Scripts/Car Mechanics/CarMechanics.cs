using System.Collections;
using UnityEngine;

public enum GearState
{
    Idle,
    Moving,
    ValidatingChange,
    Changing
};

[System.Serializable]
public class WheelColliders
{
    public WheelCollider frontRightWheel;
    public WheelCollider frontLeftWheel;
    public WheelCollider rearRightWheel;
    public WheelCollider rearLeftWheel;
}

[System.Serializable]
public class WheelMeshes
{
    public GameObject frontRightWheel;
    public GameObject frontLeftWheel;
    public GameObject rearRightWheel;
    public GameObject rearLeftWheel;
}

[System.Serializable]
public class WheelParticleEffects
{
    public ParticleSystem frontRightWheel;
    public ParticleSystem frontLeftWheel;
    public ParticleSystem rearRightWheel;
    public ParticleSystem rearLeftWheel;
}

public class CarMechanics: MonoBehaviour
{
    [Header("Car Components")]
    private Rigidbody playerRB;

    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public WheelParticleEffects wheelParticles;

    [Header("Car Settings")]
    private float brakeInput;
    private float clampedSpeed;
    private float Torque;
    private float clutch;
    private float wheelSpeed;
    private GearState gearState;
    private bool handbrake = false;

    public float accellInput;
    public GameObject smokeParticle;
    public float enginePower;
    public float brakePower;
    public float driftAngle;
    public float speed;
    public float maxSpeed;
    public float maxSteering = 70f;
    public int isEngineOn; 
    public float RPM;
    public float neutralRPM;
    public float maxRPM;
    public int currentGear;
    public float diffRatio;
    public AnimationCurve powerToRPMCurve;
    public float shiftUpRPM;
    public float shiftDownRPM;
    public float[] gearRatios;
    public float gearChangeSpeed = 0.5f;
    public Transform centerOfMass;

    private void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();

        if (centerOfMass)
            playerRB.centerOfMass = centerOfMass.localPosition;

        InitiateParticles();
    }

    private void Update()
    {
        speed = colliders.rearRightWheel.rpm * colliders.rearRightWheel.radius * 2f * Mathf.PI / 10f;
        clampedSpeed = Mathf.Lerp(clampedSpeed, speed, Time.deltaTime);
        ApplyBrake();
        ApplyAccell();
        UpdateWheelPositions();
        CheckParticleSpawn();
    }

    public void SetInput(float accellInput, float steeringInput, float clutchInput, float brakeInput)
    {
        this.accellInput = accellInput;

        if (Mathf.Abs(this.accellInput) > 0 && isEngineOn == 0)
        {
            StartCoroutine(GetComponent<CarAudio>().StartEngine());
            gearState = GearState.Moving;
        }

        ApplySteering(steeringInput);
        driftAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);

        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Idle)
            {
                clutch = 0;
                if (Mathf.Abs(this.accellInput) > 0) gearState = GearState.Moving;
            }
            else
            {
                clutch = Mathf.Abs(1 - clutchInput);
            }
        }
        else
        {
            clutch = 0;
        }

        if (movingDirection < -0.5f && this.accellInput > 0)
        {
            this.brakeInput = Mathf.Abs(this.accellInput);
        }
        else if (movingDirection > 0.5f && this.accellInput < 0)
        {
            this.brakeInput = Mathf.Abs(this.accellInput);
        }
        else
        {
            this.brakeInput = 0;
        }

        handbrake = (brakeInput > 0.5);
    }

    private void ApplyBrake()
    {
        colliders.frontRightWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.frontLeftWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.rearRightWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        colliders.rearLeftWheel.brakeTorque = brakeInput * brakePower * 0.3f;

        if (handbrake)
        {
            clutch = 0;
            colliders.rearRightWheel.brakeTorque = brakePower * 1000f;
            colliders.rearLeftWheel.brakeTorque = brakePower * 1000f;
        }
    }

    private void ApplyAccell()
    {
        Torque = CalculateEngineTorque();
        colliders.rearLeftWheel.motorTorque = Torque * accellInput;
        colliders.rearRightWheel.motorTorque = Torque * accellInput;
    }

    private float CalculateEngineTorque()
    {
        float torque = 0;

        if (RPM < neutralRPM + 200 && accellInput == 0 && currentGear == 0)
        {
            gearState = GearState.Idle;
        }

        if (gearState == GearState.Moving && clutch > 0)
        {
            if (RPM > shiftUpRPM)
            {
                StartCoroutine(ChangeGear(1));
            }
            else if (RPM < shiftDownRPM)
            {
                StartCoroutine(ChangeGear(-1));
            }
        }

        if (isEngineOn > 0)
        {
            if (clutch < 0.1f)
            {
                RPM = Mathf.Lerp(RPM, Mathf.Max(neutralRPM, maxRPM * accellInput) + Random.Range(-40, 40), Time.deltaTime);
            }
            else
            {
                wheelSpeed = Mathf.Abs((colliders.rearRightWheel.rpm + colliders.rearLeftWheel.rpm) / 2f) * gearRatios[currentGear] * diffRatio;
                RPM = Mathf.Lerp(RPM, Mathf.Max(neutralRPM - 100, wheelSpeed), Time.deltaTime * 3f);
                torque = (powerToRPMCurve.Evaluate(RPM / maxRPM) * enginePower / RPM) * gearRatios[currentGear] * diffRatio * 5252f * clutch;
            }
        }

        return torque;
    }

    public void ApplySteering(float steeringAngle)
    {
        if (driftAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        }
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteering, maxSteering);
        colliders.frontLeftWheel.steerAngle = steeringAngle;
        colliders.frontRightWheel.steerAngle = steeringAngle;
    }

    private void UpdateWheelPositions()
    {
        SetWheelPos(colliders.frontRightWheel, wheelMeshes.frontRightWheel);
        SetWheelPos(colliders.frontLeftWheel, wheelMeshes.frontLeftWheel);
        SetWheelPos(colliders.rearRightWheel, wheelMeshes.rearRightWheel);
        SetWheelPos(colliders.rearLeftWheel, wheelMeshes.rearLeftWheel);
    }

    void SetWheelPos(WheelCollider wheelColl, GameObject Mesh)
    {
        Quaternion quaternion;
        Vector3 position;
        wheelColl.GetWorldPose(out position, out quaternion);
        Mesh.transform.rotation = quaternion;
        Mesh.transform.position = position;
    }

    private void CheckParticleSpawn()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.frontRightWheel.GetGroundHit(out wheelHits[0]);
        colliders.frontLeftWheel.GetGroundHit(out wheelHits[1]);

        colliders.rearRightWheel.GetGroundHit(out wheelHits[2]);
        colliders.rearLeftWheel.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.2f;

        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))        
            wheelParticles.frontRightWheel.Play();      
        else   
            
            wheelParticles.frontRightWheel.Stop();        
        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))       
            wheelParticles.frontLeftWheel.Play();       
        else   
            
            wheelParticles.frontLeftWheel.Stop();        
        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))        
            wheelParticles.rearRightWheel.Play();        
        else        
            wheelParticles.rearRightWheel.Stop();       
        
        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))        
            wheelParticles.rearLeftWheel.Play();        
        else        
            wheelParticles.rearLeftWheel.Stop();
        
    }

    private void InitiateParticles()
    {
        if (smokeParticle)
        {
            InstantiateWheelParticles(colliders.frontRightWheel, ref wheelParticles.frontRightWheel);
            InstantiateWheelParticles(colliders.frontLeftWheel, ref wheelParticles.frontLeftWheel);
            InstantiateWheelParticles(colliders.rearRightWheel, ref wheelParticles.rearRightWheel);
            InstantiateWheelParticles(colliders.rearLeftWheel, ref wheelParticles.rearLeftWheel);
        }
    }

    private void InstantiateWheelParticles(WheelCollider wheelCollider, ref ParticleSystem wheelParticle)
    {
        wheelParticle = Instantiate(smokeParticle, wheelCollider.transform.position - Vector3.up * wheelCollider.radius, Quaternion.identity, wheelCollider.transform)
            .GetComponent<ParticleSystem>();
    }

    IEnumerator ChangeGear(int gearAmount)
    {
        gearState = GearState.ValidatingChange;
        if (currentGear + gearAmount >= 0)
        {
            if (gearAmount > 0)
            {
                yield return new WaitForSeconds(0.7f);
                if (RPM < shiftUpRPM || currentGear >= gearRatios.Length - 1)
                {
                    gearState = GearState.Moving;
                    yield break;
                }
            }
            if (gearAmount < 0)
            {
                yield return new WaitForSeconds(0.1f);
                if (RPM > shiftDownRPM || currentGear <= 0)
                {
                    gearState = GearState.Moving;
                    yield break;
                }
            }
            gearState = GearState.Changing;
            yield return new WaitForSeconds(gearChangeSpeed);
            currentGear += gearAmount;
        }
        if (gearState != GearState.Idle)
            gearState = GearState.Moving;
    }

    public float GetSpeedDifference()
    {
        var accell = Mathf.Clamp(Mathf.Abs(accellInput), 0.5f, 1f);
        return RPM * accell / maxRPM;
    }
}