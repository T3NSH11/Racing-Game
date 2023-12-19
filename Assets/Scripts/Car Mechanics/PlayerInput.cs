using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CarMechanics))]
public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    private CarInput playerInput;
    private float throttleInput;
    private float smoothedThrottle;
    private float steeringInput;
    private float smoothedSteering;
    private float clutchInput;
    private float smoothedClutch;
    private float handBrakeInput;

    [Header("Car Controller")]
    private CarMechanics carController;

    [Header("Dampening Speed")]
    public float dampeningSpeed = 1;

    [Header("Player Input")]
    public CustomButton accelerateButton;
    public CustomButton decelerateButton;
    public CustomButton turnLeftButton;
    public CustomButton turnRightButton;

    [Header("Steering Curve")]
    public AnimationCurve steeringCurve;

    private void Awake()
    {
        playerInput = new CarInput();
        carController = GetComponent<CarMechanics>();
    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.Car.Throttle.performed += UpdateThrottle;
        playerInput.Car.Throttle.canceled += ResetThrottle;
        playerInput.Car.Steering.performed += UpdateSteering;
        playerInput.Car.Steering.canceled += ResetSteering;
        playerInput.Car.Clutch.performed += UpdateClutch;
        playerInput.Car.Clutch.canceled += ResetClutch;
        playerInput.Car.Handbrake.performed += UpdateHandbrake;
        playerInput.Car.Handbrake.canceled += ResetHandbrake;
    }

    private void Update()
    {
        HandleUI();
        smoothedThrottle = SmoothInput(throttleInput, smoothedThrottle);
        smoothedSteering = SmoothInput(steeringInput, smoothedSteering);
        smoothedClutch = SmoothInput(clutchInput, smoothedClutch);
        carController.SetInput(smoothedThrottle, smoothedSteering * steeringCurve.Evaluate(carController.speed), smoothedClutch, handBrakeInput);
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private float SmoothInput(float input, float output)
    {
        return Mathf.Lerp(output, input, Time.deltaTime * dampeningSpeed);
    }

    private void UpdateThrottle(InputAction.CallbackContext inputValue)
    {
        throttleInput = inputValue.ReadValue<float>();
    }

    private void ResetThrottle(InputAction.CallbackContext inputValue)
    {
        throttleInput = 0;
    }

    private void UpdateSteering(InputAction.CallbackContext inputValue)
    {
        steeringInput = inputValue.ReadValue<float>();
    }

    private void ResetSteering(InputAction.CallbackContext inputValue)
    {
        steeringInput = 0;
    }

    private void UpdateClutch(InputAction.CallbackContext value)
    {
        clutchInput = value.ReadValue<float>();
    }

    private void ResetClutch(InputAction.CallbackContext value)
    {
        clutchInput = 0;
    }

    private void UpdateHandbrake(InputAction.CallbackContext value)
    {
        handBrakeInput = value.ReadValue<float>();
    }

    private void ResetHandbrake(InputAction.CallbackContext value)
    {
        handBrakeInput = 0;
    }

    private void HandleUI()
    {
        if (accelerateButton != null && accelerateButton.IsPressed)
        {
            throttleInput += accelerateButton.SmoothInput;
        }
        if (decelerateButton != null && decelerateButton.IsPressed)
        {
            throttleInput -= decelerateButton.SmoothInput;
        }
        if (turnRightButton != null && turnRightButton.IsPressed)
        {
            steeringInput += turnRightButton.SmoothInput;
        }
        if (turnLeftButton != null && turnLeftButton.IsPressed)
        {
            steeringInput -= turnLeftButton.SmoothInput;
        }
    }
}
