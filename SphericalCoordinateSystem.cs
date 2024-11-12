using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SphericalCoordinateSystem : MonoBehaviour
{
    public GameObject pointPrefab; // Prefab for the moving point
    public GameObject transparentSphere; // Transparent sphere directly from the hierarchy
    public Slider radiusSlider;
    public Slider thetaSlider;
    public Slider phiSlider;
    public Slider transparencySlider; // Slider for controlling transparency
    public TextMeshProUGUI rText;
    public TextMeshProUGUI thetaText;
    public TextMeshProUGUI phiText;
    public TextMeshProUGUI xText;
    public TextMeshProUGUI yText;
    public TextMeshProUGUI zText;
    public TextMeshProUGUI cylindricalRText;
    public TextMeshProUGUI cylindricalThetaText;
    public TextMeshProUGUI cylindricalZText;

    public Button startButton; // Reference to the start button
    public Button clearButton; // Reference to the clear button

    private GameObject pointInstance;
    private GameObject xAxis, yAxis, zAxis;
    private LineRenderer lineToZAxis;
    private LineRenderer lineToShadow;
    private LineRenderer lineToOrigin;
    private LineRenderer lineToPoint;
    private GameObject coneObject1, coneObject2;
    private Material coneMaterial1, coneMaterial2;
    private Mesh coneMesh1, coneMesh2;
    private Vector3 shadowPoint;

    private bool isStarted = false; // Check if the system has started

    void Start()
    {
        // Set the start button listener to trigger the StartVisualization method
        startButton.onClick.AddListener(StartVisualization);
        // Set the clear button listener to trigger the ClearVisualization method
        clearButton.onClick.AddListener(ClearVisualization);

        // Hide or deactivate initial components until the "Start" button is pressed
        pointInstance = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
        pointInstance.SetActive(false); // Disable pointInstance initially

        // Assuming transparentSphere is already in the scene and assigned through the inspector
        transparentSphere.SetActive(false); // Initially deactivate it

        transparencySlider.onValueChanged.AddListener(delegate { UpdateSphereTransparency(); });
    }

    // This method will be called when the start button is pressed
    void StartVisualization()
    {
        isStarted = true; // System is now started
        pointInstance.SetActive(true); // Activate the point
        transparentSphere.SetActive(true); // Activate the sphere
        CreateAxes();
        CreateLineRenderers();
        CreateCones();

        thetaSlider.onValueChanged.AddListener(delegate { UpdateVisualization(); });
        phiSlider.onValueChanged.AddListener(delegate { UpdateVisualization(); });
        radiusSlider.onValueChanged.AddListener(delegate { UpdateSphereRadius(); UpdateVisualization(); });

        UpdateSphereRadius();
        UpdateVisualization();
        UpdateSphereTransparency(); // Initialize transparency
    }

    // Method to stop and clear the previous visualization
    void ClearVisualization()
    {
        isStarted = false; // Stop the visualization
        pointInstance.SetActive(false); // Deactivate the point
        transparentSphere.SetActive(false); // Deactivate the sphere

        // Reset sliders to initial values
        radiusSlider.value = radiusSlider.minValue;
        thetaSlider.value = thetaSlider.minValue;
        phiSlider.value = phiSlider.minValue;
        transparencySlider.value = transparencySlider.maxValue;

        // Optionally, clear or reset any other elements
        ClearAxes();
        ClearCones();
    }

    void Update()
    {
        if (isStarted)
        {
            UpdateVisualization(); // Run visualization only after start
        }
    }

    void CreateAxes()
    {
        // X, Y, Z axes creation with cylinders
        xAxis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        xAxis.transform.position = new Vector3(0, 0, -2.5f);
        xAxis.transform.localScale = new Vector3(0.05f, 2.5f, 0.05f);
        xAxis.transform.rotation = Quaternion.Euler(90, 0, 0);
        xAxis.GetComponent<Renderer>().material.color = Color.red;

        yAxis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        yAxis.transform.position = new Vector3(2.5f, 0, 0);
        yAxis.transform.localScale = new Vector3(0.05f, 2.5f, 0.05f);
        yAxis.transform.rotation = Quaternion.Euler(0, 0, 90);
        yAxis.GetComponent<Renderer>().material.color = Color.green;

        zAxis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zAxis.transform.position = new Vector3(0, 2.5f, 0);
        zAxis.transform.localScale = new Vector3(0.05f, 2.5f, 0.05f);
        zAxis.GetComponent<Renderer>().material.color = Color.blue;
    }

    void CreateLineRenderers()
    {
        lineToZAxis = CreateLineRenderer(Color.yellow);
        lineToShadow = CreateLineRenderer(Color.magenta);
        lineToOrigin = CreateLineRenderer(Color.cyan);
        lineToPoint = CreateLineRenderer(Color.white);
    }

    LineRenderer CreateLineRenderer(Color color)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.positionCount = 2;
        return lr;
    }


    void UpdateSphereRadius()
    {
        float radius = radiusSlider.value;
        // Update the scale of the transparent sphere based on the radius slider
        transparentSphere.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
    }

    // Method to update the transparency of the sphere
    void UpdateSphereTransparency()
    {
        float transparency = transparencySlider.value;
        Material sphereMaterial = transparentSphere.GetComponent<Renderer>().material;
        Color color = sphereMaterial.color;
        color.a = transparency;
        sphereMaterial.color = color;
    }

    void UpdateVisualization()
    {
        float r = radiusSlider.value;
        float theta = thetaSlider.value;
        float phi = phiSlider.value;

        float thetaRadians = theta * Mathf.Deg2Rad;
        float phiRadians = phi * Mathf.Deg2Rad;

        float x = r * Mathf.Sin(phiRadians) * Mathf.Cos(thetaRadians);
        float y = r * Mathf.Cos(phiRadians);
        float zPos = r * Mathf.Sin(phiRadians) * Mathf.Sin(thetaRadians);

        Vector3 movingPoint = new Vector3(x, y, zPos);

        pointInstance.transform.position = movingPoint;
        shadowPoint = new Vector3(x, 0, zPos);

        UpdateLineRenderer(lineToZAxis, movingPoint, new Vector3(0, y, zPos));
        UpdateLineRenderer(lineToShadow, movingPoint, shadowPoint);
        UpdateLineRenderer(lineToOrigin, shadowPoint, Vector3.zero);
        UpdateLineRenderer(lineToPoint, Vector3.zero, movingPoint);

        rText.text = "R: " + r.ToString("F2");
        thetaText.text = "θ: " + theta.ToString("F2") + "°";
        phiText.text = "ϕ: " + phi.ToString("F2") + "°";

        xText.text = "X: " + x.ToString("F2");
        yText.text = "Y: " + zPos.ToString("F2");
        zText.text = "Z: " + y.ToString("F2");

        float cylindricalR = Mathf.Sqrt(x * x + zPos * zPos);
        float cylindricalTheta = Mathf.Atan2(zPos, x) * Mathf.Rad2Deg;
        float cylindricalZ = y;

        cylindricalRText.text = "ρ: " + cylindricalR.ToString("F2");
        cylindricalThetaText.text = "ϕ: " + cylindricalTheta.ToString("F2") + "°";
        cylindricalZText.text = "z: " + cylindricalZ.ToString("F2");

        UpdateCone(r, theta, phi);
    }

    void UpdateLineRenderer(LineRenderer lr, Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    void CreateCones()
    {
        coneObject1 = new GameObject("DynamicCone1");
        MeshRenderer meshRenderer1 = coneObject1.AddComponent<MeshRenderer>();
        coneMaterial1 = new Material(Shader.Find("Standard"));
        ConfigureMaterialForTransparency(coneMaterial1);

        // Set fixed transparency for cone 1
        coneMaterial1.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);

        meshRenderer1.material = coneMaterial1;
        coneMesh1 = new Mesh();
        MeshFilter meshFilter1 = coneObject1.AddComponent<MeshFilter>();
        meshFilter1.mesh = coneMesh1;
        coneObject1.transform.position = Vector3.zero;

        coneObject2 = new GameObject("DynamicCone2");
        MeshRenderer meshRenderer2 = coneObject2.AddComponent<MeshRenderer>();
        coneMaterial2 = new Material(Shader.Find("Standard"));
        ConfigureMaterialForTransparency(coneMaterial2);

        // Set fixed transparency for cone 2
        coneMaterial2.color = new Color(0.0f, 1.0f, 1.0f, 0.1f);

        meshRenderer2.material = coneMaterial2;
        coneMesh2 = new Mesh();
        MeshFilter meshFilter2 = coneObject2.AddComponent<MeshFilter>();
        meshFilter2.mesh = coneMesh2;
        coneObject2.transform.position = Vector3.zero;
    }

    void UpdateCone(float radius, float theta, float phi)
    {
        coneMesh1.Clear();
        coneMesh2.Clear();

        if (phi <= 0) return; // No visible cone if phi is 0 or less

        // Calculate the height and the spread of the cones
        float height = radius * Mathf.Cos(phi * Mathf.Deg2Rad);
        float coneRadius = radius * Mathf.Sin(phi * Mathf.Deg2Rad);

        // First part of the cone (0° to 180°)
        int segments1 = Mathf.CeilToInt(Mathf.Min(theta, 180) / 10);
        if (segments1 < 3) segments1 = 3;

        Vector3[] vertices1 = new Vector3[segments1 + 1];
        int[] triangles1 = new int[segments1 * 3];

        vertices1[0] = Vector3.zero; // Base vertex at the origin

        float angleStep1 = (Mathf.Min(theta, 180) / segments1) * Mathf.Deg2Rad;

        for (int i = 0; i < segments1; i++)
        {
            float currentTheta1 = i * angleStep1;
            float x = coneRadius * Mathf.Cos(currentTheta1);
            float z = coneRadius * Mathf.Sin(currentTheta1);
            vertices1[i + 1] = new Vector3(x, height, z); // Set height based on phi
        }
        vertices1[segments1] = vertices1[1]; // Close the cone

        for (int i = 0; i < segments1; i++)
        {
            triangles1[i * 3] = 0;
            triangles1[i * 3 + 1] = i + 1;
            triangles1[i * 3 + 2] = (i + 2) % (segments1 + 1);
        }

        coneMesh1.vertices = vertices1;
        coneMesh1.triangles = triangles1;
        coneMesh1.RecalculateNormals();

        // Second part of the cone (180° to 360°)
        if (theta > 180)
        {
            int segments2 = Mathf.CeilToInt((theta - 180) / 10);
            if (segments2 < 3) segments2 = 3;

            Vector3[] vertices2 = new Vector3[segments2 + 1];
            int[] triangles2 = new int[segments2 * 3];

            vertices2[0] = Vector3.zero; // Base vertex at the origin

            float angleStep2 = ((theta - 180) / segments2) * Mathf.Deg2Rad;

            for (int i = 0; i < segments2; i++)
            {
                float currentTheta2 = (i * angleStep2) + Mathf.PI; // 180 degrees in radians
                float x = coneRadius * Mathf.Cos(currentTheta2);
                float z = coneRadius * Mathf.Sin(currentTheta2);
                vertices2[i + 1] = new Vector3(x, height, z); // Set height based on phi
            }
            vertices2[segments2] = vertices2[1]; // Close the cone

            for (int i = 0; i < segments2; i++)
            {
                triangles2[i * 3] = 0;
                triangles2[i * 3 + 1] = i + 1;
                triangles2[i * 3 + 2] = (i + 2) % (segments2 + 1);
            }

            coneMesh2.vertices = vertices2;
            coneMesh2.triangles = triangles2;
            coneMesh2.RecalculateNormals();
        }

        // Set cone colors based on theta
        // Always set coneMaterial1 to cyan (0° to 180°)
        coneMaterial1.color = Color.cyan;

        // Set coneMaterial2 to a transparent green (180° to 360°) only if theta is greater than 180
        if (theta > 180)
        {
            // Set the alpha value to make it more transparent (adjust as needed)
            Color transparentGreen = new Color(0f, 1f, 0f, 0.5f); // RGBA (Green with 50% transparency)
            coneMaterial2.color = transparentGreen;
        }
        else
        {
            coneMaterial2.color = Color.cyan; // Ensure it's cyan if theta is <= 180
        }
    }

    void ConfigureMaterialForTransparency(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
    // Other methods (CreateAxes, CreateLineRenderers, UpdateVisualization, etc.) remain unchanged
    // ...

    void ClearAxes()
    {
        // Destroy or deactivate axes if they were created
        if (xAxis) Destroy(xAxis);
        if (yAxis) Destroy(yAxis);
        if (zAxis) Destroy(zAxis);
    }

    void ClearCones()
    {
        // Destroy or deactivate cones if they were created
        if (coneObject1) Destroy(coneObject1);
        if (coneObject2) Destroy(coneObject2);
    }
}
