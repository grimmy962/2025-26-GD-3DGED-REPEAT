## 2025-26 GD 3D Game Engine Development ##

This is my Repeat CA for 3D Game Engine Development. This project is a HUB with 5 zones, and each zone shows off a different system from the class engine.</br></br>


### Screencast ###
[youtube link here] </br></br>


### Architecture Overview ###
The whole project is built around SceneManager and a bunch of small IZoneBuilder classes, one per zone (HubSceneBuilder, PhysicsZoneBuilder, AudioZoneBuilder, CameraZoneBuilder, OrchestrationZoneBuilder, EventsStateZoneBuilder).
Each one build its own Scene with its own systems (I made a ZoneSystemFactory for this, so I'm not copy-pasting the same 8-line system setup into every zone), and Main.cs just registers all 6 scene adn startup and calls SetActiveScene("Hub").
There's no hardcoded demo-switching logic in Main.cs anymore. Everything is deiven by SceneManager.SetActiveScene(...) at runtime.

Moving between the Hub and zones is done through portals (physical trigger volumes with a ZonePortal component on them). When the player walks into one, PhysicsSystem fires a TriggerEvent, and a single listener in Main.cs reacts to it by switching the active scene.
The physics/trigger code itself has no idea "scenes" even exist. It just announces "something touched this", and something else decides what to do about it.
This same idea (fire an event, et something else react) is reused a lot across the project, which is basically the whole point of the design pattern below.

Because each zone is its own scene, each one also needs its own copy of things like the ground, the player capsule, and the camera. I made small shared factories for this too (e.g. ZonePlayerFactory, ZoneAnnotationFactory) so this setup only has to be written once and gets reused everywhere.</br></br>


### Elective Zones - Why I Picked R4 & R6 ###
I picked R4 (Orchestration) and R6 (Events & State) mainly because the engine already had string working building blocks for both of them (Orchestrator.Builder with If, WaitSeconds, Publish, and the fluent EventBus API), so I could focus on building a proper sequence and proper event flow
instead of fighting with a system that barely worked. It also let me reuse the same Observer-style pattern in more places, which made the cross-zone pattern story a lot stronger.</br></br>


### Cross-Zone Design Pattern - Observer ###
The EventBus is the Subject. Anything that calls Subscribe<T>() or the fluent On<T>().WithPriorityPresent(...).Do(...) is an Observer. This isn't just used in one place. It's the actual mechanism behind zone navigation (every portal publishes a TriggerEvent, one listener in Main.css reacts by switching scenes),
camera-mode switching in the Camera zone, music track switching in the Audio zone, starting the ritual sequence in the Orchestration zone, and the two custom events (SwitchActivatedEvent, AlarmTriggeredEvent) in the Events & State zone, each subscribed with a different EventPriority preset. </br></br>


### A Few Design Decisions That Have To Be Mentioned ###
While building and testing this, I found and fixed a few real bugs in the class engine itself.
- a rotation bug in CurveController that made cinematic cameras spin out of control (it was adding a rotation delta every frame instead of just setting it)
- a bug where trigger volumes could crash the physics engine on contact (an invalid SpringSettings value)
- a bug where WASD movement always followed the capsule's spawn direction instead of wherever the camera was actually looking

I've kept all my zones reset back to their starrting state every time you re-enter them, since otherwise things like the Orchestration artifact or the monkeys in the Physics zone would just keep pilling up state across repeat visits.
