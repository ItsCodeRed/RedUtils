using System;
using System.Threading;
using System.Drawing;
using RedUtils;
using RedUtils.Math;
/* 
 * This is the main file. It contains your bot class. Feel free to change the name!
 * An instance of this class will be created for each instance of your bot in the game.
 * Your bot derives from the "RedUtilsBot" class, contained in the Bot file inside the RedUtils project.
 * The run function listed below runs every tick, and should contain the custom strategy code (made by you!)
 * Right now though, it has a default ball chase strategy. Feel free to read up and use anything you like for your own strategy.
*/
namespace Bot
{
    // Your bot class! :D
    public class RedBot : RUBot
    {
        // We want the constructor for our Bot to extend from RUBot, but feel free to add some other initialization in here as well.
        public RedBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) { }

        // Runs every tick. Should be used to find an Action to execute
        public override void Run()
        {
            // Prints out the current action to the screen, so we know what our bot is doing
            Renderer.Text2D(Action != null ? Action.ToString() : "", new Vec3(10, 10), 4, Color.White);

            if (IsKickoff && Action == null)
            {
                bool goingForKickoff = true; // by default, go for kickoff
                foreach (Car teammate in Teammates)
                {
                    // if any teammates are closer to the ball, then don't go for kickoff
                    goingForKickoff = goingForKickoff && Me.Location.Dist(Ball.Location) <= teammate.Location.Dist(Ball.Location);
                }

                Action = goingForKickoff ? new Kickoff() : new GetBoost(Me, interruptible: false); // if we aren't going for the kickoff, get boost
            }
            else if (Action == null || (Action is Drive && Action.Interruptible))
            {
                // search for the first avaliable shot using DefaultShotCheck
                Shot shot = FindShot(DefaultShotCheck, new Target(TheirGoal));

                // if a shot is found, go for the shot. Otherwise, if there is an Action to execute, execute it. If none of the others apply, drive back to goal.
                Action = shot ?? Action ?? new Drive(Me, OurGoal.Location);
			}
        }
    }
}
