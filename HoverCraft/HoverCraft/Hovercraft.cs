using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class Hovercraft : PartModule
{
    [KSPField]
    public float hoverForce = 0F;
    public AnimationState[] deployStates;
    [KSPField(isPersistant = false)]
    public bool hasDeployAnimation = true;
    [KSPField(isPersistant = false)]
    public string deployAnimName = "move";
    [KSPField]
    public float rearoffset = 0.0F;
    [KSPField]
    public float fuelefficiency = 0.002F;
    [KSPField]
    public float liftCap = 20f;
    public float upwardSpeedrl = 0f;
    public float upwardSpeedl = 0f;
    public float upwardSpeedrr = 0f;
    public float upwardSpeedr = 0f;
    float rightrcrelative = 1;
    float leftrcrelative = 1;
    float rearrightrcrelative = 1;
    float rearleftrcrelative = 1;
    Vector3 rightrcrelativePosition = Vector3.forward;
    Vector3 leftrcrelativePosition = Vector3.forward;
    Vector3 rearrightrcrelativePosition = Vector3.forward;
    Vector3 rearleftrcrelativePosition = Vector3.forward;
    Transform CoMTransform = new GameObject().transform;

    //[KSPField(guiName = "hoverdamp", guiActive = true)]
    //private string damp = "None";

    [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Hover Height", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 5f, stepIncrement = 0.1f)]
    public float hoverHeight = 2.5f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Lift", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 20f, stepIncrement = 0.5f)]
    public float maxlift = 0f;

    [KSPField(guiActive = true, guiActiveEditor = true, guiName = "DAMP", isPersistant = true), UI_FloatRange(minValue = 0f, maxValue = 2f, stepIncrement = 0.1f)]
    public float hoverDamp = 1f;

    protected Transform rightrcTransform = null;

    protected Transform leftrcTransform = null;

    protected Transform rearrightrcTransform = null;

    protected Transform rearleftrcTransform = null;

    List<KSPParticleEmitter> HoverEmitters;

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
            if (base.part.FindModelTransform("rightrc") != null)
            {
                this.rightrcTransform = base.part.FindModelTransform("rightrc");
            }
            if (base.part.FindModelTransform("leftrc") != null)
            {
                this.leftrcTransform = base.part.FindModelTransform("leftrc");
            }
            if (base.part.FindModelTransform("rearrightrc") != null)
            {
                this.rearrightrcTransform = base.part.FindModelTransform("rearrightrc");
            }
            if (base.part.FindModelTransform("rearleftrc") != null)
            {
                this.rearleftrcTransform = base.part.FindModelTransform("rearleftrc");
            }

            if (deployAnimName != "")
            {
                deployStates = SetUpAnimation(deployAnimName, this.part);
            }
            if (hasDeployAnimation)
            {
                deployStates = SetUpAnimation(deployAnimName, this.part);
                foreach (AnimationState anim in deployStates)
                {
                    anim.enabled = false;
                }
            }
            foreach (var emitter in part.FindModelComponents<KSPParticleEmitter>())
            {
                emitter.emit = false;
                EffectBehaviour.RemoveParticleEmitter(emitter);
            }
            HoverEmitters = new List<KSPParticleEmitter>();
            foreach (Transform htf in part.FindModelTransforms("HoverEmi"))
            {
                KSPParticleEmitter kpe = htf.GetComponent<KSPParticleEmitter>();
                HoverEmitters.Add(kpe);
                kpe.emit = false;
            }

        }

        //CoMTransform.position = vessel.CoM;
        //CoMTransform.rotation = vessel.transform.rotation;
        //rightrcrelativePosition = CoMTransform.InverseTransformPoint(rightrcTransform.position);
        //leftrcrelativePosition = CoMTransform.InverseTransformPoint(leftrcTransform.position);
        //rearrightrcrelativePosition = CoMTransform.InverseTransformPoint(rearrightrcTransform.position);
        //rearleftrcrelativePosition = CoMTransform.InverseTransformPoint(rearleftrcTransform.position);
        //rightrcrelative = 1 / System.Math.Abs(rightrcrelativePosition.y);
        //leftrcrelative = 1 / System.Math.Abs(leftrcrelativePosition.y);
        //rearrightrcrelative = 1 / System.Math.Abs(rearrightrcrelativePosition.y);
        //rearleftrcrelative = 1 / System.Math.Abs(rearleftrcrelativePosition.y);

    }


    public override void OnFixedUpdate()
    {
        if (!HighLogic.LoadedSceneIsFlight) return;
        Vector3 gee = FlightGlobals.getGeeForceAtPosition(transform.position);
        hoverForce = vessel.GetTotalMass() * gee.magnitude;
            if (base.part.FindModelTransform("rightrc") != null&&!rightrcTransform)
            {
                this.rightrcTransform = base.part.FindModelTransform("rightrc");
            }
            if (base.part.FindModelTransform("leftrc") != null&&!leftrcTransform)
            {
                this.leftrcTransform = base.part.FindModelTransform("leftrc");
            }
            if (base.part.FindModelTransform("rearrightrc") != null&&!rearrightrcTransform)
            {
                this.rearrightrcTransform = base.part.FindModelTransform("rearrightrc");
            }
            if (base.part.FindModelTransform("rearleftrc") != null&&!rearleftrcTransform)
            {
                this.rearleftrcTransform = base.part.FindModelTransform("rearleftrc");
            }

            if (hoverHeight != 0 && maxlift != 0)
            {
                if (hasDeployAnimation)
                {
                    foreach (AnimationState anim in deployStates)
                    {
                        //animation clamping
                        if (anim.normalizedTime > 1)
                        {
                            anim.speed = 0;
                            anim.normalizedTime = 1;
                        }
                        if (anim.normalizedTime < 0)
                        {
                            anim.speed = 0;
                            anim.normalizedTime = 0;
                        }

                        //deploying
                        if (hoverHeight != 0 && maxlift != 0)
                        {
                            anim.enabled = true;
                            if (anim.normalizedTime < 1 && anim.speed < 1)
                            {
                                anim.speed = 1;
                            }
                            if (anim.normalizedTime == 1)
                            {
                                anim.enabled = false;
                            }
                        }
                    }

                }
                upwardSpeedr = DoHoverForce(rightrcTransform, hoverHeight, rearoffset, hoverDamp, hoverForce, rightrcrelative, maxlift, upwardSpeedr, this.part);
                upwardSpeedl = DoHoverForce(leftrcTransform, hoverHeight, rearoffset, hoverDamp, hoverForce, leftrcrelative, maxlift, upwardSpeedl, this.part);
                upwardSpeedrr = DoHoverForce(rearrightrcTransform, hoverHeight, rearoffset, hoverDamp, hoverForce, rearrightrcrelative, maxlift, upwardSpeedrr, this.part);
                upwardSpeedrl = DoHoverForce(rearleftrcTransform, hoverHeight, rearoffset, hoverDamp, hoverForce, rearleftrcrelative, maxlift, upwardSpeedrl, this.part);
            }
            else
            {
                upwardSpeedr = 0;
                upwardSpeedl = 0;
                upwardSpeedrr = 0;
                upwardSpeedrl = 0;
            }

            foreach (var pEmitter in HoverEmitters)
            {
                pEmitter.maxEmission = (int)(10f*(upwardSpeedr + upwardSpeedl + upwardSpeedrr + upwardSpeedrl));
                pEmitter.minEmission = (int)(10f * (upwardSpeedr + upwardSpeedl + upwardSpeedrr + upwardSpeedrl));
                pEmitter.Emit();
                //if (!pEmitter.useWorldSpace)
                //{
                //    if (pEmitter.maxEnergy < 0.5f)
                //    {
                //        float twoFrameTime = Mathf.Clamp(Time.deltaTime * 2f, 0.02f, 0.499f);
                //        pEmitter.maxEnergy = twoFrameTime;
                //        pEmitter.minEnergy = twoFrameTime / 3f;
                //    }
                //    pEmitter.Emit();
                //}
            }

            if (hoverHeight == 0 || maxlift == 0)
            {
                if (hasDeployAnimation)
                {
                    foreach (AnimationState anim in deployStates)
                    {
                        //animation clamping
                        if (anim.normalizedTime > 1)
                        {
                            anim.speed = 0;
                            anim.normalizedTime = 1;
                        }
                        if (anim.normalizedTime < 0)
                        {
                            anim.speed = 0;
                            anim.normalizedTime = 0;
                        }
                        anim.enabled = true;
                        if (anim.normalizedTime > 0 && anim.speed > -1)
                        {
                            anim.speed = -1;
                        }
                        if (anim.normalizedTime == 0)
                        {
                            anim.enabled = false;
                        }
                    }
                }
            }
    }

    static float DoHoverForce(Transform rcTransform, float hoverHeight, float offset, float hoverDamp, float hoverForce, float rcrelative, float maxlift, float upwardSpeed, Part part)
    {
        RaycastHit hit;
        Ray rcray = new Ray(rcTransform.position, rcTransform.forward);
        float height = 0.0f;
        bool Ray = Physics.Raycast(rcray, out hit, hoverHeight, 557057);
        if (FlightGlobals.ActiveVessel.mainBody.ocean) //if mainbody has ocean we land on water before the seabed 
        {
            height = FlightGlobals.getAltitudeAtPos(rcTransform.position);
        }
        if (Ray || height < hoverHeight)
        {
            float hoverError = 0.0f;

            if (Ray) hoverError = hoverHeight - hit.distance;

            if (!Ray || height < hit.distance) hoverError = hoverHeight - height;

            hoverError += offset;
            upwardSpeed -= hoverError;

            float lift = (hoverError - upwardSpeed * hoverDamp / Time.fixedDeltaTime) * hoverForce * rcrelative;

            if (lift > maxlift * hoverForce) lift = maxlift * hoverForce;

            upwardSpeed = hoverError;

            if (hoverError >= 0.0f) part.AddForceAtPosition(-lift * rcTransform.forward, rcTransform.position);
            //Debug.Log(hoverError);
            //Debug.Log(lift);
        }
        else
        {
            upwardSpeed = 0f;
        }
        return upwardSpeed;
    }

    public static AnimationState[] SetUpAnimation(string animationName, Part part)  //Thanks Majiir!(From BDA)
    {
        var states = new List<AnimationState>();
        foreach (var animation in part.FindModelAnimators(animationName))
        {
            var animationState = animation[animationName];
            animationState.speed = 0;
            animationState.enabled = true;
            animationState.wrapMode = WrapMode.ClampForever;
            animation.Blend(animationName);
            states.Add(animationState);
        }
        return states.ToArray();
    }

}
 
