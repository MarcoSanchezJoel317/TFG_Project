using UnityEngine;

public class TemporaryTargetController : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidad para mover el target

    void Update()
    {
        // Mover el target con las flechas del teclado o WASD
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;

        transform.position += movement;
    }
}
