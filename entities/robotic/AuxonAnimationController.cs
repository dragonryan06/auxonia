using Godot;
using System;

// NOTE TO FUTURE SELF: To smooth out these animations, probably put "moveforwardstart" and end animation nodes that autoplay through a middle one, so that the robot doesnt lean forward after each step, but instead the lean forward is triggered at the end of the path.

public partial class AuxonAnimationController : AnimationTree
{

	[Export]
	private int IDLE_VARIATION_RARITY = 1000;

	// Sucks but it seems this is how this must be done...
	// Literally no documentation anywhere for this part of the engine, and the code is phenomenally bad.
	public override void _Process(double delta)
	{
		Actor actor = GetParent<Actor>();

		Set("parameters/conditions/move", actor.navstate >= Actor.NavState.NAVIGATING);
		Set("parameters/Move/conditions/idle", actor.navstate == Actor.NavState.IDLE);
		Set("parameters/Move/conditions/walk", actor.navstate == Actor.NavState.MOVING);
		// Don't ask me why these are swapped, I don't know either, and I promise it's not as simple as you're thinking.
		Set("parameters/Move/conditions/turn_right", actor.navstate == Actor.NavState.TURNING_LEFT);
		Set("parameters/Move/conditions/turn_left", actor.navstate == Actor.NavState.TURNING_RIGHT);

		// Oh my god godot i love you but this is...
        AnimationNodeStateMachine moveStateMachine = (AnimationNodeStateMachine)((AnimationNodeStateMachine)TreeRoot).GetNode("Move");
		AnimationNodeAnimation walkForward = (AnimationNodeAnimation)moveStateMachine.GetNode("WalkForward");
		walkForward.TimelineLength = actor.walkCooldown.WaitTime;

		if (actor.navstate == Actor.NavState.IDLE)
		{
            Set("parameters/Idle/conditions/arm_swing", false);
            Set("parameters/Idle/conditions/look_left", false);
            Set("parameters/Idle/conditions/look_right", false);

            long rand = GD.Randi() % IDLE_VARIATION_RARITY;
			if (rand == 0)
			{
				Set("parameters/Idle/conditions/arm_swing", true);
			}
			else if (rand == 1)
			{
				Set("parameters/Idle/conditions/look_left", true);
			}
			else if (rand == 2)
			{
				Set("parameters/Idle/conditions/look_right", true);
			}
		}
	}
}
