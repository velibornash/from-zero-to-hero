extends Node3D

enum State { EMPTY, CONSTRUCTING, ACTIVE }

signal construction_completed(slot)

var state: State = State.EMPTY
var grid_pos: Vector2i
var building_type: String = ""
var build_progress: float = 0.0
var build_duration: float = 0.0
var production_timer: float = 0.0

var progress_bar: Node3D
var smoke_particles: Node3D
var building_mesh: Node3D

const SAWMILL_MODEL = preload("res://assets/packs/fantasy-town-kit/Models/glb/watermill.glb")
const STAND_MODEL = preload("res://assets/packs/fantasy-town-kit/Models/glb/stall.glb")
const TOWER_MODEL = preload("res://assets/packs/castle-kit/Models/glb/tower-square-base.glb")
const WOODCUTTER_MODEL = preload("res://assets/packs/fantasy-town-kit/Models/glb/roof-gable.glb")
const SELLER_MODEL = preload("res://assets/packs/fantasy-town-kit/Models/glb/stall-green.glb")

func _ready():
	add_to_group("building_slots")
	setup_progress_bar()

func setup_progress_bar():
	progress_bar = Node3D.new()
	progress_bar.position = Vector3(0, 0.5, 0)
	add_child(progress_bar)

	var bg = MeshInstance3D.new()
	var bg_mesh = BoxMesh.new()
	bg_mesh.size = Vector3(0.6, 0.05, 0.08)
	bg.mesh = bg_mesh
	var bg_mat = StandardMaterial3D.new()
	bg_mat.albedo_color = Color(0.2, 0.2, 0.2)
	bg.material_override = bg_mat
	bg.position = Vector3(0, 0, 0)
	progress_bar.add_child(bg)

	for i in range(3):
		var bar = MeshInstance3D.new()
		var bar_mesh = BoxMesh.new()
		bar_mesh.size = Vector3(0.18, 0.04, 0.07)
		bar.mesh = bar_mesh
		var bar_mat = StandardMaterial3D.new()
		bar_mat.albedo_color = Color(0.0, 0.7, 0.0)
		bar.material_override = bar_mat
		bar.position = Vector3(-0.21 + i * 0.21, 0.005, 0)
		bar.scale = Vector3(0, 1, 1)
		progress_bar.add_child(bar)

	progress_bar.visible = false

func start_construction(type: String):
	if state != State.EMPTY:
		return

	building_type = type
	var info = GameState.buildings[type]
	build_duration = info["build_time"]
	build_progress = 0.0
	state = State.CONSTRUCTING
	progress_bar.visible = true

func _process(delta):
	match state:
		State.CONSTRUCTING:
			build_progress += delta
			var pct = clamp(build_progress / build_duration, 0.0, 1.0)
			update_progress(pct)
			if build_progress >= build_duration:
				finish_construction()

		State.ACTIVE:
			var info = GameState.buildings.get(building_type, {})
			if info.has("production_interval") and info["production_interval"] > 0:
				production_timer += delta
				if production_timer >= info["production_interval"]:
					production_timer = 0.0
					produce()

func update_progress(pct: float):
	if not progress_bar or progress_bar.get_child_count() < 4:
		return
	var full_bars = int(pct * 3.0)
	for i in range(3):
		var bar = progress_bar.get_child(i + 1)
		if i < full_bars:
			bar.scale.x = 1.0
		elif i == full_bars:
			bar.scale.x = (pct * 3.0) - full_bars
		else:
			bar.scale.x = 0.0

func finish_construction():
	state = State.ACTIVE
	progress_bar.visible = false
	spawn_smoke()
	show_building()
	construction_completed.emit(self)

func spawn_smoke():
	var cp = GPUParticles3D.new()
	var mat = StandardMaterial3D.new()
	mat.albedo_color = Color(0.7, 0.7, 0.7, 0.6)

	var em = SphereMesh.new()
	em.radius = 0.15
	em.height = 0.3

	cp.draw_pass_1 = em
	cp.material_override = mat
	cp.amount = 40
	cp.lifetime = 2.0
	cp.one_shot = true
	cp.explosiveness = 0.8
	cp.process_material = ParticleProcessMaterial.new()
	cp.process_material.direction = Vector3.UP
	cp.process_material.initial_velocity_min = 0.5
	cp.process_material.initial_velocity_max = 1.5
	cp.process_material.scale_min = 0.05
	cp.process_material.scale_max = 0.15
	cp.position = Vector3(0, 0.3, 0)
	add_child(cp)
	cp.emitting = true
	await get_tree().create_timer(2.5).timeout
	if is_instance_valid(cp):
		cp.queue_free()

func show_building():
	var model: PackedScene
	var s = 1.0
	match building_type:
		"sawmill":
			model = SAWMILL_MODEL
			s = 0.7
		"stand":
			model = STAND_MODEL
			s = 0.65
		"watchtower":
			model = TOWER_MODEL
			s = 0.8
		"auto_woodcutter":
			model = WOODCUTTER_MODEL
			s = 0.75
		"auto_seller":
			model = SELLER_MODEL
			s = 0.7
	if not model:
		return
	building_mesh = model.instantiate()
	building_mesh.scale = Vector3(s, s, s)
	building_mesh.position = Vector3(0, 0.05, 0)
	add_child(building_mesh)

func produce():
	var info = GameState.buildings.get(building_type, {})
	match info.get("produces", ""):
		"wood":
			GameState.add_wood(info["production_rate"])
		"gold":
			GameState.add_gold(info["production_rate"])
