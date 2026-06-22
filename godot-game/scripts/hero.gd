extends CharacterBody3D

const SPEED = 4.5
const GRAVITY = 24.0
const INTERACT_RANGE = 2.5

var carrying_wood: bool = false
var facing_dir: Vector3 = Vector3.FORWARD
var target_rotation: float = 0.0
var current_rotation: float = 0.0
var was_moving: bool = false

@onready var visual: Node3D = $Visual
var skin: Node3D

func _ready():
	for c in visual.get_children():
		if c.has_method("idle"):
			skin = c
			break

func _physics_process(delta):
	if not is_on_floor():
		velocity.y -= GRAVITY * delta

	var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var dir = Vector3(input_dir.x, 0, input_dir.y).normalized()

	var is_moving = dir.length() > 0

	if is_moving:
		velocity.x = dir.x * SPEED
		velocity.z = dir.z * SPEED
		facing_dir = dir
		if visual:
			target_rotation = atan2(dir.x, -dir.z)
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED * 4.0)
		velocity.z = move_toward(velocity.z, 0, SPEED * 4.0)

	if is_moving and not was_moving:
		if skin and skin.has_method("move"):
			skin.move()
	elif not is_moving and was_moving:
		if skin and skin.has_method("idle"):
			skin.idle()
	was_moving = is_moving

	move_and_slide()

	if visual:
		current_rotation = lerp_angle(current_rotation, target_rotation, delta * 10.0)
		visual.rotation.y = current_rotation

func _unhandled_input(event):
	if event.is_action_pressed("interact"):
		try_interact()

func try_interact():
	var space = get_world_3d().direct_space_state
	var from = global_position + Vector3(0, 0.8, 0)
	var to = from + facing_dir * INTERACT_RANGE

	var query = PhysicsRayQueryParameters3D.new()
	query.from = from
	query.to = to
	query.exclude = [self]
	query.collision_mask = 1

	var result = space.intersect_ray(query)
	if result and result.collider:
		var node = result.collider
		if node.has_method("on_interact"):
			node.on_interact(self)

func set_carrying(value: bool):
	carrying_wood = value
