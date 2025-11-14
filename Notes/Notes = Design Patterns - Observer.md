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
When the player takes damage, the HUD must update.  
When an enemy dies, the score manager must adjust.  
When a level loads, UI elements, game objects, and analytics systems often need to be notified.

A naïve design tightly couples these systems together, forcing classes to know far too much about each other.  
The **Observer Pattern** avoids this by allowing one object (the subject) to publish notifications, while independent observers subscribe to these notifications without forming direct dependencies.

This note begins with the conceptual foundations of the Observer Pattern, introduces a clean and minimal Event Bus in C#, and concludes with a set of clear steps for extending the Event Bus into a fully featured system suitable for a 3D game engine. Each code sample includes an explanation of how it works and why the design choices make sense in the context of decoupled engine architecture.

---

## 1. The Observer Pattern

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
