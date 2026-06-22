extends Node3D

var hero: CharacterBody3D
var hud: Control
var demo_buildings: Array[Node3D] = []

@onready var world: Node3D = $World

const HERO_SCENE_PATH = "res://addons/gdquest_gdbot/gdbot_skin.tscn"
const SAWMILL = preload("res://assets/packs/fantasy-town-kit/Models/glb/watermill.glb")
const STALL = preload("res://assets/packs/fantasy-town-kit/Models/glb/stall.glb")

func _ready():
	DisplayServer.window_set_size(Vector2i(1280, 720))
	clean_world()
	setup_lights()
	spawn_hero()
	spawn_starting_buildings()
	setup_camera()
	setup_hud()
	auto_start_demo_construction()

func spawn_hero():
	var hero_scene = preload("res://scenes/hero.tscn")
	hero = hero_scene.instantiate()
	hero.position = Vector3(0, 1.5, 0)
	add_child(hero)

func attach_hero_model(h: CharacterBody3D):
	var visual = h.get_node("Visual")
	if not visual:
		return
	for c in visual.get_children():
		visual.remove_child(c)
		c.queue_free()

	var packed_scene = load(HERO_SCENE_PATH)
	if not packed_scene:
		print("ERROR: Failed to load hero scene")
		return
	print("Hero scene loaded, instantiating...")
	var model = packed_scene.instantiate()
	if not model:
		print("ERROR: Failed to instantiate hero model")
		return
	print("Hero model instantiated: ", model.get_child_count(), " children")
	model.scale = Vector3(1.0, 1.0, 1.0)
	model.position = Vector3(0, -0.5, 0)
	visual.add_child(model)
	visual.position = Vector3(0, -0.5, 0)
	print("Hero world pos: ", h.global_position, " Visual pos: ", visual.global_position, " Model pos: ", model.global_position)
	h.set_meta("gdbot_skin", model)

func spawn_starting_buildings():
	pass

func clean_world():
	for c in world.get_children():
		if c is WorldEnvironment:
			continue
		if c is MeshInstance3D:
			if c.scene_file_path != "":
				c.queue_free()
				continue
			var m = c.mesh
			if m and m is CylinderMesh:
				c.queue_free()
			continue
		if c is StaticBody3D:
			if c.get_script():
				c.queue_free()
			continue
		if not c.scene_file_path.is_empty():
			c.queue_free()
			continue
		c.queue_free()

func setup_lights():
	var sun = DirectionalLight3D.new()
	sun.light_energy = 1.5
	sun.shadow_enabled = true
	sun.shadow_bias = 0.05
	sun.shadow_blur = 0.1
	sun.rotation = Vector3(deg_to_rad(-45), deg_to_rad(30), 0)
	add_child(sun)

	var fill = DirectionalLight3D.new()
	fill.light_energy = 0.3
	fill.shadow_enabled = false
	fill.rotation = Vector3(deg_to_rad(30), deg_to_rad(-60), 0)
	add_child(fill)

func setup_camera():
	var cam = Camera3D.new()
	cam.current = true
	cam.position = Vector3(0, 8, 12)
	cam.rotation = Vector3(deg_to_rad(-35), 0, 0)
	cam.near = 0.1; cam.far = 60.0
	add_child(cam)

func setup_hud():
	var hud_scene = preload("res://scenes/ui/hud.tscn")
	hud = hud_scene.instantiate()
	add_child(hud)

var demo_time: float = 0.0

func _process(delta):
	demo_time += delta
	for b in demo_buildings:
		var base_y = b.position.y
		b.position.y = base_y + sin(demo_time * 1.5 + b.position.x) * 0.03
	
	if hero and Input.is_action_just_pressed("build_menu"):
		if hud:
			var bp = hud.get_node_or_null("BuildPanel")
			if bp: bp.visible = not bp.visible

func auto_start_demo_construction():
	await get_tree().create_timer(0.5).timeout
	var slots = get_tree().get_nodes_in_group("building_slots")
	for slot in slots:
		if slot.has_method("start_construction"):
			var d = slot.global_position.distance_to(Vector3(0, 0, 0))
			if d < 2.0:
				slot.start_construction("sawmill")
				break
