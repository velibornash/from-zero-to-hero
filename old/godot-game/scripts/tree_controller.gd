extends StaticBody3D

enum State { STANDING, CHOPPED, GROWING }

var state: State = State.STANDING
var respawn_time: float = 12.0

var tree_model: Node3D
var stump: Node3D

func _ready():
	if Engine.is_editor_hint():
		return
	tree_model = get_node_or_null("TreeModel")
	stump = get_node_or_null("Stump")
	if not tree_model:
		build_fallback()

func build_fallback():
	var tm = ShaderMaterial.new()
	tm.shader = preload("res://shaders/toon.gdshader")
	tm.set_shader_parameter("albedo", Color(0.42, 0.28, 0.1))
	var cm = ShaderMaterial.new()
	cm.shader = preload("res://shaders/toon.gdshader")
	cm.set_shader_parameter("albedo", Color(0.165, 0.475, 0.122))

	tree_model = MeshInstance3D.new()
	tree_model.name = "TreeModel"
	var c = CylinderMesh.new()
	c.top_radius = 0.01; c.bottom_radius = 0.6; c.height = 1.0
	tree_model.set_script(null)
	tree_model.mesh = c; tree_model.material_override = cm; tree_model.position = Vector3(0,0.5,0)
	add_child(tree_model)

	var trunk = MeshInstance3D.new()
	var tr = CylinderMesh.new()
	tr.top_radius = 0.07; tr.bottom_radius = 0.1; tr.height = 0.8
	trunk.mesh = tr; trunk.material_override = tm; trunk.position = Vector3(0,0.4,0)
	tree_model.add_child(trunk)

	stump = get_node_or_null("Stump")
	if not stump:
		var s = MeshInstance3D.new()
		s.name = "Stump"; var sr = CylinderMesh.new()
		sr.top_radius = 0.08; sr.bottom_radius = 0.12; sr.height = 0.15
		s.mesh = sr; s.material_override = tm; s.position = Vector3(0,0.075,0); s.visible = false
		add_child(s); stump = s

	var col = get_node_or_null("TreeCollision")
	if not col:
		var c2 = CollisionShape3D.new()
		c2.name = "TreeCollision"; var sh = CylinderShape3D.new()
		sh.radius = 0.3; sh.height = 1.0
		c2.shape = sh; c2.position = Vector3(0,0.5,0)
		add_child(c2)

func on_interact(_hero: Node3D):
	if state == State.STANDING:
		chop()

func chop():
	state = State.CHOPPED
	if tree_model: tree_model.visible = false
	if stump: stump.visible = true
	GameState.add_wood(randi() % 3 + 2)

	await get_tree().create_timer(respawn_time).timeout
	if not is_instance_valid(self): return
	if stump: stump.visible = false
	if tree_model: tree_model.visible = true
	state = State.STANDING
