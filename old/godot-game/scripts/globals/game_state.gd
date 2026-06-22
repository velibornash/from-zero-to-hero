extends Node

signal gold_changed(value)
signal wood_changed(value)
signal building_unlocked(type)

var gold: int = 0
var wood: int = 0

var buildings = {
	"sawmill": {
		"name": "Sawmill",
		"cost": {"wood": 5},
		"build_time": 8.0,
		"produces": "gold",
		"production_rate": 1,
		"production_interval": 5.0,
		"description": "Turns wood into gold"
	},
	"stand": {
		"name": "Market Stand",
		"cost": {"gold": 10},
		"build_time": 6.0,
		"produces": "wood",
		"production_rate": 1,
		"production_interval": 4.0,
		"description": "Produces wood over time"
	},
	"watchtower": {
		"name": "Watchtower",
		"cost": {"wood": 8, "gold": 5},
		"build_time": 12.0,
		"produces": "none",
		"production_rate": 0,
		"production_interval": 0.0,
		"description": "Defensive structure"
	},
	"auto_woodcutter": {
		"name": "Auto-Woodcutter",
		"cost": {"wood": 12, "gold": 8},
		"build_time": 15.0,
		"produces": "wood",
		"production_rate": 2,
		"production_interval": 6.0,
		"description": "Automatically gathers wood"
	},
	"auto_seller": {
		"name": "Auto-Seller",
		"cost": {"wood": 10, "gold": 15},
		"build_time": 18.0,
		"produces": "gold",
		"production_rate": 3,
		"production_interval": 8.0,
		"description": "Automatically sells resources"
	}
}

func add_gold(amount: int):
	gold += amount
	gold_changed.emit(gold)

func spend_gold(amount: int) -> bool:
	if gold >= amount:
		gold -= amount
		gold_changed.emit(gold)
		return true
	return false

func add_wood(amount: int):
	wood += amount
	wood_changed.emit(wood)

func spend_wood(amount: int) -> bool:
	if wood >= amount:
		wood -= amount
		wood_changed.emit(wood)
		return true
	return false

func can_afford(cost: Dictionary) -> bool:
	for resource in cost:
		var amount = cost[resource]
		match resource:
			"wood":
				if wood < amount:
					return false
			"gold":
				if gold < amount:
					return false
	return true
