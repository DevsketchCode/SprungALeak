using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

    public void SetProperties(Vector3 direction, float moveSpeed)
    {
        moveDirection = direction;
        speed = moveSpeed;
    }

    void Update()
    {
        // The obstacle moves along its initial Z-axis.
        transform.position += moveDirection * speed * Time.deltaTime;
    }
}