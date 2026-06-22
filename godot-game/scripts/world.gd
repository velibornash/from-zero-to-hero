@tool
extends Node3D

const WORLD_SIZE = 40
const TREE_COUNT = 28
const FLOWER_COUNT = 100
const SLOT_SIZE = 2.5
const SLOT_GAP = 0.3
const GRID_COLS = 5
const GRID_ROWS = 4

const TREE_GLBS = [
	preload("res://assets/packs/nature-kit/Models/gltf/tree_oak.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/tree_cone.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/tree_default.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/tree_pineRoundA.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/tree_detailed.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/tree_small.glb"),
]
const FLOWER_GLBS = [
	preload("res://assets/packs/nature-kit/Models/gltf/flower_redA.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/flower_purpleA.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/flower_yellowA.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/flower_redB.glb"),
	preload("res://assets/packs/nature-kit/Models/gltf/flower_purpleC.glb"),
]
const ROCK_GLBS = [
	preload("res://assets/packs/fantasy-town-kit/Models/glb/rock-large.glb"),
	preload("res://assets/packs/fantasy-town-kit/Models/glb/rock-small.glb"),
	preload("res://assets/packs/fantasy-town-kit/Models/glb/rock-wide.glb"),
]

const TREE_CONTROLLER = preload("res://scripts/tree_controller.gd")

var slot_scene: PackedScene

func _ready():
	if get_child_count() == 0:
		generate_world()
	if not Engine.is_editor_hint():
		create_building_slots()

func generate_world():
	setup_environment()
	create_terrain()
	create_water()
	create_building_platforms()
	create_trees()
	create_flowers()
	create_rocks()
	create_fence()

func setup_environment():
	var env = WorldEnvironment.new()
	add_child(env); _reg(env)
	var e = Environment.new()
	env.environment = e
	e.ambient_light_color = Color(0.7, 0.75, 0.85)
	e.ambient_light_energy = 0.4
	e.ambient_light_sky_contribution = 0.0
	e.tonemap_mode = Environment.TONE_MAPPER_ACES
	e.glow_enabled = true; e.glow_levels = 1; e.glow_intensity = 0.3; e.glow_strength = 0.8

	var light = DirectionalLight3D.new()
	add_child(light); _reg(light)
	light.light_energy = 1.2; light.light_indirect_energy = 0.5
	light.shadow_enabled = true; light.shadow_bias = 0.05
	light.rotation = Vector3(deg_to_rad(-45), deg_to_rad(30), 0)
	light.shadow_blur = 0.1

	var fill = DirectionalLight3D.new()
	add_child(fill); _reg(fill)
	fill.light_energy = 0.2; fill.shadow_enabled = false
	fill.rotation = Vector3(deg_to_rad(30), deg_to_rad(-60), 0)

func create_terrain():
	var grass = ShaderMaterial.new()
	grass.shader = preload("res://shaders/toon.gdshader")
	grass.set_shader_parameter("albedo", Color(0.302, 0.655, 0.192))
	var m = MeshInstance3D.new()
	m.mesh = PlaneMesh.new()
	m.mesh.size = Vector2(WORLD_SIZE, WORLD_SIZE)
	m.mesh.subdivide_depth = 2; m.mesh.subdivide_width = 2
	m.material_override = grass
	m.position = Vector3(0, -0.05, 0); m.rotation = Vector3(deg_to_rad(-90), 0, 0)
	add_child(m); _reg(m)

	var floor = StaticBody3D.new()
	var floor_shape = CollisionShape3D.new()
	var floor_box = BoxShape3D.new()
	floor_box.size = Vector3(WORLD_SIZE, 0.2, WORLD_SIZE)
	floor_shape.shape = floor_box; floor_shape.position = Vector3(0, 0, 0)
	floor.add_child(floor_shape); floor.position = Vector3(0, 0, 0)
	add_child(floor); _reg(floor)

	var dirt = ShaderMaterial.new()
	dirt.shader = preload("res://shaders/toon.gdshader")
	dirt.set_shader_parameter("albedo", Color(0.557, 0.424, 0.212))
	var p = MeshInstance3D.new()
	p.mesh = PlaneMesh.new(); p.mesh.size = Vector2(3, 6)
	p.material_override = dirt
	p.position = Vector3(0, -0.02, 6); p.rotation = Vector3(deg_to_rad(-90), 0, 0)
	add_child(p); _reg(p)

func create_water():
	var mat = ShaderMaterial.new()
	mat.shader = preload("res://shaders/water.gdshader")
	mat.set_shader_parameter("shallow_color", Color(0.0, 0.55, 0.75, 0.75))
	mat.set_shader_parameter("deep_color", Color(0.0, 0.18, 0.35, 0.9))
	var m = MeshInstance3D.new()
	m.mesh = PlaneMesh.new(); m.mesh.size = Vector2(WORLD_SIZE * 0.3, WORLD_SIZE * 0.4)
	m.material_override = mat
	m.position = Vector3(-WORLD_SIZE * 0.3, -0.15, WORLD_SIZE * 0.25)
	m.rotation = Vector3(deg_to_rad(-90), 0, 0)
	add_child(m); _reg(m)

func create_building_platforms():
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var sx = -total_w / 2 + SLOT_SIZE / 2
	var sz = -total_h / 2 + SLOT_SIZE / 2
	for row in range(GRID_ROWS):
		for col in range(GRID_COLS):
			var x = sx + col * (SLOT_SIZE + SLOT_GAP)
			var z = sz + row * (SLOT_SIZE + SLOT_GAP)
			var pm = ShaderMaterial.new()
			pm.shader = preload("res://shaders/toon.gdshader")
			pm.set_shader_parameter("albedo", Color(0.85, 0.7, 0.4))
			var plat = MeshInstance3D.new()
			plat.mesh = BoxMesh.new(); plat.mesh.size = Vector3(SLOT_SIZE - 0.1, 0.1, SLOT_SIZE - 0.1)
			plat.material_override = pm; plat.position = Vector3(x, 0.05, z)
			add_child(plat); _reg(plat)
			var bm = ShaderMaterial.new()
			bm.shader = preload("res://shaders/toon.gdshader")
			bm.set_shader_parameter("albedo", Color(0.6, 0.45, 0.2))
			var border = MeshInstance3D.new()
			border.mesh = BoxMesh.new(); border.mesh.size = Vector3(SLOT_SIZE + 0.1, 0.05, SLOT_SIZE + 0.1)
			border.material_override = bm; border.position = Vector3(x, 0.0, z)
			add_child(border); _reg(border)

func create_building_slots():
	slot_scene = preload("res://scenes/building_slot.tscn")
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var sx = -total_w / 2 + SLOT_SIZE / 2
	var sz = -total_h / 2 + SLOT_SIZE / 2
	for row in range(GRID_ROWS):
		for col in range(GRID_COLS):
			var x = sx + col * (SLOT_SIZE + SLOT_GAP)
			var z = sz + row * (SLOT_SIZE + SLOT_GAP)
			var slot = slot_scene.instantiate()
			slot.position = Vector3(x, 0.0, z); slot.grid_pos = Vector2i(col, row)
			add_child(slot)

func create_trees():
	var rng = RandomNumberGenerator.new(); rng.seed = 42
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var forbid = []
	for row in range(GRID_ROWS):
		for col in range(GRID_COLS):
			var cx = -total_w / 2 + col * (SLOT_SIZE + SLOT_GAP) + SLOT_SIZE / 2
			var cz = -total_h / 2 + row * (SLOT_SIZE + SLOT_GAP) + SLOT_SIZE / 2
			forbid.append(Vector3(cx, 0, cz))

	for i in range(TREE_COUNT):
		var tx: float; var tz: float; var ok = false
		for _a in range(50):
			tx = rng.randf_range(-WORLD_SIZE * 0.4, WORLD_SIZE * 0.4)
			tz = rng.randf_range(-WORLD_SIZE * 0.4, WORLD_SIZE * 0.4)
			ok = true
			if abs(tx) < 2 and abs(tz - 6) < 4: ok = false
			if tx < -WORLD_SIZE * 0.25 and tz > WORLD_SIZE * 0.1: ok = false
			if abs(tx) < total_w / 2 + 1.5 and abs(tz) < total_h / 2 + 1.5: ok = false
			for o in forbid:
				if Vector3(tx, 0, tz).distance_to(o) < 3.0: ok = false; break
			if ok: break
		if not ok: continue

		var variant = rng.randi() % TREE_GLBS.size()
		var tree_body = StaticBody3D.new()
		tree_body.set_script(TREE_CONTROLLER)
		tree_body.position = Vector3(tx, 0, tz)
		tree_body.rotation_degrees = Vector3(0, rng.randf_range(0, 360), 0)
		var s = rng.randf_range(0.8, 1.3)
		tree_body.scale = Vector3(s, s, s)

		var model = TREE_GLBS[variant].instantiate()
		model.name = "TreeModel"
		tree_body.add_child(model)

		var stump_mat = ShaderMaterial.new()
		stump_mat.shader = preload("res://shaders/toon.gdshader")
		stump_mat.set_shader_parameter("albedo", Color(0.42, 0.28, 0.1))
		var stump = MeshInstance3D.new()
		stump.name = "Stump"
		var sr = CylinderMesh.new()
		sr.top_radius = 0.08; sr.bottom_radius = 0.12; sr.height = 0.15
		stump.mesh = sr; stump.material_override = stump_mat
		stump.position = Vector3(0, 0.075, 0); stump.visible = false
		tree_body.add_child(stump)

		var col = CollisionShape3D.new()
		col.name = "TreeCollision"
		var sh = CylinderShape3D.new()
		sh.radius = 0.35; sh.height = 1.0
		col.shape = sh; col.position = Vector3(0, 0.5, 0)
		tree_body.add_child(col)

		add_child(tree_body); _reg(tree_body)

func create_flowers():
	var rng = RandomNumberGenerator.new(); rng.seed = 123
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	for i in range(FLOWER_COUNT):
		var fx = rng.randf_range(-WORLD_SIZE * 0.42, WORLD_SIZE * 0.42)
		var fz = rng.randf_range(-WORLD_SIZE * 0.42, WORLD_SIZE * 0.42)
		if abs(fx) < total_w / 2 + 1 and abs(fz) < total_h / 2 + 1: continue
		if fx < -WORLD_SIZE * 0.25 and fz > WORLD_SIZE * 0.1: continue
		var variant = rng.randi() % FLOWER_GLBS.size()
		var model = FLOWER_GLBS[variant].instantiate()
		model.position = Vector3(fx, 0, fz)
		model.rotation_degrees = Vector3(0, rng.randf_range(0, 360), 0)
		var s = rng.randf_range(0.8, 1.2)
		model.scale = Vector3(s, s, s)
		add_child(model); _reg(model)

func create_rocks():
	var rng = RandomNumberGenerator.new(); rng.seed = 456
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	for i in range(8):
		var rx = rng.randf_range(-WORLD_SIZE * 0.35, WORLD_SIZE * 0.35)
		var rz = rng.randf_range(-WORLD_SIZE * 0.35, WORLD_SIZE * 0.35)
		if abs(rx) < total_w / 2 + 2 and abs(rz) < total_h / 2 + 2: continue
		if rx < -WORLD_SIZE * 0.25 and rz > WORLD_SIZE * 0.1: continue
		var variant = rng.randi() % ROCK_GLBS.size()
		var model = ROCK_GLBS[variant].instantiate()
		model.position = Vector3(rx, 0, rz)
		model.rotation_degrees = Vector3(rng.randf_range(-10, 10), rng.randf_range(0, 360), rng.randf_range(-10, 10))
		var s = rng.randf_range(0.8, 1.3)
		model.scale = Vector3(s, s, s)
		add_child(model); _reg(model)

func create_fence():
	var fm = ShaderMaterial.new()
	fm.shader = preload("res://shaders/toon.gdshader")
	fm.set_shader_parameter("albedo", Color(0.55, 0.35, 0.15))
	var total_w = GRID_COLS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var total_h = GRID_ROWS * (SLOT_SIZE + SLOT_GAP) - SLOT_GAP
	var margin = 0.5; var hw = total_w / 2 + margin; var hh = total_h / 2 + margin
	for p in [Vector3(-hw,0,-hh), Vector3(hw,0,-hh), Vector3(-hw,0,hh), Vector3(hw,0,hh)]:
		var post = MeshInstance3D.new()
		post.mesh = CylinderMesh.new()
		post.mesh.top_radius = 0.04; post.mesh.bottom_radius = 0.05; post.mesh.height = 0.25
		post.material_override = fm; post.position = p + Vector3(0, 0.125, 0)
		add_child(post); _reg(post)

func _reg(node: Node):
	if Engine.is_editor_hint():
		var root = get_tree().edited_scene_root
		if root: node.set_owner(root)
