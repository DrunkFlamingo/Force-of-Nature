using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private PlayerDataScrObj playerData;

    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private TrailRenderer trail;

    public Vector2 aim; //the stick input of the player
    private Vector2 dashDir;
    public float faceDir;

    private LayerMask ground;
    [SerializeField]public bool grounded;
    private float coyoteTimer;

    [SerializeField]private bool jumpBuffer;

    void Start()
    {
        faceDir = 1;
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        trail = GetComponent<TrailRenderer>();
        trail.emitting = false;
        ground = LayerMask.GetMask("Platform"); //here you put in any layers that the player can step on
        playerData.currentState = PlayerDataScrObj.playerState.IDLE;
        rb.gravityScale = playerData.baseGravity;
        coyoteTimer = 0f;
        playerData.dashCd = true;
        playerData.airDashed = false;
    }

    void FixedUpdate()
    {
        grounded = GroundCheck();

        if (playerData.playerStates[(int)playerData.currentState].canMove)
        {
                if (Mathf.Abs(aim.x) > playerData.deadzoneX)
                {
                    if (faceDir == Mathf.Sign(playerData.currAccel)){
                        playerData.currAccel += playerData.accel * faceDir;
                        if (Mathf.Abs(playerData.currAccel) > playerData.maxWalkSpeed)
                        {
                            playerData.currAccel = faceDir * playerData.maxWalkSpeed;
                        }
                    } else
                    {
                        playerData.currAccel += (playerData.accel + playerData.decel) * faceDir;
                    }
                   
                } else
                {
                    if (playerData.currAccel != 0f)
                    {
                        playerData.currAccel = (Mathf.Abs(playerData.currAccel) - playerData.decel) * faceDir;
                        if (Mathf.Sign(playerData.currAccel) != faceDir)
                        {
                            playerData.currAccel = 0f;
                    }
                }
            }
                rb.velocity = new Vector2(playerData.currAccel, rb.velocity.y);
            if (grounded && rb.velocity.x != 0)
            {
                playerData.currentState = PlayerDataScrObj.playerState.RUNNING;

            }
            else if (rb.velocity == Vector2.zero)
            {
                playerData.currentState = PlayerDataScrObj.playerState.IDLE;
            }
            CheckMovementBuffers();
        }

        if (!grounded)
        {
            coyoteTimer += Time.deltaTime;
        }
        else
        {
            coyoteTimer = 0;
        }

      

    }

    

    private void OnJump()
    {
        if (playerData.playerStates[(int)playerData.currentState].canJump)
        {
            if (grounded || coyoteTimer <= playerData.coyoteTime)
            {
                playerData.currentState = PlayerDataScrObj.playerState.JUMPING;
                rb.velocity = new Vector2(rb.velocity.x, playerData.jumpSpeed);
            }
            else
            {
                StartCoroutine("BufferJump");
            }
        }
    }

    private void CheckMovementBuffers()
    {
        if (jumpBuffer && grounded)
        {
            playerData.currentState = PlayerDataScrObj.playerState.JUMPING;
            rb.velocity = new Vector2(rb.velocity.x, playerData.jumpSpeed);
            jumpBuffer = false;
        }
    }

    private void OnStickInput(InputValue value)
    {
        aim = value.Get<Vector2>();
        if (Mathf.Abs(aim.x) >= playerData.deadzoneX)
        {
            faceDir = Mathf.Sign(aim.x);
        }
    }

    private void OnDash()
    {
        if (playerData.playerStates[(int)playerData.currentState].canDash && playerData.dashCd)
        {
            if (playerData.freeDirectionDash)
            {
                dashDir = aim;
                if (aim.y < 0.35f && aim.y > -0.35f)
                {
                    dashDir.y = 0f;
                    if (aim.x == 0f)
                    {
                        dashDir.x = faceDir;
                    }
                }
            }
            else
            {
                Vector2 eightDirAim = aim;
                if (Mathf.Abs(aim.x) >= playerData.deadzoneX)
                {
                    eightDirAim.x = Mathf.Sign(aim.x);
                }
                else
                {
                    eightDirAim.x = 0;
                }
                if (Mathf.Abs(aim.y) >= playerData.deadzoneY)
                {
                    eightDirAim.y = Mathf.Sign(aim.y);
                }
                else
                {
                    eightDirAim.y = 0;
                }
                if (eightDirAim == Vector2.zero)
                {
                    eightDirAim.x = faceDir;
                }
                dashDir = eightDirAim;
            }
            dashDir.Normalize();
            StartCoroutine("PlayerDash");
        }
    }

    private bool GroundCheck()
    {
        //boxcast directly downwards
        //if it hits the "ground" layermask (which contains anything the player can stand on), it counts as being grounded
        if (Physics2D.BoxCast(gameObject.transform.position, 0.95f * coll.bounds.size, 0f, Vector2.down, 0.2f, ground))
        {
            if (playerData.airDashed)
            {
                playerData.airDashed = false;
                playerData.dashCd = true;
            }
            playerData.sideAttackBoosted = false;
            playerData.upAttackBoosted = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator PlayerDash()
    {
        if (grounded == false)
        {
            playerData.airDashed = true;
        }
        else
        {
            StartCoroutine("DashCooldown");
        }
        trail.emitting = true;
        playerData.currentState = PlayerDataScrObj.playerState.DASHING;
        playerData.dashCd = false;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(dashDir.x * playerData.dashPower, dashDir.y * playerData.dashPower);

        yield return new WaitForSeconds(playerData.dashDuration);

        rb.velocity = Vector2.zero;
        rb.gravityScale = playerData.baseGravity;
        ResetState();
        trail.emitting = false;
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(playerData.dashDuration + playerData.dashCooldown);
        playerData.dashCd = true;
    }

    public void ResetState()
    {
        if (grounded)
        {
            playerData.currentState = PlayerDataScrObj.playerState.IDLE;

        }
        else
        {
            playerData.currentState = PlayerDataScrObj.playerState.JUMPING;
        }
    }
    IEnumerator BufferJump()
    {
        jumpBuffer = true;
        yield return new WaitForSeconds(playerData.jumpBufferTime);
        jumpBuffer = false;
    }
}
