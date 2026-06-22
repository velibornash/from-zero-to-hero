extends Control

@onready var wood_label: Label = $TopBar/WoodCount
@onready var gold_label: Label = $TopBar/GoldCount
@onready var build_panel: Panel = $BuildPanel
@onready var interact_hint: Label = $InteractHint

func _ready():
	GameState.wood_changed.connect(_on_wood_changed)
	GameState.gold_changed.connect(_on_gold_changed)
	update_display()
	build_panel.visible = false
	interact_hint.text = ""

func _on_wood_changed(value: int):
	wood_label.text = str(value)

func _on_gold_changed(value: int):
	gold_label.text = str(value)

func update_display():
	wood_label.text = str(GameState.wood)
	gold_label.text = str(GameState.gold)

func show_interact_hint(text: String):
	interact_hint.text = text

func hide_interact_hint():
	interact_hint.text = ""

func _on_build_pressed(building_type: String):
	var info = GameState.buildings.get(building_type, {})
	if not GameState.can_afford(info.get("cost", {})):
		return

	var slots = get_tree().get_nodes_in_group("building_slots")

	for slot in slots:
		if slot.state == 0:
			var cost = info.get("cost", {})
			for res in cost:
				match res:
					"wood": GameState.spend_wood(cost[res])
					"gold": GameState.spend_gold(cost[res])
			slot.start_construction(building_type)
			build_panel.visible = false
			return

func _on_sawmill_pressed():
	_on_build_pressed("sawmill")

func _on_stand_pressed():
	_on_build_pressed("stand")

func _on_watchtower_pressed():
	_on_build_pressed("watchtower")

func _on_woodcutter_pressed():
	_on_build_pressed("auto_woodcutter")

func _on_seller_pressed():
	_on_build_pressed("auto_seller")
