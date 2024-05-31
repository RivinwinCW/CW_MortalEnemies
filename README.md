##   Mortal Enemies
# is a Content Warning mod
that aims to allow the modding community a simple and compatible to way to deal damage to and kill monsters. It is still is testing and there is much to be done to polish the results in-game, but already all enemy types are killable.

In truth the enemies are more knocked unconcious than anything else, as they can be instantly revived and return to normal as well. The mod works by disabling components on the enemy GameObjects and altering values associated with their ragdolls and controllers, nothing is destroyed or despawned.

# Additional Features
- A new damage/healing over time system, integrated with the players HUD, networked, and effecting both players and bots.
- An anti-flicker mechanism to try and avoid rapid flashing of the screen when taking damage from many sources.

# Known Issues
- When a Bot is killed it doesnt reset the fall timer resulting in a visible change in inertia after the fall timer expires.
- Animations need to be paused, for example the Whisks whisks keep spinning when dead.
- The drone stays hovering when dead and the button turtle stays standing.
- The current networking pattern is clunky without being very secure, need to tighten health value enforcement from server.

# To Modders
Anything killable including Players and Bots have a Mortality component automatically attached to their highest level GameObject when they spawn.
Only the host can call public methods like Damage() on Mortality components, so RPC the request to deal damage up to the host and have them execute. This is to help prevent client-only cheat mods from interacting with the Mortality components on other machines.
You can raycast to an enemy or player and use GetComponentInParent<Mortality>() to get it's Mortality component. MortalSingleton.Instance provides access to all currently spawned Mortality components if you want to do something like apply healing over time to all enemies.
Right now applying forces such as knockback is on you.