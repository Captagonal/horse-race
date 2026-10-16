using Godot;
using System;
using System.Collections.ObjectModel;

public partial class Player : CharacterBody3D
{

	[Export] public float FallAcceleration { get; set; } = 30;

	[Export] public float JumpImpulse { get; set; } = 12;

	[Export] public float CameraSensitivity { get; set; } = .006f;
	[Export] public float Speed { get; set; } = 4;
	[Export] public float SprintSpeed { get; set; } = 9;
	private float _currentSpeed = 4;
	private Node3D _head, _camera;
	private RayCast3D _lookingAt;
	private RayCast3D _onTop;
	private float _sprintSpeed;
	public override void _Ready()
	{
		_sprintSpeed = SprintSpeed;
		_head = GetNode<Node3D>("Head");
		_camera = _head.GetNode<Camera3D>("Camera3D");
		_lookingAt = _camera.GetNode<RayCast3D>("RayCast3D");
		_onTop = GetNode<RayCast3D>("RayCast3D");
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

	float time = 0;
	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;
		time += d;
		GroundMaterial Ground = _onTop.GetCollider() as GroundMaterial;
		float speedEffect = Ground?.GroundType.GetSpeedEffect() ?? 1f;
		Vector3 velocity = Velocity;
		SprintSpeed = _sprintSpeed * speedEffect;
		// Add gravity
		if (!IsOnFloor())
		{
			velocity.Y -= FallAcceleration * (float)delta;
		}

		// Handle Jump
		if (Input.IsActionPressed("Jump") && IsOnFloor())
		{
			velocity.Y = JumpImpulse;
		}

		// Get input direction
		var input = Input.GetVector("Move_Left", "Move_Right", "Move_Forward", "Move_Back");
		var direction = (_head.GlobalTransform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();

		float accelerationRate = 7;
		float decelerationRate = 10f;
		float driftFactor = 40f;

		if (direction != Vector3.Zero)
		{
			// Gradually increase speed up to SprintSpeed while moving
			_currentSpeed = Mathf.MoveToward(_currentSpeed, SprintSpeed, accelerationRate * (float)delta);
			float targetX = direction.X * _currentSpeed;
			float targetZ = direction.Z * _currentSpeed;
			velocity.X = Mathf.MoveToward(velocity.X, targetX, driftFactor * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, targetZ, driftFactor * (float)delta);
			// velocity.X = direction.X * _currentSpeed;
			// velocity.Z = direction.Z * _currentSpeed;
		}
		else
		{
			// Gradually decay speed back to base Speed when letting go of keys
			_currentSpeed = Mathf.MoveToward(_currentSpeed, Speed, decelerationRate * (float)delta);

			velocity.X = Mathf.MoveToward(Velocity.X, 0, _currentSpeed * (float)delta);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _currentSpeed * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();

		// Camera bobbing and mouse/controller look code...
		Vector2 inputCam = Input.GetVector("Look_Left", "Look_Right", "Look_Up", "Look_Down");
		// var camHeight = Math.Sin(Math.Sqrt(Position.X * Position.X + Position.Y * Position.Y) * 1.5) / 5;
		// _camera.Position = new Vector3(0, (float)camHeight, 0);
		_camera.Position = new Vector3(_camera.Position.X, _camera.Position.Y + Mathf.Sin(time * 11) * 0.04f, _camera.Position.Z);
		if (Velocity.Length() < 0.1f)
		{
			_camera.Position = new Vector3(0,0,-0.039f);
			GetNode<AudioStreamPlayer2D>("Horse").Stop();
		}
		else
		{
			if (Velocity.Length() > 0.1f && !GetNode<AudioStreamPlayer2D>("Horse").Playing)
			{
				GetNode<AudioStreamPlayer2D>("Horse").Play();
			}
		}
		if (inputCam != Vector2.Zero)
		{
			_head.RotateY(-inputCam.X * CameraSensitivity * 13);
			_camera.RotateX(-inputCam.Y * CameraSensitivity * 13);
			Vector3 camRotation = _camera.Rotation;
			camRotation.X = Mathf.Clamp(camRotation.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
			_camera.Rotation = camRotation;
		}
	}
	public void OnInteract()
	{
		if (_lookingAt.IsColliding())
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
