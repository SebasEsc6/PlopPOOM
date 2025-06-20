public interface ICollisionResponder
{
    /// <summary>
    /// Called on owner to apply the effect described by msg.
    /// </summary>
    void OnCollision(CollisionMessageInfo msg);
}
