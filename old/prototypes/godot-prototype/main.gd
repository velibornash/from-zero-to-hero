@tool
extends Node3D

func _ready() -> void:
	if get_child_count() > 0:
		return
	_setup_environment()
	_setup_lighting()
	_create_tiles()
	_create_building()
	_create_hero()
	_setup_camera()

func _mat(color: Color) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.metallic = 0.0
	m.roughness = 0.75
	return m

func _setup_environment() -> void:
	var we := WorldEnvironment.new()
	var env := Environment.new()
	env.background_color = Color(0.55, 0.65, 0.90)
	env.background_mode = Environment.BG_COLOR
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.45, 0.50, 0.60)
	env.ambient_light_energy = 0.4
	we.environment = env
	add_child(we)

func _setup_lighting() -> void:
	var d := DirectionalLight3D.new()
	d.light_energy = 2.2
	d.light_indirect_energy = 0.6
	d.shadow_enabled = true
	d.shadow_bias = 0.01
	d.rotation = Vector3(-deg_to_rad(35), deg_to_rad(50), 0)
	add_child(d)

func _setup_camera() -> void:
	var cam := Camera3D.new()
	add_child(cam)
	cam.current = true
	cam.position = Vector3(14, 11, 14)
	cam.look_at(Vector3(7.7, 0.5, 7.7))
	var vp := get_viewport()
	if vp:
		vp.msaa = Viewport.MSAA_4X

func _create_tiles() -> void:
	var pal := {
		grass=Color(0.20, 0.55, 0.10), dirt=Color(0.60, 0.35, 0.12),
		stone=Color(0.45, 0.45, 0.45), water=Color(0.10, 0.30, 0.60),
		forest=Color(0.08, 0.35, 0.05)
	}
	var kinds: Array[String] = ["grass","grass","dirt","grass","water","stone","forest","grass","dirt"]
	for x in 7:
		for z in 7:
			var t := MeshInstance3D.new()
			t.mesh = BoxMesh.new()
			t.mesh.size = Vector3(2.0, 0.3, 2.0)
			var k = kinds[(x*3+z*7)%kinds.size()]
			if x==3 and z==3: k = "dirt"
			t.mesh.material = _mat(pal[k])
			t.position = Vector3(x*2.2+1.1, 0.15, z*2.2+1.1)
			add_child(t)

func _create_building() -> void:
	var root := Node3D.new()
	root.position = Vector3(1.1, 0, 11.0)

	var wood := _mat(Color(0.48,0.28,0.10))
	var roof_c := _mat(Color(0.60,0.18,0.12))
	var door_c := _mat(Color(0.30,0.18,0.06))
	var win_c := _mat(Color(0.55,0.70,0.85))

	var walls := MeshInstance3D.new()
	walls.mesh = BoxMesh.new()
	walls.mesh.size = Vector3(2.0, 1.6, 2.0)
	walls.mesh.material = wood
	walls.position = Vector3(0, 0.8, 0)
	root.add_child(walls)

	# Gable roof — ridge along X at y=2.8, eaves at y=1.6
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	st.set_material(roof_c)
	# front slope
	st.add_vertex(Vector3(-1.3,2.8,0));st.add_vertex(Vector3(1.3,2.8,0));st.add_vertex(Vector3(-1.3,1.6,-1.3))
	st.add_vertex(Vector3(1.3,2.8,0));st.add_vertex(Vector3(1.3,1.6,-1.3));st.add_vertex(Vector3(-1.3,1.6,-1.3))
	# back slope
	st.add_vertex(Vector3(-1.3,2.8,0));st.add_vertex(Vector3(1.3,2.8,0));st.add_vertex(Vector3(-1.3,1.6,1.3))
	st.add_vertex(Vector3(1.3,2.8,0));st.add_vertex(Vector3(1.3,1.6,1.3));st.add_vertex(Vector3(-1.3,1.6,1.3))
	# left gable
	st.add_vertex(Vector3(0,2.8,0));st.add_vertex(Vector3(-1.3,1.6,-1.3));st.add_vertex(Vector3(-1.3,1.6,1.3))
	# right gable
	st.add_vertex(Vector3(0,2.8,0));st.add_vertex(Vector3(1.3,1.6,-1.3));st.add_vertex(Vector3(1.3,1.6,1.3))
	st.generate_normals()
	var roof_mi := MeshInstance3D.new()
	roof_mi.mesh = st.commit()
	root.add_child(roof_mi)

	# Door
	var door := MeshInstance3D.new()
	door.mesh = BoxMesh.new();door.mesh.size = Vector3(0.4,0.7,0.02);door.mesh.material=door_c
	door.position = Vector3(0,0.35,-1.01)
	root.add_child(door)

	# Windows
	for wx in [-0.6,0.6]:
		var w := MeshInstance3D.new()
		w.mesh = BoxMesh.new();w.mesh.size = Vector3(0.3,0.3,0.02);w.mesh.material=win_c
		w.position = Vector3(wx,0.8,-1.01)
		root.add_child(w)

	add_child(root)

func _create_hero() -> void:
	var hero := Node3D.new()
	hero.position = Vector3(7.7, 0, 7.7)

	var skin:=_mat(Color(0.95,0.78,0.60))
	var tunic:=_mat(Color(0.12,0.28,0.58))
	var boot:=_mat(Color(0.28,0.16,0.06))
	var metal:=_mat(Color(0.72,0.72,0.80))
	var gold:=_mat(Color(0.85,0.62,0.10))
	var shield_c:=_mat(Color(0.65,0.12,0.12))
	var belt_c:=_mat(Color(0.32,0.20,0.06))
	var cape_c:=_mat(Color(0.70,0.18,0.12))
	var hat_c:=_mat(Color(0.75,0.12,0.08))

	# Body
	var body:=MeshInstance3D.new()
	body.mesh = CapsuleMesh.new();body.mesh.height=0.9;body.mesh.radius=0.30;body.mesh.material=tunic
	body.position = Vector3(0,0.65,0);hero.add_child(body)

	# Head
	var head:=MeshInstance3D.new()
	head.mesh = SphereMesh.new();head.mesh.height=0.35;head.mesh.radius=0.22;head.mesh.material=skin
	head.position = Vector3(0,1.20,0);hero.add_child(head)

	# Cone hat
	var st:=SurfaceTool.new();st.begin(Mesh.PRIMITIVE_TRIANGLES);st.set_material(hat_c)
	var seg:=10;var hr:=0.26;var hh:=0.38
	for i in seg:
		var a1:=float(i)/seg*TAU;var a2:=float(i+1)/seg*TAU
		st.add_vertex(Vector3(0,hh,0));st.add_vertex(Vector3(cos(a1)*hr,0,sin(a1)*hr));st.add_vertex(Vector3(cos(a2)*hr,0,sin(a2)*hr))
	st.generate_normals()
	var hat:=MeshInstance3D.new();hat.mesh=st.commit();hat.position=Vector3(0,1.35,0);hero.add_child(hat)

	# Arms
	for side in [-0.35,0.35]:
		var arm:=MeshInstance3D.new()
		arm.mesh = CylinderMesh.new();arm.mesh.height=0.45;arm.mesh.top_radius=0.06;arm.mesh.bottom_radius=0.06;arm.mesh.material=skin
		arm.position = Vector3(side,0.90,0);arm.rotation=Vector3(0,0,-0.3 if side>0 else 0.3)
		hero.add_child(arm)

	# Legs (boots)
	for side in [-0.15,0.15]:
		var leg:=MeshInstance3D.new()
		leg.mesh = CylinderMesh.new();leg.mesh.height=0.28;leg.mesh.top_radius=0.09;leg.mesh.bottom_radius=0.09;leg.mesh.material=boot
		leg.position = Vector3(side,0.14,0);hero.add_child(leg)

	# Belt
	var belt:=MeshInstance3D.new()
	belt.mesh = CylinderMesh.new();belt.mesh.height=0.02;belt.mesh.top_radius=0.31;belt.mesh.bottom_radius=0.31;belt.mesh.material=belt_c
	belt.position=Vector3(0,0.45,0);belt.rotation=Vector3(PI/2,0,0);hero.add_child(belt)

	# Cape
	var cape:=MeshInstance3D.new()
	cape.mesh=BoxMesh.new();cape.mesh.size=Vector3(0.50,0.50,0.02);cape.mesh.material=cape_c
	cape.position=Vector3(0,0.50,-0.28);cape.rotation=Vector3(0.10,0,0);hero.add_child(cape)

	# Sword
	var sw:=Node3D.new();sw.position=Vector3(0.50,0.45,0);sw.rotation=Vector3(0,0,-0.15)
	var blade:=MeshInstance3D.new()
	blade.mesh=BoxMesh.new();blade.mesh.size=Vector3(0.04,0.55,0.012);blade.mesh.material=metal
	blade.position=Vector3(0,0.45,0);sw.add_child(blade)
	# Sword tip triangle (extends above blade top at y=0.45+0.275=0.725)
	var top:=0.45+0.275
	st=SurfaceTool.new();st.begin(Mesh.PRIMITIVE_TRIANGLES);st.set_material(metal)
	st.add_vertex(Vector3(0,top+0.10,0));st.add_vertex(Vector3(-0.02,top,0));st.add_vertex(Vector3(0.02,top,0))
	st.generate_normals()
	var tip:=MeshInstance3D.new();tip.mesh=st.commit();sw.add_child(tip)
	# Guard
	var guard:=MeshInstance3D.new()
	guard.mesh=BoxMesh.new();guard.mesh.size=Vector3(0.10,0.03,0.02);guard.mesh.material=gold
	guard.position=Vector3(0,0.18,0);sw.add_child(guard)
	# Handle
	var hdl:=MeshInstance3D.new()
	hdl.mesh=CylinderMesh.new();hdl.mesh.height=0.12;hdl.mesh.top_radius=0.02;hdl.mesh.bottom_radius=0.02;hdl.mesh.material=_mat(Color(0.22,0.10,0.04))
	hdl.position=Vector3(0,0.07,0);hdl.rotation=Vector3(PI/2,0,0);sw.add_child(hdl)
	hero.add_child(sw)

	# Shield
	var sh:=MeshInstance3D.new()
	sh.mesh=BoxMesh.new();sh.mesh.size=Vector3(0.04,0.40,0.28);sh.mesh.material=shield_c
	sh.position=Vector3(-0.42,0.60,0);hero.add_child(sh)
	# Gold cross emblem
	var ev:=MeshInstance3D.new()
	ev.mesh=BoxMesh.new();ev.mesh.size=Vector3(0.04,0.22,0.003);ev.mesh.material=gold
	ev.position=Vector3(-0.42,0.60,0.142);hero.add_child(ev)
	var eh:=MeshInstance3D.new()
	eh.mesh=BoxMesh.new();eh.mesh.size=Vector3(0.003,0.08,0.18);eh.mesh.material=gold
	eh.position=Vector3(-0.42,0.60,0.142);hero.add_child(eh)

	add_child(hero)
