using System.Diagnostics;

public interface Controllable {
    /// <summary>
    /// Called when this object is set to be the current controlled object.
    /// </summary>
    void OnGotControl();
    /// <summary>
    /// Called when this object gives up it's control, or when something forces
    /// it to.
    /// </summary>
    void OnGiveUpControl();
}

/// <summary>
/// This is not about Control nodes, but about what object is currently being
/// controlled.
/// </summary>
public static class ControlManager {
    public static ListStack<Controllable> askingForControl = new();
    public static Controllable CurrentlyControlled => askingForControl.TryPeek();
    /// <summary>
    /// Asks (and takes) control from the current controlled object.
    /// </summary>
    public static void AskForControl(Controllable c) {
        var prev = CurrentlyControlled;
        askingForControl.Push(c);
        prev?.OnGiveUpControl();
        c?.OnGotControl();
    }
    public static void GiveUpControl(Controllable c) {
        Debug.Assert(askingForControl.Contains(c));
        bool wasInControl = CurrentlyControlled == c;
        askingForControl.Remove(c);
        if (wasInControl) {
            c?.OnGiveUpControl();
            CurrentlyControlled?.OnGotControl();
        }
    }
}
