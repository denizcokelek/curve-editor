using UnityEngine;

public class SplineNaivePlacer : MonoBehaviour
{
    public Spline Spline;
    public GameObject Prefab;
    public int Count = 10;

    private void Start()
    {
        if (Spline == null || Prefab == null || Count <= 0)
            return;

        for (int i = 0; i <= Count; i++)
        {
            float t = i / (float)Count;
            Vector3 position = Spline.Evaluate(t);
            Instantiate(Prefab, position, Quaternion.identity, transform);
        }
    }
}