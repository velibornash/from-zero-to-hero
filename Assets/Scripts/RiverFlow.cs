using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RiverFlow : MonoBehaviour
{
    public Vector2 flowDirection = new Vector2(0, 1);
    public float speed = 0.08f;
    public Vector2 tiling = Vector2.one;

    Material mat;
    Vector2 offset;
    static readonly int MainTex = Shader.PropertyToID("_MainTex");
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    static readonly int Tiling = Shader.PropertyToID("_BaseMap_ST");
    int activeId;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        if (mat.HasProperty(BaseMap)) activeId = BaseMap;
        else if (mat.HasProperty(MainTex)) activeId = MainTex;
        else { enabled = false; return; }
        mat.SetTextureScale(activeId, tiling);
    }

    void Update()
    {
        if (mat == null) return;
        offset += flowDirection.normalized * speed * Time.deltaTime;
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);
        mat.SetTextureOffset(activeId, offset);
    }
}
