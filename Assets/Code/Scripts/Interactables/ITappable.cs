using UnityEngine;

/// <summary>
/// Interface used by tabbable objects.
/// By implementing this interface the object the script is attached to
/// can be tapped on in 3d space.
/// </summary>
public interface ITappable
{
    /// <summary>
    /// Function that is executed when tapped on object.
    /// </summary>
    void OnTapped();
}
