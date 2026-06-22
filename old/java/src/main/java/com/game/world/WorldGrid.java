package com.game.world;

import com.game.buildings.Building;
import com.game.buildings.BuildingType;
import com.game.entities.Enemy;
import com.game.entities.Entity;
import com.game.entities.ResourceNode;
import com.game.entities.Worker;
import com.game.resources.ResourceType;

import java.util.*;

public class WorldGrid {
    public static final float TILE_SIZE = 32f;
    public static final int GRID_SIZE = 50;

    private final Tile[][] tiles;
    private final int width;
    private final int height;
    private final List<Worker> workers;
    private final List<Enemy> enemies;
    private final List<Building> buildings;
    private final List<ResourceNode> resourceNodes;
    private final boolean[][] buildingSlots;

    public WorldGrid(int width, int height) {
        this.width = width;
        this.height = height;
        this.tiles = new Tile[width][height];
        this.workers = new ArrayList<>();
        this.enemies = new ArrayList<>();
        this.buildings = new ArrayList<>();
        this.resourceNodes = new ArrayList<>();
        this.buildingSlots = new boolean[width][height];
        initTerrain();
        initBuildingSlots();
    }

    private void initTerrain() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                TerrainType terrain = TerrainType.GRASS;
                if (x < 3 || x >= width - 3 || y < 3 || y >= height - 3) {
                    terrain = TerrainType.FOREST;
                } else if (x == 25 && y == 25) {
                    terrain = TerrainType.DIRT;
                }
                tiles[x][y] = new Tile(x, y, terrain);
            }
        }
    }

    private void initBuildingSlots() {
        int cx = 25, cy = 25;
        int[][] offsets = {
            {0, 0}, {0, 1}, {1, 0}, {0, -1}, {-1, 0},
            {1, 1}, {-1, -1}, {1, -1}, {-1, 1},
            {0, 2}, {2, 0}, {0, -2}, {-2, 0}
        };
        for (int[] o : offsets) {
            int sx = cx + o[0];
            int sy = cy + o[1];
            if (sx >= 0 && sx < width && sy >= 0 && sy < height) {
                buildingSlots[sx][sy] = true;
            }
        }
    }

    public boolean isBuildingSlot(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return buildingSlots[x][y] && !getTile(x, y).hasBuilding();
    }

    public Tile getTile(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return tiles[x][y];
    }

    public Tile getTileAtWorld(float worldX, float worldY) {
        int tx = (int)(worldX / TILE_SIZE);
        int ty = (int)(worldY / TILE_SIZE);
        return getTile(tx, ty);
    }

    public int getWidth() { return width; }
    public int getHeight() { return height; }

    public List<Worker> getWorkers() { return workers; }
    public List<Enemy> getEnemies() { return enemies; }
    public List<Building> getBuildings() { return buildings; }
    public List<ResourceNode> getResourceNodes() { return resourceNodes; }

    public void addWorker(Worker worker) {
        workers.add(worker);
        updateOccupant(worker);
    }

    public void removeWorker(Worker worker) {
        workers.remove(worker);
        clearOccupant(worker);
    }

    public void addEnemy(Enemy enemy) {
        enemies.add(enemy);
        updateOccupant(enemy);
    }

    public void removeEnemy(Enemy enemy) {
        enemies.remove(enemy);
        clearOccupant(enemy);
    }

    public void addBuilding(Building building) {
        buildings.add(building);
        Tile tile = getTile(building.getTileX(), building.getTileY());
        if (tile != null) {
            tile.setBuilding(building);
        }
    }

    public void addResourceNode(ResourceNode node) {
        resourceNodes.add(node);
        Tile tile = getTileAtWorld(node.getX(), node.getY());
        if (tile != null) {
            tile.setResourceNode(node);
        }
    }

    public void removeResourceNode(ResourceNode node) {
        resourceNodes.remove(node);
        Tile tile = getTileAtWorld(node.getX(), node.getY());
        if (tile != null) {
            tile.setResourceNode(null);
        }
    }

    private void updateOccupant(Entity entity) {
        Tile tile = getTileAtWorld(entity.getX(), entity.getY());
        if (tile != null) {
            tile.setOccupant(entity);
        }
    }

    private void clearOccupant(Entity entity) {
        Tile tile = getTileAtWorld(entity.getX(), entity.getY());
        if (tile != null && tile.getOccupant() == entity) {
            tile.setOccupant(null);
        }
    }

    public void placeInitialResources() {
        Random rand = new Random(42);
        for (int i = 0; i < 15; i++) {
            int x = 5 + rand.nextInt(40);
            int y = 5 + rand.nextInt(40);
            Tile tile = getTile(x, y);
            if (tile != null && tile.isWalkable() && !tile.hasResourceNode()) {
                ResourceType type = rand.nextBoolean() ? ResourceType.WOOD : ResourceType.FOOD;
                ResourceNode node = new ResourceNode(x * TILE_SIZE, y * TILE_SIZE, type, 50 + rand.nextFloat() * 50);
                addResourceNode(node);
            }
        }
        for (int i = 0; i < 5; i++) {
            int x = 10 + rand.nextInt(30);
            int y = 10 + rand.nextInt(30);
            Tile tile = getTile(x, y);
            if (tile != null && tile.isWalkable() && !tile.hasResourceNode()) {
                ResourceNode node = new ResourceNode(x * TILE_SIZE, y * TILE_SIZE, ResourceType.GOLD, 30 + rand.nextFloat() * 30);
                addResourceNode(node);
            }
        }
    }

    public void placeInitialBuilding(String typeId, int tileX, int tileY) {
        BuildingType type = switch (typeId) {
            case "pekara" -> new BuildingType("pekara", "Pekara Kirćanski", "Proizvodi hranu", 3, Map.of(ResourceType.WOOD, 20f), Map.of(ResourceType.FOOD, 5f), 50f);
            case "kafana" -> new BuildingType("kafana", "Kafana Čardak", "Daje moral", 2, Map.of(ResourceType.WOOD, 15f), Map.of(), 40f);
            case "kasarna" -> new BuildingType("kasarna", "Kasarna Topčider", "Trenira vojnike", 4, Map.of(ResourceType.WOOD, 30f, ResourceType.GOLD, 10f), Map.of(), 80f);
            default -> throw new IllegalArgumentException("Unknown building: " + typeId);
        };
        Building building = new Building(type, tileX, tileY);
        addBuilding(building);
    }

    public void addEnemyWave(String waveType, int count) {
        Random rand = new Random();
        for (int i = 0; i < count; i++) {
            int x, y;
            if (rand.nextBoolean()) {
                x = rand.nextBoolean() ? 0 : width - 1;
                y = rand.nextInt(height);
            } else {
                x = rand.nextInt(width);
                y = rand.nextBoolean() ? 0 : height - 1;
            }
            float hp = switch (waveType) {
                case "wolves" -> 15f;
                case "soldiers" -> 30f;
                case "boss" -> 100f;
                default -> 20f;
            };
            int dmg = switch (waveType) {
                case "wolves" -> 5;
                case "soldiers" -> 10;
                case "boss" -> 25;
                default -> 8;
            };
            Enemy enemy = new Enemy(x * TILE_SIZE, y * TILE_SIZE, hp, dmg);
            float cx = 25 * TILE_SIZE;
            float cy = 25 * TILE_SIZE;
            float angle = (float) (2 * Math.PI * i / count);
            enemy.setTargetPosition(cx + (float)Math.cos(angle) * 50, cy + (float)Math.sin(angle) * 50);
            addEnemy(enemy);
        }
    }

    public void clearDeadEntities() {
        workers.removeIf(w -> !w.isAlive());
        enemies.removeIf(e -> !e.isAlive());
        resourceNodes.removeIf(n -> !n.isAlive());
    }
}
