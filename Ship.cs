using Godot;

public partial class Ship : RigidBody3D
{
	[ExportGroup("Sit")]
	[Export] Node3D sit;
	[Export(PropertyHint.Range, "0,10")] float sitSpringStrength = 1f;
	[ExportGroup("Flight Stick")]
	[Export] FlightStick flightStick;
	[Export(PropertyHint.Range, "0,2")] float flightStickMouseSensitivity = 1f;
	[Export(PropertyHint.Range, "0,2")] float flightStickRollStrength = 1f;
	[Export(PropertyHint.Range, "0,2000")] float pitchAndYawStrength = 1000f;
	[Export(PropertyHint.Range, "0,200")] float rollStrength = 100f;
	[ExportGroup("Acceleration Stick")]
	[Export] AccelerationStick accelerationStick;
	[ExportGroup("Ramp")]
	[Export] HingeJoint3D rampHinge;

	Label debugLabel;
	Player pilot;

	bool _active;
	public bool Active {
		get => _active;
		set {
			_active = value;
			if (_active) onActivate();
			else onDeactivate();
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		debugLabel = GetNode<Label>("%DebugLabel");
		ProcessPhysicsPriority = flightStick.ProcessPhysicsPriority + 1;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (!Active) return;
		if (@event is InputEventMouseMotion mouseMotion
			&& Input.MouseMode == Input.MouseModeEnum.Captured) {
			Vector2 move = mouseMotion.ScreenRelative
				* flightStickMouseSensitivity * 0.01f;
			flightStick.Yaw += move.X;
			flightStick.Pitch += move.Y;
		}
	}

	public override void _Process(double delta) {
		if (!Active) return;

		float d = flightStickRollStrength * (float)delta;
		if (Input.IsActionPressed("roll_left")) flightStick.Roll -= d;
		if (Input.IsActionPressed("roll_right")) flightStick.Roll += d;

		if (Input.IsKeyPressed(Key.W)) {
			accelerationStick.Target = 1;
		} else {
			accelerationStick.Target = 0;
		}
		if (Input.IsKeyPressed(Key.Space))
			ApplyCentralForce(GlobalBasis.Y.Normalized() * 10.5f * Mass);
	}

	public override void _PhysicsProcess(double delta) {
		if (Active) {
			// Move the pilot towards their sit
			// PhysicsServer3D.BodySetState(
			// 	pilot.GetRid(),
			// 	PhysicsServer3D.BodyState.Transform,
			// 	sit.GlobalTransform
			// );
			// PhysicsServer3D.BodySetState(
			// 	pilot.GetRid(),
			// 	PhysicsServer3D.BodyState.LinearVelocity,
			// 	LinearVelocity
			// );
			// PhysicsServer3D.BodySetState(
			// 	pilot.GetRid(),
			// 	PhysicsServer3D.BodyState.AngularVelocity,
			// 	AngularVelocity
			// );
			pilot.UpDir = GlobalBasis.Y.Normalized();
			pilot.CameraDir = -GlobalBasis.Z.Normalized();
		}

		System.Diagnostics.Debug.Assert(flightStick.Pitch < 1);
		var f = flightStick;
		var a = accelerationStick;
		var g = GlobalBasis.Orthonormalized();
		ApplyTorque(-g.Y * f.Yaw * pitchAndYawStrength);
		ApplyTorque(-g.X * f.Pitch * pitchAndYawStrength);
		ApplyTorque(-g.Z * f.Roll * rollStrength);
		ApplyCentralForce(-g.Z * a.Acceleration * Mass * 2f);
	}

	void onActivate() {
		rampHinge.SetFlag(HingeJoint3D.Flag.EnableMotor, true);
	}

	void onDeactivate() {
		rampHinge.SetFlag(HingeJoint3D.Flag.EnableMotor, false);
	}

	void Interact(Node3D interactor) {
		if (!Active) {
			Active = true;
			if (interactor is Player p) {
				p.Active = false;
				pilot = p;
			} else {
				pilot = null;
			}
		} else {
			Active = false;
			if (pilot is not null) {
				pilot.Active = true;
			}
		}
	}
}
