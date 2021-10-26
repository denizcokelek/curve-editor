# curve-editor
Simple Bézier curve creator. Project also includes 1 example for the quadratic curve.
<br>
### Quadratic Bézier Curve
------------
<img src="https://user-images.githubusercontent.com/50044263/138821411-89ef37d3-526f-4531-a6e6-ee39e983550f.png" alt="quadratic bézier curve unity" width="300"> <img src="https://user-images.githubusercontent.com/50044263/138821394-009f7811-4a7c-4cad-adb3-e26e0bafd27f.png" alt="quadratic bézier curve unity" width="315">

### Cubic Bézier Curve
------------
Curve scripts were created but not visualized in the editor.

> Also needs a fourth point at  the BezierCurve.cs
```cs
public void Reset()
        {
            Points = new Vector3[]
            {
                new Vector3(1f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(3f, 0, 0),
				new Vector3(4f, 0, 0)
            };
        }
```