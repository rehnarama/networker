using UnityEngine;


public class Vector3PIDController
{
    private Vector3 previousError;
    private Vector3 integral;

    private float kP;
    private float kI;
    private float kD;

    public Vector3PIDController(float kP, float kI, float kD)
    {
        this.kP = kP;
        this.kI = kI;
        this.kD = kD;
    }


    public Vector3 simulate(Vector3 current, Vector3 target, float deltaTime)
    {
        // From https://en.wikipedia.org/wiki/PID_controller#Pseudocode
        var error = target - current;
        integral = integral + error * deltaTime;
        var derivative = (error - previousError) / deltaTime;

        var output = kP * error + kI * integral + kD * derivative;

        previousError = error;

        return output;
    }
}