using UnityEngine;

namespace FromZeroToHero.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int gridWidth = 12;
        [SerializeField] private int gridHeight = 12;
        [SerializeField] private float cellSize = 3.2f;

        private readonly GameState state = new();
        private readonly StoryEvents storyEvents = new();

        private Vector2Int heroTile = new(5, 5);
        private Vector2Int targetTile = new(5, 5);
        private Vector2Int towerTile = new(8, 6);
        private Vector2Int workerTile = new(3, 7);

        private HUDController hud;
        private SelectionCursor cursor;
        private MovingActor hero;

        private GameObject towerSite;
        private GameObject workerSite;
        private string eventFeed = "Selo se budi.";
        private bool towerBuilt;
        private bool workerUnlocked;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            BuildEnvironment();
            BuildWorld();
            BuildHero();
            BuildHUD();
        }

        private void Update()
        {
            HandleMovementInput();
            HandleInteractionInput();
            UpdateHeroMotion();
            UpdateStory();
            UpdateHUD();
        }

        private void BuildEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.58f, 0.67f, 0.74f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;

            var sunlight = new GameObject("Sun Light", typeof(Light));
            var sun = sunlight.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.97f, 0.92f);
            sun.intensity = 1.5f;
            sunlight.transform.rotation = Quaternion.Euler(52f, -32f, 0f);

            var fill = new GameObject("Fill Light", typeof(Light));
            var fillLight = fill.GetComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.54f, 0.72f, 1f);
            fillLight.intensity = 0.34f;
            fill.transform.rotation = Quaternion.Euler(14f, 132f, 0f);

            var camObject = new GameObject("Main Camera", typeof(Camera));
            var camera = camObject.GetComponent<Camera>();
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 220f;
            camObject.tag = "MainCamera";
            camObject.transform.position = new Vector3(11.5f, 14.6f, -12.6f);
            camObject.transform.LookAt(new Vector3(11f, 0.4f, 10.4f));

            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "River";
            water.transform.localScale = new Vector3(1.1f, 1f, 0.72f);
            water.transform.position = new Vector3(-6.8f, -0.01f, 5.6f);
            water.GetComponent<Renderer>().material = CreateMaterial(new Color(0.12f, 0.31f, 0.48f), 0.12f);
        }

        private void BuildWorld()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3.1f, 1f, 3.1f);
            ground.transform.position = new Vector3(8f, 0f, 8f);
            ground.GetComponent<Renderer>().material = CreateMaterial(new Color(0.19f, 0.32f, 0.16f), 0.55f);

            SpawnProp("FreeAssets/Nature/Pine_1", new Vector3(0.4f, 0f, 11.0f), Quaternion.Euler(0f, 35f, 0f), new Vector3(1.05f, 1.05f, 1.05f));
            SpawnProp("FreeAssets/Nature/Pine_2", new Vector3(2.8f, 0f, 11.6f), Quaternion.Euler(0f, 115f, 0f), new Vector3(1.0f, 1.0f, 1.0f));
            SpawnProp("FreeAssets/Nature/CommonTree_1", new Vector3(11.3f, 0f, 1.8f), Quaternion.Euler(0f, 200f, 0f), new Vector3(1.0f, 1.0f, 1.0f));
            SpawnProp("FreeAssets/Nature/CommonTree_2", new Vector3(13.6f, 0f, 10.0f), Quaternion.Euler(0f, 75f, 0f), new Vector3(1.0f, 1.0f, 1.0f));
            SpawnProp("FreeAssets/Nature/Bush_Common", new Vector3(4.1f, 0f, 9.9f), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.95f, 0.95f, 0.95f));
            SpawnProp("FreeAssets/Nature/Fern_1", new Vector3(12.0f, 0f, 7.8f), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.9f, 0.9f, 0.9f));
            SpawnProp("FreeAssets/Nature/Rock_Medium_1", new Vector3(1.7f, 0f, 3.1f), Quaternion.Euler(0f, 45f, 0f), new Vector3(1.0f, 1.0f, 1.0f));
            SpawnProp("FreeAssets/Nature/Rock_Medium_2", new Vector3(14.1f, 0f, 3.4f), Quaternion.Euler(0f, 15f, 0f), new Vector3(1.0f, 1.0f, 1.0f));

            SpawnProp("FreeAssets/Village/building_home_A_green", new Vector3(5.0f, 0f, 6.1f), Quaternion.Euler(0f, 20f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
            SpawnProp("FreeAssets/Village/building_tavern_green", new Vector3(7.1f, 0f, 7.8f), Quaternion.Euler(0f, 130f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
            SpawnProp("FreeAssets/Village/building_lumbermill_green", new Vector3(3.4f, 0f, 5.3f), Quaternion.Euler(0f, 200f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
            SpawnProp("FreeAssets/Village/building_church_green", new Vector3(10.2f, 0f, 4.6f), Quaternion.Euler(0f, 240f, 0f), new Vector3(0.78f, 0.78f, 0.78f));
            SpawnProp("FreeAssets/Village/building_market_green", new Vector3(8.9f, 0f, 9.8f), Quaternion.Euler(0f, 35f, 0f), new Vector3(0.76f, 0.76f, 0.76f));
            SpawnProp("FreeAssets/Village/building_watermill_green", new Vector3(9.4f, 0f, 3.1f), Quaternion.Euler(0f, 215f, 0f), new Vector3(0.78f, 0.78f, 0.78f));
            SpawnProp("FreeAssets/Village/building_well_green", new Vector3(6.3f, 0f, 4.1f), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.72f, 0.72f, 0.72f));
            SpawnProp("FreeAssets/Village/building_barracks_green", new Vector3(12.0f, 0f, 6.4f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.8f, 0.8f, 0.8f));
            SpawnProp("FreeAssets/Village/building_blacksmith_green", new Vector3(2.5f, 0f, 8.0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0.76f, 0.76f, 0.76f));

            towerSite = SpawnProp("FreeAssets/Village/building_scaffolding", GridToWorld(towerTile), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.82f, 0.82f, 0.82f));
            workerSite = SpawnProp("FreeAssets/Village/building_stage_A", GridToWorld(workerTile), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.76f, 0.76f, 0.76f));

            var cursorObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cursorObject.name = "SelectionCursor";
            cursorObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Collider cursorCollider = cursorObject.GetComponent<Collider>();
            if (cursorCollider != null)
            {
                cursorCollider.enabled = false;
            }
            cursorObject.GetComponent<Renderer>().material = CreateCursorMaterial();
            cursor = cursorObject.AddComponent<SelectionCursor>();
            cursor.Place(GridToWorld(targetTile), cellSize * 0.72f);
        }

        private void BuildHero()
        {
            hero = SpawnActor(heroTile, 0.95f, "Hero", "FreeAssets/Hero/Superhero_Male_FullBody");
            if (hero != null)
            {
                hero.MoveTo(GridToWorld(heroTile));
            }
        }

        private void BuildHUD()
        {
            hud = new GameObject("HUDController").AddComponent<HUDController>();
            hud.Build();
        }

        private void HandleMovementInput()
        {
            Vector2Int delta = Vector2Int.zero;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) delta = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) delta = Vector2Int.right;
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) delta = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) delta = Vector2Int.down;

            if (delta != Vector2Int.zero)
            {
                targetTile = ClampTile(targetTile + delta);
                cursor.Place(GridToWorld(targetTile), cellSize * 0.72f);
                if (hero != null)
                {
                    hero.MoveTo(GridToWorld(targetTile));
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (!Input.GetKeyDown(KeyCode.E))
                return;

            if (heroTile == towerTile && !towerBuilt)
            {
                if (state.CanAfford(8, 2, 5))
                {
                    state.Spend(8, 2, 5);
                    if (towerSite != null)
                    {
                        Destroy(towerSite);
                    }
                    towerSite = SpawnProp("FreeAssets/Village/building_tower_A_green", GridToWorld(towerTile), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.82f, 0.82f, 0.82f));
                    towerBuilt = true;
                    eventFeed = "Toranj je izgrađen. Odbrana raste.";
                }

                return;
            }

            if (heroTile == workerTile && !workerUnlocked)
            {
                if (state.CanAfford(4, 4, 2))
                {
                    state.Spend(4, 4, 2);
                    if (workerSite != null)
                    {
                        Destroy(workerSite);
                    }
                    workerSite = SpawnProp("FreeAssets/Village/building_home_B_green", GridToWorld(workerTile), Quaternion.Euler(0f, 0f, 0f), new Vector3(0.78f, 0.78f, 0.78f));
                    workerUnlocked = true;
                    state.Workers += 1;
                    eventFeed = "Novi automatski radnik je regrutovan.";
                }
            }
        }

        private void UpdateHeroMotion()
        {
            if (hero == null)
                return;

            Vector3 heroPos = hero.transform.position;
            Vector2Int newHeroTile = WorldToGrid(heroPos);
            if (newHeroTile != heroTile)
                heroTile = newHeroTile;
        }

        private void UpdateStory()
        {
            if (storyEvents.TryAdvance(state, (title, description) => eventFeed = $"{title}: {description}"))
            {
                if (state.FirstFire)
                {
                    state.Wood += 3;
                }

                if (state.Wolves)
                {
                    state.AddGold(2);
                }
            }

            state.BaseHp = Mathf.Max(0, state.BaseHp);
            if (state.BaseHp <= 0)
            {
                eventFeed = "Baza je pala. Reload za novi run.";
            }
        }

        private void UpdateHUD()
        {
            if (hud == null)
                return;

            string objective = towerBuilt
                ? "Proširi naselje, podigni ekonomiju i pripremi sledeću eru."
                : "Idi do gradilišta i aktiviraj toranj ili novi radnički objekat.";

            hud.UpdateValues(state, objective, eventFeed);
        }

        private MovingActor SpawnActor(Vector2Int tile, float scale, string name, params string[] resourcePaths)
        {
            GameObject prefab = null;
            foreach (string resourcePath in resourcePaths)
            {
                prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab != null)
                    break;
            }

            GameObject root = new GameObject(name);
            root.transform.position = GridToWorld(tile);
            MovingActor actor = root.AddComponent<MovingActor>();

            if (prefab != null)
            {
                GameObject model = Instantiate(prefab, root.transform);
                model.transform.localScale = Vector3.one * scale;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                actor.SetVisual(model.transform);
            }
            else
            {
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                fallback.transform.SetParent(root.transform, false);
                fallback.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
                actor.SetVisual(fallback.transform);
            }

            actor.MoveTo(GridToWorld(tile));
            return actor;
        }

        private GameObject SpawnProp(string resourcePath, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            GameObject instance = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;
            return instance;
        }

        private Vector3 GridToWorld(Vector2Int tile)
        {
            float x = tile.x * cellSize;
            float z = tile.y * cellSize;
            return new Vector3(x, 0f, z);
        }

        private Vector2Int WorldToGrid(Vector3 position)
        {
            int x = Mathf.RoundToInt(position.x / cellSize);
            int y = Mathf.RoundToInt(position.z / cellSize);
            return ClampTile(new Vector2Int(x, y));
        }

        private Vector2Int ClampTile(Vector2Int tile)
        {
            int x = Mathf.Clamp(tile.x, 0, gridWidth - 1);
            int y = Mathf.Clamp(tile.y, 0, gridHeight - 1);
            return new Vector2Int(x, y);
        }

        private static Material CreateMaterial(Color color, float smoothness)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private static Material CreateCursorMaterial()
        {
            var material = new Material(Shader.Find("Unlit/Color"));
            material.color = new Color(0.29f, 0.76f, 1f, 0.30f);
            return material;
        }
    }
}
