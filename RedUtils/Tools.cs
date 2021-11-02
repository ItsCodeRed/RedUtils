using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using RedUtils.Math;
using RLBotDotNet;

/* 
 * This file extends the RUBot class with some extra tools that make bot creation easier.
 * You probably won't want to edit to much in here, except the ShotCheck functions.
 * You are encouraged to make your own, although feel free to continue using the default one if you like.
 */

namespace RedUtils
{
	public abstract partial class RUBot : Bot
	{
		/// <summary>Encapsulates a function that finds the best shot for any ball slice, and target.</summary>
		public delegate Shot ShotCheck(BallSlice slice, Target target);

		/// <summary>Draws 2D text onto the screen</summary>
		/// <param name="text">The text you want printed</param>
		/// <param name="color">The color of the text</param>
		/// <param name="upperLeftPoint">The upper left corner point of the text. (0, 0) is the top left of the screen</param>
		/// <param name="scaleX">The horizontal scale of the text</param>
		/// <param name="scaleY">The vertical scale of the text</param>
		public void DrawText2D(string text, Color color, Vec3 upperLeftPoint, int scaleX, int scaleY)
		{
			Vector2 drawPoint = new Vector2(upperLeftPoint.x, upperLeftPoint.y);
			Renderer.DrawString2D(text, color, drawPoint, scaleX, scaleY);
		}

		/// <summary>Draws 3D text in the world. it always faces you</summary>
		/// <param name="text">The text you want printed</param>
		/// <param name="color">The color of the text</param>
		/// <param name="upperLeftPoint">The upper left corner point of the text in world space</param>
		/// <param name="scaleX">The horizontal scale of the text</param>
		/// <param name="scaleY">The vertical scale of the text</param>
		public void DrawText3D(string text, Color color, Vec3 upperLeftPoint, int scaleX, int scaleY)
		{
			Vector3 drawPoint = new Vector3(upperLeftPoint.x, upperLeftPoint.y, upperLeftPoint.z);
			Renderer.DrawString3D(text, color, drawPoint, scaleX, scaleY);
		}

		/// <summary>Draws a 2D line onto the screen</summary>
		/// <param name="startPoint">The start point of the line. (0, 0) is the top left of the screen</param>
		/// <param name="endPoint">The end point of the line. (0, 0) is the top left of the screen</param>
		/// <param name="color">The color of the line</param>
		public void DrawLine2D(Vec3 startPoint, Vec3 endPoint, Color color)
		{
			Vector2 _startPoint = new Vector2(startPoint.x, startPoint.y);
			Vector2 _endPoint = new Vector2(endPoint.x, endPoint.y);
			Renderer.DrawLine2D(color, _startPoint, _endPoint);
		}

		/// <summary>Draws a 3D line in the world</summary>
		/// <param name="startPoint">The start point of the line in world space</param>
		/// <param name="endPoint">The end point of the line in world space</param>
		/// <param name="color">The color of the line</param>
		public void DrawLine3D(Vec3 startPoint, Vec3 endPoint, Color color)
		{
			Vector3 _startPoint = new Vector3(startPoint.x, startPoint.y, startPoint.z);
			Vector3 _endPoint = new Vector3(endPoint.x, endPoint.y, endPoint.z);
			Renderer.DrawLine3D(color, _startPoint, _endPoint);
		}

		/// <summary>Draws a bunch of 2D lines that connect the points. Kinda like a connect the dots puzzle!</summary>
		/// <param name="points">A list of all the points the lines go through</param>
		/// <param name="color">The color of the lines</param>
		public void DrawPolyLine2D(Vec3[] points, Color color)
		{
			Renderer.DrawPolyLine2D(color, points.Select(vec => new Vector2(vec.x, vec.y)).ToArray());
		}

		/// <summary>Draws a bunch of 3D lines that connect the points. Kinda like a connect the dots puzzle!</summary>
		/// <param name="points">A list of all the points the lines go through</param>
		/// <param name="color">The color of the lines</param>
		public void DrawPolyLine3D(Vec3[] points, Color color)
		{
			Renderer.DrawPolyLine3D(color, points.Select(vec => new Vector3(vec.x, vec.y, vec.z)).ToArray());
		}

		/// <summary>Throttles and boosts to reach the given target speed</summary>
		/// <returns>The current forward speed of the car</returns>
		public float Throttle(float targetSpeed, bool backwards = false)
		{
			float carSpeed = Me.Local(Me.Velocity).x; // The car's speed in the forward direction
			float speedDiff = (targetSpeed * (backwards ? -1 : 1)) - carSpeed;
			Controller.Throttle = Utils.Cap(MathF.Pow(speedDiff, 2) * MathF.Sign(speedDiff) / 1000, -1, 1);
			Controller.Boost = targetSpeed > 1400 && speedDiff > 50 && carSpeed < 2250 && Controller.Throttle == 1 && !backwards;
			return carSpeed;
		}

		/// <summary>Turns to face a given target</summary>
		/// <param name="up">Which direction to face your roof</param>
		/// <returns>The target angles for pitch, yaw, and roll</returns>
		public float[] AimAt(Vec3 targetLocation, Vec3 up = new(), bool backwards = false)
		{
			Vec3 localTarget = Me.Local(targetLocation - Me.Location) * (backwards ? -1 : 1); // Where our target is in local coordinates
			Vec3 safeUp = up.Length() != 0 ? up : Vec3.Up; // Make sure "up" is not the zero vector (which is the default argument)
			Vec3 localUp = Me.Local(safeUp.Normalize()); // Where "up" is in local coordinates
			float[] targetAngles = new float[3] {
				MathF.Atan2(localTarget.z, localTarget.x), // Angle to pitch towards target
				MathF.Atan2(localTarget.y, localTarget.x), // Angle to yaw towards target
				MathF.Atan2(localUp.y, localUp.z) // Angle to roll upright
			};
			// Now that we have the angles we need to rotate, we feed them into the PD loops to determine the controller inputs
			Controller.Steer = SteerPD(targetAngles[1], -Me.LocalAngularVelocity[2] * 0.01f) * (backwards ? -1 : 1);
			Controller.Pitch = SteerPD(targetAngles[0], Me.LocalAngularVelocity[1] * 0.2f);
			Controller.Yaw = SteerPD(targetAngles[1], -Me.LocalAngularVelocity[2] * 0.15f);
			Controller.Roll = SteerPD(targetAngles[2], Me.LocalAngularVelocity[0] * 0.25f);

			return targetAngles; // Returns the angles, which could be useful for other purposes
		}

		/// <summary>A Proportional-Derivative control loop used for the "AimAt" function</summary>
		private static float SteerPD(float angle, float rate)
		{
			return Utils.Cap(MathF.Pow(35 * (angle + rate), 3) / 10 , -1f, 1f);
		}

		/// <summary>Searches through the ball prediction for the first valid shot given by the ShotCheck</summary>
		/// <param name="shotCheck">The function that determines which shot to go for, if any</param>
		/// <param name="target">The final resting place of the ball after we hit it (hopefully)</param>
		public static Shot FindShot(ShotCheck shotCheck, Target target)
		{
			return shotCheck(Ball.Prediction.Find(slice => shotCheck(slice, target) != null), target);
		}

		/// <summary>The default shot check. Will go for pretty much anything it can</summary>
		/// <param name="slice">The future moment of the ball we are aiming to hit</param>
		/// <param name="target">The final resting place of the ball after we hit it (hopefully)</param>
		public Shot DefaultShotCheck(BallSlice slice, Target target)
		{
			if (slice != null) // Check if the slice even exists
			{
				float timeRemaining = slice.Time - Game.Time;

				// Check first if the slice is in the future and if it's even possible to shoot at our target
				if (timeRemaining > 0 && target.Fits(slice.Location))
				{
					Ball ballAfterHit = slice.ToBall();
					Vec3 carFinVel = ((slice.Location - Me.Location) / timeRemaining).Cap(0, Car.MaxSpeed);
					ballAfterHit.velocity = carFinVel + slice.Velocity.Flatten(carFinVel.Normalize()) * 0.8f;
					Vec3 shotTarget = target.Clamp(ballAfterHit);

					// First, check if we can aerial
					AerialShot aerialShot = new AerialShot(Me, slice, shotTarget);
					if (aerialShot.IsValid(Me))
					{
						return aerialShot; // If so, go for it!
					}

					// If we can't aerial, let's try a ground shot
					GroundShot groundShot = new GroundShot(Me, slice, shotTarget);
					if (groundShot.IsValid(Me))
					{
						return groundShot;
					}

					// Otherwise, we'll try a jump shot
					JumpShot jumpShot = new JumpShot(Me, slice, shotTarget);
					if (jumpShot.IsValid(Me))
					{
						return jumpShot;
					}

					// And lastly, a double jump shot
					DoubleJumpShot doubleJumpShot = new DoubleJumpShot(Me, slice, shotTarget);
					if (doubleJumpShot.IsValid(Me))
					{
						return doubleJumpShot;
					}
				}
			}

			return null; // if none of those work, we'll just return null (meaning no shot was found)
		}
	}
}
