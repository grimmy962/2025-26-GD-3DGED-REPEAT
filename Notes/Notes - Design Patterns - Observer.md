# Understanding the Observer Pattern & Building a Simple Event Bus in C#

## Learning Objectives

By the end of this lesson, you should be able to:

1. **Explain** the Observer design pattern and describe when it is appropriate in game development.
2. **Implement** a minimal Observer-style event system using a simple Event Bus in C#.
3. **Analyse** how event queuing, subscription, and dispatching support decoupled game architecture.
4. **Extend** the simple Event Bus to support filters, once-only subscribers, typed events, and priorities.
5. **Reflect** on architectural trade-offs between C# events, an Event Bus, and an Event System.

---

## Overview

In game development, systems frequently need to **react** to changes that occur elsewhere in the engine.  
- When the player takes damage, the HUD must update.  
- When an enemy dies, the score manager must adjust.  
- When a level loads, UI elements, game objects, and analytics systems often need to be notified.

A naïve design tightly couples these systems together, forcing classes to know far too much about each other.  
The **Observer Pattern** avoids this by allowing one object (the subject) to publish notifications, while independent observers subscribe to these notifications without forming direct dependencies.

This note begins with the conceptual foundations of the Observer Pattern, introduces a clean and minimal Event Bus in C#, and concludes with a set of clear steps for extending the Event Bus into a fully featured system suitable for a 3D game engine. Each code sample includes an explanation of how it works and why the design choices make sense in the context of decoupled engine architecture.

---

### 1. The Observer Pattern

The **Observer Pattern** defines a one-to-many dependency between objects.  
When one object changes state, all registered observers are notified automatically.

A typical academic definition:

> “The Observer Pattern allows an object (the subject) to maintain a list of dependents (observers), and automatically notify them of any state changes.”

In game development, this pattern is used to:

- Broadcast player events (damaged, healed, died)
- Notify UI panels about inventory changes
- Signal mission systems when objectives are completed
- Trigger audio or particle effects when interactions occur

### 1.1 Basic C# Observer Example

```csharp
public class Player
{
    public event Action<int>? HealthChanged;
    private int _health = 100;

    public void TakeDamage(int amount)
    {
        _health = Math.Max(0, _health - amount);
        HealthChanged?.Invoke(_health);
    }
}

public class HealthBar
{
    public void OnHealthChanged(int newHealth)
    {
        Console.WriteLine($"Health now: {newHealth}");
    }
}
```

**Explanation**

- `Player` exposes an event called `HealthChanged`.
- Any observer (e.g. `HealthBar`) can subscribe to it without the player knowing who the observers are.
- When damage is applied, `HealthChanged?.Invoke(...)` notifies all observers.
- This decouples the player logic from the UI logic.

### 1.2 Subscribing to the Event

```csharp
var player = new Player();
var hud = new HealthBar();

player.HealthChanged += hud.OnHealthChanged;

player.TakeDamage(25);
player.TakeDamage(40);
```

**Explanation**

- The HUD subscribes by attaching `hud.OnHealthChanged` to the event.
- The player does not need any direct reference to the HUD.
- This gives basic decoupling, but only supports per-object events.  
  For large engines with many systems, a broader architecture is needed: an **Event Bus**.

---

## 2. From Observer to Event Bus

While C# events work well within a single class, larger systems often require:

- Central coordination  
- Multiple publishers and subscribers  
- Event routing decoupled from specific classes  
- Guaranteed ordering  
- Ability to buffer and process events per frame  
- Cross-system communication without mutual references

An **Event Bus** generalises the Observer pattern into a global hub for publishing and subscribing to events.

Our first step is to build a *simple educational bus*, based directly on the provided code files.

---

## 3. A Simple Event Bus in C#

The following implementation mirrors the attached files (`EventBus.cs`, `Program.cs`) and introduces the core ideas: registering handlers, publishing events, and processing a queue.

### 3.1 Simple EventBus

```csharp
public static class EventBus
{
    private static Queue<string> _events = new Queue<string>(64);
    private static Dictionary<string, List<Action<string>>> _subscribers
        = new Dictionary<string, List<Action<string>>>();

    public static void Subscribe(string name, Action<string> handler)
    {
        if (_subscribers.TryGetValue(name, out var list))
            list.Add(handler);
        else
            _subscribers[name] = new List<Action<string>> { handler };
    }

    public static void Publish(string name)
    {
        _events.Enqueue(name);
    }

    public static void ProcessAll()
    {
        while (_events.Count > 0)
        {
            var evt = _events.Dequeue();

            if (_subscribers.TryGetValue(evt, out var handlers))
            {
                foreach (var handler in handlers)
                    handler(evt);
            }
        }
    }
}
```

**Explanation**

- `_events` is a FIFO queue storing event names.
- `_subscribers` maps event names to lists of handlers.
- `Subscribe` registers a new handler for a named event.
- `Publish` places a new event into the queue.
- `ProcessAll` dequeues events and calls all registered handlers.
- This mirrors the Observer pattern but centralises the relationship:  
  many publishers → one event hub → many observers.

### 3.2 Program Example

```csharp
static void Main(string[] args)
{
    EventBus.Subscribe("flag", OnFlag);
    EventBus.Subscribe("flag", evt => Console.WriteLine($"Lambda: {evt}"));

    EventBus.Publish("flag");

    EventBus.ProcessAll();
}

static void OnFlag(string evt)
{
    Console.WriteLine($"Handled: {evt}");
}
```

**Explanation**

- Two handlers subscribe to the `"flag"` event.
- `"flag"` is published and queued.
- `ProcessAll()` dispatches the event to both handlers.
- The architecture is simple but demonstrates the power of decoupled design.

---

## 4. Why Use an Event Bus?

This approach has several benefits in a game engine:

1. **Loose coupling**  
   Systems never need references to each other.

2. **Broadcast semantics**  
   Multiple listeners can react independently.

3. **Centralised control**  
   The bus can log, filter, buffer, and order events.

4. **Frame-based dispatching**  
   Events can be queued and processed at a stable point each frame.

5. **Scalable extension path**  
   The simple bus can grow into a robust implementation with filters, once-only subscriptions, priorities, and typed events.

---

## 5. Extension Steps Toward a Robust Event Bus

This section outlines a sequence of steps that progressively transform the simple bus into the more advanced, engine-level system used in the full 3DGED engine.

Each step can be implemented independently.

### Step 1 — Replace String Events with Strongly-Typed Event Objects

Instead of publishing `"damage"`, publish:

```csharp
public sealed class DamageEvent
{
    public string TargetId { get; }
    public int Amount { get; }

    public DamageEvent(string targetId, int amount)
    {
        TargetId = targetId;
        Amount = amount;
    }
}
```

**Why**

- Strong typing prevents errors and enables richer payloads.
- Event types become part of the engine architecture.

---

### Step 2 — Generic Subscribe/Publish

Move from `Subscribe(string, Action<string>)` to:

```csharp
public static void Subscribe<T>(Action<T> handler) { ... }
public static void Publish<T>(T evt) { ... }
```

**Why**

- Each event type has its own subscriber list.
- No casting or string keys.
- Mirrors the engine’s final design.

---

### Step 3 — Return Subscription Tokens (Unsubscribe Support)

Introduce a subscription object:

```csharp
public sealed class EventSubscription : IDisposable
{
    // Stores event type, handler, and logic to remove itself.
}
```

Each call to `Subscribe<T>` returns an `EventSubscription`.

**Why**

- Components can unsubscribe when destroyed.
- Prevents memory leaks and dangling handlers.

---

### Step 4 — Add Once-Only Subscriptions

Support:

```csharp
bus.Subscribe<DamageEvent>(OnDamage, once: true);
```

Internal storage includes a `bool Once` flag.  
After dispatch, once subscriptions are removed.

**Why**

- Useful for tutorials, achievements, one-time triggers.

---

### Step 5 — Add Filters (Predicates)

Allow:

```csharp
bus.Subscribe<DamageEvent>(
    OnDamage,
    filter: e => e.Amount > 10);
```

Filters determine if a handler should run for a specific event instance.

**Why**

- Reduces branching within handlers.
- Supports complex matching (player-only events, tag-based events, etc.).

---

### Step 6 — Add Priorities

Introduce integer priorities:

```csharp
bus.Subscribe<DamageEvent>(OnDamage, priority: EventPriority.Core);
```

Sort handlers by priority before dispatch.

**Why**

- Some systems must run earlier (orchestrator) or later (UI).

---

### Step 7 — Add Frame-Based Dispatch via EventSystem

Move processing to a dedicated system that calls:

```csharp
bus.DispatchAll();
```

once per frame, ensuring events run on the main thread at a predictable lifecycle point.

**Why**

- Thread safety  
- Reproducible ordering  
- Matches engine architecture  

---

### Step 8 — Fluent Builder API

Enable expressive, chainable subscriptions:

```csharp
bus.On<DamageEvent>()
   .WithPriority(EventPriority.Gameplay)
   .When(e => e.Amount > 5)
   .Once()
   .Do(e => Console.WriteLine("Big damage detected"));
```

**Why**

- Clean DSL for students and designers.
- Matches the final `EventBusFluent` helper in the engine.

---

## Summary

- The Observer Pattern provides a foundation for decoupled communication between objects.
- A simple Event Bus is an elegant, general-purpose implementation of this pattern.
- Beginning with minimal string-based events demonstrates the key ideas clearly.
- A structured set of extensions (typed events, filters, once-only, priorities, frame-based dispatch, fluent builder) gradually transforms the simple bus into a production-grade event system.
- This architecture supports scalable, maintainable, and testable game engine design.

---

## Reflective Questions

1. In your own words, why does the Observer Pattern reduce coupling between game systems?
2. What advantages does an Event Bus offer over C#’s built-in `event` keyword?
3. Why is it useful to buffer events and process them during a controlled phase of the frame?
4. How does using strongly-typed event classes improve robustness?
5. Describe a situation in which a filter predicate would simplify event handling logic.
6. Why might a priority system be necessary for ordering event handlers?
7. Compare a once-only subscription with using a flag inside a handler. Which approach is cleaner, and why?
8. Consider the fluent builder API. What benefits does method chaining provide in terms of readability and maintainability?

---

## Appendix: Event Bus Architecture Diagram

```mermaid
flowchart LR
    subgraph Publishers
        P1[Player]
        P2[Enemy]
        P3[UI Button]
    end

    subgraph EventBus
        Q[Event Queue]
        S[Subscriber Map]
    end

    subgraph Subscribers
        H1[HUD / UI]
        H2[Audio System]
        H3[Mission System]
    end

    P1 -->|Publish(Event)| Q
    P2 -->|Publish(Event)| Q
    P3 -->|Publish(Event)| Q

    Q -->|ProcessAll / DispatchAll| S
    S --> H1
    S --> H2
    S --> H3

```
