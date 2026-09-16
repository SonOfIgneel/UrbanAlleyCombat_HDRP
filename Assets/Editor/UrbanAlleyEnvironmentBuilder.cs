using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UrbanAlleyEnvironmentBuilder
{
    const string ScenePath = "Assets/Scenes/UrbanAlley_Main.unity";
    const string MaterialFolder = "Assets/UrbanAlleyGenerated/Materials";

    static Scene scene;
    static Material asphalt, concrete, concreteLight, brick, brickDark, metal, paintedMetal, yellow, glass, door, dumpster, lamp;
    static Transform environment, ground, left, right, start, base01, transition, base02, end, props, cover;

    [MenuItem("Tools/Next Defence/Build Final Urban Alley Environment")]
    public static void Build()
    {
        scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath);

        DestroyOwned("Environment");
        DestroyOwned("GameplayMarkers");

        GameObject templateGeometry = GameObject.Find("Geometry");
        if (templateGeometry != null) templateGeometry.SetActive(false);

        CreateMaterials();
        CreateHierarchy();
        CreateGround();
        CreateArchitecture();
        CreateFacadeDetails();
        CreateCoverAndProps();
        CreateImportedProps();
        CreateMarkers();
        CreateLighting();
        PositionCamera();

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = environment.gameObject;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
        Debug.Log("Urban Alley environment built and saved. Serialized Environment objects: " + environment.GetComponentsInChildren<Transform>(true).Length);
    }

    static void DestroyOwned(string name)
    {
        GameObject owned = GameObject.Find(name);
        if (owned != null) Object.DestroyImmediate(owned);
    }

    static void CreateMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/UrbanAlleyGenerated"))
            AssetDatabase.CreateFolder("Assets", "UrbanAlleyGenerated");
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/UrbanAlleyGenerated", "Materials");

        asphalt = MakeMaterial("UA_Asphalt", new Color(.075f, .082f, .085f), 0f, .18f);
        concrete = MakeMaterial("UA_Concrete", new Color(.34f, .35f, .34f), 0f, .28f);
        concreteLight = MakeMaterial("UA_ConcreteLight", new Color(.52f, .50f, .46f), 0f, .30f);
        brick = MakeMaterial("UA_Brick", new Color(.29f, .105f, .07f), 0f, .25f);
        brickDark = MakeMaterial("UA_BrickDark", new Color(.16f, .055f, .04f), 0f, .22f);
        metal = MakeMaterial("UA_DarkMetal", new Color(.075f, .09f, .095f), .75f, .42f);
        paintedMetal = MakeMaterial("UA_PaintedMetal", new Color(.12f, .23f, .25f), .55f, .30f);
        yellow = MakeMaterial("UA_SafetyYellow", new Color(.78f, .45f, .035f), .25f, .28f);
        glass = MakeMaterial("UA_WindowDark", new Color(.025f, .06f, .075f), .25f, .75f);
        door = MakeMaterial("UA_ServiceDoor", new Color(.12f, .14f, .15f), .65f, .35f);
        dumpster = MakeMaterial("UA_DumpsterGreen", new Color(.055f, .19f, .12f), .5f, .2f);
        lamp = MakeMaterial("UA_WarmLamp", new Color(1f, .52f, .16f), 0f, .7f);
        if (lamp.HasProperty("_EmissiveColor")) lamp.SetColor("_EmissiveColor", new Color(4f, 1.25f, .25f));
    }

    static Material MakeMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("HDRP/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void CreateHierarchy()
    {
        environment = Empty("Environment", null);
        ground = Empty("Ground", environment);
        Transform buildings = Empty("Buildings", environment);
        left = Empty("LeftSide", buildings);
        right = Empty("RightSide", buildings);
        start = Empty("StartAlley", environment);
        base01 = Empty("Encounter_Base01", environment);
        transition = Empty("TransitionAlley", environment);
        base02 = Empty("Encounter_Base02", environment);
        end = Empty("EndArea", environment);
        props = Empty("Props", environment);
        cover = Empty("Cover", environment);
    }

    static Transform Empty(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go.transform;
    }

    static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool collider = true, Vector3? rotation = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        if (rotation.HasValue) go.transform.eulerAngles = rotation.Value;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = true;
        return go;
    }

    static GameObject Cylinder(string name, Vector3 position, Vector3 scale, Vector3 rotation, Material material, Transform parent, bool collider = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        go.transform.eulerAngles = rotation;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = true;
        return go;
    }

    static void CreateGround()
    {
        Cube("Ground_StartAlley", new Vector3(0, -.15f, 9), new Vector3(8, .3f, 18), asphalt, ground);
        Cube("Ground_Base01", new Vector3(0, -.15f, 25), new Vector3(12, .3f, 14), asphalt, ground);
        Cube("Ground_TransitionNorth", new Vector3(0, -.15f, 38), new Vector3(7, .3f, 12), asphalt, ground);
        Cube("Ground_TransitionTurn", new Vector3(4, -.15f, 44), new Vector3(8, .3f, 7), asphalt, ground);
        Cube("Ground_Base02", new Vector3(8, -.15f, 54), new Vector3(14, .3f, 18), asphalt, ground);
        Cube("Ground_EndArea", new Vector3(8, -.15f, 67), new Vector3(8, .3f, 8), concrete, ground);
        Cube("Sidewalk_Start_L", new Vector3(-4.35f, .02f, 9), new Vector3(.7f, .22f, 18), concreteLight, ground);
        Cube("Sidewalk_Start_R", new Vector3(4.35f, .02f, 9), new Vector3(.7f, .22f, 18), concreteLight, ground);
        Cube("Sidewalk_Base01_L", new Vector3(-6.35f, .02f, 25), new Vector3(.7f, .22f, 14), concreteLight, ground);
        Cube("Sidewalk_Base01_R", new Vector3(6.35f, .02f, 25), new Vector3(.7f, .22f, 14), concreteLight, ground);
        Cube("Sidewalk_Base02_L", new Vector3(.65f, .02f, 54), new Vector3(.7f, .22f, 18), concreteLight, ground);
        Cube("Sidewalk_Base02_R", new Vector3(15.35f, .02f, 54), new Vector3(.7f, .22f, 18), concreteLight, ground);
    }

    static void CreateArchitecture()
    {
        Cube("Building_Start_Left_Brick", new Vector3(-7, 5, 8.5f), new Vector3(5.5f, 10, 19), brick, left);
        Cube("Building_Start_Right_Concrete", new Vector3(7.25f, 4.5f, 8), new Vector3(6, 9, 20), concrete, right);
        Cube("Building_Base01_Left_Warehouse", new Vector3(-9.5f, 5.5f, 26), new Vector3(6.5f, 11, 16), concrete, left);
        Cube("Building_Base01_Right_Brick", new Vector3(9.5f, 4.5f, 25), new Vector3(6.5f, 9, 15), brickDark, right);
        Cube("Building_Transition_Left", new Vector3(-6.5f, 5, 38), new Vector3(6, 10, 13), brick, left);
        Cube("Building_Transition_Right", new Vector3(6.5f, 5.8f, 37), new Vector3(6, 11.6f, 11), concrete, right);
        Cube("Building_Turn_North", new Vector3(4, 5, 39.5f), new Vector3(8, 10, 3), brickDark, left);
        // Stop short of the east edge so the lateral connector opens clearly into Base 2.
        Cube("Building_Turn_South", new Vector3(2.5f, 4.5f, 48.5f), new Vector3(5, 9, 3), concrete, right);
        Cube("Building_Base02_Left", new Vector3(-2, 6, 55), new Vector3(6, 12, 18), brickDark, left);
        Cube("Building_Base02_Right", new Vector3(18, 5, 54), new Vector3(6, 10, 20), concrete, right);
        Cube("Building_End_Left", new Vector3(1, 4, 68), new Vector3(6, 8, 10), brick, left);
        Cube("Building_End_Right", new Vector3(15, 4.5f, 68), new Vector3(6, 9, 10), brickDark, right);
        Cube("End_BackWall", new Vector3(8, 3, 72), new Vector3(8, 6, .6f), concrete, end);
        Cube("DestinationDoor", new Vector3(8, 1.5f, 71.65f), new Vector3(2.3f, 3, .18f), yellow, end, false);
        Cube("DestinationAwning", new Vector3(8, 3.25f, 70.9f), new Vector3(3.8f, .18f, 1.5f), metal, end);
    }

    static void CreateFacadeDetails()
    {
        float[] sides = {-3.9f, 3.9f};
        foreach (float x in sides)
            for (int i = 0; i < 4; i++)
            {
                float z = 3 + i * 4;
                Cube("RecessWindow_Start_" + x + "_" + i, new Vector3(x, 5.6f, z), new Vector3(.12f, 1.5f, 1.7f), glass, x < 0 ? left : right, false);
                Cube("WindowLintel_Start_" + x + "_" + i, new Vector3(x, 6.55f, z), new Vector3(.18f, .18f, 2), concreteLight, x < 0 ? left : right, false);
            }
        for (int i = 0; i < 4; i++)
        {
            float z = 20.5f + i * 3.3f;
            Cube("Window_Base01_L_" + i, new Vector3(-6.22f, 6.2f, z), new Vector3(.12f, 1.5f, 1.5f), glass, left, false);
            Cube("Window_Base01_R_" + i, new Vector3(6.22f, 5.3f, z), new Vector3(.12f, 1.4f, 1.4f), glass, right, false);
        }
        for (int i = 0; i < 5; i++)
        {
            float z = 47.5f + i * 3.1f;
            Cube("Window_Base02_L_" + i, new Vector3(1.02f, 6.8f, z), new Vector3(.12f, 1.5f, 1.5f), glass, left, false);
            Cube("Window_Base02_R_" + i, new Vector3(14.98f, 5.9f, z), new Vector3(.12f, 1.4f, 1.35f), glass, right, false);
        }
        Cube("ServiceDoor_Start", new Vector3(-3.92f, 1.2f, 5.5f), new Vector3(.16f, 2.4f, 1.45f), door, start, false);
        Cube("ServiceDoor_Base01", new Vector3(6.18f, 1.25f, 28.8f), new Vector3(.16f, 2.5f, 1.5f), door, base01, false);
        Cube("LoadingDoor_Base02", new Vector3(14.9f, 1.7f, 58.5f), new Vector3(.2f, 3.4f, 3.2f), door, base02, false);
        for (int i = 0; i < 4; i++) Cylinder("WallPipe_Start_" + i, new Vector3(-3.78f, 3.2f + i * .15f, 10.5f + i * .28f), new Vector3(.09f, 3.1f, .09f), Vector3.zero, metal, start);
        Cylinder("Pipe_Transition_Horizontal", new Vector3(-3.42f, 4.1f, 38), new Vector3(.12f, 4.8f, .12f), new Vector3(90, 0, 0), metal, transition);
        Cube("UtilityBox_Transition_A", new Vector3(-3.45f, 1.4f, 36), new Vector3(.45f, 1.8f, 1.3f), paintedMetal, transition);
        Cube("UtilityBox_Transition_B", new Vector3(-3.45f, 1, 39), new Vector3(.42f, 1.3f, .9f), paintedMetal, transition);
        Cube("ACUnit_Base01", new Vector3(-6.35f, 3.2f, 23.5f), new Vector3(.8f, 1.2f, 1.6f), metal, base01);
        Cube("ACUnit_Base02", new Vector3(14.6f, 3.4f, 52), new Vector3(.8f, 1.3f, 1.8f), metal, base02);
    }

    static void CreateCoverAndProps()
    {
        MakeDumpster("Dumpster_Start", new Vector3(2.7f, 0, 5), 8);
        MakeDumpster("Dumpster_Base01", new Vector3(-4.6f, 0, 27.5f), -12);
        MakeDumpster("Dumpster_Base02", new Vector3(12.4f, 0, 58.5f), 18);
        CrateStack("Crates_Base01", new Vector3(3.8f, 0, 23), 3);
        CrateStack("Crates_Base02_A", new Vector3(3.2f, 0, 50), 4);
        CrateStack("Crates_Base02_B", new Vector3(10, 0, 55.5f), 3);
        Cube("LowWall_Base01", new Vector3(-.5f, .65f, 28.5f), new Vector3(4.2f, 1.3f, .55f), concrete, cover);
        Cube("LowWall_Base02_A", new Vector3(5, .65f, 58.5f), new Vector3(4.5f, 1.3f, .55f), concrete, cover, true, new Vector3(0, 28, 0));
        Cube("LowWall_Base02_B", new Vector3(11.8f, .65f, 49.5f), new Vector3(3.7f, 1.3f, .55f), concrete, cover, true, new Vector3(0, -28, 0));
        Transform fence = Empty("Fence_Base02", props);
        for (int i = 0; i < 5; i++) Cube("FencePost_" + i, new Vector3(1.2f, 1.2f, 47 + i * 2), new Vector3(.12f, 2.4f, .12f), metal, fence);
        for (int i = 0; i < 2; i++) Cube("FenceRail_" + i, new Vector3(1.2f, .75f + i * 1.1f, 51), new Vector3(.1f, .1f, 8), metal, fence);
    }

    static void MakeDumpster(string name, Vector3 position, float yaw)
    {
        Transform root = Empty(name, props);
        root.position = position;
        root.eulerAngles = new Vector3(0, yaw, 0);
        GameObject body = Cube(name + "_Body", Vector3.zero, new Vector3(2.2f, 1.5f, 1.25f), dumpster, root);
        body.transform.localPosition = new Vector3(0, .75f, 0);
        GameObject lid = Cube(name + "_Lid", Vector3.zero, new Vector3(2.25f, .16f, 1.35f), metal, root, true, new Vector3(0, 0, -4));
        lid.transform.localPosition = new Vector3(0, 1.55f, 0);
        for (int k = -1; k <= 1; k += 2)
        {
            GameObject rib = Cube(name + "_Rib_" + k, Vector3.zero, new Vector3(.12f, 1.3f, 1.3f), dumpster, root, false);
            rib.transform.localPosition = new Vector3(k * .75f, .75f, 0);
        }
    }

    static void CrateStack(string name, Vector3 position, int count)
    {
        Transform root = Empty(name, cover);
        root.position = position;
        for (int i = 0; i < count; i++)
        {
            float x = (i % 2) * 1.05f;
            float y = (i / 2) * 1.05f + .5f;
            GameObject box = Cube(name + "_" + i, Vector3.zero, Vector3.one, i % 2 == 0 ? concreteLight : brick, root);
            box.transform.localPosition = new Vector3(x, y, 0);
            GameObject band = Cube(name + "_Band_" + i, Vector3.zero, new Vector3(1.04f, .08f, 1.04f), metal, root, false);
            band.transform.localPosition = new Vector3(x, y, 0);
        }
    }

    static void CreateImportedProps()
    {
        string barrel = "Assets/Prototype Collection/Assets/FBX/Barrel/Barrel_lod0.fbx";
        Instantiate("Imported_Barrel_Start_A", barrel, new Vector3(-2.9f, 0, 12.5f), Vector3.zero, Vector3.one, props);
        Instantiate("Imported_Barrel_Start_B", barrel, new Vector3(-2.1f, 0, 12.7f), new Vector3(0, 12, 0), Vector3.one, props);
        Instantiate("Imported_Barrel_Base01", barrel, new Vector3(4.7f, 0, 29.5f), new Vector3(0, 30, 0), Vector3.one, props);
        Instantiate("Imported_Barrel_Base02_A", barrel, new Vector3(12, 0, 51.5f), new Vector3(0, -20, 0), Vector3.one, props);
        Instantiate("Imported_Barrel_Base02_B", barrel, new Vector3(12.8f, 0, 51.7f), new Vector3(0, 10, 0), Vector3.one, props);
        string barrier = "Assets/Prototype Collection/Assets/FBX/Blocks/Block_Barrier_1.fbx";
        Instantiate("Imported_ConcreteBarrier_Base01_A", barrier, new Vector3(-3.2f, 0, 21.2f), new Vector3(0, 12, 0), Vector3.one, cover);
        Instantiate("Imported_ConcreteBarrier_Base01_B", barrier, new Vector3(2.5f, 0, 27.5f), new Vector3(0, -20, 0), Vector3.one, cover);
        Instantiate("Imported_ConcreteBarrier_Base02_A", barrier, new Vector3(4, 0, 54), new Vector3(0, 25, 0), Vector3.one, cover);
        Instantiate("Imported_ConcreteBarrier_Base02_B", barrier, new Vector3(11.5f, 0, 60), new Vector3(0, -20, 0), Vector3.one, cover);
        string pedestrian = "Assets/Prototype Collection/Assets/FBX/Pedestrian Barrier/Pedestrian_Barrier_lod0.fbx";
        Instantiate("Imported_PedestrianBarrier_Transition", pedestrian, new Vector3(1.5f, 0, 42.2f), new Vector3(0, 78, 0), Vector3.one, props);
        Instantiate("Imported_PedestrianBarrier_End", pedestrian, new Vector3(5, 0, 68.8f), new Vector3(0, 20, 0), Vector3.one, props);
        string lamppost = "Assets/Tolik_Cola/Recreation park/Lamppost/1/LowPoli/Lamppost_L_1.prefab";
        Instantiate("Imported_Lamppost_Start", lamppost, new Vector3(3.3f, 0, 14.8f), new Vector3(0, 180, 0), Vector3.one, props);
        Instantiate("Imported_Lamppost_Base01", lamppost, new Vector3(-5, 0, 20.5f), Vector3.zero, Vector3.one, props);
        Instantiate("Imported_Lamppost_Base02", lamppost, new Vector3(13.8f, 0, 46.5f), new Vector3(0, 180, 0), Vector3.one, props);
        Instantiate("Imported_Transformer_Base02", "Assets/Vlad Belza/Transformer HDRP/Prefab/Transformer 1.prefab", new Vector3(12.5f, 0, 47.8f), new Vector3(0, -90, 0), Vector3.one, props);
        Instantiate("Imported_Workbench_Base01", "Assets/Factory Tools/Prefabs/WoodenWorkbench.prefab", new Vector3(-5, 0, 22.8f), new Vector3(0, 90, 0), Vector3.one, props);
        Instantiate("Imported_FireExtinguisher_Base02", "Assets/Factory Tools/Prefabs/FireExtinguisher.prefab", new Vector3(14.65f, 1.1f, 56.5f), new Vector3(0, -90, 0), Vector3.one, props);
    }

    static GameObject Instantiate(string name, string path, Vector3 position, Vector3 rotation, Vector3 scale, Transform parent)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { Debug.LogWarning("Urban Alley asset unavailable: " + path); return null; }
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
        go.name = name;
        go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.eulerAngles = rotation;
        go.transform.localScale = scale;
        if (go.GetComponentsInChildren<Collider>(true).Length == 0) AddBoundsCollider(go);
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        return go;
    }

    static void AddBoundsCollider(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(bounds.center);
        Vector3 x = go.transform.InverseTransformVector(new Vector3(bounds.size.x, 0, 0));
        Vector3 y = go.transform.InverseTransformVector(new Vector3(0, bounds.size.y, 0));
        Vector3 z = go.transform.InverseTransformVector(new Vector3(0, 0, bounds.size.z));
        box.size = new Vector3(x.magnitude, y.magnitude, z.magnitude);
    }

    static void CreateMarkers()
    {
        Transform root = Empty("GameplayMarkers", null);
        Marker("PlayerStart", new Vector3(0, 0, -.5f), Vector3.zero, root);
        Marker("Base01_Center", new Vector3(0, 0, 25), Vector3.zero, root);
        Marker("Base02_Center", new Vector3(8, 0, 54), Vector3.zero, root);
        Marker("EndPoint", new Vector3(8, 0, 68.2f), Vector3.zero, root);
    }

    static void Marker(string name, Vector3 position, Vector3 rotation, Transform parent)
    {
        Transform marker = Empty(name, parent);
        marker.position = position;
        marker.eulerAngles = rotation;
    }

    static void CreateLighting()
    {
        GameObject lightingRoot = GameObject.Find("Lighting");
        if (lightingRoot == null) lightingRoot = Empty("Lighting", null).gameObject;
        Transform prior = lightingRoot.transform.Find("Generated_AlleyLights");
        if (prior != null) Object.DestroyImmediate(prior.gameObject);
        Transform root = Empty("Generated_AlleyLights", lightingRoot.transform);
        AddLight("WallLamp_Start", new Vector3(-3.7f, 3.2f, 8), new Color(1f, .58f, .32f), 850, root);
        AddLight("WallLamp_Base01", new Vector3(6, 3.4f, 25), new Color(1f, .62f, .38f), 1000, root);
        AddLight("WallLamp_Transition", new Vector3(-3.2f, 3.2f, 39), new Color(.75f, .85f, 1f), 800, root);
        AddLight("WallLamp_Base02", new Vector3(14.7f, 3.5f, 55), new Color(1f, .58f, .30f), 1100, root);
        AddLight("DestinationLight", new Vector3(8, 3.6f, 70.8f), new Color(1f, .72f, .35f), 1000, root);
    }

    static void AddLight(string name, Vector3 position, Color color, float intensity, Transform parent)
    {
        Transform root = Empty(name, parent);
        root.position = position;
        Light lightComponent = root.gameObject.AddComponent<Light>();
        lightComponent.type = LightType.Point;
        lightComponent.color = color;
        lightComponent.intensity = intensity;
        lightComponent.range = 8.5f;
        lightComponent.shadows = LightShadows.Soft;
        GameObject fixture = Cube(name + "_Fixture", Vector3.zero, new Vector3(.35f, .18f, .35f), lamp, root, false);
        fixture.transform.localPosition = Vector3.zero;
    }

    static void PositionCamera()
    {
        GameObject cameraObject = GameObject.FindWithTag("MainCamera");
        if (cameraObject == null) return;
        cameraObject.transform.position = new Vector3(0, 1.7f, -2.2f);
        cameraObject.transform.rotation = Quaternion.LookRotation(new Vector3(0, 1.2f, 10) - cameraObject.transform.position, Vector3.up);
        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera != null) { camera.fieldOfView = 68; camera.farClipPlane = 250; }
    }
}
