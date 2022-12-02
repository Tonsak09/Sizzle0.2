using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Chargeable
{
    [Header("Slime")]
    [Header("Jumping")]
    [SerializeField] Vector3 jumpStartoffset;
    [SerializeField] float minChargeToJump;
    [SerializeField] float jumpForce;
    [SerializeField] float timeToJumpIdle;
    [SerializeField] float timeToJumpRandOffset;

    [Header("Popping")]
    [SerializeField] float shrinkTime;
    [SerializeField] float shrinkPercentFinal;
    [SerializeField] AnimationCurve shrinkCurve;
    [SerializeField] GameObject explodeFX;


    [Header("Animation")]
    [Header("Eyes")]
    [SerializeField] Renderer[] eyes;
    [SerializeField] Texture[] openEyes;
    [SerializeField] Texture closed;
    [SerializeField] float timeToSwap;
    [SerializeField] float timeToSwapRandOffset;
    [Header("Squish n' Jump!")]
    [SerializeField] Transform body;
    [SerializeField] float squishTime;
    /*[SerializeField] float targetYPercent;
    [SerializeField] AnimationCurve ySquishCurve;
    [SerializeField] float targetXZPercent;
    [SerializeField] AnimationCurve xzSquishCurve;*/
    [SerializeField] float timeTillJump;


    private ChargeObj co;
    private Rigidbody rb;
    private Vector3 dir;
    private float idleJumpTimer;
    private bool grounded;

    // The scale this slime starts as
    private Vector3 holdScale;
    private Vector3 bodyHoldScale;
    private Vector3 scaleFinal { get { return holdScale * shrinkPercentFinal; } }

    private Coroutine PopCo;
    private Coroutine SquishCo;

    private void Start()
    {
        co = this.GetComponent<ChargeObj>();
        rb = this.GetComponent<Rigidbody>();

        holdScale = this.transform.localScale;
        bodyHoldScale = body.transform.localScale;
        idleJumpTimer = timeToJumpIdle + Random.Range(-timeToJumpRandOffset, timeToJumpRandOffset);

        StartCoroutine(EyeCo());
    }

    public override void Desperse()
    {
        if(grounded)
        {
            idleJumpTimer -= Time.deltaTime;
            if(idleJumpTimer <= 0)
            {
                TryJump((Vector3.up) * jumpForce);

                idleJumpTimer = timeToJumpIdle + Random.Range(-timeToJumpRandOffset, timeToJumpRandOffset);
            }
        }
        else
        {
            idleJumpTimer = timeToJumpIdle + Random.Range(-timeToJumpRandOffset, timeToJumpRandOffset);
        }

        // Will not continue if cannot desperse 
        base.Desperse();

        if (currentCharge <= 0)
        {
            // No longer charges other chargeables 
            co.active = false;

            return;
        }
        else
        {
            // Is able to charge other objects 
            co.active = true;
        }

        if (grounded && currentCharge >= minChargeToJump)
        {
            // Add force oppoiste of direction that charge comes from 
            TryJump((dir + jumpStartoffset) * jumpForce);
        }
    }

    public void AddCharge(float chargeAmount, Vector3 dir)
    {
        base.AddCharge(chargeAmount);
        this.dir = dir;
    }

    private void TryJump(Vector3 vectorJump)
    {
        /*if(SquishCo == null)
        {
            SquishCo = StartCoroutine(ShrinkAndJump(vectorJump));
        }*/
        rb.AddForce(vectorJump, ForceMode.Impulse);
        grounded = false;
    }

    public void Pop()
    {
        if (PopCo == null)
        {
            PopCo = StartCoroutine(ShrinkCo());
        }
    }

    private IEnumerator ShrinkAndJump(Vector3 vectorJump)
    {
        bool hasJumped = false;
        float time = 0;
        while(time <= squishTime)
        {
            float lerp = time / squishTime;
            // Scales to "squish" 
            /*body.localScale = new Vector3
                (
                    Mathf.Lerp(bodyHoldScale.x, targetXZPercent * bodyHoldScale.x, xzSquishCurve.Evaluate(lerp)),
                    Mathf.Lerp(bodyHoldScale.y, targetYPercent * bodyHoldScale.y, ySquishCurve.Evaluate(lerp)),
                    Mathf.Lerp(bodyHoldScale.z, targetXZPercent * bodyHoldScale.z, xzSquishCurve.Evaluate(lerp))
                );*/

            // Jump only if not done yet 
            if (!hasJumped)
            {
                if(time >= timeTillJump)
                {
                    hasJumped = true;

                    rb.AddForce(vectorJump, ForceMode.Impulse);
                    grounded = false;
                }
            }
            time += Time.deltaTime;
            yield return null;
        }

        // Cleanup
        StopCoroutine(SquishCo);
        SquishCo = null;
    }

    private IEnumerator ShrinkCo()
    {
        float timer = 0;
        while (timer <= shrinkTime)
        {
            this.transform.localScale = Vector3.Lerp(holdScale, scaleFinal, shrinkCurve.Evaluate(timer / shrinkTime));

            timer += Time.deltaTime;
            yield return null;
        }

        // Explode with smaller slimes 
        Instantiate(explodeFX, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        StopCoroutine(PopCo);
    }

    private IEnumerator EyeCo()
    {
        float timer = timeToSwap + Random.Range(-timeToSwapRandOffset, timeToSwapRandOffset);
        while (true)
        {
            timer -= Time.deltaTime;

            if(timer <= 0)
            {
                int rand = Random.Range(0, 3);
                switch(rand)
                {
                    case 0: // Eye 1
                        eyes[0].material.SetTexture("_BaseMap", openEyes[0]);
                        eyes[1].material.SetTexture("_BaseMap", openEyes[0]);
                        break;
                    case 1: // Eye 2
                        eyes[0].material.SetTexture("_BaseMap", openEyes[1]);
                        eyes[1].material.SetTexture("_BaseMap", openEyes[1]);
                        break;
                    case 2: // Blink Animation 
                        break;
                }

                timer = timeToSwap + Random.Range(-timeToSwapRandOffset, timeToSwapRandOffset);
            }

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        grounded = false;
    }
}
