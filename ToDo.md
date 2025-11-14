# 3D Game Engine Development – ToDo List

## Overview
This document contains a step-by-step development plan of MonoGame content covered in class. The work is structured week-by-week and will be developed **live in class** for our custom game engine (`GDEngine`). We will implement a Unity‑modeled ECS in incremental milestones. Keep code changes small, demoable, and reversible.

## Goals
- **Unity‑parity mental model** (Scene, GameObject, Component, Systems, Transform, Camera)
- **Deterministic lifecycle** (Awake → Start → Update → LateUpdate → OnDestroy; Start once)
- **Data/behavior split** (Components hold data + narrow local logic; Systems iterate and act)
- **Functional rendering path** (MeshFilter + MeshRenderer + RenderingSystem with WVP)
- **Hot‑swappable input** (Keyboard/Gamepad devices, pluggable receiver)
- **Pedagogical pacing** (each step compiles and shows visible progress)

## Instructions
- Follow the tasks in order (marked with `[ ]` for incomplete and `[x]` for complete).
- Code is developed interactively in class; update this checklist as we progress.
- Unit tests are implemented in a **separate MSTest project** after the class implementation is stable.
- Each week builds on the previous week.

---

## Performance & Optimization

---

## Week 4
- [x] Create clean MonoGame Game + Engine project.
- [x] Add `EngineContext` with `GraphicsDevice`, `Content`, `GameTime`, `SpriteBatch`.
- [x] Add `SystemBase` (Update/Draw hooks).
- [x] Refactor `EngineContext` to make it a singleton and implement `IDisposable`
- [x] Add `Camera` with position, forward, up; FOV/aspect/near/far.
- [x] Add `Component` base with `Enabled`, lifecycle hooks, internals for `Awake/Start`.
- [x] Add `GameObject` with `AddComponent<T>()`, `GetComponent<T>()`.
- [x] Implement `Transform` (LocalPosition/Rotation/Scale; LocalMatrix; WorldMatrix).

## Week 5
- [x] Add Time class to support timescale; refactor update methods from GameTime to deltaTime
- [x] Parent/child with `SetParent`, and `Forward/Right/Up` helpers.
- [x] Convert standalone camera to a `Camera` **Component**.
- [x] `Camera.LateUpdate` computes View/Projection from `Transform`.
- [x] Create `MeshFilter` with `VertexBuffer`, `IndexBuffer?`, `PrimitiveType`, `PrimitiveCount`, bounds.
- [x] Add `MeshRenderer` with `BasicEffect` (`VertexColorEnabled=true`, depth on in renderer).

## Week 6
- [x] Add LayerMask in preparation for 1st stage of camer culling
- [x] Add FBX loading in MeshFilterFactory
- [x] Add some assets for grass and skybox
- [ ] Add support for deep and shallow cloning on GameObject components
- [x] Add Material and RenderState classes to support multiple effect types
- [x] Add ContentDictionary to store asset references
- [x] Add keyboard and mouse controllers for camera
- [x] Add AnimationCurve classes (1D, 2D, 3D) to support smooth movement along a curve
- [x] Add DemoAnimationCurveController to move a gameobject along a user-defined curve
- [x] Add lit and unlit textured cubes in MeshFilterFactory
- [ ] Add perf HUD with FPS, verts, primitives stats
- [ ] Add dictionary in MeshFilterFactory to reduce VB and IB usage on duplicate calls to a method
- [ ] Add `RenderingSystem` that gathers `(Transform, MeshFilter, MeshRenderer)`.
- [ ] Add `RenderLayer` sorting in `RenderingSystem`.
- [ ] Add **material abstraction**: wrap `Effect` into a `Material` with parameters; pipeline for future shaders.

## Reading Week
- [x] Define `InputState` (Move, JumpPressed, Action1, Action2).
- [x] Implement `IInputDevice` + `KeyboardInput`, `GamepadInput`.
- [x] Implement `IInputReceiver` (e.g., `PlayerController` component).
- [x] Add `InputSystem` that routes device → receiver; expose `SetDevice`, `SetReceiver`.
- [x] Add `IDisposable` to core classes
- [x] Add support for MonoGame effect types in `Material` and `MeshRenderer`.

## Week 7
- [x] Add Integer structs (e.g. `Integer2`)
- [x] Add `ScreenResolution` for easy resolution changes
- [x] Performance improvements (replace pow with direct multiplication) in `Ease`
- [x] Add `PerfStats` to show FPS, etc
- [x] Add `Scene::Find` and `Scene::FindAll` for finding GameObjects by `Predicate`
- [x] Add `Main::InitializeModel()` to load FBX models
- [x] Add **serialization**: simple JSON for spawning `GameObject`.
- [x] Add `UIReticuleRender` demo to show `UIRenderingSystem` in action

## Week 8
- [x] Improve name formatting on `ScreenResolution` fields
- [x] Add `WindowUtility` to centre game to prevent annoying drag on open
- [x] Add **event bus**: lightweight pub/sub for decoupled messages between systems.
- [x] Add JSON asset loading support to `ContentDictionary` and move JSON files to `Data` folder
- [x] Add `NamedDictionary` to support loading assets that dont use `Content::Load` (e.g. `AnimationCurve`)
- [x] Add `UITextureRenderer` and simple menu demo
- [x] Rename `UIRenderingSystem` to `UIRenderSystem`
- [x] Rename `RenderingSystem` to `RenderSystem`
- [x] Set game window title and change icon
- [x] Add drivable model with 3rd person camera
- [x] Add UI support for HUD and menu
- [x] Add MSTests for Transform to isolate bugs
- [ ] Add OrchestrationSystem for sequencing game events
- [ ] Add support for single or multi camera views in RenderSystem
- [ ] Re-factor UI renderers to share common fields in parent
- [ ] Add other camera controller types
- [ ] Add physics engine (physics and CDCR)


## Week 9
- [ ] Add support for opaque/transparent objects
- [ ] Split gameobjects in `Scene` into opaque/transparent, active/inactive, static/dynamic
- [ ] Add **audio hooks**: `AudioSystem` to support 2D and 3D sound; service access via event bus.
- [ ] Add **frustum culling** with `BoundingFrustum` against `MeshFilter.Bounds`.
- [ ] Add event on resolution change
- [ ] Add demos for effect supporting normal maps and environment maps

## Bugs
- [ ] Is camera filtering by layermask?
- [ ] Can I add multiple overlay cameras sorted by depth?
