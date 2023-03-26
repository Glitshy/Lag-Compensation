# What is Lag Compensation and why do we need it?
Lag Compensation is essential to avoid clients needing to lead their shots in a multiplayer online shooter. I discovered my need for a lag compensation system when creating my own FPS using Mirror Networking, [Client Side Prediction](https://www.gabrielgambetta.com/client-side-prediction-server-reconciliation.html) and [Snapshot Interpolation](https://gafferongames.com/post/snapshot_interpolation/).

Explained by gabriel gambetta [here](https://www.gabrielgambetta.com/lag-compensation.html)

Explained by Yahn W. Bernier [here](https://developer.valvesoftware.com/wiki/Latency_Compensating_Methods_in_Client/Server_In-game_Protocol_Design_and_Optimization)

My initial solution is also inspired by [this](https://twoten.dev/lag-compensation-in-unity.html)

# What's included
This project contains two systems: 
- My initial approach which functions by setting the transforms of each individual collider of each player to the postion and rotation it was a given time ago
- My second approach with custom collider components (sphere, capsule and box similar to native Unity physics colliders) and my own raycasting maths called "collider cast"

Two example scenes, one for testing the custom hit detection / raycasting maths and one for comparing the two systems for the purpose of lag compensation.
Code is (somewhat) documented and there is some explanation text in the lag compensation scene.

# Why two approaches?
Why did I go through the effort of writing my own raycasting system and using custom colliders? 

Well, Unity only really updates colliders each fixed update, meeaning that when you set the transforms of colliders and then withing the same frame want to perform a raycast, it will not hit where you experct it to do. If you want to do that, you need to call [Physics.SyncTransforms](https://docs.unity3d.com/ScriptReference/Physics.SyncTransforms.html). This is a computationally non-trivial task, expecially with many colliders (i.e. many players).
There is also an example scene included where the performance of the two systems can be directly compared.

# How the "NetworkTracker" and the "NetworkTrackerSystem" works (native unity physics colliders)
The initial approach utilizes components of the type "NetworkTracker" to store the position and rotation toegether with timestamps in a list.
Timestamps should be added at each fixed update together with the server time by the parent component of a player.
The "NetworkTrackerSystem" contains all "NetworkTrackers" in a static list and is used to call the functions to "set the colliders back in time".

Given a certain test time, the postion and rotation at that time is interpolated and the global transforms are set to this interpolated timestamp.
After all colliders have been "sent back in time", Physics.SyncTransforms is called and a normal Raycast is performed.
As the system is designed with non-hitscan projectiles in mind, which travel a certain distance each fixed update, the test time value is incremented in a loop in steps of Time.fixedDeltaTime. At each step, the positions and rotations of the colliders are incremented adn another Racast is performed.
With a lag delay of ~300 ms (extreme example) and a fixed update rate of 50/s, this results in 16 or 17 iterations.

# How "HitColliders" and the "ColliderCastSystem" works (custom colliders)

## "HitCollider"
Parent class of all MonoBehaviours. Implements timestamps and some shared functions.

## "HitColliderGeneric"
Inherits from the abstract class "HitCollider". Is itself also an abstract calls from which "HitColliderSphere", "HitColliderCapsule" and "HitColliderBox" inherit.
The transforms of these colliders are never overridden as the custom hit detection code allows passing arbitrary positions and rotations as parameters.

## "HitColliderCollection"
Inherits from the abstract class "HitCollider". Each entity (i.e. player) with one or more "HitColliderGeneric" should have one "HitColliderCollection" at the geometric center. 
A radius value should be set to cover all colliders (keep in mind animations). This radius defines a bounding sphere and allows for quick assessment whether any of the colliders in this collection might be hit by a collider cast.

## "ColliderCastSystem"
Class containing a static list of all "HitColliderCollection" components and the functions to simulate the postions and rotations at a give point in time and perform a collider cast.
When calling the <code>Simulate(double simulationTime)</code> function, at first only the postions and rotations of the collection components will be interpolated. The interpolation logic of each individual collider will only be performed if the bounding sphere of the collection intersects with a collider cast.

# Unity Version
Created with Unity Version 2021.2.0b12, but should work with any newer version and probably most older versions as well.
