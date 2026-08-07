using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Player : RigidBody3D
{
	[ExportGroup("Camera")]
	[Export] Node3D camera;
	[Export(PropertyHint.Range, "0,1")] float mouseSensitivity = 0.25f;

	[ExportGroup("Movement")]
	[Export] float movementSpeed = 1f;

	[ExportGroup("Physics")]
	[Export] float standUpSpringStrength = 150f;
	[Export] float lookAtSpringStrength = 1f;

	Label debugLabel;
	List<InteractableArea> interactables = new();

	public bool Active { get; set; } = true;

	public Vector3 UpDir { get; set; } = Vector3.Up;
	public Vector3 CameraDir { get; set; } = Vector3.Forward;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		debugLabel = GetNode<Label>("%DebugLabel");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (!Active) return;
		if (
			@event is InputEventMouseMotion mouseMotion
			&& Input.MouseMode == Input.MouseModeEnum.Captured
		) {
			onMouseLookAround(mouseMotion.ScreenRelative);
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetViewport().SetInputAsHandled();
		}
		if (@event is InputEventMouseButton mouseClick && mouseClick.ButtonIndex == MouseButton.Left) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
			GetViewport().SetInputAsHandled();
		}
		if (!Active) return;
		if (@event.IsActionPressed("interact")) {
			interact();
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _Process(double delta) {
		camera.LookAt(
			target: camera.GlobalPosition + CameraDir,
			up: GlobalBasis.Y
		);
	}

	public override void _PhysicsProcess(double delta) {
		if (Active) onMoveAround(moveInput());
		springUp();
		springToCameraTarget();
	}

	// The player is looking around with the mouse.
	void onMouseLookAround(Vector2 mouseMovement) {
		// The rotation is applied to the target, not to the actual camera.
		// The camera looks at the target, and the player rotates towards to the
		// target in it's Y rotation.
		CameraDir = CameraDir
			.Rotated(camera.GlobalBasis.X, -mouseMovement.Y * mouseSensitivity)
			.Rotated(Vector3.Up, -mouseMovement.X * mouseSensitivity);
	}

	// Get the movement input.
	Vector2 moveInput() {
		return Input.GetVector(
			"move_left", "move_right", "move_backward", "move_forward"
		);
	}

	// The player is moving around. Showed be called only in physics processing.
	void onMoveAround(Vector2 movement) {
		Vector3 forward = -camera.GlobalBasis.Z.Normalized();
		Vector3 right = camera.GlobalBasis.X.Normalized();
		Vector3 rotatedMovement = forward * movement.Y + right * movement.X;
		Vector3 dir = rotatedMovement.Normalized();
		ApplyCentralForce(dir * movementSpeed);
	}

	// Spring rotation to stand up
	void springUp() {
		Vector3 ourUp = GlobalBasis.Y.Normalized(), targetUp = UpDir;
		float angle = ourUp.AngleTo(targetUp);
		float angularSpringForce = angle * standUpSpringStrength;
		Vector3 axis = ourUp.Cross(targetUp);
		ApplyTorque(axis * angularSpringForce);
	}

	void springToCameraTarget() {
		Vector3 forward = -GlobalBasis.Z;
		Vector3 fixedForward = flattenY(forward);
		float angle = fixedForward.AngleTo(flattenY(CameraDir));
		float force = angle * lookAtSpringStrength;
		ApplyTorque(Vector3.Up * force);
	}

	void interact() {
		// Pick the closest interact-able
		InteractableArea i = interactables
			.MinBy(i => i.Position.DistanceSquaredTo(Position));
		if (i is not null) i.Interact(this);
	}

	static Vector3 flattenY(Vector3 v) {
		return v - Vector3.Up * v.Y;
	}

	public void AddInteractable(InteractableArea i) => interactables.Add(i);
	public void RemoveInteractable(InteractableArea i) => interactables.Remove(i);
}
