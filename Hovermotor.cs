using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class Hovermotor : PartModule
{
    [KSPField]
    public float liftratio = 1F;
    [KSPField]
    public float hoverForce = 0F;
    [KSPField]
    public float yaws = 0F;
    [KSPField]
    public float upwardSpeed = 0f;
    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Hover Height", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.1f)]
    public float hoverHeight = 0f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Lift", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 10f, stepIncrement = 0.1f)]
    public float maxlift = 0f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "DAMP", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.1f)]
    public float hoverDamp = 1f;

    public float fuelefficiency = 0.001F;
    public Vector3 parameter = Vector3.zero;
    public Vector3 parametertemp = Vector3.zero;
    public bool modified = false;

    float hoverError = 0;
    private float rotation = 0F;

    protected Transform RCTransform = null;

    protected Transform rotor = null;
    [KSPField(guiName = "hoverdis", guiActive = true)]
    private string dis = "None";




    protected Transform rotorTransform = null;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        {
            if (state != StartState.Editor && state != StartState.None)
            {
                this.enabled = true;
                this.part.force_activate();
            }
            else
            {
                this.enabled = false;
            }

            if (base.part.FindModelTransform("rotor") != null)
            {
                this.rotor = base.part.FindModelTransform("rotor");
            }

            if (base.part.FindModelTransform("RC") != null)
            {
                this.RCTransform = base.part.FindModelTransform("RC");
            }
        }

    }


    public override void OnFixedUpdate()
    {

        parameter = new Vector3(maxlift, hoverHeight, hoverDamp);
        if (parameter != parametertemp)
        {
            modified = true;
        }
        foreach (Part p in this.vessel.Parts)
        {
            foreach (PartModule m in p.Modules)
            {
                Hovermotor motor = null;
                if (m.moduleName == "Hovermotor")
                {
                    motor = (Hovermotor)m;
                    if (modified == true && motor.GetInstanceID() != GetInstanceID())
                    {
                        if (motor.modified == true)
                        {
                            hoverDamp = motor.hoverDamp;
                            maxlift = motor.maxlift;
                            hoverHeight = motor.hoverHeight;
                            parameter = motor.parameter;
                            parametertemp = motor.parameter;

                        }
                        else
                        {
                            motor.hoverDamp = hoverDamp;
                            motor.maxlift = maxlift;
                            motor.hoverHeight = hoverHeight;
                            motor.parameter = parameter;
                            motor.parametertemp = parameter;
                        }
                    }
                }
            }
        }
        modified = false;
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        float pitch = vessel.ctrlState.pitch;
        float roll = vessel.ctrlState.roll;
        float yaw = vessel.ctrlState.yaw;
        float y = vessel.ctrlState.Y;
        float x = vessel.ctrlState.X;
        float throttle = vessel.ctrlState.mainThrottle;

        Vector3 srfVelocity = vessel.GetSrfVelocity();
        float VerticalV;
        VerticalV = (float)vessel.verticalSpeed;
        Vector3 Airspeed = RCTransform.InverseTransformDirection(srfVelocity);
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(RCTransform.position);
        hoverForce = vessel.GetTotalMass() * gee.magnitude;
        if (FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.pqsAltitude < 2400)
        {

            //Vector3 yawsForce = vessel.GetTotalMass() * Airspeed.x * yaws * -part.transform.right;
            //part.rigidbody.AddForceAtPosition(yawsForce, part.transform.position);

            RaycastHit hit;
            Ray rcray = new Ray(RCTransform.position, -RCTransform.up);
            //LayerMask pRayMask = 33792;
            if (Physics.Raycast(rcray, out hit))
            {
                hoverError = hoverHeight - hit.distance;
                if (FlightGlobals.ActiveVessel.mainBody.ocean) 
                {
                    float height = FlightGlobals.getAltitudeAtPos(RCTransform.position);
                    if (hit.distance > height)
                    {
                        hoverError = hoverHeight - height;
                    }
                }
                upwardSpeed -= hoverError;
                float lift = (hoverError - upwardSpeed * hoverDamp) * hoverForce *  maxlift;
                float consumption = Mathf.Abs(lift) * fuelefficiency + 0.01f;
                upwardSpeed = hoverError;
                dis = hit.distance.ToString("R");
                if (hoverError >= 0)
                {
                float resourceDrawn = this.part.RequestResource("ElectricCharge", consumption);
                liftratio = resourceDrawn / consumption;
                    part.AddForceAtPosition(liftratio * lift * RCTransform.up, RCTransform.position);
                }
                rotor.transform.localEulerAngles = new Vector3(
                0, rotation, 0);
                rotation += 6 * lift * 120 * TimeWarp.deltaTime;
                while (rotation > 360) rotation -= 360;
                while (rotation < 0) rotation += 360;
                parametertemp = parameter;

            }

         }
    }
}

