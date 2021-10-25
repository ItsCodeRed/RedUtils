using System;
using RedUtils.Math;

namespace RedUtils
{
	/// <summary>A kickoff action, which performs a speedflip kickoff</summary>
	public class Kickoff : IAction
	{
		/// <summary>Kickoffs aren't interruptible, so this will always be false</summary>
		public bool Interruptible
		{ get; set; }
		/// <summary>Whether or not the kickoff pepriod has ended</summary>
		public bool Finished
		{ get; set; }

		/// <summary>Whether or not we have speedflipped</summary>
		private bool _speedFlipped = false;
		/// <summary>The speedflip sub action</summary>
		private SpeedFlip _speedFlip = null;

		/// <summary>Initaliazes a new kickoff action</summary>
		public Kickoff()
		{
			Interruptible = false;
			Finished = false;
		}

		/// <summary>Performs this kickoff action</summary>
		public void Run(RUBot bot)
		{
			if (_speedFlip != null && !_speedFlip.Finished)
			{
				// If we are speed flipping, make sure to hold down boost
				bot.Controller.Boost = true;
				_speedFlip.Run(bot);
			}
			else
			{
				bot.Throttle(Car.MaxSpeed);
				// Aim at a point slightly offset from the ball, so we get an optimal 50/50 on the kickoff
				bot.AimAt(Ball.Location - Ball.Location.Direction(bot.TheirGoal.Location) * 170);

				if (!bot.IsKickoff)
				{
					// If the kickoff period has ended, finish this action
					Finished = true;
				}
				else if (bot.Me.Velocity.Length() > 600 && !_speedFlipped)
				{
					// When we are moving fast enough, start speed flipping
					_speedFlipped = true;
					_speedFlip = new SpeedFlip(bot.Me.Location.Direction(Ball.Location - Ball.Location.Direction(bot.TheirGoal.Location) * 170));
				}
				else if (bot.Me.Location.Dist(Ball.Location) < 800 && bot.Me.IsGrounded)
				{
					// When we are close enough to the ball, dodge into it
					bot.Action = new Dodge(Ball.Location.Direction(bot.TheirGoal.Location), 0.18f);
				}
			}
		}
	}
}
