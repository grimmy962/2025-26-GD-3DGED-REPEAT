# Lab: JSON Serialization

## Overview
In this lab, you’ll author a small **JSON file** that describes a single model spawn (position, rotation in degrees, scale, texture key, model key, and object name), then write a few **C# helpers** to read the JSON and **call `InitializeModel(...)`** with those values.

We’ll load JSON during your **`Main.Initialize()`** setup phase—**after** assets are loaded (so model/texture keys exist) and **before** the game starts updating. This keeps data-driven spawns **declarative** (editable without code) and repeatable across runs.

## Rationale
- Your `Main` already has a helper **`InitializeModel(...)`** that takes `Vector3 position`, `Vector3 eulerRotationDegrees`, `Vector3 scale`, `string textureName`, `string modelName`, and `string objectName`. We’ll reuse it instead of re-implementing model setup.
- Students can tweak **spawn data** in JSON without recompiling.
- We’ll use `System.Text.Json` with a tiny **`Vector3JsonConverter`** so authors can type vectors as `[x, y, z]`.

---

## Prerequisites
- The content dictionaries already map at least one model (e.g., `"monkey1"`) and texture (e.g., `"checkerboard"`). These keys must match your JSON.
- Ensure your JSON file is set to **Copy to Output Directory** (`Copy if newer`), so `File.ReadAllText` can find it at runtime.

> Reference: `Main.InitializeModel(...)` exists and creates a GameObject with transform + components using your dictionaries.

---

## Step-by-step: From empty JSON to spawning a model

We’ll edit only what’s necessary, in the smallest possible steps.

### 1) Create a JSON file
Create `Content/model_spawn.json` and paste:

```json
{
  "position": [ -10, 10, 0 ],
  "rotationDegrees": [ -90, 0, 0 ],
  "scale": [ 5, 5, 5 ],
  "textureName": "checkerboard",
  "modelName": "monkey1",
  "objectName": "json-spawned-monkey"
}
```

> Tip: If you change the keys, keep them in sync with your asset dictionaries in `LoadTextures()` / `LoadModels()`.

---

### 2) Add a simple data class **inside `Main.cs`** (near the end of the file is fine)
This class mirrors the JSON shape and matches `InitializeModel(...)` parameters.

```csharp
// Add near the bottom of Main.cs (outside other methods)

/// <summary>
/// Plain data container for a single model spawn, loaded from JSON.
/// </summary>
private sealed class ModelSpawnData
{
    public Vector3 Position { get; set; }
    public Vector3 RotationDegrees { get; set; }
    public Vector3 Scale { get; set; }
    public string TextureName { get; set; }
    public string ModelName { get; set; }
    public string ObjectName { get; set; }
}
```

> We’ll configure the JSON serializer to be **case-insensitive**, so camelCase in JSON maps to PascalCase here without attributes.

---

### 3) Add a small `Vector3` converter (once, reusable)
Place this class below `ModelSpawnData` (still inside `Main.cs`). It allows JSON vectors as arrays like `[x, y, z]` **and** as objects like `{ "x": 0, "y": 1, "z": 2 }`.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

// ... keep your other usings above

/// <summary>
/// JSON converter for Microsoft.Xna.Framework.Vector3.
/// Accepts either array form [x,y,z] or object form {"x":X,"y":Y,"z":Z}.
/// Writes as [x,y,z].
/// </summary>
private sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read(); float x = (float)reader.GetDouble();
            reader.Read(); float y = (float)reader.GetDouble();
            reader.Read(); float z = (float)reader.GetDouble();
            reader.Read(); // EndArray
            return new Vector3(x, y, z);
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            float x = 0, y = 0, z = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string name = reader.GetString();
                reader.Read();
                float val = (float)reader.GetDouble();
                if (string.Equals(name, "x", StringComparison.OrdinalIgnoreCase)) x = val;
                else if (string.Equals(name, "y", StringComparison.OrdinalIgnoreCase)) y = val;
                else if (string.Equals(name, "z", StringComparison.OrdinalIgnoreCase)) z = val;
            }
            return new Vector3(x, y, z);
        }

        throw new JsonException("Vector3 must be [x,y,z] or {x,y,z}.");
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}
```

---

### 4) Add a loader method to read JSON into `ModelSpawnData`
Add this private helper method inside `Main`:

```csharp
using System.IO; // ensure this using exists at the top

/// <summary>
/// Reads a ModelSpawnData from a JSON file on disk.
/// </summary>
private ModelSpawnData LoadModelSpawnData(string relativePath)
{
    // Compose a path relative to your Content root (Content/).
    // Ensure the JSON file is copied to the output folder.
    string path = Path.Combine(Content.RootDirectory, relativePath);

    string json = File.ReadAllText(path);

    var opts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
    opts.Converters.Add(new Vector3JsonConverter());

    ModelSpawnData data = JsonSerializer.Deserialize<ModelSpawnData>(json, opts);
    return data;
}
```

> We keep the options **lenient** so students can add comments and trailing commas while learning.

---

### 5) Add a tiny adapter that calls your existing `InitializeModel(...)`
Also inside `Main`, add:

```csharp
/// <summary>
/// Loads a JSON file and spawns one model by calling InitializeModel(...).
/// </summary>
private void InitializeModelFromJson(string relativeJsonPath)
{
    var d = LoadModelSpawnData(relativeJsonPath);

    InitializeModel(
        d.Position,
        d.RotationDegrees,
        d.Scale,
        d.TextureName,
        d.ModelName,
        d.ObjectName);
}
```

---

### 6) Call it from `Initialize()` (after assets are loaded)
In `Initialize()`, **after** `LoadAssets();` and **after** your materials/effects are ready, replace your hard-coded example (or keep it) and add the JSON-driven call:

```csharp
// Existing hard-coded example (you may keep or remove):
// InitializeModel(new Vector3(-10, 10, 0),
//     new Vector3(-90, 0, 0),
//     5 * Vector3.One,
//     "checkerboard",
//     "monkey1",
//     "my first monkey game object");

// JSON-driven spawn (requires Content/model_spawn.json to be copied to output):
InitializeModelFromJson("model_spawn.json");
```

That’s it—the scene should now include a GameObject driven by your JSON file.

---

## Run & Verify
1) Build & run.  
2) If you see the model with your specified texture and transform, success.  
3) Change the JSON (e.g., scale to `[8,8,8]`, texture key to `"crate1"` if it exists) and run again. No recompile needed.

**Common gotchas**
- If you get “file not found,” set your `model_spawn.json` file property to **Copy to Output Directory → Copy if newer**.
- If you see “character not in SpriteFont” errors, that’s unrelated to this lab (comes from text drawing elsewhere).
- If keys don’t match (`textureName`, `modelName`), make sure they exist in `LoadTextures()` / `LoadModels()`.

---

## Suggested Improvements (easy wins)
- Support arrays of spawns: `model_spawns.json` with `[{...}, {...}]`, loop and call `InitializeModel(...)` for each.
- Add **defaults** in code for missing fields (e.g., `scale: [1,1,1]` if absent).
- Write a **validator** that logs missing keys or assets that aren’t found.
- Save a “snapshot” JSON with the current transform of a selected object for round-trip editing.

---

## Appendix: Minimal `model_spawns.json` (multiple entries)
If you want multiple spawns right away, use this shape and parse a `List<ModelSpawnData>`:

```json
[
  {
    "position": [0, 5, 0],
    "rotationDegrees": [0, 0, 0],
    "scale": [3, 3, 3],
    "textureName": "checkerboard",
    "modelName": "monkey1",
    "objectName": "spawn-1"
  },
  {
    "position": [10, 10, -5],
    "rotationDegrees": [0, 45, 0],
    "scale": [2, 2, 2],
    "textureName": "crate1",
    "modelName": "monkey1",
    "objectName": "spawn-2"
  }
]
```

Deserializer change example:

```csharp
private List<ModelSpawnData> LoadSpawns(string relativePath)
{
    string path = Path.Combine(Content.RootDirectory, relativePath);
    string json = File.ReadAllText(path);

    var opts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
    opts.Converters.Add(new Vector3JsonConverter());

    return JsonSerializer.Deserialize<List<ModelSpawnData>>(json, opts);
}
```

Loop them in `Initialize()` and call `InitializeModel(...)` for each.
