extends Node3D

const WALK_SPEED := 5.2
const DODGE_SPEED := 11.0
const ATTACK_RANGE := 3.2
const SKILL_COOLDOWN := 4.0

var player: CharacterBody3D
var enemy: CharacterBody3D
var camera_pivot: Node3D
var camera: Camera3D
var joystick_vector := Vector2.ZERO
var joystick_touch := -1
var joystick_origin := Vector2.ZERO
var dodge_time := 0.0
var skill_cooldown := 0.0
var attack_flash := 0.0
var hp := 100
var enemy_hp := 100
var status_label: Label
var hp_label: Label
var skill_button: Button
var joystick_base: Control
var joystick_knob: ColorRect

func _ready() -> void:
	_build_world()
	_build_player()
	_build_enemy()
	_build_hud()
	_update_hud()

func _physics_process(delta: float) -> void:
	if skill_cooldown > 0.0:
		skill_cooldown = maxf(0.0, skill_cooldown - delta)
	if dodge_time > 0.0:
		dodge_time = maxf(0.0, dodge_time - delta)
	var input_dir := joystick_vector
	input_dir.y -= Input.get_action_strength("move_forward")
	input_dir.y += Input.get_action_strength("move_back")
	input_dir.x -= Input.get_action_strength("move_left")
	input_dir.x += Input.get_action_strength("move_right")
	if input_dir.length() > 1.0:
		input_dir = input_dir.normalized()
	var forward := -camera_pivot.global_basis.z
	forward.y = 0
	forward = forward.normalized()
	var right := camera_pivot.global_basis.x
	right.y = 0
	right = right.normalized()
	var move_dir := (right * input_dir.x + forward * -input_dir.y).normalized()
	if dodge_time > 0.0:
		move_dir = -player.global_basis.z
		player.velocity.x = move_dir.x * DODGE_SPEED
		player.velocity.z = move_dir.z * DODGE_SPEED
	else:
		player.velocity.x = move_dir.x * WALK_SPEED
		player.velocity.z = move_dir.z * WALK_SPEED
	if not player.is_on_floor():
		player.velocity.y -= 24.0 * delta
	else:
		player.velocity.y = -0.1
	if move_dir.length() > 0.08 and dodge_time <= 0.0:
		player.look_at(player.global_position + move_dir, Vector3.UP)
	player.move_and_slide()
	camera_pivot.global_position = player.global_position + Vector3(0, 1.35, 0)
	if enemy and is_instance_valid(enemy) and enemy_hp > 0:
		enemy.look_at(Vector3(player.global_position.x, enemy.global_position.y, player.global_position.z), Vector3.UP)
	_update_hud()

func _build_world() -> void:
	var env := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_SKY
	environment.sky = Sky.new()
	environment.sky.sky_material = ProceduralSkyMaterial.new()
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.42, 0.48, 0.62)
	environment.ambient_light_energy = 0.65
	environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.environment = environment
	add_child(env)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-48, -32, 0)
	sun.light_energy = 1.45
	sun.shadow_enabled = true
	add_child(sun)
	_add_box(self, "Ancient Arena", Vector3(0, -0.55, 0), Vector3(70, 1, 70), Color(0.19, 0.23, 0.22))
	for i in range(28):
		var angle := float(i) * TAU / 28.0
		var radius := 12.0 + float(i % 4) * 1.2
		var h := 2.5 + float(i % 5) * 0.7
		_add_box(self, "Broken Ruin", Vector3(cos(angle) * radius, h * 0.5 - 0.2, sin(angle) * radius), Vector3(1.1, h, 1.0), Color(0.27, 0.30, 0.31))
	for i in range(12):
		var x := float((i * 7) % 21) - 10.0
		var z := float((i * 11) % 23) - 11.0
		_add_box(self, "Mossy Stone", Vector3(x, -0.02, z), Vector3(0.7, 0.16, 0.7), Color(0.29, 0.34, 0.29))
	var fog_light := OmniLight3D.new()
	fog_light.position = Vector3(0, 4, -7)
	fog_light.light_color = Color(0.18, 0.48, 1.0)
	fog_light.light_energy = 2.0
	fog_light.omni_range = 9.0
	add_child(fog_light)
	_add_box(self, "Obelisk", Vector3(0, 2.2, -8), Vector3(1.0, 4.4, 1.0), Color(0.11, 0.20, 0.30))

func _build_player() -> void:
	player = CharacterBody3D.new()
	player.name = "Player"
	player.position = Vector3(0, 1.2, 4)
	add_child(player)
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.42
	capsule.height = 1.8
	var body := MeshInstance3D.new()
	body.mesh = capsule
	body.position.y = 0.9
	body.material_override = _mat(Color(0.17, 0.29, 0.48), 0.25, 0.45)
	player.add_child(body)
	var head := MeshInstance3D.new()
	var head_mesh := SphereMesh.new()
	head_mesh.radius = 0.27
	head_mesh.height = 0.54
	head.mesh = head_mesh
	head.position = Vector3(0, 1.92, 0)
	head.material_override = _mat(Color(0.79, 0.63, 0.48))
	player.add_child(head)
	var weapon := MeshInstance3D.new()
	var blade := BoxMesh.new()
	blade.size = Vector3(0.09, 0.95, 0.13)
	weapon.mesh = blade
	weapon.position = Vector3(0.48, 1.0, -0.35)
	weapon.rotation_degrees.x = -18
	weapon.material_override = _mat(Color(0.60, 0.83, 1.0), 0.75, 0.2)
	player.add_child(weapon)
	var collider := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.42
	shape.height = 1.8
	collider.shape = shape
	collider.position.y = 0.9
	player.add_child(collider)
	camera_pivot = Node3D.new()
	camera_pivot.position = player.position + Vector3(0, 1.35, 0)
	add_child(camera_pivot)
	camera = Camera3D.new()
	camera.position = Vector3(0, 3.2, 7.0)
	camera.rotation_degrees.x = -15
	camera.current = true
	camera_pivot.add_child(camera)
	player.floor_snap_length = 0.3

func _build_enemy() -> void:
	enemy = CharacterBody3D.new()
	enemy.name = "TrainingWraith"
	enemy.position = Vector3(0, 0.95, -3)
	add_child(enemy)
	var mesh_instance := MeshInstance3D.new()
	var mesh := CapsuleMesh.new()
	mesh.radius = 0.58
	mesh.height = 1.9
	mesh_instance.mesh = mesh
	mesh_instance.position.y = 0.95
	mesh_instance.material_override = _mat(Color(0.42, 0.13, 0.20), 0.1, 0.3)
	enemy.add_child(mesh_instance)
	var eye := MeshInstance3D.new()
	var eye_mesh := SphereMesh.new()
	eye_mesh.radius = 0.13
	eye_mesh.height = 0.26
	eye.mesh = eye_mesh
	eye.position = Vector3(0, 1.35, -0.49)
	eye.material_override = _mat(Color(1.0, 0.12, 0.04), 0.2, 0.2)
	enemy.add_child(eye)
	var collider := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.58
	shape.height = 1.9
	collider.shape = shape
	collider.position.y = 0.95
	enemy.add_child(collider)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var top := PanelContainer.new()
	top.position = Vector2(22, 18)
	top.custom_minimum_size = Vector2(300, 78)
	layer.add_child(top)
	var column := VBoxContainer.new()
	top.add_child(column)
	hp_label = Label.new()
	hp_label.add_theme_font_size_override("font_size", 19)
	column.add_child(hp_label)
	status_label = Label.new()
	status_label.add_theme_font_size_override("font_size", 15)
	status_label.text = "SHATTERED REACH  •  MANUAL COMBAT"
	column.add_child(status_label)
	joystick_base = Control.new()
	joystick_base.position = Vector2(38, 0)
	joystick_base.anchor_top = 1.0
	joystick_base.anchor_bottom = 1.0
	joystick_base.offset_top = -205
	joystick_base.offset_bottom = -35
	joystick_base.custom_minimum_size = Vector2(170, 170)
	joystick_base.mouse_filter = Control.MOUSE_FILTER_STOP
	layer.add_child(joystick_base)
	var ring := ColorRect.new()
	ring.color = Color(0.15, 0.20, 0.28, 0.55)
	ring.size = Vector2(150, 150)
	ring.position = Vector2(10, 10)
	joystick_base.add_child(ring)
	joystick_knob = ColorRect.new()
	joystick_knob.color = Color(0.55, 0.72, 0.92, 0.85)
	joystick_knob.size = Vector2(64, 64)
	joystick_knob.position = Vector2(53, 53)
	joystick_base.add_child(joystick_knob)
	var attack := _make_button(layer, "ATTACK", Vector2(-180, -105), Vector2(142, 70))
	attack.pressed.connect(_manual_attack)
	var skill := _make_button(layer, "SKILL", Vector2(-290, -190), Vector2(112, 62))
	skill.pressed.connect(_use_skill)
	skill_button = skill
	var dodge := _make_button(layer, "DODGE", Vector2(-320, -95), Vector2(112, 62))
	dodge.pressed.connect(_dodge)
	var block := _make_button(layer, "BLOCK", Vector2(-180, -190), Vector2(112, 62))
	block.pressed.connect(_manual_block)
	var enemy_label := Label.new()
	enemy_label.text = "TARGET: WRAITH"
	enemy_label.position = Vector2(-250, 22)
	enemy_label.anchor_left = 1.0
	enemy_label.anchor_right = 1.0
	enemy_label.add_theme_font_size_override("font_size", 17)
	layer.add_child(enemy_label)

func _make_button(parent: Control, text_value: String, offset: Vector2, button_size: Vector2) -> Button:
	var button := Button.new()
	button.text = text_value
	button.size = button_size
	button.position = offset
	button.anchor_left = 1.0
	button.anchor_right = 1.0
	button.anchor_top = 1.0
	button.anchor_bottom = 1.0
	button.add_theme_font_size_override("font_size", 17)
	button.add_theme_color_override("font_color", Color(0.93, 0.96, 1.0))
	button.add_theme_stylebox_override("normal", _button_style(Color(0.10, 0.17, 0.28, 0.94)))
	button.add_theme_stylebox_override("pressed", _button_style(Color(0.18, 0.42, 0.68, 1.0)))
	parent.add_child(button)
	return button

func _button_style(color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.border_color = Color(0.40, 0.65, 0.92, 0.9)
	style.set_border_width_all(2)
	style.set_corner_radius_all(12)
	style.content_margin_left = 8
	style.content_margin_right = 8
	style.content_margin_top = 6
	style.content_margin_bottom = 6
	return style

func _mat(color: Color, metallic: float = 0.0, roughness: float = 0.7) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.metallic = metallic
	material.roughness = roughness
	return material

func _add_box(parent: Node3D, object_name: String, pos: Vector3, box_size: Vector3, color: Color) -> void:
	var instance := MeshInstance3D.new()
	instance.name = object_name
	var mesh := BoxMesh.new()
	mesh.size = box_size
	instance.mesh = mesh
	instance.position = pos
	instance.material_override = _mat(color)
	parent.add_child(instance)
	var static_body := StaticBody3D.new()
	static_body.position = pos
	parent.add_child(static_body)
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = box_size
	collision.shape = shape
	static_body.add_child(collision)

func _manual_attack() -> void:
	if enemy_hp <= 0:
		_status("WRAITH DEFEATED — MANUAL ATTACK")
		return
	if player.global_position.distance_to(enemy.global_position) > ATTACK_RANGE:
		_status("TOO FAR — MOVE CLOSER")
		return
	enemy_hp = maxi(0, enemy_hp - 24)
	_status("SWORD STRIKE  •  -24 HP")
	_hit_flash()
	if enemy_hp == 0:
		enemy.visible = false
		_status("WRAITH DEFEATED — NO AUTO-ATTACKS")

func _use_skill() -> void:
	if skill_cooldown > 0.0:
		_status("SKILL COOLDOWN: %.1f S" % skill_cooldown)
		return
	if enemy_hp <= 0:
		_status("NO ACTIVE TARGET")
		return
	if player.global_position.distance_to(enemy.global_position) > 5.5:
		_status("SKILL OUT OF RANGE")
		return
	enemy_hp = maxi(0, enemy_hp - 42)
	skill_cooldown = SKILL_COOLDOWN
	_status("ARCANE SLASH  •  -42 HP")
	_hit_flash()
	if enemy_hp == 0:
		enemy.visible = false
		_status("WRAITH DEFEATED")

func _dodge() -> void:
	dodge_time = 0.24
	_status("DODGE")

func _manual_block() -> void:
	_status("BLOCK — MANUAL DEFENSE")

func _hit_flash() -> void:
	if enemy and is_instance_valid(enemy) and enemy_hp > 0:
		var mesh := enemy.get_child(0) as MeshInstance3D
		if mesh:
			mesh.scale = Vector3(1.12, 0.92, 1.12)
			var tween := create_tween()
			tween.tween_property(mesh, "scale", Vector3.ONE, 0.16)

func _status(message: String) -> void:
	if status_label:
		status_label.text = message

func _update_hud() -> void:
	if hp_label:
		hp_label.text = "HP  %d/100     TARGET  %d/100" % [hp, enemy_hp]
	if skill_button:
		skill_button.text = "SKILL" if skill_cooldown <= 0.0 else "SKILL %.1f" % skill_cooldown

func _input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		var local := joystick_base.get_global_rect()
		if event.pressed and local.has_point(event.position):
			joystick_touch = event.index
			joystick_origin = local.position + local.size * 0.5
			_update_joystick(event.position)
		elif not event.pressed and event.index == joystick_touch:
			joystick_touch = -1
			joystick_vector = Vector2.ZERO
			joystick_knob.position = Vector2(53, 53)
	elif event is InputEventScreenDrag and event.index == joystick_touch:
		_update_joystick(event.position)

func _update_joystick(screen_pos: Vector2) -> void:
	var delta := screen_pos - joystick_origin
	if delta.length() > 52.0:
		delta = delta.normalized() * 52.0
	joystick_vector = Vector2(delta.x / 52.0, delta.y / 52.0)
	joystick_knob.position = Vector2(75, 75) + delta - Vector2(32, 32)
