using Godot;
using System;
using System.Collections.ObjectModel;

public partial class Player : CharacterBody3D
{

	[Export] public int FallAcceleration { get; set; } = 30;

	[Export] public int JumpImpulse { get; set; } = 12;

	[Export] public float CameraSensitivity { get; set; } = .006f;
	[Export] public int Speed { get; set; } = 4;
	[Export] public int SprintSpeed { get; set; } = 9;
	private int _currentSpeed = 4;
	private Node3D _head, _camera;
	private RayCast3D _rayCast;
	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");
		_camera = _head.GetNode<Camera3D>("Camera3D");
		_rayCast = _camera.GetNode<RayCast3D>("RayCast3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	public override void _Input(InputEvent @event)
	{
		Vector2 inputCam = Input.GetVector("Look_Left", "Look_Right", "Look_Up", "Look_Down");
		if (@event is InputEventMouseMotion mouseMotion && inputCam == Vector2.Zero && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			_head.RotateY(-mouseMotion.Relative.X * CameraSensitivity);
			_camera.RotateX(-mouseMotion.Relative.Y * CameraSensitivity);
			var camRotation = _camera.Rotation;

			camRotation.X = Mathf.Clamp(camRotation.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));

			_camera.Rotation = camRotation;
		}
		else if (@event is InputEventKey keyEvent && keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		if (@event.IsActionPressed("Interact")) OnInteract();
		// if (@event.IsActionPressed("grab")) OnGrab();
		// if (@event.IsActionPressed("throw")) OnThrow();
		// if (@event.IsActionPressed("reset")) Checkpoint?.Restore(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (Input.IsActionPressed("Sprint"))
		{
			_currentSpeed = SprintSpeed;
		}
		else
		{
			_currentSpeed = Speed;
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity.Y -= FallAcceleration * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionPressed("Jump") && IsOnFloor())
		{
			velocity.Y = JumpImpulse;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		var input = Input.GetVector("Move_Left", "Move_Right", "Move_Forward", "Move_Back");
		var direction = (_head.GlobalTransform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * _currentSpeed;
			velocity.Z = direction.Z * _currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _currentSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _currentSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
		Vector2 inputCam = Input.GetVector("Look_Left", "Look_Right", "Look_Up", "Look_Down");
		if (inputCam == Vector2.Zero)
		{
			return;
		}
		_head.RotateY(-inputCam.X * CameraSensitivity * 13);
		_camera.RotateX(-inputCam.Y * CameraSensitivity * 13);
		Vector3 camRotation = _camera.Rotation;
		var camHeight = Math.Sin(Math.Sqrt( Position.X * Position.X + Position.Y * Position.Y));
		_camera.Position.Y = camHeight;
		camRotation.X = Mathf.Clamp(camRotation.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));

		_camera.Rotation = camRotation;
	}
	public void OnInteract()
	{
		if (_rayCast.IsColliding())
		{
			// var collider = _rayCast.GetCollider();
			// if (collider is IInteractable interactable)
			// {
			// 	interactable.Interact(this);
			// }
		}
	}

	public void ShowDialogue(string name, Texture2D image, Collection<string> greetings)
	{
		throw new NotImplementedException();
		//TODO: Implement dialogue system
	}
}
