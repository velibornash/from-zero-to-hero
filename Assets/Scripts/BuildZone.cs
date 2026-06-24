using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public int cost = 15;
    public bool isBuilt;

    Material mat;
    Color baseColor = new Color(0.2f, 0.55f, 0.9f, 0.3f);
    Color highlightColor = new Color(0.3f, 0.8f, 1f, 0.6f);
    float pulseTime;

    void Start()
    {
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            mat.color = baseColor;
        }
    }

    void Update()
    {
        if (isBuilt) return;
        pulseTime += Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isBuilt) return;
        if (!other.CompareTag("Player")) return;
        HUDController.PushEvent("Press [E] to build (cost: " + cost + " gold)");
    }

    void OnTriggerStay(Collider other)
    {
        if (isBuilt) return;
        if (!other.CompareTag("Player")) return;

        if (mat != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(pulseTime * 4f);
            mat.color = Color.Lerp(baseColor, highlightColor, pulse);
        }

        if (HUDController.Gold < cost) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            HUDController.Gold -= cost;
            isBuilt = true;

            BuildHouse(transform.position);

            var rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;

            var label = GetComponentInChildren<TextMesh>();
            if (label != null) label.gameObject.SetActive(false);
        }
    }

    void BuildHouse(Vector3 pos)
    {
        var house = new GameObject("Built House");
        house.transform.position = pos;

        var woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.45f, 0.3f, 0.15f);

        var roofMat = new Material(Shader.Find("Standard"));
        roofMat.color = new Color(0.55f, 0.2f, 0.1f);

        var base_ = GameObject.CreatePrimitive(PrimitiveType.Cube);
        base_.name = "HouseBase";
        base_.transform.SetParent(house.transform);
        base_.transform.localPosition = new Vector3(0, 2f, 0);
        base_.transform.localScale = new Vector3(5f, 4f, 6f);
        base_.GetComponent<Renderer>().material = woodMat;

        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "HouseRoof";
        roof.transform.SetParent(house.transform);
        roof.transform.localPosition = new Vector3(0, 4.5f, 0);
        roof.transform.localScale = new Vector3(6f, 1.5f, 7f);
        roof.GetComponent<Renderer>().material = roofMat;

        var filters = house.GetComponentsInChildren<MeshFilter>();
        foreach (var f in filters)
        {
            if (f.sharedMesh == null) continue;
            var mc = f.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = f.sharedMesh;
            mc.convex = false;
        }

        float topY = 3f;
        var rends = house.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            var b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            topY = b.max.y + 1f;
        }
        var label = new GameObject("BuildingLabel");
        label.transform.SetParent(house.transform);
        label.transform.position = new Vector3(house.transform.position.x, topY, house.transform.position.z);
        var tm = label.AddComponent<TextMesh>();
        tm.text = "Built House";
        tm.fontSize = 36;
        tm.fontStyle = FontStyle.Bold;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = Color.black;
        label.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        label.AddComponent<Billboard>();

        HUDController.PushEvent("Built a house!");
    }
}
