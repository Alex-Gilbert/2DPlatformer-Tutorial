using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    //Now we get two Jump Heights
    //minJumpHeight being the height you'll reach just by tapping the jump button
    //maxJumpHeight being the height you'll reach by holding onto the jump button for the full jump
    public float minJumpHeight = 2;
	public float maxJumpHeight = 5;

	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 6;

	float gravity;
	float jumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

    //We have a boolean that just checks to see if we are jumping
    //jumpStartHeight is our y position when we start a jump
    //jumpHeight is how high off the ground we currently should be 
    bool isJumping = false;
    float jumpStartHeight = 0f;
    float jumpHeight = 0f;

	void Start() 
    {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		print ("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);
	}

	void Update() {

		if (controller.collisions.above || controller.collisions.below) 
        {
			velocity.y = 0;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below && !isJumping) 
        {
            //Starting our Jump Coroutine
            StopCoroutine("JumpLogic");
            StartCoroutine("JumpLogic");
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

        if (isJumping)
        {
            //When we are jumping we don't want our y value to be affected by gravity instead we should move to our current jumpHeight
            velocity.y = ((jumpStartHeight + jumpHeight) - transform.position.y);

            //our coroutine takes care of all the time.deltaTime stuff, so we don't need to multiply our y velocity by it again
            //We only need to multiply our x velocity by time.deltaTime so we need to create a temp velocity to take care of that
            Vector3 velocityToMove = new Vector3(velocity.x * Time.deltaTime, velocity.y, 0);

            controller.Move(velocityToMove);
        }
        else
        {
            //The same as before
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        
	}

    private IEnumerator JumpLogic()
    {
        //This value is just for testing purposes
        float timeTaken = 0;

        //we store our y position when the jump starts
        jumpStartHeight = transform.position.y;

        //set isJumping to True
        isJumping = true;

        //This is used for our SmoothDamp. The initial velocity of our jump should be what we calculated it should be earlier
        float jumpHeightV = jumpVelocity;

        //This is the while loop that takes care of our jump 
        //It stops when our jump stops and that occurs when the player lets go of the jump button or the jumpHeight exceeds maxJumpHeight or we hit our head
        while ((jumpHeight < minJumpHeight || Input.GetKey(KeyCode.Space)) && jumpHeight <= maxJumpHeight && !controller.collisions.above)
        {
            //This Smooth damps our jump height to maxJumpHeight + .15f (This allows the jumpHeight to exceed our maxJumpHeight which is needed to escape the while loop)
            //I'm not sure why, but when timeToJumpApex was used the jump lasted much longer than desired. I am using a value slightly less than helf of timeToJumpApex. Still not sure why this was needed.
            jumpHeight = Mathf.SmoothDamp(jumpHeight, maxJumpHeight + .15f, ref jumpHeightV, timeToJumpApex * .45f);
            timeTaken += Time.deltaTime;
            yield return null;
        }

        //End The jump
        print("Jump Height: " + jumpHeight + " Time Taken: " + timeTaken);

        jumpHeight = 0;
        isJumping = false;
        yield return null;
    }
}
