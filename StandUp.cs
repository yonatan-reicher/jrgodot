using Godot;
using System;

[GlobalClass]
public partial class StandUp : Node {
	[Export(PropertyHint.Range, "0,6.28,or_greater,suffix:rad/sec")]
	float targetAngularVelocityClose = 3.14f;
	[Export(PropertyHint.Range, "0,62.8,or_greater,suffix:rad/sec")]
	float targetAngularVelocityFar = 31.4f;
	[Export(PropertyHint.ExpEasing)]
	float targetAngularVelocityEasing = 2f;
	[Export(PropertyHint.Range, "0,6.28,or_greater,suffix:rad/sec²")]
	float angularAccelerationClose = 3.14f;
	[Export(PropertyHint.Range, "0,62.8,or_greater,suffix:rad/sec²")]
	float angularAccelerationFar = 31.4f;
	[Export(PropertyHint.ExpEasing)]
	float angularAccelerationEasing = 2f;

	public bool Active { get; set; } = true;
	/// <summary>
	/// Please make sure this is normalized
	/// </summary>
	public Vector3 UpDir { get; set; } = Vector3.Up;

	RigidBody3D parent;

	public override void _Ready() {
		parent = GetParent() as RigidBody3D;
		if (parent is null) throw new Exception();
	}

	public override void _PhysicsProcess(double delta) {
		if (!Active) return;
		float fDelta = (float)delta;
		Vector3 ourUp = parent.GlobalBasis.Y;
		Vector3 rotAxis = ourUp.Cross(UpDir).Normalized();
		if (!rotAxis.IsNormalized()) { // Happens when no rotation needed
			return;
		}
		// In this function, all velocities and accelerations mentioned are
		// angular velocities and angular accelerations.
		float angle = ourUp.AngleTo(UpDir);
		float fTargetVel = ease(
			targetAngularVelocityClose,
			targetAngularVelocityFar,
			angle / float.Pi / 2f,
			targetAngularVelocityEasing);
		float fAccel = ease(
			angularAccelerationClose,
			angularAccelerationFar,
			angle / float.Pi / 2f,
			angularAccelerationEasing);
		Vector3 targetVel = rotAxis * fTargetVel;
		Vector3 currentVel = parent.AngularVelocity;
		Vector3 velDiff = targetVel - currentVel; // Turns out this works for angular velocities as well
		velDiff -= UpDir * UpDir.Dot(velDiff);
		Vector3 toApply = velDiff.LimitLength(fAccel * fDelta); // I wish this had a name
		angularAccelerate(toApply);

		// DebugDraw3D.DrawArrowRay(parent.Position, toApply.Normalized(), toApply.Length());
		// DebugDraw3D.DrawArrowRay(parent.Position, parent.AngularVelocity.Normalized(), parent.AngularVelocity.Length(), color: Color.FromString("red", new Color()));
	}

	void angularAccelerate(Vector3 dirTimesAngle) {
		Rid rid = parent.GetRid();
		Vector3 inertia = (Vector3)PhysicsServer3D.BodyGetDirectState(rid)
			.InverseInertia.Inverse();
		parent.ApplyTorqueImpulse(dirTimesAngle * inertia.X);
	}

	static Quaternion moveTowards(Quaternion from, Quaternion to, float maxAngle) {
		float angle = from.AngleTo(to);
		float weight = float.Clamp(maxAngle / angle, 0f, 1f);
		return from.Slerp(to, weight);
	}

	static float ease(float from, float to, float x, float curve) {
		return from + (to - from) * Mathf.Ease(x, curve);
	}
}
