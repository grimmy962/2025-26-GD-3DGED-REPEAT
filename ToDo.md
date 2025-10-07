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
- [ ] Create clean MonoGame Game + Engine project.
- [ ] Add `Camera` with position, forward, up; FOV/aspect/near/far.
- [ ] Add `EngineContext` with `GraphicsDevice`, `Content`, `GameTime`, `SpriteBatch`.
- [ ] Add `SystemBase` (Update/Draw hooks).
- [ ] Add `Component` base with `Enabled`, lifecycle hooks, internals for `Awake/Start`.
- [ ] Add `GameObject` with `AddComponent<T>()`, `GetComponent<T>()`.
- [ ] Implement `Transform` (LocalPosition/Rotation/Scale; LocalMatrix; WorldMatrix).
- [ ] Parent/child with `SetParent`, and `Forward/Right/Up` helpers.
- [ ] Convert standalone camera to a `Camera` **Component**.
- [ ] `Camera.LateUpdate` computes View/Projection from `Transform`.
- [ ] Create `MeshFilter` with `VertexBuffer`, `IndexBuffer?`, `PrimitiveType`, `PrimitiveCount`, bounds.
- [ ] Add `MeshRenderer` with `BasicEffect` (`VertexColorEnabled=true`, depth on in renderer).
- [ ] Add `RenderingSystem` that gathers `(Transform, MeshFilter, MeshRenderer)`.
- [ ] Define `InputState` (Move, JumpPressed, Action1, Action2).
- [ ] Implement `IInputDevice` + `KeyboardInput`, `GamepadInput`.
- [ ] Implement `IInputReceiver` (e.g., `PlayerController` component).
- [ ] Add `InputSystem` that routes device → receiver; expose `SetDevice`, `SetReceiver`.
- [ ] Add `RenderLayer` sorting in `RenderingSystem`.
- [ ] Add **frustum culling** with `BoundingFrustum` against `MeshFilter.Bounds`.
- [ ] Add **material abstraction**: wrap `Effect` into a `Material` with parameters; pipeline for future shaders.
- [ ] Add **prefab/serialization**: simple JSON for spawning `GameObject` graphs.
- [ ] Add **event bus**: lightweight pub/sub for decoupled messages between systems.
- [ ] Add **audio hooks**: `AudioSystem` placeholder; service access via `EngineContext`.

