using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
public class CameraFreeFly : Component, IUpdatable
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 0.002f;
    public float sprintMultiplier = 2.5f;

    private float yaw;
    private float pitch;
    private bool firstMouse = true;
    private Vector2 lastMouse;

    public void Update()
    {
        // --- Mouse look ---
        Vector2 mouse = Raylib.GetMousePosition();
        if (firstMouse)
        {
            lastMouse = mouse;
            firstMouse = false;
        }

        Vector2 delta = mouse - lastMouse;
        lastMouse = mouse;

        if (Input.IsDown(MouseButton.Right))
        {
            yaw += delta.X * lookSensitivity;
            pitch -= delta.Y * lookSensitivity;
            pitch = Math.Clamp(pitch, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);

            Quaternion rotY = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            Quaternion rotX = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
            transform.rotation = rotX * rotY;
        }

        // --- Movement ---
        float speed = moveSpeed * Time.deltaTime;
        if (Input.IsDown(KeyboardKey.LeftShift)) speed *= sprintMultiplier;

        if (Input.IsDown(KeyboardKey.W)) transform.position += transform.forward * speed;
        if (Input.IsDown(KeyboardKey.S)) transform.position -= transform.forward * speed;
        if (Input.IsDown(KeyboardKey.D)) transform.position -= transform.right * speed;
        if (Input.IsDown(KeyboardKey.A)) transform.position += transform.right * speed;
        if (Input.IsDown(KeyboardKey.Space)) transform.position += Vector3.UnitY * speed;
        if (Input.IsDown(KeyboardKey.LeftControl)) transform.position -= Vector3.UnitY * speed;
    }
}
