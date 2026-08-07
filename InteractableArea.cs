using Godot;

public partial class InteractableArea : Area3D
{
	[Signal]
	public delegate void InteractedEventHandler(Node3D subject, InteractableArea interacted);

	public override void _Ready() {
		BodyEntered += body => {
			if (body is not Player p) return;
			p.AddInteractable(this);
		};
		BodyExited += body => {
			if (body is not Player p) return;
			p.RemoveInteractable(this);
		};
	}

	public void Interact(Node3D subject) {
		EmitSignalInteracted(subject, this);
	}
}
