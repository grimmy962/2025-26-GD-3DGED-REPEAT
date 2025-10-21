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
- [x] Added LayerMask in preparation for 1st stage of camer culling
- [x] Add FBX loading in MeshFilterFactory
- [x] Add some assets for grass and skybox
- [ ] Add support for deep and shallow cloning on GameObject components
- [x] Added Material and RenderState classes to support multiple effect types
- [x] Added ContentDictionary to store asset references
- [ ] Add keyboard and mouse controllers for camera
- [ ] Add `RenderingSystem` that gathers `(Transform, MeshFilter, MeshRenderer)`.
- [ ] Add `RenderLayer` sorting in `RenderingSystem`.
- [ ] Define `InputState` (Move, JumpPressed, Action1, Action2).
- [ ] Implement `IInputDevice` + `KeyboardInput`, `GamepadInput`.
- [ ] Implement `IInputReceiver` (e.g., `PlayerController` component).
- [ ] Add `InputSystem` that routes device → receiver; expose `SetDevice`, `SetReceiver`.
- [ ] Add **frustum culling** with `BoundingFrustum` against `MeshFilter.Bounds`.
- [ ] Add **material abstraction**: wrap `Effect` into a `Material` with parameters; pipeline for future shaders.
- [ ] Add **prefab/serialization**: simple JSON for spawning `GameObject` graphs.
- [ ] Add **event bus**: lightweight pub/sub for decoupled messages between systems.
- [ ] Add **audio hooks**: `AudioSystem` placeholder; service access via `EngineContext`.

