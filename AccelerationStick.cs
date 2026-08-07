using Godot;

public partial class AccelerationStick : Node3D
{
	[Export(PropertyHint.Range, "0,3.14")] float rotationMax = 1.5f;
	[Export(PropertyHint.Range, "0,50")] float springStrength = 20f;
	[Export(PropertyHint.Range, "0,50")] float dampStrength = 20f;
	[Export(PropertyHint.Range, "0,1")] float bobbiness = 0.5f;

	public float Acceleration { get; set; }
	[Export]
	public float Target { get; set; }
	public float Error => Target - Acceleration;
	public float AbsError => float.Abs(Error);

	/// <summary> The change in the acceleration. </summary>
	float delta;

	Node3D pivot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pivot = GetChild<Node3D>(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		clampTarget();
		spring(d);
		damp(d);
		bool errorWasZero = change(d);
		if (errorWasZero) zeroDamp();
		clampAccel();
		pivot.Rotation = Vector3.Right * Acceleration;
	}

	void clampTarget() {
		Target = float.Clamp(Target, 0, 1);
	}

	void spring(float d) {
		delta += Error * springStrength * d;
	}

	void damp(float d) {
		delta = MoveTowards(delta, 0, dampStrength * d / (1f + AbsError));
	}

	bool change(float d) {
		int signBefore = float.Sign(Error);
		Acceleration += delta * d;
		return signBefore != float.Sign(Error);
	}

	void zeroDamp() {
		delta *= bobbiness;
	}

	void clampAccel() {
		Acceleration = float.Clamp(Acceleration, 0, 1);
	}

	static float MoveTowards(float a, float b, float d) {
		return a + float.Sign(b - a) * float.Clamp(d, 0, float.Abs(b - a));
	}
}
