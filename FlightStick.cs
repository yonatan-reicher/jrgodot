using Godot;

public partial class FlightStick : Node3D
{
	[Export(hintString: "The part that rotates")] Node3D head;
	[Export(PropertyHint.Range, "0,1.57")] float rollMaxAngle = 1;
	[Export(PropertyHint.Range, "0,1.57")] float pitchAndYawMaxAngle = 1;
	[Export(PropertyHint.Range, "0,2")] float springStrength = 1;
	[Export(PropertyHint.Range, "0,2")] float dampStrength = 1;

	public float Roll { get; set; }
	public float Pitch { get; set; }
	public float Yaw { get; set; }

	public override void _PhysicsProcess(double delta) {
		float d = (float)delta;
		spring(d);
		clamp();
		update();
	}

	void clamp() {
		Roll = float.Clamp(Roll, -1, 1);
		Pitch = float.Clamp(Pitch, -1, 1);
		Yaw = float.Clamp(Yaw, -1, 1);
	}

	void spring(float d) {
		Roll -= Roll * springStrength * d;
		Pitch -= Pitch * springStrength * d;
		Yaw -= Yaw * springStrength * d;
	}

	void update() {
		head.RotateZ(-Roll * rollMaxAngle - head.Rotation.Z);
		Basis = new Basis(Vector3.Up, Yaw * pitchAndYawMaxAngle)
			* new Basis(Vector3.Right, -Pitch * pitchAndYawMaxAngle);
	}
}
