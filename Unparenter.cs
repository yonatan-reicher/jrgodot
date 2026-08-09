using Godot;
using System;

public partial class Unparenter : Node
{
	[Export(hintString: "A node to set this parent as the sibling of")]
	Node sibling;

	public override void _Process(double _) {
		if (sibling is null)
			throw new Exception("This `Unparenter` does not have a selected sibling");
		Node parent = GetParent();
		if (parent is null)
			throw new Exception("This node does not have a parent!");
		// The actual logic
		parent.Reparent(sibling.GetParent());
	}
}
