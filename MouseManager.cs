using Godot;

public partial class MouseManager : Node {
	public static bool Captured =>
		Input.MouseMode == Input.MouseModeEnum.Captured;
	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _UnhandledInput(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetViewport().SetInputAsHandled();
		}
		if (@event is InputEventMouseButton mouseClick
			&& mouseClick.ButtonIndex == MouseButton.Left) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
			GetViewport().SetInputAsHandled();
		}
	}
}
